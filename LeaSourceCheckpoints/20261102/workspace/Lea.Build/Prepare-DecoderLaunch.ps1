$ErrorActionPreference='Stop'
$targets=@('DecoderLaunch.fragment.cs','Build-DecoderLaunchView.generated.ps1','Test-DecoderLaunch.generated.ps1')
foreach($name in $targets){if(Test-Path -LiteralPath (Join-Path $PSScriptRoot $name)){throw 'Preserve launch diagnostic preparation'}}
$fragment=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'DecoderOwner.fragment.cs'))
$anchor='            internal string Phase = "entry";'
if(!$fragment.Contains($anchor)){throw 'Owner trace field anchor differs'}
$fragment=$fragment.Replace($anchor,$anchor+"`r`n"+'            internal string Trace = "";')
$anchor='            if (owner != null) Interlocked.Exchange(ref owner.Phase, phase);'
$replace=@'
            if (owner != null)
            {
                Interlocked.Exchange(ref owner.Phase, phase);
                var entry = phase + "=" + owner.Clock.ElapsedMilliseconds + ";";
                string previous;
                do
                {
                    previous = Interlocked.CompareExchange(ref owner.Trace, null, null);
                    if (previous.Length + entry.Length > 180) break;
                } while (Interlocked.CompareExchange(ref owner.Trace, previous + entry, previous) != previous);
            }
'@
if(!$fragment.Contains($anchor)){throw 'Owner trace mark anchor differs'}
$fragment=$fragment.Replace($anchor,$replace)
$fragment=$fragment.Replace('":cancel=" + (owner.Token.IsCancellationRequested', '":s=" + Interlocked.CompareExchange(ref owner.Trace, null, null) + ":cancel=" + (owner.Token.IsCancellationRequested')
$anchor='                var message = (text.Length > 512 - detail.Length'
if(!$fragment.Contains($anchor)){throw 'Launch report bound anchor differs'}
$fragment=$fragment.Replace($anchor,"                if (detail.Length > 448) detail = detail.Substring(0, 448);`r`n"+$anchor)
[IO.File]::WriteAllText((Join-Path $PSScriptRoot $targets[0]),$fragment,[Text.UTF8Encoding]::new($false))
$builder=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-DecoderOwnerView.generated.ps1'))
$builder=$builder.Replace('DecoderOwner.fragment.cs','DecoderLaunch.fragment.cs').Replace('D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6','52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC')
$anchor='    $traceApplied=0'
$extra=@'
    $launchHelper=Join-Path $PSScriptRoot 'New-DecoderLaunchSource.ps1';Hold $launchHelper (Get-FileHash -LiteralPath $launchHelper).Hash
    . $launchHelper
    $launchApplied=0
'@
$builder=$builder.Replace($anchor,$anchor+"`r`n"+$extra)
$builder=$builder.Replace('$updated=New-DecoderQueueTraceSource ([IO.File]::ReadAllText($from)) $fragment','$updated=Add-DecoderLaunchStages (New-DecoderQueueTraceSource ([IO.File]::ReadAllText($from)) $fragment) ''Di''')
$anchor='            }else{Copy-Item -LiteralPath $from -Destination $to}'
$extra=@'
            }elseif([IO.Path]::GetFileName($from) -ceq 'Rp.cs'){
                $updated=Add-DecoderLaunchStages ([IO.File]::ReadAllText($from)) 'Rp'
                [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false));$launchApplied++
            }else{Copy-Item -LiteralPath $from -Destination $to}
'@
if([regex]::Matches($builder,[regex]::Escape($anchor)).Count -ne 1){throw 'Launch builder Rp anchor differs'}
$builder=$builder.Replace($anchor,$extra).Replace('$traceApplied -ne 1','$traceApplied -ne 1 -or $launchApplied -ne 1')
[IO.File]::WriteAllText((Join-Path $PSScriptRoot $targets[1]),$builder,[Text.UTF8Encoding]::new($false))
$runner=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderOwnerProof.generated.ps1'))
$runner=$runner.Replace('decoder-owner-proof-v1','decoder-launch-v1').Replace('Build-DecoderOwnerView.generated','Build-DecoderLaunchView.generated').Replace('continuity-evidence/ordered-flow-v4/candidate/Lea.Engine.dll','continuity-evidence/ordered-flow-v6/candidate/Lea.Engine.dll').Replace('D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6','52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC')
foreach($text in @($builder,$runner)){
    $tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw ($errors|Out-String)}
}
[IO.File]::WriteAllText((Join-Path $PSScriptRoot $targets[2]),$runner,[Text.UTF8Encoding]::new($false))
