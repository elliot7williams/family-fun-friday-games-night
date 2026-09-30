# Container Size Queries

Milestone 136 adds a bounded used-size feedback path for CSS container size
queries. It extends Lea's compiled stylesheet and retained layout model; it does
not embed another browser engine or add a second CSS parser.

## Container State

Lea supports the longhands and shorthand below:

```css
.shell {
  container-name: shell frame;
  container-type: inline-size;
}

.panel {
  container: panel / size;
}
```

Computed style exposes `S.Ct`: `n` is `normal`, `i` is `inline-size`, and `s`
is `size`. Existing `S.Cn` continues to hold up to four space-separated names.
Both properties are non-inherited, while an explicit `inherit` value remains
available through the normal cascade.

An inline-size container can answer `width` and `inline-size` queries. A size
container can also answer `height` and `block-size` queries. Selection starts
at the element's parent, so a rule cannot query the element it is currently
styling. Named queries select the nearest eligible ancestor with that name;
unnamed queries select the nearest eligible ancestor.

## Query Shape

The supported feature forms use bounded integer pixels:

```css
@container panel (min-width: 300px) { }
@container panel (width >= 300px) { }
@container panel (300px <= inline-size < 600px) { }
@container panel (height: 180px) { }
@container panel (block-size > 160px) { }
```

Boolean width/height features, feature-first and value-first comparisons,
strict/inclusive chains, `min-`/`max-` syntax, grouped `not`/`and`/`or`, and
mixed size plus `style(--name:value)` conditions share the same compiled query
tree. Nested container rules select their own ancestors and continue to compose
with supported media, supports, layer, linked-sheet, pseudo, SVG, and native
stylesheet paths.

Milestone 137 adds bounded reduced-integer `aspect-ratio` ranges and discrete
portrait/landscape orientation over the same measurements. These features need
both axes and therefore select only `container-type:size` ancestors. See
`CONTAINER_RATIO_ORIENTATION.md` for syntax, limits, and verification.

## Feedback API

`Cz` is a compact immutable host record containing `E`, `W`, and `H` for one
element and content box. `E.Cz(page, sizes)` validates and applies one complete
snapshot atomically, returns `1` only when it changed, then runs the ordinary
retained rebuild. Mutable fields keep their object identity, values, selection,
and host ownership.

`Lea.Win.V` performs this bridge automatically. After layout settles its
existing auto-height pass, it gathers actual content rectangles for retained
containers, submits one snapshot, and lays out the rebuilt page. The process
stops when sizes are unchanged and has a hard three-feedback-pass ceiling.
This gives query styles real rendered dimensions without an unbounded feedback
loop or computed-width shortcut. The body content box is also reported when
the body itself is a container.

## Limits

- 256 measured containers per page snapshot
- `0..8192` pixels per reported axis and query value
- three automatic WinForms feedback passes
- four nested conditional/container levels
- eight size/style terms per condition
- 2,048 UTF-16 units of condition text
- horizontal writing-mode aliases only

Malformed, duplicate, foreign-page, or over-limit host snapshots are rejected
without changing the active snapshot. A missing measurement leaves the size
query inactive. `container-type:size` establishes a retained layout context,
but Lea does not yet reproduce every intrinsic size-containment side effect;
definite heights are the dependable boundary for block-axis queries.

## Deliberately Unsupported

Container-relative units (`cqw`, `cqh`, and related units), relative query
lengths, scroll-state queries, vertical writing modes, query CSSOM, and full
intrinsic containment remain outside this milestone. General-property style
queries and typed custom properties also remain outside the style-query subset.

The selection and feature model follows the CSS Containment Module Level 3:
https://www.w3.org/TR/css-contain-3/

## Verification

`container-size.html` and `Lea.Tests --container-size` cover longhands and the
shorthand, compact computed aliases, pre-feedback behavior, atomic snapshots,
nearest and named selection, both axes and logical aliases, min/max and modern
ranges, logical conditions, mixed style/size tests, nested containers,
media/supports/layers, diagnostics, malformed and ineligible queries, native
sheets, dynamic width changes, retained fields, and exact automatic WinForms
geometry. The compatibility runner and compiled Firespider smoke repeat the
host-level feedback path.
