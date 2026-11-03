$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-CurrentTemporalSuiteV2.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '461212C653A76BFEC59B2B610C4AA26222D8F3CAA894652E287335DC23286E11'){throw 'Retain original V2 runner'}
$target=Join-Path $PSScriptRoot 'Test-CurrentTemporalSuiteFooterStrict.generated.ps1'
if(Test-Path $target){throw 'Preserve strict runner'}
$text=[IO.File]::ReadAllText($base).Replace("`r`n","`n").Replace("'current-lazy-run-v2'","'current-lazy-run-v3'")
$old="    `$footer=@(Get-Content -LiteralPath `$run.stdout|Where-Object {`$_.StartsWith('PASS Lea.Script: ')})`n    `$result.behaviorPassed=`$run.passed -and `$footer.Count -eq 1"
$new=@'
    $completion=Read-LeaSuiteFooter -Lines @(Get-Content -LiteralPath $run.stdout)
    $result.checks=$completion.Checks
    $result.behaviorPassed=$run.passed
'@
if(!$text.Contains($old)){throw 'Footer block absent'}
$text=$text.Replace($old,$new)
$anchor='    $result.candidate=$build.runtime'
$insert=@'
    $footerHelper=Join-Path $workspace 'Lea.Build/Read-LeaSuiteFooter.ps1'
    $null=Pin $footerHelper 'CE0005069067DDA4810317C0B9BFAF83339CD0DC929713458F76336FAC82D3B2'
    . $footerHelper
    # A previous decoder grant must not silently become the private-file baseline.
    $imageAcl=Get-Acl -LiteralPath (Join-Path $build.runtime 'Lea.Image.exe')
    $decoderSid=[Security.Principal.SecurityIdentifier]::new('S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848')
    if(@($imageAcl.GetAccessRules($true,$true,[Security.Principal.SecurityIdentifier])|Where-Object {$_.IdentityReference -eq $decoderSid}).Count){throw 'Existing decoder grant: require separately prepared private candidate; preserve previous ACL evidence'}
'@
if(!$text.Contains($anchor)){throw 'Helper load anchor absent'}
$text=$text.Replace($anchor,$insert+"`n"+$anchor)
$t=$null;$e=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$t,[ref]$e);if($e.Count){throw 'Strict runner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
[pscustomobject]@{sha256=(Get-FileHash -LiteralPath $target).Hash;syntaxValidated=$true;deadlineChanged=$false;privateBaselineGuardAdded=$true;executed=$false}|ConvertTo-Json
