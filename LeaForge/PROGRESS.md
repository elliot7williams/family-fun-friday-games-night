# Lea 1.0 Progress

Current completion: **100%**

Post-1.0 hardening milestones completed: **78**

This measures progress toward Lea 1.0: a small dependable custom engine that can
replace Firespider's legacy browser control for supported pages. It does not
claim compatibility with every feature in Chrome, Safari, Firefox, or the full
modern web platform.

| Area | Weight | Earned | Current state |
| --- | ---: | ---: | --- |
| Core and alias API | 10% | 10% | Compact API, bounded diagnostics, repeatable reports, history, projects, backups |
| Networking | 10% | 10% | Bounded request deadlines and downloads, private cookies, typed errors, validators, compression, memory LRU and optional persistent cache |
| HTML and document tree | 20% | 20% | Bounded tokens, queries, base URLs and fragments, raw/RCDATA text, entities, recovery, HTML-hidden suppression, semantic accessibility, blocks, lists, disclosures, gauges, values, time/edit metadata, responsive image maps, tabindex focus, autofocus/access keys, tables |
| CSS | 15% | 15% | Compiled relationship, bounded strict relational/functional, An+B/reverse, and filtered structural selectors, declarations with two-tier important cascade, inherited custom properties and pixel-width media queries, fixed/percentage/calculated widths and definite heights, bounded vertical sticky positioning, dynamic hover/focus, inherited cursor/pointer interaction, text transformation and signed letter/word spacing, content/border sizing, circular/elliptical radii, bounded outlines, box shadows, gradients, bitmap backgrounds, object fitting and aspect ratios, visibility/opacity, fonts, overflow, boxes, bounded ordered flex and two-axis grid properties |
| Layout and painting | 20% | 20% | Shared fixed/percentage/calculated width and definite-height, content/border, aspect-ratio, and sticky geometry, transformed and spacing-aware presentation text with raw form retention, normalized rounded backgrounds/borders/outlines/overflow/fields/shadows/gradients/bitmaps, pointer-gated retained hits with underlying fallthrough, fitted raster images and maps, ordered flex rows/columns, bounded grid tracks/spans, distribution, alignment, wrapping, grow/shrink, clipping, reflow, painting |
| Resources, forms, and media | 10% | 10% | Native field edit commands and one-level undo/redo, bounded multipart files, richer widgets, validation, responsive and fitted images, GET/POST submit |
| Lea scripting | 5% | 5% | Host-controlled navigation, safe clicks, variables, text/title/body and ID-scoped display/width/height/style/alert, class, visibility, disclosure, field-state, focus, and literal bounded DOM translation |
| Firespider integration and reliability | 10% | 10% | Lea-first New Tab/Ctrl+T, explicit legacy fallback, Inspector with Access view, MSAA/UIA surfaces including read-only text, host-safe focus shortcuts, navigation, sessions, source, host smokes |
| **Total** | **100%** | **100%** | Scoped Lea 1.0 baseline complete; post-1.0 hardening continues |

## Updating the percentage

- Increase progress only when code is compiled and a fixture or smoke test passes.
- Do not award completion for plans alone.
- Keep the weights stable so the percentage remains meaningful.
- Update this file after every completed engine milestone.

## Completed post-1.0 hardening

- Added bounded percentage and single-expression calculated heights through
  compact `S.Hp/Ho`, `Hnp/Hno`, and `Hxp/Hxo` tuples plus `ht(id,value)`,
  readable `height(id,value)`, and literal `element.style.height`. Definite
  viewport, parent, positioned, flex, and grid references now feed one shared
  height resolver across blocks, tables, controls, images, aspect ratios,
  content/border boxes, paint, hits, editing, accessibility, and live resize.
  Percentages below auto-height parents remain natural instead of guessing a
  circular reference. Invalid declarations are atomic; plain authored
  four-digit pixel heights preserve Lea's legacy 2000px safety cap while DOM
  commands remain strict. Firespider startup now automatically restores an
  unclean session without a blocking recovery dialog, and the retired
  `fsin.glitch.me` default falls back to the bundled Lea start page. The EXE
  harness uses bounded native tab/Resume checks and inspects only separate
  top-level dialogs, avoiding menu-label false positives and startup stalls.
  Full repository and compiled-host smokes pass, along with 74 probes over 71
  fixtures, all 42 Boot cases, and all six real EXE scenarios. Measured medians
  are 578.142 ms clean diagnostic start, 3.413 ms local load, and 35.418 ms
  layout/paint. Retained managed memory is 17,177.5 KiB for 355 pages, or 48.4
  KiB/page. Required runtime files total 548.5 KiB with a 233.3 KiB GZip
  estimate. Runtime 178 is isolated; installed runtime 157 remains unchanged.
  See `CALCULATED_HEIGHTS.md` and `Startup-smooth-host.patch`. The scoped
  baseline stays at 100%, not full-web compatibility.

- Added bounded vertical `position:sticky` through compact `S.Pn=k`. Sticky
  boxes keep their normal-flow slot, establish retained positioning/stacking
  context ownership, follow signed pixel `top`/`bottom` root-scroll insets, move
  descendants as one integer-offset subtree, and stop at the nearest retained
  parent content edge. Overflow clips are rebuilt from moved owners, so paint,
  links, pointer hits, native editors, keyboard focus, and accessibility use the
  same geometry. Pages without an active vertical sticky inset retain the old
  scroll fast path. Extreme `Sc` deltas now use a bounded 64-bit intermediate
  instead of wrapping. Full engine/WinForms, compiled Firespider, and extracted
  runtime tests pass. The benchmark passes 73 probes over 70 fixtures. All 42
  Boot cases pass, including the real 35-second recovery timer without the old
  cast dialog, and all six actual EXE startup scenarios are responsive,
  dialog-free, and MSHTML-free. Measured medians are 494.192 ms clean-process,
  3.432 ms local-load, and 33.163 ms layout/paint. Retained managed memory is
  16,694.9 KiB for 350 pages, or 47.7 KiB/page. Required runtime files total
  546.5 KiB with a 232.5 KiB GZip estimate. Runtime 177 is isolated;
  installed runtime 157 remains unchanged. See `STICKY_POSITIONING.md`. The
  scoped baseline stays at 100%, not full-web compatibility.

- Added bounded percentage and single-expression calculated widths without a
  general CSS evaluator. Authored `width`, `min-width`, and `max-width` compile
  into optional percentage/pixel tuples and resolve at layout time across root
  and nested blocks, controls, images, flex, grid, tables, paint, hits, and
  accessibility. Supported calculations contain one 1..100% term, one `+` or
  `-`, and one 0..2000px term; invalid declarations remain atomic. Custom
  properties, important priority, explicit inheritance, content/border boxes,
  min/max clamping, and resize reflow share the existing bounded paths. Compact
  `wd(id,value)`, readable `width(id,value)`, and literal
  `element.style.width` use the same parser; empty assignment restores authored
  width without erasing unrelated inline style or field state. Local async file
  navigation now yields and rechecks cancellation before disk input, removing a
  disk-speed race in disposal. Full engine, WinForms, compiled Firespider, and
  staged runtime tests pass. The benchmark passes 72 probes over 69 fixtures.
  All 42 Boot cases pass, including the real 35-second recovery timer without
  the old cast dialog, and all six actual EXE startup scenarios are responsive
  and MSHTML-free. Measured medians are 504.566 ms clean-process, 3.470 ms
  local-load, and 31.742 ms layout/paint. Retained managed memory is 16,587.0
  KiB for 345 pages, or 48.1 KiB/page. Required runtime files total 544.5 KiB
  with a 231.8 KiB GZip estimate. Runtime 176 is isolated and source-backed-up;
  installed runtime 157 remains unchanged. See `CALCULATED_WIDTHS.md`. The
  scoped baseline stays at 100%, not full-web compatibility.

