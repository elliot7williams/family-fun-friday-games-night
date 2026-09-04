# Lea Bounded Size Functions

Milestone 81 / runtime 181 adds a deliberately small subset of CSS `min()`,
`max()`, and `clamp()` to Lea's existing dimension compiler. The same compact
records feed authored CSS, short commands, literal DOM styles, layout, paint,
hits, editing, accessibility, and Firespider without a general expression
evaluator or another browser runtime.

## Supported Forms

`width`, `min-width`, `max-width`, `height`, `min-height`, and `max-height`
accept two or three terms in `min()` and `max()`, and exactly three terms in
`clamp()`:

```css
width: min(75%, 500px);
width: max(20%, 160px, 8rem);
height: clamp(60px, 10vh, 120px);
max-width: min(calc(100% - 40px), 30rem);
```

Each argument is one existing bounded size term:

- fixed `0..2000px`, including unitless zero
- integer `1..100%`
- integer `1..100vw`, `vh`, `vmin`, `vmax`, `em`, or `rem`
- one `calc(PU +/- Npx)`, where `P` is 1..100 and `N` is 0..2000

Whitespace around arguments is allowed. Invalid declarations are atomic and
preserve the earlier valid cascade value. Explicit inheritance, custom
properties, important priority, media replacement, CSS-wide resets, compact
commands, and literal DOM styles use the same parser.

## Compact Model

Function state is lazy. Ordinary fixed, percentage, viewport, font-relative,
and calculated dimensions allocate no function record.

`S.Wk/Wnk/Wxk` expose function kind for preferred, minimum, and maximum width.
`S.Hk/Hnk/Hxk` are the matching height aliases. Kind `0` is a plain value, `1`
is `min`, `2` is `max`, and `3` is `clamp`.

`S.Wq(part)` and `S.Hq(part)` return the argument count. Part `0` is the
preferred size, `1` the minimum, and `2` the maximum. `Wv` and `Hv` expose one
argument value:

```text
Wv(part, argument, 0)  fixed pixels
Wv(part, argument, 1)  relative magnitude
Wv(part, argument, 2)  calculated pixel offset
Wv(part, argument, 3)  unit code
```

`Hv` uses the same indexes. Unit codes remain `0` percentage, `1` vw, `2` vh,
`3` vmin, `4` vmax, `5` em, and `6` rem.

## Resolution

One WinForms resolver evaluates each term from the same containing block,
viewport, element font, and root font references used by plain dimensions.
`min()` chooses the smallest term, `max()` chooses the largest, and `clamp()`
applies the CSS lower/preferred/upper ordering. Final geometry stays inside
Lea's existing 0..8,192 pixel ceiling and content/border-box conversion.

A height function containing a percentage needs a definite height reference.
Below an auto-height parent, the declaration remains inspectable but the whole
function uses natural layout to avoid a circular guess. Viewport-only and
font-only height functions remain resolvable there.

The shared path covers blocks, positioned content, flex, grid, tables, fields,
images, aspect ratios, content and border boxes, paint, hits, native editing,
accessibility bounds, media rebuilds, and live resize.

The existing commands accept the same syntax:

```javascript
wd("panel", "min(50vw, 30rem)");
ht("panel", "clamp(60px, 5em, 100px)");
document.getElementById("panel").style.width = "max(12rem,300px)";
```

An empty value removes only that inline declaration and reveals the authored
cascade.

## Boundary

Lea does not accept one-argument `min()`/`max()`, four or more arguments,
non-three-term `clamp()`, nested size functions, decimals, negative terms,
intrinsic keywords, arbitrary arithmetic, multiplication, or division.
Functions are accepted only for the six dimension properties, not flex basis,
grid tracks, transforms, font size, spacing, or unrelated CSS values.

`test-pages/size-functions.html` covers two- and three-term functions, every
supported term family, calculated arguments, constraints, inheritance,
variables, invalid fallback, unresolved percentage heights, flex, grid,
tables, positioning, fields, images, aspect ratio, border-box sizing,
accessibility, hits, commands, reset, media replacement, and live resize.

## Verification

The Release engine, WinForms view, complete repository suite, compiled
Firespider host, all 42 Boot cases, and all six real EXE startup scenarios pass.
The report covers 77/77 executable probes over 74 fixtures. Its 15-run medians
are 476.482 ms clean diagnostic startup, 3.451 ms local load, and 32.159 ms
layout/paint. Retained managed memory is 18,925.5 KiB for 370 pages, or 51.1
KiB/page. Required runtime files total 536.5 KiB with a 229.0 KiB GZip
estimate. These are local comparison samples, not performance promises; see
`COMPATIBILITY_PERFORMANCE.md`.
