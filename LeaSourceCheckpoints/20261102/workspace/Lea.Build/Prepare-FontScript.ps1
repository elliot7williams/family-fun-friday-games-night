$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/font-script-preparation-v1'
if(Test-Path $out){throw 'Preserve font script preparation'}
$proof=Join-Path $root 'continuity-evidence/font-shorthand-render-v2/result.json'
if((Get-FileHash $proof).Hash -cne '9A295BF4281A60B0B20245EAFDE681614972D4460A8DE0CBAF637A3EDD1AAE7A'){throw 'Font rendered pair proof differs'}
$state=Get-Content $proof -Raw|ConvertFrom-Json
if(!$state.passed -or !$state.inputsUnchanged -or $state.ownerAttention){throw 'Font proof incomplete'}
$core='0B09D9193A5819F21B055E29E8C463D32F746D7523C32FEA4E8DA25D2C30B514'
$view='A8CC9BD3693121B8931A1DE7767E8E851C9455C7B3D91452A104E9940B4AAC43'
foreach($file in @(@('Lea.Engine.dll',$core),@('Lea.Win.dll',$view))){if((Get-FileHash (Join-Path (Split-Path $proof) $file[0])).Hash -cne $file[1]){throw 'Font component differs'}}
$null=New-Item -ItemType Directory -Path $out
$receipts=[Collections.Generic.List[object]]::new()
function Project([string]$Source,[string]$Hash,[string]$Target,[array]$Edits){
    $from=Join-Path $root $Source;$to=Join-Path $root $Target
    if((Get-FileHash $from).Hash -cne $Hash -or (Test-Path $to)){throw 'Pinned source/fresh target required'}
    $original=[IO.File]::ReadAllText($from);$text=$original
    foreach($edit in $Edits){if($edit.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw ('Font script anchor ambiguous: '+$edit[0])};$text=$text.Replace($edit[0],$edit[1])}
    function Valid([string]$Value){
        for($i=$Edits.Count-1;$i -ge 0;$i--){if([regex]::Matches($Value,[regex]::Escape($Edits[$i][1])).Count -ne 1){return $false};$Value=$Value.Replace($Edits[$i][1],$Edits[$i][0])}
        return $Value -ceq $original
    }
    if(!(Valid $text)){throw 'Font script inverse differs'}
    foreach($bad in @($text.Replace('8.0.33','8.0.32'),($text+"`n"),($text+$text))){if($bad -ceq $text -or (Valid $bad)){throw 'Font script negative accepted'}}
    $tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Font script syntax failed'}
    [IO.File]::WriteAllText($to,$text,[Text.UTF8Encoding]::new($false))
    if([IO.File]::ReadAllText($to) -cne $text){throw 'Font script readback differs'}
    $receipts.Add(@{source=$Source;sourceSha256=$Hash;target=$Target;targetSha256=(Get-FileHash $to).Hash;inverseVerified=$true;negativeControls=3;syntaxVerified=$true})
}
$oldCore='A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
$oldView='2225EC83D0DB503FE7778F896E7FC41475F5B50C690FE89B4FE9604B46E856CA'
Project 'continuity-evidence/shutdown-script-preparation-v1/initializer.ps1' '7F9DC6FA31B823B233178FB9AFF67123DE329C623E5292D5D1475BE827E62640' 'continuity-evidence/font-script-preparation-v1/initializer.ps1' @(
    @('continuity-evidence/print-live-capture-v1/Lea.Engine.dll','continuity-evidence/font-shorthand-render-v2/Lea.Engine.dll'),@($oldCore,$core),
    @('continuity-evidence/decoder-owner-retirement-v10/Lea.Win.dll','continuity-evidence/font-shorthand-render-v2/Lea.Win.dll'),@($oldView,$view))
$initializerHash=$receipts[0].targetSha256
$anchor='    $result.hostAdopted=$false'
$fontProof=@'
    $fontProofPath=Join-Path $workspace 'continuity-evidence/font-shorthand-render-v2/result.json'
    Hold $fontProofPath '9A295BF4281A60B0B20245EAFDE681614972D4460A8DE0CBAF637A3EDD1AAE7A'
    $fontProof=Get-Content $fontProofPath -Raw|ConvertFrom-Json
    if(!$fontProof.passed -or !$fontProof.inputsUnchanged -or $fontProof.ownerAttention -or $fontProof.candidateCoreSha256 -cne '0B09D9193A5819F21B055E29E8C463D32F746D7523C32FEA4E8DA25D2C30B514' -or $fontProof.candidateViewSha256 -cne 'A8CC9BD3693121B8931A1DE7767E8E851C9455C7B3D91452A104E9940B4AAC43'){throw 'Exact rendered font pair proof incomplete'}
    $result.fontRenderedPair=$true;$result.currentPrintQualified=$false
'@
Project 'Lea.Build/Build-ShutdownScriptSuite-v1.generated.ps1' '55CA3F23D0438CAEB12B51401F6587AA119E96B11A42A76F5C2C6623CEE3FEDC' 'Lea.Build/Build-FontScriptSuite-v1.generated.ps1' @(
    @('shutdown-script-suite-v1','font-script-suite-v1'),
    @('continuity-evidence/shutdown-script-preparation-v1/initializer.ps1','continuity-evidence/font-script-preparation-v1/initializer.ps1'),
    @('7F9DC6FA31B823B233178FB9AFF67123DE329C623E5292D5D1475BE827E62640',$initializerHash),
    @('view2225/coreA85','viewA8CC/core0B09'),@($anchor,($anchor+"`r`n"+$fontProof)))
Project 'Lea.Build/Verify-ShutdownScriptSuite-v1.generated.ps1' 'A6F094517D735B1DFF872952050B3AAFE4FD2D87B2B8D82849995FF1E6B5ECEA' 'Lea.Build/Verify-FontScriptSuite-v1.generated.ps1' @(
    @('shutdown-script-suite-v1','font-script-suite-v1'),@($oldCore,$core),@($oldView,$view))
Project 'Lea.Build/Test-OwnedShutdownScriptSuite-v1.generated.ps1' '9F495E0CA89B2F069E5EE24C82CC3E36645F36DB73192EC3E4992314B5DD400B' 'Lea.Build/Test-OwnedFontScriptSuite-v1.generated.ps1' @(
    @('shutdown-script-owned-run-v1','font-script-owned-run-v1'),@('shutdown-script-suite-v1','font-script-suite-v1'),
    @($oldCore,$core),@($oldView,$view),@('coreA85/view2225','core0B09/viewA8CC'))
Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
$result=@{passed=$true;projections=$receipts;runtimeExecuted=$false;limitsChanged=$false;currentPrintQualified=$false;currentFullNativeQualified=$false;legacyUiAUnresolved=$true}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 6))
$result|ConvertTo-Json -Depth 6
