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
