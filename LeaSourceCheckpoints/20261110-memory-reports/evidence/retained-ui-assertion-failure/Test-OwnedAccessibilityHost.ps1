param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/accessibility-host-ui-v1'
$runtime=Join-Path $out 'runtime'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if((Test-Path -LiteralPath $out) -or (Get-PSDrive C).Free -lt 650MB -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh output/owner and staging reserve required'}
if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS' -or @(Get-Process 'Firespider*','Lea.Firespider.Boot','Lea.Script.Worker','Lea.Script.Tests','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'NTFS/no overlapping tests required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$runs=[Collections.Generic.List[object]]::new();$inputs=[Collections.Generic.List[object]]::new();$holds=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash=''){$s=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($s);$actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($Hash -and $actual -cne $Hash){throw ('Input differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$actual})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;completed=$false;runs=$runs;inputs=$inputs;runtime=$runtime;before=@();privateRetentionQualified=$false;releaseQualified=$false}
try{
 Hold $PSCommandPath
 $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
 $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
 $manifestPath='I:\LeaBuildStorage\current-firespider-runtime-20261110-v1\runtime-manifest.json';Hold $manifestPath '2836AC6BA77C2DB096C5365EED4D7B6E48D56A82A0A4DFDBEBFD2CCE3DD6E1AB'
 $manifest=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
 $buildPath='I:\LeaBuildStorage\accessibility-host-build-20261110-v1\result.json';Hold $buildPath '4FE81C3234D9D6CCBDBBA6AF3BCA3D1B4AD21E2CC7CB77CDD43C90BB75AB5806'
 $build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
 if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.products.Count -ne 22 -or $build.hostSha256 -cne '61986B5044C5D0FED6B77C6895386813184177E4B5DB9F76C8FC5DA40FF90A1A' -or $manifest.files.Count -ne 400){throw 'Exact candidate required'}
 $overlay=@{};foreach($f in $build.products){$relative=[IO.Path]::GetRelativePath('I:\LeaBuildStorage\accessibility-host-build-20261110-v1\bin',$f.path).Replace('\','/');$overlay[$relative]=$f}
 foreach($f in $manifest.files){$from=Join-Path (Split-Path $manifestPath -Parent) ('runtime/'+$f.path);$hash=$f.sha256;if($overlay.ContainsKey($f.path)){$from=$overlay[$f.path].path;$hash=$overlay[$f.path].sha256};Hold $from $hash;$to=Join-Path $runtime $f.path;New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $from -Destination $to;if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Staging readback differs'}}
 $bootSource=Join-Path $root 'Lea.Firespider/Boot.cs';Hold $bootSource
 $case=Join-Path $root 'Lea.Tests/AccessibilityHostCase.cs';Hold $case
 $original=[IO.File]::ReadAllText($bootSource)
 $call='if (mode == "navigation") CheckNavigation(form, settings, page);'
 $replacement='if (mode == "navigation") { CheckAccessibilityReport(form, settings, root); CheckNavigation(form, settings, page); }'
 $marker='        private static void CheckNavigation(Form form, ApplicationSettingsBase settings, string page)'
 if([regex]::Matches($original,[regex]::Escape($call)).Count -ne 1 -or [regex]::Matches($original,[regex]::Escape($marker)).Count -ne 1){throw 'Boot injection boundary differs'}
 $fragment=[IO.File]::ReadAllText($case)+[Environment]::NewLine
 $text=$original.Replace($call,$replacement).Replace($marker,$fragment+$marker)
 if($text.Replace($fragment+$marker,$marker).Replace($replacement,$call) -cne $original){throw 'Original boot assertions changed'}
 $source=Join-Path $out 'Boot.generated.cs';[IO.File]::WriteAllText($source,$text,[Text.UTF8Encoding]::new($false));Hold $source
 $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
 $probe=Join-Path $runtime 'Lea.Accessibility.Probe.exe'
 $args=@('/nologo','/target:exe',('/out:"'+$probe+'"'),'/reference:System.dll','/reference:System.Core.dll','/reference:System.Drawing.dll','/reference:System.Windows.Forms.dll','/reference:System.Configuration.dll',('/reference:"'+(Join-Path $runtime 'Lea.Engine.dll')+'"'),('/reference:"'+(Join-Path $runtime 'Lea.Win.dll')+'"'),('"'+$source+'"'))
 $run=Invoke-OwnedTestRun -Name 'compile-accessibility-probe' -Application $compiler -Arguments $args -WorkingDirectory $runtime -OutputDirectory $out -Seconds 60 -Records $runs
 if(!$run.passed){throw 'Accessibility probe compilation failed'}
 $result.before=@(Get-ChildItem -LiteralPath $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
 $run=Invoke-OwnedTestRun -Name 'accessibility-host-ui' -Application $probe -Arguments @(('"'+$runtime+'"'),'navigation') -WorkingDirectory $runtime -OutputDirectory $out -Seconds 90 -Records $runs
 if(!$run.passed -or @((Get-Content -LiteralPath $run.stdout)|Where-Object {$_ -ceq 'PASS Lea accessibility host: click/hover/escaping/empty-tab/no-legacy'}).Count -ne 1){throw 'Accessibility UI checks failed'}
 $result.completed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
 $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
 $result.byteChanges=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256})
 $result.aclChanges=@($result.before|Where-Object {(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
 $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
 $result.passed=$result.completed -and $result.inputsUnchanged -and !$result.byteChanges.Count -and !$result.aclChanges.Count -and !$result.ownerAttention
 if(!$result.ownerAttention){foreach($h in $holds){$h.Dispose()}}else{$global:LeaAccessibilityUiHolds=$holds}
 $result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
 $result|Select-Object passed,completed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
