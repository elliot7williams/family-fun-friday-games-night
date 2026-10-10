# Bounded startup failure and handshake reports

The startup failure message previously appended a 512-character `Sd` report
to an error prefix, then kept only the first 512 characters for `Jr.M`.
That second truncation could remove the newest worker checkpoint. Large native
error messages could also consume the whole result and hide the parent phase.

`Jp` now retains the parent summary and captured worker diagnostics separately
for the current startup attempt. `StartupFailure` reserves at most 128 characters
for the error text, marking a longer error with `...`, and formats the summary
and newest available worker trace within the final 512-character result limit.
Startup stderr is no longer duplicated ahead of the startup report. Ordinary
execution error handling and the separate 512-character `Sd` contract remain
unchanged. A later lifecycle attempt cannot rewrite an already returned `Jr.M`.

## Checkpoint meaning

- Parent `init`: writing the initial frame to the worker.
- Parent `hello`: waiting for the first worker frame to be read and decoded.
- Parent `verify`: checking that frame's fields and isolation results.
- Parent `ready`: the handshake and isolation checks succeeded.
- Worker `startup hello`: entered the first outbound hello write after the
  trusted DOM bootstrap and denial probes.
- Worker `startup ready`: that write and its flush returned.

The worker write interval includes serialization, UTF-8 encoding, framing,
pipe writes and flush. It does not identify which of those costs dominates.
Worker `ready` does not mean that the parent has received or accepted the frame.
Conversely, the parent can read the frame before the separate asynchronous
stderr drain has captured the worker's later `ready` marker. A missing marker
is not proof of an exact blocking instruction. Worker wall times and parent
wall times use different clock origins.

More checkpoints can push older text out of a bounded timing trace. The
fresh-path regression checks retained startup checkpoints rather than requiring
the oldest entry marker to survive every bounded report. The native phase test
allows up to one second for the independent stderr drain after successful page
execution; this does not change the five-second worker startup deadline.

## Verification

Evidence is retained in
`H:\Lea Evidence\Lea-146-handshake-reports-2026-10-10`.
The identical final test executable fails on the preceding runtime both for
the absent formatter and for a real controlled oversized launch failure that
loses its parent summary. The candidate passes 15 formatter checks, 24 controlled
launch-failure checks, 16 blocked-I/O checks and 12 native phase checks. The two
native handshakes cover timing tracing off and on, one launch each, without
retrying failed gates.

The single ordinary unobserved scripting run passes all 1,876 checks using
that same final executable. Earlier failed full and host gates stay preserved;
this success does not replace them or prove the intermittent timeout resolved.
The required renderer smoke also passes. The single observed Firespider script
case instead fails `scripted split pane loads`: retained left/right startup
results show parent wall times 5,020/5,077 ms and captured CPU 531/593 ms, with
complete last `parsed` lines. Neither failed result captures completed DOM,
hello-write entry or hello-write completion. These absences do not locate the
instruction that was running when the deadline expired.

The observer retires with 253 samples from nine PIDs, no counter/thread/observer
errors and no dropped requests or samples. Two fresh pre-failure main/attribute
captures are visually inspected; the flow never reaches its split capture.
Successful replacement-session `ready` reports printed beside the retained
failed results do not turn those failures into passes. The visible live PIDs
also belong to replacement sessions; matching earlier phase fingerprints to
sampled PIDs is inference, not an explicit failed-result PID.

Original host source inputs and installed binaries stay unchanged. The full
host matrix, actual-browser EXE startup scenarios and a new release are not
verified in this round. Failed gates are not retried.

Only `Lea.Script.dll` and `ScriptWorker/Lea.Script.Worker.exe` change among the
19 runtime records. The DLL grows by 512 bytes to 94,720 bytes; the worker
remains 46,592 bytes. Required Engine/Win/Image size stays 747,520 bytes.
No bootstrap, wire-format, interpreter dependency, renderer or alias change
is included.

The native startup deadline remains 5,000 ms and the default page execution
deadline remains 2,000 ms. Memory, CPU, recursion, statement, bridge, capability,
file, network, token, Job and mitigation protections remain intact. External
page scripts remain off. This is a diagnostic correction, not a demonstrated
speedup or resolution of the intermittent native startup timeout. Milestone
146 remains open and broader readiness remains about 65%.
