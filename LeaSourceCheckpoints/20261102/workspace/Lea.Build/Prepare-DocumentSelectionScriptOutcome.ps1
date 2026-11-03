$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/document-script-outcome-preparation-v1'
$target=Join-Path $PSScriptRoot 'Audit-DocumentSelectionScriptOutcome-v1.generated.ps1'
$source=Join-Path $PSScriptRoot 'Audit-OwnedRuntime8033Outcome.generated.ps1'
$sourceHash='691D3304F55E69F1649B24B1605C03900971CB10F0847B5978AF3FC94BF213C8'
if((Test-Path $out) -or (Test-Path $target)){throw 'Preserve existing preparation'}
if((Get-FileHash $source).Hash -cne $sourceHash){throw 'Original auditor differs'}
$original=[IO.File]::ReadAllText($source)
$edits=@(
    @('runtime-8033-script-owned-run-v1','document-selection-script-owned-run-v1'),
    @('current-native-script-suite-v11/result.json','document-selection-script-suite-v1/result.json'),
    @('current-native-script-suite-v11/binding-verification.json','document-selection-script-suite-v1/binding-verification.json'),
    @('B651EC32E7B3EA5ABE2E6616437AF3C858A178732EDC7982A60F3DDC72696DC5','3AE0FB5B8C8AC6AE96980905D7FD0D6C840F2B5CC725DEE12921D3144DE81676'),
    @('A761DBBC76F221787E4D11F2A0E7D42DFDEFA296E4D90AC74DEA19AC5A0C096F','F5CFBAEDD2789F7F7057E511B0062FFAB4A8481A352196DF36C3F28DAB8C4FB2'),
    @('A8D581FBF133F59ADF238C5B65C33F4A360CC6971399300F84698A75FDCFA9FA','02356A28411B30ADE710F4ECC4579A32CF9991DB8A30216D3FFFFEA8B348DD4C'),
    @('149183F5D044E04544831BA39C683D87D45F1E94242F5769CA8F0C36FD49B4B9','83A2D966562AC9E14F55C88D6842FBEBCB3FA42CF95CD27FCF8BBCFFE655D9F2')
)
$candidate=$original
foreach($edit in $edits){
    if([regex]::Matches($candidate,[regex]::Escape($edit[0])).Count -ne 1){throw 'Unique outcome projection anchor required'}
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
if(!(Valid $candidate)){throw 'Outcome inverse differs'}
foreach($bad in @($candidate.Replace('300000','300001'),$candidate.Replace('3412','3411'),($candidate+"`n"+$candidate))){
    if($bad -ceq $candidate -or (Valid $bad)){throw 'Outcome negative control accepted'}
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($candidate,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Outcome syntax failed'}
$null=New-Item -ItemType Directory -Path $out
Copy-Item $source (Join-Path $out 'original-auditor.ps1')
Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
[IO.File]::WriteAllText($target,$candidate,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $candidate){throw 'Outcome readback differs'}
$result=@{passed=$true;sourceSha256=$sourceHash;targetSha256=(Get-FileHash $target).Hash;inverseVerified=$true;negativeControls=3;syntaxVerified=$true;runtimeExecuted=$false;policyChanged=$false;fullReleaseQualified=$false}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
