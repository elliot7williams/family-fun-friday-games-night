$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'New-EntryFixtureRelocation.ps1')
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$path=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/source-cross-module-suite-v2/fixture.json'
if((Get-FileHash -LiteralPath $path).Hash -cne '11C24CC01F9BE738A43491B2F3377BDA0C89D44FCB730EC119EE4B69240A53D3'){throw 'Original fixture resource changed'}
$original=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json -AsHashtable
$snapshot=$original|ConvertTo-Json -Depth 5 -Compress
$target=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/suite-phase-relocated-v1/runtime'
$relocated=New-EntryFixtureRelocation $original $original.directory $target
if($relocated.directory -cne (Join-Path $target 'WorkerEntryFixture') -or $relocated.files.Count -ne 7){throw 'Relocation result wrong'}
foreach($name in $original.files.Keys){if($relocated.files[$name] -cne $original.files[$name]){throw 'Pin changed'}}
if(($original|ConvertTo-Json -Depth 5 -Compress) -cne $snapshot){throw 'Original mutated'}
$checks=[Collections.Generic.List[string]]::new()
$checks.Add('seven pins preserved and only directory relocated')
$checks.Add('original manifest unchanged')
function Reject([scriptblock]$action,[string]$label){
    $rejected=$false;try{& $action}catch{$rejected=$true}
    if(!$rejected){throw ('Accepted invalid relocation: '+$label)}
    $checks.Add($label)
}
Reject {New-EntryFixtureRelocation $original $original.directory $workspace} 'workspace-root destination rejected'
Reject {New-EntryFixtureRelocation $original $original.directory ($workspace+'-other\runtime')} 'sibling-prefix escape rejected'
Reject {New-EntryFixtureRelocation $original $original.directory $original.directory} 'original directory rejected'
Reject {New-EntryFixtureRelocation $original $original.directory (Join-Path $original.directory 'nested')} 'nested original directory rejected'
Reject {New-EntryFixtureRelocation $original $target $target} 'wrong original directory rejected'
$bad=$snapshot|ConvertFrom-Json -AsHashtable;$bad.files.Remove('WorkerEntryCore.exe')
Reject {New-EntryFixtureRelocation $bad $original.directory $target} 'missing pin rejected'
$bad=$snapshot|ConvertFrom-Json -AsHashtable;$bad.files['WorkerEntryCore.exe']='not-a-digest'
Reject {New-EntryFixtureRelocation $bad $original.directory $target} 'malformed digest rejected'
$bad=$snapshot|ConvertFrom-Json -AsHashtable;$bad['unexpected']=1
Reject {New-EntryFixtureRelocation $bad $original.directory $target} 'extra resource field rejected'
$out=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/entry-relocation-checks-v1.json'
if(Test-Path -LiteralPath $out){throw 'Preserve test receipt'}
$result=@{passed=$true;checks=$checks;sourceResourceSha256=(Get-FileHash -LiteralPath $path).Hash;helperSha256=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'New-EntryFixtureRelocation.ps1')).Hash;relocatedResource=$relocated;filesMoved=$false;hostCompiled=$false;filesystemAclChanged=$false;limitsChanged=$false}
[IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 6),[Text.UTF8Encoding]::new($false))
[pscustomobject]@{passed=$true;checks=$checks.Count;receipt=(Get-FileHash -LiteralPath $out).Hash}|ConvertTo-Json
