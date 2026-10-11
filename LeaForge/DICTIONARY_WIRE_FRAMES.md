# Dictionary Wire Frames

Milestone 146 remains open; broader readiness stays about 65%, an engineering
estimate rather than a conformance score. This change makes the optional script
runtime smaller and removes reflected members from its outgoing frame shapes.
It is not a demonstrated startup fix, speedup or memory improvement.

## Compact Factory

The shared private `Wire.Of` factory accepts one to eight key/value pairs with
the existing one-letter protocol keys. It rejects null/odd/oversized argument
lists, non-string/empty/unknown/long keys and duplicates. Values retain their
existing types. Each call returns a fresh ordinal dictionary, never cached state.

All five parent writes and four worker writes now use this factory instead of
anonymous objects. Microsoft's [serializer reference implementation](https://github.com/microsoft/referencesource/blob/main/System.Web.Extensions/Script/Serialization/JavaScriptSerializer.cs)
selects its dictionary path before its custom-object field/property reflection
path. No converter, hand-written JSON encoder, new package or shared serializer
is introduced. The existing JSON/UTF-8/framing code remains in use.

The compact factory removes anonymous frame types and their generated members
from both production assemblies. It adds dictionary/argument-array construction;
there is no claim of lower allocations without measurement. Dictionary storage
and interface dispatch still have costs. Avoiding reflected top-level getters
does not remove interpreter construction, parsing, JIT or all startup work.

The wire version remains 3, maximum frame 128 KiB, JSON recursion eight, maximum
root fields eight, bridge requests 256, page source 16 KiB, messages 512 characters
and the same typed sequences/cancellation flags. All handshake, denial, CPU-rate,
memory, process-count, mitigation, AppContainer and lifecycle protections remain.
`Be=0` is still the build default; the test browser remains experimental `Be=2`.
Trusted probe disposal still precedes hello. Normal frame tracing stays off.

## Compiled Verification

`Lea.Script.Tests.exe <repository> --wire-frames`, or `-wf`, passes 88 transport
assertions without launching a native worker. Sixty-four new checks use both real
production assemblies: factory shape, zero anonymous types, all nine compiled
write call sites, invalid factory inputs, eight-field admission, fresh maps and
nine message forms compared byte for byte with their preceding anonymous form.
Round trips cover integers, booleans, nulls, arrays, quotes, backslashes, control
characters and Unicode. Existing 24 parser/protocol checks also run, including
magic/version, UTF-8, frame/string/recursion limits and strict reply sequencing.

The long and short CLI spellings pass separately. The same final test tool rejects
the preceding Script DLL and, independently, the preceding worker EXE, at their
missing factory contract. Extra arguments are rejected before tests. These
controls exercise actual compiled code rather than source text searches.

All three changed projects compile without warnings/errors. The candidate also
passes 15 worker-entry, six prepared-entry, 71 controlled execution-report,
23 native command/recovery, 16 native startup-phase and seven native hard-deadline
and fresh-worker recovery checks. Controlled transports prove no native isolation
property; the separate native checks keep the original guards and deadlines.

The full suite includes the 64 new checks and passes **all 2,357 assertions**,
with actual exit zero, once using the frozen final tool and runtime. The two
preceding broad failures are retained. This is not a retry of their unchanged
binaries or evidence that their root causes have been identified.

The default `Be=0` worker also compiles cleanly and passes four compiled-entry,
88 transport and 23 native command/recovery checks. This control does not adopt
the prepared bootstrap as the default. Indirect and W32 modes are not newly run.

The standalone browser exits zero with the expected isolated counter mutation.
The copied .NET 4.8 Firespider matrix passes **all 43 cases**, plus 14 clipboard
and 12 observation contracts. Its pinned harness checks actual child exits and
one completion per case. The outer host wrapper has no EXE exit code of its own.
The script case covers first demand, state retention, timer/event behavior,
attributes, retained editors, error recovery, split isolation and disposal.

Four fresh captures are inspected: standalone 980x680, main/attribute host views
1382x736, and split view 1100x700. The edited field remains selected/retained,
its width updates correctly, and split counters remain independent (one/zero).
All owned processes retire. These successful gates do not prove that all earlier
intermittent startup timeouts are resolved, or that this version is faster.

The original installed .NET 4.6.1 host is not rebuilt, migrated or replaced;
its six actual-EXE startup scenarios are not newly verified. The renderer smoke
is not rerun, with required renderer bytes unchanged from its preceding pass.
Broader web compatibility, startup reliability, adoption and release verification
remain unfinished, so the active engine goal and milestone 146 stay open.

## Size And Preservation

Only two of the 19 production records change:

| File | Previous bytes | Current bytes | SHA256 |
| --- | ---: | ---: | --- |
| `Lea.Script.dll` | 97,792 | 93,696 | `ADCD3BAD65D64037ACA558963000B3B4803D947BAEE3F6E3ACEFF95BF26D25FC` |
| `ScriptWorker/Lea.Script.Worker.exe` | 50,176 | 46,592 | `FEA9C884B7869A35F6312CF9CAACB789FDD529CE9FC5700906DF41628BABA162` |

This is a measured 7,680-byte PE reduction. The required renderer bundle remains
749,056 bytes. Interpreter dependencies, compact DOM bytes and other runtime
records remain byte-identical. Package compression is a storage measurement,
not an execution-speed guarantee. Original/installed Firespider files and normal
production outputs are not modified by this checkpoint.

Prior failed runs, archives, source and binaries remain preserved. New evidence:
`H:/Lea Evidence/Lea-146-dictionary-frames-2026-10-11`.

The 24-file runtime-only ZIP compresses 8,264,444 tested raw bytes to 2,897,565
bytes. Every ZIP entry matches the frozen tested bytes, with tests/symbols
excluded. Keep the eight worker files together and extract to local NTFS.
Compression reduces storage/transfer size; it is not a runtime speed measurement.
The complete source is archived separately and checked entry by entry before
the Git checkpoint is sealed.

Experimental test browser:
`C:/Users/Elliot/AppData/Local/Lea/EngineBuilds/Lea-146-dictionary-frames-2026-10-11/Lea.Script.Browser.exe`.
