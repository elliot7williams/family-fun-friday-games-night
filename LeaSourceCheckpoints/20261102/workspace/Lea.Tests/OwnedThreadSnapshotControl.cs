using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using Lea.Build;

internal static class OwnedThreadSnapshotControl
{
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    private static int checks;
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); checks++; }
    private static void Reject(Action action, string name)
    { bool rejected = false; try { action(); } catch (ArgumentException) { rejected = true; } catch (InvalidDataException) { rejected = true; } Check(rejected, name); }
    private static int Main(string[] args)
    {
        try
        {
            using (var process = Process.GetCurrentProcess())
            {
                uint pid = (uint)process.Id, main = GetCurrentThreadId();
                long birth = process.StartTime.ToUniversalTime().ToFileTimeUtc();
                var before = OwnedThreadSnapshot.Read(process.Handle, pid, birth);
                var first = before.Threads.Single(item => item.Id == main);
                Check(OwnedThreadSnapshot.LayoutSupported() && before.OriginalProcessHandleVerified && first.Created > 0, "layout/current native thread identity");
                Reject(() => OwnedThreadSnapshot.Read(process.Handle, pid + 1, birth), "wrong process identity rejected");
                Reject(() => OwnedThreadSnapshot.Read(process.Handle, pid, birth + 1), "wrong birth rejected");
                Reject(() => OwnedThreadSnapshot.Read(IntPtr.Zero, pid, birth), "invalid handle rejected");
                var clock = Stopwatch.StartNew();
                while (clock.ElapsedMilliseconds < 300) Thread.SpinWait(1000);
                var after = OwnedThreadSnapshot.Read(process.Handle, pid, birth).Threads.Single(item => item.Id == main);
                Check(after.Created == first.Created && after.CpuTicks - first.CpuTicks >= 10000, "owning thread CPU advances under spin");
                Check(after.ContextSwitches >= first.ContextSwitches, "context switch counter monotonic");
                uint sleeper = 0; bool waiting = false; uint reason = 0;
                using (var ready = new ManualResetEvent(false))
                {
                    var thread = new Thread(() => { sleeper = GetCurrentThreadId(); ready.Set(); Thread.Sleep(600); });
                    thread.Start();
                    bool started = ready.WaitOne(1000);
                    try
                    {
                        var wait = Stopwatch.StartNew();
                        while (started && !waiting && wait.ElapsedMilliseconds < 400)
                        {
                            var item = OwnedThreadSnapshot.Read(process.Handle, pid, birth).Threads.SingleOrDefault(value => value.Id == sleeper);
                            if (item != null && item.State == 5) { waiting = true; reason = item.WaitReason; }
                            else Thread.Sleep(5);
                        }
                    }
                    finally { if (!thread.Join(2000)) throw new Exception("Control thread retirement unverified"); }
                }
                Check(waiting, "controlled sleeping native thread observed waiting");
                Reject(() => OwnedThreadSnapshot.RejectTruncatedControl(), "truncated native buffer rejected");
                var result = new { passed = true, checks, nativeMainThread = main, sleeper, waitReasonRaw = reason, cpuTicks = after.CpuTicks - first.CpuTicks, interpreterExecuted = false, threadSuspended = false };
                File.WriteAllText(args[0], new JavaScriptSerializer().Serialize(result));
            }
            Console.WriteLine("THREAD_SNAPSHOT PASS " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
