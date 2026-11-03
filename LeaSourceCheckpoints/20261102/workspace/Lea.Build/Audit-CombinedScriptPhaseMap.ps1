$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\combined-script-phase-map-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve phase map'}
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Phase map input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
function Unique($map,[string]$type,[string]$name){$m=@($map.methods|Where-Object {$_.type -ceq $type -and $_.name -ceq $name});if($m.Count -ne 1){throw ('Unique phase method required: '+$type+'.'+$name)};return $m[0]}
function Has-Call($method,[string]$name){return @($method.instructions|Where-Object {$_ -match ('\bcall(virt)? '+[regex]::Escape($name)+'(<spec>)?$')}).Count -gt 0}
try{
    $proof='I:\LeaBuildStorage\combined-trusted-counters-20261101-v1/result.json';Pin $proof '3652E3E658105CB7BEBC0EB7DFD112D3026B93940C275222746B548125BB921C'
    $run=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$run.diagnosticCaptured -or !$run.inputsUnchanged -or $run.ownerAttention -or !$run.firstCallUnwrapped -or $run.changedFiles.Count){throw 'Matched trusted diagnostic required'}
    $runtime=Split-Path $run.before[0].path
    $runtime='I:\LeaBuildStorage\combined-trusted-counters-20261101-v1/runtime'
    $jint=Join-Path $runtime 'Jint.dll';Pin $jint 'B54618A4E3FFE33344D478D3A1580658C4D91819B3B97A24326CB3626CB7FD1A'
    $parent=Join-Path $runtime 'Lea.Script.dll';Pin $parent '36AB8810EA87DDC6D3A840B629772210408F1ABEAF7341E204442E1275D79304'
    $decoder=Join-Path $PSScriptRoot 'ReadFailureIL.cs';Pin $decoder 'A27FC129FABD195808AA2564EB78CF275E8E0AF325D4D01EE6C8B95DC009F6B0'
    $mapSource=Join-Path $PSScriptRoot 'CombinedScriptPhaseMap.cs';Pin $mapSource
    if(@([AppDomain]::CurrentDomain.GetAssemblies()|Where-Object { $_.GetName().Name -in 'Jint','Lea.Script' }).Count){throw 'Metadata-only host must not load target assemblies'}
    Add-Type -Path @($decoder,$mapSource)
    $vm=[CombinedScriptPhaseMap]::Read($jint);$hostMap=[CombinedScriptPhaseMap]::Read($parent)
    $execute=@($vm.methods|Where-Object {$_.type -ceq 'Jint.Engine' -and $_.name -ceq 'Execute' -and (Has-Call $_ 'ParseScriptGuarded')})
    if($execute.Count -ne 2){throw 'Expected two direct string parsing overloads'}
    foreach($method in $execute){if(!(Has-Call $method 'Execute')){throw 'String execution does not feed prepared execution'}}
    $prepared=@($vm.methods|Where-Object {$_.type -ceq 'Jint.Engine' -and $_.name -ceq 'Execute' -and $_.parameters -eq 1})
    if($prepared.Count -ne 1 -or !(Has-Call $prepared[0] 'ExecuteForCompletion')){throw 'Prepared execution boundary differs'}
    $completion=Unique $vm 'Jint.Engine' 'ExecuteForCompletion'
    if(!(Has-Call $completion 'ExecuteWithConstraints')){throw 'Constrained execution entry differs'}
    $constrained=Unique $vm 'Jint.Engine' 'ExecuteWithConstraints'
    if(!(Has-Call $constrained 'ResetConstraints')){throw 'Constraint reset boundary absent'}
    $time=Unique $vm 'Jint.Constraints.TimeConstraint' 'Check'
    $reset=Unique $vm 'Jint.Constraints.TimeConstraint' 'Reset'
    if(!(Has-Call $time 'GetTimestamp') -or !(Has-Call $reset 'GetTimestamp')){throw 'Exact clock-based time constraint differs'}
    $runMethod=Unique $hostMap 'Lea.Script.Js' 'Run';$start=Unique $hostMap 'Lea.Script.Js' 'Start';$options=Unique $hostMap 'Lea.Script.Vo' 'At'
    if(!(Has-Call $runMethod 'Execute') -or !(Has-Call $start 'Evaluate') -or !(Has-Call $options 'TimeoutInterval')){throw 'Host script/bootstrap budget calls differ'}
    if(@([AppDomain]::CurrentDomain.GetAssemblies()|Where-Object { $_.GetName().Name -in 'Jint','Lea.Script' }).Count){throw 'Target interpreter was unexpectedly loaded'}
    foreach($input in $inputs){if((Get-FileHash $input.path).Hash -cne $input.sha256){throw 'Phase map input changed'}}
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;interpreterExecuted=$false;targetAssembliesLoaded=$false;causeProven=$false;productionChanged=$false;runtimeJintSha256=(Get-FileHash $jint).Hash;runtimeParentSha256=(Get-FileHash $parent).Hash;stringParsingPrecedesPreparedExecution=$true;preparedExecutionUsesConstraintWrapper=$true;timeConstraintUsesTimestamp=$true;vm=$vm;hostMap=$hostMap;inputs=$inputs;scope='Selected current PE IL boundaries, not whole-method CFG equivalence, exclusive timings or a timeout cause. No parsing/preparation or budget change authorized by this map.'}|ConvertTo-Json -Depth 10|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
