# Bounded Document Input

Milestone 67 / runtime 167 adds bounded document and linked-stylesheet readers
to Lea's existing .NET Framework engine. It does not embed another browser or
add a package dependency.

## Compact API

```csharp
int documentBytes = view.Hm;
int stylesheetBytes = view.Csm;
view.Tm = 15000;
await view.GoA(address);
```

`E/V.Hm` is the read-only 4 MiB document-body ceiling. `E/V.Csm` is the read-only
256 KiB linked-stylesheet ceiling. The existing `V.Sm` remains form submission.
The limits also cover POST responses. The separate 10 MiB upload and 64 MiB
download limits are unchanged.

## Reading And Expansion

`Ri` wraps input streams with a byte counter and cancellation checks. HTTP GET
and POST responses pass through a wire-body counter, optional platform gzip or
deflate decoding, and a decoded-body counter before bytes enter the retained
buffer. Reads use 32 KiB chunks. Known oversized lengths are rejected before
body reading; unknown-length reads consume only the extra byte needed to prove
overflow. Exact-limit bodies remain valid.

Unsupported content encodings fail with the existing `decode` error rather than
being treated as HTML. Encoding stacks and Brotli are not implemented here.
The byte counters concern bytes consumed by Lea after HTTP transfer framing,
not TLS records, chunk headers, socket buffers, or all traffic on the wire.
Trailing data that a platform decoder does not consume is not interpreted.

Local files use the same bounded body reader and existing BOM/charset decoding.
Async file opening, length inspection, and reading run on the thread pool, not
on the caller's task scheduler. The caller's wait ends on Stop or timeout;
an underlying platform open/read may finish later. Its token remains cancelled
and its owned stream is closed when that worker resumes. No platform thread
is forcibly terminated.

## Deadline And Ownership

Each document navigation captures one `Tm` cancellation budget for document I/O
and its linked stylesheets. HTTP requests register an abort callback for that
budget, including POST upload, response headers, and body reads. Slow trickling
cannot reset the whole input budget with every byte. Stop remains distinguishable
from timeout. Synchronous navigation now rejects an overlapping async load.

An exhausted budget skips remaining optional stylesheets. A loaded document
can still complete with retained title and fields after a sheet times out.
Stop, unlike an optional stylesheet failure, cancels the pending navigation.
Unexpected internal exceptions are not silently reclassified as timeouts.

Use async APIs on the UI thread. Synchronous APIs still block their caller.
This is not a CPU deadline for HTML parsing, CSS compilation, native decoding,
layout, or paint. Pictures retain their separate pass budget from runtime 166,
so this is not a single deadline for every phase of a rendered page.

HTTP response metadata is read from the response owned by that request, with
cookies, redirects, charset, cache policy, and validators preserved. Conditional
304/error responses and late failed requests are closed. The existing streamed
download path remains separate.

## Failure And Origin Policy

An oversized document raises `Er.K="large"`, with `Rt=false`, before replacing
the current page or history. It reaches the existing `V.Le/Fe` failure surface.
It is not automatically retried; a subsequent explicit navigation can succeed
after the input changes. Oversized, malformed, or failed linked CSS is skipped
and removed from the resource cache. No partial oversized sheet is applied.

The stylesheet ceiling is checked on cache reuse and stale entries too, so a
larger resource previously loaded as a document cannot bypass the sheet limit.
The existing document cache still has its smaller 2 MiB per-entry ceiling.

File stylesheets require a file document origin. HTTP(S) documents cannot load
them, and a document's `base` URL cannot grant file access. UNC stylesheet URLs
are rejected before I/O, even for local documents. Local documents retain local
stylesheet support. This is a resource check, not a complete browser sandbox,
CORS/CSP implementation, or restriction on every host-controlled file operation.

## Verification

`Lea.Tests <root> --input` covers sync/async local rejection, GET/POST wire and
decoded limits, exact-limit bodies, valid gzip/deflate, stalled headers, trickling
bodies, shared stylesheet budgets, optional-sheet retention, cached oversize
rejection, Stop, overlap, next-navigation recovery, file/base origins, no-I/O UNC
rejection, exact overflow reads, and the compact limit API. The full core suite
also exercises cookies, private contexts, redirects, POST/multipart, validators,
disk caching, failures, downloads, forms, CSS, and native rendering.

The `input` Boot case exercises the compiled Firespider form with an oversized
local page, typed failure and request cleanup, then same-view navigation to a
page whose stylesheet response is held open. It checks retained fields,
completion, no startup exceptions, and no MSHTML. `input-loading.html` and its
linked CSS add an executable benchmark probe.

The runtime 167 release run passed the focused input suite, full core suite,
isolated native editor suite, compiled host suite, 67 benchmark probes over 64
fixtures, all 40 Boot cases in one combined run, and all six actual executable
startup scenarios. The executable windows remained responsive and no startup
error dialog or MSHTML load was observed. Engine DLLs total exactly 500.0 KiB;
their GZip estimate is 212.9 KiB. These footprint and timing samples are not a
claim that compression makes execution faster.

The initial oversized-file regression failed on runtime 166. The first core
run after the reader change exposed a fixed-step scheduler assumption: file
work now runs independently of that scheduler. Those tests now retain their
per-step invariants with a five-second wall-clock wait. A later callback test's
combined assertion failed, while an isolated diagnostic run passed; its exact
early state was not captured. Callback tests now cover both local completion
and a deliberately gated HTTP successor, with a pre-call flag guarding inline
completion. The gated variant still requires observing pending ownership,
cursor state, completion counts, and suppression of stale page commands.
A later readiness assertion captured a completed load before its pending phase
was observed. That fixture now gates both document headers and picture input,
requiring the overlap checks before releasing the image response and completing.
The full Boot run also exposed a split lifecycle test that observed loaded panes
before the split form's async continuation re-enabled its comparison button.
The readiness helper now waits for that form state; five focused runs and the
subsequent combined 40-case run passed.

## Remaining Work

Input byte limits are not total or peak memory limits. Byte arrays, decoded
strings, document trees, history, cache entries, native images, and other tabs
have separate lifetimes. Native image preflight, worker decoding, and shared Lea
image-byte admission are covered by `IMAGE_PREFLIGHT.md`; killable process
isolation and measured whole-process memory policy remain priorities. HTTP cache key case handling is
now complete and covered by `CACHE_IDENTITY.md`. Image cookie/cache integration,
richer encoding support, full JavaScript, and broader web-platform behavior
still need work. Compression and short aliases alone do not establish faster
execution.
