$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\repro-sdk-font-audit-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve font audit evidence'}
$cases=@(
    @('shorthand','7D7A4F356177EF62527A95101B8595EB95F6444178FFE236527D942B69124B2D','FONT_SHORTHAND controls=12 cases=14 failures=0'),
    @('initial','01A7B092792593807EDD89BEBF221C80BA8F6F8F56D894FFC83FF0F7C05689D6','FONT_INITIAL controls=5 cases=7 failures=0'),
    @('relative','CD0A75CA9B30C07B54EF47181128716C1EF998D5734305797AC63CEAA74E7610','FONT_RELATIVE controls=6 cases=12 failures=0'),
    @('initial-render','964771AB5BFCA608E3E114C7A8FD9625DDB72D86F687E487CB90A1E6FC4A49F7','FONT_RENDER PASS 142')
)
$pins=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Font audit input differs: '+$path)}
    $pins.Add(@{path=$path;sha256=$hash})
}
function CheckRun($run,[int]$budget){
    if(!$run.started -or !$run.passed -or !$run.ownedJob -or !$run.originalHandleCaptured -or !$run.childRetirementIndependentlyAudited -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or !$run.exitObserved -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.captureOverflow){throw 'Owned run guards incomplete'}
}
function CheckFooter($lines,[string]$footer){if(@($lines|Where-Object {$_ -ceq $footer}).Count -ne 1){throw 'Unique exact font footer required'}}
try{
    Pin $PSCommandPath ''
    $proof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json'
    Pin $proof '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    $pair=@($build.products|Where-Object {$_.path -in 'bin/Release/Lea.Engine.dll','bin/Release/Lea.Win.dll'})
    if($pair.Count -ne 2){throw 'Exact source-built pair missing'}
    foreach($p in $pair){if(!$p.equal -or $p.sha256 -cne $p.secondSha256){throw 'Repro pair differs'};Pin $p.first $p.sha256;Pin $p.second $p.sha256}
    $core=($pair|Where-Object path -ceq 'bin/Release/Lea.Engine.dll').sha256
    $view=($pair|Where-Object path -ceq 'bin/Release/Lea.Win.dll').sha256
    foreach($case in $cases){
        $dir=Join-Path $root ('continuity-evidence/repro-sdk-font-'+$case[0]+'-v1')
        $receipt=Join-Path $dir 'result.json';Pin $receipt $case[1]
        $r=Get-Content $receipt -Raw|ConvertFrom-Json
        if(!$r.passed -or !$r.reproSdk -or !$r.sdkPair -or !$r.aggregateSdkPair -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.productionChanged -or $r.fullReleaseQualified -or $r.error -or $r.runs.Count -ne 2 -or $r.candidateCoreSha256 -cne $core){throw 'Exact candidate font result incomplete'}
        Pin (Join-Path $dir 'Lea.Engine.dll') $core
        if($case[0] -ceq 'initial-render'){if($r.candidateViewSha256 -cne $view){throw 'View identity differs'};Pin (Join-Path $dir 'Lea.Win.dll') $view}
        foreach($file in $r.inputs){Pin $file.path $file.sha256}
        CheckRun $r.runs[0] 120000;CheckRun $r.runs[1] 30000
        Pin $r.runs[1].stdout '';CheckFooter @(Get-Content $r.runs[1].stdout) $case[2]
    }
    $negativeControls=0
    foreach($key in @('passed','ownedJob','outputDrained','childRetirementIndependentlyAudited')){
        $copy=$r.runs[1]|ConvertTo-Json|ConvertFrom-Json;$copy.$key=$false
        $rejected=$false;try{CheckRun $copy 30000}catch{$rejected=$true}
        if(!$rejected){throw 'Malformed owned result accepted'};$negativeControls++
    }
    $copy=$r.runs[1]|ConvertTo-Json|ConvertFrom-Json;$copy.parentBudgetMs=30001
    $rejected=$false;try{CheckRun $copy 30000}catch{$rejected=$true}
    if(!$rejected){throw 'Changed deadline accepted'};$negativeControls++
    foreach($lines in @(@('wrong footer'),@('FONT_RENDER PASS 142','FONT_RENDER PASS 142'))){
        $rejected=$false;try{CheckFooter $lines 'FONT_RENDER PASS 142'}catch{$rejected=$true}
        if(!$rejected){throw 'Malformed footer accepted'};$negativeControls++
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;campaigns=4;ownedChildren=8;negativeControls=$negativeControls;inputs=$pins;relativeRenderQualified=$false;completeSdk=$false;owner90861Retired=$false;scope='Exact candidate completed font checks; child job retirement is not idle terminal retirement'}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
