# Bounded Text Alignment

Milestone 98 completes Lea's compact inherited horizontal text-alignment
family without adding a browser runtime, style record, or layout pass.

## Compact state

- `S.Ta` stores all-line alignment and `S.Tl` stores final-line alignment.
- Physical values use `l`, `c`, `r`, or `j`; logical start/end use `s` or `e`.
- Empty `Ta` means logical start. Empty `Tl` means CSS `auto`.
- Both fields inherit through Lea's existing style copy path.

Retaining logical `s/e` lets inherited start/end values resolve against each
descendant's direction. `match-parent` is different: it resolves the parent's
logical edge using the parent direction, then stores the resulting physical
edge. At the root, match-parent falls back to logical start.

## Shorthand and longhands

`text-align` is the bounded shorthand for `text-align-all` and
`text-align-last`:

- `left`, `right`, `center`, `justify`, `start`, and `end` set `Ta` and reset
  `Tl` to auto.
- `justify-all` sets both fields to justify.
- `match-parent` resolves both fields from the parent.
- `text-align-all` updates only `Ta` and preserves `Tl`.
- `text-align-last` updates only `Tl`.

The current CSS-wide values, variables, important tiers, inline overrides,
hover, and pixel-width media rebuilds use the normal compiled cascade. Invalid
or out-of-scope values preserve the previous declaration result.

## Alias and DOM routes

```text
ta("center")          body text alignment shorthand
ta("article", "end") element text alignment shorthand
taa("article", "left") element all-line alignment
tl("article", "justify") element final-line alignment
textalign(...)        readable shorthand command
textalignall(...)     readable all-line command
textalignlast(...)    readable final-line command
style.textAlign       literal DOM assignment
style.textAlignAll    literal DOM assignment
style.textAlignLast   literal DOM assignment
```

An empty literal assignment removes only that inline declaration and restores
the authored cascade.

## Layout behavior

The existing inline word-line pass resolves logical state through `S.Dr` and
positions each line. Ordinary justify still leaves final and forced lines at
logical start; `justify-all` or explicit final-line justify fills those lines
when separable words are available. `text-align-all` can change ordinary lines
without clearing a separately authored final-line value.

Only line placement changes. Outer boxes, authored text, link and pointer
targets, find results, focus, and accessibility ownership reuse the retained
geometry. Default pages retain the established fast path.

## Boundary

This is a horizontal-writing LTR/RTL subset. Vertical writing modes, full
Unicode bidi shaping, fractional alignment, general JavaScript/CSSOM
evaluation, and the rest of the modern CSS Text surface remain outside this
milestone.

## Verification

- Focused engine modes: `Lea.Tests.exe --text-align` and
  `Lea.Tests.exe --text-align-last`
- Full default and core engine suites
- Firespider compiled-host smoke
- All 42 Firespider Boot cases
- Six real EXE startup scenarios
- 94/94 compatibility probes over 89 fixtures

The canonical five-iteration report measured 497.401 ms median clean startup,
4.353 ms median local load, 38.156 ms median layout and paint, and 54.6 KiB of
retained managed memory per page. Required runtime files total 579.5 KiB, with
a 243.3 KiB GZip estimate. These are local diagnostic samples rather than
performance promises.
