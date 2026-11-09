param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\current-bootstrap-profile-run-20261109-v1'
$runtime=Join-Path $root 'continuity-evidence/current-bootstrap-profile-v1/runtime'
$baseline=Join-Path $root 'continuity-evidence/private-binding-full-suite-v5'
$recovery='I:\LeaBuildStorage\signed-font-startup-investigation-20261108-v1'
$profile='I:\LeaBuildStorage\current-bootstrap-profile-source-20261109-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path $out) -or (Test-Path $runtime) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence/runtime required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($expected -and $hash -cne $expected){throw ('Profile input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
if((Get-PSDrive C).Free -lt 604MB -or (Get-PSDrive I).Free -lt 64MB){throw 'Original600MiB reserve plus diagnostic closure required'}
New-Item -ItemType Directory -Path $out,$runtime|Out-Null
$result=@{passed=$false;diagnosticOnly=$true;runtimeQualified=$false;hostAdopted=$false;inputs=$inputs;runs=$runs;before=@();historicalMetadataRecovered=$false;metadataReplacementExplicit=$true;limitsChanged=$false}
try{
    Pin $PSCommandPath
    $sourceProof=Join-Path $profile 'source.json';Pin $sourceProof 'D74BB7C6812012197DFC11A803C13E493796D40685E953E1B6194ED9624F2968'
    $plan=Get-Content $sourceProof -Raw|ConvertFrom-Json
    if(!$plan.verified -or !$plan.inverseVerified -or !$plan.criticalOperationsVerified -or $plan.negativeControls -ne 6 -or $plan.pairShapeControls -ne 1 -or $plan.budgetsChanged -or $plan.files.Count -ne 2){throw 'Operation-preserving diagnostic proof required'}
    foreach($file in $plan.files){Pin $file.path $file.sha256}
    $buildProof=Join-Path $recovery 'parent-build.json';Pin $buildProof 'D32732C0BF3B548DB649966100D4B074663E07544BC9C25A648A80D6723AB02C'
    $build=Get-Content $buildProof -Raw|ConvertFrom-Json
    $stageProof=Join-Path $baseline 'staging.json';Pin $stageProof '591FBFFC8D0A57136FEC81442A392BE1B8F14ABC9931D7DCC9E41FD14B3F7B5B'
    $stage=Get-Content $stageProof -Raw|ConvertFrom-Json
    $dependencies=@('Acornima.dll','Jint.dll','Lea.Engine.dll','System.Buffers.dll','System.Memory.dll','System.Numerics.Vectors.dll','System.Runtime.CompilerServices.Unsafe.dll')
    $closureBytes=0L
    foreach($name in $dependencies){$closureBytes+=(Get-Item -LiteralPath (Join-Path $stage.runtime $name)).Length}
    if((Get-PSDrive C).Free -lt (600MB+$closureBytes+512KB)){throw 'Full actual diagnostic allocation plus reserve required'}
    foreach($name in $dependencies){
        $member=@($stage.files|Where-Object path -CEQ ('runtime/'+$name))
        if($member.Count -ne 1){throw 'Dependency identity ambiguous'}
        $from=Join-Path $stage.runtime $name;Pin $from $member[0].sha256
        $to=Join-Path $runtime $name;Copy-Item -LiteralPath $from -Destination $to;Pin $to $member[0].sha256
    }
    Pin (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    Pin (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $compiler='C:\Program Files\dotnet\dotnet.exe';Pin $compiler
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Pin (Join-Path $toolchain $name)}
    $refs=[Collections.Generic.List[string]]::new()
    $framework='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib','System','System.Core','System.Drawing','System.Windows.Forms','System.Web.Extensions','System.Xml','System.Runtime.Serialization','System.Security','Facades/netstandard')){
        $path=Join-Path $framework ($name+'.dll');Pin $path;$refs.Add('/reference:"'+$path+'"')
    }
    foreach($name in $dependencies){$refs.Add('/reference:"'+(Join-Path $runtime $name)+'"')}
    $sources=[Collections.Generic.List[string]]::new()
    foreach($n in 1..15){
        if($n -eq 14){continue}
        $old=Join-Path $recovery ('parent-source/'+$n+'.cs')
        $pins=@($build.inputs|Where-Object {$_.path.EndsWith('\Lea.Script\'+$n+'.cs',[StringComparison]::OrdinalIgnoreCase)}|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Recovered source identity ambiguous'}
        Pin $old $pins[0]
        $path=if($n -eq 8){Join-Path $profile '8.cs'}else{$old}
        $sources.Add('"'+$path+'"')
    }
    # Historical14.cs is unavailable. This metadata is explicitly diagnostic-only.
    $metadata=Join-Path $PSScriptRoot 'DiagnosticScriptAssemblyInfo.cs';Pin $metadata;$sources.Add('"'+$metadata+'"')
    $dll=Join-Path $runtime 'Lea.Script.dll'
    $options=@('/nologo','/noconfig','/nostdlib+','/target:library','/optimize+','/langversion:12',('/out:"'+$dll+'"'))+@($refs.ToArray())+@($sources.ToArray())+@('/resource:"'+(Join-Path $profile 'Dom.compact.js')+'",Lea.Script.Dom.js')
    $rsp=Join-Path $out 'parent.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
    $compile=Invoke-OwnedTestRun -Name 'compile-current-profile' -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compile.passed){throw 'Diagnostic parent compile failed'}
    Pin $dll;$result.diagnosticParentSha256=(Get-FileHash $dll).Hash
    $main=Join-Path $root 'Lea.Tests/BootstrapComparisonMain.cs';Pin $main
    $exe=Join-Path $runtime 'Lea.CurrentBootstrap.Tests.exe'
    $options=@('/nologo','/noconfig','/nostdlib+','/target:exe','/optimize+',('/out:"'+$exe+'"'),('"'+$main+'"'),('/reference:"'+$dll+'"'))+@($refs.ToArray())
    $rsp=Join-Path $out 'harness.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
    $compile=Invoke-OwnedTestRun -Name 'compile-current-profile-harness' -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compile.passed){throw 'Diagnostic harness compile failed'}
    $config=Join-Path $stage.runtime 'Lea.Script.Tests.exe.config';Pin $config;Copy-Item -LiteralPath $config -Destination ($exe+'.config');Pin ($exe+'.config')
    $fixture=Join-Path $baseline 'fixtures/LeaForge/test-pages/javascript-contains.html'
    $fixtureMember=@($stage.files|Where-Object path -CEQ 'fixtures/LeaForge/test-pages/javascript-contains.html')
    if($fixtureMember.Count -ne 1){throw 'Diagnostic fixture identity ambiguous'}
    Pin $fixture $fixtureMember[0].sha256;Pin $exe
    $result.before=@(Get-ChildItem -LiteralPath $runtime -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $run=Invoke-OwnedTestRun -Name 'cold-current-profile' -Application $exe -Arguments @(('"'+$fixture+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    $reports=@(Get-Content -LiteralPath $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
    $rows=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
    $result.measurementCaptured=$reports.Count -eq 1 -and $rows.Count -gt 0 -and $run.outputDrained -and $run.exitObserved
    if($reports.Count -eq 1){$result.report=$reports[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json}
    $result.profileRows=$rows.Count
    $result.passed=$run.passed -and $result.measurementCaptured -and $result.report.ok
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaCurrentProfileLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,measurementCaptured,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
