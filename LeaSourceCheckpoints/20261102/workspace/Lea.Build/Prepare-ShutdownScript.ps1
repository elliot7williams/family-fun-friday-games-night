$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/shutdown-script-preparation-v1'
if(Test-Path $out){throw 'Preserve shutdown script preparation'}
$proof=Join-Path $root 'continuity-evidence/shutdown-image-live-v1/result.json'
if((Get-FileHash $proof).Hash -cne '3890C80383BFD39523EA695757DF4B4014EBB8C99C69527480EF8996D7483636'){throw 'Shutdown pair proof differs'}
$state=Get-Content $proof -Raw|ConvertFrom-Json
if(!$state.passed -or !$state.decoderAclRestored -or !$state.inputsUnchanged -or $state.ownerAttention){throw 'Shutdown pair proof incomplete'}
$view='2225EC83D0DB503FE7778F896E7FC41475F5B50C690FE89B4FE9604B46E856CA'
if((Get-FileHash (Join-Path $root 'continuity-evidence/decoder-owner-retirement-v10/Lea.Win.dll')).Hash -cne $view){throw 'Shutdown view differs'}
$null=New-Item -ItemType Directory -Path $out
$receipts=[Collections.Generic.List[object]]::new()
function Project([string]$Source,[string]$Hash,[string]$Target,[array]$Edits){
    $sourcePath=Join-Path $root $Source;$targetPath=Join-Path $root $Target
    if((Get-FileHash $sourcePath).Hash -cne $Hash -or (Test-Path $targetPath)){throw 'Pinned original/fresh target required'}
    $original=[IO.File]::ReadAllText($sourcePath);$candidate=$original
    foreach($edit in $Edits){
        if([regex]::Matches($candidate,[regex]::Escape($edit[0])).Count -ne 1){throw ('Unique shutdown projection anchor required: '+$edit[0])}
        $candidate=$candidate.Replace($edit[0],$edit[1])
    }
    function Valid([string]$Text){
        $inverse=$Text
        for($i=$Edits.Count-1;$i -ge 0;$i--){
            if([regex]::Matches($inverse,[regex]::Escape($Edits[$i][1])).Count -ne 1){return $false}
            $inverse=$inverse.Replace($Edits[$i][1],$Edits[$i][0])
        }
        return $inverse -ceq $original
    }
    if(!(Valid $candidate)){throw 'Shutdown projection inverse differs'}
    foreach($bad in @($candidate.Replace('8.0.33','8.0.32'),($candidate+"`n"),($candidate+$candidate))){
        if($bad -ceq $candidate -or (Valid $bad)){throw 'Shutdown projection negative control accepted'}
    }
    $tokens=$null;$errors=$null
    $null=[Management.Automation.Language.Parser]::ParseInput($candidate,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Shutdown projection syntax failed'}
    [IO.File]::WriteAllText($targetPath,$candidate,[Text.UTF8Encoding]::new($false))
    if([IO.File]::ReadAllText($targetPath) -cne $candidate){throw 'Projection readback differs'}
    $receipts.Add(@{source=$Source;sourceSha256=$Hash;target=$Target;targetSha256=(Get-FileHash $targetPath).Hash;inverseVerified=$true;negativeControls=3;syntaxVerified=$true})
}
$oldView='926C1A28093A7D7AB31BFA10A3252297ED76549BAB38D0045BD3A4DB4DABD855'
Project 'continuity-evidence/document-script-view-preparation-v1/initializer.ps1' '7403776D49F8AC368E7EB5A1BC8D351F9875A032CEB48959E9C022BD025A4D3A' 'continuity-evidence/shutdown-script-preparation-v1/initializer.ps1' @(
    @('continuity-evidence/document-native-broker-v2/Lea.Win.dll','continuity-evidence/decoder-owner-retirement-v10/Lea.Win.dll'),@($oldView,$view))
$initializerHash=$receipts[0].targetSha256
Project 'Lea.Build/Build-DocumentSelectionScriptSuite-v1.generated.ps1' 'F3D4D176AAF4D525D9A4DB4E3B735638E1129A61BCBEBC432B22BF8750E2B603' 'Lea.Build/Build-ShutdownScriptSuite-v1.generated.ps1' @(
    @('document-selection-script-suite-v1','shutdown-script-suite-v1'),
    @('continuity-evidence/document-script-view-preparation-v1/initializer.ps1','continuity-evidence/shutdown-script-preparation-v1/initializer.ps1'),
    @('7403776D49F8AC368E7EB5A1BC8D351F9875A032CEB48959E9C022BD025A4D3A',$initializerHash),
    @('continuity-evidence/selection-image-live-v6/result.json','continuity-evidence/shutdown-image-live-v1/result.json'),
    @('1A901B8AB9AB389F28E89C62204D0A443E86C3DD149620BBFFC7FA2229C5D4E1','3890C80383BFD39523EA695757DF4B4014EBB8C99C69527480EF8996D7483636'),
    @('view926/coreA85','view2225/coreA85'),
    @('if(!$currentProof.passed -or !$currentProof.inputsUnchanged -or $currentProof.ownerAttention)', 'if(!$currentProof.passed -or !$currentProof.decoderAclRestored -or !$currentProof.inputsUnchanged -or $currentProof.ownerAttention)'))
Project 'Lea.Build/Verify-DocumentSelectionScriptSuite-v1.generated.ps1' 'F755107B7819830FE5E530CD7178C1FC86AAE6762EC522184203FC16B3A26B19' 'Lea.Build/Verify-ShutdownScriptSuite-v1.generated.ps1' @(
    @('document-selection-script-suite-v1','shutdown-script-suite-v1'),@($oldView,$view))
Project 'Lea.Build/Test-OwnedDocumentSelectionScriptSuite-v1.generated.ps1' '91CB6594E4446D7D0AB6AF906258C9373CFBA4E8D8817B67AC983CC8CD52C411' 'Lea.Build/Test-OwnedShutdownScriptSuite-v1.generated.ps1' @(
    @('document-selection-script-owned-run-v1','shutdown-script-owned-run-v1'),
    @('document-selection-script-suite-v1','shutdown-script-suite-v1'),@($oldView,$view),@('view926 exact8.0.33','view2225 exact8.0.33'))
Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
$result=@{passed=$true;projections=$receipts;runtimeExecuted=$false;limitsChanged=$false;currentFullNativeQualified=$false;legacyUiAUnresolved=$true}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 6))
$result|ConvertTo-Json -Depth 6
