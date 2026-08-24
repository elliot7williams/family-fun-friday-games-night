# Lea Alias Rules

Aliases are a defining part of Lea. They keep small browser programs quick to
read and quick to write.

## Naming rules

- Core objects use one letter: `L` is the entry point, `E` is the engine, `P` is a page.
- Actions use two or three letters: `Go`, `GoA`, `Bk`, `Fw`, `Rl`, `RlA`, `St`, and `At`.
- Page values use one or two letters: `U`, `T`, `H`, `Q`, `D`, `N`, `Bs`, and `Ls`.
- Page commands use one or two letters: `t`, `a`, `bg`, `fg`, `fs`, `v`, `tx`, `ac`, `rc`, `tc`, `hd`, `sh`, `op`, `cl`, `tg`, `ds`, `en`, `vl`, `ck`, `fo`, and `go`.
- New public aliases should be no more than three letters.
- Longer names may remain inside the engine when they make the implementation safer to maintain.

## Tiny example

```csharp
var e = LeaForge.L.E();
var p = e.Go("test-pages/short.html");
Console.WriteLine(p.T);
```

The aliases are the small public language of Lea. The readable internal classes
are the implementation underneath it.

Lea's first script bridge is allow-listed rather than a general JavaScript VM.
`E.Je` and `V.Je` enable page commands, while `E.Js(page, code)` and
`V.Js(code)` run a bounded host-provided command string. `P.Jc` counts accepted
commands and `P.Ms` holds bounded alert messages. `V.Al` fires once for each new
message and `V.Am` exposes the current message. Supported page commands are:

```text
t("Title")       title
a("Message")     alert message
bg("#f0fff8")    body background
fg("#164e3f")    body text color
fs(20)           body font size
bg("id", "#fff") element background
fg("id", "#222") element text color
fs("id", 18)      element font size
v("who", "Lea")  bounded page variable
tx("id", "Hi ${who}") element text
ac("id", "hot")       add class
rc("id", "hot")       remove class
tc("id", "hot")       toggle class
hd("id")              hide element
sh("id")              show element
op("id")              open details
cl("id")              close details
tg("id")              toggle details
ds("id")              disable field
en("id")              enable field
vl("id", "value")     set field value
ck("id", "1")         set checked state; one argument toggles
fo("id")              request focus
go("next.html")        request host navigation
```

The two-argument `bg/fg/fs` forms target one validated ID. They reuse the same
hex/named-color and 8..96 pixel font bounds as the body forms, reject document
metadata targets, cap the resulting inline style at 4,096 characters, and
preserve mutable fields through the shared CSS rebuild.

`P.Vs` exposes at most 16 case-insensitive variables. Names are one through 16
letters, digits, or underscores and must begin with a letter; values are capped
at 256 characters. `${name}` performs one bounded literal substitution in later
commands. There are no expressions or recursive substitutions.

`ac`, `rc`, and `tc` add, remove, or toggle one class on a safe element ID.
Class names use one through 64 ASCII letters, digits, hyphens, or underscores.
Each element is limited to 32 classes and 512 class-attribute characters.
Duplicate adds and absent removes are accepted no-ops; remove and toggle clear
all matching tokens. A successful run reapplies compiled CSS once and preserves
mutable form state.

`hd/sh` hide or show one safe ID. Command hiding remains authoritative after
author CSS and excludes the subtree from layout, focus, and semantics. Lea
retains an existing field model by DOM ownership while it is hidden, so its
edited value and checked state return when shown. `ds/en` apply only to input,
textarea, select, and button targets and update rendered, focus, form, and
accessibility state in the same rebuild.

`op/cl/tg` open, close, or toggle one bounded `details` disclosure. The ID may
name the `details` element or its first direct `summary`; later summaries and
unrelated elements are rejected. Open and close accept an already-matching
state as a safe no-op, synchronize the compact `N.Op` state with the `open`
attribute, and preserve edited fields through the shared retained rebuild.