- Added compact `dp(id,value)` display mutation plus literal
  `document.getElementById(...).style.display`, `location`, `location.href`, and
  `location.assign(...)` compatibility. Display accepts only a bounded standard
  keyword set; empty assignment restores the authored cascade while retaining
  unrelated inline declarations. Familiar navigation maps to the existing
  guarded `go` request, and relative local-file targets now preserve real query
  and fragment components. Computed values, `location.replace`, history APIs,
  arbitrary methods, unsupported display modes, and unsafe schemes remain inert.
  Core and WinForms tests verify retained layout/accessibility state and real
  flex reflow; the compiled Firespider smoke passes through the rebuilt host.
  The benchmark passes 71 probes over 68 fixtures. All 42 Boot cases pass in one
  combined run, including the real 35-second recovery timer without the old cast
  dialog, and all six actual EXE startup scenarios remain responsive and
  MSHTML-free. Measured medians are 512.230 ms clean-process, 3.203 ms local-load,
  and 32.563 ms layout/paint. Retained managed memory is 16,216.9 KiB for 340
  pages, or 47.7 KiB/page. Required runtime files total 541.0 KiB with a 230.7
  KiB GZip estimate. Runtime 175 is isolated and installed runtime 157 remains
  unchanged. See `DISPLAY_NAVIGATION.md`. The scoped baseline stays at 100%, not
  full-web compatibility.

- Added a literal `document.getElementById` compatibility bridge without adding
  a general JavaScript VM. Quoted ID lookups can use `textContent/innerText`,
  `value`, checked/disabled/hidden/open booleans, `focus()`,
  `classList.add/remove/toggle`, and background/color/font-size styles. Each
  statement maps to the existing short text, state, class, focus, and style
  commands, retaining all target, value, field, focus, style, and
  32-command bounds. `innerHTML`, selectors, computed IDs/values, callbacks,
  expressions, and unknown APIs remain inert. The dedicated fixture, full core
  and WinForms suites, strengthened compiled Firespider smoke, and benchmark all
  pass, with 70 probes over 67 fixtures. All 42 Boot cases pass in one combined
  run; recovery pumped the real 30-second timer without the old cast dialog.
  All six actual EXE startup scenarios remained responsive and MSHTML-free.
  Firespider reports ASLR true, CPU 2500, all 15 helper guard bits, and 8,486,912
  private/Job-peak bytes. Required runtime files total 540.0 KiB with a 230.1
  KiB GZip estimate; measured medians are 634.045 ms clean-process, 4.069 ms
  local-load, and 33.864 ms layout/paint, not a speedup claim. Runtime 174 is
  isolated and installed runtime 157 remains unchanged. See `DOM_BRIDGE.md`.
  The scoped baseline stays at 100%, not full-web compatibility.

- Added verified process-creation mitigations and a Job CPU hard cap to the
  image-helper boundary. The extended startup policy requests DEP, SEHOP,
  heap-corruption termination, forced relocation, bottom-up ASLR, and
  architecture-supported high-entropy ASLR; Windows must report forced
  relocation plus bottom-up randomization while the process is still suspended.
  The Job must then accept and read back enabled hard-cap flags and the exact
  CPU rate before assignment/resume. Compact `Ias/Icp/Icr` aliases report startup
  ASLR, active CPU policy, and a configurable 100-10,000 basis-point rate; 2500
  (25.00%) is the default, and a change restarts the shared worker. Live tests
  query both ASLR and the Job's private CPU policy, clamp both rate bounds, and
  prove a changed rate gets a new PID. Full picture/core/host and benchmark
  suites pass, with 69 probes over 66 fixtures, all 42 Boot cases in one combined
  run, and all six real EXE startup scenarios. The recovery case pumped the real
  30-second timer without the old cast dialog. Firespider reports ASLR true,
  CPU 2500, all 15 helper guard bits, and 8,556,544 private/Job-peak bytes.
  Required runtime files total 538.5 KiB with a 229.7 KiB GZip estimate; medians
  are 499.587 ms clean-process, 3.231 ms local-load, and 32.724 ms layout/paint,
  not a speedup claim. The helper remains outside an AppContainer and retains
  user read/network identity; capability-denied networking, custom ACLs, and a
  combined host/helper memory ceiling remain open. Runtime 173 is isolated and
  source-backed-up; installed runtime 157 remains unchanged. The scoped baseline
  stays at 100%, not full-web compatibility.

- Replaced the normal image-helper launch with a compact native restricted
  launcher. `CreateRestrictedToken(DISABLE_MAX_PRIVILEGE)` leaves at most the
  traversal privilege enabled; the token is set to low integrity RID 4096.
  Extended startup inherits only stdin/stdout/stderr, creates the helper hidden
  and suspended, assigns the existing one-process/memory Job, and resumes only
  after that boundary is active. The helper then permanently enables strict-
  handle checks, disables legacy extension points, enables supported image-load
  and child-process policies, and sends a required handshake before receiving
  encoded bytes. Compact `Irt/Ili/Isp/Ihl/Img/Ipk` aliases expose each launch
  guard and Job-reported peak process bytes. Live tests inspect the actual child
  token, integrity SID, Job membership, and mitigation policies; an unguarded
  fake worker fails before native admission. Five codecs still reuse one PID,
  timeout/restart/cap and abrupt-host cleanup pass, and the real Firespider host
  reports all 15 current guard bits with 8,540,160 private/Job-peak bytes under
  its 192 MiB cap. Full picture/core/editor/WinForms, compiled adapter, and
  benchmark suites pass, with 69 probes over 66 fixtures, all 42 Boot cases in
  one combined run, and six real EXE startup scenarios. Those windows stayed
  responsive; the 35-second recovery timer produced no startup cast dialog,
  orphan helper, or MSHTML. Required runtime files total 536.0 KiB with a 228.8
  KiB GZip estimate; medians are 521.959 ms clean-process, 3.323 ms local-load,
  and 33.453 ms layout/paint, not a speedup claim. The helper is not yet an
  AppContainer and retains user read/network identity; custom ACLs, network
  denial, startup ASLR, CPU-rate limits, combined host/helper peak memory,
  image cookie/cache sharing, and full modern-web behavior remain open. See
  `IMAGE_ISOLATION.md`. Runtime 172 is isolated and source-backed-up; installed
  runtime 157 remains unchanged. The scoped baseline stays at 100%, not full-web
  compatibility.

- Added mandatory Windows Job Object ownership around `Lea.Image.exe` before
  request bytes are sent. The Job permits one process, enforces a configurable
  192 MiB default hard memory ceiling, exits on unhandled exceptions, and kills
  the helper when its browser handle closes. Compact `Ij/Ih/Ipm` aliases expose
  last Job assignment, a clamped 64-512 MiB cap, and last sampled private bytes;
  changing the cap restarts the shared worker. Five codecs reuse one PID, cap
  bounds/restart pass, and an abruptly terminated child test host proves that a
  helper blocked for 60 seconds is removed by Job close. The real host sampled
  8,466,432 helper private bytes under its cap. This milestone also replaces
  silent native copy/cut success with bounded Lea-owned clipboard commits: busy
  clipboard operations return 0 while preserving text, selection, and undo, and
  cut mutates only after a successful write. Deterministic reader callbacks now
  cover navigation/value/selection/disposal races without clipboard-manager
  prefetch. Full picture, core, repeated isolated editor, compiled host, and
  benchmark suites pass, with 69 probes across 66 fixtures, all 42 Boot cases in
  one combined run, and six actual EXE launch scenarios. Those windows stayed
  responsive; no startup error dialog, orphan helper, or MSHTML was observed.
  Required runtime files total 525.5 KiB with a 224.0 KiB GZip estimate; current
  medians are 477.096 ms clean-process, 3.156 ms local-load, and 31.199 ms
  layout/paint, not a speedup claim. Restricted-token/AppContainer isolation,
  process mitigations, peak whole-process memory, image cookie/cache sharing,
  and full modern-web behavior remain open. See `IMAGE_ISOLATION.md` and
  `NATIVE_EDITING.md`. Runtime 171 is isolated and source-backed-up; installed
  runtime 157 remains unchanged. The scoped baseline stays at 100%, not
  full-web compatibility.

