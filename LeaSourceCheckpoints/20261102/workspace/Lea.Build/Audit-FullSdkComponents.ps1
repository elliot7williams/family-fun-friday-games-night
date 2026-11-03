$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\full-sdk-component-audit-20261101-v1'
if(Test-Path $out){throw 'Preserve component audit'}
if(@(Get-Process 'Lea.Image','Lea.Script.Worker','PrintTextChecks','PrintFriendlyChecks','DecoderRetirementChecks' -ErrorAction SilentlyContinue).Count){throw 'No component test overlap'}
$products=@{
    'Lea.Engine.dll'='13F43BE5F10072EEFDE7CE1DF06EAEDCEDF8AA3375D24A4548DD4B6BF6E1D365'
    'Lea.Win.dll'='629D7AA070C082853A2BD1F3A02E016685E348CA3A127A9DC4FFE01CD06C039D'
    'Lea.Image.exe'='9EB5E1EBA75BE747A45D23B16D95800DD424CDD77978E014164646CE17BD29FD'
    'Lea.Print.dll'='267DB69A8515E19A1AF54210EE058B3AFF8A498B5EBF3828332886BE0123354B'
}
$cases=@(
    @('full-sdk-view-images-v1','976B2F26F1C8A9AE0AF36DA58E7C5246B77E30459A654C1429144A74FEA6AD00','image',2),
    @('full-sdk-decoder-retirement-v1','69A7E73D303681905E8D1AD7753B5B40B8A0F7270DD53B00F22B58805F80BBE2','retirement',2),
    @('full-sdk-decoder-shutdown-race-v1','FBA284DA9A6C998E17E2A32C1FFBF7E13FDC260FAB9C8FB0A168FC1539489C9D','shutdown',2),
    @('full-sdk-print-runtime-v1','55B7F205F01E0B4496DACA8C115BE15285C7A6F5C309065847961C4CCE83047D','print',3)
)
$holds=[Collections.Generic.List[IDisposable]]::new();$pins=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($expected -and $hash -cne $expected){throw ('Component identity differs: '+$path)};$pins.Add(@{path=$path;sha256=$hash})
}
function Footer($run,[string]$expected){Pin $run.stdout '';if(@(Get-Content $run.stdout|Where-Object {$_ -ceq $expected}).Count -ne 1){throw ('Terminal footer missing: '+$expected)}}
try{
    $proof='I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json';Pin $proof '3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'
    $build=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.compiled -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.libraryProducts.Count -ne 6){throw 'Aggregate library build incomplete'}
    foreach($name in $products.Keys){
        $rows=@($build.libraryProducts|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $name})
        if($rows.Count -ne 1 -or $rows[0].sha256 -cne $products[$name]){throw 'Exact compiled component provenance missing'};Pin $rows[0].path $products[$name]
    }
    foreach($case in $cases){
        $directory=Join-Path $root ('continuity-evidence/'+$case[0]);$receipt=Join-Path $directory 'result.json';Pin $receipt $case[1]
        $r=Get-Content $receipt -Raw|ConvertFrom-Json
        if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne $case[3]){throw 'Passing retired component campaign required'}
        $decoder=$case[2] -in 'retirement','shutdown'
        if($decoder){if(!$r.fullSdkPair -or !$r.sdkPair -or !$r.aggregateSdkPair -or !$r.realWorkerCampaign -or !$r.decoderAclRestored -or $r.decoderAclBefore -cne $r.decoderAclAfter){throw 'Decoder identity/ACL campaign incomplete'}}
        elseif(!$r.fullSdk -or !$r.workerAclRestored){throw 'Full SDK identity or worker ACL result missing'}
        $runtime=if($case[2] -ceq 'image'){Join-Path $directory 'bundle/bin/Release'}else{$directory}
        foreach($name in $products.Keys){if($name -cne 'Lea.Print.dll' -or $case[2] -ceq 'print'){Pin (Join-Path $runtime $name) $products[$name]}}
        $acl=if($decoder){$r.decoderAclBefore}else{$r.workerAclBefore}
        if((Get-Acl (Join-Path $runtime 'Lea.Image.exe')).Sddl -cne $acl){throw 'Current decoder ACL differs'}
        foreach($file in $r.inputs){Pin $file.path $file.sha256}
        for($i=0;$i -lt $r.runs.Count;$i++){
            $run=$r.runs[$i];$budget=if($case[2] -cne 'print' -and $i -eq 0){120000}else{30000}
            if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or !$run.exitObserved -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Original component execution guards not satisfied'}
        }
        if($case[2] -ceq 'image'){Footer $r.runs[1] 'PASS SDK VIEW IMAGES 42'}
        elseif($case[2] -ceq 'retirement'){Footer $r.runs[1] 'Decoder retirement passed: 81'}
        elseif($case[2] -ceq 'shutdown'){Footer $r.runs[1] 'Decoder shutdown race passed: 14'}
        else{
            if(!$r.capturePassed -or $r.printerDispatched){throw 'Capture/no-dispatch result missing'}
            Footer $r.runs[0] 'PASS Lea live styled capture and foundation'
            foreach($footer in @('PASS Lea print range: 37','PASS Lea printer geometry: 16','PASS Lea native printer selection: 27','PASS Lea prepared printer ticket: 22')){Footer $r.runs[1] $footer}
            foreach($name in @('friendly.json','live.json')){Pin (Join-Path $directory $name) ''}
            $friendly=Get-Content (Join-Path $directory 'friendly.json') -Raw|ConvertFrom-Json;$live=Get-Content (Join-Path $directory 'live.json') -Raw|ConvertFrom-Json
            if(!$friendly.passed -or $friendly.baselineChecks -ne 40 -or $friendly.friendlyChecks -ne 46 -or $friendly.browserRouted -or $friendly.printJobSubmitted -or !$live.passed -or $live.liveChecks -ne 250 -or $live.hostRouted -or $live.printJobSubmitted){throw 'Exact image export/no-job result missing'}
        }
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;campaigns=4;ownedProcesses=9;inputs=$pins;productHashes=$products;guardsChanged=$false;completeSdk=$false;globalSecurityQualified=$false;physicalPrintQualified=$false;fullSuiteQualified=$false;hostAdopted=$false}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
