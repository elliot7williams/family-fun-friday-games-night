# Native Profile Failure Handling

Milestone 146 remains open. Broader readiness stays about 65%; this checkpoint
repairs a native setup-error path, not the intermittent bootstrap timeout.

## Reproduction

With C: reporting no free bytes, the ordinary `--startup-phase` command throws
an unhandled `COMException`, HRESULT `0x80070070`, from `Rp.ContainerSid` while
creating the worker AppContainer profile. Worker files and temporary files are
on L:. The native failure occurs before a child endpoint is returned.

`Jp.Run` already handles I/O and several typed setup exceptions, but this
otherwise-unmapped COM failure escapes that filter. It therefore bypasses the
normal bounded `setup` result and session reset. This is direct scripting API
evidence, not proof that a particular Firespider popup has the same cause.

The first attempted probe supplied a fixture path where the CLI expects the
repository root; its retained missing-directory error is a command setup error.
The corrected probe reproduces the native disk-full exception.

## Fix And Contracts

The shared native launcher now converts otherwise-unmapped profile HRESULT
`COMException` failures into `IOException`, retaining the hexadecimal native
code, description and original inner exception. Existing access-denied and
invalid-argument exception types are unchanged. A mapped out-of-memory failure
still propagates as `OutOfMemoryException`; critical failures are not hidden.

The helper is compiled into both the scripting launcher and the native-view
image launcher. Sixteen deterministic `--startup-errors` checks exercise both
copies. These verify conversion and preserved exception types, not actual OS
profile rollback or every possible HRESULT.

An internal-only `Jp` constructor can substitute the native creation operation
for tests. Public constructors still call the same `Rp.Open` path with the same
profile, file list, token checks, Job limits, mitigations and handshake. No new
public API, page capability, retry or fallback is introduced.

Eighteen deterministic `--startup-faults` checks inject the disk-full HRESULT at
that creation boundary. Two explicit calls on each of the same synchronous and
asynchronous sessions return bounded `setup` messages containing `phase=launch`.
They execute no page code, release file leases, clear running state, leave ACLs
unchanged and attempt creation again on the next explicit call. This injection
does not emulate partial OS profile creation or a successfully launched child.
The native-view cases retain the real `W` adapter and `V` event queue, replacing
only the adapter's private worker session in the test. Repeated errors arrive
as `setup`; the existing page stays idle, unmodified and visibly rendered,
without a UI exception. This is injected-failure evidence, not an actual
disk-full native launch or an actual Firespider EXE test.

Both new groups run in the ordinary suite. The opt-in `--startup-unavailable`
probe additionally expects an actual disk-full native failure, including native
view checks. It never fills a drive. Its first post-fix run does not pass:
about 7 MiB is free by then and the worker actually executes. That
invalidates its disk-full premise, not the ordinary successful-launch contract.

A later drive audit reports C: at zero free bytes again. One new run under that
freshly observed condition passes all 18 real disk-full/native-view checks,
without injecting the failure or filling a drive. Both synchronous and
asynchronous repeated calls return `setup` with the native HRESULT and phase;
leases, running state, ACLs and page state remain clean. The native view also
reports `setup`, stays idle and paints nonblank without a UI exception.
This verifies handling of this actual failure, not successful isolation startup
on an exhausted volume or partial OS profile rollback. No space is freed by
the fix; script execution correctly remains unavailable in that condition.

## Verification Boundary

Logs in the active `L:/Lea Development` checkout use the
`Lea-startup-followup-` prefix; failures are retained.

- Scripting and referenced projects build with zero warnings/errors.
- Final focused HRESULT checks pass all 16 cases; injected launch/view failures
  pass all 18 cases, including owner-context asynchronous cleanup, repeated
  calls, real adapter/queue status forwarding and nonblank native painting.
- The full core suite passes, including sandboxed image decoding, native Job
  cleanup, image cancellation/recovery, layout, painting and resource bounds.
- The full scripting run before the internal constructor addition still fails
  an isolated bridge-budget test while starting its fresh worker: 5,016 ms wall,
  562 ms child CPU, `phase=hello`, with `startup parsed` as its last checkpoint.
  The timeout budget and native protections are not relaxed.
- The final complete scripting run passes all 968 checks, including the 34 new
  error/launch/view checks. The earlier failing run is retained. A later pass
  does not explain it or establish stable cold-start performance.
- `unavailable-observed.log` passes the separate 18-case real disk-full gate
  after an audit and the probe both report C: at zero free bytes. The earlier
  healthy-launch/precondition failure remains in `unavailable.log`.
- An initial core build uses the installed older Framework compiler, which
  cannot parse the existing source syntax. The SDK-based core build succeeds
  with zero warnings/errors. No unrelated source changes repair that tool choice.

No full current Firespider matrix, package verification, new browser EXE,
release ZIP or installed-browser replacement is implied. The previous
934-check pass belongs to the prior source checkpoint. The native
profile fix does not establish a speed improvement or explain older timeouts.

System-drive space remains critical and changes during testing. Keep source and
build caches on L:, retain original Git object repositories, and free system
storage before controlled cold-start comparisons. No user files are removed by
this checkpoint.
