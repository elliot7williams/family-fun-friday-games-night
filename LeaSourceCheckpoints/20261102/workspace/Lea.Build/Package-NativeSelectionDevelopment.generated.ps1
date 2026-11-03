$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$receipt=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-native-selection-owned-v1/result.json'
$expected='C11B0A7FB0169D45A4611B22BECEFB497919F09149AB04905324F8C506BDE324'
$archive=Join-Path $workspace 'dist/LeaExes/Firespider-Lea-native-selection-development-20261027.zip'
$out=Join-Path $workspace 'continuity-evidence/native-selection-development-package-v1'
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
    if(!$proof.passed -or !$proof.completed -or !$proof.hostStagesPassed -or !$proof.inputsUnchanged -or $proof.byteChanges.Count -or !$proof.decoderPolicyAccepted -or $proof.ownerAttention -or $proof.runs.Count -ne 15 -or $proof.before.Count -ne 398){throw 'Host campaign incomplete'}
    if(@($proof.runs|Where-Object{!$_.passed -or $_.timedOut -or $_.killRequested -or !$_.originalHandleCaptured -or !$_.exitObserved -or !$_.childRetirementIndependentlyAudited -or $_.activeAfterRetirement -ne 0 -or !$_.outputDrained}).Count){throw 'Owned campaign gates incomplete'}
    $runtime=[IO.Path]::GetFullPath($proof.candidate)
    if(!$runtime.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Runtime outside workspace'}
    $ancestor=Get-Item $runtime
    while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor'};$ancestor=$ancestor.Parent}
    $items=@(Get-ChildItem $runtime -Recurse -Force)
    if(@($items|Where-Object{$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count -or @($items|Where-Object{!$_.PSIsContainer}).Count -ne 398){throw 'Runtime inventory differs'}
    $references=@(
        @{name='native-selection-source-20261027.zip';sha256='3B3A74685DC11937AA63BED297D5E2843BF5DE7511436CB9CCC645BAE082FA99'},
        @{name='native-selection-host-source-20261027.zip';sha256='D4F8FB82153418B2584C27E6D6A50C9D99499119B28DE7A64C755DACD8D0FC80'}
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
        'Firespider Internet 19.exe'='FBBD913CF0B0C654706844DAF078B01B1DFC4A06DF8687678296DE261B24E818'
        'Lea.Win.dll'='2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
        'ScriptWorker/Lea.Script.Worker.dll'='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
        'ScriptWorker/Jint.dll'='67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'
    }
    foreach($name in $identities.Keys){
        $found=@($members|Where-Object entry -ceq $name)
        if($found.Count -ne 1 -or $found[0].sha256 -cne $identities[$name]){throw 'Product component identity differs'}
    }
    $note=@'
Firespider + Lea native-selection development build, 2026-10-27

Extract the ENTIRE ZIP into a new folder and run Firespider Internet 19.exe.
Do not copy only the EXE or extract over an older browser. Keep its DLLs and
ScriptWorker folder together. Windows .NET Framework 4.8 and exact .NET Runtime
8.0.32 x64 are required on the tested machine. This ZIP installs neither runtime,
does not register a default browser and does not replace your existing project.

The exact host passed its own 15-stage campaign, including six actual-EXE startup
scenarios. Lea page colors passed metadata/body/CSS fixtures and cycle, traversal
limit, background-tab and command checks. This is current-machine functional
evidence, not clean-PC installation, cold-start reliability or all-site support.

Not a finished browser or security-certified release. Earlier intermittent worker
startup failures remain unresolved. One owned full scripting run passed 3412
checks on this view/core/worker pairing, but that does not resolve those failures.
Remote HTTP/HTTPS/UNC scripting remains OFF. Printer-friendly pages and other
tools still need migration from the explicit legacy engine routes. Web platform,
security/lifecycle/recovery, complete SDK and broader compatibility remain open.

The normal worker is included, not the timing-only diagnostic variant. The Boot
EXE is a diagnostic tool; ordinary use starts Firespider Internet 19.exe.
This is a runtime ZIP, not the complete source SDK. Matching source, recipes and
retained failure evidence are in the separately hash-pinned local checkpoints
listed in lea-test-build-manifest.json. They are not an off-device backup.
Sx() reads permitted native-field selection; ordinary page-text selection and
printing remain unfinished. This new view also passed a matched full native run.
ZIP contents are verified; fresh extraction testing is recorded separately.
'@
    $manifest=@{developmentOnly=$true;hostReceiptSha256=$expected;hostSha256=$identities['Firespider Internet 19.exe'];sourceCheckpoints=$references;sourceIncluded=$false;fullReleaseQualified=$false;files=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256;bytes=$_.bytes}})}
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
        if($zip.Entries.Count -ne 400){throw 'ZIP inventory differs'}
        foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'ZIP member differs'}}finally{$s.Dispose()}}
        foreach($name in $generated.Keys){$r=[IO.StreamReader]::new($zip.GetEntry($name).Open());try{if($r.ReadToEnd() -cne $generated[$name]){throw 'Generated member differs'}}finally{$r.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;archive=$archive;sha256=(Get-FileHash $archive).Hash;bytes=(Get-Item $archive).Length;runtimeFiles=398;hostReceiptSha256=$expected;sourceIncluded=$false;extractionExecuted=$false;developmentOnly=$true}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json')
    Get-Content (Join-Path $out 'result.json')
}finally{foreach($s in $leases){$s.Dispose()}}
