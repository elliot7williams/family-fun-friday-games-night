$ErrorActionPreference='Stop'
$builder=Join-Path $PSScriptRoot 'Build-DecoderQueueTraceView.generated.ps1'
$runner=Join-Path $PSScriptRoot 'Test-DecoderQueueTrace.generated.ps1'
if((Test-Path -LiteralPath $builder) -or (Test-Path -LiteralPath $runner)){throw 'Preserve decoder trace preparation'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-LiteralTextView.ps1'))
$text=$text.Replace('Build-LiteralTextView','Build-DecoderQueueTraceView').Replace('9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17','D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6')
$anchor='    $options=[Collections.Generic.List[string]]::new();$sources=0;$changed=0'
$setup=@'
    $helper=Join-Path $PSScriptRoot 'New-DecoderQueueTraceSource.ps1';Hold $helper (Get-FileHash -LiteralPath $helper).Hash
    . $helper
    $fragmentPath=Join-Path $PSScriptRoot 'DecoderQueueTrace.fragment.cs';Hold $fragmentPath (Get-FileHash -LiteralPath $fragmentPath).Hash
    $fragment=[IO.File]::ReadAllText($fragmentPath)
    $traceApplied=0
'@
if(!$text.Contains($anchor)){throw 'Trace builder setup anchor differs'}
$text=$text.Replace($anchor,$setup+"`r`n"+$anchor)
$anchor='            }else{Copy-Item -LiteralPath $from -Destination $to}'
$overlay=@'
            }elseif([IO.Path]::GetFileName($from) -ceq 'Di.cs'){
                $updated=New-DecoderQueueTraceSource ([IO.File]::ReadAllText($from)) $fragment
                [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false));$traceApplied++
            }else{Copy-Item -LiteralPath $from -Destination $to}
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Trace source anchor differs'}
$text=$text.Replace($anchor,$overlay).Replace('$sources -ne 6 -or $changed -ne 6','$sources -ne 6 -or $changed -ne 6 -or $traceApplied -ne 1').Replace('overlayInverseVerified=$true','overlayInverseVerified=$true;queueTraceInverseVerified=$true')
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-NamedVisuals.ps1'))
$text=$text.Replace('named-reference-visuals-v4','decoder-queue-trace-v1').Replace('9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17','D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6').Replace('NamedVisuals','DecoderQueueTraceProof').Replace('Build-LiteralTextView','Build-DecoderQueueTraceView.generated')
$text=$text.Replace('$result.viewBuild=Build-DecoderQueueTraceView.generated ','$result.viewBuild=Build-DecoderQueueTraceView ')
$text=$text.Replace('$from=Join-Path $b.candidate $pair[0];Hold $from $pair[1]','$from=Join-Path $workspace ''continuity-evidence/ordered-flow-v4/candidate/Lea.Engine.dll'';Hold $from $pair[1]')
$text=$text.Replace('Native WinForms view rendering and retained-field probe, not complete Firespider UI or all glyph coverage','Controlled decoder queue phase diagnostic, no native worker launch, not full suite or reliability proof')
$anchor='    $fixture=Join-Path $workspace'
$start=$text.IndexOf($anchor)
$end=$text.IndexOf('    Hold $PSCommandPath',$start)
if($start -lt 0 -or $end -le $start){throw 'Trace proof source span differs'}
$extra=@'
    $compat=Join-Path $workspace 'continuity-evidence/decoder-scheduling-v3/QueueProof.cs';Hold $compat (Get-FileHash -LiteralPath $compat).Hash
    $copy=Join-Path $outRoot 'QueueProof.cs';Copy-Item -LiteralPath $compat -Destination $copy;Hold $copy (Get-FileHash -LiteralPath $compat).Hash
    $options.Add('"'+$copy+'"');$options.Add('/main:DecoderQueueTraceProof')

'@
$text=$text.Remove($start,$end-$start).Insert($start,$extra)
$start=$text.IndexOf("    `$baseline=Join-Path `$outRoot 'baseline'")
$end=$text.IndexOf('    $result.before=',$start)
if($start -lt 0 -or $end -le $start){throw 'Trace baseline span differs'}
$text=$text.Remove($start,$end-$start)
$start=$text.IndexOf("    `$images=Join-Path `$outRoot 'images'")
$end=$text.IndexOf('}catch{$result.error=',$start)
if($start -lt 0 -or $end -le $start){throw 'Trace execution span differs'}
$run=@'
    $run=Run 'queue-trace-proof' $binary @(('"'+(Join-Path $result.candidate 'Lea.Win.dll')+'"')) 30
    $lines=@(Get-Content -LiteralPath $run.stdout)
    $result.compatibilityProofPassed=$lines -ccontains 'PASS decoder queue: 21 checks; no native worker launched.'
    $result.passed=$run.passed -and $result.compatibilityProofPassed -and $lines[-1] -ceq 'Decoder queue trace: checks=32; failures=0; no native worker launched'

'@
$text=$text.Remove($start,$end-$start).Insert($start,$run)
foreach($source in @([IO.File]::ReadAllText($builder),$text)){
    $tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($source,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw ($errors|Out-String)}
}
[IO.File]::WriteAllText($runner,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $builder,$runner
