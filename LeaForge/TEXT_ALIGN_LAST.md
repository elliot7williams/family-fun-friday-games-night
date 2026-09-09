# Bounded Text Align Last

Milestone 97 added a compact inherited subset of CSS `text-align-last` without
adding another layout pass or browser runtime. Milestone 98 keeps it as the
final-line longhand of Lea's completed bounded alignment family.

## Compact state

- `S.Tl` is empty for `auto`, or stores `l`, `c`, `r`, `j`, `s`, or `e`.
- The state is inherited and copied by the existing style inheritance path.
- `start` and `end` remain logical so descendants resolve them using their own
  settled horizontal `direction`.
- `match-parent` resolves a parent's logical result through the parent's
  direction before copying it.

Supported authored values are `auto`, `start`, `end`, `left`, `right`,
`center`, `justify`, `match-parent`, and the existing CSS-wide values. Invalid
or out-of-scope values preserve the previous cascade result.

## Alias and DOM routes

```text
tl("center")          body final-line alignment
tl("article", "end") element final-line alignment
textalignlast(...)    readable command alias
style.textAlignLast   literal DOM assignment
```

An empty literal DOM assignment removes only that inline declaration and
restores the authored cascade.

## Layout behavior

Lea's existing inline word-line pass applies explicit `S.Tl` alignment to the
last visual line of a block and to the line immediately before each `<br>`.
`auto` follows the existing `text-align` value, except that a justified block's
last or forced line starts normally. Explicit `text-align-last:justify` fills
those lines when they contain separable words.

The feature changes line placement only. Outer boxes, text content, link and
pointer targets, find results, focus, and accessibility ownership continue to
use the same retained geometry. Pages using the default auto state stay on the
existing layout path.

## Boundary

This is a horizontal-writing LTR/RTL subset. Lea does not yet implement vertical
writing modes, full Unicode bidi shaping, fractional alignment, or arbitrary
JavaScript and CSSOM evaluation. `match-parent` is represented as the resolved
parent-relative physical result rather than a retained logical keyword.

## Verification

- Focused engine mode: `Lea.Tests.exe --text-align-last`
- Full default and core engine suites
- Firespider compiled-host smoke
- All 42 Firespider Boot cases
- Six real EXE startup scenarios
- 94/94 compatibility probes over 89 fixtures

The Milestone 98 canonical five-iteration report measured 497.401 ms median
clean startup, 4.353 ms median local load, 38.156 ms median layout and paint,
and 54.6 KiB of retained managed memory per page. Required runtime files total
579.5 KiB, with a 243.3 KiB GZip estimate. These are local diagnostic samples rather than
performance promises.
