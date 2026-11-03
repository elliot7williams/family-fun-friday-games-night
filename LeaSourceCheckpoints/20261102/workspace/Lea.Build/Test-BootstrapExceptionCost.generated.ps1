param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/bootstrap-exception-cost-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve cost diagnostic'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Utility shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Expected){throw "Cost diagnostic input differs: $Path"}
    $inputs.Add(@{path=$Path;sha256=$Expected})
}
$result=[ordered]@{diagnosticCaptured=$false;candidate=(Join-Path $outRoot 'runtime');inputs=$inputs;runs=$runs;error=$null;fullSuitePassed=$false;budgetChanged=$false;productionChanged=$false}
Space;$null=New-Item -ItemType Directory -Path $result.candidate
try{
    if(@(Get-Process -Name 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap runtime tests'}
    $calibration=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/native-counter-calibration-v2/result.json'
    Hold $calibration '468022F6F7178448A97D590B404C0F50BE9DE8B10312B4CF0AF5C0548F076D66'
    $control=Get-Content -LiteralPath $calibration -Raw|ConvertFrom-Json
    if(!$control.passed -or !$control.inputsUnchanged -or $control.ownerAttention -or !$control.observation.gcMonotonic -or !$control.observation.allocationSupported -or !$control.observation.allocationAdvanced){throw 'Counter calibration incomplete'}
    $base=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/numeric-references-script-suite-v1'
    $proof=Join-Path $base 'result.json';Hold $proof '8992F34CA977FA5DF8F65501E8495342F53D5538EC524682FC826B6B4C192071'
    $b=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.sources.Count -ne 60 -or $b.files.Count -ne 65){throw 'Matched suite incomplete'}
    $known=@{};foreach($f in @($b.inputs)+@($b.files)){$known[$f.path]=$f.sha256}
    foreach($file in $b.files){
        Hold $file.path $file.sha256
        $relative=[IO.Path]::GetRelativePath($b.runtime,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Runtime member escapes'}
        if($relative -cin @('Lea.Script.Tests.exe','Lea.Script.Tests.pdb')){continue}
        $target=Join-Path $result.candidate $relative;$null=New-Item -ItemType Directory -Path (Split-Path $target) -Force
        Copy-Item -LiteralPath $file.path -Destination $target
    }
    $overlayHelper=Join-Path $PSScriptRoot 'New-BootstrapCostProgram.ps1'
    Hold $overlayHelper '776C702F074F4E49471815286D1B546A3E10A526727C2637F0CB382792E9F163'
    . $overlayHelper
    Hold $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    $options=[Collections.Generic.List[string]]::new();$count=0;$changed=0
    foreach($line in Get-Content -LiteralPath (Join-Path $base 'tests.rsp')){
        if($line.StartsWith('/out:')){$line='/out:"'+(Join-Path $result.candidate 'Lea.Script.Tests.exe')+'"'}
        elseif($line.StartsWith('/pdb:')){$line='/pdb:"'+(Join-Path $result.candidate 'Lea.Script.Tests.pdb')+'"'}
        elseif($line -match '^"(.+\.cs)"$'){
            $from=$Matches[1];$relative=[IO.Path]::GetRelativePath((Join-Path $base 'source'),$from)
            $pin=@($b.sources|Where-Object path -ceq $relative)
            if($pin.Count -ne 1){throw 'Source binding missing'};Hold $from $pin[0].sha256
            $to=Join-Path $outRoot ('source/'+$relative);$null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
            if($relative -ceq 'Lea.Script.Tests\Program.cs'){
                $overlay=New-BootstrapCostProgram ([IO.File]::ReadAllText($from))
                if(!$overlay.inverseVerified -or !$overlay.assertionPreserved -or $overlay.stopCount -ne 1){throw 'Cost overlay incomplete'}
                [IO.File]::WriteAllText($to,$overlay.text,[Text.UTF8Encoding]::new($false));$changed++
            }else{Copy-Item -LiteralPath $from -Destination $to}
            Hold $to (Get-FileHash -LiteralPath $to).Hash;$line='"'+$to+'"';$count++
        }elseif($line -match '^/reference:"([^"]+)"$'){
            $path=$Matches[1];if(!$known.ContainsKey($path)){throw 'Reference binding missing'};Hold $path $known[$path]
            if($path.StartsWith($b.runtime+'\',[StringComparison]::OrdinalIgnoreCase)){$path=$path.Replace($b.runtime,$result.candidate);Hold $path (Get-FileHash -LiteralPath $path).Hash}
            $line='/reference:"'+$path+'"'
        }
        $options.Add($line)
    }
    if($count -ne 60 -or $changed -ne 1){throw 'Diagnostic source inventory differs'}
    $helper=Join-Path $PSScriptRoot 'BootstrapExceptionObserver.cs';Hold $helper 'C35D6474E8F3D41E62C6C6823BE18372F284D6187419C57F1F441FCB9AAD9C84'
    $copy=Join-Path $outRoot 'source/BootstrapExceptionObserver.cs';Copy-Item -LiteralPath $helper -Destination $copy;Hold $copy (Get-FileHash -LiteralPath $copy).Hash;$options.Add('"'+$copy+'"')
    $native=Join-Path $PSScriptRoot 'NativeProcessSample.cs';Hold $native 'E5C089C8E91CFBA9B7A56FBB27ACF57DB67CDCF723641EF7884A9563BA01B3FF'
    $nativeCopy=Join-Path $outRoot 'source/NativeProcessSample.cs';Copy-Item -LiteralPath $native -Destination $nativeCopy;Hold $nativeCopy 'E5C089C8E91CFBA9B7A56FBB27ACF57DB67CDCF723641EF7884A9563BA01B3FF';$options.Add('"'+$nativeCopy+'"')
    $rsp=Join-Path $outRoot 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $hash
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $compile=Run 'compile-cost-probe' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Cost diagnostic compilation failed'}
    $result.before=@(Get-ChildItem -LiteralPath $result.candidate -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $run=Run 'same-context-cost' (Join-Path $result.candidate 'Lea.Script.Tests.exe') @(('"'+$b.fixtureSource+'"'),'--bootstrap-cost-diagnostic') 60
    $lines=@(Get-Content -LiteralPath $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COST ')})
    if($lines.Count -ne 1 -or !$run.exitObserved -or $run.timedOut -or $run.killRequested -or !$run.stdoutComplete -or !$run.stderrComplete){throw 'Diagnostic lifecycle/output incomplete'}
    $report=$lines[0].Substring('LEA_BOOTSTRAP_COST '.Length)|ConvertFrom-Json
    if(!$report.handlerRemoved -or $report.overflow -or $report.callbackErrors -ne 0){throw 'Exception observer incomplete'}
    $phases=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE \d+ \d+ (begin|end)$'})
    if($phases.Count -ne 17){throw 'Diagnostic escaped the intended prefix'}
    for($i=0;$i -lt 8;$i++){
        if($phases[$i*2] -cnotmatch ('^LEA_SUITE_PHASE \d+ '+$i+' begin$') -or $phases[$i*2+1] -cnotmatch ('^LEA_SUITE_PHASE \d+ '+$i+' end$')){throw 'Diagnostic prefix order differs'}
    }
    if($phases[16] -cnotmatch '^LEA_SUITE_PHASE \d+ 8 begin$'){throw 'Computed diagnostic not reached'}
    $stops=@(Get-Content -LiteralPath $run.stdout|Where-Object {$_ -ceq 'Diagnostic stop after computed-script assertion; not full suite.'}).Count
    if($report.executionOk -and $report.mutations -eq 6 -and ($stops -ne 1 -or !$run.passed)){throw 'Successful diagnostic did not stop cleanly'}
    $failures=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_.StartsWith('System.InvalidOperationException: FAIL computed script:')})
    if(!$report.executionOk -and ($run.exitCode -ne 1 -or $stops -ne 0 -or $failures.Count -ne 1)){throw 'Unexpected exception diagnostic outcome'}
    $result.prefixVerified=$true;$result.earlyStopCount=$stops
    $result.observation=$report;$result.overlayInverseVerified=$true;$result.diagnosticCaptured=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.candidateUnchanged=if($result.before){@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl}).Count -eq 0}else{$false}
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.diagnosticCaptured=$result.diagnosticCaptured -and $result.inputsUnchanged -and $result.candidateUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaCostInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $global:LeaCostResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 8))
}
[pscustomobject]$result|Select-Object diagnosticCaptured,inputsUnchanged,candidateUnchanged,ownerAttention,error|ConvertTo-Json