- Moved accepted PNG/JPEG/GIF/BMP/ICO native parsing out of Firespider into the
  required 7 KiB `Lea.Image.exe` helper. All views share one reusable serialized
  worker; the compact `Ie/Ii/Iw/Ik` aliases expose its path, availability, last
  PID, and cancellation kills. The bounded pipe returns validated raw 32-bit
  pixels, missing/malformed helpers remain nonfatal, and a deadline, Stop,
  replacement, or disposal can kill exact native work and restart cleanly.
  Full picture, core, isolated editor, compiled host, and benchmark suites pass,
  with 69 probes across 66 fixtures, all 42 Boot cases in one combined run, and
  six actual EXE launch scenarios. Those windows stayed responsive; no startup
  error dialog, orphan image helper, or MSHTML was observed, including deferred
  runtime resume. Required runtime files total 521.5 KiB with a 222.2 KiB GZip
  estimate; current medians are 486.317 ms for a clean diagnostic process,
  3.299 ms for local fixture loads, and 33.184 ms for layout/paint, not a speedup
  claim. Restricted-token/Job/AppContainer isolation, measured whole-process
  memory, image cookie/cache sharing, and full modern-web behavior remain open.
  Earlier intermittent fragment and recovery-cast observations remain
  unconfirmed. See `IMAGE_ISOLATION.md`. Runtime 170 is isolated and
  source-backed-up; installed runtime 157 remains unchanged. The scoped baseline
  stays at 100%, not full-web compatibility.

- Added bounded PNG/JPEG/GIF/BMP/ICO structural preflight before GDI+, moved
  validation/copying to a worker task, and added atomic process-wide Lea image
  reservations. Compact `Ip/Im/It/In/If` aliases expose the per-image pixel cap,
  configurable 128 MiB shared limit, current reserved/retained estimate, native
  attempts, and active failure sentinels. The existing 32 MiB per-view `Ic/Ib`
  budget remains. Hostile dimensions and bounded JPEG headers stop with zero
  native admission; multi-view exhaustion recovers after disposal or Reload.
  Full picture, core, isolated editor, compiled host, and benchmark suites pass,
  with 69 probes across 66 fixtures, all 42 Boot cases in one combined run, and
  six actual EXE launch scenarios. Those EXE windows stayed responsive; no
  startup error dialog or MSHTML was observed, including deferred-tab resume.
  Engine DLLs total 509.5 KiB with a 216.9 KiB GZip estimate; current medians are
  489.765 ms for a clean diagnostic process, 3.285 ms for local fixture loads,
  and 31.072 ms for layout/paint, not a speedup claim. Killable helper-process
  codecs, measured whole-process memory, image cookie/cache sharing, and full
  modern-web behavior remain open. Earlier intermittent fragment, recovery-cast,
  and editor/focus observations remain unconfirmed. See `IMAGE_PREFLIGHT.md`.
  Runtime 169 is isolated and source-backed-up; installed runtime 157 remains
  unchanged. The scoped baseline stays at 100%, not full-web compatibility.

- Preserved exact cache resource identity after URI canonicalization. Memory and
  persistent entries, SHA-256 filenames, stored-key validation, validators,
  linked CSS, same-document fragments, and navigation now compare path/query
  text ordinally while fragment variants share one representation. Older
  case-mismatched disk collisions are rejected rather than reused or migrated.
  Full cache, core, isolated editor, compiled host, and benchmark suites pass,
  with 68 probes
  across 65 fixtures, all 41 Boot cases in one combined run, and six actual EXE
  launch scenarios. Those EXE windows stayed responsive; no startup error dialog
  or MSHTML was observed, including deferred-tab resume. Engine DLLs remain
  exactly 500.0 KiB with a 212.9 KiB GZip estimate; current medians are 491.286
  ms for a clean diagnostic process, 3.251 ms for local fixture loads, and 30.497
  ms for layout/paint, not a speedup claim. Native decoder peaks, process-wide
  memory, image cookie/cache sharing, `Vary`, and full modern-web behavior remain
  open. Earlier intermittent fragment, recovery-cast, and editor/focus
  observations remain unconfirmed. See `CACHE_IDENTITY.md`. Runtime 168 is
  isolated and source-backed-up; installed runtime 157 remains unchanged. The
  scoped baseline stays at 100%, not full-web compatibility.

- Added bounded streaming reads for local/HTTP documents, GET and POST responses,
  and linked stylesheets. Compact `Hm/Csm` aliases expose the 4 MiB document and
  256 KiB CSS ceilings. Known-length, unknown-length, gzip, and deflate input is
  checked on both wire and decoded bytes; one captured `Tm` budget covers the
  document and all optional sheets. Stop, overlap, cache reuse, local-file/UNC
  origins, typed large/timeout/decode/cancel failures, retained-page recovery,
  and exact-limit acceptance are covered. A split lifecycle startup test now
  waits for the form's async ready state before invoking its comparison command.
  Final input, core, isolated editor, compiled host, and benchmark suites pass,
  with 67 probes across 64 fixtures, all 40 Boot cases in one combined run, and
  six actual EXE launch scenarios. Those EXE windows stayed responsive; no
  startup error dialog or MSHTML was observed, including deferred-tab resume.
  Engine DLLs total 500.0 KiB with a 212.9 KiB GZip estimate; current medians are
  792.480 ms for a clean diagnostic process, 4.159 ms for local fixture loads,
  and 31.697 ms for layout/paint, not a speedup claim. Native decoder peaks,
  process-wide memory, HTTP cache URL case handling, image cookie/cache sharing,
  and full modern-web behavior remain open. Earlier intermittent fragment,
  recovery-cast, and editor/focus observations remain unconfirmed. See
  `INPUT_LIMITS.md`. Runtime 167 is isolated and source-backed-up; installed
  runtime 157 remains unchanged. The scoped baseline stays at 100%, not full-web
  compatibility.

- Added bounded streaming picture reads, a shared asynchronous pass deadline,
  cancellation with late-resource disposal, and a 32 MiB retained bitmap budget.
  Compact `Ic/Ib` counters expose retained images and estimated pixel bytes;
  case-sensitive cache keys, failed-image Reload retry, and local-file origin
  checks prevent stale reuse and repaint/refetch loops. Stalled images no longer
  hold successful document completion and Firespider request cleanup indefinitely.
  Final picture, core, isolated editor, and compiled host suites pass, along with
  66 probes across 63 fixtures, all 39 Boot cases in one combined run, and six
  actual EXE launch scenarios. No startup dialogs or MSHTML were observed in
  those EXE scenarios, including deferred-tab resume. DLLs total 491.5 KiB
  (208.3 KiB GZip estimate), not a speedup claim. Native decoder peaks and
  document/CSS input buffering still need limits; earlier intermittent fragment,
  recovery-cast, and editor/focus observations remain unconfirmed. See
  `PICTURE_LIMITS.md`. Runtime 166 is isolated and source-backed-up; installed
  runtime 157 remains unchanged. The scoped baseline stays at 100%, not full-web
  compatibility.

