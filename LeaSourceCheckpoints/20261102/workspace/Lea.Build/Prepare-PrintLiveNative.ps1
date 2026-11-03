$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintStylesNative.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintLiveNative.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-live-native-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve live native preparation'}
if((Get-FileHash $source).Hash -cne '7BCA6C53F3F58786AF5CBF5A78C62AF4248C57351187C35163F2C596722DA1EE'){throw 'Original differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw 'Unique anchor required'}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-styles-native-v1' 'print-live-native-v1'
Replace-One 'Full retained native suite with print style core A85B02C6 and Sx view; no full scripting/host/release qualification' 'Full retained native suite with live print capture view568A2639 and coreA85B02C6; no full scripting/host/release qualification'
$before="    `$styleRoot=Join-Path `$workspace 'continuity-evidence/print-styles-v3'"
$after=@'
    $liveRoot=Join-Path $workspace 'continuity-evidence/print-live-capture-v1'
    $livePath=Join-Path $liveRoot 'result.json';Hold $livePath '751F1EF75A7E184DFE245D1DEFE7732EBBB4A58C5E34AD284DFB716AAE57F123'
    $liveProof=Get-Content $livePath -Raw|ConvertFrom-Json
    if(!$liveProof.passed -or !$liveProof.inputsUnchanged -or $liveProof.ownerAttention){throw 'Live view focused proof required'}
    $styleRoot=Join-Path $workspace 'continuity-evidence/print-styles-v3'
'@
Replace-One $before $after.TrimEnd("`r","`n")
Replace-One "    CopyPinned (Join-Path `$workspace 'continuity-evidence/native-selection-view-v1/Lea.Win.dll') (Join-Path `$runtime 'Lea.Win.dll') `$focused.viewSha256" "    CopyPinned (Join-Path `$liveRoot 'Lea.Win.dll') (Join-Path `$runtime 'Lea.Win.dll') '568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8'"
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;syntaxVerified=$true;source=(Get-FileHash $source);generated=(Get-FileHash $target);edits=$edits}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json')
Get-FileHash $target
