param(
    [Parameter(Mandatory=$true)][string]$CandidateReceipt,
    [Parameter(Mandatory=$true)][string]$Out,
    [ValidateSet('clean','restored','locked','oversized','lazy-background','lazy-all')]
    [string[]]$Scenarios=@('clean','restored','locked','oversized','lazy-background','lazy-all')
)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..')).TrimEnd('\')
$output=[IO.Path]::GetFullPath($Out).TrimEnd('\')
if(-not$output.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)-or(Test-Path -LiteralPath $output)){throw 'A new workspace evidence folder is required.'}
$receipt=Get-Content -LiteralPath $CandidateReceipt -Raw|ConvertFrom-Json
$runtime=[IO.Path]::GetFullPath($receipt.candidate).TrimEnd('\')
if(-not$runtime.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)-or-not$receipt.originalInputsUnchanged){throw 'Verified workspace candidate required.'}
$existing=@([Diagnostics.Process]::GetProcessesByName('Firespider Internet 19'))
try{if($existing.Count){throw 'Existing browser left untouched; close it before testing.'}}finally{foreach($p in $existing){$p.Dispose()}}
$script=$workspace+'\continuity-evidence\small-engine-v1\source\Lea.Build\Test-FirespiderStartup.ps1'
$launcher=Join-Path $PSScriptRoot 'Invoke-StartupBody.ps1'
$sha=(Get-FileHash -LiteralPath $script -Algorithm SHA256).Hash
$launcherText=[IO.File]::ReadAllText($launcher)
$pins=[Collections.Generic.List[object]]::new()
$runs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{completed=$false;passed=$false;scope='Six actual-EXE startup scenarios only';runtime=$runtime;
    priorReceiptSha256=(Get-FileHash -LiteralPath $CandidateReceipt -Algorithm SHA256).Hash;
    launcherSha256=(Get-FileHash -LiteralPath $launcher -Algorithm SHA256).Hash;startupScriptSha256=$sha;
    cases=$runs;inputs=$pins;inputsUnchanged=$false;releaseCertified=$false;error=$null}
$result.scope='Actual-EXE startup scenarios: '+($Scenarios -join ', ')
$result.expectedCaseCount=$Scenarios.Count
if($Scenarios.Count-eq0-or@($Scenarios|Select-Object -Unique).Count-ne$Scenarios.Count){throw 'A nonempty distinct scenario list is required.'}
$null=New-Item -ItemType Directory -Path $output
try{
    foreach($file in $receipt.candidateBinaries){
        $path=[IO.Path]::GetFullPath((Join-Path $runtime $file.path))
        if(-not$path.StartsWith($runtime+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Binary escapes candidate.'}
        $walk=$path
        while($walk){if((Get-Item -LiteralPath $walk -Force).Attributes-band[IO.FileAttributes]::ReparsePoint){throw 'Linked candidate path refused.'};$walk=[IO.Path]::GetDirectoryName($walk)}
        if((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash-cne$file.sha256){throw 'Candidate binary changed.'}
        $pins.Add(@{path=$path;sha256=$file.sha256})
    }
    foreach($scenario in $Scenarios){
        if([IO.DriveInfo]::new('C:\').AvailableFreeSpace-lt512MB){throw 'Working reserve unavailable.'}
        $evidence=Join-Path $output ($scenario+'.json')
        $command="& {`n"+$launcherText+"`n} -TestScript '"+$script.Replace("'","''")+"' -ExpectedSha256 '"+$sha+"' -Root '"+$runtime.Replace("'","''")+"' -Scenario '"+$scenario+"' -Evidence '"+$output.Replace("'","''")+"'"
        $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))
        $p=Start-Process -FilePath (Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe') `
            -ArgumentList @('-NoProfile','-STA','-EncodedCommand',$encoded) -WorkingDirectory $runtime -WindowStyle Hidden `
            -PassThru -RedirectStandardOutput (Join-Path $output ($scenario+'.stdout.txt')) -RedirectStandardError (Join-Path $output ($scenario+'.stderr.txt'))
        $null=$p.Handle
        $row=@{scenario=$scenario;pid=$p.Id;birthUtc=$p.StartTime.ToUniversalTime().ToString('o');timedOut=$false;exitCode=$null;passed=$false;evidence=$evidence}
        $runs.Add($row)
        $row.timedOut=-not$p.WaitForExit(100000)
        if($row.timedOut){$p.Kill($true);$p.WaitForExit()}
        $p.Refresh();$row.exitCode=$p.ExitCode;$p.Dispose()
        if($row.timedOut){throw 'Timed-out original startup tree retired; cleanup coverage remains unproven.'}
        if(Test-Path -LiteralPath $evidence){
            $record=Get-Content -LiteralPath $evidence -Raw|ConvertFrom-Json
            $row.passed=($row.exitCode-eq0-and$record.Passed-eq$true-and-not$record.ForcedCleanup-and@($record.CleanupErrors).Count-eq0)
        }
    }
    $result.passed=($runs.Count-eq$Scenarios.Count-and@($runs|Where-Object{-not$_.passed}).Count-eq0)
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=$true
    foreach($pin in $pins){if((Get-FileHash -LiteralPath $pin.path -Algorithm SHA256).Hash-cne$pin.sha256){$result.inputsUnchanged=$false;$result.passed=$false}}
    $result.completed=$true
    $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $output 'result.json') -Encoding UTF8
}
Write-Output (Join-Path $output 'result.json')
if(-not$result.passed){throw 'Actual startup gates failed or incomplete; inspect retained evidence.'}