`vl` changes one supported field's current value after bounded literal
expansion. Values stop at 1,024 characters; file, submit, reset, and ordinary
button values cannot be injected. Colors require `#rrggbb`, and selects accept
only the exact value of an enabled bounded option. Password values remain
redacted from diagnostics and accessibility. `ck` toggles a checkbox or radio
with one argument, or accepts `1/0`, `true/false`, `on/off`, or `yes/no` as its
second argument. Selecting a radio clears its bounded same-form/name group.

`fo` queues at most one enabled, non-hidden, statically focusable ID per command
run; the first request wins. Headless hosts call `E.Kp(page)` to take and clear
it. `V` consumes it automatically after final layout, ignores CSS-hidden or
clipped targets, clears focus made invalid by another command, scrolls a valid
target into view, and opens its editor only when the Lea surface already owns
input focus.

`go` queues one navigation request rather than loading a page directly. Its
expanded input is limited to 2,048 characters and resolves against `P.Bu`; the
resolved absolute URL is capped at 4,096 characters. HTTP and HTTPS are allowed.
Local file targets are allowed only from a local file response and UNC paths are
rejected. Empty values and `about`, `data`, `javascript`, `mailto`, and other
schemes are ignored. Only one request can be pending at a time.

Headless hosts call `E.Gt(page)` to take and clear the pending request. WinForms
hosts read `V.Gu` when `V.Gr` fires. A command request suppresses the same
click's default link or submit route; the host remains responsible for calling
`Go`/`GoA`, applying policy, reporting errors, or declining the request.

`E.Ck(page, element)` and `V.Ck(element)` run the same allow-list from an
element's `data-lea` attribute, falling back to `onclick`. The WinForms view
calls `Ck` for button and link hits. `tx/text` replaces only the text children
of a safe element ID and cannot target the document, head, body, title, script,
style, link, or metadata nodes. Class commands use the same target boundary.
Click code cannot access files, processes,
native controls, or arbitrary JavaScript APIs.

Developer diagnostics use `Dg`: `E.Dg(page)` and `V.Dg()` return a bounded `G`
snapshot. `Dcl()` clears request activity without touching navigation, cache,
cookies, page messages, or variables. `G` uses these compact values:

```text
U/T       URL/title
St/Ct/Sz  response status/type/bytes
Ca        response reused cached content
N/B/L/F   nodes/blocks/links/fields
C/J/V/M   CSS rules/commands/variables/alerts
Vw        applied viewport width in pixels
Vc        document custom-property count
Cc/Ch/Kc  cache entries/reuses/cookies
Dc/Db     persistent cache entries/bytes
Dh/Dw     persistent hits/stores
Dr/De     persistent recoveries/evictions
Ns/Cs     bounded node/CSS lines
Rs/Ms/Vs  request/message/variable lines
```

Snapshots retain at most 512 node lines, 256 compiled CSS summaries, and the 32
latest requests. Node lines expose structural IDs, classes, types, names, and
resource addresses, but never mutable field values, passwords, or selected file
paths. The test browser opens its inspector with `F12`, `Ctrl+Shift+I`, or `I`.

Lea also recognizes literal `document.title`, `document.body.style` color/font
assignments, and `alert(...)`. Unknown calls, expressions, nested control flow,
external scripts, and non-script data blocks do not execute.

`GoA` and `RlA` are the asynchronous forms used by browser windows. `St` cancels
an active network request. `Tm` is the per-request network deadline in
milliseconds. It defaults to 15,000 and clamps to 100 through 60,000 on both
`E` and `V`. The same bound covers synchronous requests, asynchronous page and
POST work, and streamed network-download reads. A deadline raises retryable
`Er.K == "timeout"`; `St` remains the distinct non-retryable `cancel` result.
The shorter synchronous forms remain useful for tests.

`P.U` is the active page URL and changes only its fragment during retained
same-page navigation. `P.Bu` is the document base URL resolved from the first
valid `<base href>` using at most 4,096 characters; unsupported bases are
skipped and pages without one use the response URL. `V.U` and `V.Bu` expose the
same pair. Relative stylesheets, links, images, forms, and downloads use `Bu`.

