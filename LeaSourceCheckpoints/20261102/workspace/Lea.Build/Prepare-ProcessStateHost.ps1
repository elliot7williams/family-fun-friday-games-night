$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-NormalLazyHost.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '4D28604229726221DA8975D07FC7104CB1A6990F1593BEA5405104F03B4E0203'){throw 'Base changed'}
$target=Join-Path $PSScriptRoot 'Test-ProcessStateHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve runner'}
$text=[IO.File]::ReadAllText($base)
$replacements=@(
    @('\temporal-startup-v1\host-normal-lazy-v1','\temporal-startup-v1\host-process-state-v1'),
    @("(`$permissionRoot+'\early-limits-v1\Lea.Script.dll') (`$result.candidate+'\Lea.Script.dll') 'E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'","(`$workspace+'\continuity-evidence\process-state-parent-build-v1\parent\Lea.Script.dll') (`$result.candidate+'\Lea.Script.dll') 'D5B66F9E05D422040A49EC3EE3F16B99C9731EB036D6369B3C758EEE5BBD2916'"),
    @("(`$permissionRoot+'\early-limits-v1\symbols.pdb') (`$result.candidate+'\Lea.Script.pdb') (Get-FileHash (`$permissionRoot+'\early-limits-v1\symbols.pdb')).Hash","(`$workspace+'\continuity-evidence\process-state-parent-build-v1\parent\symbols.pdb') (`$result.candidate+'\Lea.Script.pdb') (Get-FileHash -LiteralPath (`$workspace+'\continuity-evidence\process-state-parent-build-v1\parent\symbols.pdb')).Hash"),
    @("@('clean','lea','navigation','second','split','edit','style-start','script')","@('script')"),
    @("@('clean','restored','locked','oversized','lazy-background','lazy-all')",'@()'),
    @('$result.expectedRuns=15','$result.expectedRuns=2'),
    @('$runs.Count-eq15-and','$runs.Count-eq2-and'),
    @('15-stage host compatibility subset only; engine fullsuite remains failed/open','Settings and script-host diagnostic only; no full host or release validation')
)
foreach($pair in $replacements){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Ambiguous runner substitution'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Invalid runner syntax'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
