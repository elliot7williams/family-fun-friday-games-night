# Lea Main-Tab Navigation Policy

Milestone 60 connects Firespider's ordinary Lea tabs to the existing compact
`V.Ng` document-navigation guard. No new public alias, browser dependency, or
page-script permission API is added.

## Host Rules

Every mounted Lea tab checks Firespider's blocked-site list, enabled navigation
content filters and their exceptions, work-offline state, and the tab's locked
state. The existing external-navigation confirmation remains host-owned.
Offline and locked tabs reject file/fragment navigation too, matching the legacy
handler. The checks cover explicit GET, GET/POST/multipart form submission,
script-requested navigation, reload, retained fragments, and Back/Forward.
Rejection occurs before the explicit fetch or retained history change.

The guard requires a live owner and an owned tab whose current renderer is the
requesting view. Owned hidden-group tabs remain valid; detached and replaced
views cannot authorize navigation. Private tabs use the same rules without
changing their private context. Disposal clears the callback.

Resume checks the saved target before destroying its suspended placeholder.
A denial preserves that placeholder and URL. Paused address, search, and Home
commands also normalize and check their new target before replacing the
placeholder; an empty input leaves it untouched. An accepted target receives one
exact-URL approval for the immediately following initial load, on the same UI
thread with no intervening await. The approval is consumed on the first guard
call, avoiding a duplicate confirmation dialog. Later navigation rechecks rules.
Ordinary new-tab creation and denied unshown-window loads remain usable without
throwing an expected denial into a browser event callback.
A denied initial/restored tab remains empty and can navigate elsewhere; a
dedicated per-tab error page is not part of this change.

## Input Error Handling

The previous renderer reproducibly let a rejected keyboard link escape through
`V.OnKeyDown` as an unhandled `Er`. Native Enter/access-key and accessibility
activation now handle only `Er.K="blocked"`. The key is consumed; the host's
denial status remains available. Unexpected failures are not hidden by this
new boundary. Existing mouse and command-key catch behavior is unchanged.

Public `Go/GoA`, `Ka/Aa`, `Sm/SmA`, history, and reload APIs still report typed
denials to their callers. Firespider catches expected denials at its navigation
command boundary so Back/Forward and unshown initial loads do not raise popups.

## Verification

The `policy` Boot case uses loopback request counts, actual history/Home menu
dispatch, keyboard callbacks, the accessibility activation boundary, forms,
script requests, content-filter exceptions, domain boundaries, offline/lock
states, hidden/private tabs, denied and accepted resume, and disposal.
`policy-start` restores a blocked URL beside an allowed local page, checks zero
blocked requests, then navigates the empty first tab successfully.

The main-tab direct-navigation test fails against runtime 159. A second run with
the new host adapter but the runtime-159 renderer reproduces the unhandled
keyboard rejection. Updated code passes the focused policy test.

Final review also reproduced loss of a suspended URL after a blocked address
command in the first runtime-160 candidate. The final candidate checks address,
search, and Home before mounting a replacement view; regression coverage also
checks empty input, offline/locked tabs, accepted replacement, and one-use
approval consumption. Core coverage checks that public APIs and unexpected
errors retain their original behavior;
the compatibility runner adds a native-input probe. The Boot runner now has
30 cases and the compatibility runner has 60 probes across 59 local fixtures.

## Limits And Startup Status

This is explicit document-navigation policy, not a security sandbox or network
firewall. Redirect hops, page resources, and direct download APIs remain outside
`Ng`; the existing host rule helper also retains its legacy exception behavior.
Split view keeps its separately installed policy. Content-filter DOM rewriting,
popup policy, printing/properties, and automatic hibernation remain separate work.

The earlier intermittent recovery-time Lea-to-WebBrowser cast is still
unidentified. This keyboard failure is a separate confirmed issue, not evidence
that the recovery issue is fixed. Runtime 160 is an isolated test build; the
live root runtime 157 remains unchanged. Compression measures package size,
not page execution speed or arbitrary modern-web compatibility.
