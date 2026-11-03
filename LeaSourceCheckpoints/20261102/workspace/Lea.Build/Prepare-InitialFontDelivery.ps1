param([ValidatePattern('^[A-F0-9]{64}$')][string]$ArchiveSha256,[switch]$RelativePair)
$ErrorActionPreference='Stop'
function Project([string]$source,[string]$pin,[string]$target,[array]$edits){
    $from=Join-Path $PSScriptRoot $source;$to=Join-Path $PSScriptRoot $target
    if((Get-FileHash $from).Hash -cne $pin -or (Test-Path $to)){throw 'Pinned source/fresh projection required'}
    $original=[IO.File]::ReadAllText($from);$text=$original;$counts=[Collections.Generic.List[int]]::new()
    foreach($edit in $edits){$count=[regex]::Matches($text,[regex]::Escape($edit[0])).Count;if(!$count -or $text.Contains($edit[1])){throw ('Delivery projection ambiguous: '+$edit[0])};$counts.Add($count);$text=$text.Replace($edit[0],$edit[1])}
    function Valid([string]$value){for($i=$edits.Count-1;$i -ge 0;$i--){if([regex]::Matches($value,[regex]::Escape($edits[$i][1])).Count -ne $counts[$i]){return $false};$value=$value.Replace($edits[$i][1],$edits[$i][0])};return $value -ceq $original}
    if(!(Valid $text)){throw 'Delivery projection inverse differs'}
    foreach($bad in @(($text+"`n"),($text+$text),$text.Replace('8.0.33','8.0.32'))){if($bad -ceq $text -or (Valid $bad)){throw 'Delivery negative accepted'}}
    $tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Delivery syntax invalid'}
    [IO.File]::WriteAllText($to,$text,[Text.UTF8Encoding]::new($false))
    if([IO.File]::ReadAllText($to) -cne $text){throw 'Delivery readback differs'}
    Write-Output ('Prepared inverse/syntax/3negative checked '+$target+' '+(Get-FileHash $to).Hash)
}
$common=@(
    @('host-print-command-owned-v1','host-initial-font-owned-v1'),
    @('82C0C3CFCEECC781622FE265AD90EBA540F7F2BCD230F29ADB988F5DFA4BFC1A','7134EFAB5DE03ACE4A7A915579F54431601DAC610B343E3759EB551E6EDC08C2'),
    @('Firespider-Lea-print-development-20261028.zip','Firespider-Lea-font-development-20261031.zip'),
    @('FCFAA635B4403BC4F449761C49BA17F6ED9CFEF443D7164C56C924DDB09FA7C7','D10DB2F9D9303230839AEF4314CBF56EE5FD81E300D86301DFBA0C4D18B8F75E')
)
if($RelativePair){
    $updates=@{
        'host-initial-font-owned-v1'='host-relative-font-owned-v1'
        '7134EFAB5DE03ACE4A7A915579F54431601DAC610B343E3759EB551E6EDC08C2'='D9EEB35C3B9AB747640505EF07CBD03BB6D27FBD491F66F253E44F76D3D72A3B'
        'Firespider-Lea-font-development-20261031.zip'='Firespider-Lea-relative-font-development-20261031.zip'
        'D10DB2F9D9303230839AEF4314CBF56EE5FD81E300D86301DFBA0C4D18B8F75E'='E7F49DB9D59A6C73D92A3B40CA2101C61005C8A5B1BDB583D24946D6B8A5EF67'
    }
    foreach($edit in $common){if(!$updates.ContainsKey($edit[1])){throw 'Relative delivery mapping incomplete'};$edit[1]=$updates[$edit[1]]}
    $common+=,@('$archive=Join-Path $workspace ''dist/LeaExes/Firespider-Lea-relative-font-development-20261031.zip''','$archive=''I:\LeaBuildStorage\Firespider-Lea-relative-font-development-20261031.zip''')
}
if($ArchiveSha256){
    $edits=$common+@(@('print-command-package-extraction-v1','initial-font-package-extraction-v1'),@('B5C452C4932C9CBFA7582601EBE9F1F1A8E84000B62F0880B2C586E067863196',$ArchiveSha256))
    $target='Test-InitialFontExtraction.generated.ps1'
    if($RelativePair){$edits[-2][1]='relative-font-package-extraction-v1';$target='Test-RelativeFontExtraction.generated.ps1'}
    Project 'Test-PrintCommandExtraction.generated.ps1' 'B7E502D66A133BAF6485660168D9D433C232D66F048DD8011C4D0EA5360CDE4F' $target $edits
}else{
    $anchor='    $manifest=@{developmentOnly=$true;'
    $checkpoint=@'
    $currentSource='I:\LeaBuildStorage\source-20261031-v7\current-source.zip'
    $currentSourceHash='7EA8735394A7084EA1F9FA08E1C5CC8876A76E7EAB929EFE142FE6016CB31710'
    $null=Hold $currentSource $currentSourceHash
    $manifest=@{currentSourceCheckpoint=@{path=$currentSource;sha256=$currentSourceHash;githubUploaded=$false};developmentOnly=$true;
'@
    if($RelativePair){$checkpoint=$checkpoint.Replace('source-20261031-v7','source-20261031-v14').Replace('7EA8735394A7084EA1F9FA08E1C5CC8876A76E7EAB929EFE142FE6016CB31710','87F8AEFDD28EBF73BF2254B9D5309C9C095BFD4F94DC0EC546C7D6E6C0C21F59')}
    $edits=$common+@(
        @('print-command-development-package-v1','initial-font-development-package-v1'),
        @('568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8','6E342B9B68C54803B91CF796F082C788DC86E29C1A7CEFAA571302E5E4636B4F'),
        @('3C014359204CEEDD11E8164A28467D46CB3BA83E5DF6A81802C0F3A7D3AF1795','271BA491D0496E990DC67B8B849310241BCE277F4E0CDF6070C1854305812C1C'),
        @('A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791','1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'),
        @('Print-command development build, 2026-10-28','Font development build, 2026-10-31'),
        @('FCFAA/coreA85/view568/print3C014','D10DB/core1474/view6E34/print271B'),
        @('One decoder ACL delta was classified by existing unchanged','Zero runtime byte or ACL changes were observed under existing unchanged'),
        @($anchor,$checkpoint)
    )
    $target='Package-InitialFontDevelopment.generated.ps1'
    if($RelativePair){
        $updates=@{
            'initial-font-development-package-v1'='relative-font-development-package-v1'
            '6E342B9B68C54803B91CF796F082C788DC86E29C1A7CEFAA571302E5E4636B4F'='CA51FC818B119529EA8F7104D0753C5779070A7CCF0AD157675A564C4755C584'
            '271BA491D0496E990DC67B8B849310241BCE277F4E0CDF6070C1854305812C1C'='0C16B3D0113A91F83274F37D48C4D7B5F9BAC8DC1E292FBBBF318BF908711CFB'
            '1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'='4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA'
            'Font development build, 2026-10-31'='Relative-font development build, 2026-10-31'
            'D10DB/core1474/view6E34/print271B'='E7F49/core4DA8/viewCA51/print0C16'
        }
        foreach($edit in $edits){if($updates.ContainsKey($edit[1])){$edit[1]=$updates[$edit[1]]}}
        $old='    foreach($reference in $references){$null=Hold (Join-Path $workspace (''continuity-evidence/''+$reference.name)) $reference.sha256}'
        $new='    foreach($reference in $references){$reference.path=if($reference.name -ceq ''runtime-8033-matched-source-20261027.zip''){''L:\Lea-source-backup-archive-20261029-uia\runtime-8033-matched-source-20261027.zip''}else{Join-Path $workspace (''continuity-evidence/''+$reference.name)};$null=Hold $reference.path $reference.sha256}'
        $edits+=,@($old,$new);$target='Package-RelativeFontDevelopment.generated.ps1'
    }
    Project 'Package-PrintCommandDevelopment.generated.ps1' 'A11CD4BD7F820C3EDE5D9B9D24E12C62D4063D485F285F521F12702CADA57FF8' $target $edits
}
