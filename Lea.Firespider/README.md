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

Resolution media queries now stay engine-owned through compact `E/P/G/V.Dp`
aliases. The WinForms view follows its live monitor DPI until a host sets an
explicit density, and Firespider needs no adapter-side media parser. The
compiled host smoke covers `dpi`/`dppx`/`dpcm`, modern ranges, diagnostics,
retained fields, responsive geometry, and exact paint. See
`LeaForge/MEDIA_RESOLUTION.md`.

Initial fragments now stay aligned while first paint, host sizing, and decoded
intrinsic images settle. `V.Fa/Fj/Fy` expose the retained anchor, placement
count, and target Y directly to the host; normal scrolling releases the anchor.
The compiled host smoke covers these aliases with no adapter-side layout logic.
See `LeaForge/INITIAL_FRAGMENT_STABILITY.md`.

Horizontal logical sizing and insets compile into Lea's existing physical
records and host geometry. The compiled smoke covers all six size mappings,
logical axis placement, min/max clamping, short `is/bs`, familiar
`style.inlineSize/style.blockSize`, reset, and strict values. Firespider needs
no adapter-side axis state; writing-mode-dependent layout remains outside this
bounded milestone. See `LeaForge/LOGICAL_SIZING_INSETS.md`.

Horizontal logical margins and padding also compile into existing physical box
records. The compiled smoke covers axis shorthands, all underlying edge fields,
signed bounded margins, short `mi/mb/pi/pb`, familiar DOM forms, exact outer
geometry, reset, and malformed rejection. The twelve compact edge/axis aliases
need no Firespider adapter state. See `LeaForge/LOGICAL_BOX_MODEL.md`.

Horizontal logical borders compile into the existing physical widths plus one
lazy per-edge style/color record. The compiled smoke covers axis and edge
mapping, `S.Bsv/Bcv`, exact outer geometry, hidden used widths, and compact
`bi/bb/bis` command reflow. All 24 short aliases, readable forms, and familiar
DOM properties remain engine-owned; Firespider needs no adapter-side border
state. See `LeaForge/LOGICAL_BORDERS.md`.

Horizontal logical corner radii map directly onto Lea's existing clockwise
physical ellipse fields. The compiled smoke covers all four mappings, exact
outer geometry, and compact `rd/rss/rse/res/ree` commands. Physical/logical
source order, explicit inheritance/reset, variables, priority, familiar DOM
forms, normalized paint, and invalid preservation stay engine-owned; Firespider
adds no radius state or paint branch. See `LeaForge/LOGICAL_RADII.md`.

Inherited horizontal `direction: ltr/rtl` is stored in compact `S.Dr=l/r` and
resolved by Lea before logical declarations. The compiled host smoke verifies
RTL margin, padding, border, and radius mapping plus exact direction-aware flex
row, unequal explicit grid-span geometry, right-origin plain/mixed inline flow,
ordered list markers, physical-left RTL ellipsis, read-only range/point
geometry, and live `dr(id,value)` reflow; Firespider adds no
adapter-side direction or layout state. HTML `dir`, cascade details,
cross-direction inheritance, wider flex/grid/inline coverage, DOM forms, and the
bounded bidi boundary are documented in `LeaForge/DIRECTION.md`.

`Startup-smooth-host.patch` removes the blocking crash-recovery question and
automatically restores eligible unclean sessions. It also sends the retired
built-in `fsin.glitch.me` home to the bundled Lea start page when no explicit
new-tab URL is configured; custom home and new-tab settings remain intact.
The six-scenario EXE harness uses native bounded tab/Resume checks and limits UI
Automation traversal to separate dialogs, avoiding both menu-label false
positives and startup interference.

Pictures now finish with bounded response reads and a shared `Tm` deadline.
The `pictures` Boot case checks that a stalled image does not leave a loaded
native page busy or retain host request bookkeeping. `Ic/Ib` report retained
bitmap count and bytes. Use the matching view DLL; no new VB adapter patch is
needed. See `LeaForge/PICTURE_LIMITS.md`.

The `input` case adds typed oversized-page failure, recovery, shared stylesheet
deadlines, `Hm/Csm`, and request cleanup. The Boot suite now contains 42 cases.
See `LeaForge/INPUT_LIMITS.md` for limits and remaining resource work.

The `cache` case keeps path/query casing exact across selected-tab loads, shares
only fragment variants, separates case-different navigation, and checks host
request cleanup without MSHTML. See `LeaForge/CACHE_IDENTITY.md`.