`P.D` is the document root. Each node uses `K` (kind), `X` (text), `A`
(attributes), `C` (children), and `Av(name)` for a short attribute lookup.
`P.Q` preserves the URL query, including on local `file:` pages where older
.NET Framework URI parsing does not expose it through `Uri.Query`.

Paint-ready blocks expose compact computed style through `B.S`. `D` is the
normalized display text. For Lea's bounded flex subset, `Fd` is `r/x/c/y` for
row, row-reverse, column, or column-reverse; `Jc` is `s/c/e/b/a/v` for start,
center, end, space-between, space-around, or space-evenly. `Ai` is the container
cross-axis default and `As` is each direct item's override; both use `t/s/c/e`
for stretch, start, center, or end, while empty `As` means auto. `Wr` is `n/w`
for nowrap or wrap. `Gp` and `Rg` are 0..512 pixel column and row gaps
from one/two-value `gap`, `column-gap`, or `row-gap`. Each direct item uses `Fg`
for a 0..64 integer growth weight, `Sh` for a 0..64 integer shrink weight, and
`Fb` for a 0..9,999 pixel basis; `Fb=-1` means auto/content. `Or` is a signed
-64..64 item order. The common
one-to-three-part `flex` forms fill the same state. Shrink distribution uses the
outer pixel basis and freezes at explicit `Wn/Hn`; `Wx/Hx` clamp the base first
and `Bz` sizing is applied before outer-slot distribution. A locked or
exhausted axis may overflow visibly. Stable order is active on
nowrap rows and fixed-height columns. Auto-height columns retain source order
and natural stacking with `Rg`; fixed-height columns add weighted vertical
growth/shrink, reverse flow, justification, and cross-axis alignment. Intrinsic
min-content sizing, fractional factors, nonzero percentage bases, wrapped
ordering, column wrapping, wrap-reverse, and wrapped cross-axis alignment remain
outside the subset.

Document lookup stays short and bounded. `E.Id(page, id)` finds the first ID,
`E.Q(page, selector)` finds the first document-order selector match, and
`E.Qa(page, selector)` returns a read-only list of at most 256 matches. Queries
reuse Lea's compiled CSS subset and accept up to 1,024 characters, 16 groups,
and 16 total selector parts. `V.Id` and `V.Qa` forward the same operations;
`V.Q1` is first-match because the established `V.Q` alias remains the page URL
query string.

Fragments use three defining aliases. `E.Ft(page, fragment)` and `V.Ft` decode
one value capped at 4,096 characters, then find the first ID or legacy named
anchor. `E.Fg(url)` and `V.Fg` retain the current page, update `U`, and add one
entry to a 128-entry history without fetching; cross-document URLs return `0`.
`E.Rv(page, target)` and `V.Rv` reveal any closed details ancestors in one
state-preserving rebuild. Normal `Go`/`GoA`, links, and Back/Forward share this
path. Missing targets retain the current scroll, `#` returns to the top, and
valid targets are placed again after resize.

Progress and meter blocks expose `B.Vm`, one compact `Vm` value model. `V` is
the normalized value, `Mn/Mx` are the range, and `R` is the bounded 0..1 paint
ratio. Meter models also expose ordered `Lo/Hi` boundaries, optimum point `Op`,
and quality region `Q` (`0` optimum, `1` suboptimal, `2` least good).
Progress uses `In` for the indeterminate state. Numeric attribute text is capped
at 64 characters, finite values are capped at +/-10^12, malformed values use
HTML defaults, and every result is clamped before layout or painting.

Output, data, time, and edit elements expose one compact `Dv` value model. `P.Dvs` is the
page's ordered value list, while `B.Dv` and `Ir.Dv` connect block and inline
paint records to the same object. `K` is `o` for output, `d` for data, `t` for
time, `i` for inserted text, or `r` for removed text. `V` is current or
machine-readable content and `X` is visible text. `Dt` exposes bounded authored
`datetime` metadata; a time element without that attribute uses its text.
`U` resolves an edit's `cite` through `P.Bu` and accepts only file, HTTP, or
HTTPS targets. Citations stay private metadata and do not create links or
navigate. Output metadata uses `N` for name, `Fo` for form owner, and `F` for
ordered unique `for` ID tokens; `El` retains the owning document element.
Values and text are capped at 4,096 characters, datetime text at 128, names at
256, form/reference tokens at 128, citations at 4,096 input characters, and
references at 32. `tx/text` remains capped at 512 characters and rebuilds all
aliases without changing authored data/time machine values or duplicating models.

