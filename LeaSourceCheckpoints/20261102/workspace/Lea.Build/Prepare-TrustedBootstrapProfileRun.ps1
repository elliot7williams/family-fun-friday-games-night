param([ValidateSet(1,2,3)][int]$Version=2)
$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-IsConnectedTrusted.ps1'
if((Get-FileHash $from).Hash -cne 'F48FB93BD317970D30CB3861AFE183E60A26A8F6D87893B6EFB9BFC513C449FF'){throw 'Cold trusted runtime template differs'}
$to=Join-Path $PSScriptRoot "Test-TrustedBootstrapProfile-v$Version.generated.ps1"
if(Test-Path $to){throw 'Preserve cold profile runner'}
$text=[IO.File]::ReadAllText($from).Replace('"continuity-evidence/is-connected-trusted-v$Version"',"'continuity-evidence/trusted-bootstrap-profile-v1'").Replace('is-connected-script-build-20261102-v1','trusted-bootstrap-profile-build-20261102-v1').Replace('2596ED5B2B5E5210324DBE403C1526E029E4F99240080C4434DCD028445D46F9','A9100D89DE8B062DB8CFD0CA1E6755FD62B422421AF4999BB49C363943BA29AF').Replace('FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C','7197F985895A8FA74B0AB7C81F382FD2BB1CFC3EFB994250D8DFD878F787CAFF').Replace('Lea.Tests/IsConnectedMain.cs','Lea.Tests/BootstrapComparisonMain.cs')
$text=$text.Replace('    $cases=Join-Path $root ''Lea.Tests/IsConnectedCases.js'';Pin $cases','')
$start=$text.IndexOf('    $result.before=');$end=$text.IndexOf('}catch{', $start)
if($start -lt 0 -or $end -le $start){throw 'Cold profile run boundary differs'}
$body=@'
    if($Isolated){throw 'Trusted cold profile only'}
    $result.diagnosticOnly=$true;$result.runtimeQualified=$false
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    $run=Invoke-OwnedTestRun -Name 'cold-trusted-profile' -Application $exe -Arguments @(('"'+$fixture+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    $report=@(Get-Content $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
    $rows=@(Get-Content $run.stderr|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
    $result.measurementCaptured=$report.Count -eq 1 -and $rows.Count -gt 0 -and $run.outputDrained -and $run.exitObserved
    if($report.Count -eq 1){$result.report=$report[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json}
    $result.profileRows=$rows.Count
    $result.passed=$run.passed -and $result.measurementCaptured -and $result.report.ok
    $result.scope='One cold diagnostic trusted bootstrap with buffered CPU/wall/GC stamps. Original script policy and30s/5s ownership retained. Observer effects possible; not a product fix/feature/reliability qualification.'
'@
$text=$text.Substring(0,$start)+$body+[Environment]::NewLine+$text.Substring($end)
if($Version -ge 2){
    $text=$text.Replace('trusted-bootstrap-profile-build-20261102-v1','trusted-bootstrap-profile-build-20261102-v2').Replace('A9100D89DE8B062DB8CFD0CA1E6755FD62B422421AF4999BB49C363943BA29AF','07DC2217126337F8A0938BB7652B8E730B694A6ECFD9593C846112B21636E927').Replace('7197F985895A8FA74B0AB7C81F382FD2BB1CFC3EFB994250D8DFD878F787CAFF','B6B0A59D9DB51560D58A55976A971275B93EE4828E0D05DF55594BA4B51F09F1').Replace('continuity-evidence/trusted-bootstrap-profile-v1','continuity-evidence/trusted-bootstrap-profile-v2')
}
if($Version -eq 3){
    $text=$text.Replace('trusted-bootstrap-profile-build-20261102-v2','trusted-bootstrap-profile-build-20261102-v3').Replace('07DC2217126337F8A0938BB7652B8E730B694A6ECFD9593C846112B21636E927','40B540471BEBE09DD44A14642DDF378F0CC7AF1ED59C1C9D2B34918956202FE3').Replace('B6B0A59D9DB51560D58A55976A971275B93EE4828E0D05DF55594BA4B51F09F1','A03E3608E2D98CEB8CEDBD6882052692E8AE98CA5552C3545C35F42422EA807E').Replace('continuity-evidence/trusted-bootstrap-profile-v2','continuity-evidence/trusted-bootstrap-profile-v3')
}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Cold profile runner syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
