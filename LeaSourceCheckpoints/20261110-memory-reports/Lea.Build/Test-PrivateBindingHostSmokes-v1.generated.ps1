param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/private-binding-host-smokes-v1'
$runtime=Join-Path $out 'runtime'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path $out){throw 'Preserve extraction attempt'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if(@(Get-Process 'Firespider*','Lea.Firespider.Boot','Lea.Script.Worker','Lea.Script.Tests','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No overlapping browser/tests'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if(!$Hash){$Hash=(Get-FileHash $Path).Hash}
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
function Run([string]$Name,[string]$Application,[string[]]$Arguments,[int]$Seconds){
    Invoke-OwnedTestRun -Name $Name -Application $Application -Arguments $Arguments -WorkingDirectory $runtime -OutputDirectory $out -Seconds $Seconds -Records $runs
}
$result=[ordered]@{passed=$false;completed=$false;runtime=$runtime;runs=$runs;inputs=$inputs;before=@();scope='Exact private-binding host settings/navigation/actual clean EXE smokes on fresh NTFS; not full host campaign, clean-PC or release qualification';fullReleaseQualified=$false}
$null=New-Item -ItemType Directory -Path $out
try{
    Hold $PSCommandPath
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $manifestPath='I:\LeaBuildStorage\private-binding-firespider-runtime-20261103-v1\runtime-manifest.json'
    Hold $manifestPath '694C89BB4FDF24936EF5424CAFC09C57BCDB50ED7EC21483D0DBDBD3A3E122FA'
    $manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
    if(!$manifest.verified -or $manifest.files.Count -ne 400 -or $manifest.hostSha256 -cne 'B1A707A23CEB1DEE7FBDE13102BE24FAE7369FC21FC43F0688D35522197B1F85'){throw 'Current runtime manifest differs'}
    if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS'){throw 'Native NTFS required'}
    if((Get-PSDrive C).Free -lt (600MB+$manifest.bytes)){throw 'Staging plus reserve required'}
    $source=Join-Path (Split-Path $manifestPath) 'runtime'
    foreach($file in $manifest.files){
        $name=$file.path
        if([IO.Path]::IsPathRooted($name) -or $name.Contains(':') -or $name.Contains('\') -or $name.Split('/') -contains '..'){throw 'Unsafe runtime member'}
        $from=Join-Path $source $name
        $item=Get-Item -LiteralPath $from
        while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse source refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
        Hold $from $file.sha256
        $to=Join-Path $runtime $name
        New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
        Copy-Item -LiteralPath $from -Destination $to
        Hold $to $file.sha256
    }
    if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 400){throw 'Staged inventory differs'}
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    $boot=Join-Path $runtime 'Lea.Firespider.Boot.exe'
    $run=Run 'settings-path' $boot @('--settings-path') 30
    if(!$run.passed){throw 'Extracted settings lookup failed'}
    $run=Run 'navigation' $boot @(('"'+$runtime+'"'),'navigation') 90
    if(!$run.passed -or @(Get-Content $run.stdout|Where-Object{$_.StartsWith('PASS Lea boot: navigation;')}).Count -ne 1){throw 'Extracted navigation failed'}
    $startup=Join-Path $workspace 'continuity-evidence/small-engine-v1/source/Lea.Build/Test-FirespiderStartup.ps1'
    $startupHash='1042F5CB68AB16A1C66EFB8524B2C5FD6C1C5098A74F088161060692BD629414'
    Hold $startup $startupHash
    $launcher=Join-Path $PSScriptRoot 'Invoke-StartupBody.ps1';Hold $launcher 'BD2DB16A23A8C41B51BACC78AB9E6501928A44BDE72634CA79290B7EFAD68548'
    $nl=[Environment]::NewLine
    $command='& {'+$nl+[IO.File]::ReadAllText($launcher)+$nl+"} -TestScript '"+$startup.Replace("'","''")+"' -ExpectedSha256 '"+$startupHash+"' -Root '"+$runtime.Replace("'","''")+"' -Scenario 'clean' -Evidence '"+$out.Replace("'","''")+"'"
    $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))
    $run=Run 'exe-clean' (Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/powershell.exe') @('-NoProfile','-STA','-EncodedCommand',$encoded) 100
    if(!$run.passed){throw 'Extracted actual EXE startup failed'}
    $result.completed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.byteChanges=@($result.before|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256})
    $result.aclChanges=@($result.before|Where-Object{(Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.decoderPolicyAccepted=$result.before.Count -eq 400 -and !$result.aclChanges.Count
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and $runs.Count -eq 3 -and @($runs|Where-Object{!$_.passed}).Count -eq 0 -and !$result.error -and $result.inputsUnchanged -and !$result.byteChanges.Count -and $result.decoderPolicyAccepted -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPageColorExtractionInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,completed,inputsUnchanged,decoderPolicyAccepted,ownerAttention,error|ConvertTo-Json -Compress
}
