param([switch]$KeepForegroundHost,[Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$PreparationSha256)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$preparation='I:\LeaBuildStorage\private-binding-full-preparation-20261103-v1'
$out='I:\LeaBuildStorage\private-binding-full-entry-20261103-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence and original reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse compiler input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    $actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($hash -and $actual -cne $hash){throw ('Entry compiler input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$actual})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;executed=$false;fullSuiteCompiled=$false;hostAdopted=$false;assertionsChanged=$false}
try{
    Hold $PSCommandPath
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'build.ps1')
    $proof=Join-Path $preparation 'preparation.json';Hold $proof $PreparationSha256
    $prepared=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$prepared.verified -or !$prepared.readyForHostCompilation -or !$prepared.notRunnable -or $prepared.files.Count -ne 305 -or $prepared.changes.Count -ne 17 -or $prepared.generated.Count -ne 2 -or $prepared.assertionsChanged -or !$prepared.fixturesUnchanged){throw 'Full source preparation incomplete'}
    foreach($file in @($prepared.files)+@($prepared.generated)){Hold (Join-Path $preparation $file.path) $file.sha256}
    $source=Join-Path $preparation 'WorkerEntryNativeHost.cs'
    $resource=Join-Path $preparation 'entry-fixture.json'
    $fixture=Get-Content -LiteralPath $resource -Raw|ConvertFrom-Json -AsHashtable
    $expected=Join-Path $root 'continuity-evidence/private-binding-full-suite-v1/runtime/WorkerEntryFixture'
    if($fixture.directory -cne $expected -or $fixture.files.Count -ne 7 -or $fixture.files['Lea.Script.Worker.dll'] -cne $prepared.workerSha256){throw 'Entry resource points to a different pair/location'}
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $compiler='C:\Program Files\dotnet\dotnet.exe';Hold $compiler
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $toolchain $name)}
    $exe=Join-Path $out 'WorkerEntryNativeHost.exe'
    $options=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/langversion:12.0','/optimize+','/deterministic+','/warnaserror+',('/out:"'+$exe+'"'),('/resource:"'+$resource+'",Lea.EntryFixture.json'))
    $references='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll')){$path=Join-Path $references $name;Hold $path;$options+=('/reference:"'+$path+'"')}
    $options+=('"'+$source+'"')
    $recipe=Join-Path $out 'entry.rsp';[IO.File]::WriteAllLines($recipe,$options,[Text.UTF8Encoding]::new($false));Hold $recipe
    $run=Invoke-OwnedTestRun -Name 'compile-private-binding-full-entry' -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$recipe+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$run.passed){throw 'Matching entry host compile failed'}
    Hold $exe
    $manifestPath=Join-Path $preparation 'runtime/WorkerEntryProbe/manifest.json'
    $spec=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json -AsHashtable
    if($spec.files['Lea.Script.dll'] -cne 'D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'){throw 'Prior entry manifest parent differs'}
    $spec.files['Lea.Script.dll']=$prepared.parentSha256
    $spec.files['WorkerEntryNativeHost.exe']=(Get-FileHash $exe).Hash
    $spec.fixtureFiles=$fixture.files
    $manifest=Join-Path $out 'entry-manifest.json';$spec|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $manifest -Encoding utf8;Hold $manifest
    $oldHash=(Get-FileHash $manifestPath).Hash;$newHash=(Get-FileHash $manifest).Hash
    $original=[IO.File]::ReadAllText((Join-Path $preparation 'compiled-source/source/Lea.Script.Tests/ModernComponents.cs'))
    if([regex]::Matches($original,$oldHash).Count -ne 1 -or $original.Contains($newHash)){throw 'Unique prior entry dispatcher identity required'}
    $updated=$original.Replace($oldHash,$newHash)
    if($updated.Replace($newHash,$oldHash) -cne $original){throw 'Entry dispatcher inverse differs'}
    $dispatcher=Join-Path $out 'ModernComponents.cs';[IO.File]::WriteAllText($dispatcher,$updated,[Text.UTF8Encoding]::new($false));Hold $dispatcher
    $result.outputs=@(foreach($path in @($exe,$manifest,$dispatcher)){@{path=$path;sha256=(Get-FileHash $path).Hash}})
    $result.fixtureDirectory=$fixture.directory
    $result.entryBinding=@{old=$oldHash;replacement=$newHash;inverseVerified=$true}
    $result.parentSha256=$prepared.parentSha256;$result.workerSha256=$prepared.workerSha256
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaPrivateBindingEntryLeases=$holds}
    $result|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