The `decode` case sends one valid and one over-axis PNG through a selected Lea
tab. It checks `Ip/Im/It/In/If`,
`Ie/Ii/Iw/Ik/Ij/Ih/Ipm/Irt/Ili/Isp/Ihl/Ias/Icp/Icr/Img/Ipk/Iac/Ind/Ial/Icl/Ihm/Icm/Iad`, raw
pixels, a zero-capability AppContainer and exact helper-file ACL, verified
restricted low-integrity suspended launch, startup ASLR, a configurable Job CPU
hard cap, process mitigations, private/Job-peak and combined host/helper memory,
pre-native hostile rejection, retained form data, request cleanup, child exit,
and no MSHTML. See
`LeaForge/IMAGE_ISOLATION.md`.

Navigation, Retry, and Reload now use request identity tracking. `V.Cn` rejects
overlap before changing pending targets or preview CSS; an old completion cannot
clear a page-command successor's state. Use the matching adapter and view DLL.
The `load-life` Boot regression brings the suite to 38 cases. See
`LeaForge/LOAD_LIFECYCLE.md` for the callback contract and remaining startup issues.

Native Paste now preserves selected text when the clipboard is unavailable,
using the shared guarded menu/message/keyboard path. `V.Pa(text)` supports
clipboard-free host insertion. The `paste` Boot case checks the real menu,
status, undo/redo, and clipboard release recovery.
The matching view DLL is sufficient; no new host patch is required. See
`LeaForge/PASTE_SAFETY.md` for bounds and remaining issues.

Saved CSS and previews now use `Ss/Sd/Sn` directly on Lea. Startup/timer previews
never cast Lea to `WebBrowser`; a loading preview belongs to one request and
is cleared on completion/failure/close. Named replacement retains the document,
fields, native undo, and privacy. Arbitrary saved JavaScript remains unsupported.
Apply `Style-startup-host.patch` then `Native-styles-host.patch` to
`BrowserFeatures2.vb`, with the matching adapter and DLLs. The `styles` and
`style-start` regressions are part of the Boot suite. See
`LeaForge/NATIVE_STYLESHEETS.md` for bounds and remaining issues.

Lea now participates in the existing opt-in eligible-tab hibernation policy.
Compact `E.Ud/V.Ud` protects retained field data; privacy, cookies, history, script state,
scrolling, busy/error states, and existing tab/domain protections also prevent
automatic discard. Eligible renderers are disposed and resume on demand. The
`hibernate` and `hibernate-start` Boot cases bring the suite to 34, including
the real 30-second timer and renderer collection. Apply `Hibernation-host.patch`
with the matching adapter and DLLs. See `LeaForge/HIBERNATION.md` for limits.

Native document failures now publish `V.Fe/Le`; `V.Cr/Ry` supports explicit
GET retry through Reload without replaying POST. Failed first pages retain
their URL/session entry; timer ticks and background successes do not hide the
selected error. Split/test hosts display failures too. The `failures` and
`failure-start` Boot cases bring the suite to 32. See `LeaForge/LOAD_FAILURES.md`.
The recovery case now pumps the real 30-second snapshot timer for 35 seconds;
the guarded Lea/legacy dispatch completes without the reported cast dialog.

Ordinary Lea tabs now install `V.Ng` for blocked sites, navigation filters,
offline/locked state, and live tab ownership. Denied resume retains its saved
target and placeholder. Expected denials no longer escape Back/Forward,
unshown initial loads, or native keyboard/accessibility activation. The
`policy` and `policy-start` Boot cases bring the suite to 30. See
`LeaForge/NAVIGATION_POLICY.md`; the recovery timer remains in every combined
Boot release run as regression coverage.

The seven Edit commands now route native Lea fields through `V.Ce/Ec`, with
selection retention when opening menus, native one-level undo/redo, read-only
and password protection, and safe paused/empty-tab behavior. Explicit legacy
tabs keep their document editing path. The `edit` Boot case brought the suite
to 28 cases. See `LeaForge/NATIVE_EDITING.md` for the original cast paths and
their guarded engine-aware dispatch.

Normal first-window startup, toolbar New Tab, and Help now use Lea too. Startup
selects accepted crash-recovery URLs, a saved Lea session, legacy saved URLs
converted into Lea tabs, or one configured new-tab target, in that order.
Only the first non-private window restores; private startup skips recovery
prompts and saved URLs. Startup loads asynchronously before visibility and
selects the first restored page. Lea URLs also enter ordinary recovery snapshots.
See `LeaForge/LEA_STARTUP.md` for exact limits and the real form-start tests.

Startup menu creation now batches WinForms layout while retaining every
command. Matched local profiles reduced median Form.Show from 14.06 to 1.68
seconds; this is host initialization, not arbitrary network-page readiness.
`Startup-layout-host.patch` and the matching adapter retain the change.
See `LeaForge/STARTUP_PERFORMANCE.md` for samples, scope, and regression checks.

