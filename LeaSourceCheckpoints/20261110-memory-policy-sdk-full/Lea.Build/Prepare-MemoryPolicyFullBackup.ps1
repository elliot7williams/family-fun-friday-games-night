$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\memory-policy-full-publisher-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve full-source publisher'}
$template=Join-Path $PSScriptRoot 'Publish-CurrentSdkFullCheckpoint.ps1'
$text=[IO.File]::ReadAllText($template)
$pairs=@(
 @('LeaSourceCheckpoints/20261109-sdk-source-full-suite','LeaSourceCheckpoints/20261110-memory-policy-sdk-full'),
 @('I:\LeaBuildStorage\sdk-source-full-preparation-20261109-v1','I:\LeaBuildStorage\memory-policy-full-preparation-20261110-v1'),
 @('I:\LeaBuildStorage\sdk-source-full-outcome-20261109-v1','I:\LeaBuildStorage\memory-policy-full-outcome-20261110-v1'),
 @('github-sdk-source-full-readback-20261109-v1','github-memory-policy-full-readback-20261110-v1'),
 @('8FB48CA5AF341D4943387F612210CD46AE7E2E1A75491C2CDFB9B1DFE6C3FCDB','F83A0473E23E89FFB5FEECFE39AD7F0252E97B404A688692D9F5F962979D6BC3'),
 @('5136533747D4B9753C2F954C09DD0F937356793D03AADA00C5FB14C657A6F9E3','77DFD17A6E8459260558AFA8A6D6429C0FB692F19C2E22E5C858EDB2E729E288'),
 @('I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1','I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1'),
 @('51A823667E96D3ED4C3EC526E0747B8C19B1EBBE59A8A05324503DBEB4FED9C9','21414F48C7A32E6B2ED0865CBA79A8DE3018A4B581075A7244E5ED534AB7E183'),
 @('git -c safe.directory=$repo -C $repo','git -c safe.directory=$repo -c maintenance.auto=false -C $repo'),
 @("date='2026-11-09'","date='2026-11-10'"),
 @('Back up current SDK source and complete matched 3412-check scripting campaign','Back up guarded memory SDK source and exact 3412-check qualification'),
 @("@('.dll','.exe','.pdb')","@('.dll','.exe','.pdb','.pfx','.p12','.snk','.key')")
)
foreach($p in $pairs){if([regex]::Matches($text,[regex]::Escape($p[0])).Count -ne 1 -or $text.Contains($p[1])){throw 'Publisher identity boundary differs'};$text=$text.Replace($p[0],$p[1])}
$old=@'
$staged=@(Git-Checked @('diff','--cached','--name-only'))
'@
$new=@'
$stagedText=(Git-Checked @('diff','--cached','--name-only','-z')) -join "`n"
$staged=@($stagedText.Split([char]0,[StringSplitOptions]::RemoveEmptyEntries))
'@
if(!$text.Contains($old)){throw 'Raw staged-path boundary differs'};$text=$text.Replace($old,$new)
$old="foreach(`$line in Git-Checked @('ls-files','--stage','--',`$scope)){"
$new=@'
$indexText=(Git-Checked @('ls-files','--stage','-z','--',$scope)) -join "`n"
foreach($line in $indexText.Split([char]0,[StringSplitOptions]::RemoveEmptyEntries)){
'@
if(!$text.Contains($old)){throw 'Raw index boundary differs'};$text=$text.Replace($old,$new)
$old="`$url='https://raw.githubusercontent.com/"
$new="`$encoded=(`$relative.Split('/')|ForEach-Object {[uri]::EscapeDataString(`$_)}) -join '/';`$url='https://raw.githubusercontent.com/"
if(!$text.Contains($old)){throw 'Remote URI boundary differs'};$text=$text.Replace($old,$new).Replace("+`$head+'/'+`$relative;`$bytes=","+`$head+'/'+`$encoded;`$bytes=")
$extra=@'
foreach($name in @('Lea.Engine/MemoryDocument.fragment.cs','Lea.Win/MemoryDocument.fragment.cs','Lea.Tests/MemoryDocumentMain.cs','Lea.Tests/MemoryDocumentViewMain.cs','Lea.Tests/MemoryScriptPolicyMain.cs','Lea.Build/Prepare-MemoryPolicySdk.ps1','Lea.Build/Audit-MemoryPolicySdk.ps1','Lea.Build/Test-CurrentSdkWorker.ps1','Lea.Build/Test-OwnedMemoryDocument.ps1','Lea.Build/Test-OwnedMemoryScriptPolicy.ps1','Lea.Build/Audit-MemoryScriptPolicy.ps1','Lea.Build/Test-OwnedSdkMemorySurface.ps1','Lea.Build/Resume-OwnedSdkMemoryPolicy.ps1','Lea.Build/Audit-SdkMemorySurface.ps1','Lea.Build/Prepare-MemoryPolicyFull.ps1','Lea.Build/Prepare-MemoryPolicyFullBackup.ps1','LeaForge/SDK_MEMORY_POLICY.md','LeaForge/MEMORY_SCRIPT_POLICY.md','LeaForge/MEMORY_DOCUMENTS.md')){Save (Join-Path $root $name) $name}
foreach($spec in @(
 @{folder='I:\LeaBuildStorage\memory-policy-sdk-audit-20261110-v1';prefix='sdk'},
 @{folder='I:\LeaBuildStorage\memory-policy-sdk-memory-audit-20261110-v1';prefix='sdk-memory'},
 @{folder='I:\LeaBuildStorage\memory-script-policy-audit-20261110-v1';prefix='policy-negative-positive'},
 @{folder='I:\LeaBuildStorage\memory-policy-full-source-20261110-v1';prefix='projection-failure'},
 @{folder='I:\LeaBuildStorage\memory-policy-full-source-20261110-v2';prefix='projection-source'}
)){foreach($file in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object Extension -in @('.json','.txt','.ps1','.cs','.md','.png')){Save $file.FullName ('evidence/'+$spec.prefix+'/'+$file.Name)}}
Save $PSCommandPath 'evidence/publish-memory-full.generated.ps1'
'@
$marker='$dependencies=@($source.files'
if([regex]::Matches($text,[regex]::Escape($marker)).Count -ne 1){throw 'Additional evidence insertion boundary differs'};$text=$text.Replace($marker,$extra+[Environment]::NewLine+$marker)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors);if($errors.Count){throw 'Generated publisher syntax invalid'}
New-Item -ItemType Directory -Path $out|Out-Null
$target=Join-Path $out 'Publish-MemoryPolicyFull.ps1';[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false));if([IO.File]::ReadAllText($target) -cne $text){throw 'Publisher readback differs'}
@{source=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;templateSha256=(Get-FileHash -LiteralPath $template).Hash;sourceOnly=$true;automaticMaintenance=$false;keyBinaryUploads=$false}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Write-Output $target
