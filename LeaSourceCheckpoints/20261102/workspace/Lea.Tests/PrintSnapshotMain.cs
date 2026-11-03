using System;

internal static class PrintSnapshotMain
{
    private static int Main()
    {
        try
        {
            int checks = PrintSnapshotChecks.Run();
            if (checks != 40) throw new Exception("Unexpected check count: " + checks);
            Console.WriteLine("PASS Lea Framework print snapshot: 40 checks");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
