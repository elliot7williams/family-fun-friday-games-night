# Lea Logical Borders

Milestone 85 / runtime 185 added bounded logical CSS borders to Lea's horizontal
layout. Runtime 187 adds inherited LTR/RTL mapping. Logical declarations compile into the four existing border
widths. One lazy eight-string detail record stores per-edge style and color only
when the edges differ; uniform borders keep the previous compact allocation path.

## Properties

Lea supports logical border shorthands for both axes and all four edges:

```text
border-inline        -> left and right
border-block         -> top and bottom
border-inline-start  -> left (LTR) / right (RTL)
border-inline-end    -> right (LTR) / left (RTL)
border-block-start   -> top
border-block-end     -> bottom
```

Each form accepts one bounded width, one supported style, and one color in any
order. Omitted parts preserve their current component, matching Lea's compact
style-command behavior. A visible style without a width uses the existing
three-pixel medium width.

The matching `-width`, `-style`, and `-color` component properties are supported
for both axes and all four edges. Axis components accept one value for both ends
or two values in start/end order. Physical `border-width`, `border-style`, and
`border-color` now accept atomic one-to-four-value lists, and physical side
shorthands/longhands retain style and color on their own edge instead of changing
every side.

## Values And Cascade

Widths accept `thin`, `medium`, `thick`, or integer `0..100px` values. Supported
styles are `none`, `hidden`, `solid`, `dashed`, `dotted`, and `double`. Colors use
Lea's bounded named, hex, `rgb()`, and `rgba()` parser, including function values
that contain spaces.

Physical and logical declarations share source order, important tiers, custom
properties, and the same four computed edges. `inherit` copies the corresponding
logical parent edges, translating when parent and child directions differ.
`initial` and `unset` restore Lea's zero/hidden default. Invalid
lists, duplicate shorthand parts, unknown styles, and malformed colors preserve
the prior declaration atomically.

## Compact Commands

All 24 logical properties have aliases no longer than three letters:

```text
bi / bb                 border inline / block
bis / bie               border inline start / end
bbs / bbe               border block start / end
biw / biy / bic         border inline width / style / color
bbw / bby / bbc         border block width / style / color
siw / siy / sic         start-inline width / style / color
eiw / eiy / eic         end-inline width / style / color
sbw / sby / sbc         start-block width / style / color
ebw / eby / ebc         end-block width / style / color
```

Readable command forms remove punctuation, such as `borderinline`,
`borderblockend`, and `borderinlinestartcolor`. All matching literal DOM forms,
including `style.borderInline`, `style.borderBlockWidth`, and
`style.borderInlineStartColor`, translate to the same bounded commands. An empty
DOM assignment removes only that inline declaration and restores authored CSS.

## Compact Records And Paint

`S.Bt/Br/Bb/Bl` remain the physical widths. `S.Bsv(edge)` and `S.Bcv(edge)` expose
the effective style and color for edge `0=top`, `1=right`, `2=bottom`, and
`3=left`. `S.Bec` reports `0` for the uniform path or `8` mixed-edge slots.
Uniform values continue through `S.Bs/Bc` without an edge allocation.

The WinForms renderer now applies each effective style before measurement, so a
`none` or `hidden` edge contributes no used border width. Normal, positioned,
flex, grid, table, form, paint, hit, editor, and accessibility geometry continue
to share the physical box path. Uniform rounded borders retain the old fast paint
path. Mixed styles/colors use four clipped border regions so rounded corners meet
without one side recoloring the whole ring.

## Limits

Lea keeps one horizontal physical record model and remaps inline edges through
inherited `direction:ltr/rtl`. Bidi glyph reordering, `dir=auto`, vertical
writing modes, CSS-wide global keywords, fractional lengths, percentages,
images, and arbitrary border style keywords are outside this feature. `double` retains Lea's current solid-fill
visual approximation. These limits avoid a second layout model or a large CSS
runtime. See `DIRECTION.md` and `LOGICAL_RADII.md`.

## Verification

`test-pages/logical-borders.html` covers all 24 logical properties, physical
one-to-four-value components, per-edge source order, important declarations,
custom properties, functional colors, inheritance/reset, malformed atomic
fallback, all compact aliases, all familiar DOM forms, authored restore, hidden
edge geometry, and mixed rounded paint.

The Release solution, engine and WinForms suites, compiled Firespider smoke, all
42 Boot cases, and all six real EXE startup scenarios pass. The report covers
81/81 executable probes over 78 fixtures. Its 15-run medians are 534.686 ms clean
diagnostic startup, 3.658 ms local load, and 33.597 ms layout/paint. Retained
managed memory is 20,091.5 KiB for 390 pages, or 51.5 KiB/page. Required engine
runtime files total 553.0 KiB with a 234.0 KiB GZip estimate. These are local
comparison samples, not performance promises; see `COMPATIBILITY_PERFORMANCE.md`.
