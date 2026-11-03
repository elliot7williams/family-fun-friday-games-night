$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v3'
$out='I:\LeaBuildStorage\sdk-agent-wait-diagnostic-source-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve agent wait diagnostic source'}
$manifest=Join-Path $stage 'assembly.json'
if((Get-FileHash $manifest).Hash -cne '045F8EAF988199AC86119C0EAB552EF4A9E90C02ACAB5ACE2CBB8C3CD00E9992'){throw 'Current source assembly differs'}
$assembly=Get-Content $manifest -Raw|ConvertFrom-Json
$members=@($assembly.files|Where-Object {$_.path.StartsWith('compiled-source/') -and [IO.Path]::GetFileName($_.path) -ceq 'AgentWait.cs'})
if(!$assembly.verified -or $members.Count -ne 1){throw 'Unique current AgentWait source required'}
$source=Join-Path $stage $members[0].path
if((Get-FileHash $source).Hash -cne $members[0].sha256){throw 'Agent wait source differs'}
. (Join-Path $PSScriptRoot 'New-SdkAgentWaitDiagnostic.ps1')
$original=[IO.File]::ReadAllText($source);$projection=New-SdkAgentWaitDiagnostic $original
if(!$projection.inverseVerified -or $projection.negativeControls -ne 3 -or !$projection.assertionConditionUnchanged -or $projection.budgetsChanged -or $projection.newRuntimeCalls){throw 'Diagnostic projection controls incomplete'}
New-Item -ItemType Directory -Path $out|Out-Null
$candidate=Join-Path $out 'AgentWait.cs';[IO.File]::WriteAllText($candidate,$projection.text,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($candidate) -cne $projection.text){throw 'Diagnostic source readback differs'}
@{verified=$true;source=$candidate;sha256=(Get-FileHash $candidate).Hash;original=$source;originalSha256=$members[0].sha256;inverseVerified=$true;negativeControls=3;assertionConditionUnchanged=$true;successMessageUnchanged=$true;failureOnlyStatusMessage=$true;newRuntimeCalls=$false;budgetsChanged=$false;compiled=$false;executed=$false;diagnosticOnly=$true;pending='Compile fresh full-prefix diagnostic candidate with this single observation-only source overlay; retain original failure';runnerSha256=(Get-FileHash $PSCommandPath).Hash;projectionSha256=(Get-FileHash (Join-Path $PSScriptRoot 'New-SdkAgentWaitDiagnostic.ps1')).Hash}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
