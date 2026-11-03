$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/print-live-script-preparation-v1'
if(Test-Path $out){throw 'Preserve print script preparation'}
$generated=[Collections.Generic.List[object]]::new()
function Read-Pinned([string]$Name,[string]$Hash){
    $path=Join-Path $PSScriptRoot $Name
    if((Get-FileHash $path).Hash -cne $Hash){throw ('Original helper differs: '+$Name)}
    return [IO.File]::ReadAllText($path).Replace("`r",'')
}
function Apply([string]$Original,[object[]]$Edits){
    $text=$Original
    foreach($pair in $Edits){
        if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw ('Unique projection anchor required: '+$pair[0])}
        $text=$text.Replace($pair[0],$pair[1])
    }
    $inverse=$text
    for($i=$Edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($Edits[$i][1],$Edits[$i][0])}
    if($inverse -cne $Original){throw 'Inverse differs'}
    return $text
}
function Save([string]$Name,[string]$Text){
    $path=Join-Path $PSScriptRoot $Name
    if(Test-Path $path){throw 'Preserve generated helper'}
    $tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($Text,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Generated syntax differs'}
    [IO.File]::WriteAllText($path,$Text,[Text.UTF8Encoding]::new($false))
    $generated.Add(@{path=$path;sha256=(Get-FileHash $path).Hash;inverseVerified=$true;syntaxVerified=$true})
}
$core='A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
$view='568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8'
$oldCore='52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'
$oldView='2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
$init=Read-Pinned 'Initialize-NativeSelectionScriptSuite.generated.ps1' '1666B435576E7673C6D8F10D4229DEA3224DB049F1E0700EE077EDC4E48D839B'
$init=Apply $init @(
    @("'continuity-evidence/ordered-flow-v6/candidate/Lea.Engine.dll','$oldCore'","'continuity-evidence/print-live-capture-v1/Lea.Engine.dll','$core'"),
    @("'continuity-evidence/native-selection-view-v1/Lea.Win.dll','$oldView'","'continuity-evidence/print-live-capture-v1/Lea.Win.dll','$view'")
)
Save 'Initialize-PrintLiveScriptSuite.generated.ps1' $init
$initHash=(Get-FileHash (Join-Path $PSScriptRoot 'Initialize-PrintLiveScriptSuite.generated.ps1')).Hash
$build=Read-Pinned 'Build-NativeSelectionScriptSuite.generated.ps1' '2E5B4FF0720C39D3CEBFDF78332C5852CC10C3183E6C09CD746D094DD664716C'
$before=@'
    $nativeProofPath=Join-Path $workspace 'continuity-evidence/native-selection-full-v1/outcome-observation.json'
    Hold $nativeProofPath '47761CBADBADA54A4AEDB45E50A5E784F702380006ECED300BC7540E59BEFCBD'
    $nativeProof=Get-Content $nativeProofPath -Raw|ConvertFrom-Json
    if(!$nativeProof.nativeBehaviorPassed -or !$nativeProof.exactTerminalFooterVerified -or !$nativeProof.originalExitAndOwnedJobZero -or $nativeProof.productionPolicyBroadened -or $nativeProof.viewSha256 -cne '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'){throw 'Sx native qualification incomplete'}
'@
$after=@'
    $nativeProofPath=Join-Path $workspace 'continuity-evidence/print-live-native-v1/bound-outcome.json'
    Hold $nativeProofPath '496EF27807197530B8F83F8E7A0AAF4AFC5B2BC036866AE7C10CC9B5815D937C'
    $nativeProof=Get-Content $nativeProofPath -Raw|ConvertFrom-Json
    if(!$nativeProof.nativeFunctionalPassed -or !$nativeProof.focusedPassed -or !$nativeProof.knownPersistentGrantClassificationOnly -or $nativeProof.policyBroadened -or $nativeProof.viewSha256 -cne '568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8' -or $nativeProof.coreSha256 -cne 'A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'){throw 'Live print native qualification incomplete'}
'@
$build=Apply $build @(
    @('current-native-script-suite-v9','current-native-script-suite-v10'),
    @($before.TrimEnd("`r","`n"),$after.TrimEnd("`r","`n")),
    @('Lea.Build/Initialize-NativeSelectionScriptSuite.generated.ps1','Lea.Build/Initialize-PrintLiveScriptSuite.generated.ps1'),
    @('1666B435576E7673C6D8F10D4229DEA3224DB049F1E0700EE077EDC4E48D839B',$initHash),
    @("`$result.nativeSelectionView=`$true;`$result.productionChanged=`$true;`$result.scope='Matched60-source Sx view suite with relocated fixtures and owned compiler processes; runtime verification separate'","`$result.nativeSelectionView=`$true;`$result.printLiveView=`$true;`$result.productionChanged=`$true;`$result.scope='Matched60-source print coreA85/view568 suite with unchanged contracts and owned compiler processes; runtime verification separate'")
)
Save 'Build-PrintLiveScriptSuite.generated.ps1' $build
$verify=Read-Pinned 'Verify-NativeSelectionScriptSuite.generated.ps1' '3DC9096CC1C4EA9E5FFFC17C04F13AD28D82183FB6886E0EFD0A1A33E3F8BA86'
$verify=Apply $verify @(
    @('$SuiteVersion=9','$SuiteVersion=10'),@($oldCore,$core),@($oldView,$view),
    @("if(!`$b.nativeSelectionView){throw 'Sx view build flag missing'}","if(!`$b.nativeSelectionView -or !`$b.printLiveView){throw 'Live view build flag missing'}")
)
Save 'Verify-PrintLiveScriptSuite.generated.ps1' $verify
$run=Read-Pinned 'Test-OwnedNativeSelectionScriptSuite.generated.ps1' 'D13124E4AE32BD88EC5E34FDCE7F6BC8B76D1E8C9F9BD9C20D58A45D34A9EE45'
$oldGuard="    if(!`$build.nativeSelectionView -or (Get-FileHash (Join-Path `$build.runtime 'Lea.Win.dll')).Hash -cne '$oldView'){throw 'Wrong Sx candidate'}"
$newGuard="    if(!`$build.nativeSelectionView -or !`$build.printLiveView -or (Get-FileHash (Join-Path `$build.runtime 'Lea.Win.dll')).Hash -cne '$view' -or (Get-FileHash (Join-Path `$build.runtime 'Lea.Engine.dll')).Hash -cne '$core'){throw 'Wrong live print candidate'}"
$run=Apply $run @(
    @('native-selection-script-owned-run-v1','print-live-script-owned-run-v1'),
    @('current-native-script-suite-v9','current-native-script-suite-v10'),
    @('Exact60-source timed full script suite with Sx view2BEB under owned Run; original300s/5000ms budgets, other production components unchanged','Exact60-source timed full script suite with print coreA85/view568 under owned Run; original300s/5000ms budgets, parent/adapter/worker unchanged'),
    @($oldGuard,$newGuard)
)
Save 'Test-OwnedPrintLiveScriptSuite.generated.ps1' $run
$null=New-Item -ItemType Directory -Path $out
foreach($file in $generated){Copy-Item -LiteralPath $file.path -Destination (Join-Path $out ([IO.Path]::GetFileName($file.path)))}
@{passed=$true;inverseVerified=$true;syntaxVerified=$true;generated=@($generated);coreSha256=$core;viewSha256=$view;suiteExecuted=$false;scope='Only component/proof/helper identities and fresh suite10 paths changed; all60 sources/contracts/isolation/deadlines retained'}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json')
$generated|ConvertTo-Json
