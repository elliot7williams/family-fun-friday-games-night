# Lea Alias Rules

Aliases are a defining part of Lea. They keep small browser programs quick to
read and quick to write.

## Naming rules

- Core objects use one letter: `L` is the entry point, `E` is the engine, `P` is a page.
- Actions use two or three letters: `Go`, `GoA`, `Bk`, `Fw`, `Rl`, `RlA`, `St`, and `At`.
- Page values use one or two letters: `U`, `T`, `H`, `Q`, `D`, `N`, `Bs`, and `Ls`.
- Page commands use one or two letters: `t`, `a`, `bg`, `fg`, `fs`, `v`, `tx`, `ac`, `rc`, `tc`, and `go`.
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
v("who", "Lea")  bounded page variable
tx("id", "Hi ${who}") element text
ac("id", "hot")       add class
rc("id", "hot")       remove class
tc("id", "hot")       toggle class
go("next.html")        request host navigation
```

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
Cc/Ch/Kc  cache entries/reuses/cookies
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
Pixel dimensions use `S.W` and `S.H`; positive bounds use `Wn/Wx` for minimum
and maximum width and `Hn/Hx` for minimum and maximum height. Borders use
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
engine exposes `Cm` (cache maximum), `Cc` (count), `Ch` (body reuses), `Cs`
(default freshness seconds), and `Cl()` (clear). Stale validators use
`If-None-Match` or `If-Modified-Since`; reload always bypasses this cache.
External stylesheets share this same LRU and its aliases rather than creating a
second hidden cache. Lea loads at most eight linked stylesheets per page and
parses up to 256 KB from each response.

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
