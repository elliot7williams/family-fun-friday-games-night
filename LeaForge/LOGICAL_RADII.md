# Lea Logical Corner Radii

Milestone 86 / runtime 186 added bounded logical corner radii to Lea's horizontal
layout. Runtime 187 adds inherited LTR/RTL mapping. The four logical properties compile directly
into the existing clockwise `S.R0/R1/R2/R3` horizontal and `Q0/Q1/Q2/Q3`
vertical radius fields, so the feature adds no style record, layout model, or
renderer allocation.

## Corner Mapping

```text
border-start-start-radius -> top-left (LTR) / top-right (RTL)
border-start-end-radius   -> top-right (LTR) / top-left (RTL)
border-end-end-radius     -> bottom-right (LTR) / bottom-left (RTL)
border-end-start-radius   -> bottom-left (LTR) / bottom-right (RTL)
```

Each property accepts one horizontal radius or a horizontal and vertical pair.
Values are integer `0..2000px` lengths or `0..1000%` percentages. As in Lea's
physical radius support, nonnegative compact fields represent pixels and
negative fields represent percentages. Opposing radii normalize only when the
renderer knows the final box size.

Physical `border-radius` and its four corner longhands remain on the same
records. Physical and logical declarations therefore obey normal source order,
stylesheet/inline tiers, `!important`, custom-property expansion, and dynamic
rebuilds without conversion state. Invalid values preserve the previous corner
atomically.

## Inheritance And Resets

`border-radius: inherit` copies all eight elliptical parent values. Physical
corner longhands copy the matching physical corner. Logical corner `inherit`
reads the parent's direction-aware corner and writes the child's corresponding
direction-aware corner, including cross-direction nesting.
`initial`, `unset`, `revert`, and `revert-layer` restore zero radii for the
target shorthand or corner. This is explicit inheritance; corner radii remain
non-inherited by default.

## Compact Commands

Nine aliases, each no longer than three letters, expose the physical and logical
radius properties to Lea's bounded page-command bridge:

```text
rd   border-radius
rtl  border-top-left-radius
rtr  border-top-right-radius
rbr  border-bottom-right-radius
rbl  border-bottom-left-radius
rss  border-start-start-radius
rse  border-start-end-radius
ree  border-end-end-radius
res  border-end-start-radius
```

Readable forms remove punctuation, such as `borderradius`,
`bordertopleftradius`, and `borderendstartradius`. Familiar literal assignments
such as `style.borderRadius`, `style.borderTopLeftRadius`, and
`style.borderStartStartRadius` use the same validator. Empty DOM assignments
remove only that inline declaration and restore authored CSS.

Commands accept the same bounded pixel/percentage grammar, one optional slash
for the shorthand, CSS-wide inherit/reset keywords, and literal variable
expansion. Negative lengths, unsupported units, duplicate slashes, and too many
corner values are rejected before the document is rebuilt.

## Rendering

No new painting path is required. Backgrounds, per-edge borders, outlines,
overflow clips, fields, images, links, hit bounds, and accessibility geometry
already read the four physical elliptical corners. A logical declaration simply
changes the corresponding compact corner before those shared paths run. The
fixture includes an asymmetric top-left radius and zero top-right radius so an
incorrect corner mapping is visible in exact bitmap samples.

## Limits

Logical corners use horizontal-tb LTR/RTL mapping through `S.Dr`. Lea does not
implement vertical writing modes, `dir=auto`, or bidi glyph reordering.
Fractional lengths, relative units, `calc()`, size functions, and arbitrary CSS

## Verification

`test-pages/logical-radii.html` covers all four logical properties, physical and
logical source order, important tiers, custom properties, shorthand and corner
inheritance, all reset keywords, atomic invalid fallback, all nine short and
readable commands, all familiar DOM forms, authored restore, and asymmetric
paint.

The Release solution, default and core engine suites, compiled Firespider smoke,
all 42 Boot cases, and all six actual-EXE startup scenarios pass. The 15-run
report covers 82/82 executable probes over 79 fixtures. Its medians are 530.557
ms clean diagnostic startup, 3.557 ms local load, and 33.472 ms layout/paint.
Retained managed memory is 20,366.8 KiB for 395 pages, or 51.6 KiB/page.
Required engine runtime files total 555.0 KiB with a 234.6 KiB GZip estimate.
These are local comparison samples, not performance promises; see
`COMPATIBILITY_PERFORMANCE.md`.
