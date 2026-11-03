$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-IsConnectedTrusted.ps1'
if((Get-FileHash $from).Hash -cne 'F48FB93BD317970D30CB3861AFE183E60A26A8F6D87893B6EFB9BFC513C449FF'){throw 'Trusted comparison template differs'}
$to=Join-Path $PSScriptRoot 'Test-ConnectivityBootstrapComparison-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve comparison runner'}
$text=[IO.File]::ReadAllText($from).Replace('"continuity-evidence/is-connected-trusted-v$Version"',"'continuity-evidence/connectivity-bootstrap-comparison-v1'").Replace("'Lea.Tests/IsConnectedMain.cs'","'Lea.Tests/BootstrapComparisonMain.cs'")
$start=$text.IndexOf('    $result.before=')
$end=$text.IndexOf('}catch{', $start)
if($start -lt 0 -or $end -le $start){throw 'Comparison run boundary differs'}
$body=@'
    if($Isolated){throw 'Trusted comparison only'}
    $old='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1\a\bin\Release\Lea.Script.dll'
    Pin $old 'D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'
    $baseline=Join-Path $out 'baseline'
    Copy-Item -LiteralPath $runtime -Destination $baseline -Recurse
    Copy-Item -LiteralPath $old -Destination (Join-Path $baseline 'Lea.Script.dll')
    foreach($file in Get-ChildItem $runtime -File){
        $other=Join-Path $baseline $file.Name
        $expected=if($file.Name -ceq 'Lea.Script.dll'){'D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'}else{(Get-FileHash $file.FullName).Hash}
        Pin $other $expected
    }
    $result.before=@(foreach($directory in @($runtime,$baseline)){Get-ChildItem $directory -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}}})
    $result.comparisonCompleted=$false;$result.observations=[Collections.Generic.List[object]]::new();$allPassed=$true;$iteration=0
    foreach($variant in @('baseline','candidate','candidate','baseline')){
        $iteration++;$directory=if($variant -ceq 'baseline'){$baseline}else{$runtime}
        $application=Join-Path $directory 'Lea.IsConnected.Tests.exe'
        $run=Invoke-OwnedTestRun -Name ('bootstrap-'+$iteration+'-'+$variant) -Application $application -Arguments @(('"'+$fixture+'"')) -WorkingDirectory $directory -OutputDirectory $out -Seconds 30 -Records $runs
        if($run.timedOut -or $run.killRequested -or !$run.outputDrained -or $run.activeAfterRetirement -ne 0 -or $global:LeaCompositeProcessLeases.Count){throw 'Unsafe comparative attempt prevents continuation'}
        $report=@(Get-Content $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
        if($report.Count -ne 1){throw 'Unique comparison report required'}
        $observation=$report[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json
        $result.observations.Add(@{iteration=$iteration;variant=$variant;passed=$run.passed;report=$observation})
        $allPassed=$allPassed -and $run.passed
        foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Comparative runtime drift prevents continuation'}}
    }
    $result.comparisonCompleted=$result.observations.Count -eq 4
    $result.scope='Fixed ABBA comparison, identical executable/dependencies/fixture/code except script DLL; original default script budget and per-process30s/5scleanup; not feature semantics or causal/reliability proof. Four preplanned attempts, no retries.'
    $result.passed=$allPassed -and $result.comparisonCompleted
'@
$text=$text.Substring(0,$start)+$body+[Environment]::NewLine+$text.Substring($end)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Comparison syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
