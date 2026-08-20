# Lea Engine Plan

Lea is a small, fast, alias-first web engine. The goal is not to copy every
large browser feature immediately. The goal is to build a clean core that is
pleasant to code against, starts quickly, and grows through small tested steps.

## 1. Identity and limits

- Keep the public Lea vocabulary within one to three letters where practical.
- Keep the readable implementation underneath the compact public API.
- Keep `Lea.Micro` as the tiny experiment and `LeaForge` as the fuller engine.
- Avoid Chromium, WebView, and other large browser runtimes in the custom path.
- Track startup time, page load time, memory use, executable size, and compressed size.
- Keep every new feature optional when it would make the base engine much larger.

Core public vocabulary:

```text
L       Lea entry point
E       engine
P       page
N       node
Go      open
Bk      back
Fw      forward
Rl      reload
At      current page
t/a     title/alert
bg/fg   background/text color
fs      font size
v/tx    variable/element text
Ck      safe element click
Dg/Dcl  diagnostics/clear request activity
```

## 2. Engine foundation

1. Split the current single-file Forge prototype into small files.
2. Define stable interfaces for loading, parsing, layout, painting, and scripting.
3. Add a `Uri` and encoding utility that handles local files, HTTP, relative URLs,
   redirects, UTF-8, and common content types.
4. Use bounded caches for pages, images, stylesheets, and decoded resources.
   (An eight-entry LRU decoded-payload cache with a 2 MB per-page ceiling is active.)
5. Add cancellation and timeouts so a slow page cannot freeze the browser.
   (`GoA`, `RlA`, and `St` are now available in the compatibility engine.)

## 3. HTML and document tree

1. Replace regular-expression-only page parsing with a small tolerant tokenizer. (First stack-based tokenizer added.)
2. Build a lightweight document tree with elements, text, attributes, and parent links. (`N` now provides kind, text, attributes, and children; parent links remain.)
3. Support the useful base tags first: `html`, `head`, `body`, headings, paragraphs,
   lists, links, images, buttons, forms, tables, sections, and code blocks.
   (Single-select controls, URL-encoded GET/POST forms, and structured table rows,
   cells, captions, headers, and column spans are active.)
4. Preserve unknown tags safely so pages can still display their text.
5. Add malformed-markup tests and local fixture pages for every supported tag.

## 4. CSS and layout

1. Add selectors for tag, class, id, attributes, basic pseudo-classes, negation, descendant, child, adjacent/general sibling, and inline styles. (Compiled compact `>`, `+`, and `~` chains are active.)
2. Implement the cascade and inheritance with a small priority model. (First specificity/source-order cascade added.)
   (`link rel=stylesheet` and inline `style` blocks now enter that source order
   together; linked CSS uses cookies, compression, validators, and shared LRU caching.)
3. Add colors, fonts, spacing, borders, width, height, display, visibility, opacity, and overflow. (Colors, fallback font families, underline/strike decorations, margins, padding, backgrounds, `display:none`, inherited visibility, bounded opacity, borders, pixel width/height bounds, text alignment, line height, word breaking, white-space modes, and clipped overflow are active.)
4. Replace one-WinForms-control-per-node rendering with lightweight layout boxes. (Reusable `Bx` layout pass added.)
5. Paint text, backgrounds, borders, links, controls, and images onto one surface. (Text, nested bold/italic inline runs, backgrounds, solid/dashed/dotted borders, links, images, and basic form controls are active.)
6. Add scrolling and hit testing from layout boxes. (Visible-box painting and bounded vertical scrolling added.)
7. Add compact table row/cell layout. (Equal columns, wrapped text, CSS boxes,
   bounded `colspan`, nested blocks, clickable links, malformed-markup recovery,
   and narrow-view reflow are active.)

This is the largest speed milestone: fewer controls, less UI overhead, and one
predictable rendering pass.

## 5. Network and resources

