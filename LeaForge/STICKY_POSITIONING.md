# Bounded Sticky Positioning

Lea supports a deliberately small `position: sticky` model without adding a
second layout engine or a general CSS evaluator. Computed style stores sticky as
the one-letter `S.Pn` value `k`.

## Supported model

- Sticky boxes stay in normal flow and keep their original layout slot.
- Signed pixel `top` and `bottom` insets react to the WinForms view's bounded
  root vertical scroll position.
- A sticky box establishes a retained positioning and stacking context.
- The box, its inline runs, fields, links, images, and nested block descendants
  move by one shared integer offset.
- Motion stops at the nearest retained parent content rectangle. A root sticky
  box is bounded by the document rectangle.
- Overflow clips are rebuilt from retained parent boxes after sticky motion, so
  paint, hits, native editors, keyboard focus, and accessibility use the same
  visible geometry.
- Pages without an active vertical sticky inset skip sticky scroll work.

Both `top` and `bottom` may be present. Lea forms one bounded viewport interval;
when the interval is smaller than the box, `top` wins deterministically. Invalid
`position` declarations remain atomic and preserve the previous valid value.

## Intentional limits

- Lea currently has one root vertical scroller. Independent scrolling overflow
  containers are not implemented.
- `left` and `right` remain parsed inset state but do not create horizontal
  sticky motion because Lea does not expose horizontal scrolling.
- Percentage, viewport-unit, logical, and calculated sticky insets are not
  supported. Insets use the existing bounded signed pixel parser.
- Sticky table row groups and browser-specific paint compositing are outside
  this compact milestone.

`test-pages/sticky-positioning.html` covers the `k` alias, top and bottom inset
state, invalid declaration fallback, normal-flow retention, descendant motion,
viewport pinning, parent clamping, overflow visibility, link hits,
accessibility bounds, native editor placement, and Firespider host integration.
