$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Build-MappedNativeInterpreter.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '612E3C72B6CE1CDE7F9A70E69E6E51F230815349085F708B28DD49507C4C67A4'){throw 'Native owner changed'}
$target=Join-Path $PSScriptRoot 'Build-ArchivedNativeWorkerV2.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve worker native build'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('interpreter-mapped-native-v1','worker-archive-native-v2'),
    @('interpreter-mapped-build-v1/rebuild-verification.json','worker-archive-build-v1/result.json'),
    @('86199E378206B2398B934DF76DB4D1F90905FB39DD22CE32B6D8316874598DE7','15A9162F4B9E06F0D8455FC05D2250C62DC57127BACF56F9CAB6FC39FCADA75D'),
    @("Join-Path `$root 'native-pair-v2/literal-input-verification.json'","Join-Path `$workspace 'continuity-evidence/worker-archive-build-v1/archive/proofs/native-build.json'"),
    @('98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8','896375B3D0BA4DD7C26EEB9B933C5548201B15AC5669F363039EBD7EA8C3AFFF'),
    @('!$proof.passed -or $proof.verified.Count -ne 382','!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention -or $proof.inputs.Count -ne 200'),
    @('foreach($file in $proof.verified)','foreach($file in $proof.inputs)'),
    @('interpreter-mapped-build-v1/emit/Jint.dll','worker-archive-build-v1/Lea.Script.Worker.dll'),
    @('13B3EF3BA07B3DE50572D07D1B485A1ACDEBE307BE935C6E6F5D5771C492E32F','83FE295571FAFF00D4864A13C37D270B998B339EA27A61A950A3572A494AC2DB'),
    @('67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D','60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'),
    @("Join-Path `$root 'native-pair-v2/lazy/Jint.dll'","Join-Path `$workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/paired-worker-native-v1/Lea.Script.Worker.dll'"),
    @("Join-Path `$root 'native-pair-v2/lazy/native.rsp'","Join-Path `$workspace 'continuity-evidence/worker-archive-build-v1/archive/recipes/native.rsp'"),
    @("Join-Path `$outRoot 'Jint.dll'","Join-Path `$outRoot 'Lea.Script.Worker.dll'"),
    @('$result.maps.Count -eq 2','$result.maps.Count -eq 0')
)){
    if(!$text.Contains($pair[0])){throw ('Missing native worker anchor: '+$pair[0])}
    $text=$text.Replace($pair[0],$pair[1])
}
$anchor='    $lines=[IO.File]::ReadAllLines($recipe);'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Native recipe anchor differs'}
$text=$text.Replace($anchor,"    Hold `$recipe 'AABDDE8A810BA4143501045AA128E4500479EE2273269F08353F88143AF83F03'`r`n"+$anchor)
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Native worker syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
