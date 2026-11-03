$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$receipt = Join-Path $root 'continuity-evidence/print-package-cancel-v5/result.json'
if ((Get-FileHash $receipt).Hash -cne 'F7DDA9DBBBA0AE6375B628942A7EB8EC0D26D9ED7629F93847E42D3F2644270B') { throw 'Submission receipt differs' }
$r = Get-Content $receipt -Raw | ConvertFrom-Json
if (!$r.nativeCompleted -or $r.nativeStatus.Completion -ne 1 -or $r.nativeStatus.PackageStatusCode -ne 0 -or $r.ownerAttention) { throw 'Original target completion incomplete' }
$source = Join-Path $root 'continuity-evidence/print-ticket-project-v1/bin/prepared-ticket.xps'
$output = Join-Path $root 'continuity-evidence/print-package-cancel-v5/completed-output.xps'
$pin = @($r.inputs | Where-Object path -eq $source)
if ($pin.Count -ne 1 -or (Get-FileHash $source).Hash -cne $pin[0].sha256 -or (Get-FileHash $output).Hash -cne $r.completedOutputSha256) { throw 'Package pin differs' }
Add-Type -AssemblyName System.IO.Compression
function Xml($entry) {
    if (!$entry -or $entry.Length -gt 2MB) { throw 'Bounded XML entry required' }
    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = 'Prohibit'; $settings.XmlResolver = $null; $settings.MaxCharactersInDocument = 2MB
    $stream = $entry.Open(); $reader = $null
    try {
        $reader = [Xml.XmlReader]::Create($stream, $settings)
        $doc = [Xml.XmlDocument]::new(); $doc.XmlResolver = $null; $doc.Load($reader)
        return ,$doc
    } finally { if ($reader) { $reader.Dispose() }; $stream.Dispose() }
}
$a = [IO.Compression.ZipFile]::OpenRead($source)
$b = [IO.Compression.ZipFile]::OpenRead($output)
try {
    $pages = @($a.Entries | Where-Object FullName -Match '\.fpage$' | Sort-Object FullName)
    if ($pages.Count -ne 3 -or @($b.Entries | Where-Object FullName -Match '\.fpage$').Count -ne 3) { throw 'Page inventory differs' }
    $glyphCount = 0
    foreach ($page in $pages) {
        $left = Xml $page; $right = Xml ($b.GetEntry($page.FullName))
        foreach ($dimension in @('Width','Height')) {
            $x = [double]::Parse($left.DocumentElement.GetAttribute($dimension), [Globalization.CultureInfo]::InvariantCulture)
            $y = [double]::Parse($right.DocumentElement.GetAttribute($dimension), [Globalization.CultureInfo]::InvariantCulture)
            if (![double]::IsFinite($x) -or ![double]::IsFinite($y) -or [Math]::Abs($x-$y) -gt 0.000001) { throw 'Page geometry differs' }
        }
        $lg = @($left.GetElementsByTagName('Glyphs','http://schemas.microsoft.com/xps/2005/06'))
        $rg = @($right.GetElementsByTagName('Glyphs','http://schemas.microsoft.com/xps/2005/06'))
        if (!$lg.Count -or $lg.Count -ne $rg.Count) { throw 'Glyph inventory differs' }
        for ($i = 0; $i -lt $lg.Count; $i++) {
            if ($lg[$i].GetAttribute('UnicodeString') -cne $rg[$i].GetAttribute('UnicodeString')) { throw 'Glyph text differs' }
        }
        $glyphCount += $lg.Count
    }
    $fonts = @($a.Entries | Where-Object FullName -Match '\.odttf$')
    if (!$fonts.Count) { throw 'Font fixture missing' }
    foreach ($font in $fonts) {
        $other = $b.GetEntry($font.FullName)
        if (!$other -or $font.Length -ne $other.Length -or $font.Length -gt 4MB) { throw 'Font inventory differs' }
        $sa = $font.Open(); $sb = $other.Open()
        try {
            if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($sa)) -cne [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($sb))) { throw 'Font bytes differ' }
        } finally { $sa.Dispose(); $sb.Dispose() }
    }
    $result = [ordered]@{ readbackPassed = $true; pages = $pages.Count; glyphRuns = $glyphCount; fontParts = $fonts.Count;
        submissionReceiptSha256 = (Get-FileHash $receipt).Hash; outputSha256 = (Get-FileHash $output).Hash;
        defaultPrinterSafeguardPassed = !$r.defaultPrinterMutated; overallQualificationPassed = $false;
        scope = 'Page dimensions, ordered glyph text and embedded font bytes only; no render/layout equivalence or lifecycle fault qualification' }
    $path = Join-Path $root 'continuity-evidence/print-package-cancel-v5/output-readback.json'
    if (Test-Path $path) { throw 'Preserve existing audit' }
    [IO.File]::WriteAllText($path, ($result | ConvertTo-Json))
    $result | ConvertTo-Json
} finally { $a.Dispose(); $b.Dispose() }
