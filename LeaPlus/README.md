# LeaPlus

LeaPlus is the larger experiment for Lea.

It tries more browser features than `LeaBrowser`:

- basic HTML elements: headings, paragraphs, lists, links, buttons, images
- simple CSS: body background, text color, font size, inline color
- tiny JavaScript-style support: `document.title = "..."` and `alert("...")` on buttons
- visual page rendering with Windows Forms controls

This is still not Chrome. It is a small test engine so we can experiment with how much Lea can understand before it stops feeling tiny.

## Run

Open `../Lea.sln` in Visual Studio and set `LeaPlus` as the startup project.

With the .NET SDK installed:

```powershell
dotnet run --project LeaPlus
```
