using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Web.Script.Serialization;

public static class LeaExceptionDeliveryControl
{
    public static int Main()
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
        bool passed = own == 1 && other == 1 && joined && beforeRemovalCheck == own;
        Console.WriteLine(new JavaScriptSerializer().Serialize(new { passed, own, other,
            handlerRemovalVerified = beforeRemovalCheck == own, threadJoined = joined,
            framework = Environment.Version.ToString(), engineExecuted = false, productionChanged = false }));
        return passed ? 0 : 1;
    }
    private static void ThrowAndCatch()
    {
        try { throw new ApplicationException("Lea bounded delivery control"); }
        catch (ApplicationException) { }
    }
}
