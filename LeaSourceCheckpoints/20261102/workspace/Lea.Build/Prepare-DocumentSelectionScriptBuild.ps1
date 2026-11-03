param([ValidateRange(1,9)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root ('continuity-evidence/document-script-build-preparation-v'+$Version)
$target=Join-Path $PSScriptRoot ('Build-DocumentSelectionScriptSuite-v'+$Version+'.generated.ps1')
if((Test-Path $out) -or (Test-Path $target)){throw 'Preserve previous script preparation'}
$source=Join-Path $PSScriptRoot 'Build-Runtime8033Suite.generated.ps1'
$sourceHash='C40660BB4AB0692D8A4CABDCF5E2E5B6BD8F3E709A76F1A72182D4847B3652ED'
if((Get-FileHash $source).Hash -cne $sourceHash){throw 'Original matched script builder differs'}
$prepPath=Join-Path $root 'continuity-evidence/document-script-view-preparation-v1/result.json'
if((Get-FileHash $prepPath).Hash -cne 'EEA510405BC334925732F56D8B616AC088A7A7771B2B459A1A755570F2A4B3FA'){throw 'View projection receipt differs'}
$prep=Get-Content $prepPath -Raw|ConvertFrom-Json
if(!$prep.passed -or !$prep.inverseVerified -or !$prep.syntaxVerified -or $prep.runtimeExecuted -or $prep.limitsChanged -or $prep.negativeControls -ne 3){throw 'View initializer projection incomplete'}
if((Get-FileHash $prep.target).Hash -cne $prep.targetSha256){throw 'Prepared initializer bytes differ'}
$original=[IO.File]::ReadAllText($source)
$oldScope="Matched60-source print coreA85/view568 suite with exact8.0.33 config projections and unchanged contracts; runtime compatibility verification separate"
$newScope="Compile-only60-source experimental document selection view926/coreA85, exact8.0.33 configs; full native legacy UIA gate remains open and script runtime qualification is separate"
$anchor='    Hold $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash'
$qualification=@'
    $currentProofPath=Join-Path $workspace 'continuity-evidence/selection-image-live-v6/result.json'
    Hold $currentProofPath '1A901B8AB9AB389F28E89C62204D0A443E86C3DD149620BBFFC7FA2229C5D4E1'
    $currentProof=Get-Content $currentProofPath -Raw|ConvertFrom-Json
    if(!$currentProof.passed -or !$currentProof.inputsUnchanged -or $currentProof.ownerAttention){throw 'Current matched view/print proof incomplete'}
    $result.currentFullNativeQualified=$false
    $result.legacyUiAUnresolved=$true
    $result.hostAdopted=$false
'@
$edits=@(
    @("'current-native-script-suite-v11'",("'document-selection-script-suite-v"+$Version+"'")),
    @('Lea.Build/Initialize-Runtime8033Suite.generated.ps1','continuity-evidence/document-script-view-preparation-v1/initializer.ps1'),
    @('03451A26EEF5D393914AE80CD999DB5C5F82107B2E0D0DFC5828CE20F9AB6030',$prep.targetSha256),
    @($oldScope,$newScope),
    @($anchor,($anchor+[Environment]::NewLine+$qualification))
)
$candidate=$original
foreach($edit in $edits){
    if([regex]::Matches($candidate,[regex]::Escape($edit[0])).Count -ne 1){throw ('Unique script builder anchor required: '+$edit[0])}
    $candidate=$candidate.Replace($edit[0],$edit[1])
}
function Valid([string]$Text){
    $inverse=$Text
    for($i=$edits.Count-1;$i -ge 0;$i--){
        if([regex]::Matches($inverse,[regex]::Escape($edits[$i][1])).Count -ne 1){return $false}
        $inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])
    }
    return $inverse -ceq $original
}
if(!(Valid $candidate)){throw 'Fresh script builder inverse differs'}
$negative=0
foreach($bad in @($candidate.Replace(' 120',' 121'),$candidate.Replace($prep.targetSha256,('0'*64)),($candidate+"`n"+$candidate))){
    if($bad -ceq $candidate -or (Valid $bad)){throw 'Script builder negative control accepted'}
    $negative++
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($candidate,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Generated script builder syntax failed'}
$null=New-Item -ItemType Directory -Path $out
Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
Copy-Item $source (Join-Path $out 'original-builder.ps1')
[IO.File]::WriteAllText($target,$candidate,[Text.UTF8Encoding]::new($false))
Copy-Item $target (Join-Path $out 'candidate-builder.ps1')
if([IO.File]::ReadAllText($target) -cne $candidate -or (Get-FileHash $source).Hash -cne $sourceHash){throw 'Builder readback/original preservation differs'}
$result=@{passed=$true;source=$source;sourceSha256=$sourceHash;target=$target;targetSha256=(Get-FileHash $target).Hash;initializer=$prep.target;initializerSha256=$prep.targetSha256;inverseVerified=$true;negativeControls=$negative;syntaxVerified=$true;runtimeExecuted=$false;limitsChanged=$false;baselineNativeProofRetained=$true;currentFullNativeQualified=$false;scope='Fresh compile-only script builder projection; full native legacy UIA remains unresolved, script verification and runtime tests pending'}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 4))
$result|ConvertTo-Json -Depth 4
