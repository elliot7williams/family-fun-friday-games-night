param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$outRoot=Join-Path $root 'current-lazy-run-v3'
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
$result=[ordered]@{completed=$false;passed=$false;candidate=$null;inputFiles=$inputs;runs=$runs;error=$null;limitsChanged=$false;productionChanged=$false;scope='Instrumented exact normal worker and lazy interpreter current full-suite attempt; not release certification'}
Space;$null=New-Item -ItemType Directory $outRoot
try{
    $null=Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    if(@(Get-Process 'Lea.Script.Worker','Lea.Script.Tests','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap existing tests/browser'}
    $buildPath=Join-Path $root 'current-lazy-suite-v2/result.json';$null=Pin $buildPath '0D71CCA8558E48610A8B90C8A330D6D0EC1B7338E1174086E10B9AE5EEF395BA'
    $proofPath=Join-Path $root 'current-lazy-suite-v2/binding-verification.json';$null=Pin $proofPath '8B25BF778552E8D2CC4A721A4DC8CD807D5A2E266107E61871E9A3972B69542B'
    $build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
    $proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or !$proof.passed -or $build.files.Count -ne 65){throw 'Relocated suite incomplete'}
    $footerHelper=Join-Path $workspace 'Lea.Build/Read-LeaSuiteFooter.ps1'
    $null=Pin $footerHelper 'CE0005069067DDA4810317C0B9BFAF83339CD0DC929713458F76336FAC82D3B2'
    . $footerHelper
    # A previous decoder grant must not silently become the private-file baseline.
    $imageAcl=Get-Acl -LiteralPath (Join-Path $build.runtime 'Lea.Image.exe')
    $decoderSid=[Security.Principal.SecurityIdentifier]::new('S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848')
    if(@($imageAcl.GetAccessRules($true,$true,[Security.Principal.SecurityIdentifier])|Where-Object {$_.IdentityReference -eq $decoderSid}).Count){throw 'Existing decoder grant: require separately prepared private candidate; preserve previous ACL evidence'}
    $result.candidate=$build.runtime
    foreach($file in $build.files){$null=Pin $file.path $file.sha256}
    foreach($file in $build.fixtureFiles){$null=Pin $file.path $file.sha256}
    $result.before=@(Get-ChildItem -LiteralPath $result.candidate -File -Recurse|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    foreach($input in $inputs){$leases.Add([IO.File]::Open($input.path,'Open','Read','Read'))}
    $run=Run 'full-script-timed' $build.binary @(('"'+$build.fixtureSource+'"')) 300
    $result.completed=$true
    $completion=Read-LeaSuiteFooter -Lines @(Get-Content -LiteralPath $run.stdout)
    $result.checks=$completion.Checks
    $result.behaviorPassed=$run.passed
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
