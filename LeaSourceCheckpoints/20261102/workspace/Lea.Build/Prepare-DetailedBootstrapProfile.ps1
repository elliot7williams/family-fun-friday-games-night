$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\trusted-bootstrap-profile-source-20261102-v3'
if(Test-Path $out){throw 'Preserve detailed bootstrap source'}
$base='I:\LeaBuildStorage\trusted-bootstrap-profile-source-20261102-v2/source.json'
if((Get-FileHash $base).Hash -cne 'D1255F73A1101815927CF1E909D605163D1C743C30AC0C7FFA66255A11AA3CBE'){throw 'Corrected buffered profile prerequisite differs'}
$profile=Get-Content $base -Raw|ConvertFrom-Json
if(!$profile.verified -or !$profile.criticalOperationsVerified -or $profile.pairShapeControls -ne 1){throw 'Original-operation preservation required'}
$script=@($profile.files|Where-Object name -CEQ '8.cs')[0]
$resource=@($profile.files|Where-Object name -CEQ 'Dom.compact.js')[0]
foreach($file in @($script,$resource)){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Buffered profile input differs'}}
$original=[IO.File]::ReadAllText($resource.path)
$points=@(
    @{label='profile-microtask';anchor='const realm=globalThis';after=$false},
    @{label='profile-timers';anchor='let cache=null;let frames=null;';after=$false},
    @{label='profile-frames';anchor='const targets=new WeakMap;';after=$false},
    @{label='profile-state';anchor='const styleNames=Object.keys(styles);';after=$true},
    @{label='profile-handler-begin';anchor='const eventProperties={';after=$false},
    @{label='profile-handler-end';anchor='const defineEvents=Object.defineProperties;';after=$false}
)
function Project([string]$text){
    if($text.Contains('profile-microtask')){throw 'Already detailed bootstrap'}
    $value=$text;$changes=[Collections.Generic.List[object]]::new()
    foreach($point in $points){
        if([regex]::Matches($value,[regex]::Escape($point.anchor)).Count -ne 1){throw 'Unique detailed bootstrap boundary required'}
        $mark="checkpoint('"+$point.label+"');"
        $replacement=if($point.after){$point.anchor+$mark}else{$mark+$point.anchor}
        $value=$value.Replace($point.anchor,$replacement);$changes.Add(@{old=$point.anchor;replacement=$replacement})
    }
    $inverse=$value;for($i=$changes.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($changes[$i].replacement,$changes[$i].old)}
    if($inverse -cne $text){throw 'Detailed resource inverse differs'}
    return $value
}
$updated=Project $original;$negative=0
foreach($point in $points){$rejected=$false;try{$null=Project $original.Replace($point.anchor,'MISSING_DETAIL_BOUNDARY')}catch{$rejected=$true};if(!$rejected){throw 'Invalid detailed boundary accepted'};$negative++}
$markerPath='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1\a\Lea.Script\2.cs'
$markerHash='1B311CF5C774D8A32B2543A45B7359C58E1643E952073CB2A7E00973DE05A9E9'
if((Get-FileHash $markerPath).Hash -cne $markerHash){throw 'Original marker contract differs'}
$markers=[IO.File]::ReadAllText($markerPath)
$old='private static readonly string[] stages = { "declarations", "scope", "events", "interfaces", "elements", "document" };'
$labels=@('declarations','scope','profile-microtask','profile-timers','profile-frames','profile-state','profile-handler-begin','profile-handler-end','events','interfaces','elements','document')
$replacement='private static readonly string[] stages = { '+(($labels|ForEach-Object {'"'+$_+'"'}) -join ', ')+' };'
function ProjectMarkers([string]$text){if([regex]::Matches($text,[regex]::Escape($old)).Count -ne 1 -or $text.Contains($replacement)){throw 'Unique old marker contract required'};return $text.Replace($old,$replacement)}
$newMarkers=ProjectMarkers $markers
if($newMarkers.Replace($replacement,$old) -cne $markers){throw 'Marker contract inverse differs'}
$markerNegative=0
foreach($bad in @($markers.Replace($old,'MISSING_MARKERS'),($markers+$markers),$newMarkers)){$rejected=$false;try{$null=ProjectMarkers $bad}catch{$rejected=$true};if(!$rejected){throw 'Invalid marker contract accepted'};$markerNegative++}
$actual=@([regex]::Matches($updated,"(?:__jobs|checkpoint)\('([^']+)'\)")|ForEach-Object {$_.Groups[1].Value})
if(($actual -join '|') -cne ($labels -join '|')){throw 'Detailed resource and marker ordering differ'}
New-Item -ItemType Directory -Path $out|Out-Null
$rows=[Collections.Generic.List[object]]::new()
Copy-Item -LiteralPath $script.path -Destination (Join-Path $out '8.cs')
if((Get-FileHash (Join-Path $out '8.cs')).Hash -cne $script.sha256){throw 'Unchanged buffered profiler copy differs'}
$rows.Add(@{name='8.cs';path=(Join-Path $out '8.cs');originalSha256=$script.originalSha256;sha256=$script.sha256})
foreach($item in @(@{name='2.cs';text=$newMarkers;original=$markerHash},@{name='Dom.compact.js';text=$updated;original=$resource.originalSha256})){
    $path=Join-Path $out $item.name;[IO.File]::WriteAllText($path,$item.text,[Text.UTF8Encoding]::new($false));$rows.Add(@{name=$item.name;path=$path;originalSha256=$item.original;sha256=(Get-FileHash $path).Hash})
}
@{verified=$true;diagnosticOnly=$true;inverseVerified=$true;negativeControls=$negative;markerNegativeControls=$markerNegative;pairShapeControls=1;criticalOperationsVerified=$true;resourceMarkerOrderVerified=$true;files=$rows;budgetsChanged=$false;compiled=$false;executed=$false;scope='Six extra private diagnostic checkpoints only; original bootstrap statements retained, original1000ms and script guards unchanged. Extra observer costs; not product/release evidence.'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
