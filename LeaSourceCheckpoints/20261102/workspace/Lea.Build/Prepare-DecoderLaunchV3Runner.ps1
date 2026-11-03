$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$target=Join-Path $PSScriptRoot 'Test-DecoderLaunchV3Full.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve corrected future runner'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderLaunchV2Full.generated.ps1'))
$source=Join-Path $workspace 'continuity-evidence/ordered-flow-regression-v4/Smoke.cs'
$expected=[regex]::Matches([IO.File]::ReadAllText($source),'Console\.WriteLine\("(Lea smoke passed: [^"\r\n]*)"\);')
if($expected.Count -ne 1 -or $expected[0].Groups[1].Value.Contains('\')){throw 'Simple literal footer differs'}
$anchor='([IO.File]::ReadAllText($run.stdout).Trim().StartsWith(''Lea smoke passed: bounded sticky positioning with compact k,'',[StringComparison]::Ordinal))'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Future runner footer defect anchor differs'}
$helper=Join-Path $PSScriptRoot 'Test-LeaSuiteFooter.ps1'
$binding="    `$footerHelper=Join-Path `$PSScriptRoot 'Test-LeaSuiteFooter.ps1';Hold `$footerHelper '"+(Get-FileHash -LiteralPath $helper).Hash+"';. `$footerHelper"
$text=$text.Replace($anchor,"(Test-LeaSuiteFooter `$run.stdout '"+$expected[0].Groups[1].Value.Replace("'","''")+"')")
$anchor="    `$run=Run 'full-engine-traced'"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Future footer helper anchor differs'}
$text=$text.Replace($anchor,$binding+"`r`n"+$anchor).Replace('decoder-launch-full-v2','decoder-launch-full-v3')
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Write-Output 'Prepared corrected future runner only; not executed and not a new passing suite result.'
