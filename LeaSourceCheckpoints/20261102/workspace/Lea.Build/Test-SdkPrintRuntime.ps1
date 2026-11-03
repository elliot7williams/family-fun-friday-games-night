param([switch]$KeepForegroundHost,[switch]$Images,[switch]$Aggregate,[switch]$Package,[switch]$FullSdk,[switch]$ReproSdk)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/sdk-print-runtime-v1'
if($Images){$out=Join-Path $root 'continuity-evidence/sdk-print-images-v1'}
if($Aggregate){if(!$Images){throw 'Aggregate mode requires full image regression'};$out=Join-Path $root 'continuity-evidence/sdk-print-aggregate-runtime-v1'}
if($Package){if(!$Aggregate){throw 'Package mode requires full aggregate regression'};$out=Join-Path $root 'continuity-evidence/sdk-print-package-runtime-v1'}
if($FullSdk){if(!$Images -or $Aggregate -or $Package){throw 'Full SDK print checks require separate exact-product image mode'};$out=Join-Path $root 'continuity-evidence/full-sdk-print-runtime-v1'}
if($ReproSdk){if(!$Images -or $Aggregate -or $Package -or $FullSdk){throw 'Repro SDK requires separate image regression mode'};$out=Join-Path $root 'continuity-evidence/repro-sdk-print-runtime-v1'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence and reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw ('Input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
function CopyPinned([string]$from,[string]$name,[string]$pin){Hold $from $pin;$to=Join-Path $out $name;Copy-Item $from $to;Hold $to $pin}
New-Item -ItemType Directory -Path $out | Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;completeSdk=$false;browserAdopted=$false;printerDispatched=$false;globalSecurityQualified=$false}
try{
    Hold $PSCommandPath
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    if($Package){
        $archive='I:\LeaBuildStorage\sdk-print-aggregate-package-20261031-v1\Lea-SDK-development.zip'
        $archivePin='2B035A42A34A375393D5E4F4E2B27119363C81DF25239C3BF87412B4980C3133';Hold $archive $archivePin
        $packageRoot=Join-Path $out 'package'
        $zip=[IO.Compression.ZipFile]::OpenRead($archive)
        try{
            if($zip.Entries.Count -ne 93 -or @($zip.Entries.FullName|Sort-Object -Unique).Count -ne 93){throw 'Print archive coverage differs'}
            foreach($entry in $zip.Entries){if([IO.Path]::IsPathRooted($entry.FullName) -or $entry.FullName.Contains(':') -or $entry.FullName.Contains('\') -or $entry.FullName.Split('/') -contains '..'){throw 'Print archive path escape'}}
        }finally{$zip.Dispose()}
        [IO.Compression.ZipFile]::ExtractToDirectory($archive,$packageRoot)
        $manifestPath=Join-Path $packageRoot 'package-manifest.json';Hold $manifestPath
        $packageManifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
        if($packageManifest.files.Count -ne 92){throw 'Print package inventory differs'}
        foreach($file in $packageManifest.files){
            if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Contains('\') -or $file.path.Split('/') -contains '..'){throw 'Print package path escape'}
            $path=Join-Path $packageRoot $file.path;Hold $path $file.sha256
            if((Get-Item $path).Length -ne $file.bytes){throw 'Print package length differs'}
        }
        $result.packageExtractionVerified=$true;$result.packageSha256=$archivePin
    }
    $buildPath='I:\LeaBuildStorage\core-view-sdk-build-20261031-v3-print-library\result.json';$buildPin='BC0C7001376EC1FF19FFCBEF36987F0A99282A6A79E30421EE4937F6F1B46E55'
    if($Aggregate){$buildPath=Join-Path $root 'continuity-evidence/sdk-print-aggregate-source-v1/result.json';$buildPin='47C4617143CFF63510728C88587E278C423F9E3C7AFA92629BA5D29FFD48DDA6'}
    if($Package){$buildPath=Join-Path $packageRoot 'evidence/source-build.json'}
    if($FullSdk){$buildPath='I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json';$buildPin='3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'}
    if($ReproSdk){$buildPath='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json';$buildPin='39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'}
    Hold $buildPath $buildPin
    $build=Get-Content $buildPath -Raw|ConvertFrom-Json
    if($ReproSdk){
        $reproMembers=@($build.products|Where-Object {$_.path -in 'bin/Release/Lea.Engine.dll','bin/Release/Lea.Win.dll','bin/Release/Lea.Print.dll','bin/Release/Lea.Image.exe'})
        if($reproMembers.Count -ne 4 -or @($reproMembers|Where-Object {!$_.equal -or $_.sha256 -cne $_.secondSha256}).Count){throw 'Repro print closure incomplete'}
        foreach($member in $reproMembers){Hold $member.first $member.sha256;Hold $member.second $member.sha256}
        $result.reproSdk=$true
    }elseif($FullSdk){
        if(!$build.passed -or !$build.compiled -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.libraryProducts.Count -ne 6){throw 'Full SDK library build incomplete'}
        $result.fullSdk=$true
    }elseif($Aggregate){
        if(!$build.aggregateSourceBuildPassed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.runs.Count -ne 1 -or !$build.runs[0].passed -or $build.runs[0].exitCode -ne 0 -or $build.runs[0].timedOut -or $build.runs[0].killRequested -or $build.sourceBuiltProducts.Count -ne 5 -or $build.freshSourceFiles -ne 76 -or $build.prebuiltFilesExcluded -ne 0){throw 'Aggregate compile proof incomplete'}
    }elseif(!$build.passed -or !$build.printLibraryCompiled -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.outputs.Count -ne 1){throw 'SDK print build incomplete'}
    $bindings=@(
        @('Lea.Engine.dll','DC71E2B9DA704135CA022E5BBC2F1C57BA8046621C41E40AB98C7DC354BD6213'),
        @('Lea.Win.dll','6F907862F57C7640A8B0E7BE2C22DFEDD18B3966D472958FCFCEDA53A00F0F52'),
        @('Lea.Print.dll','0113B106A84DDD4E19DFD6574AD13FD2511D34C17316ADC24062A73EB8021F3B')
    )
    $binaryRoot='I:\LeaBuildStorage\sdk-print-20261031-v2\bin\Release'
    if($Aggregate){
        $binaryRoot=Join-Path $root 'continuity-evidence/sdk-print-aggregate-source-v1/bundle/Sample/bin/Release'
        $bindings=@(
            @('Lea.Engine.dll','5B8A501DA627467A6C261A355917DE5F73DF8F00758B49C1D240CF7D596E8A4A'),
            @('Lea.Win.dll','A622A1C266AD7CA27F437832E390E594F7E419CAFED700C9F466F0517970CE0D'),
            @('Lea.Print.dll','519F26E0B6866CB666ED6D91B66515F4C0C7020074C6303E5AFAF2F8D1C15A70')
        )
        $result.aggregateProducts=$true
    }
    if($Package){$binaryRoot=Join-Path $packageRoot 'Sample/bin/Release'}
    if($FullSdk){
        $binaryRoot='I:\LeaBuildStorage\full-sdk-source-20261101-v1/bin/Release'
        $bindings=@(
            @('Lea.Engine.dll','13F43BE5F10072EEFDE7CE1DF06EAEDCEDF8AA3375D24A4548DD4B6BF6E1D365'),
            @('Lea.Win.dll','629D7AA070C082853A2BD1F3A02E016685E348CA3A127A9DC4FFE01CD06C039D'),
            @('Lea.Print.dll','267DB69A8515E19A1AF54210EE058B3AFF8A498B5EBF3828332886BE0123354B')
        )
    }
    if($ReproSdk){
        $binaryRoot='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/a/bin/Release'
        $bindings=@(foreach($member in $reproMembers|Where-Object path -cne 'bin/Release/Lea.Image.exe'){,@([IO.Path]::GetFileName($member.path),$member.sha256)})
    }
    foreach($pair in $bindings){CopyPinned (Join-Path $binaryRoot $pair[0]) $pair[0] $pair[1]}
    if($Aggregate){
        CopyPinned (Join-Path $binaryRoot 'Lea.EmbeddingSample.exe') 'Lea.EmbeddingSample.exe' '358ED2BB00F2A4BDBFF4281F32F7519DB3C98792829C80DAD517F322CA9DEB5A'
        $fixture=Join-Path $binaryRoot 'embedding.html'
        $pins=@($build.inputs|Where-Object path -ceq $fixture|Select-Object -ExpandProperty sha256 -Unique)
        if($Package){$pins=@($packageManifest.files|Where-Object path -ceq 'Sample/bin/Release/embedding.html'|Select-Object -ExpandProperty sha256 -Unique)}
        if($pins.Count -ne 1){throw 'Aggregate sample fixture pin missing'}
        CopyPinned $fixture 'embedding.html' $pins[0]
        $text=Join-Path $out 'text';New-Item -ItemType Directory -Path $text|Out-Null
        $embed=Invoke-OwnedTestRun -Name 'aggregate-embedding' -Application (Join-Path $out 'Lea.EmbeddingSample.exe') -Arguments @('--test',('"'+(Join-Path $out 'embedding.html')+'"'),('"'+$text+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
        if(!$embed.passed -or @(Get-Content $embed.stdout|Where-Object {$_ -ceq 'PASS LEA SDK EMBEDDING 14'}).Count -ne 1){throw 'Aggregate embedding failed'}
        $result.embeddingChecks=14
    }
    $baseline=Join-Path $root 'continuity-evidence/relative-font-print-project-v1'
    $proofPath=Join-Path $baseline 'result.json';Hold $proofPath 'A71A3BC94E9584876DCE5480027EBAD4585A2900E6F01651C65D4869FA2D8002'
    $proof=Get-Content $proofPath -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Prior test baseline incomplete'}
    foreach($name in @('PrintTextChecks.exe','Lea.Print.Range.Tests.exe','native-selection.html','print-document.html','print-styles.html','print-styled-document.html')){
        $from=Join-Path $baseline ('bin/'+$name)
        $pins=@($proof.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw ('Test baseline pin missing: '+$name)}
        CopyPinned $from $name $pins[0]
    }
    $capture=Invoke-OwnedTestRun -Name 'sdk-print-capture' -Application (Join-Path $out 'PrintTextChecks.exe') -Arguments @('"'+(Join-Path $out 'native-selection.html')+'"') -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    $lines=@(Get-Content $capture.stdout)
    if(!$capture.passed -or $lines[-1] -cne 'PASS Lea live styled capture and foundation'){throw 'SDK print capture failed'}
    $result.capturePassed=$true
    $range=Invoke-OwnedTestRun -Name 'sdk-print-range' -Application (Join-Path $out 'Lea.Print.Range.Tests.exe') -Arguments @() -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    $lines=@(Get-Content $range.stdout)
    foreach($footer in @('PASS Lea print range: 37','PASS Lea printer geometry: 16','PASS Lea native printer selection: 27','PASS Lea prepared printer ticket: 22')){
        if(@($lines|Where-Object {$_ -ceq $footer}).Count -ne 1){throw ('SDK print footer missing: '+$footer)}
    }
    $result.passed=$range.passed -and $lines[-1] -ceq 'PASS Lea print range: 37'
    if($Images){
        if(!$result.passed){throw 'Prior print range suite failed'}
        $imageProofPath=Join-Path $root 'continuity-evidence/relative-font-image-live-v1/result.json'
        Hold $imageProofPath '41EB69CB05327FF8FE6C4226543EBE2ED75B8662E3DBB8B709B528B19D4BF1CA'
        $imageProof=Get-Content $imageProofPath -Raw|ConvertFrom-Json
        if(!$imageProof.passed -or !$imageProof.inputsUnchanged -or $imageProof.ownerAttention -or !$imageProof.decoderAclRestored){throw 'Image test baseline incomplete'}
        $from=Join-Path $root 'continuity-evidence/relative-font-image-live-v1/PrintFriendlyChecks.exe'
        $pins=@($imageProof.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Image test executable pin missing'}
        CopyPinned $from 'PrintFriendlyChecks.exe' $pins[0]
        foreach($name in @('print-friendly.html','print-cache-image.html','document-selection.html')){
            $from=Join-Path $root ('LeaForge/test-pages/'+$name)
            $pins=@($imageProof.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
            if($pins.Count -ne 1){throw 'Image test fixture pin missing'}
            CopyPinned $from $name $pins[0]
        }
        $worker=Join-Path $out 'Lea.Image.exe'
        $workerSource=Join-Path $root 'continuity-evidence/sdk-portable-source-v1/bundle/Sample/bin/Release/Lea.Image.exe';$workerPin='A4E44422DE7A0F07A3DFDE0C6827F97C6B818A35D94D8ABD54F4CBF1692E7E42'
        if($Aggregate){$workerSource=Join-Path $binaryRoot 'Lea.Image.exe';$workerPin='B5E1A90210B53FC3F957D0D8DF0E3EDBCA9D7AB7B825992A82C26648038BC8E7'}
        if($FullSdk){$workerSource=Join-Path $binaryRoot 'Lea.Image.exe';$workerPin='9EB5E1EBA75BE747A45D23B16D95800DD424CDD77978E014164646CE17BD29FD'}
        if($ReproSdk){$workerSource=Join-Path $binaryRoot 'Lea.Image.exe';$workerPin=($reproMembers|Where-Object path -ceq 'bin/Release/Lea.Image.exe').sha256}
        CopyPinned $workerSource 'Lea.Image.exe' $workerPin
        $result.workerPath=$worker;$result.workerAclBefore=(Get-Acl $worker).Sddl
        $imageRun=Invoke-OwnedTestRun -Name 'sdk-print-images' -Application (Join-Path $out 'PrintFriendlyChecks.exe') -Arguments @(('"'+(Join-Path $out 'print-friendly.html')+'"'),('"'+$out+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
        $result.checks=Get-Content (Join-Path $out 'friendly.json') -Raw|ConvertFrom-Json
        $result.live=Get-Content (Join-Path $out 'live.json') -Raw|ConvertFrom-Json
        $result.passed=$imageRun.passed -and $result.checks.passed -and $result.checks.baselineChecks -eq 40 -and $result.checks.friendlyChecks -eq 46 -and !$result.checks.browserRouted -and !$result.checks.printJobSubmitted -and $result.live.passed -and $result.live.liveChecks -eq 250 -and !$result.live.printJobSubmitted -and !$result.live.hostRouted
    }
}catch{$result.error=$_.Exception.ToString()}
finally{
    if($result.workerPath){$result.workerAclRestored=(Get-Acl $result.workerPath).Sddl -ceq $result.workerAclBefore;$result.passed=$result.passed -and $result.workerAclRestored}
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSdkPrintLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,capturePassed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
