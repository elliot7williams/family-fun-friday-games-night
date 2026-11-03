$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-GlobalInstallSharedNative-v1.generated.ps1'
$to=Join-Path $PSScriptRoot 'Test-PrivateBindingSharedNative-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve private binding native campaign'}
if((Get-FileHash $from).Hash -cne 'E42505E00572EDC739DDE7748900D0DFAD8B05C11111BE2BC36BA618B85E9309'){throw 'Native template changed'}
$original=[IO.File]::ReadAllText($from)
$a=$original.IndexOf('    if($prior.passed')
$b=$original.IndexOf('    foreach($file in $prior.before)',$a)
if($a -lt 0 -or $b -le $a){throw 'Prior native qualification boundary required'}
$old=$original.Substring($a,$b-$a)
$guard=@'
    if(!$prior.passed -or !$prior.completed -or !$prior.inputsUnchanged -or $prior.ownerAttention -or $prior.changedFiles.Count -or $prior.before.Count -ne 25 -or @(Get-ChildItem $runtime -Recurse -File).Count -ne 25 -or $prior.runs.Count -ne 5 -or $prior.cases.Count -ne 4){throw 'Qualified scheduling boundary differs'}
    foreach($run in $prior.runs){if(!$run.passed -or !$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.unsafeToContinue -or $run.retainedForOwner -or $run.timedOut -or $run.killRequested){throw 'Prior scheduling ownership incomplete'}}
'@
$pairs=@(
    @($old,($guard+"`n"),1),
    @('global-install-shared-native-20261102-v1','private-binding-shared-native-20261102-v1',1),
    @('continuity-evidence/global-install-isolated-scheduling-v2/runtime','continuity-evidence/private-binding-trusted-v1/runtime',1),
    @('global-install-shared-dom-20261102-v1/result.json','private-binding-shared-scheduling-20261102-v1/result.json',1),
    @('DC438755F25368344B992F131C27DFE892DF74FB72EE2C39EE1AAC4E08C41106','3015572627EA0427C896C52C2EB697C2CE2AEAE986B94119B17902F05286B4CC',1),
    @('F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B','406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F',1),
    @('A72EF9C070FCDE2BA57630FFFF6BEAD4ABD157F6365FA092C0A721AB3EACAFF4','F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0',1),
    @('Lea.GlobalNative.Tests','Lea.PrivateNative.Tests',2),
    @('$result.before.Count -ne 23','$result.before.Count -ne 27',1)
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 3 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne $pair[2]){throw 'Unique private native boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Private native projection inverse differs'}
foreach($critical in @('600MB+256KB','BBBF3D3842E9D827398BCF107E4F0DF6552FD90606FECE5BEA7A18C5B5263EC1','-Seconds 120','-Seconds 30','Get-Acl $file.path')){if([regex]::Matches($text,[regex]::Escape($critical)).Count -ne [regex]::Matches($original,[regex]::Escape($critical)).Count){throw 'Native safety operation changed'}}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