Image maps use two compact models. `P.Mps` is the ordered list of the first 32
valid unique named maps, and `B.Mp` connects a rendered image to its map. `Mp`
uses `N` for its name, `As` for up to 64 descendant areas, and `El` for its
document element. Each `Ar` uses `K` (`r/c/p/d` for rectangle, circle, polygon,
or default), `Cs` for coordinates, `U` for an optional resolved link, `T` for
bounded alternative text, and `El` for ownership. Rectangles and circles keep
four and three coordinates; polygons keep at most 128 and drop one odd final
value. Coordinate text and hrefs stop at 4,096 input characters, finite values
clamp to +/-1,000,000, and alt text stops at 512 characters.

`usemap` accepts one bounded percent-decoded hash-name reference. The first map
name wins case-insensitively; invalid whitespace names and non-hash references
do not bind. `B.Mw/Mh` retain the image's authored map canvas, falling back to
natural image dimensions in the renderer. Area hits scale to the settled image,
clip with it, and use first-area priority. An area without `href` remains a dead
shape that masks lower areas, while malformed or zero-radius shapes remain
host-readable but create no hit. Area links join `P.Ls`; click commands,
fragments, navigation, and downloads use the existing host-controlled routes.

Keyboard focus stays compact. `E.Ti(element)` and `V.Ti(element)` return one
nullable authored `tabindex`, capped to -32,767 through 32,767 after at most 16
characters; omitted and malformed values return null. `V.Ke` is the current DOM
focus anchor, `Kn` is the bounded sequential target count, `Ki` is its one-based
index or zero, and `Kb` is its visible focus bounds. `Kf(back)` moves forward or
backward with wrapping, while asynchronous `Ka()` activates the current target.

Lea scans at most 4,096 rendered map-region candidates and retains the first
256 total targets after ordering. Positive tabindex
values come first in ascending order and preserve document order on ties;
zero, omitted, and malformed values then use document order. Negative values
remain click focusable but stay outside `Kn/Ki` and Tab traversal. Supported
targets are valid links, each rendered image-map link region, enabled fields,
valid summaries, and rendered elements with a valid explicit tabindex. One map
used by multiple images creates one independently scaled target per image.
Hidden, disabled, malformed empty-map, and fully clipped targets do not enter
the sequence.

Tab and Shift+Tab drive `Kf`; Ctrl+Tab and Alt+Tab remain host/system shortcuts.
Enter activates links and image-map areas, while fields and summaries reuse
their existing activation paths. Space does not activate links. Focus reapplies
`:focus`, scrolls off-screen targets into view, and paints a default high-
contrast ring even when the page supplies no focus CSS. Pointer and keyboard
links share the same commands, fragments, downloads, and host navigation route.

Autofocus and access keys reuse those targets. `E.Afs(page)`/`V.Afs()` return
the first 32 authored `autofocus` candidates in document order, and `Af`
returns the first candidate or null. On a load without a resolved fragment
target, `V` focuses the
first candidate that has one visible, enabled rendered target; non-focusable,
HTML-hidden, CSS-hidden, disabled, clipped, and excess candidates are skipped.
A resolved URL-fragment target suppresses autofocus. If the Lea surface already owns input
focus and the target is editable, its compact native editor opens so typing can
begin; loading a page does not itself force the browser window to the front.

`E.Aks(element)` returns up to 16 unique authored alternatives after scanning
at most 256 `accesskey` characters. Lea's current keyboard subset accepts ASCII
letters and digits and normalizes them to uppercase; `Ak` returns the first or
an empty string. `V` assigns alternatives to visible focus targets in document
order, retains at most 64 assignments, and resolves collisions by trying the
next authored alternative. `V.An` is the assigned count, `Ak(element)` is the
actual assigned key, and `Kl(element)` is its `Alt+Shift+K` Windows label.
`Aa(key)` focuses and activates one assignment, returning `1`, or returns `0`
for an unmatched key. Links, mapped areas, fields, summaries, safe commands,
fragments, downloads, and navigation share `Ka`; generic tabindex regions are
focused without inventing activation behavior.

