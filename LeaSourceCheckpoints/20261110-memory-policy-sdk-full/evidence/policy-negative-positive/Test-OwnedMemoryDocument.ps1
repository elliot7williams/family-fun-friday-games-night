param([switch]$KeepForegroundHost,[switch]$View,[switch]$HardenMemory)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base='I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1'
$out=if($HardenMemory){if($View){'I:\LeaBuildStorage\memory-document-policy-view-20261110-v1'}else{'I:\LeaBuildStorage\memory-document-policy-model-20261110-v1'}}elseif($View){'I:\LeaBuildStorage\memory-document-view-20261110-v1'}else{'I:\LeaBuildStorage\memory-document-model-20261110-v1'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path -LiteralPath $out) -or (Get-PSDrive C).Free -lt 600MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Original reserve/fresh owner/output required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash=''){$s=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($s);$actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($Hash -and $actual -cne $Hash){throw ('Input differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$actual})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;runs=$runs;inputs=$inputs;hostIntegrated=$false;privacyFixed=$false;releaseQualified=$false}
$names=@('TEMP','TMP','DOTNET_CLI_HOME','NUGET_PACKAGES');$saved=@{};foreach($name in $names){$saved[$name]=[Environment]::GetEnvironmentVariable($name,'Process')}
try{
 Hold $PSCommandPath
 $planPath=Join-Path $base 'source.json';Hold $planPath '51A823667E96D3ED4C3EC526E0747B8C19B1EBBE59A8A05324503DBEB4FED9C9'
 $plan=Get-Content -LiteralPath $planPath -Raw|ConvertFrom-Json
 $files=@($plan.files|Where-Object {$_.path.StartsWith('Lea.Engine/',[StringComparison]::Ordinal)})
 if($files.Count -lt 30){throw 'Complete core source set required'}
 $stage=Join-Path $out 'source'
 foreach($f in $files){$from=Join-Path $base ('source/'+$f.path);Hold $from $f.sha256;$to=Join-Path $stage $f.path;New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $from -Destination $to;if((Get-FileHash -LiteralPath $to).Hash -cne $f.sha256){throw 'Core source copy differs'}}
 $fragment=Join-Path $root 'Lea.Engine/MemoryDocument.fragment.cs';Hold $fragment
 $e=Join-Path $stage 'Lea.Engine/E.cs';$original=[IO.File]::ReadAllText($e)
 $marker='        public P Go(string input)';$extra=[IO.File]::ReadAllText($fragment)+[Environment]::NewLine
 if([regex]::Matches($original,[regex]::Escape($marker)).Count -ne 1){throw 'Core extension boundary differs'}
 $pairs=@(@('var p = Read(At.U, true);','var p = At.Mm ? MemoryPage(At.H, At.U) : Read(At.U, true);'),@('var p = await ReadA(At.U, true);','var p = At.Mm ? MemoryPage(At.H, At.U) : await ReadA(At.U, true);'))
 if($HardenMemory){$pairs+=@(
  @('p.Je = je;','p.Je = je && !p.Mm;'),
  @('page.Je = je;','page.Je = je && !page.Mm;'),
  @('MediaRules(rules, vw, vh, dp, th, rm, pc, je ?','MediaRules(rules, vw, vh, dp, th, rm, pc, p.Je ?'),
  @('MediaRules(all, vw, vh, dp, th, rm, pc, je ?','MediaRules(all, vw, vh, dp, th, rm, pc, page.Je ?'),
  @('if (!je) return;','if (!p.Je) return;'),
  @('if (!je) return 0;','if (!je || page.Mm) return 0;')
 )}
 $changed=$original.Replace($marker,$extra+$marker)
 foreach($pair in $pairs){if([regex]::Matches($changed,[regex]::Escape($pair[0])).Count -ne 1){throw 'Reload boundary differs'};$changed=$changed.Replace($pair[0],$pair[1])}
 $inverse=$changed.Replace($extra+$marker,$marker);for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])};if($inverse -cne $original){throw 'Core inverse differs'}
 [IO.File]::WriteAllText($e,$changed,[Text.UTF8Encoding]::new($false))
 $p=Join-Path $stage 'Lea.Engine/P.cs';$originalP=[IO.File]::ReadAllText($p);$pmarker='        internal object R { get; set; }'
 if([regex]::Matches($originalP,[regex]::Escape($pmarker)).Count -ne 1){throw 'Page marker boundary differs'}
 [IO.File]::WriteAllText($p,$originalP.Replace($pmarker,'        internal bool Mm { get; set; }'+[Environment]::NewLine+$pmarker),[Text.UTF8Encoding]::new($false))
 foreach($f in $files){Hold (Join-Path $stage $f.path)}
 @{sourceFiles=$files.Count;coreInverseVerified=$true;changedFiles=@('Lea.Engine/E.cs','Lea.Engine/P.cs');files=@($files|ForEach-Object {@{path=$_.path;sha256=(Get-FileHash -LiteralPath (Join-Path $stage $_.path)).Hash;baselineSha256=$_.sha256}})}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
 $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
 $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
 foreach($name in $names){$value=Join-Path $out $name;New-Item -ItemType Directory -Path $value|Out-Null;[Environment]::SetEnvironmentVariable($name,$value,'Process')}
 $dotnet='C:\Program Files\dotnet\dotnet.exe';Hold $dotnet
 $msbuild='C:\Program Files\dotnet\sdk\8.0.427\MSBuild.dll';Hold $msbuild
 $project=Join-Path $stage 'Lea.Engine/Lea.Engine.csproj'
 $run=Invoke-OwnedTestRun -Name 'compile-memory-core' -Application $dotnet -Arguments @(('"'+$msbuild+'"'),('"'+$project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false') -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
 if(!$run.passed){throw 'Memory core compilation failed'}
 $core=Join-Path $stage 'bin/Release/Lea.Engine.dll';Hold $core
 $result.coreSha256=(Get-FileHash -LiteralPath $core).Hash
 $driver=Join-Path $root 'Lea.Tests/MemoryDocumentMain.cs';Hold $driver
 $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
 $exe=Join-Path (Split-Path $core -Parent) 'MemoryDocument.exe'
 $run=Invoke-OwnedTestRun -Name 'compile-memory-model' -Application $compiler -Arguments @('/nologo','/target:exe',('/out:"'+$exe+'"'),('/reference:"'+$core+'"'),('"'+$driver+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 60 -Records $runs
 if(!$run.passed){throw 'Memory model compilation failed'};Hold $exe
 $run=Invoke-OwnedTestRun -Name 'memory-model' -Application $exe -Arguments @() -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
 if(!$run.passed -or @((Get-Content -LiteralPath $run.stdout)|Where-Object {$_ -ceq 'PASS Lea memory documents: 12 model checks'}).Count -ne 1){throw 'Memory document checks failed'}
 if($View){
  $runtime=Join-Path $root $(if($HardenMemory){'continuity-evidence/memory-document-policy-view-v1/runtime'}else{'continuity-evidence/memory-document-view-v1/runtime'})
  if((Test-Path -LiteralPath $runtime) -or (Get-Volume -DriveLetter C).FileSystem -cne 'NTFS'){throw 'Fresh NTFS runtime required'}
  foreach($f in $plan.files|Where-Object {$_.path.StartsWith('Lea.Win/',[StringComparison]::Ordinal)}){$from=Join-Path $base ('source/'+$f.path);Hold $from $f.sha256;$to=Join-Path $stage $f.path;New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $from -Destination $to;if((Get-FileHash -LiteralPath $to).Hash -cne $f.sha256){throw 'View copy differs'}}
  $v=Join-Path $stage 'Lea.Win/V.cs';$vOriginal=[IO.File]::ReadAllText($v)
  $vFragment=Join-Path $root 'Lea.Win/MemoryDocument.fragment.cs';Hold $vFragment
  $vExtra=[IO.File]::ReadAllText($vFragment)+[Environment]::NewLine
  if([regex]::Matches($vOriginal,[regex]::Escape($marker)).Count -ne 1){throw 'View insertion boundary differs'}
  $vChanged=$vOriginal.Replace($marker,$vExtra+$marker)
  if($vChanged.Replace($vExtra+$marker,$marker) -cne $vOriginal){throw 'View inverse differs'}
  [IO.File]::WriteAllText($v,$vChanged,[Text.UTF8Encoding]::new($false))
  foreach($f in Get-ChildItem -LiteralPath (Join-Path $stage 'Lea.Win') -File){Hold $f.FullName}
  $viewProject=Join-Path $stage 'Lea.Win/Lea.Win.csproj'
  $run=Invoke-OwnedTestRun -Name 'compile-memory-view' -Application $dotnet -Arguments @(('"'+$msbuild+'"'),('"'+$viewProject+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Build','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false') -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
  if(!$run.passed){throw 'Memory view compilation failed'}
  $viewDll=Join-Path (Split-Path $core -Parent) 'Lea.Win.dll';Hold $viewDll
  $viewDriver=Join-Path $root 'Lea.Tests/MemoryDocumentViewMain.cs';Hold $viewDriver
  New-Item -ItemType Directory -Path $runtime -Force|Out-Null
  foreach($file in @($core,$viewDll)){$to=Join-Path $runtime ([IO.Path]::GetFileName($file));Copy-Item -LiteralPath $file -Destination $to;Hold $to (Get-FileHash -LiteralPath $file).Hash}
  $viewExe=Join-Path $runtime 'MemoryDocumentView.exe'
  $viewArgs=@('/nologo','/target:exe',('/out:"'+$viewExe+'"'),('/reference:"'+$core+'"'),('/reference:"'+$viewDll+'"'),'/reference:System.Drawing.dll','/reference:System.Windows.Forms.dll',('"'+$viewDriver+'"'))
  $run=Invoke-OwnedTestRun -Name 'compile-memory-view-probe' -Application $compiler -Arguments $viewArgs -WorkingDirectory $out -OutputDirectory $out -Seconds 60 -Records $runs
  if(!$run.passed){throw 'Memory view probe compilation failed'};Hold $viewExe
  $result.before=@(Get-ChildItem -LiteralPath $runtime -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
  $run=Invoke-OwnedTestRun -Name 'memory-view' -Application $viewExe -Arguments @(('"'+(Split-Path $runtime -Parent)+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
  if(!$run.passed -or @((Get-Content -LiteralPath $run.stdout)|Where-Object {$_ -ceq 'PASS Lea memory view: 17 checks at720/360; no HTML in isolated TEMP'}).Count -ne 1){throw 'Memory view checks failed'}
  $result.viewSha256=(Get-FileHash -LiteralPath $viewDll).Hash
  $result.byteChanges=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256})
  $result.aclChanges=@($result.before|Where-Object {(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
  if($result.byteChanges.Count -or $result.aclChanges.Count){throw 'View runtime drift'}
 }
 $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
 foreach($name in $names){[Environment]::SetEnvironmentVariable($name,$saved[$name],'Process')}
 $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
 $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
 $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
 if(!$result.ownerAttention){foreach($h in $holds){$h.Dispose()}}else{$global:LeaMemoryInputLeases=$holds}
 $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
 $result|Select-Object passed,coreSha256,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
