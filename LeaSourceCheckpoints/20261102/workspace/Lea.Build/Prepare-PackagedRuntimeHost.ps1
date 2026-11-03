$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-RebuiltAdapterHost.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '586FE306934D59A8EF1A3F2851BC925B4F14749B03720DC301BAD5F9D455510A'){throw 'Host runner changed'}
$target=Join-Path $PSScriptRoot 'Test-PackagedRuntimeHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve packaged test'}
$text=[IO.File]::ReadAllText($base).Replace('host-adapter-rebuilt-v1','host-package-extracted-v1')
$start=$text.IndexOf('    $binaryHashes=@{};',[StringComparison]::Ordinal)
$end=$text.IndexOf('    $source=$workspace+',[StringComparison]::Ordinal)
if($start -lt 0 -or $end -le $start){throw 'Staging region differs'}
$addition=@'
    $extractor=$workspace+'\Lea.Build\Expand-VerifiedRuntime.ps1'
    $null=Pin $extractor
    . $extractor
    $package=$workspace+'\continuity-evidence\portable-runtime-v1\Lea-Firespider-development-runtime.zip'
    $null=Pin $package '286EFD9712AAFF434EE83026A60CEB4AA4F40F086A1140BB4FF19762BCAC1DAC'
    $manifest=Expand-VerifiedLeaRuntime -Archive $package -ExpectedArchiveSha256 '286EFD9712AAFF434EE83026A60CEB4AA4F40F086A1140BB4FF19762BCAC1DAC' -ExpectedManifestSha256 '016A077896A27189F718BCABA5589329427A7F0F978256F850538C29AF40AEFF' -Destination ($outRoot+'\extracted')
    $result.candidate=$outRoot+'\extracted\runtime'
    $result.packageSha256='286EFD9712AAFF434EE83026A60CEB4AA4F40F086A1140BB4FF19762BCAC1DAC'
    $result.extractedFiles=$manifest.files.Count
    foreach($file in $manifest.files){$null=Pin (Join-Path ($outRoot+'\extracted') $file.path) $file.sha256}
    $result.candidateBinaries=@(Get-ChildItem $result.candidate -Recurse -File|Where-Object Extension -In '.exe','.dll','.pdb'|ForEach-Object{@{path=$_.FullName.Substring($result.candidate.Length+1);sha256=(Get-FileHash $_.FullName).Hash}})
'@
$text=$text.Substring(0,$start)+$addition+"`r`n"+$text.Substring($end)
$text=$text.Replace('15-stage host subset with rebuilt core; matched full scripting suite and release remain unverified','15-stage host subset from verified package extraction; complete release remains unverified')
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Packaged runner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
