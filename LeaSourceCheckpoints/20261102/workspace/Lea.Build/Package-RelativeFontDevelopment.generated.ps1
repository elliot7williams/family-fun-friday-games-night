$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$receipt=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-relative-font-owned-v1/result.json'
$expected='D9EEB35C3B9AB747640505EF07CBD03BB6D27FBD491F66F253E44F76D3D72A3B'
$archive='I:\LeaBuildStorage\Firespider-Lea-relative-font-development-20261031.zip'
$out=Join-Path $workspace 'continuity-evidence/relative-font-development-package-v1'
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
        @{name='print-command-source-20261028.zip';sha256='7B56572A2318AE3B491320DADAF9EC3AB3B58D57EE90207BCB97B5E4314BCB0F'},
        @{name='runtime-8033-matched-source-20261027.zip';sha256='84179CBDF6C3674E43C886FCF72B5C048056425B2D029E2CE3DBBE4CD719AEB9'}
    )
    foreach($reference in $references){$reference.path=if($reference.name -ceq 'runtime-8033-matched-source-20261027.zip'){'L:\Lea-source-backup-archive-20261029-uia\runtime-8033-matched-source-20261027.zip'}else{Join-Path $workspace ('continuity-evidence/'+$reference.name)};$null=Hold $reference.path $reference.sha256}
    $names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $proof.before){
        $relative=[IO.Path]::GetRelativePath($runtime,$file.path).Replace('\','/')
        if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..' -or !$names.Add($relative) -or $relative -in @('LEA-TEST-BUILD.txt','lea-test-build-manifest.json')){throw 'Unsafe entry'}
        $s=Hold $file.path $file.sha256
        $members.Add(@{entry=$relative;sha256=$file.sha256;bytes=$s.Length;stream=$s})
    }
    $identities=@{
        'Firespider Internet 19.exe'='E7F49DB9D59A6C73D92A3B40CA2101C61005C8A5B1BDB583D24946D6B8A5EF67'
        'Lea.Win.dll'='CA51FC818B119529EA8F7104D0753C5779070A7CCF0AD157675A564C4755C584'
        'Lea.Print.dll'='0C16B3D0113A91F83274F37D48C4D7B5F9BAC8DC1E292FBBBF318BF908711CFB'
        'Lea.Engine.dll'='4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA'
        'ScriptWorker/Lea.Script.Worker.runtimeconfig.json'='6346264D0ED421CB89893F36E6760048A72CCCEDC5A4B8BDB26B6048D6F766F6'
        'ScriptWorker/Lea.Script.Worker.dll'='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
        'ScriptWorker/Jint.dll'='67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'
    }
    foreach($name in $identities.Keys){
        $found=@($members|Where-Object entry -ceq $name)
        if($found.Count -ne 1 -or $found[0].sha256 -cne $identities[$name]){throw 'Product component identity differs'}
    }
    $note=@'
Firespider + Lea Relative-font development build, 2026-10-31

Extract the entire ZIP into a new folder and run Firespider Internet 19.exe.
Keep all DLLs and ScriptWorker together. Requires Windows .NET Framework4.8 and
exact .NET Runtime8.0.33 x64. This ZIP installs neither runtime, changes no default
browser and does not replace your installed browser or project.

Print click and the existing enabled-hover action now use Lea's native owned
printer-selection path instead of casting Lea tabs to the old WebBrowser type.
Cancel, shared print/preview reentry, accepted changed-tab rejection, busy and
unknown-tab guards are tested. Printer preparation converts/validates the selected
driver ticket and queries matching paper/imageable bounds. The driver controls
orientation; Lea defaults remain .75in margins,100% scale, header/footer. Local
filesystem paths are omitted from headers. Unresolved images are rejected.

IMPORTANT: successful printer-job submission and physical output have NOT been
verified. Do not treat this as a printing-ready release. No test job was sent to
the default physical printer. Post-attempt errors warn that queue status may be
uncertain; check the queue before manually retrying. Printer settings are remembered
only within this browser instance; system defaults are not changed.

Page Setup, silent Print, PDF-via-print and printer-friendly commands still need
migration and may fail on Lea tabs. Ordinary document selection, configurable
preview options, full CSS and image fidelity remain unfinished. Known heading/
details layout gaps persist. Remote HTTP/HTTPS/UNC scripting remains OFF.

This exact E7F49/core4DA8/viewCA51/print0C16/normal worker60FF/Jint67F3 pairing
passed its own15-stage current-machine host campaign with original deadlines and
owned-process checks. Zero runtime byte or ACL changes were observed under existing unchanged
policy, not security certified. Separate focused checks cover actual Print cancel/
guard paths and preview lifetime. Cold-start reliability, broad site compatibility,
confinement/lifecycle/recovery, clean-PC delivery and complete Visual Studio SDK
remain open. No speed improvement or final completion percentage is claimed.

The Boot EXE is diagnostic; ordinary use starts Firespider Internet 19.exe.
Runtime-only development ZIP, not a source SDK or off-device backup. Matching local
source checkpoints are pinned in the manifest. ZIP readback and fresh-extraction
smokes have separate receipts; extraction does not repeat all15 campaign stages.
'@
    $currentSource='I:\LeaBuildStorage\source-20261031-v14\current-source.zip'
    $currentSourceHash='87F8AEFDD28EBF73BF2254B9D5309C9C095BFD4F94DC0EC546C7D6E6C0C21F59'
    $null=Hold $currentSource $currentSourceHash
    $manifest=@{currentSourceCheckpoint=@{path=$currentSource;sha256=$currentSourceHash;githubUploaded=$false};developmentOnly=$true;runtimeVersion='8.0.33';hostReceiptSha256=$expected;hostSha256=$identities['Firespider Internet 19.exe'];sourceCheckpoints=$references;sourceIncluded=$false;fullReleaseQualified=$false;files=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256;bytes=$_.bytes}})}
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
