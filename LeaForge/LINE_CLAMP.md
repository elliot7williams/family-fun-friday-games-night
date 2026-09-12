# Bounded Line Clamping

Lea supports a deliberately small multi-line clamping subset through compact
non-inherited `S.Lc` state:

- `0` means no clamp and is the default.
- `1` through `32` retain that many visual lines.
- CSS accepts `line-clamp` and the compatibility spelling
  `-webkit-line-clamp`.
- Values are `none`, a positive bounded integer, or a CSS-wide keyword.

Invalid, negative, zero, fractional, string-marker, and over-limit values
preserve the preceding declaration. `inherit` copies the parent value;
`initial`, `unset`, `revert`, and `revert-layer` restore zero because line clamp
is non-inherited. Custom properties, normal/important tiers, inline styles,
dynamic state, and media queries reuse Lea's ordinary compiled cascade.

## Layout

The existing retained inline pass first performs normal wrapping, explicit
breaks, indentation, spacing, alignment, and optional bounded balancing. When
the result exceeds `S.Lc`, Lea keeps only the requested lines, shortens the last
visible atom at a .NET text-element boundary when necessary, and adds one
U+2026 marker at physical inline end. RTL places the marker at physical left.

Hidden lines and any elided suffix atoms are removed from retained boxes. They
therefore do not paint, contribute flow height, create pointer targets, or
publish visible accessibility rectangles. The authored DOM, `P.X`, form state,
and UI Automation logical document text remain complete and contain no
synthetic marker. Following content reflows to the shorter block height.

Blocks, mixed inline runs, forced breaks, balanced text, and simple table cells
share this path. A paragraph with no excess natural line receives no marker;
`white-space:nowrap` therefore remains one unclamped line.

## Aliases

The defining short route is:

```text
lc(value)
lc(id,value)
```

Readable `lineclamp` accepts the same forms. Literal migration statements map
to the same validator and retained rebuild:

```javascript
document.body.style.lineClamp = '2';
document.getElementById('summary').style.lineClamp = '3';
document.getElementById('summary').style.webkitLineClamp = '2';
```

An empty literal assignment removes the inline declaration and restores the
authored cascade.

## Boundary

Lea does not yet implement `max-lines`, `block-ellipsis`, `continue`, custom
ellipsis strings, nested descendant-block fragmentation, vertical writing,
full Unicode bidi shaping, or the historical WebKit box/display prerequisites.
The practical compatibility spelling intentionally maps directly to Lea's
bounded retained inline layout.

The model follows the bounded integer behavior of the
[CSS Overflow Level 4 line-clamp definition](https://drafts.csswg.org/css-overflow-4/#propdef-line-clamp)
without claiming the full shorthand grammar.

## Verification

- Fixture: `LeaForge/test-pages/line-clamp.html`
- Focused engine mode: `Lea.Tests.exe <repository-root> --line-clamp`
- Benchmark probe: `Bounded line clamping`
- Compiled Firespider host: standard/WebKit state, `lc`, literal DOM, responsive
  one-line geometry, wide restoration, and retained fields