Side-By-Side Tab View now creates two independent Lea panes with source privacy,
asynchronous loading, bidirectional scroll sync, and bounded in-memory text
comparison. Owner closure cancels requests and disposes views/timer/results.
`FirespiderSplit.vb` is compiled by the updated targets file. Its `V.Ng` policy
uses existing host navigation rules; legacy DOM post-processing remains unported.
That milestone brought the Boot suite to 27 cases. See `LeaForge/SPLIT_VIEW.md` and the following
`Split-view-host.patch` for integration steps, bounds, and policy limitations.

Same-domain commands now match visible active, paused, and legacy tabs by exact
IDN-normalized HTTP/HTTPS hostname. Reload skips busy/paused views; copies use
Lea and preserve privacy; seeded new windows retain the first URL and lazy
policy. Copy/snapshot exports are bounded, and Save cancellation is respected.
`Domain-commands-host.patch` follows `Bulk-opening-host.patch`; the Boot suite
had 24 cases at that milestone. See `LeaForge/DOMAIN_COMMANDS.md` for details.

Shared bulk opening and unread-watchlist batches now append bounded Lea tabs
without replacing existing documents. Workspaces/protected sessions honor lazy
restore; last/recent closed windows start directly from saved targets without
loading a default page. Hidden/paused ordinary URLs enter bounded snapshots,
while private Lea URLs stay excluded. Watch items skipped by limits stay unread.
`Bulk-opening-host.patch` follows `Lazy-restore-host.patch`; that milestone added
three Boot cases. See `LeaForge/BULK_OPENING.md` for behavior and limitations.

Saved sessions now honor the existing `lazyRestoreMode=off/background/all`
policy in Lea. Background loads only the first new page; all initially creates
only placeholders, with no renderer or page requests. Explicit opens preserve
existing tabs and fields; identity switching replaces old contexts instead.
Private flags survive deferred resume. The default remains off. The nineteen-case
Boot suite and six actual-EXE startup cases cover these routes, including the
Resume button. `Lazy-restore-host.patch` follows `Suspension-host.patch`.
See `LeaForge/LAZY_RESTORE.md` for bounds and the original milestone's scope.

Lea tabs can now suspend into lightweight placeholders and resume in Lea on
the same tab. Current/background/group/overview commands preserve the URL and
private flag, dispose old views, and cancel pending requests. Hidden/paused
tabs participate in bounded automatic sessions; private URLs remain excluded.
Reload is visible again. `Suspension-host.patch` follows `Navigation-host.patch`.
The Boot suite includes lifecycle and saved-group startup tests.
See `LeaForge/SUSPENSION.md` for fresh-reload semantics and unmigrated routes.

Lea Home now honors the saved homepage, and the original hidden/disabled Home
menu is available. Homepage/site-root new tabs, previous-session restoration,
and bounded clipboard URL opening use Lea. Site roots retain ports and IPv6;
both left-close variants share mixed-engine disposal and private-history guards.
`Navigation-host.patch` follows the milestone 52 host patch. The two new Boot
cases bring the form-start suite to fourteen. See `LeaForge/NAVIGATION_COMMANDS.md`
for behavior, bounds, and the distinction between clipboard parsing tests and
unautomated OS clipboard I/O.

Duplicate, close/reopen, close-other/right, bookmarks, reload-all, and named
sessions now use engine-aware host paths. Close Tab is no longer hidden, and
the last legacy tab closes through normal cleanup. Private Lea URLs are excluded
from recent history and automatic session saving; explicit named-session opens
can create private views. See `LeaForge/TAB_SESSION_COMMANDS.md` and the matching
`Tab-session-host.patch` for details and the commands regression case.

Milestone 49 also fixes late legacy Navigating/Navigated/DocumentCompleted
callbacks while a Lea tab is selected, background progress updates, owner-tab
locking, missing-tab notification ticks, and incorrectly typed address/search
selection handlers. `Startup-host.patch` records only this milestone's edits
to the existing host `1.vb`; it requires the matching `FirespiderLea.vb` helper.
Apply it only after `git apply --check --ignore-space-change` succeeds in the
host checkout. It is already applied to `L:\Firespider Internet`.

Firespider's Close Tab command, tab-strip close target, and middle-click now
close Lea tabs safely. The existing Page Source window displays Lea's raw `V.H`
HTML. Open File loads local pages into the selected Lea view, Save Page writes
that source as UTF-8, and Send Link builds its message from Lea's title and URL.
Up to 12 Lea URLs restore from `%LocalAppData%\Firespider Internet\Lea.session`;
private mode deletes that file instead of saving it. Legacy session settings
remain owned by Firespider.

Both saving and restoration are capped at twelve URLs, 4,096 characters per
URL, and 64 KiB per file. Oversized, locked, or inaccessible session files are
skipped with a status message, without changing the original file during the
restore operation. Invalid entries and unsupported schemes are ignored.

