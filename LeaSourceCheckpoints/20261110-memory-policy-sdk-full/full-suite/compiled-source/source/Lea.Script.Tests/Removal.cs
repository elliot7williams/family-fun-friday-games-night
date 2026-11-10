using System;
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
    internal static class Removal
    {
        internal static void Run(string fixture, string mode)
        {
            if (mode == "core") { Core(fixture); return; }
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            if (mode == "trusted")
                using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, mode);
            else
                using (var script = new Jp(engine, page) { Tm = 5000 }) Suite(engine, page, script.Ex, mode, () => script.Sd);
        }

        private static void Core(string fixture)
        {
            var remove = typeof(E).GetMethod("De", new[] { typeof(P), typeof(N) });
            Program.Check(remove != null && typeof(V).GetMethod("De", new[] { typeof(N) }) != null, "core removal: two-letter engine/view aliases");
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            int Delete(N node) { return (int)remove.Invoke(engine, new object[] { page, node }); }
            var drop = engine.Id(page, "drop");
            var child = engine.Id(page, "child");
            var field = page.Fs.Single(item => item.El.Av("id") == "field");
            var removed = page.Fs.Single(item => item.El.Av("id") == "removed-field");
            engine.Jv(page, field.El, "vl", "retained");
            var before = page.Jc;
            Program.Check(Delete(null) == 0 && Delete(page.D) == 0 && Delete(child.C[0]) == 0 && page.Jc == before,
                "core removal: null/document/text targets do not mutate");
            var other = new E { Jb = false };
            var foreign = other.Go(fixture);
            Program.Check(Delete(other.Id(foreign, "drop")) == 0 && page.Jc == before, "core removal: foreign node denied");
            try { remove.Invoke(engine, new object[] { foreign, drop }); throw new Exception("Foreign page accepted"); }
            catch (TargetInvocationException error) { Program.Check(error.InnerException is ArgumentException, "core removal: foreign page rejected"); }
            engine.Je = false;
            Program.Check(Delete(drop) == 0 && page.Jc == before && drop.Up != null, "core removal: disabled mutation is atomic");
            engine.Je = true;
            engine.Dy(page, child, removed.El);
            Program.Check(child.Mt(":hover") && removed.El.Mt(":focus") && engine.Q(page, "body").Mt(":hover"),
                "core removal: hover/focus state is active before deletion");
            var count = page.N;
            var subtree = Count(drop);
            Program.Check(Delete(drop) == 1 && page.Jc == before + 1 && page.N == count - subtree, "core removal: one mutation updates exact node count");
            Program.Check(drop.Up == null && child.Up == drop && drop.C.Contains(child), "core removal: detached subtree links retained");
            Program.Check(!drop.Mt(":hover") && !child.Mt(":hover") && !removed.El.Mt(":focus") && !engine.Q(page, "body").Mt(":hover"),
                "core removal: detached focus/hover and obsolete ancestor hover are cleared");
            Program.Check(engine.Id(page, "drop") == null && engine.Id(page, "child") == null && drop.Q("#child") == child,
                "core removal: connected lookup excludes removed nodes, retained subtree can be read");
            Program.Check(page.Fs.Contains(field) && field.V == "retained" && !page.Fs.Contains(removed), "core removal: remaining form identity/value retained, obsolete field removed");
            Program.Check(page.Bs.All(item => item.El != drop && item.El != child && item.El != removed.El) &&
                page.Axs.All(item => item.El != drop && item.El != child && item.El != removed.El), "core removal: paint/accessibility records no longer include removed subtree");
            Program.Check(engine.Q(page, "#keep:only-child") != null, "core removal: structural selectors see remaining sibling");
            before = page.Jc;
            Program.Check(Delete(drop) == 0 && Delete(child) == 0 && page.Jc == before && child.Up == drop, "core removal: detached subtree does not gain write capability");
            Program.Check(engine.Jv(page, child, "id", "denied") == 0 && engine.Sa(page, child, "x", "denied") == 0,
                "core removal: existing detached-write guards remain");
            Program.Check(Delete(field.El) == 1 && !page.Fs.Contains(field), "core removal: deleting one retained input clears its model");
            var html = engine.Q(page, "html");
            Program.Check(Delete(html) == 1 && html.Up == null && page.D.C.All(node => node.K.StartsWith("#", StringComparison.Ordinal)) && page.N == 0 && page.Fs.Count == 0,
                "core removal: document element detaches without deleting the document record");
            page = engine.Go(fixture);
            drop = engine.Id(page, "drop");
            field = page.Fs.Single(item => item.El.Av("id") == "field");
            engine.Dy(page, drop, field.El);
            Program.Check(Delete(drop) == 1 && field.El.Mt(":focus") && !engine.Q(page, "body").Mt(":hover") && page.Fs.Contains(field),
                "core removal: clearing deleted hover preserves unrelated live focus/model");
        }

        private static int Count(N node) { return (node.K.StartsWith("#", StringComparison.Ordinal) ? 0 : 1) + node.C.Sum(Count); }

        private static void Suite(E engine, P page, Func<string, Jr> ex, string mode, Func<string> diagnostics = null)
        {
            Jr Code(string code, string name)
            {
                var result = ex(code);
                Program.Check(result.Ok, mode + " removal: " + name + "; " + result.S + "; " + result.M +
                    (result.Ok || diagnostics == null ? "" : "; " + diagnostics()));
                return result;
            }
            Code("var drop=document.getElementById('drop'),child=document.getElementById('child'),parent=document.getElementById('parent'),keep=document.getElementById('keep'),field=document.getElementById('field');if(typeof drop.remove!=='function')throw Error('missing element remove');", "remove available with first handle zero");
            Code("var remove=drop.remove,descriptor=Object.getOwnPropertyDescriptor(drop,'remove');if(remove!==keep.remove||remove.length!==0||remove.name!=='remove'||descriptor.enumerable||descriptor.writable||descriptor.configurable)throw Error('descriptor');try{new remove();throw Error('constructible');}catch(e){if(!(e instanceof TypeError))throw e;}", "shared zero-argument nonconstructible method preserves own-property conventions");
            Code("for(var receiver of [null,undefined,3,{},window,document,Object.create(drop)]){try{remove.call(receiver);throw Error('receiver accepted');}catch(e){if(!(e instanceof TypeError))throw e;}}", "unbranded and inherited receivers rejected");
            Code("field.value='retained';var snapshot=parent.querySelectorAll('section'),tokens=drop.classList;", "retain native field and static query snapshot");
            var nativeField = page.Fs.Single(item => item.El.Av("id") == "field");
            var before = page.Jc;
            var removed = Code("var oldGet=WeakMap.prototype.get,oldApply=Reflect.apply;WeakMap.prototype.get=Reflect.apply=()=>{throw Error('poison');};try{if(remove.call(drop,{toString(){throw Error('converted');}})!==undefined)throw Error('return');}finally{WeakMap.prototype.get=oldGet;Reflect.apply=oldApply;}", "borrowed method uses captured receiver lookup and ignores extra arguments");
            Program.Check(removed.N == 1 && page.Jc == before + 1, mode + " removal: exactly one reported mutation");
            Code("if(document.getElementById('drop')!==null||document.getElementById('child')!==null||parent.firstElementChild!==keep||parent.lastElementChild!==keep||parent.childElementCount!==1||keep.previousElementSibling!==null||keep.nextElementSibling!==null||!keep.matches(':only-child'))throw Error('connected tree');", "live selectors and sibling endpoints update");
            Code("if(drop.parentElement!==null||child.parentElement!==drop||drop.firstElementChild!==child||drop.querySelector('#child')!==child||snapshot.length!==2||snapshot[0]!==drop||drop.classList!==tokens)throw Error('retained identity');", "detached wrapper and query/token identities remain readable");
            Program.Check(page.Fs.Contains(nativeField) && nativeField.V == "retained" && page.Fs.Count == 2, mode + " removal: form model cleanup preserves surviving field and action button");
            before = page.Jc;
            var idle = Code("drop.remove();child.remove();if(child.parentElement!==drop)throw Error('detached changed');", "repeat and detached descendant calls do not mutate the detached tree");
            Program.Check(idle.N == 0 && page.Jc == before, mode + " removal: detached no-op does not rebuild");
            Code("try{child.textContent='denied';throw Error('write accepted');}catch(e){if(!(e instanceof TypeError))throw e;}", "removal grants no detached mutation capability");
            var limited = ex("for(var i=0;i<300;i++)drop.remove();");
            Program.Check(limited.S == "limit" && limited.N == 0, mode + " removal: even no-op calls consume existing bridge budget; " + limited.S);
            Code("if(typeof drop!=='undefined')throw Error('realm retained');document.title='removal recovery';", "bridge exhaustion retires and recovers a fresh realm");
            engine.Je = false;
            var off = ex("document.getElementById('keep').remove();");
            Program.Check(off.S == "off" && off.N == 0 && engine.Id(page, "keep") != null, mode + " removal: permission revocation blocks deletion");
            engine.Je = true;
            var partial = ex("document.getElementById('keep').remove();throw Error('after removal');");
            Program.Check(partial.S == "js" && partial.N == 1 && engine.Id(page, "keep") == null, mode + " removal: completed deletion survives later script failure");
        }

        internal static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 720), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                W.On(view, null, 5000);
                form.Show();
                var page = await view.GoA(fixture);
                Program.Check(view.Xo != null && view.Xo.Ok, "native removal: inline initialization; " + (view.Xo == null ? "null" : view.Xo.S + "; " + view.Xo.M));
                var field = page.Fs.Single(item => item.El.Av("id") == "field");
                await View.Activate(view, "field");
                var editor = view.Controls.OfType<TextBox>().Single();
                editor.Text = "Elliot";
                await view.Xt;
                editor.Select(1, 3);
                var result = await view.JsA("document.getElementById('drop').remove();");
                View.Paint(view, null);
                Program.Check(result.Ok && result.N == 1 && view.Id("drop") == null && page.Fs.Count == 2 && page.Fs.Contains(field), "native removal: async deletion cleans connected field model");
                Program.Check(editor.Visible && editor.Text == "Elliot" && editor.SelectionStart == 1 && editor.SelectionLength == 3 &&
                    view.Controls.OfType<TextBox>().Single() == editor, "native removal: surviving focused editor/value/selection retained");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 720);
                View.Paint(view, output == null ? null : output + "-narrow.png");
                Program.Check(editor.Visible && view.ClientRectangle.Contains(editor.Bounds), "native removal: narrow retained editor fits");
                Program.Check(view.De(field.El) == 1 && !page.Fs.Contains(field) && !field.El.Mt(":focus"), "native removal: compact view alias deletes focused input and clears detached focus");
                View.Paint(view, null);
                Program.Check(!editor.Visible, "native removal: removed focused editor is hidden");
                page = await view.GoA(fixture);
                await View.Activate(view, "action");
                Program.Check(view.Xo != null && view.Xo.Ok && view.Id("drop") == null && view.Id("echo").C[0].X == "Removed" && page.Fs.Count == 2,
                    "native removal: click task deletes subtree and updates remaining document");
            }
        }
    }
}
