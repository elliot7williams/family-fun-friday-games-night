param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/deterministic-host-print-v2'
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
$result=[ordered]@{passed=$false;completed=$false;runtime=$runtime;runs=$runs;inputs=$inputs;before=@();scope='Exact current host print-command/preview with pinned preserved test harness; native dialog cancellation only, no queue submission; not physical output, clean-PC, fidelity or release proof';fullReleaseQualified=$false}
$null=New-Item -ItemType Directory -Path $out
try{
    Hold $PSCommandPath
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $manifestPath='I:\LeaBuildStorage\deterministic-firespider-runtime-20261102-v1\runtime-manifest.json'
    Hold $manifestPath '72D5699F0A392633EF9B967BCDE5E6AB7961FC8405C5F5CB76DE15E342BA2C17'
    $manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
    if(!$manifest.verified -or $manifest.files.Count -ne 400 -or $manifest.hostSha256 -cne 'B1B290CA5E90B0F5DD2460BFA3A90DCB81EF8F0ADF5DA96599D437B36B3F416E'){throw 'Current runtime manifest differs'}
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
    $proofPath=Join-Path $workspace 'continuity-evidence/relative-font-host-v1/result.json'
    Hold $proofPath '452492446FCAE2FD9F2DFCC5A6185ED4F05B977B89FADD584A2B6DC83150A6F6'
    $proof=Get-Content $proofPath -Raw|ConvertFrom-Json
    $probe=@($proof.before|Where-Object {$_.path.EndsWith('\Lea.Firespider.Boot.exe',[StringComparison]::OrdinalIgnoreCase)})
    if($probe.Count -ne 1 -or $probe[0].sha256 -cne '84985AC08FD5A7DC018CF6AD75006633734298153D327D53834C6B37DF30839B'){throw 'Print test binary provenance differs'}
    Hold $probe[0].path $probe[0].sha256
    $boot=Join-Path $runtime 'Lea.Firespider.PrintProbe.exe'
    Copy-Item -LiteralPath $probe[0].path -Destination $boot
    Hold $boot $probe[0].sha256
    foreach($name in @('print-styled-document.html','print-preview-image.html')){
        $fixture=@($proof.before|Where-Object {$_.path.EndsWith(('\test-pages\'+$name),[StringComparison]::OrdinalIgnoreCase)})
        if($fixture.Count -ne 1){throw 'Print fixture provenance incomplete'}
        Hold $fixture[0].path $fixture[0].sha256
        $fixtureTo=Join-Path $runtime ('test-pages/'+$name)
        if(Test-Path $fixtureTo){throw 'Do not overwrite a runtime fixture'}
        Copy-Item -LiteralPath $fixture[0].path -Destination $fixtureTo
        Hold $fixtureTo $fixture[0].sha256
    }
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    if($result.before.Count -ne 403){throw 'Print runtime inventory differs'}
    $run=Run 'print-command-preview' $boot @(('"'+$runtime+'"'),'print-command') 90
    if(!$run.passed -or @(Get-Content $run.stdout|Where-Object {$_.StartsWith('PASS Lea browser print command:')}).Count -ne 1){throw 'Current host print command/preview failed'}
    $result.completed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.byteChanges=@($result.before|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256})
    $result.aclChanges=@($result.before|Where-Object{(Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.decoderPolicyAccepted=$result.before.Count -eq 403 -and !$result.aclChanges.Count
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and $runs.Count -eq 1 -and @($runs|Where-Object{!$_.passed}).Count -eq 0 -and !$result.error -and $result.inputsUnchanged -and !$result.byteChanges.Count -and $result.decoderPolicyAccepted -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPageColorExtractionInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,completed,inputsUnchanged,decoderPolicyAccepted,ownerAttention,error|ConvertTo-Json -Compress
}
