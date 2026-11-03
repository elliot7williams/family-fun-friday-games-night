param([ValidateRange(1,9)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root ('continuity-evidence/document-script-run-preparation-v'+$Version)
$target=Join-Path $PSScriptRoot ('Test-OwnedDocumentSelectionScriptSuite-v'+$Version+'.generated.ps1')
if((Test-Path $out) -or (Test-Path $target)){throw 'Preserve owned script preparation'}
$source=Join-Path $PSScriptRoot 'Test-OwnedRuntime8033Suite.generated.ps1'
$sourceHash='6059390CE9C258FA3B380AC37663501EDA246C36C4503FB53C2D7DE81355BC44'
if((Get-FileHash $source).Hash -cne $sourceHash){throw 'Original owned script runner differs'}
$original=[IO.File]::ReadAllText($source)
$anchor='    $build=Get-Content $buildPath -Raw|ConvertFrom-Json;$binding=Get-Content $bindingPath -Raw|ConvertFrom-Json'
$gate='    if($build.currentFullNativeQualified -ne $false -or !$build.legacyUiAUnresolved -or $binding.currentFullNativeQualified -ne $false){throw ''Native legacy qualification must remain unresolved in this candidate''}'+[Environment]::NewLine+'    $result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true;$result.hostAdopted=$false'
$edits=@(
    @("'runtime-8033-script-owned-run-v1'",("'document-selection-script-owned-run-v"+$Version+"'")),
    @("'current-native-script-suite-v11'",("'document-selection-script-suite-v"+$Version+"'")),
    @('568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8','926C1A28093A7D7AB31BFA10A3252297ED76549BAB38D0045BD3A4DB4DABD855'),
    @('Exact60-source timed full script suite coreA85/view568 with exact8.0.33 configs under owned Run; original300s/5000ms budgets and worker identities retained','Experimental60-source full script suite coreA85/view926 exact8.0.33 under owned Run; native legacy UIA gate unresolved,original300s/5000ms budgets and identities retained'),
    @($anchor,($anchor+[Environment]::NewLine+$gate))
)
$candidate=$original
foreach($edit in $edits){
    if([regex]::Matches($candidate,[regex]::Escape($edit[0])).Count -ne 1){throw 'Unique owned-run projection anchor required'}
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
if(!(Valid $candidate)){throw 'Owned runner inverse differs'}
$negative=0
foreach($bad in @($candidate.Replace('-Seconds 300','-Seconds 301'),$candidate.Replace('600MB','599MB'),($candidate+"`n"+$candidate))){
    if($bad -ceq $candidate -or (Valid $bad)){throw 'Owned runner negative control accepted'}
    $negative++
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($candidate,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Owned runner syntax failed'}
$null=New-Item -ItemType Directory -Path $out
Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
Copy-Item $source (Join-Path $out 'original-owned-runner.ps1')
[IO.File]::WriteAllText($target,$candidate,[Text.UTF8Encoding]::new($false))
Copy-Item $target (Join-Path $out 'candidate-owned-runner.ps1')
if([IO.File]::ReadAllText($target) -cne $candidate -or (Get-FileHash $source).Hash -cne $sourceHash){throw 'Owned source readback differs'}
$result=@{passed=$true;sourceSha256=$sourceHash;target=$target;targetSha256=(Get-FileHash $target).Hash;inverseVerified=$true;negativeControls=$negative;syntaxVerified=$true;runtimeExecuted=$false;limitsChanged=$false;currentFullNativeQualified=$false;scope='Fresh owned script runtime runner; all deadlines, isolation, ACL/byte/footer/trace checks preserved, native legacy gate explicit'}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 4))
$result|ConvertTo-Json -Depth 4
