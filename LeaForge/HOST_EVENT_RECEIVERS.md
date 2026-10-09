# Firespider event receiver integration

## Candidate

Separate experimental v28 integrates the production dependencies verified at
`0b94a79c9264b9485aad751480c042ad009399e8` into the .NET Framework 4.8 Firespider
host. All 19 files match that source checkpoint. No engine SDK rebuild or
production-code, alias, startup-budget, sandbox or interpreter-limit change is
made in this checkpoint. The original 67 host inputs and three installed browser
binaries remain unchanged.

The candidate compiles from 67 original inputs and 25 overlays, with 90 unique
final source/build inputs. Its 376 raw runtime files match the fresh NTFS test
copy before testing. The test harness adds one file to that copy.

Candidate EXE: 1,134,080 bytes, SHA256
`8A93AEA093B65F68C397553EA75EBAF3ADA044FEA3F38F80E02B8ABA00E71A08`.
Host harness SHA256:
`17DFCC949B17E387D7D2EF81A92391CF3675B65009FB2B4CBF01A422418B075A`.

## New host checks

Both the first on-demand realm and the subsequent inline-navigation realm
check shared add/remove methods and all five supported handler accessor pairs.
They retain method names, arity and the existing own-property attributes.
Invalid receivers, including proxies and inherited fake nodes, must throw
`TypeError` before type or option getters run. The checks retain their worker
PID and report zero document mutations; this is not a claim of zero DOM reads.

The existing real keyboard click now registers its function/object listeners
through borrowed document/window methods and sets/gets/clears the counter's
handler through a borrowed accessor. A borrowed capture-listener removal must
prevent a throwing callback. Null, undefined and detached method calls register
three once-only window callbacks in order. The existing captured-intrinsic,
receiver, cancellation, restored-event-state and DOM/title repaint checks remain
in place. No extra counter click or fixture-state reset is introduced.

## Host results

One full matrix attempt runs from `2026-10-09T00:39:44.4774308Z` to
`2026-10-09T00:46:59.8844433Z` and passes all 43 cases. This includes the real
30-second session/hibernation timers, navigation, clipboard recovery, policies,
splits, resource lifecycle and the complete script case. The separate 14
clipboard and 12 observation contracts also pass.

Only the script case has opt-in whole-script observation and timing trace.
The other cases are not observed. The script case covers first demand, inline
navigation, native events/defaults, timers and microtasks, live DOM operations,
intentional script errors, fresh-realm recovery, split isolation/disposal, tab
cleanup, and refusal of remote-origin scripts. Both new receiver checks and the
borrowed native dispatch check pass. Main and split captures are inspected and
nonblank. This is an instrumented scripting integration result, not a controlled
startup-performance benchmark.

## Identical-harness negative control

The exact current compiled harness is copied into a second fresh NTFS runtime
with the same host and the preceding 19 production files. Its initial worker
starts successfully, and the exception-constant check passes. It then exits 1
at the new first-demand invalid-receiver assertion:
`host event receiver rejection`. This is the expected regression failure, not
a startup timeout. Other runtime files match the current candidate; the same
harness SHA256 is verified before the control runs.

This control specifically establishes that the preceding runtime lacks the
required invalid-receiver behavior. The passing current native-click checks
cover borrowed dispatch; do not describe the negative control as having reached
that later click. No failed control is retried. The preceding source checkpoint's
trusted-only H: control remains separately inconclusive because it timed out in
bootstrap before its receiver assertion; this host result does not erase it.

## Actual EXE startup

The current candidate then passes all six existing actual-EXE startup scenarios,
each observed for at least eight seconds: clean, restored, locked session,
oversized session, lazy-background and lazy-all. Each remains responsive, with
no checked invalid-cast/JIT/crash-recovery dialog and no MSHTML module. Lazy-all
also resumes its selected deferred tab successfully.

| Scenario | First ready ms | Observed ms |
| --- | ---: | ---: |
| Clean | 4,635 | 8,206 |
| Restored | 5,523 | 8,225 |
| Locked | 6,102 | 8,171 |
| Oversized | 5,299 | 8,177 |
| Lazy background | 6,590 | 8,182 |
| Lazy all | 4,055 | 9,089 |

These are surrounding executable/UI-readiness observations, not native worker
startup times. The start-page fixtures can leave the optional interpreter
dormant, so this matrix alone cannot establish JavaScript cold-start stability.
The helper runs in Windows PowerShell STA from its trusted repository source,
without changing execution policy. Its retained stderr contains module-import
progress CLIXML, not a test error; process exit and all six structured results
confirm completion. Caches and machine load are uncontrolled.

## Observation boundary

The complete script flow passes and the sampler retires after 33,440 ms. It
captures 239 samples across eight worker identities and 55 thread snapshots,
with zero dropped requests/samples, counter/thread/observer errors or truncated
thread lists. Minimum per-identity thread-snapshot spacing is 1,001 ms; maximum
queue lag is 29 ms. Counter mean/max is 0.10/11 ms, thread mean/max is 44.65/65 ms,
and total measured sampling wall time is 2,527 ms. These describe observer cost,
not application performance.

No actual startup timeout occurs in this run. The earlier intermittent
five-second native timeout, including v26's post-error recovery failure, remains
unresolved. Neither successful observations nor the receiver change prove its
root cause or a speed/memory improvement. All original deadlines and native
protections remain in place; no failure is relabeled or retried into a pass.

## Preservation and next gate

Evidence is under `H:\Lea Evidence\Lea-146-host-event-receivers-2026-10-08`:
the exact 377-file tested runtime, 90 final host source/build inputs, 67 original
host inputs, preceding-runtime overlays and identical harness, build/test logs,
observation, inspected captures, complete Lea source archive, checkpoint bundle
and final sealed inventory. The original atomic patch-context mismatch is
retained; it changed no file and occurred before the corrected edit or runtime
tests. Both native diagnostic copies remain retained separately.

The current candidate is at
`C:\Users\Elliot\AppData\Local\Lea\HostDiagnostics\Lea-146-event-receivers-host-v28`.
Its EXE needs the accompanying dependencies. It is an experimental test build,
not an installed replacement or a verified release package.

No new runtime/host-source release ZIP, fresh package extraction, independent
archived-source build/runtime gate, new full source-suite run, or live replacement
is claimed. The preceding 1,342-check source pass belongs to its exact earlier
test executable and unchanged production bytes. Next gates are fresh-package and
independent-source verification, startup reliability, safe remote-script policy
and broader website compatibility. Hardening milestone 146 remains open;
broader readiness stays about 65%, a practical estimate, not standards conformance.
