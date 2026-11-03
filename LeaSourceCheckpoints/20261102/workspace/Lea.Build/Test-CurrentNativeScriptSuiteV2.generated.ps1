param([switch]$KeepForegroundHost,[ValidateSet(1,2,3,4,5,6,7)][int]$SuiteVersion=1,[string]$ExpectedBuildReceipt='916925B96F63ECFE75526F6BCBF742326441FF840FFD137C3CCE9CC56DA6C7CB',[string]$ExpectedBindingReceipt='4785FEFBCE687D8BDA182A8B660126F94A6B1AAA1686B117422385FDC47F8C75')
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$outRoot=Join-Path $root $(if($SuiteVersion -eq 7){'current-native-script-run-v8'}elseif($SuiteVersion -eq 6){'current-native-script-run-v7'}elseif($SuiteVersion -eq 5){'current-native-script-run-v6'}elseif($SuiteVersion -eq 4){'current-native-script-run-v5'}elseif($SuiteVersion -eq 3){'current-native-script-run-v4'}elseif($SuiteVersion -eq 2){'current-native-script-run-v3'}else{'current-native-script-run-v2'})
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
$result=[ordered]@{completed=$false;passed=$false;behaviorPassed=$false;footerVerified=$false;candidate=$null;inputFiles=$inputs;runs=$runs;error=$null;limitsChanged=$false;productionChanged=$false;scope='Instrumented exact normal worker and lazy interpreter current full-suite attempt; not release certification'}
$result.traceEnvironmentRestored=$SuiteVersion -ne 7
$traceProofReady=$false
Space;$null=New-Item -ItemType Directory $outRoot
try{
    $null=Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    if($SuiteVersion -eq 7){
        $traceProofHelper=Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1'
        $null=Pin $traceProofHelper; . $traceProofHelper
        $traceProofReady=$true
    }
    if(@(Get-Process 'Lea.Script.Worker','Lea.Script.Tests','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap existing tests/browser'}
    $suite=Join-Path $root ('current-native-script-suite-v'+$SuiteVersion)
    $buildPath=Join-Path $suite 'result.json';$null=Pin $buildPath $ExpectedBuildReceipt
    $proofPath=Join-Path $suite 'binding-verification.json';$null=Pin $proofPath $ExpectedBindingReceipt
    $build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
    $proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or !$proof.passed -or $build.files.Count -ne 65){throw 'Relocated suite incomplete'}
    if($SuiteVersion -ge 2 -and !$build.classPaintContractInverseVerified){throw 'Contract inverse proof missing'}
    if($SuiteVersion -in @(3,5) -and !$build.startupObservationInverseVerified){throw 'Observer inverse proof missing'}
    if($SuiteVersion -ge 4 -and !$build.elementPaintContractInverseVerified){throw 'Receiver inverse proof missing'}
    if($SuiteVersion -eq 5 -and $build.observedStartup -cne 'Tree'){throw 'Tree observation target differs'}
    if($SuiteVersion -eq 6){
        if(!$build.allStartupTraceRequested -or $build.startupObservationInverseVerified){throw 'All-startup trace build shape differs'}
        $traceHelper=Join-Path $PSScriptRoot 'Invoke-WorkerStartupTrace.ps1'
        $null=Pin $traceHelper; . $traceHelper
    }
    if($SuiteVersion -eq 7 -and (!$build.allStartupTraceRequested -or !$build.inProcessStartupTraceInverseVerified -or $build.startupObservationInverseVerified)){throw 'In-process trace build shape differs'}
    $footerHelper=Join-Path $workspace 'Lea.Build/Read-LeaSuiteFooter.ps1'
    $null=Pin $footerHelper 'CE0005069067DDA4810317C0B9BFAF83339CD0DC929713458F76336FAC82D3B2'
    . $footerHelper
    # A previous decoder grant must not silently become the private-file baseline.
    $imageAcl=Get-Acl -LiteralPath (Join-Path $build.runtime 'Lea.Image.exe')
    $decoderSid=[Security.Principal.SecurityIdentifier]::new('S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848')
    if(@($imageAcl.GetAccessRules($true,$true,[Security.Principal.SecurityIdentifier])|Where-Object {$_.IdentityReference -eq $decoderSid}).Count){throw 'Existing decoder grant: require separately prepared private candidate; preserve previous ACL evidence'}
    $preflightHelper=Join-Path $workspace 'Lea.Build/Test-ComponentProbePrerequisites.ps1'
    $null=Pin $preflightHelper '21DF74BFC400ABDE2C5119C4CB8870E8D791521655568546AEA3CDB5DA3A26D5'
    . $preflightHelper
    $preflight=Get-LeaComponentProbePrerequisites
    $preflight|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $outRoot 'component-host-prerequisites.json')
    if(!$preflight.prerequisitesPresent){throw ($preflight.problems -join ' ')}
    $result.candidate=$build.runtime
    foreach($file in $build.files){$null=Pin $file.path $file.sha256}
    foreach($file in $build.fixtureFiles){$null=Pin $file.path $file.sha256}
    $result.before=@(Get-ChildItem -LiteralPath $result.candidate -File -Recurse|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    foreach($input in $inputs){$leases.Add([IO.File]::Open($input.path,'Open','Read','Read'))}
    if($SuiteVersion -eq 6){
        $previousStartupTrace=[Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE')
        $result.traceEnvironmentRestored=$false
        try{$run=Invoke-WorkerStartupTrace {Run 'full-script-timed' $build.binary @(('"'+$build.fixtureSource+'"')) 300}}
        finally{$result.traceEnvironmentRestored=[Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE') -ceq $previousStartupTrace}
    }else{$run=Run 'full-script-timed' $build.binary @(('"'+$build.fixtureSource+'"')) 300}
    $result.completed=$true
    $result.behaviorPassed=$run.passed
    try{
        $completion=Read-LeaSuiteFooter -Lines @(Get-Content -LiteralPath $run.stdout)
        $result.checks=$completion.Checks;$result.footerVerified=$true
    }catch{
        $result.completionError=$_.Exception.ToString()
        if($run.passed){throw}
    }
    if(!$run.passed){$result.error='Full-suite child failed; inspect retained stderr and original process result. Missing footer, if any, is a consequence.'}
}catch{$result.error=$_.Exception.ToString()}
finally{
    if($SuiteVersion -eq 7){
        $result.traceActivationVerified=$false
        $result.traceEnvironmentRestored=$false
        $result.traceProofEvaluated=$false
        try{
            $traceRecords=@($runs|Where-Object name -ceq 'full-script-timed')
            $expectedTracePath=Join-Path $outRoot 'full-script-timed.stderr.txt'
            if($traceProofReady -and $traceRecords.Count -eq 1 -and $traceRecords[0].stderr -ceq $expectedTracePath -and (Test-Path -LiteralPath $expectedTracePath)){
                $traceRecord=$traceRecords[0]
                $traceProof=Read-AllStartupTraceProof -Lines @(Get-Content -LiteralPath $expectedTracePath) -StreamsComplete ([bool]($traceRecord.stdoutComplete -and $traceRecord.stderrComplete)) -ExitObserved ([bool]$traceRecord.exitObserved) -TimedOut ([bool]$traceRecord.timedOut) -KillRequested ([bool]$traceRecord.killRequested)
                $result.traceActivationVerified=$traceProof.traceActivationVerified
                $result.traceEnvironmentRestored=$traceProof.traceEnvironmentRestored
                $result.traceProofEvaluated=$true
            }
        }catch{$result.traceProofError=$_.Exception.ToString()}
        if((!$result.traceActivationVerified -or !$result.traceEnvironmentRestored) -and !$result.error){$result.error='In-process trace activation/restoration proof missing or ineligible'}
    }
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in $result.before){$after=(Get-Acl -LiteralPath $file.path).Sddl;$hash=(Get-FileHash -LiteralPath $file.path).Hash;if($after -cne $file.sddl -or $hash -cne $file.sha256){@{path=$file.path;bytesChanged=$hash -cne $file.sha256;before=$file.sddl;after=$after}}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and $result.behaviorPassed -and $result.footerVerified -and !$result.error -and $result.inputsUnchanged -and !$result.ownerAttention -and $result.changedFiles.Count -eq 0 -and $result.traceEnvironmentRestored
    if($result.ownerAttention){$global:LeaSuitePhaseRunInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $result|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $outRoot 'result.json')
    $global:LeaSuitePhaseRunResult=$result
    [pscustomobject]$result|Select-Object completed,passed,behaviorPassed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
