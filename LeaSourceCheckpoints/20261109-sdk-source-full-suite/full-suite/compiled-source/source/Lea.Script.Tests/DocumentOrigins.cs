using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;
using OriginTests = Lea.Tests.Origins;

namespace Lea.Script.Tests
{
    internal static class DocumentOrigins
    {
        internal static void Probe()
        {
            var engine = new E { Jb = false };
            var page = engine.Go("about:blank");
            using (var script = new Js(engine, page))
            {
                var result = script.Ex("if(typeof origin!=='string'||origin!=='null'||window.origin!==origin)throw Error('missing origin accessor');");
                Program.Check(result.Ok, "origin accessor probe; " + result.S + "; " + result.M);
            }
        }

        internal static void Run(bool isolated)
        {
            var engine = new E { Jb = false };
            var page = engine.Go("about:blank");
            Bound(engine, page, isolated, "blank");
            using (var target = new OriginTests.Server())
            using (var entry = new OriginTests.Server(target.U.AbsoluteUri))
            {
                page = engine.Go(new Uri(entry.U, "/start#script"));
                Program.Check(page.Og.Eq(Og.Of(target.U)) && !page.Og.Eq(Og.Of(entry.U)) && page.Bu.Host == "other.invalid", "script origin binds final response authority independently of base");
                // This is explicitly host-supplied code; the website adapter remains closed.
                Bound(engine, page, isolated, "host-supplied remote");
            }
        }

        private static void Bound(E engine, P page, bool isolated, string label)
        {
            if (isolated)
                using (var script = new Jp(engine, page) { Tm = 5000 })
                {
                    Suite(engine, page, script.Ex, "isolated " + label, () => script.Sd);
                    Program.Check(script.Pid > 0 && script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15 && script.Cp == 2500 && script.Mm == 134217728, "origin reads retain native isolation and resource limits: " + label);
                }
            else
                using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, "trusted " + label);
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, string label, Func<string> diagnostics = null)
        {
            var authority = page.Og;
            var expected = "'" + authority.Tx + "'";
            Jr Code(string code, string name)
            {
                var result = ex(code);
                Program.Check(result.Ok, label + " origin: " + name + "; " + result.S + "; " + result.M + (result.Ok || diagnostics == null ? "" : "; " + diagnostics()));
                return result;
            }
            var changes = page.Jc;
            var read = Code("if(origin!==" + expected + "||window.origin!==origin)throw Error('document origin');", "global origin serializes the bound document");
            Program.Check(read.N == 0 && page.Jc == changes, label + " origin reads have zero mutations");
            Code("var originDescriptor=Object.getOwnPropertyDescriptor(window,'origin');if(typeof originDescriptor.get!=='function'||typeof originDescriptor.set!=='function'||!originDescriptor.enumerable||!originDescriptor.configurable||'value' in originDescriptor)throw Error('descriptor');", "configurable enumerable replaceable accessor");
            Code("if(originDescriptor.get.call(window)!==" + expected + "||originDescriptor.get.call(null)!==" + expected + ")throw Error('getter');for(var value of [{},Object.create(window),3])for(var method of [originDescriptor.get,originDescriptor.set]){try{method.call(value,'forged');throw Error('receiver accepted');}catch(e){if(!(e instanceof TypeError))throw e;}}", "captured getter validates its window receiver");
            Code("var originalDefine=Object.defineProperty,originValue={toString(){throw Error('unexpected conversion');}};Object.defineProperty=()=>{throw Error('public define');};try{window.origin=originValue;}finally{Object.defineProperty=originalDefine;}var originShadow=Object.getOwnPropertyDescriptor(window,'origin');if(origin!==originValue||originShadow.value!==originValue||!originShadow.writable||!originShadow.enumerable||!originShadow.configurable)throw Error('replacement');", "replacement preserves raw values and captured property construction");
            Code("window.origin='https://forged.invalid';if(origin!=='https://forged.invalid'||originDescriptor.get.call(window)!==" + expected + ")throw Error('private authority');delete window.origin;if('origin' in window)throw Error('deletion');Object.defineProperty(window,'origin',originDescriptor);if(origin!==" + expected + ")throw Error('restore');", "page shadowing and deletion cannot alter captured authority");
            Code("globalThis.__lea=()=>{throw Error('fake bridge');};if(origin!==" + expected + "||typeof call!=='undefined'||typeof realm!=='undefined')throw Error('capture leak');", "origin accessor retains private bridge state");
            Program.Check(ReferenceEquals(page.Og, authority) && page.Og.Tx == authority.Tx && page.Jc == changes, label + " engine origin remains immutable after page replacements");
            engine.Je = false;
            var denied = ex("origin;");
            Program.Check(denied.S == "off" && denied.N == 0, label + " origin reads obey host scripting revocation");
            engine.Je = true;
            Code("if(origin!==" + expected + "||typeof originDescriptor!=='undefined')throw Error('fresh realm');", "fresh realm restores origin accessor after permission revocation");
        }

