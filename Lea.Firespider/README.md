# Lea Firespider Adapter

`FirespiderLea.vb` is a reversible experimental integration for the VB.NET
Firespider host. Firespider's normal **New Lea Tab** command and `Ctrl+T` now
create `Lea.Win.V`; **New Legacy Tab** (`Ctrl+Alt+L`) deliberately uses the
original `WebBrowser` path for features that still need it. Back, forward, home,
go, search, reload, stop, and address/search Enter keys route by the selected
tab's engine. Legacy tabs delegate to Firespider's original handlers.
Its timer status handler is also routed safely across startup, empty, Lea, and
legacy-tab states, so it never dereferences a missing control or casts a Lea
view to the legacy `WebBrowser` type.

Firespider's Close Tab command, tab-strip close target, and middle-click now
close Lea tabs safely. The existing Page Source window displays Lea's raw `V.H`
HTML. Open File loads local pages into the selected Lea view, Save Page writes
that source as UTF-8, and Send Link builds its message from Lea's title and URL.
Up to 12 Lea URLs restore from `%LocalAppData%\Firespider Internet\Lea.session`;
private mode deletes that file instead of saving it. Legacy session settings
remain owned by Firespider.

Firespider's private-mode flag also drives `V.Pv`. Normal Lea cookies stay in
their bounded in-memory jar, while each private session starts empty and is
destroyed when private mode ends. Page cache is cleared between those contexts.
Lea's persistent cache is still opt-in through `V.Cd`; `Cx/Cb` set entry/byte
ceilings, `Dc/Db` report its footprint, and `Cdl()` clears only Lea entries.
Private tabs never read or write that directory.

Lea's bounded inline title/style commands run inside each Lea tab. Alert
messages cross the `V.Al` host event and appear in Firespider's existing status
bar; page code does not open a native dialog directly.
Buttons and links can run bounded `data-lea` or familiar `onclick` values through
`V.Ck`. Small `v/var` variables and `tx/text` element updates stay inside Lea's
allow-list and cannot reach native files, processes, or host controls.
ID-scoped `ac/rc/tc` class changes use the same route, reapply compiled CSS, and
preserve mutable field values.
The same public route supports bounded `hd/sh` visibility, `op/cl/tg` details
state, `ds/en` field availability, `vl` values, `ck` checked state, and `fo`
focus requests. Hidden
field identity is retained, file values cannot be injected, passwords remain
redacted, and focus is cleared or restored only through Lea's rendered target
model.
The bounded `go(url)` command raises `V.Gr` with `V.Gu`; Firespider sends that
request through its existing asynchronous Lea loader. Relative URLs resolve
against the document base, unsafe schemes and remote-to-file requests are
rejected, and a command request overrides the same click's default link route.
Lea tabs also expose the bounded `V.Tm` request deadline: 15,000 ms by default,
clamped to 100-60,000 ms, with typed timeout and cancellation failures.

Same-page `#fragment` links stay inside the current Lea page and never enter the
host loader. Compact `Ft/Fg/Rv` lookup, retained history, closed-details reveal,
and layout scrolling still raise the normal `Changed` event, so Firespider's
address bar and Back/Forward controls update without adapter-specific DOM code.

Progress and meter elements remain inside Lea's custom renderer. Compact
`B.Vm` values provide bounded ranges, indeterminate progress, meter quality,
inherited painting, and table-cell reflow without adding a native browser or
per-gauge WinForms control to Firespider.

Bounded flex row and column flow also remains entirely inside Lea's custom
renderer. Firespider can read compact `B.S.Fd/Jc/Ai/As/Gp/Wr/Rg/Fg/Sh/Fb/Or`
state while fixed, equal-share, wrapped, ordered, around/evenly
distributed, integer weighted-growth, and
min-protected weighted-shrink items, row/column reversal, pixel basis and gaps,
alignment, nesting, painting, and hit testing reuse the existing `V` surface
without adding controls or another browser runtime.

Bounded CSS Grid columns and rows use that same custom renderer. Firespider can
inspect lazy `B.S.Gc/Gv(index)` and `Rc/Rv(index)` tracks plus `Gl/Gs` and
`Rl/Rs` item placement while fixed pixel and integer `fr` tracks, simple
`repeat()`/`minmax(0,Nfr)`, gaps, bounded occupancy, two-axis spans, stable
order, implicit rows, validated named-area templates through
`Aa/Ar/Ac/Ag/An`, unique named lines through `Gnc/Rnc/Gx/Rx` and
`Gn/Ge/Rn/Re`, generated area edges, and `Gf=r/d/c/x` sparse/dense row/column
auto flow stay inside `V`. Repeating pixel/auto/integer-fr implicit patterns use
`S.Av` and `Gac/Gav/Rac/Rav`; definite endpoints, largest spans, area-created
tracks, and auto-height pixel rows share the placement and measurement passes.
Atomic `grid-template`/`grid` explicit, quoted-area, row auto-flow, and column
auto-flow forms compile into that state and retain standard gap/reset boundaries.
Compact `Ji/Js/Ai/As` state adds container/item start, center, end, and stretch
alignment plus atomic `place-items`/`place-self` without adapter-side geometry.
Compact `Al/Jc` state adds start/center/end/between/around/evenly track-group
distribution and atomic `place-content` on the same resolved slots.
Definite-first placement, nested content, fields, painting, hits, and responsive
reflow use the same path. The host smoke verifies exact mixed
column/row/named-area/named-line/dense/column-flow/auto-track/shorthand/item/content
alignment geometry, dot-cell auto placement, ownership, queries, and Inspector
CSS without adding adapter-side layout logic.

