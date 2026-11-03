$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$source=Join-Path $root 'Lea.Print/Pc.cs'
$baseline=Join-Path $root 'continuity-evidence/print-ticket-project-v1/source/Pc.cs'
if((Get-FileHash $baseline).Hash -cne '957D5527198B6D296C7396F59F060268F6E968A189A21850B6AB5E01DFC91754'){throw 'View baseline bytes differ'}
$helper=Join-Path $PSScriptRoot 'Get-PrintFriendlyViewBaseline.ps1';. $helper
$text=[IO.File]::ReadAllText($source);$expected=[IO.File]::ReadAllText($baseline).Replace("`r",'')
if((Get-PrintFriendlyViewBaseline $text) -cne $expected){throw 'View inverse differs'}
$negative=0
foreach($bad in @($text.Replace('string selection = view.Sx();','string selection = "changed";'),$text.Replace('public static Pc Cf','public static Pc Renamed'),($text+"`n"+$text))){
    $denied=$false
    try{$denied=(Get-PrintFriendlyViewBaseline $bad) -cne $expected}catch{$denied=$true}
    if(!$denied){throw 'View negative control accepted'};$negative++
}
$result=[ordered]@{passed=$true;inverseVerified=$true;negativeControls=$negative;sourceSha256=(Get-FileHash $source).Hash;
    baselineSha256=(Get-FileHash $baseline).Hash;helperSha256=(Get-FileHash $helper).Hash;
    scope='Source-only view bridge projection; no compilation/live view/host verification'}
$path=Join-Path $root 'continuity-evidence/print-friendly-v2/view-source-projection.json'
if(Test-Path $path){throw 'Preserve view projection receipt'}
[IO.File]::WriteAllText($path,($result|ConvertTo-Json));$result|ConvertTo-Json
