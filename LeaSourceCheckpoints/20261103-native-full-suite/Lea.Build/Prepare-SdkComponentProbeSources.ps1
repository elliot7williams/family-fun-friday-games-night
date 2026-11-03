param([ValidateRange(1,4)][int]$Version=1,[switch]$PrivateBindings)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base=Join-Path $root 'continuity-evidence/bootstrap-descriptor-batches-v1/source-cross-module-probes-v1'
$out='I:\LeaBuildStorage\sdk-component-probe-sources-20261101-v'+$Version
if($PrivateBindings){if($Version -ne 4){throw 'Private probes require separate v4 mode'};$out='I:\LeaBuildStorage\private-binding-component-sources-20261103-v1'}
if(Test-Path $out){throw 'Preserve component source preparation'}
$proof=Join-Path $base 'result.json'
if((Get-FileHash $proof).Hash -cne '93BE3A4BD4434131AC6CBCA065EAA0966339891D79CBA66FB69554C473F6BD7A'){throw 'Probe source proof differs'}
$record=Get-Content $proof -Raw|ConvertFrom-Json
if(!$record.passed -or !$record.inputsUnchanged -or $record.ownerAttention){throw 'Historical probe source proof incomplete'}
$old='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
$new='F8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C'
if($Version -eq 3){
    $nativeProof='I:\LeaBuildStorage\full-sdk-native-build-20261101-v1/result.json'
    if((Get-FileHash $nativeProof).Hash -cne '20E2EE1F8A87BFC2B14CD581E48B59D1DE834E87EBA729DCB5A45CCF4E1A7893'){throw 'Combined native source build proof differs'}
    $native=Get-Content $nativeProof -Raw|ConvertFrom-Json
    $selected=@($native.nativeProducts|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Lea.Script.Worker.dll'})
    if(!$native.passed -or !$native.inputsUnchanged -or !$native.workerAclRestored -or $native.ownerAttention -or $selected.Count -ne 1 -or $selected[0].sha256 -cne '5BF8B589731CB1A0A5DE38B0DAE1AF516DBF21DB5982141C9C013C55E6D39596' -or (Get-FileHash $selected[0].path).Hash -cne $selected[0].sha256){throw 'Combined worker product differs'}
    $new=$selected[0].sha256
}
if($Version -eq 4){
    $proof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json'
    if((Get-FileHash $proof).Hash -cne '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'){throw 'Deterministic build proof differs'}
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    $selected=@($build.products|Where-Object path -ceq 'bin/Release/ScriptWorker/Lea.Script.Worker.dll')
    if($selected.Count -ne 1 -or !$selected[0].equal -or $selected[0].sha256 -cne $selected[0].secondSha256 -or (Get-FileHash $selected[0].first).Hash -cne $selected[0].sha256 -or (Get-FileHash $selected[0].second).Hash -cne $selected[0].sha256){throw 'Deterministic worker copies differ'}
    $new=$selected[0].sha256
}
if($PrivateBindings){
    $proof='I:\LeaBuildStorage\private-binding-worker-build-20261102-v1/result.json'
    if((Get-FileHash $proof).Hash -cne '302C3003416F9095BA56C214FF6FD8B4F05A82F102CBAAF53B23DC3FAEE6E398'){throw 'Private worker proof differs'}
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    $selected=@($build.products|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Lea.Script.Worker.dll'})
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $selected.Count -ne 1 -or $selected[0].sha256 -cne 'F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0' -or (Get-FileHash $selected[0].path).Hash -cne $selected[0].sha256){throw 'Private worker product differs'}
    $new=$selected[0].sha256
}
function Project([string]$text,[int]$count=2){
    if([regex]::Matches($text,$old).Count -ne $count -or $text.Contains($new)){throw 'Exact original probe identity pins required'}
    $updated=$text.Replace($old,$new)
    if($updated.Replace($new,$old) -cne $text){throw 'Probe source inverse differs'}
    return $updated
}
$rows=[Collections.Generic.List[object]]::new()
$pairs=@(@('markers','MarkerHost.cs'),@('frames','FrameProtocolHost.cs'),@('clock','ClockCodecHost.cs'),@('wire','WireFramesHostV3.cs'),@('bootstrap','BootstrapBundleHost.cs'))
if($Version -ge 2){$pairs+=,@('bootstrap','OptionsUnit.cs')}
foreach($pair in $pairs){
    $source=Join-Path $base ('production-'+$pair[0]+'/'+$pair[1])
    $pins=@($record.inputs|Where-Object path -ceq $source|Select-Object -ExpandProperty sha256 -Unique)
    if($pins.Count -ne 1 -or (Get-FileHash $source).Hash -cne $pins[0]){throw 'Probe source provenance absent'}
    $count=if($pair[1] -ceq 'OptionsUnit.cs'){1}else{2}
    $text=[IO.File]::ReadAllText($source);$updated=Project $text $count
    $rejected=0
    foreach($invalid in @($text.Replace($old,'MISSING'),($text+$old),($text+$new))){try{$null=Project $invalid $count}catch{$rejected++}}
    if($rejected -ne 3){throw 'Probe projection negative controls failed'}
    $to=Join-Path $out ($pair[0]+'/'+$pair[1]);New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
    [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false))
    if([IO.File]::ReadAllText($to).Replace($new,$old) -cne $text){throw 'Written source inverse differs'}
    $rows.Add(@{component=$pair[0];path=$to;sha256=(Get-FileHash $to).Hash;origin=$source;originalSha256=$pins[0];identityReplacements=$count;inverseVerified=$true;negativeControls=3})
}
@{verified=$true;files=$rows;compiled=$false;executed=$false;assertionsChanged=$false;oldWorkerSha256=$old;selectedWorkerSha256=$new;pending=@('compile five probe hosts','rebind probe manifest binary hashes and main dispatcher','fresh candidate/full suite');runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'sources.json') -Encoding utf8
Get-FileHash (Join-Path $out 'sources.json')