Actual `Alt+Shift+key` input first follows WinForms/Firespider command routing.
Only an unclaimed combination with a current page assignment is consumed;
unmatched combinations and other modifier sets remain available to browser and
operating-system chrome. HTML `hidden` on a target or ancestor excludes it from
focus, autofocus, and access-key collection even when page CSS does not mention
the attribute. Lea's default style also omits HTML-hidden nodes from layout,
painting, links, fields, and hit regions. Author `display` CSS can override the
presentation hint, but not the focus/access-key exclusion.

Semantic accessibility uses compact `Ax` records. `P.Axs` is a document-order
flat list capped at 512 records; `Ax.Up` links the nearest included semantic
parent without duplicating the `N` tree, and `El` retains DOM ownership. Hosts
can call `E.Axs(page)` or `E.Ax(page, element)`; `V` forwards these as `Axs` and
`Ax(element)`. Each record uses `K` for role, `N` for accessible name, `D` for
description, `V` for a bounded live value, `St` for state bits, `Lv` for a
heading level, and `U` for a resolved link URL.

Names follow a bounded author-first subset: up to 16 `aria-labelledby` ID
references, then `aria-label`, HTML labels, native alt/caption/legend/summary
text, supported name-from-content roles, and title fallback. Descriptions use
`aria-describedby`, `aria-description`, then unused title text. Name,
description, and value strings stop at 512 characters. Passwords and local file
values always expose an empty `V`; mutable field values and disclosure/focus
states remain live across retained records and normal rebuilds.

`Ax.Is(flag)` tests `St`. The public flags are `Ds` disabled, `Ck` checked,
`Mx` mixed, `Sl` selected, `Ex` expanded, `Co` collapsed, `Rq` required, `Ro`
read-only, `Iv` invalid, `Fs` focusable, `Fc` focused, `Ps` pressed, `Bs` busy,
and `Cr` current. Lea includes supported native structure/widgets and the first
recognized concrete token from a bounded explicit `role` list. CSS display
suppression, inherited visibility, closed disclosures, `aria-hidden`, unused
image-map areas, and presentational roles stay out of the semantic list.

On the custom view, `Ab(record)` returns visible client bounds and `Ao` returns
the WinForms accessibility root. `V` exposes document, structure, text, link,
image, table, list, gauge, and form children through MSAA; names, descriptions,
live values, protected password state, focus, expansion, selection, assigned
keyboard shortcuts, hierarchy, hit testing, and default actions all read the
same `Ax` records. Native UI Automation tables reuse `B.Tb`, `Tb.Rs`, `Tr.Cs`,
and `Tc.Sp/El` for concurrent grid/table and grid-item/table-item patterns.
Coordinates are zero-based; the loaded view stops at 256 complete rows, 256
columns, or 512 cells and exposes column spans, containing-grid links, and
semantic row/column headers without a second table tree. This is a bounded
supported subset, not a claim of complete
HTML-AAM, AccName, WAI-ARIA, MSAA, or UI Automation coverage.

The root native provider also exposes a read-only TextPattern over the existing
`P.X` logical document string, capped at 32,768 characters and mapped through at
most 2,048 settled text segments. Ranges are tied to one page generation and
support bounded text/search, read-only and culture attributes, character, word,
line, and document movement, child/point lookup, rendered rectangles, enclosing
and embedded elements, and scroll-to-view. Password and local file values never
enter the stream. Plain wrapped blocks and inline flow produce bounded visual
line starts; `TextUnit.Line` follows those starts, rectangle output merges each
line, and partial ranges crop their first/last segments proportionally. Selection
and editing are unsupported. The current .NET Framework adapter returns an empty
visible-range collection because non-empty managed and manually constructed
interface arrays produced invalid client handles; a future bridge must cross a
different native boundary.

