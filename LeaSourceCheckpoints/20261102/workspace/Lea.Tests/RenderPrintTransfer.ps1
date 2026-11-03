param([Parameter(Mandatory)][string]$Source, [Parameter(Mandatory)][string]$Returned, [Parameter(Mandatory)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
if ([Threading.Thread]::CurrentThread.ApartmentState -ne 'STA') { throw 'STA required' }
Add-Type -AssemblyName PresentationCore,PresentationFramework,WindowsBase,ReachFramework
Add-Type -TypeDefinition @'
public static class LeaPixelDifference {
    public static long Count(byte[] a, byte[] b) {
        if (a.Length != b.Length) throw new System.ArgumentException("Pixel buffers differ");
        long count = 0;
        for (int i = 0; i < a.Length; i += 4)
            if (a[i] != b[i] || a[i+1] != b[i+1] || a[i+2] != b[i+2] || a[i+3] != b[i+3]) count++;
        return count;
    }
    public static long Visible(byte[] a) {
        long count = 0;
        for (int i = 0; i < a.Length; i += 4)
            if (a[i+3] != 0 && (a[i] < 245 || a[i+1] < 245 || a[i+2] < 245)) count++;
        return count;
    }
}
'@
function Render($paginator, [int]$index, [string]$prefix) {
    $page = $paginator.GetPage($index)
    try {
        $width = [int][Math]::Ceiling($page.Size.Width); $height = [int][Math]::Ceiling($page.Size.Height)
        if ($width -lt 1 -or $height -lt 1 -or $width -gt 2048 -or $height -gt 2048) { throw 'Bounded page dimensions required' }
        $bitmap = [Windows.Media.Imaging.RenderTargetBitmap]::new($width, $height, 96, 96, [Windows.Media.PixelFormats]::Pbgra32)
        $bitmap.Render($page.Visual)
        $pixels = [byte[]]::new($width * $height * 4)
        $bitmap.CopyPixels($pixels, $width * 4, 0)
        $encoder = [Windows.Media.Imaging.PngBitmapEncoder]::new()
        $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
        $path = Join-Path $OutputDirectory ($prefix + '-' + ($index+1) + '.png')
        $stream = [IO.File]::Open($path, 'CreateNew', 'Write', 'Read')
        try { $encoder.Save($stream) } finally { $stream.Dispose() }
        return @{ pixels = $pixels; width = $width; height = $height; path = $path; visible = [LeaPixelDifference]::Visible($pixels) }
    } finally { $page.Dispose() }
}
$a = $null; $b = $null
try {
    $a = [Windows.Xps.Packaging.XpsDocument]::new($Source, [IO.FileAccess]::Read)
    $b = [Windows.Xps.Packaging.XpsDocument]::new($Returned, [IO.FileAccess]::Read)
    $left = $a.GetFixedDocumentSequence().DocumentPaginator
    $right = $b.GetFixedDocumentSequence().DocumentPaginator
    $left.ComputePageCount(); $right.ComputePageCount()
    if ($left.PageCount -ne 3 -or $right.PageCount -ne 3) { throw 'Expected three pages' }
    $rows = @()
    for ($i = 0; $i -lt 3; $i++) {
        $l = Render $left $i 'source'; $r = Render $right $i 'returned'
        if ($l.width -ne $r.width -or $l.height -ne $r.height -or !$l.visible -or !$r.visible) { throw 'Blank or mismatched render' }
        $rows += @{ page = $i+1; width = $l.width; height = $l.height; sourceVisiblePixels = $l.visible;
            returnedVisiblePixels = $r.visible; differingPixels = [LeaPixelDifference]::Count($l.pixels, $r.pixels) }
    }
    $result = @{ rendered = $true; pages = $rows; printJobSubmitted = $false; pixelIdentical = @($rows | Where-Object differingPixels -ne 0).Count -eq 0 }
    [IO.File]::WriteAllText((Join-Path $OutputDirectory 'render.json'), ($result | ConvertTo-Json -Depth 6))
    $result | ConvertTo-Json -Depth 6
} finally { if ($a) { $a.Close() }; if ($b) { $b.Close() } }
