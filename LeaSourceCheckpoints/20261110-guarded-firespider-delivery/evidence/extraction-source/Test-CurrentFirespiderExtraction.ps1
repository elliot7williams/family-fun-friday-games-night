param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\Lea.Build' -Parent
$out=Join-Path $workspace 'continuity-evidence/guarded-sdk-package-extraction-v1'
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
$result=[ordered]@{passed=$false;completed=$false;runtime=$runtime;runs=$runs;inputs=$inputs;before=@();scope='Fresh guarded SDK ZIP extraction settings/navigation/actual EXE smokes on NTFS; not all 15 extracted workflows, clean-PC, reliability or release qualification';fullReleaseQualified=$false}
$null=New-Item -ItemType Directory -Path $out
try{
    Hold $PSCommandPath
    Hold (Join-Path 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\Lea.Build' 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\Lea.Build' 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\Lea.Build' 'OwnedTestProcess.cs')
    . (Join-Path 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\Lea.Build' 'Invoke-OwnedTestRun.ps1')
    $base='I:\LeaBuildStorage\guarded-sdk-host-package-20261110-v1'
    $proofPath=Join-Path $base 'result.json'
    Hold $proofPath 'AED08C6336CBF88745904F3818F464E9CD45C5B5936E946BB1ADEEAD69D52454'
    $proof=Get-Content $proofPath -Raw|ConvertFrom-Json
    $archive=Join-Path $base 'Firespider-Lea-guarded-development-20261110.zip'
    Hold $archive '708A70D7DDE0D4CF49EF5D1976C2CF261357D136275C0FA91D26B44532C6322A'
    if(!$proof.verified -or $proof.members.Count -ne 402 -or $proof.entries -ne 402){throw 'ZIP proof incomplete'}
    if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS'){throw 'Native NTFS required'}
    $bytes=($proof.members|Measure-Object bytes -Sum).Sum
    if((Get-PSDrive C).Free -lt (600MB+$bytes)){throw 'Extraction plus reserve required'}
    $names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne 402){throw 'ZIP inventory differs'}
        foreach($entry in $zip.Entries){
            $name=$entry.FullName
            if([IO.Path]::IsPathRooted($name) -or $name.Contains(':') -or $name.Contains('\') -or $name.Split('/') -contains '..' -or $name.Split('/') -contains '' -or !$names.Add($name)){throw 'Unsafe ZIP member'}
            $row=@($proof.members|Where-Object path -CEQ $name)
            if($row.Count -ne 1 -or $entry.Length -ne $row[0].bytes){throw 'ZIP member inventory differs'}
            $s=$entry.Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $row[0].sha256){throw 'ZIP member hash differs'}}finally{$s.Dispose()}
        }
    }finally{$zip.Dispose()}
    [IO.Compression.ZipFile]::ExtractToDirectory($archive,$runtime)
    if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 402){throw 'Extracted inventory differs'}
    foreach($row in $proof.members){Hold (Join-Path $runtime $row.path) $row.sha256}
    $manifest=Get-Content (Join-Path $runtime 'lea-development-manifest.json') -Raw|ConvertFrom-Json
    if(!$manifest.developmentOnly -or $manifest.sourceIncluded -or $manifest.runtimeVersion -cne '8.0.33' -or $manifest.rollForward -cne 'Disable' -or $manifest.hostSha256 -cne '1E390343125011CAA3F78582E91FAFC036E07A31F7E81B4B727ED8503FC39606' -or $manifest.files.Count -ne 400){throw 'Extracted package metadata differs'}
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    $boot=Join-Path $runtime 'Lea.Firespider.Boot.exe'
    $run=Run 'settings-path' $boot @('--settings-path') 30
    if(!$run.passed){throw 'Extracted settings lookup failed'}
    $run=Run 'navigation' $boot @(('"'+$runtime+'"'),'navigation') 90
    if(!$run.passed -or @(Get-Content $run.stdout|Where-Object{$_.StartsWith('PASS Lea boot: navigation;')}).Count -ne 1){throw 'Extracted navigation failed'}
    $startup=Join-Path $workspace 'continuity-evidence/small-engine-v1/source/Lea.Build/Test-FirespiderStartup.ps1'
    $startupHash='1042F5CB68AB16A1C66EFB8524B2C5FD6C1C5098A74F088161060692BD629414'
    Hold $startup $startupHash
    $launcher=Join-Path 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\Lea.Build' 'Invoke-StartupBody.ps1';Hold $launcher 'BD2DB16A23A8C41B51BACC78AB9E6501928A44BDE72634CA79290B7EFAD68548'
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
    $result.decoderPolicyAccepted=$result.before.Count -eq 402 -and !$result.aclChanges.Count
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and $runs.Count -eq 3 -and @($runs|Where-Object{!$_.passed}).Count -eq 0 -and !$result.error -and $result.inputsUnchanged -and !$result.byteChanges.Count -and $result.decoderPolicyAccepted -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPageColorExtractionInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,completed,inputsUnchanged,decoderPolicyAccepted,ownerAttention,error|ConvertTo-Json -Compress
}
