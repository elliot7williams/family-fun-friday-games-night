$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-BootstrapFunctionComparison-v1.generated.ps1'
$to=Join-Path $PSScriptRoot 'Test-PrivateBindingComparison-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve private binding comparison'}
if((Get-FileHash $from).Hash -cne 'F1A893867E612230CC92D6259E080D10254E600224B63A043BED3E8DB7C1D315'){throw 'Fixed comparison template changed'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('continuity-evidence/bootstrap-function-comparison-v1','continuity-evidence/private-binding-comparison-v1'),
    @('bootstrap-function-build-20261102-v1/result.json','private-binding-build-20261102-v1/result.json'),
    @('2597084C56DE45AA9DB11132091E02EE5F3C1D84890D11EE779F553CA2DD5D93','D32732C0BF3B548DB649966100D4B074663E07544BC9C25A648A80D6723AB02C'),
    @('62B5656A41720E124A915E36E437C95337E06259985EC10923CA9E987AE7E17E','406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F')
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique comparison boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Private binding comparison inverse differs'}
foreach($critical in @('600MB+$stageBytes',"@('baseline','candidate','candidate','baseline')",'-Seconds 120','-Seconds 30','Unsafe comparative attempt prevents continuation')){if([regex]::Matches($text,[regex]::Escape($critical)).Count -ne [regex]::Matches($original,[regex]::Escape($critical)).Count){throw 'Comparison safety operation changed'}}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
