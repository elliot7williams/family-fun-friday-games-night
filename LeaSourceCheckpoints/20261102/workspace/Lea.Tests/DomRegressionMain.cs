using System;
using System.Reflection;

internal static class DomRegressionMain
{
    [STAThread]
    public static int Main(string[] args)
    {
        string previous = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
        try
        {
            if (args.Length != 3 || (args[1] != "Selectors" && args[1] != "Tree" && args[1] != "Attributes" && args[1] != "ClassLists")) throw new ArgumentException("Supply original test assembly, suite and fixture.");
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");
            Console.Error.WriteLine("LEA_ALL_STARTUP_TRACE enabled");
            var assembly = Assembly.LoadFrom(args[0]);
            var type = assembly.GetType("Lea.Script.Tests." + args[1], true);
            var method = type.GetMethod("Run", BindingFlags.NonPublic | BindingFlags.Static);
            var parameters = method.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string)) method.Invoke(null, new object[] { args[2] });
            else if (args[1] == "Tree" && parameters.Length == 2 && parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(bool)) method.Invoke(null, new object[] { args[2], false });
            else throw new InvalidOperationException("Original regression method shape differs.");
            int checks = (int)assembly.GetType("Lea.Script.Tests.Program", true).GetField("checks", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            if (checks <= 0) throw new Exception("Original suite performed no assertions.");
            Console.WriteLine("PASS Lea DOM regression " + args[1] + ": " + checks + " checks.");
            return 0;
        }
        catch (TargetInvocationException error) { Console.Error.WriteLine(error.InnerException ?? error); return 1; }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally
        {
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previous);
            Console.Error.WriteLine("LEA_ALL_STARTUP_TRACE restored=" + (Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE") == previous).ToString().ToLowerInvariant());
        }
    }
}