Computed style uses `S`: `F` is foreground color, `Bg` is background, `Fs` is
font size, `Ff` is a font fallback list, `B` is bold, `I` is italic, and `D` is
display behavior. `Td` is decoration using `n/u/s/us` for none, underline,
strike, or both. Generic `Ff` entries use `ss/s/m/c/f/ui` for sans-serif,
serif, monospace, cursive, fantasy, and system UI. `Ta` is
text alignment using `l/c/r/j`; `Lh` is pixel line height and `Lm` is a bounded
percentage multiplier. Lea accepts CSS pixel, percentage, and unitless line
heights, then keeps each painted line tall enough for its largest font.
`Ws` is white-space mode using `n/nw/p/pw/pl` for normal, nowrap, pre,
pre-wrap, and pre-line. `Wb` is word breaking using `n/b/a/k` for normal,
break-word, anywhere/break-all, and keep-all. `Ov` is overflow using
`v/h/c/a/s` for visible, hidden, clip, auto, and scroll. `Vi` is inherited
visibility using `v/h/c` for visible, hidden, and collapse. `Op` is the effective
paint opacity from 0 through 100 after ancestor opacity is included.

Text blocks keep compatible flattened text in `B.X`. When nested markup needs
separate painting, `B.Is` exposes compact `Ir` inline runs; each run uses `K`
(kind), `X` (text), `U` (optional link), and `S` (computed style). Plain text
leaves `Is` empty and stays on the original one-box layout path.

List-item blocks expose `B.Mk` as their paint-ready marker. Unordered lists use
`-`; ordered lists use invariant decimal text ending in `.`. Ordered `start` and
per-item `value` integers are limited to -999999 through 999999, and increments
saturate at the upper bound. Nested lists compute their own sequence from their
nearest list parent.

Disclosure state stays equally compact. A `details` node uses `N.Op` for its
page-local open state. Its first direct `summary` block uses `B.Mk` as `>` when
closed and `v` when open; duplicate or stray summaries keep `Mk` empty. `B.Hd`
marks retained blocks hidden by a closed disclosure, including nested and table
content, without dropping form state or ownership. `E.Ds(page, element)` and
`V.Ds(element)` toggle one valid details/summary pair and return `1`; invalid
elements return `0`. Pointer clicks plus focused Enter/Space use the same path.

Box spacing stays compact: `Mt/Mr/Mb/Ml` are margins and `Pt/Pr/Pb/Pl` are
padding. `N.Up` links a document node to its parent for descendant and child
selectors; `N.A` holds the attributes used by compiled attribute selectors;
adjacent and general sibling selectors use earlier element siblings and ignore
text. `El` links blocks, inline runs, fields, and table cells back to their
element. `E.Dy(page, hover, focus)` reapplies the page's compiled CSS for
`:hover` and `:focus` without reloading it; mutable field values are preserved.
The WinForms `V` calls this automatically from pointer and field events.

Responsive CSS uses the compact `Vw` alias. Hosts may set `E.Vw`; it clamps to
1-8,192 pixels and reapplies rules only when the active rule set changes.
`V.Vw` is read-only and follows the control's client width automatically, while
`P.Vw` and `G.Vw` report the width applied to the page and diagnostic snapshot.
Lea compiles bounded `screen`/`all` queries with pixel `min-width`, `max-width`,
or exact `width`, comma alternatives, four nested `@media` levels, up to eight
alternatives, and `<style media>`. Print, negated, unknown, and non-pixel
conditions stay inactive. Active diagnostic CSS lines use short `@m` prefixes.

