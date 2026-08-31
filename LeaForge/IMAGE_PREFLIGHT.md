# Native Image Preflight

Milestone 69 / runtime 169 validates supported picture structure before bytes
reach GDI+, runs native decoding away from the UI thread, and reserves estimated
image memory across all Lea views.

## Compact API

```csharp
int pixelsPerImage = view.Ip;
long processLimit = view.Im;
long processImageBytes = view.It;
int nativeAttempts = view.In;
int failedImages = view.If;
```

`Ip` is the fixed 8,388,608-pixel ceiling for one decoded picture. `Im` is the
shared Lea image-memory limit in bytes: it defaults to 128 MiB and a host may set
it from 32 to 512 MiB. Changing `Im` through one view changes the process-wide
limit for every Lea view.

`It` reports process-wide bytes currently reserved for active native decoding
or retained bitmaps. `In` counts native decoder attempts for the view lifetime.
`If` counts null failure sentinels retained for the current page, so paints do
not repeatedly fetch or decode the same failed resource. Existing `Ic/Ib` still
report successfully retained bitmaps and their 32-bit pixel-byte estimate.

## Preflight

The internal compact `Pi` reader identifies bytes by structure rather than file
extension or HTTP media type. It accepts the formats already supported by Lea's
Windows renderer:

- PNG: bounded chunks, required `IHDR`, pixel data and end chunks, valid core
  color/depth settings, and bounded animation frame declarations.
- JPEG: bounded marker segments and a valid frame header within the first
  256 KiB.
- GIF: bounded logical canvas, color tables, extension/image blocks, at most 64
  frames, and bounded total frame pixels.
- BMP: bounded file/DIB headers, dimensions, planes, depth, and supported Windows
  compression values.
- ICO: at most 64 bounded directory entries with PNG or DIB frame dimensions.

Every accepted picture is at most 8,192 pixels on either axis and 8,388,608
pixels. Animated/multi-frame work is bounded to the same total pixel count. The
estimated overlapping decoder work plus retained copy is capped at 64 MiB for
one resource. Unknown, truncated, structurally invalid, over-axis, over-pixel,
over-frame, and over-work inputs become placeholders before `Image.FromStream`.

The existing 4 MiB downloaded-body ceiling remains the first gate. Preflight
does not replace the final GDI+ validation: `Image.FromStream` uses validation,
and Lea checks the decoded dimensions against the preflight and per-view budget
before retaining the copy.

## Decode And Memory Ownership

Preflight and `Image.FromStream`/bitmap copying run on the default worker
scheduler. `GoA` keeps its existing owned completion contract and awaits the
picture pass. Synchronous `Go` presents the document immediately; a paint starts
missing pictures asynchronously instead of blocking that paint in GDI+.

Before native decoding, Lea atomically reserves the preflight estimate against
`Im`. After the temporary source is disposed, the reservation shrinks to the
retained `width * height * 4` estimate. Page replacement and view disposal remove
that amount from `It`. Cross-view admission is serialized by the reservation
lock, so concurrent tabs cannot all pass against stale totals.

A deadline, Stop, replacement, or disposal releases the caller without waiting
for a late native task. Its reservation remains charged until the task actually
ends; any late bitmap is disposed and the reservation is then released. This
prevents stale work from publishing or making the budget available twice.

## Verification

`Lea.Tests <root> --pics` checks PNG, JPEG, GIF, BMP, and ICO preflight/native
decode, worker-thread execution, hostile dimensions, unknown bytes, bounded JPEG
header scanning, all compact aliases, per-view and cross-view budgets, release
and Reload recovery, plus the earlier I/O/deadline/origin suite.

`Test-FirespiderBoot.ps1 -Case decode` sends an over-axis PNG through a real Lea
tab and requires `In=0`, `If=1`, `It=0`, retained form data, host request cleanup,
no UI exception, and no MSHTML. `image-preflight.html` and the `Native image
preflight` benchmark probe cover one accepted worker decode, one pre-native
rejection, paint, form retention, and disposal.

The runtime 169 release run passes the focused picture suite, full core suite,
isolated editor suite, compiled host suite, 69 benchmark probes over 66 fixtures,
all 42 Boot cases in one combined run, and all six actual executable startup
scenarios. Engine DLLs total 509.5 KiB and have a 216.9 KiB GZip estimate. The
report records 489.765 ms clean-process, 3.285 ms local-load, and 31.072 ms
layout/paint medians. These unpaired local samples are not speedup claims.

## Remaining Limits

`It/Im` account only Lea's estimated native decode work and retained bitmap
copies. They are not total-process, private-byte, GDI heap, source-byte-array, or
GPU-memory measurements. GDI+ still runs inside the Firespider process on a
worker thread; the deadline cannot terminate native code already executing.
Moving codecs into a killable helper process remains the stronger isolation
target.

Images still do not share the document cookie jar, validator cache, or request
diagnostics. Lea does not add new codecs, CORS, sandboxing, or full modern-image
compatibility in this milestone.
