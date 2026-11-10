param([switch]$PrintSourceGraph)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$repo='I:\LeaBuildStorage\github-source-checkpoint-20261109-v1'
$tag=if($PrintSourceGraph){'print'}else{'image'}
$scope='LeaSourceCheckpoints/20261109-sdk-'+$tag+'-source-graph'
$dest=Join-Path $repo $scope
$graph='I:\LeaBuildStorage\sdk-'+$tag+'-source-graph-20261109-v1'
$auditDir='I:\LeaBuildStorage\sdk-'+$tag+'-source-graph-audit-20261109-v1'
$proofDir='I:\LeaBuildStorage\github-sdk-'+$tag+'-source-readback-20261109-v1'
function Git-Checked([string[]]$Arguments){$value=& git -c safe.directory=$repo -C $repo @Arguments;if($LASTEXITCODE){throw ('Git failed: '+($Arguments -join ' '))};return $value}
if((Git-Checked @('branch','--show-current')) -cne 'codex/lea-source-backup-20261102-v1'){throw 'Wrong backup branch'}
if((Git-Checked @('remote','get-url','origin')) -cne 'https://github.com/elliot7williams/family-fun-friday-games-night.git'){throw 'Wrong remote'}
if(Git-Checked @('status','--porcelain')){throw 'Backup checkout must be clean'}
if((Test-Path -LiteralPath $dest) -or (Test-Path -LiteralPath $proofDir)){throw 'Preserve existing checkpoint'}
$auditPath=Join-Path $auditDir 'audit.json'
$auditPin=if($PrintSourceGraph){'24A2A32A5FAAB645E3A44E7915725AC4EB9C42CC6CE6DC9C56DDEDEB201A8E03'}else{'6C654D97E50047ABEE9BB2F3FA6C3030B0732B0596F74127E4572B644DE12A4E'}
if((Get-FileHash -LiteralPath $auditPath).Hash -cne $auditPin){throw 'Audit differs'}
$audit=Get-Content -LiteralPath $auditPath -Raw|ConvertFrom-Json
$sourceCount=if($PrintSourceGraph){98}else{75};$runCount=if($PrintSourceGraph){8}else{11}
if(!$audit.verified -or $audit.sourceFiles.Count -ne $sourceCount -or $audit.ownedRuns -ne $runCount){throw 'Audit scope differs'}
$rows=[Collections.Generic.List[object]]::new()
function Save([string]$From,[string]$Relative,[string]$Expected=''){
    if([IO.Path]::IsPathRooted($Relative) -or $Relative.Contains(':') -or $Relative.Split('/') -contains '..'){throw 'Unsafe destination'}
    $hash=(Get-FileHash -LiteralPath $From).Hash
    if($Expected -and $hash -cne $Expected){throw ('Input differs: '+$From)}
    $target=Join-Path $dest $Relative
    New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force|Out-Null
    if(Test-Path -LiteralPath $target){throw 'Duplicate checkpoint member'}
    Copy-Item -LiteralPath $From -Destination $target
    if((Get-FileHash -LiteralPath $target).Hash -cne $hash){throw 'Backup copy differs'}
    $rows.Add(@{path=$Relative;sha256=$hash;bytes=(Get-Item -LiteralPath $target).Length})
}
foreach($file in $audit.sourceFiles){Save (Join-Path $graph ('source/'+$file.path)) ('framework-source/'+$file.path) $file.sha256}
foreach($relative in @('PROJECT_CONTEXT.md','Lea.Sdk/EmbeddingSample.csproj','Lea.Sdk/Lea.Image.csproj','Lea.Sdk/SOURCE_GRAPH.md','Lea.Sdk/SDK_FULL.md','Lea.Build/Test-SdkImageSourceGraph.ps1','Lea.Build/Test-DecoderRetirement.ps1','Lea.Build/Audit-SdkImageSourceGraph.ps1','Lea.Build/Publish-SdkImageSourceCheckpoint.ps1','Lea.Tests/SdkImageViewMain.cs','Lea.Tests/DecoderRetirementMain.cs','LeaForge/SDK_IMAGE_WORKER.md','LeaForge/SDK_SOURCE_GRAPH.md','LeaForge/PROGRESS.md','LeaForge/SOURCE_BACKUPS.md')){Save (Join-Path $root $relative) $relative}
if($PrintSourceGraph){foreach($relative in @('Lea.Sdk/Lea.Print.csproj','Lea.Print/README.md','Lea.Build/Test-SdkPrintSourceGraph.ps1','Lea.Build/Audit-SdkPrintSourceGraph.ps1','LeaForge/SDK_PRINT.md')){Save (Join-Path $root $relative) $relative}}
foreach($file in Get-ChildItem -LiteralPath $auditDir -File){Save $file.FullName ('evidence/audit/'+$file.Name)}
if(!$PrintSourceGraph){
    $failure='I:\LeaBuildStorage\sdk-image-source-graph-audit-failure-20261109-v1'
    foreach($file in Get-ChildItem -LiteralPath $failure -File){Save $file.FullName ('evidence/initial-audit-stop/'+$file.Name)}
}
$artifacts=if($PrintSourceGraph){$audit.artifacts}else{$audit.screenshots}
foreach($file in $artifacts){Save $file.path ('evidence/artifacts/'+[IO.Path]::GetFileName($file.path)) $file.sha256}
$dependencies=@(Get-ChildItem -LiteralPath (Join-Path $graph 'source/lib') -File|ForEach-Object {@{name=$_.Name;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;bytes=$_.Length}})
if($dependencies.Count -ne 6){throw 'Dependency inventory differs'}
$dependencies|ConvertTo-Json -Depth 4|Set-Content -LiteralPath (Join-Path $dest 'dependencies.json') -Encoding utf8
[IO.File]::WriteAllText((Join-Path $dest '.gitattributes'),"* -text`n")
@{date='2026-11-09';release=$false;frameworkSourceMembers=$sourceCount;focusedChecks=$(if($PrintSourceGraph){438}else{151});capturePassed=[bool]$audit.capturePassed;ownedRuns=$runCount;auditSha256=(Get-FileHash -LiteralPath $auditPath).Hash;thirdPartyBinariesIncluded=$false;files=$rows}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $dest 'manifest.json') -Encoding utf8
$all=@(Get-ChildItem -LiteralPath $dest -Recurse -File)
if(@($all|Where-Object Extension -in @('.dll','.exe','.pdb')).Count){throw 'Unexpected binary'}
$pattern='(?:ghp_[A-Za-z0-9]{36}|github_pat_[A-Za-z0-9_]{40,}|sk-proj-[A-Za-z0-9_-]{40,}|AKIA[A-Z0-9]{16}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----)'
foreach($file in $all|Where-Object Extension -ne '.png'){if(Select-String -LiteralPath $file.FullName -Pattern $pattern -Quiet){throw ('Potential secret: '+$file.FullName)}}
Git-Checked @('add','-f','--',$scope)|Out-Null
$staged=@(Git-Checked @('diff','--cached','--name-only'))
if($staged.Count -ne $all.Count -or @($staged|Where-Object {!$_.StartsWith($scope+'/',[StringComparison]::Ordinal)}).Count){throw 'Staging scope differs'}
foreach($line in Git-Checked @('ls-files','--stage','--',$scope)){
    if($line -notmatch '^100644 ([a-f0-9]{40}) 0\t(.+)$'){throw 'Unexpected index mode'}
    $expected=$Matches[1];$path=$Matches[2];$bytes=[IO.File]::ReadAllBytes((Join-Path $repo $path))
    $hash=[Security.Cryptography.IncrementalHash]::CreateHash([Security.Cryptography.HashAlgorithmName]::SHA1)
    try{$hash.AppendData([Text.Encoding]::UTF8.GetBytes('blob '+$bytes.LongLength+"`0"));$hash.AppendData($bytes);if([Convert]::ToHexString($hash.GetHashAndReset()).ToLowerInvariant() -cne $expected){throw 'Raw staged bytes differ'}}finally{$hash.Dispose()}
}
$message=if($PrintSourceGraph){'Back up direct print source integration and exact runtime qualification evidence'}else{'Back up source-built embedding image deployment and exact lifecycle evidence'}
Git-Checked @('-c','user.name=Elliot Williams','-c','user.email=58864955+elliot7williams@users.noreply.github.com','commit','-m',$message)
Git-Checked @('push','origin','HEAD:refs/heads/codex/lea-source-backup-20261102-v1')
$head=(Git-Checked @('rev-parse','HEAD')).Trim()
$remote=Git-Checked @('ls-remote','origin','refs/heads/codex/lea-source-backup-20261102-v1')
if(($remote -split '\s+')[0] -cne $head){throw 'Remote head differs'}
New-Item -ItemType Directory -Path $proofDir|Out-Null
$client=[Net.Http.HttpClient]::new();$client.Timeout=[TimeSpan]::FromSeconds(45)
$verified=[Collections.Generic.List[object]]::new()
try{
    foreach($file in $all){
        $relative=[IO.Path]::GetRelativePath($repo,$file.FullName).Replace('\','/')
        $url='https://raw.githubusercontent.com/elliot7williams/family-fun-friday-games-night/'+$head+'/'+$relative
        $bytes=$client.GetByteArrayAsync($url).GetAwaiter().GetResult()
        $sha=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
        if($sha -cne (Get-FileHash -LiteralPath $file.FullName).Hash){throw ('Remote bytes differ: '+$relative)}
        $verified.Add(@{path=$relative;sha256=$sha;bytes=$bytes.LongLength})
    }
}finally{$client.Dispose()}
@{verified=$true;commit=$head;files=$verified;scope=$scope;release=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $proofDir 'result.json') -Encoding utf8
@{commit=$head;verifiedRemoteFiles=$verified.Count;proof=(Join-Path $proofDir 'result.json')}|ConvertTo-Json
