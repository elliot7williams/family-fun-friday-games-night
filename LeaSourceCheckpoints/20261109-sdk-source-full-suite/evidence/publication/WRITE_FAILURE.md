# Source Checkpoint Write Interruption

After the passing full campaign and independent audit, an apply_patch update
failed to write the newly created C: Publish-CurrentSdkFullCheckpoint.ps1.
Readback found that file empty. The edit stopped on that first target; the
proposed PROJECT_CONTEXT update and publisher invocation did not execute.
No test was retried, and no engine/runtime/test receipt was modified.

C: free had fluctuated from373MiB to635MiB; the later recovery check reported
1087950848bytes free. The exact cause of the write failure is unproven.
The publisher was recreated on I: first and mechanically copied back to C:
with byte-hash verification before use. This keeps a recovery copy off C:.
No cleanup-policy bypass, source deletion or pagefile-policy change occurred.
