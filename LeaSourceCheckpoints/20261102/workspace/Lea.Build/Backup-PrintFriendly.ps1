param([ValidateRange(1,99)][int]$Version=1,[switch]$HtmlCheckpoint,[switch]$TimingCheckpoint,[switch]$ImagesCheckpoint,[switch]$CacheCheckpoint,[switch]$DataCheckpoint)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
if($TimingCheckpoint -and $HtmlCheckpoint){throw 'Choose one checkpoint scope'}
if($ImagesCheckpoint -and ($TimingCheckpoint -or $HtmlCheckpoint)){throw 'Choose one checkpoint scope'}
if($CacheCheckpoint -and ($ImagesCheckpoint -or $TimingCheckpoint -or $HtmlCheckpoint)){throw 'Choose one checkpoint scope'}
if($DataCheckpoint -and ($CacheCheckpoint -or $ImagesCheckpoint -or $TimingCheckpoint -or $HtmlCheckpoint)){throw 'Choose one checkpoint scope'}
$relative=@('Lea.Print/Ps.cs','Lea.Print/Py.cs','Lea.Tests/PrintFriendlyMain.cs',
    'Lea.Tests/PrintSnapshot.cs','Lea.Build/Test-PrintFriendly.ps1','Lea.Build/Backup-PrintFriendly.ps1',
    'Lea.Build/OwnedTestProcess.cs','Lea.Build/Invoke-OwnedTestRun.ps1',
    'PROJECT_CONTEXT.md','LeaForge/PRINT_FRIENDLY.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md',
    'Lea.Build/Get-PrintFriendlyBaseline.ps1','Lea.Build/Test-PrintFriendlyBaseline.ps1','Lea.Build/Test-PrintProject.ps1')
