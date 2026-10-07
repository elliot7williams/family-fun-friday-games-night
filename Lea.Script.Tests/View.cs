using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class View
    {
        internal static void Run(string fixture, string output = null)
        {
            Pump(async () => { await Native(fixture, output); await Queue(fixture); });
        }

        internal static void Attributes(string fixture, string output = null)
        {
            Pump(() => Lea.Script.Tests.Attributes.Native(fixture, output));
        }

        internal static void ClassLists(string fixture, string output = null)
        {
            Pump(() => Lea.Script.Tests.ClassLists.Native(fixture, output));
        }

        private static void Pump(Func<Task> work)
        {
            var previous = SynchronizationContext.Current;
            Exception failure = null;
            using (var context = new ApplicationContext())
            using (var timer = new System.Windows.Forms.Timer { Interval = 5 })
            {
                var started = false;
                timer.Tick += async (sender, args) =>
                {
                    if (started) return;
                    started = true;
                    try { await work(); }
                    catch (Exception error) { failure = error; }
                    finally { timer.Stop(); context.ExitThread(); }
                };
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                timer.Start();
                Application.Run(context);
            }
            SynchronizationContext.SetSynchronizationContext(previous);
            if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
        }

        private static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 820), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                W.On(view, null, 5000);
                var done = 0;
                var changed = 0;
                var imageReadyAtLoad = false;
                view.Done += (sender, args) => done++;
                view.Changed += (sender, args) => changed++;
                view.Xd += (sender, args) => { if (view.T == "Lea ready") imageReadyAtLoad = view.Ic == 1 && view.Ib > 0 && view.Iac && view.Ind; };
                form.Show();
                var page = await view.GoA(fixture);
                Check(view.Xo != null && view.Xo.Ok && view.T == "Lea ready" && done == 1 && view.Xn == 0, "native load awaits inline scripts and lifecycle: " + (view.Xo == null ? "null" : view.Xo.S + "; " + view.Xo.M));
                Check(Text(view.Id("events")) == "Ready:interactive" && view.Id("alias").Av("class") == "literal", "native DOMContentLoaded and explicit Lea blocks");
                Check(imageReadyAtLoad, "native window load follows image completion: count=" + view.Ic + "; bytes=" + view.Ib + "; worker=" + view.Iw);
                await Code(view, "if(starts!==1||second!==2||document.readyState!=='complete')throw new Error('load order');");
                var runtime = (W)view.Xs;
                var pid = runtime.At.Pid;
                var before = changed;
                await Activate(view, "counter");
                Check(view.T == "Lea 1" && Text(view.Id("result")) == "1" && changed > before && runtime.At.Pid == pid, "native keyboard click executes persistent JS and notifies host");
                var rect = view.Ab(view.Ax(view.Id("counter")));
                Check(rect.Width > 0 && rect.Height > 0, "native counter accessibility bounds");
                typeof(V).GetMethod("OnMouseClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view,
                    new object[] { new MouseEventArgs(MouseButtons.Left, 1, rect.Left + rect.Width / 2, rect.Top + rect.Height / 2, 0) });
                await view.Xt;
                Check(Text(view.Id("result")) == "2", "native mouse click reaches optional event runtime");
                await Activate(view, "field");
                var editor = view.Controls.OfType<TextBox>().Single();
                Check(editor.Visible, "native editor active");
                editor.Text = "Elliot";
                await view.Xt;
                Check(Text(view.Id("echo")) == "Elliot", "native text input updates computed DOM");
                await Code(view, "if(inputs!==1)throw new Error('input count');document.getElementById('field').value='script value';");
                Check(editor.Text == "script value" && page.Fs.Single(item => item.El.Av("id") == "field").V == "script value", "script field mutation synchronizes native editor");
                await Code(view, "queueMicrotask(function(){document.getElementById('field').value='microtask value';});");
                Check(editor.Text == "microtask value" && page.Fs.Single(item => item.El.Av("id") == "field").V == "microtask value" && view.Xo.N == 1,
                    "microtask field writes synchronize retained native editor before completion");
                view.Kf();
                await view.Xt;
                await Code(view, "if(inputs!==1||changes!==1)throw new Error('input feedback or missing commit');");
                await Activate(view, "check");
                Check(Text(view.Id("events")) == "click:true|input:true|change:true", "checkbox preactivation and input/change order");
                await Code(view, "document.getElementById('check').addEventListener('click',function(e){e.preventDefault();},{once:true});");
                await Activate(view, "check");
                Check(page.Fs.Single(item => item.El.Av("id") == "check").C && Text(view.Id("events")) == "click:false", "canceled checkbox restores previous state and emits no change");
                await Code(view, "var rapid=[];document.getElementById('check').addEventListener('click',function(){rapid.push(this.checked);});");
                var firstClick = view.Ka();
                var secondClick = view.Ka();
                await Task.WhenAll(firstClick, secondClick);
                await Code(view, "if(rapid.join(',')!=='false,true')throw new Error('overlapping preactivation');");
                Check(page.Fs.Single(item => item.El.Av("id") == "check").C, "rapid native checkbox activations remain ordered");
                await Activate(view, "blocked");
                Check(view.At == page && Text(view.Id("events")) == "Navigation prevented", "preventDefault suppresses native navigation");
                await Activate(view, "send");
                Check(view.At == page && Text(view.Id("submitted")) == "Hello Ada", "native validated submit event can prevent form navigation");
                await Code(view, "document.getElementById('result').style.width='180px';document.getElementById('result').textContent='Painted';");
                Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 820);
                Paint(view, output == null ? null : output + "-narrow.png");
                var partial = await view.JsA("document.title='partial';throw new Error('expected');");
                Check(partial.S == "js" && partial.N == 1 && view.T == "partial" && runtime.At.Pid == 0, "failed native script repaints completed writes and drops worker");
                await Code(view, "if(count!==0||starts!==1)throw new Error('reinitialization');document.title='fresh';");
                Check(view.T == "fresh" && runtime.At.Pid != pid, "native runtime failure reinitializes fresh inline realm");
                pid = runtime.At.Pid;
                var running = view.JsA("/(a+)+$/.test('a'.repeat(120)+'!');document.title='late';");
                await Task.Delay(20);
                view.Je = false;
                var stopped = await running;
                Check(!stopped.Ok && Exited(pid) && view.T == "fresh" && view.Xs == null && view.Xn == 0, "native permission revocation kills worker without late DOM writes");
                view.Je = true;
                await Code(view, "document.title='again';");
                runtime = (W)view.Xs;
                pid = runtime.At.Pid;
                running = view.JsA("/(a+)+$/.test('a'.repeat(120)+'!');document.title='late';");
                await Task.Delay(20);
                view.St();
                stopped = await running;
                Check(!stopped.Ok && Exited(pid) && view.T == "again" && view.Xs == null, "native Stop retires isolated session");
                await Code(view, "document.title='before navigation';");
                runtime = (W)view.Xs;
                pid = runtime.At.Pid;
                running = view.JsA("/(a+)+$/.test('a'.repeat(120)+'!');document.title='late';");
                await Task.Delay(20);
                await view.GoA(Path.Combine(Path.GetDirectoryName(fixture), "javascript-runtime.html"));
                stopped = await running;
                Check(!stopped.Ok && Exited(pid) && view.At != page && view.T == "Lea JavaScript runtime experiment" && view.Xo.Ok, "native navigation cancels old-page worker without running nonclassic script types");
                runtime = (W)view.Xs;
                Check(runtime.At.Pid == 0, "script-free page initialization leaves interpreter dormant");
                await Code(view, "if(document.readyState!=='complete')throw new Error('dormant lifecycle');document.title='Demanded';");
                Check(view.T == "Demanded" && runtime.At.Pid > 0, "supplied code initializes dormant page and completed lifecycle on demand");
                pid = runtime.At.Pid;
                using (var remote = new Remote())
                {
                    await view.GoA(remote.U);
                    Check(Exited(pid) && view.T == "Remote Lea" && view.Xo.S == "origin" && ((W)view.Xs).At.Pid == 0, "remote page skips classic scripts without launching interpreter");
                    Check((await view.JsA("document.title='unsafe';")).S == "origin" && view.T == "Remote Lea", "adapter supplied-code remote-origin gate");
                    var address = view.At.U;
                    typeof(P).GetProperty("U").SetValue(view.At, new Uri("file://lea-test.invalid/share/page.html"), null);
                    try { Check((await view.JsA("document.title='unsafe';")).S == "origin" && ((W)view.Xs).At.Pid == 0, "UNC file document is not treated as local script content"); }
                    finally { typeof(P).GetProperty("U").SetValue(view.At, address, null); }
                    await Activate(view, "remote");
                    Check(view.T == "Remote Lea" && ((W)view.Xs).At.Pid == 0, "remote click cannot fall back to literal onclick execution");
                }
                await view.GoA("about:blank");
                runtime = (W)view.Xs;
                await view.Xe(null, "click");
                Check(runtime.At.Pid == 0 && view.Xo.Ok, "script-free native events do not start a worker");
                Check((await view.Xe(null, "unknown")).S == "event" && runtime.At.Pid == 0, "dormant runtime still rejects unsupported event types");
                var otherEngine = new E();
                var foreignPage = otherEngine.Go("about:blank");
                Check((await view.Xe(foreignPage.D, "click")).S == "node" && runtime.At.Pid == 0, "dormant runtime still rejects foreign event targets");
                await Code(view, "if(document.readyState!=='complete')throw new Error('blank lifecycle');");
                Check(runtime.At.Pid > 0, "dormant about document can opt into supplied JavaScript");
                var authored = Path.Combine(Path.GetTempPath(), "lea-view-authored-" + Guid.NewGuid().ToString("N") + ".html");
                try
                {
                    File.WriteAllText(authored, "<title>Handler page</title><button id=only onclick=\"document.title='Handler only';\">Run</button>");
                    await view.GoA(authored);
                    Check(((W)view.Xs).At.Pid > 0, "authored handler without a classic block initializes the optional realm");
                    await Activate(view, "only");
                    Check(view.T == "Handler only", "handler-only document executes native click");
                    File.WriteAllText(authored, "<title>Template page</title><template><script>document.title='Unexpected';</script><button onclick=\"document.title='Unexpected'\">Hidden</button></template><p>Plain page</p>");
                    await view.GoA(authored);
                    Check(view.T == "Template page" && ((W)view.Xs).At.Pid == 0, "inert template scripts and handlers do not start a worker");
                }
                finally { File.Delete(authored); }
                await view.GoA(fixture);
                runtime = (W)view.Xs;
                pid = runtime.At.Pid;
                W.Off(view);
                Check(view.Xs == null && Exited(pid) && (await view.JsA("1;")).S == "none", "optional runtime detach kills worker and restores small-engine path");
                await view.GoA(fixture);
                Check(view.T == "Lea event browser", "unattached native view does not auto-run general JavaScript");
            }
        }

        private static async Task Queue(string fixture)
        {
            using (var view = new V())
            {
                view.Go(fixture);
                var provider = new Pending();
                var used = false;
                view.Xf = (engine, page) => { if (used) return new Immediate(); used = true; return provider; };
                var tasks = new List<Task<Xr>>();
                for (var i = 0; i < 32; i++) tasks.Add(view.JsA(i.ToString()));
                Check(view.Xn == 32 && provider.Codes.Count == 1 && (await view.JsA("overflow")).S == "busy", "native bounded queue serializes provider execution");
                for (var i = 0; i < 32; i++) { provider.Complete(); await tasks[i]; }
                Check(view.Xn == 0 && provider.Codes.SequenceEqual(Enumerable.Range(0, 32).Select(i => i.ToString())), "native queue preserves submission order");
                var completions = 0;
                view.Xd += (sender, args) => completions++;
                var running = view.JsA("old");
                var queued = view.JsA("never");
                view.Go(fixture);
                Check((await running).S == "cancel" && (await queued).S == "stale" && provider.Stops == 1 && provider.Disposals == 1 && completions == 1 && view.Xs != provider, "navigation retires active and queued work without stale completion events");
                provider = new Pending();
                view.Xf = (engine, page) => provider;
                running = view.JsA("dispose");
                view.Dispose();
                Check((await running).S == "cancel" && provider.Stops == 1 && provider.Disposals == 1, "native view disposal cancels then disposes provider exactly once");
            }
            using (var view = new V())
            {
                view.Go(fixture);
                var provider = new Pending();
                Task<Xr> nested = null;
                view.Xf = (engine, page) => { nested = view.JsA("nested"); view.Je = false; return provider; };
                Check((await view.JsA("outer")).S == "stale" && (await nested).S == "busy" && view.Xs == null && provider.Disposals == 1, "factory reentry and permission changes cannot publish stale binding");
                view.Je = true;
                var immediate = new Immediate();
                view.Xf = (engine, page) => immediate;
                nested = null;
                view.Xd += (sender, args) => { if (nested == null) nested = view.JsA("nested"); };
                Check((await view.JsA("outer")).Ok && (await nested).Ok && immediate.Codes.SequenceEqual(new[] { "outer", "nested" }), "synchronous completion callback can queue without overlapping provider calls");
            }
            using (var view = new V())
            {
                view.Go(fixture);
                view.Xf = (engine, page) => new Immediate();
                view.Xd += (sender, args) => { throw new InvalidOperationException("host event failure"); };
                Check((await view.JsA("one")).S == "host" && (await view.JsA("two")).S == "host" && view.Xn == 0, "host completion exceptions do not poison queue or escape UI callbacks");
                Exception crossThread = null;
                var thread = new Thread(() => { try { view.JsA("foreign"); } catch (Exception error) { crossThread = error; } });
                thread.Start(); thread.Join();
                Check(crossThread is InvalidOperationException, "native script API owner thread enforced");
            }
        }

        private static async Task Code(V view, string code)
        { var result = await view.JsA(code); Check(result.Ok, "native supplied script: " + result.S + "; " + result.M); }
        internal static async Task Activate(V view, string id)
        {
            for (var i = 0; i <= view.Kn; i++)
            {
                if (view.Ke != null && view.Ke.Av("id") == id) { await view.Ka(); return; }
                view.Kf();
            }
            throw new Exception("Native focus target not found: " + id);
        }
        private static string Text(N node) { return node.K == "#text" ? node.X : String.Concat(node.C.Select(Text)); }
        private static bool Exited(int pid) { try { using (var process = Process.GetProcessById(pid)) return process.WaitForExit(2000); } catch (ArgumentException) { return true; } }
        internal static void Paint(V view, string output)
        {
            view.Sc(-view.Sy);
            using (var bitmap = new Bitmap(view.Width, view.Height))
            {
                view.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                var colored = 0;
                for (var y = 0; y < bitmap.Height; y += 4) for (var x = 0; x < bitmap.Width; x += 4)
                    if (bitmap.GetPixel(x, y).ToArgb() != Color.White.ToArgb()) colored++;
                Check(colored > 300, "native event page renders nonblank at " + view.Width + "px");
                if (output != null) bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        private static void Check(bool condition, string name) { Program.Check(condition, name); }

        private class Immediate : Jx
        {
            internal readonly List<string> Codes = new List<string>();
            public virtual Task<Xr> ExA(string code) { Codes.Add(code); return Task.FromResult(new Xr("ok", 0, "")); }
            public Task<Xr> EvA(N target, string type) { return ExA(type); }
            public Task<Xr> PgA() { return ExA("load"); }
            public virtual void St() { }
            public virtual void Dispose() { }
        }
        private sealed class Pending : Immediate
        {
            private TaskCompletionSource<Xr> current;
            internal int Stops;
            internal int Disposals;
            public override Task<Xr> ExA(string code)
            {
                if (current != null) throw new Exception("Overlapping provider calls");
                Codes.Add(code); current = new TaskCompletionSource<Xr>(); return current.Task;
            }
            internal void Complete(string status = "ok") { var run = current; current = null; run.SetResult(new Xr(status, 0, "")); }
            public override void St() { Stops++; if (current != null) Complete("cancel"); }
            public override void Dispose() { Disposals++; }
        }

        private sealed class Remote : IDisposable
        {
            private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            private readonly Task serving;
            internal Uri U { get; private set; }
            internal Remote()
            {
                listener.Start(1);
                U = new Uri("http://127.0.0.1:" + ((IPEndPoint)listener.LocalEndpoint).Port + "/");
                serving = Task.Run(() =>
                {
                    using (var client = listener.AcceptTcpClient())
                    using (var stream = client.GetStream())
                    {
                        stream.ReadTimeout = 5000;
                        var header = new byte[4096];
                        stream.Read(header, 0, header.Length);
                        var body = System.Text.Encoding.UTF8.GetBytes("<title>Remote Lea</title><button id='remote' onclick=\"t('unsafe');\">Remote</button><script>document.title='unsafe';</script>");
                        var response = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: " + body.Length + "\r\nConnection: close\r\n\r\n");
                        stream.Write(response, 0, response.Length);
                        stream.Write(body, 0, body.Length);
                    }
                });
            }
            public void Dispose() { listener.Stop(); serving.GetAwaiter().GetResult(); }
        }
    }
}
