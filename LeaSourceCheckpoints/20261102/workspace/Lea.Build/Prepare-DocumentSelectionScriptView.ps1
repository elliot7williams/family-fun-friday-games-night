param([ValidateRange(1,9)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root ('continuity-evidence/document-script-view-preparation-v'+$Version)
if(Test-Path $out){throw 'Preserve existing preparation'}
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 32MB){throw 'Source preparation workspace required; not a build reserve waiver'}
$source=Join-Path $PSScriptRoot 'Initialize-Runtime8033Suite.generated.ps1'
$sourceHash='03451A26EEF5D393914AE80CD999DB5C5F82107B2E0D0DFC5828CE20F9AB6030'
if((Get-FileHash $source).Hash -cne $sourceHash){throw 'Original 8033 initializer differs'}
$proof=Join-Path $root 'continuity-evidence/selection-image-live-v6/result.json'
$proofHash='1A901B8AB9AB389F28E89C62204D0A443E86C3DD149620BBFFC7FA2229C5D4E1'
if((Get-FileHash $proof).Hash -cne $proofHash){throw 'Matched current view proof differs'}
$qualified=Get-Content $proof -Raw|ConvertFrom-Json
if(!$qualified.passed -or !$qualified.inputsUnchanged -or $qualified.ownerAttention){throw 'Matched current pair proof incomplete'}
$view=Join-Path $root 'continuity-evidence/document-native-broker-v2/Lea.Win.dll'
$viewHash='926C1A28093A7D7AB31BFA10A3252297ED76549BAB38D0045BD3A4DB4DABD855'
if((Get-FileHash $view).Hash -cne $viewHash){throw 'Current view bytes differ'}
$before="        'Lea.Win.dll'=@('continuity-evidence/print-live-capture-v1/Lea.Win.dll','568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8')"
$after="        'Lea.Win.dll'=@('continuity-evidence/document-native-broker-v2/Lea.Win.dll','$viewHash')"
$original=[IO.File]::ReadAllText($source)
if([regex]::Matches($original,[regex]::Escape($before)).Count -ne 1){throw 'Unique view binding required'}
$candidate=$original.Replace($before,$after)
function Valid([string]$Text){
    return [regex]::Matches($Text,[regex]::Escape($after)).Count -eq 1 -and $Text.Replace($after,$before) -ceq $original
}
if(!(Valid $candidate)){throw 'Initializer inverse differs'}
$negative=0
foreach($bad in @($candidate.Replace('600MB','599MB'),$candidate.Replace('8.0.33','8.0.32'),($candidate+"`n"+$candidate))){
    if($bad -ceq $candidate -or (Valid $bad)){throw 'Initializer negative control accepted'}
    $negative++
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($candidate,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Prepared initializer syntax differs'}
$null=New-Item -ItemType Directory -Path $out
Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
$target=Join-Path $out 'initializer.ps1'
[IO.File]::WriteAllText($target,$candidate,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $candidate -or (Get-FileHash $source).Hash -cne $sourceHash){throw 'Prepared/original source readback differs'}
$result=@{passed=$true;source=$source;sourceSha256=$sourceHash;target=$target;targetSha256=(Get-FileHash $target).Hash;view=$view;viewSha256=$viewHash;proofSha256=$proofHash;inverseVerified=$true;negativeControls=$negative;syntaxVerified=$true;runtimeExecuted=$false;limitsChanged=$false;legacyBrokerQualified=$false;scope='Only one view binding changed in fresh 8033 script initializer; full builder/verification/owned-run projection and execution still required'}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 4))
$result|ConvertTo-Json -Depth 4
