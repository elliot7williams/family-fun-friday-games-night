# Staged projection only; frozen source and baseline DLL stay untouched.
$baselinePath=Join-Path $root 'continuity-evidence/print-styles-v3/result.json'
Hold $baselinePath '445574DF56FB88273F314D07496DF63656C7BCAB4F448AF88463F29DA0750188'
$baseline=Get-Content $baselinePath -Raw|ConvertFrom-Json
$recipe=Join-Path $root 'continuity-evidence/print-styles-v3/compile-style-core.rsp'
Hold $recipe 'A5D0A29D5BE04505C0A51527C8482351380622C3A3B9C6A6977EECDF50CD3DBC'
$stage=Join-Path $out 'core-source';$null=New-Item -ItemType Directory -Path $stage
$options=[Collections.Generic.List[string]]::new()
foreach($line in Get-Content $recipe){
    if($line.StartsWith('/out:')){$line='/out:"'+$copy+'"'}
    elseif($line -match '^/reference:"(.+)"$'){Hold $Matches[1]}
    elseif($line -match '^"(.+\.cs)"$'){
        $from=$Matches[1];$pins=@($baseline.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Mature font source pin missing'}
        Hold $from $pins[0];$to=Join-Path $stage ([IO.Path]::GetFileName($from));$text=[IO.File]::ReadAllText($from)
        if([IO.Path]::GetFileName($from) -eq 'E.cs'){
            $original=$text
            $pairs=@(@('public sealed class E','public sealed partial class E'),@('                else if (name == "font-family") FontFamily(style, value);',('                else if (name == "font") FontShorthand(style, inherited, value);'+"`r`n"+'                else if (name == "font-family") FontFamily(style, value);')),@('                    else ViewportFontSize(style, value);',('                    else if (TryRelativeFontSize(inherited, value, out number)) style.Fs = number;'+"`r`n"+'                    else ViewportFontSize(style, value);')))
            foreach($pair in $pairs){if($pair.Count -ne 2 -or ([regex]::Matches($text,[regex]::Escape($pair[0]))).Count -ne 1){throw 'Font projection hook ambiguous'};$text=$text.Replace($pair[0],$pair[1])}
            $inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
            if($inverse -cne $original){throw 'Font projection inverse differs'}
        }
        [IO.File]::WriteAllText($to,$text,[Text.UTF8Encoding]::new($false));Hold $to;$line='"'+$to+'"'
    }
    $options.Add($line)
}
$from=Join-Path $root 'Lea.Engine/FontShorthand.cs';Hold $from
$to=Join-Path $stage 'FontShorthand.cs';Copy-Item $from $to;Hold $to;$options.Add('"'+$to+'"')
$response=Join-Path $out 'font-core.rsp';[IO.File]::WriteAllLines($response,$options,[Text.UTF8Encoding]::new($false));Hold $response
$public=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $public 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
$publicSource=Join-Path $PSScriptRoot 'PublicCompiler.cs';Hold $publicSource 'D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB'
$hostPath=(Get-Process -Id $PID).Path;Hold $hostPath
$built=Invoke-OwnedTestRun -Name 'compile-font-core' -Application $hostPath -Arguments @('-NoProfile','-File',('"'+$public+'"'),'-ResponseFile',('"'+$response+'"'),'-ExpectedResponseSha256',(Get-FileHash $response).Hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
if(!$built.passed){throw 'Projected font core compilation failed'}
Hold $copy;$result.candidateCoreSha256=(Get-FileHash $copy).Hash
