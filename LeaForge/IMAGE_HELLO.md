# Native Image Handshake Prefix Validation

Milestone 146 remains open. This follow-up rejects a malformed first handshake
word immediately and makes partial-handshake diagnostics more precise. It does
not fix the intermittent CLR startup delay, increase deadlines, or establish a
browser-wide speed improvement.

## Contract

The decoder reads the four-byte `0x3241454c` hello marker before reading the
hardening guard word. A wrong marker now faults with the existing
`InvalidDataException` message immediately, without waiting for guard bytes.
The decoder retires its owned worker and releases its serialized permit. No
host-memory admission, request write, or started callback can follow rejection.

A valid marker still requires a guard word containing both required bits before
`Ready` becomes true. A helper that sends only that marker remains blocked and
is still cancelable. Read-only `V.Idg` now distinguishes:

- `hello`: waiting for or rejecting the first word.
- `guards`: first word accepted; waiting for or rejecting the guard word.

Both phase-entry times are recorded in the existing bounded `steps` field. The
512-character report limit, native token/Job/pipe protections, AppContainer,
CPU and memory caps, cancellation behavior and parent deadline remain unchanged.
There is no new public command, protocol version or runtime dependency.

## Evidence

The preceding native core run first failed on full C: storage. After the user
restored space, the identical runtime failed to observe the partial handshake
within its existing three-second setup wait. Pipe read count was zero and
stderr empty. That failure's cause remains unproven; an ordinary CLR helper
launch is not proof of a passing sandboxed gate. Both failures are preserved.

A separate diagnostic using synchronous `PeekNamedPipe` blocked until the
helper's intentional 60-second exit. Its process exited zero, but its timing
was invalid and is explicitly recorded as such. A replacement memory-only
probe observed the valid first word and stderr marker at 2,601ms, then canceled
with worker retirement and permit recovery. This passing observation does not
clear the earlier intermittent startup failure.

The test helper also has a malformed-prefix mode: it flushes the wrong four-byte
word, reports that fact, then sleeps without sending guard bytes. With the
preceding decoder, a memory-only observation proves the wrong word was consumed
at 526ms and its marker observed at 543ms, while the task remained running at
544ms. The new regression then fails its three-second completion assertion on
that preceding DLL and cancels it for cleanup. This is a separate, reproduced
protocol rejection issue, not an explanation of the zero-byte startup failure.

With the fix, the focused malformed-prefix case faults normally at about 401ms
in that sample. Valid partial-handshake cancellation reports `phase=guards`
and read count four. Admission/start/host cancellation, blocked queue and
pre-cancel behavior, diagnostic-consumer failure containment and native-view
fresh-worker recovery pass. These individual samples are not a controlled
cold-cache benchmark.

The full `--core` suite passes. After adding a concurrent-retirement null guard
to the test's memory-only observer, the final rebuilt core EXE also passes that
suite with the same production DLLs. `--core` does not run the separate native
clipboard-contention/edit-command branches. Earlier results are retained rather
than replaced.

The complete scripting run on the updated production DLLs fails during isolated
selector bridge-limit recovery. Its fresh worker reaches `startup evaluate`
but misses the existing startup deadline: `phase=hello; launch=69; probe=6;
wall=5017; cpu=546 ms`. There is no final check-count summary and no rerun to
replace this failure. The earlier 1,713-check attribute-edit checkpoint is
retained, not claimed as a passing full scripting gate for this updated runtime.

The separately planned actual standalone-browser lifecycle check passes once:
the EXE exits zero, the rendered page says `Lea 1`, and a native click increments
its counter. The captured bitmap is inspected and retains the Lea icon and
visible fields. The script worker is isolated without a loopback exemption at
the unchanged 128 MiB limit; no mshtml/WebView2/libcef/msedgewebview module is
loaded. This narrow check does not replace the failed full scripting gate.
The engine stays experimental; no installed Firespider file is changed.

Evidence roots:

- `H:\Lea Evidence\Lea-146-attribute-edits-core-2026-10-09`: sealed restored-storage failure.
- `H:\Lea Evidence\Lea-146-image-handshake-diagnostics-2026-10-09`: sealed valid/invalid diagnostic attempts.
- `H:\Lea Evidence\Lea-146-image-handshake-phase-2026-10-09`: prefix regression, builds and final verification.

## Footprint

Only `Lea.Win.dll` changes among the 19 production dependency records. It
remains 315,392 bytes. The required Engine/Win/image bundle remains 745,472
bytes, with Engine 420,864 and image helper 9,216. The 4,608-byte handshake
helper is test-only and is not shipped with the browser. These are binary
sizes, not runtime memory or compressed speed measurements.

View DLL SHA256:
`CB63393302FC6E10653CCCC61B464062D6DC4ECB4FC075C8F66DC8115EAAB4A6`.

Lea-only broader site compatibility, remote-origin scripting policy and startup
reliability remain work. No installed Firespider replacement or stable release
is claimed; broader readiness remains about 65%.
