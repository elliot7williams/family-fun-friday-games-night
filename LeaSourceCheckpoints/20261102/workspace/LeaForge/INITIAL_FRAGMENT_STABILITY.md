# Lea Initial Fragment Stability

Milestone 82 / runtime 182 keeps an initial URL fragment aligned while the
first native layout settles. The feature stays inside `Lea.Win.V`; it does not
add a timer, request, DOM copy, browser runtime, or host-specific layout path.

## Behavior

After `Go`, `GoA`, `Fg`, Back, or Forward resolves a fragment, Lea retains the
current page and exact fragment as one temporary anchor. It reapplies that
target when layout can legitimately move:

- first paint after navigation
- host width or height changes
- host font changes
- named stylesheet or bounded DOM-command rebuilds
- completion of asynchronous intrinsic image decoding

Each placement reuses `Ft`, details reveal, the retained box list, and the
normal bounded scroll path. It performs no page request and creates no new
history entry. Responsive text and an image without authored dimensions can
therefore move before the requested target without leaving that target behind.

## Compact Diagnostics

The WinForms view exposes three short read-only aliases:

```text
Fa  initial fragment anchor is active
Fj  placement count, saturated at 255
Fy  latest target Y in document coordinates, or -1 when unresolved
```

These are host diagnostics, not controls. `Fj` and `Fy` retain the latest
navigation evidence after the anchor is released. A non-fragment document
navigation resets them to `0` and `-1`.

## Lifetime And Bounds

The anchor belongs to one `P` instance and one ordinal fragment string. It is
released by normal wheel or `Sc` scrolling, find/focus/editor scrolling, a
missing or unrendered target, a different fragment/page, and view disposal.
That gives user and host movement priority over automatic placement. There is
only one pending placement flag and one retained page reference; the counter
saturates at 255.

The target remains subject to Lea's existing document height, layout-box,
details-reveal, URL, and fragment bounds. `#` still moves to document top, and
a missing fragment preserves the previous viewport while reporting `Fy=-1`.
This is targeted initial-fragment stabilization, not a general scroll-anchoring
implementation for arbitrary page mutations.

## Image Settlement

Synchronous `V.Go` can present a page before its image worker finishes. The
completion path now invalidates layout after a decoded intrinsic size becomes
available, then reapplies an active fragment before repainting. A completed
image for a page that is no longer current does not dirty or reposition the
new page.

## Verification

`test-pages/fragment-stability.html` contains a responsive paragraph before
one target and an asynchronously decoded intrinsic image before another.
Repository tests cover first paint, width settlement, image settlement,
diagnostics, missing targets, reset, and manual-scroll release. The compiled
Firespider smoke verifies the aliases through the real VB host.

The Release engine, WinForms view, complete repository suite, compiled
Firespider host, all 42 Boot cases, and all six real EXE startup scenarios pass.
The report covers 78/78 executable probes over 75 fixtures. Its 15-run medians
are 528.623 ms clean diagnostic startup, 3.501 ms local load, and 34.481 ms
layout/paint. Retained managed memory is 19,046.1 KiB for 375 pages, or 50.8
KiB/page. Required runtime files total 554.5 KiB with a 236.1 KiB GZip
estimate. These are local comparison samples, not performance promises; see
`COMPATIBILITY_PERFORMANCE.md`.
