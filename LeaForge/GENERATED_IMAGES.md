# Bounded Generated Images

Milestone 112 adds `url(...)` bitmap fragments to Lea's shared terminal
`::before`, `::after`, and `::marker` `content` grammar. The feature reuses the
existing sandboxed picture loader, cache, decoder helper, layout pass, and
paint path. It does not add Chromium, WebView, MSHTML, another decoder, or a
Firespider-side pseudo-element adapter.

## Grammar And Limits

A visual content list may mix images with Lea's existing strings, attributes,
counters, and generated quote keywords in source order:

```css
.download::before {
  content: "[" url("download.png") "] " / "Download: ";
}

li.ready::marker {
  content: url("ready.png") " " / "Ready: ";
}
```

The visual half still stops at eight items and 128 UTF-16 text units. Images
count toward the eight-item limit but not the text-unit limit. The optional
alternative half has its own eight-item and 128-unit bounds and accepts text,
attributes, and counters only. An image after `/`, a ninth visual item, an
unsafe scheme, malformed syntax, or oversized output invalidates the whole
declaration and preserves the earlier cascade value.

Quoted or unquoted `url(...)` values may resolve to data, file, HTTP, or HTTPS
resources. Relative values use the declaration or custom-property origin.
Remote documents cannot read local files, UNC paths are rejected, and the
normal resource limits still apply. See `PICTURE_LIMITS.md`,
`IMAGE_PREFLIGHT.md`, and `IMAGE_ISOLATION.md`.

## Compact Model

Text-only generated content keeps the existing allocation-light `S.Mc` path.
Only content containing an image receives read-only `S.Mf` fragments:

- `Cf.K` is `t` for text or `i` for an image.
- `Cf.X` contains a text fragment.
- `Cf.U` contains a resolved picture source.
- `Ir.P` carries a generated inline picture source.
- `Ir.U` remains the independent link target inherited from the owner.

This separation lets an image-only generated run inside an anchor paint and
hit-test as part of that anchor without treating the image URL as navigation.
`S.Ma` remains the optional accessibility alternative for the whole pseudo.

## Layout And Loading

Decoded generated pictures preserve intrinsic aspect ratio and fit inside a
64 by 64 pixel box. Before and after pictures are indivisible inline atoms and
participate in wrapping, line height, links, tables, LTR/RTL layout, and
responsive rebuilds. Mixed marker fragments use the existing logical-start
outside gutter or inside first-line path.

Generated pictures share the view-wide limit of 24 distinct picture sources,
4 MiB encoded input ceiling, 32 MiB retained bitmap budget, cache, origin
checks, and isolated `Lea.Image.exe` decoder. Repeated URLs are deduplicated.
An admitted source that is missing or fails decoding retains bounded placeholder
geometry and paints no pixels; it does not turn a loaded document into a page
failure.

## Commands And Accessibility

The existing aliases accept the same grammar; no new command is required:

```text
be("status", "content:'<' url(icon.png) '>' / 'Status icon '")
af("status", "content:url(done.png) / ' complete'")
ms("item", "content:url(bullet.png) ' ' / 'Item: '")
```

Accessible output uses one `S.Ma` alternative per pseudo, even when the visual
side contains several text and image fragments. An explicit empty alternative
makes the visual content decorative. Without an alternative, text fragments
remain available to name-from-content behavior while image fragments add no
invented speech. Explicit `aria-labelledby` and `aria-label` retain priority.

## Verification And Boundary

`test-pages/generated-images.html`, focused
`Lea.Tests.exe <root> --generated-images`, the full engine suite, benchmark,
and compiled Firespider smoke cover mixed order, relative/data URLs, variables,
alternatives, links, tables, markers, exact item limits, invalid preservation,
commands, hover/media rebuilds, retained fields, geometry, pixels, and shared
resource admission.

This slice does not implement gradients, `image-set()`, `cross-fade()`,
`element()`, generated SVG decoding, author-controlled generated-image sizing,
pseudo box-model properties, vertical writing, or a general CSSOM evaluator.