if($Version -ge 4){$relative+=@('Lea.Print/Pc.cs','Lea.Build/Get-PrintFriendlyViewBaseline.ps1','Lea.Build/Test-PrintFriendlyViewBaseline.ps1')}
if($Version -ge 6){
    $relative+=@('Lea.Tests/PrintFriendlyLiveMain.cs','Lea.Tests/PrintCaptureProbe.cs','LeaForge/test-pages/print-friendly.html')
    $relative+=@(Get-ChildItem (Join-Path $root 'continuity-evidence/print-friendly-live-v1') -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
}
if($Version -ge 7){
    $relative+=@(Get-ChildItem (Join-Path $root 'continuity-evidence/print-friendly-live-v2') -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
}
if($Version -ge 5){
    $relative+=@('Lea.Build/Reclaim-OldPackage.ps1','Lea.Build/Archive-VerifyM111.ps1','Lea.Build/Test-ReclaimedOldPackages.ps1','continuity-evidence/reclaimed-old-packages-20261028-v1.json')
    foreach($name in @('210-v3-verify210','211-v3-verifym111','210-v4-verifym111','210-v5-verifym111own')){
        $relative+=@(Get-ChildItem (Join-Path $root ('continuity-evidence/old-package-reclamation-20261026-'+$name)) -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
    }
}
$relative+=@(Get-ChildItem (Join-Path $root 'continuity-evidence/print-friendly-v1') -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
if($Version -gt 1){$relative+=@(Get-ChildItem (Join-Path $root 'continuity-evidence/print-friendly-v2') -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})}
if($Version -ge 3){
    $relative+=@('Lea.Build/Compress-HistoricalEvidence.ps1','LeaForge/STORAGE_CLEANUP.md')
    foreach($scope in @('classlisthistory','earlierruntimehistory')){
        $folder=Join-Path $root ('continuity-evidence/project-compression-20261027-'+$scope+'-friendly-reserve')
        $relative+=@(Get-ChildItem $folder -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
    }
}
if($HtmlCheckpoint){
    $relative=@('Lea.Tests/PrintHtmlChecks.cs','Lea.Tests/PrintFriendlyLiveMain.cs','Lea.Tests/PrintFriendlyMain.cs','Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintCaptureProbe.cs',
        'LeaForge/test-pages/print-friendly.html','Lea.Build/Test-PrintFriendly.ps1','Lea.Build/Backup-PrintFriendly.ps1',
        'Lea.Build/OwnedTestProcess.cs','Lea.Build/Invoke-OwnedTestRun.ps1','PROJECT_CONTEXT.md','LeaForge/PRINT_FRIENDLY.md','LeaForge/PROGRESS.md',
        'continuity-evidence/print-friendly-project-v1/result.json')
    if($Version -ge 2){
        $relative+='Lea.Build/Test-PrintProject.ps1'
        foreach($folder in @('print-html-project-v1','print-html-library-live-v1')){
            $relative+=@(Get-ChildItem (Join-Path $root ('continuity-evidence/'+$folder)) -Recurse -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
        }
    }
    $relative+=@(Get-ChildItem (Join-Path $root 'Lea.Print') -File|Where-Object {$_.Extension -in @('.cs','.csproj')}|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
    foreach($attempt in 1..3){$relative+=@(Get-ChildItem (Join-Path $root ('continuity-evidence/print-friendly-html-v'+$attempt)) -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})}
}
if($TimingCheckpoint){
    $relative=@('Lea.Tests/PrintSuiteTiming.cs','Lea.Build/Test-PrintSuiteTiming.ps1','Lea.Build/Backup-PrintFriendly.ps1',
        'Lea.Build/OwnedTestProcess.cs','Lea.Build/Invoke-OwnedTestRun.ps1','PROJECT_CONTEXT.md','LeaForge/PRINT_FRIENDLY.md','LeaForge/PROGRESS.md',
        'continuity-evidence/print-html-project-v1/result.json','continuity-evidence/print-html-library-live-v1/result.json')
    $relative+=@(Get-ChildItem (Join-Path $root 'continuity-evidence/print-suite-timing-v1') -Recurse -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
}
if($ImagesCheckpoint){
    $relative=@('Lea.Tests/PrintImageChecks.cs','Lea.Tests/PrintHtmlChecks.cs','Lea.Tests/PrintFriendlyLiveMain.cs','Lea.Tests/PrintFriendlyMain.cs','Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintCaptureProbe.cs',
        'Lea.Build/Test-PrintFriendly.ps1','Lea.Build/Test-PrintProject.ps1','Lea.Build/Backup-PrintFriendly.ps1',
        'Lea.Build/OwnedTestProcess.cs','Lea.Build/Invoke-OwnedTestRun.ps1','LeaForge/test-pages/print-friendly.html',
        'PROJECT_CONTEXT.md','LeaForge/PRINT_FRIENDLY.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md')
    $relative+=@(Get-ChildItem (Join-Path $root 'Lea.Print') -File|Where-Object {$_.Extension -in @('.cs','.csproj')}|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
    foreach($attempt in 1..2){$relative+=@(Get-ChildItem (Join-Path $root ('continuity-evidence/print-html-images-v'+$attempt)) -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})}
}
if($CacheCheckpoint -or $DataCheckpoint){
    $relative=@('Lea.Win/PrintImages.cs','Lea.Tests/PrintCacheImageChecks.cs','Lea.Tests/PrintImageChecks.cs','Lea.Tests/PrintHtmlChecks.cs',
        'Lea.Tests/PrintFriendlyLiveMain.cs','Lea.Tests/PrintFriendlyMain.cs','Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintCaptureProbe.cs',
        'Lea.Build/Test-PrintFriendly.ps1','Lea.Build/Test-PrintProject.ps1','Lea.Build/Build-PrintImageView.ps1','Lea.Build/Backup-PrintFriendly.ps1',
        'Lea.Build/OwnedTestProcess.cs','Lea.Build/Invoke-OwnedTestRun.ps1','Lea.Build/Invoke-PublicCompiler.20261026.ps1','Lea.Build/PublicCompiler.cs',
        'LeaForge/test-pages/print-friendly.html','LeaForge/test-pages/print-cache-image.html','PROJECT_CONTEXT.md','LeaForge/PRINT_FRIENDLY.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md')
    $relative+=@(Get-ChildItem (Join-Path $root 'Lea.Print') -File|Where-Object {$_.Extension -in @('.cs','.csproj')}|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
    $folders=if($DataCheckpoint){@('print-data-image-project-v1','print-data-image-live-v1','print-cache-images-v1/view-source')}else{@('print-cache-images-v1','print-cache-project-v1','print-cache-project-v2','print-cache-bridge-v1')}
    if($DataCheckpoint -and $Version -ge 2){$folders=@('print-image-layout-project-v1','print-image-layout-live-v1','print-image-layout-live-v2','print-cache-images-v1/view-source')}
    if($DataCheckpoint -and $Version -ge 3){$folders=@('document-selection-api-v1')}
    if($DataCheckpoint -and $Version -ge 4){$folders=@('document-selection-gestures-v1')}
    if($DataCheckpoint -and $Version -ge 5){$folders=@('document-selection-gestures-v1','document-selection-gestures-v2')}
    if($DataCheckpoint -and $Version -ge 6){$folders=@('document-selection-gestures-v1','document-selection-gestures-v2','document-selection-gestures-v3')}
    if($DataCheckpoint -and $Version -ge 7){$folders=@('document-selection-gestures-v4')}
    if($DataCheckpoint -and $Version -ge 8){$folders=@('document-selection-gestures-v4','document-selection-gestures-v5')}
    if($DataCheckpoint -and $Version -ge 9){$folders=@('document-selection-gestures-v4','document-selection-gestures-v5','document-selection-gestures-v6')}
    if($DataCheckpoint -and $Version -ge 10){$folders=@('document-selection-gestures-v4','document-selection-gestures-v5','document-selection-gestures-v6','document-selection-gestures-v7')}
    if($DataCheckpoint -and $Version -ge 11){$folders=@('document-selection-gestures-v4','document-selection-gestures-v5','document-selection-gestures-v6','document-selection-gestures-v7','document-selection-gestures-v8')}
    if($DataCheckpoint -and $Version -ge 12){$folders=@('document-copy-v1')}
    if($DataCheckpoint -and $Version -ge 13){$folders=@('document-copy-v2','private-clipboard-v1','private-clipboard-v2')}
    if($DataCheckpoint -and $Version -ge 14){$folders=@('document-copy-v2','document-copy-v3','private-clipboard-v1','private-clipboard-v2')}
    if($DataCheckpoint -and $Version -ge 15){$folders=@('document-uia-v1')}
    if($DataCheckpoint -and $Version -ge 16){$folders=@('document-uia-v1','document-uia-v2')}
    if($DataCheckpoint -and $Version -ge 17){$folders=@('document-uia-v3')}
    if($DataCheckpoint -and $Version -ge 18){$folders=@('document-uia-v3','selection-print-project-v1')}
    if($DataCheckpoint -and $Version -ge 19){$folders=@('selection-image-live-v1','selection-image-live-v2')}
    if($DataCheckpoint -and $Version -ge 20){$folders=@('document-uia-v4','selection-print-project-v2','selection-image-live-v3','selection-image-live-v4')}
    if($DataCheckpoint -and $Version -ge 22){$folders=@()}
    if($DataCheckpoint -and $Version -ge 23){$folders=@('selection-print-project-v4','selection-image-live-v5')}
    if($DataCheckpoint -and $Version -ge 24){$folders=@('document-uia-v5','document-selection-native-full-v1','document-selection-native-full-v2')}
    if($DataCheckpoint -and $Version -ge 25){$folders=@('document-selection-native-full-v3','document-selection-native-full-v4')}
    if($DataCheckpoint -and $Version -ge 26){$folders=@('document-broker-v1')}
    if($DataCheckpoint -and $Version -ge 27){$folders=@('document-broker-v2')}
    if($DataCheckpoint -and $Version -ge 28){$folders=@()}
    if($DataCheckpoint -and $Version -ge 29){$folders=@('document-broker-v3')}
    if($DataCheckpoint -and $Version -ge 30){$folders=@('document-broker-v4','old-package-reclamation-20261026-192-v9-verify192earlier')}
    if($DataCheckpoint -and $Version -ge 31){$folders=@()}
    if($DataCheckpoint -and $Version -ge 32){$folders=@('document-broker-v5','document-broker-v6','document-native-broker-v1')}
    if($DataCheckpoint -and $Version -ge 33){$folders=@('document-broker-v7','document-native-broker-v2')}
    if($DataCheckpoint -and $Version -ge 34){$folders=@('selection-print-project-v5','selection-image-live-v6','document-script-view-preparation-v1')}
    if($DataCheckpoint -and $Version -ge 35){$folders=@('document-script-build-preparation-v1','document-script-verify-preparation-v1','document-script-run-preparation-v1','document-script-outcome-preparation-v1')}
    if($DataCheckpoint -and $Version -ge 36){$folders=@('decoder-grant-ownership-v1')}
    if($DataCheckpoint -and $Version -ge 37){$folders=@('decoder-owner-retirement-v2')}
    if($DataCheckpoint -and $Version -ge 38){$folders=@('decoder-owner-retirement-v3','document-uia-v6')}
    if($DataCheckpoint -and $Version -ge 39){$folders=@('decoder-owner-retirement-v5','document-uia-v8')}
    if($DataCheckpoint -and $Version -ge 40){$folders=@('decoder-owner-retirement-v7','decoder-owner-retirement-v8','document-uia-v9')}
    if($DataCheckpoint -and $Version -ge 41){$folders=@('decoder-owner-retirement-v9')}
    if($DataCheckpoint -and $Version -ge 42){$folders=@('decoder-print-project-v1','decoder-image-live-v1')}
    if($DataCheckpoint -and $Version -ge 43){$folders=@('decoder-owner-retirement-v10','shutdown-print-project-v1','shutdown-image-live-v1')}
    if($DataCheckpoint -and $Version -ge 44){$folders=@('shutdown-script-preparation-v1')}
    if($DataCheckpoint -and $Version -ge 45){$folders=@('decoder-shutdown-race-v1','qualified-decoder-shutdown-race-v1','qualified-decoder-shutdown-race-v2')}
    if($DataCheckpoint -and $Version -ge 46){$folders=@('document-broker-v8')}
    if($DataCheckpoint -and $Version -ge 47){$folders=@('document-threading-probe-v1')}
    if($DataCheckpoint -and $Version -ge 48){$folders=@('document-broker-v9','document-broker-v10','document-broker-v11','document-range-lease-probe-v1')}
    if($DataCheckpoint -and $Version -ge 49){$folders=@('minimal-text-provider-v1','minimal-text-provider-v2','minimal-text-provider-v3','minimal-text-provider-v4','minimal-managed-provider-v1','minimal-managed-provider-v2','minimal-managed-provider-v3')}
    if($DataCheckpoint -and $Version -ge 50){$folders=@('minimal-cross-process-provider-v1','minimal-pair-control-v1','minimal-cross-process-provider-v2')}
    if($DataCheckpoint -and $Version -ge 51){$folders=@('lea-cross-process-provider-v1')}
    if($DataCheckpoint -and $Version -ge 52){$folders=@('document-native-broker-v3','minimal-typed-array-provider-v1','minimal-typed-array-provider-v2','minimal-typed-array-provider-v3','minimal-native-array-provider-v1')}
    if($DataCheckpoint -and $Version -ge 53){$folders=@('minimal-cold-legacy-provider-v1','wpf-cold-legacy-provider-v1','wpf-cold-legacy-x86-client-v1')}
    if($DataCheckpoint -and $Version -ge 54){$folders=@('wpf-cold-legacy-provider-v2')}
    if($DataCheckpoint -and $Version -ge 55){$folders=@('minimal-original-variant-provider-v1','minimal-original-variant-provider-v2','minimal-pair-control-v2','uia-pair-completion-v1')}
    if($DataCheckpoint -and $Version -ge 56){$folders=@('minimal-native-legacy-client-v1')}
    if($DataCheckpoint -and $Version -ge 57){$folders=@('native-edit-legacy-client-v1','native-edit-legacy-client-v2','native-edit-legacy-client-v3')}
    if($DataCheckpoint -and $Version -ge 58){$folders=@('font-shorthand-baseline-v1')}
    if($DataCheckpoint -and $Version -ge 59){$folders=@('font-shorthand-baseline-v2','font-shorthand-candidate-v1','font-shorthand-candidate-v2','font-shorthand-candidate-v3')}
    if($DataCheckpoint -and $Version -ge 60){$folders=@('font-shorthand-render-v1','font-shorthand-render-v2','font-shorthand-render-baseline-v1')}
    if($DataCheckpoint -and $Version -ge 61){$folders=@('font-script-preparation-v1','temporal-startup-v1/font-script-owned-run-v1')}
    if($DataCheckpoint -and $Version -ge 62){$folders=@()}
    if($DataCheckpoint -and $Version -ge 63){$folders=@('temporal-startup-v1/font-framework-interpreter-map-v1','temporal-startup-v1/font-framework-interpreter-map-v2','temporal-startup-v1/font-framework-interpreter-map-v3')}
    if($DataCheckpoint -and $Version -ge 64){$folders=@('temporal-startup-v1/owned-thread-snapshot-control-v1','temporal-startup-v1/font-thread-counters-v1')}
    foreach($folder in $folders){
        $relative+=@(Get-ChildItem (Join-Path $root ('continuity-evidence/'+$folder)) -Recurse -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
    }
    if($CacheCheckpoint -and $Version -ge 2){
        $relative+=@('Lea.Tests/PrintDecoderRoundTrip.cs','Lea.Build/Audit-PrintDecoderRoundTrip.ps1')
        foreach($attempt in 1..2){$relative+=@(Get-ChildItem (Join-Path $root ('continuity-evidence/print-decoder-roundtrip-v'+$attempt)) -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})}
    }
    if($DataCheckpoint){$relative+=@('Lea.Tests/PrintDecoderRoundTrip.cs','Lea.Build/Audit-PrintDecoderRoundTrip.ps1','Lea.Build/Get-PrintImageKeyBaseline.ps1','Lea.Build/Get-PrintFriendlyBaseline.ps1','Lea.Build/Get-PrintFriendlyViewBaseline.ps1')}
    if($DataCheckpoint -and $Version -ge 2){$relative+=@('Lea.Tests/PrintWpfImageChecks.cs','Lea.Build/Get-PrintImageLayoutBaseline.ps1')}
    if($DataCheckpoint -and $Version -ge 3){$relative+=@('Lea.Win/DocumentSelection.cs','Lea.Tests/DocumentSelectionChecks.cs','LeaForge/test-pages/document-selection.html','LeaForge/DOCUMENT_SELECTION.md')}
    if($DataCheckpoint -and $Version -ge 4){$relative+='Lea.Tests/DocumentSelectionGestures.cs'}
    if($DataCheckpoint -and $Version -ge 11){$relative+=@('LeaForge/COMPATIBILITY.md','LeaForge/history/PROJECT_CONTEXT_20261028_before_measured_selection.md')}
    if($DataCheckpoint -and $Version -ge 12){$relative+='Lea.Tests/DocumentCopyChecks.cs'}
    if($DataCheckpoint -and $Version -ge 13){$relative+=@('Lea.Tests/PrivateClipboardStation.cs','Lea.Tests/PrivateClipboardMain.cs','Lea.Build/Test-PrivateClipboard.ps1','LeaForge/PRIVATE_CLIPBOARD.md')}
    if($DataCheckpoint -and $Version -ge 15){$relative+=@('Lea.Win/DocumentSelectionAutomation.cs','Lea.Tests/DocumentSelectionAutomationChecks.cs')}
    if($DataCheckpoint -and $Version -ge 24){$relative+='Lea.Build/Test-NativeSelectionFull.ps1'}
    if($DataCheckpoint -and $Version -ge 25){$relative+='Lea.Build/Project-DocumentSelectionNativeSmoke.ps1'}
    if($DataCheckpoint -and $Version -ge 26){$relative+='Lea.Tests/DocumentBrokerChecks.cs'}
    if($DataCheckpoint -and $Version -ge 27){$relative+='Lea.Build/Reclaim-OldPackage.ps1'}
    if($DataCheckpoint -and $Version -ge 28){$relative+='Lea.Tests/DocumentRangeAbiChecks.cs'}
    if($DataCheckpoint -and $Version -ge 31){$relative+=@('Lea.Tests/DocumentRangeApartmentChecks.cs','LeaForge/history/PROJECT_CONTEXT_20261029_before_broker_apartment.md')}
    if($DataCheckpoint -and $Version -ge 32){$relative+='Lea.Tests/DocumentNativeClientChecks.cs'}
    if($DataCheckpoint -and $Version -ge 34){$relative+='Lea.Build/Prepare-DocumentSelectionScriptView.ps1'}
    if($DataCheckpoint -and $Version -ge 35){
        $relative+='LeaForge/DOCUMENT_SCRIPT.md'
        foreach($part in @('Build','Verify','Run','Outcome')){$relative+=('Lea.Build/Prepare-DocumentSelectionScript'+$part+'.ps1')}
        $relative+=@('Lea.Build/Build-DocumentSelectionScriptSuite-v1.generated.ps1','Lea.Build/Verify-DocumentSelectionScriptSuite-v1.generated.ps1','Lea.Build/Test-OwnedDocumentSelectionScriptSuite-v1.generated.ps1','Lea.Build/Audit-DocumentSelectionScriptOutcome-v1.generated.ps1')
        foreach($name in @('result.json','binding-verification.json')){$relative+=('continuity-evidence/temporal-startup-v1/document-selection-script-suite-v1/'+$name)}
        $relative+=@(Get-ChildItem (Join-Path $root 'continuity-evidence/temporal-startup-v1/document-selection-script-owned-run-v1') -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
    }
    if($DataCheckpoint -and $Version -ge 36){$relative+='Lea.Build/Audit-DecoderGrantOwnership.ps1'}
    if($DataCheckpoint -and $Version -ge 37){$relative+=@('Lea.Build/Project-DecoderRetirement.ps1','Lea.Build/Test-DecoderRetirement.ps1','Lea.Tests/DecoderRetirementMain.cs','Lea.Build/Build-PrintImageView.ps1')}
    if($DataCheckpoint -and $Version -ge 39){$relative+='Lea.Build/Project-DecoderLauncherFailure.ps1'}
    if($DataCheckpoint -and $Version -ge 46){$relative+='Lea.Tests/DocumentLegacyHandleChecks.cs'}
    if($DataCheckpoint -and $Version -ge 47){$relative+=@('Lea.Build/Relocate-UiaSourceBackup.ps1','Lea.Build/Move-UiaSourceBackup.generated.ps1','Lea.Build/Restore-UiaSourceBackup.ps1','continuity-evidence/uia-source-relocation-20261029.json','LeaForge/STORAGE_CLEANUP.md','LeaForge/RUNTIME_8033_PLAN.md')}
    if($DataCheckpoint -and $Version -ge 49){$relative+=@('Lea.Tests/MinimalTextProviderMain.cs','Lea.Build/Test-MinimalTextProvider.ps1','LeaForge/LEGACY_UIA_BASELINE.md')}
    if($DataCheckpoint -and $Version -ge 50){$relative+=@('Lea.Tests/MinimalTextProviderPairModes.cs','Lea.Build/Invoke-UiaProcessPair.ps1')}
    if($DataCheckpoint -and $Version -ge 51){$relative+='Lea.Tests/LeaTextProviderServerMain.cs'}
    if($DataCheckpoint -and $Version -ge 52){$relative+=@('Lea.Tests/TypedTextProviderContract.cs','Lea.Tests/ProviderArrayMetadata.cs','Lea.Tests/NativeProviderArray.cs')}
    if($DataCheckpoint -and $Version -ge 53){$relative+='Lea.Tests/WpfTextProviderServerMain.cs'}
    if($DataCheckpoint -and $Version -ge 54){$relative+=@('Lea.Tests/UiaComponentInventory.cs','Lea.Build/Audit-UiaComponentInventory.ps1','Lea.Build/Audit-FrameworkUiaDeclarations.ps1')}
    if($DataCheckpoint -and $Version -ge 55){$relative+='Lea.Build/Test-UiaPairCompletion.ps1'}
    if($DataCheckpoint -and $Version -ge 56){$relative+=@('Lea.Tests/NativeLegacyUiaClient.cpp','Lea.Build/Build-NativeLegacyUiaClient.ps1')}
    if($DataCheckpoint -and $Version -ge 58){$relative+=@('Lea.Tests/FontShorthandMain.cs','Lea.Build/Test-FontShorthand.ps1','LeaForge/test-pages/font-shorthand.html','LeaForge/FONT_SHORTHAND.md','LeaForge/COMPATIBILITY.md')}
    if($DataCheckpoint -and $Version -ge 59){$relative+=@('Lea.Engine/FontShorthand.cs','Lea.Build/Build-FontShorthandCore.ps1')}
    if($DataCheckpoint -and $Version -ge 60){$relative+=@('Lea.Tests/FontShorthandRenderMain.cs','Lea.Build/Build-FontShorthandView.ps1','LeaForge/test-pages/font-shorthand-render.html')}
    if($DataCheckpoint -and $Version -ge 61){
        $relative+=@('Lea.Build/Prepare-FontScript.ps1','Lea.Build/Build-FontScriptSuite-v1.generated.ps1','Lea.Build/Verify-FontScriptSuite-v1.generated.ps1','Lea.Build/Test-OwnedFontScriptSuite-v1.generated.ps1','Lea.Build/Audit-FontScriptFailure.ps1','LeaForge/TRUSTED_BOOTSTRAP_COST.md')
        $fontSuite=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-script-suite-v1'
        $relative+=@(Get-ChildItem $fontSuite -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
        $relative+=@(Get-ChildItem (Join-Path $fontSuite 'source') -Recurse -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
    }
    if($DataCheckpoint -and $Version -ge 62){
        $relative+=@('Lea.Build/Test-FontBootstrapCounters.ps1','Lea.Build/Audit-FontBootstrapCounters.ps1','Lea.Build/ExternalProcessCounters.cs','Lea.Build/New-ExternalBootstrapProgram.ps1')
        $counterSuite=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-bootstrap-counters-v1'
        $relative+=@(Get-ChildItem $counterSuite -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
        $relative+=@(Get-ChildItem (Join-Path $counterSuite 'source') -Recurse -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
    }
    if($DataCheckpoint -and $Version -ge 63){$relative+=@('Lea.Build/Audit-FrameworkInterpreterMap.ps1','Lea.Build/FrameworkInterpreterIlMap.cs','Lea.Build/ReadFailureIL.cs')}
    if($DataCheckpoint -and $Version -ge 64){$relative+=@('Lea.Build/OwnedThreadSnapshot.cs','Lea.Tests/OwnedThreadSnapshotControl.cs','Lea.Build/Test-OwnedThreadSnapshot.ps1','Lea.Build/Test-FontThreadCounters.ps1','Lea.Build/Audit-FontThreadCounters.ps1')}
    if($DataCheckpoint -and $Version -ge 44){
        $relative+=@('Lea.Build/Prepare-ShutdownScript.ps1','Lea.Build/Build-ShutdownScriptSuite-v1.generated.ps1','Lea.Build/Verify-ShutdownScriptSuite-v1.generated.ps1','Lea.Build/Test-OwnedShutdownScriptSuite-v1.generated.ps1')
        $suite=Join-Path $root 'continuity-evidence/temporal-startup-v1/shutdown-script-suite-v1'
        $relative+=@(Get-ChildItem $suite -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
        $relative+=@(Get-ChildItem (Join-Path $suite 'source') -File -Recurse|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
        $relative+=@(Get-ChildItem (Join-Path $root 'continuity-evidence/temporal-startup-v1/shutdown-script-owned-run-v1') -File|ForEach-Object {[IO.Path]::GetRelativePath($root,$_.FullName)})
    }
}
$files=@($relative|ForEach-Object {Get-Item -LiteralPath (Join-Path $root $_)})
$total=($files|Measure-Object Length -Sum).Sum
$checkpointLimit=if($DataCheckpoint -or $CacheCheckpoint -or ($HtmlCheckpoint -and $Version -ge 2)){16MB}else{8MB}
if($total -gt $checkpointLimit -or (Get-PSDrive C).Free -lt 32MB+2*$total){throw 'Bounded checkpoint storage reserve required; this does not authorize a build'}
$name=if($Version -eq 1){'print-friendly-wip-source-20261028.zip'}else{'print-friendly-source-20261028-v'+$Version+'.zip'}
if($HtmlCheckpoint){$name='print-html-source-20261028-v'+$Version+'.zip'}
if($TimingCheckpoint){$name='print-suite-timing-source-20261028-v'+$Version+'.zip'}
if($ImagesCheckpoint){$name='print-images-source-20261028-v'+$Version+'.zip'}
if($CacheCheckpoint){$name='print-cache-source-20261028-v'+$Version+'.zip'}
if($DataCheckpoint){$name='print-data-source-20261028-v'+$Version+'.zip'}
$path=Join-Path $root ('continuity-evidence/'+$name)
if(Test-Path $path){throw 'Preserve existing checkpoint'}
Add-Type -AssemblyName System.IO.Compression
$zip=[IO.Compression.ZipFile]::Open($path,'Create')
try{foreach($entry in $relative){$null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,(Join-Path $root $entry),$entry.Replace('\','/'),'Optimal')}}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($path)
try{
    if($zip.Entries.Count -ne $relative.Count){throw 'Archive inventory differs'}
    foreach($entry in $zip.Entries){
        $stream=$entry.Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($hash -cne (Get-FileHash (Join-Path $root $entry.FullName)).Hash){throw 'Archive bytes differ'}
    }
}finally{$zip.Dispose()}
[pscustomobject]@{files=$relative.Count;sourceBytes=$total;zipBytes=(Get-Item $path).Length;sha256=(Get-FileHash $path).Hash;buildRun=$false}|ConvertTo-Json
