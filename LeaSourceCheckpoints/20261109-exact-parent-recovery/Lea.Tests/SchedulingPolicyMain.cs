using System;
using System.Threading;
using System.Web.Script.Serialization;
using Lea.Build;

internal static class SchedulingPolicyMain
{
    private static int Main()
    {
        try
        {
            var before = CurrentSchedulingPolicy.Capture();
            Thread.Sleep(100);
            var after = CurrentSchedulingPolicy.Capture();
            if (before.Pid != after.Pid || before.Tid != after.Tid || before.BirthFileTime != after.BirthFileTime)
                throw new InvalidOperationException("Self identity changed between snapshots.");
            Console.WriteLine("LEA_SCHEDULING_POLICY " + new JavaScriptSerializer().Serialize(new {
                before, after, selfIdentityStable = true, settingsChanged = false,
                scope = "Two self snapshots in an owned native control. No interpreter, priority changes or ancestor-policy query."
            }));
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
