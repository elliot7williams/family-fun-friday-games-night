# Trusted Bootstrap Cache

This retains the trusted-cache checkpoint. Subsequent
[native preparation work](NATIVE_PREPARED_BOOTSTRAP.md) keeps this Script DLL
unchanged and moves test delivery to a persistent folder after temporary
dependency files disappear.

Milestone 146 remains open. Broader browser readiness stays about 65%, an
engineering estimate rather than a web-platform conformance score. This is a
trusted-runtime optimization with focused isolation evidence, not a native
startup fix, default worker adoption or release.

## Shared Syntax, Separate Realms

`Js` now lazily prepares the fixed embedded bootstrap once per process and
evaluates that same syntax tree for each new interpreter. The holder uses
`Lazy<Prepared<Acornima.Ast.Script>>`; ordinary lazy publication serializes its
initialization. Loading metadata or leaving scripting disabled does not force
preparation. The private implementation introduces no public command or alias.
Existing short `Js.Ex/Ev/Tk` and the host bridge stay unchanged.

Only the trusted asset is shared. Engines, JavaScript globals, bridge functions,
document wrappers, model state, timers and dispatch closures belong to each
session. Page scripts still use normal `vm.Execute(code)`, retain their 16-KiB
input bound and are not cached in this process-wide holder. Reset creates a
new realm from the shared syntax; disposal drops the interpreter and dispatch.

Preparation explicitly selects static analysis, `FoldConstants=false`,
interpreted regexes and the same 100-ms regex timeout. Referenced-global
collection stays off. Jint documents prepared scripts as reusable and
thread-safe in its pinned
[preparation implementation](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Engine.Ast.cs).
The additional shared tree is retained for process lifetime; this is not a
claim of lower first-session heap use. The disposed-interpreter collection
test instead checks that the shared tree does not retain retired realms.

The trusted execution budget remains 1,000 ms. Parsing was already outside
Jint's constrained execution entry; preparation now also performs static
analysis there. A passing execution-budget check alone therefore does not
prove a faster total first call. The timed gate includes preparation and
session initialization, and no preparatory contract runs before that gate.
Preparation exceptions derive from `JintException` in the pinned assembly and
remain covered by the existing bounded error/reset path.

## Frozen Candidate

The 19-file candidate changes only root `Lea.Script.dll`, from 96,256 to
96,768 bytes. Its SHA256 is
`ECC4EF4E0DF8123C8B1B2770E393F31D4E901CAEB92191AEBA5A77404F25BC82`.
The required Engine/Win/Image bundle remains 749,056 bytes. Neither
dependency packages nor the readable/generated bootstrap are changed.

The compact asset remains 24,766 bytes, SHA256
`E0CE6586B3A718D065650236CB282E2EB6E866CBB0CC8463C55C3A2AC348716C`.
This change does not equate compression or short names with execution speed.

All eight deployed worker files remain byte-identical to the preceding native
candidate. That retained experimental worker uses `Be=1`; source default
builds still use `Be=0`. No prepared-tree change is made to the worker, and
the two experimental choices are not conflated. The worker's parent 5,000-ms
startup deadline, interpreter limits, 128-MiB Job, one-process restriction,
25% CPU-rate hard cap, low zero-capability AppContainer, canary/network checks,
three inherited pipes and verified-hello-before-page ordering stay unchanged.
Remote/external scripts remain disabled.

## Checks And Timing

Each declared gate runs once in its own fresh process. Native gates run
sequentially from NTFS, without an external debugger or startup observer.

| Gate | Result |
| --- | --- |
| Candidate default first call | Pass, one check; six changes and title `Lea 7` |
| Same compiled first-call test with previous Script DLL | Pass, one check |
| Cache compiled-call/options/strict-tree contract | Pass, five checks |
| Same compiled cache contract with previous Script DLL | Expected rejection at the missing lazy holder |
| Cache state, reset, concurrent sessions and collection | Pass, 14 checks |
| Interpreter options | Pass, 14 checks |
| Embedded assets | Pass, four checks |
| Existing combined trusted/native scope suite | Pass, 36 checks |
| Native execution-report deadline/recovery | Pass, seven checks |
| Broad scripting suite | Fail after 180 passing assertions |
| Standalone browser | Pass, actual exit zero |
| Copied Firespider scripting case | Pass; helper verifies child exit zero |

The cache-state gate proves identical bootstrap AST identity across realms,
independent titles/node tokens/globals/bridges, collision-resistant private
closures, budget reset, replacement-realm isolation, two concurrent owning
threads and collection of a disposed default interpreter. Its AST-observation
factory is used only in that functional gate, not the default first-call gate.
The existing scope suite additionally exercises hidden helpers through eval
and Function, captured intrinsics, microtasks and global string timers.

| Fresh first-call sample | Session wall/CPU ms | Whole-process wall/CPU ms |
| --- | ---: | ---: |
| Cached candidate | 2,689 / 234 | 5,665 / 359.375 |
| Previous Script DLL | 2,946 / 343 | 6,307 / 546.875 |

Session timing begins after document/script extraction; method JIT and page
loading are not a complete cold-OS benchmark. Whole-process timing includes
launch overhead. One pair, run candidate then control, is insufficient for a
speedup or intermittent-failure claim. Reuse avoids a new parse per realm by
construction; these samples do not quantify long-run performance or heap use.

## Remaining Failure

The broad suite passes its early trusted runtime checks but stops in the
native timing-enabled startup checkpoint gate:

```text
phase worker preserves verified isolation: timing=True
Worker startup deadline exceeded. phase=hello
launch=221; probe=17; wall=5016; cpu=250 ms
last checkpoint: startup evaluate; wall=460; cpu=78 ms
```

It does not pass the complete native/security/DOM matrix. The failed attempt
and earlier trusted/native failures are retained; there is no retry or
warmup-to-green run. Separate passing native and browser/host checks do not
replace it. C: has usable headroom again, but disk space is not established
as the failure's cause.

The browser and copied-host main, attributes and split captures are freshly
inspected. The attribute capture has a narrow viewport and unused black area;
it is not full attribute-layout evidence. The full host matrix and renderer
smoke are not rerun. The original .NET 4.6.1 Firespider host is not rebuilt,
migrated or installed, and no fix to its installed EXE is claimed.

## Reproduction And Preservation

Run these diagnostics separately to preserve fresh-process lazy-init checks:

```powershell
$tests = 'C:\Users\Elliot\AppData\Local\Temp\Lea-146-trusted-bootstrap-cache-2026-10-10\Lea.Script.Tests.exe'
& $tests 'L:\Lea Development' --bootstrap-cache-cold
& $tests 'L:\Lea Development' --bootstrap-cache-contract
& $tests 'L:\Lea Development' --bootstrap-cache
```

The experimental browser is beside that tool as `Lea.Script.Browser.exe`.
Normal output binaries, 67 original Firespider inputs, three installed files
and the 379-file host basis are verified unchanged. No earlier delivered
browser or source is deleted. The compiled old-runtime controls, candidate,
all reports/failures, captures, verified source ZIP and Git checkpoint are
retained under
`H:/Lea Evidence/Lea-146-trusted-bootstrap-cache-2026-10-10`.

The next reliability work must address native first-start latency and pass
the broad suite and required host gates before adoption. Full modern-site
compatibility remains outside the completed scoped 1.0 baseline.
