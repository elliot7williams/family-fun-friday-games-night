# Native Image Isolation

Milestone 70 / runtime 170 moves accepted native picture codecs out of the
browser process. Lea still performs bounded structural preflight in `Lea.Win`,
then sends the accepted source to the required 8 KiB `Lea.Image.exe` helper.
The helper returns validated raw pixels rather than another encoded image.

Milestone 71 / runtime 171 assigns that helper to an unnamed Windows Job Object
before sending request bytes. The Job allows one process, enforces a configurable
hard process-memory ceiling, exits on an unhandled exception, and kills the
helper when the browser loses or closes the Job handle.

Milestone 72 / runtime 172 creates the helper with a privilege-stripped token at
low mandatory integrity. It inherits only the three protocol/error pipe handles,
starts suspended, enters the Job before its primary thread resumes, applies
decoder-safe process mitigations, and must acknowledge those guards before the
host sends encoded image bytes. The Job also reports peak process memory.

## Compact API

```csharp
string decoder = view.Ie;
bool isolated = view.Ii;
int lastWorkerProcess = view.Iw;
int killedWorkers = view.Ik;
bool jobBound = view.Ij;
long hardWorkerBytes = view.Ih;
long lastPrivateBytes = view.Ipm;
bool restrictedToken = view.Irt;
bool lowIntegrity = view.Ili;
bool suspendedJobPrebind = view.Isp;
bool inheritedHandleList = view.Ihl;
int mitigationMask = view.Img;
long peakWorkerBytes = view.Ipk;
```

`Ie` is the image-helper executable path. It defaults to `Lea.Image.exe` beside
the application or `Lea.Win.dll`; a host may set an explicit full path before
navigation. `Ii` reports whether that file currently exists. Missing helpers
produce a normal failed-image placeholder and do not fail the document.

`Iw` is the process ID used by the view's most recent admitted decode request.
It remains available after the request for diagnostics. `Ik` counts helpers
that this view terminated because Stop, replacement, disposal, or the shared
picture deadline cancelled active native work.

`Ij` confirms that the view's last admitted worker was assigned to its Job.
Assignment is mandatory: if Job creation, limit configuration, or assignment
fails, Lea terminates the unbounded process and stores the normal failed-image
sentinel. Encoded image bytes are not sent to an unbounded helper.

`Ih` configures the process-wide hard worker-memory ceiling. It defaults to
192 MiB and clamps host values from 64 to 512 MiB. A changed value takes effect
on the next admitted decode and causes the shared worker to restart under a new
Job. `Ipm` is the private-byte value sampled from the last successful worker
response; it is diagnostic rather than a peak or reservation counter.

`Irt` confirms that `CreateRestrictedToken(DISABLE_MAX_PRIVILEGE)` produced the
launch token and inspection found at most Windows' traversal privilege enabled.
`Ili` confirms low mandatory integrity RID 4096. `Isp` confirms that Job
assignment completed while the new process's primary thread was suspended.
`Ihl` confirms that `PROC_THREAD_ATTRIBUTE_HANDLE_LIST` limited inheritance to
stdin, stdout, and stderr pipes.

`Img` is the helper's mitigation handshake. Bit 1 is permanent strict-handle
checking and bit 2 disables legacy extension points; both are required before
admission. Bit 4 blocks remote/low-integrity executable images and prefers
System32, and bit 8 blocks child-process creation when those Windows 10 policies
are available. `Ipk` is `PeakProcessMemoryUsed` queried from the Job after the
last successful response; unlike `Ipm`, it tracks the worker lifetime peak.

Existing image aliases retain their meanings. `In` counts admitted native
decode requests, `If` counts current failed-image sentinels, `Ic/Ib` count
retained bitmaps and estimated bytes, and `Ip/Im/It` expose the per-image pixel
cap plus shared reservation limit/current bytes.

## Process And Protocol

All Lea views share one reusable helper process and serialize decode requests
through it. Reuse avoids starting a process for every image while keeping only
one decoder process resident. A changed `Ie` path, helper arguments, or `Ih`
limit restarts that shared helper.

The host creates a primary token with maximum privileges disabled, verifies that
no more than the traversal privilege remains enabled, and sets its mandatory
label to low integrity. It creates three anonymous pipes and uses an extended
startup handle list so no other inheritable host handle enters the child. The
helper is created hidden and suspended with `CreateProcessAsUser`.

Before `ResumeThread`, the host creates the Job, sets `ACTIVE_PROCESS`,
`PROCESS_MEMORY`, `DIE_ON_UNHANDLED_EXCEPTION`, and `KILL_ON_JOB_CLOSE`, and
assigns the suspended process. The one-process limit and the helper's child
policy prevent child creation. Closing the Job kills a worker even when it is
sleeping inside native code and cannot observe pipe EOF.

After resume, the helper applies permanent strict-handle and extension-point
policies plus supported image-load/child policies, then writes its marker and
mask. The host requires the marker and base mask before writing the first source
length. A missing/old/unhardened helper is killed and becomes a normal nonfatal
failed image without receiving encoded bytes.

The private little-endian pipe protocol is deliberately small and bounded:

1. The helper writes a startup marker and mitigation mask; the host validates
   the required bits before image admission.
2. The host writes a 32-bit source length and at most 4 MiB of preflighted data.
3. The helper decodes with GDI+, validates an 8,192-axis/8,388,608-pixel ceiling,
   and paints the first image frame into 32-bit ARGB pixels.
4. The helper writes a fixed marker, status, width, height, exact byte count,
   and at most 32 MiB of raw pixels.
5. `Lea.Win` validates every response field before allocation, copies rows into
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
memory admission, one-PID codec reuse, Job/cap aliases, cap clamping/restart,
private/Job-peak sampling, live token integrity/privilege inspection, live Job
membership, each mitigation policy, an invalid-handshake helper, network limits,
retry, Stop, disposal, and successor loads. A child test host then blocks its
helper for 60 seconds, is terminated,
and requires Job close to remove that exact helper PID within three seconds.

The runtime 172 release run passes the full core and isolated editor suites, the
compiled adapter smoke, 69 benchmark probes over 66 fixtures, all 42 Boot cases
in one combined run, and all six actual EXE startup scenarios. The Boot `decode`
case loads one valid image in the helper, rejects one over-axis PNG before
admission, reports all 15 current guard bits and samples 8,540,160 private/Job
peak bytes under the 192 MiB cap, verifies host cleanup/no MSHTML, and checks
child-process exit.

The measured runtime files total 536.0 KiB with a 228.8 KiB GZip estimate. The
report records 521.959 ms clean-process, 3.323 ms local-load, and 33.453 ms
layout/paint medians. These unpaired local samples are not speedup claims.

## Remaining Limits

The helper is privilege-stripped and low integrity, but it is not an AppContainer
or complete operating-system sandbox. It retains the user's read identity and
network credentials and does not yet have a capability-denied network token,
custom object ACL, CPU-time/rate limit, or startup ASLR policy. The restricting-
SID experiment was not retained because it prevented the .NET CLR from loading;
the token boundary here is disabled privileges plus mandatory low integrity.
`Ipm/Ipk` cover the helper, while `It/Im` remain Lea estimates rather than a
combined host-plus-helper memory budget.

One shared worker deliberately serializes native decodes. Image requests still
do not share the document cookie jar, validator cache, or request diagnostics.
Lea does not add new codecs, CORS, GPU decoding, multi-frame animation playback,
or full modern-image compatibility here.
