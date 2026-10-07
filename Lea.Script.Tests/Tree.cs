using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class Tree
    {
        internal static void Run(string fixture, bool trustedOnly = false)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, "in-process");
            if (trustedOnly) return;
            page = engine.Go(fixture);
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(engine, page, script.Ex, "isolated", () => script.Sd);
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, string mode, Func<string> diagnostics = null)
        {
            Jr Code(string code, string name)
            {
                var result = ex(code);
                Program.Check(result.Ok, mode + " tree: " + name + "; " + result.S + "; " + result.M +
                    (result.Ok || diagnostics == null ? "" : "; " + diagnostics()));
                return result;
            }
            Code("var child=document.getElementById('child');if(!('parentElement' in child)||!('firstElementChild' in child)||!('lastElementChild' in child)||!('previousElementSibling' in child)||!('nextElementSibling' in child)||!('childElementCount' in child))throw Error('missing tree navigation');", "six element navigation properties; first wrapper uses handle zero");
            Code("var panel=document.getElementById('panel'),tree=document.getElementById('tree'),action=document.getElementById('action'),field=document.getElementById('field'),echo=document.getElementById('echo');if(child.parentElement!==action||action.parentElement!==panel||panel.parentElement!==tree||tree.parentElement!==document.body||document.body.parentElement!==document.documentElement||document.documentElement.parentElement!==null)throw Error('parents');", "live parents and document root boundary");
            Code("if(panel.firstElementChild!==action||panel.lastElementChild!==echo||panel.childElementCount!==3||action.firstElementChild!==child||action.lastElementChild!==child||action.childElementCount!==1)throw Error('children');", "child endpoints and counts skip whitespace text and comments");
            Code("if(action.previousElementSibling!==null||action.nextElementSibling!==field||field.previousElementSibling!==action||field.nextElementSibling!==echo||echo.previousElementSibling!==field||echo.nextElementSibling!==null||child.previousElementSibling!==null||child.nextElementSibling!==null)throw Error('siblings');", "adjacent element siblings skip interleaved text");
            Code("if(child.firstElementChild!==null||child.lastElementChild!==null||child.childElementCount!==0||field.firstElementChild!==null||field.childElementCount!==0)throw Error('leaf');", "text-only and void elements have no element children");
            Code("if(document.firstElementChild!==document.documentElement||document.lastElementChild!==document.documentElement||document.childElementCount!==1)throw Error('document children');", "document child endpoints and count retain element identity");
            Code("var head=document.querySelector('head');if(head.firstElementChild.tagName!=='TITLE'||head.lastElementChild.tagName!=='STYLE'||document.body.lastElementChild.tagName!=='SCRIPT')throw Error('non-rendered elements');", "navigation includes non-rendered title style and script elements");
            Code("var names=['parentElement','firstElementChild','lastElementChild','previousElementSibling','nextElementSibling','childElementCount'];for(var name of names){var d=Object.getOwnPropertyDescriptor(child,name);if(typeof d.get!=='function'||d.set!==undefined)throw Error('descriptor');try{(function(){'use strict';child[name]=37;})();throw Error('write accepted');}catch(e){if(!(e instanceof TypeError))throw e;}}", "navigation properties are read only");
            Code("for(var name of names)if(Object.getOwnPropertyDescriptor(child,name).get!==Object.getOwnPropertyDescriptor(action,name).get)throw Error('per-node getter allocation');for(var name of ['firstElementChild','lastElementChild','childElementCount'])if(Object.getOwnPropertyDescriptor(child,name).get!==Object.getOwnPropertyDescriptor(document,name).get)throw Error('per-document getter allocation');", "elements and document share one private getter set");
            Code("var first=Object.getOwnPropertyDescriptor(child,'firstElementChild').get,parent=Object.getOwnPropertyDescriptor(child,'parentElement').get,next=Object.getOwnPropertyDescriptor(child,'nextElementSibling').get,count=Object.getOwnPropertyDescriptor(child,'childElementCount').get;if(first.call(panel)!==action||parent.call(field)!==panel||next.call(field)!==echo||count.call(panel)!==3||first.call(document)!==document.documentElement||parent.call(document)!==null)throw Error('borrowed getter');", "borrowed getters use the actual supported receiver");
            Code("for(var name of names){var get=Object.getOwnPropertyDescriptor(child,name).get;for(var receiver of [null,undefined,3,{},window,Object.create(child)]){try{get.call(receiver);throw Error('receiver accepted');}catch(e){if(!(e instanceof TypeError))throw e;}}}try{next.call(document);throw Error('document sibling accepted');}catch(e){if(!(e instanceof TypeError))throw e;}", "unbranded receivers and document sibling getters are rejected");
            Code("var oldGet=WeakMap.prototype.get,oldApply=Reflect.apply;WeakMap.prototype.get=Reflect.apply=()=>{throw Error('poisoned intrinsic');};try{if(child.parentElement!==action||first.call(panel)!==action||count.call(document)!==1)throw Error('private state');}finally{WeakMap.prototype.get=oldGet;Reflect.apply=oldApply;}", "private receiver lookup survives intrinsic replacement");
            var changes = page.Jc;
            var read = Code("if(panel.firstElementChild.parentElement!==panel||field.nextElementSibling!==panel.lastElementChild)throw Error('live read');", "navigation is read only");
            Program.Check(read.N == 0 && page.Jc == changes, mode + " tree: reads do not mutate or rebuild the page");
            var field = page.Fs.Single(item => item.El.Av("id") == "field");
            Code("field.value='retained tree value';if(field.parentElement!==panel||field.previousElementSibling!==action||field.value!=='retained tree value')throw Error('field');", "navigation preserves edited form state");
            Program.Check(page.Fs.Contains(field) && field.V == "retained tree value", mode + " tree: native field model identity is retained");
            var parent = engine.Id(page, "panel");
            var children = parent.C.ToArray();
            try
            {
                while (parent.C.Count < 8192) parent.C.Add(children[0]);
                Code("if(panel.childElementCount!==3)throw Error('visit boundary');", "exact 8192-visit boundary accepts a host-supplied child list");
                parent.C.Add(children[0]);
                var bounded = ex("panel.childElementCount;");
                Program.Check(bounded.S == "limit" && bounded.N == 0, mode + " tree: oversized host child list fails at the visit ceiling; " + bounded.S);
            }
            finally
            {
                parent.C.Clear();
                foreach (var node in children) parent.C.Add(node);
            }
            Code("var panel=document.getElementById('panel'),tree=document.getElementById('tree'),action=document.getElementById('action'),child=document.getElementById('child');if(panel.firstElementChild!==action||child.parentElement!==action)throw Error('recovery');", "visit exhaustion resets and recovers the realm");
            Code("tree.textContent='replacement';if(tree.firstElementChild!==null||tree.lastElementChild!==null||tree.childElementCount!==0||panel.parentElement!==null||panel.previousElementSibling!==null||panel.nextElementSibling!==null||child.parentElement!==action||action.parentElement!==panel||panel.firstElementChild!==action)throw Error('detached');", "removed subtrees retain internal links without old ancestors");
            Code("try{child.id='detached write';throw Error('write accepted');}catch(e){if(!(e instanceof TypeError))throw e;}if(child.parentElement!==action)throw Error('detached read');", "detached navigation does not grant mutation capability");
            var denied = ex("for(var i=0;i<300;i++)child.childElementCount;");
            Program.Check(denied.S == "limit" && denied.N == 0, mode + " tree: existing bridge-call ceiling still applies; " + denied.S);
            Code("if(typeof child!=='undefined')throw Error('realm survived limit');document.title='tree recovery';", "bridge exhaustion resets the realm and permits recovery");
            engine.Je = false;
            var off = ex("document.childElementCount;");
            Program.Check(off.S == "off" && off.N == 0, mode + " tree: permission revocation gates navigation");
            engine.Je = true;
        }

        internal static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 820), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                W.On(view, null, 5000);
                form.Show();
                var page = await view.GoA(fixture);
                Program.Check(view.Xo != null && view.Xo.Ok, "native tree: inline navigation initializes; " + (view.Xo == null ? "null" : view.Xo.S + "; " + view.Xo.M));
                var field = page.Fs.Single(item => item.El.Av("id") == "field");
                await View.Activate(view, "action");
                Program.Check(field.V == "Lea tree" && view.Id("echo").C[0].X == "Selected", "native tree: click uses sibling and parent navigation for typed writes");
                await View.Activate(view, "field");
                var editor = view.Controls.OfType<TextBox>().Single();
                editor.Text = "Elliot";
                await view.Xt;
                Program.Check(view.T == "panel|3|Elliot" && editor.Visible, "native tree: input reads live parent and element count");
                var read = await view.JsA("if(document.getElementById('field').previousElementSibling!==document.getElementById('panel').firstElementChild)throw Error('native identity');");
                Program.Check(read.Ok && read.N == 0 && page.Fs.Contains(field) && field.V == "Elliot" && view.Controls.OfType<TextBox>().Single() == editor,
                    "native tree: read retains model editor and value");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 820);
                View.Paint(view, output == null ? null : output + "-narrow.png");
                Program.Check(editor.Visible && view.ClientRectangle.Contains(editor.Bounds), "native tree: narrow retained editor fits viewport");
            }
        }
    }
}
