# Lea Visual Studio embedding sample

Small WinForms host for the mature Lea.Engine/Lea.Win path, not the earlier
LeaBrowser prototype or a Chromium/WebView wrapper. No test-harness source dependency.

2026-10-26: modern worker packaging fixed and verified against the current
core52CE/view74956DA3 private runtime. Fresh installed VS2017 MSBuild build has
zero warnings/errors; all six worker files, including .deps.json and
.runtimeconfig.json, match the selected input bytes. Twelve layout-only controls
cover complete Framework/modern packages and missing required files. Native
window smoke passes seven checks; screenshot inspected. Receipt:
../continuity-evidence/embedding-modern-worker-v2/result.json, SHA256F8965695.
This does not prove active script-worker startup or repair the missing system
component-probe CLI. The previous project validation below remains historical.

Verified using installed Visual Studio2017 MSBuild with net48 references:
fresh output build, zero warnings/errors, native-window smoke7 checks PASS.
See ../continuity-evidence/embedding-sample-v1/PROJECT_RESULTS.md for exact
runtime/source hashes and limitations. This does not certify script startup.

Open Lea.Embed.Sample.csproj in Visual Studio with .NET Framework4.8 targeting
tools. Explicitly select a matching runtime using an MSBuild property:

```powershell
msbuild Lea.Embed.Sample.csproj /p:Configuration=Release /p:LeaRuntime="C:\path\to\matched-runtime"
```

LeaRuntime must contain matching engine/view/script DLLs, their dependencies,
Lea.Image.exe and the complete ScriptWorker directory. Do not mix individual
DLLs across candidates. No runtime is implicitly selected or certified here.
The project copies these runtime files, not an installed/default browser.
Modern-worker JSON sidecars are required when Lea.Script.Worker.dll is present;
the Framework-only worker layout does not require them. Missing files fail before
reference resolution. These are layout checks, not binary-identity or isolation
certification; select and retain an explicitly matched package.

Integration points: Vw is the owned Lea view; await Go(address) navigates.
W.On(Vw) attaches the existing isolated local-document script adapter with
unchanged defaults. Never cast Vw to System.Windows.Forms.WebBrowser. Normal
WinForms form disposal owns the view and its associated session lifecycle.
Back/forward/reload delegate directly to Lea. Errors remain in the status bar.
HTTP/HTTPS/UNC scripting remains disabled by the engine policy.

Run Lea.Embed.Sample.exe with an optional address. For bounded embedding checks:

```powershell
Lea.Embed.Sample.exe --smoke "C:\new-output\embedding.png"
```

Smoke opens the actual sample window, checks local markup, dormant script adapter,
two-page navigation/back/forward/reload and view completion, then captures and
closes it. It does NOT prove fresh script startup, confinement or modern-web
compatibility. The broader engine and actual Firespider gates remain mandatory.
See continuity evidence for the tested candidate and verification boundary.
