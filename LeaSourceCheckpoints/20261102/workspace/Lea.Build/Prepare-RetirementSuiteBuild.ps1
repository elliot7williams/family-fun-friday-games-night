param([switch]$Closure,[switch]$MainOnly)
$ErrorActionPreference='Stop'
if($Closure -and $MainOnly){throw 'Choose one diagnostic scope'}
$template=Join-Path $PSScriptRoot 'Build-SdkFullScript.ps1'
if((Get-FileHash $template).Hash -cne '3871EFEB48EF912C1A9D8E296AD9C32D8CF7231C4913E4680CD29420B5FC23BB'){throw 'Suite build template differs'}
$to=Join-Path $PSScriptRoot $(if($MainOnly){'Build-RetirementSuite-v4.generated.ps1'}elseif($Closure){'Build-RetirementSuite-v3.generated.ps1'}else{'Build-RetirementSuite-v2.generated.ps1'})
if(Test-Path $to){throw 'Preserve diagnostic suite builder'}
$original=[IO.File]::ReadAllText($template)
$pairs=@(
    @('I:\LeaBuildStorage\contains-recovery-assembly-20261102-v1','I:\LeaBuildStorage\retirement-prefix-diagnostic-20261102-v2'),
    @('I:\LeaBuildStorage\contains-recovery-compile-20261102-v1','I:\LeaBuildStorage\retirement-prefix-compile-20261102-v2'),
    @('AFFDEA3E82AE42779C1ABA5292D8E40847D72CB4A8C4E8B61F0B1E99E2BE196F','1E3342366AFBC319D9A8297A3716262F65150F36F6A0DD90CDD4F552EBD67A33')
)
$text=$original
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique build projection required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Suite build inverse differs'}
if($Closure){
    $text=$text.Replace('retirement-prefix-diagnostic-20261102-v2','retirement-prefix-diagnostic-20261102-v3').Replace('retirement-prefix-compile-20261102-v2','retirement-prefix-compile-20261102-v3').Replace('1E3342366AFBC319D9A8297A3716262F65150F36F6A0DD90CDD4F552EBD67A33','CF629A10368624EB10E144D5C8D861DC86E097246DAFD6EA7EE4631B5468FEBD').Replace('$assembly.changes.Count -ne 4','$assembly.changes.Count -ne 5')
}
if($MainOnly){
    $text=$text.Replace('retirement-prefix-diagnostic-20261102-v2','retirement-prefix-main-only-20261102-v4').Replace('retirement-prefix-compile-20261102-v2','retirement-prefix-compile-20261102-v4').Replace('1E3342366AFBC319D9A8297A3716262F65150F36F6A0DD90CDD4F552EBD67A33','043777669F93998DC2D7FA51F66DBAAB7FD6B9612CD568AEFAE65C512B2D2C6C')
}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
