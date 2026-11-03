param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBuildReceipt)
$SuiteVersion=9
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$suite=Join-Path $root ('current-native-script-suite-v'+$SuiteVersion)
$buildPath=Join-Path $suite 'result.json'
if((Get-FileHash -LiteralPath $buildPath).Hash -cne $ExpectedBuildReceipt){throw 'Build receipt changed'}
$b=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
$sourceCount=if($SuiteVersion -in @(3,5)){62}else{60}
if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.files.Count -ne 65 -or $b.sources.Count -ne $sourceCount){throw 'Build incomplete'}
$out=Join-Path $suite 'binding-verification.json'
if($SuiteVersion -ge 2 -and !$b.classPaintContractInverseVerified){throw 'Contract inverse proof missing'}
if($SuiteVersion -in @(3,5) -and !$b.startupObservationInverseVerified){throw 'Observer inverse proof missing'}
if($SuiteVersion -ge 4 -and !$b.elementPaintContractInverseVerified){throw 'Receiver inverse proof missing'}
if($SuiteVersion -eq 5 -and $b.observedStartup -cne 'Tree'){throw 'Tree observation target differs'}
if($SuiteVersion -ge 6 -and (!$b.allStartupTraceRequested -or $b.startupObservationInverseVerified)){throw 'All-startup trace build shape differs'}
if($SuiteVersion -ge 7 -and !$b.inProcessStartupTraceInverseVerified){throw 'In-process trace inverse missing'}
if($SuiteVersion -ge 8 -and !$b.ownedRunRequested){throw 'Owned Run candidate proof missing'}
if(!$b.nativeSelectionView){throw 'Sx view build flag missing'}
if(Test-Path $out){throw 'Preserve binding verification'}
foreach($file in $b.files){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Runtime changed'}}
$expected=@{
    'Lea.Script.Win.dll'='AD4A057F5E82C8A36FA8362E54D1F746AD6264B9DF8D5447491DE001E34A59EA'
    'Lea.Engine.dll'='52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'
    'Lea.Script.dll'='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
    'Lea.Win.dll'='2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
    'ScriptWorker/Lea.Script.Worker.dll'='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
    'ScriptWorker/Jint.dll'='67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'
    'WorkerEntryProbe/Lea.Script.dll'='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
}
foreach($name in $expected.Keys){if((Get-FileHash -LiteralPath (Join-Path $b.runtime $name)).Hash -cne $expected[$name]){throw 'Component pairing mismatch'}}
$bindings=@($b.relocation.componentBindings)+@(@{old=$b.relocation.oldManifestSha256;replacement=$b.relocation.newManifestSha256;name='entry'})
if($bindings.Count -ne 6){throw 'Dispatcher incomplete'}
$sourcePath=Join-Path $suite 'source/Lea.Script.Tests/ModernComponents.cs'
$source=[IO.File]::ReadAllText($sourcePath)
foreach($binding in $bindings){
    if([regex]::Matches($source,$binding.replacement).Count -ne 1 -or $source.Contains($binding.old)){throw 'Dispatcher digest not uniquely rebound'}
    $manifest=if($binding.name -eq 'entry'){Join-Path $b.runtime 'WorkerEntryProbe/manifest.json'}else{Join-Path $b.runtime ('ComponentProbes/'+$binding.name+'/manifest.json')}
    if((Get-FileHash -LiteralPath $manifest).Hash -cne $binding.replacement){throw 'Manifest digest mismatch'}
    $spec=Get-Content -LiteralPath $manifest -Raw|ConvertFrom-Json -AsHashtable
    foreach($leaf in $spec.files.Keys){if((Get-FileHash -LiteralPath (Join-Path (Split-Path $manifest) $leaf)).Hash -cne $spec.files[$leaf]){throw 'Manifest file changed'}}
}
$inverse=$source
for($i=$bindings.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($bindings[$i].replacement,$bindings[$i].old)}
$baseline=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/source-cross-module-suite-v2'
if($inverse -cne [IO.File]::ReadAllText((Join-Path $baseline 'source/Lea.Script.Tests/ModernComponents.cs'))){throw 'Dispatcher changed beyond digests'}
Add-Type -Path @((Join-Path $root 'ReadEmbeddedEntryResource.cs'),(Join-Path $root 'EntryParentDigestAudit.cs'),(Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs'))
$resource=[ReadEmbeddedEntryResource]::Read($b.relocation.host)|ConvertFrom-Json -AsHashtable
if($resource.directory -cne (Join-Path $b.runtime 'WorkerEntryFixture') -or $resource.files.Count -ne 7){throw 'Embedded entry fixture wrong'}
foreach($leaf in $resource.files.Keys){if((Get-FileHash -LiteralPath (Join-Path $resource.directory $leaf)).Hash -cne $resource.files[$leaf]){throw 'Embedded fixture file mismatch'}}
$il=[EntryParentDigestAudit]::Compare((Join-Path $baseline 'runtime/WorkerEntryProbe/WorkerEntryNativeHost.exe'),$b.relocation.host)
$result=@{passed=$true;buildReceiptSha256=(Get-FileHash -LiteralPath $buildPath).Hash;runtimeFiles=65;sourceFiles=$sourceCount;dispatcherDigests=6;dispatcherInverseVerified=$true;embeddedFixturePins=7;hostIlComparison=$il;pairing=$expected;suiteExecuted=$false;limitsChanged=$false}
[IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 6),[Text.UTF8Encoding]::new($false))
$result|ConvertTo-Json -Depth 6
(Get-FileHash -LiteralPath $out).Hash
