param([switch]$KeepForegroundHost,[ValidateRange(1,99)][int]$Version=1,[switch]$RealDecoder,[switch]$ShutdownRace,[switch]$QualifiedView,[switch]$SdkPair,[switch]$AggregateSdkPair,[switch]$PrintSdk,[switch]$FullSdkPair,[switch]$ReproSdk)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root ('continuity-evidence/decoder-owner-retirement-v'+$Version)
if($ShutdownRace){if(!$RealDecoder){throw 'Shutdown race requires real worker'};$out=Join-Path $root ('continuity-evidence/decoder-shutdown-race-v'+$Version)}
if($QualifiedView){if(!$ShutdownRace){throw 'Qualified view mode requires shutdown race'};$out=Join-Path $root ('continuity-evidence/qualified-decoder-shutdown-race-v'+$Version)}
if($SdkPair){if(!$RealDecoder -or $QualifiedView){throw 'SDK decoder requires real worker and no historical qualified-view substitution'};$out=Join-Path $root ('continuity-evidence/sdk-decoder-'+$(if($ShutdownRace){'shutdown-race'}else{'retirement'})+'-v'+$Version)}
if($AggregateSdkPair){if(!$SdkPair){throw 'Aggregate decoder requires SDK pair mode'};$out=Join-Path $root ('continuity-evidence/sdk-aggregate-decoder-'+$(if($ShutdownRace){'shutdown-race'}else{'retirement'})+'-v'+$Version)}
if($PrintSdk){if(!$AggregateSdkPair){throw 'Print SDK requires aggregate SDK pair mode'};$out=Join-Path $root ('continuity-evidence/sdk-print-aggregate-decoder-'+$(if($ShutdownRace){'shutdown-race'}else{'retirement'})+'-v'+$Version)}
if($FullSdkPair){if(!$AggregateSdkPair -or $PrintSdk){throw 'Full SDK requires separate aggregate SDK pair mode'};$out=Join-Path $root ('continuity-evidence/full-sdk-decoder-'+$(if($ShutdownRace){'shutdown-race'}else{'retirement'})+'-v'+$Version);if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}}
if($ReproSdk){if(!$AggregateSdkPair -or $PrintSdk -or $FullSdkPair){throw 'Repro SDK requires separate aggregate pair mode'};$out=Join-Path $root ('continuity-evidence/repro-sdk-decoder-'+$(if($ShutdownRace){'shutdown-race'}else{'retirement'})+'-v'+$Version);if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner required'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Hash -and $Hash -cne $actual){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$actual})
}
$null=New-Item -ItemType Directory -Path $out
$result=@{passed=$false;inputs=$inputs;runs=$runs;securityQualified=$false;decoderProfileChanged=$false;browserAdopted=$false}
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path $owned;. $helper
    $core=Join-Path $root 'continuity-evidence/print-ticket-project-v1/bin/Lea.Engine.dll'
    $corePin='A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
    if($SdkPair){
        if($ReproSdk){
            $sdkProof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json';Hold $sdkProof '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
            $state=Get-Content $sdkProof -Raw|ConvertFrom-Json
            $members=@($state.products|Where-Object {$_.path -in 'bin/Release/Lea.Engine.dll','bin/Release/Lea.Win.dll','bin/Release/Lea.Image.exe'})
            if($members.Count -ne 3 -or @($members|Where-Object {!$_.equal -or $_.sha256 -cne $_.secondSha256}).Count){throw 'Repro decoder closure incomplete'}
            foreach($member in $members){Hold $member.first $member.sha256;Hold $member.second $member.sha256}
            $aggregateBin='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/a/bin/Release'
            $core=Join-Path $aggregateBin 'Lea.Engine.dll';$corePin=($members|Where-Object path -ceq 'bin/Release/Lea.Engine.dll').sha256
            $result.reproSdk=$true
        }elseif($FullSdkPair){
            $sdkProof='I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json';Hold $sdkProof '3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'
            $state=Get-Content $sdkProof -Raw|ConvertFrom-Json
            if(!$state.passed -or !$state.compiled -or !$state.inputsUnchanged -or $state.ownerAttention -or $state.libraryProducts.Count -ne 6){throw 'Full SDK library proof incomplete'}
            $aggregateBin='I:\LeaBuildStorage\full-sdk-source-20261101-v1/bin/Release'
            $core=Join-Path $aggregateBin 'Lea.Engine.dll';$corePin='13F43BE5F10072EEFDE7CE1DF06EAEDCEDF8AA3375D24A4548DD4B6BF6E1D365'
        }elseif($PrintSdk){
            $sdkProof=Join-Path $root 'continuity-evidence/sdk-print-aggregate-runtime-v1/result.json';Hold $sdkProof 'D6D0E65685041CFB517712C92824F3AB5E50E22E4ED6DB5086E882B058D0B676'
            $state=Get-Content $sdkProof -Raw|ConvertFrom-Json
            if(!$state.passed -or !$state.aggregateProducts -or !$state.inputsUnchanged -or $state.ownerAttention -or $state.runs.Count -ne 4){throw 'Print aggregate runtime proof incomplete'}
            $aggregateBin=Join-Path $root 'continuity-evidence/sdk-print-aggregate-source-v1/bundle/Sample/bin/Release'
            $core=Join-Path $aggregateBin 'Lea.Engine.dll';$corePin='5B8A501DA627467A6C261A355917DE5F73DF8F00758B49C1D240CF7D596E8A4A'
        }elseif($AggregateSdkPair){
            $sdkProof=Join-Path $root 'continuity-evidence/sdk-portable-source-v1/result.json';Hold $sdkProof '57E993FAE281B565D502BBEE77A95C98A71B5A0816C5DC5D66250F3EE9C32E36'
            $state=Get-Content $sdkProof -Raw|ConvertFrom-Json
            if(!$state.passed -or !$state.inputsUnchanged -or $state.ownerAttention -or !$state.aggregateSourceBuildPassed -or $state.sourceBuiltProducts.Count -ne 4){throw 'Aggregate source build proof incomplete'}
            $aggregateBin=Join-Path $root 'continuity-evidence/sdk-portable-source-v1/bundle/Sample/bin/Release'
            $core=Join-Path $aggregateBin 'Lea.Engine.dll';$corePin='DC71E2B9DA704135CA022E5BBC2F1C57BA8046621C41E40AB98C7DC354BD6213'
        }else{
        $sdkProof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-attempt2\result.json';Hold $sdkProof '55595335946DB681D98788BDCB219900FE9CF9DCC75D60CE64FD03BD9ED4325A'
        $imageProof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-image-worker\result.json';Hold $imageProof '95F4A5D6DC02D49A9A27DFF1245010385FD4BF349ADA253962D895C814E277A0'
        foreach($proofPath in @($sdkProof,$imageProof)){$state=Get-Content $proofPath -Raw|ConvertFrom-Json;if(!$state.passed -or !$state.inputsUnchanged -or $state.ownerAttention){throw 'SDK decoder build proof incomplete'}}
        $core='I:\LeaBuildStorage\core-view-sdk-20261031-v3\bin\Release\Lea.Engine.dll';$corePin='772A51ADD8C8A9CE80964BEDB043D912B0DFB95422E3DC84FAC6F49B93366270'
        }
        $result.sdkPair=$true;$result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true
        $result.aggregateSdkPair=[bool]$AggregateSdkPair
        $result.printSdk=[bool]$PrintSdk
        $result.fullSdkPair=[bool]$FullSdkPair
    }
    Hold $core $corePin
    Copy-Item $core (Join-Path $out 'Lea.Engine.dll');Hold (Join-Path $out 'Lea.Engine.dll')
    $DocSelection=$true;$DocGestures=$true;$DocCopy=$true;$DocUia=$true
    if($SdkPair){
        $sdkView='I:\LeaBuildStorage\core-view-sdk-20261031-v3\bin\Release\Lea.Win.dll';$viewPin='80085FA1CD380CDF2E7AB0F8BBE5AC8982A11E95F82C43F776EE16ADC1A0D0C0'
        if($AggregateSdkPair){$sdkView=Join-Path $aggregateBin 'Lea.Win.dll';$viewPin='6F907862F57C7640A8B0E7BE2C22DFEDD18B3966D472958FCFCEDA53A00F0F52'}
        if($PrintSdk){$viewPin='A622A1C266AD7CA27F437832E390E594F7E419CAFED700C9F466F0517970CE0D'}
        if($FullSdkPair){$viewPin='629D7AA070C082853A2BD1F3A02E016685E348CA3A127A9DC4FFE01CD06C039D'}
        if($ReproSdk){$viewPin=($members|Where-Object path -ceq 'bin/Release/Lea.Win.dll').sha256}
        Hold $sdkView $viewPin
        Copy-Item $sdkView (Join-Path $out 'Lea.Win.dll')
    }
    elseif($QualifiedView){
        $qualification=Join-Path $root 'continuity-evidence/temporal-startup-v1/shutdown-script-owned-run-v1/result.json'
        Hold $qualification '5F4C37A3C7858E1C8EB95E57147A82FCED3D19D8AAE9E5C97EA1C0C29F39C5E0'
        $qualified=Get-Content $qualification -Raw|ConvertFrom-Json
        if(!$qualified.passed -or !$qualified.inputsUnchanged -or !$qualified.traceEnvironmentRestored -or $qualified.ownerAttention -or $qualified.changedFiles.Count -ne 0){throw 'Exact scripting campaign proof incomplete'}
        $qualifiedAssembly=Join-Path $root 'continuity-evidence/temporal-startup-v1/shutdown-script-suite-v1/runtime/Lea.Win.dll'
        Hold $qualifiedAssembly '2225EC83D0DB503FE7778F896E7FC41475F5B50C690FE89B4FE9604B46E856CA'
        Copy-Item $qualifiedAssembly (Join-Path $out 'Lea.Win.dll')
        $result.exactQualifiedView=$true;$result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true
    }else{
        $builder=Join-Path $PSScriptRoot 'Build-PrintImageView.ps1';Hold $builder;. $builder
    }
    Hold (Join-Path $out 'Lea.Win.dll')
    $workerPath=Join-Path $out 'Lea.Image.exe'
    if($RealDecoder){
        $workerSource=Join-Path $root 'continuity-evidence/nested-overlap-regression-v1/runtime/Lea.Image.exe'
        $workerPin='FE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535'
        if($SdkPair){$workerSource='I:\LeaBuildStorage\sdk-image-worker-20261031-v1\bin\Release\Lea.Image.exe';$workerPin='EF89F8AA04E51921B54D1BAFE889244BF39229E57A08FBD7BFCB0D6B5E098BD3'}
        if($AggregateSdkPair){$workerSource=Join-Path $aggregateBin 'Lea.Image.exe';$workerPin='A4E44422DE7A0F07A3DFDE0C6827F97C6B818A35D94D8ABD54F4CBF1692E7E42'}
        if($PrintSdk){$workerPin='B5E1A90210B53FC3F957D0D8DF0E3EDBCA9D7AB7B825992A82C26648038BC8E7'}
        if($FullSdkPair){$workerPin='9EB5E1EBA75BE747A45D23B16D95800DD424CDD77978E014164646CE17BD29FD'}
        if($ReproSdk){$workerPin=($members|Where-Object path -ceq 'bin/Release/Lea.Image.exe').sha256}
        Hold $workerSource $workerPin
        Copy-Item $workerSource $workerPath;Hold $workerPath
        $result.decoderAclBefore=(Get-Acl $workerPath).Sddl
    }
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $exe=Join-Path $out 'DecoderRetirementChecks.exe'
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'))
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll')){$path=Join-Path $refs $name;Hold $path;$arguments+=('/reference:"'+$path+'"')}
    $testSource=Join-Path $root 'Lea.Tests/DecoderRetirementMain.cs';Hold $testSource
    $test=Join-Path $out 'DecoderRetirementMain.cs';Copy-Item $testSource $test;Hold $test
    $arguments+=('"'+$test+'"')
    $compiled=Invoke-OwnedTestRun -Name 'compile-retirement' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Retirement checks compilation failed'}
    Hold $exe
    $testArguments=@('"'+(Join-Path $out 'Lea.Win.dll')+'"')
    if($RealDecoder){$testArguments+=('"'+$workerPath+'"')}
    if($ShutdownRace){$testArguments+='--shutdown-race'}
    $tested=Invoke-OwnedTestRun -Name 'decoder-retirement' -Application $exe -Arguments $testArguments -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    $result.passed=$tested.passed
    if($RealDecoder){
        $result.decoderAclAfter=(Get-Acl $workerPath).Sddl
        $result.decoderAclRestored=$result.decoderAclBefore -ceq $result.decoderAclAfter
        $result.realWorkerCampaign=$true;$result.decoderProfileChanged=$true
        $result.passed=$result.passed -and $result.decoderAclRestored
    }
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaDecoderRetirementInputLeases=$holds}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