- HTTP and HTTPS loading through a small request layer.
- URL-encoded POST navigation with synchronous and asynchronous `Po` aliases.
- Redirects, response status, content type, content encoding, and character encoding. (Final response URL and compact page metadata are active.)
- GZip/Deflate response decompression. (Both are decoded transparently and covered by loopback tests.)
- Image loading for PNG, JPEG, GIF, and ICO where the platform supports them. (Basic async image loading and bounded memory cache added.)
- A small memory and disk cache with explicit size limits. (Bounded memory payload cache active; disk cache remains.)
- Cache freshness from `max-age`, `Expires`, `no-cache`, `no-store`, and a configurable default. (Active.)
- Conditional cache revalidation with ETag/If-None-Match and
  Last-Modified/If-Modified-Since. (`304` responses reuse the decoded body and
  refresh its lifetime; explicit reload remains unconditional.)
- Compact typed `Er` failures for URL, file, HTTP, network, timeout, cancellation, and decoding problems. (Active.)
- Basic cookies and a clear permission boundary for local files and scripts.
  (A bounded `CookieContainer` now handles HTTP cookie rules; `Kc`, `Km`,
  `Kcl()`, and `Pv` expose count, limit, clearing, and isolated private mode.)

## 6. Lea scripting

The first scripting layer should be a deliberately small command language, not a
full JavaScript replacement.

```text
t("Title")       set title
a("Message")     alert
bg("#fff")       page background
fg("#222")       text color
fs(12)           font size
Go("url")        navigate
```

The bounded layer is active in `Lea.Engine`. It accepts `t`, `a`, `bg`, `fg`,
`fs`, `v`, `tx`, `ac`, `rc`, `tc`, and host-controlled `go`, plus equivalent
title/body-style assignments and `alert(...)`.
Only top-level literal commands run. Execution is limited to eight inline
blocks, 16 KB per block, 32 accepted commands, and 16 queued alerts. External
scripts, JSON data blocks, expressions, control flow, and unknown calls are
ignored. `Je` is the host permission switch and `Js` is the compact manual
bridge. The WinForms host emits `Al` and exposes `Am` instead of allowing page
code to open native UI itself. `Ck` safely dispatches button and link attributes;
`P.Vs` holds 16 bounded variables and `tx` performs ID-scoped text replacement.
`go` resolves one pending HTTP, HTTPS, or safe file-origin URL against `P.Bu`;
`E.Gt` consumes it, while `V.Gu`/`Gr` expose it to browser policy. Every command
has a short alias, a fixed limit, and a local fixture. A full
JavaScript runtime can remain a separate comparison project.

## 7. Browser shell

- Tabs and per-tab history.
- Address bar with local file and web URL support.
- Back, forward, reload, stop, and home.
- Bookmarks saved to a small JSON file.
- Downloads with a visible destination and status.
- Find-in-page.
- Page source and a small developer panel showing nodes, styles, requests, and logs.
- Keyboard shortcuts that do not complicate the engine API.

Downloads and find-in-page are active in the .NET Framework engine and WinForms
view. `Dl`/`DlA` stream local, HTTP, and HTTPS responses through an explicit
host-selected destination, decode HTTP content encoding, use a same-folder
temporary file, and reject payloads above 64 MB. `Fd` is capped at 256 matches;
the WinForms view highlights painted matches and cycles them with `Nx`/`Pr`.
Download links raise a host request rather than gaining direct file access.

Bounded developer diagnostics are active through `Dg`. The compact `G` snapshot
contains page/response counts, up to 512 redacted tree lines, 256 compiled CSS
summaries, and the latest 32 local/network/cache/download events. `Dcl` clears
request activity. `Lea.Win.Test` presents Summary, Nodes, CSS, Requests, and
Messages tabs in a resizable inspector without adding controls to rendered pages.
Firespider now exposes the same bounded data in a separate Lea Inspector window
from its View menu or a Lea tab's context menu. `F12` opens it, `R` refreshes,
and `C` clears only request activity. Legacy tabs remain on their original path.

## 8. Safety and reliability

- Never execute arbitrary native code from a page.
- Keep script commands allow-listed.
- Isolate local file access behind explicit browser behavior.
- Handle broken markup, failed images, bad URLs, and network errors gracefully.
- Add regression fixtures before expanding the feature set.

## 9. Testing and speed work

- Parser tests for tags, nesting, attributes, malformed HTML, and entities.
- CSS tests for selector priority, inheritance, and computed values.
- Layout tests for wrapping, spacing, scrolling, and hit testing.
- Engine tests for cache hits, reloads, history, redirects, and failures.
- Browser smoke tests using the `test-pages` folder.
- Benchmarks for startup, parse time, render time, memory, and EXE size.
- A compressed release build alongside the readable development build.

