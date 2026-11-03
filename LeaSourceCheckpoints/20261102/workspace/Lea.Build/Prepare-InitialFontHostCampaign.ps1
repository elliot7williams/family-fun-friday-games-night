param([switch]$RelativePair)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintCommandHostCampaign.generated.ps1'
if((Get-FileHash $source).Hash -cne '0DED365D47D5EC0E29F0D2523B47B4911628EFF7796B611512BA636A61A58D93'){throw 'Pinned host campaign differs'}
$target=Join-Path $PSScriptRoot 'Test-InitialFontHostCampaign.generated.ps1'
if($RelativePair){$target=Join-Path $PSScriptRoot 'Test-RelativeFontHostCampaign.generated.ps1'}
if(Test-Path $target){throw 'Preserve current host campaign'}
$original=[IO.File]::ReadAllText($source);$text=$original
$edits=@(
    @('host-print-command-owned-v1','host-initial-font-owned-v1'),
    @('print-command-host-v2','initial-font-host-v1'),
    @('1C60031947F0486701B0369A490BFE8B5052936E332C2CC4CE7DFEC8E160FECD','BA33363FEBA025AA499FC38496478C0672FFE9A9EAAC33D6D87D389C88514E7C'),
    @('FCFAA635B4403BC4F449761C49BA17F6ED9CFEF443D7164C56C924DDB09FA7C7','D10DB2F9D9303230839AEF4314CBF56EE5FD81E300D86301DFBA0C4D18B8F75E'),
    @('continuity-evidence/print-live-capture-v1/Lea.Engine.dll','continuity-evidence/font-initial-render-v1/Lea.Engine.dll'),
    @('A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791','1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'),
    @('continuity-evidence/print-live-capture-v1/Lea.Win.dll','continuity-evidence/font-initial-render-v1/Lea.Win.dll'),
    @('568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8','6E342B9B68C54803B91CF796F082C788DC86E29C1A7CEFAA571302E5E4636B4F'),
    @('continuity-evidence/print-ticket-project-v1/bin/Lea.Print.dll','continuity-evidence/initial-font-print-project-v1/bin/Lea.Print.dll'),
    @('3C014359204CEEDD11E8164A28467D46CB3BA83E5DF6A81802C0F3A7D3AF1795','271BA491D0496E990DC67B8B849310241BCE277F4E0CDF6070C1854305812C1C'),
    @('FCFAA print-command host coreA85/view568/print3C014','D10DB current-font host core1474/view6E34/print271B')
)
if($RelativePair){
    $updates=@{
        'host-initial-font-owned-v1'='host-relative-font-owned-v1'
        'initial-font-host-v1'='relative-font-host-v1'
        'BA33363FEBA025AA499FC38496478C0672FFE9A9EAAC33D6D87D389C88514E7C'='452492446FCAE2FD9F2DFCC5A6185ED4F05B977B89FADD584A2B6DC83150A6F6'
        'D10DB2F9D9303230839AEF4314CBF56EE5FD81E300D86301DFBA0C4D18B8F75E'='E7F49DB9D59A6C73D92A3B40CA2101C61005C8A5B1BDB583D24946D6B8A5EF67'
        'continuity-evidence/font-initial-render-v1/Lea.Engine.dll'='continuity-evidence/font-relative-render-v1/Lea.Engine.dll'
        '1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'='4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA'
        'continuity-evidence/font-initial-render-v1/Lea.Win.dll'='continuity-evidence/font-relative-render-v1/Lea.Win.dll'
        '6E342B9B68C54803B91CF796F082C788DC86E29C1A7CEFAA571302E5E4636B4F'='CA51FC818B119529EA8F7104D0753C5779070A7CCF0AD157675A564C4755C584'
        'continuity-evidence/initial-font-print-project-v1/bin/Lea.Print.dll'='continuity-evidence/relative-font-print-project-v1/bin/Lea.Print.dll'
        '271BA491D0496E990DC67B8B849310241BCE277F4E0CDF6070C1854305812C1C'='0C16B3D0113A91F83274F37D48C4D7B5F9BAC8DC1E292FBBBF318BF908711CFB'
        'D10DB current-font host core1474/view6E34/print271B'='E7F49 relative-font host core4DA8/viewCA51/print0C16'
    }
    foreach($edit in $edits){if(!$updates.ContainsKey($edit[1])){throw 'Relative host mapping incomplete'};$edit[1]=$updates[$edit[1]]}
}
$counts=[Collections.Generic.List[int]]::new()
foreach($edit in $edits){$count=[regex]::Matches($text,[regex]::Escape($edit[0])).Count;if(!$count -or $text.Contains($edit[1])){throw 'Ambiguous campaign substitution'};$counts.Add($count);$text=$text.Replace($edit[0],$edit[1])}
function Valid([string]$value){
    for($i=$edits.Count-1;$i -ge 0;$i--){if([regex]::Matches($value,[regex]::Escape($edits[$i][1])).Count -ne $counts[$i]){return $false};$value=$value.Replace($edits[$i][1],$edits[$i][0])}
    return $value -ceq $original
}
if(!(Valid $text)){throw 'Campaign inverse differs'}
foreach($bad in @(($text+"`n"),($text+$text),$text.Replace('8.0.33','8.0.32'))){if($bad -ceq $text -or (Valid $bad)){throw 'Campaign negative accepted'}}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Campaign syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $text){throw 'Campaign readback differs'}
Write-Output ('Prepared exact-inverse/syntax/3-negative checked campaign '+(Get-FileHash $target).Hash)
