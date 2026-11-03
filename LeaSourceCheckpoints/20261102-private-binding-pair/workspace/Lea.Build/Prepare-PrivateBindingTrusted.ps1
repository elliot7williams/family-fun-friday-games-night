$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
function Derive([string]$from,[string]$expected,[string]$to,$pairs){
    if(Test-Path $to){throw 'Preserve private binding semantic artifacts'}
    if((Get-FileHash $from).Hash -cne $expected){throw 'Semantic template changed'}
    $original=[IO.File]::ReadAllText($from);$text=$original
    foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique semantic projection boundary required'};$text=$text.Replace($pair[0],$pair[1])}
    $inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
    if($inverse -cne $original){throw 'Semantic projection inverse differs'}
    [IO.File]::WriteAllText($to,$text)
    Get-FileHash $to
}
$harnessPairs=@(@('internal static class EventMapTrustedMain','internal static class PrivateBindingTrustedMain'),@('PASS Lea event-map trusted ','PASS Lea private-binding trusted '))
Derive (Join-Path $root 'Lea.Tests/EventMapTrustedMain.cs') '48D227CCCCE888BBF0A9490FA3525B769775CDDFDDEC525F696B81B186CDE2FD' (Join-Path $root 'Lea.Tests/PrivateBindingTrustedMain.cs') $harnessPairs
$runnerPairs=@(
    @('event-map-trusted-v1','private-binding-trusted-v1'),
    @('event-map-build-20261102-v1','private-binding-build-20261102-v1'),
    @('4EB46F1448E476A466B8DE01A179DF4EFDBA7DF8738DC5E00EE67878935F4202','D32732C0BF3B548DB649966100D4B074663E07544BC9C25A648A80D6723AB02C'),
    @('B245644C86BC706F1F54CB896B138419A7D9CECD9150F525E0F7790BF5C01A2E','406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F'),
    @('Lea.Tests/EventMapTrustedMain.cs','Lea.Tests/PrivateBindingTrustedMain.cs'),
    @('PASS Lea event-map trusted ','PASS Lea private-binding trusted ')
)
Derive (Join-Path $PSScriptRoot 'Test-EventMapTrusted-v1.generated.ps1') '83C24CD1C756BDA90D9A25575716C3A0873232C7040E8E10FBC55D0B624A3CE1' (Join-Path $PSScriptRoot 'Test-PrivateBindingTrusted-v1.generated.ps1') $runnerPairs
