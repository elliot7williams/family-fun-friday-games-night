# Lea.Build

These files are fallback build sources for machines that have the old Windows C# compiler but do not have the modern .NET SDK.

The compiled test executables go in:

```text
dist/LeaExes
```

They are .NET Framework executables:

- `LeaMicro.exe`
- `LeaBrowser.exe`
- `LeaPlus.exe`
- `LeaForge.exe`

Each exe is compiled with `../assets/lea.ico` embedded as its Windows icon.

The cleaner source projects are still:

- `Lea.Core`
- `Lea`
- `LeaBrowser`
- `Lea.Micro`
- `LeaPlus`
- `LeaForge`

`LeaForge.exe` is compiled directly from `../LeaForge/Program.cs` because that file is intentionally compatible with the older Windows C# compiler.

## Source Backups

Run `Backup-Lea.ps1` to create a dated, source-only ZIP in the Documents
`Lea Source Backups` folder. Git and GitHub remain the main version history;
the ZIP is an additional local copy without `bin`, `obj`, or `dist` files.

Run `Backup-Firespider.ps1` to create the matching source-only host ZIP from
`L:\Firespider Internet` in the Documents `Firespider Source Backups` folder.
It excludes Git/editor/build/cache output and compiled binaries while retaining
the VB project, Lea adapter, resources, configuration, installer source, and
design assets. Pass `-Source` only when the host checkout moves.

## Compatibility And Performance

`Lea.Bench` is the repeatable .NET Framework report runner. After a Release
build, run it from the repository root:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -r . -o .\LeaForge\COMPATIBILITY_PERFORMANCE.md -n 15
```

Its compact switches are `-r` for repository root, `-o` for report output,
`-n` for iterations, and optional `-b` for a Firespider executable path.
The same runner recognizes an extracted Lea runtime package and measures its
local DLLs and Firespider executable without requiring a source solution.

## Firespider Startup Checks

Run the executable checks with Windows PowerShell and the built host root:

```powershell
powershell.exe -NoProfile -STA -File .\Lea.Build\Test-FirespiderStartup.ps1 -Root 'L:\Firespider Internet'
```

The default checks clean, restored, locked, and oversized temporary sessions,
waits eight seconds per launch, and inspects responsiveness and UI Automation
for exception dialogs. Use `-Scenario clean` for one launch. It never closes
an existing browser process, and restores the session environment override.
The browser can still update its ordinary profile settings when it closes;
use an extracted runtime test directory for isolated package checks.

The compiled `Lea.Firespider.Smoke.exe <host-root> --startup` runs eighteen
event/session regressions off-screen. Omitting `--startup` runs the full host
suite. Source for the corresponding host edits is retained in
`Lea.Firespider/Startup-host.patch`, with reproduction notes in
`LeaForge/STARTUP_RELIABILITY.md`.