Firespider's private-mode flag also drives `V.Pv`. Normal Lea cookies stay in
their bounded in-memory jar. Existing private Lea views remain private when
the global flag is turned off; new ordinary tabs start ordinary contexts.
Explicit embedder changes to `V.Pv` reset the private jar, restore the separate
normal jar when leaving private mode, and clear the page cache.
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
Familiar literal `document.getElementById("id")` statements can now express the
same bounded text, value, boolean state, focus, classList, and display/color/font/width/height
operations. `V.Js` and `V.Ck` translate them to Lea's short commands; Firespider
does not receive a DOM object or host callback. The compiled smoke verifies this
path and confirms that `innerHTML` remains rejected.

Compact `wd(id,value)` and literal `style.width` use Lea's bounded fixed,
percentage, and `calc(P% +/- Npx)` parser. Authored width/min/max values compile
to the same optional numeric tuple, and the hosted view resolves them through
normal block, field, image, flex, grid, table, paint, hit, and accessibility
layout. Firespider adds no expression evaluator or adapter-side sizing logic.
Compact `ht(id,value)`, readable `height(id,value)`, and literal `style.height`
use the matching parser. Authored height/min/max tuples resolve against definite
viewport, retained-parent, positioned, flex, grid, and table references; an
auto-height parent leaves percentage children on natural layout.
Classic bounded `vw`, `vh`, `vmin`, and `vmax` values use the same width/height,
minimum/maximum, command, and literal paths. Compact `S.Wu/Hu` unit aliases and
`V.Vh` retain the live viewport reference inside Lea, including auto-height
parents and height-only resizes; Firespider adds no adapter-side unit handling.
Bounded `em` and `rem` values use that route as well. `em` follows the target's
computed font; `P.Rf` and `V.Rf` expose the explicit and applied root font used
by `rem`. The host smoke verifies both units, calculations, commands, reset,
and exact live custom-surface geometry.
Bounded two/three-term `min()` and `max()` plus three-term `clamp()` reuse those
same dimension paths. `S.Wk/Hk` and `Wq/Hq/Wv/Hv` expose compact lazy records;
the host does not parse or resolve function arguments. The compiled smoke checks
authored and command geometry, min/max constraints, literal style updates,
reset, invalid preservation, and the live post-paint viewport. See
`LeaForge/SIZE_FUNCTIONS.md`.
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
Literal `location`, `location.href`, and `location.assign(...)` forms use this
same event. Local relative queries/fragments remain URI components, and
replacement/history APIs remain unsupported.
Lea tabs also expose the bounded `V.Tm` request deadline: 15,000 ms by default,
clamped to 100-60,000 ms, with typed timeout and cancellation failures.

Same-page `#fragment` links stay inside the current Lea page and never enter the
host loader. Compact `Ft/Fg/Rv` lookup, retained history, closed-details reveal,
and retained `Fa/Fj/Fy` placement through layout settlement still raise the
normal `Changed` event, so Firespider's address bar and Back/Forward controls
update without adapter-specific DOM code.

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
border-box sizing. Lea applies it to fixed/percentage/calculated width, height,
and min/max values plus pixel flex basis
before the shared block, field, image, flex, table, cell, hit, paint, and
accessibility-bound paths, without adding a native browser dependency.

Rounded CSS boxes stay in that same path. Firespider can inspect clockwise
`S.R0/R1/R2/R3` horizontal and `Q0/Q1/Q2/Q3` vertical aliases for bounded pixel
or percentage circles and ellipses. Lea normalizes oversized opposing corners
and reuses one curve for backgrounds, solid/dashed/dotted borders, descendant
overflow clips, and ordinary fields. The adapter adds no rounded host controls;
its smoke verifies the compiled browser receives the compact radius model.
Logical corner longhands and all nine bounded physical/logical radius commands
feed those same fields for horizontal LTR/RTL pages.

Responsive CSS also stays inside that surface. `V.Vw/Vh` follow the hosted
control's client viewport, while `P.Vw/Vh` and `G.Vw/Vh` expose the applied
pair to the page and Inspector. Bounded screen/all pixel min/max/exact width
and height, portrait/landscape, reduced integer aspect-ratio ranges,
feature/value-first and same-direction chained strict/inclusive comparisons,
conjunction, comma, nested, and style media conditions switch compiled rules
on resize without replacing live field models. The host smoke verifies
portrait, short-landscape, tall/wide/intermediate ratio and range states,
narrow, restored wide, diagnostics, and retained fields plus classic
viewport-unit geometry and commands. See `LeaForge/MEDIA_FEATURES.md`,
`LeaForge/MEDIA_ASPECT_RATIO.md`, and `LeaForge/MEDIA_RANGE_SYNTAX.md`.

