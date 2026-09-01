# Native Image Isolation

Milestone 70 / runtime 170 moves accepted native picture codecs out of the
browser process. Lea still performs bounded structural preflight in `Lea.Win`,
then sends the accepted source to the required 7 KiB `Lea.Image.exe` helper.
The helper returns validated raw pixels rather than another encoded image.

## Compact API

```csharp
string decoder = view.Ie;
bool isolated = view.Ii;
int lastWorkerProcess = view.Iw;
int killedWorkers = view.Ik;
```

`Ie` is the image-helper executable path. It defaults to `Lea.Image.exe` beside
the application or `Lea.Win.dll`; a host may set an explicit full path before
navigation. `Ii` reports whether that file currently exists. Missing helpers
produce a normal failed-image placeholder and do not fail the document.

`Iw` is the process ID used by the view's most recent admitted decode request.
It remains available after the request for diagnostics. `Ik` counts helpers
that this view terminated because Stop, replacement, disposal, or the shared
picture deadline cancelled active native work.

Existing image aliases retain their meanings. `In` counts admitted native
decode requests, `If` counts current failed-image sentinels, `Ic/Ib` count
retained bitmaps and estimated bytes, and `Ip/Im/It` expose the per-image pixel
cap plus shared reservation limit/current bytes.

## Process And Protocol

All Lea views share one reusable helper process and serialize decode requests
through it. Reuse avoids starting a process for every image while keeping only
one decoder process resident. A changed `Ie` path restarts that shared helper.

The private little-endian pipe protocol is deliberately small and bounded:

1. The host writes a 32-bit source length and at most 4 MiB of preflighted data.
2. The helper decodes with GDI+, validates an 8,192-axis/8,388,608-pixel ceiling,
   and paints the first image frame into 32-bit ARGB pixels.
3. The helper writes a fixed marker, status, width, height, exact byte count,
   and at most 32 MiB of raw pixels.
4. `Lea.Win` validates every response field before allocation, copies rows into
   a new bitmap, and applies the existing preflight, per-view, and shared-memory
   checks before retention.

Encoded bytes are never passed back to the host, so hostile codec parsing stays
inside `Lea.Image.exe`. The browser still allocates and paints ordinary GDI+
bitmaps from already bounded raw pixels.

## Cancellation And Recovery

Decode work keeps its `Im/It` reservation while active. The request token is
linked to Stop, replacement, disposal, and the picture-pass budget. Cancellation
terminates the exact helper process, unblocks the pipe task, disposes any late
bitmap, and releases the reservation once that task ends. The next admitted
image starts a clean helper automatically.

A codec exception, malformed response, closed pipe, missing executable, bad
dimensions, or bad byte count is nonfatal to the page. Lea stores the normal
null image sentinel so painting does not create a restart/refetch loop. Reload
can retry it under the existing rules.

When the browser exits, inherited pipe handles close. The helper sees input EOF
and exits; the compiled Boot launcher verifies the reported worker PID is gone
after the host process ends.

## Visual Studio And Firespider

`Lea.Image` is a .NET Framework 4.6.1 project in `Lea.sln`. Release builds of
`Lea.Win`, `Lea.Tests`, `Lea.Bench`, and `Lea.Win.Test` copy its executable next
to their output. The packaged Firespider target copies `Lea/Lea.Image.exe` to
the browser output directory after each build. A packaged integration therefore
requires these three matching runtime files:

```text
Lea.Engine.dll
Lea.Win.dll
Lea.Image.exe
```

No Chromium, WebView, Safari, downloaded browser runtime, service, installer,
or third-party codec package is introduced.

## Verification

`Lea.Tests <root> --pics` covers PNG, JPEG, GIF, BMP, and ICO through a different
PID, missing-helper failure, a forced two-second native stall killed by a 300 ms
view budget, zero leaked reservation, clean restart, hostile preflight, shared
memory admission, network limits, retry, Stop, disposal, and successor loads.

The runtime 170 release run passes the full core and isolated editor suites, the
compiled adapter smoke, 69 benchmark probes over 66 fixtures, all 42 Boot cases
in one combined run, and all six actual EXE startup scenarios. The Boot `decode`
case loads one valid image in the helper, rejects one over-axis PNG before
admission, verifies host cleanup/no MSHTML, and checks child-process exit.

The measured runtime files total 521.5 KiB with a 222.2 KiB GZip estimate. The
report records 486.317 ms clean-process, 3.299 ms local-load, and 33.184 ms
layout/paint medians. These unpaired local samples are not speedup claims.

## Remaining Limits

The helper is a killable process boundary, not a restricted-token or operating-
system sandbox. It does not yet use a Job Object memory ceiling, AppContainer,
low-integrity token, process mitigation policy, or measured private-byte cap.
`It/Im` remain Lea estimates rather than total host-plus-helper memory.

One shared worker deliberately serializes native decodes. Image requests still
do not share the document cookie jar, validator cache, or request diagnostics.
Lea does not add new codecs, CORS, GPU decoding, multi-frame animation playback,
or full modern-image compatibility here.
