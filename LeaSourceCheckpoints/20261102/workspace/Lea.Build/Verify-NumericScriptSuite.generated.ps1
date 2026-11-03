$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$buildPath=Join-Path $root 'numeric-references-script-suite-v1/result.json'
if((Get-FileHash -LiteralPath $buildPath).Hash -cne '8992F34CA977FA5DF8F65501E8495342F53D5538EC524682FC826B6B4C192071'){throw 'Build receipt changed'}
$b=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.files.Count -ne 65 -or $b.sources.Count -ne 60){throw 'Build incomplete'}
$out=Join-Path $root 'numeric-references-script-suite-v1/binding-verification.json'
if(Test-Path $out){throw 'Preserve binding verification'}
foreach($file in $b.files){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Runtime changed'}}
$expected=@{
    'Lea.Script.Win.dll'='AD4A057F5E82C8A36FA8362E54D1F746AD6264B9DF8D5447491DE001E34A59EA'
    'Lea.Engine.dll'='515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1'
    'Lea.Script.dll'='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
    'Lea.Win.dll'='E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B'
    'ScriptWorker/Lea.Script.Worker.dll'='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
    'ScriptWorker/Jint.dll'='67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'
    'WorkerEntryProbe/Lea.Script.dll'='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
}
foreach($name in $expected.Keys){if((Get-FileHash -LiteralPath (Join-Path $b.runtime $name)).Hash -cne $expected[$name]){throw 'Component pairing mismatch'}}
$bindings=@($b.relocation.componentBindings)+@(@{old=$b.relocation.oldManifestSha256;replacement=$b.relocation.newManifestSha256;name='entry'})
if($bindings.Count -ne 6){throw 'Dispatcher incomplete'}
$sourcePath=Join-Path $root 'numeric-references-script-suite-v1/source/Lea.Script.Tests/ModernComponents.cs'
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
$result=@{passed=$true;buildReceiptSha256=(Get-FileHash -LiteralPath $buildPath).Hash;runtimeFiles=65;sourceFiles=60;dispatcherDigests=6;dispatcherInverseVerified=$true;embeddedFixturePins=7;hostIlComparison=$il;pairing=$expected;suiteExecuted=$false;limitsChanged=$false}
[IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 6),[Text.UTF8Encoding]::new($false))
$result|ConvertTo-Json -Depth 6
(Get-FileHash -LiteralPath $out).Hash
