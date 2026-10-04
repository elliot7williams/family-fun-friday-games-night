# Bootstrap Follow-Up

Experimental follow-up to the bounded timers work. Broader readiness remains
about 65%, not a standards score. Intermittent isolated-worker startup remains
unresolved; successful samples do not explain earlier failed launches.

## Implementation

The shared DOM bootstrap no longer constructs a Set for its six supported event
types. A private exact comparison retains the same whitelist without consulting
page-modifiable Set helpers. Fixed handler/style names use indexed private lists;
style names are captured once rather than enumerated for every new wrapper.
Changing the page's array iterator cannot interfere with those two private lists.
Other DOM/event bookkeeping is not claimed to resist every modified intrinsic.

The worker adds canary/network/bridge/source/parse checkpoints to opt-in startup
tracing. Normal initialization keeps its regular string parser and remains
sequential. Only tracing separates parsing with a parse-only prepared program,
explicit interpreted regexes and the existing 100-ms regex timeout. Prepared
programs have independent regex defaults, so the diagnostic path does not rely
on the VM's regex setting propagating automatically. See the pinned Jint
[parsing options](https://raw.githubusercontent.com/sebastienros/jint/v4.16.4/Jint/ParsingOptions.cs)
and [preparation options](https://raw.githubusercontent.com/sebastienros/jint/v4.16.4/Jint/PreparationOptions.cs).

Startup remains bounded by the existing 5,000-ms parent handshake; page execution
still defaults to 2,000 ms (maximum 5,000), with the verified zero-capability
AppContainer, network/canary probes, one-process Job, 128-MiB memory and 25% CPU
hard cap. No new permission, required-core dependency or public alias is added.
Event and microtask setup failures now include the bounded startup report and
at most 256 characters of the authored test code. Explicit tracing is respected
by every startup case, including its first fresh-path launch.

## Experiments

Logs are retained in `%TEMP%`. Earlier files and diagnostic runtimes are preserved.

- `Lea-bootstrap-phases-build.log`: focused build passes without warnings/errors.
  `Lea-bootstrap-phases-host-script.log`: a separate diagnostic copy of the v8
  host passes its scripting case, including the timer assertion. First startup
  wall is 3,689 ms; parsing consumes 926 ms and DOM evaluation 1,256 ms between
  worker checkpoints. These phase experiments used prepared-bootstrap defaults
  before the final explicit-regex diagnostic adjustment; they are observations,
  not a production-path benchmark or a stable-startup claim.
- `Lea-bootstrap-comparison-build.log`: builds the concurrent parsing experiment
  and comparison harness without warnings/errors. `Lea-bootstrap-comparison.log`
  fails the final current launch at 5,016 ms wall / 93 ms CPU, before VM/parse
  completion is reported. Other current samples are not consistently faster.
  The concurrent initialization experiment is removed; the cause of this stall
  is not established, and no deadlock diagnosis is claimed.
- `Lea-bootstrap-private-lists-red-build.log`: builds without warnings/errors.
  `Lea-bootstrap-private-lists-red.log` reproduces a wrapper failure when page
  code replaces Array.prototype's iterator. The wrapper asserts that exact
  expected failure, not merely any nonzero process result.
- `Lea-bootstrap-private-lists-events.log`: all 136 event checks pass after the
  private-list/whitelist fix, through both trusted and isolated transports.
- `Lea-bootstrap-fixed-comparison.log`: all 26 checks pass across three alternating
  baseline/current pairs, with identical dependency hashes and fresh runtime
  directories. Normal parsing is used and tracing is disabled for the comparison.
  All workers preserve isolation/defaults and retire their exact file ACLs.

The baseline is the preserved v8 timer worker, SHA256
`7D123757A3A29B1FBA6883BBE350CB7AB96EA9D7A015D1D67318A8A2B2E4D54D`.
The measured simplified worker, before the two later canary/network trace calls,
is `7F7856DBBB46F79317CCABE8D86C2D106B25B07177F83DAC15357C5FA2A6DAD8`.

| Pair | Baseline wall ms | Current wall ms | Baseline Job peak bytes | Current Job peak bytes |
| --- | ---: | ---: | ---: | ---: |
| 0 | 2969 | 2492 | 33591296 | 32096256 |
| 1 | 2348 | 2361 | 33603584 | 32268288 |
| 2 | 2412 | 2379 | 33452032 | 32194560 |

The median native peak drops by 1,396,736 bytes (about 1.3 MiB) in this sample.
Startup ranges overlap; the 33-ms median difference does not establish a speed
improvement. Fresh file paths are not cold OS/CLR caches. This is a small local
comparison, not site-load, rendering, compression, smoothness or latency proof.

## Regression Gates

- `Lea-bootstrap-solution-build.log`: solution succeeds with 18 pre-existing
  warnings in unchanged prototype projects. Focused builds have no warnings/errors.
- `Lea-bootstrap-full-source-tests.log`: the normal full suite passes startup,
  worker/isolation and event regressions, then fails a fresh isolated realm in
  microtask tests at the startup deadline. That helper did not yet include `Sd`.
- `Lea-bootstrap-traced-full-source-tests.log`: fails its first fresh worker at
  5,004 ms wall / 390 ms CPU. The startup harness was overriding explicit tracing
  for that case, so no child checkpoints are retained in that failure.
- `Lea-bootstrap-trace-override-build.log`: the corrected harness builds without
  warnings/errors. `Lea-bootstrap-traced-startup-tests.log`: all 64 checks pass;
  first-path startup walls are 3,596 / 2,797 / 2,439 ms. The successful UI case
  has a 239-ms maximum tick gap; this is not a responsiveness SLA.

- `Lea-bootstrap-final-diagnostic-source-tests.log`: the normal full run again
  passes startup/worker/event regressions, then fails startup of a fresh microtask
  realm. The improved helper records hello phase, launch 127 ms, probe 8 ms,
  total 5,052 ms wall / 453 ms CPU, before the supplied 128-admission test code
  can execute. This is a startup failure, not evidence that admission bounds fail.
- `Lea-bootstrap-traced-micro-tests.log`: all 76 focused microtask checks pass
  with explicit tracing. This does not erase the normal full-run failure.
- `Lea-bootstrap-final-timer-tests.log`: all 165 focused timer checks pass,
  including native delivery, the 25-ms hard deadline, retirement and rendering.
- `Lea-bootstrap-final-view-tests.log`: all 58 native-view checks pass, including
  field synchronization, queue/reentry handling, navigation/disposal, dormancy
  and the remote-origin gate. `Lea-bootstrap-final-browser-build.log` builds the
  current standalone browser without warnings/errors.
- `Lea-bootstrap-command-guard-build.log` builds the final comparison argument
  guard without warnings/errors. `Lea-bootstrap-command-guard.log` confirms
  a missing baseline path is rejected explicitly rather than starting the full suite.

The expected complete source count is 624, including the eight new event
assertions, but a full run at that count has not passed. The full 43-case host
matrix, fresh extracted packages and independent source rebuild are not yet
verified. No new release ZIP is published from the failed full gate. Do not
transfer the earlier v7/616-check results to these gates or install over the
preserved live browser.

## Firespider Candidate

`Lea-bootstrap-v9-candidate-build.log` compiles the separate net48/optional-script
candidate at `dist/Firespider/Lea-146-bootstrap-v9`, with 67 source inputs.
Its `runtime/Firespider Internet 19.exe` SHA256 is
`6A24ACD1F0B94AB625015CEAF3257CF6032B735CBEEEDFE82F48E80DCDD02C4E`.
`Lea-bootstrap-v9-host-script.log` passes the scripting case, including timers,
split isolation, recovery, lifecycle and remote refusal. First demand is 6,090 ms
and split-load wait 5,534 ms; these aggregate multi-call host waits are not the
individual startup budget or evidence of a fast-start claim.

`Lea-bootstrap-v9-exe-startup.log` passes all six actual-EXE scenarios, with
responsive windows and no legacy engine loaded. Observed readiness ranges from
4,586 to 6,452 ms, not a latency SLA. The full 43-case Boot matrix is not repeated.
The installed EXE/Engine/Win hashes match their preserved pre-work values.
Neither the candidate nor the standalone browser is published as a release ZIP;
the source full gate still fails. Earlier archives and source copies are retained.

## Commands

```powershell
dotnet build Lea.sln -c Release --no-restore
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --events
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --bootstrap 'C:\baseline\ScriptWorker'
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe .
```

The comparison requires matching file inventories and identical dependency
hashes, except the worker EXE under comparison. Failed diagnostic runtimes remain
on disk; successful temporary runs delete only their own known copied files.
See [timer work](TIMERS.md) and [earlier startup diagnostics](STARTUP_DIAGNOSTICS.md).
