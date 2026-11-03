param([ValidateSet(2,3,4)][int]$Version=2)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$snapshot='I:\LeaBuildStorage\sdk-full-script-preparation-20261101-v1'
$out='I:\LeaBuildStorage\sdk-full-script-bindings-20261101-v'+$Version
if(Test-Path $out){throw 'Preserve SDK suite bindings'}
$receipt=Join-Path $snapshot 'preparation.json'
if((Get-FileHash $receipt).Hash -cne 'C54650F589334D3D58512FF46047EC053857DE1A35D03CA64A7AC86C35664FD2'){throw 'Suite snapshot differs'}
$inventory=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$inventory.verified -or !$inventory.notRunnable -or $inventory.compiledSources -ne 60 -or $inventory.fixtures -ne 182 -or $inventory.baselineRuntimeFiles -ne 65 -or $inventory.sdkReplacementFiles -ne 11){throw 'Suite snapshot coverage incomplete'}
$rows=[Collections.Generic.List[object]]::new();$changes=[Collections.Generic.List[object]]::new()
$components=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
if($Version -eq 3){
    $libraryProof='I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json'
    $nativeProof='I:\LeaBuildStorage\full-sdk-native-build-20261101-v1/result.json'
    if((Get-FileHash $libraryProof).Hash -cne '3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B' -or (Get-FileHash $nativeProof).Hash -cne '20E2EE1F8A87BFC2B14CD581E48B59D1DE834E87EBA729DCB5A45CCF4E1A7893'){throw 'Combined SDK component proofs differ'}
    $library=Get-Content $libraryProof -Raw|ConvertFrom-Json;$native=Get-Content $nativeProof -Raw|ConvertFrom-Json
    foreach($record in @($library,$native)){if(!$record.passed -or !$record.inputsUnchanged -or $record.ownerAttention){throw 'Combined SDK prerequisite incomplete'}}
    if($library.libraryProducts.Count -ne 6 -or $native.nativeProducts.Count -ne 6 -or !$native.workerAclRestored){throw 'Combined component closure incomplete'}
    foreach($file in $library.libraryProducts){$name=[IO.Path]::GetFileName($file.path);if($name -cne 'Lea.Print.dll'){$components.Add($name,$file)}}
    foreach($file in $native.nativeProducts){$components.Add(('ScriptWorker/'+[IO.Path]::GetFileName($file.path)),$file)}
    if($components.Count -ne 11){throw 'Exact eleven script campaign replacements required'}
}
if($Version -eq 4){
    $proof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json'
    if((Get-FileHash $proof).Hash -cne '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'){throw 'Deterministic build proof differs'}
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    $products=@($build.products|Where-Object {$_.path -in 'bin/Release/Lea.Engine.dll','bin/Release/Lea.Win.dll','bin/Release/Lea.Image.exe','bin/Release/Lea.Script.dll','bin/Release/Lea.Script.Win.dll' -or $_.path.StartsWith('bin/Release/ScriptWorker/')})
    if($products.Count -ne 11){throw 'Deterministic role closure incomplete'}
    foreach($p in $products){
        if(!$p.equal -or $p.sha256 -cne $p.secondSha256 -or (Get-FileHash $p.first).Hash -cne $p.sha256 -or (Get-FileHash $p.second).Hash -cne $p.sha256){throw 'Deterministic role copies differ'}
        $components.Add($p.path.Substring('bin/Release/'.Length),@{path=$p.first;sha256=$p.sha256})
    }
}
function SnapshotFile([string]$relative){
    if($Version -ge 3 -and $relative.StartsWith('sdk-components/')){
        $key=$relative.Substring('sdk-components/'.Length)
        if(!$components.ContainsKey($key)){throw 'Combined role absent'};$file=$components[$key]
        $item=Get-Item -LiteralPath $file.path
        while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Combined source reparse refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
        if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Combined source product differs'};return $file.path
    }
    $rows=@($inventory.files|Where-Object path -ceq $relative)
    if($rows.Count -ne 1){throw ('Snapshot member missing: '+$relative)}
    $path=Join-Path $snapshot $relative
    if((Get-FileHash $path).Hash -cne $rows[0].sha256){throw 'Snapshot member changed'}
    return $path
}
function Record([string]$relative){$path=Join-Path $out $relative;$rows.Add(@{path=$relative;sha256=(Get-FileHash $path).Hash})}
function CopySnapshot([string]$from,[string]$to){
    $path=SnapshotFile $from;$destination=Join-Path $out $to
    New-Item -ItemType Directory -Path (Split-Path $destination) -Force|Out-Null;Copy-Item $path $destination
    if((Get-FileHash $destination).Hash -cne (Get-FileHash $path).Hash){throw 'Binding copy differs'}
    Record $to
}
function ProjectIdentity([string]$text,$pairs){
    $original=$text
    foreach($pair in $pairs){
        if([regex]::Matches($text,[regex]::Escape($pair.old)).Count -ne 1 -or $text.Contains($pair.replacement)){throw 'Unique old/new identity required'}
        $text=$text.Replace($pair.old,$pair.replacement)
    }
    $inverse=$text
    for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i].replacement,$pairs[$i].old)}
    if($inverse -cne $original){throw 'Identity source inverse differs'}
    return $text
}
$oldWorker='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
$newWorker='F8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C'
$oldParent='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
$newParent='906E3CAAE4C160C7FDCF9D7E79B08076B9A12F46A76715AE02CAE9E72983764C'
if($Version -eq 3){
    $newWorker='5BF8B589731CB1A0A5DE38B0DAE1AF516DBF21DB5982141C9C013C55E6D39596'
    $newParent='36AB8810EA87DDC6D3A840B629772210408F1ABEAF7341E204442E1275D79304'
    if($components['ScriptWorker/Lea.Script.Worker.dll'].sha256 -cne $newWorker -or $components['Lea.Script.dll'].sha256 -cne $newParent){throw 'Selected combined identities differ'}
}
$manifestBindings=[Collections.Generic.List[object]]::new()
if($Version -eq 4){$newWorker=$components['ScriptWorker/Lea.Script.Worker.dll'].sha256;$newParent=$components['Lea.Script.dll'].sha256}
foreach($name in @('markers','frames','clock','wire','bootstrap')){
    $relative='baseline-runtime/ComponentProbes/'+$name+'/manifest.json';$path=SnapshotFile $relative
    $spec=Get-Content $path -Raw|ConvertFrom-Json -AsHashtable
    if($spec.files['Lea.Script.Worker.dll'] -cne $oldWorker -or $spec.checks -le 0 -or $spec.checks -gt 256){throw 'Original component binding differs'}
    $oldHash=(Get-FileHash $path).Hash
    foreach($leaf in @($spec.files.Keys)){
        if([IO.Path]::GetFileName($leaf) -cne $leaf){throw 'Component leaf escape'}
        $from=if($leaf -ceq 'Lea.Script.Worker.dll'){'sdk-components/ScriptWorker/'+$leaf}else{'baseline-runtime/ComponentProbes/'+$name+'/'+$leaf}
        $to='runtime/ComponentProbes/'+$name+'/'+$leaf;CopySnapshot $from $to
        if($leaf -ceq 'Lea.Script.Worker.dll'){$spec.files[$leaf]=$newWorker}
        if((Get-FileHash (Join-Path $out $to)).Hash -cne $spec.files[$leaf]){throw 'Component runtime differs'}
    }
    $to='runtime/ComponentProbes/'+$name+'/manifest.json'
    $spec|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out $to) -Encoding utf8;Record $to
    $manifestBindings.Add(@{name=$name;old=$oldHash;replacement=(Get-FileHash (Join-Path $out $to)).Hash;checks=$spec.checks})
}
foreach($name in @('CoreProbeBridge.cs','WorkerEntryBridge.cs','ModernComponents.cs')){
    $path=SnapshotFile ('compiled-source/source/Lea.Script.Tests/'+$name)
    $text=[IO.File]::ReadAllText($path)
    $pairs=[Collections.Generic.List[object]]::new()
    if($name -ceq 'ModernComponents.cs'){foreach($binding in $manifestBindings){$pairs.Add(@{old=$binding.old;replacement=$binding.replacement})}}else{$pairs.Add(@{old=$oldWorker;replacement=$newWorker})}
    $updated=ProjectIdentity $text $pairs
    $negative=0
    foreach($bad in @($text.Replace($pairs[0].old,'MISSING_IDENTITY'),($text+$text),$updated)){
        $denied=$false;try{$null=ProjectIdentity $bad $pairs}catch{$denied=$true}
        if(!$denied){throw 'Identity projection negative control failed'};$negative++
    }
    $to='source/'+$name;$destination=Join-Path $out $to;New-Item -ItemType Directory -Path (Split-Path $destination) -Force|Out-Null
    [IO.File]::WriteAllText($destination,$updated,[Text.UTF8Encoding]::new($false));Record $to
    $changes.Add(@{source=$name;originalSha256=(Get-FileHash $path).Hash;candidateSha256=(Get-FileHash $destination).Hash;inverseVerified=$true;negativeControls=$negative;identityReplacements=$pairs.Count})
}
$entry=Get-Content (SnapshotFile 'baseline-runtime/WorkerEntryProbe/manifest.json') -Raw|ConvertFrom-Json -AsHashtable
if($entry.fixtureFiles.Count -ne 7 -or $entry.files['Lea.Script.dll'] -cne $oldParent){throw 'Original entry fixture differs'}
foreach($leaf in @($entry.fixtureFiles.Keys)){
    if([IO.Path]::GetFileName($leaf) -cne $leaf){throw 'Entry leaf escape'}
    $from=if($leaf.StartsWith('Lea.Script.Worker.',[StringComparison]::Ordinal)){'sdk-components/ScriptWorker/'+$leaf}else{'baseline-runtime/WorkerEntryFixture/'+$leaf}
    CopySnapshot $from ('runtime/WorkerEntryFixture/'+$leaf)
    $entry.fixtureFiles[$leaf]=(Get-FileHash (Join-Path $out ('runtime/WorkerEntryFixture/'+$leaf))).Hash
}
CopySnapshot 'sdk-components/Lea.Script.dll' 'runtime/WorkerEntryProbe/Lea.Script.dll'
CopySnapshot 'baseline-runtime/WorkerEntryProbe/WorkerEntryNativeHost.exe.config' 'runtime/WorkerEntryProbe/WorkerEntryNativeHost.exe.config'
$oldSuite=Join-Path $root 'continuity-evidence/temporal-startup-v1/relative-font-script-suite-v1'
$source=Join-Path $oldSuite 'WorkerEntryNativeHost.cs'
if((Get-FileHash $source).Hash -cne '66ABE045C4F0C79B0CD0C114743FE92AFC9C0B579D9E4AF9D47F73F0C88B0183'){throw 'Entry host source differs'}
$text=[IO.File]::ReadAllText($source);$entryPairs=[Collections.Generic.List[object]]::new();$entryPairs.Add(@{old=$oldParent;replacement=$newParent});$updated=ProjectIdentity $text $entryPairs
if($Version -ge 3){
    $negative=0;foreach($bad in @($text.Replace($oldParent,'MISSING_IDENTITY'),($text+$text),$updated)){try{$null=ProjectIdentity $bad $entryPairs}catch{$negative++}}
    if($negative -ne 3){throw 'Entry host parent projection negative controls failed'}
}
$to='source/WorkerEntryNativeHost.cs';[IO.File]::WriteAllText((Join-Path $out $to),$updated,[Text.UTF8Encoding]::new($false));Record $to
$changes.Add(@{source='WorkerEntryNativeHost.cs';originalSha256=(Get-FileHash $source).Hash;candidateSha256=(Get-FileHash (Join-Path $out $to)).Hash;inverseVerified=$true;identityReplacements=1})
$futureRuntime=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v1/runtime'
if($Version -eq 4){$futureRuntime=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v8/runtime'}
@{files=$entry.fixtureFiles;directory=(Join-Path $futureRuntime 'WorkerEntryFixture')}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'entry-fixture.json') -Encoding utf8;Record 'entry-fixture.json'
foreach($row in $rows){if((Get-FileHash (Join-Path $out $row.path)).Hash -cne $row.sha256){throw 'Binding output readback differs'}}
@{verified=$true;notRunnable=$true;files=$rows;sourceChanges=$changes;componentBindings=$manifestBindings;componentManifestsRebound=5;workerHashConstantsRebound=2;entryFixtureFiles=7;futureRuntime=$futureRuntime;entryHostCompilationPending=$true;entryManifestRebindingPending=$true;assertionsChanged=$false;fullSuiteCompiled=$false;executed=$false;requiredChecks=3412;requiredPhases=89;compileBudgetMs=120000;testBudgetMs=300000;cleanupBudgetMs=5000}|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'bindings.json') -Encoding utf8
Get-FileHash (Join-Path $out 'bindings.json')
