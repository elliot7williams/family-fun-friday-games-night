param([switch]$KeepForegroundHost,[switch]$SourceBuild,[switch]$Package,[switch]$PrintSdk,[switch]$FullSdk,[switch]$ReproSdk)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$bundle='I:\LeaBuildStorage\portable-sdk-20261031-v1'
$out=Join-Path $root 'continuity-evidence/sdk-portable-images-v1'
if($SourceBuild){$out=Join-Path $root 'continuity-evidence/sdk-portable-source-v1'}
if($Package){$out=Join-Path $root ('continuity-evidence/sdk-package-'+$(if($SourceBuild){'source'}else{'runtime'})+'-v1')}
if($PrintSdk){if(!$SourceBuild){throw 'Print SDK requires source build mode'};$bundle='I:\LeaBuildStorage\portable-print-sdk-20261031-v1';$out=Join-Path $root ('continuity-evidence/sdk-print-'+$(if($Package){'package'}else{'aggregate'})+'-source-v1')}
if($FullSdk){if($SourceBuild -or $Package -or $PrintSdk){throw 'Full SDK image checks require separate exact-product runtime mode'};$bundle='I:\LeaBuildStorage\full-sdk-source-20261101-v1';$out=Join-Path $root 'continuity-evidence/full-sdk-view-images-v1'}
if($ReproSdk){if($SourceBuild -or $Package -or $PrintSdk -or $FullSdk){throw 'Repro SDK requires separate exact-product mode'};$FullSdk=$true;$bundle='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1\a';$out=Join-Path $root 'continuity-evidence/repro-sdk-view-images-v1'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained TTY owner required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence and reserve required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($pin -and $pin -cne $hash){throw 'Portable SDK input differs'};$inputs.Add(@{path=$path;sha256=$hash})}
New-Item -ItemType Directory -Path $out | Out-Null
$result=@{passed=$false;runs=$runs;inputs=$inputs;completeSdk=$false;securityQualified=$false}
try{
    Hold $PSCommandPath
    $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    if($Package){
        $archive='I:\LeaBuildStorage\sdk-aggregate-package-20261031-v1\Lea-SDK-development.zip'
        $archivePin='8E16F555B7D94D5ED65A02E5CD57EFBE30764EC8B092F404088A0A4C87F194EA';$entryCount=66
        if($PrintSdk){$archive='I:\LeaBuildStorage\sdk-print-aggregate-package-20261031-v1\Lea-SDK-development.zip';$archivePin='2B035A42A34A375393D5E4F4E2B27119363C81DF25239C3BF87412B4980C3133';$entryCount=93}
        Hold $archive $archivePin
        $bundle=Join-Path $out 'package'
        $zip=[IO.Compression.ZipFile]::OpenRead($archive)
        try{
            if($zip.Entries.Count -ne $entryCount -or @($zip.Entries.FullName|Sort-Object -Unique).Count -ne $entryCount){throw 'Package archive coverage differs'}
            foreach($entry in $zip.Entries){if([IO.Path]::IsPathRooted($entry.FullName) -or $entry.FullName.Contains(':') -or $entry.FullName.Contains('\') -or $entry.FullName.Split('/') -contains '..'){throw 'Package member escape'}}
        }finally{$zip.Dispose()}
        [IO.Compression.ZipFile]::ExtractToDirectory($archive,$bundle)
        $manifestPath=Join-Path $bundle 'package-manifest.json';Hold $manifestPath
        $packageManifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
        if($packageManifest.files.Count -ne ($entryCount-1)){throw 'Package manifest coverage differs'}
        foreach($file in $packageManifest.files){
            if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Contains('\') -or $file.path.Split('/') -contains '..'){throw 'Package inventory escape'}
            $path=Join-Path $bundle $file.path;Hold $path $file.sha256
            if((Get-Item $path).Length -ne $file.bytes){throw 'Extracted package length differs'}
        }
        $manifest=@{staged=$true;files=@($packageManifest.files|Where-Object {$_.path -notmatch '^evidence/' -and $_.path -cne 'PACKAGE_STATUS.md'})}
        $result.packageSha256=(Get-FileHash $archive).Hash;$result.packageExtractionVerified=$true
    }elseif($ReproSdk){
        $proof='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json';Hold $proof '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
        $built=Get-Content $proof -Raw|ConvertFrom-Json
        $members=@($built.products|Where-Object {$_.path -in 'bin/Release/Lea.Engine.dll','bin/Release/Lea.Win.dll','bin/Release/Lea.Image.exe'})
        if($members.Count -ne 3 -or @($members|Where-Object {!$_.equal -or $_.sha256 -cne $_.secondSha256}).Count){throw 'Repro image closure incomplete'}
        foreach($file in $members){Hold $file.first $file.sha256;Hold $file.second $file.sha256}
        $manifest=@{staged=$true;files=@(foreach($file in $members){@{path=$file.path;sha256=$file.sha256}})}
        $result.reproSdk=$true;$result.sourceBuiltProducts=$members
    }elseif($FullSdk){
        $proof='I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json';Hold $proof '3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'
        $built=Get-Content $proof -Raw|ConvertFrom-Json
        if(!$built.passed -or !$built.compiled -or !$built.inputsUnchanged -or $built.ownerAttention -or $built.libraryProducts.Count -ne 6){throw 'Full SDK library proof incomplete'}
        $members=@($built.libraryProducts|Where-Object {[IO.Path]::GetFileName($_.path) -in 'Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe'})
        if($members.Count -ne 3){throw 'Exact image closure missing'}
        $manifest=@{staged=$true;files=@(foreach($file in $members){@{path=('bin/Release/'+[IO.Path]::GetFileName($file.path));sha256=$file.sha256}})}
        $result.fullSdk=$true;$result.sourceBuiltProducts=$members
    }else{
        $manifestPath=Join-Path $bundle 'bundle-manifest.json';Hold $manifestPath
        $manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
    }
    if(!$manifest.staged -or $manifest.files.Count -ne $(if($FullSdk){3}elseif($PrintSdk){if($Package){86}else{76}}else{61})){throw 'Portable inventory differs'}
    if($PrintSdk -and !$Package -and (!$manifest.printIncluded -or $manifest.prebuiltBinaries -ne 0)){throw 'Print source-only manifest incomplete'}
    $target=Join-Path $out 'bundle';New-Item -ItemType Directory -Path $target | Out-Null
    foreach($file in $manifest.files){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split('/') -contains '..'){throw 'Portable member escape'}
        if($SourceBuild -and $file.path -match '(^|/)bin/'){continue}
        $from=Join-Path $bundle $file.path;Hold $from $file.sha256;$to=Join-Path $target $file.path
        New-Item -ItemType Directory -Path (Split-Path $to) -Force | Out-Null;Copy-Item $from $to;Hold $to $file.sha256
    }
    if($SourceBuild){
        if(@(Get-ChildItem $target -Recurse -File|Where-Object {$_.Extension -in @('.dll','.exe')}).Count){throw 'Fresh source build contains prebuilt binaries'}
        $expectedSources=if($PrintSdk){76}else{53}
        if(@(Get-ChildItem $target -Recurse -File).Count -ne $expectedSources){throw 'Fresh source-only inventory differs'}
        $result.prebuiltFilesExcluded=if($PrintSdk){if($Package){10}else{0}}else{8};$result.freshSourceFiles=$expectedSources;$result.printIncluded=[bool]$PrintSdk
        $dotnet='C:\Program Files\dotnet\dotnet.exe';Hold $dotnet
        $toolchain='C:\Program Files\dotnet\sdk\8.0.427'
        foreach($name in @('MSBuild.dll','Microsoft.Build.dll','Roslyn/bincore/csc.dll','Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $toolchain $name)}
        $aggregate=Invoke-OwnedTestRun -Name 'build-source-sdk' -Application $dotnet -Arguments @(('"'+(Join-Path $toolchain 'MSBuild.dll')+'"'),('"'+(Join-Path $target 'SDK.Build.proj')+'"'),'/nologo','/m:1','/nodeReuse:false','/v:minimal','/t:Build','/p:Configuration=Release','/p:UseSharedCompilation=false') -WorkingDirectory $target -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$aggregate.passed){throw 'Aggregate source SDK build failed'}
        $result.aggregateSourceBuildPassed=$true
    }
    $runtime=Join-Path $target 'Sample/bin/Release'
    if($FullSdk){$runtime=Join-Path $target 'bin/Release'}
    if($SourceBuild -or $Package){
        $components=@('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe');if($PrintSdk){$components+='Lea.Print.dll'}
        $result.sourceBuiltProducts=@(foreach($name in ($components+@('Lea.EmbeddingSample.exe'))){$path=Join-Path $runtime $name;Hold $path;@{path=$path;sha256=(Get-FileHash $path).Hash}})
        foreach($name in $components){
            $builtRoot=Join-Path $target ('bin/Release/'+$name);Hold $builtRoot
            if((Get-FileHash $builtRoot).Hash -cne (Get-FileHash (Join-Path $runtime $name)).Hash){throw 'Source sample copied component differs'}
        }
        $textEvidence=Join-Path $out 'text';New-Item -ItemType Directory -Path $textEvidence | Out-Null
        $fixture=Join-Path $runtime 'embedding.html';Hold $fixture
        $embedded=Invoke-OwnedTestRun -Name 'source-embedding' -Application (Join-Path $runtime 'Lea.EmbeddingSample.exe') -Arguments @('--test',('"'+$fixture+'"'),('"'+$textEvidence+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
        if(!$embedded.passed -or @((Get-Content $embedded.stdout)|Where-Object {$_ -ceq 'PASS LEA SDK EMBEDDING 14'}).Count -ne 1){throw 'Fresh source embedding failed'}
        $result.sourceEmbeddingChecks=14
    }
    $result.workerPath=Join-Path $runtime 'Lea.Image.exe';$result.workerAclBefore=(Get-Acl $result.workerPath).Sddl
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $exe=Join-Path $runtime 'Lea.Sdk.ImageProbe.exe'
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'))
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll')){$path=Join-Path $refs $name;Hold $path;$arguments+=('/reference:"'+$path+'"')}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll')){$arguments+=('/reference:"'+(Join-Path $runtime $name)+'"')}
    $source=Join-Path $root 'Lea.Tests/SdkImageViewMain.cs';Hold $source;$arguments+=('"'+$source+'"')
    $built=Invoke-OwnedTestRun -Name 'compile-image-view' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$built.passed){throw 'Portable image probe compilation failed'};Hold $exe
    $assets=Join-Path $out 'assets';New-Item -ItemType Directory -Path $assets | Out-Null
    $tested=Invoke-OwnedTestRun -Name 'image-view' -Application $exe -Arguments @('"'+$assets+'"') -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    $result.passed=$tested.passed -and @((Get-Content $tested.stdout)|Where-Object {$_ -ceq 'PASS SDK VIEW IMAGES 42'}).Count -eq 1
}catch{$result.error=$_.Exception.ToString()}
finally{
    if($result.workerPath -and (Test-Path $result.workerPath)){$result.workerAclRestored=(Get-Acl $result.workerPath).Sddl -ceq $result.workerAclBefore}
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and $result.workerAclRestored -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaPortableSdkLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,workerAclRestored,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
