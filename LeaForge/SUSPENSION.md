# Lea Tab Suspension

Milestone 62 later adds conservative native eligibility to the existing
optional hibernation policy through `Ud`; see `HIBERNATION.md`. Milestone 55
added native lazy restoration; see `LAZY_RESTORE.md`. The following records
the original milestone-54 suspension behavior and limits.

Milestone 54 adds native Lea suspension to Firespider without adding a browser
runtime or changing the short public navigation aliases. Use the matching
adapter, engine DLLs, and `Lea.Firespider/Suspension-host.patch` together. The
patch follows milestone 53 and changes BrowserFeatures3/4/5/6.vb; it is already
applied to the live host. Its application to the saved baseline reproduces all
four live source files, ignoring line endings.

## Lifecycle

- Suspend Or Resume Current Tab and background/group/overview suspension now
  replace a Lea view with a small Panel and Resume button. The old view and
  context menu are disposed, and pending document/image requests are canceled.
- Resume, selecting a paused tab, and Reload recreate Lea on the same TabPage.
  Reload's old hidden menu resource is now overridden. Home or an explicit
  address replaces the paused target without first loading the old URL.
- Supported saved targets are validated with the existing HTTP(S)/file URL
  bounds. Invalid targets leave the placeholder intact. Initial in-flight
  navigation retains its requested URL even before there is a document.
- Lea groups retain membership and coloring, support collapse/expand and
  suspension/resumption, and create a Lea fallback when all visible tabs hide.
  Intermediate selection changes while collapsing do not wake paused tabs.
- Automatic Lea session snapshots include owned hidden and paused tabs within
  existing limits. Overview reload/close and paused middle-close share the
  engine-aware lifecycle. Duplicate preserves Lea and the source private flag.

Resume creates a fresh engine context and reloads the URL. It does not snapshot
DOM/script state, cookies, back/forward history, scroll position, or unsent form
values. Do not suspend a tab whose unsaved state you need to retain. Automatic
hibernation eligibility and advanced lazy-session restoration are still
legacy-oriented and are not migrated or enabled for Lea by this milestone.

## Privacy

A private Lea view stays private when the global private flag is switched off;
new ordinary tabs use the ordinary context. Suspension carries the private
flag to the recreated view before its first request. Existing private views
are not silently downgraded by timer or selection callbacks. Embedders can
still explicitly set `V.Pv` themselves.

Paused private URLs are excluded from automatic Lea sessions and shared recent-
history close paths. This does not claim comprehensive privacy for every old
Firespider feature, group record, explicit export, or legacy recovery path.

## Engine Cleanup

`V.Dispose()` stops active requests and image loading. Async presentation checks
disposal before touching controls or delivering further callbacks. Local-file
and stylesheet continuations check engine cancellation before publishing a
page. Navigation and download entry points reject disposed views. Disposing
inside Changed/Al stops further load notifications; repeated Dispose/St is safe.

The view now notifies native UI Automation on WM_DESTROY to release the provider
event map, and refuses to return its provider after disposal begins. Without
this notification, the suspension weak-reference regression retained the view;
with it, the old renderer is collectible. This follows Microsoft's
[provider cleanup contract](https://learn.microsoft.com/en-us/windows/win32/api/uiautomationcoreapi/nf-uiautomationcoreapi-uiareturnrawelementprovider).
The managed wrapper rejects a null provider, so cleanup uses a private native
call. No new external dependency is added: UIAutomationCore is a Windows API.

## Verification

`Test-FirespiderBoot.ps1 -Case suspend` checks disposal and collection, canceled
pending navigation, private state, same-tab resume, selection, Reload/Home,
replacement navigation, invalid targets, duplicate, and paused close cleanup.
`-Case groups` exercises the saved-group startup timer twice, hidden ownership,
session retention, collapse/expand, resume, overview operations, and privacy.
Both assert that their Lea-only paths do not load MSHTML.

The suite has sixteen cases. Core tests cover local/HTTP/CSS/image cancellation,
disposed callback reentry, rejected navigation, and no late image-cache writes.
The full host smoke checks the sticky private-view behavior while retaining an
explicit embedder cookie-mode reset. The real EXE script checks clean,
restored, locked, and oversized sessions and observes for at least eight seconds.
The newly reported startup popup was not reproduced in those four scenarios;
the earlier `tk` invalid cast remains guarded in the source and tested directly.

The compatibility report remains 57/57. This is lifecycle hardening, not a claim
of full modern-web support or a general JavaScript VM. ZIP/GZip measurements
describe storage size, not execution speed. Garbage collection is used only in
the regression test, never forced during ordinary tab suspension.

All sixteen Boot cases, full engine and compiled-host smokes, and four real
EXE startup scenarios passed for this build. Local diagnostic measurements:
471.0 KiB combined engine DLLs; 198.3 KiB GZip estimate; 3.646 ms median fixture
load and 34.650 ms median layout/bitmap paint. The standalone EXE became
observable to UI Automation in 4.154-6.688 seconds in those four checks. These
are different timing boundaries, not matched before/after performance claims.
