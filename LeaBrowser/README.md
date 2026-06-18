# LeaBrowser

LeaBrowser is a tiny Windows Forms test browser for the Lea engine.

It uses `Lea.Core` directly. That means the window is only a shell around the small engine:

- address box
- back and forward buttons
- reload button
- readable page text
- extracted page links

## Run

Open `../Lea.sln` in Visual Studio and set `LeaBrowser` as the startup project.

Or from a terminal with the .NET SDK installed:

```powershell
dotnet run --project LeaBrowser
```

## Test Flow

1. Type `example.com`.
2. Press Enter or click `Go`.
3. Double-click a link in the right panel to follow it.
4. Use `<`, `>`, and `R` to test history and reload.
