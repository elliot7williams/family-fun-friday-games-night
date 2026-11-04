param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ManifestSha256,[Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$AuditSha256)
$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Test-DeterministicHostSmokes-v2.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrivateBindingHostSmokes-v1.generated.ps1'
$out='I:\LeaBuildStorage\private-binding-host-smoke-source-20261103-v1'
$manifestPath='I:\LeaBuildStorage\private-binding-firespider-runtime-20261103-v1\runtime-manifest.json'
$auditPath='I:\LeaBuildStorage\private-binding-firespider-runtime-audit-20261103-v1\audit.json'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve existing host smoke source'}
if((Get-FileHash $template).Hash -cne 'D7E21BDABCBD872FD98D8983BA14B630783F207B7EFB7161C67AD1413E147825' -or (Get-FileHash $manifestPath).Hash -cne $ManifestSha256 -or (Get-FileHash $auditPath).Hash -cne $AuditSha256){throw 'Host smoke projection inputs differ'}
$manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
$audit=Get-Content $auditPath -Raw|ConvertFrom-Json
if(!$manifest.verified -or !$manifest.privateBindingPair -or $manifest.files.Count -ne 400 -or !$audit.passed -or $audit.manifestSha256 -cne $ManifestSha256 -or $audit.currentOutputs -ne 22 -or $audit.workerMembers -ne 6){throw 'Matching audited host closure required'}
foreach($pair in @(@('Lea.Script.dll','406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F'),@('ScriptWorker/Lea.Script.Worker.dll','F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0'))){$rows=@($manifest.files|Where-Object path -ceq $pair[0]);if($rows.Count -ne 1 -or $rows[0].sha256 -cne $pair[1]){throw 'Qualified Lea pair missing from host'}}
$replacements=@(
    @('continuity-evidence/deterministic-host-smokes-v2','continuity-evidence/private-binding-host-smokes-v1'),
    @('I:\LeaBuildStorage\deterministic-firespider-runtime-20261102-v1\runtime-manifest.json',$manifestPath),
    @('72D5699F0A392633EF9B967BCDE5E6AB7961FC8405C5F5CB76DE15E342BA2C17',$ManifestSha256),
    @('B1B290CA5E90B0F5DD2460BFA3A90DCB81EF8F0ADF5DA96599D437B36B3F416E',$manifest.hostSha256),
    @('Exact deterministic host settings/navigation/actual clean EXE smokes','Exact private-binding host settings/navigation/actual clean EXE smokes')
)
$original=[IO.File]::ReadAllText($template);$text=$original
foreach($pair in $replacements){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1 -or $text.Contains($pair[1])){throw 'Unique host smoke identity projection required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text
for($i=$replacements.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($replacements[$i][1],$replacements[$i][0])}
if($inverse -cne $original){throw 'Host smoke projection inverse differs'}
$tokens=$null;$parseErrors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$parseErrors)
if($parseErrors.Count){throw 'Generated host smoke syntax invalid'}
New-Item -ItemType Directory -Path $out|Out-Null
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $text){throw 'Generated source readback differs'}
@{verified=$true;templateSha256=(Get-FileHash $template).Hash;manifestSha256=$ManifestSha256;runtimeAuditSha256=$AuditSha256;source=$target;sha256=(Get-FileHash $target).Hash;replacements=$replacements;inverseVerified=$true;assertionsChanged=$false;settingsBudgetMs=30000;navigationBudgetMs=90000;actualExeBudgetMs=100000;retirementBudgetMs=5000;executed=$false;hostAdopted=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
