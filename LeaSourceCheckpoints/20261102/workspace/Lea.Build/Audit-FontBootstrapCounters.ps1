param([ValidateRange(1,9)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-bootstrap-counters-v1'
$path=Join-Path $directory 'result.json';$output=Join-Path $directory 'counter-audit.json'
if($Version -gt 1){$output=Join-Path $directory ('counter-audit-v'+$Version+'.json')}
if(Test-Path $output){throw 'Preserve prior counter audit'}
if((Get-FileHash $path).Hash -cne '0A5F0A16D34A3F566EE47B02A317C05BF47452C865330A8DE54A600A1841F345'){throw 'Raw counter receipt differs'}
$result=Get-Content $path -Raw|ConvertFrom-Json
$buildPath=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-script-suite-v1/result.json'
if((Get-FileHash $buildPath).Hash -cne 'E87409ED36E23F7C968490DDF206AAEF2D9D990AC9FAED2E144C1712A72EEF7E'){throw 'Matched build proof differs'}
$build=Get-Content $buildPath -Raw|ConvertFrom-Json
$recipe=Join-Path (Split-Path $buildPath) 'tests.rsp'
$recipePins=@($build.inputs|Where-Object path -ceq $recipe|Select-Object -ExpandProperty sha256 -Unique)
if($recipePins.Count -ne 1 -or (Get-FileHash $recipe).Hash -cne $recipePins[0]){throw 'Prefix compile recipe provenance differs'}
if(!$result.diagnosticCaptured -or !$result.prefixVerified -or !$result.computedPassed -or $result.fullSuitePassed -or !$result.inputsUnchanged -or $result.ownerAttention -or $result.changedFiles.Count -or $result.sampleErrors.Count -or $result.sampleOverflow){throw 'Diagnostic scope/input/ownership differs'}
$process=@($result.runs|Where-Object name -ceq 'font-prefix')
if($process.Count -ne 1 -or $process[0].exitCode -ne 0 -or $process[0].timedOut -or $process[0].killRequested -or $process[0].activeAfterRetirement -ne 0 -or !$process[0].outputDrained){throw 'Original process retirement incomplete'}
$birth=([datetime]$process[0].birthUtc).ToFileTimeUtc()
$samples=@($result.samples)
if($samples.Count -lt 3){throw 'Insufficient samples'}
for($i=0;$i -lt $samples.Count;$i++){
    $sample=$samples[$i]
    if(!$sample.originalHandleBorrowed -or $sample.ownerCreationFileTime -ne $birth -or $sample.queryMs -lt 0 -or $sample.native.AvailablePhysicalBytes -gt $sample.native.TotalPhysicalBytes){throw 'Sample identity/value invalid'}
    if($i -gt 0){
        $previous=$samples[$i-1]
        if($sample.elapsedMs -le $previous.elapsedMs -or $sample.stderrBytesObserved -lt $previous.stderrBytesObserved){throw 'Sample order invalid'}
        foreach($field in @('CpuTicks','PageFaults','ReadBytes','WriteBytes')){if($sample.native.$field -lt $previous.native.$field){throw 'Native counter not monotonic'}}
    }
}
$stderr=Join-Path $directory 'font-prefix.stderr.txt';$text=[IO.File]::ReadAllText($stderr)
$matches=@([regex]::Matches($text,'(?m)^LEA_SUITE_PHASE \d+ 8 begin\r?$'))
if($matches.Count -ne 1 -or !($text.Contains('LEA_ALL_STARTUP_TRACE restored=true'))){throw 'Marker/trace evidence differs'}
$newline=$text.IndexOf("`n",$matches[0].Index)
if($newline -lt 0){throw 'Marker newline missing'}
$markerBytes=[Text.Encoding]::UTF8.GetByteCount($text.Substring(0,$newline+1))
$before=@($samples|Where-Object stderrBytesObserved -lt $markerBytes)
$after=@($samples|Where-Object stderrBytesObserved -ge $markerBytes)
if(!$before.Count -or !$after.Count){throw 'Marker delivery not bracketed'}
$first=$before[-1];$last=$samples[-1]
$flags=@($samples|ForEach-Object {$_.native.WorkingSetFlags}|Sort-Object -Unique)
$minimumAvailable=($samples|ForEach-Object {$_.native.AvailablePhysicalBytes}|Measure-Object -Minimum).Minimum
foreach($file in $result.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Diagnostic source/input changed'}}
$audit=@{audited=$true;sourceRecipeVerified=$true;sourceRecipeSha256=$recipePins[0];rawReceiptSha256=(Get-FileHash $path).Hash;runnerSha256=(Get-FileHash $PSCommandPath).Hash;stderrSha256=(Get-FileHash $stderr).Hash;samples=$samples.Count;markerByteOffset=$markerBytes;markerArrivalLowerMs=$first.elapsedMs;markerArrivalUpperMs=$after[0].elapsedMs;lastSampleMs=$last.elapsedMs;windowWallMs=$last.elapsedMs-$first.elapsedMs;windowCpuMs=($last.native.CpuTicks-$first.native.CpuTicks)/10000;windowPageFaults=$last.native.PageFaults-$first.native.PageFaults;windowLogicalReadBytes=$last.native.ReadBytes-$first.native.ReadBytes;windowLogicalWriteBytes=$last.native.WriteBytes-$first.native.WriteBytes;workingSetFlags=$flags;minimumAvailablePhysicalBytes=$minimumAvailable;maxQueryMs=($samples|Measure-Object queryMs -Maximum).Maximum;scope='External marker-delivery bracket through last sample; NOT exclusive Evaluate timing';causeProven=$false;fullSuitePassed=$false;limitsChanged=$false}
[IO.File]::WriteAllText($output,($audit|ConvertTo-Json -Depth 5))
if(!(Get-Content $output -Raw|ConvertFrom-Json).audited){throw 'Counter audit readback differs'}
$audit|Select-Object audited,samples,markerArrivalLowerMs,markerArrivalUpperMs,windowWallMs,windowCpuMs,windowPageFaults,windowLogicalReadBytes,workingSetFlags,minimumAvailablePhysicalBytes,causeProven|ConvertTo-Json
