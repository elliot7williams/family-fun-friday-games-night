$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $workspace 'continuity-evidence/print-style-core-source-v2/E.cs'
$coreOut=Join-Path $workspace 'continuity-evidence/print-style-core-source-v3'
$runner=Join-Path $PSScriptRoot 'Test-PrintStylesV2.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintStylesV3.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-styles-preparation-v3'
if((Test-Path $coreOut) -or (Test-Path $out) -or (Test-Path $target)){throw 'Preserve V3 evidence'}
if((Get-FileHash $source).Hash -cne '859BC6D6F3922903C4AC4CEE33144BBAED9F27625951E1DFC498BD54DB451D77' -or (Get-FileHash $runner).Hash -cne '9C8E71C94914848201B90125820B8869EC1E0A1A8A5946BC60EABC77C39C24E2'){throw 'Original differs'}
$original=[IO.File]::ReadAllText($source);$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw 'Unique source anchor required'}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
$fragment=Join-Path $PSScriptRoot 'PrintFontSize.fragment.cs'
Replace-One '        private static void Decls(S style, S inherited, Pd[] declarations, bool important)' (([IO.File]::ReadAllText($fragment).Replace("`r",''))+"`n"+'        private void Decls(S style, S inherited, Pd[] declarations, bool important)')
$before='                    if (size.Success && int.TryParse(size.Groups["n"].Value, out number) && number > 0 && number < 200) style.Fs = number;'
Replace-One $before ($before+"`n                    else ViewportFontSize(style, value);")
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Core inverse differs'}
$null=New-Item -ItemType Directory -Path $coreOut
[IO.File]::WriteAllText((Join-Path $coreOut 'E.cs'),$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;source=(Get-FileHash $source);fragment=(Get-FileHash $fragment);candidate=(Get-FileHash (Join-Path $coreOut 'E.cs'));edits=$edits}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $coreOut 'result.json')
$original=[IO.File]::ReadAllText($runner)
if([regex]::Matches($original,'print-styles-v2').Count -ne 1){throw 'Runner anchor differs'}
$text=$original.Replace('print-styles-v2','print-styles-v3')
if($text.Replace('print-styles-v3','print-styles-v2') -cne $original){throw 'Runner inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Runner syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;syntaxVerified=$true;original=(Get-FileHash $runner);generated=(Get-FileHash $target)}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json')
Get-FileHash (Join-Path $coreOut 'E.cs'),$target