Firespider can also read `B.S.Bz` as `c/b` for non-inherited content-box or
border-box sizing. Lea applies it to pixel width, height, min/max, and flex basis
before the shared block, field, image, flex, table, cell, hit, paint, and
accessibility-bound paths, without adding a native browser dependency.

Responsive CSS also stays inside that surface. `V.Vw` follows the hosted
control's client width, while `P.Vw` and `G.Vw` expose the applied value to the
page and Inspector. Bounded screen/all pixel min/max/exact, comma, nested, and
style media conditions switch compiled rules on resize without replacing live
field models. The host smoke verifies narrow and restored wide states.

CSS custom properties stay compact as well. Firespider can read `S.Vc`, perform
case-sensitive `S.Cv(name)` lookup, and see the document count through `G.Vc`
and the Inspector. Declarations compile once, inherited maps remain lazy and
copy on override, and bounded nested `var()` fallbacks update through media and
dynamic rebuilds without replacing fields. The host smoke verifies computed
wide/narrow values and Inspector reporting.

The same compiled records now retain a bounded important-priority bit. Final
top-level literal `!important` markers create normal/important stylesheet and
inline tiers for every supported ordinary and custom property while keeping
specificity, source order, dynamic state, and responsive rebuilds. Firespider's
Inspector reports active stylesheet declarations through `G.Ic`; the host smoke
uses conflicting inline normal values to verify the important path and retained
field identity without another browser runtime.

Bounded selector functions stay on that compiler too. Firespider pages and host
queries can use `:is`, zero-specificity `:where`, and selector-list `:not` with
up to eight compound alternatives and sixteen authored tests per simple
selector. Forgiving `:is`/`:where`, strict `:not`, maximum specificity, outer
combinators, dynamic state, and retained fields need no adapter-side selector
logic. The host smoke verifies computed border/font geometry, `Q1/Qa`, and CSS
Inspector output.

Structural arithmetic stays precompiled too. Firespider pages and host queries
can use signed integers and bounded `An+B` expressions with `:nth-child`,
`:nth-last-child`, `:nth-of-type`, and `:nth-last-of-type`. Lea stores only the
two parsed integers, caps expressions at 32 characters and absolute values at
8,192, and performs no per-match regex work. Host smoke coverage checks forward
and reverse computed borders, padding, color, `Qa`, geometry, and Inspector CSS.

Filtered child positions reuse those records. Firespider pages and host queries
can use strict bounded `:nth-child(... of S)` and `:nth-last-child(... of S)`
compound lists. Matching siblings alone enter the forward/reverse count, the
most specific filter contributes to cascade weight, and eight-alternative plus
sixteen-total-test ceilings prevent unbounded matching. The host smoke checks
separate forward/reverse borders, `Qa`, Inspector CSS, and flex geometry.

Relational selectors remain engine-owned as well. Firespider pages and host
queries can use strict bounded `:has(...)` lists with implicit descendants,
leading child, adjacent, and following relationships plus four-part compiled
chains. Eight alternatives, sixteen total parts, and 4,096 candidate visits
bound the work; maximum argument specificity drives the cascade. The host smoke
checks parent style and geometry, complex queries, descendant-focus rebuilds,
retained field identity, and Inspector CSS without adding adapter-side tree
logic.

Output and data elements also stay inside Lea's custom renderer. Firespider can
read ordered `P.Dvs` values or the matching `B.Dv`/`Ir.Dv` paint ownership,
including bounded output references, data machine values, and safe text/class
rebuilds, without adding per-value WinForms controls.

Time, inserted, and removed elements reuse that model with `K=t/i/r`, bounded
`Dt` date text, and a safe file/HTTP/HTTPS citation `U`. Citations remain host
metadata rather than clickable links. Native underline/strike painting,
inline/block/table ownership, and safe text/class rebuilds stay inside the same
custom surface; the off-screen Firespider smoke reads and updates these aliases.

Image maps remain on Lea's surface as well. Firespider can enumerate `P.Mps`,
follow an image's `B.Mp`, and inspect bounded `Ar.K/Cs/U/T/El` area data.
Rectangle, circle, polygon, default, and dead regions scale from `B.Mw/Mh` to
the settled image, clip with it, and preserve first-area priority. Area links,
fragments, click commands, and downloads reuse existing host routes. The
off-screen host smoke paints a mapped image and verifies its live hit cache.

