param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/page-color-package-extraction-v1'
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
$result=[ordered]@{passed=$false;completed=$false;runtime=$runtime;runs=$runs;inputs=$inputs;before=@();scope='Fresh verified ZIP extraction settings/navigation/actual clean EXE smokes; current machine only, not all15 extracted, clean-PC or full release';fullReleaseQualified=$false}
$null=New-Item -ItemType Directory -Path $out
try{
    Hold $PSCommandPath
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $proofPath=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-page-color-owned-v1/result.json'
    Hold $proofPath '3829CAFC58C5DFFCE1F84CC1B4E3756757E25954E696FB9F5144663375A53449'
    $proof=Get-Content $proofPath -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.completed -or !$proof.inputsUnchanged -or $proof.ownerAttention -or $proof.before.Count -ne 398){throw 'Host campaign incomplete'}
    $archive=Join-Path $workspace 'dist/LeaExes/Firespider-Lea-page-color-development-20261026.zip'
    Hold $archive 'BF7C6C6602FC0F65E38952873D89211AFB7138629A64E06C9B4971FF01D70DCF'
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    $entries=[Collections.Generic.List[object]]::new()
    try{
        if($zip.Entries.Count -ne 400){throw 'ZIP inventory differs'}
        $names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach($entry in $zip.Entries){
            $name=$entry.FullName.Replace('/','\')
            if([IO.Path]::IsPathRooted($name) -or $name.Contains(':') -or $name.Split('\') -contains '..' -or !$names.Add($name)){throw 'Unsafe ZIP name'}
            $s=$entry.Open()
            try{$entries.Add(@{name=$name;sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));bytes=$entry.Length})}finally{$s.Dispose()}
        }
    }finally{$zip.Dispose()}
    [IO.Compression.ZipFile]::ExtractToDirectory($archive,$runtime)
    if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 400){throw 'Extracted inventory differs'}
    foreach($entry in $entries){Hold (Join-Path $runtime $entry.name) $entry.sha256}
    foreach($file in $proof.before){
        $relative=[IO.Path]::GetRelativePath($proof.candidate,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Host receipt path escape'}
        if((Get-FileHash (Join-Path $runtime $relative)).Hash -cne $file.sha256){throw 'Extracted runtime differs from campaign'}
    }
    $manifest=Get-Content (Join-Path $runtime 'lea-test-build-manifest.json') -Raw|ConvertFrom-Json
    if($manifest.hostReceiptSha256 -cne '3829CAFC58C5DFFCE1F84CC1B4E3756757E25954E696FB9F5144663375A53449' -or $manifest.hostSha256 -cne 'FBBD913CF0B0C654706844DAF078B01B1DFC4A06DF8687678296DE261B24E818' -or !$manifest.developmentOnly -or $manifest.sourceIncluded -or $manifest.files.Count -ne 398){throw 'Package manifest identity differs'}
    $policy=Join-Path $PSScriptRoot 'Test-DecoderAclDelta.ps1';Hold $policy '15F46DBC7F4D4AD5B816DDFA1FE7213D02CAE1EDE79844AFBD904F281299E71E';. $policy
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
    if($result.aclChanges.Count -eq 1 -and (Get-Command Test-DecoderAclDelta -ErrorAction SilentlyContinue)){
        $file=$result.aclChanges[0]
        $result.decoderPolicyAccepted=Test-DecoderAclDelta -Path $file.path -ExpectedDecoderPath (Join-Path $runtime 'Lea.Image.exe') -Before $file.sddl -After (Get-Acl $file.path).Sddl -ProfileName 'Lea.Image.Decoder'
    }
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and $runs.Count -eq 3 -and @($runs|Where-Object{!$_.passed}).Count -eq 0 -and !$result.error -and $result.inputsUnchanged -and !$result.byteChanges.Count -and $result.decoderPolicyAccepted -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPageColorExtractionInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,completed,inputsUnchanged,decoderPolicyAccepted,ownerAttention,error|ConvertTo-Json -Compress
}
