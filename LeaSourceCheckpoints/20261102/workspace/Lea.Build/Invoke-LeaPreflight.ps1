param([Parameter(Mandatory)][string]$Root)
$ErrorActionPreference='Stop'
try{
    . (Join-Path $PSScriptRoot 'Test-RuntimePrerequisites.ps1')
    $result=Get-LeaRuntimePrerequisites -Root $Root
    $result|ConvertTo-Json -Depth 6
    if(!$result.prerequisitesPresent){exit 2}
}catch{
    [pscustomobject]@{prerequisitesPresent=$false;error=$_.Exception.Message;runtimeExecuted=$false}|ConvertTo-Json
    exit 1
}
