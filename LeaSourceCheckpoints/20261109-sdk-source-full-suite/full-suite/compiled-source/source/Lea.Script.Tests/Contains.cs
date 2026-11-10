using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class Contains
    {
        internal static void Run(string fixture, string mode)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            if (mode == "aliases") { Aliases(engine, page); return; }
            if (mode == "trusted")
                using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, mode);
            else if (mode == "isolated")
                using (var script = new Jp(engine, page) { Tm = 5000 }) Suite(engine, page, script.Ex, mode);
            else throw new ArgumentException("Unknown contains mode.");
        }

        private static void Aliases(E engine, P page)
        {
            var method = typeof(N).GetMethod("Co", new[] { typeof(N) });
            Program.Check(method != null && method.ReturnType == typeof(bool), "contains aliases: public two-letter Co is present");
            var contains = (Func<N, N, bool>)Delegate.CreateDelegate(typeof(Func<N, N, bool>), method);
            var group = engine.Id(page, "group");
            var child = engine.Id(page, "child");
            var field = engine.Id(page, "field");
            Program.Check(contains(group, group), "contains aliases: inclusive self");
            Program.Check(contains(group, child) && contains(page.D, child), "contains aliases: nested descendant and document root");
            Program.Check(!contains(child, group) && !contains(child, field), "contains aliases: ancestor and sibling excluded");
            Program.Check(!contains(group, null), "contains aliases: null excluded");
            var foreign = new E().Go("about:blank");
            Program.Check(!contains(group, foreign.D), "contains aliases: other document excluded");
            var changes = page.Jc;
            Program.Check(contains(group, field) && page.Jc == changes && page.Fs.Any(item => item.El == field), "contains aliases: no mutation or field replacement");
            engine.De(page, group);
            Program.Check(contains(group, child) && !contains(page.D, child), "contains aliases: detached internal ancestry without old document");
            var parent = typeof(N).GetProperty("Up");
            var saved = group.Up;
            parent.SetValue(group, child, null);
            try
            {
                var bounded = false;
                try { contains(foreign.D, child); } catch (FormatException) { bounded = true; }
                Program.Check(bounded, "contains aliases: malformed parent cycle is bounded");
            }
            finally { parent.SetValue(group, saved, null); }
            var constructor = typeof(N).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
            var root = (N)constructor.Invoke(new object[] { "section" });
            var leaf = root;
            for (var index = 0; index < 128; index++)
            {
                var next = (N)constructor.Invoke(new object[] { "span" });
                parent.SetValue(next, leaf, null);
                leaf = next;
            }
            Program.Check(contains(root, leaf), "contains aliases: exact 128-edge ancestry boundary");
            var extra = (N)constructor.Invoke(new object[] { "span" });
            parent.SetValue(extra, leaf, null);
            var rejected = false;
            try { contains(root, extra); } catch (FormatException) { rejected = true; }
            Program.Check(rejected, "contains aliases: 129 edges rejected");
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, string mode)
        {
            Jr Code(string code, string name)
            {
                var result = ex(code);
                Program.Check(result.Ok, mode + " contains: " + name + "; " + result.S + "; " + result.M);
                return result;
            }
            Code("var group=document.getElementById('group'),child=document.getElementById('child'),field=document.getElementById('field');", "wrappers acquired");
            Code("if(!group.contains(group)||!group.contains(child)||!document.contains(document)||!document.contains(child))throw Error('inclusive ancestry');", "self nested node and document");
            Code("if(child.contains(group)||child.contains(field)||group.contains(document)||group.contains(document.getElementById('outside')))throw Error('unrelated ancestry');", "ancestor sibling outside and document excluded");
            Code("if(group.contains(null)||group.contains(undefined)||document.contains(null))throw Error('nullable');", "nullable arguments return false");
            Code("try{group.contains();throw Error('missing accepted');}catch(e){if(!(e instanceof TypeError))throw e;}", "required argument");
            Code("var contains=group.contains;if(contains!==child.contains||contains!==document.contains||contains.name!=='contains'||contains.length!==1||Object.hasOwn(contains,'prototype'))throw Error('method shape');", "shared nonconstructible method");
            Code("if(!Reflect.apply(contains,group,[child])||Reflect.apply(contains,child,[group])||!Reflect.apply(contains,document,[group]))throw Error('borrowed');", "actual borrowed receiver");
            Code("for(var receiver of [null,undefined,window,3,{},Object.create(group)]){try{Reflect.apply(contains,receiver,[child]);throw Error('receiver accepted');}catch(e){if(!(e instanceof TypeError))throw e;}}", "invalid receivers rejected");
            Code("var conversions=0,bad={toString(){conversions++;throw Error('coerced');}};for(var other of [window,{},3,'child',Symbol('node'),bad,new Proxy(child,{get(){throw Error('proxy trap');}})]){try{group.contains(other);throw Error('argument accepted');}catch(e){if(!(e instanceof TypeError))throw e;}}if(conversions)throw Error('conversion');", "unbranded arguments without coercion or proxy traps");
            Code("for(var method of [contains]){try{new method(child);throw Error('constructor accepted');}catch(e){if(!(e instanceof TypeError))throw e;}}", "construction rejected");
            Code("var getter=WeakMap.prototype.get,apply=Reflect.apply;WeakMap.prototype.get=Reflect.apply=()=>{throw Error('public intrinsic');};try{if(!group.contains(child))throw Error('capture');}finally{WeakMap.prototype.get=getter;Reflect.apply=apply;}", "captured state intrinsics");
            var changes = page.Jc;
            var read = Code("if(!document.contains(field)||!group.contains(field))throw Error('read');", "live read");
            Program.Check(read.N == 0 && page.Jc == changes, mode + " contains: reads do not mutate");
            var nativeField = page.Fs.Single(item => item.El.Av("id") == "field");
            Code("field.value='retained';if(!group.contains(field))throw Error('field');", "edited field retained");
            Program.Check(page.Fs.Contains(nativeField) && nativeField.V == "retained", mode + " contains: field identity retained");
            var ancestor = engine.Id(page, "group");
            var parent = typeof(N).GetProperty("Up");
            var previous = ancestor.Up;
            parent.SetValue(ancestor, engine.Id(page, "child"), null);
            try
            {
                var bounded = ex("document.contains(child);");
                Program.Check(bounded.S == "limit" && bounded.N == 0, mode + " contains: cycle becomes typed limit; " + bounded.S);
            }
            finally { parent.SetValue(ancestor, previous, null); }
            Code("var group=document.getElementById('group'),child=document.getElementById('child');if(!group.contains(child))throw Error('recovery');", "limit resets and recovers");
            Code("group.remove();if(document.contains(group)||document.contains(child)||!group.contains(child)||!child.contains(child))throw Error('detached');", "retained detached relationships");
            Code("try{child.id='forbidden';throw Error('write accepted');}catch(e){if(!(e instanceof TypeError))throw e;}if(!group.contains(child))throw Error('detached read');", "containment grants no detached write capability");
            var exhausted = ex("for(var i=0;i<300;i++)group.contains(child);");
            Program.Check(exhausted.S == "limit" && exhausted.N == 0, mode + " contains: existing bridge-call bound; " + exhausted.S);
            Code("if(typeof group!=='undefined')throw Error('realm survived');document.title='Contains recovery';", "bridge exhaustion resets realm");
            engine.Je = false;
            Program.Check(ex("document.contains(document);").S == "off", mode + " contains: revoked permission");
            engine.Je = true;
        }

        internal static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 700), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); W.On(view, null, 5000); form.Show();
                var page = await view.GoA(fixture);
                Program.Check(view.Xo != null && view.Xo.Ok && view.T == "Contains ready", "native contains: inline lifecycle");
                var field = page.Fs.Single(item => item.El.Av("id") == "field");
                await View.Activate(view, "action");
                Program.Check(view.T == "Inside" && view.Id("result").C[0].X == "Inside" && view.Xo.Ok, "native contains: click target inside group");
                await View.Activate(view, "outside");
                Program.Check(view.T == "Outside" && view.Id("result").C[0].X == "Outside", "native contains: unrelated target excluded");
                var read = await view.JsA("if(!document.contains(document)||!document.contains(document.getElementById('field')))throw Error('document');");
                Program.Check(read.Ok && read.N == 0 && page.Fs.Contains(field), "native contains: read preserves field");
                await View.Activate(view, "field");
                var editor = view.Controls.OfType<TextBox>().Single(); editor.Text = "Elliot"; await view.Xt;
                Program.Check(editor.Visible && field.V == "Elliot", "native contains: retained edit");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 700); View.Paint(view, output == null ? null : output + "-narrow.png");
                Program.Check(editor.Visible && view.ClientRectangle.Contains(editor.Bounds), "native contains: narrow editor fits");
                var runtime = ((W)view.Xs).At; var pid = runtime.Pid;
                using (var owned = Process.GetProcessById(pid))
                {
                    var handle = owned.Handle;
                    var detached = await view.JsA("var retained=document.getElementById('group'),nested=document.getElementById('child');retained.remove();if(document.contains(nested)||!retained.contains(nested))throw Error('detached');");
                    Program.Check(detached.Ok && detached.N == 1 && runtime.Pid == pid, "native contains: detached read retains realm");
                    view.Dispose();
                    Program.Check(owned.WaitForExit(2000), "native contains: exact worker retired");
                }
            }
        }
    }
}
