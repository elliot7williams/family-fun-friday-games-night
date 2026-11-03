using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

internal static class DecoderRetirementProof
{
    private static int checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new InvalidOperationException(name);
        checks++;
        Console.WriteLine("PASS " + name);
    }

    private static void Done(Task task)
    {
        if (!((IAsyncResult)task).AsyncWaitHandle.WaitOne(5000))
            throw new TimeoutException("Controlled retirement task did not complete.");
    }

    private static int Main(string[] args)
    {
        try
        {
            var prior = typeof(DecoderQueueTraceProof).GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
            Check((int)prior.Invoke(null, new object[] { args }) == 0, "all prior queue controls retained");
            var type = Assembly.LoadFrom(args[0]).GetType("Lea.Win.Di+DecodeQueue", true);
            var run = type.GetMethod("Run", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(typeof(int));
            using (var gate = new SemaphoreSlim(1, 1))
            using (var entered = new ManualResetEventSlim())
            using (var retiring = new ManualResetEventSlim())
            using (var release = new ManualResetEventSlim())
            using (var ownerCancel = new CancellationTokenSource())
            using (var waiterCancel = new CancellationTokenSource())
            {
                var queue = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new object[] { gate }, null);
                Task<int> owner = null, waiter = null, next = null;
                int waiterCalls = 0, nextCalls = 0;
                try
                {
                    owner = (Task<int>)run.Invoke(queue, new object[] { new Func<int>(() => {
                        entered.Set();
                        if (!ownerCancel.Token.WaitHandle.WaitOne(5000)) throw new TimeoutException("No owner cancellation.");
                        retiring.Set();
                        if (!release.Wait(5000)) throw new TimeoutException("No retirement release.");
                        ownerCancel.Token.ThrowIfCancellationRequested();
                        return 1;
                    }), ownerCancel.Token });
                    Check(entered.Wait(5000), "first decoder entered");
                    ownerCancel.Cancel();
                    Check(retiring.Wait(5000), "canceled owner reached controlled retirement");
                    Check(!owner.IsCompleted && gate.CurrentCount == 0, "cancellation does not release an unretired owner");
                    waiter = (Task<int>)run.Invoke(queue, new object[] { new Func<int>(() => {
                        Interlocked.Increment(ref waiterCalls); return 2;
                    }), waiterCancel.Token });
                    Check(!waiter.IsCompleted && waiterCalls == 0, "next request waits for actual retirement");
                    waiterCancel.Cancel();
                    Done(waiter);
                    Check(waiter.IsCanceled && waiterCalls == 0, "waiting request cancels without decoding");
                    Check(gate.CurrentCount == 0 && !owner.IsCompleted, "waiting cancellation cannot steal owner admission");
                    next = (Task<int>)run.Invoke(queue, new object[] { new Func<int>(() => {
                        Interlocked.Increment(ref nextCalls); return 3;
                    }), CancellationToken.None });
                    Check(!next.IsCompleted && nextCalls == 0, "third request still waits behind retiring owner");
                    release.Set();
                    Done(owner); Done(next);
                    Check(owner.IsCanceled, "owner cancellation propagates after retirement");
                    Check(next.GetAwaiter().GetResult() == 3 && nextCalls == 1, "next live request recovers exactly once");
                    Check(gate.CurrentCount == 1, "retirement and canceled waiter leave exactly one permit");
                }
                finally
                {
                    ownerCancel.Cancel(); waiterCancel.Cancel(); release.Set();
                    foreach (var task in new Task[] { owner, waiter, next }) if (task != null) Done(task);
                }
            }
            Console.WriteLine("Decoder retirement: checks=" + checks + "; failures=0; no native worker launched");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