Firespider can also read `V.Ke/Kn/Ki/Kb`, call `Kf(back)` to move through the
first 256 sequential targets, and await `Ka()` to activate the current link,
image-map area, field, or summary. `Ti` exposes bounded nullable tabindex
metadata. Lea paints a default focus ring, reapplies page `:focus`, scrolls
off-screen targets into view, and keeps Ctrl+Tab available to Firespider's tab
chrome. The host smoke focuses and activates a mapped area off-screen.

Autofocus and page shortcuts stay on that same custom surface. `Af/Afs` expose
at most 32 authored candidates, and a load without a resolved fragment selects the first
visible focusable target without forcing the Firespider window forward.
`Ak/Aks` parse bounded ASCII letter/digit alternatives; the view exposes its
first 64 conflict-free assignments through `An`, `Ak`, and `Kl`, while `Aa`
focuses and activates one. Firespider's WinForms command route gets first
refusal, and Lea consumes only an otherwise-unclaimed assigned
`Alt+Shift+key`. The host smoke covers editable autofocus, a negative-tabindex
focus key, conflict fallback, unmatched fallthrough, and mapped-area shortcut
activation.

Semantic accessibility stays on the same Lea tab. Firespider can enumerate
`V.Axs`, resolve an element through `Ax`, and read one record's rendered client
bounds through `Ab`. Compact `Ax.K/N/D/V/St/Lv/U/El/Up` aliases expose bounded
roles, names, descriptions, live values and states, heading levels, links, DOM
ownership, and hierarchy. Password and file values are redacted. `V.Ao` and
the control's native provider expose the records as WinForms/MSAA children and
as a native UI Automation fragment tree. UIA provides stable bounded runtime
IDs, Lea control types, protected state, focus and property notifications, and
invoke/value/toggle/selection/disclosure/range/scroll patterns over the same
records. `Tb/Tr/Tc` tables additionally expose grid/table and
grid-item/table-item patterns with bounded dimensions, zero-based coordinates,
column spans, containing-grid links, and row/column headers. The adapter target
also exposes bounded read-only logical TextPattern document/child/point ranges,
search, movement, attributes, rendered bounds, embedded children, scrolling,
strict password/file-value exclusion, visual-line movement and rectangles, and
partial-range cropping. Visible-range arrays use a stable empty fallback because
the .NET Framework managed provider boundary produces invalid handles for
non-empty interface arrays. The adapter target adds the framework `Accessibility`,
`UIAutomationProvider`, `UIAutomationTypes`, and `WindowsBase` references.

Firespider can read `V.Dg()` for bounded response, document, CSS, request,
command, memory/persistent cache, and cookie diagnostics without reflecting into Lea internals.
The snapshot omits mutable form values, passwords, and selected file paths.
**View > Lea Inspector** (`F12`) and the Lea tab context menu open Summary,
Nodes, CSS, Requests, Messages, and Access tabs backed only by public bounded
models. `R`/`F5` refresh, `C` clears request activity through `V.Dcl()`, and
`X`/Escape closes the owned window. Legacy tabs remain on Firespider's original
path.

Firespider's existing Find command (`Ctrl+F`) now calls Lea's highlighted,
case-insensitive match cycling for Lea tabs and keeps the original DOM range
path for legacy tabs. Lea download links open Firespider's Save dialog, honor its
default save folder, stream through `V.DlA`, show the full destination and byte
count in the status bar, and join Firespider's recent-download list.

Inline SVG remains inside the same hosted `V` surface. Firespider can inspect
`B.Vg` and compact `Vg/Vs/Vp` viewport, shape, paint, path, and link aliases;
the adapter adds no SVG control or external runtime. The host smoke verifies a
compiled Firespider tab's exact vector size and graphic accessibility name.

The host project imports `Lea/FirespiderLea.targets`, which adds the two DLL
references, compiles `FirespiderLea.vb`, and copies `start.html` to the output
`Lea` folder. No Chromium, WebView, Safari, or hidden browser control is used.
If SDK `dotnet build` forces old binary `.resx` files through its newer resource
serializer, build this legacy host with the installed .NET Framework MSBuild:
`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe`. Lea source and
resources do not need to be changed for that tooling mismatch.

The adapter is kept here as the clean source of record because the live
Firespider working tree contains unrelated in-progress changes.

`Smoke.cs` loads two compiled Firespider forms off-screen and verifies Lea-first
New Tab creation, the explicit legacy fallback, Inspector open/refresh/clear and legacy isolation, diagnostics,
script/style, host-controlled `go`, and safe click execution, alert routing,
ID-scoped `bg/fg/fs` style commands, state commands, retained fields,
autofocus/access keys, keyboard navigation, local
open, find cycling, streamed download saves, page save, send-link metadata,
close routes, source, and session restoration using temporary files rather than
the user's data.
