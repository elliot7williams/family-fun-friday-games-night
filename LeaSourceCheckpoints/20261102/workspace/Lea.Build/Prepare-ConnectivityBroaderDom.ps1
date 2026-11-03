$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-IsConnectedNative-v1.generated.ps1'
if((Get-FileHash $from).Hash -cne '4AE6E2A697C84EE9860338140E8F5F521FDF1CF2855EC88F5643D636A1CC44D7'){throw 'Candidate runtime template differs'}
$to=Join-Path $PSScriptRoot 'Test-ConnectivityBroaderDom-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve broader DOM campaign'}
$text=[IO.File]::ReadAllText($from).Replace('continuity-evidence/is-connected-native-v1','continuity-evidence/connectivity-broader-dom-v1').Replace('Lea.Tests/IsConnectedNativeMain.cs','Lea.Tests/DomRegressionMain.cs')
$text=$text.Replace('    $fixture=Join-Path $root ''LeaForge/test-pages/is-connected-native.html'';Pin $fixture','').Replace('    $cases=Join-Path $root ''Lea.Tests/IsConnectedCases.js'';Pin $cases','')
$needle='    $source=Join-Path'
if([regex]::Matches($text,[regex]::Escape($needle)).Count -ne 1){throw 'Unique test-input boundary required'}
$bindings=@'
    $oldTests=Join-Path $root 'continuity-evidence/connectivity-dom-regressions-v1/runtime/Lea.Script.Tests.exe'
    Pin $oldTests '6E46584EA1539D331D8FC17D73F8AC59081BF6BF98E31A7B89F0BF6375890D48'
    $testLibrary=Join-Path $runtime 'Lea.Script.Tests.exe';Copy-Item -LiteralPath $oldTests -Destination $testLibrary
    Pin $testLibrary '6E46584EA1539D331D8FC17D73F8AC59081BF6BF98E31A7B89F0BF6375890D48'
    $fixtureProof=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v8/staging.json';Pin $fixtureProof '90FD0D60BA1CCE04C0923D6E18E7FB5D218F4F04C129DD49F97477AFFD2384CA'
    $fixtureManifest=Get-Content $fixtureProof -Raw|ConvertFrom-Json
    $fixtures=@{}
    foreach($name in @('selectors','tree','attributes')){
        $relative='fixtures/LeaForge/test-pages/javascript-'+$name+'.html'
        $file=@($fixtureManifest.files|Where-Object path -CEQ $relative)
        if($file.Count -ne 1){throw 'Unique original fixture required'}
        Pin $file[0].origin $file[0].sha256
        $path=Join-Path $out ('fixtures/'+$name+'.html');New-Item -ItemType Directory -Path (Split-Path $path) -Force|Out-Null
        Copy-Item -LiteralPath $file[0].origin -Destination $path;Pin $path $file[0].sha256;$fixtures.Add($name,$path)
    }
'@
$text=$text.Replace($needle,$bindings+[Environment]::NewLine+$needle)
$start=$text.IndexOf('    $result.before=');$end=$text.IndexOf('}catch{', $start)
if($start -lt 0 -or $end -le $start){throw 'Original run boundary differs'}
$body=@'
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    $trace=Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1';Pin $trace '6454310B2E41C38B25C9A7085687BCADF9C7D7F95B3DF7D96E7F702DC9241CDF';. $trace
    $result.cases=[Collections.Generic.List[object]]::new();$result.completed=$false;$result.passed=$true
    foreach($case in @(@{name='Selectors';fixture='selectors'},@{name='Tree';fixture='tree'},@{name='Attributes';fixture='attributes'},@{name='ClassLists';fixture='attributes'})){
        $run=Invoke-OwnedTestRun -Name ('original-'+$case.name) -Application $exe -Arguments @(('"'+$testLibrary+'"'),$case.name,('"'+$fixtures[$case.fixture]+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
        $pattern='^PASS Lea DOM regression '+[regex]::Escape($case.name)+': ([0-9]+) checks\.$'
        $footers=@(Get-Content $run.stdout|Where-Object {$_ -cmatch $pattern})
        $count=if($footers.Count -eq 1 -and $footers[0] -cmatch $pattern){[int]$Matches[1]}else{0}
        $proof=Read-AllStartupTraceProof -Lines @(Get-Content $run.stderr) -StreamsComplete ([bool]($run.stdoutComplete -and $run.stderrComplete)) -ExitObserved ([bool]$run.exitObserved) -TimedOut ([bool]$run.timedOut) -KillRequested ([bool]$run.killRequested)
        $ok=$run.passed -and $footers.Count -eq 1 -and $count -gt 0 -and $proof.traceActivationVerified -and $proof.traceEnvironmentRestored
        $result.cases.Add(@{name=$case.name;passed=$ok;checks=$count;traceActivationVerified=$proof.traceActivationVerified;traceEnvironmentRestored=$proof.traceEnvironmentRestored})
        if(!$ok){$result.passed=$false;throw ('Unchanged DOM suite failed: '+$case.name)}
        foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Runtime drift prevents next original suite'}}
        if($global:LeaCompositeProcessLeases.Count -or (Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'Original suite process retirement incomplete'}
    }
    $result.completed=$result.cases.Count -eq 4;$result.passed=$result.passed -and $result.completed
    $result.scope='Original compiled Selectors/Tree/Attributes/ClassLists Run methods, both trusted and isolated branches; reflection driver only, no assertion/budget changes. Not full suite/host/startup reliability qualification.'
'@
$text=$text.Substring(0,$start)+$body+[Environment]::NewLine+$text.Substring($end)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Broader DOM campaign syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