## 10. Release milestones

### Lea 0.1: Tiny core

HTML text, links, local files, compact aliases, history, and a small test window.

### Lea 0.2: Fast renderer

Document tree, CSS cascade, layout boxes, one-surface painting, scrolling, and images.

### Lea 0.3: Useful browser

Tabs, bookmarks, cache, forms, downloads, find-in-page, and robust networking.

### Lea 0.4: Lea language

Safe short commands, events, page updates, examples, and a standalone Lea API.

### Lea 1.0: Small dependable browser

Stable API, repeatable EXE packaging, benchmarks, developer tools, documentation,
and a clear compatibility list.

## 11. Visual Studio integration

Lea must be easy to add to a normal Visual Studio browser project without
bringing in another browser engine.

### Project shape

- `Lea.Engine`: platform-neutral core library containing networking, parsing,
  document nodes, CSS, layout, cache, and the compact API.
- `Lea.Render`: small rendering library with a platform adapter boundary.
- `Lea.Win`: optional Windows adapter for WinForms/WPF drawing and input.
- `Lea.Browser`: optional ready-made test browser shell.
- `Lea.Micro`: tiny single-file experiment and size benchmark.

The custom engine libraries must have no dependency on Chrome, Chromium,
WebView, Safari, Edge WebView, or another embedded browser runtime.

### How a project will consume Lea

Support three simple Visual Studio paths:

1. Add the Lea projects to the solution with a Project Reference during development.
2. Reference a built `Lea.Engine.dll` and the optional platform adapter directly.
3. Use a small local package or release folder containing DLLs, symbols, docs,
   and the Lea icon for larger projects.

### API boundary

- Keep the compact aliases stable after Lea 0.4.
- Keep UI code separate from the engine so a user's own browser window can host it.
- Expose page loading, navigation, document access, layout, painting, input,
  downloads, and permissions through small interfaces.
- Avoid exposing internal parser classes or WinForms controls from the core.
- Provide one short example that opens a page inside a user's own `Form`.
- Provide a second example that uses Lea without any browser window for testing.

### Compatibility targets

- First target: Windows and Visual Studio with .NET 8.
- Keep core code independent enough that a later Windows or cross-platform
  adapter does not require changing the document and layout engine.
- Record any platform-specific image, font, networking, or input behavior.

### Integration tests

- A clean sample Visual Studio app references Lea only through the public API.
- The sample opens a local page, a web page, follows a link, paints a page,
  and handles a failed request.
- The sample build does not install or load any external browser runtime.
- A versioned API smoke test catches accidental alias or interface changes.

## Current next milestone

Add bounded same-page fragment navigation so `#id` links resolve through the
existing compact document lookup and scroll one retained layout target into
view. Preserve history and base-URL behavior, and cover missing, hidden, table,
encoded, and resize cases.

The repeatable compatibility and performance report is now implemented by
`Lea.Bench`. It probes supported HTML, CSS, forms, scripting, layout, downloads,
and diagnostics, then records local load timing, bitmap paint timing, retained
managed memory, binary footprint, and compressed runtime size in
`COMPATIBILITY_PERFORMANCE.md`.

List-item blocks now carry compact paint-ready `B.Mk` markers. Unordered lists
use `-`; ordered lists use bounded invariant decimal markers and honor safe
`start` and per-item `value` integers. Each nested list computes its own scope,
increments saturate at 999999, and malformed or oversized values fall back
without unbounded text. Simple blocks, inline runs, table-cell parts, measurement,
paint, and resize all share the same marker text path through `lists.html`.

Details elements now carry compact page-local `N.Op` state. The first direct
summary uses `B.Mk` as `>` or `v`; all content masked by a closed ancestor keeps
its block and form models behind `B.Hd`. `E.Ds`/`V.Ds` toggle through one bounded
rebuild that reapplies `[open]` CSS and preserves fields and nested state.
`disclosure.html` covers pointer and Enter/Space activation, duplicate and stray
summaries, summary-free host control, table cells, nesting, paint, and resize.