Computed styles expose bounded custom properties through `S.Vc` (count) and
`S.Cv(name)` (case-sensitive raw lookup). `G.Vc` reports the body/document count
without copying raw values into diagnostics. Styles inherit their map lazily and
copy only when an element overrides it. Lea compiles up to 128 declarations per
block once per stylesheet rule, then resolves ordinary declarations against the
final custom-property cascade. `var(--name, fallback)` supports nested and comma
fallback values, inline styles, dynamic pseudo-classes, and media rules. Each
style keeps at most 32 properties; names are 3-64 ASCII letters/digits/hyphens/
underscores, raw values are at most 512 characters, recursion is capped at eight,
and expanded values at 1,024 characters. Missing, cyclic, invalid, oversized, or
excess properties make that declaration use its fallback or remain ignored.
Pixel dimensions use `S.W` and `S.H`; positive bounds use `Wn/Wx` for minimum
and maximum width and `Hn/Hx` for minimum and maximum height. `Bz` is `c/b` for
the non-inherited `content-box` default or `border-box`; it applies the same
edge choice to pixel width, height, min/max, and flex-basis values. Border-box
content dimensions subtract visible border and padding, exclude margins, and
floor at the renderer's one-pixel layout minimum. Blocks, fields, flex items,
images, tables, and table cells share that conversion. Borders use
`Bt/Br/Bb/Bl` for side widths and `Bc/Bs` for color and style. `V.Rb(index)`
returns the cached outer border rectangle, while `V.Rc(index)` returns its
content rectangle.

Positioning stays compact: `Pn` is `s/r/a/f` for static, relative, absolute, or
fixed; `Tp/Rt/Bm/Lt` are nullable signed pixel offsets, and `Zi` is the bounded
z-index. The `inset` shorthand expands atomically into those four offsets;
opposing offsets stretch auto-sized absolute and fixed boxes. `Tx/Ty` are
bounded pixel translation offsets accepted from `translate`, `translateX`, and
`translateY`. `Za` marks a numeric z-index rather than `auto`; `Zc` marks a local
opacity, isolation, or translated stacking context. `B.Up` points to the nearest
retained visual, positioned, or clipping parent block. Styled semantic
containers are retained when their background,
border, spacing, or dimensions affect layout; unstyled wrappers stay on the
flat fast path. Auto-height visual parents grow around nested normal-flow
children. Positioned auto-height owners receive one bounded measurement pass
before final placement, so bottom anchors and overflow clips use settled geometry.
Relative blocks and tables shift while retaining normal flow. Absolute
boxes leave flow and use the nearest positioned containing block; fixed boxes
use viewport coordinates, including when nested. Overflow clips inherit through
the parent chain. Numeric positioned z-index, fixed positioning, local opacity,
isolation, and supported pixel translations create compact hierarchical `Zi`
paths; `z-index:auto` does not trap descendants. Paint and hit testing share
that order. Static boxes retain parsed offsets but do not apply them, while
translate offsets apply after normal or positioned placement without consuming
flow.

The WinForms view exposes `Dh` (document height), `Sy` (scroll position), and
`Bc` (cached layout-box count) for compact host diagnostics. `Sc(delta)` moves
the bounded scroll position.

Find-in-page uses `Fd`: `E.Fd(page, query)` counts up to 256 case-insensitive
text occurrences. `V.Fd(query)` builds and highlights up to 256 painted matches;
`Nx()` and `Pr()` cycle forward and backward, while `Fc()` clears the search.
`Fq`, `Fn`, `Fx`, and `Fr` expose the query, match count, one-based current
index, and visible current rectangle.

Downloads use `Dl(address, path)` and asynchronous `DlA`. Local, HTTP, and HTTPS
content streams to a same-folder temporary file before replacing the explicit
destination. `Dm` exposes the fixed 64 MB ceiling. The compact `D` result uses
`U` (final URL), `P` (full path), `N` (file name), `Ct` (media type), `Sz`
(written bytes), and `St` (status). In `V`, `Dr` requests a destination for an
HTML `download` link, `Du`/`Dn` provide its URL and safe suggested name, and
`Ld` holds the latest completed result. Pages never choose native paths.

Response metadata stays short: `P.St` is status, `Ct` is media type, `Ce` is
content encoding, `Ch` is charset, `Sz` is decoded size, and `Ca` reports a
cache-body reuse. `Et` is the response ETag and `Lm` is Last-Modified. The
engine and WinForms view expose `Cm` (memory maximum), `Cc` (memory count),
`Ch` (body reuses), `Cs` (default freshness seconds), and `Cl()` (memory clear).
Stale validators use
`If-None-Match` or `If-Modified-Since`; reload always bypasses this cache.
External stylesheets share this same LRU and its aliases rather than creating a
second hidden cache. Lea loads at most eight linked stylesheets per page and
parses up to 256 KB from each response.

