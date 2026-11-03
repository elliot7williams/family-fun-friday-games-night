param([switch]$KeepForegroundHost,[ValidateRange(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$candidate=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v7'
$out='I:\LeaBuildStorage\combined-worker-startup-20261101-v'+$Version
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence and reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','CombinedWorkerStartupProbe','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse diagnostic input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw ('Diagnostic input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{diagnosticCaptured=$false;fullSuitePassed=$false;causeProven=$false;limitsChanged=$false;inputs=$inputs;runs=$runs;before=@()}
try{
    Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $prior=Join-Path $root 'continuity-evidence/sdk-full-script-owned-run-v7/result.json';Hold $prior 'CBECBD7932C23EF6FF187ED115A43D792528FE36B92675D4B1456302D2DBEB65'
    $failure=Get-Content $prior -Raw|ConvertFrom-Json
    if($failure.passed -or !$failure.completed -or !$failure.inputsUnchanged -or $failure.ownerAttention -or $failure.changedFiles.Count -or $failure.before.Count -ne 65){throw 'Retained clean failure required'}
    $result.before=$failure.before
    foreach($file in $failure.before){Hold $file.path $file.sha256;if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Candidate ACL changed'}}
    $control=Join-Path $root 'continuity-evidence/temporal-startup-v1/owned-thread-snapshot-control-v1/result.json';Hold $control 'BCC044C4CE8A57C497A7E5D13A339F8A2F3227F4457956A70650FDA5B380E41D'
    $checked=Get-Content $control -Raw|ConvertFrom-Json
    if(!$checked.passed -or !$checked.inputsUnchanged -or $checked.ownerAttention -or $checked.control.checks -ne 8){throw 'Thread snapshot controls incomplete'}
    $fixture=Join-Path $candidate 'fixtures/LeaForge/test-pages/javascript-runtime.html'
    $stagingPath=Join-Path $candidate 'staging.json';Hold $stagingPath 'F693258746A0E08024A72011F9E5E6EE242766FAD468BE4849FF4BCC1C0BFBE8'
    $staging=Get-Content $stagingPath -Raw|ConvertFrom-Json
    $row=@($staging.files|Where-Object path -ceq 'fixtures/LeaForge/test-pages/javascript-runtime.html')
    if($row.Count -ne 1){throw 'Exact computed fixture required'};Hold $fixture $row[0].sha256
    $exe=Join-Path $out 'CombinedWorkerStartupProbe.exe'
    $config=Join-Path $candidate 'runtime/Lea.Script.Tests.exe.config';Hold $config
    Copy-Item -LiteralPath $config -Destination ($exe+'.config');Hold ($exe+'.config') (Get-FileHash $config).Hash
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $args=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'))
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Web.Extensions.dll')){$path=Join-Path $refs $name;Hold $path;$args+=('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Script.dll','Jint.dll','Acornima.dll')){$args+=('/reference:"'+(Join-Path $candidate ('runtime/'+$name))+'"')}
    $snapshotSource='OwnedThreadSnapshot.cs';$snapshotPin='43F229376D069D3BF7CA7BC9CFDA5F0F36AD9BABC80DB125ADF800A40E552D52'
    if($Version -eq 2){
        $snapshotSource='OwnedThreadSnapshotV2.cs';$snapshotPin=''
        $controlSource=Join-Path $root 'Lea.Tests/OwnedThreadSnapshotV2Control.cs';Hold $controlSource
        $controlExe=Join-Path $out 'ThreadSnapshotV2Control.exe'
        $controlArgs=@($args|Where-Object {!$_.StartsWith('/out:')})+@('/out:"'+$controlExe+'"',('"'+(Join-Path $PSScriptRoot $snapshotSource)+'"'),('"'+$controlSource+'"'))
        Hold (Join-Path $PSScriptRoot $snapshotSource)
        $controlBuild=Invoke-OwnedTestRun -Name 'compile-snapshot-control' -Application $compiler -Arguments $controlArgs -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$controlBuild.passed){throw 'New snapshot controls did not compile'};Hold $controlExe
        $controlled=Invoke-OwnedTestRun -Name 'test-snapshot-control' -Application $controlExe -Arguments @() -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
        if(!$controlled.passed -or @(Get-Content $controlled.stdout|Where-Object {$_ -ceq 'PASS THREAD SNAPSHOT V2 13'}).Count -ne 1){throw 'New snapshot controls failed'}
        $result.snapshotControls=13;$args+='/define:LEA_THREAD_V2'
    }
    foreach($entry in @(@('CombinedWorkerStartupProbe.cs',''),@('ExternalProcessCounters.cs','128CCB2A4B1E97D6D98B37A6E6A9B70AF7FF15F3DFCBA66CAA274E2D8C95B886'),@($snapshotSource,$snapshotPin))){$path=Join-Path $PSScriptRoot $entry[0];Hold $path $entry[1];$args+=('"'+$path+'"')}
    $compiled=Invoke-OwnedTestRun -Name 'compile-startup-probe' -Application $compiler -Arguments $args -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Startup diagnostic compilation failed'};Hold $exe
    $tested=Invoke-OwnedTestRun -Name 'observe-startup' -Application $exe -Arguments @(('"'+(Join-Path $candidate 'runtime')+'"'),('"'+$fixture+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$tested.passed){throw 'Diagnostic capture/retirement failed; retain evidence'}
    $lines=@(Get-Content $tested.stdout|Where-Object { $_.StartsWith('COMBINED_WORKER_STARTUP ',[StringComparison]::Ordinal) })
    if($lines.Count -ne 1){throw 'Unique startup observation required'}
    $report=$lines[0].Substring('COMBINED_WORKER_STARTUP '.Length)|ConvertFrom-Json
    if(!$report.valid -or !$report.exited -or !$report.observerRetired -or !$report.traceRestored -or $report.sampleError -or $report.overflow -or $report.samples.Count -lt 1 -or $report.samples.Count -gt 60 -or $report.startupBudgetMs -ne 5000 -or $report.executionBudgetMs -ne 2000 -or $report.cpuRate -ne 2500 -or $report.memoryBytes -ne 134217728){throw 'Bounded startup report incomplete'}
    $result.observation=$report;$result.diagnosticCaptured=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in $result.before){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){$file.path}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.diagnosticCaptured=$result.diagnosticCaptured -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaCombinedStartupInputLeases=$holds}
    $result|ConvertTo-Json -Depth 15|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object diagnosticCaptured,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
