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
- basic HTML rendering for headings, paragraphs, bounded unordered/decimal lists, details disclosures, links, buttons, images, code, pre blocks, and structured tables
- page-local `<details>`/`<summary>` state with compact markers, nested retention, pointer activation, and Enter/Space keyboard activation
- bounded quote-aware HTML tokenization with comments, declarations, raw text, RCDATA, entities, and malformed-delimiter recovery
- compact `Id`, `Q`/`Q1`, and read-only bounded `Qa` document queries
- bounded `<base href>` resolution through compact `Bu`
- retained same-page fragments through compact `Ft`/`Fg`/`Rv`, 128-entry history, percent-decoded IDs, named anchors, details reveal, and resize-safe scrolling
- custom `<progress>` and `<meter>` painting through compact bounded `B.Vm` values, meter quality regions, inherited styles, and table-cell reflow
- compact `<output>` and `<data>` values through `P.Dvs`/`B.Dv`/`Ir.Dv`, bounded metadata, safe updates, and inline/table ownership
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
- auto-height styled semantic containers with nested normal-flow ownership
- nested positioning with explicit z-index, opacity/isolation contexts, settled auto height, and inherited clipping
- compact inset stretching plus translated block stacking contexts
- nested fields, responsive images, and tables with clip-aware native editing
- bounded multipart file uploads plus range, color, and temporal form fields
- tiny script handling for `alert("...")` and `document.title = "..."`
- bounded engine scripting for title, body colors, font size, and host-routed alerts
- safe button/link commands with bounded variables and ID-scoped text updates
- bounded `ac`/`rc`/`tc` class mutations with compiled CSS restyling
- bounded developer snapshots plus a tabbed inspector in the test browser
- repeatable compatibility, timing, memory, binary-size, and compression report
- streamed local/HTTP downloads with explicit destinations and a 64 MB ceiling
- highlighted, case-insensitive find-in-page with next/previous cycling

## Lea Short Commands

LeaForge accepts compact aliases for common page commands:

```text
t("Title")       title
a("Message")     alert
bg("#fff")       background
fg("#222")       text color
fs(12)           font size
v("name", "Lea") variable
tx("id", "Hi ${name}") element text
ac("id", "hot")       add class
rc("id", "hot")       remove class
tc("id", "hot")       toggle class
go("next.html")        request navigation from the host
```

The longer `alert(...)` and `document.title = ...` forms still work. The short
forms are intended for small, fast Lea pages and are limited to simple values.
`go` never navigates the engine directly: it queues one bounded HTTP, HTTPS, or
safe local-file request for the browser host to approve and load.

The .NET Framework test browser opens Lea Inspector with `F12`, `Ctrl+Shift+I`,
or the `I` toolbar button. Its Summary, Nodes, CSS, Requests, and Messages tabs
use the compact `Dg` API and never inspect private renderer fields.
Firespider's experimental Lea tabs expose the same bounded inspector from the
View menu (`F12`) and the tab context menu, with refresh and request-log clearing.
Firespider's normal New Tab command and `Ctrl+T` now open Lea; **New Legacy Tab**
(`Ctrl+Alt+L`) keeps the original engine available for unmigrated features.

## Compact Engine API

Small C# programs can use the short engine facade:

```csharp
var e = new LeaForge.E();
var p = e.Go("test-pages/short.html");
Console.WriteLine(p.T);
Console.WriteLine(p.N + " nodes, " + p.Ls + " links");
```

The main actions are `Go`, `Bk`, `Fw`, `Rl`, and `At`. Page values are shortened
to `U` (response URL), `Bu` (document base URL), `T` (title), `H` (HTML), `N`
(node count), and `Ls` (link count).

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

After a Release solution build, regenerate the engine report with:

```powershell
..\Lea.Bench\bin\Release\Lea.Bench.exe -r .. -o COMPATIBILITY_PERFORMANCE.md -n 15
```

The report marks unsupported browser-platform features explicitly; a passing
fixture probe is not presented as full Chrome, Safari, Firefox, or HTML Living
Standard compatibility.

## Local Test Page

Open this path in LeaForge:

```text
LeaForge/test-pages/index.html
```

## Important

LeaForge is still our own experimental renderer. It does not use Chromium, Edge WebView, or a full JavaScript VM.
Lea.Engine now includes only a small allow-listed JavaScript-shaped command
bridge; it does not evaluate arbitrary JavaScript.
