using System;
using System.Reflection;
using Lea.Engine;
using Lea.Script;

internal static class SchedulingRegressionMain
{
    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3 || (args[1] != "Timers" && args[1] != "Frames"))
                throw new ArgumentException("Supply original assembly, scheduling suite and fixture.");
            var assembly = Assembly.LoadFrom(args[0]);
            var type = assembly.GetType("Lea.Script.Tests." + args[1], true);
            if (args[1] == "Frames")
            {
                var run = type.GetMethod("Run", BindingFlags.NonPublic | BindingFlags.Static,
                    null, new[] { typeof(string), typeof(string) }, null);
                if (run == null) throw new InvalidOperationException("Original trusted frame entry differs.");
                run.Invoke(null, new object[] { args[2], "trusted" });
            }
            else
            {
                var signature = new[] { typeof(E), typeof(P), typeof(Func<string, Jr>), typeof(Func<Jr>),
                    typeof(Func<int>), typeof(Func<int>), typeof(Func<string>), typeof(string) };
                var suite = type.GetMethod("Suite", BindingFlags.NonPublic | BindingFlags.Static, null, signature, null);
                if (suite == null) throw new InvalidOperationException("Original timer suite shape differs.");
                var engine = new E { Jb = false };
                var page = engine.Go(args[2]);
                using (var script = new Js(engine, page))
                    suite.Invoke(null, new object[] { engine, page, new Func<string, Jr>(script.Ex),
                        new Func<Jr>(script.Tk), new Func<int>(() => script.Td), new Func<int>(() => script.Tn),
                        new Func<string>(() => ""), "in-process" });
            }
            int checks = (int)assembly.GetType("Lea.Script.Tests.Program", true)
                .GetField("checks", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            if (checks <= 0) throw new Exception("No original assertions executed.");
            Console.WriteLine("PASS Lea scheduling " + args[1] + ": " + checks + " checks.");
            return 0;
        }
        catch (TargetInvocationException error) { Console.Error.WriteLine(error.InnerException ?? error); return 1; }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
