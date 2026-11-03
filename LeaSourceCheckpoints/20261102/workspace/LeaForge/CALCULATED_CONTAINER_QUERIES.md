# Calculated Container Query Lengths

Milestone 141 accepts bounded two-term `calc()` lengths in size-container
conditions. It extends the existing `Kx` tree rather than introducing a second
query evaluator.

```css
@container card (width > calc(50cqw + 60px)) { }
@container card (calc(10vw + 239.99px) < width <= calc(25vh + 170px)) { }
@container card (min-width: calc(1rem + 300px)) { }
```

Each expression contains exactly two nonnegative length terms joined by `+`
or `-`, with whitespace on both sides of the operator. Either term can use a
supported `px`, `em`, `rem`, classic viewport, or container-relative unit.
Both calculation terms require a unit; unitless zero remains accepted as an
ordinary threshold outside `calc()`. The existing per-unit ceilings and
two-decimal precision apply to both terms. Calculated results retain their
sign, including comparisons with a zero-width container. Nested arithmetic, multiplication, division,
percentages, dynamic viewport families, and more than two terms remain outside
this bounded form.

Compilation stores both fixed-hundredth terms and their one-character unit
codes. Matching resolves current container, root-font, ancestor, and viewport
bases into a shared integer scale. No expression parsing or floating-point
arithmetic runs during layout. Legacy, direct, value-first, exact, and chained
comparisons share the same evaluator and retain existing axis eligibility,
condition limits, and WinForms feedback behavior.

`container-query-calc.html` and `Lea.Tests --container-query-calc` cover mixed
units, fractional boundaries, malformed expressions, diagnostics, live
viewport/font/ancestor changes, retained fields, and native geometry. The
compatibility benchmark and compiled Firespider host smoke cover the same
integration path. The staged browser passed all 42 boot scenarios and six
direct EXE startup scenarios; the installed runtime was not replaced.

The arithmetic syntax and relative-unit bases follow
[CSS Values and Units Level 4](https://www.w3.org/TR/css-values-4/#calc-syntax)
and [CSS Containment Level 3](https://www.w3.org/TR/css-contain-3/#size-features).
