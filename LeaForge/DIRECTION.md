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

This milestone maps horizontal logical CSS and alignment. It does not yet
implement Unicode bidirectional glyph reordering, `dir="auto"`, vertical
writing modes, or direction-based flex/grid item reversal. Text content remains
in authored character order.

`direction-logical.html` verifies HTML/CSS precedence, variables, important
rules, declaration-order independence, logical insets/boxes/borders/radii,
cross-direction inheritance, alignment, command remapping, DOM forms, reset,
and invalid-value preservation. The same `dr` path is exercised inside the
compiled Firespider host smoke.
