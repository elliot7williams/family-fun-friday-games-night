# Compact exception constants

## Change

The shared readable DOM bootstrap now uses an ordered private list of the 25
legacy DOM exception constant names and one bounded loop to create their
descriptors. The descriptor map has a null prototype. Both existing bulk
`Object.defineProperties` calls are retained, so constructor and prototype
receive separate own properties with the same numeric values, order,
enumerability, and non-writable/non-configurable attributes as before.

The list and loop run during trusted bootstrap, before page frames are accepted.
No interface is deferred into page execution, exposed as a new global, renamed,
removed, or sent over the DOM bridge. Public web-platform names stay readable;
the embedding aliases are unchanged. The existing generator reparses and checks
the compact AST, fixed-point output and committed source/output hashes.

This is a verified size reduction, not a demonstrated startup optimization.
The loop introduces work too; fewer source bytes alone do not prove lower CPU,
memory use, smoother rendering, or faster page loading.

## Size

| Item | Before | After | Reduction |
| --- | ---: | ---: | ---: |
| Readable bootstrap bytes | 30,291 | 29,609 | 682 |
| Compact bootstrap bytes | 21,716 | 21,241 | 475 |
| Parent `Lea.Script.dll` bytes | 90,624 | 90,112 | 512 |
| Worker `Lea.Script.Worker.exe` bytes | 44,032 | 43,520 | 512 |

The compact resource is about 2.2% smaller than the preceding resource, not
2.2% smaller than the entire engine/package. There is no decompressor, new
runtime dependency, identifier mangling, or change to the pinned interpreter.

Source SHA256:
`412A74C4868AF09D3C8D1682FB869324A481F4E44864567116C93A966263E6B8`.
Compact SHA256:
`F107409057AD5C83CF3DCA6CB982B54E3FD08D65A7BC578BD4CC74D6F3FD38C9`.
Parent SHA256:
`F07258CB91AC967B48CB68FDEF237DD33DBBD9356BCC15F17642DC289B8FF5D5`.
Worker SHA256:
`7C479F20C417973C545C83F3EE42B0ABCC523E8F294E6CB53C095136E4295C24`.

## Verification

- Generator self-tests pass seven syntax round trips and their changed-literal
  control. Write/check agree with matching AST JSON and committed hashes.
- Parent, worker, test harness and standalone browser compile without warnings
  or errors. All four compiled-resource checks pass for the exact generated
  payload and unchanged resource identity.
- The focused class-list/exception/native suite passes 105 checks. New checks
  cover own-property order on both owners, rejected writes/deletion/redefinition,
  and zero document-bridge calls. Existing checks cover all 25 descriptor values,
  legacy codes, branding, subclassing, receiver errors and captured constructors.
- The ordinary full source run passes all 1,267 checks, without startup observation
  or an externally enabled timing trace. Individual existing startup fixtures
  still exercise their own trace modes. The final test executable is
  `174C28B7A4EA9D345880BB0533464F35255A6897308AA927B2081A941CE8EE86`.
- The standalone browser EXE smoke passes using all 19 byte-matched tested runtime
  files. It verifies inline startup, the native click/counter update, AppContainer
  use, no loopback exemption, and the memory limit. Its module report contains no
  MSHTML, Chromium/WebView/CEF or WebKit browser module. Desktop/narrow focused
  captures and the actual standalone-window capture are inspected and nonblank.
- Browser EXE: 405,504 bytes, SHA256
  `CAD5495CAD57EFF726C787E08C1826FA90C10114086C351D5C22B9BDD2B3216E`.

Only the parent DLL and worker EXE change among the 19 production dependencies.
The 5,000 ms native startup deadline, 128 MiB Job process-memory limit, CPU hard
cap, one-process Job, AppContainer/probes, restricted handles, mitigations, and
statement/memory/recursion/regex/array limits remain unchanged. The pinned
[Jint allocation constraint](https://github.com/sebastienros/jint/blob/v4.16.4/Jint/Constraints/MemoryLimitConstraint.cs)
continues to check allocation at each statement; the JavaScript table does not
replace or relax it.

## Fresh-worker comparison

The prior eight-file worker is frozen before editing. Three opposite-order pairs
use the same current parent/harness and byte-identical seven other worker files.
Each launch gets a fresh NTFS directory, tracing is disabled, and all six launches
pass: 27 checks including isolation/defaults, process exit and exact file-ACL
retirement. Successful temporary directories are removed by the existing harness;
there are no failed launches to replace or retry.

| Pair | Baseline wall ms | Current wall ms | Baseline CPU ms | Current CPU ms | Baseline peak bytes | Current peak bytes |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 0 | 3,197 | 2,540 | 468 | 234 | 32,010,240 | 32,186,368 |
| 1 | 2,557 | 2,536 | 312 | 171 | 32,145,408 | 32,092,160 |
| 2 | 2,363 | 2,473 | 78 | 234 | 32,219,136 | 32,022,528 |

Median startup wall is 2,557 versus 2,536 ms, with overlapping ranges. Three pairs
and uncontrolled OS/CLR caches/load do not establish a general speed or memory
improvement. Fresh paths are not controlled cold caches. These passing launches
do not explain or erase the earlier five-second startup failures.

## Preservation and next gate

Evidence is under `H:\Lea Evidence\Lea-146-exception-table-2026-10-08`, with
the old worker/source, exact final test/browser runtimes, results, source archive,
checkpoint bundle and final sealed inventory. Two external verification-command
errors are retained: slash/backslash comparison before any runtime test, and
PowerShell's automatic `$Matches` replacing a local comparison-analysis variable.
Both are corrected against the same bytes/logs without rerunning a build or test.

The test browser is at
`L:\Lea Development\Lea.Script.Browser\bin\Release\net462\Lea.Script.Browser.exe`.
Its dependencies must stay alongside it; this is a source-output test build, not
a standalone single-file distribution or installed Firespider replacement.

No new Firespider candidate, host-matrix/actual-EXE verification, release ZIP, or
installed replacement is claimed for these new bootstrap bytes. Earlier host
passes belong to their original runtimes. Original Firespider inputs and its
three installed binaries remain unchanged. Startup reliability and safe remote
scripting remain open; milestone 146 stays open and broader readiness stays
about 65%.
