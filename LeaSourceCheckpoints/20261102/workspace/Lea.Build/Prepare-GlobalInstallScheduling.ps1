param([ValidateSet(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-IsConnectedTrusted.ps1'
if((Get-FileHash $from).Hash -cne 'F48FB93BD317970D30CB3861AFE183E60A26A8F6D87893B6EFB9BFC513C449FF'){throw 'Trusted runtime template differs'}
$to=Join-Path $PSScriptRoot "Test-GlobalInstallScheduling-v$Version.generated.ps1"
if(Test-Path $to){throw 'Preserve scheduling campaign'}
$text=[IO.File]::ReadAllText($from).Replace('"continuity-evidence/is-connected-trusted-v$Version"',"'continuity-evidence/global-install-scheduling-v1'").Replace('is-connected-script-build-20261102-v1','global-install-build-20261102-v1').Replace('2596ED5B2B5E5210324DBE403C1526E029E4F99240080C4434DCD028445D46F9','FA61813EBDA435DFF4DC1138E450FE2C385AC3156534B507DB9216AA9878B21F').Replace('FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C','F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B').Replace('Lea.Tests/IsConnectedMain.cs','Lea.Tests/SchedulingRegressionMain.cs')
$anchor="    if(`$dlls.Count -ne 8){throw 'Eight-file trusted closure required'}"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Scheduling reserve boundary differs'}
$reserve=@'
    if($Isolated){throw 'This campaign qualifies trusted scheduling only'}
    $stageBytes=[long](($dlls|Measure-Object Length -Sum).Sum)+857600+350720+256KB
    if((Get-PSDrive C).Free -lt (600MB+$stageBytes)){throw 'Scheduling reserve plus full staged closure required'}
'@
$text=$text.Replace($anchor,$anchor+[Environment]::NewLine+$reserve)
$a=$text.IndexOf("    `$fixture=Join-Path");$b=$text.IndexOf('    $source=Join-Path',$a)
if($a -lt 0 -or $b -le $a){throw 'Scheduling fixture boundary differs'}
$bindings=@'
    $oldTests=Join-Path $root 'continuity-evidence/connectivity-dom-regressions-v1/runtime/Lea.Script.Tests.exe'
    Pin $oldTests '6E46584EA1539D331D8FC17D73F8AC59081BF6BF98E31A7B89F0BF6375890D48'
    $testLibrary=Join-Path $runtime 'Lea.Script.Tests.exe';Copy-Item -LiteralPath $oldTests -Destination $testLibrary
    Pin $testLibrary '6E46584EA1539D331D8FC17D73F8AC59081BF6BF98E31A7B89F0BF6375890D48'
    $sdkPath='I:/LeaBuildStorage/sdk-reproducibility-20261101-v1/result.json';Pin $sdkPath '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
    $sdk=Get-Content $sdkPath -Raw|ConvertFrom-Json
    foreach($name in @('Lea.Win.dll','Lea.Script.Win.dll')){
        $product=@($sdk.products|Where-Object path -CEQ ('bin/Release/'+$name))
        if($product.Count -ne 1 -or !$product[0].equal){throw 'Exact view dependency required'}
        Pin $product[0].first $product[0].sha256;Copy-Item -LiteralPath $product[0].first -Destination $runtime
    }
    $fixtureProof=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v8/staging.json';Pin $fixtureProof '90FD0D60BA1CCE04C0923D6E18E7FB5D218F4F04C129DD49F97477AFFD2384CA'
    $manifest=Get-Content $fixtureProof -Raw|ConvertFrom-Json
    $row=@($manifest.files|Where-Object path -CEQ 'fixtures/LeaForge/test-pages/javascript-view.html')
    if($row.Count -ne 1){throw 'Original scheduling fixture required'}
    $fixture=$row[0].origin;Pin $fixture $row[0].sha256
'@
$text=$text.Substring(0,$a)+$bindings+[Environment]::NewLine+$text.Substring($b)
$a=$text.IndexOf('    $result.before=');$b=$text.IndexOf('}catch{',$a)
if($a -lt 0 -or $b -le $a){throw 'Scheduling run boundary differs'}
$body=@'
    $result.candidateOnly=$true;$result.completed=$false;$result.cases=[Collections.Generic.List[object]]::new()
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    foreach($name in @('Timers','Frames')){
        $run=Invoke-OwnedTestRun -Name ('trusted-'+$name) -Application $exe -Arguments @(('"'+$testLibrary+'"'),$name,('"'+$fixture+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
        $pattern='^PASS Lea scheduling '+$name+': ([0-9]+) checks\.$'
        $footers=@(Get-Content $run.stdout|Where-Object {$_ -cmatch $pattern})
        $count=if($footers.Count -eq 1 -and $footers[0] -cmatch $pattern){[int]$Matches[1]}else{0}
        $ok=$run.passed -and $count -gt 0
        $result.cases.Add(@{name=$name;passed=$ok;checks=$count})
        if(!$ok){throw ('Original trusted scheduling suite failed: '+$name)}
        foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Scheduling runtime drift prevents continuation'}}
        if($global:LeaCompositeProcessLeases.Count -or (Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'Scheduling retirement incomplete'}
    }
    $result.completed=$result.cases.Count -eq 2;$result.passed=$result.completed
    $result.scope='Original compiled trusted Timers.Suite and Frames.Run(trusted), unchanged assertions and budgets. Does not run timer scheduler/native/isolated branches or frame scheduler/protocol/native/isolated tests. No retry or release qualification.'
'@
$text=$text.Substring(0,$a)+$body+[Environment]::NewLine+$text.Substring($b)
if($Version -eq 2){$text=$text.Replace('continuity-evidence/global-install-scheduling-v1','continuity-evidence/global-install-scheduling-v2')}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Scheduling runner syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
