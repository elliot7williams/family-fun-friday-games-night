$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Test-ComponentProbePrerequisites8033.generated.ps1')
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/component-host-prerequisites-8033-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve prerequisite evidence'}
$null=New-Item -ItemType Directory -Path $out
$base=@{windows=$true;os64=$true;launcherPresent=$true;launcherMachine=34404;coreClrPresent=$true;hostFxrPresent=$true}
$cases=@(
    @{name='matching';key=$null;value=$null;expected=$true},
    @{name='non-windows';key='windows';value=$false;expected=$false},
    @{name='32-bit-os';key='os64';value=$false;expected=$false},
    @{name='missing-cli-with-runtime-present';key='launcherPresent';value=$false;expected=$false},
    @{name='wrong-cli-architecture';key='launcherMachine';value=332;expected=$false},
    @{name='missing-coreclr';key='coreClrPresent';value=$false;expected=$false},
    @{name='missing-resolver';key='hostFxrPresent';value=$false;expected=$false}
)
$rows=@(foreach($case in $cases){
    $inventory=$base.Clone();if($case.key){$inventory[$case.key]=$case.value}
    $report=Test-LeaComponentHostInventory ([pscustomobject]$inventory)
    if($report.prerequisitesPresent -ne $case.expected -or $report.runtimeExecuted -or $report.securityOrConfinementVerified){throw ('Prerequisite control failed: '+$case.name)}
    @{name=$case.name;passed=$true}
})
$actual=Get-LeaComponentProbePrerequisites
$result=@{passed=$true;controls=$rows;actual=$actual;helperSha256=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'Test-ComponentProbePrerequisites8033.generated.ps1')).Hash;scope='Synthetic inventory controls and read-only current-machine inventory. No runtime launched or system installation changed.'}
$result|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'result.json')
$result|ConvertTo-Json -Depth 6
