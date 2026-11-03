param([switch]$Replay)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v8'
$hostRoot='I:\LeaBuildStorage\contains-recovery-entry-20261102-v1'
$traceRoot='I:\LeaBuildStorage\contains-recovery-trace-source-20261102-v1'
$out='I:\LeaBuildStorage\contains-recovery-assembly-20261102-v1'
if($Replay){$hostRoot='I:\LeaBuildStorage\retirement-full-entry-20261102-v2';$out='I:\LeaBuildStorage\retirement-prefix-diagnostic-20261102-v2'}
if(Test-Path $out){throw 'Preserve diagnostic assembly'}
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Pin([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse source refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    if((Get-FileHash $path).Hash -cne $pin){throw 'Diagnostic assembly input differs'};$inputs.Add(@{path=$path;sha256=$pin})
}
function Proof([string]$path,[string]$pin){Pin $path $pin;return Get-Content $path -Raw|ConvertFrom-Json}
try{
    $original=Proof (Join-Path $base 'assembly.json') 'D1E745E50AFE789D845BB43B27216BF52A555D0A29FF8E65F511AE5874430BCB'
    $hostPin=if($Replay){'2E913DDA683A9AF5684BB28EDDCD743D7BCE7971D1125607FDC9698B5E18D659'}else{'5D9D93F187EBC88C34929855DF0E94C082839F8A1B66EA05E3EC52BE58020C79'}
    $hostBuild=Proof (Join-Path $hostRoot 'result.json') $hostPin
    $priorHost=Proof 'I:\LeaBuildStorage\sdk-full-script-entry-20261101-v8/result.json' '5FE22B8F112411009E9C7DF70654A372480FD7EBD3142E71BBDF3E15E7100F56'
    if(!$Replay){$trace=Proof (Join-Path $traceRoot 'source.json') '8D3789D0D3B41D1B6057A4186CA461BD7A113F0C43CB727C59AF5C87D78E6BAB'}
    if(!$original.verified -or $original.files.Count -ne 305 -or $original.compiledSources -ne 60 -or $original.fixtures -ne 182 -or !$hostBuild.passed -or !$hostBuild.inputsUnchanged -or $hostBuild.ownerAttention -or !$hostBuild.parentPinInverseVerified -or $hostBuild.parentPinNegativeControls -ne 3 -or !$hostBuild.hostWorkerPinInverseVerified -or $hostBuild.hostWorkerPinNegativeControls -ne 3 -or !$hostBuild.entryBinding.inverseVerified){throw 'Prefix prerequisites incomplete'}
    if(!$Replay -or !$hostBuild.diagnosticOnly -or !$hostBuild.diagnosticParentInverseVerified -or $hostBuild.diagnosticParentSha256 -cne 'E5896C436C840022CC4B840F866F7D7F5D92B5E0CA1B546B20AC19C5A1FB1111'){throw 'Exact diagnostic parent binding required'}
    if(!$Replay -and (!$hostBuild.diagnosticOnly -or !$trace.verified -or !$trace.inverseVerified -or $trace.negativeControls -ne 3 -or $trace.assertionsChanged -or $trace.engineChanged -or $trace.budgetsChanged)){throw 'Diagnostic projection prerequisites incomplete'}
    $expected=Join-Path $root 'continuity-evidence/contains-recovery-full-prefix-v1/runtime/WorkerEntryFixture'
    if($Replay){$expected=Join-Path $root 'continuity-evidence/retirement-full-prefix-v1/runtime/WorkerEntryFixture'}
    if($hostBuild.fixtureDirectory -cne $expected){throw 'Diagnostic fixture targets wrong candidate'}
    $selected=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $original.files){$from=Join-Path $base $file.path;Pin $from $file.sha256;$selected.Add($file.path,@{path=$file.path;origin=$from;sha256=$file.sha256})}
    $changes=[Collections.Generic.List[object]]::new()
    function Replace([string]$relative,[string]$path,[string]$hash){
        if(!$selected.ContainsKey($relative)){throw 'No original diagnostic member'};Pin $path $hash
        $changes.Add(@{path=$relative;originalSha256=$selected[$relative].sha256;sha256=$hash})
        $selected[$relative]=@{path=$relative;origin=$path;sha256=$hash}
    }
    $contains='compiled-source/source/Lea.Script.Tests/Contains.cs'
    if($Replay){
        if($selected[$contains].sha256 -cne '809837EC846B255428FA99FF6A25452D29CAD31F9EBDDECE90D34BA11DA9600B' -or [IO.File]::ReadAllText($selected[$contains].origin).Contains('LEA_CONTAINS_RECOVERY')){throw 'Replay must retain original Contains'}
    }else{
    if($selected[$contains].sha256 -cne $trace.originalSha256){throw 'Original Contains differs'}
    Pin $trace.source $trace.sha256
    $updated=[IO.File]::ReadAllText($trace.source)
    $insertions='(?m)^\s*(?:var recoveryTrace = Stopwatch.StartNew\(\);|var recoveryStep = 0;|var traceStep = \+\+recoveryStep;|Console.Error.WriteLine\("LEA_CONTAINS_RECOVERY [^\r\n]*;)\r?\n'
    if([regex]::Matches($updated,$insertions).Count -ne 9 -or [regex]::Replace($updated,$insertions,'') -cne [IO.File]::ReadAllText($selected[$contains].origin)){throw 'Contains diagnostic exact inverse differs'}
    Replace $contains $trace.source $trace.sha256
    }
    $dispatcher='compiled-source/source/Lea.Script.Tests/ModernComponents.cs'
    $outputs=@($hostBuild.outputs|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'ModernComponents.cs'})
    if($outputs.Count -ne 1){throw 'Unique new dispatcher required'}
    Pin $outputs[0].path $outputs[0].sha256
    $oldText=[IO.File]::ReadAllText($selected[$dispatcher].origin);$newText=[IO.File]::ReadAllText($outputs[0].path)
    $oldHash=$priorHost.entryBinding.replacement;$newHash=$hostBuild.entryBinding.replacement
    if([regex]::Matches($oldText,$oldHash).Count -ne 1 -or [regex]::Matches($newText,$newHash).Count -ne 1 -or $newText.Replace($newHash,$oldHash) -cne $oldText){throw 'Dispatcher common-original inverse differs'}
    Replace $dispatcher $outputs[0].path $outputs[0].sha256
    foreach($pair in @(@('WorkerEntryNativeHost.exe','runtime/WorkerEntryProbe/WorkerEntryNativeHost.exe'),@('entry-manifest.json','runtime/WorkerEntryProbe/manifest.json'))){
        $files=@($hostBuild.outputs|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $pair[0]})
        if($files.Count -ne 1){throw 'Unique entry replacement required'};Replace $pair[1] $files[0].path $files[0].sha256
    }
    $diagnostic='I:\LeaBuildStorage\worker-retirement-trace-build-20261102-v2\source\bin\Release\Lea.Script.dll'
    if($selected['runtime/Lea.Script.dll'].sha256 -cne 'D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'){throw 'Original script binding differs'}
    Replace 'runtime/Lea.Script.dll' $diagnostic 'E5896C436C840022CC4B840F866F7D7F5D92B5E0CA1B546B20AC19C5A1FB1111'
    if($changes.Count -ne 4){throw 'Exact prefix member change count differs'}
    New-Item -ItemType Directory -Path $out|Out-Null
    foreach($file in $selected.Values){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split('/') -contains '..'){throw 'Assembly escape'}
        $to=Join-Path $out $file.path;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
        Copy-Item -LiteralPath $file.origin -Destination $to
        if((Get-FileHash $to).Hash -cne $file.sha256){throw 'Diagnostic member readback differs'}
    }
    @{verified=$true;files=@($selected.Values|Sort-Object path);inputs=$inputs;changes=$changes;compiledSources=60;fixtures=182;baselineTestBinariesExcluded=$true;compiled=$false;executed=$false;notRunnable=$true;diagnosticOnly=$true;uninstrumentedReplay=$false;fullPrefixRetained=$true;containsInverseVerified=$true;dispatcherInverseVerified=$true;requiredChecks=3412;requiredPhases=89;compileBudgetMs=120000;testBudgetMs=300000;cleanupBudgetMs=5000;entryFixtureDirectory=$expected;scope=$(if($Replay){'Diagnostic script DLL plus exact entry-resource/manifest/dispatcher bindings; all original test assertions, Contains and phase calls retained. Not release evidence'}else{'Only Contains stderr instrumentation and entry-resource rebinding; engine products/all phase calls unchanged, timing perturbation possible'})}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'assembly.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'assembly.json')
}finally{foreach($s in $holds){$s.Dispose()}}
