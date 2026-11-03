$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Assemble-ContainsRecoveryPrefix.ps1'
if((Get-FileHash $template).Hash -cne '010331E9267E08B5EEAC9FD4031C6FCCEE448CBDC3F30B4351CC732E275E9F95'){throw 'Prefix assembly template differs'}
$to=Join-Path $PSScriptRoot 'Assemble-RetirementPrefix-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve prefix assembler'}
$original=[IO.File]::ReadAllText($template)
$pairs=@(
    @('I:\LeaBuildStorage\deterministic-replay-entry-20261102-v1','I:\LeaBuildStorage\retirement-full-entry-20261102-v1'),
    @('I:\LeaBuildStorage\deterministic-replay-assembly-20261102-v1','I:\LeaBuildStorage\retirement-prefix-baseline-20261102-v1'),
    @('2042149CA97DD09E83E175DB28E11D6467E0788FBFF1ADE9F5152202CAAA80CB','8FC9160A1BCB659D2503445C505213B9CAD62CA73DE667042401C4A4DE07AC1D'),
    @('continuity-evidence/deterministic-full-replay-v1/runtime/WorkerEntryFixture','continuity-evidence/retirement-full-prefix-v1/runtime/WorkerEntryFixture')
)
$text=$original
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique prefix projection required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text
for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Prefix assembler inverse differs'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
