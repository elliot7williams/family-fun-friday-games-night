using System;

internal static class PrintGeometryMain
{
    private static int Main()
    {
        try
        {
            int snapshot = PrintSnapshotChecks.Run(), geometry = PrintGeometryChecks.Run();
            if (snapshot != 40 || geometry != 20) throw new Exception("Unexpected check counts");
            Console.WriteLine("PASS Lea print snapshot and geometry: 60 checks"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
