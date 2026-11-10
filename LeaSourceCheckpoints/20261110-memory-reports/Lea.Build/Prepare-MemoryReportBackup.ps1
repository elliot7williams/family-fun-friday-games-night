$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\memory-report-publisher-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve memory report publisher'}
$proof='I:\LeaBuildStorage\memory-report-host-ui-audit-20261110-v1\audit.json'
if((Get-FileHash -LiteralPath $proof).Hash -cne 'CEED3213A51E7410E3E0F723EC0C6439A0B76F03ED231D3ED68A6E2119C6AABA'){throw 'Memory host audit differs'}
$template=Join-Path $PSScriptRoot 'Publish-CurrentFirespider.ps1'
$text=[IO.File]::ReadAllText($template)
$pairs=@(
 @("`$root=Split-Path `$PSScriptRoot -Parent",("`$root='"+(Split-Path $PSScriptRoot -Parent).Replace("'","''")+"'")),
 @('LeaSourceCheckpoints/20261110-current-firespider','LeaSourceCheckpoints/20261110-memory-reports'),
 @('github-current-firespider-readback-20261110-v1','github-memory-reports-readback-20261110-v1'),
 @('git -c safe.directory=$repo -C $repo','git -c safe.directory=$repo -c maintenance.auto=false -C $repo'),
 @('Back up current-source Firespider 19 build and original 15 workflow qualification','Back up Lea memory documents and Firespider private-report regression')
)
foreach($p in $pairs){if([regex]::Matches($text,[regex]::Escape($p[0])).Count -ne 1){throw 'Publisher projection boundary differs'};$text=$text.Replace($p[0],$p[1])}
$extra=@'
foreach($name in @('Lea.Engine/MemoryDocument.fragment.cs','Lea.Win/MemoryDocument.fragment.cs','Lea.Firespider/AccessibilityReport.vb','Lea.Firespider/AccessibilityRoute.vb','Lea.Tests/AccessibilityReportMain.vb','Lea.Tests/AccessibilityHostCase.cs','Lea.Tests/MemoryDocumentMain.cs','Lea.Tests/MemoryDocumentViewMain.cs','Lea.Build/Audit-CurrentHostLegacyRoutes.ps1','Lea.Build/Test-OwnedAccessibilityReport.ps1','Lea.Build/Prepare-AccessibilityHostSource.ps1','Lea.Build/Build-AccessibilityHost.ps1','Lea.Build/Test-OwnedAccessibilityHost.ps1','Lea.Build/Audit-AccessibilityHostUi.ps1','Lea.Build/Test-OwnedMemoryDocument.ps1','Lea.Build/Audit-MemoryDocumentView.ps1','Lea.Build/Prepare-MemoryReportHost.ps1','Lea.Build/Prepare-MemoryReportHostProbe.ps1','Lea.Build/Prepare-MemoryReportBackup.ps1','LeaForge/HOST_LEGACY_ROUTES.md','LeaForge/MEMORY_DOCUMENTS.md')){Save (Join-Path $root $name) $name}
$memorySource='I:\LeaBuildStorage\memory-document-view-20261110-v1\source'
foreach($folder in @('Lea.Engine','Lea.Win')){foreach($file in Get-ChildItem -LiteralPath (Join-Path $memorySource $folder) -File){Save $file.FullName ('memory-sdk-source/'+$folder+'/'+$file.Name)}}
$memoryHost='I:\LeaBuildStorage\memory-report-host-source-20261110-v1\source'
foreach($file in $hostFiles){Save (Join-Path $memoryHost $file.path) ('memory-host-source/'+$file.path)}
foreach($spec in @(
 @{folder='I:\LeaBuildStorage\memory-document-model-20261110-v1';prefix='memory-model'},
 @{folder='I:\LeaBuildStorage\memory-document-view-audit-20261110-v1';prefix='memory-view-audit'},
 @{folder='I:\LeaBuildStorage\accessibility-host-ui-failure-20261110-v1';prefix='retained-ui-assertion-failure'},
 @{folder='I:\LeaBuildStorage\accessibility-host-ui-audit-20261110-v3';prefix='retained-private-file-reproduction'},
 @{folder='I:\LeaBuildStorage\memory-report-host-ui-audit-20261110-v1';prefix='memory-host-ui-audit'},
 @{folder='I:\LeaBuildStorage\memory-report-host-build-20261110-v1';prefix='memory-host-build'},
 @{folder='I:\LeaBuildStorage\memory-report-host-source-20261110-v1';prefix='memory-host-source'},
 @{folder='I:\LeaBuildStorage\memory-report-host-probe-20261110-v1';prefix='memory-host-probe'}
)){foreach($file in Get-ChildItem -LiteralPath $spec.folder -File|Where-Object Extension -in @('.cs','.vb','.ps1','.json','.txt','.md','.png')){Save $file.FullName ('evidence/'+$spec.prefix+'/'+$file.Name)}}
Save $PSCommandPath 'evidence/publish-memory-reports.generated.ps1'
'@
$marker='$dependencies=@($sdkPlan.files'
if([regex]::Matches($text,[regex]::Escape($marker)).Count -ne 1){throw 'Publisher insertion boundary differs'}
$text=$text.Replace($marker,$extra+[Environment]::NewLine+$marker)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Publisher syntax invalid'}
New-Item -ItemType Directory -Path $out|Out-Null
$target=Join-Path $out 'Publish-MemoryReports.ps1'
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $text){throw 'Publisher readback differs'}
@{source=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;templateSha256=(Get-FileHash -LiteralPath $template).Hash;replacements=$pairs;extra=$extra;binaryUploads=$false;completeSdkRebuilt=$false;scope='Baseline SDK/host plus separately labeled memory core/view/host source and exact evidence'}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Write-Output $target
