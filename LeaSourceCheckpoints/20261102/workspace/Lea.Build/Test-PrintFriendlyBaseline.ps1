$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$source=Join-Path $root 'Lea.Print/Ps.cs'
$baseline=Join-Path $root 'continuity-evidence/print-ticket-project-v1/source/Ps.cs'
if((Get-FileHash $baseline).Hash -cne '547AA5CC649336D3587120A566D695312E861F809A0A1D35B64B94A34CDAEEC5'){throw 'Baseline bytes differ'}
. (Join-Path $PSScriptRoot 'Get-PrintFriendlyBaseline.ps1')
$text=[IO.File]::ReadAllText($source);$expected=[IO.File]::ReadAllText($baseline).Replace("`r",'')
if((Get-PrintFriendlyBaseline $text) -cne $expected){throw 'Friendly inverse differs'}
$negative=0
foreach($bad in @($text.Replace('Title = title, Source = source','Title = title, Source = "changed"'),$text.Replace('private static bool FriendlyExcluded','private static bool RenamedExcluded'),($text+"`n"+$text))){
    $denied=$false
    try{$denied=(Get-PrintFriendlyBaseline $bad) -cne $expected}catch{$denied=$true}
    if(!$denied){throw 'Negative control accepted'};$negative++
}
$result=[ordered]@{passed=$true;inverseVerified=$true;negativeControls=$negative;sourceSha256=(Get-FileHash $source).Hash;
    baselineSha256=(Get-FileHash $baseline).Hash;helperSha256=(Get-FileHash (Join-Path $PSScriptRoot 'Get-PrintFriendlyBaseline.ps1')).Hash;
    scope='Source-only projection check, not compilation/runtime or browser adoption'}
$path=Join-Path $root 'continuity-evidence/print-friendly-v2/source-projection.json'
if(Test-Path $path){throw 'Preserve projection receipt'}
[IO.File]::WriteAllText($path,($result|ConvertTo-Json));$result|ConvertTo-Json