Bounded `@supports` feature queries remain inside the same engine surface.
Declaration conditions reuse Lea's value parsers, `selector(...)` reuses its
compiled selector grammar, and grouped `not`/`and`/`or` conditions can nest
with media rules. The compiled host smoke verifies wide and narrow states,
exact geometry, compact `@s` plus `@m` diagnostics, intermediate restoration,
and retained live fields. Firespider adds no capability API or condition parser.
See `LeaForge/SUPPORTS_QUERIES.md`.

Bounded named cascade layers also remain entirely inside Lea. The compiled host
smoke verifies later-layer normal order, reversed important order, unlayered
normal precedence, supports/media nesting, compact `@l` diagnostics, wide,
narrow, and intermediate geometry, and retained live fields. Authored and
native `Ss` sheets share the engine's bounded layer ranks; Firespider adds no
layer state or parser. See `LeaForge/CASCADE_LAYERS.md`.

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
strict password/file-value exclusion, direction-aware visual-line movement,
rectangles, point lookup, and partial-range cropping. Visible-range arrays use a stable empty fallback because
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
the adapter adds no SVG control or external runtime. Curves, cumulative
transforms, preserveAspectRatio modes, linear/radial gradient fills, nonzero or
evenodd fill/clip winding, caps, joins, miter limits, and dash styles use the
same model and paint path. The host smoke verifies a compiled
Firespider tab's exact vector size, curve steps, inherited matrix, compact
gradient definitions/stops, resolved paints, reusable group expansion, compact
clip definitions/references, stylesheet stroke aliases, and graphic accessibility name.

The host project imports `Lea/FirespiderLea.targets`, which adds the two DLL
references, compiles `FirespiderLea.vb`, and copies `Lea.Image.exe` plus `start.html` to the output
`Lea` folder. Lea tabs use no Chromium, WebView, Safari, or hidden browser
control. The normal initial-page path now uses Lea. The explicit fallback and
advanced legacy-only commands remain; the entire host is not yet Lea-only.
If SDK `dotnet build` forces old binary `.resx` files through its newer resource
serializer, build this legacy host with the installed .NET Framework MSBuild:
`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe`. Lea source and
resources do not need to be changed for that tooling mismatch.

The adapter is kept here as the clean source of record because the live
Firespider working tree contains unrelated in-progress changes.

Hosted Lea pages also expose bounded CSS box shadows through `S.Sw` and compact
`Sw.X/Y/B/R/C/I` state. The host smoke compiles outer plus inset shadows on its
existing rounded border-box fixture and verifies that visual shadows do not
alter the host-visible rectangle.

Hosted Lea pages expose bounded CSS background gradients through `S.Gi`,
`Gi.K/A/X/Y/Ss`, and `Cs.C/P` as well. The host smoke compiles a linear gradient
onto the same rounded box and verifies its kind, angle, normalized stops, and
colors through the public model without adding adapter-side paint logic.

Hosted Lea pages also accept bounded legacy and modern HSL/HSLA colors through
the existing authored-color records. Unitless, degree, gradian, radian, and turn
hues, normalized wrapping, percentage channels, numeric/percentage alpha,
shadows, gradients, generated text, fields, and inline SVG all use the shared
engine and WinForms paths. The compiled host verifies exact background, border,
shadow, and gradient pixels without adding a Firespider color converter.

Modern `hwb()` colors share those records and paint paths. The engine handles
clamped whiteness/blackness, CSS gray normalization, numeric/percentage alpha,
variables, layers, media, fields, shadows, gradients, generated text, and SVG.
The compiled host verifies the public state and exact mixed-background, border,
shadow, and gradient pixels without adding a host-side converter.

Modern CIE `lab()` and `lch()` colors also stay inside Lea. The engine handles
numeric/percentage channels, four LCH hue units, alpha, D50 Lab conversion,
Bradford D50-to-D65 adaptation, and bounded local-MINDE sRGB gamut mapping
across variables, layers, media, fields, shadows, gradients, generated text,
and SVG. The compiled host verifies public state plus exact background, border,
shadow, and gradient paint without a Firespider color-space implementation.

Bounded predefined CSS `color()` values also stay entirely inside Lea. The
engine handles sRGB, linear sRGB, Display P3, linear Display P3, XYZ D50/D65,
the `xyz` alias, optional alpha, Bradford adaptation, and local-MINDE sRGB gamut
mapping across the existing shared paths. The compiled host verifies public
state plus exact background, border, shadow, generated-content, and gradient
paint without adding a Firespider color-space implementation.

