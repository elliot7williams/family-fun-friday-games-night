param([ValidateRange(2,8)][int]$Version=2)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v1'
$probes='I:\LeaBuildStorage\sdk-component-probes-20261101-v2'
if($Version -eq 7){$probes='I:\LeaBuildStorage\sdk-component-probes-20261101-v3'}
if($Version -eq 8){$probes='I:\LeaBuildStorage\sdk-component-probes-20261101-v4'}
$entry='I:\LeaBuildStorage\sdk-full-script-entry-20261101-v'+$Version
$out='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v'+$Version
if(Test-Path -LiteralPath $out){throw 'Preserve v2 assembly evidence'}
$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse assembly input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    if((Get-FileHash -LiteralPath $path).Hash -cne $hash){throw ('V2 assembly input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
function Proof([string]$path,[string]$hash){Pin $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
$original=Proof (Join-Path $base 'assembly.json') 'ABB3D08DDAB7FE8068E79F81B0ED0256FE6564EAE4C099CF77238EA8E91A5CF3'
$probePin=if($Version -eq 7){'436983AEF289AC8B205F8878FBF7BD31FCFF079FB14F9B99B6D3DCB26F7FC953'}else{'3A4F36BE459CC0CDB82AAE9D149C806909AF56062197BB1773DC6B817090CF7B'}
if($Version -eq 8){$probePin='97CD0F8118FF656CB7A14B5F2451EC87CE3F7A3B0627B22B5D757D3D76C352D7'}
$compiled=Proof (Join-Path $probes 'result.json') $probePin
$hostPin=if($Version -eq 5){'B8126C3AC7F3E0C3A22A82CD6CF16F0782025064D9FD7FE76269480705215E0D'}elseif($Version -eq 4){'E2D5BA60162158CA8DF06F05A4D01846148B2D6FEE5E918C39C8A64764431930'}elseif($Version -eq 3){'C06EF41BC485CF9B3AE6FFC232A87C52BD779D1D133A3715E628104B7B59657E'}else{'FADCD305CF1A6871B2513A7A7D8DC985FA424C7C0610D5183430CBD6D83C9852'}
if($Version -eq 6){$hostPin='DC4A7F6CB63ECF78556D2DD20CDFEA83E138C30C7FA547BBA94A273524FAE204'}
if($Version -eq 7){$hostPin='9DF8BD45DF373E83AC45A1D5CB3E092F71160730DF624E6BFC6969D0A3BCD02E'}
if($Version -eq 8){$hostPin='5FE22B8F112411009E9C7DF70654A372480FD7EBD3142E71BBDF3E15E7100F56'}
$hostBuild=Proof (Join-Path $entry 'result.json') $hostPin
$runtimeVersion=if($Version -eq 7){3}else{2}
$runtimePin=if($Version -eq 7){'49904DB790CB7286FC8DC1887B5793616D5D649EC2BEAF9866195FD90EDF3F86'}else{'D66767E5C517FD9F1D99E20FAEF8211B16662A9CCCE6AB3AFE07C437970E6B20'}
if($Version -eq 8){$runtimeVersion=4;$runtimePin='44FFA4F443AC7680721CE56E86334FD1A1D7708049C78660B24C9D4C2DEC53BC'}
$runtimeProof=Proof (Join-Path $root ('continuity-evidence/sdk-component-probes-runtime-v'+$runtimeVersion+'/result.json')) $runtimePin
if(!$original.verified -or !$compiled.passed -or !$compiled.inputsUnchanged -or $compiled.ownerAttention -or !$compiled.dispatcherInverseVerified -or !$hostBuild.passed -or !$hostBuild.inputsUnchanged -or $hostBuild.ownerAttention -or !$hostBuild.entryBinding.inverseVerified -or !$runtimeProof.passed -or $runtimeProof.reports.Count -ne 5){throw 'V2 integration prerequisite incomplete'}
$selected=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($file in $original.files){$from=Join-Path $base $file.path;Pin $from $file.sha256;$selected.Add($file.path,@{path=$file.path;origin=$from;sha256=$file.sha256})}
function Replace([string]$relative,[string]$from,[string]$hash){
    if(!$selected.ContainsKey($relative)){throw 'Replacement has no baseline member'}
    Pin $from $hash;$selected[$relative]=@{path=$relative;origin=$from;sha256=$hash}
}
if($Version -ge 7){
    if($Version -eq 8){
        $bindingsRoot='I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v4'
        $bindings=Proof (Join-Path $bindingsRoot 'bindings.json') '206B9E49C275F6305DDEFB5DEC7448502C8873001E0E24095B68C7A311A92A3D'
        $repro=Proof 'I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json' '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
        $gate=Proof 'I:\LeaBuildStorage\repro-sdk-consumers-20261101-v1/result.json' 'B8AA0C9ABCA57492875D09471D29AE809C17D89E748C2ACF3FD389161360C927'
        if(!$bindings.verified -or $bindings.assertionsChanged -or !$gate.passed -or !$gate.inputsUnchanged -or !$gate.workerAclRestored -or $gate.ownerAttention -or !$runtimeProof.inputsUnchanged -or $runtimeProof.ownerAttention -or $runtimeProof.changedFiles.Count -or !$hostBuild.parentPinInverseVerified -or $hostBuild.parentPinNegativeControls -ne 3){throw 'Deterministic SDK prerequisites incomplete'}
        $products=@($repro.products|Where-Object {$_.path -in 'bin/Release/Lea.Engine.dll','bin/Release/Lea.Win.dll','bin/Release/Lea.Image.exe','bin/Release/Lea.Script.dll','bin/Release/Lea.Script.Win.dll' -or $_.path.StartsWith('bin/Release/ScriptWorker/')})
        if($products.Count -ne 11){throw 'Deterministic role closure incomplete'}
        foreach($p in $products){if(!$p.equal -or $p.sha256 -cne $p.secondSha256){throw 'Deterministic pair differs'};Pin $p.first $p.sha256;Pin $p.second $p.sha256}
        $libraryRoles=@(foreach($p in $products|Where-Object {$_.path -notlike 'bin/Release/ScriptWorker/*'}){@{path=$p.first;sha256=$p.sha256}})
        $nativeRoles=@(foreach($p in $products|Where-Object {$_.path -like 'bin/Release/ScriptWorker/*'}){@{path=$p.first;sha256=$p.sha256}})
    }else{
    $bindingsRoot='I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v3'
    $bindings=Proof (Join-Path $bindingsRoot 'bindings.json') '9E759782A6A76F56C28DE08CB0237E35657CBAFDBBB0C0CA0317A72BFBA34EE5'
    $libraries=Proof 'I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1\result.json' '3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'
    $native=Proof 'I:\LeaBuildStorage\full-sdk-native-build-20261101-v1\result.json' '20E2EE1F8A87BFC2B14CD581E48B59D1DE834E87EBA729DCB5A45CCF4E1A7893'
    $gate=Proof 'I:\LeaBuildStorage\full-sdk-worker-gate-20261101-v1\result.json' '479876832FDD6A811032A6857E4036A9DDDBBCA3DDB1AE34C2F6A0B49EEA1EFC'
    if(!$bindings.verified -or $bindings.assertionsChanged -or !$libraries.passed -or !$libraries.inputsUnchanged -or $libraries.ownerAttention -or $libraries.libraryProducts.Count -ne 6 -or !$native.passed -or !$native.inputsUnchanged -or !$native.workerAclRestored -or $native.ownerAttention -or $native.nativeProducts.Count -ne 6 -or !$gate.passed -or $gate.ownerAttention -or !$hostBuild.parentPinInverseVerified -or $hostBuild.parentPinNegativeControls -ne 3){throw 'Combined SDK integration prerequisites incomplete'}
    $libraryRoles=@($libraries.libraryProducts|Where-Object {[IO.Path]::GetFileName($_.path) -cne 'Lea.Print.dll'});$nativeRoles=@($native.nativeProducts)
    }
    foreach($file in $bindings.files|Where-Object {$_.path.StartsWith('runtime/')}){Replace $file.path (Join-Path $bindingsRoot $file.path) $file.sha256}
    foreach($name in @('CoreProbeBridge.cs','WorkerEntryBridge.cs')){
        $target=@($selected.Keys|Where-Object {$_.StartsWith('compiled-source/') -and [IO.Path]::GetFileName($_) -ceq $name})
        $file=@($bindings.files|Where-Object path -ceq ('source/'+$name))
        $change=@($bindings.sourceChanges|Where-Object source -ceq $name)
        if($target.Count -ne 1 -or $file.Count -ne 1 -or $change.Count -ne 1 -or !$change[0].inverseVerified -or $change[0].negativeControls -ne 3){throw 'Combined bridge projection incomplete'}
        $snapshot=Join-Path 'I:\LeaBuildStorage\sdk-full-script-preparation-20261101-v1' $target[0]
        Pin $snapshot $change[0].originalSha256
        $originalText=[IO.File]::ReadAllText($snapshot)
        $oldText=[IO.File]::ReadAllText($selected[$target[0]].origin)
        $newPath=Join-Path $bindingsRoot $file[0].path;Pin $newPath $file[0].sha256
        $newText=[IO.File]::ReadAllText($newPath)
        $originalWorker='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
        $priorWorker='F8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C'
        $combinedWorker='5BF8B589731CB1A0A5DE38B0DAE1AF516DBF21DB5982141C9C013C55E6D39596'
        if($Version -eq 8){$combinedWorker='E777E0CAE71D694BBA96E43E01BEED102B2B512B0B24B4ABE11AF2CDB3E7C2C6'}
        if([regex]::Matches($oldText,$priorWorker).Count -ne 1 -or [regex]::Matches($newText,$combinedWorker).Count -ne 1 -or $oldText.Replace($priorWorker,$originalWorker) -cne $originalText -or $newText.Replace($combinedWorker,$originalWorker) -cne $originalText){throw 'Bridge common-original inverse differs'}
        Replace $target[0] (Join-Path $bindingsRoot $file[0].path) $file[0].sha256
    }
    $roles=0
    foreach($file in $libraryRoles){Replace ('runtime/'+[IO.Path]::GetFileName($file.path)) $file.path $file.sha256;$roles++}
    foreach($file in $nativeRoles){Replace ('runtime/ScriptWorker/'+[IO.Path]::GetFileName($file.path)) $file.path $file.sha256;$roles++}
    if($roles -ne 11){throw 'Combined SDK role inventory differs'}
}
if($Version -ge 3){
    if($Version -eq 8){
        $core=Proof 'I:\LeaBuildStorage\sdk-entry-core-20261101-v3/result.json' '444492AA954130B23FA4E9275FB14F3888BED75FF10BDC859C7243BD7D610D95'
        $focused=Proof (Join-Path $root 'continuity-evidence/sdk-entry-core-runtime-v3/result.json') '32257313C2E49F7CBE553CA8B29B32F28759C8495C1B5A23952B5AD5CBCFB0C3'
    }elseif($Version -eq 7){
        $core=Proof 'I:\LeaBuildStorage\sdk-entry-core-20261101-v2\result.json' '6B7FC1DE3C6A425F07146E244BC31CEEA5390C63195E0969735533B65CE85F72'
        $focused=Proof (Join-Path $root 'continuity-evidence/sdk-entry-core-runtime-v2/result.json') 'DE54D00B08DD1269B93FA431234D93DB9141B7BC37EDEE275F367F42D05732E4'
    }else{
        $core=Proof 'I:\LeaBuildStorage\sdk-entry-core-20261101-v1\result.json' '075F98C47F7FF7D2AA02E4577F8D1C4453D8C42D2A269012AE9DE3F2C64CC730'
        $focused=Proof (Join-Path $root 'continuity-evidence/sdk-entry-core-runtime-v1/result.json') 'A2FFBAC416AF66095D5DBF7E49A02BC95D77BD7CDF88F5ED3CD3630B76EAB255'
    }
    if(!$core.passed -or !$core.inputsUnchanged -or $core.ownerAttention -or $core.outputs.Count -ne 3 -or !$focused.passed -or !$focused.inputsUnchanged -or $focused.ownerAttention -or $focused.checks -ne 15 -or !$hostBuild.hostWorkerPinInverseVerified){throw 'Updated entry integration prerequisites incomplete'}
    foreach($file in $core.outputs){Replace ('runtime/WorkerEntryFixture/'+[IO.Path]::GetFileName($file.path)) $file.path $file.sha256}
}
if($Version -eq 5){
    $libraries=Proof 'I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48-session-io\result.json' 'FA92DF2B826A0918CAA47818D918C6A20B6A417BAB1255DC0E930EF832512BD3'
    $lifecycle=Proof (Join-Path $root 'continuity-evidence/sdk-script-session-io-lifecycle-v2/result.json') '8D5F034EF2EBEC7D14257270689C571B28309F6266B2A13A1610DEB4EF9DF617'
    $adapter=Proof (Join-Path $root 'continuity-evidence/sdk-script-session-io-adapter-v1/result.json') 'C7B86C94CE91B5C15285F3099CC0AD58C07C21724BD60366F10D48024B0ECD91'
    if(!$libraries.passed -or !$libraries.inputsUnchanged -or $libraries.ownerAttention -or $libraries.outputs.Count -ne 2 -or !$hostBuild.parentPinInverseVerified -or !$lifecycle.passed -or !$lifecycle.workerAclRestored -or $lifecycle.ownerAttention -or !$adapter.passed -or !$adapter.workerAclRestored -or $adapter.ownerAttention){throw 'Session I/O full integration prerequisites incomplete'}
    foreach($file in $libraries.outputs){
        $name=[IO.Path]::GetFileName($file.path);Replace ('runtime/'+$name) $file.path $file.sha256
        if($name -ceq 'Lea.Script.dll'){Replace 'runtime/WorkerEntryProbe/Lea.Script.dll' $file.path $file.sha256}
    }
}
if($Version -eq 6){
    $libraries=Proof 'I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48-pooled-io\result.json' '8730DBD801165B3E815A54777E35F4628D143AC608C0A66F160CBE7CFCC64DB0'
    $focused=@(
        @('sdk-script-pooled-io-runtime-v2','FF0A0DF9BDC982C16D0AC226E57FA47800BE578D5380F77838283361F63AA6D8'),
        @('sdk-script-pooled-io-lifecycle-v1','0C06A0C1C8B29A2B959AE1542138A455F916B7696270B3A6ABA430A4455D2F55'),
        @('sdk-script-pooled-io-native-v2','89732237035943C3B0B43EDD632083158632E832497B595EEA09BD4504A7DBAF'),
        @('sdk-script-pooled-io-adapter-v1','6E0BF5E7C6AEDC708928376FBC65661386C9A02149AF942DC1DD2C07506F98D3')
    )
    if(!$libraries.passed -or !$libraries.inputsUnchanged -or $libraries.ownerAttention -or $libraries.outputs.Count -ne 2 -or !$hostBuild.parentPinInverseVerified -or $hostBuild.parentPinNegativeControls -ne 3){throw 'Pooled I/O source build incomplete'}
    foreach($case in $focused){
        $run=Proof (Join-Path $root ('continuity-evidence/'+$case[0]+'/result.json')) $case[1]
        if(!$run.passed -or !$run.inputsUnchanged -or !$run.workerAclRestored -or $run.ownerAttention -or !$run.pooledIoPrototype){throw 'Pooled focused qualification incomplete'}
    }
    foreach($file in $libraries.outputs){
        $name=[IO.Path]::GetFileName($file.path);Replace ('runtime/'+$name) $file.path $file.sha256
        if($name -ceq 'Lea.Script.dll'){Replace 'runtime/WorkerEntryProbe/Lea.Script.dll' $file.path $file.sha256}
    }
}
foreach($change in $compiled.dispatcherChanges){
    $directory=Join-Path $probes $change.component;$manifest=Join-Path $directory 'manifest.json';Pin $manifest $change.replacement
    $spec=Get-Content $manifest -Raw|ConvertFrom-Json -AsHashtable
    $relative='runtime/ComponentProbes/'+$change.component+'/'
    Replace ($relative+'manifest.json') $manifest $change.replacement
    if([IO.Path]::GetFileName($spec.binary) -cne $spec.binary){throw 'Probe binary path escape'}
    Replace ($relative+$spec.binary) (Join-Path $directory $spec.binary) $spec.files[$spec.binary]
}
foreach($pair in @(@('WorkerEntryNativeHost.exe','runtime/WorkerEntryProbe/WorkerEntryNativeHost.exe'),@('entry-manifest.json','runtime/WorkerEntryProbe/manifest.json'))){
    $file=@($hostBuild.outputs|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $pair[0]})
    if($file.Count -ne 1){throw 'Unique entry output required'}
    Replace $pair[1] $file[0].path $file[0].sha256
}
$target=@($selected.Keys|Where-Object {$_.StartsWith('compiled-source/') -and [IO.Path]::GetFileName($_) -ceq 'ModernComponents.cs'})
$dispatcher=@($hostBuild.outputs|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'ModernComponents.cs'})
if($target.Count -ne 1 -or $dispatcher.Count -ne 1){throw 'Unique dispatcher required'}
Replace $target[0] $dispatcher[0].path $dispatcher[0].sha256
if($Version -ge 4){
    $diagnostic=Proof 'I:\LeaBuildStorage\sdk-agent-wait-diagnostic-source-20261101-v1\source.json' '7BD381AB59747BF981F45DEC81B9060231F0242F593BAEEDF9F540B08EF66A12'
    if(!$diagnostic.verified -or !$diagnostic.inverseVerified -or $diagnostic.negativeControls -ne 3 -or !$diagnostic.assertionConditionUnchanged -or !$diagnostic.successMessageUnchanged -or $diagnostic.newRuntimeCalls -or $diagnostic.budgetsChanged -or !$diagnostic.diagnosticOnly){throw 'Failure-only diagnostic projection incomplete'}
    $agent=@($selected.Keys|Where-Object {$_.StartsWith('compiled-source/') -and [IO.Path]::GetFileName($_) -ceq 'AgentWait.cs'})
    if($agent.Count -ne 1 -or $selected[$agent[0]].sha256 -cne $diagnostic.originalSha256){throw 'Diagnostic original source identity differs'}
    Replace $agent[0] $diagnostic.source $diagnostic.sha256
}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($row in $selected.Values){
    if([IO.Path]::IsPathRooted($row.path) -or $row.path.Contains(':') -or $row.path.Split('/') -contains '..'){throw 'Assembly path escape'}
    $to=Join-Path $out $row.path;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
    Copy-Item -LiteralPath $row.origin -Destination $to
    if((Get-FileHash -LiteralPath $to).Hash -cne $row.sha256){throw 'V2 assembly readback differs'}
}
foreach($input in $inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'V2 assembly input changed'}}
@{verified=$true;files=@($selected.Values|Sort-Object path);inputs=$inputs;compiledSources=60;fixtures=182;baselineTestBinariesExcluded=$true;compiled=$false;executed=$false;notRunnable=$true;diagnosticOnly=($Version -eq 4);requiredChecks=3412;requiredPhases=89;compileBudgetMs=120000;testBudgetMs=300000;cleanupBudgetMs=5000;probeHostsRebuilt=5;entryFixtureDirectory=$hostBuild.fixtureDirectory;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'assembly.json') -Encoding utf8
Get-FileHash (Join-Path $out 'assembly.json')
