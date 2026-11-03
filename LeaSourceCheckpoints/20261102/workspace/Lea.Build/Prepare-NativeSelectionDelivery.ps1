param([ValidateSet('Package','Extraction')][string]$Stage='Package')
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace ('continuity-evidence/native-selection-delivery-preparation-'+$Stage.ToLowerInvariant()+'-v1')
$name=if($Stage -eq 'Package'){'Package'}else{'Test'}
$target=Join-Path $PSScriptRoot ($name+'-NativeSelection'+$(if($Stage -eq 'Package'){'Development'}else{'Extraction'})+'.generated.ps1')
if((Test-Path $out) -or (Test-Path $target)){throw 'Preserve delivery preparation'}
$source=Join-Path $PSScriptRoot $(if($Stage -eq 'Package'){'Package-PageColorDevelopment.ps1'}else{'Test-PageColorExtraction.ps1'})
$pin=if($Stage -eq 'Package'){'7B35B78DF06B16412513BE748EC8ACCE5EBAA0D7BD53A0DAE9FA0925D807A5B7'}else{'E13E0B58D5642FC9787117BDCE230081B61B881402FFAD8402B41812FDFF82F5'}
if((Get-FileHash $source).Hash -cne $pin){throw 'Original source differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-Exact([string]$Before,[string]$After,[int]$Count=1){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne $Count){throw ('Anchor count differs: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After,$Count))
}
Replace-Exact 'host-page-color-owned-v1' 'host-native-selection-owned-v1'
Replace-Exact '3829CAFC58C5DFFCE1F84CC1B4E3756757E25954E696FB9F5144663375A53449' 'C11B0A7FB0169D45A4611B22BECEFB497919F09149AB04905324F8C506BDE324' $(if($Stage -eq 'Package'){1}else{2})
Replace-Exact 'Firespider-Lea-page-color-development-20261026.zip' 'Firespider-Lea-native-selection-development-20261027.zip'
if($Stage -eq 'Package'){
    Replace-Exact 'page-color-development-package-v1' 'native-selection-development-package-v1'
    Replace-Exact "@{name='page-color-source-20261026.zip';sha256='D4815CEA7C040DA98C2BEB6C86970AAB2F58170A2ED3AC074A909B1B386210E6'}" "@{name='native-selection-source-20261027.zip';sha256='3B3A74685DC11937AA63BED297D5E2843BF5DE7511436CB9CCC645BAE082FA99'}"
    Replace-Exact "@{name='page-color-qualification-source-20261026.zip';sha256='DC5D1464FC77FC9A67A287DC230C5340F685E95B616A900D4A05E194A4C4602B'}" "@{name='native-selection-host-source-20261027.zip';sha256='D4F8FB82153418B2584C27E6D6A50C9D99499119B28DE7A64C755DACD8D0FC80'}"
    Replace-Exact "        'ScriptWorker/Lea.Script.Worker.dll'=" "        'Lea.Win.dll'='2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'`n        'ScriptWorker/Lea.Script.Worker.dll'="
    Replace-Exact 'Firespider + Lea page-color development build, 2026-10-26' 'Firespider + Lea native-selection development build, 2026-10-27'
    Replace-Exact 'checks on the unchanged engine/worker, but that does not resolve those failures.' 'checks on this view/core/worker pairing, but that does not resolve those failures.'
    Replace-Exact 'ZIP contents are verified; fresh extraction testing is recorded separately.' "Sx() reads permitted native-field selection; ordinary page-text selection and`nprinting remain unfinished. This new view also passed a matched full native run.`nZIP contents are verified; fresh extraction testing is recorded separately."
}else{
    Replace-Exact 'page-color-package-extraction-v1' 'native-selection-package-extraction-v1'
    $package=Join-Path $workspace 'continuity-evidence/native-selection-development-package-v1/result.json'
    $proof=Get-Content $package -Raw|ConvertFrom-Json
    if(!$proof.verified -or $proof.hostReceiptSha256 -cne 'C11B0A7FB0169D45A4611B22BECEFB497919F09149AB04905324F8C506BDE324'){throw 'New package proof required'}
    if((Get-FileHash $proof.archive).Hash -cne $proof.sha256){throw 'Package hash differs'}
    Replace-Exact 'BF7C6C6602FC0F65E38952873D89211AFB7138629A64E06C9B4971FF01D70DCF' $proof.sha256
}
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Projection inverse differs'}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
$receipt=@{stage=$Stage;syntaxVerified=$true;inverseVerified=$true;executed=$false;original=(Get-FileHash $source);generated=(Get-FileHash $target);edits=$edits}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($receipt|ConvertTo-Json -Depth 6))
$receipt|Select-Object stage,syntaxVerified,inverseVerified,generated|ConvertTo-Json
