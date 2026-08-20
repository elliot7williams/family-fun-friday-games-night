# Lea Firespider Adapter

`FirespiderLea.vb` is a reversible experimental integration for the VB.NET
Firespider host. Firespider's normal **New Lea Tab** command and `Ctrl+T` now
create `Lea.Win.V`; **New Legacy Tab** (`Ctrl+Alt+L`) deliberately uses the
original `WebBrowser` path for features that still need it. Back, forward, home,
go, search, reload, stop, and address/search Enter keys route by the selected
tab's engine. Legacy tabs delegate to Firespider's original handlers.
Its timer status handler is also routed safely so it never casts a Lea view to
the legacy `WebBrowser` type.

Firespider's Close Tab command, tab-strip close target, and middle-click now
close Lea tabs safely. The existing Page Source window displays Lea's raw `V.H`
HTML. Open File loads local pages into the selected Lea view, Save Page writes
that source as UTF-8, and Send Link builds its message from Lea's title and URL.
Up to 12 Lea URLs restore from `%LocalAppData%\Firespider Internet\Lea.session`;
private mode deletes that file instead of saving it. Legacy session settings
remain owned by Firespider.

Firespider's private-mode flag also drives `V.Pv`. Normal Lea cookies stay in
their bounded in-memory jar, while each private session starts empty and is
destroyed when private mode ends. Page cache is cleared between those contexts.

Lea's bounded inline title/style commands run inside each Lea tab. Alert
messages cross the `V.Al` host event and appear in Firespider's existing status
bar; page code does not open a native dialog directly.
Buttons and links can run bounded `data-lea` or familiar `onclick` values through
`V.Ck`. Small `v/var` variables and `tx/text` element updates stay inside Lea's
allow-list and cannot reach native files, processes, or host controls.
ID-scoped `ac/rc/tc` class changes use the same route, reapply compiled CSS, and
preserve mutable field values.
The bounded `go(url)` command raises `V.Gr` with `V.Gu`; Firespider sends that
request through its existing asynchronous Lea loader. Relative URLs resolve
against the document base, unsafe schemes and remote-to-file requests are
rejected, and a command request overrides the same click's default link route.
Lea tabs also expose the bounded `V.Tm` request deadline: 15,000 ms by default,
clamped to 100-60,000 ms, with typed timeout and cancellation failures.

Same-page `#fragment` links stay inside the current Lea page and never enter the
host loader. Compact `Ft/Fg/Rv` lookup, retained history, closed-details reveal,
and layout scrolling still raise the normal `Changed` event, so Firespider's
address bar and Back/Forward controls update without adapter-specific DOM code.

Progress and meter elements remain inside Lea's custom renderer. Compact
`B.Vm` values provide bounded ranges, indeterminate progress, meter quality,
inherited painting, and table-cell reflow without adding a native browser or
per-gauge WinForms control to Firespider.

Firespider can read `V.Dg()` for bounded response, document, CSS, request,
command, cache, and cookie diagnostics without reflecting into Lea internals.
The snapshot omits mutable form values, passwords, and selected file paths.
**View > Lea Inspector** (`F12`) and the Lea tab context menu open Summary,
Nodes, CSS, Requests, and Messages tabs backed only by that snapshot. `R`/`F5`
refresh, `C` clears request activity through `V.Dcl()`, and `X`/Escape closes
the owned window. Legacy tabs remain on Firespider's original path.

Firespider's existing Find command (`Ctrl+F`) now calls Lea's highlighted,
case-insensitive match cycling for Lea tabs and keeps the original DOM range
path for legacy tabs. Lea download links open Firespider's Save dialog, honor its
default save folder, stream through `V.DlA`, show the full destination and byte
count in the status bar, and join Firespider's recent-download list.

The host project imports `Lea/FirespiderLea.targets`, which adds the two DLL
references, compiles `FirespiderLea.vb`, and copies `start.html` to the output
`Lea` folder. No Chromium, WebView, Safari, or hidden browser control is used.
If SDK `dotnet build` forces old binary `.resx` files through its newer resource
serializer, build this legacy host with the installed .NET Framework MSBuild:
`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe`. Lea source and
resources do not need to be changed for that tooling mismatch.

The adapter is kept here as the clean source of record because the live
Firespider working tree contains unrelated in-progress changes.

`Smoke.cs` loads two compiled Firespider forms off-screen and verifies Lea-first
New Tab creation, the explicit legacy fallback, Inspector open/refresh/clear and legacy isolation, diagnostics,
script/style, host-controlled `go`, and safe click execution, alert routing, keyboard navigation, local
open, find cycling, streamed download saves, page save, send-link metadata,
close routes, source, and session restoration using temporary files rather than
the user's data.
