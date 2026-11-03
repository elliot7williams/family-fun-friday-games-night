param([switch]$KeepForegroundHost,[ValidateRange(1,9)][int]$Version=1,[switch]$CompilerTrace,[switch]$PrivateCompilerKit,[switch]$ReuseVerifiedKit,[switch]$DocumentTransfer)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace ('continuity-evidence/print-package-cancel-v'+$Version)
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Threading.Thread]::CurrentThread.ApartmentState -ne 'STA' -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit STA TTY owner required'}
if(![Environment]::Is64BitProcess){throw 'Current64-bit browser architecture required'}
if($PrivateCompilerKit -and $ReuseVerifiedKit){throw 'Create or reuse the private kit, not both'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin owner required'}
if((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue) -or (Get-Variable LeaNativePackageLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh attempt and owner required'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$global:LeaNativePackageLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if(!$Hash){$Hash=(Get-FileHash $Path).Hash}
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
function Check-Abi($Native,$Managed){
    if(@($Native.PSObject.Properties).Count -ne $Managed.Count){throw 'ABI property inventory differs'}
    foreach($property in $Native.PSObject.Properties){if(!$Managed.ContainsKey($property.Name) -or [string]$property.Value -ine [string]$Managed[$property.Name]){throw ('ABI mismatch: '+$property.Name)}}
}
$result=[ordered]@{passed=$false;inputs=$inputs;runs=$runs;documentContentSent=$false;physicalPrinterUsed=$false;defaultPrinterMutated=$false;backendAdopted=$false;nativeCreationObservationBudgetMs=30000;nativeRetirementBudgetMs=5000;nativeRpcPreemptive=$false;scope='SDK ABI and retained-original-target empty virtual package cancellation only; native RPC observation is cooperative, no document pages/output or production backend adoption'}
$null=New-Item -ItemType Directory -Path $out
$oldPath=$env:PATH;$job=$null;$retirement=$null
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    $runHelper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $runHelper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path $owned;. $runHelper
    if($CompilerTrace){
        $observer=Join-Path $PSScriptRoot 'Read-OwnedJobMembers.cs';Hold $observer;Add-Type -Path $observer
        $original=[IO.File]::ReadAllText($runHelper).Replace("`r",'')
        $anchor='            if($owner.WaitForExit([int][Math]::Min(10,$remaining))){$row.exitObserved=$true;$row.exitCode=$owner.ExitCode;break}'
        $insert=@'
            if($Name -eq 'compile-sdk-abi' -and (!$row.ContainsKey('diagnosticAt') -or $clock.ElapsedMilliseconds-$row.diagnosticAt -ge 50)){
                if(!$row.ContainsKey('diagnosticMembers')){$row.diagnosticMembers=[Collections.Generic.List[object]]::new()}
                $row.diagnosticAt=$clock.ElapsedMilliseconds
                $handle=$owner.GetType().GetField('job',[Reflection.BindingFlags]'Instance,NonPublic').GetValue($owner)
                $row.diagnosticMembers.Add(@{atMs=$row.diagnosticAt;members=[Lea.Build.OwnedJobMembers]::Read($handle)})
            }
'@
        if([regex]::Matches($original,[regex]::Escape($anchor)).Count -ne 1){throw 'Compiler observation anchor differs'}
        $derived=$original.Replace($anchor,$insert+"`n"+$anchor)
        if($derived.Replace($insert+"`n",'') -cne $original){throw 'Compiler observer inverse differs'}
        $path=Join-Path $out 'Invoke-OwnedObserved.ps1';[IO.File]::WriteAllText($path,$derived);Hold $path;. $path
        $result.readOnlyCompilerObserver=$true;$result.observerInverseVerified=$true
    }
    $vc='C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VC\Tools\MSVC\14.11.25503'
    $sdk='C:\Program Files (x86)\Windows Kits\10'
    $sdkVersion='10.0.16299.0'
    $compiler=Join-Path $vc 'bin/Hostx64/x64/cl.exe';Hold $compiler
    Hold (Join-Path $vc 'bin/Hostx64/x64/link.exe')
    Hold (Join-Path $sdk ('Include/'+$sdkVersion+'/um/documenttarget.h'))
    Hold (Join-Path $sdk ('Lib/'+$sdkVersion+'/um/x64/ole32.lib'))
    if($ReuseVerifiedKit){
        $priorRoot=Join-Path $workspace 'continuity-evidence/print-package-cancel-v3'
        $priorPath=Join-Path $priorRoot 'result.json';Hold $priorPath '46E73C48FF311478C8A3F46F93E5777D0EDA3B94FBB186D7AD0E26F30311E5E0'
        $prior=Get-Content $priorPath -Raw|ConvertFrom-Json
        if(!$prior.passed -or !$prior.nativeCanceled -or !$prior.inputsUnchanged -or $prior.ownerAttention){throw 'Private compiler producer proof incomplete'}
        $manifestPath=Join-Path $priorRoot 'compiler-kit-manifest.json';Hold $manifestPath 'A7BF4C74CBD4EAC8E4D6099835B9B9520E1DDA59CCB952CD8C8DE14420EE4D19'
        $manifest=@(Get-Content $manifestPath -Raw|ConvertFrom-Json)
        if($manifest.Count -ne 56){throw 'Private kit inventory differs'}
        $kit=Join-Path $priorRoot 'compiler-kit';$toolRoot=Split-Path $compiler
        foreach($file in $manifest){
            $relative=[IO.Path]::GetRelativePath($toolRoot,$file.source)
            if([IO.Path]::IsPathFullyQualified($relative) -or $relative.Split('\') -contains '..' -or $file.path -cne (Join-Path $kit $relative) -or [IO.Path]::GetFileName($file.path) -ieq 'vctip.exe'){throw 'Private kit source escape'}
            Hold $file.source $file.sha256;Hold $file.path $file.sha256
        }
        $compiler=Join-Path $kit 'cl.exe'
        $result.privateCompilerKit=@{files=56;manifestSha256='A7BF4C74CBD4EAC8E4D6099835B9B9520E1DDA59CCB952CD8C8DE14420EE4D19';producerReceipt='46E73C48FF311478C8A3F46F93E5777D0EDA3B94FBB186D7AD0E26F30311E5E0';reused=$true;optionalExcluded='vctip.exe';installedToolsChanged=$false;binaryBytesChanged=$false}
    }
    if($PrivateCompilerKit){
        $toolRoot=Split-Path $compiler
        $files=@(Get-ChildItem -LiteralPath $toolRoot -Recurse -File|Where-Object Name -ine 'vctip.exe')
        if((Get-PSDrive C).Free -lt 600MB+($files|Measure-Object Length -Sum).Sum){throw 'Private compiler copy would cross reserve'}
        $kit=Join-Path $out 'compiler-kit';$null=New-Item -ItemType Directory -Path $kit
        $manifest=[Collections.Generic.List[object]]::new()
        foreach($file in $files){
            if($file.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Compiler source link denied'}
            $relative=[IO.Path]::GetRelativePath($toolRoot,$file.FullName)
            if([IO.Path]::IsPathFullyQualified($relative) -or $relative.Split('\') -contains '..'){throw 'Compiler path escape'}
            Hold $file.FullName
            $hash=(Get-FileHash $file.FullName).Hash;$destination=Join-Path $kit $relative
            $null=New-Item -ItemType Directory -Path (Split-Path $destination) -Force
            Copy-Item -LiteralPath $file.FullName -Destination $destination;Hold $destination $hash
            $manifest.Add(@{source=$file.FullName;path=$destination;sha256=$hash;bytes=$file.Length})
        }
        $manifestPath=Join-Path $out 'compiler-kit-manifest.json'
        [IO.File]::WriteAllText($manifestPath,($manifest|ConvertTo-Json -Depth 4));Hold $manifestPath
        $compiler=Join-Path $kit 'cl.exe'
        $result.privateCompilerKit=@{files=$manifest.Count;manifestSha256=(Get-FileHash $manifestPath).Hash;optionalExcluded='vctip.exe';installedToolsChanged=$false;binaryBytesChanged=$false}
    }
    $source=Join-Path $workspace 'Lea.Tests/PrintPackageAbi.cpp';Hold $source
    $copy=Join-Path $out 'PrintPackageAbi.cpp';Copy-Item $source $copy;Hold $copy
    $env:PATH=(Split-Path $compiler)+';'+$oldPath
    $arguments=@('/nologo','/EHsc','/W4','/WX','/MT',('/I"'+(Join-Path $vc 'include')+'"'))
    foreach($part in @('ucrt','shared','um')){$arguments+=('/I"'+(Join-Path $sdk ('Include/'+$sdkVersion+'/'+$part))+'"')}
    $exe=Join-Path $out 'PrintPackageAbi.exe'
    $arguments+=@(('/Fe:"'+$exe+'"'),('/Fo:"'+(Join-Path $out 'PrintPackageAbi.obj')+'"'),('"'+$copy+'"'),'/link',('/LIBPATH:"'+(Join-Path $vc 'lib/x64')+'"'),('/LIBPATH:"'+(Join-Path $sdk ('Lib/'+$sdkVersion+'/um/x64'))+'"'),('/LIBPATH:"'+(Join-Path $sdk ('Lib/'+$sdkVersion+'/ucrt/x64'))+'"'),'ole32.lib')
    $result.compilerArguments=$arguments
    $build=Invoke-OwnedTestRun -Name 'compile-sdk-abi' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$build.passed){throw 'SDK ABI compilation failed'}
    Hold $exe
    $abi=Invoke-OwnedTestRun -Name 'sdk-abi' -Application $exe -Arguments @() -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$abi.passed){throw 'SDK ABI executable failed'}
    $result.nativeAbi=Get-Content $abi.stdout -Raw|ConvertFrom-Json
    if($DocumentTransfer){
        $transferSource=Join-Path $workspace 'Lea.Tests/PrintPackageTransfer.cpp';Hold $transferSource
        Copy-Item $transferSource (Join-Path $out 'PrintPackageTransfer.cpp')
        $fixture=Join-Path $workspace 'continuity-evidence/print-ticket-project-v1/bin/prepared-ticket.xps';Hold $fixture
        $fixtureBytes=[IO.File]::ReadAllBytes($fixture)
        if(!$fixtureBytes.Length -or $fixtureBytes.Length -gt 4MB){throw 'Bounded fixture required'}
        $transferDll=Join-Path $out 'LeaPrintTransfer.dll'
        $dllArguments=@('/nologo','/EHsc','/W4','/WX','/MT','/LD',('/I"'+(Join-Path $vc 'include')+'"'))
        foreach($part in @('ucrt','shared','um','winrt')){$dllArguments+=('/I"'+(Join-Path $sdk ('Include/'+$sdkVersion+'/'+$part))+'"')}
        $dllArguments+=@(('/Fe:"'+$transferDll+'"'),('/Fo:"'+(Join-Path $out 'transfer.obj')+'"'),('"'+$transferSource+'"'),'/link',('/LIBPATH:"'+(Join-Path $vc 'lib/x64')+'"'),('/LIBPATH:"'+(Join-Path $sdk ('Lib/'+$sdkVersion+'/um/x64'))+'"'),('/LIBPATH:"'+(Join-Path $sdk ('Lib/'+$sdkVersion+'/ucrt/x64'))+'"'),'ole32.lib','shlwapi.lib')
        $dllBuild=Invoke-OwnedTestRun -Name 'compile-transfer-dll' -Application $compiler -Arguments $dllArguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$dllBuild.passed){throw 'Transfer DLL compilation failed'}
        Hold $transferDll
        $result.scope='Trusted three-page Lea XPS fixture through original virtual target; completed output readback, not browser adoption or physical printer qualification'
    }
    $managedSource=Join-Path $workspace 'Lea.Tests/PrintPackageLease.cs';Hold $managedSource
    Copy-Item $managedSource (Join-Path $out 'PrintPackageLease.cs')
    Add-Type -Path $managedSource
    $result.managedAbi=[Lea.PrintProbe.PackageLease]::Abi()
    $result.currentUserPrinterBefore=[Lea.PrintProbe.PrinterSafety]::Capture()
    if($DocumentTransfer){[Lea.PrintProbe.PrinterSafety]::RequireManualDefault($result.currentUserPrinterBefore)}
    Check-Abi $result.nativeAbi $result.managedAbi
    $negative=0
    foreach($key in @('factory','statusSize','messageSize')){
        $bad=[Collections.Generic.Dictionary[string,object]]::new($result.managedAbi)
        $bad[$key]='wrong';$denied=$false;try{Check-Abi $result.nativeAbi $bad}catch{$denied=$true}
        if(!$denied){throw 'ABI negative control failed'};$negative++
    }
    $result.abiNegativeControls=$negative
    if(@(Get-CimInstance Win32_Process|Where-Object ParentProcessId -eq $PID).Count){throw 'Native owner has an unexplained live child before creation'}
    $virtual=@(Get-CimInstance Win32_Printer -Filter "Name='Microsoft XPS Document Writer'"|Select-Object Name,DriverName,PortName,Local,Network,Shared,Default)
    if($virtual.Count -ne 1 -or $virtual[0].DriverName -notin @('Microsoft XPS Document Writer','Microsoft XPS Document Writer v4') -or $virtual[0].PortName -notin @('PORTPROMPT:','XPSPort:') -or !$virtual[0].Local -or $virtual[0].Network){throw 'Explicit local virtual driver/port required; no physical/default fallback'}
    $result.virtualPrinterProfile=$virtual[0]
    $result.ownerRuntime=[Environment]::Version.ToString()
    $result.defaultPrintersBefore=@(Get-CimInstance Win32_Printer -Filter 'Default=True'|Select-Object -ExpandProperty Name|Sort-Object)
    if($DocumentTransfer){
        $mode=Get-ItemProperty 'HKCU:\Software\Microsoft\Windows NT\CurrentVersion\Windows' -Name LegacyDefaultPrinterMode -ErrorAction SilentlyContinue
        $result.legacyDefaultPrinterMode=$mode.LegacyDefaultPrinterMode
        if(!$mode -or $mode.LegacyDefaultPrinterMode -ne 1){throw 'Virtual submission blocked while Windows automatic default-printer management is enabled or unknown; do not change user preference automatically'}
    }
    $job=[Lea.PrintProbe.PackageLease]::new()
    $global:LeaNativePackageLeases.Add($job)
    if($DocumentTransfer){$job.LoadTransferBridge($transferDll)}
    $clock=[Diagnostics.Stopwatch]::StartNew()
    $job.Start();$result.creationReturned=$job.CreationReturned
    $result.creationMs=$clock.ElapsedMilliseconds
    if($result.creationMs -ge 30000){throw 'Creation exceeded30s observation budget; original target retained for cancellation'}
    if($DocumentTransfer){
        $submission=[Diagnostics.Stopwatch]::StartNew()
        $result.documentContentSent=$true
        $result.writtenPages=$job.WriteXps($fixtureBytes)
        $result.submissionMs=$submission.ElapsedMilliseconds
        if($result.submissionMs -ge 30000){throw 'Submission exceeded30s cooperative observation budget'}
    }
    $retirement=[Diagnostics.Stopwatch]::StartNew()
    if(!$DocumentTransfer){$job.Cancel();$result.cancelReturned=$job.CancellationReturned}
    $result.terminalWithinRetirement=$job.PumpUntilTerminal([int][Math]::Max(0,5000-$retirement.ElapsedMilliseconds))
    $result.nativeRetirementElapsedMs=$retirement.ElapsedMilliseconds
    $result.nativeElapsedMs=$clock.ElapsedMilliseconds
    $result.nativeStatus=$job.Status.Terminal;$result.statusHistory=$job.Status.History
    $result.outputBytes=$job.OutputBytes
    $expectedCompletion=if($DocumentTransfer){1}else{2}
    if(!$result.terminalWithinRetirement -or $result.nativeRetirementElapsedMs -gt 5000 -or $job.Status.Overflow -or $job.Status.Terminal.Completion -ne $expectedCompletion -or $job.Status.Terminal.PackageStatusCode -lt 0){throw 'No verified expected original-target completion within original retirement budget'}
    if($DocumentTransfer){$completedBytes=$job.CompletedOutput();$result.nativeCompleted=$true}else{$result.nativeCanceled=$true}
}catch{$result.error=$_.Exception.ToString()}
finally{
    $env:PATH=$oldPath
    if($job -and !$job.Disposed){
        try{
            if($job.CreationReturned -and !$job.Status.Complete){
                if(!$retirement){$retirement=[Diagnostics.Stopwatch]::StartNew()}
                if(!$job.CancellationReturned -and $retirement.ElapsedMilliseconds -lt 5000){$job.Cancel()}
                $null=$job.PumpUntilTerminal([int][Math]::Max(0,5000-$retirement.ElapsedMilliseconds))
            }
            $result.finalStatus=$job.Status.Terminal;$result.statusHistory=$job.Status.History
            if($job.CanDispose){$job.Dispose();$null=$global:LeaNativePackageLeases.Remove($job)}
        }catch{$result.retirementError=$_.Exception.ToString()}
    }
    if($retirement){$result.nativeRetirementTotalElapsedMs=$retirement.ElapsedMilliseconds;$result.retirementOverBudget=$retirement.ElapsedMilliseconds -gt 5000}
    if($completedBytes){
        try{
            $outputPath=Join-Path $out 'completed-output.xps';[IO.File]::WriteAllBytes($outputPath,$completedBytes)
            $result.completedOutputSha256=(Get-FileHash $outputPath).Hash
            Add-Type -AssemblyName System.IO.Compression
            $zip=[IO.Compression.ZipFile]::OpenRead($outputPath)
            try{
                $pageParts=@($zip.Entries|Where-Object FullName -Match '\.fpage$')
                $result.outputPageParts=$pageParts.Count
                if($pageParts.Count -ne 3 -or $result.writtenPages -ne 3){throw 'Expected three output pages'}
                $result.outputReadback=$true
            }finally{$zip.Dispose()}
        }catch{$result.outputError=$_.Exception.ToString()}
    }
    if($result.Contains('defaultPrintersBefore')){
        $result.defaultPrintersAfter=@(Get-CimInstance Win32_Printer -Filter 'Default=True'|Select-Object -ExpandProperty Name|Sort-Object)
        $result.defaultPrinterMutated=($result.defaultPrintersBefore|ConvertTo-Json -Compress) -cne ($result.defaultPrintersAfter|ConvertTo-Json -Compress)
    }
    if($result.Contains('currentUserPrinterBefore')){
        try{
            $result.currentUserPrinterAfter=[Lea.PrintProbe.PrinterSafety]::Capture()
            $result.currentUserPrinterChanged=![Lea.PrintProbe.PrinterSafety]::Unchanged($result.currentUserPrinterBefore,$result.currentUserPrinterAfter)
        }catch{$result.printerReadbackError=$_.Exception.ToString()}
    }
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.unexplainedNativeChildren=@(Get-CimInstance Win32_Process|Where-Object ParentProcessId -eq $PID|Select-Object ProcessId,Name,CreationDate)
    if($result.unexplainedNativeChildren.Count){$global:LeaNativePackageUnverifiedChild=$result.unexplainedNativeChildren}
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or $global:LeaNativePackageLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue) -or [bool](Get-Variable LeaNativePackageUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $nativeSuccess=if($DocumentTransfer){$result.nativeCompleted -and $result.outputReadback -and !$result.outputError}else{$result.nativeCanceled}
    $result.passed=$nativeSuccess -and !$result.error -and !$result.retirementError -and !$result.retirementOverBudget -and !$result.defaultPrinterMutated -and !$result.currentUserPrinterChanged -and !$result.printerReadbackError -and $result.inputsUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPackageInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,nativeCanceled,inputsUnchanged,ownerAttention,error,retirementError|ConvertTo-Json -Compress
}
