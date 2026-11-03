$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PageColorCampaign.ps1'
$target=Join-Path $PSScriptRoot 'Test-NativeSelectionHostCampaign.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/native-selection-host-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve existing preparation'}
if((Get-FileHash $source).Hash -cne 'B57DE77900A03E530DBFF40DE8F7A50622D0FE9E24C2303CF8778B85AFE83DC6'){throw 'Original campaign changed'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'')
$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After)
    $edits.Add(@($Before,$After))
}
Replace-One 'host-page-color-owned-v1' 'host-native-selection-owned-v1'
Replace-One 'Own 15-stage FBBD host campaign' 'Own 15-stage FBBD host with new native-selection view2BEB campaign'
$anchor='    $basePath=Join-Path $workspace'
$proof=@'
    foreach($proof in @(
        @('continuity-evidence/native-selection-full-v1/outcome-observation.json','47761CBADBADA54A4AEDB45E50A5E784F702380006ECED300BC7540E59BEFCBD'),
        @('continuity-evidence/temporal-startup-v1/native-selection-script-owned-run-v1/outcome-observation.json','7387BB5A4FE461712DB314D7BAF65616DD5B047DFADF92AB09467EDDAF0458B0')
    )){Hold (Join-Path $workspace $proof[0]) $proof[1]}
    $newView=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/Lea.Win.dll'
    $newViewHash='2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
    Hold $newView $newViewHash
'@
# Exact receipts bind prerequisites; no older host pass is transferred.
Replace-One $anchor ($proof+"`n"+$anchor)
$before='        Copy-Item -LiteralPath $file.path -Destination $to'
$after=@'
        if($relative -ceq 'Lea.Win.dll'){
            if($file.sha256 -cne '74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC'){throw 'Original view differs'}
            Copy-Item -LiteralPath $newView -Destination $to
        }else{Copy-Item -LiteralPath $file.path -Destination $to}
'@
Replace-One $before $after
Replace-One 'if((Get-FileHash $to).Hash -cne $file.sha256)' "if((Get-FileHash `$to).Hash -cne `$(if(`$relative -ceq 'Lea.Win.dll'){`$newViewHash}else{`$file.sha256}))"
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Inverse differs'}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
$receipt=[ordered]@{prepared=$true;executed=$false;inverseVerified=$true;syntaxVerified=$true;original=(Get-FileHash $source);generated=(Get-FileHash $target);edits=$edits;scope='Fresh actual host pairing; original fifteen cases, budgets, ownership and decoder policy unchanged; no runtime qualification yet'}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($receipt|ConvertTo-Json -Depth 8))
$receipt|ConvertTo-Json -Depth 3
