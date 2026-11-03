$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$folder=Join-Path $root 'continuity-evidence/retirement-full-prefix-owned-v2'
$out='I:\LeaBuildStorage\retirement-full-prefix-audit-20261102-v2'
if(Test-Path $out){throw 'Preserve full-prefix audit'}
$path=Join-Path $folder 'result.json'
if((Get-FileHash $path).Hash -cne 'AAF5E03C8085FEC830CF479DC2D1DEE198AC5D9A6E70779D95FFE46D3219BE9B'){throw 'Full diagnostic receipt differs'}
$r=Get-Content $path -Raw|ConvertFrom-Json
if(!$r.passed -or !$r.completed -or !$r.phasesVerified -or !$r.footerVerified -or $r.checks -ne 3412 -or !$r.inputsUnchanged -or !$r.traceActivationVerified -or !$r.traceEnvironmentRestored -or $r.ownerAttention -or $r.changedFiles.Count -or $r.before.Count -ne 65 -or $r.runs.Count -ne 1){throw 'Full-prefix evidence incomplete'}
$run=$r.runs[0]
if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.parentBudgetMs -ne 300000 -or $run.sharedRetirementDrainBudgetMs -ne 5000){throw 'Original owned boundaries not met'}
foreach($file in $r.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Runtime byte/ACL differs'}}
foreach($file in $r.inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Held input differs'}}
$lines=@(Get-Content $run.stderr)
$begin=@(for($i=0;$i -lt $lines.Count;$i++){if($lines[$i] -cmatch '^LEA_SUITE_PHASE [0-9]+ 86 begin$'){$i}})
$end=@(for($i=0;$i -lt $lines.Count;$i++){if($lines[$i] -cmatch '^LEA_SUITE_PHASE [0-9]+ 86 end$'){$i}})
if($begin.Count -ne 1 -or $end.Count -ne 1 -or $end[0] -le $begin[0]){throw 'Unique completed isolated Contains phase required'}
. (Join-Path $PSScriptRoot 'Read-RetirementTrace.ps1')
$trace=Read-RetirementTrace -Lines $lines[$begin[0]..$end[0]] -Frequency ([Diagnostics.Stopwatch]::Frequency)
if(@($trace.spans.phase|Select-Object -Unique).Count -ne 8){throw 'All retirement phase types required'}
New-Item -ItemType Directory -Path $out|Out-Null
foreach($file in Get-ChildItem $folder -File){Copy-Item -LiteralPath $file.FullName -Destination $out;if((Get-FileHash (Join-Path $out $file.Name)).Hash -cne (Get-FileHash $file.FullName).Hash){throw 'Evidence copy differs'}}
@{passed=$true;diagnosticOnly=$true;checks=3412;phases=89;runtimeFiles=65;elapsedMs=$run.elapsedMs;isolatedContainsTrace=$trace;timeoutReproduced=$false;timeoutFixed=$false;scope='One full-prefix diagnostic pass; original phase57 zero-stderr contract retained with original private DLL. Main runtime retirement trace perturbs timing; not product reliability or release proof.'}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash (Join-Path $out 'audit.json')