The remaining CSS Color 4 predefined RGB spaces also stay inside Lea. A98 RGB,
ProPhoto RGB, and Rec. 2020 use their bounded extended transfer curves and XYZ
matrices; ProPhoto shares Lea's Bradford D50-to-D65 adaptation before the
existing local-MINDE sRGB gamut path. The compiled host verifies public state,
UI Automation reachability, and exact background, border, shadow, generated
content, and gradient paint without any Firespider color conversion.

Modern `oklab()` and `oklch()` colors follow the same host-neutral route. Lea
handles numeric/percentage channels, four hue units, alpha, D65 conversion, and
bounded local-MINDE sRGB gamut mapping across variables, layers, media, fields,
shadows, gradients, generated text, and SVG. The compiled host verifies public
state plus exact background, border, shadow, and gradient paint without a
Firespider color-space implementation.

Bounded two-stop `color-mix(in srgb, ...)` expressions, including CIE Lab/LCH
and Oklab/OkLCh base stops, also flow through the
existing authored-color records. Omitted and normalized weights, low-total
alpha, premultiplied transparent/translucent stops, `currentColor`, variables,
layers, media, fields, shadows, gradients, generated text, and SVG need no host
logic. The compiled host verifies exact background, current-color border,
shadow, and gradient pixels.

Hosted Lea pages expose bounded bitmap backgrounds through `S.Bi` and compact
`Bi.U/R/X/Y/Xp/Yp/W/H/Wp/Hp/Wa/Ha/K` state. The host smoke compiles a data
image with no-repeat, bottom-right positioning, pixel sizing, and rounded
clipping, then verifies both the public model and exact custom-surface pixels.
Firespider adds no image control or adapter-side background painter.

Hosted Lea raster images expose bounded `object-fit` and `object-position`
through compact `S.Of=f/c/v/n/s` and `Ox/Oy/Oxp/Oyp` state. The host smoke
compiles a two-color image with contain/center fitting, verifies letterbox and
source pixels on the custom surface, and confirms that mapped-area keyboard and
accessibility rectangles follow the fitted pixels. Firespider needs no native
image control or adapter-side fitting logic.

Hosted Lea pages expose bounded CSS `aspect-ratio` through compact
`S.Rw/Rh/Ra` state. The host smoke compiles a `2/1` ratio, verifies the public
aliases and exact `160x80` custom-surface geometry, and samples the painted
background. Firespider adds no ratio calculations or extra native control.

Hosted Lea pages expose bounded CSS outlines through compact `S.Ow/Os/Oc/Oo`
state. The host smoke compiles a positive-offset solid outline, verifies its
public aliases and unchanged `150x44` box geometry, and samples the exact paint
outside that box. Firespider adds no outline calculations or extra control.

Hosted Lea pages expose inherited CSS cursor and pointer targeting through
compact `S.Cu/Pe` state. The host smoke compiles one live pointer link and one
mouse-inert not-allowed link, verifies their public aliases, confirms only the
live element enters the retained hit list, and checks WinForms cursor mapping
plus underlying fallthrough. Keyboard and accessibility routes stay owned by
Lea and require no Firespider-side event filter.

Hosted Lea text uses inherited `S.Tt=n/u/l/c` for none, uppercase, lowercase,
and capitalize presentation plus shared `E.Tx` host conversion. The host smoke
compiles an uppercase block with a lowercase inline run, a lowercase field,
and a responsive transform. It verifies compact aliases, transformed retained
text, exact raw field casing, field identity across narrow/wide rebuilds, and
restored display state. Firespider needs no adapter-side text conversion, and
submitted or focused field values remain authored rather than transformed.

Hosted text also exposes inherited `S.Ls/Wg` letter and word spacing. The host
smoke compares retained widths for identical normal and spaced text, checks
signed responsive values and custom-field aliases, and verifies raw field
identity through narrow/wide rebuilds. Layout and paint remain Lea-owned;
Firespider adds no text-spacing calculations or additional native controls.
Focused native editors and choice popups keep Windows' native text spacing.

Hosted block text now exposes inherited `S.Ti/Tip` first-line indentation. The
compiled host checks positive pixel LTR and RTL inline-start geometry, responsive
percentage resolution, compact `ti` calls, literal `style.textIndent`, and wide
state restoration. Firespider adds no adapter-side text measurements: Lea's
retained layout continues to drive paint, hits, find, focus, and accessibility.
Signed pixels are bounded to -2000..2000 and percentages to -100..100; see
`LeaForge/TEXT_INDENT.md`.

Hosted text also exposes one inherited bounded shadow through nullable `S.Ts`.
The compiled host verifies exact authored paint, signed offsets, optional blur
and color, compact `ts`, literal `style.textShadow`, and narrow/wide responsive
rebuilds. Firespider adds no adapter state or extra controls, and Lea keeps the
same layout, hit, find, focus, and accessibility geometry. See
`LeaForge/TEXT_SHADOW.md`.

