$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/runtime-8033-verification-preparation-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve verification preparation'}
$policyGuard=@'
if($b.relocation.runtimeConfigVersion -cne '8.0.33' -or $b.relocation.configProjections -ne 8 -or !$b.relocation.fixtureNonConfigPinsUnchanged -or $b.relocation.workerChanged){throw 'Runtime projection binding incomplete'}
$configs=@(Get-ChildItem -LiteralPath $b.runtime -Recurse -File -Filter '*.runtimeconfig.json')
if($configs.Count -ne 8){throw 'Runtime config coverage changed'}
foreach($config in $configs){
    $options=(Get-Content -LiteralPath $config.FullName -Raw|ConvertFrom-Json).runtimeOptions
    if($options.framework.name -cne 'Microsoft.NETCore.App' -or $options.framework.version -cne '8.0.33' -or $options.rollForward -cne 'Disable'){throw 'Runtime policy changed'}
}
'@
$specs=@(
    @{source='Verify-PrintLiveScriptSuite.generated.ps1';hash='312201DFA75555E10A3059EB0B4AB828434812CEF54F888D4B493D6DCE39ACBE';target='Verify-Runtime8033Suite.generated.ps1';edits=@(
        @('$SuiteVersion=10','$SuiteVersion=11'),
        @('if(!$b.nativeSelectionView -or !$b.printLiveView)',($policyGuard.TrimEnd("`r","`n")+"`nif(!`$b.nativeSelectionView -or !`$b.printLiveView)"))
    )},
    @{source='Test-OwnedPrintLiveScriptSuite.generated.ps1';hash='AC4348337195ACA0F60EBF2F4EBE14D5BC101288529E9134C0E2A30B9557C0FF';target='Test-OwnedRuntime8033Suite.generated.ps1';edits=@(
        @('print-live-script-owned-run-v1','runtime-8033-script-owned-run-v1'),
        @('current-native-script-suite-v10','current-native-script-suite-v11'),
        @('Test-ComponentProbePrerequisites.ps1','Test-ComponentProbePrerequisites8033.generated.ps1'),
        @('21DF74BFC400ABDE2C5119C4CB8870E8D791521655568546AEA3CDB5DA3A26D5','7B2FC0FE6EFA950A72DCFFC63F2251AAD37FC266BED76E4492D821643AE6BC42'),
        @('    $result.candidate=$build.runtime',"    if(`$build.relocation.runtimeConfigVersion -cne '8.0.33' -or `$build.relocation.configProjections -ne 8 -or `$build.relocation.workerChanged){throw 'Wrong runtime candidate'}`n    `$result.candidate=`$build.runtime"),
        @('Exact60-source timed full script suite with print coreA85/view568 under owned Run; original300s/5000ms budgets, parent/adapter/worker unchanged','Exact60-source timed full script suite coreA85/view568 with exact8.0.33 configs under owned Run; original300s/5000ms budgets and worker identities retained')
    )}
)
$prepared=@(foreach($spec in $specs){
    $source=Join-Path $PSScriptRoot $spec.source;$target=Join-Path $PSScriptRoot $spec.target
    if(Test-Path -LiteralPath $target){throw 'Preserve generated verifier/runner'}
    if((Get-FileHash -LiteralPath $source).Hash -cne $spec.hash){throw 'Original helper changed'}
    $original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
    foreach($edit in $spec.edits){
        if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw ('Unique anchor required: '+$edit[0])}
        $text=$text.Replace($edit[0],$edit[1])
    }
    $inverse=$text
    for($i=$spec.edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($spec.edits[$i][1],$spec.edits[$i][0])}
    if($inverse -cne $original){throw 'Inverse differs'}
    $tokens=$null;$errors=$null
    $null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Syntax differs'}
    @{source=$source;sourceSha256=$spec.hash;target=$target;text=$text}
})
$null=New-Item -ItemType Directory -Path $out
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
$records=@(foreach($entry in $prepared){
    [IO.File]::WriteAllText($entry.target,$entry.text,[Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $entry.target -Destination (Join-Path $out ([IO.Path]::GetFileName($entry.target)))
    @{source=$entry.source;sourceSha256=$entry.sourceSha256;target=$entry.target;sha256=(Get-FileHash -LiteralPath $entry.target).Hash;inverseVerified=$true;syntaxVerified=$true}
})
@{passed=$true;helpers=$records;runtimeExecuted=$false;limitsChanged=$false;scope='Suite11 verifier and owned runner source projections with exact runtime policy guard; not runtime qualification'}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'result.json')
$records|ConvertTo-Json
