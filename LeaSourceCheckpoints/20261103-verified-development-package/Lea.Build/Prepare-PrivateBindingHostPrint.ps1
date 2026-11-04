$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Test-DeterministicHostPrint-v2.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrivateBindingHostPrint-v1.generated.ps1'
$out='I:\LeaBuildStorage\private-binding-host-print-source-20261103-v1'
$manifest='I:\LeaBuildStorage\private-binding-firespider-runtime-20261103-v1\runtime-manifest.json'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve print source attempt'}
if((Get-FileHash $template).Hash -cne 'D508993FC484A2185DCBD770F842763EA7D1E12CBC97F54E8B13BAB617F170D3' -or (Get-FileHash $manifest).Hash -cne '694C89BB4FDF24936EF5424CAFC09C57BCDB50ED7EC21483D0DBDBD3A3E122FA'){throw 'Print projection inputs differ'}
$pairs=@(
    @('continuity-evidence/deterministic-host-print-v2','continuity-evidence/private-binding-host-print-v1'),
    @('I:\LeaBuildStorage\deterministic-firespider-runtime-20261102-v1\runtime-manifest.json',$manifest),
    @('72D5699F0A392633EF9B967BCDE5E6AB7961FC8405C5F5CB76DE15E342BA2C17','694C89BB4FDF24936EF5424CAFC09C57BCDB50ED7EC21483D0DBDBD3A3E122FA'),
    @('B1B290CA5E90B0F5DD2460BFA3A90DCB81EF8F0ADF5DA96599D437B36B3F416E','B1A707A23CEB1DEE7FBDE13102BE24FAE7369FC21FC43F0688D35522197B1F85')
)
$original=[IO.File]::ReadAllText($template);$text=$original
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1 -or $text.Contains($pair[1])){throw 'Unique print identity projection required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text
for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Print projection inverse differs'}
$tokens=$null;$parseErrors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$parseErrors)
if($parseErrors.Count){throw 'Generated print source invalid'}
New-Item -ItemType Directory -Path $out|Out-Null
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{verified=$true;source=$target;sha256=(Get-FileHash $target).Hash;templateSha256=(Get-FileHash $template).Hash;manifestSha256=(Get-FileHash $manifest).Hash;inverseVerified=$true;assertionsChanged=$false;printBudgetMs=90000;retirementBudgetMs=5000;queueSubmissionAllowed=$false;executed=$false;hostAdopted=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
