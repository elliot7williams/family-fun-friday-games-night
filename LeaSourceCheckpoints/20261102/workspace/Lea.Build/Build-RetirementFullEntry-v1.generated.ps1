param([switch]$KeepForegroundHost,[ValidateRange(1,8)][int]$Version=1,[switch]$FocusedEntry,[switch]$RecoveryTrace,[switch]$Replay)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v2'
if($Version -eq 7){$stage='I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v3'}
if($Version -eq 8){$stage='I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v4'}
$out='I:\LeaBuildStorage\sdk-full-script-entry-20261101-v'+$Version
if($FocusedEntry){if($Version -notin 3,7,8){throw 'Focused entry requires a selected companion candidate'};$out=if($Version -eq 8){'I:\LeaBuildStorage\sdk-entry-host-focused-20261101-v3'}elseif($Version -eq 7){'I:\LeaBuildStorage\sdk-entry-host-focused-20261101-v2'}else{'I:\LeaBuildStorage\sdk-entry-host-focused-20261101-v1'}}
if($RecoveryTrace){if($Version -ne 8 -or $FocusedEntry){throw 'Recovery trace requires separate deterministic full-prefix mode'};$out='I:\LeaBuildStorage\contains-recovery-entry-20261102-v1'}
if($Replay){if($Version -ne 8 -or $FocusedEntry -or $RecoveryTrace){throw 'Replay requires separate uninstrumented deterministic mode'};$out='I:\LeaBuildStorage\retirement-full-entry-20261102-v1'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence and reserve required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($pin -and $pin -cne $hash){throw 'Entry build input differs'};$inputs.Add(@{path=$path;sha256=$hash})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;fullSuiteCompiled=$false;executed=$false;completeSdk=$false}
try{
    Hold $PSCommandPath
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'build.ps1')
    $result.diagnosticOnly=$Version -eq 4 -or [bool]$RecoveryTrace
    $parentHash='906E3CAAE4C160C7FDCF9D7E79B08076B9A12F46A76715AE02CAE9E72983764C'
    if($Version -eq 8){
        $proof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json';Hold $proof '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
        $build=Get-Content $proof -Raw|ConvertFrom-Json
        $parents=@($build.products|Where-Object path -ceq 'bin/Release/Lea.Script.dll')
        if($parents.Count -ne 1 -or !$parents[0].equal -or $parents[0].sha256 -cne $parents[0].secondSha256){throw 'Deterministic parent selection differs'}
        Hold $parents[0].first $parents[0].sha256;Hold $parents[0].second $parents[0].sha256
        $parentHash=$parents[0].sha256
    }elseif($Version -ge 5){
        $libraryProof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48-session-io\result.json';$libraryPin='FA92DF2B826A0918CAA47818D918C6A20B6A417BAB1255DC0E930EF832512BD3';$selectedParent='0425BFF9D9289C5F50DCA565BB257A394DA3D529F96202FA6D6D7324905707B7'
        if($Version -eq 6){$libraryProof='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48-pooled-io\result.json';$libraryPin='8730DBD801165B3E815A54777E35F4628D143AC608C0A66F160CBE7CFCC64DB0';$selectedParent='AFA6D95F21EC093F20F88177FD0B3710B956B1775958317C4BE6ED09864341B6'}
        if($Version -eq 7){$libraryProof='I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json';$libraryPin='3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B';$selectedParent='36AB8810EA87DDC6D3A840B629772210408F1ABEAF7341E204442E1275D79304'}
        Hold $libraryProof $libraryPin
        $library=Get-Content $libraryProof -Raw|ConvertFrom-Json
        $libraryFiles=@($library.outputs)
        if($Version -eq 7){if(!$library.compiled -or $library.libraryProducts.Count -ne 6){throw 'Combined library build incomplete'};$libraryFiles=@($library.libraryProducts|Where-Object {[IO.Path]::GetFileName($_.path) -in 'Lea.Script.dll','Lea.Script.Win.dll'})}
        if(!$library.passed -or !$library.inputsUnchanged -or $library.ownerAttention -or $libraryFiles.Count -ne 2){throw 'Selected library build incomplete'}
        foreach($file in $libraryFiles){Hold $file.path $file.sha256}
        $parents=@($libraryFiles|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Lea.Script.dll'})
        if($parents.Count -ne 1 -or $parents[0].sha256 -cne $selectedParent){throw 'Selected parent identity differs'}
        $parentHash=$parents[0].sha256
    }
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $bindingPin=if($Version -eq 7){'9E759782A6A76F56C28DE08CB0237E35657CBAFDBBB0C0CA0317A72BFBA34EE5'}else{'C7C8A080179284DE563E4F04A1FD15EE497870420010150D3955E20D688DBDFB'}
    if($Version -eq 8){$bindingPin='206B9E49C275F6305DDEFB5DEC7448502C8873001E0E24095B68C7A311A92A3D'}
    $proof=Join-Path $stage 'bindings.json';Hold $proof $bindingPin
    $bindings=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$bindings.verified -or $bindings.componentManifestsRebound -ne 5 -or $bindings.workerHashConstantsRebound -ne 2 -or $bindings.entryFixtureFiles -ne 7 -or $bindings.sourceChanges.Count -ne 4){throw 'Entry source preparation incomplete'}
    foreach($file in $bindings.files){Hold (Join-Path $stage $file.path) $file.sha256}
    $source=Join-Path $stage 'source/WorkerEntryNativeHost.cs';$resource=Join-Path $stage 'entry-fixture.json'
    if($Version -ge 2){
        $fixture=Get-Content $resource -Raw|ConvertFrom-Json -AsHashtable
        $expectedOld=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v1/runtime/WorkerEntryFixture'
        if($Version -eq 8){$expectedOld=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v8/runtime/WorkerEntryFixture'}
        if($fixture.directory -cne $expectedOld){throw 'Original fixture resource path differs'}
        $fixture.directory=Join-Path $root ('continuity-evidence/sdk-full-script-suite-v'+$Version+'/runtime/WorkerEntryFixture')
        if($RecoveryTrace){$fixture.directory=Join-Path $root 'continuity-evidence/contains-recovery-full-prefix-v1/runtime/WorkerEntryFixture'}
        if($Replay){$fixture.directory=Join-Path $root 'continuity-evidence/retirement-full-prefix-v1/runtime/WorkerEntryFixture'}
        if($FocusedEntry){$fixture.directory=Join-Path $root 'continuity-evidence/sdk-entry-core-runtime-v1/WorkerEntryFixture'}
        if($FocusedEntry -and $Version -eq 7){$fixture.directory=Join-Path $root 'continuity-evidence/sdk-entry-core-runtime-v2/WorkerEntryFixture'}
        if($FocusedEntry -and $Version -eq 8){$fixture.directory=Join-Path $root 'continuity-evidence/sdk-entry-core-runtime-v3/WorkerEntryFixture'}
        if($Version -ge 3){
            $coreProof='I:\LeaBuildStorage\sdk-entry-core-20261101-v1\result.json';$corePin='075F98C47F7FF7D2AA02E4577F8D1C4453D8C42D2A269012AE9DE3F2C64CC730'
            if($Version -eq 7){$coreProof='I:\LeaBuildStorage\sdk-entry-core-20261101-v2/result.json';$corePin='6B7FC1DE3C6A425F07146E244BC31CEEA5390C63195E0969735533B65CE85F72'}
            if($Version -eq 8){$coreProof='I:\LeaBuildStorage\sdk-entry-core-20261101-v3/result.json';$corePin='444492AA954130B23FA4E9275FB14F3888BED75FF10BDC859C7243BD7D610D95'}
            Hold $coreProof $corePin
            $core=Get-Content $coreProof -Raw|ConvertFrom-Json
            if(!$core.passed -or !$core.inputsUnchanged -or $core.ownerAttention -or $core.outputs.Count -ne 3){throw 'Updated entry companion incomplete'}
            foreach($file in $core.outputs){Hold $file.path $file.sha256;$name=[IO.Path]::GetFileName($file.path);if(!$fixture.files.ContainsKey($name)){throw 'Unknown companion fixture member'};$fixture.files[$name]=$file.sha256}
            $oldWorker='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6';$newWorker='F8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C'
            if($Version -eq 7){$newWorker='5BF8B589731CB1A0A5DE38B0DAE1AF516DBF21DB5982141C9C013C55E6D39596'}
            if($Version -eq 8){$newWorker='E777E0CAE71D694BBA96E43E01BEED102B2B512B0B24B4ABE11AF2CDB3E7C2C6'}
            $originalHost=[IO.File]::ReadAllText($source)
            function ProjectHost([string]$text){if([regex]::Matches($text,$oldWorker).Count -ne 1 -or $text.Contains($newWorker)){throw 'Unique old host worker report pin required'};return $text.Replace($oldWorker,$newWorker)}
            $updatedHost=ProjectHost $originalHost
            if($updatedHost.Replace($newWorker,$oldWorker) -cne $originalHost){throw 'Host worker identity inverse differs'}
            $negative=0;foreach($invalid in @($originalHost.Replace($oldWorker,'MISSING'),($originalHost+$oldWorker),($originalHost+$newWorker))){try{$null=ProjectHost $invalid}catch{$negative++}}
            if($negative -ne 3){throw 'Host identity negative controls failed'}
            if($Version -ge 7){
                $oldParent='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
                $priorSource=Join-Path $root 'continuity-evidence/temporal-startup-v1/relative-font-script-suite-v1/WorkerEntryNativeHost.cs';Hold $priorSource '66ABE045C4F0C79B0CD0C114743FE92AFC9C0B579D9E4AF9D47F73F0C88B0183'
                $priorText=[IO.File]::ReadAllText($priorSource)
                function VerifyParent([string]$text){if([regex]::Matches($text,$parentHash).Count -ne 1 -or $text.Contains($oldParent) -or $text.Replace($parentHash,$oldParent) -cne $priorText){throw 'Exact combined parent-only projection required'}}
                VerifyParent $originalHost
                $parentNegative=0;foreach($invalid in @($originalHost.Replace($parentHash,'MISSING'),($originalHost+$originalHost),$priorText)){try{VerifyParent $invalid}catch{$parentNegative++}}
                if($parentNegative -ne 3){throw 'Combined parent negative controls failed'}
                $result.parentPinInverseVerified=$true;$result.parentPinNegativeControls=$parentNegative
            }elseif($Version -ge 5){
                $oldParent='906E3CAAE4C160C7FDCF9D7E79B08076B9A12F46A76715AE02CAE9E72983764C'
                function ProjectParent([string]$text){if([regex]::Matches($text,$oldParent).Count -ne 1 -or $text.Contains($parentHash)){throw 'Unique old parent identity required'};return $text.Replace($oldParent,$parentHash)}
                $priorHost=$updatedHost;$updatedHost=ProjectParent $priorHost
                if($updatedHost.Replace($parentHash,$oldParent) -cne $priorHost){throw 'Host parent identity inverse differs'}
                $parentNegative=0;foreach($invalid in @($priorHost.Replace($oldParent,'MISSING'),($priorHost+$oldParent),($priorHost+$parentHash))){try{$null=ProjectParent $invalid}catch{$parentNegative++}}
                if($parentNegative -ne 3){throw 'Host parent identity negative controls failed'}
                $result.parentPinInverseVerified=$true;$result.parentPinNegativeControls=$parentNegative
            }
            $source=Join-Path $out 'WorkerEntryNativeHost.cs';[IO.File]::WriteAllText($source,$updatedHost,[Text.UTF8Encoding]::new($false));Hold $source
            $result.hostWorkerPinInverseVerified=$true;$result.hostWorkerPinNegativeControls=$negative
        }
        $resource=Join-Path $out 'entry-fixture.json';$fixture|ConvertTo-Json -Depth 5|Set-Content $resource -Encoding utf8;Hold $resource
        $result.fixtureDirectory=$fixture.directory
    }
    $compiler='C:\Program Files\dotnet\dotnet.exe';Hold $compiler
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $toolchain $name)}
    $exe=Join-Path $out 'WorkerEntryNativeHost.exe'
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/langversion:12.0','/optimize+','/deterministic+','/warnaserror+',('/out:"'+$exe+'"'),('/resource:"'+$resource+'",Lea.EntryFixture.json'))
    $references='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll')){$path=Join-Path $references $name;Hold $path;$arguments+=('/reference:"'+$path+'"')}
    $arguments+=('"'+$source+'"')
    $recipe=Join-Path $out 'entry.rsp';[IO.File]::WriteAllLines($recipe,$arguments,[Text.UTF8Encoding]::new($false));Hold $recipe
    $run=Invoke-OwnedTestRun -Name 'compile-entry-host' -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$recipe+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$run.passed){throw 'SDK entry host compile failed'};Hold $exe
    $original='I:\LeaBuildStorage\sdk-full-script-preparation-20261101-v1\baseline-runtime\WorkerEntryProbe\manifest.json'
    Hold $original '5F5C0E04DBF024C07993024591CE3485D7199DFC8597EEFB617D01758868376D'
    $spec=Get-Content $original -Raw|ConvertFrom-Json -AsHashtable
    $spec.files['Lea.Script.dll']=$parentHash
    $spec.files['WorkerEntryNativeHost.exe']=(Get-FileHash $exe).Hash
    $spec.fixtureFiles=(Get-Content $resource -Raw|ConvertFrom-Json -AsHashtable).files
    $manifest=Join-Path $out 'entry-manifest.json';$spec|ConvertTo-Json -Depth 5|Set-Content $manifest -Encoding utf8;Hold $manifest
    $old='5F5C0E04DBF024C07993024591CE3485D7199DFC8597EEFB617D01758868376D';$new=(Get-FileHash $manifest).Hash
    $originalSource=Join-Path $stage 'source/ModernComponents.cs';$text=[IO.File]::ReadAllText($originalSource)
    if($Version -ge 2){
        $probeRoot='I:\LeaBuildStorage\sdk-component-probes-20261101-v2'
        $probePin='3A4F36BE459CC0CDB82AAE9D149C806909AF56062197BB1773DC6B817090CF7B'
        if($Version -eq 7){$probeRoot='I:\LeaBuildStorage\sdk-component-probes-20261101-v3';$probePin='436983AEF289AC8B205F8878FBF7BD31FCFF079FB14F9B99B6D3DCB26F7FC953'}
        if($Version -eq 8){$probeRoot='I:\LeaBuildStorage\sdk-component-probes-20261101-v4';$probePin='97CD0F8118FF656CB7A14B5F2451EC87CE3F7A3B0627B22B5D757D3D76C352D7'}
        $probeProof=Join-Path $probeRoot 'result.json';Hold $probeProof $probePin
        $probes=Get-Content $probeProof -Raw|ConvertFrom-Json
        if(!$probes.passed -or !$probes.inputsUnchanged -or $probes.ownerAttention -or !$probes.dispatcherInverseVerified){throw 'Probe dispatcher preparation incomplete'}
        $selected=@($probes.outputs|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'ModernComponents.cs'})
        if($selected.Count -ne 1){throw 'Unique probe dispatcher required'}
        Hold $selected[0].path $selected[0].sha256
        $text=[IO.File]::ReadAllText($selected[0].path)
        $old='9BDB2E7FEBD8C3B96016C29736E42FCBD641A44AE19BE8119B0C41CA60811586'
    }
    if([regex]::Matches($text,$old).Count -ne 1 -or $text.Contains($new)){throw 'Unique old entry binding required'}
    $updated=$text.Replace($old,$new);if($updated.Replace($new,$old) -cne $text){throw 'Entry dispatcher inverse differs'}
    $dispatcher=Join-Path $out 'ModernComponents.cs';[IO.File]::WriteAllText($dispatcher,$updated,[Text.UTF8Encoding]::new($false));Hold $dispatcher
    $result.outputs=@(foreach($path in @($exe,$manifest,$dispatcher)){@{path=$path;sha256=(Get-FileHash $path).Hash}})
    $result.entryBinding=@{old=$old;replacement=$new;inverseVerified=$true;compiledHostSha256=(Get-FileHash $exe).Hash}
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSdkEntryLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
