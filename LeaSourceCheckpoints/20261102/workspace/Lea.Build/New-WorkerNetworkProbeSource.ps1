function New-WorkerNetworkProbeSource {
    param([string]$Program,[string]$Probe)
    function Overlay([string]$Text,[object[]]$Edits){
        $original=$Text.Replace("`r`n","`n");$updated=$original
        foreach($edit in $Edits){
            if([regex]::Matches($updated,[regex]::Escape($edit.old)).Count -ne 1){throw 'Network diagnostic anchor differs'}
            $updated=$updated.Replace($edit.old,$edit.new)
        }
        $inverse=$updated
        for($i=$Edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($Edits[$i].new,$Edits[$i].old)}
        if($inverse -cne $original){throw 'Network diagnostic inverse differs'}
        return $updated
    }
    $call='                using (var probe = new NetworkProbe(Wire.Number(init, "p")))'
    $programText=Overlay $Program @(@{old=$call;new=('                Trace("network-before", clock, trace);'+"`n"+'                using (var probe = new NetworkProbe(Wire.Number(init, "p"), clock, trace))')})
    $helper=@'
        private static void ProbeTrace(string stage, Stopwatch startupClock, bool enabled)
        {
            if (!enabled) return;
            using (var process = Process.GetCurrentProcess())
                Console.Error.WriteLine("network-probe " + stage + ": wall=" + startupClock.ElapsedMilliseconds +
                    "; cpu=" + (long)process.TotalProcessorTime.TotalMilliseconds + " ms");
        }

'@
    $dispose='        public void Dispose() { if (client != null) client.Close(); }'
    $ctor='        internal NetworkProbe(int port)'
    $validate='            if (port < 1 || port > 65535) return;'
    $client='                client = new TcpClient();'
    $connect='                connect = client.BeginConnect(IPAddress.Loopback, port, null, null);'
    $probeText=Overlay $Probe @(
        @{old=$ctor;new='        internal NetworkProbe(int port, Stopwatch startupClock, bool trace)'},
        @{old=$validate;new=('            ProbeTrace("entered", startupClock, trace);'+"`n"+$validate)},
        @{old=$client;new=('                ProbeTrace("client-before", startupClock, trace);'+"`n"+$client+"`n"+'                ProbeTrace("client-after", startupClock, trace);')},
        @{old=$connect;new=('                ProbeTrace("connect-before", startupClock, trace);'+"`n"+$connect+"`n"+'                ProbeTrace("connect-after", startupClock, trace);')},
        @{old=$dispose;new=($helper+"`n"+$dispose)}
    )
    @{program=$programText;probe=$probeText;inverseVerified=$true;normalization='CRLF to LF only before inverse comparison';scope='Timing-only source overlay; original socket/denial/cleanup statements preserved, not a production optimization'}
}
