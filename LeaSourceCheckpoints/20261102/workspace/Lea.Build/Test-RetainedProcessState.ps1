$ErrorActionPreference='Stop'
$path=Join-Path $PSScriptRoot '../Lea.Win/Rp.cs'
$source=[IO.File]::ReadAllText($path)
# Use the actual implementation without creating or modifying an AppContainer.
$start=$source.IndexOf('        internal static string ProcessState(SafeProcessHandle handle)',[StringComparison]::Ordinal)
$end=$source.IndexOf('        private Rp(', $start,[StringComparison]::Ordinal)
if($start -lt 0 -or $end -lt 0){throw 'Process-state source shape changed'}
$methods=$source.Substring($start,$end-$start)
$code=@'
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
public static class RetainedStateTest {
    [DllImport("kernel32.dll", SetLastError=true)]
    private static extern uint WaitForSingleObject(SafeProcessHandle process, uint milliseconds);
'@ + $methods + @'
    public static void Run() {
        if (ProcessState(null) != "unavailable") throw new Exception("null handle");
        using (var invalid = new SafeProcessHandle(IntPtr.Zero, false))
            if (ProcessState(invalid) != "unavailable") throw new Exception("invalid handle");
        var closed = new SafeProcessHandle(IntPtr.Zero, false);
        closed.Dispose();
        if (ProcessState(closed) != "unavailable") throw new Exception("closed handle");
        using (var current = Process.GetCurrentProcess())
            if (ProcessState(current.SafeHandle) != "running") throw new Exception("live process");
        using (var child = new Process()) {
            child.StartInfo = new ProcessStartInfo(Environment.GetEnvironmentVariable("ComSpec"), "/d /c exit 23") {
                UseShellExecute=false, CreateNoWindow=true
            };
            if (!child.Start()) throw new Exception("start failed");
            var handle = child.SafeHandle;
            if (!child.WaitForExit(5000)) throw new Exception("test child did not exit");
            if (ProcessState(handle) != "exited=23") throw new Exception("exit code");
        }
    }
}
'@
Add-Type -TypeDefinition $code
[RetainedStateTest]::Run()
'PASS retained process state: 5 checks; null, invalid, closed, live, original exited handle/code.'
