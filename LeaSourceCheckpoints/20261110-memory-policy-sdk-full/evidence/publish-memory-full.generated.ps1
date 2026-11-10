param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$OutcomeSha256)
$ErrorActionPreference='Stop'
$root='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy'
$repo='I:\LeaBuildStorage\github-source-checkpoint-20261109-v1'
$scope='LeaSourceCheckpoints/20261110-memory-policy-sdk-full'
$dest=Join-Path $repo $scope
$prep='I:\LeaBuildStorage\memory-policy-full-preparation-20261110-v1'
$auditDir='I:\LeaBuildStorage\memory-policy-full-outcome-20261110-v1'
$readback='I:\LeaBuildStorage\github-memory-policy-full-readback-20261110-v1'
function Git-Checked([string[]]$Arguments){$value=& git -c safe.directory=$repo -c maintenance.auto=false -C $repo @Arguments;if($LASTEXITCODE){throw 'Git operation failed'};return $value}
if((Git-Checked @('branch','--show-current')) -cne 'codex/lea-source-backup-20261102-v1' -or (Git-Checked @('remote','get-url','origin')) -cne 'https://github.com/elliot7williams/family-fun-friday-games-night.git' -or (Git-Checked @('status','--porcelain'))){throw 'Clean intended backup checkout required'}
if((Test-Path -LiteralPath $dest) -or (Test-Path -LiteralPath $readback)){throw 'Preserve existing checkpoint'}
$auditPath=Join-Path $auditDir 'audit.json'
if((Get-FileHash -LiteralPath $auditPath).Hash -cne $OutcomeSha256){throw 'Outcome audit differs'}
$audit=Get-Content -LiteralPath $auditPath -Raw|ConvertFrom-Json
if(!$audit.observationVerified -or !$audit.fullSuitePassed -or $audit.completedPhases -ne 89 -or !$audit.runtimeBytesAndAclUnchanged -or !$audit.traceEnvironmentRestored -or $audit.quarantineRequired -or $audit.assertionsChanged -or $audit.limitsChanged -or $audit.preparationSha256 -cne 'F83A0473E23E89FFB5FEECFE39AD7F0252E97B404A688692D9F5F962979D6BC3' -or $audit.runSha256 -cne '77DFD17A6E8459260558AFA8A6D6429C0FB692F19C2E22E5C858EDB2E729E288'){throw 'Exact passing full-suite checkpoint required'}
$sourceRoot='I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1'
$sourcePath=Join-Path $sourceRoot 'source.json'
if((Get-FileHash -LiteralPath $sourcePath).Hash -cne '21414F48C7A32E6B2ED0865CBA79A8DE3018A4B581075A7244E5ED534AB7E183'){throw 'Source export differs'}
$source=Get-Content -LiteralPath $sourcePath -Raw|ConvertFrom-Json
$preparedPath=Join-Path $prep 'result.json'
if((Get-FileHash -LiteralPath $preparedPath).Hash -cne $audit.preparationSha256){throw 'Preparation differs'}
$prepared=Get-Content -LiteralPath $preparedPath -Raw|ConvertFrom-Json
$rows=[Collections.Generic.List[object]]::new()
function Save([string]$From,[string]$Relative,[string]$Expected=''){
    if([IO.Path]::IsPathRooted($Relative) -or $Relative.Contains(':') -or $Relative.Split('/') -contains '..'){throw 'Checkpoint path escape'}
    $hash=(Get-FileHash -LiteralPath $From).Hash;if($Expected -and $hash -cne $Expected){throw 'Source/evidence differs'}
    $to=Join-Path $dest $Relative;if(Test-Path -LiteralPath $to){throw 'Checkpoint collision'}
    New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $From -Destination $to
    if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Copy readback differs'};$rows.Add(@{path=$Relative;sha256=$hash;bytes=(Get-Item -LiteralPath $to).Length})
}
$sdkFiles=@($source.files|Where-Object {$_.path -notmatch '^lib/'})
$suiteFiles=@($prepared.members|Where-Object {$_.path -match '^(compiled-source|fixtures)/'})
if($sdkFiles.Count -ne 128 -or $suiteFiles.Count -ne 242){throw 'Source/fixture coverage differs'}
foreach($file in $sdkFiles){Save (Join-Path $sourceRoot ('source/'+$file.path)) ('sdk-source/'+$file.path) $file.sha256}
foreach($file in $suiteFiles){Save (Join-Path $prep ('assembly/'+$file.path)) ('full-suite/'+$file.path) $file.sha256}
foreach($relative in @('PROJECT_CONTEXT.md','Lea.Build/Prepare-CurrentSdkFullSuite.ps1','Lea.Build/Audit-CurrentSdkFullOutcome.ps1','Lea.Build/Test-OwnedSdkFullScript.ps1','Lea.Build/Publish-CurrentSdkFullCheckpoint.ps1','LeaForge/SDK_SOURCE_FULL_SUITE.md','LeaForge/SDK_CURRENT_SOURCE.md','LeaForge/SDK_SOURCE_GRAPH.md','LeaForge/SDK_FULL.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/SOURCE_BACKUPS.md','Lea.Sdk/CURRENT_BUILD.md')){Save (Join-Path $root $relative) $relative}
foreach($file in Get-ChildItem -LiteralPath $auditDir -File){Save $file.FullName ('evidence/outcome/'+$file.Name)}
foreach($file in Get-ChildItem -LiteralPath $prep -File|Where-Object Extension -in @('.cs','.json','.rsp','.ps1','.txt')){Save $file.FullName ('evidence/preparation/'+$file.Name)}
foreach($file in Get-ChildItem -LiteralPath 'I:\LeaBuildStorage\sdk-source-full-tools-20261109-v1' -File){Save $file.FullName ('evidence/reader/'+$file.Name)}
Save 'I:\LeaBuildStorage\sdk-source-full-publication-recovery-20261109-v1\WRITE_FAILURE.md' 'evidence/publication/WRITE_FAILURE.md'
foreach($name in @('Lea.Engine/MemoryDocument.fragment.cs','Lea.Win/MemoryDocument.fragment.cs','Lea.Tests/MemoryDocumentMain.cs','Lea.Tests/MemoryDocumentViewMain.cs','Lea.Tests/MemoryScriptPolicyMain.cs','Lea.Build/Prepare-MemoryPolicySdk.ps1','Lea.Build/Audit-MemoryPolicySdk.ps1','Lea.Build/Test-CurrentSdkWorker.ps1','Lea.Build/Test-OwnedMemoryDocument.ps1','Lea.Build/Test-OwnedMemoryScriptPolicy.ps1','Lea.Build/Audit-MemoryScriptPolicy.ps1','Lea.Build/Test-OwnedSdkMemorySurface.ps1','Lea.Build/Resume-OwnedSdkMemoryPolicy.ps1','Lea.Build/Audit-SdkMemorySurface.ps1','Lea.Build/Prepare-MemoryPolicyFull.ps1','Lea.Build/Prepare-MemoryPolicyFullBackup.ps1','LeaForge/SDK_MEMORY_POLICY.md','LeaForge/MEMORY_SCRIPT_POLICY.md','LeaForge/MEMORY_DOCUMENTS.md')){Save (Join-Path $root $name) $name}
foreach($spec in @(
 @{folder='I:\LeaBuildStorage\memory-policy-sdk-audit-20261110-v1';prefix='sdk'},
 @{folder='I:\LeaBuildStorage\memory-policy-sdk-memory-audit-20261110-v1';prefix='sdk-memory'},
 @{folder='I:\LeaBuildStorage\memory-script-policy-audit-20261110-v1';prefix='policy-negative-positive'},
 @{folder='I:\LeaBuildStorage\memory-policy-full-source-20261110-v1';prefix='projection-failure'},
 @{folder='I:\LeaBuildStorage\memory-policy-full-source-20261110-v2';prefix='projection-source'}
)){foreach($file in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object Extension -in @('.json','.txt','.ps1','.cs','.md','.png')){Save $file.FullName ('evidence/'+$spec.prefix+'/'+$file.Name)}}
Save $PSCommandPath 'evidence/publish-memory-full.generated.ps1'
$dependencies=@($source.files|Where-Object {$_.path -match '^lib/'})
if($dependencies.Count -ne 8){throw 'Dependency inventory differs'}
$dependencies|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $dest 'dependencies.json') -Encoding utf8
[IO.File]::WriteAllText((Join-Path $dest '.gitattributes'),"* -text`n")
@{date='2026-11-10';release=$false;sourceSdkMembers=128;testSources=60;fixtures=182;checks=3412;phases=89;outcomeSha256=$OutcomeSha256;thirdPartyBinariesIncluded=$false;files=$rows}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $dest 'manifest.json') -Encoding utf8
$all=@(Get-ChildItem -LiteralPath $dest -Recurse -File)
if(@($all|Where-Object Extension -in @('.dll','.exe','.pdb','.pfx','.p12','.snk','.key')).Count){throw 'Unexpected binary'}
$pattern='(?:ghp_[A-Za-z0-9]{36}|github_pat_[A-Za-z0-9_]{40,}|sk-proj-[A-Za-z0-9_-]{40,}|AKIA[A-Z0-9]{16}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----)'
foreach($file in $all|Where-Object Extension -notin @('.png','.gif','.jpg')){if(Select-String -LiteralPath $file.FullName -Pattern $pattern -Quiet){throw 'Potential secret found'}}
Git-Checked @('add','-f','--',$scope)|Out-Null
$stagedText=(Git-Checked @('diff','--cached','--name-only','-z')) -join "`n"
$staged=@($stagedText.Split([char]0,[StringSplitOptions]::RemoveEmptyEntries))
if($staged.Count -ne $all.Count -or @($staged|Where-Object {!$_.StartsWith($scope+'/',[StringComparison]::Ordinal)}).Count){throw 'Staging scope differs'}
$indexText=(Git-Checked @('ls-files','--stage','-z','--',$scope)) -join "`n"
foreach($line in $indexText.Split([char]0,[StringSplitOptions]::RemoveEmptyEntries)){
    if($line -notmatch '^100644 ([a-f0-9]{40}) 0\t(.+)$'){throw 'Unexpected index mode'}
    $expected=$Matches[1];$bytes=[IO.File]::ReadAllBytes((Join-Path $repo $Matches[2]))
    $hash=[Security.Cryptography.IncrementalHash]::CreateHash([Security.Cryptography.HashAlgorithmName]::SHA1)
    try{$hash.AppendData([Text.Encoding]::UTF8.GetBytes('blob '+$bytes.LongLength+"`0"));$hash.AppendData($bytes);if([Convert]::ToHexString($hash.GetHashAndReset()).ToLowerInvariant() -cne $expected){throw 'Raw staged bytes differ'}}finally{$hash.Dispose()}
}
Git-Checked @('-c','user.name=Elliot Williams','-c','user.email=58864955+elliot7williams@users.noreply.github.com','commit','-m','Back up guarded memory SDK source and exact 3412-check qualification')
Git-Checked @('push','origin','HEAD:refs/heads/codex/lea-source-backup-20261102-v1')
$head=(Git-Checked @('rev-parse','HEAD')).Trim();$remote=Git-Checked @('ls-remote','origin','refs/heads/codex/lea-source-backup-20261102-v1')
if(($remote -split '\s+')[0] -cne $head){throw 'Remote head differs'}
New-Item -ItemType Directory -Path $readback|Out-Null
$client=[Net.Http.HttpClient]::new();$client.Timeout=[TimeSpan]::FromSeconds(45);$verified=[Collections.Generic.List[object]]::new()
try{foreach($file in $all){$relative=[IO.Path]::GetRelativePath($repo,$file.FullName).Replace('\','/');$encoded=($relative.Split('/')|ForEach-Object {[uri]::EscapeDataString($_)}) -join '/';$url='https://raw.githubusercontent.com/elliot7williams/family-fun-friday-games-night/'+$head+'/'+$encoded;$bytes=$client.GetByteArrayAsync($url).GetAwaiter().GetResult();$sha=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes));if($sha -cne (Get-FileHash -LiteralPath $file.FullName).Hash){throw 'Remote bytes differ'};$verified.Add(@{path=$relative;sha256=$sha;bytes=$bytes.LongLength})}}finally{$client.Dispose()}
@{verified=$true;commit=$head;files=$verified;scope=$scope;release=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $readback 'result.json') -Encoding utf8
@{commit=$head;verifiedRemoteFiles=$verified.Count;proof=(Join-Path $readback 'result.json')}|ConvertTo-Json
