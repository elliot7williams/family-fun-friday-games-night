using System;

internal static class PrintUnifiedMain
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (PrintSnapshotChecks.Run() != 40 || PrintGeometryChecks.Run() != 20)
                throw new Exception("Unexpected managed check counts");
            if (PrintCaptureProbe.Run(args) != 0) return 1;
            Console.WriteLine("PASS Lea unified print foundation: 73 checks"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
