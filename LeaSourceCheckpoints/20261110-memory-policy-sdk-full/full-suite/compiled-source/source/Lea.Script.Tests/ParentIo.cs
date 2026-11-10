using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class ParentIo
    {
        private static readonly MethodInfo Io = typeof(Jp).GetMethod("Io", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo Reset = typeof(Jp).GetMethod("Reset", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void Run(string fixture)
        {
            Reuse(fixture);
            View.Pump(() => Bounded(fixture));
            View.Pump(() => Retire(fixture));
        }

        private static Task<T> Dispatch<T>(Jp script, Func<T> action, bool asynchronous = false)
        {
            return (Task<T>)Io.MakeGenericMethod(typeof(T)).Invoke(script,
                new object[] { action, Stopwatch.StartNew(), 3000, asynchronous });
        }

        private static void Reuse(string fixture)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            var owner = Thread.CurrentThread.ManagedThreadId;
            var threads = new HashSet<int>();
            Thread first = null, next = null;
            var script = new Jp(engine, page);
            try
            {
                for (var index = 0; index < 32; index++)
                    threads.Add(Dispatch(script, () =>
                    { first = first ?? Thread.CurrentThread; return Thread.CurrentThread.ManagedThreadId; }).GetAwaiter().GetResult());
                Console.WriteLine("Parent I/O dispatch threads: " + threads.Count + " for 32 sequential actions");
                Program.Check(threads.Count == 1, "parent I/O reuses one thread across sequential operations");
                Program.Check(first.IsAlive && first.IsBackground && !first.IsThreadPoolThread && !threads.Contains(owner),
                    "parent I/O is a dedicated background thread outside the owner and shared pool");
                var expected = new IOException("controlled I/O failure");
                Exception failure = null;
                try { Dispatch<int>(script, () => { throw expected; }).GetAwaiter().GetResult(); }
                catch (Exception error) { failure = error; }
                Program.Check(Object.ReferenceEquals(failure, expected), "parent I/O retains the original action exception");
                var recovered = Dispatch(script, () => Thread.CurrentThread.ManagedThreadId).GetAwaiter().GetResult();
                Program.Check(recovered == first.ManagedThreadId, "parent I/O action failure does not kill the dispatcher");
                Reset.Invoke(script, null);
                Program.Check(first.Join(2000), "parent I/O reset retires the idle dispatcher");
                Dispatch(script, () => { next = Thread.CurrentThread; return true; }).GetAwaiter().GetResult();
                Program.Check(next != first && next.IsAlive, "parent I/O reset creates an independent successor dispatcher");
                Program.Check(script.Pid == 0 && script.Tm == 2000 && script.Cp == 2500 && script.Mm == 128L * 1024 * 1024,
                    "controlled parent I/O does not launch a worker or change runtime limits");
            }
            finally { script.Dispose(); }
            Program.Check(next != null && next.Join(2000), "parent I/O disposal retires the successor dispatcher");
        }

        private static async Task Bounded(string fixture)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            using (var entered = new ManualResetEvent(false))
            using (var release = new ManualResetEvent(false))
            using (var script = new Jp(engine, page))
            {
                var order = new List<int>();
                Task<int> first = null, second = null;
                try
                {
                    first = Dispatch(script, () => { entered.Set(); release.WaitOne(); order.Add(1); return 1; }, true);
                    await Until(() => entered.WaitOne(0));
                    second = Dispatch(script, () => { order.Add(2); return 2; }, true);
                    Exception rejected = null;
                    try { await Dispatch(script, () => { order.Add(3); return 3; }, true); }
                    catch (Exception error) { rejected = error; }
                    Program.Check(rejected is InvalidOperationException && !second.IsCompleted,
                        "parent I/O permits only one queued action behind the active action");
                    release.Set();
                    Program.Check(await first == 1 && await second == 2 && order.SequenceEqual(new[] { 1, 2 }),
                        "parent I/O drains accepted actions in FIFO order without executing rejected work");
                }
                finally
                {
                    release.Set();
                    if (first != null) await Ignore(first);
                    if (second != null) await Ignore(second);
                }
            }
        }

        private static async Task Retire(string fixture)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            Thread old = null, next = null;
            using (var entered = new ManualResetEvent(false))
            using (var release = new ManualResetEvent(false))
            using (var script = new Jp(engine, page))
            {
                var pending = Dispatch(script, () =>
                { old = Thread.CurrentThread; entered.Set(); release.WaitOne(); return 17; }, true);
                try
                {
                    await Until(() => entered.WaitOne(0));
                    var clock = Stopwatch.StartNew();
                    Reset.Invoke(script, null);
                    Program.Check(clock.ElapsedMilliseconds < 1000 && old.IsAlive && !pending.IsCompleted,
                        "parent I/O retirement does not join a blocked producer on the owner thread");
                    var result = await Dispatch(script, () => { next = Thread.CurrentThread; return 29; }, true);
                    Program.Check(result == 29 && next != old, "parent I/O successor does not wait behind retired work");
                    release.Set();
                    Program.Check(await pending == 17 && old.Join(2000), "parent I/O late producer exits its own retired dispatcher");
                    Program.Check(await Dispatch(script, () => Thread.CurrentThread == next, true),
                        "parent I/O late completion cannot replace the successor dispatcher");
                }
                finally { release.Set(); await Ignore(pending); }
            }
            Program.Check(next != null && next.Join(2000), "parent I/O successor retires after overlapping old completion");
        }

        private static async Task Until(Func<bool> ready)
        {
            var clock = Stopwatch.StartNew();
            while (!ready() && clock.ElapsedMilliseconds < 2000) await Task.Delay(5);
            if (!ready()) throw new InvalidOperationException("Controlled I/O producer did not enter.");
        }

        private static async Task Ignore(Task task)
        { try { await task; } catch { } }

        internal static void Bench(string fixture)
        {
            var engine = new E();
            var page = engine.Go(fixture);
            using (var script = new Jp(engine, page))
                for (var sample = 0; sample < 7; sample++)
                {
                    var threads = new HashSet<int>();
                    var clock = Stopwatch.StartNew();
                    for (var index = 0; index < 128; index++)
                        threads.Add(Dispatch(script, () => Thread.CurrentThread.ManagedThreadId).GetAwaiter().GetResult());
                    Console.WriteLine("IO dispatch sample=" + sample + "; actions=128; threads=" + threads.Count +
                        "; wallTicks=" + clock.ElapsedTicks + "; frequency=" + Stopwatch.Frequency);
                }
            using (var script = new Jp(engine, page))
            {
                var warmup = script.Ex("1;");
                Console.WriteLine("IO native startup: " + warmup.S + "; " + warmup.M + "; " + script.Sd);
                if (!warmup.Ok) throw new InvalidOperationException("Native I/O benchmark startup failed.");
                for (var sample = 0; sample < 7; sample++)
                {
                    var clock = Stopwatch.StartNew();
                    var result = script.Ex("var length=0;for(var i=0;i<32;i++)length+=document.title.length;");
                    Console.WriteLine("IO native sample=" + sample + "; reads=32; wallTicks=" + clock.ElapsedTicks +
                        "; frequency=" + Stopwatch.Frequency + "; status=" + result.S);
                    if (!result.Ok || result.N != 0) throw new InvalidOperationException("Native I/O benchmark read failed: " + result.M);
                }
            }
        }
    }
}
