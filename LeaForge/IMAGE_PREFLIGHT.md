# Native Image Preflight

Milestone 69 / runtime 169 introduced bounded picture structure validation and
process-wide image reservations. Milestone 70 / runtime 170 moves accepted
native decoding into `Lea.Image.exe`; milestone 71 / runtime 171 adds a hard
Job Object process-memory ceiling. See `IMAGE_ISOLATION.md` for the process,
Job, and pipe contract. Milestone 72 / runtime 172 adds a privilege-stripped
low-integrity token, suspended Job assignment, an inherited-handle allowlist,
process mitigations, and Job peak-memory reporting before image admission.

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
over-frame, and over-work inputs become placeholders before helper admission.

The existing 4 MiB downloaded-body ceiling remains the first gate. Preflight
does not replace final codec validation: `Image.FromStream` runs with validation
inside `Lea.Image.exe`. The host validates the returned raw dimensions and byte
count against the preflight, per-view budget, and shared reservation before
retaining a bitmap.

## Decode And Memory Ownership

Preflight and raw-pixel copying run on the default worker scheduler. Native
codec parsing runs in the shared helper process. `GoA` keeps its existing owned
completion contract and awaits the picture pass. Synchronous `Go` presents the
document immediately; a paint starts missing pictures asynchronously instead of
blocking that paint in GDI+.

Before native decoding, Lea atomically reserves the preflight estimate against
`Im`. After the temporary source is disposed, the reservation shrinks to the
retained `width * height * 4` estimate. Page replacement and view disposal remove
that amount from `It`. Cross-view admission is serialized by the reservation
lock, so concurrent tabs cannot all pass against stale totals.

A deadline, Stop, replacement, or disposal cancels the exact active decode and
terminates its helper process when necessary. Its reservation remains charged
until the pipe task ends; any late bitmap is disposed and the reservation is
then released. This prevents stale work from publishing or making the budget
available twice. The next admitted decode starts a clean helper automatically.

## Verification

`Lea.Tests <root> --pics` checks PNG, JPEG, GIF, BMP, and ICO preflight/native
decode in another PID, hostile dimensions, unknown bytes, bounded JPEG header
scanning, missing-helper behavior, forced helper termination and restart, all
compact aliases, per-view and cross-view budgets, release and Reload recovery,
plus the earlier I/O/deadline/origin suite.

`Test-FirespiderBoot.ps1 -Case decode` sends one valid and one over-axis PNG
through a real Lea tab. It requires a distinct helper PID for the valid image,
zero native admission for the hostile image, retained form data, host request
cleanup, helper exit with the host, no UI exception, and no MSHTML.
`image-preflight.html` and the `Native image preflight` benchmark probe cover an
accepted isolated decode, a pre-native rejection, paint, form retention, and
disposal.

The runtime 172 release run passes the focused picture suite, full core suite,
isolated editor suite, compiled host suite, 69 benchmark probes over 66 fixtures,
all 42 Boot cases in one combined run, and all six actual executable startup
scenarios. Required runtime files total 536.0 KiB and have a 228.8 KiB GZip
estimate. The report records 521.959 ms clean-process, 3.323 ms local-load, and
33.453 ms layout/paint medians. These unpaired local samples are not speedup
claims.

## Remaining Limits

`It/Im` account only Lea's estimated native decode work and retained bitmap
copies. They are not total-process, helper private-byte, GDI heap,
source-byte-array, or GPU-memory measurements. `Ih` separately enforces the
helper's Job memory cap and `Ipm` samples private bytes after a successful
response. `Ipk` reports the Job's peak process bytes. The helper now has a
privilege-stripped low-integrity token and required process mitigations, but it
does not yet have an AppContainer, network-denial capability set, custom object
ACL, CPU-rate limit, or combined host-plus-helper peak measurement.

Images still do not share the document cookie jar, validator cache, or request
diagnostics. Lea does not add new codecs, CORS, multi-frame playback, or full
modern-image compatibility in this milestone.
