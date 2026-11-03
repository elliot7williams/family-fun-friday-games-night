$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Compress-ArchivedBinaries.ps1'
$target=Join-Path $PSScriptRoot 'Compress-RetainedEvidence.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve compression script'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @("'dist'","'continuity-evidence'"),
    @("'continuity-evidence/archive-binary-compression-v1'","'continuity-evidence/retained-compression-v1'"),
    @('Only archived dist DLL storage compression; no current engine candidate or ZIP changes','Lossless NTFS storage compression of retained evidence DLL/PDB/CSV; preserve bytes, ACLs, timestamps and paths; no deletions'),
    @("-Recurse -File -Filter '*.dll'|Where-Object{","-Recurse -File|Where-Object{`n        `$_.Extension -in @('.dll','.pdb','.csv')-and"),
    @('[int]$TargetFreeMB=1024','[int]$TargetFreeMB=1536')
)){
    if(!$text.Contains($pair[0])){throw ('Missing anchor: '+$pair[0])}
    $text=$text.Replace($pair[0],$pair[1])
}
# Compact needs little working space; this override is local to cleanup, never build/test runners.
$anchor='$runs=[Collections.Generic.List[object]]::new()'
if(!$text.Contains($anchor)){throw 'Cleanup guard anchor missing'}
$text=$text.Replace($anchor,"function Space { if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 16MB){throw 'Cleanup journal reserve required'} }`n"+$anchor)
$tokens=$null;$errors=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Generated compression syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
(Get-FileHash -LiteralPath $target).Hash
