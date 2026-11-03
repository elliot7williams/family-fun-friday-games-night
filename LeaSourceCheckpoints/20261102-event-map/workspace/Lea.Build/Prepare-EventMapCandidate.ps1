$ErrorActionPreference='Stop'
$proof='I:/LeaBuildStorage/lazy-map-source-20261102-v1/source.json'
$out='I:/LeaBuildStorage/event-map-source-20261102-v1'
if(Test-Path $out){throw 'Preserve event map candidate'}
if((Get-FileHash $proof).Hash -cne '4E8524E487B5F6E59408097EE7C5A7950C7BCADFF272B45BD7847B64334B5803'){throw 'Lazy map source proof differs'}
$plan=Get-Content $proof -Raw|ConvertFrom-Json
$readable=@(
    @('const targets = new WeakMap();',"let targets = null, windowTarget = null, documentTarget = null;`n    function targetState(receiver) {`n        if (receiver === realm) return windowTarget;`n        if (documentTarget !== null && receiver === documentTarget.node) return documentTarget;`n        return targets === null ? undefined : apply(getState, targets, [receiver]);`n    }",1),
    @('const state = apply(getState, targets, [receiver === null || receiver === undefined ? realm : receiver]);','const state = targetState(receiver === null || receiver === undefined ? realm : receiver);',1),
    @('apply(setState, targets, [node, state]);',"if (node === realm) windowTarget = state;`n        else if (node === document) documentTarget = state;`n        else { if (targets === null) targets = new StateMap(); apply(setState, targets, [node, state]); }",1),
    @('const state = apply(getState, targets, [receiver]);','const state = targetState(receiver);',3)
)
$compact=@(
    @('const targets=new WeakMap;','let targets=null,windowTarget=null,documentTarget=null;function targetState(receiver){if(receiver===realm)return windowTarget;if(documentTarget!==null&&receiver===documentTarget.node)return documentTarget;return targets===null?undefined:apply(getState,targets,[receiver])}',1),
    @('const state=apply(getState,targets,[receiver===null||receiver===undefined?realm:receiver]);','const state=targetState(receiver===null||receiver===undefined?realm:receiver);',1),
    @('apply(setState,targets,[node,state]);','if(node===realm)windowTarget=state;else if(node===document)documentTarget=state;else{if(targets===null)targets=new StateMap;apply(setState,targets,[node,state])}',1),
    @('const state=apply(getState,targets,[receiver]);','const state=targetState(receiver);',3)
)
function Project([string]$text,$pairs){
    if($text.Contains('windowTarget')){throw 'Already projected event map'}
    $value=$text
    foreach($pair in $pairs){
        if($pair.Count -ne 3 -or [regex]::Matches($value,[regex]::Escape($pair[0])).Count -ne $pair[2]){throw 'Unique event map boundary required'}
        $value=$value.Replace($pair[0],$pair[1])
    }
    $inverse=$value
    for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
    if($inverse -cne $text){throw 'Event map exact inverse differs'}
    return $value
}
$rows=[Collections.Generic.List[object]]::new();$negative=0
foreach($file in $plan.files){
    if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Lazy map source changed'}
    $text=[IO.File]::ReadAllText($file.path);$pairs=if($file.name -ceq 'Dom.js'){$readable}else{$compact}
    $updated=Project $text $pairs
    $bad=[Collections.Generic.List[string]]::new();$bad.Add($updated);$bad.Add($text+$text)
    foreach($pair in $pairs){$bad.Add($text.Replace($pair[0],'MISSING_EVENT_MAP_BOUNDARY'))}
    foreach($case in $bad){$rejected=$false;try{$null=Project $case $pairs}catch{$rejected=$true};if(!$rejected){throw 'Malformed event map projection accepted'};$negative++}
    $rows.Add(@{name=$file.name;original=$file.path;originalSha256=$file.sha256;sdkOriginalSha256=$file.sdkOriginalSha256;text=$updated})
}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($row in $rows){$path=Join-Path $out $row.name;[IO.File]::WriteAllText($path,$row.text,[Text.UTF8Encoding]::new($false));$row.Remove('text');$row.path=$path;$row.sha256=(Get-FileHash $path).Hash}
@{verified=$true;candidateOnly=$true;files=$rows;inverseVerified=$true;negativeControls=$negative;hostPolicyChanged=$false;runtimeTested=$false;performanceImprovementProven=$false;scope='Direct private window/document event state; allocate captured-constructor element WeakMap on first wrapped element. Preserve all branded lookups and event algorithms; no host or policy changes.'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
