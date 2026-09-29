# Bounded Match Media

Lea exposes its compiled media-query subset through the compact `E.Mq` and
`V.Mq` aliases and through literal `matchMedia(...).matches` page-command
guards. Both routes evaluate the same live viewport, density, preference, and
scripting state used by `@media`; Lea does not carry a second query parser.

```csharp
engine.Mq("(min-width: 800px)");
view.Mq("(prefers-color-scheme: dark)");
```

```javascript
if (matchMedia("(width >= 800px)").matches) { ac("panel", "wide"); }
if (window.matchMedia("(orientation: portrait)").matches) { sh("portrait"); }
if (!matchMedia("(prefers-reduced-motion: reduce)").matches) { sh("motion"); }
```

## Query Surface

`Mq` supports the media features already compiled by Lea:

- `screen`, `all`, and fixed non-matching `print` media types;
- whole-query `not` and no-op `only` before a media type;
- pixel width and height, orientation, and reduced integer aspect ratios;
- `dpi`, `dppx`, and `dpcm` resolution;
- color scheme, reduced motion, reduced data, and contrast preferences;
- `scripting` state;
- legacy min/max forms and strict or inclusive feature/value ranges;
- `and` conjunctions and up to eight comma alternatives.

An empty string matches the ordinary all-media state. Null, malformed,
unknown, misplaced-modifier, or over-limit queries return false. Values are checked
against current engine state on every host call. A WinForms `V.Mq` call after a
resize or preference update therefore sees the new state without retaining a
separate query object.

## Script Boundary

The allow-listed page-command parser accepts only one quoted literal argument,
the exact global `matchMedia` or `window.matchMedia` name, and the exact
`.matches` property. A leading `!` and one `else` block are supported. Media
and `CSS.supports()` guards can be mixed to four total levels and share the
existing 32-command budget.

Computed arguments, extra arguments, wrong-case names, comparisons after
`.matches`, saved query objects, `onchange`, `addEventListener`, `addListener`,
and `removeListener` remain inert. Startup scripts run once; changing the
viewport does not rerun commands that were skipped earlier. Host-provided and
click-handler guards evaluate the page's current media state when invoked.

## Limits And Coverage

- query text: 2,048 characters;
- comma alternatives: 8;
- mixed conditional guard depth: 4;
- accepted commands per invocation: 32;
- viewport dimensions: 1-8,192 pixels under existing host clamps.

Coverage lives in `test-pages/match-media-script.html` and the focused
`Lea.Tests.exe . --match-media-script` suite. The same fixture is exercised by
the compatibility benchmark and the compiled Firespider host smoke.
Modifier grammar, fixed screen/print semantics, and nested intersections are
covered separately in `MEDIA_MODIFIERS.md`.

The API shape follows the W3C CSSOM View definition of `matchMedia()` and the
read-only `MediaQueryList.matches` state:
<https://www.w3.org/TR/cssom-view/#dom-window-matchmedia>.
