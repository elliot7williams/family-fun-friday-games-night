$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\full-sdk-font-audit-20261101-v1'
if(Test-Path $out){throw 'Preserve font audit'}
$core='13F43BE5F10072EEFDE7CE1DF06EAEDCEDF8AA3375D24A4548DD4B6BF6E1D365'
$view='629D7AA070C082853A2BD1F3A02E016685E348CA3A127A9DC4FFE01CD06C039D'
$cases=@(
    @('shorthand','F1CCD70D92FFF3DBCE1281D14892461CBBBB86E138B3D06EFE96228EBA7E6E29','FONT_SHORTHAND controls=12 cases=14 failures=0',$false),
    @('initial','5CE0B090C12B0840F298E1886E3D50BECA93AC0A23487553827F08CDDA8D09D6','FONT_INITIAL controls=5 cases=7 failures=0',$false),
    @('relative','82E16592C569D4434F7EC0CC58AC04062F82F2FAA0842FFF2283A06956FEEE1D','FONT_RELATIVE controls=6 cases=12 failures=0',$false),
    @('initial-render','9EB47A7D8BACE649C492E8F6B3CAA9D6A506C3AEEBCB2A0D1E9329116DB733C0','FONT_RENDER PASS 142',$true),
    @('relative-render','CC55DB9ACB74315A8CC2C74D878FAF4E15411BB11D42E292FAD65E52A2AD9031','FONT_RENDER PASS 142',$true)
)
$holds=[Collections.Generic.List[IDisposable]]::new();$pins=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Font audit identity differs: '+$path)};$pins.Add(@{path=$path;sha256=$hash})
}
try{
    $proof='I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json';Pin $proof '3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.compiled -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.libraryProducts.Count -ne 6){throw 'Aggregate build incomplete'}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll')){
        $files=@($build.libraryProducts|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $name})
        $expected=if($name -ceq 'Lea.Engine.dll'){$core}else{$view}
        if($files.Count -ne 1 -or $files[0].sha256 -cne $expected){throw 'Unique matching aggregate product required'};Pin $files[0].path $expected
    }
    foreach($case in $cases){
        $directory=Join-Path $root ('continuity-evidence/full-sdk-font-'+$case[0]+'-v1');$receipt=Join-Path $directory 'result.json';Pin $receipt $case[1]
        $r=Get-Content $receipt -Raw|ConvertFrom-Json
        if(!$r.passed -or !$r.fullSdkPair -or !$r.sdkPair -or !$r.aggregateSdkPair -or $r.printSdk -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.productionChanged -or $r.error -or $r.runs.Count -ne 2 -or $r.candidateCoreSha256 -cne $core){throw 'Full SDK font closure incomplete'}
        Pin (Join-Path $directory 'Lea.Engine.dll') $core
        if($case[3]){if($r.candidateViewSha256 -cne $view){throw 'Exact view identity missing'};Pin (Join-Path $directory 'Lea.Win.dll') $view}
        foreach($file in $r.inputs){Pin $file.path $file.sha256}
        for($i=0;$i -lt 2;$i++){
            $run=$r.runs[$i];$budget=if($i -eq 0){120000}else{30000}
            if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or !$run.exitObserved -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original font execution guards not satisfied'}
        }
        Pin $r.runs[1].stdout ''
        if(@(Get-Content $r.runs[1].stdout|Where-Object {$_ -ceq $case[2]}).Count -ne 1){throw 'Exact font footer required'}
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;coreSha256=$core;viewSha256=$view;campaigns=5;ownedProcesses=10;inputs=$pins;assertionsChanged=$false;guardsChanged=$false;completeSdk=$false;fullSuiteQualified=$false;hostAdopted=$false}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
