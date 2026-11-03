$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-CurrentHitFullRegression.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve current hit full regression'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-TitleMetadataRegressionV8.generated.ps1'))
$text=$text.Replace('title-metadata-regression-v8','current-hit-full-regression-v1')
$old="CopyExact (Join-Path `$workspace 'continuity-evidence/title-metadata-visuals-v1/runtime/Lea.Win.dll') (Join-Path `$result.candidate 'Lea.Win.dll') 'FF99C552ABD3AEFB2F6DD12ECF83A3B7EAF2A3FDD392571B286F9F804D2939EC'"
$new="CopyExact (Join-Path `$workspace 'continuity-evidence/current-hit-geometry-v3/Lea.Win.dll') (Join-Path `$result.candidate 'Lea.Win.dll') '74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AAC'"
if([regex]::Matches($text,[regex]::Escape($old)).Count -ne 1){throw 'Full view anchor differs'}
$text=$text.Replace($old,$new)
$text=$text.Replace('Invoke-PublicCompiler.20261022.ps1','Invoke-PublicCompiler.20261026.ps1').Replace('1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03','C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A')
$anchor='try{'
$insert=@'
try{
    $hitProof=Join-Path $workspace 'continuity-evidence/current-hit-geometry-v3/result.json'
    $null=Pin $hitProof '7EF54C6150CB19A89AF32A04E756C95F8C0D2EC411711B010B3AC8D794C523E0'
    $hit=Get-Content -LiteralPath $hitProof -Raw|ConvertFrom-Json
    if(!$hit.passed -or !$hit.inputsUnchanged -or !$hit.bytesUnchanged -or $hit.ownerAttention){throw 'Focused hit proof incomplete'}
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Full runner entry differs'}
$text=$text.Replace($anchor,$insert)
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
