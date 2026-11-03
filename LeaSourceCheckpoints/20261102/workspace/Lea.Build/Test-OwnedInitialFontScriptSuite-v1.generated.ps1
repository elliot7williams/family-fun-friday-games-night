param(
    [switch]$KeepForegroundHost,
    [Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBuildReceipt,
    [Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBindingReceipt,
    [Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedAdapterReceipt
)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$outRoot=Join-Path $root 'initial-font-script-owned-run-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(!$identity.User.Value.EndsWith('-1001') -or ([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve owned full-suite attempt'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Preserve relocation/testing reserve'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Original utility differs'}
$tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$tokens,[ref]$errors)
foreach($name in @('Space','Pin')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($errors.Count -or $defs.Count -ne 1){throw 'Original input helper shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
$result=[ordered]@{completed=$false;passed=$false;behaviorPassed=$false;footerVerified=$false;traceActivationVerified=$false;traceEnvironmentRestored=$false;runs=$runs;inputFiles=$inputs;error=$null;limitsChanged=$false;productionChanged=$true;scope='Experimental60-source full script suite core1474/view6E34 exact8.0.33 under owned Run; native legacy UIA gate unresolved,original300s/5000ms budgets and identities retained'}
$null=New-Item -ItemType Directory -Path $outRoot
try{
    $null=Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    $controls=Join-Path $workspace 'continuity-evidence/owned-run-adapter-controls-v2/result.json';$null=Pin $controls $ExpectedAdapterReceipt
    $control=Get-Content $controls -Raw|ConvertFrom-Json
    if(!$control.passed -or !$control.inputsUnchanged -or $control.ownerAttention -or $control.controls.Count -ne 5){throw 'Owned adapter controls incomplete'}
    foreach($name in @('OwnedTestProcess.cs','Invoke-OwnedTestRun.ps1')){
        $path=Join-Path $PSScriptRoot $name
        $hashes=@($control.inputs|Where-Object Path -ceq $path|ForEach-Object Hash|Sort-Object -Unique)
        if($hashes.Count -ne 1){throw 'Controlled launch source provenance missing'}
        $null=Pin $path $hashes[0]
    }
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $suite=Join-Path $root 'initial-font-script-suite-v1'
    $buildPath=Join-Path $suite 'result.json';$null=Pin $buildPath $ExpectedBuildReceipt
    $bindingPath=Join-Path $suite 'binding-verification.json';$null=Pin $bindingPath $ExpectedBindingReceipt
    $build=Get-Content $buildPath -Raw|ConvertFrom-Json;$binding=Get-Content $bindingPath -Raw|ConvertFrom-Json
    if($build.currentFullNativeQualified -ne $false -or !$build.legacyUiAUnresolved -or $binding.currentFullNativeQualified -ne $false){throw 'Native legacy qualification must remain unresolved in this candidate'}
    $result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true;$result.hostAdopted=$false
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or !$binding.passed -or $build.sources.Count -ne 60 -or $build.files.Count -ne 65 -or !$build.ownedRunRequested -or !$build.inProcessStartupTraceInverseVerified -or !$build.elementPaintContractInverseVerified -or !$build.classPaintContractInverseVerified){throw 'Owned full-suite build/binding incomplete'}
    if($binding.buildReceiptSha256 -cne $ExpectedBuildReceipt){throw 'Binding does not identify this build'}
    if(!$build.nativeSelectionView -or !$build.printLiveView -or (Get-FileHash (Join-Path $build.runtime 'Lea.Win.dll')).Hash -cne '6E342B9B68C54803B91CF796F082C788DC86E29C1A7CEFAA571302E5E4636B4F' -or (Get-FileHash (Join-Path $build.runtime 'Lea.Engine.dll')).Hash -cne '1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'){throw 'Wrong live print candidate'}
    if($build.relocation.runtimeConfigVersion -cne '8.0.33' -or $build.relocation.configProjections -ne 8 -or $build.relocation.workerChanged){throw 'Wrong runtime candidate'}
    $result.candidate=$build.runtime
    foreach($file in @($build.files)+@($build.fixtureFiles)){$null=Pin $file.path $file.sha256}
    $preflightHelper=Join-Path $PSScriptRoot 'Test-ComponentProbePrerequisites8033.generated.ps1';$null=Pin $preflightHelper '7B2FC0FE6EFA950A72DCFFC63F2251AAD37FC266BED76E4492D821643AE6BC42';. $preflightHelper
    $preflight=Get-LeaComponentProbePrerequisites
    [IO.File]::WriteAllText((Join-Path $outRoot 'component-host-prerequisites.json'),($preflight|ConvertTo-Json -Depth 5))
    if(!$preflight.prerequisitesPresent){throw 'Exact component runtime prerequisites missing'}
    $image=Join-Path $build.runtime 'Lea.Image.exe'
    $sid=[Security.Principal.SecurityIdentifier]::new('S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848')
    if(@((Get-Acl $image).GetAccessRules($true,$true,[Security.Principal.SecurityIdentifier])|Where-Object IdentityReference -eq $sid).Count){throw 'Existing decoder grant: preserve it and prepare a fresh candidate'}
    $footerHelper=Join-Path $PSScriptRoot 'Read-LeaSuiteFooter.ps1';$null=Pin $footerHelper 'CE0005069067DDA4810317C0B9BFAF83339CD0DC929713458F76336FAC82D3B2';. $footerHelper
    $traceHelper=Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1';$null=Pin $traceHelper;. $traceHelper
    $result.before=@(foreach($file in $build.files){@{path=$file.path;sha256=$file.sha256;sddl=(Get-Acl -LiteralPath $file.path).Sddl}})
    foreach($input in $inputs){$leases.Add([IO.File]::Open($input.path,'Open','Read','Read'))}
    $run=Invoke-OwnedTestRun -Name 'full-script-timed' -Application $build.binary -Arguments @(('"'+$build.fixtureSource+'"')) -WorkingDirectory $build.runtime -OutputDirectory $outRoot -Seconds 300 -Records $runs
    $result.completed=$true;$result.behaviorPassed=$run.passed
    try{
        $footer=Read-LeaSuiteFooter -Lines @(Get-Content -LiteralPath $run.stdout)
        $result.checks=$footer.Checks;$result.footerVerified=$true
    }catch{$result.completionError=$_.Exception.ToString();if($run.passed){throw}}
    if(!$run.passed){$result.error='Owned full-suite child failed; retain behavior/deadline failure even when cleanup is independently verified'}
}catch{$result.error=$_.Exception.ToString()}
finally{
    try{
        if($runs.Count -eq 1 -and (Test-Path -LiteralPath $runs[0].stderr) -and (Get-Command Read-AllStartupTraceProof -ErrorAction SilentlyContinue)){
            $proof=Read-AllStartupTraceProof -Lines @(Get-Content -LiteralPath $runs[0].stderr) -StreamsComplete ([bool]($runs[0].stdoutComplete -and $runs[0].stderrComplete)) -ExitObserved ([bool]$runs[0].exitObserved) -TimedOut ([bool]$runs[0].timedOut) -KillRequested ([bool]$runs[0].killRequested)
            $result.traceActivationVerified=$proof.traceActivationVerified;$result.traceEnvironmentRestored=$proof.traceEnvironmentRestored
        }
    }catch{$result.traceProofError=$_.Exception.ToString()}
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in $result.before){$sddl=(Get-Acl $file.path).Sddl;$hash=(Get-FileHash $file.path).Hash;if($sddl -cne $file.sddl -or $hash -cne $file.sha256){@{path=$file.path;bytesChanged=$hash -cne $file.sha256;before=$file.sddl;after=$sddl}}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and $result.behaviorPassed -and $result.footerVerified -and $result.traceActivationVerified -and $result.traceEnvironmentRestored -and !$result.error -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaOwnedFullSuiteInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 9))
    [pscustomobject]$result|Select-Object completed,passed,behaviorPassed,footerVerified,ownerAttention,error|ConvertTo-Json -Compress
}
