# Compact bootstrap checkpoint

This is an experimental size improvement, not a new verified Firespider
release or a fix for intermittent startup deadlines. Milestone 146 remains
open; broader lightweight-browser readiness stays about 65%.

## What changed

The readable `Lea.Script/Dom.js` is unchanged. A developer-only .NET 8 tool
uses pinned Acornima.Extras 1.7.0 to print its AST in compact form, reparse it,
compare the default AST JSON, and verify fixed-point generation. No identifier
mangling, expression optimizer, runtime decompressor, language rewrite, or new
runtime dependency is involved. Function source strings and source positions
reflect compact text, so structural equality is not a claim of every possible
observable equivalence. Runtime regressions remain necessary.

Both scripting runtimes embed the generated file under `Lea.Script.Dom.js`.
Stock MSBuild SHA-256 checks reject stale source or compact output. Ordinary
Visual Studio builds do not require running the generator. Readable source,
generated output, hashes, tool, lock file, and line-ending rules are preserved.
See `../Lea.Build/Bootstrap/README.md` for regeneration commands.
The generator also offers `-c`, `-w`, and `-t` check/write/test aliases.

| Item | Readable checkpoint | Compact checkpoint |
| --- | ---: | ---: |
| Bootstrap UTF-8 bytes, CRLF | 29,423 | 21,105 |
| `Lea.Script.dll` bytes | 97,792 | 89,600 |
| `Lea.Script.Worker.exe` bytes | 51,712 | 43,520 |

The resource is 8,318 bytes / 28.3% smaller; each PE shrinks 8,192 bytes.
This is not a 28% reduction in the entire engine, runtime, package, or memory.
Jint, the other dependencies, isolation, and execution limits are unchanged.

Source SHA-256: `FD62D9D552069C2D64509AC5FA989C96B4901679DA010A05441FF23DB8826C76`.
Compact SHA-256: `E3ADF17D3977D8A9EDF88AC4F0353409683F506E980B1317B8D80A0DD764DFC1`.
Parent DLL: `839628B8BDF405BD3E2B1D4CC38C57A220ABFB5521B222029736BF03FB5F879A`.
Worker EXE: `7C946C43ACF1396C4C345787D5D4EBD5682A284F9C8F11F44565A29DAB01A3AA`.

## Verification and failures

- Generator build and locked restore pass. Seven syntax round trips and a
  changed-literal control pass. `--write` and `--check` agree byte-for-byte.
- Four MSBuild expected-hash override negatives fail as required, covering
  source and compact guards in both projects. Three separate stale-file
  fixtures fail `--check` as required, including comment-only source changes.
- Four new compiled-resource checks pass. Both compiled payloads exactly
  match the generated file; no extra readable resource is embedded.
- Bootstrap privacy passes 28 checks; profile error-reporting passes eight.
- Separate tree, class-list, event, and microtask runs pass 55, 99, 136, and
  76 checks, including their trusted/isolated and applicable native coverage.
- The planned 1,051-check full source run fails at the first isolated tree
  execution. Parent timing: launch 129 ms, probe 34 ms, wall 5,005 ms, CPU
  312 ms; the last captured child marker is `startup evaluate` at 3,807 ms.
  This is not a full pass. Earlier full passes belong to earlier checkpoints.
- The separate timer run fails its first in-process script with `limit` /
  `Script budget exceeded`. The preserved readable checkpoint independently
  fails the same first script check. No retry was substituted for either
  failure; this does not establish the cause or prove timer compatibility.

The default `--class-profile` now explicitly reports only embedded bootstrap
VM/parse/completion timing. Supplying readable `Dom.js` retains statement
markers where its anchors match. Missing anchors are no longer printed as
though they were injected. Profiling is diagnostic, not the production gate.

## Fresh-worker comparison

`--bootstrap` makes three opposite-order pairs in fresh NTFS directories,
using the same parent and byte-matched seven other worker files. All six
launches pass, including original limits, verified isolation, worker exit,
and exact ACL retirement: 27 checks. The earlier worker source and linked
code are unchanged between its recorded e437f4e build and ff4bea2; SDK revision
metadata differs as well as this generated resource. This is not a controlled
cold OS-cache experiment or a whole-browser benchmark.

| Pair | Order | Readable ready wall/CPU ms | Compact ready wall/CPU ms |
| --- | --- | ---: | ---: |
| 0 | readable, compact | 4,166 / 578 | 3,020 / 406 |
| 1 | compact, readable | 4,069 / 578 | 2,842 / 515 |
| 2 | readable, compact | 2,667 / 234 | 2,826 / 546 |

The third pair is slower with compact text. These few variable samples do
not establish a speedup, memory improvement, or reliable cold-start fix.

## Loader evidence

The opt-in `--runtime-load` harness uses the real restricted `Rp.Open`
launcher and direct .NET Framework test EXE. Two native samples and one
trusted sample pass on the preserved readable checkpoint. Both ready workers
and parent map the same seven .NET native images: mscorlib, System,
System.Core, System.Configuration, System.Numerics, System.Web, and System.Xml.
This provides no support for a blanket native-image access problem in those
observed workers. It does not describe a previously failed worker or prove
the timeout cause. Early snapshots precede CLR initialization, and native
module enumeration is not a complete managed-assembly inventory. Observation
changes startup work; these are not benchmark samples. No NGen installation,
framework ACL change, or Windows setting change was made.

An earlier PowerShell probe used an incompatible runtime and failed a
.NET Framework-only File ACL API. It is retained as a probe setup failure,
not a Lea compatibility result. The direct-EXE harness avoids that host issue.

## Boundaries

The standalone browser builds with the compact runtime. Its packer now
includes the missing tree fixture and bootstrap test assets separately from
the embedded payload. Source backups also include `.gitattributes` so newline
rules survive outside Git. Packaging checks are not a full package-suite pass.

Experimental package:
`L:/Lea Builds/Lea-146-compact-bootstrap/Lea-js-146-native_2026-10-07_21-26-35.zip`,
2,979,196 bytes, SHA-256
`993FF51F819E214EE39F8EC529FF87693AFA7BB6A13F1CD4C22F687B7342E8E0`.
All 45 payload files plus the manifest pass exact inventory, byte-length,
and SHA-256 checks in the stage, ZIP streams, and fresh extraction.
The extracted EXE separately passes four resource, 28 privacy, and eight
native startup-phase checks. No extracted full-suite pass is claimed.
The initial package-script invocation is retained as an execution-policy
setup failure; the workspace script subsequently ran with a process-local
policy override, without changing the machine's policy.

No new Firespider candidate is built for this resource change, no corrected
43-case host matrix or extracted full-suite pass is claimed, and the installed
browser is unchanged. The prior v21 startup passes do not transfer to this
checkpoint. Remaining work includes reliable startup under the existing
limits, full source/package/host verification, broader DOM/CSS/web-platform
coverage, and explicit integration/release gates before replacing installed
files.
