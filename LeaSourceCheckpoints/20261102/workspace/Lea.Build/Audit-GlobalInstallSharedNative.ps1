$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$folder='I:/LeaBuildStorage/global-install-shared-native-20261102-v1'
$out='I:/LeaBuildStorage/global-install-shared-native-audit-20261102-v1'
if(Test-Path $out){throw 'Preserve shared native audit'}
$receipt=Join-Path $folder 'result.json'
if((Get-FileHash $receipt).Hash -cne 'EB81FF97CE8BCFCC677D5497A891F05090D050B204ABFAF1FF6C12089B6CF62B'){throw 'Native receipt differs'}
$r=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.completed -or !$r.nativeOnly -or !$r.runtimeReused -or $r.engineBinariesChanged -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 23 -or $r.runs.Count -ne 2 -or $r.images.Count -ne 3 -or $r.driverFilesAdded.Count -ne 2 -or $r.browserAdopted){throw 'Shared native qualification incomplete'}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Native input changed'}}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Native runtime bytes/ACL changed'}}
if(@(Get-ChildItem $r.runtime -Recurse -File).Count -ne 23){throw 'Native runtime inventory drift'}
$priorPath='I:/LeaBuildStorage/global-install-shared-dom-20261102-v1/result.json'
if((Get-FileHash $priorPath).Hash -cne 'DC438755F25368344B992F131C27DFE892DF74FB72EE2C39EE1AAC4E08C41106'){throw 'Prior DOM failure evidence changed'}
$prior=Get-Content $priorPath -Raw|ConvertFrom-Json
if($prior.passed -or $prior.completed -or $prior.before.Count -ne 21 -or $prior.cases[3].passed){throw 'Prior DOM failure must remain failed'}
foreach($file in $prior.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Original shared DOM member changed'}}
$added=@($r.before|Where-Object {$_.path -cnotin $prior.before.path}|Select-Object -ExpandProperty path)
if($added.Count -ne 2 -or @($added|Where-Object {$_ -cnotin $r.driverFilesAdded}).Count){throw 'Only native driver/config additions allowed'}
for($i=0;$i -lt 2;$i++){
    $run=$r.runs[$i];$budget=if($i -eq 0){120000}else{30000}
    if(!$run.passed -or $run.exitCode -ne 0 -or !$run.exitObserved -or !$run.outputDrained -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.childRetirementIndependentlyAudited -or $run.activeAfterRetirement -ne 0 -or $run.retainedForOwner -or $run.unsafeToContinue -or $run.timedOut -or $run.killRequested -or $run.parentBudgetMs -ne $budget -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Owned native result incomplete'}
}
$footer='PASS Lea isConnected native: inline lifecycle, real activation, field retention, detached editor cleanup, desktop/narrow render, guarded worker retirement.'
if(@(Get-Content $r.runs[1].stdout|Where-Object {$_ -ceq $footer}).Count -ne 1){throw 'Native semantic footer missing/duplicated'}
$images=[Collections.Generic.List[object]]::new()
foreach($name in @('connected-desktop.png','detached-desktop.png','detached-narrow.png')){
    $path=Join-Path $folder $name;$expected=@($r.images|Where-Object path -CEQ $path)
    if($expected.Count -ne 1 -or (Get-FileHash $path).Hash -cne $expected[0].sha256 -or (Get-Item $path).Length -gt 2MB){throw 'Native image identity/size differs'}
    $bytes=[IO.File]::ReadAllBytes($path)
    if($bytes.Length -lt 33 -or [Convert]::ToHexString($bytes[0..7]) -cne '89504E470D0A1A0A' -or [Text.Encoding]::ASCII.GetString($bytes,12,4) -cne 'IHDR'){throw 'PNG header differs'}
    $width=[Net.IPAddress]::NetworkToHostOrder([BitConverter]::ToInt32($bytes,16));$height=[Net.IPAddress]::NetworkToHostOrder([BitConverter]::ToInt32($bytes,20))
    $expectedWidth=if($name -ceq 'detached-narrow.png'){420}else{940}
    if($width -ne $expectedWidth -or $height -ne 700){throw 'Native image dimensions differ'}
    $images.Add(@{name=$name;width=$width;height=$height;sha256=$expected[0].sha256})
}
if((Get-FileHash (Join-Path $r.runtime 'Lea.Script.dll')).Hash -cne 'F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B' -or (Get-FileHash (Join-Path $r.runtime 'ScriptWorker/Lea.Script.Worker.dll')).Hash -cne 'A72EF9C070FCDE2BA57630FFFF6BEAD4ABD157F6365FA092C0A721AB3EACAFF4'){throw 'Native matched engine/worker differs'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Native evidence readback differs'}}
@{evidenceVerified=$true;nativeViewPassed=$true;runtimeFiles=23;originalDomFilesUnchanged=21;nativeDriverFilesAdded=2;images=$images;classListCampaignStillFailed=$true;hostAdopted=$false;startupReliabilityProven=$false;scope=$r.scope}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
