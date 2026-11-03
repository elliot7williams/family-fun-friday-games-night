# Lea Portable Core/View/Image Candidate

This intermediate source layout contains the matched core/view/image sources,
relative-reference net48 projects, a WinForms sample and previously tested
binaries. It is not the complete SDK or a final browser release. JavaScript,
printing, complete notices and clean-machine qualification remain unfinished.

Use a modern Roslyn compiler supporting C#12 and the .NET Framework4.8 targeting
pack. From a Visual Studio Developer Command Prompt:

```bat
MSBuild SDK.Build.proj /m:1 /nodeReuse:false /p:Configuration=Release
```

Core/view/image outputs go to bin/Release; the sample goes to Sample/bin/Release.
The sample project copies Lea.Image.exe beside its EXE. It uses Lea.Win.V, not
WebBrowser/WebView. Sample startup opens bundled local HTML; it also accepts one
local HTML path as an argument. Missing workers must fail closed, not fall back
to unrestricted in-process decoding.

The bundle manifest records source and binary hashes. Existing binaries have
candidate-specific evidence; rebuilding can produce different identities and
requires new verification. The aggregate source build and complete portable SDK
are not certified merely by copying previously tested components. Full browser
compatibility, legacy UIA, startup reliability and remote scripting remain open.

Framework/runtime assemblies are prerequisites, not bundled redistributables.
Licensing/provenance review for the full source SDK remains incomplete; no new
license or third-party redistribution permission is asserted by this candidate.
