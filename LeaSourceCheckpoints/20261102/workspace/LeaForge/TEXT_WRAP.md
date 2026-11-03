# Bounded Text Wrapping

Milestone 99 adds a compact inherited subset of CSS Text Level 4 wrapping
without adding an external browser runtime or a general CSS evaluator.

## Compact state

- `S.Wm` stores wrapping mode: empty is normal wrap, `w` is explicit wrap, and
  `n` is nowrap.
- `S.Wt` stores wrapping style: empty is auto and `b` is balance.
- Both fields inherit through Lea's existing style copy path.
- Explicit `w` lets authored `text-wrap-mode:wrap` override the native nowrap
  default of a `pre` element.

## Shorthand and longhands

`text-wrap` accepts one mode, one style, or both in either order. An omitted
longhand resets to its initial value. `text-wrap-mode` updates only mode and
accepts `wrap` or `nowrap`; `text-wrap-style` updates only style and accepts
`auto` or `balance`.

The existing CSS-wide values, variables, important tiers, inline overrides,
hover, and pixel-width media rebuilds use the normal compiled cascade.
`white-space` updates wrap mode in declaration order without clearing wrap
style. Invalid values and the out-of-scope `stable`, `pretty`, and
`avoid-short-last-line` styles preserve the earlier cascade result.

## Alias and DOM routes

```text
tw("balance")              body wrapping shorthand
tw("article", "wrap balance") element wrapping shorthand
twm("article", "nowrap")  element wrapping mode
tws("article", "auto")    element wrapping style
textwrap(...)              readable shorthand command
textwrapmode(...)          readable mode command
textwrapstyle(...)         readable style command
style.textWrap             literal DOM assignment
style.textWrapMode         literal DOM assignment
style.textWrapStyle        literal DOM assignment
```

An empty literal assignment removes only that inline declaration and restores
the authored cascade.

## Balanced layout

Lea first creates its ordinary greedy lines. For two through eight lines and at
most 2,048 text characters, balance searches for the smallest virtual content
width that keeps the same line count. It accepts the result only when the
variance of unused line space is lower than the ordinary layout. Final
placement still uses the real content width.

The line count and outer block geometry therefore remain stable. Existing
LTR/RTL placement, percentage or pixel first-line indentation, forced breaks,
table cells, links, hits, find, focus, and accessibility reuse the selected
line boxes. Longer text and more than eight lines stay on the ordinary fast
path.

## Boundary

This is a horizontal-writing LTR/RTL subset. Lea does not implement stable,
pretty, or avoid-short-last-line wrapping, hyphenation, language-specific line
breaking, vertical writing modes, full Unicode bidi shaping, or arbitrary
JavaScript/CSSOM evaluation. Exact balance choices are deliberately bounded and
may differ from other engines while preserving Lea's tested invariants.

## Verification

- Focused engine mode: `Lea.Tests.exe --text-wrap`
- Full default and core engine suites
- Firespider compiled-host smoke and all 18 startup regressions
- All 42 Firespider Boot cases
- Six real EXE startup scenarios
- 95/95 compatibility probes over 90 fixtures

The canonical five-iteration report measured 526.125 ms median clean startup,
4.257 ms median local load, 38.151 ms median layout and paint, and 55.3 KiB of
retained managed memory per page. Required runtime files total 583.0 KiB, with
a 244.4 KiB GZip estimate. These are local diagnostic samples rather than
performance promises.
