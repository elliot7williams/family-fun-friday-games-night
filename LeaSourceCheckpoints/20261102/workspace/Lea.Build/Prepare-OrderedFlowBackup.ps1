$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Backup-OrderedFlowSource.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve ordered source backup recipe'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Backup-NamedReferenceCandidate.ps1'))
$text=$text.Replace('named-references-v1','ordered-flow-v4').Replace('D2B7756C2DE57DA73F6F32C7097CAE52F3FF0000D9F4DD4B05C6FF6D6205FF70','FD5A70395712EC00EDE31D3D36077C80E99647BAC3B2345445E9486392F3D054')
$text=$text.Replace("@('NamedReferences.cs','NumericReferences.cs'","@('OrderedFlow.cs','NamedReferences.cs','NumericReferences.cs'")
$old="@('Generate-HtmlNames.ps1','Prepare-NamedReferenceCandidate.ps1','Build-NamedReferenceCandidate.generated.ps1','Prepare-NamedReferenceRegression.ps1','Backup-NamedReferenceCandidate.ps1')"
$new="@('Generate-HtmlNames.ps1','New-OrderedFlowSource.ps1','Prepare-OrderedFlowCandidate.ps1','Build-OrderedFlowCandidate.v4.generated.ps1','Prepare-OrderedFlowRegression.ps1','Prepare-OrderedFlowVisuals.ps1','Backup-OrderedFlowSource.generated.ps1')"
if([regex]::Matches($text,[regex]::Escape($old)).Count -ne 1){throw 'Ordered backup recipe inventory differs'}
$text=$text.Replace($old,$new)
$anchor='$zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Ordered backup archive anchor differs'}
$extra='$members.Add(@{path=(Join-Path $workspace ''continuity-evidence/named-reference-visuals-v4/literal-text-source.zip'');entry=''literal-view-source.zip''})'
$text=$text.Replace($anchor,$extra+"`r`n"+$anchor)
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
