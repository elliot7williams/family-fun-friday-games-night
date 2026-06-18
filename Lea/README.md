# Lea

Lea is a very small toy web browser engine written in C#.

It is not a full Chrome/Safari-style engine. It is a short, basic reader:

- fetches a web page
- parses the page title
- extracts links
- strips HTML into readable text
- keeps simple back/forward history
- uses tiny letter commands

## Open in Visual Studio

Open `../Lea.sln` in Visual Studio.

- Run `Lea` to test the engine in the short-command console.
- Run `LeaBrowser` to test the engine in a small browser window.
- Run `Lea.Micro` to test the compressed one-file experiment.
- Run `LeaPlus` to test more HTML, CSS, and tiny JavaScript-style support.

## Icon

Lea uses the shared icon files in `../assets`:

- `lea-icon.svg` is the editable vector source.
- `lea.ico` is the Windows app icon used by the projects and compiled test executables.

You can also run it from a terminal:

```powershell
dotnet run --project Lea
```

Run a short-code script:

```powershell
dotnet run --project Lea -- Lea/sample.lea
```

## Commands

```text
u site   open URL
o site   open URL
g n      go to link number
b        back
f        forward
r        reload
l        show links
h        help
q        quit
```

Example:

```text
lea> u example.com
lea> l
lea> g 1
lea> b
```

## How the short code works

Lea maps short letters to browser functions:

| Letter | Function |
| --- | --- |
| `u` | open a URL |
| `g` | follow a numbered link |
| `b` | go back |
| `f` | go forward |
| `r` | reload |
| `l` | list links |
| `h` | help |
| `q` | quit |

Script lines use the same commands:

```text
u example.com
l
g 1
b
q
```

## Next ideas

- add tabs with `t`
- add bookmarks with `m`
- add a tiny CSS parser
- add a simple page cache
- add a custom `.lea.html` page format for very fast Lea-native pages
- compare `Lea.Core` and `Lea.Micro` startup time and file size
