param([ValidateSet(1,2,3,4,5)][int]$Version=5)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace ('continuity-evidence/print-snapshot-v'+$Version)
if(Test-Path $out){throw 'Preserve snapshot test'}
$core=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/Lea.Engine.dll'
$paths=@($core,(Join-Path $workspace 'Lea.Print/Ps.cs'),(Join-Path $workspace 'Lea.Tests/PrintSnapshot.cs'),$PSCommandPath)
$leases=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;checks=0;inputs=$inputs;scope='Pure managed snapshot tests on pinned core; no UI, renderer, network, printer, host or release qualification'}
$null=New-Item -ItemType Directory -Path $out
try{
    foreach($path in $paths){$s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s);$inputs.Add(@{path=$path;sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))})}
    if($inputs[0].sha256 -cne '52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'){throw 'Core differs'}
    $null=[Reflection.Assembly]::LoadFrom($core)
    $references=@(Get-ChildItem (Join-Path $PSHOME 'ref') -Filter '*.dll'|ForEach-Object FullName)
    $result.compilerReferences=@($references|ForEach-Object{Get-FileHash $_}|Select-Object Path,Hash)
    Add-Type -Path $paths[1],$paths[2] -ReferencedAssemblies ($references+@($core))
    $result.checks=[PrintSnapshotChecks]::Run()
    $result.passed=$result.checks -eq 40
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.passed=$result.passed -and $result.inputsUnchanged
    foreach($s in $leases){$s.Dispose()}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 5))
    [pscustomobject]$result|Select-Object passed,checks,inputsUnchanged,error|ConvertTo-Json
}