Persistent caching is opt-in: `Cd` is its directory and the empty default means
no disk I/O. `Cx` is the entry ceiling (32 by default, clamped to 0-128), `Cb`
is the byte ceiling (16 MB by default, clamped to 0-128 MB), and `Dc`/`Db`
report current Lea entry count and bytes. `Cdl()` removes only versioned Lea
cache files. Entries use gzip bodies, SHA-256 URL filenames, bounded metadata,
same-directory atomic replacement, and last-use timestamps for LRU trimming.
Fresh entries survive a new `E`/`V`; stale ETag or Last-Modified entries retain
conditional `304` reuse. Reload stays unconditional, `no-store` is removed,
malformed entries fall back to the network, and private mode never reads or
writes the persistent cache. `G.Dc/Db/Dh/Dw/Dr/De` expose bounded footprint,
hit, store, recovery, and eviction diagnostics without response bodies.

Cookies stay compact: `Kc` is the active cookie count, `Km` is the bounded
maximum, and `Kcl()` clears the active jar plus page cache. `Pv` switches to a
fresh isolated private jar; leaving private mode destroys it and restores the
normal in-memory jar without allowing cached pages across the boundary.

Loading failures use `Er`: `K` is the failure kind, `U` is the address, `St` is
an HTTP status when available, and `Rt` reports whether retrying may help.

Forms use `Fi`: `K` is field kind, `N` is name, `V` is the editable value, `P`
is placeholder text, `C` is checked, and `Ds`/`Ro` are disabled/read-only.
`F`, `U`, and `M` identify the owning form, action URL, and method. `D` and `Dc`
hold reset defaults. Pages expose fields through `P.Fs`, while `B.I` links a
paint block to its field. The WinForms view raises `Ed` for edits and `Sb` for
form actions.

File and richer field aliases stay short: `Fp` contains selected local file
paths, `Ac` is the accept hint, `Mu` enables multiple files or email addresses,
`Sp` is the numeric/range step, and `En` is `u/m` for URL-encoded or multipart.
`V.Sf(field, paths)` safely selects up to eight existing files, with a 4 MB
per-file and 8 MB combined limit. HTML-provided file values are ignored. Range
fields paint as click-to-set sliders; color fields paint a swatch and open the
native color picker. Time, month, week, and local datetime remain compact text
editors with type-aware validation.

Validation metadata remains short: `Rq` is required, `Mn/Mx` are text-length
bounds, `Pa` is a bounded pattern, and `Lo/Hi` hold number or ISO-date bounds.
`Nv` bypasses checks for `novalidate`/`formnovalidate`; `Ok` and `Vm` expose the
mutable validity result and compact reason. `V.Vf(button)` validates one form,
while `Sm/SmA` call it automatically before raising `Sb` or navigating.
Messages additionally cover `file/files/large/step/color/time/month/week` and
`datetime-local` where applicable.
The CSS `:valid` and `:invalid` pseudo-classes follow the same result state.

Select fields expose options through `Fi.Os`; each compact `O` has `V` (value),
`T` (text), and `Ds` (disabled). `Si` is the selected index and `Di` is its
reset default.

Hosts can call `Rs(field)` to reset a form and `Sm(field)` or `SmA(field)` to
submit GET, URL-encoded POST, or multipart POST forms. `E.Po`/`PoA` and
`V.Po`/`PoA` provide direct text POST navigation; `Pb`/`PbA` send a content type
plus a binary body under a 10 MB limit.

Table blocks expose `B.Tb`. `Tb.Cp` is the caption, `Rs` contains compact
`Tr` rows, and `Cs` is the computed column count. Each row exposes cells through
`Tr.Cs`; a `Tc` cell uses `X` for text, `S` for computed style, `H` for a header,
`Sp` for its bounded column span, and `Bs` for nested paint blocks. Plain cells
keep one compact block, while structured cells can expose headings, paragraphs,
emphasis, line breaks, and links through the same short `B` model.
