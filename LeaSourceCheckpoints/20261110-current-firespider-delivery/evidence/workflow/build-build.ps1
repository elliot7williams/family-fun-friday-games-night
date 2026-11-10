param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base=Join-Path $root 'continuity-evidence/relative-font-host-v1/host-source'
$sdk='I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1\source'
$out='I:\LeaBuildStorage\current-firespider-build-20261109-v1'
$stage=Join-Path $out 'source'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Get-PSDrive I).Free -lt 128MB -or (Test-Path -LiteralPath $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output and original reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new();$files=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
function Pin([string]$Path,[string]$Expected){
    $item=Get-Item -LiteralPath $Path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse source refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($Expected -and $hash -cne $Expected){throw ('Input differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$hash})
}
function Stage([string]$From,[string]$Relative,[string]$Hash,[string]$Kind){
    if([IO.Path]::IsPathRooted($Relative) -or $Relative.Contains(':') -or $Relative.Split('/') -contains '..' -or $files.ContainsKey($Relative)){throw 'Source path escape/collision'}
    Pin $From $Hash;$to=Join-Path $stage $Relative
    New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $From -Destination $to
    if((Get-FileHash -LiteralPath $to).Hash -cne $Hash){throw 'Source copy differs'};Pin $to $Hash
    $files.Add($Relative,@{path=$Relative;origin=$From;sha256=$Hash;kind=$Kind})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;products=@();browserExecuted=$false;installedBrowserChanged=$false;liveProjectChanged=$false;hostSourceUnchanged=$true;releaseQualified=$false}
$oldTemp=$env:TEMP;$oldTmp=$env:TMP
try{
    Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'build.ps1')
    $hostProof=Join-Path $root 'continuity-evidence/relative-font-host-v1/result.json';Pin $hostProof '452492446FCAE2FD9F2DFCC5A6185ED4F05B977B89FADD584A2B6DC83150A6F6'
    $hostRecord=Get-Content -LiteralPath $hostProof -Raw|ConvertFrom-Json
    if(!$hostRecord.passed -or !$hostRecord.inputsUnchanged -or $hostRecord.ownerAttention -or !$hostRecord.hostSourceInverseVerified -or !$hostRecord.commandSourceInverseVerified){throw 'Preserved migrated host source provenance required'}
    $auditPath='I:\LeaBuildStorage\sdk-source-full-outcome-20261109-v1\audit.json';Pin $auditPath '322CFA7E9F381B052687482A543DFE5009135DCC4EC0DE45DE6C900B6840B86C'
    $audit=Get-Content -LiteralPath $auditPath -Raw|ConvertFrom-Json
    if(!$audit.observationVerified -or !$audit.fullSuitePassed -or $audit.completedPhases -ne 89 -or !$audit.runtimeBytesAndAclUnchanged -or !$audit.traceEnvironmentRestored -or $audit.quarantineRequired){throw 'Exact current source SDK full campaign required'}
    $sdkProof='I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1\result.json';Pin $sdkProof '2A2A5DEC3B9919B235CDD1223447B99B60C85C585FFCBC43D40529F34D66BCA2'
    $sdkRecord=Get-Content -LiteralPath $sdkProof -Raw|ConvertFrom-Json
    if(!$sdkRecord.passed -or !$sdkRecord.inputsUnchanged -or $sdkRecord.ownerAttention -or $sdkRecord.libraryProducts.Count -ne 6 -or $sdkRecord.nativeProducts.Count -ne 6){throw 'Current SDK build proof incomplete'}
    $sources=@(Get-ChildItem -LiteralPath $base -Recurse -File|Where-Object {$_.FullName -notmatch '\\(bin|obj)\\' -and $_.Extension -notin '.dll','.exe','.pdb' -and !$_.FullName.StartsWith((Join-Path $base 'Lea/ScriptWorker')+'\',[StringComparison]::OrdinalIgnoreCase)})
    if($sources.Count -ne 87){throw 'Preserved host source inventory differs'}
    foreach($file in $sources){$pins=@($hostRecord.inputs|Where-Object path -CEQ $file.FullName|Select-Object -ExpandProperty sha256 -Unique);if($pins.Count -ne 1){throw 'Unique host source pin required'};Stage $file.FullName ([IO.Path]::GetRelativePath($base,$file.FullName).Replace('\','/')) $pins[0] 'preserved-host-source'}
    foreach($file in @($sdkRecord.libraryProducts)+@($sdkRecord.nativeProducts)){
        $relative='Lea/'+[IO.Path]::GetRelativePath((Join-Path $sdk 'bin/Release'),$file.path).Replace('\','/')
        Stage $file.path $relative $file.sha256 'current-sdk-product'
    }
    $sdkManifest='I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1\source.json';Pin $sdkManifest '51A823667E96D3ED4C3EC526E0747B8C19B1EBBE59A8A05324503DBEB4FED9C9'
    $sdkSource=Get-Content -LiteralPath $sdkManifest -Raw|ConvertFrom-Json
    foreach($file in $sdkSource.files|Where-Object {$_.path -match '^lib/[^/]+\.dll$'}){Stage (Join-Path $sdk $file.path) ('Lea/'+[IO.Path]::GetFileName($file.path)) $file.sha256 'framework-dependency'}
    if($files.Count -ne 105 -or @($files.Values|Where-Object kind -CEQ 'framework-dependency').Count -ne 6){throw 'Coherent 87source/12product/6dependency closure required'}
    $rows=@($files.Values|Sort-Object path)
    @{verified=$true;currentSourceSdk=$true;files=$rows;hostSourceUnchanged=$true;liveProjectChanged=$false;fullSuiteAuditSha256='322CFA7E9F381B052687482A543DFE5009135DCC4EC0DE45DE6C900B6840B86C';compiled=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'candidate-manifest.json') -Encoding utf8
    $compilerProof=Join-Path $root 'continuity-evidence/firespider-source-rebuild-v1/result.json';Pin $compilerProof '9DDF92B7273943C6CB5481A8CCC8033F09053BE2C5968DC5E53EDA14E0A5F3DA'
    $previous=Get-Content -LiteralPath $compilerProof -Raw|ConvertFrom-Json
    if(!$previous.passed -or !$previous.compilerPassed -or $previous.ownerAttention){throw 'Original host compiler provenance required'}
    $compiler=$previous.compiler;Pin $compiler.path $compiler.sha256
    $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Pin $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Pin $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $env:TEMP=Join-Path $out 'temp';$env:TMP=$env:TEMP;New-Item -ItemType Directory -Path $env:TEMP|Out-Null
    $bin=Join-Path $out 'bin'
    $arguments=@(('"'+(Join-Path $stage 'Firespider Internet.vbproj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:GenerateManifests=false','/p:AutoGenerateBindingRedirects=true','/p:LeaScriptEnabled=true','/p:UseSharedCompilation=false',('/p:OutputPath="'+$bin+'"'))
    $built=Invoke-OwnedTestRun -Name 'compile-current-firespider' -Application $compiler.path -Arguments $arguments -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$built.passed){throw 'Current Firespider compilation failed'}
    foreach($file in $files.Values|Where-Object kind -CNE 'preserved-host-source'){Pin (Join-Path $bin $file.path.Substring('Lea/'.Length)) $file.sha256}
    if(@(Get-ChildItem -LiteralPath (Join-Path $bin 'ScriptWorker') -File).Count -ne 6){throw 'Built six-file worker closure differs'}
    $result.products=@(Get-ChildItem -LiteralPath $bin -Recurse -File|ForEach-Object {Pin $_.FullName;@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;bytes=$_.Length}})
    if($result.products.Count -ne 22){throw 'Expected complete host output inventory differs'}
    $result.binary=Join-Path $bin 'Firespider Internet 19.exe';$result.hostSha256=(Get-FileHash -LiteralPath $result.binary).Hash;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $env:TEMP=$oldTemp;$env:TMP=$oldTmp
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaCurrentHostInputLeases=$holds}
    $result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,hostSha256,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
