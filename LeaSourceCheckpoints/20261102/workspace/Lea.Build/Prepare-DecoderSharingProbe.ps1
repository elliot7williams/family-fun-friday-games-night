$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-TrustedBootstrapStages.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne 'DD0A3222C7D0C91810501383C055B947999C42D4028D36D579671F147646B008'){throw 'Probe runner changed'}
$target=Join-Path $PSScriptRoot 'Test-DecoderSharing.generated.ps1'
if(Test-Path $target){throw 'Preserve sharing probe'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('trusted-stage-probe-v1','decoder-sharing-v1'),
    @('suite-phase-relocated-v2/result.json','current-lazy-suite-v2/result.json'),
    @('1DF0860AAEF98A61FDBE997A31889228061A2E30D02E9D705C7F9503CA352A18','0D71CCA8558E48610A8B90C8A330D6D0EC1B7338E1174086E10B9AE5EEF395BA'),
    @('TrustedBootstrapProbe','DecoderSharingProbe'),
    @("'compile-trusted-probe'","'compile-decoder-probe'"),
    @("'trusted-bootstrap'","'decoder-sharing'"),
    @("if([IO.Path]::GetExtension(`$name) -ceq '.dll')","if([IO.Path]::GetExtension(`$name) -ceq '.dll' -or `$name -ceq 'Lea.Image.exe')"),
    @("`$fixture=Join-Path `$build.fixtureSource 'LeaForge/test-pages/javascript-runtime.html'","`$fixture=Join-Path `$result.candidate 'Lea.Image.exe'"),
    @('$row=@($build.fixtureFiles|Where-Object path -ceq $fixture)','$row=@($build.files|Where-Object { [IO.Path]::GetFileName($_.path) -ceq ''Lea.Image.exe'' -and (Split-Path $_.path) -ceq $build.runtime })'),
    @('if(!$result.report.observerRetired -or $result.report.observerError -or $result.report.samplesOverflow)','if(!$result.report.passed -or !$result.report.cleanupVerified -or $result.report.processes.Count -ne 2)')
)){
    if(!$text.Contains($pair[0])){throw ('Missing adaptation: '+$pair[0])}
    $text=$text.Replace($pair[0],$pair[1])
}
$old='$path=Join-Path $result.candidate ([IO.Path]::GetFileName($path));$null=Pin $path $hashes[0]'
$new='$name=[IO.Path]::GetFileName($path);$selected=@($build.files|Where-Object { $_.path -ceq (Join-Path $build.runtime $name) });if($selected.Count -ne 1){throw ''Current reference provenance missing''};$path=Join-Path $result.candidate $name;$null=Pin $path $selected[0].sha256'
if(!$text.Contains($old)){throw 'Reference adaptation missing'}
$text=$text.Replace($old,$new)
$t=$null;$e=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$t,[ref]$e);if($e.Count){throw 'Sharing runner syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
(Get-FileHash -LiteralPath $target).Hash
