# Lea Calculated Widths

Milestone 76 / runtime 176 adds responsive width values without adding a general
CSS expression evaluator. Values compile once into a compact percentage and
pixel-offset tuple, then resolve against the current containing width during
normal layout and resize.

## Supported Values

Authored `width`, `min-width`, and `max-width` accept:

```css
width: 320px;
width: 50%;
width: calc(100% - 40px);
min-width: calc(50% + 12px);
max-width: 75%;
```

Fixed terms are limited to 0..2000 pixels. Percentage terms are limited to
1..100. The calculated form contains exactly one percentage, one `+` or `-`,
and one 0..2000 pixel term. Whitespace inside `calc()` is optional.

Invalid declarations are atomic: they do not erase an earlier valid declaration
in the cascade. `auto`, `initial`, `unset`, `revert`, and `revert-layer` reset
width/minimum state; `none` resets maximum width. Explicit `inherit` copies the
parent's fixed or calculated tuple. Custom-property substitution and important
priority use the existing compiled declaration path.

## Short Command

The compact ID-scoped form is:

```javascript
wd("panel", "calc(100% - 24px)");
wd("panel", "50%");
wd("panel", "");
```

`width(id,value)` is the readable alias. An empty value removes only the inline
width declaration and reveals the authored cascade while retaining unrelated
inline styles and mutable form state.

The literal compatibility form maps to the same command:

```javascript
document.getElementById("panel").style.width = "50%";
```

IDs, source length, command count, inline-style length, safe-target checks, and
the retained single-rebuild behavior are unchanged.

## Layout

`Lea.Win.V` resolves calculated widths against the active containing content
width. The shared path covers nested blocks, controls, images, flex items, grid
areas, tables, table-cell content, paint bounds, hit testing, and accessibility
bounds. Percentages and calculations reevaluate when the hosted view resizes.

Content-box and border-box conversion happens after the declared value resolves.
Lea still caps rendered geometry to its bounded available slot and 8,192-pixel
viewport ceiling.

## Boundary

Lea does not accept `min()`, `max()`, `clamp()`, multiplication, division,
nested calculations, arbitrary operators, negative percentages, percentages
above 100, font units, or intrinsic keywords. Calculated heights are covered by
`CALCULATED_HEIGHTS.md`; classic bounded viewport units are covered by
`VIEWPORT_UNITS.md`. Unsupported values remain inert.

`calculated-widths.html` covers fixed and percentage widths, positive and
negative calculated offsets, calculated minimum/maximum constraints, custom
properties, explicit inheritance, invalid cascade fallback, nested flex/grid
and table geometry, compact and literal commands, inline reset, and live resize.

## Verification

The Release engine, WinForms suite, compiled Firespider adapter, and exact staged
runtime all pass. The compatibility report passes 72/72 probes over 69 fixtures.
All 42 Boot cases pass in one combined run, including the real 35-second recovery
timer without the previous startup cast dialog. All six actual EXE scenarios are
responsive and MSHTML-free.

Fifteen-run medians on the packaged Framework build are 504.566 ms for a clean
diagnostic process, 3.470 ms for local fixture loads, and 31.742 ms for layout
and paint. Retained managed memory is 16,587.0 KiB for 345 pages, or 48.1
KiB/page. Required runtime files total 544.5 KiB with a 231.8 KiB GZip estimate.
These local diagnostics are comparison samples, not performance promises.
