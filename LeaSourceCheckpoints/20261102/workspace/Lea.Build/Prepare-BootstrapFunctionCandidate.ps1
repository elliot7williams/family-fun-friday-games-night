$ErrorActionPreference='Stop'
$proof='I:/LeaBuildStorage/global-install-source-20261102-v1/source.json'
$out='I:/LeaBuildStorage/bootstrap-function-source-20261102-v1'
if(Test-Path $out){throw 'Preserve bootstrap function candidate'}
if((Get-FileHash $proof).Hash -cne '8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5'){throw 'Global installation source proof differs'}
$plan=Get-Content $proof -Raw|ConvertFrom-Json
function Project([string]$text,[bool]$compact){
    $nl=if($compact){''}elseif($text.Contains("`r`n")){"`r`n"}else{"`n"}
    $head="'use strict';"+$nl+"__jobs('declarations');"+$nl+'{'
    $dispatch=if($compact){'(function dispatch(type,keys,time){'}else{'(function dispatch(type, keys, time) {'}
    $trim=$text.TrimEnd([char[]]"`r`n");$suffix=$text.Substring($trim.Length)
    if(!$trim.StartsWith($head) -or !$trim.EndsWith('}') -or [regex]::Matches($trim,[regex]::Escape($head)).Count -ne 1 -or [regex]::Matches($trim,[regex]::Escape($dispatch)).Count -ne 1){throw 'Unique original bootstrap block/dispatcher required'}
    $inside=$trim.Substring($head.Length,$trim.Length-$head.Length-1)
    $returned=if($compact){'return'+$dispatch}else{'return '+$dispatch}
    $functionHead=if($compact){'(function(){'}else{'(function() {'}
    $newHead=$head.Substring(0,$head.Length-1)+$functionHead
    $updated=$newHead+$inside.Replace($dispatch,$returned)+'})();'+$suffix
    $inverse=$head+$updated.Substring($newHead.Length,$updated.Length-$newHead.Length-5-$suffix.Length).Replace($returned,$dispatch)+'}'+$suffix
    if($inverse -cne $text){throw 'Bootstrap body/dispatcher inverse differs'}
    return $updated
}
$rows=[Collections.Generic.List[object]]::new();$negative=0
foreach($file in $plan.files){
    if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Global installation source changed'}
    $original=[IO.File]::ReadAllText($file.path);$compact=$file.name -ceq 'Dom.compact.js'
    $updated=Project $original $compact
    $trim=$original.TrimEnd([char[]]"`r`n")
    foreach($bad in @($updated,($original+$original),$original.Replace("__jobs('declarations');",'MISSING'),$original.Replace('(function dispatch(','(function MISSING('),($trim.Substring(0,$trim.Length-1)+'!'))){
        $rejected=$false;try{$null=Project $bad $compact}catch{$rejected=$true};if(!$rejected){throw 'Invalid bootstrap function projection accepted'};$negative++
    }
    $rows.Add(@{name=$file.name;original=$file.path;originalSha256=$file.sha256;sdkOriginalSha256=$file.sdkOriginalSha256;text=$updated})
}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($row in $rows){$path=Join-Path $out $row.name;[IO.File]::WriteAllText($path,$row.text,[Text.UTF8Encoding]::new($false));$row.Remove('text');$row.path=$path;$row.sha256=(Get-FileHash $path).Hash}
@{verified=$true;candidateOnly=$true;files=$rows;bodyInverseVerified=$true;negativeControls=$negative;singleEvaluationRequired=$true;hostPolicyChanged=$false;runtimeTested=$false;performanceImprovementProven=$false;scope='Replace fixed private top-level block with one strict IIFE returning the same private dispatcher. All original body operations preserved by exact inverse; no new Evaluate, warmup, realm reuse or reset-budget chunks.'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
