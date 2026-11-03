param(
    [Parameter(Mandatory=$true)][string]$NativeBuild,
    [Parameter(Mandatory=$true)][string]$Out
)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..')).TrimEnd('\')
$outRoot=[IO.Path]::GetFullPath($Out).TrimEnd('\')
if(-not$outRoot.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)-or(Test-Path -LiteralPath $outRoot)){throw 'A new workspace output directory is required.'}
$native=[IO.Path]::GetFullPath($NativeBuild).TrimEnd('\')
$buildPath=$workspace+'\continuity-evidence\bootstrap-numeric-stores-v1\builds-resume-v2\build-result.json'
$harnessReceipt=$workspace+'\continuity-evidence\small-engine-v1\harness-build-result.json'
$startupScript=$workspace+'\continuity-evidence\small-engine-v1\source\Lea.Build\Test-FirespiderStartup.ps1'
$bridge=Join-Path $PSScriptRoot 'Invoke-NativeCompiler.ps1'
$startupLauncher=Join-Path $PSScriptRoot 'Invoke-StartupBody.ps1'
$references='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1'
$inputs=[Collections.Generic.List[object]]::new()
$runs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{completed=$false;passed=$false;candidate=$outRoot+'\runtime';runs=$runs;
    inputFiles=$inputs;originalInputsUnchanged=$false;hostRecompiled=$false;
    nativeComponentsRebuiltPreviously=$true;fullReleaseCertified=$false;failure=$null}
function Space {
    if([IO.DriveInfo]::new('C:\').AvailableFreeSpace-lt512MB){throw 'Insufficient working reserve.'}
}
function Pin([string]$path,[string]$expected='') {
    $full=[IO.Path]::GetFullPath($path)
    if(-not$full.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Input outside workspace.'}
    $walk=$full
    while($walk){if((Get-Item -LiteralPath $walk -Force).Attributes-band[IO.FileAttributes]::ReparsePoint){throw 'Linked input refused.'};$walk=[IO.Path]::GetDirectoryName($walk)}
    $hash=(Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash
    if($expected-and$hash-cne$expected){throw ('Input identity differs: '+$full)}
    $inputs.Add(@{path=$full;sha256=$hash})
    return $hash
}
function CopyExact([string]$from,[string]$to,[string]$expected) {
    $null=Pin $from $expected
    $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($to)) -Force
    Copy-Item -LiteralPath $from -Destination $to
    if((Get-FileHash -LiteralPath $to -Algorithm SHA256).Hash-cne$expected){throw 'Candidate copy mismatch.'}
}
function OverlayWorkerEngine([object[]]$binaries,[string]$nativeRoot,[string]$runtimeRoot) {
    $engine=@($binaries|Where-Object{[IO.Path]::GetFileName($_.path)-ceq'Lea.Engine.dll'})
    if($engine.Count-ne1){throw 'Exactly one native engine binary is required.'}
    $row=$engine[0]
    $parent=Join-Path $runtimeRoot 'Lea.Engine.dll'
    $worker=Join-Path $runtimeRoot 'ScriptWorker\Lea.Engine.dll'
    if((Get-FileHash -LiteralPath $parent -Algorithm SHA256).Hash-cne$row.sha256){throw 'Parent engine overlay is not the selected native binary.'}
    CopyExact (Join-Path $nativeRoot $row.path) $worker $row.sha256
    if((Get-FileHash -LiteralPath $worker -Algorithm SHA256).Hash-cne$row.sha256){throw 'Worker engine differs from the selected parent engine.'}
    return @{sha256=$row.sha256;parent=$parent;worker=$worker;matching=$true}
}
function Run([string]$name,[string]$exe,[string[]]$arguments,[int]$seconds) {
    Space
    $stdout=Join-Path $outRoot ($name+'.stdout.txt');$stderr=Join-Path $outRoot ($name+'.stderr.txt')
    $clock=[Diagnostics.Stopwatch]::StartNew()
    $p=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory $result.candidate `
        -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    $null=$p.Handle
    $row=@{name=$name;pid=$p.Id;birthUtc=$p.StartTime.ToUniversalTime().ToString('o');exitCode=$null;
        timedOut=$false;stdout=$stdout;stderr=$stderr;passed=$false}
    $runs.Add($row)
    $row.timedOut=-not$p.WaitForExit($seconds*1000)
    if($row.timedOut){$p.Kill($true);$p.WaitForExit()}
    $p.Refresh();$row.exitCode=$p.ExitCode;$row.elapsedMs=$clock.ElapsedMilliseconds
    $row.passed=(-not$row.timedOut-and$row.exitCode-eq0)
    $p.Dispose()
    if($row.timedOut){throw ('Timed-out process tree retired; child isolation/profile cleanup is unproven: '+$name)}
    return $row
}
Space
$null=New-Item -ItemType Directory -Path $outRoot
try {
    $null=Pin $PSCommandPath
    $null=Pin $bridge
    $startupHash=Pin $startupScript
    $null=Pin $startupLauncher
    $null=Pin $buildPath 'A8BF14FE6903807E60B5E47FE10051F06440E864D33FAC7C8B89F7667A2F9ECF'
    $null=Pin $harnessReceipt '7D16D055C6138A86D92495D6B34E137FDEDDBED885596DBA9DEA17FD9AE3DF0F'
    $nativeReceipt=Join-Path $native 'result.json';$null=Pin $nativeReceipt
    $n=Get-Content -LiteralPath $nativeReceipt -Raw|ConvertFrom-Json
    if(-not$n.passed-or-not$n.originalInputsUnchanged){throw 'A passed native candidate is required.'}
    $existing=@([Diagnostics.Process]::GetProcessesByName('Firespider Internet 19'))
    try{if($existing.Count){throw 'An existing Firespider is open; leave it untouched and do not overlap candidate tests.'}}finally{foreach($p in $existing){$p.Dispose()}}
    $b=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
    if(-not$b.completed-or-not$b.protectedInputsAfterVerified-or$b.engineStartupMs-ne5000-or$b.engineExecutionMs-ne2000-or$b.engineCpuRate-ne2500-or$b.engineJobMemoryBytes-ne134217728){throw 'Numeric runtime/policies differ.'}
    $v=$b.variants[0]
    if($v.files.Count-ne397){throw 'Unexpected runtime inventory.'}
    foreach($file in $v.files){
        $relative=([string]$file.path).Replace('/','\')
        if([IO.Path]::IsPathRooted($relative)-or$relative.Split('\')-contains'..'){throw 'Unsafe runtime path.'}
        CopyExact (Join-Path $v.runtime $relative) (Join-Path $result.candidate $relative) $file.sha256
    }
    foreach($file in $n.binaries){
        $name=[IO.Path]::GetFileName($file.path)
        if($name-notin@('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe','Lea.Win.Test.exe','Lea.Tests.exe')){throw 'Unexpected native output.'}
        CopyExact (Join-Path $native $file.path) (Join-Path $result.candidate $name) $file.sha256
    }
    $result.enginePair=OverlayWorkerEngine $n.binaries $native $result.candidate
    $h=Get-Content -LiteralPath $harnessReceipt -Raw|ConvertFrom-Json
    CopyExact $h.harnessPath (Join-Path $result.candidate 'Lea.Firespider.Boot.exe') $h.harnessSha256
    $sourceRoot=Join-Path $native 'source'
    foreach($file in $n.sourceInputs){$null=Pin (Join-Path $sourceRoot $file.path) $file.sha256}
    $handshake=Join-Path $sourceRoot 'Lea.Tests\ImageHandshake.cs'
    $null=Pin $handshake
    $rsp=Join-Path $outRoot 'image-handshake.rsp'
    $options=@('/nologo','/nostdlib+','/optimize+','/target:exe',('/out:"'+$result.candidate+'\Lea.Image.Handshake.exe"'))
    foreach($name in @('mscorlib','System','System.Core','Accessibility','System.Drawing','System.Windows.Forms','UIAutomationClient','UIAutomationTypes','WindowsBase')){$options+=('/reference:"'+(Join-Path $references ($name+'.dll'))+'"')}
    $options+=('"'+$handshake+'"')
    $options|Set-Content -LiteralPath $rsp -Encoding UTF8
    $hash=(Get-FileHash -LiteralPath $rsp -Algorithm SHA256).Hash
    $compile=Run 'build-image-handshake' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$bridge+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash) 120
    if(-not$compile.passed){throw 'Image-handshake test helper failed to compile.'}
    # Prove this relocated harness has a distinct settings bucket before opening forms.
    $boot=Join-Path $result.candidate 'Lea.Firespider.Boot.exe'
    $probe=Run 'settings-path' $boot @('--settings-path') 30
    if(-not$probe.passed){throw 'Host settings-path probe failed.'}
    $lines=@(Get-Content -LiteralPath $probe.stdout)
    if(@($lines|Where-Object{$_-ceq('LEA_BOOT_ENTRY_PATH: '+$boot)}).Count-ne1-or@($lines|Where-Object{$_.StartsWith('LEA_BOOT_SETTINGS_PATH: ')}).Count-ne1){throw 'Host settings-path output differs.'}
    $test=Join-Path $result.candidate 'Lea.Tests.exe'
    $null=Run 'native-view-layout' (Join-Path $result.candidate 'Lea.Win.Test.exe') @('--smoke',('"'+$sourceRoot+'\LeaForge\test-pages\layout.html"'),('"'+$outRoot+'\native-view-layout.png"')) 60
    $null=Run 'engine-full' $test @(('"'+$sourceRoot+'"')) 300
    foreach($case in @('clean','lea','navigation','second','split','edit','style-start','script')){
        $row=Run ('host-'+$case) $boot @(('"'+$result.candidate+'"'),$case) 90
        if($row.passed){
            $footer=@(Get-Content -LiteralPath $row.stdout|Where-Object{$_.StartsWith('PASS Lea boot: '+$case+';')})
            $row.passed=($footer.Count-eq1)
        }
        if(-not$row.passed-and$case-eq'script'){$result.scriptingGateFailed=$true}
    }
    $windowsPowerShell=Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe'
    foreach($scenario in @('clean','restored','locked','oversized','lazy-background','lazy-all')){
        $evidence=$outRoot
        # Execute only the hash-checked local test body, without changing execution
        # policy or granting different rights to Windows PowerShell or the browser.
        $command="& {`n"+[IO.File]::ReadAllText($startupLauncher)+"`n} -TestScript '"+$startupScript.Replace("'","''")+"' -ExpectedSha256 '"+$startupHash+"' -Root '"+$result.candidate.Replace("'","''")+"' -Scenario '"+$scenario+"' -Evidence '"+$evidence.Replace("'","''")+"'"
        $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))
        $null=Run ('exe-'+$scenario) $windowsPowerShell @('-NoProfile','-STA','-EncodedCommand',$encoded) 100
    }
    $result.passed=(@($runs|Where-Object{-not$_.passed}).Count-eq0)
} catch {$result.failure=$_.Exception.ToString()}
finally {
    try {
        foreach($inputFile in $inputs){if((Get-FileHash -LiteralPath $inputFile.path -Algorithm SHA256).Hash-cne$inputFile.sha256){throw 'Original protected input changed.'}}
        $result.originalInputsUnchanged=$true
    } catch {$result.failure=$_.Exception.ToString();$result.passed=$false}
    $result.candidateBinaries=@(Get-ChildItem -LiteralPath $result.candidate -Recurse -File -ErrorAction SilentlyContinue|Where-Object{$_.Extension-in@('.exe','.dll')}|ForEach-Object{@{path=$_.FullName.Substring($result.candidate.Length+1);sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}})
    $result.completed=$true
    $result|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $outRoot 'result.json') -Encoding UTF8
}
Write-Output (Join-Path $outRoot 'result.json')
if(-not$result.passed){throw 'Candidate gates failed or incomplete; inspect retained result and logs. Not a release.'}
