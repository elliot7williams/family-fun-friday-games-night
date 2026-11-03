$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\sdk-full-script-preparation-20261101-v1'
if(Test-Path $out){throw 'Preserve full suite preparation'}
$rows=[Collections.Generic.List[object]]::new()
function Stage([string]$from,[string]$relative,[string]$pin){
    if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('/') -contains '..'){throw 'Suite preparation path escape'}
    $item=Get-Item -LiteralPath $from
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Suite reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    if((Get-FileHash $from).Hash -cne $pin){throw ('Suite preparation input differs: '+$relative)}
    $to=Join-Path $out $relative;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
    Copy-Item $from $to
    if((Get-FileHash $to).Hash -cne $pin){throw 'Suite preparation copy differs'}
    $rows.Add(@{path=$relative;sha256=$pin;origin=$from})
}
function ReadProof([string]$relative,[string]$pin,[string]$name){
    $path=Join-Path $root $relative;Stage $path ('evidence/'+$name) $pin
    return Get-Content $path -Raw|ConvertFrom-Json
}
$suite=Join-Path $root 'continuity-evidence/temporal-startup-v1/relative-font-script-suite-v1'
$build=ReadProof 'continuity-evidence/temporal-startup-v1/relative-font-script-suite-v1/result.json' '39B11B7FFB696DED434C9E0313DEED6744E0A3A08590BB8B14977C064EA6608C' 'baseline-build.json'
$run=ReadProof 'continuity-evidence/temporal-startup-v1/relative-font-script-owned-run-v1/result.json' '7F1448D51CF16421602D7D02DDC58F066BDAF5DB9E22A8E8B3728C525B5BA205' 'baseline-run.json'
if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.sources.Count -ne 60 -or $build.files.Count -ne 65 -or $build.fixtureFiles.Count -ne 182 -or !$run.passed -or !$run.inputsUnchanged -or $run.ownerAttention -or $run.checks -ne 3412){throw 'Prior full suite evidence incomplete'}
$recipe=Join-Path $suite 'tests.rsp';$recipePin='DF5DEEA4C810B46BB1A8D7925E9BC676EDF5EFBDA201EFC29431B1FE6C64F048'
Stage $recipe 'evidence/baseline-tests.rsp' $recipePin
$compiled=0
foreach($line in Get-Content $recipe){
    if($line -notmatch '^"(.+\.cs)"$'){continue};$path=$Matches[1]
    $relative=[IO.Path]::GetRelativePath($suite,$path).Replace('\','/')
    $pins=@($build.inputs|Where-Object path -ceq $path|Select-Object -ExpandProperty sha256 -Unique)
    if($pins.Count -ne 1){throw 'Compiled test source provenance missing'}
    Stage $path ('compiled-source/'+$relative) $pins[0];$compiled++
}
if($compiled -ne 60){throw 'Compiled source inventory differs'}
foreach($file in $build.fixtureFiles){Stage $file.path ('fixtures/'+[IO.Path]::GetRelativePath($build.fixtureSource,$file.path).Replace('\','/')) $file.sha256}
foreach($file in $build.files){Stage $file.path ('baseline-runtime/'+[IO.Path]::GetRelativePath($build.runtime,$file.path).Replace('\','/')) $file.sha256}
$bindings=[Collections.Generic.List[object]]::new()
foreach($spec in @(
    @('Lea.Engine.dll','continuity-evidence/sdk-print-aggregate-source-v1/bundle/Sample/bin/Release/Lea.Engine.dll','5B8A501DA627467A6C261A355917DE5F73DF8F00758B49C1D240CF7D596E8A4A'),
    @('Lea.Win.dll','continuity-evidence/sdk-print-aggregate-source-v1/bundle/Sample/bin/Release/Lea.Win.dll','A622A1C266AD7CA27F437832E390E594F7E419CAFED700C9F466F0517970CE0D'),
    @('Lea.Image.exe','continuity-evidence/sdk-print-aggregate-source-v1/bundle/Sample/bin/Release/Lea.Image.exe','B5E1A90210B53FC3F957D0D8DF0E3EDBCA9D7AB7B825992A82C26648038BC8E7')
)){
    $from=Join-Path $root $spec[1];Stage $from ('sdk-components/'+$spec[0]) $spec[2]
    $bindings.Add(@{relative=$spec[0];sha256=$spec[2]})
}
foreach($spec in @(
    @('I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-script-libraries-net48\result.json','E3B542B8CAD81E81321E30EBCDF6682F2AC0E371A3DCF6BE0A5B299498793AD7','sdk-script-build.json',''),
    @('I:\LeaBuildStorage\sdk-script-worker-native-20261101-v1\result.json','81690529F6933FA641969386747B6C9549BBD14D91F29B5CF1D70BF908CE6F97','sdk-worker-build.json','ScriptWorker/')
)){
    Stage $spec[0] ('evidence/'+$spec[2]) $spec[1];$proof=Get-Content $spec[0] -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'SDK component build incomplete'}
    foreach($file in $proof.outputs){$relative=$spec[3]+[IO.Path]::GetFileName($file.path);Stage $file.path ('sdk-components/'+$relative) $file.sha256;$bindings.Add(@{relative=$relative;sha256=$file.sha256})}
}
if($bindings.Count -ne 11){throw 'SDK runtime replacement inventory differs'}
foreach($row in $rows){if((Get-FileHash (Join-Path $out $row.path)).Hash -cne $row.sha256){throw 'Full suite preparation readback differs'}}
@{verified=$true;notRunnable=$true;compiledSources=60;fixtures=182;baselineRuntimeFiles=65;sdkReplacementFiles=11;files=$rows;bindings=$bindings;componentManifestsToRebind=6;workerHashConstantsToRebind=2;entryFixtureRebuildRequired=$true;assertionsChanged=$false;compiled=$false;executed=$false;requiredChecks=3412;requiredPhases=89;compileBudgetMs=120000;testBudgetMs=300000;cleanupBudgetMs=5000;completeSdk=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'preparation.json') -Encoding utf8
Get-FileHash (Join-Path $out 'preparation.json')
