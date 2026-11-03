$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-GlobalInstallComparison-v1.generated.ps1'
if((Get-FileHash $from).Hash -cne '34BC345AB3144DF2EE0DDAFD185A04270EA287DD87E9678D47917E09C3C348AD'){throw 'Comparison template differs'}
$to=Join-Path $PSScriptRoot 'Test-BootstrapFunctionComparison-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve function comparison'}
$original=[IO.File]::ReadAllText($from)
$text=$original.Replace('continuity-evidence/global-install-comparison-v1','continuity-evidence/bootstrap-function-comparison-v1').Replace('global-install-build-20261102-v1/result.json','bootstrap-function-build-20261102-v1/result.json').Replace('FA61813EBDA435DFF4DC1138E450FE2C385AC3156534B507DB9216AA9878B21F','2597084C56DE45AA9DB11132091E02EE5F3C1D84890D11EE779F553CA2DD5D93').Replace('F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B','62B5656A41720E124A915E36E437C95337E06259985EC10923CA9E987AE7E17E').Replace('FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C','F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B')
$text=$text.Replace('2L*[long](($dlls|Measure-Object Length -Sum).Sum)+4MB','[long](($dlls|Measure-Object Length -Sum).Sum)+4MB').Replace('reserve plus BOTH staged closures','reserve plus new candidate closure and driver space; baseline is not copied')
$a=$text.IndexOf('    $old=');$b=$text.IndexOf('    foreach($file in Get-ChildItem $runtime -File)',$a)
if($a -lt 0 -or $b -le $a){throw 'Verified baseline reuse boundary differs'}
$baseline=@'
    $baseline=Join-Path $root 'continuity-evidence/global-install-comparison-v1/runtime'
    $baselineProof=Join-Path $root 'continuity-evidence/global-install-comparison-v1/result.json';Pin $baselineProof 'EFCA1D194071A89041D8A6B5283A753C32F05C6A161B7E03A1D4C6D1F27693D8'
    $oldComparison=Get-Content $baselineProof -Raw|ConvertFrom-Json
    if(!$oldComparison.inputsUnchanged -or $oldComparison.ownerAttention -or $oldComparison.changedFiles.Count -or !$oldComparison.comparisonCompleted){throw 'Baseline prior ownership/bytes incomplete'}
    foreach($oldRun in $oldComparison.runs){if(!$oldRun.exitObserved -or !$oldRun.outputDrained -or $oldRun.activeAfterRetirement -ne 0 -or $oldRun.unsafeToContinue -or $oldRun.retainedForOwner -or $oldRun.killRequested -or $oldRun.timedOut){throw 'Baseline prior process retirement incomplete'}}
    $baselineFiles=@($oldComparison.before|Where-Object {$_.path.StartsWith($baseline+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)})
    if($baselineFiles.Count -ne 10 -or @(Get-ChildItem $baseline -File).Count -ne 10){throw 'Exact baseline closure required'}
    foreach($file in $baselineFiles){Pin $file.path $file.sha256;if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Baseline ACL differs'}}
    $result.baselineReused=$true
'@
$text=$text.Substring(0,$a)+$baseline+[Environment]::NewLine+$text.Substring($b)
foreach($critical in @('foreach($file in $dlls)','Copy-Item -LiteralPath $file.FullName','600MB+$stageBytes',"@('baseline','candidate','candidate','baseline')",'-Seconds 120','-Seconds 30','Unsafe comparative attempt prevents continuation')){
    if([regex]::Matches($text,[regex]::Escape($critical)).Count -ne [regex]::Matches($original,[regex]::Escape($critical)).Count){throw 'Critical comparison operation lost'}
}
if($text.Contains('Copy-Item -LiteralPath $runtime -Destination $baseline') -or !$text.Contains('$stageBytes=[long](($dlls|Measure-Object Length -Sum).Sum)+4MB')){throw 'Comparison staging plan differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Function comparison syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
