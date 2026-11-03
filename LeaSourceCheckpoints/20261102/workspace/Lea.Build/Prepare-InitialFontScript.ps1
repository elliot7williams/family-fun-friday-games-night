param([switch]$RelativePair)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Prepare-FontScript.ps1'
if((Get-FileHash $source).Hash -cne '6230CA38F946B14467EEEC8EF2B3E73FE5D129EC0CFE3892620495F323753747'){throw 'Original preparation changed'}
$target=Join-Path $PSScriptRoot 'Prepare-InitialFontScript.generated.ps1'
if($RelativePair){$target=Join-Path $PSScriptRoot 'Prepare-RelativeFontScript.generated.ps1'}
if(Test-Path $target){throw 'Preserve prepared projection'}
$original=[IO.File]::ReadAllText($source);$text=$original
$edits=@(
    @('font-script-preparation-v1','initial-font-script-preparation-v1'),
    @('font-shorthand-render-v2','font-initial-render-v1'),
    @('9A295BF4281A60B0B20245EAFDE681614972D4460A8DE0CBAF637A3EDD1AAE7A','9774F4B14D7F0E4D783961529A9A6F5A1969A81A6DFEC060F0A75696AC867649'),
    @('0B09D9193A5819F21B055E29E8C463D32F746D7523C32FEA4E8DA25D2C30B514','1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'),
    @('A8CC9BD3693121B8931A1DE7767E8E851C9455C7B3D91452A104E9940B4AAC43','6E342B9B68C54803B91CF796F082C788DC86E29C1A7CEFAA571302E5E4636B4F'),
    @('font-script-suite-v1','initial-font-script-suite-v1'),
    @('font-script-owned-run-v1','initial-font-script-owned-run-v1'),
    @('FontScriptSuite-v1.generated.ps1','InitialFontScriptSuite-v1.generated.ps1'),
    @('viewA8CC/core0B09','view6E34/core1474'),
    @('core0B09/viewA8CC','core1474/view6E34')
)
if($RelativePair){
    $updates=@{
        'initial-font-script-preparation-v1'='relative-font-script-preparation-v1'
        'font-initial-render-v1'='font-relative-render-v1'
        '9774F4B14D7F0E4D783961529A9A6F5A1969A81A6DFEC060F0A75696AC867649'='5FCA5A4730ECA3F2894533665FC22CAA9BA520FA49A2B71622B8B45D295E03A0'
        '1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'='4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA'
        '6E342B9B68C54803B91CF796F082C788DC86E29C1A7CEFAA571302E5E4636B4F'='CA51FC818B119529EA8F7104D0753C5779070A7CCF0AD157675A564C4755C584'
        'initial-font-script-suite-v1'='relative-font-script-suite-v1'
        'initial-font-script-owned-run-v1'='relative-font-script-owned-run-v1'
        'InitialFontScriptSuite-v1.generated.ps1'='RelativeFontScriptSuite-v1.generated.ps1'
        'view6E34/core1474'='viewCA51/core4DA8'
        'core1474/view6E34'='core4DA8/viewCA51'
    }
    foreach($edit in $edits){if(!$updates.ContainsKey($edit[1])){throw 'Relative projection mapping incomplete'};$edit[1]=$updates[$edit[1]]}
}
$counts=[Collections.Generic.List[int]]::new()
foreach($edit in $edits){
    $count=[regex]::Matches($text,[regex]::Escape($edit[0])).Count
    if(!$count -or $text.Contains($edit[1])){throw 'Ambiguous initial font projection'}
    $counts.Add($count);$text=$text.Replace($edit[0],$edit[1])
}
function Valid([string]$value){
    for($i=$edits.Count-1;$i -ge 0;$i--){
        if([regex]::Matches($value,[regex]::Escape($edits[$i][1])).Count -ne $counts[$i]){return $false}
        $value=$value.Replace($edits[$i][1],$edits[$i][0])
    }
    return $value -ceq $original
}
if(!(Valid $text)){throw 'Projection inverse differs'}
foreach($bad in @(($text+"`n"),($text+$text),$text.Replace('8.0.33','8.0.32'))){if($bad -ceq $text -or (Valid $bad)){throw 'Projection negative accepted'}}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Projected syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $text){throw 'Projection readback differs'}
& $target
$proof=Join-Path $root 'continuity-evidence/initial-font-script-preparation-v1/projection.json'
if($RelativePair){$proof=Join-Path $root 'continuity-evidence/relative-font-script-preparation-v1/projection.json'}
@{sourceSha256=(Get-FileHash $source).Hash;targetSha256=(Get-FileHash $target).Hash;inverseVerified=$true;negativeControls=3;syntaxVerified=$true;runtimeExecuted=$false;limitsChanged=$false;replacementCounts=$counts}|ConvertTo-Json -Depth 4|Set-Content -LiteralPath $proof -Encoding utf8
