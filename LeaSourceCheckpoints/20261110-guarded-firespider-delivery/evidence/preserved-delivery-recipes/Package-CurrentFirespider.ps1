param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$PrintAuditSha256)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\current-firespider-package-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve current package'}
$leases=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$Path,[string]$Hash){
    $item=Get-Item -LiteralPath $Path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse package source refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw ('Package source differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$Hash})
}
function Proof([string]$Path,[string]$Hash){Pin $Path $Hash;return Get-Content -LiteralPath $Path -Raw|ConvertFrom-Json}
try{
    Pin $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash
    $workflowPath=Join-Path $root 'continuity-evidence/current-source-host-workflows-v1/result.json'
    $r=Proof $workflowPath '38C557C0287B6EFD9E803D438ACF216243231B1D5099D8B750E9F28BE6BB6CD6'
    $workflowAuditPath='I:\LeaBuildStorage\current-firespider-workflow-audit-20261110-v1\audit.json'
    $a=Proof $workflowAuditPath '26806608D4BED93F2688B4C7993EC776BFE28A2AFD3E658ADCCECE079FD60253'
    if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.before.Count -ne 400 -or $r.runs.Count -ne 15 -or $r.byteChanges.Count -or $r.aclChanges.Count -or !$a.verified -or !$a.all15WorkflowsQualified){throw 'Current workflow qualification incomplete'}
    $printAuditPath='I:\LeaBuildStorage\current-firespider-print-audit-20261110-v1\audit.json'
    $printAudit=Proof $printAuditPath $PrintAuditSha256
    if(!$printAudit.verified -or !$printAudit.printCommandPassed -or !$printAudit.previewPassed -or !$printAudit.runtimeBytesAndAclStable -or $printAudit.queueSubmitted -or $printAudit.assertionsChanged -or $printAudit.limitsChanged -or $printAudit.hostSha256 -cne '9513A0BD7C620589F4AB332CE19F155686C01104B1E8D99A5DECAAF1814F2586'){throw 'Exact current print qualification required'}
    $printPath=Join-Path $root 'continuity-evidence/current-source-host-print-v1/result.json'
    $p=Proof $printPath $printAudit.printSha256
    if(!$p.passed -or !$p.inputsUnchanged -or $p.ownerAttention -or $p.before.Count -ne 403 -or $p.byteChanges.Count -or $p.aclChanges.Count){throw 'Current print receipt incomplete'}
    foreach($file in $r.before){$name=[IO.Path]::GetRelativePath($r.runtime,$file.path);$same=@($p.before|Where-Object {[IO.Path]::GetRelativePath($p.runtime,$_.path) -ceq $name});if($same.Count -ne 1 -or $same[0].sha256 -cne $file.sha256){throw 'Print/workflow product binding differs'}}
    $sourceReceipt='I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1\source.json';Pin $sourceReceipt '51A823667E96D3ED4C3EC526E0747B8C19B1EBBE59A8A05324503DBEB4FED9C9'
    $buildReceipt='I:\LeaBuildStorage\current-firespider-build-20261109-v1\result.json';Pin $buildReceipt 'CD72727F31282AABB42DABBEE2F55BCA69C63A9352A3528C8EB5657A03A2B408'
    $githubProof='I:\LeaBuildStorage\github-current-firespider-readback-20261110-v1\result.json';Pin $githubProof (Get-FileHash -LiteralPath $githubProof).Hash
    $backup=Get-Content -LiteralPath $githubProof -Raw|ConvertFrom-Json
    if(!$backup.verified -or $backup.commit -cne '0ee1f39095a0ab22db08f3a32d16e026ae30fcc3' -or $backup.files.Count -ne 325){throw 'Verified source backup required'}
    $rows=[Collections.Generic.List[object]]::new();$seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $r.before){
        $name=[IO.Path]::GetRelativePath($r.runtime,$file.path).Replace('\','/')
        if([IO.Path]::IsPathRooted($name) -or $name.Contains(':') -or $name.Split('/') -contains '..' -or !$seen.Add($name)){throw 'Package path escape/collision'}
        Pin $file.path $file.sha256;if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Qualified runtime ACL changed'}
        $rows.Add(@{path=$name;sha256=$file.sha256;bytes=(Get-Item -LiteralPath $file.path).Length;source=$file.path})
    }
    if((Get-ChildItem -LiteralPath $r.runtime -Recurse -File).Count -ne 400){throw 'Runtime inventory differs'}
    New-Item -ItemType Directory -Path $out|Out-Null
    $manifest=@{developmentOnly=$true;sourceIncluded=$false;runtimeFiles=400;hostSha256=$a.hostSha256;runtimeVersion='8.0.33';rollForward='Disable';requires=@('.NET Framework 4.8','Microsoft.NETCore.App 8.0.33 x64','Fresh NTFS extraction directory');workflowSha256=(Get-FileHash -LiteralPath $workflowPath).Hash;workflowAuditSha256=(Get-FileHash -LiteralPath $workflowAuditPath).Hash;printSha256=$printAudit.printSha256;printAuditSha256=$PrintAuditSha256;sourceReferences=@{githubCommit=$backup.commit;githubScope=$backup.scope;sourceReceiptSha256=(Get-FileHash -LiteralPath $sourceReceipt).Hash;hostBuildSha256=(Get-FileHash -LiteralPath $buildReceipt).Hash};files=@($rows|Select-Object path,sha256,bytes);limitations=@('Fresh ZIP extraction execution not yet verified at package creation','Historical startup failures unresolved','No all-sites, clean-PC or physical-print qualification','Third-party notices/redistribution review incomplete','Not a final release')}
    $manifest|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'lea-development-manifest.json') -Encoding utf8
    'Firespider with Lea - development build. Extract ALL files to a NEW NTFS directory, not the exFAT archive drive. Requires .NET Framework 4.8 and Microsoft.NETCore.App 8.0.33 x64, roll-forward disabled. Run Firespider Internet 19.exe (inherited window title17). Keep the full matching runtime together; do not overwrite an installed browser/profile. Compression is for storage, not an automatic execution speedup. Source references, hashes and unresolved limits are in lea-development-manifest.json. No final release, all-sites or redistribution-readiness claim.'|Set-Content -LiteralPath (Join-Path $out 'DEVELOPMENT.txt') -Encoding utf8
    foreach($name in @('lea-development-manifest.json','DEVELOPMENT.txt')){if(!$seen.Add($name)){throw 'Metadata collision'};$path=Join-Path $out $name;Pin $path (Get-FileHash -LiteralPath $path).Hash;$rows.Add(@{path=$name;sha256=(Get-FileHash -LiteralPath $path).Hash;bytes=(Get-Item -LiteralPath $path).Length;source=$path})}
    $archive=Join-Path $out 'Firespider-Lea-current-source-development-20261110.zip'
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{foreach($file in $rows){$null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$file.source,$file.path,[IO.Compression.CompressionLevel]::Optimal)}}finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{if($zip.Entries.Count -ne 402){throw 'Archive inventory differs'};foreach($file in $rows){$entry=$zip.GetEntry($file.path);if(!$entry -or $entry.Length -ne $file.bytes){throw 'Archive member length differs'};$stream=$entry.Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $file.sha256){throw 'Archive byte readback differs'}}finally{$stream.Dispose()}}}finally{$zip.Dispose()}
    foreach($file in $inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Package input drift'}}
    @{verified=$true;archive=$archive;sha256=(Get-FileHash -LiteralPath $archive).Hash;bytes=(Get-Item -LiteralPath $archive).Length;entries=402;runtimeFiles=400;members=$rows;inputs=$inputs;sourceIncluded=$false;extractedExecutionVerified=$false;installedBrowserChanged=$false;releaseQualified=$false}|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash -LiteralPath $archive|Format-List Hash
}finally{foreach($lease in $leases){$lease.Dispose()}}
