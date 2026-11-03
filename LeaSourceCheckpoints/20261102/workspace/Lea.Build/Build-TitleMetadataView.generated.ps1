function Build-DecoderQueueTraceView {
    param([string]$Workspace,[string]$OutputRoot,[string]$Runtime)
    $base=Join-Path $Workspace 'continuity-evidence/archived-parent-view-build-v3'
    $proof=Join-Path $base 'result.json';Hold $proof '9D29961B492A52A457A32E75AB11BC8EAB2A955A3096480F417DA0484EC54630'
    $b=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$b.passed -or !$b.inputsUnchanged -or $b.ownerAttention){throw 'Archived view build incomplete'}
    $known=@{};foreach($file in $b.inputs){$known[$file.path]=$file.sha256}
    $changes=@(
        @('TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);','TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);',1),
        @('TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width','TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding).Width',4),
        @('TextRenderer.DrawText(graphics, text, font, rect, color, flags);','TextRenderer.DrawText(graphics, text, font, rect, color, flags | TextFormatFlags.NoPrefix);',1)
    )
    $helper=Join-Path $PSScriptRoot 'New-DecoderOwnerSource.ps1';Hold $helper (Get-FileHash -LiteralPath $helper).Hash
    . $helper
    $fragmentPath=Join-Path $PSScriptRoot 'DecoderLaunch.fragment.cs';Hold $fragmentPath (Get-FileHash -LiteralPath $fragmentPath).Hash
    $fragment=[IO.File]::ReadAllText($fragmentPath)
    $traceApplied=0
    $launchHelper=Join-Path $PSScriptRoot 'New-DecoderLaunchV2Source.ps1';Hold $launchHelper (Get-FileHash -LiteralPath $launchHelper).Hash
    . $launchHelper
    $launchApplied=0
    $titleHelper=Join-Path $PSScriptRoot 'Remove-DocumentTitleSource.ps1';Hold $titleHelper (Get-FileHash -LiteralPath $titleHelper).Hash
    . $titleHelper
    $titleApplied=0
    $options=[Collections.Generic.List[string]]::new();$sources=0;$changed=0
    $binary=Join-Path $Runtime 'Lea.Win.dll'
    foreach($line in Get-Content -LiteralPath (Join-Path $base 'view/compile.rsp')){
        if($line.StartsWith('/out:')){$line='/out:"'+$binary+'"'}
        elseif($line -match '^"(.+\.cs)"$'){
            $from=$Matches[1]
            if(!$known.ContainsKey($from)){throw 'Unpinned view source'};Hold $from $known[$from]
            $to=Join-Path $OutputRoot ('view-source/'+[IO.Path]::GetFileName($from));$null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
            if([IO.Path]::GetFileName($from) -ceq 'V.cs'){
                $original=[IO.File]::ReadAllText($from);$updated=$original
                foreach($edit in $changes){
                    if([regex]::Matches($updated,[regex]::Escape($edit[0])).Count -ne $edit[2] -or $updated.Contains($edit[1])){throw 'Literal view overlay differs'}
                    $updated=$updated.Replace($edit[0],$edit[1]);$changed+=$edit[2]
                }
                $inverse=$updated;for($i=$changes.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($changes[$i][1],$changes[$i][0])}
                if($inverse -cne $original){throw 'Literal view inverse differs'}
                $updated=Remove-DocumentTitleSource $updated;$titleApplied++
                [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false))
            }elseif([IO.Path]::GetFileName($from) -ceq 'Di.cs'){
                $updated=Add-DecoderLaunchStages (New-DecoderQueueTraceSource ([IO.File]::ReadAllText($from)) $fragment) 'Di'
                [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false));$traceApplied++
            }elseif([IO.Path]::GetFileName($from) -ceq 'Rp.cs'){
                $updated=Add-DecoderLaunchStages ([IO.File]::ReadAllText($from)) 'Rp'
                [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false));$launchApplied++
            }else{Copy-Item -LiteralPath $from -Destination $to}
            Hold $to (Get-FileHash -LiteralPath $to).Hash;$line='"'+$to+'"';$sources++
        }elseif($line -match '^/reference:"([^"]+)"$'){
            $path=$Matches[1];if(!$known.ContainsKey($path)){throw 'Unpinned view reference'};Hold $path $known[$path]
            if([IO.Path]::GetFileName($path) -ceq 'Lea.Engine.dll'){$path=Join-Path $Runtime 'Lea.Engine.dll';Hold $path '52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'}
            $line='/reference:"'+$path+'"'
        }
        $options.Add($line)
    }
    if($sources -ne 6 -or $changed -ne 6 -or $traceApplied -ne 1 -or $launchApplied -ne 1 -or $titleApplied -ne 1){throw 'View source inventory differs'}
    $rsp=Join-Path $OutputRoot 'view.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $hash
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $compile=Run 'compile-literal-view' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Literal view compilation failed'}
    return @{binary=$binary;sha256=(Get-FileHash -LiteralPath $binary).Hash;sources=6;edits=6;overlayInverseVerified=$true;queueTraceInverseVerified=$true;titleOverlayInverseVerified=$true}
}
