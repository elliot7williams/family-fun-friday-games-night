param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy'
$base='I:\LeaBuildStorage\private-binding-full-assembly-20261103-v5-assets'
$sdk='I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1\source'
$out='I:\LeaBuildStorage\memory-policy-full-preparation-20261110-v1'
$assembly=Join-Path $out 'assembly'
$candidate=Join-Path $root 'continuity-evidence/memory-policy-full-suite-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 700MB -or (Get-PSDrive I).Free -lt 160MB -or (Test-Path -LiteralPath $out) -or (Test-Path -LiteralPath $candidate) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/outputs and reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$Path,[string]$Expected){
    $item=Get-Item -LiteralPath $Path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($Expected -and $hash -cne $Expected){throw ('Input differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$hash})
}
function Copy-Pinned([string]$From,[string]$To,[string]$Expected){Pin $From $Expected;New-Item -ItemType Directory -Path (Split-Path $To -Parent) -Force|Out-Null;Copy-Item -LiteralPath $From -Destination $To;if((Get-FileHash -LiteralPath $To).Hash -cne (Get-FileHash -LiteralPath $From).Hash){throw 'Copy readback differs'}}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;engineExecuted=$false;assertionsChanged=$false;limitsChanged=$false;completeSdk=$false;hostAdopted=$false}
$oldTemp=$env:TEMP;$oldTmp=$env:TMP
try{
    Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'prepare.ps1')
    $basePath=Join-Path $base 'assembly.json';Pin $basePath 'F647679D939FD32355585B61561761BAFAB7B6239D83601A0FFE23B0BCF21DEA'
    $prior=Get-Content -LiteralPath $basePath -Raw|ConvertFrom-Json
    if(!$prior.verified -or $prior.files.Count -ne 305 -or $prior.compiledSources -ne 60 -or $prior.fixtures -ne 182 -or !$prior.baselineTestBinariesExcluded -or $prior.assertionsChanged){throw 'Original full suite inventory differs'}
    $auditPath='I:\LeaBuildStorage\memory-policy-sdk-audit-20261110-v1\audit.json';Pin $auditPath 'FD6BBBC7479A5F26F983E180D0C4BBA1EE75582C28F1F692F3D61C63DE52B3DC'
    $sdkAudit=Get-Content -LiteralPath $auditPath -Raw|ConvertFrom-Json
    if(!$sdkAudit.verified -or $sdkAudit.libraryProducts.Count -ne 6 -or $sdkAudit.nativeProducts.Count -ne 6){throw 'Current source products required'}
    $members=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $prior.files){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split('/') -contains '..' -or $members.ContainsKey($file.path)){throw 'Assembly member escape/collision'}
        Copy-Pinned (Join-Path $base $file.path) (Join-Path $assembly $file.path) $file.sha256
        $members.Add($file.path,@{path=$file.path;sha256=$file.sha256})
    }
    $replacements=[Collections.Generic.List[object]]::new()
    foreach($product in $sdkAudit.libraryProducts){
        $name=[IO.Path]::GetFileName($product.path);if($name -ceq 'Lea.Print.dll'){continue}
        $relative='runtime/'+$name;if(!$members.ContainsKey($relative)){throw 'Unexpected source product role'}
        $replacements.Add(@{path=$relative;oldSha256=$members[$relative].sha256;sha256=$product.sha256})
        Copy-Pinned (Join-Path $sdk ('bin/Release/'+$name)) (Join-Path $assembly $relative) $product.sha256;$members[$relative].sha256=$product.sha256
    }
    foreach($product in $sdkAudit.nativeProducts){
        $relative='runtime/ScriptWorker/'+[IO.Path]::GetFileName($product.path)
        if(!$members.ContainsKey($relative) -or $members[$relative].sha256 -cne $product.sha256){throw 'Changed native worker requires separately rebound component fixtures'}
        Copy-Pinned $product.path (Join-Path $assembly $relative) $product.sha256
    }
    if($replacements.Count -ne 5){throw 'Five engine/view/script/image product roles required'}
    $recovery='I:\LeaBuildStorage\signed-suite-recovery-complete-20261108-v1'
    $entrySource=Join-Path $out 'WorkerEntryNativeHost.cs'
    Copy-Pinned (Join-Path $recovery 'WorkerEntryNativeHost.cs') $entrySource '28EF2916951EE0C5ABA9E30D39FF20549119898292AC2BDAF5FCFBA7D105EFB8'
    Pin $entrySource '28EF2916951EE0C5ABA9E30D39FF20549119898292AC2BDAF5FCFBA7D105EFB8'
    $originalResource=Join-Path $recovery 'entry-fixture.json';Pin $originalResource '64C6F06B04EF62D6541A6E155943EA14C32824E41F5ADC3DE4D87A668FE4C046'
    $fixture=Get-Content -LiteralPath $originalResource -Raw|ConvertFrom-Json -AsHashtable
    if($fixture.files.Count -ne 7 -or $fixture.files['Lea.Script.Worker.dll'] -cne 'F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0'){throw 'Entry fixture identity differs'}
    foreach($name in $fixture.files.Keys){if([IO.Path]::GetFileName($name) -cne $name -or $members['runtime/WorkerEntryFixture/'+$name].sha256 -cne $fixture.files[$name]){throw 'Entry fixture table differs'}}
    $fixture.directory=Join-Path $candidate 'runtime/WorkerEntryFixture'
    $resource=Join-Path $out 'entry-fixture.json';$fixture|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $resource -Encoding utf8;Pin $resource
    $owner=Join-Path 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\Lea.Build' 'OwnedTestProcess.cs';Pin $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
    $helper=Join-Path 'C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\Lea.Build' 'Invoke-OwnedTestRun.ps1';Pin $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet;$tools='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Pin (Join-Path $tools $name)}
    $env:TEMP=Join-Path $out 'temp';$env:TMP=$env:TEMP;New-Item -ItemType Directory -Path $env:TEMP|Out-Null
    $entry=Join-Path $out 'WorkerEntryNativeHost.exe'
    $options=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/langversion:12.0','/optimize+','/deterministic+','/warnaserror+',('/out:"'+$entry+'"'),('/resource:"'+$resource+'",Lea.EntryFixture.json'))
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll')){$path=Join-Path $refs $name;Pin $path;$options+=('/reference:"'+$path+'"')};$options+=('"'+$entrySource+'"')
    $rsp=Join-Path $out 'entry.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));Pin $rsp
    $built=Invoke-OwnedTestRun -Name 'compile-current-full-entry' -Application $dotnet -Arguments @(('"'+(Join-Path $tools 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$built.passed){throw 'Fresh entry host compilation failed'};Pin $entry
    $entryManifestPath=Join-Path $assembly 'runtime/WorkerEntryProbe/manifest.json'
    $oldManifestHash=$members['runtime/WorkerEntryProbe/manifest.json'].sha256
    $entryManifest=Get-Content -LiteralPath $entryManifestPath -Raw|ConvertFrom-Json -AsHashtable
    $entryManifest.files['WorkerEntryNativeHost.exe']=(Get-FileHash -LiteralPath $entry).Hash;$entryManifest.fixtureFiles=$fixture.files
    $entryManifest|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $entryManifestPath -Encoding utf8
    Copy-Item -LiteralPath $entry -Destination (Join-Path $assembly 'runtime/WorkerEntryProbe/WorkerEntryNativeHost.exe')
    foreach($name in @('manifest.json','WorkerEntryNativeHost.exe')){$relative='runtime/WorkerEntryProbe/'+$name;$members[$relative].sha256=(Get-FileHash -LiteralPath (Join-Path $assembly $relative)).Hash}
    $newManifestHash=$members['runtime/WorkerEntryProbe/manifest.json'].sha256
    $dispatcher='compiled-source/source/Lea.Script.Tests/ModernComponents.cs';$dispatcherPath=Join-Path $assembly $dispatcher
    $original=[IO.File]::ReadAllText($dispatcherPath)
    function Bind-Manifest([string]$Text){if([regex]::Matches($Text,$oldManifestHash).Count -ne 1 -or $Text.Contains($newManifestHash)){throw 'Unique entry manifest constant required'};return $Text.Replace($oldManifestHash,$newManifestHash)}
    $updated=Bind-Manifest $original
    if($updated.Replace($newManifestHash,$oldManifestHash) -cne $original){throw 'Dispatcher identity-only inverse differs'}
    $negative=0;foreach($bad in @($original.Replace($oldManifestHash,'MISSING'),($original+$oldManifestHash),($original+$newManifestHash))){try{$null=Bind-Manifest $bad}catch{$negative++}}
    if($negative -ne 3){throw 'Dispatcher negative controls failed'}
    [IO.File]::WriteAllText($dispatcherPath,$updated,[Text.UTF8Encoding]::new($false));$members[$dispatcher].sha256=(Get-FileHash -LiteralPath $dispatcherPath).Hash
    $baselinePath='I:\LeaBuildStorage\sdk-full-script-preparation-20261101-v1\evidence\baseline-build.json';Pin $baselinePath '39B11B7FFB696DED434C9E0313DEED6744E0A3A08590BB8B14977C064EA6608C'
    $baseline=Get-Content -LiteralPath $baselinePath -Raw|ConvertFrom-Json;$oldRoot=Split-Path $baseline.runtime -Parent
    $oldRecipe='I:\LeaBuildStorage\sdk-full-script-preparation-20261101-v1\evidence\baseline-tests.rsp';Pin $oldRecipe 'DF5DEEA4C810B46BB1A8D7925E9BC676EDF5EFBDA201EFC29431B1FE6C64F048'
    $arguments=[Collections.Generic.List[string]]::new();$sources=0;$references=0;$outputs=0;$symbols=0
    foreach($line in Get-Content -LiteralPath $oldRecipe){
        if($line -match '^"(.+\.cs)"$'){$relative='compiled-source/'+[IO.Path]::GetRelativePath($oldRoot,$Matches[1]).Replace('\','/');if(!$members.ContainsKey($relative)){throw 'Missing compiled source'};$arguments.Add('"'+(Join-Path $assembly $relative)+'"');$sources++;continue}
        if($line -match '^/reference:"(.+)"$'){
            $path=$Matches[1]
            if($path.StartsWith($baseline.runtime+'\',[StringComparison]::OrdinalIgnoreCase)){$relative='runtime/'+[IO.Path]::GetRelativePath($baseline.runtime,$path).Replace('\','/');if(!$members.ContainsKey($relative)){throw 'Missing runtime reference'};$path=Join-Path $assembly $relative}
            else{$pins=@($baseline.inputs|Where-Object path -CEQ $path|Select-Object -ExpandProperty sha256 -Unique);if($pins.Count -ne 1){throw 'Reference provenance absent'};Pin $path $pins[0]}
            $arguments.Add('/reference:"'+$path+'"');$references++;continue
        }
        if($line.StartsWith('/out:')){$arguments.Add('/out:"'+(Join-Path $out 'Lea.Script.Tests.exe')+'"');$outputs++;continue}
        if($line.StartsWith('/pdb:')){$arguments.Add('/pdb:"'+(Join-Path $out 'Lea.Script.Tests.pdb')+'"');$symbols++;continue}
        if(!$line.StartsWith('/')){throw 'Unknown compiler recipe member'};$arguments.Add($line)
    }
    if($sources -ne 60 -or $references -ne 193 -or $outputs -ne 1 -or $symbols -ne 1){throw 'Original compiler recipe inventory differs'}
    foreach($file in $members.Values){Pin (Join-Path $assembly $file.path) $file.sha256}
    $rsp=Join-Path $out 'tests.rsp';[IO.File]::WriteAllLines($rsp,$arguments,[Text.UTF8Encoding]::new($false));Pin $rsp
    $compiled=Invoke-OwnedTestRun -Name 'compile-current-full-suite' -Application $dotnet -Arguments @(('"'+(Join-Path $tools 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Current full-suite compilation failed'}
    $rows=[Collections.Generic.List[object]]::new();foreach($file in $members.Values|Where-Object {$_.path -match '^(runtime|fixtures)/'}){$rows.Add($file)}
    foreach($name in @('Lea.Script.Tests.exe','Lea.Script.Tests.pdb')){$path=Join-Path $out $name;Pin $path;$rows.Add(@{path=('runtime/'+$name);sha256=(Get-FileHash -LiteralPath $path).Hash})}
    if($rows.Count -ne 247 -or @($rows|Where-Object {$_.path.StartsWith('runtime/')} ).Count -ne 65){throw 'Full runtime/fixture coverage differs'}
    $bytes=($rows|ForEach-Object {$path=if($_.path -match '^runtime/Lea.Script.Tests\.(exe|pdb)$'){Join-Path $out ([IO.Path]::GetFileName($_.path))}else{Join-Path $assembly $_.path};(Get-Item -LiteralPath $path).Length}|Measure-Object -Sum).Sum
    if((Get-PSDrive C).Free -lt (600MB+$bytes) -or (Get-Volume -DriveLetter C).FileSystem -cne 'NTFS'){throw 'NTFS plus actual allocation/original reserve required'}
    foreach($file in $rows){$from=if($file.path -match '^runtime/Lea.Script.Tests\.(exe|pdb)$'){Join-Path $out ([IO.Path]::GetFileName($file.path))}else{Join-Path $assembly $file.path};Copy-Pinned $from (Join-Path $candidate $file.path) $file.sha256}
    $runtime=Join-Path $candidate 'runtime'
    $manifests=@(Get-ChildItem -LiteralPath (Join-Path $runtime 'ComponentProbes') -Filter manifest.json -Recurse -File)+@(Get-Item -LiteralPath (Join-Path $runtime 'WorkerEntryProbe/manifest.json'))
    if($manifests.Count -ne 6){throw 'Six component manifests required'}
    foreach($file in $manifests){$spec=Get-Content -LiteralPath $file.FullName -Raw|ConvertFrom-Json -AsHashtable;foreach($name in $spec.files.Keys){if([IO.Path]::GetFileName($name) -cne $name){throw 'Component path escape'};Pin (Join-Path $file.DirectoryName $name) $spec.files[$name]}}
    $reader='I:\LeaBuildStorage\sdk-source-full-tools-20261109-v1\ReadEmbeddedEntryResource.cs';Pin $reader '0F4A2FA89F91C0142435A975FEE1EE0331803E6EFFDD4896D7BDFA9E1B874644';Add-Type -Path $reader
    $embedded=[ReadEmbeddedEntryResource]::Read((Join-Path $runtime 'WorkerEntryProbe/WorkerEntryNativeHost.exe'))|ConvertFrom-Json -AsHashtable
    if($embedded.directory -cne $fixture.directory -or $embedded.files.Count -ne 7){throw 'Embedded fixture path differs'}
    foreach($name in $embedded.files.Keys){if($fixture.files[$name] -cne $embedded.files[$name]){throw 'Embedded fixture identity differs'};Pin (Join-Path $embedded.directory $name) $embedded.files[$name]}
    $staging=Join-Path $candidate 'staging.json'
    @{verified=$true;executed=$false;currentSdkSourceCandidate=$true;diagnosticOnly=$false;files=$rows;runtime=$runtime;binary=(Join-Path $runtime 'Lea.Script.Tests.exe');fixtureSource=(Join-Path $candidate 'fixtures');compiledSources=60;componentManifests=6;requiredChecks=3412;requiredPhases=89;assertionsChanged=$false;limitsChanged=$false;sourceSdkAuditSha256='FD6BBBC7479A5F26F983E180D0C4BBA1EE75582C28F1F692F3D61C63DE52B3DC'}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $staging -Encoding utf8
    @{verified=$true;currentSdkSourceCandidate=$true;diagnosticOnly=$false;stagingReceiptSha256=(Get-FileHash -LiteralPath $staging).Hash;embeddedFixtureReadWithoutLoadingHost=$true;embeddedFiles=7;embeddedDirectory=$fixture.directory;selectedFiles=$rows.Count;testBinarySha256=(Get-FileHash -LiteralPath (Join-Path $runtime 'Lea.Script.Tests.exe')).Hash;assertionsChanged=$false;limitsChanged=$false}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $candidate 'staging-audit.json') -Encoding utf8
    $result.members=@($members.Values|Sort-Object path);$result.replacements=$replacements;$result.dispatcherInverseVerified=$true;$result.dispatcherNegativeControls=$negative;$result.compiledSources=$sources;$result.references=$references;$result.candidate=$candidate;$result.stagingSha256=(Get-FileHash -LiteralPath $staging).Hash;$result.auditSha256=(Get-FileHash -LiteralPath (Join-Path $candidate 'staging-audit.json')).Hash;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $env:TEMP=$oldTemp;$env:TMP=$oldTmp
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaCurrentFullPreparationLeases=$holds}
    $result|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,compiledSources,references,stagingSha256,auditSha256,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
