# Bounded CSS Cascade Layers

Lea supports a deliberately bounded subset of named CSS cascade layers. Named
block layers join the existing compiled-rule path and do not add a public host
API or another browser runtime.

```css
@layer base {
  #notice { color: #164e3f }
}

@layer theme {
  .notice { color: #a13b18 }
}
```

For normal declarations, later named layers beat earlier named layers and
unlayered declarations beat every named layer. Specificity and source order
still decide conflicts inside one layer. Inline normal declarations remain
stronger than stylesheet normal declarations.

For `!important` declarations, named layer order is reversed: the earliest
named layer wins. Layered important declarations beat unlayered important
declarations, and inline important declarations remain strongest. The same
ordering is used for supported ordinary declarations, custom properties,
generated marker/before/after content and styles, direction, SVG paint, and SVG
gradient stops.

## Names And Nesting

- Only named block syntax is accepted: `@layer name { ... }`.
- Reopening the same name extends the existing layer without changing its rank.
- Nested names compose with dots, such as `framework.theme`.
- Names may contain up to three dot-separated segments.
- Each segment is 1-32 ASCII letters, digits, hyphens, or underscores and must
  begin with a letter, underscore, or an optional hyphen followed by one.
- CSS-wide keyword segments are rejected.
- One compiled sheet set accepts at most 16 distinct named layers.
- Combined conditional at-rule nesting retains Lea's existing four-level limit.

Named layers can nest with bounded `@media` and `@supports` rules in either
direction. Named native sheets installed through `Ss` share one global layer
order with authored sheets; replacing or removing a native sheet preserves the
remaining source order and retained field objects.

## Diagnostics

Active rules add a compact `@l(name)` label to `G.Cs`. Labels compose in source
nesting order with existing supports and media labels:

```text
@l(base)
@l(framework.theme)
@l(responsive)&@s(display:grid)&@m>=800
```

## Boundary

Lea does not currently implement anonymous layers, comma-separated layer-order
statements such as `@layer reset, theme;`, origin-wide layer ordering, or full
`revert-layer` rollback semantics. Invalid, anonymous, over-depth, over-length,
and seventeenth-or-later layer blocks stay inactive. Existing bounded reset and
revert handling remains in place but is not a complete `revert-layer` model.

Coverage lives in `test-pages/cascade-layers.html`,
`test-pages/cascade-layer-limits.html`, the focused
`Lea.Tests.exe . --layers` suite, the compatibility benchmark, and the compiled
Firespider host smoke.
