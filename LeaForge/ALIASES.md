# Lea Alias Rules

Aliases are a defining part of Lea. They keep small browser programs quick to
read and quick to write.

## Naming rules

- Core objects use one letter: `L` is the entry point, `E` is the engine, `P` is a page.
- Actions use two or three letters: `Go`, `GoA`, `Bk`, `Fw`, `Rl`, `RlA`, `St`, and `At`.
- `V.Cn` means can navigate: no active page operation/download and not disposed.
  `Busy` also includes background pictures. Async ownership lasts through view
  presentation; callback successors retain their own state. See `LOAD_LIFECYCLE.md`.
- `V.Ic/Ib` report retained bitmaps/bytes. `Ip` is the per-image pixel ceiling;
  `Im/It` configure/report shared Lea image bytes; `In/If` report native attempts
  and current failed entries. `Ie/Ii` expose the helper path/availability;
  `Iw/Ik` report its last PID and cancellation kills. `Ij/Ih/Ipm` report Job
  assignment, the hard worker-memory cap, and last sampled private bytes.
  `Irt/Ili/Isp/Ihl/Ias/Icp/Icr/Img/Ipk` report the privilege-stripped token, low
  integrity, suspended Job prebind, inherited-handle allowlist, startup ASLR,
  active CPU hard-cap policy, configurable CPU rate in basis points, mitigation
  mask, and Job peak process bytes. `Iac/Ind/Ial` confirm the running helper's
  AppContainer identity, zero capability set (including no network capability),
  and exact identity-scoped read/execute helper-file ACL. `Icl` configures the
  combined host/helper ceiling; `Ihm/Icm/Iad` report the last host-private
  sample, conservative combined admission signal, and whether it passed. See
  `PICTURE_LIMITS.md`, `IMAGE_PREFLIGHT.md`, and `IMAGE_ISOLATION.md`.
- `V.Ce(code)` queries native field editing and `V.Ec(code)` runs it: `cp`, `cu`,
  `pa`, `sa`, `de`, `un`, and `re`. Commands are host-only, protect read-only and
  password fields, retain a menu-blurred editor, and return 0 without mutation
  when a bounded copy/cut clipboard commit cannot acquire ownership. See
  `NATIVE_EDITING.md`.
- `Ng` is an optional host navigation guard on `E` and `V`: `Func<Uri, bool>`.
  False rejects explicit document navigation with `Er.K="blocked"`; null keeps
  normal behavior. It covers page GET/POST, history, fragments, and reload, not
  redirects or resource requests. Firespider installs it on ordinary and split
  tabs. See `NAVIGATION_POLICY.md` and `SPLIT_VIEW.md` for its callback contract.
- Page values use one or two letters: `U`, `T`, `H`, `Q`, `D`, `N`, `Bs`, and `Ls`.
- `Dp` is display density in dots per inch. `E.Dp` and explicit `V.Dp` writes
  clamp to 1..960; `P.Dp` stores the value used by the page and `G.Dp` reports
  it. WinForms `V` follows the live monitor DPI until a host sets an explicit
  value. Resolution media queries accept `dpi`, `dppx`, and `dpcm`; see
  `MEDIA_RESOLUTION.md`.
- `Th` is the preferred color theme. `l` means light and `d` means dark.
  `E.Th` defaults to light, `P.Th` stores the applied value, and `G.Th` reports
  it. WinForms `V` samples the Windows application theme until a host sets an
  explicit value. See `MEDIA_COLOR_SCHEME.md`.
- `Rm` is the reduced-motion preference. `n` means no preference and `r` means
  reduce. `E.Rm` defaults to no preference, `P.Rm` stores the applied value,
  and `G.Rm` reports it. WinForms `V` samples Windows client-area animation
  policy until a host sets an explicit value. See `MEDIA_REDUCED_MOTION.md`.
- `Pdt` is the reduced-data preference. `n` means no preference and `r` means
  reduce. `E.Pdt` defaults to no preference, while `P.Pdt` and `G.Pdt` expose
  the applied value. `V.Pdt` is explicitly host-controlled and does not infer
  network policy or change resource loading. See `MEDIA_REDUCED_DATA.md`.
- `Pc` is the preferred-contrast state. `n` means no preference, `l` less, `m`
  more, and `c` custom. `E.Pc` defaults to no preference, `P.Pc` stores the
  applied value, and `G.Pc` reports it. WinForms `V` maps Windows High Contrast
  to custom until a host sets an explicit value. See `MEDIA_CONTRAST.md`.
- `Je` is the scripting permission and media state. `true` means enabled and
  `false` means none. `E.Je` defaults to true, `P.Je` stores the applied state,
  and `G.Je` reports it. Lea recognizes `scripting: initial-only` but never
  selects it because the engine has no one-shot script mode. See
  `MEDIA_SCRIPTING.md`.
- `Pt/Hv/Ap/Ah` describe primary pointer, primary hover, any-pointer union,
  and any-hover state. Pointer values are `n/c/f`; `Ap` also accepts `b` for a
  coarse-plus-fine hybrid. `Mi(pointer, hover, anyPointer, anyHover)` validates
  and applies the complete state atomically. Headless Lea defaults to desktop
  input, while WinForms samples Windows devices until explicitly overridden.
  See `MEDIA_INTERACTION.md`.
- Page commands use one or two letters where practical: `t`, `a`, `bg`, `fg`, `fs`, `dp`, `wd`, `ht`, `dr`, `ti`, `tw`, `twm`, `tws`, `ta`, `taa`, `tl`, `lc`, `ls`, `lst`, `lp`, `li`, `ms`, `be`, `af`, `ts`, `td`, `uo`, `v`, `tx`, `ac`, `rc`, `tc`, `hd`, `sh`, `op`, `cl`, `tg`, `ds`, `en`, `vl`, `ck`, `fo`, and `go`; text-decoration components and logical edge families stay within three letters.
- New public aliases should be no more than three letters.
- `E/V.Hm` reports the 4 MiB document-body ceiling; `E/V.Csm` reports the
  256 KiB linked-stylesheet ceiling. GET/POST input and optional CSS share
  bounded reads and a captured `Tm` I/O budget. See `INPUT_LIMITS.md`.
- `E.Ss(page, key, css)` / `V.Ss(key, css)` set a bounded named native sheet;
  `Sd` removes it and `P/V/G.Sn` counts named sheets. Replacement preserves
  order, document identity, and retained fields. See `NATIVE_STYLESHEETS.md`.
- `V.Pa(text)` pastes bounded host-supplied text with native undo and the same
  target protections as guarded `Ec("pa")`, without reading the clipboard.
  This is a view command, separate from `Fi.Pa` patterns. See `PASTE_SAFETY.md`.
- `E.Ud(page)` and `V.Ud` conservatively report field data worth retaining before
  a URL-only reload, including changed hidden fields and selected files. They
  do not serialize state or authorize tab disposal. See `HIBERNATION.md`.
- `V.Fe` reports typed page-load failure; `V.Le` holds it, `V.Cr` reports an
  eligible explicit GET retry, and `V.Ry()` performs it. POST is never replayed
  by Retry. Page-load failures still throw `Er`. See `LOAD_FAILURES.md`.
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
`E.Je` and `V.Je` enable page commands and drive scripting media queries, while `E.Js(page, code)` and
`V.Js(code)` run a bounded host-provided command string. `P.Jc` counts accepted
commands and `P.Ms` holds bounded alert messages. `V.Al` fires once for each new
message and `V.Am` exposes the current message. `E/V.Su(condition)` and
`E/V.Su(property, value)` report the same bounded CSS capability used by
`@supports`; literal page scripts may use those shapes through
`if (CSS.supports(...)) { ... } else { ... }`. See `SUPPORTS_QUERIES.md`.
`E/V.Mq(query)` evaluates Lea's compiled media-query subset against current
host state. Literal scripts may use `matchMedia("...").matches` or
`window.matchMedia("...").matches` as the same bounded `if` guard. See
`MATCH_MEDIA.md`, `MEDIA_MODIFIERS.md`, and `MEDIA_INTERACTION.md`.
Supported page commands are:

