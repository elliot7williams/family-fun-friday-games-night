param([ValidateRange(1,3)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base=Join-Path $root 'continuity-evidence/bootstrap-descriptor-batches-v1/paired-entry-core-v1'
$out='I:\LeaBuildStorage\sdk-entry-core-source-20261101-v'+$Version
if(Test-Path -LiteralPath $out){throw 'Preserve entry companion source preparation'}
$proof=Join-Path $base 'result.json'
if((Get-FileHash $proof).Hash -cne 'DE0BA93B8B278DBC40C4F9F27F9FC9AAE09239A672BA24BA1661D40DD95EB208'){throw 'Original entry companion proof differs'}
$record=Get-Content $proof -Raw|ConvertFrom-Json
if(!$record.passed -or !$record.inputsUnchanged -or $record.compilerExitCode -ne 0){throw 'Original companion build incomplete'}
$source=Join-Path $root 'continuity-evidence/bootstrap-descriptor-batches-v1/paired-entry-source-v1/WorkerEntryCore.cs';$sourceHash='37789573C4977BA469AB2844BA64C84DF722F29CC814407094A177D1440DDFDD'
$pins=@($record.inputs|Where-Object path -ceq $source|Select-Object -ExpandProperty sha256 -Unique)
if($pins.Count -ne 1 -or $pins[0] -cne $sourceHash -or (Get-FileHash $source).Hash -cne $sourceHash){throw 'Companion source provenance differs'}
$old='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
$new='F8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C'
if($Version -eq 2){
    $nativeProof='I:\LeaBuildStorage\full-sdk-native-build-20261101-v1/result.json'
    if((Get-FileHash $nativeProof).Hash -cne '20E2EE1F8A87BFC2B14CD581E48B59D1DE834E87EBA729DCB5A45CCF4E1A7893'){throw 'Combined native build proof differs'}
    $native=Get-Content $nativeProof -Raw|ConvertFrom-Json
    $selected=@($native.nativeProducts|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Lea.Script.Worker.dll'})
    if(!$native.passed -or !$native.inputsUnchanged -or !$native.workerAclRestored -or $native.ownerAttention -or $selected.Count -ne 1 -or $selected[0].sha256 -cne '5BF8B589731CB1A0A5DE38B0DAE1AF516DBF21DB5982141C9C013C55E6D39596' -or (Get-FileHash $selected[0].path).Hash -cne $selected[0].sha256){throw 'Combined worker product differs'}
    $new=$selected[0].sha256
}
if($Version -eq 3){
    $proof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json'
    if((Get-FileHash $proof).Hash -cne '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'){throw 'Deterministic build proof differs'}
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    $selected=@($build.products|Where-Object path -ceq 'bin/Release/ScriptWorker/Lea.Script.Worker.dll')
    if($selected.Count -ne 1 -or !$selected[0].equal -or $selected[0].sha256 -cne $selected[0].secondSha256 -or (Get-FileHash $selected[0].first).Hash -cne $selected[0].sha256 -or (Get-FileHash $selected[0].second).Hash -cne $selected[0].sha256){throw 'Deterministic worker copies differ'}
    $new=$selected[0].sha256
}
function Project([string]$text){
    if([regex]::Matches($text,$old).Count -ne 1 -or $text.Contains($new)){throw 'Unique old worker pin required'}
    $updated=$text.Replace($old,$new);if($updated.Replace($new,$old) -cne $text){throw 'Companion identity inverse differs'}
    return $updated
}
$text=[IO.File]::ReadAllText($source);$updated=Project $text;$negative=0
foreach($invalid in @($text.Replace($old,'MISSING'),($text+$old),($text+$new))){try{$null=Project $invalid}catch{$negative++}}
if($negative -ne 3){throw 'Companion negative controls failed'}
$recipe=Join-Path $base 'core.rsp';$recipeHash='296F27F62411EDCAF2A3405B6E6F4853DD53C149A0118B0241881A39D4C87D60'
if((Get-FileHash $recipe).Hash -cne $recipeHash){throw 'Original companion recipe differs'}
New-Item -ItemType Directory -Path $out|Out-Null
$candidate=Join-Path $out 'WorkerEntryCore.cs';[IO.File]::WriteAllText($candidate,$updated,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($candidate).Replace($new,$old) -cne $text){throw 'Written companion inverse differs'}
Copy-Item -LiteralPath $recipe -Destination (Join-Path $out 'original-core.rsp')
@{verified=$true;source=$candidate;sha256=(Get-FileHash $candidate).Hash;origin=$source;originalSha256=$sourceHash;sourceProofSha256=(Get-FileHash $proof).Hash;recipeSha256=$recipeHash;identityReplacements=1;inverseVerified=$true;negativeControls=3;assertionsChanged=$false;compiled=$false;executed=$false;pending=@('compile companion with installed8.0.33 references','rebind seven-file fixture/resource/host and dispatcher','fresh full suite') ;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
