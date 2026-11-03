using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class SdkScriptAdapterMain
{
    private static int checks;
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static void Until(Func<bool> done)
    {
        var clock = Stopwatch.StartNew();
        while (!done() && clock.ElapsedMilliseconds < 10000) { Forms.Application.DoEvents(); Thread.Sleep(5); }
        if (!done()) throw new TimeoutException("Adapter workflow deadline");
    }
    private static Xr Await(Task<Xr> task)
    { Until(() => task.IsCompleted); return task.GetAwaiter().GetResult(); }
    private static N Find(N root, string id)
    {
        var pending = new Stack<N>(); pending.Push(root);
        while (pending.Count > 0) { var node = pending.Pop(); if (node.Av("id") == id) return node; foreach (var child in node.C) pending.Push(child); }
        throw new Exception("Missing fixture node " + id);
    }
    [STAThread]
    private static int Main(string[] args)
    {
        Forms.Application.EnableVisualStyles();
        var previous = SynchronizationContext.Current;
        int result = 1;
        using (var context = new Forms.ApplicationContext())
        using (var timer = new Forms.Timer { Interval = 5 })
        {
            timer.Tick += (sender, eventArgs) =>
            {
                timer.Stop();
                try { result = Run(args); }
                finally { context.ExitThread(); }
            };
            try
            {
                SynchronizationContext.SetSynchronizationContext(new Forms.WindowsFormsSynchronizationContext());
                timer.Start(); Forms.Application.Run(context);
            }
            finally { SynchronizationContext.SetSynchronizationContext(previous); }
        }
        return result;
    }
    private static int Run(string[] args)
    {
        try
        {
            if (args.Length != 1) return 2;
            var runtime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var original = Directory.GetFiles(runtime).ToDictionary(path => path, path => File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All));
            using (var form = new Forms.Form { ClientSize = new Size(720, 420), AutoScaleMode = Forms.AutoScaleMode.None })
            using (var view = new V { Dock = Forms.DockStyle.Fill })
            {
                form.Controls.Add(view); form.Show();
                Check(view.Xf == null, "adapter remains opt-in");
                W.On(view); Check(view.Xf != null, "WinForms adapter attached");
                view.Go(args[0]);
                Until(() => !view.Busy && view.Xn == 0 && view.Xo != null);
                Check(view.Xo.Ok, "automatic inline activation: " + view.Xo.S + "; " + view.Xo.M);
                Check(view.T == "Ready", "inline then DOM-ready order");
                Check(Find(view.At.D, "result").C[0].X == "Loaded", "automatic window load handler");
                var adapter = (W)view.Xs;
                Check(adapter != null && adapter.At.Pid != 0, "view uses real isolated worker");
                Check(adapter.At.Tm == 2000 && adapter.At.Cp == 2500 && adapter.At.Mm == 128L * 1024 * 1024, "adapter original worker budgets");
                var click = Await(view.Xe(Find(view.At.D, "button"), "click"));
                Check(click.Ok && click.D, "inline click handler cancels default: " + click.S + "; " + click.M + "; prevented=" + click.D + "; text=" + Find(view.At.D, "result").C[0].X);
                Check(Find(view.At.D, "result").C[0].X == "Clicked 1", "inline handler mutates retained DOM");
                Check(Await(view.JsA("document.getElementById('result').textContent=JSON.stringify([1,2,3]);")).Ok, "view supplied code");
                Check(Find(view.At.D, "result").C[0].X == "[1,2,3]", "view supplied-code result");
                Check(Await(view.JsA("queueMicrotask(function(){document.getElementById('result').textContent='Microtask';});")).Ok, "view microtask execution");
                Check(Find(view.At.D, "result").C[0].X == "Microtask", "view microtask checkpoint");
                Check(Await(view.JsA("setTimeout(function(){document.title='Timer';},0);")).Ok, "view timer scheduled");
                Until(() => view.T == "Timer"); Check(view.T == "Timer", "view timer callback pumped");
                Check(Await(view.JsA("requestAnimationFrame(function(){document.getElementById('result').textContent='Frame';});")).Ok, "view frame scheduled");
                Until(() => Find(view.At.D, "result").C[0].X == "Frame"); Check(true, "view frame callback pumped");
                Check(Await(view.JsA(new string(' ', 16385))).S == "code", "view source bound");
                var endpoint = typeof(Jp).GetField("worker", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(adapter.At);
                var process = (Process)endpoint.GetType().GetProperty("P").GetValue(endpoint, null);
                var handle = process.SafeHandle; bool retained = false;
                handle.DangerousAddRef(ref retained);
                try
                {
                    var native = handle.DangerousGetHandle();
                    Check(retained && WaitForSingleObject(native, 0) == 258, "original adapter worker handle alive");
                    var retiredTail = view.Xt;
                    view.Go(Path.Combine(Path.GetDirectoryName(args[0]), "sdk-script-local.html"));
                    Await(retiredTail);
                    Until(() => !view.Busy && view.Xn == 0 && view.T == "SDK local script");
                    Check(WaitForSingleObject(native, 5000) == 0, "navigation retires original worker handle");
                }
                finally { if (retained) handle.DangerousRelease(); }
                Check(original.All(pair => pair.Value == File.GetAccessControl(pair.Key).GetSecurityDescriptorSddlForm(AccessControlSections.All)), "navigation restores worker ACLs");
                Check(view.Xs == null || ((W)view.Xs).At.Pid == 0, "script-free navigation stays dormant");
                view.Je = false;
                Check(Await(view.JsA("document.title='Denied';")).S == "off" && view.T == "SDK local script", "view permission revocation");
                view.Je = true;
                W.Off(view); Check(view.Xf == null && view.Xs == null, "adapter detachment retires binding");
                Check(Await(view.JsA("1;")).S == "none", "detached view cannot execute scripts");
            }
            Check(original.All(pair => pair.Value == File.GetAccessControl(pair.Key).GetSecurityDescriptorSddlForm(AccessControlSections.All)), "view disposal preserves original ACLs");
            Console.WriteLine("PASS SDK ADAPTER " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
