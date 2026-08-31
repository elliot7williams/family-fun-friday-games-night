# Lea Split View And Navigation Guard

Post-1.0 milestone 58 replaces Firespider's two legacy split-view controls with
independent Lea views. The real Side-By-Side Tab View command uses the same
source selectors and scroll toggle. Changing a source disposes the previous
view and cancels its pending work. Original tabs and unsent fields are untouched.

## Source And Lifetime Rules

Sources are snapshots of supported visible tab URLs, including suspended and
explicit legacy tabs. Hidden groups and unsupported URLs are excluded. Selection
visits at most 256 tabs, retains at most 64 KiB of UTF-8 URLs, and keeps the
existing 4,096-character normalized URL bound. Labels are limited to 256
characters. Only the two selected sources load; suspended originals stay paused.

Each pane starts a fresh engine context rather than sharing its source's DOM,
form values, cookies, cache, or back/forward history. Private source flags are
retained, and an initiating private window makes its new panes private. Later
navigation respects the owner's current private flag without downgrading a
previously private pane. Switching to an ordinary source creates a fresh ordinary
context when the owner is no longer private.

Loads are asynchronous. Scroll synchronization operates in either direction and
records actual clamped positions to avoid oscillation. Loading panes are skipped.
Closing the split window or its owning browser disposes both views, the scroll
timer, and the comparison window; pending requests are cancelled and later
callbacks cannot update closed UI. Split URLs do not enter browser tab/session
lists. Download links use an explicit Save dialog and Lea's bounded downloader;
they do not create automatic browser download-history records.

## Compact Navigation Guard

`E.Ng` and `V.Ng` accept an optional `Func<Uri, bool>`. A true result allows an
explicit document navigation; false raises `Er` with `K="blocked"` and `Rt=false`
before fetching or mutating history. Null retains the previous behavior. The
guard covers GET, POST/multipart, reload, retained fragments, and Back/Forward.
It runs synchronously on the caller's thread, before asynchronous page work, so
hosts should keep it short and must not re-enter navigation from the callback.
Callback exceptions propagate without starting that navigation. A disposed view
releases its policy, and disposal within the callback cannot authorize a request.

```csharp
view.Ng = url => AllowedByHost(url);
await view.GoA(address);
```

Split view uses this guard to consult existing blocked-site/content-navigation
rules, including direct navigation, links, script requests, and form submission.
The guard is not a network firewall or sandbox: HTTP redirects, subresources,
and direct downloads are outside its engine-level scope. Split downloads consult
the host before showing their Save dialog. Milestone 60 also installs the policy
on ordinary Firespider Lea tabs; see `NAVIGATION_POLICY.md` for their ownership,
offline/lock, resume, and input-error handling.

## In-Memory Comparison

Text Differences now opens an owned, read-only two-column tool instead of writing
a temporary HTML file and opening a legacy tab. Repeated comparisons replace the
previous result window. Text is treated literally, not parsed as HTML.

Comparison uses whitespace-normalized, case-insensitive unique paint-ready text
blocks, including table cells. Hidden/transparent blocks and live form editors
are excluded. This is not a line-level edit diff or a formatting comparison.
Each side visits at most 4,096 blocks, retains at most 2,048 distinct blocks and
just under 64 KiB of UTF-8 text, and processes at most 4,096 characters per block.
Surrogate boundaries and control characters are handled before comparison. Each
output shows at most 150 unmatched blocks; limited results are visibly labelled.

## Evidence And Limits

The previous runtime failed the new test because its split panes were legacy
controls. Three additions bring the Boot suite to 27 cases:

- `split`: actual command, fresh private/ordinary views, original fields,
  bidirectional/off/clamped scrolling, literal comparison and table/visibility
  handling, link/form/script rejection with zero blocked loopback requests,
  allowed requests, source replacement, small-window geometry, and disposal.
- `split-life`: hidden/invalid/deferred sources, replacement during a slow load,
  private upgrades and fresh-context reset, session isolation, and owner-close
  cancellation of an active page request plus timer/comparison disposal.
- `split-limits`: 150-result and 2,048-block bounds, UTF-8/line/control limits,
  256 source candidates, 64 KiB source URLs, untouched unselected placeholders,
  and empty-source handling.

Engine tests cover guard aliases, denied sync/async GET/POST, fragments, history,
reload/cache, caller-thread execution, retained state, callback failure, and
disposal during authorization. The benchmark adds a navigation-guard probe for
58 total. Full engine/host suites and six actual-EXE startup scenarios accompany
the build. Desktop and minimum-size split screenshots were inspected.

One initial full-suite run reported a Lea-to-WebBrowser UI-thread cast in the
recovery case. At that point the harness retained only exception messages in
its failure text, so the callback was not identified. Nine isolated repeats
then passed. The harness now includes full exception stacks; the intermittent
cast remains an unresolved host issue, not a fixed defect or a proven effect
of the new split view. Do not interpret passing reruns as proof it is gone.

`Split-view-host.patch` follows `Domain-commands-host.patch`; it removes the old
split classes/handlers from `BrowserFeatures2.vb` and redirects the command.
The matching targets file now compiles `FirespiderSplit.vb` alongside the adapter.
Both files must be retained when embedding the integration in Visual Studio.

Legacy post-load DOM injectors for user CSS, annotations, permission mutations,
and form restoration are not ported by this change. Split pages use Lea's own
supported HTML/CSS/forms behavior. Pop-up/target-window parity,
redirects/resources, other DOM tools, and automatic
hibernation remain future work. OS download dialogs are not automated. The
reported popup's exact callback has not been identified by this milestone.
Compression and short names do not establish runtime speed gains.
