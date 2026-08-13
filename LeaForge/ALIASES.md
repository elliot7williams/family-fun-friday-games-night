# Lea Alias Rules

Aliases are a defining part of Lea. They keep small browser programs quick to
read and quick to write.

## Naming rules

- Core objects use one letter: `L` is the entry point, `E` is the engine, `P` is a page.
- Actions use two or three letters: `Go`, `GoA`, `Bk`, `Fw`, `Rl`, `RlA`, `St`, and `At`.
- Page values use one or two letters: `U`, `T`, `H`, `Q`, `D`, `N`, `Bs`, and `Ls`.
- Page commands use one or two letters: `t`, `a`, `bg`, `fg`, and `fs`.
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

`GoA` and `RlA` are the asynchronous forms used by browser windows. `St` cancels
an active network request. The shorter synchronous forms remain useful for tests.

`P.D` is the document root. Each node uses `K` (kind), `X` (text), `A`
(attributes), `C` (children), and `Av(name)` for a short attribute lookup.
`P.Q` preserves the URL query, including on local `file:` pages where older
.NET Framework URI parsing does not expose it through `Uri.Query`.

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
z-index. `B.Up` points to the nearest retained positioned or clipping parent
block. Relative blocks and tables shift while retaining normal flow. Absolute
boxes leave flow and use the nearest positioned containing block; fixed boxes
use viewport coordinates, including when nested. Overflow clips inherit through
the parent chain. Lea paints and hit-tests boxes with stable hierarchical `Zi`
paths so a child remains inside its parent's stacking layer. Static boxes retain
parsed offsets but do not apply them.

The WinForms view exposes `Dh` (document height), `Sy` (scroll position), and
`Bc` (cached layout-box count) for compact host diagnostics. `Sc(delta)` moves
the bounded scroll position.

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
