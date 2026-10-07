# Live Element Tree Navigation

The optional JavaScript bridge now exposes six read-only element properties:
`parentElement`, `firstElementChild`, `lastElementChild`,
`previousElementSibling`, `nextElementSibling`, and `childElementCount`.
The document exposes the two child endpoints and element-child count.

Reads follow Lea's live tree, skip text nodes, preserve wrapper identity, and
return null at element boundaries. Removed subtrees retain their internal links
but lose their former document ancestors; reading them does not grant a
detached mutation capability. Semantics follow the relevant
[ParentNode](https://dom.spec.whatwg.org/#interface-parentnode),
[NonDocumentTypeChildNode](https://dom.spec.whatwg.org/#interface-nondocumenttypechildnode),
and [parentElement](https://dom.spec.whatwg.org/#dom-node-parentelement) rules.
This is not a complete Node/Element prototype or collection implementation.

## Compact Bridge And Allocation

| Private Command | Read |
| --- | --- |
| `pe` | Parent element |
| `fc` / `lc` | First / last element child |
| `ps` / `ns` | Previous / next element sibling |
| `cc` | Element-child count |

The parent-owned `Ja` bridge reads the existing `N.Up/C` model. Only opaque
handles, null, or an integer cross the worker pipe; no CLR object, URL, network
permission, or mutation operation is added. Existing public embedding aliases,
protocol version 3, permission/lifecycle rules, 256-call and 512-handle bounds,
and worker isolation/deadlines are unchanged. Each list scan permits at most
8,192 visits, then fails with the existing limit result and realm retirement.

One private set of six getter functions is reused across every element wrapper
and the applicable document properties. Null-prototype descriptor templates
avoid inheriting page-added descriptor fields. Getters use their actual branded
receiver and captured lookup/apply intrinsics, not a captured per-node handle.
Tests verify function identity, read-only behavior, and receiver rejection. This
avoids allocating six new getter functions per wrapper; no measured peak-memory
or speed improvement is claimed.

## Verification

Logs in `L:/Lea Development` use `Lea-tree-followup-`, with two initial bootstrap
observations under `Lea-bootstrap-followup-`:

- `red.log` reproduces the exact missing-navigation assertion before the change.
- `trusted-final.log` passes all 23 trusted-only checks. This diagnostic option
  is explicitly separate from the combined isolated/native gate.
- `combined.log` fails AppContainer creation with real disk-full HRESULT
  `0x80070070`, before isolated page code executes. Its 22 trusted checks precede
  the later shared-getter assertion and are not counted as a combined pass.
- `combined-final.log` passes all 53 checks: 23 trusted, 23 isolated, and seven
  native-view checks. These include synthetic host child lists at 8,192/8,193
  visits, bridge exhaustion/recovery, detached subtrees, native field retention,
  real click/input handlers, and nonblank 940px/420px paint. Both captures are
  inspected; the Lea icon renders and the edited field fits without overlap.
- `scope.log` passes all 28 private-scope checks, including the new helper and
  descriptor-template names. Test and standalone browser builds succeed with
  zero warnings/errors.
- `full.log` fails worker bootstrap in `phase=hello`, 5,013 ms wall / 781 ms CPU,
  with `startup parsed` last. This is not a full-suite pass.
- A subsequent storage audit reports 1,027,768,320 free C: bytes rather than zero.
  `full-storage-recovered.log` then passes all **1,021** current-source checks,
  including the 53 new checks. That observed external-state change motivated
  a distinct validation run; no runtime retry or relaxed deadline was added.

The initial bootstrap profiler completes samples at 1,857/38 ms. The later
`profile-final.log` fails its first sample at 2,342 ms during event-target setup;
its warm sample completes at 119 ms and the aggregate correctly exits 1. These
instrumented observations are not a controlled comparison or a diagnosis of
native startup. The subsequent full pass does not erase either failed run.

## Size And Release Boundaries

Embedded `Dom.js` grows from 27,948 to 29,423 bytes; the worker EXE grows from
50,176 to 51,712 bytes. Current worker SHA256 is
`CAAA463E772703952FE39674A4E8284AD65FEFE3D2507FC8CEA35705F7E60E3F`.
The standalone browser and test runtimes match across all 19 dependencies;
the eight-file worker inventory is unchanged, and its other seven files match
the pre-feature baseline. No dependency or required renderer feature is added.

The installed EXE/Engine/Win remain unchanged. The earlier v18 host candidate
does not contain this feature, and its passes do not transfer to a new host
build. The source checkpoint above predates the separate v19 candidate and
expanded tests documented in [HOST_TREE_NAVIGATION.md](HOST_TREE_NAVIGATION.md).
No verified new release ZIP, extracted-package or archived host-source rebuild
gate is claimed. Broader readiness stays about 65%.
Cold-start stability, complete current host/package verification, document
origin/resource policy, DOM creation/removal, node collections, and wider web
compatibility remain work. Short aliases and compression are not speed proof.

## Commands

```powershell
$env:TEMP = 'L:\Lea Tool Cache\temp'
$env:TMP = $env:TEMP
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --tree
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --tree-view
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --tree-trusted
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe .
```
