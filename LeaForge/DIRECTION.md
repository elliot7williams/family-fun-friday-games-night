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

## Inline Flow And Markers

Horizontal RTL lines keep inline atoms in source order but place the first atom
at the physical right and advance toward the left. This applies to plain text,
mixed styled runs and links, wrapped and justified lines, preserved line groups,
and structured table-cell parts. Characters inside each bounded atom remain in
authored order; Lea does not run a Unicode bidi shaping algorithm.

Physical `text-align: left/right/center` values keep their physical meaning.
Logical `start/end` values are already mapped through `S.Dr`, and the default or
last justified RTL line starts at the right edge. Unwrapped clipped lines keep
that right edge and overflow toward the left.

List and disclosure markers reuse the existing compact `B.Mk` atom. Ordered,
unordered, nested, and summary markers therefore appear at inline-start on the
right in RTL and their content continues toward the left. DOM, link,
accessibility, and painted-box enumeration remain in authored source order.

## Ellipsis And Text Ranges

For a bounded horizontal line that overflows toward inline-end, Lea places the
ellipsis at the physical right in LTR and the physical left in RTL. The
authored prefix remains the visible text in both directions. RTL prefixes are
right-aligned inside their original source atom, while original layout and link
hit rectangles remain unchanged. Prefix selection stays bounded to .NET text
elements, so combining sequences are not split.

The read-only UI Automation TextPattern keeps the complete authored `P.X`
string and never inserts the synthetic marker. Each retained text segment also
carries its physical inline direction privately: partial RTL ranges grow from
the right, point lookup converts physical positions back to logical offsets,
and wrapped RTL line ranges restart at the right while preserving source order.
Hidden suffixes have no visible rectangle; the painted prefix does.

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

This milestone maps horizontal logical CSS, flex/grid axes, inline atom flow,
markers, alignment, inline-end ellipsis, and read-only range geometry. It does
not implement Unicode bidirectional glyph reordering or embedding, `dir="auto"`,
vertical writing modes, `wrap-reverse`, two-ended/custom overflow markers, or
line clamping. Text content remains in authored character order.

`direction-logical.html` verifies HTML/CSS precedence, variables, important
rules, declaration-order independence, logical insets/boxes/borders/radii,
cross-direction inheritance, alignment, command remapping, DOM forms, reset,
and invalid-value preservation. `direction-layout.html` verifies exact LTR/RTL
row, reverse, order, wrap, column cross-axis, grid auto/dense/column/explicit/
area placement, unequal spans, content/self alignment, paint, source order, and
live `dr` reflow. `direction-inline.html` verifies plain/mixed/wrapped/nowrap/
justified lines, physical and logical alignment, nested list and disclosure
markers, table parts, links, source order, paint, and command reflow. The same
geometry paths are exercised inside the compiled Firespider host smoke.
`direction-overflow.html` verifies LTR/RTL marker edges, right-aligned RTL paint
prefixes, combining text elements, retained link hits, table-cell clipping,
full logical automation text, RTL partial/wrapped/point range geometry, and live
`dr` remapping.
