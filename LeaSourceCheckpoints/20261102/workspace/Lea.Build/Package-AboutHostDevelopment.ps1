param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedHostReceipt,[switch]$LibraryHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$receipt=Join-Path $workspace $(if($LibraryHost){'continuity-evidence/temporal-startup-v1/host-library-current-native-v1/result.json'}else{'continuity-evidence/temporal-startup-v1/host-about-current-native-v1/result.json'})
if((Get-FileHash $receipt).Hash -cne $ExpectedHostReceipt){throw 'Host campaign receipt differs'}
$proof=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$proof.passed -or !$proof.hostStagesPassed -or !$proof.originalInputsUnchanged -or !$proof.candidateBytesUnchanged -or !$proof.decoderPolicyAccepted -or $proof.ownerAttention -or $proof.runs.Count -ne 15 -or @($proof.runs|Where-Object{!$_.passed -or $_.timedOut -or $_.killRequested}).Count){throw 'Host campaign incomplete'}
if($proof.aboutHostReceiptSha256 -cne '6E90FF84B17140992FC2D9B3EFDAA36DD438B94490E261804B15E5C4DB9825C1'){throw 'Expected exact About-fixed host'}
if($LibraryHost -and $proof.libraryHostReceiptSha256 -cne '3739C0B1979EC72622E8030CBB49FAF50824B912DB2C2B24C14DD76D53F38021'){throw 'Expected exact library-fixed host'}
$runtime=[IO.Path]::GetFullPath($proof.candidate)
if(!$runtime.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Runtime escapes workspace'}
$archive=Join-Path $workspace $(if($LibraryHost){'dist/LeaExes/Firespider-Lea-library-development-20261026.zip'}else{'dist/LeaExes/Firespider-Lea-development-20261026.zip'})
$out=Join-Path $workspace $(if($LibraryHost){'continuity-evidence/library-host-development-package-v1'}else{'continuity-evidence/about-host-development-package-v1'})
if((Test-Path $archive) -or (Test-Path $out)){throw 'Preserve prior package'}
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 512MB){throw 'Preserve storage reserve'}
$items=@(Get-ChildItem -LiteralPath $runtime -Recurse -Force)
if(@($items|Where-Object{$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Runtime reparse entry'}
$ancestor=Get-Item -LiteralPath $runtime
while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor'};$ancestor=$ancestor.Parent}
if(@($items|Where-Object{!$_.PSIsContainer}).Count -ne $proof.before.Count){throw 'Runtime inventory changed after campaign'}
$null=New-Item -ItemType Directory $out
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    $leases.Add([IO.File]::Open($receipt,'Open','Read','Read'))
    foreach($file in $proof.before){
        $relative=[IO.Path]::GetRelativePath($runtime,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..' -or $relative -in @('LEA-TEST-BUILD.txt','lea-test-build-manifest.json')){throw 'Package entry identity differs'}
        $s=[IO.File]::Open($file.path,'Open','Read','Read');$leases.Add($s)
        if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $file.sha256){throw 'Runtime bytes changed after campaign'}
        $s.Position=0;$members.Add(@{entry=$relative.Replace('\','/');sha256=$file.sha256;bytes=$s.Length;stream=$s})
    }
    $note=@'
Firespider + Lea development test build, 2026-10-26

Not a finished browser or a general website-compatibility release.
Extract the entire ZIP into a new folder. Keep all DLLs and ScriptWorker files
beside Firespider Internet 19.exe. Run that EXE, not just a copied single file.
The exact .NET Runtime 8.0.32 x64 must remain installed on this tested machine.
No installation, default-browser registration or existing-project replacement
is performed by this package. Do not extract over an older browser folder.

This exact EXE passed 15 private host checks, including six actual-EXE startup
scenarios, and separate four-route About-dialog regression checks. This is one
functional campaign, not a reliable cold-start certification. Full JavaScript
regression remains failed at an intermittent 5-second worker startup deadline.
Remote scripting and broad web-platform compatibility remain incomplete. Legacy
host tools remain under audit. Do not treat this as complete or security certified.

The timing-only diagnostic worker is NOT included. The normal tested worker and
interpreter are packaged unchanged. The diagnostic Boot EXE is a test tool;
ordinary use starts Firespider Internet 19.exe.

This ZIP contains matching runtime bytes, not the source distribution. Source,
recipes and failure evidence remain in the workspace's verified checkpoints.
ZIP member hashes were verified without claiming extracted-package qualification
or clean-machine installation tests.
'@
    $manifest=@{developmentOnly=$true;hostReceiptSha256=$ExpectedHostReceipt;aboutReceiptSha256=$proof.aboutHostReceiptSha256;knownFullScriptFailureSha256='23E8711EFA033D5F4E10F8FC4FF6313913460166F815CF998301CF04DFF71656';sourceCheckpoint=@{name='about-and-cleanup-20261026.zip';sha256='67405EB185F26B790C6A272F0785B5495D916E15C5B9A0E45B5A317F72292F95'};files=@($members|ForEach-Object{@{entry=$_.entry;bytes=$_.bytes;sha256=$_.sha256}})}
    if($LibraryHost){
        $note=@'
Firespider + Lea history/bookmark development test build, 2026-10-26

Not a finished browser or a general website-compatibility release.
Extract the entire ZIP into a new folder. Keep all DLLs and ScriptWorker files
beside Firespider Internet 19.exe, then run that EXE. Do not copy just the EXE
or extract over an older browser folder. Exact .NET Runtime 8.0.32 x64 is required
on the tested machine. No installation or default-browser registration is done.

This exact EXE passed 15 private host checks, including six actual-EXE startup
scenarios, plus a focused history/bookmark regression. Both lists reuse the
selected Lea tab, ignore empty selections and retain blocked-site decisions.
The earlier About fixes remain in the matching source; this is not a claim of
a fresh four-route About regression on this EXE or cold-start reliability.

Full scripting is NOT qualified: intermittent startup failures remain, and a
timed diagnostic full suite hit its unchanged 300-second limit in phase86.
Remote scripting, broad web APIs, security/lifecycle and compatibility remain
incomplete. This is not a final release or security certification.

The normal worker is included unchanged; diagnostic timing-only worker03C0 is
not included. Boot EXEs are test tools; ordinary use starts Firespider Internet
19.exe. Runtime ZIP only, not the complete source SDK. Matching source, build
recipes and failure evidence are retained separately by manifest references.
ZIP hashes are verified, but extraction and clean-machine testing are separate.
'@
        $manifest.libraryReceiptSha256=$proof.libraryHostReceiptSha256
        $manifest.knownFullScriptFailureSha256='BD9A11F7449617962043E7435A92E32E4A50F79704FEB9D4ADB7D89D77BC1EAF'
        $manifest.sourceCheckpoint=@{name='library-and-trace-source-20261026.zip';sha256='A1515801B6A9696951BA07F874CAAD976E4A7896DAAD18F85D8138F6B8E38AC0'}
    }
    $generated=@{'LEA-TEST-BUILD.txt'=$note;'lea-test-build-manifest.json'=($manifest|ConvertTo-Json -Depth 6)}
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($member in $members){$s=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($s)}finally{$s.Dispose()}}
        foreach($name in $generated.Keys){$writer=[IO.StreamWriter]::new($zip.CreateEntry($name).Open(),[Text.UTF8Encoding]::new($false));try{$writer.Write($generated[$name])}finally{$writer.Dispose()}}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+2){throw 'Package entry count differs'}
        foreach($member in $members){$s=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $member.sha256){throw 'ZIP member differs'}}finally{$s.Dispose()}}
        foreach($name in $generated.Keys){$reader=[IO.StreamReader]::new($zip.GetEntry($name).Open());try{if($reader.ReadToEnd() -cne $generated[$name]){throw 'Generated package note/manifest differs'}}finally{$reader.Dispose()}}
    }finally{$zip.Dispose()}
    $result=@{verified=$true;archive=$archive;sha256=(Get-FileHash $archive).Hash;bytes=(Get-Item $archive).Length;runtimeFiles=$members.Count;hostReceiptSha256=$ExpectedHostReceipt;extractedPackageExecuted=$false;sourceIncluded=$false;developmentOnly=$true}
    $result|ConvertTo-Json|Set-Content (Join-Path $out 'result.json')
    $result|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
