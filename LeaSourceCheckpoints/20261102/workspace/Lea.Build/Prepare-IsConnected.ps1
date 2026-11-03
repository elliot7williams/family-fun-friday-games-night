$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$out='I:\LeaBuildStorage\is-connected-source-20261102-v1'
if(Test-Path $out){throw 'Preserve connectivity candidate'}
$source=Join-Path $root 'continuity-evidence/deterministic-full-replay-v1/fixtures/Lea.Script/Dom.js'
$compact='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1\a\Lea.Script\Dom.compact.js'
$specs=@(
    @{name='Dom.js';path=$source;hash='11848A226794CE5D0D51F5002A17D33411B31F4934050A9DBF6E621EFC158F3E';pairs=@(
        @("const nodeProperties = { __proto__: null, contains: { __proto__: null, value: nodeMethods.contains } };","const nodeProperties = { __proto__: null, contains: { __proto__: null, value: nodeMethods.contains }, isConnected: { __proto__: null, get() { return call('co', null, nodeKey(this)); }, enumerable: true, configurable: true } };"),
        @('        contains: nodeProperties.contains,','        contains: nodeProperties.contains, isConnected: nodeProperties.isConnected,')
    )},
    @{name='Dom.compact.js';path=$compact;hash='EC4B6513A77230C3B8439568CC3FED41B7712CA3779182D38076880DCC83BC8A';pairs=@(
        @('const nodeProperties={__proto__:null,contains:{__proto__:null,value:nodeMethods.contains}};',"const nodeProperties={__proto__:null,contains:{__proto__:null,value:nodeMethods.contains},isConnected:{__proto__:null,get(){return call('co',null,nodeKey(this))},enumerable:true,configurable:true}};"),
        @('contains:nodeProperties.contains,','contains:nodeProperties.contains,isConnected:nodeProperties.isConnected,')
    )}
)
$holds=[Collections.Generic.List[IDisposable]]::new();$rows=[Collections.Generic.List[object]]::new();$negative=0
function Project([string]$text,$pairs){
    if($text.Contains('isConnected')){throw 'Already connected source'}
    $value=$text
    foreach($pair in $pairs){if([regex]::Matches($value,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique original DOM boundary required'};$value=$value.Replace($pair[0],$pair[1])}
    $inverse=$value;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
    if($inverse -cne $text){throw 'DOM projection inverse differs'}
    return $value
}
try{
    foreach($spec in $specs){
        $hold=[IO.File]::Open($spec.path,'Open','Read','Read');$holds.Add($hold)
        if((Get-FileHash $spec.path).Hash -cne $spec.hash){throw 'Mature DOM source differs'}
        $original=[IO.File]::ReadAllText($spec.path);$updated=Project $original $spec.pairs
        foreach($bad in @($updated,($original+$original),$original.Replace($spec.pairs[0][0],'MISSING_NODE_PROPERTIES'))){$rejected=$false;try{$null=Project $bad $spec.pairs}catch{$rejected=$true};if(!$rejected){throw 'Invalid DOM projection accepted'};$negative++}
        $rows.Add(@{name=$spec.name;original=$spec.path;originalSha256=$spec.hash;text=$updated})
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    foreach($row in $rows){$path=Join-Path $out $row.name;[IO.File]::WriteAllText($path,$row.text,[Text.UTF8Encoding]::new($false));$row.Remove('text');$row.path=$path;$row.sha256=(Get-FileHash $path).Hash}
    @{verified=$true;feature='Node.isConnected';files=$rows;inverseVerified=$true;negativeControls=$negative;bridgeCommandsAdded=0;remoteScriptingEnabled=$false;runtimeTested=$false;scope='Read-only live connectivity for existing document/element wrappers, existing branded nodeKey and bounded co bridge. No text-node/shadow DOM/full Node prototype conformance claim.';standard='https://dom.spec.whatwg.org/#dom-node-isconnected'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'source.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
