$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/runtime-8033-outcome-preparation-v1'
$source=Join-Path $PSScriptRoot 'Audit-OwnedNativeSelectionScriptOutcome.generated.ps1'
$target=Join-Path $PSScriptRoot 'Audit-OwnedRuntime8033Outcome.generated.ps1'
if((Test-Path -LiteralPath $out) -or (Test-Path -LiteralPath $target)){throw 'Preserve outcome preparation'}
if((Get-FileHash -LiteralPath $source).Hash -cne '922840D23BD0D8C20C1BD3368928A5EE5AC0BC73D5525CB43C60B0AE1D653E6A'){throw 'Original audit changed'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'')
$guard=@'
    if($b.relocation.runtimeConfigVersion -cne '8.0.33' -or $b.relocation.configProjections -ne 8 -or $b.relocation.workerChanged){throw 'Runtime projection identity differs'}
    $configs=@(Get-ChildItem -LiteralPath $b.runtime -Recurse -File -Filter '*.runtimeconfig.json')
    if($configs.Count -ne 8){throw 'Eight exact runtime configurations required'}
    foreach($config in $configs){
        $options=(Get-Content -LiteralPath $config.FullName -Raw|ConvertFrom-Json).runtimeOptions
        if($options.framework.version -cne '8.0.33' -or $options.framework.name -cne 'Microsoft.NETCore.App' -or $options.rollForward -cne 'Disable'){throw 'Exact runtime config differs'}
    }
'@
$edits=@(
    @('native-selection-script-owned-run-v1','runtime-8033-script-owned-run-v1'),
    @('current-native-script-suite-v9/result.json','current-native-script-suite-v11/result.json'),
    @('current-native-script-suite-v9/binding-verification.json','current-native-script-suite-v11/binding-verification.json'),
    @('6AC81773C57751A2665B1FABB51647BC5A956257CBE1A7D1277EABF2D05E4012','B651EC32E7B3EA5ABE2E6616437AF3C858A178732EDC7982A60F3DDC72696DC5'),
    @('A39E41059D57DBE88668E4790C2FE7B7E948D80C6199DB284D91938C5E7D03FE','A761DBBC76F221787E4D11F2A0E7D42DFDEFA296E4D90AC74DEA19AC5A0C096F'),
    @('5EE6B967BAF69F336C393F6AED5E90BE8D2C69D203FA1A24B13B64A92BC53537','A8D581FBF133F59ADF238C5B65C33F4A360CC6971399300F84698A75FDCFA9FA'),
    @('C5C64B012DDF6738FB95D586B06F77B260F78DAF00EF3A58039D0DEC1D415653','149183F5D044E04544831BA39C683D87D45F1E94242F5769CA8F0C36FD49B4B9'),
    @('    . $paths.footer',($guard.TrimEnd("`r","`n")+"`n    . `$paths.footer")),
    @('    [IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 6))',"    `$result.runtimeVersion='8.0.33';`$result.exactRuntimeConfigsVerified=8`n    [IO.File]::WriteAllText(`$out,(`$result|ConvertTo-Json -Depth 6))")
)
$text=$original
foreach($edit in $edits){
    if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw ('Unique anchor required: '+$edit[0])}
    $text=$text.Replace($edit[0],$edit[1])
}
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Audit inverse differs'}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Audit syntax invalid'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
foreach($path in @($PSCommandPath,$target)){Copy-Item -LiteralPath $path -Destination (Join-Path $out ([IO.Path]::GetFileName($path)))}
@{passed=$true;inverseVerified=$true;syntaxVerified=$true;sourceSha256=(Get-FileHash -LiteralPath $source).Hash;targetSha256=(Get-FileHash -LiteralPath $target).Hash;aclsModified=$false;policyChanged=$false;scope='Pinned outcome audit projection plus exact runtime config checks; raw result remains unchanged'}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $out 'result.json')
Get-Content -LiteralPath (Join-Path $out 'result.json')