HTML tokenization now uses a small quote-aware scanner rather than a single
tag-splitting regex. It recognizes comments and declarations, preserves raw
`script/style` text, decodes title/textarea RCDATA and normal text entities,
keeps malformed `<` delimiters as text, and caps scan steps, tree nodes, stack
depth, attributes, names, and attribute values. `html-tokens.html` plus a
generated stress document verify each limit.

Document queries now reuse the compiled CSS selector subset. `E.Id/Q/Qa` and
`V.Id/Q1/Qa` provide ID, first document-order match, and a read-only list of at
most 256 matches. Query text, groups, and total selector parts are capped;
invalid or unsupported selectors return no matches rather than entering an
unbounded fallback parser. `V.Q1` preserves the older `V.Q` URL-query alias.

Document base resolution now honors the first valid `<base href>` under a
4,096-character ceiling. `P.Bu` and `V.Bu` expose the result while `U` remains
the response URL. Stylesheets, links, images, forms, and download targets share
the resolved HTTP, HTTPS, or file base; invalid and unsupported candidates are
skipped, and pages without a valid base fall back to `U`.

Safe class mutation now uses `ac/rc/tc` for add, remove, and toggle against one
validated element ID. Names are 64 characters or fewer, each element retains
at most 32 tokens and 512 class-attribute characters, and metadata/script
targets are rejected. Valid no-op adds/removes remain accepted commands;
remove and toggle clear duplicate matches. The existing single CSS rebuild per
run preserves mutable fields across page, host, click, and WinForms execution.

The `inset` shorthand now expands atomically into `Tp/Rt/Bm/Lt`, and opposing
edges stretch auto-sized absolute and fixed boxes against their containing
rectangle. Supported pixel `translate`, `translateX`, and `translateY` values
use compact `Tx/Ty` aliases, apply after normal positioning without consuming
flow, and create the existing `Zc` local stacking context. The
`position-values.html` fixture verifies cascade reset, used edges, horizontal
and vertical stretch, parent-relative translation, trapped high-z descendants,
paint order, click routing, and following normal flow.

Nested fields, responsive images, and structured tables now retain their
nearest ordinary visual owner through `B.Up`, including fields reached through
an unstyled form wrapper. They participate in parent-relative width, ordered
flow, automatic height, inherited overflow clipping, and following root flow.
The WinForms editor intersects its content box with the inherited clip and
viewport before it becomes visible, so focused fields cannot paint outside a
clipped ancestor. `nested-content.html` verifies ownership, geometry, image
paint, table height, clipped editing, and mutable field state together.

`S.Za` now distinguishes numeric z-index from `auto`, and `S.Zc` records local
opacity or `isolation:isolate` context creation. Fixed boxes remain contexts.
The hierarchical paint and hit path therefore traps descendants only when the
parent establishes a compact context; positioned `z-index:auto` descendants can
participate in the ancestor layer. Auto-height context owners use one bounded
measurement pass followed by final placement, so bottom/right offsets and
overflow clips resolve against settled geometry without making flat pages pay
for another layout pass.

Ordinary visual containers now retain compact layout ownership through `B.Up`
when backgrounds, borders, spacing, or dimensions make their own box meaningful.
The layout closes auto-height contexts from the deepest subtree outward, grows
each parent around normal-flow descendants, then places its next sibling or root
block. Mixed direct text, nested links, three-level containers, and narrow-view
reflow share this path. Unstyled semantic wrappers remain flattened so simple
pages keep the original low-overhead layout.

Nested positioning now retains compact parent relationships through `B.Up`.
The WinForms layout records each context's content rectangle, child-flow cursor,
inherited clip, and fixed ancestry. Absolute descendants resolve against their
nearest positioned container; nested fixed descendants stay viewport anchored.
Paint and hit testing use the same hierarchical z-index path, so a high-z child
cannot escape beneath or above its parent's stacking layer. A focused fixture
covers nested top/right/bottom/left offsets, inherited overflow clipping,
overlapping links, normal-flow continuation, and fixed behavior while scrolling.

File fields now use compact `Fi.Fp/Ac/Mu/En` state and `V.Sf` selection. The
native picker and multipart serializer enforce eight-file, 4 MB per-file, and
8 MB combined bounds; HTML file values never become local paths. Synchronous
and asynchronous submit paths send successful controls and binary file content
through bounded `Pb/PbA` requests. Range and color have dedicated painting and
interaction, while time, month, week, local datetime, multiple email, and step
validation share the existing compact validity path.

