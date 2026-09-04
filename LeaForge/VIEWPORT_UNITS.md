# Lea Viewport Units

Milestone 79 / runtime 179 adds bounded CSS viewport units to Lea's existing
compact size tuples. The engine supports the classic `vw`, `vh`, `vmin`, and
`vmax` units without adding a general expression evaluator or another browser
runtime.

## Supported Values

Authored `width`, `min-width`, `max-width`, `height`, `min-height`, and
`max-height` accept:

```css
width: 50vw;
height: 25vh;
min-width: 20vmin;
max-height: 75vmax;
width: calc(100vw - 24px);
height: calc(50vmin + 10px);
```

The unit term is an integer from 1 through 100. A calculated value contains
exactly one supported percentage or viewport-unit term, one `+` or `-`, and one
0..2000 pixel term. Whitespace inside `calc()` is optional. Fixed values and
ordinary percentages keep their existing behavior.

Invalid declarations are atomic and preserve an earlier valid cascade value.
Explicit `inherit`, custom-property substitution, important priority, resets,
compact commands, and literal DOM style assignment all use the same compiled
path.

## Compact Model

Responsive sizes retain their percentage magnitude and pixel offset in the
existing tuples. One compact unit code distinguishes the reference:

```text
0  containing-block percentage
1  vw
2  vh
3  vmin
4  vmax
```

`S.Wu/Wnu/Wxu` expose the preferred, minimum, and maximum width unit codes.
`S.Hu/Hnu/Hxu` are the matching height aliases. The arrays remain lazy: a page
using only fixed dimensions does not allocate responsive tuple storage.

`V.Vw` and `V.Vh` expose the current WinForms client viewport. Existing
`E.Vw`, `P.Vw`, and `G.Vw` continue to describe the width used by media-query
selection.

## Resolution

`vw` always uses client width, `vh` uses client height, `vmin` uses the smaller
axis, and `vmax` uses the larger axis. Unlike ordinary percentages, viewport
units do not depend on a definite parent height. They therefore remain valid
inside auto-height parents and across width-only or height-only resizes.

One shared resolver feeds blocks, positioned content, flex, grid, tables,
fields, images, aspect ratios, content/border-box conversion, paint, hit tests,
native editing, and accessibility bounds. Declared geometry remains clamped to
Lea's bounded 0..8,192 pixel result and the available layout slot.

The existing commands accept the same values:

```javascript
wd("panel", "calc(50vw - 20px)");
ht("panel", "25vh");
document.getElementById("panel").style.width = "40vmin";
```

An empty value removes only that inline declaration and reveals the authored
cascade.

## Boundary

Lea does not accept decimal or negative unit terms, zero, values above 100,
`svh`, `lvh`, `dvh`, nested calculations, multiplication, division, or
arbitrary operators. A bounded viewport term may be used in standalone
`min()`, `max()`, and `clamp()` dimensions as described by `SIZE_FUNCTIONS.md`;
nested size functions remain unsupported. Bounded `em` and `rem` dimensions are
covered separately by `FONT_RELATIVE_UNITS.md`. The compact engine intentionally
implements deterministic classic viewport units rather than browser-specific
dynamic viewport variants.

`test-pages/viewport-units.html` covers each unit on both axes, cross-axis
sizes, calculations, minimum/maximum constraints, nesting, auto-height parents,
inheritance, custom properties, invalid fallback, flex, grid, tables,
positioning, fields, images, aspect ratios, border-box sizing, accessibility,
hits, compact and literal commands, reset, and live resize.

## Verification

The Release engine, WinForms view, benchmark, compiled Firespider adapter, Boot
harness, all six actual browser startup scenarios, and extracted packaged
runtime pass. The report covers 75/75 executable probes over 72 fixtures. Its
15-run medians are 507.920 ms clean diagnostic startup, 3.299 ms local load,
and 32.998 ms layout/paint. Runtime files total 550.0 KiB with a 233.9 KiB GZip
estimate. These are local comparison samples, not performance promises; see
`COMPATIBILITY_PERFORMANCE.md`.
