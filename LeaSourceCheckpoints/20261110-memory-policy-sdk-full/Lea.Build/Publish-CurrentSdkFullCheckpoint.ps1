param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$OutcomeSha256)
$ErrorActionPreference='Stop'
$root='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy'
$repo='I:\LeaBuildStorage\github-source-checkpoint-20261109-v1'
$scope='LeaSourceCheckpoints/20261109-sdk-source-full-suite'
$dest=Join-Path $repo $scope
$prep='I:\LeaBuildStorage\sdk-source-full-preparation-20261109-v1'
$auditDir='I:\LeaBuildStorage\sdk-source-full-outcome-20261109-v1'
$readback='I:\LeaBuildStorage\github-sdk-source-full-readback-20261109-v1'
function Git-Checked([string[]]$Arguments){$value=& git -c safe.directory=$repo -C $repo @Arguments;if($LASTEXITCODE){throw 'Git operation failed'};return $value}
if((Git-Checked @('branch','--show-current')) -cne 'codex/lea-source-backup-20261102-v1' -or (Git-Checked @('remote','get-url','origin')) -cne 'https://github.com/elliot7williams/family-fun-friday-games-night.git' -or (Git-Checked @('status','--porcelain'))){throw 'Clean intended backup checkout required'}
if((Test-Path -LiteralPath $dest) -or (Test-Path -LiteralPath $readback)){throw 'Preserve existing checkpoint'}
$auditPath=Join-Path $auditDir 'audit.json'
if((Get-FileHash -LiteralPath $auditPath).Hash -cne $OutcomeSha256){throw 'Outcome audit differs'}
$audit=Get-Content -LiteralPath $auditPath -Raw|ConvertFrom-Json
if(!$audit.observationVerified -or !$audit.fullSuitePassed -or $audit.completedPhases -ne 89 -or !$audit.runtimeBytesAndAclUnchanged -or !$audit.traceEnvironmentRestored -or $audit.quarantineRequired -or $audit.assertionsChanged -or $audit.limitsChanged -or $audit.preparationSha256 -cne '8FB48CA5AF341D4943387F612210CD46AE7E2E1A75491C2CDFB9B1DFE6C3FCDB' -or $audit.runSha256 -cne '5136533747D4B9753C2F954C09DD0F937356793D03AADA00C5FB14C657A6F9E3'){throw 'Exact passing full-suite checkpoint required'}
$sourceRoot='I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1'
$sourcePath=Join-Path $sourceRoot 'source.json'
if((Get-FileHash -LiteralPath $sourcePath).Hash -cne '51A823667E96D3ED4C3EC526E0747B8C19B1EBBE59A8A05324503DBEB4FED9C9'){throw 'Source export differs'}
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
$dependencies=@($source.files|Where-Object {$_.path -match '^lib/'})
if($dependencies.Count -ne 8){throw 'Dependency inventory differs'}
$dependencies|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $dest 'dependencies.json') -Encoding utf8
[IO.File]::WriteAllText((Join-Path $dest '.gitattributes'),"* -text`n")
@{date='2026-11-09';release=$false;sourceSdkMembers=128;testSources=60;fixtures=182;checks=3412;phases=89;outcomeSha256=$OutcomeSha256;thirdPartyBinariesIncluded=$false;files=$rows}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $dest 'manifest.json') -Encoding utf8
$all=@(Get-ChildItem -LiteralPath $dest -Recurse -File)
if(@($all|Where-Object Extension -in @('.dll','.exe','.pdb')).Count){throw 'Unexpected binary'}
$pattern='(?:ghp_[A-Za-z0-9]{36}|github_pat_[A-Za-z0-9_]{40,}|sk-proj-[A-Za-z0-9_-]{40,}|AKIA[A-Z0-9]{16}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----)'
foreach($file in $all|Where-Object Extension -notin @('.png','.gif','.jpg')){if(Select-String -LiteralPath $file.FullName -Pattern $pattern -Quiet){throw 'Potential secret found'}}
Git-Checked @('add','-f','--',$scope)|Out-Null
$staged=@(Git-Checked @('diff','--cached','--name-only'))
if($staged.Count -ne $all.Count -or @($staged|Where-Object {!$_.StartsWith($scope+'/',[StringComparison]::Ordinal)}).Count){throw 'Staging scope differs'}
foreach($line in Git-Checked @('ls-files','--stage','--',$scope)){
    if($line -notmatch '^100644 ([a-f0-9]{40}) 0\t(.+)$'){throw 'Unexpected index mode'}
    $expected=$Matches[1];$bytes=[IO.File]::ReadAllBytes((Join-Path $repo $Matches[2]))
    $hash=[Security.Cryptography.IncrementalHash]::CreateHash([Security.Cryptography.HashAlgorithmName]::SHA1)
    try{$hash.AppendData([Text.Encoding]::UTF8.GetBytes('blob '+$bytes.LongLength+"`0"));$hash.AppendData($bytes);if([Convert]::ToHexString($hash.GetHashAndReset()).ToLowerInvariant() -cne $expected){throw 'Raw staged bytes differ'}}finally{$hash.Dispose()}
}
Git-Checked @('-c','user.name=Elliot Williams','-c','user.email=58864955+elliot7williams@users.noreply.github.com','commit','-m','Back up current SDK source and complete matched 3412-check scripting campaign')
Git-Checked @('push','origin','HEAD:refs/heads/codex/lea-source-backup-20261102-v1')
$head=(Git-Checked @('rev-parse','HEAD')).Trim();$remote=Git-Checked @('ls-remote','origin','refs/heads/codex/lea-source-backup-20261102-v1')
if(($remote -split '\s+')[0] -cne $head){throw 'Remote head differs'}
New-Item -ItemType Directory -Path $readback|Out-Null
$client=[Net.Http.HttpClient]::new();$client.Timeout=[TimeSpan]::FromSeconds(45);$verified=[Collections.Generic.List[object]]::new()
try{foreach($file in $all){$relative=[IO.Path]::GetRelativePath($repo,$file.FullName).Replace('\','/');$url='https://raw.githubusercontent.com/elliot7williams/family-fun-friday-games-night/'+$head+'/'+$relative;$bytes=$client.GetByteArrayAsync($url).GetAwaiter().GetResult();$sha=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes));if($sha -cne (Get-FileHash -LiteralPath $file.FullName).Hash){throw 'Remote bytes differ'};$verified.Add(@{path=$relative;sha256=$sha;bytes=$bytes.LongLength})}}finally{$client.Dispose()}
@{verified=$true;commit=$head;files=$verified;scope=$scope;release=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $readback 'result.json') -Encoding utf8
@{commit=$head;verifiedRemoteFiles=$verified.Count;proof=(Join-Path $readback 'result.json')}|ConvertTo-Json