```text
t("Title")       title
a("Message")     alert message
bg("#f0fff8")    body background
fg("#164e3f")    body text color
fs(20)           body font size
bg("id", "#fff") element background
fg("id", "#222") element text color
fs("id", 18)      element font size
dp("id", "none") element display
wd("id", "50%")  element width
ht("id", "120px") element height
dr("rtl")          body direction
dr("id", "rtl")   element direction
ti("18px")         body first-line indent
ti("id", "15%")   element first-line indent
tw("balance")      body wrapping shorthand
tw("id", "wrap balance") element wrapping shorthand
twm("id", "nowrap") element wrapping mode
tws("id", "auto") element wrapping style
ta("center")       body text alignment shorthand
ta("id", "end")   element text alignment shorthand
taa("id", "left") element all-line alignment longhand
tl("center")       body last-line alignment
tl("id", "end")   element last-line alignment
lc("id", "2")     element line clamp
lst("upper-roman") body list marker style
lst("id", "square") element list marker style
lp("id", "inside") element list marker position
li("id", "url('marker.png')") element list marker image
ls("id", "square url('marker.png') inside") list style shorthand
ms("color:#2563eb") page marker default
ms("id", "content:'Lea ';font-weight:bold") element marker style
ms("id", "content:attr(data-code)") generated marker text
ms("id", "content:counter(list-item)") generated list number
be("content:'Lea '") body before style
be("id", "content:attr(data-code)") element before style
af("id", "content:' done'") element after style
be("id", "content:'[!] ' / 'Important '") visual / accessibility text
qu("auto")          body generated quote pairs
qu("id", "'<' '>'") element generated quote pairs
cr("id", "chapter 2") element counter reset
ci("id", "chapter") element counter increment
cs("id", "chapter 9") element counter set
ts("3px 2px #555") body text shadow
ts("id", "5px 4px 0 #dc2626") element text shadow
td("underline dashed red 2px") body text decoration
td("id", "line-through double #2563eb 3px") element decoration
tdl("id", "underline") decoration line only
tdc/tds/tdw("id", value) decoration color/style/thickness
uo("6px")          body underline offset
uo("id", "-3px") element underline offset
is("id", "50%")  logical inline size
bs("id", "120px") logical block size
mi("id", "8px 12px") logical inline margin
mb("id", "6px")      logical block margin
pi("id", "8px 12px") logical inline padding
pb("id", "6px")      logical block padding
mis/mie/mbs/mbe("id", "8px") logical margin edge
pis/pie/pbs/pbe("id", "8px") logical padding edge
bi/bb("id", "2px solid red") logical inline/block border
bis/bie/bbs/bbe("id", "2px solid red") logical border edge
biw/biy/bic("id", "2px") inline border components
bbw/bby/bbc("id", "2px") block border components
siw/siy/sic and eiw/eiy/eic  inline start/end components
sbw/sby/sbc and ebw/eby/ebc  block start/end components
rd("id", "12px / 8px") border radius shorthand
rtl/rtr/rbr/rbl("id", "12px 8px") physical corner radius
rss/rse/res/ree("id", "12px 8px") logical corner radius
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

The two-argument `bg/fg/fs/dp/wd/ht/dr/qu/ti/tw/twm/tws/ta/taa/tl/lc/ls/lst/lp/li/ms/be/af/ts/td/tdl/tdc/tds/tdw/uo/is/bs`, logical box, logical border, and
radius forms
target one validated ID. Color and font
forms reuse the same
hex/named-color and 8..96 pixel font bounds as the body forms, reject document
metadata targets, cap the resulting inline style at 4,096 characters, and
preserve mutable fields through the shared CSS rebuild.

`be/af` set bounded `::before`/`::after` style overrides on the body or one
validated ID. Readable `beforestyle/afterstyle` use the same forms. They accept
the marker text-property subset and bounded
string/attribute/named-counter/quote-keyword/`url(...)` `content` grammar; an
empty value restores authored CSS. The same image syntax works through `ms`;
no extra image command is needed. See `GENERATED_CONTENT.md` and
`GENERATED_IMAGES.md`.

`qu` sets the inherited `quotes` property on the body or one validated ID;
readable `quotes` and literal `style.quotes` use the same forms. `S.Qc` returns
the computed pair count and `S.Qo(depth)`/`S.Qx(depth)` expose the opening and
closing strings. Values accept `auto`, `none`, CSS-wide forms, or one to eight
32-unit pairs; generated depth stops at eight. See `GENERATED_QUOTES.md`.

`S.Dr` stores inherited horizontal direction as `l` or `r`. CSS `direction`,
HTML `dir`, variables, inline/important tiers, and CSS-wide values resolve
before logical declarations. `dr` accepts `ltr`, `rtl`, bounded CSS-wide values,
or an empty authored reset through body/ID, readable `direction`, and literal
`style.direction` forms. Logical edges and corners remap for RTL; explicit
logical inheritance translates between different parent/child directions. See
`DIRECTION.md`.

`ti` and readable `textindent` accept one body value or an ID plus value.
Signed integer pixels are bounded to -2000..2000 and percentages to -100..100;
unitless zero and CSS-wide values share the authored parser. Literal body and
element `style.textIndent` assignments use the same route, and an empty value
restores the authored cascade. See `TEXT_INDENT.md`.

`S.Ts` stores one optional inherited text-shadow record. `ts` and readable
`textshadow` accept one body value or an ID plus value, with signed integer
offsets bounded to -128..128 pixels, an optional 0..32 pixel blur, and one
optional bounded color. Literal `style.textShadow` uses the same parser; empty
assignments restore authored CSS. See `TEXT_SHADOW.md`.

`S.Td/Tc/Tds/Tw` store bounded inherited text-decoration line flags, optional
color, style, and integer thickness. `td` is the atomic shorthand;
`tdl/tdc/tds/tdw` update one component without clearing the others. Readable
names and literal `style.textDecoration*` assignments share the same parser;
empty values restore the authored cascade. See `TEXT_DECORATION.md`.

`S.Uo/Uop` store a bounded inherited signed integer underline offset and its
pixel/percentage kind. `uo` and readable `textunderlineoffset` accept one body
value or an ID plus value. Pixels are limited to -32..32 and percentages to
-100..100; `auto`, unitless zero, and CSS-wide values use the same cascade.
Literal `style.textUnderlineOffset` assignments share the parser, and
`text-decoration` shorthand updates never reset the offset. See
`TEXT_UNDERLINE_OFFSET.md`.

`S.Wm/Wt` store inherited wrapping mode and style. Empty `Wm` is the normal
wrap mode, `w` is explicit wrap, and `n` is nowrap; empty `Wt` is auto and `b`
is balance. `tw` is the shorthand and resets any omitted part to its initial
value, while `twm` and `tws` update one longhand. Readable
`textwrap/textwrapmode/textwrapstyle` and literal
`style.textWrap/textWrapMode/textWrapStyle` use the same strict parser. The
supported style values are `auto` and `balance`; `stable`, `pretty`, and
`avoid-short-last-line` remain inert. `white-space` updates only `Wm` in source
order. Empty literal assignments restore authored CSS. See `TEXT_WRAP.md`.

`S.Ta/Tl` store inherited all-line and final-line alignment as compact
`l/c/r/j/s/e` state; empty `Ta` is logical start and empty `Tl` is auto. `ta`,
`taa`, and `tl` accept one body value or an ID plus value, with readable
`textalign`, `textalignall`, and `textalignlast` forms. Ordinary `text-align`
values update `Ta` and reset `Tl` to auto, `justify-all` sets both to justify,
and `text-align-all` changes only `Ta`. Logical start/end survive inheritance;
`match-parent` resolves the parent's logical edge through the parent direction.
CSS-wide values and empty literal assignments share the compiled cascade.
Literal `style.textAlign`, `style.textAlignAll`, and `style.textAlignLast` use
the same route. See `TEXT_ALIGN.md` and `TEXT_ALIGN_LAST.md`.

`dp` accepts a bounded standard display keyword set. `none` removes the subtree
from layout and accessibility; `flex`/`inline-flex` and `grid`/`inline-grid`
enter Lea's existing layout modes. An empty value removes only the inline
display declaration and restores the authored cascade. Unsupported values are
inert. See `DISPLAY_NAVIGATION.md`.

`wd` and logical `is` accept fixed `0..2000px`, `1..100%`, classic `1..100vw/vh/vmin/vmax`,
bounded `1..100em/rem`, or one `calc(PU +/- Npx)` expression where `P` is
1..100, `U` is `%` or a supported relative unit, and `N` is 0..2000. It also
accepts two/three-term `min()` and `max()`, or three-term `clamp()`, when every
argument is one of those bounded forms. An empty value removes only the inline
width and restores the authored cascade. Authored `width`, `min-width`,
`max-width`, `inline-size`, `min-inline-size`, and `max-inline-size` accept the
same forms. Nested functions and multiplication/division remain unsupported.
`ht`, logical `bs`, and authored physical/logical height/min/max use the matching
parser. Readable `inlinesize`/`blocksize` command forms and literal
`style.inlineSize`/`style.blockSize` assignments map to the same compact paths.
See `CALCULATED_WIDTHS.md`, `CALCULATED_HEIGHTS.md`, `VIEWPORT_UNITS.md`,
`FONT_RELATIVE_UNITS.md`, `SIZE_FUNCTIONS.md`, and `LOGICAL_SIZING_INSETS.md`.

`mi/mb/pi/pb` set direction-aware horizontal logical margin/padding axes with one or two fixed
pixel values. `mis/mie/mbs/mbe` and `pis/pie/pbs/pbe` target one logical edge;
the readable forms are `margininline`, `marginblockend`,
`paddinginlinestart`, and their matching names. Margins accept signed values
from -2,000 through 2,000; padding accepts 0 through 2,000. Literal
`style.marginInline`, `style.paddingBlock`, and all eight logical edge
properties use the same compact paths. Empty assignments restore authored CSS.
See `LOGICAL_BOX_MODEL.md`.

`bi/bb` set a full direction-aware horizontal logical inline/block border; `bis/bie/bbs/bbe`
target one start/end edge. Component aliases use `w` for width, `y` for style,
and `c` for color: `biw/biy/bic`, `bbw/bby/bbc`, plus `si*`, `ei*`, `sb*`, and
`eb*` for the four logical edges. Axis components accept one or two values.
Widths are bounded to 0..100 pixels or thin/medium/thick; styles are
none/hidden/solid/dashed/dotted/double; colors use bounded named, hex, RGB/RGBA,
legacy or modern HSL/HSLA, modern HWB, modern CIE Lab/LCH, predefined sRGB,
linear-sRGB, Display-P3, linear-Display-P3, A98-RGB, ProPhoto-RGB, Rec.-2020,
and XYZ `color()`, modern
Oklab/OkLCh, and two-stop sRGB mix forms. Readable names and all 24 literal `style.borderInline*` and
`style.borderBlock*` properties share the same atomic parser. Empty assignments
restore authored CSS. See `LOGICAL_BORDERS.md`.

`rd` sets the physical `border-radius` shorthand. `rtl/rtr/rbr/rbl` target its
four clockwise physical corners, while `rss/rse/ree/res` target logical
start-start, start-end, end-end, and end-start. Each corner accepts one bounded
pixel/percentage value or a horizontal/vertical pair; `rd` accepts the existing
one-to-four values and optional slash-separated vertical list. Readable names
such as `borderradius` and `borderstartstartradius`, plus all matching literal
`style.borderRadius`, physical-corner, and logical-corner assignments, share the
same validator. Empty DOM assignments restore authored CSS. See
`LOGICAL_RADII.md`.

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

Literal `location`, `location.href`, and `location.assign(...)` assignments or
calls, including `window` and `document` spellings, map to the same `go` path.
Relative local-file requests preserve query and fragment components. Computed
values, `location.replace`, history mutation, and arbitrary methods remain
unsupported.

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
Vw/Vh     applied viewport width/height in pixels
Vc        document custom-property count
Ic        active compiled important-declaration count
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

The familiar literal DOM bridge recognizes one bounded ID lookup per statement:

```javascript
document.getElementById("status").textContent = "Ready";
document.getElementById("field").value = "Lea";
document.getElementById("choice").checked = true;
document.getElementById("field").disabled = false;
document.getElementById("panel").hidden = true;
document.getElementById("more").open = true;
document.getElementById("field").focus();
document.getElementById("status").classList.add("ready");
document.getElementById("status").style.color = "#164e63";
document.getElementById("panel").style.display = "none";
document.getElementById("panel").style.width = "calc(100% - 24px)";
document.getElementById("status").style.textWrap = "wrap balance";
document.getElementById("status").style.textWrapMode = "nowrap";
document.getElementById("status").style.textWrapStyle = "auto";
document.getElementById("status").style.textAlign = "end";
document.getElementById("status").style.textAlignAll = "center";
document.getElementById("status").style.textAlignLast = "center";
```

`innerText` is an alias for `textContent`; `classList.remove/toggle` and
`style.background/backgroundColor/fontSize/display/width/height/direction/textIndent/textWrap/textWrapMode/textWrapStyle/textAlign/textAlignAll/textAlignLast` are also accepted. IDs and class
arguments must be quoted literals. Boolean state accepts only literal `true` or
`false`. The parser translates each accepted statement to the existing
`tx/vl/ck/ds/en/hd/sh/op/cl/fo/ac/rc/tc/bg/fg/fs/dp/wd/ht/dr/ti/tw/twm/tws/ta/taa/tl/lc/ls/lst/lp` command, so all target, value,
style, rebuild, focus, and 32-command limits remain unchanged. `innerHTML`,
selectors, computed IDs or values, event registration, loops, expressions, and
unknown properties or methods remain inert. See `DOM_BRIDGE.md`.

The separate typed `E.Jv(page, element, alias, value)` path also accepts `id`
(replace ID) and `cn` (replace raw class name), bounded to 512 UTF-16 units.
These are not statements in the literal command parser above. The optional
`Lea.Script` module maps writable `element.id`/`element.className` to them and
adds `hasAttribute`; it keeps the interpreter out of the required core runtime.
Values are literal, empty attributes stay present, and connected-node ownership
and scripting permission are required. See `ATTRIBUTES.md` for limits/tests.

Core class/word selectors and `ac/rc/tc` share ASCII token separators: space,
tab, line feed, form feed and carriage return. Optional classList membership
uses these separators too; NBSP/vertical tab do not separate tokens. Existing
case and mutation bounds remain, and raw `cn` values are not normalized.
See `CLASS_TOKENS.md` for the allocation measurements and compatibility limits.

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

For Lea's bounded grid subset, `D` is `grid` or `inline-grid`. Column tracks stay
lazy in compact `Gd`, with `Gc/Gv(index)` inspection; rows mirror them through
`Rd` and `Rc/Rv(index)`. A template accepts one to eight `0..2000px`, `auto`, or
`1..64fr` tracks, simple `repeat(1..8, track)` expansion, and `minmax(0, Nfr)`
as a bounded flexible-track spelling. Nonnegative values are pixels, `S.Av=-65`
is auto, and the other negative values are integer `fr` weights. `Gp/Rg` retain the
0..512 pixel column/row gaps, including legacy `grid-column-gap`,
`grid-row-gap`, and `grid-gap`. Direct items use `Gl=0` for auto placement or
`1..8` for a numeric column start, while `Gs=1..8` is the column span. Rows use
matching `Rl/Rs` start/span aliases.
`grid-column`, `grid-column-start`, and `grid-column-end:span N` fill those two
aliases; `grid-row`, `grid-row-start`, and `grid-row-end:span N` fill the row
pair. A bounded occupancy map prevents two-axis spans from overlapping. Explicit
rows use definite container height or Lea's auto-height measurement pass, while
implicit rows remain available for additional items. Existing nested layout,
positioning, clipping, paint, hit, field, and accessibility paths are reused.

Named templates use `grid-template-areas` and `grid-area`. `Aa` is the container
area count, `Ar/Ac` are its template row/column counts, and `Ag(name, part)`
returns row start, column start, row span, or column span for parts `0..3`;
starts are one-based and zero means unavailable. `An` is an item's exact
case-sensitive area name. Templates are limited to 8x8 cells, 16 names,
32-character ASCII identifiers, and 512 source characters. Every quoted row
must have the same cell count, dot runs are null cells, and repeated names must
form one filled rectangle. Definite named/numeric items reserve cells before
automatic items, and area dimensions can create equal-share implicit tracks.

Bracket groups in `grid-template-columns/rows` add bounded named lines without
allocating maps for unnamed templates. `Gnc/Rnc` count explicit column/row
names, while `Gx(name)` and `Rx(name)` return a one-based line index or zero.
Each item exposes column start/end names through `Gn/Ge` and row start/end names
through `Rn/Re`; existing `Gl/Gs/Rl/Rs` continue to carry numeric starts and
spans. Shorthands accept `name/name`, `name/span N`, `span N/name`,
`auto/name`, and `number/name`, and the four longhands accept exact names.
A bare area name first resolves its generated `name-start/name-end` edges, and
`Gx/Rx` expose those generated names directly without counting them in
`Gnc/Rnc`.

Explicit names are case-sensitive and unique per axis. A template keeps at most
16 names, eight names on one line, 32 characters per explicit name, eight
tracks, and 512 source characters. Simple repeat groups can sit between named
outer lines. Duplicate, reserved, malformed, oversized, and excess names reject
the whole declaration while preserving a prior valid value. Escaped names,
repeated-name occurrence indexes, named spans, intrinsic/percentage tracks,
auto-fit/fill, and subgrid remain outside this subset.

`grid-auto-flow` uses one compact container alias: `Gf=r/d/c/x` means sparse
row, dense row, sparse column, or dense column. Row flow keeps Lea's existing
forward cursor and creates implicit rows. Dense row restarts that cursor for
each automatic item so a later smaller span can fill an earlier hole. Column
flow swaps the search axes, wraps through one to eight explicit/template rows,
and creates equal-share implicit columns. Dense column combines both behaviors.
Definite items still reserve cells first and automatic items still enter in
order-modified source order; non-default visual traversal follows resolved
rows/columns while DOM, focus, form, and accessibility ownership remain intact.
At most eight columns are generated. Content beyond that ceiling safely uses
additional rows in the final column instead of overlapping.

`grid-auto-columns` and `grid-auto-rows` add repeating implicit-track patterns.
`Gac/Gav(index)` expose the column count/value and `Rac/Rav(index)` mirror the
row pattern. One declaration accepts one to eight `0..2000px`, `1..64fr`,
`auto`, or `minmax(0,Nfr)` entries. Positive values are pixels, `-1..-64` are
integer `fr` weights, and `S.Av=-65` identifies bounded `auto`; a missing
pattern also behaves as one auto-weight track. Patterns cycle from the first
track after the explicitly sized template, including area-created tracks that
have no explicit size. Definite numeric endpoints and the largest automatic
span can grow the implicit column map to the shared eight-column ceiling.
Pixel auto rows contribute a minimum during Lea's measurement pass, so an
auto-height grid settles to its authored repeating row sizes before final
placement. `initial`, `unset`, `revert`, `revert-layer`, and a lone `auto`
restore the default; malformed, percentage, intrinsic, repeated, oversized,
or excess patterns preserve the prior value atomically.

`grid-template` and `grid` compile into those same aliases without adding a
second layout model. Bounded `grid-template` accepts explicit `rows / columns`
or one to eight quoted area rows, each with an optional pixel, auto, integer-fr,
or minmax size, followed by optional `/ columns`. It resets `Gd/Rd/Gnd/Rnd` and
`Ad/Ar/Ac` while preserving `Gf/Gad/Rad`. `grid` accepts the same explicit forms
and also `auto-flow [dense] auto-rows / columns` or
`rows / [dense] auto-flow auto-columns`. An explicit `grid` form resets all
three automatic properties to row/auto defaults; an auto-flow form resets the
unused explicit and automatic axis. Both shorthands accept one top-level slash
and at most 1,024 characters. `none`, `initial`, `unset`, `revert`, and
`revert-layer` reset their owned longhands, while `grid` deliberately leaves
`Gp/Rg` gaps untouched. Parsing uses scratch style state, so malformed sides,
areas, flow markers, track lists, or extra slashes preserve every prior owned
value atomically. Reverse flow, pre-explicit implicit tracks, line names around
quoted shorthand area rows, intrinsic or percentage tracks, auto-fit/fill, and
subgrid remain outside the bounded subset.

Grid item alignment stays compact: container `Ji/Ai` and item `Js/As` represent
the inline/block axes. All four use `t/s/c/e` for stretch, start, center, or end;
empty `Js/As` means auto and reads the corresponding container default.
`justify-items`, `align-items`, `justify-self`, and `align-self` fill those
aliases, while one/two-value `place-items` and `place-self` update both axes
atomically. Lea aligns the margin box inside the resolved grid area. Non-stretch
simple text uses a bounded measured auto width, naturally sized images/buttons
retain their compact width, and definite sizes remain exact. Block-axis
center/end/stretch requires a resolved row rectangle; an auto-height unresolved
row keeps natural start flow. `normal`, reset keywords, and auto self values
restore stretch/parent defaults. Safe/unsafe overflow alignment, baselines,
legacy alignment, and writing-mode axis reversal remain outside this subset.

Grid track-group alignment uses `Al/Jc=s/c/e/b/a/v` for block/inline-axis start,
center, end, space-between, space-around, or space-evenly. `align-content` fills
`Al`; the existing `justify-content` alias supplies `Jc`; one/two-value
`place-content` updates both atomically in block/inline order. Distribution runs
after bounded track sizing and authored gaps. Fixed tracks inside a definite
grid can leave free space for offsets or enlarged between-track gaps, while
`fr` and bounded `auto` tracks normally consume that space first. `normal`,
`stretch`, and reset keywords use the start-compatible compact default; auto
tracks already provide Lea's bounded stretch behavior. Invalid values preserve
both prior axes. Baseline, safe/unsafe overflow, fallback positions, and
writing-mode axis reversal remain outside this subset.

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
line, and partial ranges crop their first/last segments proportionally. Retained
segments also carry private horizontal direction, so RTL partial ranges grow
from the right and point lookup maps physical X back to logical source offsets.
Selection and editing are unsupported. The current .NET Framework adapter returns an empty
visible-range collection because non-empty managed and manually constructed
interface arrays produced invalid client handles; a future bridge must cross a
different native boundary.

Computed style uses `S`: `F` is foreground color, `Bg` is background, `Fs` is
font size, `Ff` is a font fallback list, `B` is bold, `I` is italic, and `D` is
display behavior. `Td` is decoration using `n/u/s/us` for none, underline,
strike, or both. Generic `Ff` entries use `ss/s/m/c/f/ui` for sans-serif,
serif, monospace, cursive, fantasy, and system UI. `Ta/Tl` are inherited
all-line/final-line alignment using `l/c/r/j/s/e`, with empty states for
logical start/auto; `Tt` is inherited text transformation using
`n/u/l/c` for none, uppercase, lowercase, and capitalize. `Ls` is inherited
letter spacing and `Wg` is inherited word spacing in signed pixels. `Ti` is
inherited signed first-line indentation and `Tip` marks percentage rather than
pixel storage.
`Lh` is pixel line height and `Lm` is a bounded
percentage multiplier. Lea accepts CSS pixel, percentage, and unitless line
heights, then keeps each painted line tall enough for its largest font.
`Ws` is white-space mode using `n/nw/p/pw/pl` for normal, nowrap, pre,
pre-wrap, and pre-line. `Wm` is inherited wrapping mode using empty/`w`/`n`
for normal wrap, explicit wrap, and nowrap; `Wt` is inherited wrapping style
using empty/`b` for auto and balance. `Wb` is word breaking using `n/b/a/k` for normal,
break-word, anywhere/break-all, and keep-all. `Ov` is overflow using
`v/h/c/a/s` for visible, hidden, clip, auto, and scroll. `Vi` is inherited
visibility using `v/h/c` for visible, hidden, and collapse. `Op` is the effective
paint opacity from 0 through 100 after ancestor opacity is included.

Text blocks keep compatible flattened text in `B.X`. When nested markup needs
separate painting, `B.Is` exposes compact `Ir` inline runs; each run uses `K`
(kind), `X` (text), `U` (optional link), and `S` (computed style). Plain text
leaves `Is` empty and stays on the original one-box layout path.

CSS `text-transform` accepts only `none`, `uppercase`, `lowercase`, and
`capitalize` plus CSS-wide inherit/reset values. Unknown values preserve the
prior computed state; custom properties, important tiers, hover/focus, and
pixel-width media rebuilds use the normal cascade. `E.Tx(text, style)` exposes
the same compact presentation transform to hosts. Lea applies it to block and
inline display strings, table cells, semantic values, buttons, placeholders,
select labels, and unfocused field paint without changing `N.X`, `P.X`,
`Fi.V`, option values, or form submission. The native editor deliberately
shows and stores the raw value while focused. UI Automation retains authored
document text and maps case-transformed visual segments with ordinal
case-insensitive matching. Capitalization is deterministic and invariant; it
starts words at the beginning, after whitespace, and after a hyphen rather
than implementing locale-specific CSS casing rules.

CSS `letter-spacing` and `word-spacing` accept `normal`, integer pixels from
-32 through 128, and unitless zero. Invalid values preserve the previous
declaration; `normal`/`initial` reset to zero, while `inherit`/`unset`/`revert`
restore the parent's computed value even after earlier declarations. Variables,
important tiers, inline overrides, hover, and media queries use the same cascade.
The renderer shares measurement between wrapping, button sizing, and painting.
Tracking uses .NET text elements, omits line-edge gaps, and averages adjacent
inline values with integer rounding. Word-only spacing preserves shaped word
runs and adjusts spaces and non-breaking spaces. Individual advances retain a
one-pixel floor and measured runs cap at 8,192 pixels. Unspaced content retains
the existing fast path. Raw DOM/form values and automation text are unchanged;
the retained rectangles reflect the new layout. Focused native editors and
native select popups retain Windows text spacing. Fractional/relative units,
full bidi tracking, and script-specific shaping are not implemented here.
The spacing model follows the supported parts of
[CSS Text spacing](https://www.w3.org/TR/css-text-3/#spacing).

CSS `text-indent` uses inherited `S.Ti/Tip`. `Ti` stores a signed integer and
`Tip` distinguishes percentage from pixel values. Lea accepts -2000..2000
integer pixels, -100..100 integer percentages, unitless zero, and CSS-wide
inherit/reset values. Invalid, fractional, over-limit, `hanging`, `each-line`,
and multiple values preserve the preceding declaration. Custom properties,
important tiers, inline overrides, hover/focus, and media queries use the
ordinary cascade. Percentages resolve against the settled content width. The
renderer changes only the first visual line's available width and inline-start:
positive LTR values move right, positive RTL values move left from the right
edge, negative values extend outward, and later lines return to full width.
Paint, hits, find, focus, and accessibility reuse the resulting retained
rectangles. Compact `ti(value)` and `ti(id,value)` calls, readable `textindent`,
and literal body/element `style.textIndent` assignments share the same bounded
path; an empty literal assignment restores authored CSS. Native controls,
vertical writing, Unicode bidi shaping, relative/fractional lengths, `hanging`,
and `each-line` remain outside this subset. See `TEXT_INDENT.md`.

`To` is non-inherited text overflow: `c/e` means clip/ellipsis and defaults to
clip. CSS `text-overflow` accepts those two full keywords, explicit `inherit`,
and initial/unset/revert resets. Unsupported strings or multiple values preserve
the preceding declaration; variables and important, hover, and media rules work
through the ordinary cascade. Ellipsis requires non-visible overflow and an
actually overflowing line; it does not enable clipping or disable wrapping.
Lea retains original line sizes, text, and pointer rectangles while painting
bounded .NET text-element prefixes and a block-styled U+2026 marker. Collapsed
spaces inside links keep their link target. Logical automation text remains
complete, and visual text segments map only to painted prefixes. Horizontal RTL
overflow places the marker at physical left and right-aligns its authored prefix
inside the original atom; partial and point TextPattern geometry follows that
direction. The first character is retained and clipped in very narrow boxes. Blocks, inline runs,
explicit lines, unbreakable wrapped words, and table cells share this path.
Prefix searches examine at most 8,192 text elements. Native controls, SVG text,
  two-ended/custom-string overflow, Unicode bidi shaping, vertical
  layout, and complex-script baseline behavior are not added by this feature.
  See [CSS Overflow text-overflow](https://www.w3.org/TR/css-overflow-3/#text-overflow).

`Lc` is non-inherited line clamping: `0` means none and `1..32` is the maximum
visible visual-line count. CSS `line-clamp` and `-webkit-line-clamp` share the
same strict declaration slot. Explicit `inherit` copies the parent; initial,
unset, revert, and revert-layer restore zero. Invalid zero, negative,
fractional, string-marker, multi-token, and over-limit values preserve the
preceding declaration. Variables, important tiers, inline overrides, hover,
and media rules use the ordinary cascade. Lea wraps and optionally balances
first, removes excess retained lines, shortens the final atom at a bounded text
element boundary, and places one U+2026 marker at physical inline end. Removed
boxes do not paint, hit, or expose visual automation geometry, while authored
DOM and logical automation text remain complete. Defining `lc(value)` and
`lc(id,value)`, readable `lineclamp`, plus literal body/element
`style.lineClamp` and `style.webkitLineClamp` assignments share this route.
Longhands, custom marker strings, descendant block fragmentation, vertical
writing, and historical WebKit display prerequisites remain outside the
subset. See `LINE_CLAMP.md` and
[CSS Overflow Level 4 line-clamp](https://drafts.csswg.org/css-overflow-4/#propdef-line-clamp).

`Lst` is inherited list-marker type state. Empty uses HTML defaults; `n/d/c/s/1/0/a/A/r/R`
mean none, disc, circle, square, decimal, decimal-leading-zero, lower/upper
alpha, and lower/upper Roman. CSS `list-style-type` and Lea's bounded
`list-style` shorthand share variables, priority, CSS-wide values, hover, media,
and invalid preservation. `lst(value)` and `lst(id,value)`, readable
`liststyletype`, and literal `style.listStyleType` assignments use the same
validator. List-item blocks expose the formatted result through `B.Mk`; `none`
keeps it empty and adds no spacer. HTML list/per-item `type`, ordered `start`,
`value`, and `reversed` are supported. Integers are bounded to -999999 through
999999 and stepping saturates in either direction. Alpha rolls over after `z`;
Roman output is bounded to 1..3999 and otherwise falls back to decimal. Nested
lists count only direct items. `Lp=o/i` separately stores inherited outside or
inside position. Defining `lp`, readable `liststyleposition`, and literal
`style.listStylePosition` change only position. `Li` stores an inherited resolved
marker-image `Uri`, with `null` meaning none. Defining `li`, readable
`liststyleimage`, and literal `style.listStyleImage` use the existing bounded
image validator and loader. Atomic `ls`/`liststyle` and literal
`style.listStyle` accept one type, one image, and one position in any order and
reset omitted components. Outside uses a measured logical-start gutter; inside
consumes first-line width. Images preserve aspect ratio inside 64x64 pixels and
fall back to `B.Mk` after a load/decode failure. `B.Ms` separately stores the
computed marker pseudo style; `S.Mc/Ma/Mn` distinguish bounded visual content,
its optional accessibility alternative, ordinary marker content, and
suppression. `Ma=null` means no alternative, while `Ma=""` intentionally
suppresses decorative generated text from accessibility. Terminal `::marker`
selectors reuse
the compiled originating-element matcher and cascade while staying isolated
from `B.S`. The defining `ms(value)` page default, `ms(id,value)` direct style,
and readable `markerstyle` accept at most 1,024 characters and 16 declarations;
content accepts up to eight quoted strings, `attr(name)`/`attr(name raw-string)`
with one optional quoted missing-value fallback, or implicit `list-item`
counter and generated quote items, with 64-character attribute names,
named/implicit counter names up to 32 ASCII characters, eight counter levels,
32-unit separators, and 128 UTF-16 output units. A slash-separated alternative
list accepts up to eight strings, attributes, or counters and 128 further
UTF-16 units. Non-inherited `S.Cr/Ci/Cs`
store canonical `counter-reset`, `counter-increment`, and `counter-set` pairs.
Defining `cr/ci/cs` accept page or ID-scoped forms; readable
`counterreset/counterincrement/counterset` and literal
`style.counterReset/style.counterIncrement/style.counterSet` use the same
strict validator. Values stop at eight operations, snapshots at eight active
instances, and integers at -999999..999999. See
`LIST_STYLE.md`, `LIST_POSITION.md`, `LIST_IMAGE.md`, `MARKER.md`,
`MARKER_CONTENT.md`, `ATTR_FALLBACKS.md`, `MARKER_COUNTERS.md`, and
`CSS_COUNTERS.md`, `GENERATED_QUOTES.md`, and `GENERATED_ALT_TEXT.md`.

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

Functional selector lists stay compiled and bounded. `:is(...)`, `:where(...)`,
and `:not(...)` accept up to eight compound simple-selector alternatives, with
at most sixteen authored alternatives across one simple selector and no nested
selector-list functions or inner combinators. `:is` and `:not` contribute the maximum
argument specificity; `:where` contributes zero. Unsupported or malformed
alternatives are discarded inside forgiving `:is`/`:where` lists, while
`:not` rejects its complete selector when any alternative is invalid. An empty
forgiving result remains valid and matches nothing. Stylesheets and compact
`Id/Q/Qa` queries share this compiler, including dynamic hover/focus matching.

Relational selectors use compact precompiled `Hr` records internally: `L` is
the leading implicit-descendant, child (`>`), adjacent (`+`), or following
sibling (`~`) relationship; `P/J` hold up to four compound parts and their
joins; and `W` is the argument specificity. Strict `:has(...)` lists accept up
to eight alternatives and sixteen total parts per outer simple selector. The
matcher visits at most 4,096 candidate elements across one `:has` evaluation,
uses maximum argument specificity, and shares stylesheet plus `Id/Q/Qa` query
behavior. Descendant focus changes can therefore restyle a retained parent.
Malformed or empty lists, nested selector-list functions, pseudo-elements,
oversized chains, and excess alternatives or work remain outside this subset.

Structural positions compile to compact `A/B` integers. `:nth-child`,
`:nth-last-child`, `:nth-of-type`, and `:nth-last-of-type` accept `odd`, `even`,
signed integers, and whitespace-aware `An+B` forms such as `3n + 1`, `-n+3`,
and `n+5`. Expressions are limited to 32 characters and each absolute
coefficient or offset to 8,192, matching Lea's document-node ceiling. Reverse
forms reuse the sibling count; matching performs no regex or integer parsing.
`:nth-child` and `:nth-last-child` also accept a strict `of S` filter with up to
eight compound alternatives. Only matching inclusive siblings enter the
forward/reverse position list, and the most specific filter is added to the
pseudo-class specificity. Filter alternatives count toward the same sixteen
authored tests per simple selector. Inner combinators, nested selector-list
functions, filtered `nth-of-type`, and filtered `nth-last-of-type` stay outside.

Responsive CSS uses compact `Vw` and `Vh` aliases. Hosts may set `E.Vw` or
`E.Vh`; each clamps to 1-8,192 pixels. `E.Vz(width, height)` applies both as one
atomic update and is preferred during host resize. Rules are reapplied only
when the active set changes. `V.Vw/Vh` are read-only and follow the control's
client viewport automatically. `P.Vw/Vh` and `G.Vw/Vh` report the pair applied
to the page and diagnostic snapshot.
`P.Rf` is the explicitly computed root font size, or zero for the host default;
`V.Rf` is the root font size actually used for layout.
Lea compiles bounded `screen`/`all` plus fixed non-matching `print` queries with
one leading whole-query `not` or no-op `only` before a media type, pixel `min-width`, `max-width`,
exact `width`, `min-height`, `max-height`, exact `height`, and
`orientation:portrait/landscape`, and integer `aspect-ratio`,
`min-aspect-ratio`, or `max-aspect-ratio`, plus conjunctions, comma
alternatives, four nested `@media` levels, up to eight resulting alternatives,
and `<style media>`. Modern `<`, `<=`, `=`, `>=`, and `>` comparisons accept
feature-first, value-first, and same-direction chained width, height, and
integer-ratio forms. Strict boundaries survive nested intersections. A square
viewport is portrait. Unknown media types, misplaced modifiers, non-pixel,
mixed-direction, and malformed ratio conditions stay inactive. Active diagnostic CSS lines use
short `@m`, `@h`, `@r`, and `@o` prefixes. See `MEDIA_FEATURES.md`,
`MEDIA_ASPECT_RATIO.md`, `MEDIA_RANGE_SYNTAX.md`, and `MEDIA_MODIFIERS.md`.

Bounded `@supports` blocks compile without a new public object or long command.
Declaration conditions reuse the documented display, position, sizing, flex,
grid, object, wrapping, overflow, clamp, pointer, and custom-property parsers;
`selector(...)` reuses the compiled selector grammar. Grouped `not`, `and`, and
`or` stop at eight terms and four levels, and mixed operators require explicit
grouping. Active diagnostic lines use short `@s(...)` labels and compose with
the existing media labels. Unknown, malformed, and over-limit conditions stay
inactive. See `SUPPORTS_QUERIES.md`.

Bounded named `@layer` blocks also need no public object or command. Later named
layers win normal declarations, unlayered normal declarations win over layers,
and important order reverses so the earliest named layer wins over later and
unlayered important rules. Inline normal and important declarations retain their
existing strongest positions. Reopened names merge; nested names compose as
`parent.child`; authored and `Ss` native sheets share one layer order. Active
diagnostic lines add compact `@l(name)` labels that compose with `@s` and `@m`.
Lea accepts at most 16 names with three 32-character segments. Anonymous layers,
comma order statements, and full `revert-layer` rollback stay unsupported. See
`CASCADE_LAYERS.md`.

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

`S.Cn` is the computed `container-name` list as one space-separated string;
empty means `none`. Lea retains up to four unique 32-character ASCII names and
uses them for bounded named `@container ... style(--property:value)` ancestor
selection. Unnamed style queries select the nearest ancestor. Query conditions
reuse computed `S.Cv(...)` values and bounded `var()` resolution; no new host
object or long public API is required. See `CONTAINER_STYLE_QUERIES.md`.

Each compiled declaration also keeps one priority bit. A final, top-level,
case-insensitive literal `!important` marker is removed from the value and moves
that declaration into Lea's important tier; text inside quotes or functions is
left untouched. Stylesheet normal, inline normal, stylesheet important, and
inline important tiers apply in that order. Existing specificity, source order,
shorthand/longhand mutation, custom properties, dynamic pseudo-classes, and
media activation remain inside their tier. `G.Ic` counts active stylesheet
important declarations without exposing their values.
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

Responsive dimensions use magnitude/pixel-offset/unit tuples. `Wp/Wo/Wu`,
`Wnp/Wno/Wnu`, and `Wxp/Wxo/Wxu` represent width, minimum, and maximum;
`Hp/Ho/Hu`, `Hnp/Hno/Hnu`, and `Hxp/Hxo/Hxu` are their height counterparts.
Unit codes are `0` for a containing-block percentage, `1` for `vw`, `2` for
`vh`, `3` for `vmin`, `4` for `vmax`, `5` for `em`, and `6` for `rem`.
Percentage heights resolve only
against a definite viewport, retained parent, positioned containing block, or
grid area; unresolved percentages retain authored state and natural auto
layout. Viewport units always use `V.Vw/Vh`, including below auto-height
parents. Font-relative units use the element's computed font or `V.Rf` root
font. Both axes accept one bounded `calc(PU +/- Npx)` expression; see
`CALCULATED_WIDTHS.md`, `CALCULATED_HEIGHTS.md`, `VIEWPORT_UNITS.md`, and
`FONT_RELATIVE_UNITS.md`.

Bounded size functions use lazy `S.Wk/Wnk/Wxk` and `Hk/Hnk/Hxk` kind aliases:
`0` is plain, `1` is `min`, `2` is `max`, and `3` is `clamp`. `Wq/Hq(part)`
return argument counts for preferred/minimum/maximum part `0/1/2`.
`Wv/Hv(part,argument,value)` expose pixels, magnitude, offset, and unit at value
indexes `0/1/2/3`. See `SIZE_FUNCTIONS.md`.

Rounded corners use clockwise horizontal `R0/R1/R2/R3` and vertical
`Q0/Q1/Q2/Q3` values for top-left, top-right, bottom-right, and bottom-left.
Nonnegative aliases are pixels; negative aliases encode percentages. CSS
`border-radius` accepts one through four horizontal values and an optional `/`
plus one through four vertical values; each corner longhand accepts one or two.
Pixel and compact unitless values cap at 2,000, percentages cap at 1,000, and
opposing radii normalize to the rendered box. Shorthand parsing is atomic and
invalid declarations preserve prior state. The shared path paints backgrounds,
solid/dashed/dotted borders, inherited rounded overflow clips, and ordinary
form fields without changing rectangular layout or link bounds.
Logical `border-start-start-radius`, `border-start-end-radius`,
`border-end-end-radius`, and `border-end-start-radius` map to those same four
physical corners for Lea's horizontal left-to-right model. Explicit shorthand
or corner `inherit` copies the matching parent ellipse; CSS-wide reset keywords
clear only the addressed corners. See `LOGICAL_RADII.md`.

CSS outlines use non-inherited `S.Ow/Os/Oc/Oo`. `Ow` stores the 0..32 pixel
width, `Os=n/s/d/o/a` stores none, solid, dashed, dotted, or automatic style,
`Oc` stores the authored color, and `Oo` stores a signed -64..64 pixel offset.
The `outline` shorthand accepts up to three unordered width/style/color parts
atomically; `outline-width`, `outline-style`, `outline-color`, and
`outline-offset` update their individual state. Named, 3/4/6/8-digit hex,
`rgb()`/`rgba()`, `hsl()`/`hsla()`, `hwb()`, `lab()`/`lch()`, predefined
`color()`, `oklab()`/`oklch()`, and bounded `color-mix()` colors, alpha, transparent, and
`currentColor` share the bounded color path. Variables, important tiers,
CSS-wide resets, malformed preservation, responsive rules, and dynamic
`:focus` rebuilds use the normal cascade. The custom painter follows rounded
corners, opacity, stacking, and ancestor overflow for boxes, images, and
ordinary fields. Positive or negative outlines remain visual only: box sizes,
document flow, hit regions, accessibility bounds, and native editors do not
grow around them.

Pointer interaction uses inherited `S.Cu/Pe`. `Cu` is empty for CSS `auto`, or
`d/p/t/w/c/m/h/n/e/s` for default, pointer, text, wait/progress, crosshair,
move, help, not-allowed, horizontal resize, and vertical resize. Common grab,
all-scroll, no-drop, row/column-resize, and directional-resize keywords fold
into those same compact states. `Pe` is true initially; bounded
`pointer-events:auto/none` enables or suppresses mouse targeting for retained
boxes, links, summaries, fields, fitted image maps, and linked SVG shapes.
Both properties inherit, accept custom properties and important tiers, and
preserve prior state for unknown values. `inherit`/`unset` keep inherited
state, while `initial`/`revert` restore automatic cursor or enabled pointer
events. The WinForms view maps `Cu` to built-in cursors and lets disabled boxes
fall through to an eligible box underneath. `Pe=false` does not change paint,
layout, keyboard focus, access keys, or accessibility bounds. Cursor image
URLs and SVG-specific painted/fill/stroke pointer modes remain outside this
bounded subset.

Box shadows use the non-inherited `S.Sw` list. Each `Sw` entry stores horizontal
and vertical offsets in `X/Y`, blur in `B`, spread in `R`, authored color in
`C`, and inset state in `I`. CSS `box-shadow` accepts one to four comma-separated
entries atomically, with two required and up to four pixel or compact unitless
lengths. Offsets and spread are signed and clamp to 512 pixels; blur is
nonnegative and clamps to 128 pixels. Named and 3/4/6/8-digit hex colors,
`rgb()`/`rgba()`, `hsl()`/`hsla()`, modern space/slash `hwb()`, `lab()`,
`lch()`, predefined `color()`, `oklab()`, `oklch()`, and bounded sRGB mix forms, `transparent`, and `currentColor`
are accepted. `none` and CSS-wide reset values clear the list. The WinForms
painter caps soft shadows at 12 bands, respects rounded corners, opacity and
ancestor overflow, supports outer and inset field shadows, and leaves layout,
document height, accessibility rectangles, and hits unchanged.

CSS bitmap backgrounds use the non-inherited `S.Bi` alias. A compact `Bi` uses
`U` for the resolved image URL and `R=b/x/y/n` for both-axis, horizontal,
vertical, or no repeat. `X/Y` store position values and `Xp/Yp` mark percentage
units. `W/H` store image size values, `Wp/Hp` mark percentages, `Wa/Ha` mark
auto dimensions, and `K=c/n` selects cover or contain. Lea accepts one atomic
`url()` layer in `background` or `background-image`, repeat keywords,
one/two-value pixel or percentage positions, one/two-value pixel/percentage/
auto sizes after `/`, and cover/contain. URLs resolve against the declaration's
document, linked stylesheet, or direct custom-property origin and are limited
to bounded data images, local files for local origins, HTTP, or HTTPS. Page,
rounded-box, and ordinary-field painters share the existing 24-entry cache,
4 MB resource ceiling, effective opacity, clipping, and responsive loading.
Multiple layers, attachment, origin/clip, blending, and arbitrary file access
from remote styles remain outside this subset.

Replaced raster images use non-inherited compact `S.Of=f/c/v/n/s` for CSS
`object-fit:fill/contain/cover/none/scale-down`. `Ox/Oy` store each
`object-position` coordinate and `Oxp/Oyp` mark percentage rather than pixel
units; the initial position is centered at `50% 50%`. One or two bounded
keyword, signed pixel, compact unitless-pixel, or percentage values are
accepted. Variables, important tiers, CSS-wide reset values, responsive rules,
and malformed-declaration preservation share the computed-style cascade. The
custom view caps fitted dimensions at 8,192 pixels and clips paint to the image
content box. Image-map hit tests, focus rings, and accessibility bounds reuse
the same positioned object rectangle, so contain/none letterboxes are inert
and cover crops retain correct source-coordinate mapping. These properties do
not change the element's layout dimensions.

CSS aspect-ratio sizing uses non-inherited `S.Rw/Rh/Ra`. `Rw` and `Rh` store
the positive preferred width and height components; `Ra` records an authored
`auto` preference so a loaded raster or vector keeps its intrinsic ratio and a
missing resource can use the supplied fallback. Lea accepts one positive
number or `A/B`, with optional `auto` before or after the ratio. Each component
has up to four integer and three fractional digits, permits compact `.5` and
`1.` forms, is capped at 10,000, and the
resolved ratio stays between 0.001 and 1,000. Variables, important tiers,
CSS-wide resets, responsive rules, malformed-declaration preservation, and
noninheritance share the existing cascade. Explicit width or height can derive
the opposite auto axis; two explicit axes win. Content/border boxes, min/max
clamps, text minimums, nested owners, raster/SVG media, fields, flex, grid, and
table minimums use the same 8,192-pixel-capped resolution path.

CSS background gradients use the non-inherited `S.Gi` alias. A compact `Gi` uses
`K=l/r` for linear or radial kind, `A` for the CSS angle, `X/Y` for a radial
percentage origin, and `Ss` for up to eight `Cs` color stops. Each `Cs` stores
its authored color in `C` and normalized 0..1 position in `P`. Lea accepts one
atomic `linear-gradient()` or `radial-gradient()` layer in `background` or
`background-image`; omitted positions interpolate between explicit stops.
Linear heads accept integer `deg` values or `to` direction keywords. Radial
heads accept `circle`/`ellipse` and keyword or percentage origins after `at`.
Named, hex, `rgb()`/`rgba()`, `hsl()`/`hsla()`, `hwb()`, `lab()`, `lch()`, predefined `color()`, `oklab()`, `oklch()`,
bounded sRGB mixes, `transparent`, and `currentColor` colors share the
bounded color parser. Reset values clear the image; malformed, overlong,
multiple-layer, repeating, length-stop, or over-eight-stop declarations preserve
the previous computed image. Page, rounded-box, and ordinary-field painters
apply effective opacity and ancestor clipping without changing layout,
accessibility bounds, document height, or hits.

Positioning stays compact: `Pn` is `s/r/a/f/k` for static, relative, absolute,
fixed, or sticky; `Tp/Rt/Bm/Lt` are nullable signed pixel offsets, and `Zi` is the bounded
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
use viewport coordinates, including when nested. Sticky boxes retain flow and
apply bounded root-scroll `top`/`bottom` motion through their nearest retained
parent; see `STICKY_POSITIONING.md`. Overflow clips inherit through
the parent chain. Numeric positioned z-index, fixed positioning, local opacity,
isolation, and supported pixel translations create compact hierarchical `Zi`
paths; `z-index:auto` does not trap descendants. Paint and hit testing share
that order. Static boxes retain parsed offsets but do not apply them, while
translate offsets apply after normal or positioned placement without consuming
flow.

The WinForms view exposes `Dh` (document height), `Sy` (scroll position), and
`Bc` (cached layout-box count) for compact host diagnostics. `Sc(delta)` moves
the bounded scroll position. `Fa` reports whether one initial page/fragment
anchor is active, `Fj` is its saturated placement count, and `Fy` is the latest
target Y or `-1` when unresolved. First paint, viewport/font changes,
stylesheet/DOM rebuilds, and completed intrinsic images can reapply the target;
ordinary scrolling or focus movement releases it. See
`INITIAL_FRAGMENT_STABILITY.md`.

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

Cache identity is exact without adding another alias. Lea canonicalizes the URI,
compares path and query text ordinally, and removes only the fragment for a
representation key. Case-different resources never share bodies or validators;
fragment-only addresses still share the same response. See `CACHE_IDENTITY.md`.

Persistent caching is opt-in: `Cd` is its directory and the empty default means
no disk I/O. `Cx` is the entry ceiling (32 by default, clamped to 0-128), `Cb`
is the byte ceiling (16 MB by default, clamped to 0-128 MB), and `Dc`/`Db`
report current Lea entry count and bytes. `Cdl()` removes only versioned Lea
cache files. Entries use gzip bodies, SHA-256 exact-URL filenames, bounded metadata,
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

Inline vectors stay compact through `B.Vg`. `Vg.W/H` are intrinsic dimensions;
`X/Y/Vw/Vh` hold the bounded `viewBox`; `Vb` says it was authored; and `Ss`
contains paint-ready `Vs` shapes. `Vg.A=n/nn/nm/nx/mn/mm/mx/xn/xm/xx` stores
none or min/mid/max aspect alignment and `M=m/s` selects meet or slice. A shape
uses `K` for kind, `P` for primitive coordinates, `D` for canonical `Vp` path
steps, `M` for its six-number cumulative transform matrix, `T` for text, `F/S/C` for fill,
stroke, and current color, `W` for stroke width, `O/Fo/So` for total/fill/stroke
opacity, `Z` for font size, `A=s/m/e` for text anchor, and `U/El` for a linked
target and source element. `Fr=n/e` selects nonzero or evenodd fill winding;
`Lc=b/r/s` selects butt, round, or square caps; `Lj=m/r/b` selects miter, round,
or bevel joins; `Ml` is the bounded miter limit; `Da` is the normalized even
dash list; and `Do` is its offset. Each `Vp` is one absolute `K=M/L/C/Q/Z` step with
`X/Y` as its endpoint; `A/B` hold one curve control and cubic steps also use
`C/D` for their second control. Smooth commands are resolved and arcs are
converted to bounded cubic steps while parsing.

Gradient paint remains compact through `Vg.Ds`. Each `Vd` definition uses `I`
for its local ID, `K=l/r` for linear/radial kind, `U=b/u` for object-bounds or
user-space units, `P` for four linear or five radial coordinates, `M` for the
six-number gradient transform, and `Ss` for up to 16 `Vt` stops. A `Vt` exposes
normalized offset `O`, color `C`, and alpha `A`. Shapes resolve fill/stroke
references once into `Vs.Fd/Sd`; unresolved or cyclic local references become
`none` rather than retaining an unrelated inherited solid paint.

Reusable and clipped vectors stay short through `Vg.Cs` and `S.Cp`. A `Vc`
uses `I` for its local ID, `U=u/b` for user-space or object-bounds units, `M`
for its six-number root transform, and `Ss` for up to 32 geometry-only `Vs`
shapes. A painted shape uses `Vs.Cl` to say a clip was requested, `Cp` for the
resolved definition, and `Cm` for the six-number matrix where the clip became
active. Geometry inside `Vc.Ss` uses `Vs.Fr` as its effective nonzero/evenodd
clip winding. Local `<use>` references expand into ordinary `Vs` records, so hosts do
not need a second instance tree; four nested references and 64 indexed IDs are
the bounded ceilings, and cycles add no shapes.
