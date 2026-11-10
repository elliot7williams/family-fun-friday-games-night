param([switch]$GuardedSdk)
$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Test-PrivateBindingHostPrint-v1.generated.ps1'
$out='I:\LeaBuildStorage\current-firespider-print-source-20261110-v1'
$manifest='I:\LeaBuildStorage\current-firespider-runtime-20261110-v1\runtime-manifest.json'
$manifestHash='2836AC6BA77C2DB096C5365EED4D7B6E48D56A82A0A4DFDBEBFD2CCE3DD6E1AB'
$hostHash='9513A0BD7C620589F4AB332CE19F155686C01104B1E8D99A5DECAAF1814F2586'
if($GuardedSdk){$out='I:\LeaBuildStorage\guarded-sdk-host-print-source-20261110-v1';$manifest='I:\LeaBuildStorage\guarded-sdk-host-runtime-20261110-v1\runtime-manifest.json';$manifestHash='A566E5A5AEF6F7898D8B1DE8230D19C171FDB8F01804F58186AD96BC269FB8A0';$hostHash='1E390343125011CAA3F78582E91FAFC036E07A31F7E81B4B727ED8503FC39606'}
if(Test-Path -LiteralPath $out){throw 'Preserve print source'}
if((Get-FileHash -LiteralPath $template).Hash -cne '8EEBE670453A7C6604A1C3560EA8EF6EDF91F461789CCCBCCF58B708CBB05365' -or (Get-FileHash -LiteralPath $manifest).Hash -cne $manifestHash){throw 'Current print projection inputs differ'}
$original=[IO.File]::ReadAllText($template);$text=$original
$pairs=@(
    @('continuity-evidence/private-binding-host-print-v1','continuity-evidence/current-source-host-print-v1'),
    @('I:\LeaBuildStorage\private-binding-firespider-runtime-20261103-v1\runtime-manifest.json',$manifest),
    @('694C89BB4FDF24936EF5424CAFC09C57BCDB50ED7EC21483D0DBDBD3A3E122FA',$manifestHash),
    @('B1A707A23CEB1DEE7FBDE13102BE24FAE7369FC21FC43F0688D35522197B1F85',$hostHash)
)
if($GuardedSdk){$pairs[0][1]='continuity-evidence/guarded-sdk-host-print-v1'}
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1 -or $text.Contains($pair[1])){throw 'Unique print identity boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$literal="'"+$PSScriptRoot.Replace("'","''")+"'"
if($text.Contains($literal)){throw 'Helper relocation collision'};$uses=[regex]::Matches($text,[regex]::Escape('$PSScriptRoot')).Count
$text=$text.Replace('$PSScriptRoot',$literal);$inverse=$text.Replace($literal,'$PSScriptRoot')
for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Print identity/helper-path-only inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors);if($errors.Count){throw 'Print source syntax invalid'}
New-Item -ItemType Directory -Path $out|Out-Null
$target=Join-Path $out 'Test-CurrentFirespiderPrint.ps1';[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $text){throw 'Print source readback differs'}
@{verified=$true;templateSha256=(Get-FileHash -LiteralPath $template).Hash;manifestSha256=(Get-FileHash -LiteralPath $manifest).Hash;source=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;replacements=$pairs;helperPathReplacements=$uses;inverseVerified=$true;assertionsChanged=$false;printBudgetMs=90000;retirementBudgetMs=5000;queueSubmissionAllowed=$false;executed=$false;hostAdopted=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'source.json')|Format-List Hash
