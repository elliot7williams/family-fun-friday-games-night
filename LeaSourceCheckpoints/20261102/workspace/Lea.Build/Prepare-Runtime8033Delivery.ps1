param([ValidateSet('Package','Extraction')][string]$Stage='Package')
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace ('continuity-evidence/runtime-8033-delivery-preparation-'+$Stage.ToLowerInvariant()+'-v1')
$source=Join-Path $PSScriptRoot $(if($Stage -eq 'Package'){'Package-NativeSelectionDevelopment.generated.ps1'}else{'Test-NativeSelectionExtraction.generated.ps1'})
$target=Join-Path $PSScriptRoot $(if($Stage -eq 'Package'){'Package-Runtime8033Development.generated.ps1'}else{'Test-Runtime8033Extraction.generated.ps1'})
$pin=if($Stage -eq 'Package'){'E3438CFD9F0F85DE4C61B7ADCA6026ECFCDF64AF345C5D02F5DDDB1837A6D9B8'}else{'FCDC9FBE04C97F26FD1B1894F2A3B3206F803740F1CE73926B05EF050296AFE7'}
if((Test-Path -LiteralPath $out) -or (Test-Path -LiteralPath $target)){throw 'Preserve delivery preparation'}
if((Get-FileHash -LiteralPath $source).Hash -cne $pin){throw 'Original delivery helper changed'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-Exact([string]$Before,[string]$After,[int]$Count=1){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne $Count){throw ('Anchor count differs: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-Exact 'host-native-selection-owned-v1' 'host-runtime8033-owned-v1'
Replace-Exact 'C11B0A7FB0169D45A4611B22BECEFB497919F09149AB04905324F8C506BDE324' 'A1B1A96C9358525A7704BE283B434AD946D4906C47025E1E01D2C768BD0A12CA' $(if($Stage -eq 'Package'){1}else{2})
Replace-Exact 'Firespider-Lea-native-selection-development-20261027.zip' 'Firespider-Lea-runtime8033-development-20261027.zip'
if($Stage -eq 'Package'){
    Replace-Exact 'native-selection-development-package-v1' 'runtime-8033-development-package-v1'
    Replace-Exact "@{name='native-selection-source-20261027.zip';sha256='3B3A74685DC11937AA63BED297D5E2843BF5DE7511436CB9CCC645BAE082FA99'}" "@{name='runtime-8033-matched-source-20261027.zip';sha256='84179CBDF6C3674E43C886FCF72B5C048056425B2D029E2CE3DBBE4CD719AEB9'}"
    Replace-Exact "'Lea.Win.dll'='2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'" "'Lea.Win.dll'='568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8'`n        'Lea.Engine.dll'='A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'`n        'ScriptWorker/Lea.Script.Worker.runtimeconfig.json'='6346264D0ED421CB89893F36E6760048A72CCCEDC5A4B8BDB26B6048D6F766F6'"
    Replace-Exact '@{developmentOnly=$true;hostReceiptSha256=' "@{developmentOnly=`$true;runtimeVersion='8.0.33';hostReceiptSha256="
    $tokens=$null;$errors=$null
    $ast=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
    $notes=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.AssignmentStatementAst] -and $n.Left.Extent.Text -ceq '$note'},$true))
    if($errors.Count -or $notes.Count -ne 1){throw 'Unique development note assignment required'}
    $note=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Runtime8033DevelopmentNote.txt')).Replace("`r",'').TrimEnd("`n")
    if($note.Contains("`n'@")){throw 'Invalid development note terminator'}
    Replace-Exact $notes[0].Extent.Text ("`$note=@'`n"+$note+"`n'@")
}else{
    Replace-Exact 'native-selection-package-extraction-v1' 'runtime-8033-package-extraction-v1'
    $package=Get-Content (Join-Path $workspace 'continuity-evidence/runtime-8033-development-package-v1/result.json') -Raw|ConvertFrom-Json
    if(!$package.verified -or $package.hostReceiptSha256 -cne 'A1B1A96C9358525A7704BE283B434AD946D4906C47025E1E01D2C768BD0A12CA' -or (Get-FileHash -LiteralPath $package.archive).Hash -cne $package.sha256){throw 'New verified package required'}
    Replace-Exact '0C8A9663F5E0211E8D18840A548510819457A3E3092114A0281D32B6F0903A6A' $package.sha256
    Replace-Exact "    if(`$manifest.hostReceiptSha256 -cne" "    if(`$manifest.runtimeVersion -cne '8.0.33'){throw 'Wrong runtime package'}`n    if(`$manifest.hostReceiptSha256 -cne"
}
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Delivery source inverse differs'}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Delivery source syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
foreach($path in @($PSCommandPath,$target)){Copy-Item -LiteralPath $path -Destination (Join-Path $out ([IO.Path]::GetFileName($path)))}
@{passed=$true;stage=$Stage;inverseVerified=$true;syntaxVerified=$true;sourceSha256=$pin;targetSha256=(Get-FileHash -LiteralPath $target).Hash;runtimeExecuted=$false;scope='Versioned development package/extraction projection; original verification, startup cases and ownership limits retained'}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $out 'result.json')
Get-Content -LiteralPath (Join-Path $out 'result.json')
