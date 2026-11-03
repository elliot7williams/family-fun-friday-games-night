$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/framework-exception-delivery-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve delivery control'}
if($PSVersionTable.PSEdition -ne 'Desktop'){throw 'Run in Windows PowerShell for .NET Framework control'}
Add-Type -TypeDefinition @'
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
public static class LeaExceptionDeliveryControl
{
    public static int[] Run()
    {
        int owner = Thread.CurrentThread.ManagedThreadId;
        int own = 0, other = 0;
        EventHandler<FirstChanceExceptionEventArgs> handler = (s, e) => {
            if (!(e.Exception is ApplicationException)) return;
            if (Thread.CurrentThread.ManagedThreadId == owner) own++;
            else Interlocked.Increment(ref other);
        };
        bool joined;
        AppDomain.CurrentDomain.FirstChanceException += handler;
        try
        {
            ThrowAndCatch();
            var thread = new Thread(ThrowAndCatch) { IsBackground = true };
            thread.Start();
            joined = thread.Join(2000);
        }
        finally { AppDomain.CurrentDomain.FirstChanceException -= handler; }
        int beforeRemovalCheck = own;
        ThrowAndCatch();
        return new[] { own, other, beforeRemovalCheck, joined ? 1 : 0 };
    }
    private static void ThrowAndCatch()
    {
        try { throw new ApplicationException("Lea bounded delivery control"); }
        catch (ApplicationException) { }
    }
}
'@
$counts=[LeaExceptionDeliveryControl]::Run()
$result=@{passed=$counts[0] -eq 1 -and $counts[1] -eq 1 -and $counts[2] -eq 1 -and $counts[3] -eq 1;own=$counts[0];other=$counts[1];handlerRemovalVerified=$counts[0] -eq $counts[2];threadJoined=$counts[3] -eq 1;framework=[Environment]::Version.ToString();engineExecuted=$false;productionChanged=$false;sourceSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash}
$null=New-Item -ItemType Directory -Path $out
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json))
$result|ConvertTo-Json
if(!$result.passed){throw 'First-chance delivery control failed'}
