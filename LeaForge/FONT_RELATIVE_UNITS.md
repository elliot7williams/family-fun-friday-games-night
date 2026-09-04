# Lea Font-Relative Units

Milestone 80 / runtime 180 adds bounded `em` and `rem` dimensions to Lea's
existing compact size tuples. They share the percentage and viewport-unit
compiler, layout resolver, commands, and retained rendering path without adding
a general CSS expression evaluator or another browser runtime.

## Supported Values

Authored `width`, `min-width`, `max-width`, `height`, `min-height`, and
`max-height` accept:

```css
width: 10em;
height: 4rem;
min-width: 8rem;
max-height: 5em;
width: calc(10em - 20px);
height: calc(5rem + 10px);
```

The font-relative term is an integer from 1 through 100. A calculated value
contains exactly one supported relative term, one `+` or `-`, and one 0..2000
pixel term. Whitespace inside `calc()` is optional.

Invalid declarations are atomic and preserve an earlier valid cascade value.
Explicit `inherit`, custom-property substitution, important priority, CSS-wide
resets, compact commands, and literal DOM style assignment use the same path.

## Compact Model

Responsive sizes retain a magnitude and pixel offset in the existing tuples.
The complete unit-code set is:

```text
0  containing-block percentage
1  vw
2  vh
3  vmin
4  vmax
5  em
6  rem
```

`S.Wu/Wnu/Wxu` expose preferred, minimum, and maximum width units.
`S.Hu/Hnu/Hxu` are the matching height aliases. Storage remains lazy, so pages
using only fixed dimensions do not allocate responsive tuple arrays.

`P.Rf` exposes an explicitly computed root `<html>` font size, or zero when the
page uses the host default. `V.Rf` exposes the root size actually used by layout,
including the hosted control's fallback font size.

## Resolution

`em` resolves from the target element's computed `S.Fs`, falling back to the
hosted view font. `rem` resolves from the computed root `<html>` font, with the
same host fallback when the root does not specify one. Root media changes and
host font changes therefore update relative geometry on the next layout.

One resolver feeds blocks, positioned content, flex, grid, tables, fields,
images, aspect ratios, content/border-box conversion, paint, hit tests, native
editing, and accessibility bounds. Results stay inside Lea's existing bounded
0..8,192 pixel geometry.

The existing commands accept the same values:

```javascript
wd("panel", "8rem");
ht("panel", "5em");
document.getElementById("panel").style.width = "10em";
```

An empty value removes only that inline declaration and reveals the authored
cascade.

## Boundary

Font-relative dimensions do not make `font-size` font-relative. Lea continues
to accept only its bounded integer pixel form for `font-size`; unsupported
values such as `font-size: 2em` are rejected atomically instead of being partly
parsed as pixels.

Lea does not accept decimal, zero, negative, or above-100 relative terms, `ex`,
`ch`, `cap`, `ic`, `lh`, `rlh`, nested calculations, multiplication, division,
or arbitrary operators. A bounded `em` or `rem` term may be used in standalone
`min()`, `max()`, and `clamp()` dimensions as described by `SIZE_FUNCTIONS.md`;
nested size functions remain unsupported. The compact model favors a small
deterministic implementation over full CSS Values compatibility.

`test-pages/font-relative-units.html` covers both units on both axes,
calculations, min/max constraints, nested element fonts, root media changes,
inheritance, custom properties, invalid fallback, flex, grid, tables,
positioning, fields, images, aspect ratios, border-box sizing, accessibility,
hits, compact and literal commands, reset, font mutation, and live resize.

## Verification

The Release engine, WinForms view, complete repository suite, compiled
Firespider host, all 42 Boot cases, and all six real EXE startup scenarios pass.
The report covers 76/76 executable probes over 73 fixtures. Its 15-run medians
are 517.283 ms clean diagnostic startup, 3.494 ms local load, and 32.135 ms
layout/paint. Retained managed memory is 18,243.2 KiB for 365 pages, or 50.0
KiB/page. Required runtime files total 533.5 KiB with a 227.7 KiB GZip
estimate. These are local comparison samples, not performance promises; see
`COMPATIBILITY_PERFORMANCE.md`.
