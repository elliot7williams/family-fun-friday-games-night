using System;
using System.Reflection;
using Lea.Engine;
using Lea.Script;

internal static class IsolatedSchedulingRegressionMain
{
    [STAThread]
    public static int Main(string[] args)
    {
        var previous = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
        try
        {
            if (args.Length != 3 || (args[1] != "Timers" && args[1] != "Frames"))
                throw new ArgumentException("Supply original assembly, scheduling suite and fixture.");
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");
            Console.Error.WriteLine("LEA_ALL_STARTUP_TRACE enabled");
            var assembly = Assembly.LoadFrom(args[0]);
            var type = assembly.GetType("Lea.Script.Tests." + args[1], true);
            if (args[1] == "Frames")
            {
                var run = type.GetMethod("Run", BindingFlags.NonPublic | BindingFlags.Static,
                    null, new[] { typeof(string), typeof(string) }, null);
                if (run == null) throw new InvalidOperationException("Original isolated frame entry differs.");
                run.Invoke(null, new object[] { args[2], "isolated" });
            }
            else
            {
                var signature = new[] { typeof(E), typeof(P), typeof(Func<string, Jr>), typeof(Func<Jr>),
                    typeof(Func<int>), typeof(Func<int>), typeof(Func<string>), typeof(string) };
                var suite = type.GetMethod("Suite", BindingFlags.NonPublic | BindingFlags.Static, null, signature, null);
                if (suite == null) throw new InvalidOperationException("Original timer suite shape differs.");
                var engine = new E { Jb = false };
                var page = engine.Go(args[2]);
                var script = new Jp(engine, page) { Tm = 5000 };
                int guards = 0;
                try
                {
                    Func<string, Jr> execute = code =>
                    {
                        var result = script.Ex(code);
                        if (script.Pid != 0)
                        {
                            if (!script.Ac || !script.Nd || !script.Ne || !script.Fd || script.Cp != 2500 || script.Mm != 128L * 1024 * 1024)
                                throw new Exception("Isolated timer guards changed.");
                            guards++;
                        }
                        return result;
                    };
                    suite.Invoke(null, new object[] { engine, page, execute, new Func<Jr>(script.Tk),
                        new Func<int>(() => script.Td), new Func<int>(() => script.Tn), new Func<string>(() => script.Sd), "isolated" });
                    if (guards == 0) throw new Exception("No live worker guard observation.");
                }
                finally { script.Dispose(); }
                if (script.Pc || script.Pid != 0) throw new Exception("Isolated timer worker retirement incomplete.");
                Console.WriteLine("LEA_SCHEDULING_GUARDS " + guards);
            }
            int checks = (int)assembly.GetType("Lea.Script.Tests.Program", true)
                .GetField("checks", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            if (checks <= 0) throw new Exception("No original assertions executed.");
            Console.WriteLine("PASS Lea isolated scheduling " + args[1] + ": " + checks + " checks.");
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
