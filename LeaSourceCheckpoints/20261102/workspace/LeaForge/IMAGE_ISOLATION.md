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

Milestone 73 / runtime 173 adds creation-time DEP, SEHOP, heap-corruption
termination, forced-relocation ASLR, and bottom-up ASLR before the suspended
process exists. Its Job now enforces a configurable hard CPU-rate cap and
verifies the applied flags/rate before assigning and resuming the helper.

Milestone 91 / runtime 191 moves the helper into a deterministic per-user
AppContainer with an empty capability set. The launcher grants that exact
AppContainer SID read/execute access to `Lea.Image.exe` only, verifies the
actual child token identity, zero capabilities, low integrity, and stripped
privileges while it is suspended, then applies the existing Job and mitigation
gates. A live listener and a real host-owned canary file prove that network and
ungranted host-file access are denied while bounded decoding still succeeds.

Milestone 92 / runtime 192 adds a process-wide combined host/helper admission
ceiling. After the hardened worker completes its handshake but before the host
writes encoded image bytes, Lea samples host private bytes, worker private
bytes, and the worker Job lifetime peak, then adds the structural preflight
reservation with saturating arithmetic. Over-limit or unavailable samples fail
closed, release the bitmap reservation, and terminate the helper.

Milestone 130 / runtime 230 keeps that isolation available when the browser is
run from FAT32 or another filesystem without persistent ACLs. Lea hashes the
bounded helper, copies only those verified bytes to a content-addressed per-user
cache on an ACL-capable local volume, re-verifies the cached copy before reuse,
and launches the same zero-capability AppContainer from there. The source path
reported by `Ie` does not change, and there is no unrestricted fallback.

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
bool startupAslr = view.Ias;
bool cpuPolicy = view.Icp;
int cpuRateBasisPoints = view.Icr;
int mitigationMask = view.Img;
long peakWorkerBytes = view.Ipk;
bool appContainer = view.Iac;
bool networkDenied = view.Ind;
bool helperAcl = view.Ial;
long combinedLimit = view.Icl;
long hostPrivateBytes = view.Ihm;
long combinedSignal = view.Icm;
bool memoryAdmitted = view.Iad;
```

`Ie` is the image-helper executable path. It defaults to `Lea.Image.exe` beside
the application or `Lea.Win.dll`; a host may set an explicit full path before
navigation. `Ii` reports whether that file currently exists. Missing helpers
produce a normal failed-image placeholder and do not fail the document. When
that path is on a volume without persistent ACLs, the launcher stages a
SHA-256-addressed copy under the current user's local application-data folder.
The source is capped at 16 MiB and both the copied bytes and reused local copy
must match the source digest before the AppContainer ACL is applied.

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
Job. `Ipm` is the worker private-byte value sampled at the last admission or
successful response; it is diagnostic rather than a peak or reservation
counter.

`Irt` confirms inspection of the actual AppContainer child token found at most
Windows' traversal privilege enabled. `Ili` confirms low mandatory integrity
RID 4096. `Isp` confirms that Job assignment completed while the new process's
primary thread was suspended.
`Ihl` confirms that `PROC_THREAD_ATTRIBUTE_HANDLE_LIST` limited inheritance to
stdin, stdout, and stderr pipes.

`Ias` confirms that Windows reports forced-relocation and bottom-up ASLR on the
suspended child. Lea also requests high-entropy ASLR; the current deliberately
small 32-bit helper cannot use that 64-bit-only mode. `Icp` confirms that the
Job accepted and returned an enabled hard CPU cap. `Icr` configures that cap in
basis points: it defaults to 2500 (25.00%) and clamps from 100 through 10000
(1.00%-100.00%). A changed rate restarts the shared worker at the next decode.

`Img` is the helper's mitigation handshake. Bit 1 is permanent strict-handle
checking and bit 2 disables legacy extension points; both are required before
admission. Bit 4 blocks remote/low-integrity executable images and prefers
System32, and bit 8 blocks child-process creation when those Windows 10 policies
are available. `Ipk` is `PeakProcessMemoryUsed` queried from the Job after the
last successful response; unlike `Ipm`, it tracks the worker lifetime peak.

`Iac` confirms the running process token carries Lea's exact AppContainer SID.
`Ind` confirms that token has zero capabilities, so no `internetClient` or
private-network capability is present. `Ial` confirms an explicit
identity-scoped read/execute ACE exists on the selected helper executable. The
launcher creates or reuses the `Lea.Image.Decoder` profile on the first admitted
native decode. A failure in profile creation, ACL installation, or live token
verification becomes the ordinary failed-image sentinel; there is no less
restricted decoder fallback.

`Icl` configures the process-wide combined image-memory ceiling. It defaults to
384 MiB and clamps host values from 128 MiB through 1 GiB without restarting the
worker. `Ihm` is the host process private-byte sample. `Icm` retains the larger
of the pre-decode projection and post-decode sample: host private bytes plus the
larger worker private/Job-peak value, with the preflight reservation added
before payload admission. `Iad` is true only when that projection was complete
and no greater than `Icl`. Sampling failure and arithmetic overflow reject the
image before `In` increments or encoded bytes cross the pipe.

Existing image aliases retain their meanings. `In` counts admitted native
decode requests, `If` counts current failed-image sentinels, `Ic/Ib` count
retained bitmaps and estimated bytes, and `Ip/Im/It` expose the per-image pixel
cap plus shared reservation limit/current bytes.

## Process And Protocol

All Lea views share one reusable helper process and serialize decode requests
through it. Reuse avoids starting a process for every image while keeping only
one decoder process resident. A changed `Ie` path, helper arguments, `Ih`
memory limit, or `Icr` CPU rate restarts that shared helper. `Icl` is checked
for every request against current samples and therefore needs no restart.

The host creates or reuses one deterministic AppContainer profile and adds an
explicit read/execute ACE for that SID to the configured helper file. It passes
an empty `SECURITY_CAPABILITIES` set through the extended startup attribute
list, alongside the three-handle allowlist and creation mitigations. No user
file, directory tree, network, or package capability is granted. Windows builds
the restricted low-integrity child token; while the process is still suspended,
Lea queries that actual token and requires the exact AppContainer identity,
zero capabilities, low integrity, and at most the traversal privilege enabled.
For a non-ACL source volume, the configured helper at launch time is the
verified content-addressed local copy; no directory tree or source volume is
granted to the AppContainer.
The same startup request enables DEP, SEHOP, heap-corruption termination, forced
relocation, bottom-up ASLR, and architecture-supported high-entropy ASLR. The
helper is created hidden and suspended with `CreateProcessAsUser`, and Windows'
ASLR policy is queried before admission.

Before `ResumeThread`, the host creates the Job, sets `ACTIVE_PROCESS`,
`PROCESS_MEMORY`, `DIE_ON_UNHANDLED_EXCEPTION`, and `KILL_ON_JOB_CLOSE`, then
sets and reads back `CPU_RATE_CONTROL_ENABLE | CPU_RATE_CONTROL_HARD_CAP` with
the requested `Icr` rate. Only then does it assign the suspended process. The
one-process limit and the helper's child policy prevent child creation. Closing
the Job kills a worker even when it is sleeping inside native code and cannot
observe pipe EOF.

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
PID, missing-helper failure, a forced native stall killed by a bounded view
budget, zero leaked reservation, clean restart, hostile preflight, shared
memory admission, one-PID codec reuse, Job/cap aliases, memory/CPU cap
clamping/restart, private/Job-peak sampling, live token integrity/privilege
inspection, live Job membership, queried ASLR and CPU policies, each helper
mitigation, an invalid-handshake helper, AppContainer identity/capability
inspection, automatic helper-file ACL installation, active denied connection to
a listening loopback socket, active denied access to a host-owned canary file,
network limits, retry, Stop, disposal, and successor loads. A child test host then blocks its
helper for 60 seconds, is terminated,
and requires Job close to remove that exact helper PID within three seconds.

The runtime 192 release run adds a dedicated combined-memory compatibility
probe and compiled Firespider checks alongside the AppContainer/ACL boundary.
The Boot `decode` case
loads one valid image in the helper, rejects one over-axis PNG before admission,
reports the zero-capability AppContainer, exact helper ACL, startup ASLR, the
2500 CPU rate, current guard bits, private/Job-peak bytes under the 192 MiB cap,
and an admitted host/helper projection under the 384 MiB combined ceiling,
plus host cleanup/no MSHTML and child-process exit. Focused tests force a
128 MiB over-limit projection, require zero native payload admissions, release
the reservation, and observe termination of the exact helper. Current full-suite and package
measurements are recorded in `PROGRESS.md` and
`COMPATIBILITY_PERFORMANCE.md`.

Runtime 230 also runs the compiled load lifecycle directly from a FAT32 release
stage. That probe exercises non-ACL detection, verified local helper caching,
the AppContainer handshake, bounded image response, and normal page completion.

## Remaining Limits

The helper is now a zero-capability AppContainer, but it is not a complete
operating-system or codec sandbox. Its deterministic per-user profile persists
after browser exit, and Windows/system paths available to AppContainers remain
available. The helper-file ACE is intentionally retained so later launches do
not need broader directory access. Content-addressed helper copies from non-ACL
volumes also remain in the per-user local cache; each is bounded to 16 MiB and
is re-verified before reuse. Lea has not enabled the `internetClient`,
private-network, documents, pictures, music, videos, or broad-filesystem
capabilities. `It/Im` remain explicit bitmap reservations while `Icl/Ihm/Icm/Iad`
add the combined admission boundary. The combined signal is conservative point
sampling, not a continuous operating-system-wide hard limit: host allocation
can change between samples, and the helper's separate Job remains the hard
per-process backstop. Deployment must allow
the current user to add the helper-file ACE; failure remains nonfatal to the
document and does not downgrade isolation.

One shared worker deliberately serializes native decodes. Image requests still
do not share the document cookie jar, validator cache, or request diagnostics.
Lea does not add new codecs, CORS, GPU decoding, multi-frame animation playback,
or full modern-image compatibility here.