- Fixed reproduced async view-readiness and old-image cache races with compact
  `Cn` admission, request ownership through presentation, and coordinated picture
  passes. Completion/failure callbacks can start successors without old cleanup
  resetting their cursor or dispatching stale page commands. Firespider now owns
  navigation/retry/reload bookkeeping by request identity; page-command successors
  retain pending targets and preview CSS, while overlapping input is harmless.
  Core, isolated native editing, and full compiled host suites pass, plus all
  65 probes across 62 fixtures. All 38 Boot cases passed in one combined run,
  and six real EXE scenarios passed with responsive windows, deferred resume,
  no observed startup dialogs, and no MSHTML. DLLs total 484 KiB (204.8 KiB GZip
  estimate), not a claim of increased execution speed. Test waits now distinguish
  title/data commitment from view completion. Earlier intermittent fragment,
  recovery-cast, and editor/clipboard/focus issues remain unconfirmed. See
  `LOAD_LIFECYCLE.md`. Runtime 165 is isolated and source-backed-up; installed
  runtime 157 is unchanged. The scoped baseline stays at 100%, not full-web support.

- Added compact `Ss/Sd/Sn` named native CSS with byte/rule limits, atomic
  replacement, retained documents/fields/history, and live media/font updates.
  Firespider now applies matching saved CSS and startup/timer previews without
  a legacy DOM. Fixed a reproduced preview queue surviving failed requests;
  missing records and closed/paused views are guarded. Core, isolated native
  editing, and compiled host suites pass; all 64 probes pass across 61 fixtures.
  Engine DLLs total 485 KiB. All 37 Boot cases passed across combined/focused
  runs, but not in one clean combined run: an editor password Copy/Cut assertion
  failed, followed by a toolbar-focus assertion on a focused run. Four later
  isolated runtime 164 editor runs and three runtime 163 comparison runs passed.
  Those early assertions did not distinguish clipboard changes from field
  changes; added diagnostics do, without logging field or clipboard contents.
  Their cause remains unconfirmed, as do the older fragment/recovery issues.
  Six real EXE launch scenarios pass, including deferred-tab resume, with
  responsive windows and no MSHTML or startup error dialogs observed.
  See `NATIVE_STYLESHEETS.md`. Runtime 164 is isolated and source-backed-up;
  installed runtime 157 remains unchanged. The scoped baseline stays at 100%,
  not a percentage of complete modern-web support.

- Fixed reproduced selected-text loss when Windows clipboard access fails.
  Compact `V.Pa(text)` shares bounded native insertion, one-level undo, Unicode
  limits, target validation, and callback protection with guarded clipboard
  paste. Native menu/message/keyboard routes share the fix, and mouse-edited
  fields beyond the keyboard-list limit now accept edit commands. Also fixed
  a reproduced Firespider saved-style automation startup/timer legacy cast;
  unsupported preview leaves Lea unchanged and later initialization continues.
  The release core and compiled host suites, 36 Boot cases, six real EXE launch
  scenarios, and all 63 probes across 60 fixtures pass. Engine DLLs total
  482 KiB. An intermittent initial-fragment failure recurred earlier in this
  milestone and remains open with before/after-paint diagnostics. The earlier
  stackless recovery observation cannot be conclusively attributed to the new
  confirmed style cast. See `PASTE_SAFETY.md` and `STYLE_STARTUP_SAFETY.md`.
  Runtime 163 is isolated; the installed runtime 157 remains unchanged.

- Added compact `E.Ud/V.Ud` retained-field queries and Lea-aware optional
  Firespider hibernation. Eligible idle renderers are disposed and collectible;
  private/protected/edited/busy/error tabs, cookies, and non-restorable live state are
  kept. The real 30-second timer, close cleanup, URL retention, and fresh-view
  resume are covered. All 62 compatibility probes pass across 60 fixtures;
  core-only and compiled host suites pass, with 12 separate editor passes and
  300 focused fragment navigation cycles. Earlier intermittent paste/clipboard
  failures and one initial-fragment failure remain unexplained; diagnostics and
  independent test modes are retained. The 34-case Boot suite includes two
  hibernation cases. The separate recovery cast is still open. See
  `HIBERNATION.md`; the scoped baseline percentage is unchanged.

- Added compact native document-failure state/notification `V.Le/Fe` and
  explicit GET retry `V.Cr/Ry`, with retained pages/edits/history, cancellation
  cleanup, UI-thread notification, and no POST replay. Fixed reproduced
  unhandled HTTP failures from keyboard navigation and editor access keys;
  keys are consumed before awaiting the network. Firespider keeps selected-tab
  errors visible, preserves failed first-page addresses/sessions, and retries
  through Reload. Split/test hosts also report failures. The Boot suite covers
  32 cases and all 61 compatibility probes pass across 59 local fixtures.
  This is separate from the still-unidentified recovery cast. See
  `LOAD_FAILURES.md`; the scoped baseline percentage is unchanged.

- Installed compact `V.Ng` policy on ordinary Firespider Lea tabs for blocked
  sites, navigation filters, offline/locked state, and live ownership. Hidden
  and private tabs retain policy; denied Resume/address/search/Home preserve
  suspended targets and placeholders.
  Fixed reproduced unhandled keyboard navigation denials and handled expected
  errors in native accessibility and host history/initial-load callbacks.
  Thirty Boot cases and sixty compatibility probes pass, including blocked
  saved-URL startup with an allowed sibling. The intermittent recovery cast
  remains unidentified; see `NAVIGATION_POLICY.md` for explicit policy limits.

- Added compact `V.Ce/Ec` native field commands for copy, cut, paste,
  select-all, delete, undo, and redo. Firespider's real Edit menu no longer
  casts Lea views to WebBrowser; private/read-only fields, retained menu
  selection, toolbar focus, paused/empty tabs, and legacy fallback are covered.
  Fixed shared-font disposal while switching fields and retained native undo
  for Delete. The Boot suite has 28 cases, including a 35-second recovery-timer
  observation; the compatibility runner has 59 probes. The separate
  intermittent recovery cast remains unresolved. See `NATIVE_EDITING.md`.

- Replaced split view's legacy controls with independent Lea panes, retained
  private source flags, asynchronous loading, synchronized scrolling, bounded
  source lists, and explicit disposal/cancellation. Text comparison stays in
  memory with bounded read-only results. Compact `E.Ng/V.Ng` guards explicit
  page navigation without changing retained state on rejection. Three Boot
  cases bring the suite to 27; the guard adds a 58th compatibility probe. See
  `SPLIT_VIEW.md` for tests, policy boundaries, an unresolved intermittent
  recovery-time legacy cast, and remaining DOM migration work.

- Migrated same-domain reload, close, bookmark, duplicate, next-tab, new-window,
  copy, and snapshot commands to Lea-aware paths. Matching includes active,
  paused, and mixed-engine visible tabs; hidden tabs stay untouched. Copies
  preserve privacy and fields, new windows retain the first URL without a
  default request, and exports are bounded by rows and UTF-8 bytes. Fixed
  snapshot cancellation falling through to a write. Two new Boot cases bring
  the suite to 24. See `DOMAIN_COMMANDS.md` for evidence and remaining scope.

- Migrated shared bulk/workspace/protected-session opening, last/recent closed
  windows, and unread watch batches to Lea. Fixed reproduced first-tab loss and
  a closed-window legacy cast. Explicit opens retain existing forms; seeded new
  windows avoid default requests and honor private/lazy state. Bounded snapshots
  include hidden/paused Lea URLs while excluding private ones. Watch batches
  retain skipped unread records and case-sensitive paths. Three new Boot cases
  bring the suite to 22. A step-scheduled disposal fixture replaces a reproduced
  test race. See `BULK_OPENING.md` for verification and remaining migration scope.

