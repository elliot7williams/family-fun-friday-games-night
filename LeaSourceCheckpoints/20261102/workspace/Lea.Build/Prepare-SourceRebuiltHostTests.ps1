$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-RebuiltCoreHost.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '4C74B0982F4996CB840E609871AD01FFB968A65A50872A79FE8E22D72CFBBA84'){throw 'Host runner changed'}
$target=Join-Path $PSScriptRoot 'Test-SourceRebuiltHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve host run'}
$text=[IO.File]::ReadAllText($base).Replace('host-rebuilt-core-v1','host-source-rebuilt-v1')
$anchor="    CopyExact (`$permissionRoot+'\retry-view-v1\Lea.Win.dll')"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Host replacement anchor differs'}
$addition=@'
    $hostBuildPath=$workspace+'\continuity-evidence\firespider-source-rebuild-v1\result.json'
    $null=Pin $hostBuildPath '9DDF92B7273943C6CB5481A8CCC8033F09053BE2C5968DC5E53EDA14E0A5F3DA'
    $hostBuild=Get-Content -LiteralPath $hostBuildPath -Raw|ConvertFrom-Json
    if(!$hostBuild.passed -or !$hostBuild.inputsUnchanged -or $hostBuild.ownerAttention){throw 'Source host build incomplete'}
    CopyExact ($workspace+'\continuity-evidence\firespider-source-rebuild-v1\bin\Firespider Internet 19.exe') ($result.candidate+'\Firespider Internet 19.exe') '950FEB59E55837BC84F42175B91104690CBFD75F7C492169F782A47CAD72EA67'
    CopyExact ($workspace+'\continuity-evidence\firespider-source-rebuild-v1\bin\Firespider Internet 19.exe.config') ($result.candidate+'\Firespider Internet 19.exe.config') 'E91AC2CC41148E4F9F2FA181E5B17C8A4A35E07A56055A9BA6567A6EEC92BFAF'
'@
$text=$text.Replace($anchor,$addition+"`r`n"+$anchor)
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Host runner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
