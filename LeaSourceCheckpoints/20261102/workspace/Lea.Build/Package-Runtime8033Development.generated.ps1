$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$receipt=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-runtime8033-owned-v1/result.json'
$expected='A1B1A96C9358525A7704BE283B434AD946D4906C47025E1E01D2C768BD0A12CA'
$archive=Join-Path $workspace 'dist/LeaExes/Firespider-Lea-runtime8033-development-20261027.zip'
$out=Join-Path $workspace 'continuity-evidence/runtime-8033-development-package-v1'
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
        @{name='runtime-8033-matched-source-20261027.zip';sha256='84179CBDF6C3674E43C886FCF72B5C048056425B2D029E2CE3DBBE4CD719AEB9'},
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
        'Lea.Win.dll'='568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8'
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
Firespider + Lea runtime8.0.33 development build, 2026-10-27

Extract the entire ZIP into a new folder and run Firespider Internet 19.exe.
Keep all DLLs and the ScriptWorker folder together. Windows .NET Framework4.8
and exact .NET Runtime8.0.33 x64 are required on the tested machine. This ZIP
installs neither runtime and changes neither default browser nor installed files.

Experimental pairing: coreA85/view568, normal worker60FF/Jint67F3, exact8.0.33.
The candidate passed its own15-stage current-machine campaign: settings lookup,
eight host cases and six actual-EXE startup scenarios. A separate matched full
scripting campaign completed3412 checks/all89 phases within its original deadline.
The existing decoder policy classified one metadata-only grant; no ACL/policy
changes were made to obtain a pass. Raw script ACL audit remainsfalse.

Not a finished browser or security-certified release. Earlier intermittent worker
startup failures, cold-start reliability and clean-PC installation remain open.
Remote HTTP/HTTPS/UNC scripting staysOFF. Broad web compatibility, lifecycle/
recovery, ordinary document selection and complete Visual Studio SDK remain open.
Print styles/capture/preview have experimental library tests, but browser print
commands and several tools still use explicit legacy routes and are unfinished.

The Boot EXE is a diagnostic tool; ordinary use starts Firespider Internet19.exe.
This runtime ZIP is not the complete source SDK. Matching source/recipes/evidence
are retained in the separately pinned local checkpoints in the manifest.
Those local checkpoints are not off-device backups. Package readback and fresh
extraction tests are distinct; see their own receipts for exact verification scope.
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
        if($zip.Entries.Count -ne 400){throw 'ZIP inventory differs'}
        foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'ZIP member differs'}}finally{$s.Dispose()}}
        foreach($name in $generated.Keys){$r=[IO.StreamReader]::new($zip.GetEntry($name).Open());try{if($r.ReadToEnd() -cne $generated[$name]){throw 'Generated member differs'}}finally{$r.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;archive=$archive;sha256=(Get-FileHash $archive).Hash;bytes=(Get-Item $archive).Length;runtimeFiles=398;hostReceiptSha256=$expected;sourceIncluded=$false;extractionExecuted=$false;developmentOnly=$true}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json')
    Get-Content (Join-Path $out 'result.json')
}finally{foreach($s in $leases){$s.Dispose()}}
