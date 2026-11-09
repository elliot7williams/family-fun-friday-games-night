# Exact Script Parent Source Recovery

2026-11-09. Source/reproducibility milestone, not a startup fix or release.

The restored GitHub checkout contains the previously missing mature SDK14.cs:
LeaSourceCheckpoints/20261102/mature-sdk/Lea.Script/14.cs. Its SHA256 is exactly
CE6F3CCA760E5227243DBFB3279FBDF1AA571D23073500A0E9F68791377588B8,
the value in original parent build receipt D32732C0. Raw GitHub readback at commit
5fbf9d3 also matches. It supplies original net48/version/company/product/title/
configuration/informational metadata; no behavioral source was substituted.

All15original parent files now match their original hashes. Recovery proof:
I:/LeaBuildStorage/signed-font-startup-investigation-20261108-v1/
parent-source-complete-20261109-v1.json SHA
74C7A0C634DD9D64ED34BA67C8F652A720B68EE9B4E72360DC8B16FC31F58D02.
Earlier14/15reports remain historical evidence, superseded by this completion.

## Exact Rebuild

Build-RecoveredScriptParent.ps1 reconstructs a fresh source project on I: using
all15original files, original csproj/Directory.Build.targets from source manifest
B5A02838, exact original9A968D02core reference and six dependency DLLs, and exact
private-binding bootstrap196BCE96/A228494E. Original installed dotnet/MSBuild hashes
are pinned against the old build receipt. No profiler, guessed assembly metadata,
constant folding or native-window experiment is incorporated.

Original120s/5s ownership/deadlines unchanged. Compile8041ms/retire116ms, exit0,
Job0/drained/no kill/timeout, all input hashes stable. Owner67174 guarded exit0.
Output SHA256 matches the known parent exactly:
406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F.

Project: I:/LeaBuildStorage/recovered-script-parent-build-20261109-v1/
source/Lea.Script/Lea.Script.csproj. Component dependencies are staged locally;
this is not a complete engine SDK or portable dependency/toolchain distribution.
Receipt result.json in the build directory SHA
0039F8C17DBC82B4E3FB1586D30B65748CEA529BCB7576E7C7B2B49AA2A1275F.

Independent I:/LeaBuildStorage/recovered-script-parent-audit-20261109-v1/audit.json
SHA6C86D17230DDB201EF3D04F263A8922758889F1685E2B3580A26AF468343525B
checks all input hashes and ownership, rechecks raw remote metadata, compares
every DLL byte with the existing406C runtime, and copies/readback-verifies raw
build/recovery receipts and logs. Parent DLL101376bytes is literally identical.

This proves recovery and reproducibility of that component, not repair of the
C5CC/4EE7 full-suite bootstrap failure. No runtime was executed in this rebuild,
no browser/worker binary changed, and no release gate or overall percentage closed.
Prior diagnostic builds used explicit replacement metadata; their artifacts and
bounded results are unchanged, not retroactively rebuilt or requalified.

## Source Archive And Verified Round Trip

Lea.Script-recovered-source.zip in recovered-script-parent-source-package-
20261109-v1 on I: contains19source/project/resource/targets entries, each readback
verified. SHA D759DFFA618D37A31943DD80733C9A96DF6C057B2B6016B33AA390A57282D793;
package proof0E6AB3F0. Dependency hashes are recorded, binaries/toolchain excluded.
This archive is not a complete SDK or ready-to-browse browser package.

The successful direct-build recipe is preserved byte-exact as build.ps1 beside
its original build receipt: SHA5B35128AECB889F25EF1DB3539D0FD5F75D92070FE0C69E07159D1635F1F6267.
It is a historical recipe snapshot requiring its original helper paths and fresh
output conditions for replay, not a self-contained installer. The current wrapper
adds -FromSourceArchive; a fresh extraction subsequently rebuilt successfully.

The fresh-extraction attempt stopped at the original600MiB C: reserve BEFORE
launch or output creation. Observed free627695616bytes; owner10792 guarded exit0,
no test child started. Record20808823 at source-package directory's
rebuild-preflight-20261109-v1.json. This stopped attempt remains retained.
After space recovered above the unchanged reserve, fresh archive extraction
compiled7383ms/retire109ms, exit0/Job0/drained, stable inputs, no kill/timeout;
owner33671 retired0. Output is byte-identical406C. Receipt at
I:/LeaBuildStorage/recovered-script-parent-package-rebuild-20261109-v1/result.json
SHA7ED30B07F6974F515218343F484D4B774FAC6834931BC670EA7B4426E140D487.
Independent recovered-script-parent-package-audit-20261109-v1/audit.json SHA
80E9CC01EFA1006737BD549EA28A308AA00C405272393782D4CD52E48E93F61F
rechecks extraction, inputs, ownership and literal bytes. No runtime qualification.

## Related Scheduling Control

CurrentSchedulingPolicy.cs uses read-only self process/thread queries. The owned
control matched original process HANDLE identity/birth, then sampled twice around
100ms sleep: priority class32, process/thread memory priority5, successful power
queries with all masks0. No priority/power changes. Compile2929/retire167ms,
control656/retire104ms, both exit0/Job0/drained, owner81334 guarded exit0.
Receipt I:/LeaBuildStorage/owned-scheduling-policy-20261109-v1/result.json SHA
6FAA0177DDCD59E72A9880E38287326F51274E2EB474063731DFE233B4633A9E.
This is not a measurement of the earlier interpreter failure or every enclosing
job, and does not establish the timing cause or justify changing limits.
