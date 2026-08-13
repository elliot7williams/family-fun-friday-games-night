# LeaForge

LeaForge is a new, bigger browser-engine experiment for Lea.

The earlier projects stay small:

- `Lea.Micro` is the compressed tiny version.
- `LeaBrowser` is the simple test window.
- `LeaPlus` adds more HTML/CSS/script tests.

LeaForge is where we can grow the more full-featured browser.

## Current Features

- tabs
- back and forward history per tab
- reload
- bookmarks
- source viewer
- local HTML file loading
- basic HTTP page loading
- bounded in-memory page cache for faster repeat navigation
- basic HTML rendering for headings, paragraphs, lists, links, buttons, images, code, pre blocks, and structured tables
- wrapped inline bold/italic text, styled spans, line breaks, and links inside text blocks
- inherited font-family fallbacks plus underline and line-through decoration
- inherited left/center/right/justify alignment and pixel/relative line spacing
- external stylesheets with document-order cascade and shared bounded caching
- compiled relationship, attribute, structural, negation, link, and form-state selectors
- live `:hover` and `:focus` restyling that preserves mutable form values
- min/max dimensions plus normal, break-word, and break-all text flow
- inherited visibility and effective opacity across text, boxes, controls, and images
- responsive `<picture>`/`srcset` selection and bounded base64 embedded images
- compact CSS cascade for colors, fonts, boxes, overflow, and inline styles
- root-level relative, absolute, and fixed positioning with z-index and clipping
- tiny script handling for `alert("...")` and `document.title = "..."`

## Lea Short Commands

LeaForge accepts compact aliases for common page commands:

```text
t("Title")       title
a("Message")     alert
bg("#fff")       background
fg("#222")       text color
fs(12)           font size
```

The longer `alert(...)` and `document.title = ...` forms still work. The short
forms are intended for small, fast Lea pages and are limited to simple values.

## Compact Engine API

Small C# programs can use the short engine facade:

```csharp
var e = new LeaForge.E();
var p = e.Go("test-pages/short.html");
Console.WriteLine(p.T);
Console.WriteLine(p.N + " nodes, " + p.Ls + " links");
```

The main actions are `Go`, `Bk`, `Fw`, `Rl`, and `At`. Page values are shortened
to `U` (URL), `T` (title), `H` (HTML), `N` (node count), and `Ls` (link count).

Alias naming is a core Lea rule. See [ALIASES.md](ALIASES.md) for the vocabulary
and limits used by future engine features.

Normal navigation reuses up to 12 recently parsed pages. The Reload button bypasses
that cache and fetches the current page again.

## Run

Open `../Lea.sln` in Visual Studio after `LeaForge` is added to it, or compile the fallback executable from `Lea.Build`.

The compiled test exe is expected at:

```text
../dist/LeaExes/LeaForge.exe
```

## Local Test Page

Open this path in LeaForge:

```text
LeaForge/test-pages/index.html
```

## Important

LeaForge is still our own experimental renderer. It does not use Chromium, Edge WebView, or a full JavaScript VM.
