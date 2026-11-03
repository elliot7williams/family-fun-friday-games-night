$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Assemble-RetirementPrefix-v1.generated.ps1'
if((Get-FileHash $template).Hash -cne '8F7177FE929EB7A218DDC846BAE3B335BEDF4E3FEF6060ECE3A9FC47C5C845B5'){throw 'Baseline assembler differs'}
$to=Join-Path $PSScriptRoot 'Assemble-RetirementBound-v2.generated.ps1'
if(Test-Path $to){throw 'Preserve bound assembler'}
$text=[IO.File]::ReadAllText($template).Replace('I:\LeaBuildStorage\retirement-full-entry-20261102-v1','I:\LeaBuildStorage\retirement-full-entry-20261102-v2').Replace('I:\LeaBuildStorage\retirement-prefix-baseline-20261102-v1','I:\LeaBuildStorage\retirement-prefix-diagnostic-20261102-v2').Replace('8FC9160A1BCB659D2503445C505213B9CAD62CA73DE667042401C4A4DE07AC1D','2E913DDA683A9AF5684BB28EDDCD743D7BCE7971D1125607FDC9698B5E18D659')
$old="    if(`$Replay -and `$hostBuild.diagnosticOnly){throw 'Replay cannot use diagnostic host'}"
$new="    if(!`$Replay -or !`$hostBuild.diagnosticOnly -or !`$hostBuild.diagnosticParentInverseVerified -or `$hostBuild.diagnosticParentSha256 -cne 'E5896C436C840022CC4B840F866F7D7F5D92B5E0CA1B546B20AC19C5A1FB1111'){throw 'Exact diagnostic parent binding required'}"
if(!$text.Contains($old)){throw 'Original scope guard absent'};$text=$text.Replace($old,$new)
$needle='    if($changes.Count -ne $(if($Replay){3}else{4}))'
if(!$text.Contains($needle)){throw 'Original change-count guard absent'}
$insert=@'
    $diagnostic='I:\LeaBuildStorage\worker-retirement-trace-build-20261102-v2\source\bin\Release\Lea.Script.dll'
    if($selected['runtime/Lea.Script.dll'].sha256 -cne 'D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'){throw 'Original script binding differs'}
    Replace 'runtime/Lea.Script.dll' $diagnostic 'E5896C436C840022CC4B840F866F7D7F5D92B5E0CA1B546B20AC19C5A1FB1111'
'@
$text=$text.Replace($needle,$insert+[Environment]::NewLine+'    if($changes.Count -ne 4)')
$text=$text.Replace('diagnosticOnly=(!$Replay);uninstrumentedReplay=[bool]$Replay','diagnosticOnly=$true;uninstrumentedReplay=$false')
$text=$text.Replace('Uninstrumented test sources; only fresh entry-resource/manifest/dispatcher bindings, engine products/all phase calls unchanged','Diagnostic script DLL plus exact entry-resource/manifest/dispatcher bindings; all original test assertions, Contains and phase calls retained. Not release evidence')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Bound assembler syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
