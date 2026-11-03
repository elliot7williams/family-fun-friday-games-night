$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Test-RuntimePrerequisites.ps1')
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $workspace 'continuity-evidence/runtime-prerequisites-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve preflight tests'}
$null=New-Item -ItemType Directory -Path $out
$rows=[Collections.Generic.List[object]]::new()
$base=@{windows=$true;os64=$true;workerMachine=34404;frameworkVersion='4.8.09032';frameworkName='Microsoft.NETCore.App';frameworkRequired='8.0.32';rollForward='Disable';coreVersions=@('8.0.32');hostFxrPresent=$true;missingFiles=@()}
$cases=@(
    @{name='matching';key=$null;value=$null;expected=$true},
    @{name='non-windows';key='windows';value=$false;expected=$false},
    @{name='32-bit-os';key='os64';value=$false;expected=$false},
    @{name='wrong-apphost';key='workerMachine';value=332;expected=$false},
    @{name='missing-framework';key='frameworkVersion';value=$null;expected=$false},
    @{name='old-framework';key='frameworkVersion';value='4.7.2';expected=$false},
    @{name='newer-framework';key='frameworkVersion';value='4.8.1';expected=$true},
    @{name='wrong-framework-name';key='frameworkName';value='Microsoft.WindowsDesktop.App';expected=$false},
    @{name='wrong-required-version';key='frameworkRequired';value='8.0.31';expected=$false},
    @{name='rollforward-changed';key='rollForward';value='LatestPatch';expected=$false},
    @{name='newer-core-not-substitute';key='coreVersions';value=@('8.0.33','9.0.0');expected=$false},
    @{name='missing-hostfxr';key='hostFxrPresent';value=$false;expected=$false},
    @{name='missing-worker';key='missingFiles';value=@('ScriptWorker/Lea.Script.Worker.exe');expected=$false}
)
foreach($case in $cases){
    $inventory=$base.Clone();if($case.key){$inventory[$case.key]=$case.value}
    $result=Test-LeaPrerequisiteInventory ([pscustomobject]$inventory)
    $rows.Add(@{case=$case.name;passed=$result.prerequisitesPresent -eq $case.expected -and !$result.runtimeExecuted -and !$result.confinementVerified -and !$result.cleanPcValidated;problems=$result.problems})
}
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-package-extracted-v1/extracted/runtime'
$before=@(Get-ChildItem -LiteralPath $root -Recurse -File|ForEach-Object {@{path=$_.FullName;hash=(Get-FileHash -LiteralPath $_.FullName).Hash}})
$actual=Get-LeaRuntimePrerequisites -Root $root
$unchanged=@($before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.hash}).Count -eq 0
$rows.Add(@{case='actual-extracted-candidate';passed=$actual.prerequisitesPresent -and $unchanged;observation=$actual})
$result=@{passed=@($rows|Where-Object {!$_.passed}).Count -eq 0;checks=$rows;candidateBytesUnchanged=$unchanged;runtimeExecuted=$false;sourceSha256=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'Test-RuntimePrerequisites.ps1')).Hash}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
[pscustomobject]@{passed=$result.passed;checks=$rows.Count;candidateBytesUnchanged=$unchanged}|ConvertTo-Json
