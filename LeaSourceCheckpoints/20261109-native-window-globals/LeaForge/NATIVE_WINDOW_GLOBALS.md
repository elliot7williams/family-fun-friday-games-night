# Native Window Global Installer Experiment

2026-11-09. Separate experimental parent, NOT adopted. Current fullv5 remains
failed and the packaged browser is unchanged. No speed or size improvement claim.

## Change

WindowGlobals.cs supplies a per-realm, owning-thread, one-shot Gd installer.
The existing six timer/frame JavaScript functions are passed unchanged to the
temporary __wg helper. It validates every function and realm before any writes,
then installs writable/enumerable/configurable data descriptors in the original
order. The bootstrap deletes the helper immediately. No new public short command,
CLR bridge, callback body, page-script policy or worker was introduced.

Prepare-NativeWindowGlobals.cjs uses pinned Acorn from the existing Terser5.44.1
lockfile, installed on I: with lifecycle scripts disabled and cache/temp on I:.
It parses and validates the exact six descriptors, extracts original function
source slices without rewriting them, and replaces one installation statement.
Eight malformed/missing/duplicate/already-projected controls and exact inverses
pass. Compact JS saves438bytes/readable558bytes, but the rebuilt parent DLL is
102912bytes versus102400baseline:512bytes larger. Source shortening is not a
binary size or performance gain.

Source proof I:/LeaBuildStorage/native-window-globals-source-20261109-v1/source.json
SHA6E5B5FD89DCE987831D6BD0D190A27AC1EF76CE625537333F46BFC63579378E4.
Node mock312behavior checks and two public descriptor/name/arity comparisons
PASS33905C1E52585A4A17CB7E718B5D89AF873215B26CAE02F217B990EA0B4A01E5.
Node is not proof of the C# installer or Jint performance.

## Actual Jint Contract And Fixed Comparison

Candidate39595C95EC3D1226736AADF71F6E38327C0ECDF3451D57F327E099BBFD12D0A7
uses the recovered14parent sources with explicit diagnostic replacement metadata,
the same corrected C5CC core/dependencies, unchanged1000ms/20000statement/8MiB/
64recursion/regex/array/interop policy, and the buffered profiler. Historical14.cs
is still unrecovered. FoldConstants remainsfalse. Only the host binding, resource
installation and new installer differ from matched diagnostic baseline C56FB88B.

Actual compiled Gd passes19contract checks: null/double binding, short/long args,
invalid final function without partial writes, foreign realm, six function
identities, public metadata/order, duplicate installation, wrong-thread rejection
and retirement, helper removal and no CLR exposure. These are targeted checks,
not full scheduling, sandbox or browser qualification.

| Fresh process | Caller total ms | Preparation ms | Evaluation ms |
| --- | ---: | ---: | ---: |
| Baseline1 | 2039 | 824.78 | 693.41 |
| Native2 | 2548 | 1028.41 | 949.93 |
| Native3 | 1776 | 666.32 | 694.50 |
| Baseline4 | 1756 | 703.88 | 599.10 |

Four containment probes complete; fixed ABBA, no retries. Mean caller baseline
1897.5ms/native2162ms. No dependable speed improvement; not recommended for adoption.
Only two observations per arm, observer costs, and OS cache/scheduling variation
remain limits. Do not attribute differences to native callback JIT or any other
cause from this experiment alone.

Three compiles, one contract and four probes exit0/Job0/drain/no timeout/kill.
All22runtime hashes/SDDL and inputs stable. Original owner83730 guarded exit0.
Receipt350F8F2F14AD50699BD4437098BE734E6717D766AD1CA7D114ABEB89E1AE9E1A
at I:/LeaBuildStorage/native-window-globals-abba-20261109-v1/result.json.
Audit38C9699E0E47A9C9D3EE719A64A3B93E3ECF34D0D8BE8CB12152B99E5CB9D9DB
in native-window-globals-audit-20261109-v1 verifies raw contract footer, all60
ordered profile markers, source/host boundaries, fixed order, unchanged runtime
and ownership, plus copied raw evidence readback.

Do not carry this into the worker, Firespider or full-suite candidate without a
demonstrated benefit and broader matched qualification. Preserve sources/results;
do not repeat this unchanged experiment to obtain a better timing sample.
