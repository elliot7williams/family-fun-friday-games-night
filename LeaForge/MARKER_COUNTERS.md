# Bounded Marker Counters

Milestone 106 added the implicit HTML `list-item` value to terminal `::marker`
content. Milestone 107 extends the same bounded output grammar to named CSS
counters while keeping implicit list numbering on its existing compact `N.Mk`
path. Named counter properties and scope are documented in `CSS_COUNTERS.md`.

## Output Grammar

Marker `content` accepts these counter items:

```css
li::marker { content: "(" counter(list-item, lower-roman) ") "; }
li::marker { content: counters(list-item, ".") ". "; }
li::marker { content: counter(chapter, upper-roman) ". "; }
li::marker { content: counters(section, ".") " "; }
```

The name is a valid case-sensitive counter identifier of at most 32 ASCII
characters. `list-item` resolves from HTML `ol`/`ul` numbering; any other valid
name resolves from the nearest active named-counter instance. An absent named
counter emits `0`. An optional style defaults to `decimal` and accepts Lea's ten
existing bounded representations:

- `decimal` and `decimal-leading-zero`
- `lower-alpha`/`lower-latin` and `upper-alpha`/`upper-latin`
- `lower-roman` and `upper-roman`
- `disc`, `circle`, `square`, and `none`

One `counters()` item joins at most eight outermost-to-innermost values. Its
decoded separator stops at 32 UTF-16 units. The complete declaration retains
the shared limit of eight generated-content items and 128 UTF-16 output units.
Malformed functions, names, styles, separators, controls, nesting, or output
invalidate only that declaration, preserving the preceding cascade value.

## Numbering And Commands

Implicit `list-item` output preserves Lea's bounded HTML `start`, per-item
`value`, `reversed`, saturation, alpha rollover, Roman fallback, and nested-list
behavior. Named output uses the reset/increment/set tree-order pass described in
`CSS_COUNTERS.md`. `counter()` adds no punctuation; authored strings provide it.

Existing marker commands still define content:

```text
ms("content:counter(chapter)")
ms("item", "content:counters(section,'.',lower-roman)")
markerstyle("item", "content:counter(list-item,upper-alpha)")
```

Named properties use `cr`, `ci`, and `cs`, with readable and literal DOM forms.
The same strict validators run before a direct style is replaced.

## Layout And Verification

Counter text retains content-over-image priority, inside/outside geometry,
marker typography, LTR/RTL placement, table ownership, and exact paint.
`marker-counters.html` and `css-counters.html`, their focused tests, the full
default suite, all 103 benchmark probes over 98 fixtures, and the compiled
Firespider host cover implicit and named paths. All 18 startup regressions, all
42 Boot cases, and all six actual EXE startup scenarios pass.

## Boundary

Lea does not implement custom counter styles or `symbols()`, `reversed()` named
counters, pseudo-element counter properties, arbitrary `display:list-item`
implicit counters, typed attributes, unquoted or functional attribute
fallbacks, quote/image/alternative generated content, separate fragment boxes,
or a general generated-content/CSSOM evaluator.

Reference: [CSS Lists and Counters Level 3](https://drafts.csswg.org/css-lists-3/#counter-functions).
