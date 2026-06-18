# Bubble Wallpaper MSIX Package

This folder is ready to build a Windows MSIX package for Bubble Wallpaper.

## Contents

- `PackageRoot/Package.appxmanifest`: MSIX manifest for the packaged app.
- `PackageRoot/Bubble Wallpaper Standalone.exe`: the app executable.
- `PackageRoot/Assets`: Windows tile, store, splash, and taskbar assets.
- `PackageRoot/BubbleWallpaperApp`: the original HTML/CSS/JS app assets included for packaging completeness.
- `build-msix.ps1`: builds `Output/BubbleWallpaper.msix` with `makeappx.exe`.
- `sign-msix.ps1`: signs the MSIX with `signtool.exe`.

## Build

Install the Windows SDK if `makeappx.exe` and `signtool.exe` are not already available.

```powershell
powershell -ExecutionPolicy Bypass -File .\build-msix.ps1
```

## Signing

MSIX packages must be signed before installation. The certificate subject must match the manifest publisher, currently `CN=Elliot`.

```powershell
powershell -ExecutionPolicy Bypass -File .\sign-msix.ps1 -Certificate C:\Path\To\Certificate.pfx
```

If you use a different certificate subject, rebuild with the matching publisher:

```powershell
powershell -ExecutionPolicy Bypass -File .\build-msix.ps1 -Publisher "CN=YourPublisherName"
```
