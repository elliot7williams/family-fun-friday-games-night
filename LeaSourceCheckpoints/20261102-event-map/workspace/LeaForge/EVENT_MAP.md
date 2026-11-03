# Deferred Event Target Map Experiment

Updated 2026-11-02. Experimental, not adopted. The better-qualified candidate
remains global-install F5BDB1FF/A72EF9C0. All six release gates remain open.

## Change

Prepare-EventMapCandidate.ps1 derives from the lazy-map candidate, not the rejected
IIFE. Window and document retain private direct event state. Element state uses
a captured-constructor WeakMap created on first wrapped element. The four lookup
sites retain branded access without touching properties on untrusted receivers.
Public descriptors, bridge operations, dispatch algorithms, host budgets and
evaluation/checkpoint code are unchanged. Exact inverse and 12 malformed controls
cover readable and compact projections.

Source: I:/LeaBuildStorage/event-map-source-20261102-v1/source.json,
SHA256 FE6451DEA2C77AF80A4D43020D5F6CEE8B7EFE47C24C32AB61ABF070F4CFC0F3.
EventMapProjection.test.cjs passes 320 checks across both forms and their original
sources. Mock startup maps decrease 1 to 0; both end at 3 after element, token-list
and exception use. Guards reject invalid/proxy receivers without traps before any
map exists; replacing public WeakMap/get/set/Reflect.apply before first element
does not break operations. Direct document/window events, capture/bubble ordering,
once/removal/passive behaviour, handlers and element lookup paths are exercised.

The initial mock attempt failed on an unchanged-source Node VM global setter
receiver artifact. The test now invokes that setter explicitly with null, using
the existing receiver normalization; no engine workaround was added. This mock
does not qualify Jint allocation counts, native window setters or performance.

## Build And Native Failure

Build receipt I:/LeaBuildStorage/event-map-build-20261102-v1/result.json,
SHA256 4EB46F1448E476A466B8DE01A179DF4EFDBA7DF8738DC5E00EE67878935F4202.
Parent DLL B245644C86BC706F1F54CB896B138419A7D9CECD9150F525E0F7790BF5C01A2E.
Compile7115ms/retire137ms, exit0/Job0/drained; original owner29118 guarded exit0.
Host8.cs, policy10.cs and checkpoint2.cs exactly match the mature SDK input.

Fresh trusted campaign runs FirstUse before a separate original ClassLists process.
FirstUse FAILED before page assertions: limit/phase=bootstrap/TimeoutException/
stage=elements. ClassLists was NOT launched. No retry, warmup, realm reuse, budget
increase or matching isolated worker. Compilation alone is not qualification.

Receipt continuity-evidence/event-map-trusted-v1/result.json,
SHA256 D8D271F5B2BE4CCEAFD0859CAA53F8862ED2E962FD7F8551EF112EDDF6C980E8.
Compile3887/run3762ms; retire139/12ms; exit0/1, Job0/drained/no kill or outer timeout.
13 runtime hashes and ACLs unchanged, inputs unchanged, owner11185 guarded exit0.
Audit I:/LeaBuildStorage/event-map-audit-20261102-v1/audit.json,
SHA256 14A064575EC948D51B6AE7E4B13C0CC0F65CE8AF2D307C795E00D2A00A1D734F,
checks source/mock/build/failure identity, unchanged policy, owned retirement and
current bytes/ACLs, plus copied evidence readback. candidateQualified=false.

## Storage And Next Step

Two explicitly selected batches relocated 20 older source ZIPs, 17275119 bytes,
to I: after SHA256/length/readback and before source removal. Relocation manifests:
event-map-source-zips-20261102-v1 and event-probe-source-zips-20261102-v1.
Working source/current builds/raw failures were untouched. Old archive-path
auditors require reconstruction from these manifests, not waived checks.

The experiment does not demonstrate a startup fix or a speed improvement.
Do not adopt it or retry unchanged FirstUse/ClassLists to get a passing sample.
Next investigate the trusted bootstrap wall-time versus CPU discrepancy with
bounded diagnostic observations, distinguishing current-thread CPU, process CPU,
page faults and I/O where available. No cause is established by the older timing
gap or this failure, and diagnostic observations cannot replace release gates.
