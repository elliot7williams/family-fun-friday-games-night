param([switch]$KeepForegroundHost,[ValidateRange(1,99)][int]$Version=1,[switch]$Candidate,[switch]$Visual,[switch]$VisualBaseline,[switch]$Initial,[switch]$ReuseInitialCore,[switch]$InitialRender,[switch]$Relative,[switch]$ReuseRelativeCore,[switch]$RelativeRender,[switch]$RelativeRenderBaseline,[switch]$ReuseRelativeRenderPair,[switch]$SdkPair,[switch]$AggregateSdkPair,[switch]$PrintSdk,[switch]$FullSdkPair,[switch]$ReproSdk)
$ErrorActionPreference='Stop'
if($ReuseInitialCore -and (!$Candidate -or $Initial -or $Visual -or $VisualBaseline)){throw 'Reuse is only for original model checks on the exact initial candidate'}
if($InitialRender -and (!$Visual -or !$Candidate -or $VisualBaseline -or $Initial -or $ReuseInitialCore)){throw 'Initial render requires matched candidate visual mode'}
if($ReuseRelativeCore -and (!$Candidate -or $Visual -or $Relative -or $ReuseInitialCore)){throw 'Relative reuse requires earlier model regression on exact candidate'}
if($RelativeRender -and (!$Visual -or !$Candidate -or $VisualBaseline -or $Initial -or $InitialRender -or $ReuseInitialCore -or $ReuseRelativeCore -or $Relative)){throw 'Relative render requires separate matched visual mode'}
if($RelativeRenderBaseline -and !$RelativeRender){throw 'Relative visual baseline requires relative render mode'}
if($ReuseRelativeRenderPair -and (!$InitialRender -or $RelativeRender -or $RelativeRenderBaseline)){throw 'Prior visual regression requires initial-render mode on exact relative pair'}
if($SdkPair -and (!$Candidate -or $VisualBaseline -or $RelativeRenderBaseline -or $ReuseRelativeRenderPair -or $ReuseRelativeCore -or $ReuseInitialCore)){throw 'SDK regression requires exact rebuilt pair without baseline/reuse modes'}
$root=Split-Path $PSScriptRoot -Parent
if($AggregateSdkPair -and !$SdkPair){throw 'Aggregate SDK regression requires SDK pair mode'}
$out=Join-Path $root ('continuity-evidence/font-shorthand-baseline-v'+$Version)
if($Candidate){$out=Join-Path $root ('continuity-evidence/font-shorthand-candidate-v'+$Version)}
if($Visual){if(!$Candidate){throw 'Visual mode requires candidate'};$out=Join-Path $root ('continuity-evidence/font-shorthand-render-v'+$Version)}
if($VisualBaseline){if(!$Visual){throw 'Visual baseline requires visual mode'};$out=Join-Path $root ('continuity-evidence/font-shorthand-render-baseline-v'+$Version)}
if($Initial){if($Visual -or $VisualBaseline){throw 'Initial model tests are separate from visual qualification'};$out=Join-Path $root ('continuity-evidence/font-initial-'+$(if($Candidate){'candidate'}else{'baseline'})+'-v'+$Version)}
if($InitialRender){$out=Join-Path $root ('continuity-evidence/font-initial-render-v'+$Version)}
if($Relative){if($Visual -or $Initial -or $ReuseInitialCore){throw 'Relative model mode must be separate'};$out=Join-Path $root ('continuity-evidence/font-relative-'+$(if($Candidate){'candidate'}else{'baseline'})+'-v'+$Version)}
if($RelativeRender){$out=Join-Path $root ('continuity-evidence/font-relative-render'+$(if($RelativeRenderBaseline){'-baseline'})+'-v'+$Version)}
if($ReuseRelativeRenderPair){$out=Join-Path $root ('continuity-evidence/font-relative-prior-render-v'+$Version)}
if($SdkPair){$mode=if($RelativeRender){'relative-render'}elseif($InitialRender){'initial-render'}elseif($Visual){'shorthand-render'}elseif($Relative){'relative'}elseif($Initial){'initial'}else{'shorthand'};$out=Join-Path $root ('continuity-evidence/sdk-font-'+$mode+'-v'+$Version)}
if($AggregateSdkPair){$out=Join-Path $root ('continuity-evidence/sdk-aggregate-font-'+$mode+'-v'+$Version)}
if($PrintSdk){if(!$AggregateSdkPair){throw 'Print font SDK requires aggregate pair mode'};$out=Join-Path $root ('continuity-evidence/sdk-print-aggregate-font-'+$mode+'-v'+$Version)}
if($FullSdkPair){if(!$AggregateSdkPair -or $PrintSdk){throw 'Full SDK font checks require separate aggregate pair mode'};$out=Join-Path $root ('continuity-evidence/full-sdk-font-'+$mode+'-v'+$Version)}
if($ReproSdk){if(!$AggregateSdkPair -or $PrintSdk -or $FullSdkPair){throw 'Repro SDK requires separate aggregate pair mode'};$out=Join-Path $root ('continuity-evidence/repro-sdk-font-'+$mode+'-v'+$Version)}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner required'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Hash -and $Hash -cne $actual){throw 'Pinned input differs'};$inputs.Add(@{path=$Path;sha256=$actual})
}
$null=New-Item -ItemType Directory -Path $out
$result=@{passed=$false;productionChanged=$false;fullReleaseQualified=$false;inputs=$inputs;runs=$runs}
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path $owned;. $helper
    $core=Join-Path $root 'continuity-evidence/print-styles-v3/Lea.Engine.dll'
    $hash='A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
    Hold $core $hash;$copy=Join-Path $out 'Lea.Engine.dll'
    if($ReproSdk){
        $proof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json'
        Hold $proof '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
        $qualified=Get-Content $proof -Raw|ConvertFrom-Json
        $members=@($qualified.products|Where-Object {$_.path -in 'bin/Release/Lea.Engine.dll','bin/Release/Lea.Win.dll'})
        if($members.Count -ne 2 -or @($members|Where-Object {!$_.equal -or $_.sha256 -cne $_.secondSha256}).Count){throw 'Repro font pair incomplete'}
        foreach($member in $members){
            Hold $member.first $member.sha256;Hold $member.second $member.sha256
            $name=[IO.Path]::GetFileName($member.path)
            if($Visual -or $name -ceq 'Lea.Engine.dll'){
                $target=Join-Path $out $name;Copy-Item -LiteralPath $member.first -Destination $target;Hold $target $member.sha256
                if($name -ceq 'Lea.Engine.dll'){$result.candidateCoreSha256=$member.sha256}else{$view=$target;$result.candidateViewSha256=$member.sha256}
            }
        }
        $result.reproSdk=$true;$result.sdkPair=$true;$result.aggregateSdkPair=$true
    }elseif($SdkPair){
        $sdkProof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-attempt2\result.json';$proofPin='55595335946DB681D98788BDCB219900FE9CF9DCC75D60CE64FD03BD9ED4325A'
        if($AggregateSdkPair){$sdkProof=Join-Path $root 'continuity-evidence/sdk-portable-source-v1/result.json';$proofPin='57E993FAE281B565D502BBEE77A95C98A71B5A0816C5DC5D66250F3EE9C32E36'}
        if($PrintSdk){$sdkProof=Join-Path $root 'continuity-evidence/sdk-print-aggregate-runtime-v1/result.json';$proofPin='D6D0E65685041CFB517712C92824F3AB5E50E22E4ED6DB5086E882B058D0B676'}
        if($FullSdkPair){$sdkProof='I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json';$proofPin='3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'}
        Hold $sdkProof $proofPin
        $qualified=Get-Content $sdkProof -Raw|ConvertFrom-Json
        if(!$qualified.passed -or !$qualified.inputsUnchanged -or $qualified.ownerAttention){throw 'Rebuilt SDK proof incomplete'}
        if($FullSdkPair){if(!$qualified.compiled -or $qualified.libraryProducts.Count -ne 6){throw 'Full SDK library proof incomplete'}}elseif($PrintSdk){if(!$qualified.aggregateProducts -or $qualified.runs.Count -ne 4){throw 'Print aggregate proof incomplete'}}elseif($AggregateSdkPair){if(!$qualified.aggregateSourceBuildPassed -or $qualified.sourceBuiltProducts.Count -ne 4){throw 'Aggregate source proof incomplete'}}elseif($qualified.outputs.Count -ne 2){throw 'SDK pair proof incomplete'}
        $from='I:\LeaBuildStorage\core-view-sdk-20261031-v3\bin\Release\Lea.Engine.dll';$pin='772A51ADD8C8A9CE80964BEDB043D912B0DFB95422E3DC84FAC6F49B93366270'
        if($AggregateSdkPair){$from=Join-Path $root 'continuity-evidence/sdk-portable-source-v1/bundle/Sample/bin/Release/Lea.Engine.dll';$pin='DC71E2B9DA704135CA022E5BBC2F1C57BA8046621C41E40AB98C7DC354BD6213'}
        if($PrintSdk){$from=Join-Path $root 'continuity-evidence/sdk-print-aggregate-source-v1/bundle/Sample/bin/Release/Lea.Engine.dll';$pin='5B8A501DA627467A6C261A355917DE5F73DF8F00758B49C1D240CF7D596E8A4A'}
        if($FullSdkPair){$from='I:\LeaBuildStorage\full-sdk-source-20261101-v1/bin/Release/Lea.Engine.dll';$pin='13F43BE5F10072EEFDE7CE1DF06EAEDCEDF8AA3375D24A4548DD4B6BF6E1D365'}
        Hold $from $pin;Copy-Item $from $copy;Hold $copy $pin;$result.candidateCoreSha256=$pin
        if($Visual){$from='I:\LeaBuildStorage\core-view-sdk-20261031-v3\bin\Release\Lea.Win.dll';$pin='80085FA1CD380CDF2E7AB0F8BBE5AC8982A11E95F82C43F776EE16ADC1A0D0C0';if($AggregateSdkPair){$from=Join-Path $root 'continuity-evidence/sdk-portable-source-v1/bundle/Sample/bin/Release/Lea.Win.dll';$pin='6F907862F57C7640A8B0E7BE2C22DFEDD18B3966D472958FCFCEDA53A00F0F52'};if($PrintSdk){$from=Join-Path $root 'continuity-evidence/sdk-print-aggregate-source-v1/bundle/Sample/bin/Release/Lea.Win.dll';$pin='A622A1C266AD7CA27F437832E390E594F7E419CAFED700C9F466F0517970CE0D'};if($FullSdkPair){$from='I:\LeaBuildStorage\full-sdk-source-20261101-v1/bin/Release/Lea.Win.dll';$pin='629D7AA070C082853A2BD1F3A02E016685E348CA3A127A9DC4FFE01CD06C039D'};Hold $from $pin;$view=Join-Path $out 'Lea.Win.dll';Copy-Item $from $view;Hold $view $pin;$result.candidateViewSha256=$pin}
        $result.sdkPair=$true
        $result.aggregateSdkPair=[bool]$AggregateSdkPair
        $result.printSdk=[bool]$PrintSdk
        $result.fullSdkPair=[bool]$FullSdkPair
    }
    elseif($Visual){
        $proof=Join-Path $root 'continuity-evidence/font-shorthand-candidate-v3/result.json';Hold $proof '9E962711AD549D7BC2D609439F2EAA07ED22724DAF85B2CCD4D742E18D4B9D94'
        $from=Join-Path $root 'continuity-evidence/font-shorthand-candidate-v3/Lea.Engine.dll';$pin='0B09D9193A5819F21B055E29E8C463D32F746D7523C32FEA4E8DA25D2C30B514'
        if($InitialRender){
            $initialProof=Join-Path $root 'continuity-evidence/font-initial-candidate-v2/result.json';Hold $initialProof 'BDB121BB1E3C5BF5E9433368BE1726E32D1967C89A0B635E94AE85E40D7450D8'
            $from=Join-Path $root 'continuity-evidence/font-initial-candidate-v2/Lea.Engine.dll';$pin='1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'
        }
        if($RelativeRender){
            $relativeProof=Join-Path $root 'continuity-evidence/font-relative-candidate-v1/result.json';Hold $relativeProof 'D828D1B49F3E9DC6C30E2C8186004834F6B8929172A19D645592AC02B06306DB'
            $from=Join-Path $root 'continuity-evidence/font-relative-candidate-v1/Lea.Engine.dll';$pin='4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA'
            if($RelativeRenderBaseline){$from=Join-Path $root 'continuity-evidence/font-initial-candidate-v2/Lea.Engine.dll';$pin='1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'}
        }
        if($VisualBaseline){$from=$core;$pin=$hash;$result.visualBaseline=$true}
        if($ReuseRelativeRenderPair){
            $relativePairProof=Join-Path $root 'continuity-evidence/font-relative-render-v1/result.json';Hold $relativePairProof '5FCA5A4730ECA3F2894533665FC22CAA9BA520FA49A2B71622B8B45D295E03A0'
            $from=Join-Path $root 'continuity-evidence/font-relative-render-v1/Lea.Engine.dll';$pin='4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA'
        }
        Hold $from $pin;Copy-Item $from $copy;Hold $copy $pin;$result.candidateCoreSha256=$pin
        if($ReuseRelativeRenderPair){
            $from=Join-Path $root 'continuity-evidence/font-relative-render-v1/Lea.Win.dll';$pin='CA51FC818B119529EA8F7104D0753C5779070A7CCF0AD157675A564C4755C584';Hold $from $pin
            $view=Join-Path $out 'Lea.Win.dll';Copy-Item $from $view;Hold $view $pin;$result.candidateViewSha256=$pin
        }else{$builder=Join-Path $PSScriptRoot 'Build-FontShorthandView.ps1';Hold $builder;. $builder}
    }
    elseif($ReuseInitialCore){
        $proof=Join-Path $root 'continuity-evidence/font-initial-candidate-v2/result.json';Hold $proof 'BDB121BB1E3C5BF5E9433368BE1726E32D1967C89A0B635E94AE85E40D7450D8'
        $from=Join-Path $root 'continuity-evidence/font-initial-candidate-v2/Lea.Engine.dll';$pin='1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'
        Hold $from $pin;Copy-Item $from $copy;Hold $copy $pin;$result.candidateCoreSha256=$pin
    }
    elseif($ReuseRelativeCore){
        $proof=Join-Path $root 'continuity-evidence/font-relative-candidate-v1/result.json';Hold $proof 'D828D1B49F3E9DC6C30E2C8186004834F6B8929172A19D645592AC02B06306DB'
        $from=Join-Path $root 'continuity-evidence/font-relative-candidate-v1/Lea.Engine.dll';$pin='4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA'
        Hold $from $pin;Copy-Item $from $copy;Hold $copy $pin;$result.candidateCoreSha256=$pin
    }
    elseif($Candidate){$builder=Join-Path $PSScriptRoot 'Build-FontShorthandCore.ps1';Hold $builder;. $builder}
    elseif($Relative){$from=Join-Path $root 'continuity-evidence/font-initial-candidate-v2/Lea.Engine.dll';$pin='1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA';Hold $from $pin;Copy-Item $from $copy;Hold $copy $pin}
    elseif($Initial){$from=Join-Path $root 'continuity-evidence/font-shorthand-candidate-v3/Lea.Engine.dll';Hold $from '0B09D9193A5819F21B055E29E8C463D32F746D7523C32FEA4E8DA25D2C30B514';Copy-Item $from $copy;Hold $copy '0B09D9193A5819F21B055E29E8C463D32F746D7523C32FEA4E8DA25D2C30B514'}
    else{Copy-Item $core $copy;Hold $copy $hash}
    $sourceName=if($Visual){'FontShorthandRenderMain.cs'}else{'FontShorthandMain.cs'}
    if($Initial){$sourceName='FontInitialMain.cs'}
    if($Relative){$sourceName='FontRelativeMain.cs'}
    $source=Join-Path $root ('Lea.Tests/'+$sourceName);Hold $source
    $test=Join-Path $out $sourceName;Copy-Item $source $test;Hold $test
    $fixtureName=if($Visual){'font-shorthand-render.html'}else{'font-shorthand.html'}
    if($Initial){$fixtureName='font-initial.html'}
    if($Relative){$fixtureName='font-relative.html'}
    $fixture=Join-Path $root ('LeaForge/test-pages/'+$fixtureName);Hold $fixture
    $html=Join-Path $out 'font-shorthand.html';Copy-Item $fixture $html;Hold $html
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    $exe=Join-Path $out 'FontShorthandTests.exe'
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'),('/reference:"'+$copy+'"'))
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll')){$path=Join-Path $refs $name;Hold $path;$arguments+=('/reference:"'+$path+'"')}
    if($Visual){$arguments+=('/reference:"'+$view+'"');foreach($name in @('System.Drawing.dll','System.Windows.Forms.dll')){$path=Join-Path $refs $name;Hold $path;$arguments+=('/reference:"'+$path+'"')}}
    $arguments+=('"'+$test+'"')
    $build=Invoke-OwnedTestRun -Name 'compile-font-tests' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$build.passed){throw 'Font regression compilation failed'}
    Hold $exe
    $runArguments=@('"'+$html+'"');if($Visual){$runArguments+=('"'+$out+'"')}
    if($InitialRender){$runArguments+='--initial'}
    if($RelativeRender){$runArguments+='--relative'}
    $run=Invoke-OwnedTestRun -Name 'font-shorthand' -Application $exe -Arguments $runArguments -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    $lines=@(Get-Content $run.stdout)
    $result.passed=$run.passed -and @($lines|Where-Object {$_ -ceq 'FONT_SHORTHAND controls=12 cases=14 failures=0'}).Count -eq 1
    $result.baselineMissingShorthandObserved=@($lines|Where-Object {$_ -ceq 'FONT_SHORTHAND controls=12 cases=14 failures=14'}).Count -eq 1
    if($Visual){$result.passed=$run.passed -and @($lines|Where-Object {$_ -ceq 'FONT_RENDER PASS 86'}).Count -eq 1;$result.visualComparisonOnly=$true}
    if($InitialRender){$result.passed=$run.passed -and @($lines|Where-Object {$_ -ceq 'FONT_RENDER PASS 142'}).Count -eq 1}
    if($RelativeRender){$result.passed=$run.passed -and @($lines|Where-Object {$_ -ceq 'FONT_RENDER PASS 142'}).Count -eq 1;$result.relativeVisualBaseline=[bool]$RelativeRenderBaseline}
    if($Initial){$result.passed=$run.passed -and @($lines|Where-Object {$_ -ceq 'FONT_INITIAL controls=5 cases=7 failures=0'}).Count -eq 1;$result.initialBaselineFailureObserved=@($lines|Where-Object {$_ -ceq 'FONT_INITIAL controls=5 cases=7 failures=7'}).Count -eq 1}
    if($Relative){$result.passed=$run.passed -and @($lines|Where-Object {$_ -ceq 'FONT_RELATIVE controls=6 cases=12 failures=0'}).Count -eq 1;$result.relativeBaselineFailureObserved=@($lines|Where-Object {$_ -ceq 'FONT_RELATIVE controls=6 cases=12 failures=12'}).Count -eq 1}
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaFontInputLeases=$holds}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    $result|Select-Object passed,baselineMissingShorthandObserved,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