Form fields now parse compact `Rq/Mn/Mx/Pa/Lo/Hi/Nv` constraints and expose
`Ok/Vm` results. `V.Vf` and both submit paths validate required controls,
radio groups, selected values, lengths, bounded patterns, email/URL syntax,
numbers, and ISO dates. Invalid forms remain on-page, receive visible state and
`:invalid` CSS, and suppress submit events; `novalidate` remains an explicit
bypass. Select, textarea, and button child text no longer paints twice.

Positioning uses compact `S.Pn/Tp/Rt/Bm/Lt/Zi` fields and `B.Up` context links.
Relative blocks and tables move while preserving their normal-flow slot.
Absolute boxes leave flow and resolve against the nearest positioned ancestor;
fixed boxes remain in viewport coordinates through nested context chains.
Inherited overflow clips constrain descendants. Hierarchical z-index paths use
one stable order for painting, hover selection, and overlap clicks. Auto-sized
positioned containers and the full CSS stacking-context trigger set remain
later work.

Responsive images now select compact `srcset` width or density candidates and
screen-compatible `<picture>` sources before falling back to `<img src>`.
Base64 `data:image/...` resources decode under a 4 MB bound and share the same
24-entry image cache. HTML dimensions apply when CSS does not override them,
and tiny embedded pixels use stable nearest-neighbor expansion.

Dynamic `:hover` and `:focus` selectors are compiled with the static selector
set. Paint blocks and runs retain a compact `El` element reference, allowing
`E.Dy` and the WinForms view to reapply styles without loading or reparsing the
page. Form objects are reused so edits, checks, and selections survive restyles.

`S.Vi` stores inherited `v/h/c` visibility. Hidden and collapsed boxes retain
layout but skip every paint, hit-registration, and native-editor path; a child
may explicitly return to visible. `S.Op` stores the effective `0..100` opacity
after combining ancestor and local decimal/percentage values. The opaque fast
path is unchanged, while faded text, boxes, borders, controls, and images use
alpha painting. Lea intentionally uses lightweight primitive alpha rather than
a full stacking-context compositor.

General sibling (`~`) chains scan the preceding element siblings once and keep
the nearest compiled match. `:not(...)` accepts one simple selector containing
tag, universal, ID, class, attribute, or an existing basic pseudo-class. Its
argument contributes specificity directly, and the argument is compiled once
with the rest of the rule. Complex selector lists and nested negation remain a
later extension.

`S.Wn/Wx/Hn/Hx` store positive pixel min/max dimensions. The shared layout
clamps apply the declared size, maximum, and then minimum consistently to normal
blocks, fields, tables, and nested table content while keeping minimum widths
inside the available viewport. `S.Wb` stores `n/b/a/k` for normal, emergency
break-word, anywhere/break-all, and keep-all behavior. `word-break`,
`overflow-wrap`, and legacy `word-wrap` all use the existing inline layout pass.

Lea compiles every simple selector once when its stylesheet loads. Attribute
presence plus `=`, `~=`, `|=`, `^=`, `$=`, and `*=` matching are active, as
are root, empty, link, fixed/odd/even child and type positions, only/first/last
positions, initial checked/disabled/enabled/required/read-only/read-write form
states, and live hover/focus state. Hover follows the pointed element and its
ancestors; focus follows the active form element.

Lea now supports inherited
left, center, right, and basic justified text plus pixel, percentage, and
unitless line heights in normal blocks and table cells. Default text still uses
the original one-box path; only requested text flow enters the word-line pass.

`S.Ff` stores a bounded, compact font fallback list with generic aliases, and
`V` caches up to 32 resolved Windows families. `S.Td` uses `n/u/s/us` for none,
underline, strike, or both. CSS `text-decoration`/`text-decoration-line`, plain
link overrides, and native `u/s/strike/del` tags share the same paint path.

CSS relationship matching supports descendant, direct-child (`>`), and
adjacent-element (`+`) combinators with or without spaces. Matching runs from
right to left, skips text nodes for adjacency, includes every simple selector
in specificity scoring, and compiles each selector once when CSS is loaded.