- Added bounded Lea-native lazy session restoration under the existing off,
  background, and all policies. Deferred tabs create no renderer or page request
  until resumed. Explicit sessions preserve existing fields; identity switches
  replace old mixed/hidden/private contexts and menus. Private deferred sessions,
  batch limits, eager clipboard behavior, accurate restore counts, and status
  retention are covered. All 19 Boot cases, six real EXE startup scenarios,
  full engine/host suites, and 57 compatibility probes passed. See
  `LAZY_RESTORE.md` for remaining legacy routes and startup-report limitations.

- Added Lea-native tab/group/overview suspension and same-tab fresh-view resume,
  pending-load cancellation, hidden/paused automatic sessions, private-state
  retention, and paused close/duplicate routes. Restored the hidden Reload menu.
  Local/CSS/image continuations and callback disposal are guarded. Native UI
  Automation teardown fixes a reproduced retained-view regression; a weak-
  reference test now confirms collection. Sixteen Boot cases, full engine and
  host suites, 57 compatibility probes, and four real EXE startup cases passed.
  The newly reported startup popup was not reproduced. The prior timer cast
  remains guarded and tested. See `SUSPENSION.md` for unsaved-state and migration
  limits; archive size is not a runtime-speed claim.

- Migrated saved/default Home, previous-session reopening, both left-close
  variants, tab-list selection, and clipboard/site-root navigation to Lea-aware
  paths. Enabled the hidden Home menu, preserved existing tabs during explicit
  restore, retained ports/IPv6 while dropping root-URL credentials, and bounded
  clipboard text and session scanning. Private explicit opens and mixed-engine
  disposal have compiled-host coverage. Fourteen boot cases, the full engine
  and host suites, and four real EXE startup scenarios passed. The launcher now
  retains process exit codes under Windows PowerShell. See
  `NAVIGATION_COMMANDS.md` for limits and unautomated clipboard OS I/O.

- Migrated common Firespider duplicate, close/reopen, bookmark, reload-all,
  and named-session commands to engine-aware paths. Exposed the hidden Close
  Tab command, retained legacy duplication and tab sizing, and replaced its
  last-tab abrupt termination with normal cleanup. Mixed bulk closes dispose
  controls and prune retained state. Navigation recognizes absolute file URLs
  before search heuristics. Recent history and automatic Lea session saves
  exclude private Lea URLs; explicit private named-session opening works.
  Named opens preserve existing tabs/records, enforce Lea restore bounds, and
  report skipped URLs. Clear Saved Session now includes the Lea file with
  locked-file preservation. Twelve boot cases, including real command/shortcut
  checks and a mandatory completion marker, plus full engine/host and four EXE
  startup suites passed. See `TAB_SESSION_COMMANDS.md` for coverage and limits.

- Profiled and reduced Firespider's startup menu-layout cost. Root and category
  dropdowns now batch layout with Finally cleanup, retaining the same 552-item
  command tree. Five alternating fresh-process samples reduced median
  Form.Show from 14.062 to 1.683 seconds (88.0% less time), and root layout
  events from 1,075 to 8. Eleven form-start cases include nested-menu bounds,
  resumed layout, and forced category-build failure cleanup; the full engine
  and compiled host suites and four standalone EXE startup scenarios passed.
  Opt-in, bounded phase timing performs no file I/O; EXE tests now report
  readiness while still observing for delayed dialogs. This changes host
  startup, not engine DLLs or web-platform compatibility. See
  `STARTUP_PERFORMANCE.md` for measurements and limits.

- Moved normal Firespider first-page startup, toolbar New Tab, and Help onto
  Lea. One bounded restore path prioritizes accepted crash recovery, Lea
  sessions, and legacy URLs converted to Lea tabs; it selects the first page,
  avoids duplicate restoration in later windows, and honors private startup.
  Requests can remain pending while the window opens, and recovery snapshots
  now retain Lea URLs. Real EXE tests also exposed and fixed the legacy .NET
  reader's rejection of compact settings XML, preserving all 89 values, and
  session-write failures skipping normal shutdown. Ten form-start cases,
  four standalone startup cases, eighteen event/session regressions, full
  engine/host suites, and all 57 compatibility probes passed. Default startup
  exposes a Lea document without loading MSHTML. Existing host initialization
  still takes roughly 11-19 seconds in the form harness; profiling that cost
  remains next work, not a claimed speedup. See `LEA_STARTUP.md`.

- Hardened Firespider startup and mixed-engine events. Regression tests first
  reproduced Lea-to-WebBrowser casts in three legacy callbacks, invalid
  selection-event casts, a missing-tab timer fault, background progress/lock
  misrouting, and session-file failures. Callbacks now validate their owning
  tab, selected-tab chrome stays isolated, selection handlers accept EventArgs,
  and empty-tab navigation is harmless. Session I/O is bounded to 64 KiB,
  twelve URLs, and 4,096 characters per URL, with readable status on I/O errors.
  Eighteen focused host checks, the full engine/host suites, all 57 compatibility
  probes, and clean/restored/locked/oversized executable startup checks passed.
  This milestone changes host reliability, not the supported HTML/CSS subset
  or engine speed. See `STARTUP_RELIABILITY.md` for reproduction and limits.

- Added bounded non-inherited CSS text overflow through compact `S.To=c/e`
  clip/ellipsis state. Strict values, explicit inheritance, resets, invalid
  preservation, variables, important tiers, hover, and responsive rules feed
  paint-only text-element truncation. Original line sizes, authored text, and
  pointer targets remain intact, including corrected collapsed link spaces.
  Synthetic markers, first-character clipping, mixed inline runs, explicit
  and unbreakable lines, tables, automation prefixes, and retained form state
  have engine, renderer, benchmark, and compiled Firespider coverage. The suite
  now has 58 fixtures and 57 executable compatibility probes.

- Added bounded inherited CSS letter and word spacing through compact
  `S.Ls/Wg` signed -32..128 pixel state. Strict normal/reset parsing,
  parent-restoring inheritance, invalid preservation, variables, important
  priority, inline overrides, hover, and responsive rules share the cascade.
  Measurement and paint share text-element metrics, omit line-edge tracking,
  average inline boundary gaps, preserve word-only shaping, and retain the
  zero-spacing fast path. Blocks, multiline content, table cells, button sizing,
  unfocused controls, authored values, native editing, and automation rectangles
  have fixture-backed coverage. Full engine, renderer, benchmark, and compiled
  Firespider smokes bring the suite to 57 fixtures and 56 executable probes.

- Added bounded inherited CSS `text-transform` through compact `S.Tt=n/u/l/c`
  state and shared `E.Tx` presentation handling. Strict none, uppercase,
  lowercase, and capitalize parsing supports variables, important tiers,
  CSS-wide inherit/reset behavior, malformed preservation, inline overrides,
  multiline text, table cells, semantic values, buttons, placeholders, select
  labels, unfocused fields, dynamic hover, and responsive rules. Authored DOM
  text, mutable field and option values, submitted form data, and focused native
  editor text retain their original casing. UI Automation keeps logical source
  text and maps transformed visual segments case-insensitively. Exact engine,
  renderer, automation, editor, resize, benchmark, and compiled Firespider
  coverage bring the suite to 56 fixtures and 55 executable probes.

