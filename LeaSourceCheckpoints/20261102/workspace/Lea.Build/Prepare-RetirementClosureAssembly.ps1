$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Assemble-RetirementBound-v2.generated.ps1'
if((Get-FileHash $from).Hash -cne '62017784851715DD96D199511F2E166D0E53F0F518B4E283849D3ED4B8790584'){throw 'Bound assembly source differs'}
$to=Join-Path $PSScriptRoot 'Assemble-RetirementClosure-v3.generated.ps1'
if(Test-Path $to){throw 'Preserve closure assembler'}
$text=[IO.File]::ReadAllText($from).Replace('retirement-prefix-diagnostic-20261102-v2','retirement-prefix-diagnostic-20261102-v3')
$needle='    if($changes.Count -ne 4)'
if([regex]::Matches($text,[regex]::Escape($needle)).Count -ne 1){throw 'Unique closure boundary required'}
$insert=@'
    if($selected['runtime/WorkerEntryProbe/Lea.Script.dll'].sha256 -cne 'D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'){throw 'Original private parent differs'}
    Replace 'runtime/WorkerEntryProbe/Lea.Script.dll' $diagnostic 'E5896C436C840022CC4B840F866F7D7F5D92B5E0CA1B546B20AC19C5A1FB1111'
'@
$text=$text.Replace($needle,$insert+[Environment]::NewLine+'    if($changes.Count -ne 5)')
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
