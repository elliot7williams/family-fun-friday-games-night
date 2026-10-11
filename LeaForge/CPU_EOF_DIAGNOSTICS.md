# Controlled worker EOF diagnostics

Milestone 146 remains open; broader readiness stays about **65%**. This change
improves test evidence for the unresolved worker-exit timeout, not Lea's
production startup path. All 19 production runtime records, including the
script worker and required 749,056-byte Engine/Win/Image bundle, are unchanged.

## Test command

`Lea.Script.Tests.exe "L:\Lea Development" -ce` is the short alias for
`--cpu-eof`. Extra arguments are rejected before worker launch. Ordinary
`--native-cpu` and full-suite runs leave helper tracing off.

The controlled helper can emit five ASCII phase bytes on its otherwise unused
stdout pipe: `i` enters managed Main, `m` follows mitigation setup, `e` observes
stdin EOF, `c` follows bounded CPU work, and `x` precedes normal return. A byte
instead of EOF emits `d` and does not satisfy the success sequence. The
fault-only `--trace --hold` mode emits `h` instead of `x` and deliberately holds
exit until the unchanged Job disposal kills it. This mode is a test helper,
not an engine dependency, fallback, production worker option or browser mode.

The parent takes one bounded pipe snapshot after its existing wait, without a
background stdout reader, sampling loop or startup observer. It uses the
read/available counts and does not consume the marker bytes. An absent or
failed snapshot is explicitly unavailable, not proof that Main never ran.
Reaching `x` is not itself proof of process exit; the independent native handle
must signal and the native exit code must also be zero.

## Wait reporting

Previously every nonzero native wait result became `Controlled EOF worker did
not exit`. Now only `WAIT_TIMEOUT` becomes TimeoutException. `WAIT_FAILED`
retains its actual Windows error through Win32Exception; unexpected results
remain distinct. Failure output includes launcher owner, PID, exact wait value,
elapsed time, independent native CPU/exit readings and the optional marker
snapshot. Missing counters stay unavailable, not fabricated zero. Structured
Exception.Data lets the fault checks assert the same values without parsing
message text.

The classification follows Microsoft's [WaitForSingleObject contract](https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject).
[PeekNamedPipe](https://learn.microsoft.com/en-us/windows/win32/api/namedpipeapi/nf-namedpipeapi-peeknamedpipe)
supports anonymous-pipe snapshots and an omitted optional message-remainder
output; this test leaves that output unused. No nonblocking guarantee is made
for general synchronous multi-threaded pipe use.

The test wait remains 10 seconds, the helper's CPU-work guard remains three
seconds, production startup remains five seconds and the default page budget
remains two seconds. CPU-rate, memory, exact three-pipe inheritance,
zero-capability AppContainer, loopback, ASLR/mitigation and file-grant guards
are not relaxed. Held workers are independently watched through cleanup.

## Evidence

Proof: `H:\Lea Evidence\Lea-146-cpu-eof-2026-10-11`.

- Both test-tool compilations finish without warnings/errors.
- The quiet native CPU lifetime command passes its unchanged 17 assertions.
- The corrected `-ce` command passes 33 checks: native wait classification,
  anonymous-pipe byte counts/non-consumption, successful phase/exit/CPU readings,
  intentional held-exit timeouts through both linked launchers, exact child
  retirement and restored file grants.
- The extra-argument control and preceding helper control both exit 1 as
  expected. The latter reaches a normally exited worker with native CPU data,
  but rejects missing phase markers; it is not a startup timeout control.
- One final quiet full-suite invocation exits 1 after **714 passing assertions**
  in isolated event-realm recovery. The native startup report has `phase=hello`,
  launch 106 ms, probe 6 ms, wall 5,006 ms, CPU 109 ms and final checkpoint
  `startup evaluate`; the worker does not deliver ready before its unchanged
  five-second deadline. It fails before reaching the CPU lifetime tests, not
  at a controlled EOF wait. The expected 2,418-check completion is not reached.
  No second invocation substitutes for this failure.

The initial traced probe fails despite a zero child exit and 156 ms CPU time:
it rejects an unused byte-pipe message-remainder output. A separate in-process
pipe control records five bytes read/available but remainder 4294967291. The
corrected tool omits that optional output and bounds a five-byte snapshot.
The native helper and all production runtime files are byte-identical between
these two tools. Original binaries, source-input hashes and failed results are
retained, not overwritten or relabeled. Each tool gate has one invocation.

The preceding full-suite failure after 1,980 assertions remains unexplained.
Trace-mode success or a later quiet success cannot retroactively identify its
wait result, native CPU state or failure phase. Production behavior is not
changed by this diagnostic work. Existing browser deployments and original
Firespider inputs/installed binaries remain untouched; no new host matrix,
actual Firespider startup, stable release, speedup or intermittent-failure fix
is claimed. A separate complete source backup preserves this change.
