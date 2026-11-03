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

internal static class SdkSessionIoMain
{
    private static int checks;
    private static readonly FieldInfo QueueField = typeof(Jp).GetField("io", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo WorkerField = typeof(Jp).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static object Queue(Jp script) { return QueueField.GetValue(script); }
    private static Thread QueueThread(object queue)
    {
        if (queue == null) throw new Exception("Missing session queue");
        var method = queue.GetType().GetMethod("Invoke", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(typeof(Thread));
        var task = (Task<Thread>)method.Invoke(queue, new object[] { new Func<Thread>(() => Thread.CurrentThread) });
        if (!task.Wait(2000)) throw new Exception("I/O thread observation deadline");
        return task.GetAwaiter().GetResult();
    }
    private static Task<bool> HoldQueue(object queue, ManualResetEvent entered, ManualResetEvent release)
    {
        var method = queue.GetType().GetMethod("Invoke", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(typeof(bool));
        return (Task<bool>)method.Invoke(queue, new object[] { new Func<bool>(() => { entered.Set(); return release.WaitOne(5000); }) });
    }
    private sealed class OriginalWorker : IDisposable
    {
        private readonly SafeProcessHandle handle;
        private bool retained;
        internal OriginalWorker(Jp script)
        {
            var endpoint = WorkerField.GetValue(script);
            var process = (Process)endpoint.GetType().GetProperty("P").GetValue(endpoint, null);
            handle = process.SafeHandle; handle.DangerousAddRef(ref retained);
            if (!retained || WaitForSingleObject(handle.DangerousGetHandle(), 0) != 258) throw new Exception("Original live worker handle required");
        }
        internal bool Exited() { return WaitForSingleObject(handle.DangerousGetHandle(), 5000) == 0; }
        public void Dispose() { if (retained) { retained = false; handle.DangerousRelease(); } }
    }
    private static void Ready(Jp script)
    {
        var result = script.Ex("var retained=1;");
        Check(result.Ok, "session starts: " + result.S + "; " + result.M);
        Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15 && script.Tm == 2000 && script.Cp == 2500 && script.Mm == 134217728,
            "original isolation and resource limits");
    }
    private static void IdleStop(E engine, P page)
    {
        using (var script = new Jp(engine, page))
        {
            Ready(script);
            var queue = Queue(script); var thread = QueueThread(queue);
            Check(thread.IsBackground && thread.Name == "Lea script I/O" && thread != Thread.CurrentThread, "dedicated background I/O thread observed");
            for (var i = 0; i < 8; i++)
                Check(script.Ex("retained++;").Ok && Object.ReferenceEquals(queue, Queue(script)), "successful request reuses queue " + i);
            Check(Object.ReferenceEquals(thread, QueueThread(Queue(script))), "successful requests reuse the same Thread object");
            using (var worker = new OriginalWorker(script))
            {
                Exception failure = null;
                var stopper = new Thread(() => { try { script.St(); } catch (Exception error) { failure = error; } });
                stopper.Start(); Check(stopper.Join(2000) && failure == null, "foreign-thread idle stop returns");
                Check(thread.Join(2000), "idle stop retires retained I/O thread");
                Check(worker.Exited(), "idle stop retires original worker handle");
                Check(script.Tk().S == "cancel" && Queue(script) == null && !script.Pc, "stopped timer clears queue and endpoint");
            }
        }
    }
    private static void ErrorAndRevocation(E engine, P page)
    {
        using (var script = new Jp(engine, page))
        {
            Ready(script); var queue = Queue(script); var thread = QueueThread(queue);
            Check(script.Ex(new string(' ', 16385)).S == "code" && Object.ReferenceEquals(queue, Queue(script)), "source rejection does not replace healthy queue");
            using (var worker = new OriginalWorker(script))
            {
                Check(script.Ex("throw new Error('retire');").S == "js" && Queue(script) == null, "JavaScript error clears queue");
                Check(thread.Join(2000) && worker.Exited() && !script.Pc, "JavaScript error retires I/O thread and original worker");
            }
            Check(script.Ex("if(typeof retained!=='undefined')throw Error('old realm');var fresh=1;").Ok, "fresh realm after JavaScript failure");
            var replacement = Queue(script); var replacementThread = QueueThread(replacement);
            Check(!Object.ReferenceEquals(queue, replacement) && !Object.ReferenceEquals(thread, replacementThread), "recovery creates a distinct queue and thread");
            using (var worker = new OriginalWorker(script))
            {
                engine.Je = false;
                try { Check(script.Ex("1;").S == "off" && Queue(script) == null, "revocation clears queue without execution"); }
                finally { engine.Je = true; }
                Check(replacementThread.Join(2000) && worker.Exited() && !script.Pc, "revocation retires replacement thread and original worker");
            }
        }
    }
    private static void ProtocolExit(E engine, P page)
    {
        using (var script = new Jp(engine, page))
        {
            Ready(script); var thread = QueueThread(Queue(script));
            using (var worker = new OriginalWorker(script))
            {
                var endpoint = WorkerField.GetValue(script);
                var input = (Stream)endpoint.GetType().GetProperty("I").GetValue(endpoint, null);
                input.Write(new byte[12], 0, 12); input.Flush();
                Check(worker.Exited(), "malformed header retires original worker");
                var result = script.Ex("1;");
                Check((result.S == "worker" || result.S == "protocol") && Queue(script) == null, "protocol/transport failure clears queue");
                Check(thread.Join(2000) && !script.Pc, "protocol failure retires I/O thread and cleanup");
            }
        }
    }
    private static void InFlightStop(E engine, P page)
    {
        using (var script = new Jp(engine, page))
        {
            Ready(script); var thread = QueueThread(Queue(script));
            using (var worker = new OriginalWorker(script))
            using (var entered = new ManualResetEvent(false))
            using (var release = new ManualResetEvent(false))
            {
                Exception failure = null;
                var held = HoldQueue(Queue(script), entered, release);
                if (!entered.WaitOne(2000)) throw new Exception("I/O handshake was not entered");
                var running = typeof(Jp).GetField("running", BindingFlags.Instance | BindingFlags.NonPublic);
                var stopper = new Thread(() => {
                    try
                    {
                        var clock = Stopwatch.StartNew();
                        while (!(bool)running.GetValue(script) && clock.ElapsedMilliseconds < 2000) Thread.Sleep(1);
                        if (!(bool)running.GetValue(script)) throw new Exception("No in-flight request observed");
                        script.St();
                    }
                    catch (Exception error) { failure = error; }
                    finally { release.Set(); }
                });
                Jr result;
                try { stopper.Start(); result = script.Ex("1;"); }
                finally { release.Set(); }
                Check(stopper.Join(2000) && failure == null && result.S == "cancel", "foreign-thread stop cancels in-flight I/O: " + result.S + "; " + result.M);
                Check(held.Wait(2000) && held.Result && thread.Join(2000) && worker.Exited() && Queue(script) == null && !script.Pc, "in-flight stop retires thread, endpoint and cleanup");
            }
        }
    }
    private static void IdleDispose(E engine, P page)
    {
        var script = new Jp(engine, page);
        try
        {
            Ready(script); var thread = QueueThread(Queue(script));
            using (var worker = new OriginalWorker(script))
            {
                script.Dispose();
                Check(thread.Join(2000) && worker.Exited() && Queue(script) == null && !script.Pc, "idle disposal retires thread and original endpoint");
            }
        }
        finally { script.Dispose(); }
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) return 2;
            var files = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker"));
            var acl = files.ToDictionary(path => path, path => File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All));
            var engine = new E(); var page = engine.Go(args[0]);
            IdleStop(engine, page); ErrorAndRevocation(engine, page); ProtocolExit(engine, page); InFlightStop(engine, page); IdleDispose(engine, page);
            Check(files.All(path => File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All) == acl[path]), "all worker ACLs exactly restored");
            Console.WriteLine("PASS SDK SESSION IO " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
