$ErrorActionPreference='Stop'
$proof='I:/LeaBuildStorage/global-install-source-20261102-v1/source.json'
$out='I:/LeaBuildStorage/lazy-map-source-20261102-v1'
if(Test-Path $out){throw 'Preserve lazy map source'}
if((Get-FileHash $proof).Hash -cne '8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5'){throw 'Global installation source proof differs'}
$plan=Get-Content $proof -Raw|ConvertFrom-Json
$readable=@(
    @('const listStates = new WeakMap(), getState = WeakMap.prototype.get, setState = WeakMap.prototype.set;','let listStates = null; const StateMap = WeakMap, getState = StateMap.prototype.get, setState = StateMap.prototype.set;'),
    @('const exceptions = new WeakMap();','let exceptions = null;'),
    @('const state = apply(getState, exceptions, [value]);','const state = exceptions === null ? undefined : apply(getState, exceptions, [value]);'),
    @('apply(setState, exceptions, [value, { message, name }]);','if (exceptions === null) exceptions = new StateMap(); apply(setState, exceptions, [value, { message, name }]);'),
    @('const state = apply(getState, listStates, [receiver]);','const state = listStates === null ? undefined : apply(getState, listStates, [receiver]);'),
    @('apply(setState, listStates, [proxy, key]);','if (listStates === null) listStates = new StateMap(); apply(setState, listStates, [proxy, key]);')
)
$compact=@(
    @('const listStates=new WeakMap,getState=WeakMap.prototype.get,setState=WeakMap.prototype.set;','let listStates=null;const StateMap=WeakMap,getState=StateMap.prototype.get,setState=StateMap.prototype.set;'),
    @('const exceptions=new WeakMap;','let exceptions=null;'),
    @('const state=apply(getState,exceptions,[value]);','const state=exceptions===null?undefined:apply(getState,exceptions,[value]);'),
    @('apply(setState,exceptions,[value,{message,name}]);','if(exceptions===null)exceptions=new StateMap;apply(setState,exceptions,[value,{message,name}]);'),
    @('const state=apply(getState,listStates,[receiver]);','const state=listStates===null?undefined:apply(getState,listStates,[receiver]);'),
    @('apply(setState,listStates,[proxy,key]);','if(listStates===null)listStates=new StateMap;apply(setState,listStates,[proxy,key]);')
)
function Project([string]$text,$pairs){
    if($text.Contains('StateMap')){throw 'Already lazy map source'}
    $value=$text
    foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($value,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique original private map boundary required'};$value=$value.Replace($pair[0],$pair[1])}
    $inverse=$value;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
    if($inverse -cne $text){throw 'Lazy map exact inverse differs'}
    return $value
}
$rows=[Collections.Generic.List[object]]::new();$negative=0
foreach($file in $plan.files){
    if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Global source changed'}
    $text=[IO.File]::ReadAllText($file.path);$pairs=if($file.name -ceq 'Dom.js'){$readable}else{$compact}
    $updated=Project $text $pairs
    $bad=[Collections.Generic.List[string]]::new();$bad.Add($updated);$bad.Add($text+$text)
    foreach($pair in $pairs){$bad.Add($text.Replace($pair[0],'MISSING_PRIVATE_MAP_BOUNDARY'))}
    foreach($case in $bad){$rejected=$false;try{$null=Project $case $pairs}catch{$rejected=$true};if(!$rejected){throw 'Malformed lazy map projection accepted'};$negative++}
    $rows.Add(@{name=$file.name;original=$file.path;originalSha256=$file.sha256;sdkOriginalSha256=$file.sdkOriginalSha256;text=$updated})
}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($row in $rows){$path=Join-Path $out $row.name;[IO.File]::WriteAllText($path,$row.text,[Text.UTF8Encoding]::new($false));$row.Remove('text');$row.path=$path;$row.sha256=(Get-FileHash $path).Hash}
@{verified=$true;candidateOnly=$true;files=$rows;inverseVerified=$true;negativeControls=$negative;privateMapsDeferred=2;constructorCaptured=$true;hostPolicyChanged=$false;runtimeTested=$false;performanceImprovementProven=$false;scope='Defer only private DOMTokenList and DOMException state maps until first legitimate creation; capture WeakMap constructor and methods at bootstrap. Invalid receiver calls remain nonallocating. No policy, API descriptor, realm or host evaluation changes.'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