Hosted text exposes bounded inherited decoration line/color/style/thickness
through `S.Td/Tc/Tds/Tw`. The compiled host checks atomic shorthand, longhand
component preservation, compact `td/tdl/tdc/tds/tdw`, literal
`style.textDecoration*`, exact custom-color paint, and narrow/wide responsive
rebuilds. Current-color solid lines retain Lea's native-font fast path; custom
double/dotted/dashed/wavy lines add no Firespider controls or adapter state. See
`LeaForge/TEXT_DECORATION.md`.

Hosted underlines also expose bounded inherited offset through `S.Uo/Uop`.
The compiled host verifies signed pixel and percentage state, compact `uo`,
literal `style.textUnderlineOffset`, shorthand independence, custom paint, and
narrow/wide responsive restoration. Default unstyled links remain native until
an offset or custom decoration component needs Lea's small painter. Firespider
adds no adapter state or extra controls. See
`LeaForge/TEXT_UNDERLINE_OFFSET.md`.

Hosted wrapping exposes bounded inherited mode/style through `S.Wm/Wt`. The
compiled host verifies auto and balance geometry, same-line-count variance,
compact `tw/twm/tws`, literal `style.textWrap*`, nowrap/wrap transitions, and
narrow/wide responsive restoration. Balancing stays inside Lea's existing
inline layout pass with fixed work ceilings; Firespider adds no adapter state or
extra controls. See `LeaForge/TEXT_WRAP.md`.

Hosted multi-line text exposes bounded non-inherited clamping through
`S.Lc=0/1..32`. The compiled host verifies standard `line-clamp`, compatibility
`-webkit-line-clamp`, compact `lc`, literal `style.webkitLineClamp`, exact one-
and two-line retained geometry, markers, narrow/wide responsive restoration,
authored text, and field identity. Lea removes hidden boxes and hits inside its
existing inline pass and shortens normal flow; Firespider adds no parser, state,
layout branch, or extra control. See `LeaForge/LINE_CLAMP.md`.

Hosted lists expose bounded inherited marker styles through
`S.Lst=n/d/c/s/1/0/a/A/r/R`. The compiled host verifies HTML type and reversed
sequences, CSS precedence, compact `lst`, literal `style.listStyleType`, alpha
rollover, exact marker geometry, retained fields, and narrow/wide responsive
replacement. Lea formats and places the existing `B.Mk` atom; Firespider adds
no counter, parser, adapter state, layout branch, or extra control. See
`LeaForge/LIST_STYLE.md`.

Hosted list markers now expose inherited outside/inside position through
`S.Lp=o/i`. The compiled host verifies a CSS type/position shorthand, LTR
outside gutter and inside first-line geometry, compact `lp`, literal
`style.listStyle`, type-only independence, responsive position replacement,
and retained fields. Lea owns the measured gutter and shorthand reset;
Firespider adds no parser, counter, adapter state, layout branch, or control.
See `LeaForge/LIST_POSITION.md`.

Hosted lists now expose bounded inherited marker images through `S.Li`. The
compiled host verifies CSS and data-URL images, compact `li`, literal
`style.listStyleImage`, three-component shorthand, exact inside/outside image
geometry, fallback markers, responsive replacement, and retained fields. Lea
reuses its sandboxed image cache/decoder and applies the 64px display cap;
Firespider adds no parser, resource cache, adapter state, layout branch, or
control. See `LeaForge/LIST_IMAGE.md`.

Hosted lists now expose bounded terminal `::marker` styling through `B.Ms` and
`S.Mc/Ma/Mn`. The compiled host verifies isolated marker typography/content,
selector specificity, image/content priority, compact `ms`, readable
`markerstyle`, page defaults, direct item overrides, responsive replacement,
inside/outside LTR/RTL geometry, and retained fields. Lea owns selector
matching, cascade, measurement, and paint; Firespider adds no marker parser,
adapter state, layout branch, or control. See `LeaForge/MARKER.md`.

Marker content can now concatenate up to eight quoted strings and
`attr(name)`/`attr(name raw-string)` values, including one quoted missing-value
fallback, into the existing `S.Mc` atom. The compiled host verifies fallback
resolution, present/empty priority, string/attribute composition, responsive
replacement, compact/readable commands, strict invalid rejection, exact
outside geometry, and retained fields. Firespider still adds no DOM evaluator
or generated-content adapter. See `LeaForge/MARKER_CONTENT.md` and
`LeaForge/ATTR_FALLBACKS.md`.

