using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

internal static class DecoderQueueTraceProof
{
    private static int checks;
    private static Type queueType;
    private static MethodInfo observed;
    private static void Check(bool value, string name)
    { if (!value) throw new InvalidOperationException(name); checks++; Console.WriteLine("PASS " + name); }
    private static void Done(Task task)
    { Check(((IAsyncResult)task).AsyncWaitHandle.WaitOne(5000), "controlled task completed"); }
    private static Task<int> Start(SemaphoreSlim gate, Func<int> work, CancellationToken token, Action<string> observer)
    {
        var queue = Activator.CreateInstance(queueType, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { gate }, null);
        return (Task<int>)observed.Invoke(queue, new object[] { work, token, observer });
    }
    private static int Main(string[] args)
    {
        try
        {
            var compatibility = typeof(QueueProof).GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
            Check((int)compatibility.Invoke(null, new object[] { args }) == 0, "original 21-check queue proof retained");
            var assembly = Assembly.LoadFrom(args[0]);
            queueType = assembly.GetType("Lea.Win.Di+DecodeQueue", true);
            observed = queueType.GetMethod("RunObserved", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(typeof(int));
            using (var gate = new SemaphoreSlim(1, 1))
            {
                var phases = new List<string>(); bool pool = true;
                var task = Start(gate, () => { pool = Thread.CurrentThread.IsThreadPoolThread; return 7; }, CancellationToken.None, phases.Add);
                Done(task);
                Check(task.GetAwaiter().GetResult() == 7 && !pool, "observed decode stays on dedicated thread");
                Check(phases.SequenceEqual(new[] { "wait", "admit", "dispatch", "thread" }), "successful phase order");
                Check(gate.CurrentCount == 1, "success releases admission");
            }
            foreach (string cancelAt in new[] { "pre", "held", "admit", "dispatch" })
                using (var gate = new SemaphoreSlim(1, 1))
                using (var cancel = new CancellationTokenSource())
                {
                    var phases = new List<string>(); int calls = 0;
                    if (cancelAt == "pre") cancel.Cancel();
                    if (cancelAt == "held") gate.Wait();
                    Task<int> task = null;
                    try
                    {
                        task = Start(gate, () => { calls++; return 8; }, cancel.Token, phase => {
                            phases.Add(phase); if (phase == cancelAt) cancel.Cancel();
                        });
                        if (cancelAt == "held") cancel.Cancel();
                        Done(task);
                        Check(task.IsCanceled && calls == 0, cancelAt + " cancellation never invokes decode");
                        var expected = cancelAt == "admit" ? new[] { "wait", "admit" } : cancelAt == "dispatch" ?
                            new[] { "wait", "admit", "dispatch", "thread" } : new[] { "wait" };
                        Check(phases.SequenceEqual(expected), cancelAt + " cancellation phase distinguished");
                        Check(gate.CurrentCount == (cancelAt == "held" ? 0 : 1), cancelAt + " admission ownership preserved");
                    }
                    finally { if (cancelAt == "held") gate.Release(); if (task != null) Done(task); }
                }
            using (var gate = new SemaphoreSlim(1, 1))
            {
                var task = Start(gate, () => 9, CancellationToken.None, phase => { throw new ApplicationException("observer control"); });
                Done(task);
                Check(task.GetAwaiter().GetResult() == 9 && gate.CurrentCount == 1, "observer failure cannot alter result or ownership");
                var failure = Start(gate, () => { throw new InvalidOperationException("decode control"); }, CancellationToken.None, phase => { });
                Done(failure);
                Check(failure.IsFaulted && failure.Exception.InnerException.Message == "decode control" && gate.CurrentCount == 1, "decode failure retains cleanup and propagation");
            }
            var decoder = assembly.GetType("Lea.Win.Di", true);
            var shared = (SemaphoreSlim)decoder.GetField("one", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            using (var cancel = new CancellationTokenSource())
            {
                string report = ""; shared.Wait(); Task task = null;
                try
                {
                    task = (Task)decoder.GetMethod("Rd", BindingFlags.Static | BindingFlags.Public).Invoke(null,
                        new object[] { new byte[0], null, null, cancel.Token, 0L, 0L, null, null, null, null, null, new Action<string>(text => report = text) });
                    cancel.Cancel(); Done(task);
                    Check(task.IsCanceled && report.Contains("queue=wait") && report.Contains("admitMs=-1") && report.Contains("threadMs=-1") && report.Length <= 512,
                        "queued Rd diagnostic identifies no admission or thread");
                }
                finally { shared.Release(); if (task != null) Done(task); }
            }
            Console.WriteLine("Decoder queue trace: checks=" + checks + "; failures=0; no native worker launched");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
