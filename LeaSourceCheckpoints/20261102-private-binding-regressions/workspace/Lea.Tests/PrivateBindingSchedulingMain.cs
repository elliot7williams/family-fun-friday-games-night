using System;

internal static class PrivateBindingSchedulingMain
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length != 4 || (args[3] != "trusted" && args[3] != "isolated"))
            throw new ArgumentException("Supply original assembly, suite, fixture and mode.");
        var original = new[] { args[0], args[1], args[2] };
        if (args[3] == "isolated") return IsolatedSchedulingRegressionMain.Main(original);
        var previous = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
        try
        {
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");
            Console.Error.WriteLine("LEA_ALL_STARTUP_TRACE enabled");
            return SchedulingRegressionMain.Main(original);
        }
        finally
        {
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previous);
            Console.Error.WriteLine("LEA_ALL_STARTUP_TRACE restored=" + (Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE") == previous).ToString().ToLowerInvariant());
        }
    }
}
