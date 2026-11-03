using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lea.Engine;
using Lea.Script;

internal static class SdkParentIoContractMain
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) return 2;
            var engine = new E(); var page = engine.Go(args[0]);
            var dispatch = typeof(Jp).GetMethod("Io", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(typeof(Thread));
            var ids = new HashSet<int>(); Thread first = null;
            using (var script = new Jp(engine, page))
            {
                for (var i = 0; i < 32; i++)
                {
                    var task = (Task<Thread>)dispatch.Invoke(script, new object[] {
                        new Func<Thread>(() => Thread.CurrentThread), Stopwatch.StartNew(), 3000, false });
                    var thread = task.GetAwaiter().GetResult(); first = first ?? thread; ids.Add(thread.ManagedThreadId);
                }
                Console.WriteLine("CONTRACT_THREADS " + ids.Count);
                Check(ids.Count == 1, "parent I/O reuses one thread across sequential operations");
                Check(first.IsAlive && first.IsBackground && !first.IsThreadPoolThread && first != Thread.CurrentThread,
                    "parent I/O uses dedicated background thread outside owner and shared pool");
            }
            Check(first.Join(2000), "parent I/O disposal retires dedicated dispatcher");
            Console.WriteLine("PASS SDK PARENT IO CONTRACT " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
