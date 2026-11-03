$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/sdk-image-worker-audit-v1'
if(Test-Path $out){throw 'Preserve SDK image audit'}
$buildPath='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-image-worker\result.json'
if((Get-FileHash $buildPath).Hash -cne '95F4A5D6DC02D49A9A27DFF1245010385FD4BF349ADA253962D895C814E277A0'){throw 'Image build receipt differs'}
$build=Get-Content $buildPath -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.outputs.Count -ne 1){throw 'Image build gate incomplete'}
$sourcePins=@{'Mg.cs'='CDF6E334CF99742BC9F3427ECE57B14AB39A5433141040ED42120F43B7FA83F1';'Program.cs'='4B0EBB73E7ED355373DD75D45A83B6D160900F0BCB93F79536F4E7C5BC83DA44'}
foreach($name in $sourcePins.Keys){
    $path='I:\LeaBuildStorage\sdk-image-worker-20261031-v1\Lea.Image\'+$name
    $row=@($build.inputs|Where-Object path -ceq $path)
    if($row.Count -ne 1 -or $row[0].sha256 -cne $sourcePins[$name] -or (Get-FileHash $path).Hash -cne $sourcePins[$name]){throw 'Image compiled source pin differs'}
}
$products=@{'Lea.Engine.dll'='772A51ADD8C8A9CE80964BEDB043D912B0DFB95422E3DC84FAC6F49B93366270';'Lea.Win.dll'='80085FA1CD380CDF2E7AB0F8BBE5AC8982A11E95F82C43F776EE16ADC1A0D0C0';'Lea.Image.exe'='EF89F8AA04E51921B54D1BAFE889244BF39229E57A08FBD7BFCB0D6B5E098BD3'}
$cases=@(@('sdk-decoder-retirement-v1','DC95142ED4BA9E7972A24E1A9F07BF0570ACB1AB6053C940C9815C149BBE69C2','Decoder retirement passed: 81'),@('sdk-decoder-shutdown-race-v1','7868A8864A905EA44F12A6C21BD1A0723612CEFBA4F8BB31B72A5F5B203CD15B','Decoder shutdown race passed: 14'))
foreach($case in $cases){
    $directory=Join-Path $root ('continuity-evidence/'+$case[0]);$path=Join-Path $directory 'result.json'
    if((Get-FileHash $path).Hash -cne $case[1]){throw 'Image run receipt differs'}
    $r=Get-Content $path -Raw|ConvertFrom-Json
    if(!$r.passed -or !$r.sdkPair -or !$r.realWorkerCampaign -or !$r.decoderAclRestored -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.runs.Count -ne 2){throw 'Image run gate incomplete'}
    foreach($run in $r.runs){if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Image run lifecycle gate failed'}}
    $lines=@(Get-Content $r.runs[1].stdout);if($lines[-1] -cne $case[2]){throw 'Image terminal footer differs'}
    foreach($name in $products.Keys){
        $copy=Join-Path $directory $name;$row=@($r.inputs|Where-Object path -ceq $copy)
        if($row.Count -ne 1 -or $row[0].sha256 -cne $products[$name] -or (Get-FileHash $copy).Hash -cne $products[$name]){throw 'Image tested product identity differs'}
    }
    foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Image test input changed'}}
}
New-Item -ItemType Directory -Path $out | Out-Null
@{verified=$true;products=$products;compiledSourcePins=$sourcePins;buildReceiptSha256=(Get-FileHash $buildPath).Hash;retirementChecks=81;shutdownRaceChecks=14;globalSecurityQualified=$false;completeSdk=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 4|Set-Content (Join-Path $out 'result.json') -Encoding utf8
Get-FileHash (Join-Path $out 'result.json')
