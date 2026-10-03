# Viewport and Container Query Lengths

Milestone 140 extends size-container query thresholds with bounded classic
viewport and container-relative lengths. It uses the existing compiled `Kx`
tree and measured `E.Cz` content boxes.

## Supported Units

```css
@container card (width > 35vw) { }
@container card (height >= 40vh) { }
@container card (50vmin < width <= 40vmax) { }
@container card (width > 60cqw) { }
@container card (55cqw < width <= 55vmin) { }
```

`vw`, `vh`, `vmin`, and `vmax` use the current layout viewport. `cqw`, `cqh`,
`cqi`, `cqb`, `cqmin`, and `cqmax` use the selected query container's computed
value basis: the nearest eligible ancestor for each horizontal axis. If an
axis has no eligible ancestor, Lea uses the viewport for that axis. `cqmin`
and `cqmax` compare those independently selected inline and block bases.

Each term is stored as integer hundredths with one `Kx.U` character. A
viewport or container unit is a percentage of its basis, so comparison
cross-multiplies the measured pixel dimension by 10,000. Decimal and
floating-point work happens only while compiling CSS. Matching uses bounded
integer arithmetic and preserves fractional thresholds without rounding to
whole pixels.

The new units work in legacy min/max, exact colon, direct, value-first, and
chained comparisons. Either side of a chain can use a different supported
unit. A viewport change reevaluates active container conditions even when
the set of matching media rules is unchanged. Measurements, field objects,
values, and host ownership remain retained.

## Limits

- 0 through 100 of each supported relative unit
- up to two fractional digits
- 8 combined size/style terms and 4 conditional/container nesting levels
- 2,048 UTF-16 units of condition text
- 256 measured containers and 3 WinForms feedback passes
- horizontal writing mode only; `cqi` maps to width and `cqb` to height

Negative values, exponent notation, `calc()` in query thresholds, `sv*`,
`lv*`, and `dv*` viewport families, font-metric units such as `ch`, and values
above these limits remain unsupported. Lea's fixed desktop viewport is the
fallback where the CSS specification calls for the small viewport. Full
intrinsic containment and vertical writing remain future work.

The unit bases follow [CSS Values and Units Level 4](https://www.w3.org/TR/css-values-4/#viewport-relative-lengths)
and [CSS Containment Level 3](https://www.w3.org/TR/css-contain-3/#container-lengths).

## Verification

`container-query-viewport.html` and `Lea.Tests --container-query-viewport`
cover all ten spellings, independent viewport and ancestor changes, viewport
fallback, mixed and fractional boundaries, malformed values, separate engine
instances, retained fields, and exact WinForms geometry. The compatibility
probe and compiled Firespider smoke repeat the live browser paths.
