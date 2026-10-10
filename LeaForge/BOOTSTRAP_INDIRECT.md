# Indirect Bootstrap Investigation

Milestone 146 stays open. Broader browser readiness remains about 65%.
This is a test-only experiment, not a production startup fix or release.

A subsequent [native worker experiment](NATIVE_BOOTSTRAP_ENTRY.md) adds
opt-in `Be=1` and real isolated-worker evidence. The observations below remain
the preceding test-only checkpoint; default production entry is unchanged.

## Entry And Scope

The existing bootstrap diagnostic accepts `indirect` as its fourth strategy:

```powershell
Lea.Script.Tests.exe "L:\Lea Development" -bp indirect "H:\new-report.json"
```

The report must be new and its parent directory must already exist. No files
are overwritten. `direct`, `parsed` and `analyzed` remain available.

Indirect mode calls `Engine.Invoke(vm.Intrinsics.Eval, Undefined, source)`.
It does not look up a replaceable page-global eval, use reflection to enter
the interpreter, introduce a new parser, modify the bootstrap, or change
production startup. The source still begins with its strict-mode directive
and keeps bridge captures inside its existing private block.

The pinned Jint entry is constrained. Unlike ordinary script evaluation,
indirect eval includes parsing inside that entry's budget. This difference
must be considered before adopting it in the one-second trusted session.
This diagnostic still uses five seconds, 20,000 statements, 8 MiB allocation,
recursion 64, 100-ms regex timeout, arrays 65,536, stack guarding and disabled
CLR access. It does not launch a native AppContainer or enforce Job policies.

Jint does not emit `BeforeEvaluate` for indirect eval. The report now names
that absence explicitly and adds `scriptEvaluationCallbacks`. The legacy
`bootstrapEvaluations` field remains a callback count, not an invocation
count; its zero value for indirect mode does not mean bootstrap was skipped.
No artificial post-parse checkpoint is reported. Production tracing and
its existing parsed marker remain unchanged.

## Retained Samples

Four fresh processes run sequentially once each, without retries, on H:.
All use the same compiled test tool and identical runtime dependencies.

| Entry | Completion wall ms | Completion CPU ms | Checks |
| --- | ---: | ---: | ---: |
| Direct | 2,647 | 234 | 4 pass |
| Indirect | 2,413 | 203 | 4 pass |
| Parsed | 3,134 | 375 | 4 pass |
| Analyzed | 2,470 | 406 | 4 pass |

These are individual CLR diagnostic observations, not statistically reliable
performance estimates. Resource reading and initial method JIT are outside
the timed region. Validation follows the completion sample. There is no
cold-cache claim, native handshake comparison, page-load measurement or
proof that the alternative avoids the actual startup failure.

Each mode checks a callable private dispatcher, zero host calls, its expected
script-evaluation callback count, bridge-free globals, descriptors, exception
metadata, shared non-constructible event methods and private helper isolation
against globals, eval and Function. The latter privacy probe is new for all
four modes. These checks do not replace the DOM mutation and native security
test suites.

The embedded source remains 24,766 bytes, SHA256
`E0CE6586B3A718D065650236CB282E2EB6E866CBB0CC8463C55C3A2AC348716C`.
Four resource checks and 14 options/compiled-constructor checks also pass.
Three negative controls reject an invalid strategy, existing report and
missing parent, with actual exit code one and no report overwrite/creation.

## Decision And Preservation

Only tests and documentation change. All 19 runtime files are verified against
the preceding execution-report candidate. Normal outputs, 67 original
Firespider inputs, three installed files and the 379-file host basis remain
unchanged. No user files are deleted and no new browser executable is delivered.

Native startup, the full scripting suite, renderer and browser/host gates are
not rerun for this test-only change. The preceding broad failure after 175
assertions, startup wall 5,021 ms / CPU 343 ms with last checkpoint VM, remains
open. Existing passing browser/host samples are not presented as new results.
C: still has approximately 60 MB free at the start of this investigation.
This evidence does not establish that disk space caused the startup failure.

Production stays on `Engine.Evaluate`. An actual native-worker comparison,
constraint preservation and equivalent private-closure/bridge behavior are
required before changing its entry. No startup fix, speedup, milestone
completion or readiness increase is claimed.

Source backup, compiled test tool, all reports, logs and byte inventories are
preserved under `H:/Lea Evidence/Lea-146-bootstrap-indirect-2026-10-10`.

The pinned upstream [engine entry](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Engine.cs)
and [indirect eval implementation](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Native/Function/EvalFunction.cs)
describe the constrained invocation, global environment choice and absence
of the script-evaluation debugger callback. They inform this experiment;
they do not establish a startup-failure cause in Lea.
