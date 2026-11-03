$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$root=Join-Path $workspace 'continuity-evidence/library-navigation-v1/host-source'
$out=Join-Path $workspace 'continuity-evidence/legacy-host-tools-audit-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve previous audit'}
$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Expected){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($Expected -and $hash -cne $Expected){throw ('Input differs: '+$Path)}
    return $hash
}
function Method([string]$Text,[string]$Name){
    $pattern='(?ms)^    (?:Friend |Private |Public )?(?:Sub|Function) '+[regex]::Escape($Name)+'\([^\r\n]*\r?\n.*?^    End (?:Sub|Function)\r?$'
    $matches=[regex]::Matches($Text,$pattern)
    if($matches.Count -ne 1){throw ('Unique method required: '+$Name)}
    return $matches[0].Value
}
try{
    $checkpoint=Join-Path $workspace 'continuity-evidence/library-and-trace-source-20261026.zip'
    $null=Hold $checkpoint 'A1515801B6A9696951BA07F874CAAD976E4A7896DAAD18F85D8138F6B8E38AC0'
    $zip=[IO.Compression.ZipFile]::OpenRead($checkpoint)
    try{
        $reader=[IO.StreamReader]::new($zip.GetEntry('manifest.json').Open())
        try{$manifest=$reader.ReadToEnd()|ConvertFrom-Json}finally{$reader.Dispose()}
        $prefix='continuity-evidence/library-navigation-v1/host-source/'
        $pins=@($manifest.members|Where-Object{$_.entry.StartsWith($prefix,[StringComparison]::Ordinal)})
        if($pins.Count -ne 104){throw 'Matching archived non-obj source inventory differs'}
        $current=@(Get-ChildItem -LiteralPath $root -Recurse -File|Where-Object{$_.FullName -notmatch '\\obj\\'})
        if($current.Count -ne $pins.Count){throw 'Current non-obj inventory differs'}
        foreach($pin in $pins){$null=Hold (Join-Path $workspace $pin.entry) $pin.sha256}
    }finally{$zip.Dispose()}
    $exe=Join-Path $workspace 'continuity-evidence/library-navigation-v1/runtime/Firespider Internet 19.exe'
    $exeHash=Hold $exe '638F46BD41B791001861E1AEB50DE9A047277F4BAE475702E903E0EF4CA3A5D5'
    $features=[IO.File]::ReadAllText((Join-Path $root 'BrowserFeatures5.vb'))
    $theme=[IO.File]::ReadAllText((Join-Path $root 'BrowserFeatures10.vb'))
    $routes=[IO.File]::ReadAllText((Join-Path $root 'FirespiderLea.vb'))
    $print=Method $features 'q945'
    $color=Method $theme 'qsitechromecolor'
    $selector=Method $theme 'qthemebrowser'
    $registrations=Method $features 'qfeatures5'
    foreach($anchor in @('AddressOf q945, AddressOf q946','qfeatures10()')){
        if(!$registrations.Contains($anchor)){throw 'Tool registration differs'}
    }
    if(!$print.Contains('Dim source As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)') -or
       !$print.Contains('If source Is Nothing OrElse source.Document Is Nothing OrElse source.Document.Body Is Nothing Then') -or
       !$print.Contains('The current page is not ready to print') -or $print.Contains('qlea')){throw 'Print boundary differs'}
    if(!$selector.Contains('Return TryCast(k.SelectedTab.Controls(0), WebBrowser)') -or
       !$color.Contains('If browser Is Nothing OrElse browser.Document Is Nothing OrElse browser.Document.Body Is Nothing Then Return Color.Empty') -or
       $selector.Contains('qlea') -or $color.Contains('qlea')){throw 'Theme boundary differs'}
    if($routes.Contains('q945') -or $routes.Contains('qsitechromecolor') -or $routes.Contains('qthemebrowser')){throw 'New route requires review'}
    $controls=[Collections.Generic.List[object]]::new()
    foreach($text in @(($features.Replace('Sub q945(', 'Sub renamed(')),($features+[Environment]::NewLine+$print))){
        $rejected=$false;try{$null=Method $text 'q945'}catch{$rejected=$_.Exception.Message -ceq 'Unique method required: q945'}
        if(!$rejected){throw 'Method inventory negative control failed'}
        $controls.Add(@{name=if($controls.Count){'duplicate-method'}else{'missing-method'};passed=$true})
    }
    $rows=@(
        @{feature='Printer-friendly page';entry='q945/q946';status='source-proven missing Lea dispatch';reason='Lea V fails TryCast to WebBrowser and takes not-ready return before options/output';scope='No printer output, dialog, selection or printer device was exercised'},
        @{feature='Current-page toolbar color';entry='qusecurrentpagecolor/qthemechanged';status='source-proven missing Lea document access';reason='qthemebrowser returns Nothing for Lea V; qsitechromecolor returns Color.Empty';scope='No rendered toolbar or color-parser compatibility was exercised'}
    )
    $inventory=@(Get-ChildItem -LiteralPath $root -Filter '*.vb' -File|ForEach-Object{
        $file=$_
        Select-String -LiteralPath $file.FullName -Pattern '\bWebBrowser\b'|ForEach-Object{
            @{file=$file.Name;line=$_.LineNumber;text=$_.Line.Trim();classification='reference only; reachability and migration status not inferred'}
        }
    })
    $null=New-Item -ItemType Directory -Path $out
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'audit.ps1')
    $result=@{auditPassed=$true;sourceFilesVerified=$pins.Count;sourceCheckpointSha256=(Get-FileHash $checkpoint).Hash;executableSha256=$exeHash;findings=$rows;negativeControls=$controls;legacyReferences=$inventory;runtimeExecuted=$false;productionModified=$false;releaseQualified=$false;nextAction='Implement bounded Lea document color lookup without eval or remote scripting; preserve legacy fallback and verify against the actual host'}
    $result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'result.json')
    $result|Select-Object auditPassed,sourceFilesVerified,findings,runtimeExecuted|ConvertTo-Json -Depth 5
}finally{foreach($s in $leases){$s.Dispose()}}
