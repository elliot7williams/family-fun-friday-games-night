$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-GlobalInstallSharedDom.ps1'
if((Get-FileHash $from).Hash -cne 'F54921FF5C3AE86284F0721CE83C1545D795170D755DF48A4DEDDA45C1976510'){throw 'Shared runtime template differs'}
$to=Join-Path $PSScriptRoot 'Test-GlobalInstallSharedNative-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve independent native-view runner'}
$original=[IO.File]::ReadAllText($from)
$text=$original.Replace('global-install-shared-dom-20261102-v1','global-install-shared-native-20261102-v1').Replace('Lea.GlobalDom.Tests','Lea.GlobalNative.Tests').Replace('Lea.Tests/DomRegressionMain.cs','Lea.Tests/IsConnectedNativeMain.cs')
$old="    `$proof=Join-Path `$root 'continuity-evidence/global-install-isolated-scheduling-v2/result.json';Pin `$proof '170F5F1992E5BA32F406402FB23BC01A1B8E71DF34E9B7D44ACBAED277152FC0'"
$new="    `$proof='I:/LeaBuildStorage/global-install-shared-dom-20261102-v1/result.json';Pin `$proof 'DC438755F25368344B992F131C27DFE892DF74FB72EE2C39EE1AAC4E08C41106'"
if(!$text.Contains($old)){throw 'Native provenance projection differs'};$text=$text.Replace($old,$new)
$a=$text.IndexOf('    if(!$prior.passed');$b=$text.IndexOf("`n",$a)
if($a -lt 0 -or $b -le $a){throw 'Native prior-state boundary differs'}
$priorGuard=@'
    if($prior.passed -or $prior.completed -or !$prior.inputsUnchanged -or $prior.ownerAttention -or $prior.changedFiles.Count -or $prior.before.Count -ne 21 -or @(Get-ChildItem $runtime -Recurse -File).Count -ne 21 -or $prior.runs.Count -ne 5 -or $prior.cases.Count -ne 4 -or $prior.cases[3].passed -or $prior.error -cnotlike '*Original DOM suite failed: ClassLists*'){throw 'Expected clean partial DOM boundary differs'}
    foreach($run in $prior.runs){if(!$run.exitObserved -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $run.unsafeToContinue -or $run.retainedForOwner -or $run.timedOut -or $run.killRequested){throw 'Prior DOM ownership incomplete'}}
    if(!(Get-Content $prior.runs[4].stderr -Raw).Contains('Script budget exceeded. phase=bootstrap; reason=TimeoutException; stage=events')){throw 'Expected typed bootstrap failure differs'}
'@
$text=$text.Substring(0,$a)+$priorGuard+$text.Substring($b)
$a=$text.IndexOf('    $fixtureProof=');$b=$text.IndexOf('    $source=',$a)
if($a -lt 0 -or $b -le $a){throw 'Native fixture boundary differs'}
$fixture="    `$fixture=Join-Path `$root 'LeaForge/test-pages/is-connected-native.html';Pin `$fixture"
$text=$text.Substring(0,$a)+$fixture+[Environment]::NewLine+$text.Substring($b)
$text=$text.Replace("'Lea.Tests/IsConnectedNativeMain.cs';Pin `$source","'Lea.Tests/IsConnectedNativeMain.cs';Pin `$source 'BBBF3D3842E9D827398BCF107E4F0DF6552FD90606FECE5BEA7A18C5B5263EC1'")
$text=$text.Replace("@('mscorlib','System','System.Core')","@('mscorlib','System','System.Core','System.Drawing','System.Windows.Forms','System.Web.Extensions')")
$anchor='    $rsp=Join-Path'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Native project reference boundary differs'}
$references=@'
    foreach($name in @('Lea.Engine.dll','Lea.Script.dll','Lea.Win.dll','Lea.Script.Win.dll')){$options.Add('/reference:"'+(Join-Path $runtime $name)+'"')}
'@
$text=$text.Replace($anchor,$references+[Environment]::NewLine+$anchor).Replace("-Name 'compile-dom-driver'","-Name 'compile-native-driver'").Replace('$result.before.Count -ne 21','$result.before.Count -ne 23')
$a=$text.IndexOf('    $trace=');$b=$text.IndexOf('}catch{',$a)
if($a -lt 0 -or $b -le $a){throw 'Native test boundary differs'}
$body=@'
    $result.nativeOnly=$true
    $run=Invoke-OwnedTestRun -Name 'native-connectivity' -Application $exe -Arguments @(('"'+$fixture+'"'),('"'+$out+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    $footer='PASS Lea isConnected native: inline lifecycle, real activation, field retention, detached editor cleanup, desktop/narrow render, guarded worker retirement.'
    $verifiedFooter=@(Get-Content $run.stdout|Where-Object {$_ -ceq $footer}).Count -eq 1
    $result.images=@(foreach($name in @('connected-desktop.png','detached-desktop.png','detached-narrow.png')){$path=Join-Path $out $name;if(Test-Path $path){@{path=$path;sha256=(Get-FileHash $path).Hash}}})
    $result.passed=$run.passed -and $verifiedFooter -and $result.images.Count -eq 3
    $result.completed=$result.passed
    $result.scope='Independent unchanged native V connectivity fixture: inline lifecycle, real activation/edit retention/detachment, desktop+narrow paint, sandbox guards and exact worker retirement. Same engine/worker bytes; two new driver files only. Not a ClassLists retry, Firespider host/full-suite or startup-reliability qualification.'
'@
$text=$text.Substring(0,$a)+$body+[Environment]::NewLine+$text.Substring($b)
foreach($critical in @('foreach($file in $prior.before)','Get-Acl $file.path','600MB+256KB','D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA','-Seconds 120','-Seconds 30')){
    if([regex]::Matches($text,[regex]::Escape($critical)).Count -lt 1){throw 'Critical native runner operation lost'}
}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Native runner syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
