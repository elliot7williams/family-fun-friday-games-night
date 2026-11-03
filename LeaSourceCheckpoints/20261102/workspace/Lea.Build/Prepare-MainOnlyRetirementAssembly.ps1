$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Assemble-RetirementPrefix-v1.generated.ps1'
if((Get-FileHash $from).Hash -cne '8F7177FE929EB7A218DDC846BAE3B335BEDF4E3FEF6060ECE3A9FC47C5C845B5'){throw 'Original-parent assembler differs'}
$to=Join-Path $PSScriptRoot 'Assemble-MainOnlyRetirement-v4.generated.ps1'
if(Test-Path $to){throw 'Preserve main-only assembler'}
$text=[IO.File]::ReadAllText($from).Replace('retirement-full-entry-20261102-v1','retirement-full-entry-20261102-v4').Replace('retirement-prefix-baseline-20261102-v1','retirement-prefix-main-only-20261102-v4').Replace('8FC9160A1BCB659D2503445C505213B9CAD62CA73DE667042401C4A4DE07AC1D','A2346D5030FC74C542CF87738CDB4B81478627E266536222B463C954DA428715').Replace('continuity-evidence/retirement-full-prefix-v1/runtime/WorkerEntryFixture','continuity-evidence/retirement-full-prefix-v2/runtime/WorkerEntryFixture')
$needle='    if($changes.Count -ne $(if($Replay){3}else{4}))'
if(!$text.Contains($needle)){throw 'Exact projection count guard absent'}
$body=@'
    $buildPath='I:\LeaBuildStorage\worker-retirement-trace-build-20261102-v2/result.json'
    $diagnosticBuild=Proof $buildPath '8B971B3212FEDAD1F5F01380FDC963F4F862BB683CB01B77356535774537ECEC'
    if(!$diagnosticBuild.passed -or !$diagnosticBuild.inputsUnchanged -or $diagnosticBuild.ownerAttention -or $diagnosticBuild.sha256 -cne 'E5896C436C840022CC4B840F866F7D7F5D92B5E0CA1B546B20AC19C5A1FB1111'){throw 'Diagnostic script provenance incomplete'}
    $parent='D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'
    if($selected['runtime/Lea.Script.dll'].sha256 -cne $parent -or $selected['runtime/WorkerEntryProbe/Lea.Script.dll'].sha256 -cne $parent){throw 'Original scripting copies differ'}
    $entryManifest=Get-Content $selected['runtime/WorkerEntryProbe/manifest.json'].origin -Raw|ConvertFrom-Json -AsHashtable
    if($entryManifest.files['Lea.Script.dll'] -cne $parent){throw 'Private helper must retain original parent identity'}
    Replace 'runtime/Lea.Script.dll' $diagnosticBuild.binary $diagnosticBuild.sha256
'@
$text=$text.Replace($needle,$body+[Environment]::NewLine+'    if($changes.Count -ne 4)')
$text=$text.Replace('diagnosticOnly=(!$Replay);uninstrumentedReplay=[bool]$Replay','diagnosticOnly=$true;uninstrumentedReplay=$false')
$text=$text.Replace('Uninstrumented test sources; only fresh entry-resource/manifest/dispatcher bindings, engine products/all phase calls unchanged','Main-runtime-only diagnostic script DLL plus fresh original-parent helper/manifest/dispatcher; private helper keeps zero-stderr contract, original test assertions and phase calls unchanged')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Main-only assembler syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
