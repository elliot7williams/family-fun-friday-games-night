$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-SourceRebuiltHost.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '96B63F2C513D1C3497375EAB2A0E295D2F8672A27AB9180CBC25B8593579F056'){throw 'Host runner changed'}
$target=Join-Path $PSScriptRoot 'Test-RebuiltAdapterHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve host run'}
$text=[IO.File]::ReadAllText($base).Replace('host-source-rebuilt-v1','host-adapter-rebuilt-v1')
$anchor="    CopyExact (`$permissionRoot+'\retry-view-v1\Lea.Win.dll')"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Replacement anchor differs'}
$addition=@'
    $adapterRoot=$workspace+'\continuity-evidence\script-adapter-source-build-v2'
    $null=Pin ($adapterRoot+'\result.json') '7178C265B7BCAF50738DC8F610432F4C6B282DAB77442D689D06437CD3BD3E35'
    $adapter=Get-Content -LiteralPath ($adapterRoot+'\result.json') -Raw|ConvertFrom-Json
    if(!$adapter.passed -or !$adapter.inputsUnchanged -or $adapter.ownerAttention){throw 'Adapter build incomplete'}
    CopyExact ($adapterRoot+'\Lea.Script.Win.dll') ($result.candidate+'\Lea.Script.Win.dll') 'AD4A057F5E82C8A36FA8362E54D1F746AD6264B9DF8D5447491DE001E34A59EA'
'@
$text=$text.Replace($anchor,$addition+"`r`n"+$anchor)
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Host runner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
