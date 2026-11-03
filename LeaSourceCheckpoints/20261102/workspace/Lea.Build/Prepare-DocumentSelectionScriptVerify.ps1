param([ValidateRange(1,9)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root ('continuity-evidence/document-script-verify-preparation-v'+$Version)
$target=Join-Path $PSScriptRoot ('Verify-DocumentSelectionScriptSuite-v'+$Version+'.generated.ps1')
if((Test-Path $out) -or (Test-Path $target)){throw 'Preserve script verification preparation'}
$source=Join-Path $PSScriptRoot 'Verify-Runtime8033Suite.generated.ps1'
$sourceHash='D0EBB6D624A9E0FCF0D361D7584D8EA527FC3066E0F333FCFD8CC3D66E048FC4'
if((Get-FileHash $source).Hash -cne $sourceHash){throw 'Original script verifier differs'}
$original=[IO.File]::ReadAllText($source)
$anchor='$b=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json'
$gate='if($b.currentFullNativeQualified -ne $false -or !$b.legacyUiAUnresolved){throw ''Current native gate must remain explicitly unresolved''}'
$write='[IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 6),[Text.UTF8Encoding]::new($false))'
$edits=@(
    @('$suite=Join-Path $root (''current-native-script-suite-v''+$SuiteVersion)',('$suite=Join-Path $root ''document-selection-script-suite-v'+$Version+'''')),
    @('568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8','926C1A28093A7D7AB31BFA10A3252297ED76549BAB38D0045BD3A4DB4DABD855'),
    @($anchor,($anchor+[Environment]::NewLine+$gate)),
    @($write,('$result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true'+[Environment]::NewLine+$write))
)
$candidate=$original
foreach($edit in $edits){
    if([regex]::Matches($candidate,[regex]::Escape($edit[0])).Count -ne 1){throw 'Unique verifier projection anchor required'}
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
if(!(Valid $candidate)){throw 'Verifier inverse differs'}
$negative=0
foreach($bad in @($candidate.Replace('$b.files.Count -ne 65','$b.files.Count -ne 64'),$candidate.Replace('8.0.33','8.0.32'),($candidate+"`n"+$candidate))){
    if($bad -ceq $candidate -or (Valid $bad)){throw 'Verifier negative accepted'}
    $negative++
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($candidate,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Projected verifier syntax failed'}
$null=New-Item -ItemType Directory -Path $out
Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
Copy-Item $source (Join-Path $out 'original-verifier.ps1')
[IO.File]::WriteAllText($target,$candidate,[Text.UTF8Encoding]::new($false))
Copy-Item $target (Join-Path $out 'candidate-verifier.ps1')
if([IO.File]::ReadAllText($target) -cne $candidate -or (Get-FileHash $source).Hash -cne $sourceHash){throw 'Verifier source readback differs'}
$result=@{passed=$true;sourceSha256=$sourceHash;target=$target;targetSha256=(Get-FileHash $target).Hash;inverseVerified=$true;negativeControls=$negative;syntaxVerified=$true;runtimeExecuted=$false;limitsChanged=$false;currentFullNativeQualified=$false;scope='Source-only verifier projection retains all file/config/manifest/resource/IL checks, new view pin, native gate explicitly unresolved'}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 4))
$result|ConvertTo-Json -Depth 4