        internal static async Task Native(string output)
        {
            var path = Path.Combine(Path.GetTempPath(), "Lea-origin-script-" + Guid.NewGuid().ToString("N") + ".html");
            var pid = 0;
            try
            {
                using (var server = new OriginTests.Server())
                using (var form = new Form { ClientSize = new Size(800, 600), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
                using (var view = new V { Dock = DockStyle.Fill })
                {
                    server.Markup = "<base href='file:///C:/'>" +
                        "<title>remote origin</title><script>document.title='remote executed';</script>";
                    File.WriteAllText(path, "<base href='" + server.U + "'><title>local origin</title><h1>Lea Origin Test</h1><input id='field' value='retained'><script src='/script.js'></script><script>if(origin!=='null')throw Error('local origin');window.origin='https://forged.invalid';document.title='local ready';</script>");
                    form.Controls.Add(view); W.On(view); form.Show();
                    var page = await view.GoA(path);
                    var runtime = (W)view.Xs; pid = runtime.At.Pid;
                    Program.Check(view.Xo.Ok && view.T == "local ready" && pid > 0 && page.Og.Op && page.Og.Lc && page.Bu.Port == server.U.Port && server.ScriptHits == 0,
                        "local base cannot change origin; external script remains unfetched; " + view.Xo.S + "; " + view.Xo.M + "; " + runtime.At.Sd);
                    var result = await view.JsA("if(origin!=='https://forged.invalid')throw Error('shadow');document.title='local retained';");
                    Program.Check(result.Ok && page.Og.Op && page.Og.Lc && runtime.At.Pid == pid && page.Fs[0].V == "retained", "page-visible origin replacement does not change local policy or fields");
                    Program.Check(runtime.At.Tm == 2000 && runtime.At.Cp == 2500 && runtime.At.Mm == 134217728 && runtime.At.Ac && runtime.At.Ne && runtime.At.Nd && runtime.At.Fd && runtime.At.Mg == 15,
                        "automatic origin access keeps original execution budgets and native protections");
                    View.Paint(view, output);
                    var remote = await view.GoA(server.U);
                    Program.Check(Exited(pid) && !remote.Og.Lc && !remote.Og.Op && remote.Bu.IsFile && view.Xo.S == "origin" && ((W)view.Xs).At.Pid == 0 && view.T == "remote origin",
                        "remote document with a local file base cannot start automatic scripting");
                    var supplied = await view.JsA("window.origin='null';document.title='unsafe';");
                    var timer = await ((W)view.Xs).TkA();
                    var load = await view.Xe(null, "load");
                    Program.Check(supplied.S == "origin" && timer.S == "origin" && load.S == "origin" && ((W)view.Xs).At.Pid == 0 && view.T == "remote origin" && server.ScriptHits == 0,
                        "adapter supplied code timers and events remain origin-gated without a worker");
                }
                Program.Check(Exited(pid), "origin test worker retires on navigation and disposal");
            }
            finally { File.Delete(path); }
        }

        private static bool Exited(int pid)
        {
            if (pid <= 0) return false;
            try { using (var process = Process.GetProcessById(pid)) return process.HasExited || process.WaitForExit(3000); }
            catch (ArgumentException) { return true; }
        }
    }
}
