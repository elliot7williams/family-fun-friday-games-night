param([ValidateSet(1,4)][int]$Version=1)
$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Build-SdkFullScriptEntry.ps1'
if((Get-FileHash $template).Hash -cne 'DF00514FE6C95D24FA89BFBB5E5CC2C40B7D5BADC6371A469713DAB6AAAC532A'){throw 'Entry build template differs'}
$to=Join-Path $PSScriptRoot "Build-RetirementFullEntry-v$Version.generated.ps1"
if(Test-Path $to){throw 'Preserve diagnostic entry builder'}
$original=[IO.File]::ReadAllText($template)
$text=$original.Replace('I:\LeaBuildStorage\deterministic-replay-entry-20261102-v1','I:\LeaBuildStorage\retirement-full-entry-20261102-v1').Replace('continuity-evidence/deterministic-full-replay-v1/runtime/WorkerEntryFixture','continuity-evidence/retirement-full-prefix-v1/runtime/WorkerEntryFixture')
$inverse=$text.Replace('I:\LeaBuildStorage\retirement-full-entry-20261102-v1','I:\LeaBuildStorage\deterministic-replay-entry-20261102-v1').Replace('continuity-evidence/retirement-full-prefix-v1/runtime/WorkerEntryFixture','continuity-evidence/deterministic-full-replay-v1/runtime/WorkerEntryFixture')
if($inverse -cne $original -or $text -ceq $original){throw 'Entry path projection inverse differs'}
if($Version -eq 4){$text=$text.Replace('retirement-full-entry-20261102-v1','retirement-full-entry-20261102-v4').Replace('continuity-evidence/retirement-full-prefix-v1/runtime/WorkerEntryFixture','continuity-evidence/retirement-full-prefix-v2/runtime/WorkerEntryFixture')}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Entry builder syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
