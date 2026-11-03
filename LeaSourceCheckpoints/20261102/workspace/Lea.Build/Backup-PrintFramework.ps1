param([ValidateSet('Framework','Capture','Geometry','Unified','Text','Font','Wpf','WpfLayout','Frame','Paginator','Xps','Preview','Host','Document','Styles','StyledDocument','LiveCapture')][string]$Stage='Framework')
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$stages=@{
    Framework=@('print-framework-source','print-snapshot-framework-v1')
    Capture=@('print-capture-source','print-capture-view-v1')
    Geometry=@('print-geometry-source','print-geometry-framework-v1')
    Unified=@('print-unified-source','print-unified-view-v1')
    Text=@('print-text-source','print-text-view-v1')
    Font=@('print-font-source','print-font-probe-v1')
    Wpf=@('print-wpf-source','print-wpf-probe-v1')
    WpfLayout=@('print-wpf-layout-source','print-wpf-layout-v2')
    Frame=@('print-frame-source','print-frame-v1')
    Paginator=@('print-paginator-source','print-paginator-v1')
    Xps=@('print-xps-source','print-xps-v3')
    Preview=@('print-preview-source','print-preview-v1')
    Host=@('print-host-source','print-host-v1')
    Document=@('print-document-source','print-document-v2')
    Styles=@('print-styles-source','print-styles-v3')
    StyledDocument=@('print-styled-document-source','print-styled-document-v2')
    LiveCapture=@('print-live-capture-source','print-live-capture-v1')
}
$archive=Join-Path $workspace ('continuity-evidence/'+$stages[$Stage][0]+'-20261027.zip')
if(Test-Path $archive){throw 'Preserve checkpoint'}
$root=Join-Path $workspace ('continuity-evidence/'+$stages[$Stage][1])
$paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($relative in @('PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/PRINTING.md','Lea.Print/Ps.cs','Lea.Print/Lea.Print.csproj','Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintSnapshotMain.cs','Lea.Build/Test-PrintSnapshotFramework.ps1','Lea.Build/Backup-PrintFramework.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
foreach($file in Get-ChildItem $root -File){$null=$paths.Add($file.FullName)}
if($Stage -eq 'Capture'){
    foreach($relative in @('Lea.Print/Pc.cs','Lea.Tests/PrintCaptureProbe.cs','Lea.Build/Prepare-PrintCapture.ps1','Lea.Build/Test-PrintCapture.generated.ps1','continuity-evidence/print-capture-preparation-v1/result.json')){$null=$paths.Add((Join-Path $workspace $relative))}
}
if($Stage -eq 'Geometry'){
    foreach($relative in @('Lea.Print/Po.cs','Lea.Tests/PrintGeometry.cs','Lea.Tests/PrintGeometryMain.cs','Lea.Build/Prepare-PrintGeometry.ps1','Lea.Build/Test-PrintGeometry.generated.ps1','continuity-evidence/print-geometry-preparation-v1/result.json')){$null=$paths.Add((Join-Path $workspace $relative))}
}
if($Stage -eq 'Unified'){
    foreach($relative in @('Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Tests/PrintCaptureProbe.cs','Lea.Tests/PrintGeometry.cs','Lea.Tests/PrintUnifiedMain.cs','Lea.Build/Prepare-PrintUnified.ps1','Lea.Build/Test-PrintUnified.generated.ps1','continuity-evidence/print-unified-preparation-v1/result.json')){$null=$paths.Add((Join-Path $workspace $relative))}
}
if($Stage -eq 'Text'){
    foreach($relative in @('Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintCaptureProbe.cs','Lea.Tests/PrintGeometry.cs','Lea.Tests/PrintUnifiedMain.cs','Lea.Tests/PrintTextMain.cs','Lea.Build/Prepare-PrintText.ps1','Lea.Build/Test-PrintText.generated.ps1','continuity-evidence/print-text-preparation-v1/result.json')){$null=$paths.Add((Join-Path $workspace $relative))}
}
if($Stage -eq 'Font'){
    foreach($relative in @('Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintFontProbe.cs','Lea.Build/Prepare-PrintFontProbe.ps1','Lea.Build/Test-PrintFontProbe.generated.ps1','continuity-evidence/print-font-preparation-v1/result.json')){$null=$paths.Add((Join-Path $workspace $relative))}
}
if($Stage -eq 'Wpf'){
    foreach($relative in @('Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintWpfProbe.cs','Lea.Build/Prepare-PrintWpfProbe.ps1','Lea.Build/Test-PrintWpfProbe.generated.ps1','continuity-evidence/print-wpf-preparation-v1/result.json')){$null=$paths.Add((Join-Path $workspace $relative))}
}
if($Stage -eq 'WpfLayout'){
    foreach($relative in @('Lea.Print/Pw.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintWpfLayoutMain.cs','Lea.Build/Prepare-PrintWpfLayout.ps1','Lea.Build/Test-PrintWpfLayout.generated.ps1','Lea.Build/Test-PrintWpfLayoutV2.generated.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
    foreach($folder in @('print-wpf-layout-v1','print-wpf-layout-preparation-v1','print-wpf-layout-preparation-v2')){foreach($file in Get-ChildItem (Join-Path $workspace ('continuity-evidence/'+$folder)) -File){$null=$paths.Add($file.FullName)}}
}
if($Stage -eq 'Frame'){
    foreach($relative in @('Lea.Print/Pw.cs','Lea.Print/Pf.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintFrameMain.cs','Lea.Tests/PrintWpfLayoutMain.cs','Lea.Build/Prepare-PrintFrame.ps1','Lea.Build/Test-PrintFrame.generated.ps1','continuity-evidence/print-frame-preparation-v1/result.json','continuity-evidence/print-frame-preflight-v1/README.md')){$null=$paths.Add((Join-Path $workspace $relative))}
}
if($Stage -eq 'Paginator'){
    foreach($relative in @('Lea.Print/Pw.cs','Lea.Print/Pf.cs','Lea.Print/Pp.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintPaginatorMain.cs','Lea.Tests/PrintFrameMain.cs','Lea.Tests/PrintWpfLayoutMain.cs','Lea.Build/Prepare-PrintPaginator.ps1','Lea.Build/Test-PrintPaginator.generated.ps1','continuity-evidence/print-paginator-preparation-v1/result.json')){$null=$paths.Add((Join-Path $workspace $relative))}
}
if($Stage -eq 'Xps'){
    foreach($relative in @('Lea.Print/Pw.cs','Lea.Print/Pf.cs','Lea.Print/Pp.cs','Lea.Print/Px.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintXpsMain.cs','Lea.Tests/PrintPaginatorMain.cs','Lea.Tests/PrintFrameMain.cs','Lea.Tests/PrintWpfLayoutMain.cs','Lea.Build/Prepare-PrintXps.ps1','Lea.Build/Prepare-PrintXpsV2.ps1','Lea.Build/Prepare-PrintXpsV3.ps1','Lea.Build/Test-PrintXps.generated.ps1','Lea.Build/Test-PrintXpsV2.generated.ps1','Lea.Build/Test-PrintXpsV3.generated.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
    foreach($folder in @('print-xps-v1','print-xps-v2','print-xps-preparation-v1','print-xps-preparation-v2','print-xps-preparation-v3')){foreach($file in Get-ChildItem (Join-Path $workspace ('continuity-evidence/'+$folder)) -File){$null=$paths.Add($file.FullName)}}
}
if($Stage -eq 'Preview'){
    foreach($relative in @('Lea.Print/Pw.cs','Lea.Print/Pf.cs','Lea.Print/Pp.cs','Lea.Print/Px.cs','Lea.Print/Pv.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintPreviewMain.cs','Lea.Tests/PrintXpsMain.cs','Lea.Tests/PrintPaginatorMain.cs','Lea.Tests/PrintFrameMain.cs','Lea.Tests/PrintWpfLayoutMain.cs','Lea.Build/Prepare-PrintPreview.ps1','Lea.Build/Test-PrintPreview.generated.ps1','continuity-evidence/print-preview-preparation-v1/result.json')){$null=$paths.Add((Join-Path $workspace $relative))}
}
if($Stage -eq 'Host'){
    foreach($relative in @('Lea.Print/Pw.cs','Lea.Print/Pf.cs','Lea.Print/Pp.cs','Lea.Print/Px.cs','Lea.Print/Pv.cs','Lea.Print/Ph.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintHostMain.cs','Lea.Tests/PrintPreviewMain.cs','Lea.Tests/PrintXpsMain.cs','Lea.Tests/PrintPaginatorMain.cs','Lea.Tests/PrintFrameMain.cs','Lea.Tests/PrintWpfLayoutMain.cs','Lea.Build/Prepare-PrintHost.ps1','Lea.Build/Test-PrintHost.generated.ps1','continuity-evidence/print-host-preparation-v1/result.json')){$null=$paths.Add((Join-Path $workspace $relative))}
}
if($Stage -eq 'Document'){
    foreach($relative in @('Lea.Print/Pw.cs','Lea.Print/Pm.cs','Lea.Print/Pf.cs','Lea.Print/Pp.cs','Lea.Print/Px.cs','Lea.Print/Pv.cs','Lea.Print/Ph.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintDocumentMain.cs','Lea.Tests/PrintHostMain.cs','Lea.Tests/PrintPreviewMain.cs','Lea.Tests/PrintXpsMain.cs','Lea.Tests/PrintPaginatorMain.cs','Lea.Tests/PrintFrameMain.cs','Lea.Tests/PrintWpfLayoutMain.cs','LeaForge/test-pages/print-document.html','Lea.Build/Prepare-PrintDocument.ps1','Lea.Build/Prepare-PrintDocumentV2.ps1','Lea.Build/Test-PrintDocument.generated.ps1','Lea.Build/Test-PrintDocumentV2.generated.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
    foreach($folder in @('print-document-v1','print-document-preparation-v1','print-document-preparation-v2')){foreach($file in Get-ChildItem (Join-Path $workspace ('continuity-evidence/'+$folder)) -File){$null=$paths.Add($file.FullName)}}
}
if($Stage -eq 'Styles'){
    foreach($relative in @('Lea.Print/Pw.cs','Lea.Print/Pm.cs','Lea.Print/Pf.cs','Lea.Print/Pp.cs','Lea.Print/Px.cs','Lea.Print/Pv.cs','Lea.Print/Ph.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintStylesMain.cs','Lea.Tests/PrintDocumentMain.cs','Lea.Tests/PrintHostMain.cs','Lea.Tests/PrintPreviewMain.cs','Lea.Tests/PrintXpsMain.cs','Lea.Tests/PrintPaginatorMain.cs','Lea.Tests/PrintFrameMain.cs','Lea.Tests/PrintWpfLayoutMain.cs','LeaForge/test-pages/print-styles.html','LeaForge/test-pages/print-document.html','Lea.Build/PrintStyles.fragment.cs','Lea.Build/PrintFontSize.fragment.cs','Lea.Build/Prepare-PrintStylesCore.ps1','Lea.Build/Build-PrintStylesCore.ps1','Lea.Build/Prepare-PrintStyles.ps1','Lea.Build/Prepare-PrintStylesV2.ps1','Lea.Build/Prepare-PrintStylesV3.ps1','Lea.Build/Prepare-PrintStylesNative.ps1','Lea.Build/Test-PrintStyles.generated.ps1','Lea.Build/Test-PrintStylesV2.generated.ps1','Lea.Build/Test-PrintStylesV3.generated.ps1','Lea.Build/Test-PrintStylesNative.generated.ps1','Lea.Build/Audit-PrintStylesNative.ps1','Lea.Build/Record-PrintStylesOutcome.ps1')){$null=$paths.Add((Join-Path $workspace $relative))}
    foreach($folder in @('print-styles-v1','print-styles-v2','print-style-core-source-v1','print-style-core-source-v2','print-style-core-source-v3','print-styles-preparation-v1','print-styles-preparation-v2','print-styles-preparation-v3','print-styles-native-preparation-v1','print-styles-native-v1')){foreach($file in Get-ChildItem (Join-Path $workspace ('continuity-evidence/'+$folder)) -Recurse -File){$null=$paths.Add($file.FullName)}}
}
if($Stage -eq 'StyledDocument'){
    foreach($relative in @('Lea.Print/Py.cs','Lea.Print/Pz.cs','Lea.Print/Pw.cs','Lea.Print/Pm.cs','Lea.Print/Pf.cs','Lea.Print/Pp.cs','Lea.Print/Px.cs','Lea.Print/Pv.cs','Lea.Print/Ph.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintStyledDocumentMain.cs','Lea.Tests/PrintStylesMain.cs','Lea.Tests/PrintDocumentMain.cs','Lea.Tests/PrintHostMain.cs','Lea.Tests/PrintPreviewMain.cs','Lea.Tests/PrintXpsMain.cs','Lea.Tests/PrintPaginatorMain.cs','Lea.Tests/PrintFrameMain.cs','Lea.Tests/PrintWpfLayoutMain.cs','LeaForge/test-pages/print-styled-document.html','LeaForge/test-pages/print-styles.html','LeaForge/test-pages/print-document.html','Lea.Build/Prepare-PrintStyledDocument.ps1','Lea.Build/Prepare-PrintStyledDocumentV2.ps1','Lea.Build/Test-PrintStyledDocument.generated.ps1','Lea.Build/Test-PrintStyledDocumentV2.generated.ps1','continuity-evidence/print-styles-v3/result.json','continuity-evidence/print-styles-native-v1/bound-outcome.json')){$null=$paths.Add((Join-Path $workspace $relative))}
    foreach($folder in @('print-styled-document-v1','print-styled-document-preparation-v1','print-styled-document-preparation-v2')){foreach($file in Get-ChildItem (Join-Path $workspace ('continuity-evidence/'+$folder)) -File){$null=$paths.Add($file.FullName)}}
}
if($Stage -eq 'LiveCapture'){
    foreach($relative in @('Lea.Print/Py.cs','Lea.Print/Pz.cs','Lea.Print/Pw.cs','Lea.Print/Pm.cs','Lea.Print/Pf.cs','Lea.Print/Pp.cs','Lea.Print/Px.cs','Lea.Print/Pv.cs','Lea.Print/Ph.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Print/Pt.cs','Lea.Tests/PrintLiveCaptureMain.cs','Lea.Tests/PrintStyledDocumentMain.cs','Lea.Tests/PrintStylesMain.cs','Lea.Tests/PrintDocumentMain.cs','Lea.Tests/PrintHostMain.cs','Lea.Tests/PrintPreviewMain.cs','Lea.Tests/PrintXpsMain.cs','Lea.Tests/PrintPaginatorMain.cs','Lea.Tests/PrintFrameMain.cs','Lea.Tests/PrintWpfLayoutMain.cs','LeaForge/test-pages/print-styled-document.html','LeaForge/test-pages/print-styles.html','LeaForge/test-pages/print-document.html','Lea.Build/PrintViewStyles.fragment.cs','Lea.Build/Prepare-PrintViewStyles.ps1','Lea.Build/Build-PrintViewStyles.ps1','Lea.Build/Prepare-PrintLiveCapture.ps1','Lea.Build/Test-PrintLiveCapture.generated.ps1','Lea.Build/Prepare-PrintLiveNative.ps1','Lea.Build/Test-PrintLiveNative.generated.ps1','Lea.Build/Audit-PrintLiveNative.ps1','Lea.Build/Record-PrintLiveOutcome.ps1','LeaForge/history/PROJECT_CONTEXT_20261027_before_live_capture_consolidation.md')){$null=$paths.Add((Join-Path $workspace $relative))}
    foreach($folder in @('print-view-styles-source-v1','print-live-capture-preparation-v1','print-live-native-preparation-v1','print-live-native-v1')){foreach($file in Get-ChildItem (Join-Path $workspace ('continuity-evidence/'+$folder)) -Recurse -File){$null=$paths.Add($file.FullName)}}
}
$proof=Get-Content (Join-Path $root 'result.json') -Raw|ConvertFrom-Json
if(!$proof.passed -or $proof.ownerAttention){throw 'Framework proof required'}
foreach($input in $proof.inputs){if([IO.Path]::GetExtension($input.path) -in @('.cs','.ps1','.rsp')){$null=$paths.Add($input.path)}}
$leases=[Collections.Generic.List[IDisposable]]::new();$members=[Collections.Generic.List[object]]::new()
try{
    foreach($path in $paths){
        if(!$path.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Path outside workspace'}
        $s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s)
        $members.Add(@{entry=[IO.Path]::GetRelativePath($workspace,$path).Replace('\','/');sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));stream=$s});$s.Position=0
    }
    $zip=[IO.Compression.ZipFile]::Open($archive,'Create')
    try{
        foreach($m in $members){$s=$zip.CreateEntry($m.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$m.stream.CopyTo($s)}finally{$s.Dispose()}}
        $w=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$w.Write((@{scope='Local Framework print library/test source, exact binaries, recipes and owned execution evidence. Compiler/framework references remain hash-recorded external inputs, not portable SDK/off-device backup or printer/host qualification.';members=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})}|ConvertTo-Json -Depth 5))}finally{$w.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Inventory differs'}
        foreach($m in $members){$s=$zip.GetEntry($m.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $m.sha256){throw 'Readback differs'}}finally{$s.Dispose()}}
    }finally{$zip.Dispose()}
    @{verified=$true;entries=$members.Count+1;bytes=(Get-Item $archive).Length;sha256=(Get-FileHash $archive).Hash}|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
