$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-ProcessStateHost.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '4B9C89B8413E300031DE1CB217917D29002395CE009573EFC3726BF4B0316E85'){throw 'Diagnostic runner changed'}
$target=Join-Path $PSScriptRoot 'Test-StartupCampaign.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve campaign runner'}
$text=[IO.File]::ReadAllText($base)
$changes=@(
    @('\temporal-startup-v1\host-process-state-v1','\temporal-startup-v1\startup-campaign-v1'),
    @("foreach(`$case in @('script')){","foreach(`$attempt in 1..3){`r`n        `$case='script'"),
    @("('host-'+`$case)","('host-'+`$case+'-'+`$attempt)"),
    @('$result.expectedRuns=2','$result.expectedRuns=4'),
    @('$runs.Count-eq2-and','$runs.Count-eq4-and'),
    @('Settings and script-host diagnostic only; no full host or release validation','Predetermined three separate host scripting launches plus settings; retain all outcomes, not a cold-start benchmark or full release validation')
)
foreach($pair in $changes){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Ambiguous campaign edit'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Invalid campaign syntax'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