- Added bounded inherited CSS pointer interaction through compact `S.Cu/Pe`.
  Automatic/default, pointer, text, wait/progress, crosshair, move, help,
  not-allowed, and horizontal/vertical resize cursor families map to ten small
  states; `pointer-events:auto/none` controls one boolean mouse gate. Variables,
  important tiers, CSS-wide inheritance/resets, invalid preservation, dynamic
  hover, and responsive rules share the compiled cascade. Retained boxes,
  links, summaries, fields, fitted image maps, and linked SVG shapes use one
  z-ordered gate with eligible underlying fallthrough. Paint, geometry,
  keyboard focus, access keys, and accessibility bounds remain available.
  Exact cursor, hover, hit, click, field, focus, responsive, benchmark, and
  compiled Firespider coverage bring the suite to 55 fixtures and 54 executable
  probes.

- Added bounded CSS outlines through compact non-inherited `S.Ow/Os/Oc/Oo`
  width, style, color, and signed-offset aliases. Atomic shorthand and
  longhands accept solid, dashed, dotted, and automatic focus styles; widths
  cap at 32 pixels and offsets at -64..64. Named, hex, `rgb()`/`rgba()`, alpha,
  `currentColor`, variables, important tiers, CSS-wide resets, malformed
  preservation, responsive rules, and dynamic `:focus` share the existing
  cascade. Rounded boxes, images, fields, retained nested owners, opacity, and
  ancestor overflow share one paint path while layout, hits, accessibility
  bounds, and following flow remain unchanged. Exact pixels, field retention,
  resize, benchmark, and compiled Firespider coverage bring the suite to 54
  fixtures and 53 executable probes.

- Added bounded CSS `aspect-ratio` sizing through compact non-inherited
  `S.Rw/Rh/Ra` preferred-width, preferred-height, and intrinsic-auto aliases.
  Atomic parsing accepts one positive number or a two-part ratio, optional
  `auto`, variables, important tiers, CSS-wide resets, malformed preservation,
  and responsive media rebuilds under strict numeric limits. One shared sizing
  path covers content/border boxes, normal and nested blocks, text minimums,
  raster and vector intrinsic fallback, fields, flex bases, grid tracks, and
  table minimums while explicit dimensions and min/max clamps remain decisive.
  Exact geometry, pixels, resize, benchmark, and compiled Firespider host
  coverage bring the suite to 53 fixtures and 52 executable probes.

- Added bounded replaced-image fitting through compact non-inherited
  `S.Of=f/c/v/n/s` fill, contain, cover, none, and scale-down state plus
  `Ox/Oy/Oxp/Oyp` pixel-or-percentage position aliases. Atomic variables,
  important tiers, resets, malformed preservation, and responsive media rules
  feed one 8,192-pixel-capped object rectangle. Raster paint clips cover/none
  overflow to the element content box without changing layout. Image-map
  pointer, keyboard, and accessibility geometry shares that positioned object,
  excludes contain/none letterboxes, and clips cover crops. Exact pixels,
  cache reuse, narrow repaint, benchmark, and compiled Firespider host coverage
  bring the suite to 52 fixtures and 51 executable probes.

- Added bounded CSS bitmap backgrounds through compact `S.Bi` and
  `Bi.U/R/X/Y/Xp/Yp/W/H/Wp/Hp/Wa/Ha/K` aliases. One atomic image layer accepts
  bounded `data:`, local file, HTTP, or HTTPS URLs; both-axis, horizontal,
  vertical, or no-repeat modes; pixel/percentage positions and sizes;
  `auto`, `cover`, and `contain`; custom-property and linked-stylesheet origins;
  important tiers, resets, malformed preservation, and noninheritance. A shared
  24-entry, 4 MB-per-image cache feeds page, rounded box, and ordinary-field
  paint with effective opacity, clipping, stable layout/hits, responsive
  restyling, exact pixels, benchmark, and Firespider host coverage. The suite
  now has 51 fixtures and 50 executable probes.

- Added bounded CSS background gradients through compact `S.Gi`,
  `Gi.K/A/X/Y/Ss`, and `Cs.C/P` aliases. One atomic linear or radial layer
  accepts up to eight normalized stops, integer angles or direction keywords,
  centered or percentage radial origins, named/hex/`rgb()`/`rgba()` colors,
  `currentColor`, variables, important tiers, resets, and noninheritance.
  Malformed or over-limit declarations preserve the previous computed image.
  Rounded page, box, and ordinary-field painting shares effective opacity,
  overflow clips, stable layout and hit geometry, resize, exact pixel tests,
  benchmark, and Firespider host coverage. The suite now has 50 fixtures and 49
  executable probes.

- Added bounded CSS box shadows through compact `S.Sw` lists and `Sw.X/Y/B/R/C/I`
  aliases for offsets, blur, spread, color, and inset state. Atomic comma lists
  accept up to four shadows with signed pixel offsets/spread, nonnegative blur,
  named/hex/`rgb()`/`rgba()`/`currentColor`, custom properties, important tiers,
  resets, and noninheritance. Offsets/spread cap at 512 pixels, blur at 128, and
  soft paint at 12 concentric bands. Rounded outer and inset shadows, alpha,
  ordinary fields, unchanged layout/link hits, narrow repaint, exact pixels,
  benchmark, and Firespider host coverage bring the suite to 49 fixtures and 48
  executable probes.

- Added bounded circular and elliptical CSS corner radii through compact
  `S.R0/R1/R2/R3` horizontal and `Q0/Q1/Q2/Q3` vertical aliases. Atomic
  one-to-four-value `border-radius` lists, optional slash-separated vertical
  lists, four corner longhands, pixels, percentages, unitless compact pixels,
  custom properties, important tiers, and reset values feed one normalized
  GDI+ path. Opposing radii scale to fit; pixels cap at 2,000 and percentages
  at 1,000. Rounded backgrounds, solid/dashed/dotted borders, inherited overflow
  clips, ordinary form fields, links, narrow reflow, exact pixels, benchmark,
  and Firespider host coverage bring the suite to 48 fixtures and 47 executable
  probes.

- Added bounded SVG winding and stroke styles through compact
  `Vs.Fr/Lc/Lj/Ml/Da/Do` aliases. Presentation, specificity-ordered stylesheet,
  inline, custom-property, and important declarations now share one inherited
  vector paint state for nonzero/evenodd fill and clip rules, butt/round/square
  caps, miter/round/bevel joins, miter limits, and dash arrays/offsets. Odd dash
  lists repeat to an even list; `none` clears them; malformed values retain the
  inherited state; and 16-value, 256-character, 2,000-unit, and 64-miter ceilings
  keep parsing and GDI+ paint bounded. Region clip unions, painted holes, stroke
  bounds, linked hits, exact pixels, benchmark, and Firespider host coverage
  bring the suite to 47 fixtures and 46 executable probes.

- Added bounded SVG reusable geometry and clip paths through compact `Vg.Cs`,
  `Vs.Cp/Cl/Cm`, `Vc`, and computed `S.Cp` aliases. Local `<use>` expands
  shapes, groups, symbols, nested uses, `x/y`, transforms, and inherited paint
  under four-link, 64-source, and 128-result ceilings with cycle rejection.
  User-space and object-bounds clip paths accept primitive/path/text geometry,
  transforms, multi-shape unions, reusable children, presentation/stylesheet/
  inline attachment, and inherited group contexts. Paint and linked hit bounds
  share the transformed clip, while missing or empty clips fail closed. Exact
  model, matrix, paint, hit, benchmark, and Firespider host coverage bring the
  suite to 46 fixtures and 45 executable probes.

- Added bounded SVG gradient paint servers through compact `Vg.Ds`, `Vs.Fd/Sd`,
  `Vd`, and `Vt` aliases. Linear and radial gradients support object-bounds and
  user-space coordinates, percentages, radial focus, local `href` inheritance,
  `gradientTransform`, presentation/stylesheet/inline stop color and opacity,
  `currentColor`, transformed fills and strokes, and linked-shape hits. Parsing
  is capped at 16 definitions, 16 normalized stops each, and four inheritance
  hops; duplicate IDs are deterministic while unsupported spread modes,
  unresolved references, malformed coordinates, and cycles fail closed. Exact
  model, opacity, color, paint, link, benchmark, and Firespider host coverage
  bring the suite to 45 fixtures and 44 executable probes.

