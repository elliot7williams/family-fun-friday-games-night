param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\accessibility-report-model-20261110-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if((Test-Path -LiteralPath $out) -or (Get-PSDrive C).Free -lt 600MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh output/owner and original storage reserve required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$runs=[Collections.Generic.List[object]]::new();$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash=''){$s=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($s);$actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($Hash -and $actual -cne $Hash){throw 'Input differs'};$inputs.Add(@{path=$Path;sha256=$actual})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;runs=$runs;inputs=$inputs;hostIntegrated=$false;runtimeUiQualified=$false}
try{
 Hold $PSCommandPath
 $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
 $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
 $fragment=Join-Path $root 'Lea.Firespider/AccessibilityReport.vb';Hold $fragment
 $driver=Join-Path $root 'Lea.Tests/AccessibilityReportMain.vb';Hold $driver
 $source=Join-Path $out 'Report.generated.vb'
 [IO.File]::WriteAllText($source,"Imports System`r`nImports System.Text`r`nImports System.Collections.Generic`r`nPartial Class j`r`n"+[IO.File]::ReadAllText($fragment)+"`r`nEnd Class`r`n")
 Hold $source
 $core='I:\LeaBuildStorage\current-firespider-build-20261109-v1\bin\Lea.Engine.dll';Hold $core '11B154A135F6A1B455080A384AF281759EF09DB18347C15C805149153A8C362A'
 Copy-Item -LiteralPath $core -Destination $out
 $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/vbc.exe';Hold $compiler
 $vb=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/Microsoft.VisualBasic.dll';Hold $vb
 $exe=Join-Path $out 'AccessibilityReport.exe'
 $args=@('/nologo','/target:exe','/optioninfer+','/main:j',('/out:"'+$exe+'"'),('/vbruntime:"'+$vb+'"'),('/reference:"'+$core+'"'),('"'+$source+'"'),('"'+$driver+'"'))
 $run=Invoke-OwnedTestRun -Name 'compile-accessibility-report' -Application $compiler -Arguments $args -WorkingDirectory $out -OutputDirectory $out -Seconds 60 -Records $runs
 if(!$run.passed){throw 'Accessibility report compilation failed'}
 Hold $exe
 $run=Invoke-OwnedTestRun -Name 'accessibility-report-model' -Application $exe -Arguments @() -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
 if(!$run.passed -or @((Get-Content -LiteralPath $run.stdout)|Where-Object {$_ -ceq 'PASS Lea accessibility report: 10 model checks'}).Count -ne 1){throw 'Accessibility model checks failed'}
 $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
 $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
 $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
 $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
 if(!$result.ownerAttention){foreach($h in $holds){$h.Dispose()}}else{$global:LeaAccessInputLeases=$holds}
 $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
 $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
