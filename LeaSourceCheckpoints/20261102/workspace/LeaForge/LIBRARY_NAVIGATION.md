# History And Bookmark Navigation

2026-10-26. Focused development fix, not full changed-host qualification.

The mature Firespider history form f6/o4 and bookmark form F7/p1 cast the
selected tab's control to System.Windows.Forms.WebBrowser. Lea tabs contain
Lea.Win.V, so the cast throws and the existing empty catch silently discards
the selection. Baseline EXE96154915 reproduces this: the real history handler
never reaches the selected Lea view's original blocked-site policy.

Each handler now forwards CStr(list.SelectedItem) to the existing j.q1 route.
This retains current-tab dispatch, address/alias normalization, empty-input
handling, paused-tab handling, navigation policy and legacy fallback. No new
tab, browser engine, network permission or remote-script capability is added.
Only one line per form changes; inverse checks recover the exact source.

Source: continuity-evidence/library-navigation-v1/host-source/6.vb and7.vb.
The matching116-file host source retains the earlier About fixes too.
New EXE SHA256:
638F46BD41B791001861E1AEB50DE9A047277F4BAE475702E903E0EF4CA3A5D5.
Other product engine/view/parent/adapter/worker binaries are unchanged.

## Verification

Source/harness inverse and negative controls:9 PASS, receipt
BB2CAA33CC1258BB006DDA30279915DC56BE7822FF5E0B56283E0163FEE95196.

Real-host regression receipt:
3739C0B1979EC72622E8030CBB49FAF50824B912DB2C2B24C14DD76D53F38021
at continuity-evidence/library-navigation-v1/result.json.
Original baseline fails the specific history policy-routing assertion, exit1/
15870ms. Changed host passes, exit0/5765ms. No outer timeout or kill.
The harness uses the real default main form and actual list handlers, checking
history AND bookmarks, empty selection, original blocked-site decisions,
blocked error state, allowed local-file loads and selected-tab/view identity.
External requests are denied regardless of the observed policy decision.
Settings are isolated/restored by the retained host harness; no updater runs.
Inputs and positive-run runtime bytes/ACLs held, owner40064 retired0 with zero
leases/no unverified child. This is functional host verification, not a visual
redesign or screenshot claim.

## Remaining

After user reboot, new638 EXE passes its OWN15-stage campaign:
C2731D13019074BDB7E82353DDE0B1DDC11D47FFF1AA58ABA8D1A157468008FA,
host-library-current-native-v1. All six actual EXE startup scenarios pass, plus
settings/eight host cases. Script subset28824ms. No timeout/kill; inputs/bytes
held; one decoder ACL delta accepted by unchanged policy. Owner15864 retired0
safely. This does not transfer old961 evidence or establish cold reliability.

Separate development ZIP Firespider-Lea-library-development-20261026.zip,
7464503bytes/398runtimefiles, SHA256
A87429089BCC81724C133355EADD689E91D8ABF00B547D9C154FE2CC33AD1D17.
ZIP hashes verified. Fresh extraction passes settings lookup, navigation and
actual clean EXE startup3/3, receipt
D16DD1E9E3E263F636C1D34DA2B864A3292FB16797FE5121782CF1B3FFA3B591.
Inputs/bytes/ACLs unchanged, no timeout/kill, owner67452 retired0 safely. Current
machine only, not all15 extracted or clean-machine install. ZIP note conservatively
predates these smokes and makes no extracted-package qualification claim.
Source checkpoint A1515801 retains matching116-file host source and prior
dependencies by references. ZIP is runtime-only, not a complete portable SDK.

Fresh four-route About regression on638, broader legacy tools, full extraction
campaign and clean-machine tests remain open. Do not replace installed/default
browser or L: build. Old ZIP A3B64730 retained. Full scripting startup failures
remain independent and unresolved; no release gate or percentage promotion.
