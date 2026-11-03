using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lea.Script;

internal static class SdkPooledIoMain
{
    private static readonly Type QueueType = typeof(Jp).GetNestedType("IoQueue", BindingFlags.NonPublic);
    private static int checks;
    private static object NewQueue() { return Activator.CreateInstance(QueueType, true); }
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static object Field(object queue, string name)
    { return QueueType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(queue); }
    private static void Stop(object queue)
    { QueueType.GetMethod("Stop", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(queue, null); }
    private static Task<T> Invoke<T>(object queue, Func<T> action)
    {
        try { return (Task<T>)QueueType.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.NonPublic)
            .MakeGenericMethod(typeof(T)).Invoke(queue, new object[] { action }); }
        catch (TargetInvocationException error) { throw error.InnerException; }
    }
    private static T Result<T>(Task<T> task)
    {
        if (!SpinWait.SpinUntil(() => task.IsCompleted, 2000)) throw new Exception("Queue completion deadline");
        return task.GetAwaiter().GetResult();
    }
    private static bool Drained(object queue)
    {
        return SpinWait.SpinUntil(() => { lock (Field(queue, "gate")) return !(bool)Field(queue, "scheduled"); }, 2000);
    }
    private static bool Disposed(object queue)
    {
        try { var ignored = ((BlockingCollection<Action>)Field(queue, "actions")).Count; return false; }
        catch (ObjectDisposedException) { return true; }
    }
    private static bool Rejected<T>(object queue, Func<T> action, Type expected)
    {
        try { Invoke(queue, action); return false; }
        catch (Exception error) { return error.GetType() == expected; }
    }
    private static void StopBeforeWork()
    {
        var queue = NewQueue(); Stop(queue); Stop(queue);
        Check(Rejected(queue, () => 1, typeof(ObjectDisposedException)), "stopped empty queue rejects admission");
        Check(Drained(queue), "empty stop schedules no drain");
        Check(Disposed(queue), "empty stop disposes collection");
    }
    private static void Errors()
    {
        var queue = NewQueue();
        try
        {
            var sentinel = new InvalidOperationException("action sentinel"); Exception observed = null;
            try { Result(Invoke<int>(queue, () => { throw sentinel; })); } catch (Exception error) { observed = error; }
            Check(Object.ReferenceEquals(sentinel, observed), "action error reaches caller unchanged");
            Check(Result(Invoke(queue, () => 7)) == 7, "queue remains usable after action error");
            Check(Result(Invoke(queue, () => Thread.CurrentThread.IsThreadPoolThread)), "action executes on pool thread");
            Check(Drained(queue), "idle drain returns without retained consumer");
        }
        finally { Stop(queue); }
        Check(Drained(queue) && Disposed(queue), "stop after idle drain disposes collection");
    }
    private static void BoundedStop()
    {
        var queue = NewQueue(); var order = new ConcurrentQueue<int>();
        using (var entered = new ManualResetEvent(false))
        using (var release = new ManualResetEvent(false))
        {
            try
            {
                var first = Invoke(queue, () => { entered.Set(); if (!release.WaitOne(5000)) throw new Exception("Release deadline"); order.Enqueue(1); return 1; });
                Check(entered.WaitOne(2000), "active action handshake entered");
                var second = Invoke(queue, () => { order.Enqueue(2); return 2; });
                Check(Rejected(queue, () => 3, typeof(InvalidOperationException)), "capacity one rejects third action");
                Stop(queue); Stop(queue);
                Check(Rejected(queue, () => 4, typeof(ObjectDisposedException)), "stop rejects new work while active");
                Check(!first.IsCompleted && !second.IsCompleted, "pending action cannot overtake blocked action");
                release.Set();
                Check(Result(first) == 1, "active action completes after stop");
                Check(Result(second) == 2, "previously admitted action drains after stop");
                Check(order.ToArray().SequenceEqual(new[] { 1, 2 }), "admitted actions execute in order");
                Check(Drained(queue), "stopped drain terminates");
                Check(Disposed(queue), "last completion disposes stopped collection");
            }
            finally { release.Set(); Stop(queue); if (!Drained(queue)) throw new Exception("Bounded queue did not retire"); }
        }
    }
    private static void IndependentQueues()
    {
        var a = NewQueue(); var b = NewQueue();
        using (var entered = new CountdownEvent(2))
        using (var release = new ManualResetEvent(false))
        {
            try
            {
                Func<int> action = () => { entered.Signal(); if (!release.WaitOne(5000)) throw new Exception("Parallel release deadline"); return 1; };
                var first = Invoke(a, action); var second = Invoke(b, action);
                Check(entered.Wait(2000), "independent queues enter concurrently without global serialization");
                release.Set(); Check(Result(first) + Result(second) == 2, "independent completions reach both callers");
            }
            finally { release.Set(); Stop(a); Stop(b); if (!Drained(a) || !Drained(b)) throw new Exception("Parallel queues did not retire"); }
            Check(Disposed(a) && Disposed(b), "both independent queues dispose after stop");
        }
    }
    private static int Main(string[] args)
    {
        try
        {
            StopBeforeWork(); Errors(); BoundedStop(); IndependentQueues();
            Console.WriteLine("PASS SDK POOLED IO " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
