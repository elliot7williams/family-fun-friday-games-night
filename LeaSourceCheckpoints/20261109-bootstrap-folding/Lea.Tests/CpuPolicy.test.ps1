$ErrorActionPreference='Stop'
Add-Type -Path (Join-Path (Split-Path $PSScriptRoot) 'Lea.Build/CurrentCpuPolicy.cs')
if([Lea.Build.CurrentCpuPolicy]::Decode(0,0).Mode -cne 'disabled'){throw 'Disabled policy decode failed'}
$hard=[Lea.Build.CurrentCpuPolicy]::Decode(5,1000)
if($hard.Mode -cne 'hard-cap' -or $hard.Percent -ne 10){throw 'Hard cap decode failed'}
$weight=[Lea.Build.CurrentCpuPolicy]::Decode(3,9)
if($weight.Mode -cne 'weight' -or $weight.Weight -ne 9 -or $null -ne $weight.Percent){throw 'Weight union decode failed'}
$range=[Lea.Build.CurrentCpuPolicy]::Decode(17,((3000 -shl 16) -bor 100))
if($range.Mode -cne 'min-max' -or $range.MinimumRate -ne 100 -or $range.MaximumRate -ne 3000 -or $null -ne $range.Percent){throw 'Range union decode failed'}
$rate=[Lea.Build.CurrentCpuPolicy]::Decode(1,10000)
if($rate.Mode -cne 'rate' -or $rate.Percent -ne 100){throw 'Plain rate decode failed'}
$checks=5
foreach($pair in @(@(4,1000),@(7,1000),@(21,1000),@(3,10),@(5,0),@(5,10001),@(17,((100 -shl 16) -bor 3000)),@(32,1000))){
    $rejected=$false;try{$null=[Lea.Build.CurrentCpuPolicy]::Decode($pair[0],$pair[1])}catch [Management.Automation.MethodInvocationException]{$rejected=$true}
    if(!$rejected){throw 'Invalid CPU policy accepted'};$checks++
}
"PASS CPU policy decoder: $checks cases"
[Lea.Build.CurrentCpuPolicy]::Capture()|ConvertTo-Json -Depth 3
