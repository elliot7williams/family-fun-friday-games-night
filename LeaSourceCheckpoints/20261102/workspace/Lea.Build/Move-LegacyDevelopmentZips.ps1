$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path (Split-Path $PSScriptRoot) 'dist/LeaExes'))
$out='I:\LeaBuildStorage\legacy-development-zips-20261102-v1'
if(Test-Path $out){throw 'Preserve relocation receipt'}
$names=@('Firespider-Lea-font-development-20261031.zip','Firespider-Lea-print-development-20261028.zip','Firespider-Lea-preview-development-20261027.zip','Firespider-Lea-runtime8033-development-20261027.zip','Firespider-Lea-native-selection-development-20261027.zip','Firespider-Lea-page-color-development-20261026.zip','Firespider-Lea-library-development-20261026.zip','Firespider-Lea-development-20261026.zip')
New-Item -ItemType Directory -Path $out|Out-Null
$rows=[Collections.Generic.List[object]]::new()
foreach($name in $names){
    $source=[IO.Path]::GetFullPath((Join-Path $root $name))
    if((Split-Path $source) -cne $root){throw 'Source outside authorized development ZIP directory'}
    $item=Get-Item -LiteralPath $source
    $ancestor=$item
    while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse relocation refused'};$ancestor=if($ancestor -is [IO.FileInfo]){$ancestor.Directory}else{$ancestor.Parent}}
    $hash=(Get-FileHash $source).Hash
    $to=Join-Path $out $name
    Copy-Item -LiteralPath $source -Destination $to
    if((Get-FileHash $to).Hash -cne $hash -or (Get-Item $to).Length -ne $item.Length){throw 'Archive readback differs'}
    $rows.Add(@{source=$source;destination=$to;sha256=$hash;bytes=$item.Length;sddl=(Get-Acl $source).Sddl;lastWriteUtc=$item.LastWriteTimeUtc.ToString('o');removed=$false})
}
$receipt=Join-Path $out 'relocation.json'
@{verified=$true;files=$rows;scope='Older development ZIPs relocated; all bytes retained on I:, no source/current runtime/test evidence deleted'}|ConvertTo-Json -Depth 5|Set-Content $receipt -Encoding utf8
foreach($row in $rows){
    if((Get-FileHash $row.source).Hash -cne $row.sha256 -or (Get-FileHash $row.destination).Hash -cne $row.sha256){throw 'Archive changed before source removal'}
    Remove-Item -LiteralPath $row.source
    $row.removed=!(Test-Path -LiteralPath $row.source)
    @{verified=$true;files=$rows;scope='Older development ZIPs relocated; all bytes retained on I:, no source/current runtime/test evidence deleted'}|ConvertTo-Json -Depth 5|Set-Content $receipt -Encoding utf8
}
Get-FileHash $receipt
Get-PSDrive C|Select-Object Free
