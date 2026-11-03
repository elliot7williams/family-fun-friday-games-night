# Container Relative Units

Milestone 138 adds bounded container-relative dimensions to Lea's existing
compact size-value records and WinForms feedback loop. It does not add a
second value parser or a parallel layout path.

## Units

The supported unit codes are:

| CSS unit | Compact code | Basis |
| --- | ---: | --- |
| `cqw` | 7 | 1% of the nearest eligible width container |
| `cqh` | 8 | 1% of the nearest eligible height container |
| `cqi` | 9 | 1% of the nearest eligible inline-axis container |
| `cqb` | 10 | 1% of the nearest eligible block-axis container |
| `cqmin` | 11 | smaller of the independently selected `cqi` and `cqb` bases |
| `cqmax` | 12 | larger of the independently selected `cqi` and `cqb` bases |

Lea's current layout model is horizontal, so `cqw` and `cqi` share the inline
axis while `cqh` and `cqb` share the block axis. Inline selection accepts the
nearest ancestor with `container-type:inline-size` or `size`. Block selection
skips inline-only containers and requires `container-type:size`. A unit never
uses the element itself as its container.

The two axes are selected independently. One element can therefore resolve
`cqi` against a close inline-size container and `cqb` against a more distant
size container. `cqmin` and `cqmax` compare those two resulting one-percent
lengths. If an axis has no eligible ancestor, Lea uses the current client
viewport for that axis, matching the small-viewport fallback shape in the CSS
Containment specification.

## Value Shape

Container units reuse the existing dimension tuple and function records:

```css
.card {
  width: 50cqw;
  height: calc(25cqb + 5px);
  min-width: 20cqi;
  max-width: clamp(120px, 60cqmin, 400px);
}
```

They work in `width`, `height`, `min-width`, `max-width`, `min-height`, and
`max-height`; in bounded `calc()`, `min()`, `max()`, and `clamp()` terms; and
through custom properties, inheritance, native sheets, compact `wd`/`ht`,
readable `width`/`height`, and literal DOM style commands. Each relative
magnitude is an integer from 1 through 100. `calc()` accepts one signed pixel
offset up to 2,000 pixels. Size functions retain the existing two/three-term
limit.

Explicit `inherit` preserves the originating property's container chain. This
matters when the parent is itself a query container: the child receives the
parent's computed basis instead of incorrectly selecting that new nearer
container. Lazy `S.Wqo/Hqo` arrays exist only when an inherited property needs
that origin. Public compact `S.Cq(unit, viewportWidth, viewportHeight, height,
part)` exposes the selected bounded basis for host diagnostics without
exposing the internal ancestor chain.

## Feedback

`Lea.Win.V` continues to gather content boxes and submit one atomic `E.Cz`
snapshot after layout. A rebuilt style can then resolve container units from
those measurements and lay out again. The same hard three-pass ceiling handles
units, size queries, and nested containers; unchanged snapshots stop the loop.
Fields retain identity, values, selection, and ownership across every rebuild.

## Limits

- integer magnitudes from 1 through 100
- dimensions and dimension constraints only
- up to three terms in one bounded size function
- one signed pixel offset in `calc()`
- 256 measured containers per snapshot
- `0..8192` measured pixels per axis
- three automatic WinForms feedback passes
- horizontal writing mode only

Zero, decimal, over-100, unknown-unit, nested-expression, multiplication, and
over-limit values are rejected atomically, preserving the earlier declaration.

## Deliberately Unsupported

Container units in margins, padding, borders, insets, transforms, typography,
grid tracks, and gaps remain outside this slice. Bounded query thresholds are
documented in `CONTAINER_QUERY_LENGTHS.md` and
`CONTAINER_QUERY_VIEWPORT.md`.
Vertical writing modes, named-unit selection, dynamic viewport unit families,
general CSS Typed OM, and complete intrinsic containment are also unsupported.

The axis-selection and fallback behavior follows the W3C CSS Containment
Module Level 3: https://www.w3.org/TR/css-contain-3/#container-lengths

## Verification

`container-units.html` and `Lea.Tests --container-units` cover all six compact
codes, separate inline/block ancestor selection, `cqmin`/`cqmax`, missing-axis
viewport fallback, `calc()` and size functions, custom properties, computed
inheritance, invalid preservation, compact/readable/DOM commands, native
sheets, dynamic container sizes, retained fields, and exact WinForms geometry.
The compatibility runner and compiled Firespider smoke repeat the model,
feedback, retention, and wide/narrow host paths.
