# Bounded Picture Loading

Milestone 66 / runtime 166 replaces whole-response image buffering with bounded
streaming reads, a picture-pass deadline, and retained bitmap accounting. It
uses Lea's existing .NET Framework networking and GDI+ environment, not another
browser or a new package dependency.

Milestone 69 / runtime 169 adds bounded format/dimension preflight, worker-thread
native decoding, and process-wide Lea image admission. See `IMAGE_PREFLIGHT.md`.

Milestone 70 / runtime 170 moves native codec parsing into the required,
killable `Lea.Image.exe` helper and returns bounded raw pixels. See
`IMAGE_ISOLATION.md`.

## Compact API

```csharp
int images = view.Ic;
long bitmapBytes = view.Ib;
view.Tm = 15000;
await view.GoA(address);
```

`V.Ic` counts retained decoded bitmaps, excluding failed/over-budget entries.
`V.Ib` reports their width * height * 4 byte estimate. Both become zero when
the view is disposed. These are view counters; `G.Ic` remains the independent
CSS important-declaration counter. Call view APIs on the UI thread.

## Input And Time Bounds

- At most 24 distinct referenced picture URLs per pass, including CSS images.
- File and HTTP/HTTPS picture bodies: at most 4 MiB of bytes passed to decoding.
- A known oversized length is rejected before body reading. Unknown-length
  input stops after the one extra byte needed to establish overflow.
- Reads use a 32 KiB buffer and never append the overflow byte to the output.
- HTTP gzip/deflate content is decoded by the platform before the same body
  ceiling is applied. Unsupported encodings/formats remain placeholders.
- Existing bounded Base64 data-picture decoding remains available.
- Local-file pictures require a local-file document origin. Remote pages cannot
  read or reuse cached local pictures; a `base` URL cannot grant file access.
  UNC picture paths are rejected before I/O, including on local pages.
- One `Tm` budget is captured for the whole picture pass, not for every image.
  It covers asynchronous opening/reads; remaining unstarted sources become
  failed entries after exhaustion. `Tm` keeps its existing 100-60,000 ms range.

Async requests need an explicit deadline rather than relying on the synchronous
`Timeout` property alone. See Microsoft's
[WebRequest timeout contract](https://learn.microsoft.com/en-us/dotnet/api/system.net.webrequest.timeout?view=net-9.0).
Lea races resource tasks against one deadline, aborts the active request, and
disposes its stream. Late successful resources and HTTP error responses are
disposed, and late task failures are observed.

Local file opening and length checks run off the UI thread. Stop also releases
the caller's wait for an unfinished open, with eventual disposal of a late
file handle. This does not terminate an underlying platform operation or thread.

## Bitmap Retention

Before making the retained bitmap copy, Lea rejects dimensions above 8,192 on
either axis, more than 8,388,608 pixels, or a copy that would exceed the view's
32 MiB retained pixel budget. A 3,840 by 2,160 bitmap fits if the budget is free.

The helper keeps its source stream alive through `Image.FromStream`, then sends
validated 32-bit pixels to the host. The encoded source and native decoder stay
outside Firespider; only the bounded copied bitmap is retained in the host.

Only referenced cache entries survive a page transition or a subsequent picture
pass. Repeated references share one image. URL path/query/Base64 case is preserved
with ordinal keys. Failed and over-budget entries retain a null sentinel so a
repaint does not repeatedly fetch or evict current images. Document navigation,
history presentation, and Reload allow failed pictures to retry. Fragment-only
presentation and ordinary repaint do not clear those sentinels. Loaded shared
URLs retain the existing in-memory reuse behavior; HTTP image revalidation is
not added here.

Images that do not fit the current pass are not silently downscaled. They use
the existing placeholder/alt rendering. Images no longer referenced are disposed
and removed from the accounting. Source and field data are not changed.

## Ownership And Failures

Runtime 165's serialized picture passes, page/generation checks, `Cn` admission,
and load ownership remain in place. Stop, replacement, and disposal abort active
picture I/O and prevent stale results entering the cache. Picture failure or
timeout does not turn a successfully loaded document into a `Fe` page failure.
The view finishes `Done`, and Firespider releases its host request bookkeeping.

The native `pictures` Boot case holds an image response open after loading a
page, then checks completion, retained title/field, `Ic/Ib`, timer cleanup, and
absence of UI exceptions or MSHTML. The existing `load-life` case checks that
this does not regress page-command successors and queued native CSS.

## Verification

`Lea.Tests <root> --pics` covers held headers, stalled bodies, a shared three-image
deadline, known/unknown oversized responses, compressed expansion, valid gzip
and deflate images, truncated bodies, HTTP errors, ordinal keys, failed-image
Reload, retained cache exhaustion, no repaint/refetch loop, dimension rejection,
Stop/disposal, successor navigation, exact byte boundaries, and late resource
disposal, plus local-origin/cache checks and no-I/O UNC rejection. Core tests also
exercise local files, embedded images, CSS backgrounds,
image maps, object fitting, and native paint. The benchmark adds
`image-loading.html` and a retained-picture probe.

The first timeout regression failed on runtime 165. A test-fixture cleanup race
was removed by leaving the held response blocked until server disposal. An
expanded fixture initially had a zero-sized view; paint checks now use explicit
dimensions. Full-suite runs also hit the previous download-lifecycle test's
combined completion/observation assertion. Added status diagnostics showed a
local file had already completed synchronously, without an observable pending
step. Local synchronous completion is now accepted; gated HTTP separately
requires pending ownership. Real I/O uses a five-second wall-clock bound with
per-step ownership checks and drains pending work during failure cleanup.

The final Release build, including the local-origin guard, passes `--pics`,
`--core`, and isolated `--edit`, the full compiled Firespider host suite, and
all 66 compatibility probes across 63 fixtures. All 39 Boot cases passed in a
single combined run. Six actual EXE launch scenarios passed with responsive
windows, deferred-tab resume, no observed startup dialogs, and no MSHTML.
The inherited timer and saved-style cast guards remain covered. Older
intermittent fragment, stackless recovery-cast, and editor/focus observations
are not considered resolved by these passing runs.

Engine DLLs total 503,296 bytes (491.5 KiB); the GZip estimate is 208.3 KiB.
The final diagnostic report records a 724.836 ms clean-process median,
3.474 ms local-load median, and 33.835 ms layout/paint median. These are unpaired
local samples, not evidence of a speedup or regression. The full test runtime
also includes the host, fixtures, and test tools; it is larger than the engine.
Existing optional Shockwave/ClickOnce build warnings remain. Runtime 166 is
packaged separately; the installed runtime 157 EXE and root DLLs are unchanged.

## Remaining Work

The 32 MiB figure remains the per-view retained limit. Runtime 170 preserves
runtime 169's preflight/shared admission and moves accepted codec work to a
killable shared helper; see `IMAGE_PREFLIGHT.md` and `IMAGE_ISOLATION.md`.
The estimate is still not a complete host-plus-helper private-byte guarantee.
Restricted-token/Job Object sandboxing and measured whole-process budgets remain.

Milestone 67 adds separate bounded HTTP/file document and linked-CSS input;
see `INPUT_LIMITS.md`. Images do not yet share the document cookie jar,
HTTP validator cache, or request diagnostics. This milestone does not add
sandboxing, CORS enforcement, new codecs, or complete modern-web compatibility.
Compression and short aliases alone do not establish faster execution.
