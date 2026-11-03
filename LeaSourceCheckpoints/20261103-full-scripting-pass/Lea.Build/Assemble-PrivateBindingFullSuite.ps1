param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$EntrySha256,[ValidateRange(1,3)][int]$PrivateBindingVersion=1,[switch]$FinalizeAssets)
$ErrorActionPreference='Stop'
$preparedRoot='I:\LeaBuildStorage\private-binding-full-preparation-20261103-v1'
$entryRoot='I:\LeaBuildStorage\private-binding-full-entry-20261103-v'+$PrivateBindingVersion
$out='I:\LeaBuildStorage\private-binding-full-assembly-20261103-v'+$PrivateBindingVersion
if($FinalizeAssets){if($PrivateBindingVersion -lt 2){throw 'Asset binding requires rebuilt probes'};$out+='-assets'}
if(Test-Path -LiteralPath $out){throw 'Preserve existing private-binding assembly'}
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse assembly input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $hash){throw ('Assembly input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
try{
    $proof=Join-Path $preparedRoot 'preparation.json';Pin $proof 'D39F737A12DDBD7A4EEF01A25B7EFF06D7665F3EF493180F81A607C6CFA8F247'
    $prepared=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    $proof=Join-Path $entryRoot 'result.json';Pin $proof $EntrySha256
    $entry=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$prepared.verified -or !$prepared.notRunnable -or $prepared.files.Count -ne 305 -or $prepared.changes.Count -ne 17 -or $prepared.assertionsChanged -or !$prepared.fixturesUnchanged){throw 'Prepared assembly incomplete'}
    if(!$entry.passed -or !$entry.inputsUnchanged -or $entry.ownerAttention -or $entry.outputs.Count -ne 3 -or $entry.coreOutputs.Count -ne 3 -or !$entry.entryBinding.inverseVerified -or $entry.assertionsChanged -or $entry.parentSha256 -cne $prepared.parentSha256 -or $entry.workerSha256 -cne $prepared.workerSha256){throw 'Matching entry host incomplete'}
    $selected=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $prepared.files){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split('/') -contains '..' -or $selected.ContainsKey($file.path)){throw 'Invalid assembly member'}
        $path=Join-Path $preparedRoot $file.path;Pin $path $file.sha256
        $selected.Add($file.path,@{path=$file.path;origin=$path;sha256=$file.sha256})
    }
    $changes=[Collections.Generic.List[object]]::new()
    $probeChanges=[Collections.Generic.List[object]]::new()
    if($PrivateBindingVersion -ge 2){
        if($entry.privateBindingVersion -ne $PrivateBindingVersion -or $entry.probeOutputs.Count -ne 11){throw 'Rebuilt probe outputs missing'}
        $probeRoot=if($PrivateBindingVersion -eq 3){'I:\LeaBuildStorage\private-binding-component-build-20261103-v2'}else{'I:\LeaBuildStorage\private-binding-component-build-20261103-v1'}
        foreach($file in $entry.probeOutputs){
            Pin $file.path $file.sha256
            if([IO.Path]::GetFileName($file.path) -ceq 'ModernComponents.cs'){continue}
            $relative=[IO.Path]::GetRelativePath($probeRoot,$file.path).Replace('\','/')
            if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..' -or $relative.Split('/').Count -ne 2 -or $relative.Split('/')[0] -cnotin @('markers','frames','clock','wire','bootstrap')){throw 'Probe output path escape'}
            $relative='runtime/ComponentProbes/'+$relative
            if(!$selected.ContainsKey($relative)){throw 'Probe member missing from baseline'}
            $probeChanges.Add(@{path=$relative;originalSha256=$selected[$relative].sha256;sha256=$file.sha256})
            $selected[$relative]=@{path=$relative;origin=$file.path;sha256=$file.sha256}
        }
        if($probeChanges.Count -ne 10){throw 'Ten rebuilt probe products required'}
    }
    foreach($pair in @(@('WorkerEntryNativeHost.exe','runtime/WorkerEntryProbe/WorkerEntryNativeHost.exe'),@('entry-manifest.json','runtime/WorkerEntryProbe/manifest.json'),@('ModernComponents.cs','compiled-source/source/Lea.Script.Tests/ModernComponents.cs'))){
        $files=@($entry.outputs|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $pair[0]})
        if($files.Count -ne 1 -or !$selected.ContainsKey($pair[1])){throw 'Unique entry replacement required'}
        $file=$files[0];Pin $file.path $file.sha256
        $changes.Add(@{path=$pair[1];originalSha256=$selected[$pair[1]].sha256;sha256=$file.sha256})
        if($pair[0] -ceq 'ModernComponents.cs'){
            $old=[IO.File]::ReadAllText($selected[$pair[1]].origin);$new=[IO.File]::ReadAllText($file.path)
            if($PrivateBindingVersion -ge 2){$dispatchers=@($entry.probeOutputs|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'ModernComponents.cs'});if($dispatchers.Count -ne 1){throw 'Unique rebuilt dispatcher required'};$old=[IO.File]::ReadAllText($dispatchers[0].path)}
            if([regex]::Matches($old,$entry.entryBinding.old).Count -ne 1 -or [regex]::Matches($new,$entry.entryBinding.replacement).Count -ne 1 -or $new.Replace($entry.entryBinding.replacement,$entry.entryBinding.old) -cne $old){throw 'Dispatcher identity-only inverse differs'}
        }
        $selected[$pair[1]]=@{path=$pair[1];origin=$file.path;sha256=$file.sha256}
    }
    foreach($file in $entry.coreOutputs){
        $relative='runtime/WorkerEntryFixture/'+[IO.Path]::GetFileName($file.path)
        if(!$selected.ContainsKey($relative)){throw 'Unexpected entry core member'};Pin $file.path $file.sha256
        $changes.Add(@{path=$relative;originalSha256=$selected[$relative].sha256;sha256=$file.sha256})
        $selected[$relative]=@{path=$relative;origin=$file.path;sha256=$file.sha256}
    }
    $assets=[Collections.Generic.List[object]]::new()
    if($FinalizeAssets){
        $assetProof='I:\LeaBuildStorage\private-binding-source-20261102-v1/source.json';Pin $assetProof '9CD78B418ED5CE1216123B7093CD0CB3593EF0D7812414FF4D35D718CC266BA2'
        $assetSource=Get-Content -LiteralPath $assetProof -Raw|ConvertFrom-Json
        if(!$assetSource.verified -or $assetSource.files.Count -ne 2){throw 'Verified candidate assets required'}
        foreach($file in $assetSource.files){
            $relative='fixtures/Lea.Script/'+$file.name
            if($file.name -cnotin @('Dom.js','Dom.compact.js') -or !$selected.ContainsKey($relative) -or $selected[$relative].sha256 -cne $file.sdkOriginalSha256){throw 'Product asset baseline differs'}
            Pin $file.path $file.sha256
            $assets.Add(@{path=$relative;originalSha256=$selected[$relative].sha256;sha256=$file.sha256})
            $selected[$relative]=@{path=$relative;origin=$file.path;sha256=$file.sha256}
        }
    }
    if($changes.Count -ne 6 -or @($selected.Keys|Where-Object {$_.StartsWith('compiled-source/')} ).Count -ne 60 -or @($selected.Keys|Where-Object {$_.StartsWith('fixtures/')} ).Count -ne 182){throw 'Full suite source inventory differs'}
    New-Item -ItemType Directory -Path $out|Out-Null
    foreach($file in $selected.Values){
        $to=Join-Path $out $file.path;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
        Copy-Item -LiteralPath $file.origin -Destination $to
        if((Get-FileHash -LiteralPath $to).Hash -cne $file.sha256){throw 'Assembly readback differs'}
    }
    foreach($input in $inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Assembly input changed'}}
    @{verified=$true;privateBindingVersion=$PrivateBindingVersion;productAssetChanges=$assets;testFixturesUnchanged=$true;probeChanges=$probeChanges;files=@($selected.Values|Sort-Object path);inputs=$inputs;preparationChanges=$prepared.changes;entryChanges=$changes;parentSha256=$prepared.parentSha256;workerSha256=$prepared.workerSha256;compiledSources=60;fixtures=182;baselineTestBinariesExcluded=$true;assertionsChanged=$false;fixturesUnchanged=(!$FinalizeAssets);readyForSuiteCompilation=$true;compiled=$false;executed=$false;notRunnable=$true;requiredChecks=3412;requiredPhases=89;compileBudgetMs=120000;testBudgetMs=300000;cleanupBudgetMs=5000;pending=@('compile all60 sources','fresh NTFS stage/resource/closure audit','owned full3412/89 campaign');runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'assembly.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'assembly.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
