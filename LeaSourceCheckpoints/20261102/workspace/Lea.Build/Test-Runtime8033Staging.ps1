$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/runtime-8033-staging-controls-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve staging controls'}
$fragment=Join-Path $PSScriptRoot 'Runtime8033Staging.fragment.ps1'
if((Get-FileHash -LiteralPath $fragment).Hash -cne 'B31F165476E316ECE4783877C0A33C26134F14581087F805EE32AFAEBB3E9FD8'){throw 'Staging fragment changed'}
$null=New-Item -ItemType Directory -Path $out
$runtime=Join-Path $out 'runtime'
$Workspace=$workspace
$pins=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Expected){
    if((Get-FileHash -LiteralPath $Path).Hash -cne $Expected){throw 'Control input hash differs'}
    $pins.Add(@{path=$Path;sha256=$Expected})
}
. $fragment
$rows=[Collections.Generic.List[object]]::new()
foreach($config in $configProof.configs){
    $to=Join-Path $runtime $config.relative
    $null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
    $hash=Stage-Runtime8033Input $config.source $to $config.sourceSha256
    if($hash -cne $config.targetSha256 -or [IO.File]::ReadAllText($to) -cne [IO.File]::ReadAllText($config.target)){throw 'Projection readback differs'}
    $rows.Add(@{name='stage-'+$config.relative;passed=$true})
}
if($configUsed.Count -ne 8){throw 'Projection coverage differs'}
$first=$configProof.configs[0]
function Expect-Rejection([string]$Name,[scriptblock]$Action,[string]$Message){
    $caught=$false
    try{& $Action}catch{
        if(!$_.Exception.Message.Contains($Message)){throw}
        $caught=$true
    }
    if(!$caught){throw "Missing rejection: $Name"}
    $rows.Add(@{name=$Name;passed=$true})
}
Expect-Rejection 'duplicate-config' {Stage-Runtime8033Input $first.source (Join-Path $runtime $first.relative) $first.sourceSha256} 'Config original identity or coverage differs'
Expect-Rejection 'path-escape' {Stage-Runtime8033Input $first.source (Join-Path $out 'escape.json') $first.sourceSha256} 'Staging path escape'
Expect-Rejection 'wrong-source-hash' {Stage-Runtime8033Input $first.source (Join-Path $runtime $first.relative) ('0'*64)} 'Control input hash differs'
# Correctly hashed but wrong original bytes must not be accepted as a config source.
Expect-Rejection 'wrong-original-identity' {Stage-Runtime8033Input $first.target (Join-Path $runtime $first.relative) $first.targetSha256} 'Config original identity or coverage differs'
if(Test-Path -LiteralPath (Join-Path $out 'escape.json')){throw 'Path escape wrote a file'}
foreach($pin in $pins){if((Get-FileHash -LiteralPath $pin.path).Hash -cne $pin.sha256){throw 'Pinned source changed'}}
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
Copy-Item -LiteralPath $fragment -Destination (Join-Path $out 'fragment.ps1')
$result=@{passed=$true;controls=@($rows);inputsUnchanged=$true;runtimeExecuted=$false;candidateQualified=$false;scope='Exact production staging fragment tested on eight small config copies with four rejection controls; no compiler, engine or worker launched'}
$result|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'result.json')
[pscustomobject]@{passed=$true;controls=$rows.Count;runtimeExecuted=$false}|ConvertTo-Json
