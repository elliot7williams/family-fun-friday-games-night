$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-NativeSelectionFull.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintStylesNative.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-styles-native-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve native preparation'}
if((Get-FileHash $source).Hash -cne '30DD3AA7671B96FF7829D62648F187121945A95F733CB06ECB81970C4017A631'){throw 'Original differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw 'Unique anchor required'}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'native-selection-full-v1' 'print-styles-native-v1'
Replace-One 'Full retained native engine/view suite with experimental Sx view; no full scripting/host/release qualification' 'Full retained native suite with print style core A85B02C6 and Sx view; no full scripting/host/release qualification'
$before="    `$baseRoot=Join-Path `$workspace 'continuity-evidence/nested-overlap-regression-v1'"
$after=@'
    $styleRoot=Join-Path $workspace 'continuity-evidence/print-styles-v3'
    $stylePath=Join-Path $styleRoot 'result.json';Hold $stylePath '445574DF56FB88273F314D07496DF63656C7BCAB4F448AF88463F29DA0750188'
    $styleProof=Get-Content $stylePath -Raw|ConvertFrom-Json
    if(!$styleProof.passed -or !$styleProof.inputsUnchanged -or $styleProof.ownerAttention){throw 'Style core focused proof required'}
    $baseRoot=Join-Path $workspace 'continuity-evidence/nested-overlap-regression-v1'
'@
Replace-One $before $after.TrimEnd("`r","`n")
$before="        if(`$name -ceq 'Lea.Win.dll'){continue}"
$after=@'
        if($name -ceq 'Lea.Win.dll'){continue}
        if($name -ceq 'Lea.Engine.dll'){
            CopyPinned (Join-Path $styleRoot 'Lea.Engine.dll') (Join-Path $runtime $name) 'A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
            continue
        }
'@
Replace-One $before $after.TrimEnd("`r","`n")
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;syntaxVerified=$true;source=(Get-FileHash $source);generated=(Get-FileHash $target);edits=$edits}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json')
Get-FileHash $target
