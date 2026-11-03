$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-DeterministicContainsRecovery.ps1'
if((Get-FileHash $from).Hash -cne '5C41FAA35D0515D0605DB2F86EE57D8ABD73A80A2EC1020BA38C47F180381F2E'){throw 'Original recovery assertions differ'}
$to=Join-Path $PSScriptRoot 'Test-ContainsRecoveryStress-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve stress runner'}
$text=[IO.File]::ReadAllText($from).Replace('continuity-evidence/deterministic-contains-recovery-v1','continuity-evidence/contains-recovery-stress-v1')
$start=$text.IndexOf('    $run=Invoke-OwnedTestRun')
$end=$text.IndexOf('}catch{', $start)
if($start -lt 0 -or $end -le $start){throw 'Original focused run boundaries differ'}
$block=$text.Substring($start,$end-$start)
$projected=$block.Replace("-Name 'contains-isolated'","-Name ('contains-isolated-'+`$iteration)")
if($projected -ceq $block){throw 'Unique run-name projection required'}
$before=@'
    $result.attempts=[Collections.Generic.List[object]]::new()
    for($iteration=1;$iteration -le 3;$iteration++){
'@
$after=@'
        $result.attempts.Add(@{iteration=$iteration;passed=$result.passed;footerVerified=$result.footerVerified;recoveryAssertionVerified=$result.recoveryAssertionVerified;traceActivationVerified=$result.traceActivationVerified;traceEnvironmentRestored=$result.traceEnvironmentRestored})
        if(!$result.passed){throw 'Focused recovery attempt failed; stop rather than retry'}
        foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Runtime byte/ACL drift between attempts'}}
        if($global:LeaCompositeProcessLeases.Count -or (Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'Unretired process prevents next attempt'}
    }
    if($runs.Count -ne 3 -or $result.attempts.Count -ne 3){throw 'Three completed attempts required'}
'@
$text=$text.Substring(0,$start)+$before+[Environment]::NewLine+$projected+$after+[Environment]::NewLine+$text.Substring($end)
$text=$text.Replace('Existing --contains isolated entry on fresh exact-byte NTFS closure; no full-prefix/cold-equivalence claim','Three separate existing uninstrumented --contains isolated processes; each original30s with verified retirement and byte/ACL equality before next attempt. Runtime path reused; no full-prefix/cold-cache/reliability-release claim')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Stress runner syntax invalid'}
if(!$text.Contains($projected) -or !$text.Contains('-Seconds 30') -or !$text.Contains('PASS Lea.Script contains isolated: 22 checks.')){throw 'Original checks and budget not retained'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
