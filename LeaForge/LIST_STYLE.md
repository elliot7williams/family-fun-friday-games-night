# Bounded List Marker Styles

Milestone 101 expands Lea's existing list marker atom without adding another
parser, counter engine, layout pass, or Firespider control. The inherited
`S.Lst` field stores one defining marker code:

| Code | CSS value | Marker |
| --- | --- | --- |
| empty | HTML/default behavior | decimal for `ol`, disc for `ul`/`menu` |
| `n` | `none` | no marker or marker spacer |
| `d` | `disc` | U+2022 bullet |
| `c` | `circle` | U+25E6 white bullet |
| `s` | `square` | U+25AA small square |
| `1` | `decimal` | invariant integer plus `.` |
| `0` | `decimal-leading-zero` | at least two digits plus `.` |
| `a` / `A` | lower/upper alpha or latin | base-26 letters plus `.` |
| `r` / `R` | lower/upper Roman | Roman text plus `.` |

CSS `list-style-type` and Lea's bounded type/image/position `list-style` shorthand
use the ordinary compiled cascade. Variables, important tiers, inline styles,
explicit inheritance, CSS-wide values, hover, media replacement, and invalid
declaration preservation therefore need no list-specific cascade. The shorthand
accepts one supported type, one image, and one `inside`/`outside` position in any
order; omitted components reset and invalid combinations remain atomic. See
`LIST_POSITION.md` for position geometry and `LIST_IMAGE.md` for image loading,
fallback, limits, and `none` ambiguity.

HTML `ol type="1|a|A|i|I"`, `ul/menu type="disc|circle|square"`, and matching
per-`li` `type` attributes establish presentation hints before authored CSS.
Normal CSS can override them. `<ol reversed>` starts at its direct `li` count
unless `start` is present. `start` and per-item `value` accept invariant
integers from -999999 through 999999, and forward or reverse stepping saturates
at those bounds. Nested lists count only their own direct items.

Alpha output rolls over from `z` to `aa`. Roman output is used only for 1
through 3999; zero, negatives, and larger values fall back to decimal text.
Leading-zero output keeps a negative sign outside the padded digits. These
rules keep formatting deterministic and bound every conversion to a few
characters for Lea's accepted number range.

## Commands

The defining command is `lst`:

```text
lst("upper-roman")                body list style
lst("items", "lower-alpha")     element list style
liststyletype("items", "square") readable form
lp("items", "inside")           marker position
li("items", "url('marker.png')") marker image
ls("items", "upper-roman url('marker.png') inside") shorthand
```

Literal `style.listStyleType`, `style.listStyleImage`,
`style.listStylePosition`, and `style.listStyle` assignments on the body or one
literal ID translate to the same validators. An empty literal value removes the
inline declaration and restores authored CSS. Unsupported values are rejected
without rebuilding the page or disturbing retained field identity.

## Rendering

`B.Mk` remains the paint-ready marker, so list text, tables, wrapping, links,
find, focus, and accessibility reuse the existing inline records. `none`
leaves `B.Mk` empty and adds no synthetic spacer. Unstyled unordered lists now
use the standard disc instead of Lea's earlier hyphen placeholder. Marker
placement follows physical inline-start, including the existing horizontal RTL
mapping documented in `DIRECTION.md`.

`test-pages/list-style.html`, focused `Lea.Tests --list-style`, the full core
suite, the benchmark probe, and the compiled Firespider host cover HTML hints,
CSS precedence, every compact code, alpha rollover, Roman fallback, leading
zero and negative output, reversed/start/value sequences, nested lists,
commands, RTL geometry, tables, hover, media replacement, invalid values, and
retained controls.

## Boundary

Terminal `::marker` styling and bounded string/attribute generated content were
added in Milestones 104 and 105; see `MARKER.md` and `MARKER_CONTENT.md`. Lea
does not yet implement custom counter styles, arbitrary `counter()`/`counters()`,
general `::before`/`::after` generated boxes, vertical writing, or full Unicode
bidi shaping. Bounded inside/outside positioning was added in Milestone 102 and
bounded marker images in Milestone 103; see `LIST_POSITION.md` and
`LIST_IMAGE.md`.

References: [CSS Lists and Counters](https://drafts.csswg.org/css-lists/) and
[CSS Counter Styles](https://drafts.csswg.org/css-counter-styles-3/).
