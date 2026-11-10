# Image Source Graph Audit Stop

The first audit stopped at `Decoder lifecycle scope differs`, before creating
its output directory. The preserved recipe has SHA256
`B9BC0BB0E0EA2F2DB96712D15C70BA9A30F6CF96B609D065010ECAD7CA48373C`.

Its assertion incorrectly required decoderProfileChanged=false. The existing
Test-DecoderRetirement.ps1 unconditionally sets this field true after every
real-worker campaign. The corrected auditor requires the existing true flag
and realWorkerCampaign=true; original receipts and native tests are unchanged.
Neither flag alone establishes a production policy change or global security.
Product hashes, source pins, ACL restoration, deadlines and ownership checks
remain required. The original audit stop is not a native test failure.
