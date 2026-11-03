$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/view-source/V.cs'
$fragment=Join-Path $PSScriptRoot 'PrintViewStyles.fragment.cs'
$out=Join-Path $workspace 'continuity-evidence/print-view-styles-source-v1'
if(Test-Path $out){throw 'Preserve view preparation'}
if((Get-FileHash $source).Hash -cne '3DBCDB0363BA5C291807D395D5F3FFB50956F697536A71FD3C0A2B00EE1744AD'){throw 'Mature view differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'')
$anchor='        // Read a permitted native-field selection without changing focus or clipboard.'
if([regex]::Matches($original,[regex]::Escape($anchor)).Count -ne 1){throw 'Unique view anchor required'}
$insert=[IO.File]::ReadAllText($fragment).Replace("`r",'')+"`n"+$anchor
$text=$original.Replace($anchor,$insert)
if($text.Replace($insert,$anchor) -cne $original){throw 'View inverse differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText((Join-Path $out 'V.cs'),$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;source=(Get-FileHash $source);fragment=(Get-FileHash $fragment);candidate=(Get-FileHash (Join-Path $out 'V.cs'))}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'result.json')
Get-FileHash (Join-Path $out 'V.cs')
