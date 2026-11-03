using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using Lea.Engine;
using Lea.Script;
using Microsoft.Win32.SafeHandles;
using Forms = System.Windows.Forms;

internal static class SdkPooledNativeMain
{
    private static int checks;
    private static readonly FieldInfo Io = typeof(Jp).GetField("io", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo Worker = typeof(Jp).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static void Until(Func<bool> done, int milliseconds)
    {
        var clock = Stopwatch.StartNew();
        while (!done() && clock.ElapsedMilliseconds < milliseconds) { Forms.Application.DoEvents(); Thread.Sleep(1); }
        if (!done()) throw new TimeoutException("Native pooled test handshake deadline");
    }
    private static bool QueueRetired(object queue)
    {
        var type = queue.GetType(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var gate = type.GetField("gate", flags).GetValue(queue);
        return SpinWait.SpinUntil(() => {
            lock (gate)
            {
                if ((bool)type.GetField("scheduled", flags).GetValue(queue)) return false;
                try { var collection = (System.Collections.Concurrent.BlockingCollection<Action>)type.GetField("actions", flags).GetValue(queue); return collection.Count < 0; }
                catch (ObjectDisposedException) { return true; }
            }
        }, 2000);
    }
    private static void Scenario(string fixture, bool revoke)
    {
        var files = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker"));
        var acl = files.ToDictionary(path => path, path => File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All));
        var engine = new E(); var page = engine.Go(fixture);
        using (var script = new Jp(engine, page))
        using (var entered = new CountdownEvent(2))
        using (var release = new ManualResetEvent(false))
        {
            var ready = script.Ex("var nativeReady=1;");
            Check(ready.Ok && Io.GetValue(script) == null, "native session ready without idle dispatcher: " + ready.S + "; " + ready.M);
            Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15 && script.Tm == 2000 && script.Cp == 2500 && script.Mm == 134217728,
                "native original isolation and execution budgets");
            var endpoint = Worker.GetValue(script);
            var process = (Process)endpoint.GetType().GetProperty("P").GetValue(endpoint, null);
            SafeProcessHandle handle = process.SafeHandle; bool retained = false;
            Task[] blockers = null; Thread stopper = null; Task<Jr> execution = null;
            handle.DangerousAddRef(ref retained);
            try
            {
                Check(retained && WaitForSingleObject(handle.DangerousGetHandle(), 0) == 258, "original native worker handle alive");
                // Bounded shared-pool pressure, never a global pool configuration change.
                blockers = Enumerable.Range(0, 2).Select(_ => Task.Run(() => {
                    entered.Signal(); if (!release.WaitOne(10000)) throw new TimeoutException("Pool pressure release deadline");
                })).ToArray();
                Check(entered.Wait(2000), "two unrelated pool callbacks held concurrently");
                var marker = revoke ? "Native revoke entered" : "Native stop entered";
                execution = script.ExA("document.title='" + marker + "';while(true){}");
                Until(() => page.T == marker || execution.IsCompleted, 2000);
                var queue = Io.GetValue(script);
                Check(page.T == marker && !execution.IsCompleted && queue != null, "native DOM handshake precedes cancellation with active I/O");
                var clock = Stopwatch.StartNew(); Exception stopFailure = null;
                if (revoke)
                {
                    engine.Je = false;
                    Check(!engine.Je && !execution.IsCompleted, "permission revoked during pending native execution");
                }
                else
                {
                    stopper = new Thread(() => { try { script.St(); } catch (Exception error) { stopFailure = error; } });
                    stopper.Start();
                    Check(stopper.Join(2000) && stopFailure == null, "foreign-thread stop returns while owner is not pumping");
                }
                Until(() => execution.IsCompleted, 2000);
                var result = execution.GetAwaiter().GetResult();
                Console.WriteLine("NATIVE_CANCEL " + (revoke ? "revoke" : "stop") + " " + clock.ElapsedMilliseconds + "ms " + result.S);
                Check(result.S == (revoke ? "off" : "cancel") && clock.ElapsedMilliseconds < 1000, "state change observed before original execution deadline under pool pressure");
                Check(WaitForSingleObject(handle.DangerousGetHandle(), 5000) == 0, "original native worker exits");
                Check(Io.GetValue(script) == null && script.Pid == 0 && !script.Pc, "parent clears dispatcher endpoint and pending cleanup");
                Check(QueueRetired(queue), "late native I/O drain completes and disposes collection");
                Check(page.T == marker, "cancelled request has no later DOM mutation");
                Check(files.All(path => File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All) == acl[path]), "native cancellation restores exact worker ACLs");
            }
            finally
            {
                release.Set(); script.St();
                try
                {
                    if (stopper != null && !stopper.Join(2000)) throw new Exception("Stop thread did not retire");
                    if (execution != null && !execution.IsCompleted) Until(() => execution.IsCompleted, 2000);
                    if (blockers != null && !Task.WaitAll(blockers, 2000)) throw new Exception("Pool pressure callbacks did not retire");
                }
                finally { script.Dispose(); if (retained) handle.DangerousRelease(); }
            }
        }
    }
    [STAThread]
    private static int Main(string[] args)
    {
        var previous = SynchronizationContext.Current;
        if (args.Length != 1) return 2;
        Forms.Application.EnableVisualStyles();
        int exit = 1;
        using (var context = new Forms.ApplicationContext())
        using (var timer = new Forms.Timer { Interval = 5 })
        {
            timer.Tick += (sender, eventArgs) => {
                timer.Stop();
                try
                {
                    int minWorker, minIo, maxWorker, maxIo;
                    ThreadPool.GetMinThreads(out minWorker, out minIo); ThreadPool.GetMaxThreads(out maxWorker, out maxIo);
                    Scenario(args[0], false); Scenario(args[0], true);
                    int afterMinWorker, afterMinIo, afterMaxWorker, afterMaxIo;
                    ThreadPool.GetMinThreads(out afterMinWorker, out afterMinIo); ThreadPool.GetMaxThreads(out afterMaxWorker, out afterMaxIo);
                    Check(minWorker == afterMinWorker && minIo == afterMinIo && maxWorker == afterMaxWorker && maxIo == afterMaxIo, "global pool limits unchanged");
                    Console.WriteLine("PASS SDK POOLED NATIVE " + checks); exit = 0;
                }
                catch (Exception error) { Console.Error.WriteLine(error); }
                finally { context.ExitThread(); }
            };
            try
            {
                SynchronizationContext.SetSynchronizationContext(new Forms.WindowsFormsSynchronizationContext());
                timer.Start(); Forms.Application.Run(context); return exit;
            }
            finally { SynchronizationContext.SetSynchronizationContext(previous); }
        }
    }
}