- Extended bounded inline SVG with compact `Vg.A/M` aspect-ratio aliases,
  cumulative `Vs.M` transform matrices, and canonical curve controls in
  `Vp.A/B/C/D`. The engine now handles matrix, translate, scale, centered
  rotate, skewX/skewY lists; `none`, aligned `meet`, and aligned `slice`
  viewBox mapping; and absolute/relative `C/S/Q/T/A` geometry. Smooth controls
  resolve during parsing and each elliptical arc becomes at most four cubic
  steps, retaining the existing coordinate, path, transform, depth, and shape
  ceilings. Exact matrix/control models, transformed paint and links, clipping,
  invalid fallback, benchmark, and Firespider host coverage bring the suite to
  44 fixtures and 43 executable probes.

- Added bounded inline SVG through compact `B.Vg`, `Vg`, `Vs`, and `Vp`
  aliases. Intrinsic sizing, default meet `viewBox` scaling, groups, links,
  inherited presentation/inline fill, stroke, opacity, `currentColor`, six
  basic shapes, text, and `M/L/H/V/Z` straight paths share Lea's existing
  layout, clipping, paint, hit, fragment, and graphic-accessibility paths.
  Eight-level, 128-shape/point, 256-text, finite-coordinate limits, invalid
  rejection, exact paint, benchmark, and Firespider host coverage bring the
  suite to 43 fixtures and 42 executable probes.

- Added bounded CSS Grid content alignment. Compact `Al/Jc=s/c/e/b/a/v` state
  distributes fixed track groups on the block/inline axes using start, center,
  end, between, around, or evenly modes. Atomic one/two-value `place-content`,
  resets, invalid preservation, definite-height row offsets, authored gaps,
  exact geometry, paint, hits, following flow, benchmark, and Firespider host
  coverage bring the suite to 42 fixtures and 41 executable probes.

- Added bounded CSS Grid item alignment. Compact `Ji/Js` inline-axis state joins
  `Ai/As` block-axis state for stretch/start/center/end container defaults and
  item overrides. Atomic `place-items`/`place-self`, margin-box placement,
  measured simple-text auto widths, resolved-row vertical alignment, reset and
  invalid preservation, exact geometry, paint, hits, following flow, benchmark,
  and Firespider host coverage bring the suite to 41 fixtures and 40 executable
  probes.

- Added bounded atomic CSS Grid shorthands. `grid-template` compiles explicit
  rows/columns and optionally sized quoted areas while preserving automatic
  state. `grid` adds row/column `auto-flow` and `dense` forms, resets all owned
  explicit/implicit longhands but not gaps, and accepts `auto` in explicit track
  lists through `S.Av`. Scratch parsing prevents malformed sides, areas, tracks,
  flow markers, or extra slashes from partially changing a prior layout. Exact
  aliases, reset boundaries, dense geometry, paint, hits, following flow,
  benchmark, and Firespider host coverage bring the suite to 40 fixtures and 39
  executable probes.

- Added bounded repeating CSS Grid implicit-track sizing through `S.Av` and
  `S.Gac/Gav/Rac/Rav`. One-to-eight-entry pixel, auto, integer-fr, and minmax
  patterns cycle after explicit tracks, size area-created columns, honor
  definite endpoints and largest spans, and settle authored pixel rows in
  auto-height grids. Atomic reset/invalid parsing, exact mixed geometry, paint,
  hits, following flow, benchmark, and Firespider host coverage bring the suite
  to 39 fixtures and 38 executable probes.

- Added bounded CSS Grid auto placement through `S.Gf=r/d/c/x`. Sparse and
  dense row/column flow, earlier-hole backfill, equal-share implicit columns,
  definite-first occupancy, real-height visual traversal, and a safe
  non-overlapping eight-column overflow path all reuse the existing renderer.
  Reset/invalid parsing, exact geometry, paint, hits, following flow, benchmark,
  and Firespider host coverage bring the suite to 38 fixtures and 37 executable
  probes.

- Added bounded named CSS Grid lines. Lazy `S.Gnc/Rnc` maps and one-based
  `Gx/Rx(name)` lookup inspect unique explicit names, while item
  `Gn/Ge/Rn/Re` edges support shorthand and longhand placement. Named areas
  generate start/end lines without duplicate storage. Case-sensitive names,
  multiple names per line, outer named repeat groups, mixed numeric/name spans,
  atomic invalid declarations, 8-track/16-name ceilings, exact paint/hit/flow
  geometry, and Firespider host coverage bring the suite to 37 fixtures and 36
  executable probes.

- Added bounded named CSS Grid areas. `S.Aa/Ar/Ac/Ag` inspect a validated
  8x8 template and `S.An` exposes each item's area name. Equal-row validation,
  dot cells, filled rectangles, 16-name and 32-character ceilings,
  definite-first placement, implicit columns, nested inline hits, exact paint
  geometry, a dedicated fixture/probe, and Firespider host coverage bring the
  suite to 36 fixtures and 35 executable probes.

- Added bounded CSS Grid rows with lazy compact `Rd`, public `Rc/Rv`
  inspection, and direct-item `Rl/Rs` placement aliases alongside the existing
  column model. Fixed pixel and integer-fr row templates, repeat/minmax forms,
  row starts/spans, bounded occupancy, two-axis spans, auto-height measurement,
  exact paint/reflow geometry, benchmark, and Firespider host coverage all pass.

- Added bounded `grid`/`inline-grid` columns with a lazy compact `Gd` track
  array, public `Gc/Gv` inspection, and direct-item `Gl/Gs` start/span aliases.
  One to eight fixed pixel or integer-fr tracks, simple repeat/minmax forms,
  pixel gaps, sparse automatic rows, stable order-modified traversal, spans,
  nesting, fields, paint, hits, resize, strict invalid limits, benchmark,
  Inspector, and Firespider host coverage bring the suite to 35 fixtures and 34
  executable probes.

- Added bounded `autofocus` candidates and assigned `accesskey` alternatives
  without consuming unmatched or host-owned shortcuts.
- Added bounded `Ax` semantic accessibility records, compact live state flags,
  rendered bounds, host aliases, and a WinForms/MSAA hierarchy with protected
  values, focus, keyboard shortcuts, and default actions.
- Added bounded `hd/sh`, `ds/en`, `vl`, `ck`, and `fo` state commands, retained
  field ownership while hidden, one-shot `Kp` focus handoff, WinForms focus
  validation, and engine/host/benchmark fixtures.
- Added a native HWND-rooted UI Automation fragment provider over `Ax`, stable
  bounded runtime IDs, native control properties, protected values,
  invoke/value/toggle/selection/disclosure/range/scroll patterns, and
  structure/focus/property notifications; the provider and patterns are
  verified by an MTA UIA client.
- Added `Tb/Tr/Tc`-backed UI Automation grid, table, grid-item, and table-item
  patterns with zero-based coordinates, bounded loaded rows/cells, column spans,
  containing-grid links, row/column header relationships, and public MTA plus
  Firespider host verification.
- Added a bounded read-only UI Automation TextPattern over `P.X` and settled
  layout ownership with document/child/point ranges, character/word/line/document
  movement, search, attributes, rendered bounds, embedded children, scrolling,
  stale-page rejection, protected-value exclusion, and public MTA plus Firespider
  host verification.
