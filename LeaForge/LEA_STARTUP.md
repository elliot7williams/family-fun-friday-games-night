# Lea-First Firespider Startup

Milestone 50 moves the normal first-page path onto Lea, not a hidden or
background Windows WebBrowser control. The configured homepage/new-tab target
is retained. Toolbar New Tab, the New Lea Tab menu, Ctrl+T, and Help use Lea;
New Legacy Tab remains an explicit compatibility option.

## Restoration

The first non-private window chooses one source, in this order:

1. Accepted crash-recovery URLs.
2. The saved Lea session.
3. Legacy saved URLs when restore-session is enabled, now opened as Lea tabs.
4. One configured new-tab page.

Only one source is used, without an extra initial legacy tab. The first restored
tab is selected. Later windows open one new Lea tab without repeating restoration.
Private startup skips both saved-session sources and crash-recovery prompting.
The shared restored-URL path checks at most 256 input entries, accepts at most
twelve HTTP(S)/file URLs, limits normalized URLs to 4,096 characters, and limits
their UTF-8 total to 64 KiB. Lea session-file input retains its 64 KiB ceiling.

Startup calls the existing asynchronous Lea loader even before the form becomes
visible. Recovery snapshots now obtain URLs from Lea views through the host's
existing persisted-tab URL helper. A failed session-file write no longer skips
the rest of normal shutdown and incorrectly leaves the clean-exit flag unset.

## Executable Config Fix

The real EXE exposed a startup ConfigurationErrorsException at `app.config`
line 141 that the older form-only harness did not see. The XML was well-formed,
but the installed .NET Framework settings reader rejected consecutive compact
settings entries. Expanding 59 single-line entries fixed deserialization; all
89 setting nodes were compared before/after with whitespace-normalized XML and
their values were unchanged. Keep this config readable; compress distributions
as archives instead of removing its XML whitespace.

Both launch test scripts now parse the actual EXE config through
`ConfigurationManager.OpenMappedExeConfiguration` and `ClientSettingsSection`
before testing the UI. This catches the configuration failure without needing
to wait for a JIT dialog.

## Verification Tools

`Lea.Build/Test-FirespiderBoot.ps1` compiles and runs `Lea.Firespider/Boot.cs`
in separate, off-screen test processes. Its ten cases cover clean, Lea-saved,
legacy-saved, crash-recovery, private, second-window, restore-limit, invalid-URL,
slow-network, and locked-session shutdown behavior. Tests assert actual Lea
control ownership, ordering, primary tab routes, recovery snapshots, and absence
of `mshtml.dll` before explicitly testing the legacy fallback. A gated local HTTP
response verifies that a visible form and busy Lea view exist before the server
responds. Each case has a sixty-second process watchdog.

`Test-FirespiderStartup.ps1` launches the actual Firespider EXE with clean,
restored, locked, and oversized temporary sessions. It now waits for the Lea
UI Automation document instead of accepting just an early top-level window,
checks for error/recovery dialogs during that wait, and rejects MSHTML loading.
It refuses to run alongside an existing Firespider process and closes only
its own process. A separate extracted runtime directory keeps package tests
apart from the live executable's profile.

The existing full engine suite, eighteen event/session regressions, full compiled
host suite, and 57 compatibility probes remain separate verification gates.

## Source And Remaining Work

`Lea.Firespider/Lea-first-host.patch` preserves the host startup, Help, private
prompt, and persisted-URL edits. `Settings-format.patch` preserves the config
fix. These follow milestone 49's `Startup-host.patch` and require the matching
`FirespiderLea.vb`. Check applicability before applying to another host checkout;
all three are already applied to the live host. Full source ZIP backups remain
the additional copy of the otherwise dirty Firespider checkout.

This is not a startup-speed claim. The real form harness observed roughly
11-19 seconds inside Form.Show including existing host initialization, distinct
from the small engine-only startup sample in COMPATIBILITY_PERFORMANCE.md.
Milestone 51 profiles and improves that cost; see STARTUP_PERFORMANCE.md for
the paired menu-layout measurements and remaining startup costs.
Advanced legacy-only commands and lazy/hibernated legacy placeholders still need
migration; this milestone does not claim every Firespider feature is Lea-only
or that Lea implements the full modern web platform.
