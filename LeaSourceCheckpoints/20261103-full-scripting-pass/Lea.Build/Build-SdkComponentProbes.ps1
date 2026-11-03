param([switch]$KeepForegroundHost,[ValidateRange(1,4)][int]$Version=1,[switch]$PrivateBindings,[ValidatePattern('^[A-F0-9]{64}$')][string]$PrivateBindingSourceSha256,[ValidateRange(1,2)][int]$PrivateBindingSourceVersion=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$sources='I:\LeaBuildStorage\sdk-component-probe-sources-20261101-v'+$Version
$assembly='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v1'
$base=Join-Path $root 'continuity-evidence/bootstrap-descriptor-batches-v1/source-cross-module-probes-v1'
$out='I:\LeaBuildStorage\sdk-component-probes-20261101-v'+$Version
$assemblyPin='ABB3D08DDAB7FE8068E79F81B0ED0256FE6564EAE4C099CF77238EA8E91A5CF3'
if($PrivateBindings){
    if($Version -ne 4 -or !$PrivateBindingSourceSha256){throw 'Private probes require separate pinned v4 mode'}
    $sources='I:\LeaBuildStorage\private-binding-component-sources-20261103-v'+$PrivateBindingSourceVersion
    $out='I:\LeaBuildStorage\private-binding-component-build-20261103-v'+$PrivateBindingSourceVersion
    $assembly='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v8'
    $assemblyPin='D1E745E50AFE789D845BB43B27216BF52A555D0A29FF8E65F511AE5874430BCB'
}elseif($PrivateBindingSourceSha256){throw 'Unexpected private probe pin'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence/reserve required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$products=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse build input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($pin -and $pin -cne $hash){throw ('Probe build input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;outputs=$products;executed=$false;assertionsChanged=$false;referencePackProjection='8.0.32 to installed8.0.33';completeSdk=$false}
try{
    Hold $PSCommandPath
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $proof=Join-Path $base 'result.json';Hold $proof '93BE3A4BD4434131AC6CBCA065EAA0966339891D79CBA66FB69554C473F6BD7A';$prior=Get-Content $proof -Raw|ConvertFrom-Json
    $sourcePin=if($Version -eq 2){'7A8A221F702FCA7DF2B7386C29DA65461DA5900A51F875D17914A8FF0E4ED9C4'}else{'CE9DFBB0C2A7DA6F04B9821176B06EE6AC90CD52C422FF4D3ACA1F38DCFD5BE7'}
    if($Version -eq 3){$sourcePin='32A7635DA8C3C6566451F0445B895BD80052796F4CEBCFD968D7989182EF4526'}
    if($Version -eq 4){$sourcePin='AF66E1B7EC800A6C155C676E92E65B99A2F732151A55F82822505F7A328E9912'}
    if($PrivateBindings){$sourcePin=$PrivateBindingSourceSha256}
    $sourceProof=Join-Path $sources 'sources.json';Hold $sourceProof $sourcePin;$projected=Get-Content $sourceProof -Raw|ConvertFrom-Json
    if($PrivateBindings -and $PrivateBindingSourceVersion -eq 2){
        $asset=$projected.resourceIdentityChange
        if(!$asset.verified -or !$asset.inverseVerified -or $asset.negativeControls -ne 3 -or $asset.changedSources -ne 1 -or $asset.replacement -cne '196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3'){throw 'Verified bootstrap asset pin projection required'}
        Hold $asset.asset $asset.replacement
    }
    $assemblyProof=Join-Path $assembly 'assembly.json';Hold $assemblyProof $assemblyPin;$members=(Get-Content $assemblyProof -Raw|ConvertFrom-Json).files
    if($Version -eq 3){
        $nativeProof='I:\LeaBuildStorage\full-sdk-native-build-20261101-v1/result.json';Hold $nativeProof '20E2EE1F8A87BFC2B14CD581E48B59D1DE834E87EBA729DCB5A45CCF4E1A7893'
        $native=Get-Content $nativeProof -Raw|ConvertFrom-Json
        $module=@($native.nativeProducts|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Lea.Script.Worker.dll'})
        if(!$native.passed -or !$native.inputsUnchanged -or !$native.workerAclRestored -or $native.ownerAttention -or $module.Count -ne 1 -or $module[0].sha256 -cne $projected.selectedWorkerSha256){throw 'Combined native module selection differs'}
        Hold $module[0].path $module[0].sha256;$result.workerModule=$module[0]
    }
    $expectedSources=if($Version -ge 2){6}else{5}
    if($Version -eq 4 -and !$PrivateBindings){
        $buildProof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json';Hold $buildProof '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
        $build=Get-Content $buildProof -Raw|ConvertFrom-Json
        $selected=@($build.products|Where-Object path -ceq 'bin/Release/ScriptWorker/Lea.Script.Worker.dll')
        if($selected.Count -ne 1 -or !$selected[0].equal -or $selected[0].sha256 -cne $projected.selectedWorkerSha256 -or $selected[0].secondSha256 -cne $selected[0].sha256){throw 'Deterministic module selection differs'}
        Hold $selected[0].first $selected[0].sha256;Hold $selected[0].second $selected[0].sha256
        $module=@(@{path=$selected[0].first;sha256=$selected[0].sha256});$result.workerModule=$module[0]
    }
    if($PrivateBindings){
        $buildProof='I:\LeaBuildStorage\private-binding-worker-build-20261102-v1/result.json';Hold $buildProof '302C3003416F9095BA56C214FF6FD8B4F05A82F102CBAAF53B23DC3FAEE6E398'
        $build=Get-Content $buildProof -Raw|ConvertFrom-Json
        $module=@($build.products|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Lea.Script.Worker.dll'})
        if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $module.Count -ne 1 -or $module[0].sha256 -cne $projected.selectedWorkerSha256 -or $module[0].sha256 -cne 'F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0'){throw 'Private probe module differs'}
        Hold $module[0].path $module[0].sha256;$result.workerModule=$module[0];$result.privateBindingPair=$true
    }
    if(!$projected.verified -or $projected.assertionsChanged -or $projected.files.Count -ne $expectedSources){throw 'Complete verified probe projections required'}
    function Prior([string]$path){$pins=@($prior.inputs|Where-Object path -ceq $path|Select-Object -ExpandProperty sha256 -Unique);if($pins.Count -ne 1){throw ('Probe provenance absent: '+$path)};Hold $path $pins[0]}
    $compiler='C:\Program Files\dotnet\dotnet.exe';Hold $compiler
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $toolchain $name)}
    $dispatcherMember=@($members|Where-Object {$_.path.StartsWith('compiled-source/') -and [IO.Path]::GetFileName($_.path) -ceq 'ModernComponents.cs'})
    if($dispatcherMember.Count -ne 1){throw 'Unique dispatcher required'}
    $dispatcherPath=Join-Path $assembly $dispatcherMember[0].path;Hold $dispatcherPath $dispatcherMember[0].sha256
    $originalDispatcher=[IO.File]::ReadAllText($dispatcherPath);$dispatcher=$originalDispatcher
    $changes=[Collections.Generic.List[object]]::new()
    foreach($source in $projected.files|Where-Object {[IO.Path]::GetFileName($_.path) -cne 'OptionsUnit.cs'}){
        Hold $source.path $source.sha256
        $component=$source.component;$directory=Join-Path $out $component;New-Item -ItemType Directory -Path $directory|Out-Null
        $recipe=Join-Path $base ('production-'+$component+'/probe.rsp');Prior $recipe
        $manifestRelative='runtime/ComponentProbes/'+$component+'/manifest.json'
        $manifestMember=@($members|Where-Object path -ceq $manifestRelative)
        if($manifestMember.Count -ne 1){throw 'Component manifest provenance absent'}
        $manifestPath=Join-Path $assembly $manifestRelative;Hold $manifestPath $manifestMember[0].sha256
        $spec=Get-Content $manifestPath -Raw|ConvertFrom-Json -AsHashtable
        if($Version -ge 3){$priorWorker=if($PrivateBindings){'E777E0CAE71D694BBA96E43E01BEED102B2B512B0B24B4ABE11AF2CDB3E7C2C6'}else{'F8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C'};if($spec.files['Lea.Script.Worker.dll'] -cne $priorWorker){throw 'Original manifest module identity differs'};$spec.files['Lea.Script.Worker.dll']=$module[0].sha256}
        $binary=Join-Path $directory $spec.binary;$options=[Collections.Generic.List[string]]::new();$sourceCount=0;$outputCount=0;$projectedCount=0
        foreach($line in Get-Content $recipe){
            if($line -match '^"(.+\.cs)"$'){
                $from=$Matches[1];Prior $from
                $projection=@($projected.files|Where-Object origin -ceq $from)
                if($projection.Count -gt 1){throw 'Duplicate projected source'}
                if($projection.Count -eq 1){Hold $projection[0].path $projection[0].sha256;$from=$projection[0].path;$projectedCount++}
                $options.Add('"'+$from+'"');$sourceCount++;continue
            }
            if($line -match '^/reference:"(.+)"$'){
                $from=$Matches[1]
                $prefix='C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.32\ref\net8.0\'
                if($from.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)){$from='C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.33\ref\net8.0\'+$from.Substring($prefix.Length);Hold $from}
                else{
                    $name=[IO.Path]::GetFileName($from)
                    if($name -notin @('Jint.dll','Acornima.dll')){throw 'Unexpected component reference'}
                    $relative='runtime/ComponentProbes/'+$component+'/'+$name;$member=@($members|Where-Object path -ceq $relative)
                    if($member.Count -ne 1){throw 'Interpreter reference provenance absent'}
                    $from=Join-Path $assembly $relative;Hold $from $member[0].sha256
                }
                $options.Add('/reference:"'+$from+'"');continue
            }
            if($line.StartsWith('/out:')){$options.Add('/out:"'+$binary+'"');$outputCount++;continue}
            if(!$line.StartsWith('/')){throw 'Unexpected probe recipe line'}
            $options.Add($line)
        }
        $expectedProjections=if($Version -ge 2 -and $component -ceq 'bootstrap'){2}else{1}
        if($outputCount -ne 1 -or $projectedCount -ne $expectedProjections -or $sourceCount -lt 1){throw 'Probe recipe projection incomplete'}
        $rsp=Join-Path $directory 'probe.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));Hold $rsp
        $run=Invoke-OwnedTestRun -Name ('compile-'+$component) -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $directory -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$run.passed){throw ('Probe compilation failed: '+$component)}
        Hold $binary;$binaryHash=(Get-FileHash $binary).Hash;$spec.files[$spec.binary]=$binaryHash
        $manifest=Join-Path $directory 'manifest.json';$spec|ConvertTo-Json -Depth 5|Set-Content $manifest -Encoding utf8;Hold $manifest
        $old=$manifestMember[0].sha256;$new=(Get-FileHash $manifest).Hash
        if([regex]::Matches($dispatcher,$old).Count -ne 1 -or $dispatcher.Contains($new)){throw 'Unique old probe manifest pin required'}
        $dispatcher=$dispatcher.Replace($old,$new);$changes.Add(@{old=$old;replacement=$new;component=$component;sourceFiles=$sourceCount})
        foreach($path in @($binary,$manifest)){$products.Add(@{path=$path;sha256=(Get-FileHash $path).Hash})}
    }
    $inverse=$dispatcher;foreach($change in $changes){$inverse=$inverse.Replace($change.replacement,$change.old)}
    if($inverse -cne $originalDispatcher -or $changes.Count -ne 5){throw 'Dispatcher five-manifest inverse differs'}
    $updated=Join-Path $out 'ModernComponents.cs';[IO.File]::WriteAllText($updated,$dispatcher,[Text.UTF8Encoding]::new($false));Hold $updated
    $products.Add(@{path=$updated;sha256=(Get-FileHash $updated).Hash});$result.dispatcherChanges=$changes;$result.dispatcherInverseVerified=$true;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSdkProbeBuildLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
