param([switch]$Images,[switch]$Aggregate,[switch]$Package)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/sdk-print-runtime-v1'
$path=Join-Path $directory 'result.json'
$pin='B04530D0F69FC45CE28241660B43F8D0A57425AF22254D46CDD8B968B1B89B3D'
if($Images){$directory=Join-Path $root 'continuity-evidence/sdk-print-images-v1';$path=Join-Path $directory 'result.json';$pin='CD013D8EB1C4AFC55112F8337DE60ABC8D1EB1294C8F74C1C3EF1A104EFFF6A5'}
if($Aggregate){if(!$Images){throw 'Aggregate audit requires images'};$directory=Join-Path $root 'continuity-evidence/sdk-print-aggregate-runtime-v1';$path=Join-Path $directory 'result.json';$pin='D6D0E65685041CFB517712C92824F3AB5E50E22E4ED6DB5086E882B058D0B676'}
if($Package){if(!$Aggregate){throw 'Package audit requires aggregate mode'};$directory=Join-Path $root 'continuity-evidence/sdk-print-package-runtime-v1';$path=Join-Path $directory 'result.json';$pin='408F197F20E1A1EFCE406906DD1EEDBF75DFF7DAE07362A9E55B330046BCF6B2'}
if((Get-FileHash $path).Hash -cne $pin){throw 'Print receipt differs'}
$r=Get-Content $path -Raw|ConvertFrom-Json
if($Package -and (!$r.packageExtractionVerified -or $r.packageSha256 -cne '2B035A42A34A375393D5E4F4E2B27119363C81DF25239C3BF87412B4980C3133')){throw 'Print package identity differs'}
if(!$r.passed -or !$r.capturePassed -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.runs.Count -ne $(if($Aggregate){4}elseif($Images){3}else{2})){throw 'Print gate incomplete'}
foreach($run in $r.runs){
    if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or $run.timedOut -or $run.killRequested -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.parentBudgetMs -ne 30000 -or $run.elapsedMs -gt 30000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Print lifecycle/deadline gate failed'}
}
$offset=0
if($Aggregate){
    if(!$r.aggregateProducts -or $r.embeddingChecks -ne 14 -or (Get-Content $r.runs[0].stdout -Tail 1) -cne 'PASS LEA SDK EMBEDDING 14'){throw 'Aggregate embedding gate differs'}
    $offset=1
}
$capture=@(Get-Content $r.runs[$offset].stdout)
if($capture[-1] -cne 'PASS Lea live styled capture and foundation'){throw 'Capture footer differs'}
$range=@(Get-Content $r.runs[1+$offset].stdout)
foreach($footer in @('PASS Lea print range: 37','PASS Lea printer geometry: 16','PASS Lea native printer selection: 27','PASS Lea prepared printer ticket: 22')){if(@($range|Where-Object {$_ -ceq $footer}).Count -ne 1){throw 'Range suite footer differs'}}
if($range[-1] -cne 'PASS Lea print range: 37'){throw 'Range terminal footer differs'}
$products=@{'Lea.Engine.dll'='DC71E2B9DA704135CA022E5BBC2F1C57BA8046621C41E40AB98C7DC354BD6213';'Lea.Win.dll'='6F907862F57C7640A8B0E7BE2C22DFEDD18B3966D472958FCFCEDA53A00F0F52';'Lea.Print.dll'='0113B106A84DDD4E19DFD6574AD13FD2511D34C17316ADC24062A73EB8021F3B'}
if($Aggregate){$products=@{'Lea.Engine.dll'='5B8A501DA627467A6C261A355917DE5F73DF8F00758B49C1D240CF7D596E8A4A';'Lea.Win.dll'='A622A1C266AD7CA27F437832E390E594F7E419CAFED700C9F466F0517970CE0D';'Lea.Print.dll'='519F26E0B6866CB666ED6D91B66515F4C0C7020074C6303E5AFAF2F8D1C15A70';'Lea.EmbeddingSample.exe'='358ED2BB00F2A4BDBFF4281F32F7519DB3C98792829C80DAD517F322CA9DEB5A'}}
if($Images){
    $products['Lea.Image.exe']='A4E44422DE7A0F07A3DFDE0C6827F97C6B818A35D94D8ABD54F4CBF1692E7E42'
    if($Aggregate){$products['Lea.Image.exe']='B5E1A90210B53FC3F957D0D8DF0E3EDBCA9D7AB7B825992A82C26648038BC8E7'}
    if(!$r.workerAclRestored -or (Get-Acl $r.workerPath).Sddl -cne $r.workerAclBefore){throw 'Print worker ACL differs'}
    $checks=Get-Content (Join-Path $directory 'friendly.json') -Raw|ConvertFrom-Json
    $live=Get-Content (Join-Path $directory 'live.json') -Raw|ConvertFrom-Json
    if(!$checks.passed -or $checks.baselineChecks -ne 40 -or $checks.friendlyChecks -ne 46 -or $checks.browserRouted -or $checks.printJobSubmitted -or !$live.passed -or $live.liveChecks -ne 250 -or $live.printJobSubmitted -or $live.hostRouted){throw 'Image print coverage differs'}
    $lines=@(Get-Content $r.runs[2+$offset].stdout)
    $terminal=$lines[-1]|ConvertFrom-Json
    if(!$terminal.passed -or $terminal.liveChecks -ne 250 -or $terminal.printJobSubmitted -or $terminal.hostRouted){throw 'Live print terminal footer differs'}
}
foreach($name in $products.Keys){
    $copy=Join-Path $directory $name;$rows=@($r.inputs|Where-Object path -ceq $copy)
    if($rows.Count -ne 1 -or $rows[0].sha256 -cne $products[$name] -or (Get-FileHash $copy).Hash -cne $products[$name]){throw 'Print tested binary differs'}
}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Print input changed'}}
$out=Join-Path $directory 'runtime-audit.json';if(Test-Path $out){throw 'Preserve print audit'}
@{verified=$true;receiptSha256=$pin;products=$products;rangeChecks=37;geometryChecks=16;selectionChecks=27;ticketChecks=22;completeSdk=$false;printerDispatchQualified=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 4|Set-Content $out -Encoding utf8
Get-FileHash $out
