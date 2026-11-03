$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$receipt=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-print-preview-owned-v1/result.json'
$expected='B721DF1664B22445A09DA73AD2B5F06DD449D8CFD58EA927A547E140AA7AA4DA'
$archive=Join-Path $workspace 'dist/LeaExes/Firespider-Lea-preview-development-20261027.zip'
$out=Join-Path $workspace 'continuity-evidence/print-preview-development-package-v1'
if((Test-Path $archive) -or (Test-Path $out)){throw 'Preserve existing package'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Storage reserve required'}
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Input differs: '+$Path)}
    $s.Position=0;return $s
}
try{
    $null=Hold $receipt $expected
    $proof=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.completed -or !$proof.hostStagesPassed -or !$proof.inputsUnchanged -or $proof.byteChanges.Count -or !$proof.decoderPolicyAccepted -or $proof.ownerAttention -or $proof.runs.Count -ne 15 -or $proof.before.Count -ne 399){throw 'Host campaign incomplete'}
    if(@($proof.runs|Where-Object{!$_.passed -or $_.timedOut -or $_.killRequested -or !$_.originalHandleCaptured -or !$_.exitObserved -or !$_.childRetirementIndependentlyAudited -or $_.activeAfterRetirement -ne 0 -or !$_.outputDrained}).Count){throw 'Owned campaign gates incomplete'}
    $runtime=[IO.Path]::GetFullPath($proof.candidate)
    if(!$runtime.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Runtime outside workspace'}
    $ancestor=Get-Item $runtime
    while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor'};$ancestor=$ancestor.Parent}
    $items=@(Get-ChildItem $runtime -Recurse -Force)
    if(@($items|Where-Object{$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count -or @($items|Where-Object{!$_.PSIsContainer}).Count -ne 399){throw 'Runtime inventory differs'}
    $references=@(
        @{name='print-preview-host-source-20261027.zip';sha256='3BE58454C829D6A3D8E17FE5B762206E47F030F0501C3206E0EDFA6DA3488524'},
        @{name='runtime-8033-matched-source-20261027.zip';sha256='84179CBDF6C3674E43C886FCF72B5C048056425B2D029E2CE3DBBE4CD719AEB9'}
    )
    foreach($reference in $references){$null=Hold (Join-Path $workspace ('continuity-evidence/'+$reference.name)) $reference.sha256}
    $names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $proof.before){
        $relative=[IO.Path]::GetRelativePath($runtime,$file.path).Replace('\','/')
        if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..' -or !$names.Add($relative) -or $relative -in @('LEA-TEST-BUILD.txt','lea-test-build-manifest.json')){throw 'Unsafe entry'}
        $s=Hold $file.path $file.sha256
        $members.Add(@{entry=$relative;sha256=$file.sha256;bytes=$s.Length;stream=$s})
    }
    $identities=@{
        'Firespider Internet 19.exe'='F5EBF8F4593FACD4CA33083DEF1B854951DC21232674EAB6C9F3DC0361DD8FBE'
        'Lea.Win.dll'='568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8'
        'Lea.Print.dll'='FC867957DF7CD7029BF15455C5163027755A79EA3CAECF5EF55963A6F3FE8A6A'
        'Lea.Engine.dll'='A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
        'ScriptWorker/Lea.Script.Worker.runtimeconfig.json'='6346264D0ED421CB89893F36E6760048A72CCCEDC5A4B8BDB26B6048D6F766F6'
        'ScriptWorker/Lea.Script.Worker.dll'='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
        'ScriptWorker/Jint.dll'='67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'
    }
    foreach($name in $identities.Keys){
        $found=@($members|Where-Object entry -ceq $name)
        if($found.Count -ne 1 -or $found[0].sha256 -cne $identities[$name]){throw 'Product component identity differs'}
    }
    $note=@'
Firespider + Lea preview development build, 2026-10-27

Extract the entire ZIP into a new folder and run Firespider Internet 19.exe.
Keep all DLLs and ScriptWorker together. Requires Windows .NET Framework4.8 and
exact .NET Runtime8.0.33 x64. This ZIP installs neither runtime, changes no default
browser and does not replace your installed project.

The Print Preview command now uses Lea styled detached capture and a modal
preview. Default paper is portrait Letter, .75in margins,100% scale with header
and footer. Local filesystem paths are omitted from preview headers. Pages with
unresolved images are rejected rather than silently losing images. Ordinary
document selection, configurable preview options and full CSS fidelity remain
unfinished. Known heading/details layout gaps remain.

Physical Print, silent Print, PDF-via-print, Page Setup and printer-friendly
commands are not yet migrated and may fail on Lea tabs. This is NOT a printing-
ready or finished browser release. Remote HTTP/HTTPS/UNC scripting remainsOFF.
Earlier intermittent startup failures, broad website compatibility, confinement/
lifecycle/recovery, clean-PC installation and complete Visual Studio SDK remain
open. No security certification or speed improvement is claimed.

This exact F5/coreA85/view568/printFC/normal worker60FF/Jint67F3 pairing passed its
own15-stage current-machine host campaign. Separate focused checks cover the
preview route/modal lifetime/guards. Original deadlines/ownership were retained;
one metadata-only decoder grant was classified by existing unchanged policy.
The Boot EXE is diagnostic; ordinary use starts Firespider Internet 19.exe.

Runtime-only development ZIP, not the source SDK. Exact local source/recipe/
evidence checkpoints are pinned in the manifest; they are not off-device backups.
ZIP readback and fresh extraction tests are separate; their own receipts define
the verification boundary. Not all15 cases are repeated in an extraction smoke.
'@
    $manifest=@{developmentOnly=$true;runtimeVersion='8.0.33';hostReceiptSha256=$expected;hostSha256=$identities['Firespider Internet 19.exe'];sourceCheckpoints=$references;sourceIncluded=$false;fullReleaseQualified=$false;files=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256;bytes=$_.bytes}})}
    $generated=@{'LEA-TEST-BUILD.txt'=$note;'lea-test-build-manifest.json'=($manifest|ConvertTo-Json -Depth 6)}
    $null=New-Item -ItemType Directory -Path $out
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        foreach($name in $generated.Keys){$w=[IO.StreamWriter]::new($zip.CreateEntry($name).Open(),[Text.UTF8Encoding]::new($false));try{$w.Write($generated[$name])}finally{$w.Dispose()}}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne 401){throw 'ZIP inventory differs'}
        foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'ZIP member differs'}}finally{$s.Dispose()}}
        foreach($name in $generated.Keys){$r=[IO.StreamReader]::new($zip.GetEntry($name).Open());try{if($r.ReadToEnd() -cne $generated[$name]){throw 'Generated member differs'}}finally{$r.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;archive=$archive;sha256=(Get-FileHash $archive).Hash;bytes=(Get-Item $archive).Length;runtimeFiles=399;hostReceiptSha256=$expected;sourceIncluded=$false;extractionExecuted=$false;developmentOnly=$true}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json')
    Get-Content (Join-Path $out 'result.json')
}finally{foreach($s in $leases){$s.Dispose()}}
