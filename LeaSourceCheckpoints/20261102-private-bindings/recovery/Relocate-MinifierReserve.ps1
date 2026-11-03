$ErrorActionPreference='Stop'
$root='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\continuity-evidence'
$out='I:\LeaBuildStorage\minifier-reserve-source-zips-20261102-v1'
if(Test-Path $out){throw 'Preserve relocation evidence'}
New-Item -ItemType Directory -Path $out|Out-Null
$rows=[Collections.Generic.List[object]]::new()
foreach($name in @('historical-stage-cleanup-source-20261028.zip','owned-launch-source-20261026.zip','native-selection-delivery-source-20261027.zip')){
    $from=[IO.Path]::GetFullPath((Join-Path $root $name))
    if(!(Split-Path $from -Parent).Equals($root,[StringComparison]::OrdinalIgnoreCase)){throw 'Archive path escape'}
    $item=Get-Item -LiteralPath $from
    $walk=$item;while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse archive refused'};$walk=if($walk -is [IO.FileInfo]){$walk.Directory}else{$walk.Parent}}
    if(@(Get-Item -LiteralPath $from -Stream *|Where-Object Stream -ne ':$DATA').Count){throw 'Alternate stream preservation required'}
    $to=Join-Path $out $name
    $hash=(Get-FileHash $from).Hash
    $input=[IO.File]::Open($from,'Open','Read','Read')
    try{$output=[IO.File]::Open($to,'CreateNew','Write','None');try{$input.CopyTo($output);$output.Flush($true)}finally{$output.Dispose()}}finally{$input.Dispose()}
    if((Get-Item $to).Length -ne $item.Length -or (Get-FileHash $to).Hash -cne $hash -or (Get-FileHash $from).Hash -cne $hash){throw 'Archive copy mismatch'}
    $row=@{source=$from;destination=$to;sha256=$hash;bytes=$item.Length;verified=$true;removed=$false}
    $rows.Add($row);$rows|ConvertTo-Json -Depth 4|Set-Content (Join-Path $out 'relocation.json') -Encoding utf8
    Remove-Item -LiteralPath $from
    $row.removed=!(Test-Path $from);$rows|ConvertTo-Json -Depth 4|Set-Content (Join-Path $out 'relocation.json') -Encoding utf8
}
$rows|Select-Object bytes,verified,removed|ConvertTo-Json
