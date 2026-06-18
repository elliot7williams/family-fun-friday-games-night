param(
    [string]$Package = "",
    [string]$Certificate = "",
    [string]$Password = ""
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $Package) {
    $Package = Join-Path $Root "Output\BubbleWallpaper.msix"
}

if (-not $Certificate) {
    throw "Provide a .pfx certificate path with -Certificate. Its subject must match the Publisher in Package.appxmanifest."
}

$signTool = Get-Command signtool.exe -ErrorAction SilentlyContinue
if (-not $signTool) {
    $sdkBin = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending |
        Select-Object -First 1
    if ($sdkBin) {
        $signTool = $sdkBin
    }
}

if (-not $signTool) {
    throw "signtool.exe was not found. Install the Windows SDK, then run this script again."
}

$args = @("sign", "/fd", "SHA256", "/f", $Certificate)
if ($Password) {
    $args += @("/p", $Password)
}
$args += $Package

& $signTool.Source @args
Write-Host "Signed $Package"
