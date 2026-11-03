param([switch]$KeepForegroundHost,[ValidateRange(1,256)][int]$MaxFiles=256,[ValidateRange(768,2048)][int]$TargetFreeMB=1024)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$archive=[IO.Path]::GetFullPath((Join-Path $workspace 'dist'))
$outRoot=Join-Path $workspace 'continuity-evidence/archive-exe-compression-v1'
if(!$KeepForegroundHost-or'-NoExit'-notin[Environment]::GetCommandLineArgs()-or[Console]::IsInputRedirected-or[Console]::IsOutputRedirected){throw 'Retained NoExit owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve compression evidence'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash-cne'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Runner differs'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n-is[Management.Automation.Language.FunctionDefinitionAst]-and$n.Name-ceq$name},$false))
    if($e.Count-or$defs.Count-ne1){throw 'Runner shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$runs=[Collections.Generic.List[object]]::new();$records=[Collections.Generic.List[object]]::new()
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$result=[ordered]@{completed=$false;deletedFiles=0;sourceChanged=$false;scope='Only archived dist EXE storage compression; no current engine candidate or ZIP changes';records=$records;runs=$runs;error=$null}
$journal=$null
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    $helper=Join-Path $PSScriptRoot 'CompressionAllocation.cs'
    Copy-Item -LiteralPath $helper -Destination (Join-Path $outRoot 'CompressionAllocation.cs')
    Add-Type -Path $helper
    $journal=[IO.StreamWriter]::new((Join-Path $outRoot 'journal.jsonl'),$false,[Text.UTF8Encoding]::new($false))
    $journal.AutoFlush=$true
    $result.availableBefore=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
    $files=@(Get-ChildItem -LiteralPath $archive -Recurse -File -Filter '*.exe'|Where-Object{
        $_.Length-gt128KB-and-not($_.Attributes-band([IO.FileAttributes]::Compressed-bor[IO.FileAttributes]::ReparsePoint-bor[IO.FileAttributes]::ReadOnly))
    }|Sort-Object Length -Descending|Select-Object -First $MaxFiles)
    $compact=Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::System)) 'compact.exe'
    $index=0
    foreach($file in $files){
        if([IO.DriveInfo]::new('C:\').AvailableFreeSpace-ge$TargetFreeMB*1MB){break}
        $path=[IO.Path]::GetFullPath($file.FullName)
        if(!$path.StartsWith($archive+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Archive path escapes'}
        $parent=$file.Directory
        while($parent-and$parent.FullName.Length-ge$archive.Length){
            if($parent.Attributes-band[IO.FileAttributes]::ReparsePoint){throw 'Archive reparse ancestor rejected'}
            $parent=$parent.Parent
        }
        $before=[ordered]@{path=$path;sha256=(Get-FileHash -LiteralPath $path).Hash;sddl=(Get-Acl -LiteralPath $path).Sddl;
            modifiedUtc=$file.LastWriteTimeUtc.ToString('o');length=$file.Length;allocatedBytes=[CompressionAllocation]::Bytes($path)}
        $journal.WriteLine((@{phase='before';file=$before}|ConvertTo-Json -Depth 4 -Compress));$journal.BaseStream.Flush($true)
        $index++
        $run=Run ('compress-'+$index) $compact @('/C','/Q','/A',('"'+$path+'"')) 20
        if(!$run.passed){throw 'Archive compression failed; preserve original process result'}
        $after=Get-Item -LiteralPath $path
        if((Get-FileHash -LiteralPath $path).Hash-cne$before.sha256-or(Get-Acl -LiteralPath $path).Sddl-cne$before.sddl-or
            $after.LastWriteTimeUtc.ToString('o')-cne$before.modifiedUtc-or$after.Length-ne$before.length){throw 'Archive preservation check failed'}
        $allocated=[CompressionAllocation]::Bytes($path)
        $record=@{path=$path;sha256=$before.sha256;allocatedBefore=$before.allocatedBytes;allocatedAfter=$allocated;
            savedBytes=$before.allocatedBytes-$allocated;hashAclMtimeLengthPreserved=$true}
        $records.Add($record)
        $journal.WriteLine((@{phase='verified';file=$record}|ConvertTo-Json -Depth 4 -Compress));$journal.BaseStream.Flush($true)
    }
    $result.completed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    if($journal){$journal.Dispose()}
    $result.availableAfter=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
    $result.savedAllocatedBytes=($records|Measure-Object savedBytes -Sum).Sum
    $result.targetReached=$result.availableAfter-ge$TargetFreeMB*1MB
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count-gt0-or[bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $outRoot 'result.json')
    $global:LeaCompositeResult=$result
    [pscustomobject]@{completed=$result.completed;files=$records.Count;savedAllocatedBytes=$result.savedAllocatedBytes;availableAfter=$result.availableAfter;ownerAttention=$result.ownerAttention;error=$result.error}|ConvertTo-Json -Compress
}
