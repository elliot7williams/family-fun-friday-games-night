function Project-DecoderRetirement([string]$Original){
    $edits=@(
        @('        private static string workerKey = "";', @'
        private static string workerKey = "";
        private static int shuttingDown;

        static Di()
        {
            AppDomain.CurrentDomain.ProcessExit += OnShutdown;
            AppDomain.CurrentDomain.DomainUnload += OnShutdown;
        }

        private static void OnShutdown(object sender, EventArgs args)
        {
            if (!Shutdown())
                try { Console.Error.WriteLine("LEA_DECODER_SHUTDOWN pending"); }
                catch (IOException) { }
        }

        internal static bool Shutdown()
        {
            Interlocked.Exchange(ref shuttingDown, 1);
            if (!Monitor.TryEnter(state, 1000)) return false;
            try
            {
                Drop();
                return true;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException ||
                error is InvalidOperationException || error is System.ComponentModel.Win32Exception ||
                error is System.Security.SecurityException)
            { return false; }
            finally { Monitor.Exit(state); }
        }
'@),
        @("            lock (state)`r`n            {`r`n                if (worker != null)", "            lock (state)`r`n            {`r`n                if (Interlocked.CompareExchange(ref shuttingDown, 0, 0) != 0) throw new InvalidOperationException(`"Image decoder is shutting down.`");`r`n                if (worker != null)"),
        @('if (!worker.P.HasExited && workerKey == key && worker.J != null) return worker;', 'if (worker.P != null && !worker.P.HasExited && workerKey == key && worker.J != null) return worker;'),
        @('running = !worker.P.HasExited;', 'running = worker.P != null && !worker.P.HasExited;'),
        @('if (worker != null) worker.Dispose();', 'if (worker != null) { worker.Dispose(); if (worker.P != null || worker.CleanupPending) throw new IOException("Image-worker retirement remains pending."); }'),
        @('if (created != null) created.Dispose();', 'RetireOpenFailure(created, error);'),
        @("                catch`r`n                {`r`n                    if (writer != null)", "                catch (Exception error)`r`n                {`r`n                    if (writer != null)"),
        @('        private static long Private(Process process)', @'
        private static void RetireFailedOpen(Rp created)
        {
            if (created == null) return;
            // Publish ownership before cleanup, which may fail or remain pending.
            worker = created;
            workerKey = "";
            Drop();
        }

        private static void RetireOpenFailure(Rp created, Exception cause)
        {
            var failed = cause as Rp.SetupFailure;
            try { RetireFailedOpen(created ?? (failed == null ? null : failed.Endpoint)); }
            catch (Exception cleanup) when (cleanup is IOException || cleanup is UnauthorizedAccessException ||
                cleanup is InvalidOperationException || cleanup is System.ComponentModel.Win32Exception ||
                cleanup is System.Security.SecurityException)
            { throw new Rp.SetupFailure(new AggregateException(cause, cleanup), worker); }
        }

        private static long Private(Process process)
'@)
    )
    $candidate=$Original
    foreach($edit in $edits){
        if([regex]::Matches($candidate,[regex]::Escape($edit[0])).Count -ne 1){throw 'Decoder retirement anchor differs'}
        $candidate=$candidate.Replace($edit[0],$edit[1])
    }
    $inverse=$candidate
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Decoder retirement inverse differs'}
    return $candidate
}
