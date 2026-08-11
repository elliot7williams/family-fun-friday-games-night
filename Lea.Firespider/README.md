# Lea Firespider Adapter

`FirespiderLea.vb` is a reversible experimental integration for the VB.NET
Firespider host. It adds **File > New Lea Tab** (`Ctrl+Alt+L`) and routes the
existing back, forward, home, go, search, reload, and stop controls to Lea only
while a Lea tab is selected. Legacy tabs delegate to Firespider's original
handlers. Its timer status handler is also routed safely so it never casts a
Lea view to the legacy `WebBrowser` type.

Firespider's Close Tab command, tab-strip close target, and middle-click now
close Lea tabs safely. The existing Page Source window displays Lea's raw `V.H`
HTML. Up to 12 Lea URLs restore from `%LocalAppData%\Firespider Internet\Lea.session`;
private mode deletes that file instead of saving it. Legacy session settings
remain owned by Firespider.

The host project imports `Lea/FirespiderLea.targets`, which adds the two DLL
references, compiles `FirespiderLea.vb`, and copies `start.html` to the output
`Lea` folder. No Chromium, WebView, Safari, or hidden browser control is used.

The adapter is kept here as the clean source of record because the live
Firespider working tree contains unrelated in-progress changes.

`Smoke.cs` loads two compiled Firespider forms off-screen and verifies the menu,
tab creation, start page, navigation, close routes, source window, session save,
and session restoration using a temporary file rather than the user's session.
