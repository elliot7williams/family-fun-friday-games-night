$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Build-RetirementFullEntry-v1.generated.ps1'
if((Get-FileHash $template).Hash -cne '9B71EBD0BE75E7C2F4509E109A57E80D39F7355C6CBE8792261CC490404686EC'){throw 'Fresh entry template differs'}
$to=Join-Path $PSScriptRoot 'Build-RetirementBoundEntry-v2.generated.ps1'
if(Test-Path $to){throw 'Preserve bound entry builder'}
$text=[IO.File]::ReadAllText($template).Replace('I:\LeaBuildStorage\retirement-full-entry-20261102-v1','I:\LeaBuildStorage\retirement-full-entry-20261102-v2')
$needle='            $source=Join-Path $out ''WorkerEntryNativeHost.cs'';'
if([regex]::Matches($text,[regex]::Escape($needle)).Count -ne 1){throw 'Unique validated host boundary required'}
$binding=@'
            $diagnosticBuild='I:\LeaBuildStorage\worker-retirement-trace-build-20261102-v2/result.json'
            Hold $diagnosticBuild '8B971B3212FEDAD1F5F01380FDC963F4F862BB683CB01B77356535774537ECEC'
            $diagnostic=Get-Content $diagnosticBuild -Raw|ConvertFrom-Json
            if(!$diagnostic.passed -or !$diagnostic.inputsUnchanged -or $diagnostic.ownerAttention -or !$diagnostic.diagnosticOnly -or $diagnostic.sha256 -cne 'E5896C436C840022CC4B840F866F7D7F5D92B5E0CA1B546B20AC19C5A1FB1111'){throw 'Diagnostic parent build incomplete'}
            Hold $diagnostic.binary $diagnostic.sha256
            $originalParent=$parentHash
            if($originalParent -cne 'D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518' -or [regex]::Matches($updatedHost,$originalParent).Count -ne 1 -or $updatedHost.Contains($diagnostic.sha256)){throw 'Unique original diagnostic parent boundary required'}
            $originalParentText=$updatedHost
            $updatedHost=$updatedHost.Replace($originalParent,$diagnostic.sha256)
            if($updatedHost.Replace($diagnostic.sha256,$originalParent) -cne $originalParentText){throw 'Diagnostic parent inverse differs'}
            $parentHash=$diagnostic.sha256
            $result.diagnosticParentInverseVerified=$true
            $result.diagnosticParentSha256=$parentHash
            $result.diagnosticOnly=$true
'@
$text=$text.Replace($needle,$binding+[Environment]::NewLine+$needle)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Bound entry builder syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
