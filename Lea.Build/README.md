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

Each exe is compiled with `../assets/lea.ico` embedded as its Windows icon.

The cleaner source projects are still:

- `Lea.Core`
- `Lea`
- `LeaBrowser`
- `Lea.Micro`
- `LeaPlus`
