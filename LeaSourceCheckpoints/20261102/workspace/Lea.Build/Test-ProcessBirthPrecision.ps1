$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\process-birth-precision-controls-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve birth precision controls'}
$audit=Join-Path $PSScriptRoot 'Audit-CombinedTrustedCounters.ps1'
$pin='4B76A673E941DF3780E6EE1AB86891DCAC5B46541A4270A207E28F60285DC113'
if((Get-FileHash $audit).Hash -cne $pin){throw 'Tested audit source differs'}
$tokens=$null;$errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile($audit,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Audit source parse failed'}
$assignments=@($ast.FindAll({param($node) $node -is [Management.Automation.Language.AssignmentStatementAst] -and $node.Left -is [Management.Automation.Language.VariableExpressionAst] -and $node.Left.VariablePath.UserPath -ceq 'birth'},$true))
if($assignments.Count -ne 1){throw 'Unique actual audit birth conversion required'}
$convert=[scriptblock]::Create($assignments[0].Extent.Text+"`n`$birth")
$expected=134380551761328718L
$utc=[datetime]::FromFileTimeUtc($expected)
$iso=$utc.ToString('o',[Globalization.CultureInfo]::InvariantCulture)
$decoded=('{"birthUtc":"'+$iso+'"}'|ConvertFrom-Json).birthUtc
$offset=[DateTimeOffset]::new($utc).ToOffset([TimeSpan]::FromHours(-5)).ToString('o',[Globalization.CultureInfo]::InvariantCulture)
$cases=@(
    @{name='typed-utc';value=$utc},
    @{name='typed-local';value=$utc.ToLocalTime()},
    @{name='json-decoded';value=$decoded},
    @{name='roundtrip-utc-string';value=$iso},
    @{name='roundtrip-offset-string';value=$offset}
)
$positive=0
foreach($case in $cases){
    $tested=[pscustomobject]@{birthUtc=$case.value}
    $actual=& $convert
    if($actual -ne $expected){throw ('Lost original creation precision: '+$case.name)};$positive++
}
$tested=[pscustomobject]@{birthUtc='NOT_A_DATE'}
$denied=$false;try{$null=& $convert}catch{$denied=$true}
if(!$denied){throw 'Malformed birth timestamp accepted'}
$legacy=[datetime]::Parse($utc).ToUniversalTime().ToFileTimeUtc()
if($legacy -eq $expected){throw 'Regression fixture does not expose legacy lossy conversion'}
if((Get-FileHash $audit).Hash -cne $pin){throw 'Tested source changed'}
New-Item -ItemType Directory -Path $out|Out-Null
@{passed=$true;positiveControls=$positive;malformedNegativeControls=1;legacyPrecisionLossObserved=$true;expectedFileTime=$expected;legacyFileTime=$legacy;actualAuditAssignmentTested=$true;auditSourceSha256=$pin;runnerSha256=(Get-FileHash $PSCommandPath).Hash;interpreterExecuted=$false}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json') -Encoding utf8
Get-FileHash (Join-Path $out 'result.json')
