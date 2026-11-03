$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-NativeSelectionHostCampaign.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-Runtime8033HostCampaign.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/runtime-8033-host-preparation-v1'
if((Test-Path -LiteralPath $target) -or (Test-Path -LiteralPath $out)){throw 'Preserve host preparation'}
if((Get-FileHash -LiteralPath $source).Hash -cne 'BC2EDE82F5CE1F7F35D0424847BED51A025A7E8869A3C4C22559A7FD5664A516'){throw 'Original host campaign changed'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'')
$before=@'
    foreach($proof in @(
        @('continuity-evidence/native-selection-full-v1/outcome-observation.json','47761CBADBADA54A4AEDB45E50A5E784F702380006ECED300BC7540E59BEFCBD'),
        @('continuity-evidence/temporal-startup-v1/native-selection-script-owned-run-v1/outcome-observation.json','7387BB5A4FE461712DB314D7BAF65616DD5B047DFADF92AB09467EDDAF0458B0')
    )){Hold (Join-Path $workspace $proof[0]) $proof[1]}
    $newView=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/Lea.Win.dll'
    $newViewHash='2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
    Hold $newView $newViewHash
'@
$after=@'
    $nativePath=Join-Path $workspace 'continuity-evidence/print-live-native-v1/bound-outcome.json'
    Hold $nativePath '496EF27807197530B8F83F8E7A0AAF4AFC5B2BC036866AE7C10CC9B5815D937C'
    $nativeProof=Get-Content -LiteralPath $nativePath -Raw|ConvertFrom-Json
    $scriptPath=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/runtime-8033-script-owned-run-v1/outcome-observation.json'
    Hold $scriptPath '8FDBA6D0F5BB19D776C033C33663DC03769215F12D252A7297823B1E42F498D9'
    $scriptProof=Get-Content -LiteralPath $scriptPath -Raw|ConvertFrom-Json
    if(!$nativeProof.nativeFunctionalPassed -or !$nativeProof.focusedPassed -or $nativeProof.policyBroadened -or !$scriptProof.executionPassed -or $scriptProof.terminalChecks -ne 3412 -or $scriptProof.completedPhases -ne 89 -or !$scriptProof.singleDecoderDeltaAccepted -or $scriptProof.runtimeVersion -cne '8.0.33' -or $scriptProof.exactRuntimeConfigsVerified -ne 8){throw 'Matched prerequisite evidence incomplete'}
    $replacements=@{
        'Lea.Engine.dll'=@{source='continuity-evidence/print-live-capture-v1/Lea.Engine.dll';old='52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC';hash='A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'}
        'Lea.Win.dll'=@{source='continuity-evidence/print-live-capture-v1/Lea.Win.dll';old='74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC';hash='568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8'}
        'ScriptWorker\Lea.Script.Worker.runtimeconfig.json'=@{source='continuity-evidence/temporal-startup-v1/current-native-script-suite-v11/runtime/ScriptWorker/Lea.Script.Worker.runtimeconfig.json';old='658DE72465526C187EF01042E9411A867E3A4ADB7FAF5A8BA3FA8D5CFE1BABEA';hash='6346264D0ED421CB89893F36E6760048A72CCCEDC5A4B8BDB26B6048D6F766F6'}
    }
    $replaced=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach($replacement in $replacements.Values){Hold (Join-Path $workspace $replacement.source) $replacement.hash}
'@
$oldCopy=@'
        if($relative -ceq 'Lea.Win.dll'){
            if($file.sha256 -cne '74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC'){throw 'Original view differs'}
            Copy-Item -LiteralPath $newView -Destination $to
        }else{Copy-Item -LiteralPath $file.path -Destination $to}
        if((Get-FileHash $to).Hash -cne $(if($relative -ceq 'Lea.Win.dll'){$newViewHash}else{$file.sha256})){throw 'Copied component differs'}
'@
$newCopy=@'
        $expected=$file.sha256
        if($replacements.ContainsKey($relative)){
            $replacement=$replacements[$relative]
            if($expected -cne $replacement.old -or !$replaced.Add($relative)){throw 'Original component identity/coverage differs'}
            Copy-Item -LiteralPath (Join-Path $workspace $replacement.source) -Destination $to
            $expected=$replacement.hash
        }else{Copy-Item -LiteralPath $file.path -Destination $to}
        if((Get-FileHash -LiteralPath $to).Hash -cne $expected){throw 'Copied component differs'}
'@
$anchor="    `$exe=Join-Path `$workspace 'continuity-evidence/page-color-host-v5/host-bin/Firespider Internet 19.exe'"
$guard=@'
    if($replaced.Count -ne 3){throw 'All three runtime replacements required'}
    $configs=@(Get-ChildItem -LiteralPath $runtime -Recurse -File -Filter '*.runtimeconfig.json')
    if($configs.Count -ne 1){throw 'Host runtime config coverage differs'}
    $options=(Get-Content -LiteralPath $configs[0].FullName -Raw|ConvertFrom-Json).runtimeOptions
    if($options.framework.version -cne '8.0.33' -or $options.framework.name -cne 'Microsoft.NETCore.App' -or $options.rollForward -cne 'Disable'){throw 'Host runtime policy differs'}
    foreach($binding in @(
        @('ScriptWorker/Lea.Script.Worker.dll','60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'),
        @('ScriptWorker/Jint.dll','67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'),
        @('Lea.Script.dll','E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'),
        @('Lea.Script.Win.dll','AD4A057F5E82C8A36FA8362E54D1F746AD6264B9DF8D5447491DE001E34A59EA')
    )){Hold (Join-Path $runtime $binding[0]) $binding[1]}
    $result.runtimeVersion='8.0.33';$result.replacedComponents=3
'@
$edits=@(
    @('host-native-selection-owned-v1','host-runtime8033-owned-v1'),
    @('Own 15-stage FBBD host with new native-selection view2BEB campaign','Own 15-stage FBBD host coreA85/view568 exact8.0.33 campaign'),
    @($before.TrimEnd("`n"),$after.TrimEnd("`n")),
    @($oldCopy.TrimEnd("`n"),$newCopy.TrimEnd("`n")),
    @($anchor,($guard.TrimEnd("`n")+"`n"+$anchor))
)
$text=$original
foreach($edit in $edits){
    if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw ('Unique anchor required: '+$edit[0])}
    $text=$text.Replace($edit[0],$edit[1])
}
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Host projection inverse differs'}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Host projection syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
foreach($path in @($PSCommandPath,$target)){Copy-Item -LiteralPath $path -Destination (Join-Path $out ([IO.Path]::GetFileName($path)))}
@{passed=$true;inverseVerified=$true;syntaxVerified=$true;sourceSha256=(Get-FileHash -LiteralPath $source).Hash;targetSha256=(Get-FileHash -LiteralPath $target).Hash;runtimeExecuted=$false;limitsChanged=$false;expectedRuns=15;scope='Three matched runtime replacements and prerequisite/config/component identity guards; all original host cases, deadlines, ownership and decoder policy retained'}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $out 'result.json')
Get-Content -LiteralPath (Join-Path $out 'result.json')
