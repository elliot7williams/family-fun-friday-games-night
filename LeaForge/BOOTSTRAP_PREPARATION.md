# Bootstrap Preparation Investigation

Milestone 146 remains open and broader readiness stays about 65%. No production
startup optimization, startup-timeout fix or new browser release is claimed.

## Diagnostic

The test executable gains `--bootstrap-preparation`, with the short alias `-bp`:

```powershell
Lea.Script.Tests.exe . -bp direct direct.json
Lea.Script.Tests.exe . -bp parsed parsed.json
Lea.Script.Tests.exe . -bp analyzed analyzed.json
```

Each invocation measures one CLR process evaluating the actual embedded compact
bootstrap. Modes use direct evaluation, parse-only preparation, or preparation
with static analysis. A new JSON report records VM, preparation, parsed,
completion and validation boundaries, wall/CPU samples, source hash and limits.
Existing report files and missing parent directories are rejected before work.

The interpreter uses 20,000 statements, 8-MiB allocation, a five-second execution
limit, 64 recursion depth, 100-ms regex timeout, 65,536 maximum array size,
stack-overflow guarding and disabled CLR interop. Prepared modes explicitly
use interpreted regexes and their own 100-ms parser timeout. The ordinary trusted
one-second bootstrap and native five-second handshake are not changed.

This standalone diagnostic does not launch an AppContainer, enforce the native
Job CPU/memory settings, or simulate cold OS/CLR caches. Resource loading and
the test method's initial JIT are outside its timed region. It is not a native
worker, page-load, responsiveness or general performance benchmark.

Three grouped assertions per sample check the private dispatcher, one bootstrap
evaluation with no host calls, and bridge-free globals, descriptors, exception
bindings and shared non-constructible event methods. Validation is timed
separately from bootstrap completion.

## Evidence And Decision

Both diagnostic revisions retain six predeclared fresh-process samples in the
order direct, parsed, analyzed, analyzed, parsed, direct. Production dependencies
and the test binary are identical within each revision. The embedded resource
is 22,576 bytes, SHA256
`60B97515B42374D4C160010327E78985EC3A855B3FA909CD392BD6E4958FB3B7`.

Revision A uses prepared-parser defaults. Revision B makes parser regex settings
explicit and adds `-bp`. A's source, binary and reports remain preserved; B's
six samples pass all 18 grouped checks and exercise both CLI names. Three
negative controls reject an invalid strategy, existing report and missing
parent, without overwriting or creating reports.

| Final sample | VM ms | Parse/preparation ms | Execution ms | Total ms |
| --- | ---: | ---: | ---: | ---: |
| Direct 0 | 206 | 719 | 1051 | 1976 |
| Parsed 1 | 216 | 560 | 821 | 1597 |
| Analyzed 2 | 214 | 676 | 690 | 1580 |
| Analyzed 3 | 258 | 698 | 711 | 1667 |
| Parsed 4 | 202 | 544 | 785 | 1531 |
| Direct 5 | 209 | 538 | 786 | 1533 |

The final ranges overlap. The earlier analyzed samples total 1,657 and 2,388 ms,
also showing variability. This small experiment does not justify adopting
preparation as a startup fix. Production continues using its existing string
parser; the next investigation still needs evidence from an actual native
worker timeout, not a successful or differently instrumented CLR sample.

## Regression And Preservation

- The final test tool builds without warnings/errors. A dependency rebuild
  regenerates three assemblies despite unchanged source and informational
  versions. Those bytes are retained separately, not used for comparisons.
  Exact preceding production dependencies are restored for all tests, and
  the owned regenerated direct project outputs are restored after testing.
- The ordinary scripting suite completes all 1,391 checks once with exit code
  zero and the final test binary, without a startup observer or timing trace.
  Its original outer wrapper expects the wrong completion punctuation and
  reports a summary mismatch. A read-only handle attached to the already-running
  process confirms exit zero and the genuine complete summary. The wrapper
  failure and separate verification are retained; the suite is not rerun.
- All 19 production files match in eight checked roots. Native/trusted worker
  sources, original 67 Firespider inputs and three installed binaries remain
  unchanged. No candidate browser, package or installed replacement is made.
- Source, both diagnostic binaries, reports, logs, byte inventories, source
  backup and Git checkpoint are preserved under
  `H:/Lea Evidence/Lea-146-bootstrap-preparation-2026-10-09`.

The benchmark consumes the repository's pinned Jint 4.16.4. The exact upstream
[evaluation implementation](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Engine.cs)
and [preparation implementation](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Engine.Ast.cs)
describe the two entry paths. Prepared parser options have separate regex
defaults, documented in the pinned
[parsing options](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/ParsingOptions.cs)
and [preparation options](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/PreparationOptions.cs).