Hosted marker content also resolves bounded `counter(list-item)` and nested
`counters(list-item, separator)` values through Lea's existing HTML list pass.
The compiled host verifies start/value-aware nested Roman paths, compact and
readable commands, unsupported-name rejection, exact outside geometry, and
retained fields. Firespider adds no counter map, numbering adapter, or layout
branch. See `LeaForge/MARKER_COUNTERS.md`.

Hosted pages now also resolve bounded named CSS counters through engine-owned
`S.Cr/Ci/Cs` and tree-order `N.Ct` snapshots. The compiled host verifies reset,
increment, set, nested output, compact `cr/ci/cs`, literal DOM assignment,
exact outside geometry, and retained fields. Firespider adds no counter parser,
state map, numbering adapter, layout branch, or control. See
`LeaForge/CSS_COUNTERS.md`.

Hosted pages now expose bounded inline `::before`/`::after` generated text
through Lea-owned `be/af` runs. The compiled host verifies string, attribute,
quoted missing-value fallback, named-counter, nested link, table, responsive,
accessibility, compact `be`, and readable `afterstyle` paths with retained
fields and painted geometry. Firespider adds no pseudo parser,
generated-content evaluator, layout branch, or control. See
`LeaForge/GENERATED_CONTENT.md` and `LeaForge/ATTR_FALLBACKS.md`.

Hosted marker and before/after content also supports bounded generated quotes.
The compiled host verifies inherited explicit pairs, nested document-order
depth, accessible link names, compact `qu`, literal `style.quotes`, responsive
pair replacement, geometry, and retained fields. Firespider adds no quote state,
pseudo parser, layout branch, or control. See `LeaForge/GENERATED_QUOTES.md`.

Hosted marker and before/after content now also exposes optional bounded
alternative text through `S.Ma`. The compiled host keeps visual marker/link/
table/responsive output unchanged while verifying substituted accessible names,
explicit empty decoration suppression, compact `be/ms` commands, invalid
preservation, and retained fields. Firespider adds no speech parser,
accessibility adapter, layout branch, or control. See
`LeaForge/GENERATED_ALT_TEXT.md`.

Hosted marker and before/after content now also accepts bounded visual
`url(...)` bitmap fragments. The compiled host verifies mixed text/image
fragments, image-only links, marker geometry, alternatives, compact `be`,
responsive replacement, accessibility, and retained fields. Lea owns the
24-source admission, origin checks, sandboxed decode, 64px intrinsic display
cap, layout, hit testing, and paint; Firespider adds no image-content parser,
cache, decoder, adapter state, layout branch, or control. See
`LeaForge/GENERATED_IMAGES.md`.

Hosted text alignment now exposes the bounded shorthand/all/last family through
`S.Ta/Tl`. The compiled host verifies `justify-all`, retained logical RTL
state, compact `ta/taa/tl`, literal `style.textAlign`, shorthand reset,
longhand preservation, and narrow/wide responsive geometry. Lea resolves the
one-character state inside its existing inline pass; Firespider adds no adapter
state or controls. See `LeaForge/TEXT_ALIGN.md`.

Hosted block text also exposes bounded inherited final-line alignment through
`S.Tl`. The compiled host verifies physical and horizontal LTR/RTL logical
state, compact `tl`, literal `style.textAlignLast`, final and forced-line
geometry, and narrow/wide responsive restoration. Lea reuses its existing flow
pass and retained rectangles; Firespider adds no adapter state or extra
controls. See `LeaForge/TEXT_ALIGN_LAST.md`.

Hosted block text supports non-inherited `S.To=c/e` clip/ellipsis overflow.
The compiled smoke checks LTR/RTL physical inline-end markers, right-aligned RTL
prefix paint, original elided link hits, direction-aware TextPattern range and
point geometry, responsive restoration, and full authored text retention.
Ellipsis is a Lea paint operation; the adapter keeps the same document, tab,
and field objects and does not truncate page strings. Native controls retain
their existing presentation and editing behavior.

`Smoke.cs` loads two compiled Firespider forms off-screen and verifies Lea-first
New Tab creation, the explicit legacy fallback, Inspector open/refresh/clear and legacy isolation, diagnostics,
script/style, literal DOM translation, host-controlled `go`, and safe click execution, alert routing,
ID-scoped `bg/fg/fs` style commands, state commands, retained fields,
autofocus/access keys, keyboard navigation, local
open, find cycling, streamed download saves, page save, send-link metadata,
close routes, source, and session restoration using temporary files rather than
the user's data.

Pass a host root and `--startup` to the compiled smoke for its eighteen focused
startup regressions only. The full smoke includes them by default. The separate
`Lea.Build/Test-FirespiderStartup.ps1` launches the actual EXE for clean, restored,
locked-session, and oversized-session cases and checks responsiveness and UIA
error-dialog text. It closes only the processes it starts.
