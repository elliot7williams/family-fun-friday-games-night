param(
    [string]$Version = "1.0.0.0",
    [string]$Publisher = "CN=Elliot",
    [string]$Output = ""
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$PackageRoot = Join-Path $Root "PackageRoot"
$Manifest = Join-Path $PackageRoot "Package.appxmanifest"

if (-not $Output) {
    $OutputDir = Join-Path $Root "Output"
    New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
    $Output = Join-Path $OutputDir "BubbleWallpaper.msix"
}

$makeAppx = Get-Command makeappx.exe -ErrorAction SilentlyContinue
if (-not $makeAppx) {
    $sdkBin = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse -Filter makeappx.exe -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending |
        Select-Object -First 1
    if ($sdkBin) {
        $makeAppx = $sdkBin
    }
}

if (-not $makeAppx) {
    throw "makeappx.exe was not found. Install the Windows SDK, then run this script again."
}

[xml]$xml = Get-Content $Manifest
$xml.Package.Identity.Version = $Version
$xml.Package.Identity.Publisher = $Publisher
$xml.Save($Manifest)

& $makeAppx.Source pack /d $PackageRoot /p $Output /o
Write-Host "Created $Output"
