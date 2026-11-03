$ErrorActionPreference='Stop'
$root='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy'
$source=[IO.Path]::GetFullPath((Join-Path $root 'continuity-evidence/portable-full-sdk-source-v2.zip'))
$allowed=[IO.Path]::GetFullPath((Join-Path $root 'continuity-evidence'))
if([IO.Path]::GetDirectoryName($source) -cne $allowed){throw 'Explicit source archive path required'}
$item=Get-Item -LiteralPath $source
$walk=$item;while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse source refused'};$walk=if($walk -is [IO.FileInfo]){$walk.Directory}else{$walk.Parent}}
if(@(Get-Item -LiteralPath $source -Stream *|Where-Object Stream -ne ':$DATA').Count){throw 'Alternate stream preservation required'}
$out='I:\LeaBuildStorage\sdk-source-reserve-relocation-20261102-v1'
if(Test-Path $out){throw 'Preserve relocation'}
New-Item -ItemType Directory -Path $out|Out-Null
$to=Join-Path $out $item.Name
$record=@{source=$source;destination=$to;bytes=$item.Length;sha256=(Get-FileHash $source).Hash;sddl=(Get-Acl $source).Sddl;lastWriteUtc=$item.LastWriteTimeUtc;verified=$false;removed=$false;workingSourceTouched=$false;failureEvidenceTouched=$false}
$input=[IO.File]::Open($source,'Open','Read','Read')
try{
    $output=[IO.File]::Open($to,'CreateNew','Write','None')
    try{$input.CopyTo($output);$output.Flush($true)}finally{$output.Dispose()}
    if((Get-Item $to).Length -ne $record.bytes -or (Get-FileHash $to).Hash -cne $record.sha256 -or (Get-FileHash $source).Hash -cne $record.sha256){throw 'Archive copy readback differs'}
    $record.verified=$true
    $record|ConvertTo-Json|Set-Content (Join-Path $out 'relocation.json') -Encoding utf8
}finally{$input.Dispose()}
if(!$record.verified -or (Get-FileHash $source).Hash -cne $record.sha256){throw 'Verified removal precondition differs'}
Remove-Item -LiteralPath $source
$record.removed=!(Test-Path $source)
$record|ConvertTo-Json|Set-Content (Join-Path $out 'relocation.json') -Encoding utf8
$record|ConvertTo-Json -Compress
