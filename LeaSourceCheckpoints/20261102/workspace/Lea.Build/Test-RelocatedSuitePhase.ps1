param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$outRoot=Join-Path $root 'suite-phase-run-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$id=[Security.Principal.WindowsIdentity]::GetCurrent()
if(!$id.User.Value.EndsWith('-1001') -or ([Security.Principal.WindowsPrincipal]::new($id)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve full-suite attempt'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Runner changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','Pin','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Runner shape changed'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
$result=[ordered]@{completed=$false;passed=$false;candidate=$null;inputFiles=$inputs;runs=$runs;error=$null;limitsChanged=$false;productionChanged=$false;scope='Instrumented relocated baseline full-suite diagnostic; not current lazy release gate'}
Space;$null=New-Item -ItemType Directory $outRoot
try{
    $null=Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    if(@(Get-Process 'Lea.Script.Worker','Lea.Script.Tests','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap existing tests/browser'}
    $buildPath=Join-Path $root 'suite-phase-relocated-v2/result.json';$null=Pin $buildPath '1DF0860AAEF98A61FDBE997A31889228061A2E30D02E9D705C7F9503CA352A18'
    $proofPath=Join-Path $root 'suite-phase-relocated-v2/relocation-verification.json';$null=Pin $proofPath '30E76D099D28F8520DA4AC1FAE074EAE7D7DDF8D3386C0017951A3522AEAF831'
    $build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
    $proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or !$proof.passed -or $build.files.Count -ne 65){throw 'Relocated suite incomplete'}
    $result.candidate=$build.runtime
    foreach($file in $build.files){$null=Pin $file.path $file.sha256}
    foreach($file in $build.fixtureFiles){$null=Pin $file.path $file.sha256}
    $result.before=@(Get-ChildItem -LiteralPath $result.candidate -File -Recurse|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    foreach($input in $inputs){$leases.Add([IO.File]::Open($input.path,'Open','Read','Read'))}
    $run=Run 'full-script-timed' $build.binary @(('"'+$build.fixtureSource+'"')) 300
    $result.completed=$true
    $footer=@(Get-Content -LiteralPath $run.stdout|Where-Object {$_.StartsWith('PASS Lea.Script: ')})
    $result.behaviorPassed=$run.passed -and $footer.Count -eq 1
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in $result.before){$after=(Get-Acl -LiteralPath $file.path).Sddl;$hash=(Get-FileHash -LiteralPath $file.path).Hash;if($after -cne $file.sddl -or $hash -cne $file.sha256){@{path=$file.path;bytesChanged=$hash -cne $file.sha256;before=$file.sddl;after=$after}}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and $result.behaviorPassed -and $result.inputsUnchanged -and !$result.ownerAttention -and $result.changedFiles.Count -eq 0
    if($result.ownerAttention){$global:LeaSuitePhaseRunInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $result|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $outRoot 'result.json')
    $global:LeaSuitePhaseRunResult=$result
    [pscustomobject]$result|Select-Object completed,passed,behaviorPassed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
