using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class LoadFailures
    {
        internal static void Controlled(string fixture)
        {
            View.Pump(async () =>
            {
                foreach (var status in new[] { "setup", "timeout", "limit", "js", "protocol", "worker", "cancel", "off", "origin", "ok" })
                    foreach (var navigation in new[] { false, true })
                        foreach (var pending in new[] { false, true })
                            await Case(fixture, status, navigation, pending);
            });
        }

        private static async Task Case(string fixture, string status, bool navigation, bool pending)
        {
            var probe = new Probe(status, pending);
            var done = 0;
            using (var form = Window())
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                view.Xf = (engine, page) => probe;
                view.Done += (sender, args) => done++;
                form.Show();
                var label = status + "; GoA=" + navigation + "; pending=" + pending;
                if (navigation) await view.GoA(fixture);
                else
                {
                    view.Go(fixture);
                    var clock = Stopwatch.StartNew();
                    while (done == 0 && clock.ElapsedMilliseconds < 10000) await Task.Delay(5);
                    if (done == 0) throw new InvalidOperationException("Synchronous shown-page completion did not finish: " + label);
                }
                Program.Check(probe.Pages == 1 && probe.Loads == (status == "ok" ? 1 : 0),
                    "load completion does not dispatch after failed initial scripts: " + label + "; pages=" + probe.Pages + "; load=" + probe.Loads);
                Program.Check(Object.ReferenceEquals(view.Xo, status == "ok" ? probe.LoadResult : probe.PageResult),
                    "load completion preserves the initial failure or successful load result: " + label);
                Program.Check(done == 1 && !view.Busy && view.Xn == 0 && view.Ic == 1 && view.Ib > 0 && view.Iac && view.Ind,
                    "script result does not prevent image isolation, document completion or idle view state: " + label);
                var recovery = await view.JsA("explicit recovery");
                Program.Check(recovery.Ok && probe.Codes == 1 && probe.Pages == 1 && view.Xn == 0,
                    "later explicit commands are still admitted after completed navigation: " + label);
            }
            Program.Check(probe.Stops == 1 && probe.Disposals == 1,
                "load completion provider is stopped and disposed exactly once: " + status + "; GoA=" + navigation + "; pending=" + pending);
        }

        internal static async Task Native(string fixture, string output)
        {
            var done = 0;
            var pid = 0;
            using (var form = Window())
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                W.On(view);
                view.Done += (sender, args) => done++;
                form.Show();
                await view.GoA(fixture);
                var runtime = (W)view.Xs;
                var failure = view.Xo;
                Program.Check(failure != null && failure.S == "js" && failure.N == 2 && view.T == "Lea load 1" && Text(view.Id("attempts")) == "1" && runtime.At.Pid == 0,
                    "native failed initial script is not replayed by the automatic load event: " + (failure == null ? "none" : failure.S + "; " + failure.M));
                Program.Check(done == 1 && !view.Busy && view.Xn == 0 && view.Ic == 1 && view.Ib > 0 && view.Iac && view.Ind,
                    "native script failure preserves rendered content, isolated images and completed navigation");
                await Task.Delay(100);
                Program.Check(Object.ReferenceEquals(view.Xo, failure) && Text(view.Id("attempts")) == "1" && runtime.At.Pid == 0,
                    "native idle failed page retains its result without an implicit replacement realm");
                Program.Check(runtime.At.Tm == 2000 && runtime.At.Cp == 2500 && runtime.At.Mm == 128L * 1024 * 1024 &&
                    runtime.At.Ac && runtime.At.Ne && runtime.At.Nd && runtime.At.Fd && runtime.At.Mg == 15,
                    "native failed initial script keeps the existing deadlines and verified isolation");
                View.Paint(view, output);
                var retry = await view.Xl();
                pid = runtime.At.Pid;
                Program.Check(retry.Ok && pid > 0 && Text(view.Id("attempts")) == "2" && runtime.At.Ac && runtime.At.Ne && runtime.At.Nd && runtime.At.Fd && runtime.At.Mg == 15,
                    "explicit initial-script retry creates a fresh isolated realm and runs the block once: " + retry.S + "; " + retry.M);
                var load = await view.Xe(null, "load");
                var recovered = await view.JsA("if(document.readyState!=='complete')throw Error('load state');document.title='Recovered';");
                Program.Check(load.Ok && recovered.Ok && view.T == "Recovered" && runtime.At.Pid == pid && Text(view.Id("attempts")) == "2" && done == 1 && !view.Busy,
                    "explicit load completion and later code reuse the recovered realm without another block replay");
            }
            Program.Check(Exited(pid), "native recovered load realm retires on view disposal");
        }

        private static Form Window()
        {
            return new Form { ClientSize = new Size(940, 700), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) };
        }

        private static string Text(N node)
        {
            if (node.K == "#text") return node.X;
            var text = "";
            foreach (var child in node.C) text += Text(child);
            return text;
        }

        private static bool Exited(int pid)
        {
            try { using (var process = Process.GetProcessById(pid)) return process.WaitForExit(2000); }
            catch (ArgumentException) { return true; }
        }

        private sealed class Probe : Jx
        {
            private readonly bool pending;
            internal readonly Xr PageResult;
            internal readonly Xr LoadResult = new Xr("ok", 0, "automatic load");
            internal int Pages, Loads, Codes, Stops, Disposals;

            internal Probe(string status, bool pending)
            { PageResult = new Xr(status, 0, "initial " + status); this.pending = pending; }

            public async Task<Xr> PgA()
            { Pages++; if (pending) await Task.Delay(10); return PageResult; }
            public Task<Xr> EvA(N target, string type)
            { if (target != null || type != "load") throw new InvalidOperationException("Unexpected automatic event"); Loads++; return Task.FromResult(LoadResult); }
            public Task<Xr> ExA(string code)
            { Codes++; return Task.FromResult(new Xr("ok", 0, code)); }
            public void St() { Stops++; }
            public void Dispose() { Disposals++; }
        }
    }
}
