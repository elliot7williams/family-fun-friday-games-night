$ErrorActionPreference='Stop'
$root='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy'
$repo='I:\LeaBuildStorage\github-source-checkpoint-20261109-v1'
$scope='LeaSourceCheckpoints/20261110-memory-reports'
$dest=Join-Path $repo $scope
$readback='I:\LeaBuildStorage\github-memory-reports-readback-20261110-v1'
$build='I:\LeaBuildStorage\current-firespider-build-20261109-v1'
$sdk='I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1'
$auditRoot='I:\LeaBuildStorage\current-firespider-workflow-audit-20261110-v1'
function Git-Checked([string[]]$Arguments){$value=& git -c safe.directory=$repo -c maintenance.auto=false -C $repo @Arguments;if($LASTEXITCODE){throw 'Git operation failed'};return $value}
if((Git-Checked @('branch','--show-current')) -cne 'codex/lea-source-backup-20261102-v1' -or (Git-Checked @('remote','get-url','origin')) -cne 'https://github.com/elliot7williams/family-fun-friday-games-night.git' -or (Git-Checked @('status','--porcelain'))){throw 'Clean intended backup checkout required'}
if((Test-Path -LiteralPath $dest) -or (Test-Path -LiteralPath $readback)){throw 'Preserve existing source checkpoint'}
$auditPath=Join-Path $auditRoot 'audit.json'
if((Get-FileHash -LiteralPath $auditPath).Hash -cne '26806608D4BED93F2688B4C7993EC776BFE28A2AFD3E658ADCCECE079FD60253'){throw 'Workflow audit differs'}
$audit=Get-Content -LiteralPath $auditPath -Raw|ConvertFrom-Json
if(!$audit.verified -or !$audit.all15WorkflowsQualified -or !$audit.runtimeBytesAndAclStable -or $audit.runtimeMembers -ne 400 -or $audit.hostSha256 -cne '9513A0BD7C620589F4AB332CE19F155686C01104B1E8D99A5DECAAF1814F2586'){throw 'Exact current qualified host required'}
$sdkPath=Join-Path $sdk 'source.json'
if((Get-FileHash -LiteralPath $sdkPath).Hash -cne '51A823667E96D3ED4C3EC526E0747B8C19B1EBBE59A8A05324503DBEB4FED9C9'){throw 'SDK export differs'}
$sdkPlan=Get-Content -LiteralPath $sdkPath -Raw|ConvertFrom-Json
$hostPlan=Get-Content -LiteralPath (Join-Path $build 'candidate-manifest.json') -Raw|ConvertFrom-Json
$sdkFiles=@($sdkPlan.files|Where-Object {$_.path -notmatch '^lib/'})
$hostFiles=@($hostPlan.files|Where-Object kind -CEQ 'preserved-host-source')
if($sdkFiles.Count -ne 128 -or $hostFiles.Count -ne 87 -or !$hostPlan.hostSourceUnchanged){throw 'Complete current source coverage required'}
$rows=[Collections.Generic.List[object]]::new()
function Save([string]$From,[string]$Relative,[string]$Expected=''){
    if([IO.Path]::IsPathRooted($Relative) -or $Relative.Contains(':') -or $Relative.Split('/') -contains '..'){throw 'Checkpoint path escape'}
    $hash=(Get-FileHash -LiteralPath $From).Hash;if($Expected -and $hash -cne $Expected){throw ('Source/evidence drift: '+$From)}
    $to=Join-Path $dest $Relative;if(Test-Path -LiteralPath $to){throw 'Checkpoint collision'}
    New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $From -Destination $to
    if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Checkpoint readback differs'};$rows.Add(@{path=$Relative;sha256=$hash;bytes=(Get-Item -LiteralPath $to).Length})
}
foreach($file in $sdkFiles){Save (Join-Path $sdk ('source/'+$file.path)) ('sdk-source/'+$file.path) $file.sha256}
foreach($file in $hostFiles){Save (Join-Path $build ('source/'+$file.path)) ('firespider-source/'+$file.path) $file.sha256}
foreach($relative in @('PROJECT_CONTEXT.md','Lea.Build/Build-CurrentFirespider.ps1','Lea.Build/Prepare-CurrentFirespiderRuntime.ps1','Lea.Build/Prepare-CurrentFirespiderSmokes.ps1','Lea.Build/Audit-CurrentFirespider.ps1','Lea.Build/Publish-CurrentFirespider.ps1','Lea.Build/Test-PrivateBindingHostSmokes-v1.generated.ps1','Lea.Build/Test-PrivateBindingHostWorkflows-v1.generated.ps1','Lea.Build/Invoke-StartupBody.ps1','LeaForge/CURRENT_SOURCE_HOST.md','LeaForge/CURRENT_HOST.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/SOURCE_BACKUPS.md','Lea.Sdk/CURRENT_BUILD.md')){Save (Join-Path $root $relative) $relative}
Save (Join-Path $root 'continuity-evidence/small-engine-v1/source/Lea.Build/Test-FirespiderStartup.ps1') 'evidence/startup/Test-FirespiderStartup.ps1' '1042F5CB68AB16A1C66EFB8524B2C5FD6C1C5098A74F088161060692BD629414'
foreach($spec in @(@{folder=$auditRoot;prefix='workflow'},@{folder='I:\LeaBuildStorage\current-firespider-audit-20261110-v1';prefix='smoke'},@{folder='I:\LeaBuildStorage\current-firespider-workflow-audit-failure-20261110-v1';prefix='initial-audit-stop'},@{folder='I:\LeaBuildStorage\current-firespider-workflow-pictures-20261110-v1';prefix='pictures'})){foreach($file in Get-ChildItem -LiteralPath $spec.folder -File){Save $file.FullName ('evidence/'+$spec.prefix+'/'+$file.Name)}}
Save (Join-Path $build 'candidate-manifest.json') 'evidence/host-source-manifest.json'
foreach($name in @('Lea.Engine/MemoryDocument.fragment.cs','Lea.Win/MemoryDocument.fragment.cs','Lea.Firespider/AccessibilityReport.vb','Lea.Firespider/AccessibilityRoute.vb','Lea.Tests/AccessibilityReportMain.vb','Lea.Tests/AccessibilityHostCase.cs','Lea.Tests/MemoryDocumentMain.cs','Lea.Tests/MemoryDocumentViewMain.cs','Lea.Build/Audit-CurrentHostLegacyRoutes.ps1','Lea.Build/Test-OwnedAccessibilityReport.ps1','Lea.Build/Prepare-AccessibilityHostSource.ps1','Lea.Build/Build-AccessibilityHost.ps1','Lea.Build/Test-OwnedAccessibilityHost.ps1','Lea.Build/Audit-AccessibilityHostUi.ps1','Lea.Build/Test-OwnedMemoryDocument.ps1','Lea.Build/Audit-MemoryDocumentView.ps1','Lea.Build/Prepare-MemoryReportHost.ps1','Lea.Build/Prepare-MemoryReportHostProbe.ps1','Lea.Build/Prepare-MemoryReportBackup.ps1','LeaForge/HOST_LEGACY_ROUTES.md','LeaForge/MEMORY_DOCUMENTS.md')){Save (Join-Path $root $name) $name}
$memorySource='I:\LeaBuildStorage\memory-document-view-20261110-v1\source'
foreach($folder in @('Lea.Engine','Lea.Win')){foreach($file in Get-ChildItem -LiteralPath (Join-Path $memorySource $folder) -File){Save $file.FullName ('memory-sdk-source/'+$folder+'/'+$file.Name)}}
$memoryHost='I:\LeaBuildStorage\memory-report-host-source-20261110-v1\source'
foreach($file in $hostFiles){Save (Join-Path $memoryHost $file.path) ('memory-host-source/'+$file.path)}
foreach($spec in @(
 @{folder='I:\LeaBuildStorage\memory-document-model-20261110-v1';prefix='memory-model'},
 @{folder='I:\LeaBuildStorage\memory-document-view-audit-20261110-v1';prefix='memory-view-audit'},
 @{folder='I:\LeaBuildStorage\accessibility-host-ui-failure-20261110-v1';prefix='retained-ui-assertion-failure'},
 @{folder='I:\LeaBuildStorage\accessibility-host-ui-audit-20261110-v3';prefix='retained-private-file-reproduction'},
 @{folder='I:\LeaBuildStorage\memory-report-host-ui-audit-20261110-v1';prefix='memory-host-ui-audit'},
 @{folder='I:\LeaBuildStorage\memory-report-host-build-20261110-v1';prefix='memory-host-build'},
 @{folder='I:\LeaBuildStorage\memory-report-host-source-20261110-v1';prefix='memory-host-source'},
 @{folder='I:\LeaBuildStorage\memory-report-host-probe-20261110-v1';prefix='memory-host-probe'}
)){foreach($file in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object Extension -in @('.cs','.vb','.ps1','.json','.txt','.md','.png')){Save $file.FullName ('evidence/'+$spec.prefix+'/'+$file.Name)}}
Save $PSCommandPath 'evidence/publish-memory-reports.generated.ps1'
$dependencies=@($sdkPlan.files|Where-Object {$_.path -match '^lib/'})
if($dependencies.Count -ne 8){throw 'Dependency inventory differs'}
$dependencies|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $dest 'dependencies.json') -Encoding utf8
[IO.File]::WriteAllText((Join-Path $dest '.gitattributes'),"* -text`n")
@{date='2026-11-10';release=$false;sourceSdkMembers=128;hostSourceMembers=87;smokeCases=3;workflowCases=15;hostSha256=$audit.hostSha256;workflowAuditSha256=(Get-FileHash -LiteralPath $auditPath).Hash;thirdPartyBinariesIncluded=$false;files=$rows}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $dest 'manifest.json') -Encoding utf8
$all=@(Get-ChildItem -LiteralPath $dest -Recurse -File)
if(@($all|Where-Object Extension -in @('.dll','.exe','.pdb','.pfx','.p12','.snk','.key')).Count){throw 'Unexpected product/dependency/key material'}
$pattern='(?:ghp_[A-Za-z0-9]{36}|github_pat_[A-Za-z0-9_]{40,}|sk-proj-[A-Za-z0-9_-]{40,}|AKIA[A-Z0-9]{16}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----)'
foreach($file in $all|Where-Object Extension -notin @('.png','.gif','.jpg','.ico','.wav')){if(Select-String -LiteralPath $file.FullName -Pattern $pattern -Quiet){throw ('Potential secret: '+$file.FullName)}}
Git-Checked @('add','-f','--',$scope)|Out-Null
$stagedText=(Git-Checked @('diff','--cached','--name-only','-z')) -join "`n"
$staged=@($stagedText.Split([char]0,[StringSplitOptions]::RemoveEmptyEntries))
if($staged.Count -ne $all.Count -or @($staged|Where-Object {!$_.StartsWith($scope+'/',[StringComparison]::Ordinal)}).Count){throw 'Staging scope differs'}
$indexText=(Git-Checked @('ls-files','--stage','-z','--',$scope)) -join "`n"
foreach($line in $indexText.Split([char]0,[StringSplitOptions]::RemoveEmptyEntries)){
    if($line -notmatch '^100644 ([a-f0-9]{40}) 0\t(.+)$'){throw 'Unexpected raw index record'}
    $expected=$Matches[1];$bytes=[IO.File]::ReadAllBytes((Join-Path $repo $Matches[2]))
    $hash=[Security.Cryptography.IncrementalHash]::CreateHash([Security.Cryptography.HashAlgorithmName]::SHA1)
    try{$hash.AppendData([Text.Encoding]::UTF8.GetBytes('blob '+$bytes.LongLength+"`0"));$hash.AppendData($bytes);if([Convert]::ToHexString($hash.GetHashAndReset()).ToLowerInvariant() -cne $expected){throw 'Raw staged bytes differ'}}finally{$hash.Dispose()}
}
Git-Checked @('-c','user.name=Elliot Williams','-c','user.email=58864955+elliot7williams@users.noreply.github.com','commit','-m','Back up Lea memory documents and Firespider private-report regression')
Git-Checked @('push','origin','HEAD:refs/heads/codex/lea-source-backup-20261102-v1')
$head=(Git-Checked @('rev-parse','HEAD')).Trim();$remote=Git-Checked @('ls-remote','origin','refs/heads/codex/lea-source-backup-20261102-v1')
if(($remote -split '\s+')[0] -cne $head){throw 'Remote head differs'}
New-Item -ItemType Directory -Path $readback|Out-Null
$client=[Net.Http.HttpClient]::new();$client.Timeout=[TimeSpan]::FromSeconds(45);$verified=[Collections.Generic.List[object]]::new()
try{foreach($file in $all){$relative=[IO.Path]::GetRelativePath($repo,$file.FullName).Replace('\','/');$encoded=($relative.Split('/')|ForEach-Object {[uri]::EscapeDataString($_)}) -join '/';$url='https://raw.githubusercontent.com/elliot7williams/family-fun-friday-games-night/'+$head+'/'+$encoded;$bytes=$client.GetByteArrayAsync($url).GetAwaiter().GetResult();$sha=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes));if($sha -cne (Get-FileHash -LiteralPath $file.FullName).Hash){throw 'Remote readback differs'};$verified.Add(@{path=$relative;sha256=$sha;bytes=$bytes.LongLength})}}finally{$client.Dispose()}
@{verified=$true;commit=$head;files=$verified;scope=$scope;release=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $readback 'result.json') -Encoding utf8
@{commit=$head;verifiedRemoteFiles=$verified.Count;proof=(Join-Path $readback 'result.json')}|ConvertTo-Json
