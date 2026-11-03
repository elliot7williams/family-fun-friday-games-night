$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$target=Join-Path $PSScriptRoot 'Test-OrderedFlowVisuals.v2.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve ordered flow visuals'}
$text=[IO.File]::ReadAllText((Join-Path $workspace 'continuity-evidence/named-reference-visuals-v4/runner.ps1'))
$text=$text.Replace('named-reference-visuals-v4','ordered-flow-visuals-v2').Replace('9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17','D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6')
$anchor='        $from=Join-Path $b.candidate $pair[0];Hold $from $pair[1]'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Ordered visual core anchor differs'}
$text=$text.Replace($anchor,'        $from=Join-Path $workspace ''continuity-evidence/ordered-flow-v4/candidate/Lea.Engine.dll'';Hold $from $pair[1]')
$start=$text.IndexOf("    `$builder=Join-Path `$PSScriptRoot 'Build-LiteralTextView.ps1'")
$end=$text.IndexOf('    $options=[Collections.Generic.List[string]]::new()',$start)
if($start -lt 0 -or $end -le $start){throw 'Ordered visual view span differs'}
$copy=@'
    $view=Join-Path $workspace 'continuity-evidence/named-reference-visuals-v4/runtime/Lea.Win.dll'
    Hold $view '9D9C7A3937B59206514D5631164A442744D9F5728CC43952AD643A86A5EB4867'
    $to=Join-Path $result.candidate 'Lea.Win.dll';Copy-Item -LiteralPath $view -Destination $to
    Hold $to '9D9C7A3937B59206514D5631164A442744D9F5728CC43952AD643A86A5EB4867'

'@
$text=$text.Remove($start,$end-$start).Insert($start,$copy)
$text=$text.Replace("@('desktop','narrow','narrow-edited','desktop-edited')","@('desktop-top','narrow-top','desktop','narrow','narrow-edited','desktop-edited')").Replace('Named visuals: checks=35; failures=0','Named visuals: checks=45; failures=0')
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
