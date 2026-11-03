$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-GlobalInstallSharedDom.ps1'
$to=Join-Path $PSScriptRoot 'Test-PrivateBindingSharedDom-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve private binding DOM campaign'}
if((Get-FileHash $from).Hash -cne 'F54921FF5C3AE86284F0721CE83C1545D795170D755DF48A4DEDDA45C1976510'){throw 'Shared DOM template changed'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('global-install-shared-dom-20261102-v1','private-binding-shared-dom-20261102-v1'),
    @('continuity-evidence/global-install-isolated-scheduling-v2/runtime','continuity-evidence/private-binding-trusted-v1/runtime'),
    @('continuity-evidence/global-install-isolated-scheduling-v2/result.json','continuity-evidence/private-binding-feature-isolated-v1/result.json'),
    @('170F5F1992E5BA32F406402FB23BC01A1B8E71DF34E9B7D44ACBAED277152FC0','8244E1A2816FF045B17445FE152CB49AD3AA0646097F443B7FAD2763010B70FD'),
    @('!$prior.completed','!$prior.isolatedWorkerTested'),
    @('$prior.before.Count -ne 19','$prior.before.Count -ne 21'),
    @('@(Get-ChildItem $runtime -Recurse -File).Count -ne 19','@(Get-ChildItem $runtime -Recurse -File).Count -ne 21'),
    @('F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B','406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F'),
    @('A72EF9C070FCDE2BA57630FFFF6BEAD4ABD157F6365FA092C0A721AB3EACAFF4','F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0'),
    @('Lea.GlobalDom.Tests.exe','Lea.PrivateDom.Tests.exe'),
    @("'Lea.GlobalDom.Tests'","'Lea.PrivateDom.Tests'"),
    @('$result.before.Count -ne 21','$result.before.Count -ne 23')
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique private DOM boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Private DOM projection inverse differs'}
foreach($critical in @('600MB+256KB','Lea.Tests/DomRegressionMain.cs','-Seconds 120','-Seconds 30','Read-AllStartupTraceProof')){if([regex]::Matches($text,[regex]::Escape($critical)).Count -ne [regex]::Matches($original,[regex]::Escape($critical)).Count){throw 'DOM safety operation changed'}}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