- Added bounded visual-line TextPattern snapshots for plain and inline wrapped
  text, visual `TextUnit.Line` expansion/movement, one rectangle per visible
  line, proportional partial-range cropping, and public MTA, benchmark, and
  Firespider host verification.
- Added bounded `op/cl/tg` disclosure commands for a `details` ID or its first
  direct `summary`, synchronized `N.Op`/`open` state, retained nested fields,
  strict invalid-target rejection, and engine, WinForms, benchmark, plus
  Firespider verification.
- Added bounded single-line flex rows with compact `Fd/Jc/Ai/Gp` computed-style
  aliases, `flex`/`inline-flex`, row reversal, fixed and equal-share auto slots,
  pixel gaps, distribution and cross-axis alignment, direct inline-item model
  ownership, nested content, hit testing, painting, and fixture-backed reflow.
- Extended that flex subset with compact `Wr/Rg/Fg/Fb` state, bounded wrapping,
  two-value and separate row/column gaps, pixel basis, common `flex` shorthand,
  integer weighted growth, row reversal per line, exact geometry, narrow reflow,
  benchmark coverage, and Firespider host verification.
- Added compact `Sh` state and bounded integer `flex-shrink`, corrected shorthand
  shrink semantics, clamped flex bases through `Wx/Wn`, distributed deficits by
  outer-basis weight, froze items at explicit pixel minimums, retained overflow
  when locked floors cannot fit, and verified exact desktop/narrow geometry plus
  benchmark and Firespider host models.
- Added compact `Or` state with stable -64..64 ordering for nowrap rows and
  fixed-height columns, extended `Fd` with `c/y` column aliases, added bounded
  vertical grow/shrink, column-reverse flow, main/cross-axis alignment,
  `Hn/Hx` protection, and natural auto-height row gaps, then verified parser,
  exact geometry, paint, hits, following flow, resize, benchmark, and Firespider
  host paths.
- Added compact `As` item alignment plus `Jc=a/v` around/evenly distribution,
  shared the integer spacing helper across row and column axes, allowed direct
  stretch/start/center/end overrides without changing container defaults, and
  verified exact remainder geometry, stretching, narrow reflow, painting, link
  hits, benchmark probes, and the Firespider host model.
- Added two-argument `bg/fg/fs` overloads for ID-scoped background, foreground,
  and font-size changes while retaining the one-argument body forms, existing
  color and 8..96px bounds, safe target filtering, 4KB inline-style ceiling,
  field-preserving CSS rebuild, click execution, WinForms, benchmark, and
  Firespider host verification.
- Added non-inherited compact `S.Bz=c/b` state for `content-box` and
  `border-box`, applying one bounded conversion to pixel width, height, min/max,
  and flex-basis values across blocks, fields, images, flex items, tables, and
  cells. Exact wide/narrow geometry, cascade rejection, paint, hit,
  accessibility bounds, benchmark, and Firespider host coverage verify it.
- Added an opt-in gzip persistent representation cache with compact
  `Cd/Cx/Cb/Dc/Db/Cdl` aliases, SHA-256 URL filenames, same-directory atomic
  replacement, 32-entry/16 MB defaults, 128-entry/128 MB hard ceilings, LRU
  timestamps, cross-engine fresh hits and validator-backed `304` reuse. Private
  sessions perform no persistent reads or writes; `no-store`, reload, malformed
  files, entry/byte trimming, unrelated host files, bounded diagnostics,
  WinForms forwarding, benchmark loopback probes, and Firespider Inspector
  coverage are verified.
- Added a bounded responsive CSS compiler for `screen`/`all`, `min-width`,
  `max-width`, exact `width`, comma alternatives, nested `@media`, and
  `<style media>`. Compact `E/V/P/G.Vw` viewport aliases, 1-8,192 pixel bounds,
  four-level nesting, eight alternatives, 1,024 compiled rules, active-rule
  diagnostics, field-preserving boundary rebuilds, WinForms resize forwarding,
  fixtures, benchmark probes, and Firespider host coverage are verified.
- Added compiled declaration records plus bounded inherited CSS custom properties
  and `var()` fallback. Compact `S.Vc/Cv` and `G.Vc` aliases expose counts and
  case-sensitive lookup; lazy copy-on-write storage keeps ordinary styles on the
  low-allocation path. The engine caps each style at 32 properties, 64-character
  names, 512-character raw values, eight nested resolutions, 1,024 expanded
  characters, and 128 declarations per block. Missing, cyclic, oversized, and
  excess values fail into authored fallbacks while inline, hover, media,
  WinForms, benchmark, Inspector, and Firespider paths preserve live fields.
- Added one compiled important-priority bit and a bounded two-tier cascade for
  every supported ordinary and custom property. A final top-level literal
  `!important` marker is stripped while quoted/function text stays intact;
  stylesheet and inline priority, specificity, source order, shorthand and
  longhand interaction, inherited variables, hover, media, and field retention
  are verified. Compact `G.Ic` diagnostics and both Inspectors report the active
  stylesheet count. The compatibility suite now covers 30 fixtures and 29
  executable probes.
- Added bounded `:is(...)`, `:where(...)`, and selector-list `:not(...)` to the
  shared stylesheet/query compiler. Each function accepts eight compound
  alternatives, each simple selector permits sixteen authored functional tests,
  and nesting/inner combinators remain excluded. `:is`/`:not` use maximum
  argument specificity, `:where` uses zero, forgiving lists discard unsupported
  alternatives, and `:not` stays strict. Engine, query, dynamic state, WinForms,
  benchmark, Inspector, and Firespider host coverage bring the suite to 31
  fixtures and 30 executable probes.
- Added precompiled `An+B` arithmetic plus `:nth-last-child` and
  `:nth-last-of-type` to the existing child/type position matcher. Positive,
  negative, zero, unit, odd/even, fixed, forward, and reverse forms share
  compact `A/B` integers, 32-character expressions, and absolute 8,192-value
  bounds. Specificity, selector-list composition, invalid syntax, document
  queries, WinForms geometry, benchmark, Inspector, and Firespider coverage
  bring the suite to 32 fixtures and 31 executable probes.
- Added bounded `of S` filtering to `:nth-child` and `:nth-last-child`. Strict
  compound lists compile into the existing selector array, count only matching
  inclusive siblings in forward/reverse order, add maximum filter specificity,
  and share the eight-alternative/sixteen-test limits. Compound attributes and
  pseudo states, dynamic rebuilds, retained fields, invalid lists, nesting,
  typed misuse, stylesheet/query parity, WinForms geometry, benchmark,
  Inspector, and Firespider coverage bring the suite to 33 fixtures and 32
  executable probes.
- Added strict bounded `:has(...)` relative lists to the shared stylesheet/query
  compiler. Compact records cover implicit descendants plus leading child,
  adjacent, and following relationships with four-part chains, eight
  alternatives, sixteen total parts, maximum argument specificity, and a 4,096
  candidate-visit ceiling. Dynamic descendant focus rebuilds preserve fields;
  malformed lists, nested selector functions, pseudo-elements, oversized
  chains, and excess work fail closed. Engine, query, WinForms, benchmark,
  Inspector, and Firespider coverage bring the suite to 34 fixtures and 33
  executable probes.

## Next gains

- Investigate initial fragment placement with retained diagnostics.
- Add an AppContainer or capability-denied network boundary, custom object ACLs,
  and combined host/helper peak sampling without expanding the public
  integration surface.
- Add a native-safe non-empty visible-range bridge outside the .NET Framework
  managed provider array boundary; retain the stable empty fallback until range
  handles survive both in-process and external MTA clients.
- Keep expanding the bounded HTML/CSS subset and Lea-first host routes without
  adding a general JavaScript VM or external browser runtime.
