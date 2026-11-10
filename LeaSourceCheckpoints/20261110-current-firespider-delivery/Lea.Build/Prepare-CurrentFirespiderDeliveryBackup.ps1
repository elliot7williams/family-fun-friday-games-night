$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\current-firespider-delivery-publisher-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve publisher preparation'}
$template=Join-Path $PSScriptRoot 'Publish-CurrentFirespider.ps1'
$text=[IO.File]::ReadAllText($template)
$pairs=@(
 @("`$root=Split-Path `$PSScriptRoot -Parent",("`$root='"+(Split-Path $PSScriptRoot -Parent).Replace("'","''")+"'")),
 @('LeaSourceCheckpoints/20261110-current-firespider','LeaSourceCheckpoints/20261110-current-firespider-delivery'),
 @('github-current-firespider-readback-20261110-v1','github-current-firespider-delivery-readback-20261110-v1'),
 @('git -c safe.directory=$repo -C $repo','git -c safe.directory=$repo -c maintenance.auto=false -C $repo'),
 @('Back up current-source Firespider 19 build and original 15 workflow qualification','Back up current-source Firespider print and verified ZIP extraction evidence')
)
foreach($p in $pairs){if([regex]::Matches($text,[regex]::Escape($p[0])).Count -ne 1){throw 'Publisher projection boundary differs'};$text=$text.Replace($p[0],$p[1])}
$extra=@'
foreach($name in @('Prepare-CurrentFirespiderPrint.ps1','Audit-CurrentFirespiderPrint.ps1','Package-CurrentFirespider.ps1','Prepare-CurrentFirespiderExtraction.ps1','Audit-CurrentFirespiderDelivery.ps1','Prepare-CurrentFirespiderDeliveryBackup.ps1','Test-PrivateBindingHostPrint-v1.generated.ps1','Test-PrivateBindingExtraction-v1.generated.ps1')){Save (Join-Path $root ('Lea.Build/'+$name)) ('Lea.Build/'+$name)}
foreach($spec in @(
 @{folder='I:\LeaBuildStorage\current-firespider-print-audit-20261110-v1';prefix='print'},
 @{folder='I:\LeaBuildStorage\current-firespider-delivery-audit-20261110-v1';prefix='delivery'},
 @{folder='I:\LeaBuildStorage\current-firespider-extraction-source-20261110-v1';prefix='extraction-source'}
)){foreach($file in Get-ChildItem -LiteralPath $spec.folder -File){Save $file.FullName ('evidence/'+$spec.prefix+'/'+$file.Name)}}
foreach($name in @('result.json','lea-development-manifest.json','DEVELOPMENT.txt')){Save (Join-Path 'I:\LeaBuildStorage\current-firespider-package-20261110-v1' $name) ('evidence/package/'+$name)}
Save (Join-Path $root 'continuity-evidence/current-source-host-print-v1/browser-preview.png') 'evidence/current-print-preview.png' 'D758D48BE01C6E57F7861A9B80DE37845F4593CBE04A2DFEEE8C9B1B35690207'
Save $PSCommandPath 'evidence/publish-delivery.generated.ps1'
'@
$marker='$dependencies=@($sdkPlan.files'
if([regex]::Matches($text,[regex]::Escape($marker)).Count -ne 1){throw 'Publisher evidence insertion differs'}
$text=$text.Replace($marker,$extra+[Environment]::NewLine+$marker)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Publisher syntax invalid'}
New-Item -ItemType Directory -Path $out|Out-Null
$target=Join-Path $out 'Publish-CurrentFirespiderDelivery.ps1'
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $text){throw 'Publisher readback differs'}
@{source=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;templateSha256=(Get-FileHash -LiteralPath $template).Hash;replacements=$pairs;extraEvidence=$extra;binaryUploads=$false;automaticMaintenance=$false}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Write-Output $target
