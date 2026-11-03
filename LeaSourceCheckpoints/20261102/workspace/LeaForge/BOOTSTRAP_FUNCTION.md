# Rejected Bootstrap Function Experiment

Updated2026-11-02, base535add5/dirty codex/lea-engine-foundation. REJECTED as a
startup optimization; NOT adopted. Current candidate remains global-install
parentF5BDB1FF/workerA72EF9C0. No worker built for this experiment.

## Hypothesis And Source

The fixed bootstrap's private top-level block creates many lexical/function
bindings. A single strict IIFE might use a more efficient interpreter function
initialization path. This was a hypothesis, not a measured cause of the timeout.
One IIFE returns the same private dispatcher inside the SAME host Evaluate call;
no warmup, realm reuse, extra Evaluate or reset-budget chunks. Public API bodies
remain unchanged. Original1000ms/20000statement/8MiB/64recursion policy unchanged;
an extra bootstrap call frame remains subject to those limits.

Source `I:/LeaBuildStorage/bootstrap-function-source-20261102-v1/source.json`:
`8BE6CCF6D976ED680FE2D5507D440FAB663B712B30B0EFE8A608F170B47BBA22`.
Readable/compact transformations have exact body inverse and10rejected malformed/
duplicate/already-projected controls. `Prepare-BootstrapFunctionCandidate.ps1`
does not modify the prior source/candidate or earlier prototypes.

Node full-bootstrap mock-bridge equivalence PASS268 (67x4), retained as
`node-equivalence-test.json` beside source. Tests dispatcher shape, checkpoint order,
private-helper nonexposure, containment/branding, classList, DOMException constants,
performance, timer/frame callbacks and load dispatch, with paired mock bridge logs.
This is NOT Jint/native/sandbox/performance qualification. The first mock attempt
failed on the ORIGINAL baseline's bare performance accessor in a Node VM context.
The harness now invokes that getter with explicit globalThis receiver; no engine
source changed for that mock-host issue. Actual Lea performance getters were not
requalified here. Test source hash680FC52E... is recorded in the Node receipt.

## Build And Fixed Comparison

Separate parent built under original120s/5s limits, owner74697 guardedexit0.
Build `I:/LeaBuildStorage/bootstrap-function-build-20261102-v1/result.json`:
`2597084C56DE45AA9DB11132091E02EE5F3C1D84890D11EE779F553CA2DD5D93`.
DLL `62B5656A41720E124A915E36E437C95337E06259985EC10923CA9E987AE7E17E`.
Host8.cs evaluation,10.cs policy and2.cs checkpoints match previous source exactly.

One preplanned ABBA comparison, identical harness/dependencies/fixture except script
DLL. Baseline F5BDB1FF's existing ten-file NTFS closure reverified/read-only reused;
only new candidate closure staged.600MiB plus actual new closure AND4MiB headroom
preflight retained; no copied-baseline staging charged or reserve reduction.

| Observation | Result | Caller Wall Ms | Process CPU Ms |
| --- | --- | --- | --- |
| baseline1 | PASS | 2790 | 234.375 |
| candidate2 | FAILED bootstrap/document TimeoutException | 2918 | 390.625 |
| candidate3 | PASS | 2934 | 343.75 |
| baseline4 | PASS | 2711 | 468.75 |

Receipt `continuity-evidence/bootstrap-function-comparison-v1/result.json`:
`92627174E37475AE86DCA8C96514766BF012076C0CE34A8A45C2CD6B4F315FD9`.
Campaign completed but FAILED. Both candidate observations exceeded baseline caller
wall observations, but this small sample does not prove a causal timing regression.
No useful gain demonstrated; candidate failed the startup gate, so it is rejected.
No repeated attempts beyond the preplanned four, no ClassLists retry, no adoption.

Compiler3372ms/retire113ms; attempts3835/4413/4079/3873ms, retire43/32/42/28ms.
All5jobs0/drained/no kill/outertimeout, candidate2exit1/others0,20runtime bytes+ACL
and inputs stable, owner75166 guardedexit0. Original120s/30s/5s boundaries.
Audit `368DEF7B83798E89A638C4C892DD97681EA6236D1C6BCEDD81A4E7C6C58DE329`
at `I:/LeaBuildStorage/bootstrap-function-comparison-audit-20261102-v1/audit.json`
checks exact outcomes/raw reports/paired closure identities, unchanged host policy,
Node mock-test source identity, current bytes/ACL and copied evidence readback.

## Next

Do not continue worker/host qualification for this rejected wrapper. Investigate
reducing eager private allocations instead: list-state and exception WeakMaps may
be initialized only on first use, provided constructor/intrinsic capture, invalid
receiver behavior, proxy avoidance, limits and all observable descriptors remain
unchanged. This is a NEW unimplemented hypothesis, not a performance claim.
Current candidate's ClassLists/bootstrap failure and all6release gates remain OPEN.
