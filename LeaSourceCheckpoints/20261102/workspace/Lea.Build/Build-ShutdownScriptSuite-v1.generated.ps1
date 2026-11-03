param([switch]$KeepForegroundHost,[switch]$ClassPaintContract,[switch]$StartupObservation,[switch]$ElementPaintContract,[switch]$TreeStartupObservation,[switch]$AllStartupTrace,[switch]$InProcessStartupTrace,[switch]$OwnedRunCandidate)
$ErrorActionPreference='Stop'
if(!$OwnedRunCandidate){throw 'Sx suite requires the owned timed candidate flags'}
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$outRoot=Join-Path $root 'shutdown-script-suite-v1'
if($OwnedRunCandidate -and !$InProcessStartupTrace){throw 'Owned Run candidate must retain in-process timed trace'}
if($InProcessStartupTrace -and !$AllStartupTrace){throw 'In-process trace requires all-startup trace candidate'}
if($AllStartupTrace -and (!$ElementPaintContract -or !$ClassPaintContract -or $StartupObservation -or $TreeStartupObservation)){throw 'All-startup trace requires both paint contracts and no one-test observer'}
if($TreeStartupObservation -and (!$ElementPaintContract -or !$ClassPaintContract -or $StartupObservation)){throw 'Tree observer requires both current paint contracts without removal observer'}
if($StartupObservation -and !$ClassPaintContract){throw 'Observer must retain current classList paint contract'}
if($ElementPaintContract -and (!$ClassPaintContract -or $StartupObservation)){throw 'Receiver contract requires current paint contract without startup observer'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$id=[Security.Principal.WindowsIdentity]::GetCurrent()
if(!$id.User.Value.EndsWith('-1001') -or ([Security.Principal.WindowsPrincipal]::new($id)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve build attempt'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Runner changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Runner shape changed'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
function Run([string]$name,[string]$exe,[string[]]$arguments,[int]$seconds){
    Invoke-OwnedTestRun -Name $name -Application $exe -Arguments $arguments -WorkingDirectory $workspace -OutputDirectory $outRoot -Seconds $seconds -Records $runs
}
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$path,[string]$expected){
    $full=[IO.Path]::GetFullPath($path);$stream=[IO.File]::Open($full,'Open','Read','Read');$leases.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if(!$expected -or $hash -cne $expected){throw ('Input differs: '+$full)}
    $inputs.Add(@{path=$full;sha256=$hash})
}
$result=[ordered]@{passed=$false;candidate=$outRoot;inputs=$inputs;runs=$runs;sources=[Collections.Generic.List[object]]::new();error=$null;suiteExecuted=$false;productionChanged=$false;scope='Compile-only matched current native core/view suite; runtime validation separate'}
if($AllStartupTrace){$result.allStartupTraceRequested=$true;$result.scope='Fresh normal60-source suite for timed tracing across all worker launches; no observer or product binary changes. Diagnostic runtime validation separate.'}
if($OwnedRunCandidate){$result.ownedRunRequested=$true}
$result.nativeSelectionView=$true;$result.printLiveView=$true;$result.productionChanged=$true;$result.scope='Compile-only60-source experimental document selection view2225/coreA85, exact8.0.33 configs; full native legacy UIA gate remains open and script runtime qualification is separate'
Space;if((Get-PSDrive C).Free -lt 600MB){throw 'Relocation reserve required'};$null=New-Item -ItemType Directory $outRoot
try{
    Hold $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash
    $currentProofPath=Join-Path $workspace 'continuity-evidence/shutdown-image-live-v1/result.json'
    Hold $currentProofPath '3890C80383BFD39523EA695757DF4B4014EBB8C99C69527480EF8996D7483636'
    $currentProof=Get-Content $currentProofPath -Raw|ConvertFrom-Json
    if(!$currentProof.passed -or !$currentProof.decoderAclRestored -or !$currentProof.inputsUnchanged -or $currentProof.ownerAttention){throw 'Current matched view/print proof incomplete'}
    $result.currentFullNativeQualified=$false
    $result.legacyUiAUnresolved=$true
    $result.hostAdopted=$false
    $nativeProofPath=Join-Path $workspace 'continuity-evidence/print-live-native-v1/bound-outcome.json'
    Hold $nativeProofPath '496EF27807197530B8F83F8E7A0AAF4AFC5B2BC036866AE7C10CC9B5815D937C'
    $nativeProof=Get-Content $nativeProofPath -Raw|ConvertFrom-Json
    if(!$nativeProof.nativeFunctionalPassed -or !$nativeProof.focusedPassed -or !$nativeProof.knownPersistentGrantClassificationOnly -or $nativeProof.policyBroadened -or $nativeProof.viewSha256 -cne '568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8' -or $nativeProof.coreSha256 -cne 'A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'){throw 'Live print native qualification incomplete'}
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'build.ps1')
    $base=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/source-cross-module-suite-v2'
    $buildPath=Join-Path $base 'result.json';Hold $buildPath '779061CB3422B80038653503E7AA375521E5A868DBA30FA88A0509CC206069F5'
    $build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.sourceCount -ne 60){throw 'Frozen suite incomplete'}
    function OriginalHash([string]$path){
        $hashes=@(@($build.inputs)+@($build.files)|Where-Object path -ceq $path|ForEach-Object sha256|Sort-Object -Unique)
        if($hashes.Count -ne 1){throw ('Missing provenance: '+$path)}
        return $hashes[0]
    }
    $relocationHelper=Join-Path $workspace 'continuity-evidence/shutdown-script-preparation-v1/initializer.ps1'
    Hold $relocationHelper '7F9DC6FA31B823B233178FB9AFF67123DE329C623E5292D5D1475BE827E62640'
    . $relocationHelper
    $result.relocation=Initialize-CurrentNativeScriptSuite $build $base $outRoot $workspace
    $result.runtime=$result.relocation.runtime
    $overlayPath=Join-Path $root 'suite-phase-source-v1/result.json';Hold $overlayPath '22948AC8AD6303C1D1FEAEFFE15CB30FBB03871619445D45F6606E176628C8C3'
    $overlay=Get-Content -LiteralPath $overlayPath -Raw|ConvertFrom-Json
    if(!$overlay.inverseVerified -or $overlay.stages.Count -ne 89 -or $overlay.executed -or $overlay.compiled){throw 'Overlay proof incomplete'}
    if($InProcessStartupTrace){
        $traceHelper=Join-Path $PSScriptRoot 'New-AllStartupTraceSource.ps1';Hold $traceHelper (Get-FileHash $traceHelper).Hash;. $traceHelper
        $phaseProgram=Join-Path $root 'suite-phase-source-v1/Program.cs';Hold $phaseProgram $overlay.candidateSha256
        $traceProgram=New-AllStartupTraceSource ([IO.File]::ReadAllText($phaseProgram))
        if(!$traceProgram.inverseVerified){throw 'In-process trace inverse missing'}
        $tracePath=Join-Path $outRoot 'Program.trace.cs'
        [IO.File]::WriteAllText($tracePath,$traceProgram.text,[Text.UTF8Encoding]::new($false));Hold $tracePath (Get-FileHash $tracePath).Hash
        $result.inProcessStartupTraceInverseVerified=$true
    }
    $recipe=Join-Path $base 'tests.rsp';Hold $recipe '804234F6C0058ABE81D0914A79630DC9FEC0B942FCE6521B899CB5DE8F527E9F'
    $options=[Collections.Generic.List[string]]::new();$oldSource=Join-Path $base 'source';$outputs=0;$pdbs=0
    if($ClassPaintContract){
        $helper=Join-Path $workspace 'Lea.Build/New-ClassListPaintContractSource.ps1';Hold $helper (Get-FileHash $helper).Hash
        $rootView=Join-Path $workspace 'Lea.Script.Tests/View.cs';Hold $rootView (Get-FileHash $rootView).Hash
        $originalView=Join-Path $oldSource 'Lea.Script.Tests/View.cs';Hold $originalView (OriginalHash $originalView)
        $originalClasses=Join-Path $oldSource 'Lea.Script.Tests/ClassLists.cs';Hold $originalClasses (OriginalHash $originalClasses)
        . $helper
        $contract=New-ClassListPaintContractSource ([IO.File]::ReadAllText($originalView)) ([IO.File]::ReadAllText($originalClasses)) ([IO.File]::ReadAllText($rootView))
        if(!$contract.inverseVerified){throw 'ClassList contract inverse missing'}
        $result.classPaintContractInverseVerified=$true
    }
    if($ElementPaintContract){
        # Both callers render the same javascript-attributes.html fixture.
        $receivers=Join-Path $oldSource 'Lea.Script.Tests/ElementReceivers.cs';Hold $receivers (OriginalHash $receivers)
        $receiverContract=New-ClassListPaintContractSource ([IO.File]::ReadAllText($originalView)) ([IO.File]::ReadAllText($receivers)) ([IO.File]::ReadAllText($rootView))
        if(!$receiverContract.inverseVerified -or $receiverContract.view -cne $contract.view){throw 'Shared attribute-fixture contract differs'}
        $result.elementPaintContractInverseVerified=$true
    }
    if($StartupObservation -or $TreeStartupObservation){
        $helper=Join-Path $PSScriptRoot 'New-WorkerStartupObservationSource.ps1';Hold $helper (Get-FileHash $helper).Hash
        $observedName=if($TreeStartupObservation){'Tree'}else{'Removal'}
        $originalRemoval=Join-Path $oldSource ('Lea.Script.Tests/'+$observedName+'.cs');Hold $originalRemoval (OriginalHash $originalRemoval)
        . $helper
        $observation=New-WorkerStartupObservationSource ([IO.File]::ReadAllText($originalRemoval))
        if(!$observation.inverseVerified -or !$observation.originalAssertionsPreserved){throw 'Observer source proof incomplete'}
        $result.startupObservationInverseVerified=$true
        $result.observedStartup=$observedName
        $result.scope='Diagnostic full suite with unchanged preceding tests, current paint contracts, normal60FF worker and one bounded startup observation; not release qualification'
    }
    foreach($line in [IO.File]::ReadAllLines($recipe)){
        if($line.StartsWith('"') -and $line.EndsWith('.cs"')){
            $from=$line.Trim('"');Hold $from (OriginalHash $from)
            if(!$from.StartsWith($oldSource+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Source path escapes'}
            $relative=$from.Substring($oldSource.Length+1);$selected=$from
            $changed=$relative -ceq 'Lea.Script.Tests\Program.cs'
            if($changed){$selected=Join-Path $root 'suite-phase-source-v1/Program.cs';Hold $selected $overlay.candidateSha256}
            if($changed -and $InProcessStartupTrace){$selected=$tracePath}
            if($ClassPaintContract -and $relative -cin @('Lea.Script.Tests\View.cs','Lea.Script.Tests\ClassLists.cs')){
                $selected=Join-Path $outRoot ([IO.Path]::GetFileNameWithoutExtension($relative)+'.contract.cs')
                $text=if($relative -ceq 'Lea.Script.Tests\View.cs'){$contract.view}else{$contract.classes}
                [IO.File]::WriteAllText($selected,$text,[Text.UTF8Encoding]::new($false));Hold $selected (Get-FileHash $selected).Hash
                $changed=$true
            }
            if(($StartupObservation -or $TreeStartupObservation) -and $relative -ceq ('Lea.Script.Tests\'+$observedName+'.cs')){
                $selected=Join-Path $outRoot ($observedName+'.observed.cs')
                [IO.File]::WriteAllText($selected,$observation.text,[Text.UTF8Encoding]::new($false));Hold $selected (Get-FileHash $selected).Hash
                $changed=$true
            }
            if($ElementPaintContract -and $relative -ceq 'Lea.Script.Tests\ElementReceivers.cs'){
                $selected=Join-Path $outRoot 'ElementReceivers.contract.cs'
                [IO.File]::WriteAllText($selected,$receiverContract.classes,[Text.UTF8Encoding]::new($false));Hold $selected (Get-FileHash $selected).Hash
                $changed=$true
            }
            if($relative -ceq 'Lea.Script.Tests\ModernComponents.cs'){
                $original=[IO.File]::ReadAllText($from)
                $bindings=@($result.relocation.componentBindings)+@(@{name='entry';old=$result.relocation.oldManifestSha256;replacement=$result.relocation.newManifestSha256})
                if($bindings.Count -ne 6){throw 'Six dispatcher bindings required'}
                $updated=$original
                foreach($binding in $bindings){if([regex]::Matches($updated,$binding.old).Count -ne 1){throw 'Dispatcher anchor differs'};$updated=$updated.Replace($binding.old,$binding.replacement)}
                $inverse=$updated
                for($i=$bindings.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($bindings[$i].replacement,$bindings[$i].old)}
                if($inverse -cne $original){throw 'Dispatcher source changed beyond digests'}
                $selected=Join-Path $outRoot 'ModernComponents.overlay.cs'
                [IO.File]::WriteAllText($selected,$updated,[Text.UTF8Encoding]::new($false))
                Hold $selected (Get-FileHash -LiteralPath $selected).Hash
                $changed=$true
            }
            $to=Join-Path (Join-Path $outRoot 'source') $relative
            $null=New-Item -ItemType Directory (Split-Path $to) -Force
            Copy-Item -LiteralPath $selected -Destination $to
            $hash=(Get-FileHash -LiteralPath $selected).Hash;Hold $to $hash
            $result.sources.Add(@{path=$relative;sha256=$hash;changed=$changed;originalSha256=(Get-FileHash -LiteralPath $from).Hash})
            $options.Add('"'+$to+'"')
        }elseif($line.StartsWith('/out:')){$outputs++;$options.Add('/out:"'+(Join-Path $result.runtime 'Lea.Script.Tests.exe')+'"')}
        elseif($line.StartsWith('/pdb:')){$pdbs++;$options.Add('/pdb:"'+(Join-Path $result.runtime 'Lea.Script.Tests.pdb')+'"')}
        else{
            if($line.StartsWith('/reference:')){
                $path=$line.Substring(11).Trim('"');$expected=OriginalHash $path
                $newPath=$path.Replace($build.candidate,$result.runtime)
                if($newPath -cne $path){
                    $pins=@($inputs|Where-Object path -ceq $newPath|ForEach-Object sha256|Sort-Object -Unique)
                    if($pins.Count -ne 1){throw 'Relocated reference lacks pinned provenance'}
                    $expected=$pins[0]
                }
                Hold $newPath $expected
                $line='/reference:"'+$newPath+'"'
            }
            $options.Add($line)
        }
    }
    $expectedChanges=if($TreeStartupObservation){6}elseif($StartupObservation -or $ElementPaintContract){5}elseif($ClassPaintContract){4}else{2}
    if($result.sources.Count -ne 60 -or @($result.sources|Where-Object changed).Count -ne $expectedChanges -or $outputs -ne 1 -or $pdbs -ne 1){throw 'Recipe/source coverage differs'}
    if($StartupObservation -or $TreeStartupObservation){
        foreach($name in @('WorkerStartupObserver.cs','ExternalProcessCounters.cs')){
            $from=Join-Path $PSScriptRoot $name;Hold $from (Get-FileHash $from).Hash
            $to=Join-Path $outRoot ('source/Lea.Script.Tests/'+$name);Copy-Item -LiteralPath $from -Destination $to
            $hash=(Get-FileHash $to).Hash;Hold $to $hash
            $result.sources.Add(@{path=('Lea.Script.Tests\'+$name);sha256=$hash;changed=$true;added=$true;originalSha256=$null})
            $options.Add('"'+$to+'"')
        }
        if($result.sources.Count -ne 62){throw 'Expected62 observed suite sources'}
    }
    $rsp=Join-Path $outRoot 'tests.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));$rspHash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $rspHash
    $wrapper=Join-Path $workspace 'Lea.Build/Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
    $compile=Run 'compile-timed-suite' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$rspHash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Diagnostic compilation failed'}
    $result.binary=Join-Path $result.runtime 'Lea.Script.Tests.exe';$result.sha256=(Get-FileHash -LiteralPath $result.binary).Hash
    $result.files=@(Get-ChildItem -LiteralPath $result.runtime -File -Recurse|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
    if($result.files.Count -ne 65){throw 'Expected65 relocated runtime files'}
    $result.fixtureSource=$build.fixtureSource;$result.fixtureFiles=$build.fixtureFiles
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    if(!$result.inputsUnchanged -or $result.ownerAttention){$result.passed=$false}
    if($result.ownerAttention){$global:LeaSuitePhaseInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $result|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $outRoot 'result.json')
    $global:LeaSuitePhaseBuildResult=$result
    [pscustomobject]$result|Select-Object passed,inputsUnchanged,ownerAttention,error,sha256|ConvertTo-Json -Compress
}
