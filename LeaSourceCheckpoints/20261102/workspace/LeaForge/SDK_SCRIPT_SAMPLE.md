# Native JavaScript SDK Consumer Sample

2026-11-01. A separate WinForms consumer now demonstrates the mature engine/view
and source-built native JavaScript parent/adapter. This is focused embedding
evidence, not full SDK qualification or Firespider migration.

## Source And Deployment

- Lea.Sdk/ScriptEmbeddingSample.cs: address/Go/local-scripting controls and
  actual adapter lifecycle. No Chromium/WebView. Existing short API aliases used.
- ScriptEmbeddingSample.csproj: net48/C#12, relative LeaSdkBin references,
  framework dependency copies, six worker copies, ScriptHost.config EXE AppConfig.
- script-embedding.html: local title/DOM mutation and interactive update button.
- Lea.Build/Test-ScriptEmbeddingSample.ps1: pinned mature libraries/native worker,
  actual project build on I:, fresh hash-verified NTFS runtime, owned120s/30s/5s
  limits, original input holds and exact worker ACL before/after checks.

The tested project tree is
I:/LeaBuildStorage/script-embedding-sample-build-20261101-v4/Sample.
It is beside bin/Release and can be opened in Visual Studio with the net48
targeting pack and C#12-capable compiler. Source project references those built
libraries; it does not silently replace/rebuild mature library source.

Runnable tested EXE:
continuity-evidence/sdk-script-embedding-v4/Lea.ScriptEmbeddingSample.exe
SHA25661D21FE11EAA2FAD84F7A7C10A7EFEB4F53CA6DCFDE46C3669B716DA51CC71D3.
Keep its sibling DLLs, EXE.config, HTML and six-file ScriptWorker subdirectory.
Do not run the I: exFAT worker layout as an ACL-qualified runtime.

Parent906E3CAA...,adapter1C3425A5...,core5B8A501D...,viewA622A1C2... and worker
F8BE1887... match the pinned source-build products. Parent and worker Jint/
Acornima libraries are distinct. No changes to mature libraries, former sample,
existing aggregate package or Firespider binary.

## Permission And Origin Boundary

Default view.Je=false AND no optional adapter. Toggle on attaches W and enables
permission, then reloads local HTML. Toggle off disables permission then detaches,
retiring the native worker. Detaching W alone leaves the existing core's legacy
script subset available; the sample's initial versions exposed this mistake.
LocalFile guard rejects HTTP/HTTPS/UNC before access and disallows mapped-network
drive types. This is a local embedding sample, not remote browsing support.
Pure HTTP/HTTPS/UNC rejection cases tested without contacting a server; no mapped
network drive was created or runtime-tested. Worker APIs/budgets unchanged.

## Exact Evidence

V1 compiled, failed script-off title check:
D8FB2F2C5C2A01F65F13A0DEA359E185D7B639DA62AE388AF10535098D336C37.
V2 unchanged condition with diagnostic: actual title Lea ready while adapter off,
F42DA3AF69CD7785863E53A24BC5D7E1F1FDD7654FC0A21FF2B16960D1FF9C9C.
V3 fixes view.Je and passes16:
CB0771FC1B27F50A8E7027673687925B6D28009CB257EC332959C01611544362.
All failures retained; no assertion waived, deadline extended or test isolated
less strictly. V4 adds origin guard and full-window screenshot; new project/run.

V4 receipt I:/LeaBuildStorage/script-embedding-sample-build-20261101-v4/result.json
SHA256F6356225C78B5113177DFA50CC71336D2030150AD0427EFC534AF38130EF4187.
Project6994ms/120s, sample5231ms/30s; both0/no timeout/kill,Job0/outputdrained,
retirement119/77ms within5s, input bytes unchanged, exact worker ACLs restored,
original owner retired0. PASS LEA SDK SCRIPT SAMPLE17 covers default permission
off, origin rejection, initial document/no session, opt-in activation/title/DOM,
real isolated worker/flags/original resource caps, original worker handle alive,
consumer API mutation, nonblank pixels, opt-out permission+detachment, original
worker exit, supplied-code denial and ACL restoration. Counts overlap existing
adapter/worker coverage and must not be summed into release completeness.

script-sample.png and script-sample-window.png retained on C:. Full-window image
inspected: controls/rendered content visible, no overlap. Not cross-DPI, all-
viewport, native click-dispatch, crash-recovery or clean-PC qualification.

## Remaining Work

Integrate into a NEW aggregate source package with scripts/native worker source,
deployment validation and notices. Keep earlier aggregate source/binary evidence
separate. Full mature-parent regression/startup diagnosis, broad compatibility,
legacy UIA and all six release gates remain open. Rejected I/O prototypes remain
rejected; no performance claim, remote scripting or new Firespider release.
