$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Audit-DecoderNativeAcls.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve native diagnostic ACL audit'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Audit-NamedFixtureAcls.generated.ps1'))
foreach($pair in @(
    @('named-references-regression-v1','decoder-queue-native-v1'),
    @('89E80A57BFBDF83EBCDCAE79F92984ECD8EE631FE0BBC83A11C0D32CF2A0304C','F760B078269B86A6053C4A29EF29194A70FE2129434E1644B68E606966FC6130'),
    @('71AF48EACCF6D373EBAFA17535A1B9284C937FAD4F2EB5824CC25F2CFE2DBDA6','13000CE7C894A3719ECC1682041793591929DD595EFCE202D9BFBDB3B3CD8933')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Native diagnostic ACL anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$text=$text.Replace('!$run.behaviorPassed -or !$run.fullSuiteFooter','!$run.nativeBehaviorPassed -or !$run.diagnosticCaptured').Replace('!$run.bytesUnchanged','!$run.candidateBytesUnchanged').Replace('$observations.Count -ne 3','$observations.Count -ne 2').Replace('exactly three','exactly two').Replace('three exact private fixture grants','two exact private fixture grants')
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
