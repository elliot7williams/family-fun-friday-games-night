using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class SubtreeQueries
    {
        internal static void Run(string fixture, string mode)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            if (mode == "aliases") { Aliases(engine, page); return; }
            if (mode == "trusted")
                using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, mode);
            else
                using (var script = new Jp(engine, page) { Tm = 5000 }) Suite(engine, page, script.Ex, mode);
        }

        private static void Aliases(E engine, P page)
        {
            var first = typeof(N).GetMethod("Q", new[] { typeof(string) });
            var all = typeof(N).GetMethod("Qa", new[] { typeof(string) });
            Program.Check(first != null && all != null && first.ReturnType == typeof(N) && all.ReturnType == typeof(IList<N>), "subtree aliases: Q/Qa methods are present");
            var q = (Func<N, string, N>)Delegate.CreateDelegate(typeof(Func<N, string, N>), first);
            var qa = (Func<N, string, IList<N>>)Delegate.CreateDelegate(typeof(Func<N, string, IList<N>>), all);
            var parent = engine.Id(page, "parent");
            var action = engine.Id(page, "action");
            var child = engine.Id(page, "child");
            var field = engine.Id(page, "field");
            Program.Check(q(parent, "#parent,#target") == null && qa(parent, "#parent,#target").Count == 0, "subtree aliases: exclude self and nodes outside the subtree");
            Program.Check(q(parent, "input,span,button") == action, "subtree aliases: first descendant follows tree order, not selector-list order");
            var ordered = qa(parent, "input,#child,#action,input");
            Program.Check(ordered.Count == 4 && ordered[0] == action && ordered[1] == child && ordered[2] == field && ordered[3] == engine.Id(page, "flag"), "subtree aliases: ordered unique matches from selector lists");
            Program.Check(q(parent, "body div > section button > span") == child, "subtree aliases: ancestor matching can extend outside the search subtree");
            Program.Check(q(child, "*") == null && qa(child, "*").Count == 0, "subtree aliases: text children are not element matches");
            var snapshot = qa(parent, "[data-query]");
            var readonlyList = false;
            try { snapshot.Add(field); } catch (NotSupportedException) { readonlyList = true; }
            Program.Check(readonlyList && snapshot.IsReadOnly, "subtree aliases: returned snapshot cannot mutate the document");
            var nativeField = page.Fs.Single(item => item.El == field);
            engine.Sa(page, field, "data-query", "yes");
            Program.Check(snapshot.Count == 0 && q(parent, "[data-query=yes]") == field && qa(parent, "[data-query]").Count == 1, "subtree aliases: later queries see live attributes while earlier snapshots stay static");
            Program.Check(page.Fs.Contains(nativeField), "subtree aliases: read queries retain field identity");
            foreach (var selector in new[] { null, "", "[", "span >", "#child,", ":scope", new string(' ', 1025), String.Join(",", Enumerable.Repeat("span", 17)) })
                foreach (var many in new[] { false, true })
                {
                    var rejected = false;
                    try { if (many) qa(parent, selector); else q(parent, selector); } catch (FormatException) { rejected = true; }
                    Program.Check(rejected, "subtree aliases: reject invalid or unsupported selector; many=" + many + "; length=" + (selector == null ? -1 : selector.Length));
                }
            engine.Jv(page, engine.Id(page, "tree"), "tx", "replacement");
            Program.Check(q(parent, "button > span") == child && q(parent, "body span") == null && qa(parent, "input").Count == 2, "subtree aliases: detached reads retain internal children without document ancestors");
            child.C.Add(parent);
            try
            {
                foreach (var many in new[] { false, true })
                {
                    var rejected = false;
                    try { if (many) qa(parent, ".missing"); else q(parent, ".missing"); } catch (FormatException) { rejected = true; }
                    Program.Check(rejected, "subtree aliases: native malformed cycles are bounded by traversal depth; many=" + many);
                }
            }
            finally { child.C.Remove(parent); }
            var original = child.C.Count;
            try
            {
                for (var index = 0; index < 8192; index++) child.C.Add(field);
                foreach (var many in new[] { false, true })
                {
                    var rejected = false;
                    try { if (many) qa(parent, ".missing"); else q(parent, ".missing"); } catch (FormatException) { rejected = true; }
                    Program.Check(rejected, "subtree aliases: native oversized child lists have an 8192-visit bound; many=" + many);
                }
            }
            finally { while (child.C.Count > original) child.C.RemoveAt(child.C.Count - 1); }
            var path = Path.Combine(Path.GetTempPath(), "Lea-subtree-" + Guid.NewGuid().ToString("N") + ".html");
            try
            {
                var html = new StringBuilder("<!doctype html><html><body><section id='dense'>");
                for (var index = 0; index < 300; index++) html.Append("<span id='n").Append(index).Append("'>x</span>");
                html.Append("</section></body></html>");
                File.WriteAllText(path, html.ToString());
                var dense = engine.Id(engine.Go(path), "dense");
                var matches = qa(dense, "span");
                Program.Check(matches.Count == 256 && matches[0].Av("id") == "n0" && matches[255].Av("id") == "n255", "subtree aliases: at most 256 matches in document order");
                Program.Check(q(dense, "span").Av("id") == "n0", "subtree aliases: single-result traversal stops at the first match");
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, string mode)
        {
            Jr Code(string code, string name)
            {
                var result = ex(code);
                Program.Check(result.Ok, mode + " subtree queries: " + name + "; " + result.S + "; " + result.M);
                return result;
            }
            Code("var parent=document.getElementById('parent'),child=document.getElementById('child'),field=document.getElementById('field'),tree=document.getElementById('tree');if(typeof parent.querySelector!=='function'||typeof parent.querySelectorAll!=='function'||parent.querySelector.length!==1||parent.querySelectorAll.length!==1)throw Error('missing element query API');", "element query methods and arity");
            Code("if(parent.querySelector('#parent,#target')!==null||parent.querySelectorAll('#parent,#target').length!==0||child.querySelector('*')!==null)throw Error('subtree boundary');", "exclude self outside nodes and text children");
            Code("var action=document.getElementById('action'),flag=document.getElementById('flag'),list=parent.querySelectorAll('input,#child,#action,input');if(list.length!==4||list[0]!==action||list[1]!==child||list[2]!==field||list[3]!==flag||parent.querySelector('input,span,button')!==action)throw Error('tree order');", "ordered unique descendants and existing wrapper identity");
            Code("if(parent.querySelector('body div > section button > span')!==child||parent.querySelector(':is(button,span):not(.missing)')!==action||parent.querySelectorAll('input + input:disabled')[0]!==flag)throw Error('compiled selectors');", "existing relationships functions and live form state");
            Code("var one=child.querySelector,many=child.querySelectorAll;if(one!==parent.querySelector||many!==parent.querySelectorAll||one.call(parent,'span')!==child||many.call(parent,'input').length!==2)throw Error('borrowed');for(var method of [one,many]){try{new method('span');throw Error('constructor');}catch(e){if(!(e instanceof TypeError))throw e;}}", "shared non-constructible methods use the actual receiver");
            Code("var converted=0;for(var method of [one,many])for(var receiver of [null,undefined,3,{},document,window,Object.create(child)]){try{method.call(receiver,{toString(){converted++;return 'span';}});throw Error('receiver');}catch(e){if(!(e instanceof TypeError))throw e;}}if(converted!==0)throw Error('early conversion');", "brand checks precede selector conversion");
            Code("for(var method of [one,many]){try{method.call(parent);throw Error('missing');}catch(e){if(!(e instanceof TypeError))throw e;}try{method.call(parent,Symbol());throw Error('symbol');}catch(e){if(!(e instanceof TypeError))throw e;}}if(parent.querySelector(undefined)!==null||parent.querySelectorAll(null).length!==0)throw Error('DOM strings');", "required arguments and explicit DOM string conversion");
            Code("var conversions=0;if(parent.querySelector({toString(){conversions++;return 'span';}})!==child||conversions!==1)throw Error('conversion');var oldString=String,oldMap=Array.prototype.map;String=undefined;Array.prototype.map=()=>{throw Error('public map');};try{if(parent.querySelectorAll('input')[0]!==field)throw Error('captures');}finally{String=oldString;Array.prototype.map=oldMap;}", "single conversion and captured conversion and result mapping");
            Code("for(var selector of ['', '[', 'span >', '#child,', 'span,,div', ':unknown', ':scope'])for(var method of [one,many]){try{method.call(parent,selector);throw Error('invalid accepted');}catch(e){if(!(e instanceof DOMException)||e.name!=='SyntaxError'||e.code!==12)throw e;}}", "invalid and unsupported selectors throw branded SyntaxError");
            Code("for(var selector of [Array(18).join('span,')+'span',Array(18).join('div ')+'span','span'+' '.repeat(1021)])for(var method of [one,many]){try{method.call(parent,selector);throw Error('limit accepted');}catch(e){if(!(e instanceof DOMException)||e.name!=='SyntaxError')throw e;}}if(parent.querySelector('span'+' '.repeat(1020))!==child)throw Error('exact bound');", "group part and raw 1024-unit selector limits");
            Code("var snapshot=parent.querySelectorAll('input');snapshot.pop();if(parent.querySelectorAll('input').length!==2||snapshot.length!==1||snapshot[0]!==field)throw Error('snapshot');", "JavaScript result arrays are independent static snapshots");
            var changes = page.Jc;
            var read = Code("if(parent.querySelector('span')!==child||parent.querySelectorAll('input').length!==2)throw Error('read');", "subtree queries are read only");
            Program.Check(read.N == 0 && page.Jc == changes, mode + " subtree queries: no mutation or rebuild for reads");
            var nativeField = page.Fs.Single(item => item.El.Av("id") == "field");
            Code("var before=parent.querySelectorAll('[data-query]');field.setAttribute('data-query','yes');field.value='edited';if(before.length!==0||parent.querySelector('[data-query=yes]')!==field||parent.querySelectorAll('[data-query]').length!==1||field.value!=='edited')throw Error('live attributes');", "new queries see live attributes without changing old snapshots");
            Program.Check(page.Fs.Contains(nativeField) && nativeField.V == "edited", mode + " subtree queries: native field identity and value stay retained");
            Code("tree.textContent='replacement';if(parent.querySelector('button > span')!==child||parent.querySelector('body span')!==null||parent.querySelectorAll('input').length!==2||parent.querySelector('#parent')!==null)throw Error('detached');", "detached subtree reads exclude old document ancestors");
            Code("try{parent.querySelector('span').id='denied';throw Error('write accepted');}catch(e){if(!(e instanceof TypeError))throw e;}if(parent.querySelector('#child')!==child)throw Error('identity');", "detached query results do not gain write permissions");
            Code("try{parent.querySelector('[');}catch(e){if(e.name!=='SyntaxError')throw e;}if(parent.querySelector('span')!==child)throw Error('lost realm');if(typeof query!=='undefined'||typeof __lea!=='undefined')throw Error('leak');", "caught syntax failures retain realm and private helper scope");
            var denied = ex("for(var i=0;i<300;i++)parent.querySelector('span');");
            Program.Check(denied.S == "limit", mode + " subtree queries: existing bridge-call ceiling retires the realm; " + denied.S);
            Code("if(typeof parent!=='undefined')throw Error('old realm');document.title='subtree recovery';if(document.body.querySelector('#target')===null)throw Error('recovery');", "fresh realm recovers after bridge exhaustion");
            engine.Je = false;
            var off = ex("document.body.querySelector('*');");
            Program.Check(off.S == "off" && off.N == 0, mode + " subtree queries: permission revocation gates reads");
            engine.Je = true;
        }

        internal static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 700), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                W.On(view, null, 5000);
                form.Show();
                var page = await view.GoA(fixture);
                Program.Check(view.Xo != null && view.Xo.Ok, "native subtree: scoped inline queries initialize; " + (view.Xo == null ? "null" : view.Xo.S + "; " + view.Xo.M));
                await View.Activate(view, "action");
                Program.Check(view.Id("echo").C[0].X == "Chosen", "native subtree: click updates a descendant found through the panel");
                await View.Activate(view, "field");
                var editor = view.Controls.OfType<TextBox>().Single();
                editor.Text = "Elliot";
                await view.Xt;
                Program.Check(view.T == "Elliot" && editor.Visible, "native subtree: input queries the retained field");
                var runtime = (W)view.Xs;
                Program.Check(runtime.At.Ac && runtime.At.Ne && runtime.At.Nd && runtime.At.Fd && runtime.At.Mg == 15 && runtime.At.Mp <= 128L * 1024 * 1024, "native subtree: existing isolated worker protections apply");
                if (output != null)
                    using (var bitmap = new Bitmap(view.Width, view.Height))
                    { view.DrawToBitmap(bitmap, view.ClientRectangle); bitmap.Save(output); }
                form.Close();
            }
        }
    }
}
