# Bounded List Marker Images

Milestone 103 adds inherited CSS `list-style-image` without introducing another
resource loader, decoder, layout pass, or Firespider control. Compact `S.Li`
stores the resolved marker `Uri`; `null` is the initial `none` state.

## Cascade And Shorthand

`list-style-image` accepts one `url(...)`, `none`, and Lea's existing CSS-wide
values. Relative URLs resolve against the stylesheet or document origin. Data,
file, HTTP, and HTTPS URLs pass through the same origin and scheme validation as
bitmap backgrounds. Variables, important tiers, inline declarations, explicit
inheritance, hover, media replacement, empty authored restoration, and invalid
preservation use the ordinary compiled cascade.

The bounded `list-style` shorthand accepts up to one marker type, one image, and
one `inside`/`outside` position in any order. It is atomic, parenthesis-aware,
and limited to 4,096 characters. Omitted components reset to `disc`, `none`, and
`outside`. A lone `none` clears both type and image; with a URL it means a
`none` text fallback, while with an explicit type it means no image. Duplicate,
unknown, or overfilled combinations preserve the preceding declaration.

```css
li { list-style-image: url("marker.png"); }
li { list-style: square url("marker.png") inside; }
li { list-style: url("marker.png") none outside; }
```

## Commands

The defining image command is `li`:

```text
li("url('marker.png')")                       body marker image
li("items", "none")                         element marker image
liststyleimage("items", "url('marker.png')") readable form
ls("items", "circle url('marker.png') inside") shorthand
```

Literal `style.listStyleImage` and `style.listStyle` assignments on
`document.body` or one literal ID use the same validators. Empty literal values
remove the inline declaration and restore authored CSS. Every accepted command
uses Lea's retained rebuild, preserving fields, document identity, history, and
request state.

## Loading And Rendering

Marker images reuse Lea's deduplicated page picture cache and sandboxed decoder.
They share the 24-source per-page admission limit, 4 MiB image input ceiling,
32 MiB view budget, and PNG, JPEG, GIF, BMP, and ICO support documented in
`PICTURE_LIMITS.md`. No separate marker-image cache or decoder process exists.

A decoded image preserves its intrinsic aspect ratio and fits inside a bounded
64 by 64 pixel box. It takes precedence over `B.Mk`. A missing, blocked,
unsupported, or failed image falls back to the computed text marker; a
`list-style-type:none` fallback stays markerless. Outside images occupy the
logical-start gutter without consuming the principal line width. Inside images
are the first inline atom and affect wrapping and line height. LTR and horizontal
RTL use the same direction-aware geometry as text markers.

## Verification

`test-pages/list-image.html`, focused `Lea.Tests --list-image`, the full default
suite, the 99-probe benchmark, and the compiled Firespider host cover inherited
and reset values, relative/data URLs, variables, priority, invalid atomicity,
`none` ambiguity, compact/readable/literal commands, cache reuse, exact image
geometry and pixels, text fallback, tables, LTR/RTL, hover, responsive
replacement, and retained controls. All 42 Boot cases and all six real EXE
startup scenarios also pass from the packaged runtime.

## Boundary

Lea does not yet implement `::marker`, `image-set()`, gradients as marker
images, SVG image decoding, author-controlled marker image sizing, marker-side,
cross-item marker-column alignment, custom counter styles, generated content,
vertical writing, full Unicode bidi shaping, or arbitrary CSSOM.

Reference: [CSS Lists and Counters](https://drafts.csswg.org/css-lists/).
