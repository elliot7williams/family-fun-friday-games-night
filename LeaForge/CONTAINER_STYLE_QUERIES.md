# Container Style Queries

Milestone 135 adds a bounded, standards-shaped subset of CSS container style
queries to Lea's existing compiled stylesheet path. It does not add a second CSS
engine, layout feedback loop, or browser dependency.

## Supported Shape

Every element can act as a style-query container. A descendant may select the
nearest ancestor:

```css
.panel { --mode: compact; }

@container style(--mode: compact) {
  .summary { display: block; }
}
```

Or it may select the nearest ancestor with a computed `container-name`:

```css
.panel {
  container-name: panel frame;
  --mode: compact;
}

@container panel style(--mode: compact) {
  .summary { width: 240px; }
}
```

The query compares the selected ancestor's computed custom-property value with
the expected value after bounded `var()` resolution. Comparisons are exact after
outer whitespace is trimmed. A container cannot select itself for a rule used to
compute that same element.

Lea accepts grouped `not`, `and`, and `or` conditions. Ungrouped mixtures of
`and` and `or` are rejected instead of guessing precedence. Nested container
rules remain separate, so each can select a different ancestor. They also
compose with supported `@media`, `@supports`, and `@layer` blocks, linked CSS,
`<style media>`, pseudo styles, SVG paint rules, and named native stylesheets.

## Compact API

Computed style exposes `S.Cn`, a space-separated list of up to four valid
container names. `none` is represented by an empty string. The existing
`E/V.Su` capability route recognizes supported `container-name` declarations.

No new host object is required. Class, hover, focus, viewport, and native-sheet
rebuilds reevaluate queries through the existing retained page path, preserving
field identity, user-entered values, and host ownership.

Diagnostics add compact `@c(...)` condition labels. Nested labels compose with
existing `@m`, `@s`, and `@l` media, supports, and layer labels.

## Limits

- four nested conditional/container levels
- eight style-query terms per condition
- 2,048 UTF-16 units of condition text
- four unique container names per element
- 32 ASCII identifier characters per name
- custom properties only, using the existing declaration and variable bounds

Malformed rules, unknown names, CSS-wide cascade-dependent query values, size
conditions, and unsupported property queries stay inactive.

## Deliberately Unsupported

Lea does not yet implement size or scroll-state container queries,
`container-type`, container-relative units, general property style queries,
relative-value comparisons, typed custom properties, or query-related CSSOM.
Size queries need a stable used-size/containment feedback model; adding syntax
without that model would produce misleading results.

The behavior follows the container selection and computed-value model in the
W3C CSS Containment Module Level 3 draft:
https://www.w3.org/TR/css-contain-3/

## Verification

`container-style.html` and `Lea.Tests --container-style` cover unnamed and named
ancestor selection, name lists, self-query suppression, custom-property
resolution, grouped logic, different nested ancestors, media/supports/layer
composition, malformed and unsupported rules, diagnostics, class and native
sheet rebuilds, retained fields, and exact WinForms geometry. The compatibility
runner and compiled Firespider smoke repeat the integration-level paths.
