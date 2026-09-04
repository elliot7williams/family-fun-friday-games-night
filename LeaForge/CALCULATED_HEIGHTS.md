# Lea Calculated Heights

Milestone 78 / runtime 178 extends Lea's compact sizing model to responsive
heights without adding a general CSS expression evaluator. Values compile once
into a percentage and pixel-offset tuple, then resolve only when layout has a
definite containing height.

## Supported Values

Authored `height`, `min-height`, and `max-height` accept:

```css
height: 320px;
height: 50%;
height: calc(100% - 40px);
min-height: calc(50% + 12px);
max-height: 75%;
```

Fixed terms are limited to 0..2000 pixels. To preserve Lea's earlier authored
CSS behavior, a plain four-digit pixel height above the limit is capped at
2000 pixels. Compact and literal DOM commands reject out-of-range values.
Percentage terms are limited to 1..100. The calculated form contains exactly
one percentage, one `+` or `-`, and one 0..2000 pixel term. Whitespace inside
`calc()` is optional.

Invalid declarations are atomic and preserve an earlier valid declaration.
`auto`, `initial`, `unset`, `revert`, and `revert-layer` reset preferred or
minimum height; `none` resets maximum height. Explicit `inherit` copies the
parent's fixed or calculated tuple. Custom-property substitution and important
priority use the existing compiled declaration path.

## Compact State And Commands

`S.Hp/Ho`, `Hnp/Hno`, and `Hxp/Hxo` expose preferred, minimum, and maximum
percentage/offset tuples. The fixed pixel aliases remain `H/Hn/Hx`. Tuple
storage is allocated only when a responsive height is present.

The compact ID-scoped command is:

```javascript
ht("panel", "calc(100% - 24px)");
ht("panel", "50%");
ht("panel", "");
```

`height(id,value)` is the readable alias. An empty value removes only the inline
height and reveals the authored cascade while retaining unrelated inline styles
and mutable form state. The literal compatibility form uses the same path:

```javascript
document.getElementById("panel").style.height = "50%";
```

## Definite Layout

Root percentages resolve against the WinForms viewport below Lea's document
inset. Nested percentages resolve against an explicitly sized retained parent,
the definite containing block for absolute/fixed positioning, or a definite
grid area. A percentage under an auto-height parent remains authored state but
uses natural auto layout, avoiding circular percentage guesses.

The shared resolution path covers ordinary and positioned blocks, native
controls, images and vectors, row/column flex items, definite grid areas,
tables, aspect-ratio width derivation, content/border-box conversion, paint,
hits, native editing, accessibility bounds, and live vertical resize.

## Boundary

Lea does not accept `min()`, `max()`, `clamp()`, multiplication, division,
nested calculations, arbitrary operators, negative percentages, percentages
above 100, intrinsic keywords, or calculated flex basis. Classic bounded
viewport units are covered by `VIEWPORT_UNITS.md`; bounded `em`/`rem`
dimensions are covered by `FONT_RELATIVE_UNITS.md`.
Percentage heights inside auto-height table cells do not establish a definite
reference. Independent overflow scrollers remain outside the compact renderer.

`test-pages/calculated-heights.html` covers fixed and percentage heights, the
legacy bounded fixed-height cap, positive and negative offsets, calculated
minimum/maximum constraints, custom
properties, inheritance, invalid fallback, unresolved auto parents, flex/grid,
table and aspect-ratio geometry, border-box conversion, compact and literal
commands, positioned blocks, native controls and images, accessibility bounds,
inline reset, and live viewport resize.

## Verification

The final Release engine, WinForms view, tests, benchmark, and Firespider host
build successfully. The full repository and compiled host smokes pass. The
benchmark passes 74/74 probes over 71 fixtures, with 578.142 ms clean diagnostic
startup, 3.413 ms median local load, and 35.418 ms median layout/paint. All 42
Firespider Boot cases and all six real EXE startup scenarios pass; the startup
windows remain responsive, use Lea, avoid MSHTML, and show no invalid-cast, JIT,
script-error, or crash-recovery dialog.
