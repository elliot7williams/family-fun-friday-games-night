# Exact Cache Identity

Milestone 68 / runtime 168 makes Lea's document and stylesheet caches preserve
the exact identity of HTTP, HTTPS, file, about, and data resources.

## Contract

Lea first uses .NET `Uri` canonicalization, then compares the resulting absolute
address with ordinal string equality. This normalizes URI syntax that is already
equivalent, including scheme and host casing and an explicit default HTTP port.
It does not fold path or query casing.

The fragment is removed only when constructing a representation key. Therefore:

- `/Page?Mode=One` and `/page?Mode=One` are different resources.
- `/page?Mode=One` and `/page?Mode=one` are different resources.
- `/page?Mode=One#top` and `/page?Mode=One#next` share one representation.
- `HTTP://EXAMPLE.COM:80/Page` and `http://example.com/Page` use the same
  canonical absolute address.

The rule applies to the in-memory LRU, persistent cache filenames and stored-key
validation, stale validators, linked stylesheets, same-document fragment checks,
and full-address navigation checks. A validator from one path/query spelling is
never sent for another spelling. Case-different navigation with a fragment is a
real load rather than a fragment-only move.

No new public alias is required. Existing compact `Cm/Cc/Ch/Cl` memory aliases
and `Cd/Cx/Cb/Dc/Db/Cdl` persistent aliases retain their behavior with exact
resource identity.

## Persistent Cache Upgrade

Older Lea runtimes derived persistent filenames from an upper-cased key. Runtime
168 hashes the exact UTF-8 key instead. Most older files are consequently not
addressed by the new runtime and remain bounded by the existing entry/byte trim
or can be removed with `Cdl()`.

If an exact new key happens to address an older filename, ordinal stored-key
validation rejects a different spelling and recovers through the normal network
path. Lea does not guess, merge, or automatically migrate case-folded entries.

## Verification

`Lea.Tests <root> --cache` covers synchronous and asynchronous memory loads,
path/query casing, fragments, validators, linked CSS, navigation, persistent
restart reuse, and separate disk files. `Lea.Firespider.Boot.exe <root> cache`
drives the same behavior through a real selected Lea tab and checks request-owner
cleanup, startup errors, and absence of MSHTML.

`cache-identity.html` is also a visible benchmark fixture. The benchmark's
`Exact cache identity` probe checks the private representation key, distinct
path/query spellings, shared fragment keys, authored URL retention, and native
paint output.

## Remaining Limits

This closes Lea's known path/query case collision. It does not implement the
full HTTP caching model, including `Vary`, shared-cache partitioning, service
workers, or every RFC cache directive. Image cookie/cache integration and
process-wide memory accounting remain separate work.
