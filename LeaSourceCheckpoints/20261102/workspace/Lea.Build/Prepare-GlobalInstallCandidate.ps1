$ErrorActionPreference='Stop'
$source='I:\LeaBuildStorage\is-connected-source-20261102-v1'
$out='I:\LeaBuildStorage\global-install-source-20261102-v1'
if(Test-Path $out){throw 'Preserve global installation candidate'}
$proof=Join-Path $source 'source.json'
if((Get-FileHash $proof).Hash -cne 'B4BEA8392042E88DE4D833AA158B35869811A95AC9D5D8CA5AF8D02B53FA359E'){throw 'Connectivity source proof differs'}
$plan=Get-Content $proof -Raw|ConvertFrom-Json
$rows=[Collections.Generic.List[object]]::new();$negative=0
function Project([string]$text,[bool]$compact){
    $nl=if($text.Contains("`r`n")){"`r`n"}else{"`n"}
    $start=if($compact){'Object.defineProperties(globalThis,{setTimeout:'}else{"    Object.defineProperties(globalThis, {$nl        setTimeout:"}
    $end=if($compact){'const targets=new WeakMap;'}else{'    const targets = new WeakMap();'}
    $middle=if($compact){'let cache=null;'}else{'    let cache = null;'}
    $frame=if($compact){'Object.defineProperties(globalThis,{requestAnimationFrame:'}else{"    Object.defineProperties(globalThis, {$nl        requestAnimationFrame:"}
    foreach($anchor in @($start,$end,$middle,$frame)){if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Unique original global installation boundary required'}}
    $a=$text.IndexOf($start);$b=$text.IndexOf($middle);$c=$text.IndexOf($frame);$d=$text.IndexOf($end)
    if(!($a -lt $b -and $b -lt $c -and $c -lt $d)){throw 'Global installation order differs'}
    $timer=$text.Substring($a,$b-$a);$locals=$text.Substring($b,$c-$b);$frames=$text.Substring($c,$d-$c)
    $head=if($compact){'Object.defineProperties(globalThis,{'}else{"    Object.defineProperties(globalThis, {$nl"}
    $tail=if($compact){'});'}else{"$nl    });$nl"}
    if(!$timer.EndsWith($tail) -or !$frames.StartsWith($head)){throw 'Descriptor call boundary differs'}
    $separator=if($compact){''}else{$nl}
    $replacement=$locals+$timer.Substring(0,$timer.Length-$tail.Length)+','+$separator+$frames.Substring($head.Length)
    $original=$text.Substring($a,$d-$a)
    $updated=$text.Substring(0,$a)+$replacement+$text.Substring($d)
    if([regex]::Matches($updated,[regex]::Escape($replacement)).Count -ne 1 -or $updated.Replace($replacement,$original) -cne $text){throw 'Global installation inverse differs'}
    if([regex]::Matches($text,'Object\.defineProperties\(globalThis,').Count-[regex]::Matches($updated,'Object\.defineProperties\(globalThis,').Count -ne 1){throw 'Expected one fewer global descriptor call'}
    return $updated
}
foreach($file in $plan.files){
    if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Connectivity bootstrap changed'}
    $original=[IO.File]::ReadAllText($file.path);$compact=$file.name -ceq 'Dom.compact.js'
    $updated=Project $original $compact
    foreach($bad in @($updated,($original+$original),$original.Replace('Object.defineProperties(globalThis,','BROKEN('))){
        $rejected=$false;try{$null=Project $bad $compact}catch{$rejected=$true};if(!$rejected){throw 'Malformed installation source accepted'};$negative++
    }
    $rows.Add(@{name=$file.name;original=$file.path;originalSha256=$file.sha256;sdkOriginalSha256=$file.originalSha256;text=$updated})
}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($row in $rows){$path=Join-Path $out $row.name;[IO.File]::WriteAllText($path,$row.text,[Text.UTF8Encoding]::new($false));$row.Remove('text');$row.path=$path;$row.sha256=(Get-FileHash $path).Hash}
@{verified=$true;candidateOnly=$true;files=$rows;inverseVerified=$true;negativeControls=$negative;globalDescriptorCallsRemoved=1;budgetsChanged=$false;runtimeTested=$false;scope='Merge timer/frame descriptor installation; same descriptor literals and order, internal frame locals initialized before the merged call. No claimed speed improvement or release qualification.'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
