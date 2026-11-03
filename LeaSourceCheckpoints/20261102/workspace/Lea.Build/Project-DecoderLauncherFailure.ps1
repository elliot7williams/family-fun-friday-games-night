function Project-DecoderLauncherFailure([string]$Original){
    $edits=@(
        @('return Open(path, args, directory, memory, cpu, ContainerName, new[] { path }, false);', 'return Open(path, args, directory, memory, cpu, ContainerName + "." + Guid.NewGuid().ToString("N"), new[] { path }, true);'),
        @('        private Func<bool> release;', "        private Func<bool> release;`n        private bool terminateOnDispose;"),
        @("            catch`r`n            {`r`n                if (job != null) job.Dispose();", @'
            catch (Exception error)
            {
                if (temporaryProfile && identity != null)
                {
                    var failed = new Rp(process, input, output, errors ?? Stream.Null, job, false, false, true, true, false);
                    failed.processHandle = retained;
                    if (failed.processHandle == null && created && info.Process != IntPtr.Zero)
                    {
                        failed.processHandle = new SafeProcessHandle(info.Process, true);
                        info.Process = IntPtr.Zero;
                    }
                    failed.terminateOnDispose = created && job == null;
                    failed.Sid = identity.Value;
                    failed.B = true;
                    failed.release = () => Release(profile, files, identity);
                    try { failed.Dispose(); }
                    catch (Exception cleanup) when (cleanup is IOException || cleanup is UnauthorizedAccessException ||
                        cleanup is InvalidOperationException || cleanup is Win32Exception || cleanup is System.Security.SecurityException)
                    { throw new SetupFailure(new AggregateException(error, cleanup), failed); }
                    if (failed.P != null || failed.processHandle != null || failed.CleanupPending)
                        throw new SetupFailure(error, failed);
                    throw;
                }
                if (job != null) job.Dispose();
'@),
        @('            if (J != null) J.Dispose();', @'
            if (J != null) J.Dispose();
            else if (terminateOnDispose && processHandle != null && !processHandle.IsClosed && !processHandle.IsInvalid)
            {
                try { TerminateRetained(processHandle, 1); }
                catch (ObjectDisposedException) { return; }
            }
'@),
        @('        internal bool CleanupPending { get { return release != null; } }', @'
        internal bool CleanupPending { get { return release != null; } }

        internal sealed class SetupFailure : InvalidOperationException
        {
            internal Rp Endpoint { get; private set; }
            internal SetupFailure(Exception cause, Rp endpoint)
                : base("Worker setup failed; cleanup remains pending. " + cause.Message, cause)
            { Endpoint = endpoint; }
        }

        [DllImport("kernel32.dll", EntryPoint = "TerminateProcess", SetLastError = true)]
        private static extern bool TerminateRetained(SafeProcessHandle process, uint exitCode);
'@)
    )
    $candidate=$Original
    foreach($edit in $edits){
        if([regex]::Matches($candidate,[regex]::Escape($edit[0])).Count -ne 1){throw 'Launcher failure anchor differs'}
        $candidate=$candidate.Replace($edit[0],$edit[1])
    }
    $inverse=$candidate
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Launcher failure inverse differs'}
    return $candidate
}
