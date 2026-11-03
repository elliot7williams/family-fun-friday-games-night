# Class Tokens and Selector Allocations

Milestone 146 remains experimental; broader browser readiness stays about 65%,
not a standards score. This follow-up fixes class-token separator disagreement
and reduces transient allocations without a per-node cache or new dependency.

## Implementation

Core class selectors and `[attribute~=word]` use a shared ASCII word scanner.
One through eight class conditions scan the raw attribute directly. Larger
compounds build a temporary case-insensitive hash set, preserving long and
repeated compounds without a fixed-width match mask. The selector compiler's
existing 512-character limit is unchanged. Dense matching still allocates.

The same scanner serves bounded core `ac/rc/tc` mutations. Optional JavaScript
`classList.contains/toggle` now split on exactly space, tab, line feed, form feed,
and carriage return. Form feed previously worked in script membership checks
but not core selectors/removal; NBSP and vertical tab were incorrectly split
by the script helper. These separators follow
[Infra ASCII whitespace](https://infra.spec.whatwg.org/#ascii-whitespace) and
[Selectors class and word matching](https://www.w3.org/TR/selectors-4/).

Raw `cn`/`className` writes still retain whitespace and duplicates. Existing
case-insensitive core matching, case-sensitive script membership, classList
32-token/64-character/512-unit mutation bounds, single-token operations, and
incomplete DOMTokenList behavior remain limitations. This is separator parity,
not full DOM or CSS conformance. No URL writers, generic attributes, new alias,
protocol revision, persistent cache, or interpreter dependency is added.

## Measurements

Run the focused benchmark after building `Lea.Bench` Release:

```powershell
.\Lea.Bench\bin\Release\Lea.Bench.exe -c -r . -n 5
```

The generated fixture has 256 six-class rows, child spans, six style rules and
one 32-class row (518 element nodes as reported by `P.N`). Query counts are
bounded `Qa` results; the `p` control reaches the existing 256-result cap.
Each case warms up three times, then
runs five batches of ten operations. Reports show median per-operation time,
calling-thread allocated bytes (not retained or all-thread memory), and Gen0
collections across the measured batches. Unsupported allocation counters show
`-1`. Load and class writes include the existing full document rebuild work.

Six serial controlled runs use baseline/candidate/candidate/baseline/baseline/
candidate ordering, identical benchmark/view/image binaries and fixture source,
and no concurrent test/build. Only the engine DLL differs. Values below are
medians of the three reported batch medians per variant, not a confidence bound.

| Case | Baseline ms | Candidate ms | Baseline bytes | Candidate bytes |
| --- | ---: | ---: | ---: | ---: |
| `p` control | 0.1404 | 0.1478 | 22,140 | 22,140 |
| `.alpha` | 1.2358 | 0.4997 | 234,828 | 22,416 |
| `.alpha.beta.gamma` | 1.2744 | 0.4636 | 236,080 | 23,668 |
| `.missing` | 1.6157 | 0.4501 | 235,200 | 20,408 |
| `.alpha.missing` | 1.1326 | 0.4437 | 235,720 | 20,928 |
| `[class~='gamma']` | 0.6711 | 0.3784 | 150,688 | 22,832 |
| `.alpha:has(> .omega)` | 1.4515 | 0.4739 | 313,972 | 30,904 |
| 32 distinct conditions | 1.2137 | 0.9187 | 255,500 | 184,844 |
| 80 repeated `.c0` conditions | 1.2015 | 1.0069 | 295,488 | 224,832 |
| Load | 122.0575 | 116.4544 | 27,929,989 | 25,637,543 |
| Class write | 116.5153 | 104.6191 | 26,714,338 | 24,422,951 |

Common class-query allocations decrease about 90%; this fixture's class-write
allocation decreases 8.6%. Individual timings vary: the uncontrolled first
candidate's 32-condition sample is slower than its first baseline. The tag-only
control is slightly slower in the repeated aggregate with unchanged allocation.
These measurements are not startup, rendering, modern-site, or browser-wide
speed claims. Compression and short alias spelling do not prove runtime speed.
The eight-condition threshold is a bounded heuristic, not a universal optimum.

Controlled benchmark EXE SHA256:
`92CF1F50086E1442CB7B19F94B313F6F302BB4CE904813A11A98B47F405FB436`.
Baseline engine:
`54BC7B3503BFE826CD9E10897A04C0B3D15E18E2E3B80119BD4AF6E3FDB123FE`.
Candidate engine:
`7C81BCF979F915975330FCDF1C4B2F5DA6C2CA781EDED40C190DC7EC65F575B1`.
Raw records are retained as `Lea-tokens-controlled-1-baseline.tsv` through
`Lea-tokens-controlled-6-candidate.tsv` under the temporary log directory, with
verified copies of this round's logs in
`dist/Bench/Lea-146-class-v12-evidence`.

## Verification

Before the fix, the scripting regression fails specifically with
`form-feed selectors` (`Lea-tokens-red-script.log`). The corrected focused core
token suite and all 83 focused script/native attribute checks pass. Core tests
cover all five separators, nonseparating Unicode/vertical whitespace, whole
words, empty/missing words, duplicates, 70 repeated/distinct conditions, and
1/8/9/32/64/70-condition direct/hash boundaries. Native checks retain field/editor
identity, edited text, styles and accessibility through form-feed removal.

The first core build catches a missing LINQ reference in the added test; the
test now follows the existing StringBuilder style. The first token test uses
560 characters for 80 `.active` conditions and correctly hits the existing
512-character rejection; its corrected 70-condition input is 490 characters.
Neither production bounds nor success criteria are relaxed.

The first full core run fails the delayed decoder's admission assertion:
3,057 ms, one killed helper, zero admitted images/worker-hardening flags. The
isolated `--pics` suite passes; the cause of that first failure is not established.
The normal full rerun passes the complete core suite, and all 713 source
scripting checks pass. Focused native 940/420-pixel screenshots are inspected.
The solution builds with zero errors and 18 existing prototype warnings; the
standalone browser build has zero errors/warnings. Separate v12 compiles from
67 host inputs and passes all 43 host cases and all six actual-EXE startup
scenarios. Readiness samples range from 4,539 to 6,517 ms; lazy-all is initially
deferred and resumes into Lea, with no legacy renderer. Script first-demand
and split waits are 5,584/5,063 ms, aggregate host waits rather than individual
worker startup measurements. Real 30-second recovery/hibernation timers run.

All 713 checks also pass from a fresh standalone ZIP. The extracted host runtime
passes its scripting case, and the host-source ZIP independently rebuilds with
Framework 4.8 MSBuild. Its 19 engine/script dependency records match the runtime
manifest; 348 fixture/license files are copied with path/length/hash verification
for the harness, and the rebuilt host passes scripting too. The full 43-case and
six-EXE matrices are not repeated on the extractions/rebuild.

The engine DLL grows 512 bytes to 419,840; the WinForms DLL remains 313,856
and the image worker 9,216, giving a 742,912-byte required bundle. The optional
script worker remains 35,328 bytes. Binary size is not compressed size or a
speed measurement. No timeout, CPU/memory budget or isolation limit is relaxed.
The installed Firespider browser remains untouched.

## Experimental Artifacts

Candidate EXE: `dist/Firespider/Lea-146-class-v12/runtime/Firespider Internet 19.exe`.
SHA256: `EEFE69D0FBEEE229BE90DA779406D2BA3FC739F92D22164B8C11C025A366B143`.

Three ZIPs pass exact inventory, file-length/SHA256, current-snapshot and fresh
extraction checks. These remain experimental, not a stable cold-start release.

| Kind | Files | ZIP bytes | Archive name |
| --- | ---: | ---: | --- |
| Standalone | 41 | 2,982,205 | `Lea-js-146-native_2026-10-04_05-14-53.zip` |
| Host runtime | 372 | 3,692,215 | `Firespider-Lea-146-script-runtime_2026-10-04_05-15-04.zip` |
| Host source | 104 | 3,342,858 | `Firespider-Lea-146-script-host-source_2026-10-04_05-15-04.zip` |

Standalone ZIP SHA256:
`BB77688FC3463E10A189DF7BA9F99A1CA561B11CA63E64256E45074E025D8DE9`.
Runtime ZIP SHA256:
`5689018B8A6B58DDD4694864A239A0DDA529B189A20063DCA286D41ECA1FF00C`.
Host-source ZIP SHA256:
`72A23198541E9F53CB595CF8EB1712E25F044B470E22981D8F1CF56F40035BDA`.

Standalone files are under `dist/LeaScript/Lea-146-class-v12`; host ZIPs are under
`dist/Firespider/Lea-146-class-v12-packages`. Extracted worker runtimes need an
ACL-capable local filesystem, not FAT32. Host-source contains buildable VB plus
compiled Lea dependencies; complete engine source is preserved separately by
`Backup-Lea.ps1` and Git. Earlier startup failures and this round's first decoder
admission failure remain unexplained. Broader DOM/site support, remote-origin
policy, full event-loop semantics and Lea-only host integration remain work.
