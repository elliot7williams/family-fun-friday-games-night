param([switch]$Apply)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$source=[IO.Path]::GetFullPath((Join-Path $root 'continuity-evidence/private-binding-full-suite-v3'))
$destination='I:\LeaBuildStorage\private-binding-full-suite-v3-archive-20261109-v1'
$locator=Join-Path $root 'continuity-evidence/private-binding-full-suite-v3-relocation.json'
if(!$source.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase) -or !(Test-Path -LiteralPath $source)){throw 'Explicit old stage required'}
if((Test-Path -LiteralPath $destination) -or (Test-Path -LiteralPath $locator)){throw 'Preserve existing relocation evidence'}
if((Get-PSDrive I).Free -lt 200MB){throw 'Archive reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap permitted'}
$walk=Get-Item -LiteralPath $source
while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor refused'};$walk=$walk.Parent}
$tree=@(Get-ChildItem -LiteralPath $source -Recurse -Force)
if(@($tree|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Reparse tree refused'}
$proof=Join-Path $root 'continuity-evidence/private-binding-full-owned-v3/result.json'
if((Get-FileHash -LiteralPath $proof).Hash -cne 'FBF61B45F9D8F933B7C57BF44C67F78BA26484098BC5848A7B7754989EA44B55'){throw 'Historical pass receipt differs'}
$pass=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
if(!$pass.passed -or !$pass.inputsUnchanged -or $pass.ownerAttention -or $pass.before.Count -ne 65){throw 'Historical qualification incomplete'}
$stagePath=Join-Path $source 'staging.json'
if((Get-FileHash -LiteralPath $stagePath).Hash -cne '9EBB2B57ABFEDAB0275414887CF04115DA24F85D0AF28CDE2C91DEB6806CC846'){throw 'Historical staging receipt differs'}
if((Get-FileHash -LiteralPath (Join-Path $source 'staging-audit.json')).Hash -cne '2BDC07FCFC776C02A155F1673A19BB75B454A4462254562943539FD99A780546'){throw 'Historical staging audit differs'}
$stage=Get-Content -LiteralPath $stagePath -Raw|ConvertFrom-Json
$files=@($tree|Where-Object {!$_.PSIsContainer})
if($stage.files.Count -ne 247 -or $files.Count -ne 249){throw 'Historical stage inventory differs'}
foreach($member in $stage.files){
    $path=[IO.Path]::GetFullPath((Join-Path $source $member.path))
    if(!$path.StartsWith($source+'\',[StringComparison]::OrdinalIgnoreCase) -or (Get-FileHash -LiteralPath $path).Hash -cne $member.sha256){throw 'Historical member differs'}
}
foreach($member in $pass.before){
    if(!$member.path.StartsWith($source+'\',[StringComparison]::OrdinalIgnoreCase) -or (Get-FileHash -LiteralPath $member.path).Hash -cne $member.sha256 -or (Get-Acl -LiteralPath $member.path).Sddl -cne $member.sddl){throw 'Historical runtime bytes/ACL differ'}
}
$inventory=@(foreach($item in @((Get-Item -LiteralPath $source))+$tree){
    @{path=[IO.Path]::GetRelativePath($source,$item.FullName);directory=[bool]$item.PSIsContainer;
      sha256=$(if(!$item.PSIsContainer){(Get-FileHash -LiteralPath $item.FullName).Hash});
      sddl=(Get-Acl -LiteralPath $item.FullName).Sddl;attributes=[int]$item.Attributes;
      lastWriteTimeUtc=$item.LastWriteTimeUtc.ToString('o')}
})
$bytes=($files|Measure-Object Length -Sum).Sum
if(!$Apply){@{verified=$true;apply=$false;files=$files.Count;bytes=$bytes;source=$source;destination=$destination}|ConvertTo-Json;return}
New-Item -ItemType Directory -Path $destination|Out-Null
foreach($item in $inventory|Where-Object directory){New-Item -ItemType Directory -Path (Join-Path $destination ('stage/'+$item.path)) -Force|Out-Null}
foreach($item in $inventory|Where-Object {!$_.directory}){
    $from=Join-Path $source $item.path;$to=Join-Path $destination ('stage/'+$item.path)
    Copy-Item -LiteralPath $from -Destination $to
    if((Get-FileHash -LiteralPath $to).Hash -cne $item.sha256){throw 'Archive readback differs; source retained'}
}
$record=@{verified=$true;removed=$false;source=$source;destination=$destination;files=$files.Count;bytes=$bytes;inventory=$inventory;
    restoreRequiredBeforeReplay=$true;note='Restore original absolute paths and recorded SDDL on NTFS before replay; exFAT copy is archival only.'}
$metadata=Join-Path $destination 'relocation.json'
$record|ConvertTo-Json -Depth 7|Set-Content -LiteralPath $metadata -Encoding utf8
Copy-Item -LiteralPath $metadata -Destination $locator
if((Get-FileHash -LiteralPath $metadata).Hash -cne (Get-FileHash -LiteralPath $locator).Hash){throw 'Locator readback differs; source retained'}
# Revalidate the complete source and archive immediately before scoped removal.
if(@(Get-ChildItem -LiteralPath $source -Recurse -Force -File).Count -ne $files.Count){throw 'Source inventory changed'}
foreach($item in $inventory){
    $from=Join-Path $source $item.path
    if((Get-Acl -LiteralPath $from).Sddl -cne $item.sddl){throw 'Source ACL changed'}
    if(!$item.directory -and ((Get-FileHash -LiteralPath $from).Hash -cne $item.sha256 -or (Get-FileHash -LiteralPath (Join-Path $destination ('stage/'+$item.path))).Hash -cne $item.sha256)){throw 'Source/archive changed'}
}
if([IO.Path]::GetFullPath($source) -cne (Join-Path $root 'continuity-evidence/private-binding-full-suite-v3')){throw 'Removal scope differs'}
Remove-Item -LiteralPath $source -Recurse -Force
$record.removed=!(Test-Path -LiteralPath $source)
$record|ConvertTo-Json -Depth 7|Set-Content -LiteralPath $metadata -Encoding utf8
Copy-Item -LiteralPath $metadata -Destination $locator -Force
if((Get-FileHash -LiteralPath $metadata).Hash -cne (Get-FileHash -LiteralPath $locator).Hash){throw 'Final locator readback differs'}
$record|Select-Object verified,removed,files,bytes,destination|ConvertTo-Json
