# Firespider Internet + Lea Integration Map

This map records the first read of the host project at:

```text
L:\Firespider Internet
```

## Host project

- Visual Basic WinForms application.
- Project file: `Firespider Internet.vbproj`.
- Target framework: .NET Framework 4.6.1.
- Startup object: `Firespider_Internet.My.MyApplication`.
- Main browser form: `1.vb` with generated controls in `1.Designer.vb`.
- Browser tabs: a `TabControl` named `k`.
- Address/search input: `x`.
- Status label: `su`.
- Current tab content is normally assumed to be `k.SelectedTab.Controls(0)`.

## Current engine

The legacy engine is `System.Windows.Forms.WebBrowser`, which wraps the old
Windows Internet Explorer/MSHTML control. The host uses it directly throughout
`1.vb` and `BrowserFeatures.vb` through `BrowserFeatures9.vb`.

Important legacy events include:

- `Navigating` -> `nn`
- `Navigated` -> `nv`
- `DocumentCompleted` -> `n0`
- `ProgressChanged` -> `q14`
- `NewWindow` -> `qpopup`

The basic navigation helpers are:

- `q1` opens an address or search result.
- `q2` searches from the search box.
- `q3` creates a tab and browser control.
- `q4` duplicates the current tab.

## What depends on the old control

The project has a large feature surface that reads or changes WebBrowser state:

- URL, title, document HTML, body text, and DOM elements.
- Back, forward, reload, stop, printing, and page setup.
- DOM selection, editing commands, zoom, page styles, and script calls.
- Links, images, forms, popups, downloads, and external protocol checks.
- Tab suspension, lazy restore, overview thumbnails, and archive capture.
- Site rules, content filters, permissions, annotations, analytics, and watchlists.
- Bookmarks, sessions, history, search, diagnostics, and browser data export.

This means replacing `WebBrowser` with a different control is not a one-line
change. The safest route is to create an adapter that exposes the small set of
browser operations the feature modules need, then migrate feature groups one at
a time.

## Proposed Lea boundary

```text
Firespider UI and feature modules
        |
        v
Firespider browser adapter contract
        |
        v
Lea host adapter for WinForms
        |
        +-- Lea.Engine: load, parse, DOM, CSS, layout, cache, script
        +-- Lea.Render: paint and hit testing
        +-- Lea.Win: Control, input, scroll, image, clipboard bridges
```

The adapter should provide equivalents for:

```text
Go, Bk, Fw, Rl, Stop
U, T, H, Text
CanBk, CanFw
Load, Done, Prog, NewWin
Find, Sel, Print, Save
```

The short names match Lea's alias-first identity, while a VB-friendly wrapper
can expose descriptive properties where that makes the host migration safer.

## Port sequence

### Phase A: compatibility shell

1. Create a small `LeaHost` WinForms control that can live inside a Firespider
   `TabPage`.
2. Give it navigation, current URL, title, source, text, history, and load events.
3. Add a feature flag that lets one test tab use Lea while other tabs retain the
   legacy control.
4. Port only `q1`, `q3`, `q4`, back, forward, reload, stop, address updates, and
   tab titles first.

### Phase B: browser essentials

1. Port links, scrolling, find-in-page, source view, local files, images, and
   page save.
2. Add the Lea adapter methods needed by bookmarks, sessions, history, and tabs.
3. Keep legacy-only actions visible but disabled or clearly reported until Lea
   supports them.

### Phase C: feature migration

Migrate feature modules by capability rather than by file:

1. navigation and tabs
2. bookmarks, history, sessions, and search
3. page text, source, save, print, and find
4. archives, annotations, reading list, and research tools
5. site rules, permissions, content filters, downloads, and watchlists
6. automation, diagnostics, and appearance integration

### Phase D: engine replacement

When the Lea adapter passes the Firespider smoke suite, make Lea the default
engine for new tabs, then remove old-control assumptions from each migrated
feature. Only remove the legacy reference after all required behavior has a Lea
equivalent and the existing project builds cleanly.

## Important compatibility risks

- The host targets .NET Framework 4.6.1 while current Lea projects target .NET 8.
- Lea needs a .NET Framework-compatible host build or a multi-targeted core before
  it can be referenced directly by Firespider.
- Existing code expects a live DOM object and COM-backed `HtmlDocument`; Lea will
  need its own small DOM abstraction and compatibility helpers.
- Existing code uses synchronous-looking navigation while Lea should load without
  freezing the WinForms UI.
- Existing code assumes every tab contains a WebBrowser at control index zero;
  the adapter should preserve that placement initially to reduce migration risk.
- Print, script execution, media, and complex forms need explicit capability
  decisions instead of pretending to be supported.

## First implementation target

Do not port the entire browser at once. First build a `LeaHost` adapter that can
replace one `WebBrowser` inside one Firespider tab and support:

```text
open address
back / forward / reload / stop
title and URL updates
links
local test pages
source and readable text
basic scrolling
```

Once that works in the real Firespider window, the rest of the port has a clear
home and can proceed feature by feature.

## Experimental tab status

Phase A now has a working opt-in path. `Lea.Firespider/FirespiderLea.vb` adds a
New Lea Tab command and routes Firespider's primary navigation controls through
`Lea.Win.V` only for that tab. Legacy tabs retain their original handlers. The
adapter is included through one reversible MSBuild import and passes Debug and
Release host smoke tests. It also replaces the status timer's unconditional
`WebBrowser` cast with type-aware routing, so selecting a Lea tab cannot trigger
an `InvalidCastException` on `Timer.Tick`.

Phase B routes address/search Enter keys, Open File, Save Page, and Send Link
through Lea state when a Lea tab is active and retains the original handlers for
legacy tabs. Close Tab, the tab-strip close target, middle-click, Page Source,
and the bounded isolated Lea session remain capability-aware. Debug and Release
host smoke tests cover these paths. Firespider's shared New Tab commands stay on
the legacy path until internal callers that immediately require `WebBrowser` are
migrated.

Find-in-page, explicit streamed downloads, safe button/link page commands, and
the host-readable `Dg` diagnostics snapshot now route through Lea for Lea tabs.
**View > Lea Inspector** (`F12`) and each Lea tab's context menu now present that
bounded snapshot as Summary, Nodes, CSS, Requests, and Messages tabs. Refresh
and request-log clearing stay on the public `Dg`/`Dcl` boundary, while legacy
tabs are explicitly left untouched. `Lea.Bench` emits the repeatable
compatibility/performance report.
