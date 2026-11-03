# Bounded Attribute Fallbacks

Milestone 109 adds one small shared extension to Lea's generated-content
grammar: a missing attribute may provide a quoted text fallback.

```css
li::marker { content: attr(data-state, "Pending "); }
a::before { content: attr(data-prefix raw-string, "Open "); }
```

The first argument remains a bounded unnamespaced attribute name, optionally
followed by `raw-string`. The second argument is optional and, when present,
must be exactly one quoted CSS string. A top-level comma separates the two
arguments. Quoted commas remain part of the fallback text.

A present attribute always wins, even when its value is empty. Only a missing
attribute uses the fallback. Without a fallback, a missing attribute still
produces empty text. Attribute values and fallbacks are decoded as text and are
never parsed as markup, URLs, commands, or CSS.

The existing bounds remain unchanged: at most eight content items, 64
characters in an attribute name, and 128 UTF-16 units in the final generated
text. The fallback itself stops at 128 units and rejects unsafe controls.
Custom properties resolve before validation, so a variable may supply the
quoted fallback token.

Typed attributes, unquoted fallbacks, nested fallback functions, more than two
arguments, malformed quotes, unsafe controls, and oversized output are
rejected atomically. The prior valid cascade value remains in effect. The same
parser serves `::marker`, `::before`, `::after`, `ms`/`markerstyle`, and
`be`/`af`/`beforestyle`/`afterstyle`; Firespider needs no adapter-side parser.

Focused coverage lives in `test-pages/marker-content.html` and
`test-pages/generated-content.html`. It includes missing, present, and empty
attributes; raw-string mode; composition and escapes; variable resolution;
exact output bounds; invalid preservation; commands; tables; links;
accessibility; hover; and responsive media rebuilds.
