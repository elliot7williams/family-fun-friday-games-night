$ErrorActionPreference='Stop'
$workspace='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy'
$sourceRoot=[IO.Path]::GetFullPath((Join-Path $workspace 'dist/LeaExes'))
$destination=$PSScriptRoot
$names=@('SUPERSEDED-Lea-runtime-209-ready_2026-09-19_14-47-12.do-not-use','SUPERSEDED-Lea-runtime-207-ready_2026-09-13_16-03-28.do-not-use','SUPERSEDED-Lea-runtime-208-ready_2026-09-19_04-59-29.do-not-use')
if(Test-Path (Join-Path $destination 'result.json')){throw 'Preserve relocation evidence'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','CombinedWorkerStartupProbe' -ErrorAction SilentlyContinue).Count){throw 'No native test/browser overlap'}
$rows=[Collections.Generic.List[object]]::new()
foreach($name in $names){
    $path=[IO.Path]::GetFullPath((Join-Path $sourceRoot $name));$to=Join-Path $destination $name
    if([IO.Path]::GetDirectoryName($path) -cne $sourceRoot -or (Test-Path -LiteralPath $to)){throw 'Source scope or fresh target differs'}
    foreach($location in @($path,$destination)){
        $item=Get-Item -LiteralPath $location
        while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse relocation refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    }
    if(@(Get-Item -LiteralPath $path -Stream *|Where-Object Stream -ne ':$DATA').Count){throw 'Alternate stream requires separate preservation'}
    $hold=[IO.File]::Open($path,'Open','Read','Read')
    try{
        $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($hold));$hold.Position=0
        $target=[IO.File]::Open($to,'CreateNew','Write','None')
        try{$hold.CopyTo($target);$target.Flush($true)}finally{$target.Dispose()}
        if((Get-FileHash -LiteralPath $to).Hash -cne $hash -or (Get-Item -LiteralPath $to).Length -ne $hold.Length){throw 'Copied artifact differs; source retained'}
        $row=@{source=$path;destination=$to;sha256=$hash;bytes=$hold.Length;sddl=(Get-Acl -LiteralPath $path).Sddl;sourceRemoved=$false};$rows.Add($row)
        $rows|ConvertTo-Json -Depth 4|Set-Content (Join-Path $destination 'result.json') -Encoding utf8
    }finally{$hold.Dispose()}
    if((Get-FileHash -LiteralPath $path).Hash -cne $hash){throw 'Source changed; retained'}
    Remove-Item -LiteralPath $path;$row.sourceRemoved=$true
    $rows|ConvertTo-Json -Depth 4|Set-Content (Join-Path $destination 'result.json') -Encoding utf8
}
@{verified=$true;files=$rows.Count;bytes=($rows|Measure-Object bytes -Sum).Sum;freeAfter=(Get-PSDrive C).Free}|ConvertTo-Json
