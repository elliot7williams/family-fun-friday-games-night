# Lea Horizontal Direction

Lea supports inherited horizontal `direction: ltr` and `direction: rtl` through
the compact `S.Dr` field (`l` or `r`). The default is `l`. An HTML `dir="ltr"`
or `dir="rtl"` attribute supplies the element's base direction, and CSS can
override it through the normal, inline, and `!important` cascade.

Direction is resolved before ordinary declarations. This keeps logical
properties correct even when `direction` appears after them in the authored
rule or obtains its value from a bounded custom property.

## Logical Mapping

In horizontal flow, block start/end remain top/bottom. Inline edges map as:

| Direction | Inline start | Inline end |
| --- | --- | --- |
| `ltr` | left | right |
| `rtl` | right | left |

The mapping applies to logical insets, margin, padding, border shorthands,
border width/style/color components, all four logical corner radii, and
`text-align: start/end`. Physical top/right/bottom/left properties keep their
physical meaning.

Explicit `inherit` on a logical box, border, or radius property reads the
parent's corresponding logical side using the parent's direction and writes it
to the child's corresponding side using the child's direction. This preserves
logical intent when an LTR child is nested inside an RTL parent or vice versa.

## Flex And Grid Layout

Horizontal flex rows use inline direction for their main axis. In RTL, a normal
`row` starts at the physical right and advances left; `row-reverse` cancels that
reversal and advances right. `justify-content: start/end`, explicit `order`, and
wrapped line placement use the same effective direction. Column flex keeps its
vertical main axis, while horizontal `align-items` and `align-self` start/end
map through the container direction.

Grid placement remains logical. Logical column 1 is inline-start, so it is the
rightmost track in RTL. The renderer maps auto placement, row/column dense flow,
explicit lines and spans, named areas, unequal tracks, content alignment, and
item self-alignment onto physical coordinates only after bounded placement is
complete. This avoids a second RTL occupancy model and leaves document,
accessibility, and command target order unchanged.

## Short Command

`dr` is the compact direction command:

```text
dr("rtl")                 body direction
dr("panel", "ltr")       element direction
direction("panel", "rtl") readable form
```

The literal forms `document.body.style.direction = "rtl"` and
`document.getElementById("panel").style.direction = "ltr"` use the same bounded
path. Assigning an empty value removes the inline direction and restores the
authored cascade. Unsupported values are rejected atomically.

## Boundary

This milestone maps horizontal logical CSS, flex/grid axes, and alignment. It
does not yet implement Unicode bidirectional glyph reordering, `dir="auto"`,
vertical writing modes, or `wrap-reverse`. Text content remains in authored
character order.

`direction-logical.html` verifies HTML/CSS precedence, variables, important
rules, declaration-order independence, logical insets/boxes/borders/radii,
cross-direction inheritance, alignment, command remapping, DOM forms, reset,
and invalid-value preservation. `direction-layout.html` verifies exact LTR/RTL
row, reverse, order, wrap, column cross-axis, grid auto/dense/column/explicit/
area placement, unequal spans, content/self alignment, paint, source order, and
live `dr` reflow. The same geometry path is exercised inside the compiled
Firespider host smoke.
