$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$archiveRoot=Join-Path $workspace 'continuity-evidence/tested-interpreter-inputs-v1'
$outRoot=Join-Path $workspace 'continuity-evidence/interpreter-ordered-map-v1'
if(Test-Path -LiteralPath $outRoot){throw 'Preserve ordered map'}
$archive=$archiveRoot+'.zip'
if((Get-FileHash -LiteralPath $archive).Hash -cne 'E8F3D3CDF7F25589A8E7FDBFEEC15732C46310444147648C7F8DF0EF84F58E1C'){throw 'Input archive changed'}
$buildPath=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/build-v1/result.json'
$sourcePath=Join-Path $workspace 'continuity-evidence/modern-worker-v1/interpreter-source-v1/result.json'
$generationPath=Join-Path $workspace 'continuity-evidence/modern-worker-v1/interpreter-generation-check-v5/result.json'
foreach($pair in @(@($buildPath,'7D82A029AA5977798CF8B323920D606E21DFE4D803243914378EC1CAD8761D29'),@($sourcePath,'D013E5CCEDED3C5CC52C94EBE87E3E9CF2A4629CD4DCE750A6B234BCF06F3E28'),@($generationPath,'54E2A86AFC4ED2E12E757EE5A4BEBBF84A7A48C591EE6C3A8FEA5CF2C9C7A8C0'))){if((Get-FileHash -LiteralPath $pair[0]).Hash -cne $pair[1]){throw 'Provenance changed'}}
$build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
$snapshot=Get-Content -LiteralPath $sourcePath -Raw|ConvertFrom-Json
$generation=Get-Content -LiteralPath $generationPath -Raw|ConvertFrom-Json
$inventory=Get-Content -LiteralPath (Join-Path $archiveRoot 'manifest.json') -Raw|ConvertFrom-Json
$known=@{};foreach($file in $build.inputs){$full=[IO.Path]::GetFullPath($file.path);if($known.ContainsKey($full) -and $known[$full] -cne $file.sha256){throw 'Conflicting input pin'};$known[$full]=$file.sha256}
$archived=@{};foreach($file in $inventory.files){$archived[[IO.Path]::GetFullPath($file.originalPath)]=$file}
function ResolveInput([string]$Path){
    $full=[IO.Path]::GetFullPath($Path)
    if(!$known.ContainsKey($full) -or !$archived.ContainsKey($full)){throw "Missing archived build pin: $full"}
    $record=$archived[$full]
    $local=[IO.Path]::GetFullPath((Join-Path $archiveRoot $record.archivePath))
    if(!$local.StartsWith($archiveRoot+'\',[StringComparison]::OrdinalIgnoreCase) -or $record.sha256 -cne $known[$full] -or (Get-FileHash -LiteralPath $local).Hash -cne $known[$full]){throw 'Archive identity/path mismatch'}
    return @{originalPath=$full;archivePath=$record.archivePath;sha256=$known[$full]}
}
$modern=Join-Path $workspace 'continuity-evidence/modern-worker-v1'
$sourceRoot=Join-Path $modern 'interpreter-source-v1/source'
$selected=[Collections.Generic.List[object]]::new();$documentation=0;$timezone=0
foreach($file in $snapshot.files|Where-Object{$_.relative.StartsWith('Jint/',[StringComparison]::Ordinal) -and $_.relative.EndsWith('.cs',[StringComparison]::Ordinal)}|Sort-Object relative){
    $path=Join-Path $sourceRoot $file.relative
    $overlay=Join-Path $modern ('interpreter-generation-check-v5/doc-overlay/'+$file.relative.Substring(5))
    if($archived.ContainsKey([IO.Path]::GetFullPath($overlay))){$path=$overlay;$documentation++}
    if($file.relative -ceq 'Jint/Native/Temporal/DefaultTimeZoneProvider.cs'){$path=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/DefaultTimeZoneProvider.cs';$timezone++}
    $entry=ResolveInput $path;$entry['order']=$selected.Count;$selected.Add($entry)
}
if($selected.Count -ne 586 -or $documentation -ne 3 -or $timezone -ne 1){throw 'Upstream/overlay coverage differs'}
$entry=ResolveInput (Join-Path $modern 'InterpreterRuntimeUsings.cs');$entry['order']=$selected.Count;$selected.Add($entry)
foreach($file in $generation.generated|Sort-Object name){
    $entry=ResolveInput (Join-Path $modern ('interpreter-generation-check-v5/generated/'+$file.name))
    if($entry.sha256 -cne $file.sha256){throw 'Generator provenance differs'}
    $entry['order']=$selected.Count;$selected.Add($entry)
}
if($selected.Count -ne 698 -or @($selected.archivePath|Sort-Object -Unique).Count -ne 698 -or $generation.generated.Count -ne 111){throw 'Ordered source count/uniqueness differs'}
$refRoot=[IO.Path]::GetFullPath('C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.32\ref\net8.0')
$references=@($known.Keys|Where-Object{[IO.Path]::GetDirectoryName($_) -ceq $refRoot -and [IO.Path]::GetExtension($_) -ceq '.dll'}|Sort-Object {[IO.Path]::GetFileName($_)}|ForEach-Object{@{path=$_;sha256=$known[$_]}})
$parser=Join-Path $modern 'apphost-v5/Acornima.dll'
if(!$known.ContainsKey($parser) -or $known[$parser] -cne 'A11417B8542B4E55330E751FCA44DA5CBDE1D95E79B1519C2ABA59BE7A7B941C'){throw 'Compiler parser reference differs'}
$references+=@{path=$parser;sha256=$known[$parser]}
foreach($reference in $references){if((Get-FileHash -LiteralPath $reference.path).Hash -cne $reference.sha256){throw 'Reference unavailable/changed'}}
$compiler=ResolveInput (Join-Path $workspace 'continuity-evidence/temporal-startup-v1/build-v1/TemporalStartupCompiler.cs')
$key=ResolveInput (Join-Path $sourceRoot 'Jint/Jint.snk')
$resource=ResolveInput (Join-Path $sourceRoot 'Jint/Native/Intl/Data/LikelySubtags.txt')
$null=New-Item -ItemType Directory -Path $outRoot
$result=@{orderedSources=$selected;references=$references;compiler=$compiler;publicSigningKey=$key;embeddedResource=$resource;sourceCount=698;emittedTreeCount=699;documentationOverlays=3;timezoneOverlay=1;generatedSources=111;archiveSha256=(Get-FileHash -LiteralPath $archive).Hash;scope='Reconstructed ordered lazy-interpreter input map using retained baseline selection and pinned overlays; map verified, not yet rebuilt or portable without pinned external references';compiled=$false;releaseCertified=$false}
[IO.File]::WriteAllText((Join-Path $outRoot 'build-map.json'),($result|ConvertTo-Json -Depth 8))
[pscustomobject]@{sources=$selected.Count;references=$references.Count;documentationOverlays=$documentation;timezoneOverlays=$timezone;compiled=$false;sha256=(Get-FileHash -LiteralPath (Join-Path $outRoot 'build-map.json')).Hash}|ConvertTo-Json
