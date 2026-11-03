$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-NormalLazyHost.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '4D28604229726221DA8975D07FC7104CB1A6990F1593BEA5405104F03B4E0203'){throw 'Host runner changed'}
$target=Join-Path $PSScriptRoot 'Test-ArchivedPairHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve runner'}
$text=[IO.File]::ReadAllText($base).Replace("`r`n","`n")
function ReplaceOnce([string]$Old,[string]$New){
    if($script:text.IndexOf($Old,[StringComparison]::Ordinal) -lt 0 -or $script:text.IndexOf($Old,[StringComparison]::Ordinal) -ne $script:text.LastIndexOf($Old,[StringComparison]::Ordinal)){throw 'Ambiguous substitution'}
    $script:text=$script:text.Replace($Old,$New)
}
ReplaceOnce '\temporal-startup-v1\host-normal-lazy-v1' '\temporal-startup-v1\host-archived-pair-v1'
ReplaceOnce '    Space;$null=Pin $PSCommandPath' @'
    Space;$null=Pin $PSCommandPath
    $comparisonPath=$workspace+'\continuity-evidence\archived-parent-view-build-v3\comparison.json'
    $null=Pin $comparisonPath '990EB454354D25272B35600BCE9D95019DDE27AC7EC24C62AEA7A83269D1F2E1'
    $comparison=Get-Content -LiteralPath $comparisonPath -Raw|ConvertFrom-Json
    if(!$comparison.passed -or $comparison.modules.Count -ne 2){throw 'Rebuilt pair comparison incomplete'}
'@
ReplaceOnce "(`$permissionRoot+'\retry-view-v1\Lea.Win.dll') (`$result.candidate+'\Lea.Win.dll') 'E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B'" "(`$workspace+'\continuity-evidence\archived-parent-view-build-v3\view\Lea.Win.dll') (`$result.candidate+'\Lea.Win.dll') 'DEE7E4932F8D708FE3E0CE2F429FC7F6296990198251EE51C7DD59DFE063BEEB'"
ReplaceOnce "(`$permissionRoot+'\early-limits-v1\Lea.Script.dll') (`$result.candidate+'\Lea.Script.dll') 'E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'" "(`$workspace+'\continuity-evidence\archived-parent-view-build-v3\parent\Lea.Script.dll') (`$result.candidate+'\Lea.Script.dll') '3AA3B8E81372738DA30A75C538A81266EED0F24E19A4EF7A8540B331A811A8AE'"
ReplaceOnce "(`$permissionRoot+'\early-limits-v1\symbols.pdb') (`$result.candidate+'\Lea.Script.pdb') (Get-FileHash (`$permissionRoot+'\early-limits-v1\symbols.pdb')).Hash" "(`$workspace+'\continuity-evidence\archived-parent-view-build-v3\parent\symbols.pdb') (`$result.candidate+'\Lea.Script.pdb') (Get-FileHash -LiteralPath (`$workspace+'\continuity-evidence\archived-parent-view-build-v3\parent\symbols.pdb')).Hash"
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Invalid runner syntax'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
