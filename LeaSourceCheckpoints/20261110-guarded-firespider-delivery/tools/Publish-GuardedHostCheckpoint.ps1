param([switch]$Results,[switch]$Delivery)
$ErrorActionPreference='Stop'
$root='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy'
$repo='I:\LeaBuildStorage\github-source-checkpoint-20261109-v1'
$scope='LeaSourceCheckpoints/20261110-guarded-firespider'
$dest=Join-Path $repo $scope
$readback='I:\LeaBuildStorage\github-guarded-firespider-readback-20261110-v1'
if($Delivery){$Results=$true}
if($Results){$scope='LeaSourceCheckpoints/20261110-guarded-firespider-results';$dest=Join-Path $repo $scope;$readback='I:\LeaBuildStorage\github-guarded-firespider-results-readback-20261110-v1'}
if($Delivery){$scope='LeaSourceCheckpoints/20261110-guarded-firespider-delivery';$dest=Join-Path $repo $scope;$readback='I:\LeaBuildStorage\github-guarded-firespider-delivery-readback-20261110-v1'}
$env:TEMP='I:\LeaBuildStorage\github-temp';$env:TMP=$env:TEMP
New-Item -ItemType Directory -Path $env:TEMP -Force|Out-Null
function Git-Checked([string[]]$Arguments){$value=& git -c safe.directory=$repo -c maintenance.auto=false -C $repo @Arguments;if($LASTEXITCODE){throw 'Git operation failed'};return $value}
if((Git-Checked @('branch','--show-current')) -cne 'codex/lea-source-backup-20261102-v1' -or (Git-Checked @('remote','get-url','origin')) -cne 'https://github.com/elliot7williams/family-fun-friday-games-night.git' -or (Git-Checked @('status','--porcelain'))){throw 'Clean intended backup checkout required'}
if((Test-Path -LiteralPath $dest) -or (Test-Path -LiteralPath $readback)){throw 'Preserve checkpoint'}
$rows=[Collections.Generic.List[object]]::new()
function Save([string]$From,[string]$Relative,[string]$Expected=''){
 if([IO.Path]::IsPathRooted($Relative) -or $Relative.Contains(':') -or $Relative.Split('/') -contains '..' -or [IO.Path]::GetExtension($Relative) -in @('.dll','.exe','.pdb','.pfx','.p12','.snk','.key')){throw 'Checkpoint path/binary refusal'}
 $hash=(Get-FileHash -LiteralPath $From).Hash;if($Expected -and $hash -cne $Expected){throw ('Input drift: '+$From)}
 $to=Join-Path $dest $Relative;if(Test-Path -LiteralPath $to){throw 'Checkpoint collision'}
 New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $From -Destination $to
 if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Copy drift'};$rows.Add(@{path=$Relative;sha256=$hash;bytes=(Get-Item -LiteralPath $to).Length})
}
$hostPath='I:\LeaBuildStorage\guarded-sdk-host-source-20261110-v1\source.json'
$sdkPath='I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1\source.json'
$hostPlan=Get-Content -LiteralPath $hostPath -Raw|ConvertFrom-Json
$sdk=Get-Content -LiteralPath $sdkPath -Raw|ConvertFrom-Json
if((Get-FileHash -LiteralPath $hostPath).Hash -cne 'A61CA720C37A4511774728230E74021609BFEB82F18BAFD46ED824FB452D6788' -or (Get-FileHash -LiteralPath $sdkPath).Hash -cne '21414F48C7A32E6B2ED0865CBA79A8DE3018A4B581075A7244E5ED534AB7E183' -or !$hostPlan.guardedSourceSdk -or !$sdk.verified){throw 'Exact source plans required'}
$hostFiles=@($hostPlan.files|Where-Object {$_.path -cnotin $hostPlan.sdkProducts.path -and [IO.Path]::GetExtension($_.path) -notin @('.dll','.exe','.pdb','.pfx','.p12','.snk','.key')})
$sdkFiles=@($sdk.files|Where-Object {$_.path -notmatch '^lib/'})
if($hostFiles.Count -ne 87 -or $sdkFiles.Count -ne 128){throw 'Complete source inventory required'}
if(!$Results){foreach($f in $hostFiles){Save (Join-Path $hostPlan.source $f.path) ('host-source/'+$f.path) $f.sha256}
foreach($f in $sdkFiles){Save (Join-Path 'I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1\source' $f.path) ('sdk-source/'+$f.path) $f.sha256}}
foreach($p in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/MEMORY_DOCUMENTS.md','LeaForge/SDK_MEMORY_POLICY.md','LeaForge/SDK_SOURCE_FULL_SUITE.md','LeaForge/SOURCE_BACKUPS.md','Lea.Build/Prepare-GuardedSdkHost.ps1','Lea.Build/Build-AccessibilityHost.ps1','Lea.Build/Test-OwnedAccessibilityHost.ps1','Lea.Build/Audit-AccessibilityHostUi.ps1','Lea.Build/Prepare-CurrentFirespiderRuntime.ps1','Lea.Build/Prepare-CurrentFirespiderSmokes.ps1','Lea.Build/Audit-GuardedSdkHostCampaign.ps1','Lea.Build/Prepare-CurrentFirespiderPrint.ps1','Lea.Build/Audit-CurrentFirespiderPrint.ps1','Lea.Firespider/AccessibilityReport.vb','Lea.Firespider/AccessibilityRoute.vb')){Save (Join-Path $root $p) $p}
Save $hostPath 'evidence/host-source.json'
Save $sdkPath 'evidence/sdk-source.json'
Save $PSCommandPath 'tools/Publish-GuardedHostCheckpoint.ps1'
if($Delivery){foreach($p in @('Lea.Build/Package-CurrentFirespider.ps1','Lea.Build/Prepare-CurrentFirespiderExtraction.ps1','Lea.Build/Audit-CurrentFirespiderDelivery.ps1')){Save (Join-Path $root $p) $p}}
foreach($spec in @(
 @{folder='I:\LeaBuildStorage\guarded-sdk-host-build-20261110-v1';prefix='build'},
 @{folder='I:\LeaBuildStorage\guarded-sdk-host-ui-audit-20261110-v1';prefix='ui-audit'},
 @{folder='I:\LeaBuildStorage\guarded-sdk-host-smoke-audit-20261110-v1';prefix='startup-audit'},
 @{folder='I:\LeaBuildStorage\guarded-sdk-host-smoke-source-20261110-v1';prefix='smoke-source'},
 @{folder='I:\LeaBuildStorage\guarded-sdk-host-workflow-source-20261110-v1';prefix='workflow-source'},
 @{folder='I:\LeaBuildStorage\guarded-sdk-host-print-source-20261110-v1';prefix='print-source'},
 @{folder='I:\LeaBuildStorage\guarded-sdk-host-continuation-20261110-v1';prefix='handoff'}
)){foreach($f in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object Extension -in @('.json','.txt','.ps1','.cs','.vb','.md','.png')){Save $f.FullName ('evidence/'+$spec.prefix+'/'+$f.Name)}}
if($Results){foreach($spec in @(@{folder='I:\LeaBuildStorage\guarded-sdk-host-workflow-audit-20261110-v1';prefix='workflow-audit'},@{folder='I:\LeaBuildStorage\guarded-sdk-host-print-audit-20261110-v1';prefix='print-audit'})){foreach($f in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object Extension -in @('.json','.txt','.ps1','.png')){Save $f.FullName ('evidence/'+$spec.prefix+'/'+$f.Name)}}}
if($Delivery){foreach($spec in @(@{folder='I:\LeaBuildStorage\guarded-sdk-host-delivery-audit-20261110-v1';prefix='delivery-audit'},@{folder='I:\LeaBuildStorage\guarded-sdk-host-package-20261110-v1';prefix='package'},@{folder='I:\LeaBuildStorage\guarded-sdk-host-extraction-source-20261110-v1';prefix='extraction-source'},@{folder='I:\LeaBuildStorage\guarded-sdk-delivery-work-20261110-v1';prefix='preserved-delivery-recipes'})){foreach($f in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object Extension -in @('.json','.txt','.ps1','.md')){Save $f.FullName ('evidence/'+$spec.prefix+'/'+$f.Name)}}}
$manifest=@{date='2026-11-10';hostSourceMembers=87;sdkSourceMembers=128;hostSha256='1E390343125011CAA3F78582E91FAFC036E07A31F7E81B4B727ED8503FC39606';uiPassed=$true;startupSmokesPassed=$true;workflowCampaignStarted=$true;workflowCampaignQualified=$false;workflowPreflightStoppedForStorage=$true;printExecuted=$false;releaseQualified=$false;hostAdopted=$false;priorSdkQualificationCommit='0e44ad7691e71862eb1b6634170a06b63b2aa56c';files=$rows}
if($Results){$wa=Get-Content -LiteralPath 'I:\LeaBuildStorage\guarded-sdk-host-workflow-audit-20261110-v1\audit.json' -Raw|ConvertFrom-Json;$pa=Get-Content -LiteralPath 'I:\LeaBuildStorage\guarded-sdk-host-print-audit-20261110-v1\audit.json' -Raw|ConvertFrom-Json;if(!$wa.verified -or !$wa.all15WorkflowsQualified -or !$pa.verified -or !$pa.printCommandPassed -or !$pa.previewPassed -or $wa.hostSha256 -cne $manifest.hostSha256 -or $pa.hostSha256 -cne $manifest.hostSha256){throw 'Exact qualification required'};$manifest.hostSourceMembers=0;$manifest.sdkSourceMembers=0;$manifest.sourceCheckpointCommit='c870b8b581e2118fcfc1d074f8b2ded077152c5e';$manifest.workflowCampaignQualified=$true;$manifest.printExecuted=$true;$manifest.physicalPrintingQualified=$false}
if($Delivery){$da=Get-Content -LiteralPath 'I:\LeaBuildStorage\guarded-sdk-host-delivery-audit-20261110-v1\audit.json' -Raw|ConvertFrom-Json;if(!$da.verified -or !$da.all402ArchiveMembersReadBack -or !$da.actualExeResponsive -or !$da.allOwnedProcessesRetired -or $da.archiveSha256 -cne '708A70D7DDE0D4CF49EF5D1976C2CF261357D136275C0FA91D26B44532C6322A'){throw 'Exact delivery qualification required'};$manifest.archiveSha256=$da.archiveSha256;$manifest.freshExtractionSmokesPassed=$true;$manifest.cleanMachineQualified=$false;$manifest.all15ExtractedWorkflowsQualified=$false}
$manifest|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $dest 'manifest.json') -Encoding utf8
[IO.File]::WriteAllText((Join-Path $dest '.gitattributes'),"* -text`n")
$all=@(Get-ChildItem -LiteralPath $dest -Recurse -File)
$pattern='(?:ghp_[A-Za-z0-9]{36}|github_pat_[A-Za-z0-9_]{40,}|sk-proj-[A-Za-z0-9_-]{40,}|AKIA[A-Z0-9]{16}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----)'
foreach($f in $all|Where-Object Extension -notin @('.png','.gif','.jpg','.ico','.wav')){if(Select-String -LiteralPath $f.FullName -Pattern $pattern -Quiet){throw 'Potential secret refused'}}
Git-Checked @('add','-f','--',$scope)|Out-Null
$stagedText=(Git-Checked @('diff','--cached','--name-only','-z')) -join "`n"
$staged=@($stagedText.Split([char]0,[StringSplitOptions]::RemoveEmptyEntries))
if($staged.Count -ne $all.Count -or @($staged|Where-Object {!$_.StartsWith($scope+'/',[StringComparison]::Ordinal)}).Count){throw 'Index scope differs'}
$indexText=(Git-Checked @('ls-files','--stage','-z','--',$scope)) -join "`n"
foreach($line in $indexText.Split([char]0,[StringSplitOptions]::RemoveEmptyEntries)){
 if($line -notmatch '^100644 ([a-f0-9]{40}) 0\t(.+)$'){throw 'Unexpected index record'}
 $expected=$Matches[1];$bytes=[IO.File]::ReadAllBytes((Join-Path $repo $Matches[2]));$hash=[Security.Cryptography.IncrementalHash]::CreateHash([Security.Cryptography.HashAlgorithmName]::SHA1)
 try{$hash.AppendData([Text.Encoding]::UTF8.GetBytes('blob '+$bytes.LongLength+"`0"));$hash.AppendData($bytes);if([Convert]::ToHexString($hash.GetHashAndReset()).ToLowerInvariant() -cne $expected){throw 'Index bytes differ'}}finally{$hash.Dispose()}
}
$message=if($Results){'Back up exact guarded Firespider 15 workflows and print preview results'}else{'Back up guarded Lea Firespider source, UI and actual EXE startup checks'}
if($Delivery){$message='Back up guarded Firespider development package and fresh extraction qualification'}
Git-Checked @('-c','user.name=Elliot Williams','-c','user.email=58864955+elliot7williams@users.noreply.github.com','commit','-m',$message)
Git-Checked @('push','origin','HEAD:refs/heads/codex/lea-source-backup-20261102-v1')
$head=(Git-Checked @('rev-parse','HEAD')).Trim();$remote=Git-Checked @('ls-remote','origin','refs/heads/codex/lea-source-backup-20261102-v1')
if(($remote -split '\s+')[0] -cne $head){throw 'Remote head differs'}
New-Item -ItemType Directory -Path $readback|Out-Null
$client=[Net.Http.HttpClient]::new();$client.Timeout=[TimeSpan]::FromSeconds(45);$verified=[Collections.Generic.List[object]]::new()
try{foreach($f in $all){$relative=[IO.Path]::GetRelativePath($repo,$f.FullName).Replace('\','/');$encoded=($relative.Split('/')|ForEach-Object {[uri]::EscapeDataString($_)}) -join '/';$bytes=$client.GetByteArrayAsync('https://raw.githubusercontent.com/elliot7williams/family-fun-friday-games-night/'+$head+'/'+$encoded).GetAwaiter().GetResult();$sha=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes));if($sha -cne (Get-FileHash -LiteralPath $f.FullName).Hash){throw 'Remote bytes differ'};$verified.Add(@{path=$relative;sha256=$sha;bytes=$bytes.LongLength})}}finally{$client.Dispose()}
@{verified=$true;commit=$head;scope=$scope;files=$verified;releaseQualified=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $readback 'result.json') -Encoding utf8
@{commit=$head;verifiedRemoteFiles=$verified.Count;proof=(Join-Path $readback 'result.json')}|ConvertTo-Json
