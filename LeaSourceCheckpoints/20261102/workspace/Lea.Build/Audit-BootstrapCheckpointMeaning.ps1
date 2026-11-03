$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/current-bootstrap-checkpoint-v2'
if(Test-Path -LiteralPath $root){throw 'Preserve checkpoint audit'}
$null=New-Item -ItemType Directory -Path $root
$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){if((Get-FileHash -LiteralPath $path).Hash -cne $hash){throw 'Checkpoint audit input differs'};$inputs.Add(@{path=$path;sha256=$hash})}
$parent=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-flow-script-suite-v1/runtime/Lea.Script.dll'
Pin $parent 'E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
$proof=Join-Path $workspace 'continuity-evidence/permission-contract-v1/early-limits-v1/result.json'
Pin $proof '9AFFEF57588F16A4D84426B972AB8EA4E2E7F78A3F305CBE59D0402B0EDFB8AF'
$build=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.binarySha256 -cne 'E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'){throw 'Parent source build incomplete'}
foreach($name in @('2.cs','8.cs')){
    $source=Join-Path $workspace ('continuity-evidence/permission-contract-v1/early-limits-v1/source/'+$name)
    $row=@($build.sourceProofs|Where-Object copy -ceq $source)
    if($row.Count -ne 1 -or $row[0].changed){throw 'Checkpoint source binding differs'}
    Pin $source $row[0].sha256
    Copy-Item -LiteralPath $source -Destination (Join-Path $root $name)
}
$helper=Join-Path $PSScriptRoot 'ReadBootstrapResource.cs';Pin $helper (Get-FileHash -LiteralPath $helper).Hash
Add-Type -Path $helper
$data=[ReadBootstrapResource]::Read($parent)
$resourceHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($data))
if($resourceHash -cne 'EC4B6513A77230C3B8439568CC3FED41B7712CA3779182D38076880DCC83BC8A'){throw 'Embedded bootstrap identity differs'}
$text=[Text.UTF8Encoding]::new($false,$true).GetString($data)
$marker="checkpoint('document');"
$at=$text.IndexOf($marker,[StringComparison]::Ordinal)
if($at -lt 0 -or $text.LastIndexOf($marker,[StringComparison]::Ordinal) -ne $at){throw 'Document marker must occur exactly once'}
$before='eventTarget(document);eventTarget(globalThis);'
$after='delete globalThis.__lea;delete globalThis.__jobs;(function dispatch('
if($text.Substring(0,$at).EndsWith($before,[StringComparison]::Ordinal) -ne $true -or !$text.Substring($at+$marker.Length).StartsWith($after,[StringComparison]::Ordinal)){throw 'Document completion boundary differs'}
$hostSource=[IO.File]::ReadAllText((Join-Path $root '8.cs'))
if(!$hostSource.Contains('else startupStage = stage;') -or $hostSource.IndexOf('dispatch = vm.Evaluate(syntax);') -gt $hostSource.IndexOf('startupStage = "checkpoints";') -or !$hostSource.Contains('var options = factory == null ? Vo.Of(1000) : null;')){throw 'Evaluation stage/budget contract differs'}
$wrong=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-flow-script-suite-v1/runtime/Lea.Image.exe'
Pin $wrong 'FE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535'
$rejected=$false;try{$null=[ReadBootstrapResource]::Read($wrong)}catch{$rejected=$_.Exception.ToString().Contains('Expected one embedded bootstrap resource')}
if(!$rejected){throw 'Wrong-resource negative control failed'}
[IO.File]::WriteAllBytes((Join-Path $root 'Dom.compact.js'),$data)
Copy-Item -LiteralPath $helper -Destination (Join-Path $root 'ReadBootstrapResource.cs')
foreach($input in $inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Checkpoint audit input changed'}}
$result=@{verified=$true;inputs=$inputs;resourceSha256=$resourceHash;resourceBytes=$data.Length;documentMarkerOffset=$at;interpreterBudgetMs=1000;markerReportsLastCompletedSection=$true;remainingTail='Delete private bridge globals and instantiate dispatch function, then interpreter evaluation completion';wrongResourceRejected=$true;nativeWorkerLaunched=$false;runtimeModified=$false;causeProven=$false;scope='Pinned source/resource checkpoint semantics only, not exclusive document cost or a latency fix'}
[IO.File]::WriteAllText((Join-Path $root 'result.json'),($result|ConvertTo-Json -Depth 5))
Get-FileHash -LiteralPath (Join-Path $root 'result.json')
