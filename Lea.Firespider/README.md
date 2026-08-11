# Lea Firespider Adapter

`FirespiderLea.vb` is a reversible experimental integration for the VB.NET
Firespider host. It adds **File > New Lea Tab** (`Ctrl+Alt+L`) and routes the
existing back, forward, home, go, search, reload, and stop controls to Lea only
while a Lea tab is selected. Legacy tabs delegate to Firespider's original
handlers. Its timer status handler is also routed safely so it never casts a
Lea view to the legacy `WebBrowser` type.

The host project imports `Lea/FirespiderLea.targets`, which adds the two DLL
references, compiles `FirespiderLea.vb`, and copies `start.html` to the output
`Lea` folder. No Chromium, WebView, Safari, or hidden browser control is used.

The adapter is kept here as the clean source of record because the live
Firespider working tree contains unrelated in-progress changes.

`Smoke.cs` loads the compiled Firespider form off-screen and verifies the menu,
Lea tab creation, bundled start page, and routing of Firespider's Back control.
