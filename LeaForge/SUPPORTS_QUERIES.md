# Bounded CSS Supports Queries

Lea accepts a deliberately bounded subset of CSS `@supports` feature queries.
The condition is evaluated while the stylesheet is compiled. A true condition
adds its rules to the ordinary compiled cascade; false, malformed, unknown, or
over-limit conditions leave the whole block inactive.

```css
@supports (display: grid) {
  .layout { display: grid; grid-template-columns: 1fr 2fr }
}

@supports selector(:has(> .ready)) {
  .panel:has(> .ready) { border-left: 4px solid #176b4d }
}
```

## Conditions

Lea supports:

- one declaration condition inside parentheses, such as `(display: grid)`;
- one `selector(...)` condition compiled through Lea's real selector grammar;
- case-insensitive `not`, `and`, and `or` operators;
- parenthesized groups and nested `@supports` blocks;
- either operator at one level; mixing `and` and `or` requires grouping;
- nesting in either direction with bounded `@media` rules.

`selector(...)` tests syntax and Lea capability, not whether a matching element
exists in the current document. It accepts one selector under the normal Lea
selector length, chain, specificity, functional-selector, and traversal limits.

## Declaration Families

Feature queries conservatively recognize these existing Lea parsers:

- `display`, `position`, `box-sizing`, and `aspect-ratio`;
- physical and logical width/height plus their min/max forms;
- flex direction, wrapping, grow, shrink, and basis;
- `gap`, legacy grid-gap names, and row/column gap;
- content, item, and self justification/alignment;
- explicit and automatic grid tracks plus `grid-auto-flow`;
- `object-fit` and `object-position`;
- `text-wrap`, `text-wrap-mode`, and `text-wrap-style`;
- `pointer-events`, `overflow`, `text-overflow`, `line-clamp`, and
  `-webkit-line-clamp`;
- bounded custom-property declarations.

Each condition calls the same bounded value parser used by the corresponding
declaration. A property or value outside this documented list returns false,
even when another part of Lea happens to understand it. This conservative rule
keeps capability checks predictable as the engine grows.

## Limits

- condition text: 2,048 characters;
- logical terms at one level: 8;
- condition grouping depth: 4;
- combined conditional at-rule nesting: 4;
- diagnostic text retained for one condition: 96 compact characters;
- total stylesheet rules: the existing 1,024-rule ceiling.

An unmatched parenthesis or quote, an empty term, excess depth or terms, a
mixed ungrouped operator level, multiple declarations, `!important`, or an
unknown function keeps the block inactive. Lea does not implement
`font-tech()`, `font-format()`, general-enclosed future syntax, container style
queries, or a JavaScript `CSS.supports()` object.

## Diagnostics And Rebuilds

Active rules append a compact `@s(...)` label to `G.Cs` diagnostic lines. Nested
labels remain ordered and compose with media labels, for example:

```text
@s(display:grid)
@s(display:grid)&@s(selector(:has(*)))&@m>=800
@s(not(display:ruby))&@m<=500
```

Support capability is static for one Lea build. A nested media condition still
re-evaluates on `E.Vz(width,height)` or a `Lea.Win.V` resize, and that rebuild
retains existing field objects and their mutable values.

Coverage lives in `test-pages/supports-queries.html`, the focused
`Lea.Tests.exe . --supports` suite, the compatibility benchmark, and the
compiled Firespider host smoke.
