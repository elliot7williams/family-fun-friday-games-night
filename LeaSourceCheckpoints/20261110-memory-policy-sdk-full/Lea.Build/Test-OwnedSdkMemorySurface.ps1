param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/memory-policy-sdk-memory-v1'
$runtime=Join-Path $out 'runtime'
$evidence='I:\LeaBuildStorage\memory-policy-sdk-memory-20261110-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if((Test-Path -LiteralPath $out) -or (Test-Path -LiteralPath $evidence) -or (Get-PSDrive C).Free -lt 650MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh output/owner/reserve required'}
if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS' -or @(Get-Process 'Firespider*','Lea.Script.Worker','Lea.Script.Tests','Lea.Image','Lea.ScriptEmbeddingSample' -ErrorAction SilentlyContinue).Count){throw 'NTFS/no overlap required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash=''){$s=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($s);$actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($Hash -and $actual -cne $Hash){throw 'SDK memory input differs'};$inputs.Add(@{path=$Path;sha256=$actual})}
New-Item -ItemType Directory -Path $runtime -Force|Out-Null
New-Item -ItemType Directory -Path $evidence|Out-Null
$result=@{passed=$false;completed=$false;runs=$runs;inputs=$inputs;runtime=$runtime;before=@();hostAdopted=$false;globalSecurityQualified=$false}
try{
 Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination $evidence
 $audit='I:\LeaBuildStorage\memory-policy-sdk-audit-20261110-v1\audit.json';Hold $audit 'FD6BBBC7479A5F26F983E180D0C4BBA1EE75582C28F1F692F3D61C63DE52B3DC'
 $proof=Get-Content -LiteralPath $audit -Raw|ConvertFrom-Json
 if(!$proof.verified -or $proof.sourceMembers -ne 128 -or $proof.dependencyMembers -ne 8 -or $proof.ownedRuns -ne 9){throw 'Source-built SDK audit required'}
 foreach($name in @('Lea.Engine.dll','Lea.Win.dll')){$file=@($proof.libraryProducts|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $name});if($file.Count -ne 1){throw 'Unique SDK product required'};Hold $file[0].path $file[0].sha256;$to=Join-Path $runtime $name;Copy-Item -LiteralPath $file[0].path -Destination $to;Hold $to $file[0].sha256}
 $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
 $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
 $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
 $cases=@(
  @{name='model';source='Lea.Tests/MemoryDocumentMain.cs';footer='PASS Lea memory documents: 12 model checks';args=@()},
  @{name='view';source='Lea.Tests/MemoryDocumentViewMain.cs';footer='PASS Lea memory view: 17 checks at720/360; no HTML in isolated TEMP';args=@(('"'+$out+'"'))},
  @{name='policy';source='Lea.Tests/MemoryScriptPolicyMain.cs';footer='PASS Lea memory script policy: 7 checks';args=@()}
 )
 foreach($case in $cases){$driver=Join-Path $root $case.source;Hold $driver;Copy-Item -LiteralPath $driver -Destination $evidence;$exe=Join-Path $runtime ('SdkMemory-'+$case.name+'.exe');$case.exe=$exe;$arguments=@('/nologo','/target:exe',('/out:"'+$exe+'"'),('/reference:"'+(Join-Path $runtime 'Lea.Engine.dll')+'"'),('"'+$driver+'"'));if($case.name -eq 'view'){$arguments+=@('/reference:System.Drawing.dll','/reference:System.Windows.Forms.dll',('/reference:"'+(Join-Path $runtime 'Lea.Win.dll')+'"'))};$run=Invoke-OwnedTestRun -Name ('compile-sdk-memory-'+$case.name) -Application $compiler -Arguments $arguments -WorkingDirectory $runtime -OutputDirectory $evidence -Seconds 60 -Records $runs;if(!$run.passed){throw 'SDK memory probe compilation failed'};Hold $exe}
 $result.before=@(Get-ChildItem -LiteralPath $runtime -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
 foreach($case in $cases){$run=Invoke-OwnedTestRun -Name ('sdk-memory-'+$case.name) -Application $case.exe -Arguments $case.args -WorkingDirectory $runtime -OutputDirectory $evidence -Seconds 30 -Records $runs;if(!$run.passed -or @((Get-Content -LiteralPath $run.stdout)|Where-Object {$_ -ceq $case.footer}).Count -ne 1){throw 'SDK memory fixture failed'}}
 $result.completed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
 $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
 $result.byteChanges=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256})
 $result.aclChanges=@($result.before|Where-Object {(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
 $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
 $result.passed=$result.completed -and $result.inputsUnchanged -and !$result.byteChanges.Count -and !$result.aclChanges.Count -and !$result.ownerAttention
 if(!$result.ownerAttention){foreach($h in $holds){$h.Dispose()}}else{$global:LeaSdkMemoryInputLeases=$holds}
 $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $evidence 'result.json') -Encoding utf8
 $result|Select-Object passed,completed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
