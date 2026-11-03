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
    internal static class Selectors
    {
        internal static void Run(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, "in-process");
            page = engine.Go(fixture);
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(engine, page, script.Ex, "isolated", () => script.Sd);
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, string mode, Func<string> diagnostics = null)
        {
            Jr Code(string code, string name)
            {
                var result = ex(code);
                Program.Check(result.Ok, mode + " selectors: " + name + "; " + result.S + "; " + result.M +
                    (result.Ok || diagnostics == null ? "" : "; " + diagnostics()));
                return result;
            }
            Code("var child=document.getElementById('child');if(typeof child.matches!=='function'||typeof child.closest!=='function'||child.matches.length!==1||child.closest.length!==1)throw Error('missing element selector API');", "element selector methods and arity; first wrapper uses handle zero");
            Code("var parent=document.getElementById('parent'),action=document.getElementById('action'),field=document.getElementById('field'),tree=document.getElementById('tree');if(!child.matches('span#child')||child.matches('section')||!child.matches('body div > section button > span'))throw Error('matching');", "self compound and relationship matching");
            Code("if(child.closest('span')!==child||child.closest('button')!==action||child.closest('.panel')!==parent||child.closest('body')!==document.body||child.closest('html')!==document.documentElement||child.closest('.missing')!==null)throw Error('ancestors');", "inclusive nearest ancestor and null result");
            Code("if(child.closest('body,section')!==parent||child.closest('section,body')!==parent||!child.matches('section,span'))throw Error('list order');", "selector-list order does not override nearest ancestor order");
            Code("if(!child.matches(':is(span,div):not(.missing)')||!parent.matches('section:has(> button > span)')||!child.matches('button > span:first-child:nth-last-child(1)')||!document.documentElement.matches(':root'))throw Error('functions');", "functional relational structural and root selectors reuse the compiler");
            Code("var flag=document.getElementById('flag');if(!flag.matches('input + input')||!flag.matches('input ~ input:disabled')||field.matches(':disabled'))throw Error('siblings');", "sibling relationships and form state");
            Code("var mt=child.matches,cl=child.closest;if(!mt.call(parent,'section')||cl.call(field,'section')!==parent||!mt.call(child,'span'))throw Error('borrowed receiver');", "borrowed methods use the actual branded element receiver");
            Code("var converted=0;for(var method of [mt,cl])for(var receiver of [null,undefined,3,{},document,window,Object.create(child)]){try{method.call(receiver,{toString(){converted++;return 'span';}});throw Error('receiver accepted');}catch(e){if(!(e instanceof TypeError))throw e;}}if(converted!==0)throw Error('early conversion');", "invalid receivers fail before selector conversion");
            Code("for(var method of [mt,cl]){try{method.call(child);throw Error('missing accepted');}catch(e){if(!(e instanceof TypeError))throw e;}try{method.call(child,Symbol());throw Error('Symbol accepted');}catch(e){if(!(e instanceof TypeError))throw e;}}if(child.matches(undefined)||child.matches(null)||child.closest(undefined)!==null)throw Error('explicit DOM strings');", "required argument and DOM string conversion");
            Code("var conversions=0;if(!child.matches({toString(){conversions++;return '#child';}})||conversions!==1)throw Error('conversion');var oldString=String;String=undefined;try{if(child.closest('#parent')!==parent)throw Error('captured converter');}finally{String=oldString;}", "single conversion and captured intrinsic");
            Code("var oldGet=WeakMap.prototype.get,oldSet=WeakMap.prototype.set,oldKey=Object.getOwnPropertyDescriptor(Object.prototype,'key');WeakMap.prototype.get=WeakMap.prototype.set=()=>{throw Error('poisoned receiver map');};Object.defineProperty(Object.prototype,'key',{set(){throw Error('poisoned receiver key');},configurable:true});try{var other=document.getElementById('target');if(!other.matches('p')||other.closest('#target')!==other||!mt.call(child,'span')||cl.call(field,'.panel')!==parent)throw Error('captured receiver state');}finally{WeakMap.prototype.get=oldGet;WeakMap.prototype.set=oldSet;if(oldKey)Object.defineProperty(Object.prototype,'key',oldKey);else delete Object.prototype.key;}", "private receiver registration and lookup survive map and prototype replacement");
            Code("for(var selector of ['', '[', 'span >', '#child,', 'span,,div', ':unknown', ':scope'])for(var method of [mt,cl]){try{method.call(child,selector);throw Error('invalid accepted '+selector);}catch(e){if(!(e instanceof DOMException)||e.name!=='SyntaxError'||e.code!==12)throw e;}}", "invalid and unsupported selectors throw branded SyntaxError");
            Code("for(var selector of [Array(18).join('span,' )+'span',Array(18).join('div ')+'span','#child'+' '.repeat(1019)])for(var method of [mt,cl]){try{method.call(child,selector);throw Error('limit accepted');}catch(e){if(!(e instanceof DOMException)||e.name!=='SyntaxError')throw e;}}if(!child.matches('#child'+' '.repeat(1018)))throw Error('exact bound');", "group part and raw 1024-unit selector bounds");
            var changes = page.Jc;
            var read = Code("if(!child.matches('span')||child.closest('section')!==parent)throw Error('read');", "selector methods are read only");
            Program.Check(read.N == 0 && page.Jc == changes, mode + " selectors: reads do not mutate or rebuild the bound page");
            Code("child.id='renamed-child';parent.className='panel active';if(child.matches('#child')||!child.matches('#renamed-child')||child.closest('.active')!==parent)throw Error('live state');", "subsequent calls observe live metadata");
            var field = page.Fs.Single(item => item.El.Av("id") == "field");
            Code("field.value='edited';if(!field.matches('input')||field.closest('.panel')!==parent||field.value!=='edited')throw Error('field');", "field matching retains edited values");
            Program.Check(page.Fs.Contains(field) && field.V == "edited", mode + " selectors: matching preserves native field identity");
            Code("tree.textContent='replacement';if(child.closest('section')!==parent||child.closest('div')!==null||!child.matches('button > span')||child.closest('#renamed-child')!==child)throw Error('detached tree');", "detached subtree retains internal ancestry without old document ancestors");
            Code("var rejected=false;try{child.id='detached write';}catch(e){rejected=e instanceof TypeError;}if(!rejected||!child.matches('#renamed-child'))throw Error('detached ownership');", "detached reads do not grant mutation capability");
            Code("try{child.matches('[');}catch(e){if(e.name!=='SyntaxError')throw e;}if(child.closest('section')!==parent)throw Error('caught selector failure reset realm');", "caught syntax failures preserve the existing realm");
            var denied = ex("for(var i=0;i<300;i++)child.matches('span');");
            Program.Check(denied.S == "limit", mode + " selectors: existing bridge-call ceiling applies; " + denied.S);
            Code("if(typeof child!=='undefined')throw Error('realm survived limit');document.title='selector recovery';", "bridge exhaustion resets the realm and permits recovery");
            engine.Je = false;
            var off = ex("document.getElementById('target').matches('p');");
            Program.Check(off.S == "off" && off.N == 0, mode + " selectors: permission revocation still gates reads");
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
                Program.Check(view.Xo != null && view.Xo.Ok, "native selectors: inline delegation initializes; " + (view.Xo == null ? "null" : view.Xo.S + "; " + view.Xo.M));
                await View.Activate(view, "action");
                Program.Check(view.Id("echo").C[0].X == "Chosen", "native selectors: delegated click finds a matching button");
                await View.Activate(view, "field");
                var editor = view.Controls.OfType<TextBox>().Single();
                editor.Text = "Elliot";
                await view.Xt;
                Program.Check(view.T == "parent|Elliot" && editor.Visible, "native selectors: input handler finds nearest panel");
                var field = page.Fs.Single(item => item.El.Av("id") == "field");
                var result = await view.JsA("var field=document.getElementById('field');if(field.closest('.panel')!==document.getElementById('parent')||!field.matches('input'))throw Error('native identity');");
                Program.Check(result.Ok && result.N == 0 && page.Fs.Contains(field) && field.V == "Elliot" && view.Controls.OfType<TextBox>().Single() == editor,
                    "native selectors: reads retain model editor and value");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 820);
                View.Paint(view, output == null ? null : output + "-narrow.png");
                Program.Check(editor.Visible && view.ClientRectangle.Contains(editor.Bounds), "native selectors: narrow retained editor fits viewport");
            }
        }
    }
}
