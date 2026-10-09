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
    internal static class AttributeEdits
    {
        internal static void Run(string fixture)
        {
            Core(fixture);
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, "trusted");
            engine.Je = true;
            page = engine.Go(fixture);
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(engine, page, script.Ex, "isolated", () => script.Sd);
        }

        private static void Core(string fixture)
        {
            void Check(bool value, string name) { Program.Check(value, "core attribute edits: " + name); }
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            var target = engine.Id(page, "target");
            // Invoked through reflection so the preceding runtime can run the same negative control.
            var set = typeof(E).GetMethod("Sa");
            var remove = typeof(E).GetMethod("Ra");
            Check(set != null && remove != null, "compact Sa/Ra aliases exist");
            int Set(N node, string name, string value) { return (int)set.Invoke(engine, new object[] { page, node, name, value }); }
            int Remove(N node, string name) { return (int)remove.Invoke(engine, new object[] { page, node, name }); }
            Check(Set(target, "DATA-state", "ready") == 1 && target.A.ContainsKey("data-state"), "ASCII-normalized literal attribute write");
            Check(page.Bs.Single(item => item.El == target).S.W == 190, "generic selector rebuild");
            var before = page.Jc;
            Check(Remove(target, "missing") == 0 && Remove(target, "") == 0 && page.Jc == before, "missing and invalid-name removal is a no-op");
            Check(Remove(target, "DATA-STATE") == 1 && page.Bs.Single(item => item.El == target).S.W == 120, "removal rebuilds selector style");
            before = page.Jc;
            foreach (var name in new[] { "", "a b", "a\tb", "a\nb", "a\rb", "a\fb", "a\0b", "a/b", "a=b", "a>b", new string('n', 65), null })
                Check(Set(target, name, "no") == 0 && page.Jc == before, "invalid or excessive name is atomic: " + (name == null ? "null" : name.Length.ToString()));
            foreach (var name in new[] { "0", ":", "<", "\"", "'", "a\vb", "a\u00a0b", "\u00c9" })
                Check(Set(target, name, "literal") == 1 && target.Av(name) == "literal" && Remove(target, name) == 1, "valid local name round trip: " + name.Length);
            Check(Set(target, new string('n', 64), new string('v', 1024)) == 1, "exact name and value bounds");
            before = page.Jc;
            Check(Set(target, "large", new string('v', 1025)) == 0 && Set(target, "null", null) == 0 && page.Jc == before, "value rejection is atomic");
            Check(Set(target, "id", new string('i', 513)) == 0 && Set(target, "class", new string('c', 513)) == 0, "ID/class retain existing reflection bounds");
            Check(Set(null, "x", "v") == 0 && Remove(null, "x") == -1 && Set(page.D, "x", "v") == 0 && Remove(page.D, "x") == -1 &&
                Set(target.C[0], "x", "v") == 0 && Remove(target.C[0], "x") == -1, "null/document/text targets are denied");
            var other = new E { Jb = false };
            var foreign = other.Go(fixture);
            Check(Set(other.Id(foreign, "target"), "x", "v") == 0 && Remove(other.Id(foreign, "target"), "x") == -1, "foreign nodes are denied");
            var child = engine.Id(page, "child");
            Check(engine.Jv(page, engine.Id(page, "parent"), "tx", "replacement") == 1 && Set(child, "x", "v") == 0 && Remove(child, "x") == -1,
                "detached nodes are denied");
            var limited = engine.Id(page, "echo");
            for (var index = limited.A.Count; index < 128; index++)
                Check(Set(limited, "data-" + index, "") == 1, "bounded attribute slot " + index);
            before = page.Jc;
            Check(Set(limited, "overflow", "no") == 0 && page.Jc == before && limited.A.Count == 128, "attribute count limit rejects new entries");
            Check(Set(limited, "id", "echo") == 1 && Remove(limited, "data-127") == 1 && Set(limited, "replacement", "yes") == 1, "replacement and removal work at count limit");
            for (var index = 1; index <= 63; index++) Check(Set(limited, "data-" + index, new string('x', 1024)) == 1, "bounded attribute text " + index);
            before = page.Jc;
            Check(Set(limited, "data-64", new string('x', 1024)) == 0 && page.Jc == before && limited.Av("data-64") == "", "aggregate text limit rejects atomically");
            engine.Je = false;
            Check(Set(target, "x", "v") == 0 && Remove(target, "id") == -1 && target.Av("id") == "target", "permission revocation denies both aliases");
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, string mode, Func<string> diagnostics = null)
        {
            void Check(bool value, string name) { Program.Check(value, mode + " attribute edits: " + name); }
            Jr Code(string code)
            {
                var result = ex(code);
                Check(result.Ok, "code: " + result.S + "; " + result.M + (result.Ok || diagnostics == null ? "" : "; " + diagnostics()));
                return result;
            }
            var target = engine.Id(page, "target");
            var field = page.Fs.Single(item => item.El.Av("id") == "field");
            Code("var target=document.getElementById('target'),field=document.getElementById('field');if(typeof target.setAttribute!=='function'||typeof target.removeAttribute!=='function')throw new Error('missing attribute writers');");
            var changed = Code("if(target.setAttribute('DATA-state','ready')!==undefined||target.getAttribute('data-STATE')!=='ready'||document.querySelector('[data-state=ready]')!==target)throw new Error('write');");
            Check(changed.N == 1 && page.Bs.Single(item => item.El == target).S.W == 190, "attribute selector and layout update");
            changed = Code("if(target.removeAttribute('missing')!==undefined||target.removeAttribute('')!==undefined||target.removeAttribute('bad name')!==undefined)throw new Error('missing removal');");
            Check(changed.N == 0, "missing removals succeed without mutation counts");
            changed = Code("target.removeAttribute('DATA-state');if(target.hasAttribute('data-state')||target.getAttribute('data-state')!==null)throw new Error('remove');");
            Check(changed.N == 1 && page.Bs.Single(item => item.El == target).S.W == 120, "attribute removal restores cascade");
            Code("target.setAttribute('ID','renamed');target.setAttribute('CLASS',' active  active ');if(target.id!=='renamed'||target.className!==' active  active '||document.getElementById('renamed')!==target)throw new Error('reflection');target.removeAttribute('id');target.removeAttribute('class');if(target.id!==''||target.className!==''||target.hasAttribute('id')||target.classList.length!==0)throw new Error('reflection removal');target.id='target';");
            Check(engine.Id(page, "target") == target && !target.A.ContainsKey("class"), "generic ID/class removal retains wrapper identity");
            changed = Code("var order='';target.setAttribute({toString(){order+='n';return 'data-literal';}},{toString(){order+='v';return '${literal}';}});if(order!=='nv'||target.getAttribute('data-literal')!=='${literal}')throw new Error('conversion');target.setAttribute(null,undefined);target.setAttribute(37n,null);if(target.getAttribute('null')!=='undefined'||target.getAttribute('37')!=='null')throw new Error('DOM strings');");
            Check(changed.N == 3 && target.Av("data-literal") == "${literal}", "literal values and ordered realm conversion");
            Code("var rejected=0,conversions=0;for(var receiver of [null,{},new Proxy(target,{})])for(var method of ['setAttribute','removeAttribute']){try{Reflect.apply(target[method],receiver,[{toString(){conversions++;return 'x';}},{toString(){conversions++;return 'v';}}]);}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}if(rejected!==6||conversions!==0)throw new Error('receiver order');");
            Code("var rejected=0;for(var args of [[],['x']]){try{Reflect.apply(target.setAttribute,target,args);}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}try{target.removeAttribute();}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}if(rejected!==3)throw new Error('required arguments');");
            Code("var rejected=0;for(var args of [[Symbol('n'),'v'],['n',Symbol('v')]]){try{Reflect.apply(target.setAttribute,target,args);}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}try{target.removeAttribute(Symbol('n'));}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}if(rejected!==3)throw new Error('symbols');");
            changed = Code("var converted=0,rejected=0;for(var name of ['', 'a b','a\\tb','a\\nb','a\\rb','a\\fb','a\\0b','a/b','a=b','a>b']){try{target.setAttribute(name,{toString(){converted++;return 'v';}});}catch(e){if(!(e instanceof DOMException)||e.name!=='InvalidCharacterError'||e.code!==5)throw e;rejected++;}}if(rejected!==10||converted!==10)throw new Error('invalid names');");
            Check(changed.N == 0, "invalid names throw DOMException after value conversion without writes");
            Code("for(var name of ['0',':','<','\"',\"'\",'a\\vb','a\\u00a0b','\\u00c9']){target.setAttribute(name,'literal');if(target.getAttribute(name)!=='literal')throw new Error('local name');target.removeAttribute(name);}");
            changed = Code("var rejected=0;for(var args of [['n'.repeat(65),'v'],['large','x'.repeat(1025)],['id','i'.repeat(513)],['class','c'.repeat(513)]]){try{Reflect.apply(target.setAttribute,target,args);}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}if(rejected!==4)throw new Error('bounds');");
            Check(changed.N == 0 && target.Av("id") == "target", "bounded writes reject without changing ID or count");
            Code("target.setAttribute('n'.repeat(64),'v'.repeat(1024));if(target.getAttribute('n'.repeat(64)).length!==1024)throw new Error('exact bounds');target.removeAttribute('n'.repeat(64));");
            Code("var set=target.setAttribute,remove=target.removeAttribute;if(set!==field.setAttribute||remove!==field.removeAttribute||set.name!=='setAttribute'||set.length!==2||remove.name!=='removeAttribute'||remove.length!==1)throw new Error('method metadata');for(var method of [set,remove]){if(Object.prototype.hasOwnProperty.call(method,'prototype'))throw new Error('method prototype');var rejected=false;try{Reflect.construct(String,[],method);}catch(e){rejected=e instanceof TypeError;}if(!rejected)throw new Error('method constructor');}set.call(field,'data-state','ready');remove.call(field,'data-state');");
            Code("var oldString=String,oldException=DOMException,oldError=TypeError;String=DOMException=TypeError=undefined;try{target.setAttribute('data-captured',37n);target.removeAttribute('data-captured');var rejected=false;try{target.setAttribute('bad name','v');}catch(e){rejected=e instanceof oldException;}if(!rejected)throw new Error('captured exception');}finally{String=oldString;DOMException=oldException;TypeError=oldError;}");
            Code("field.value='edited';field.setAttribute('aria-label','Renamed field');field.setAttribute('required','false');field.setAttribute('readonly','');field.setAttribute('placeholder','Hint');field.setAttribute('name','renamed');field.setAttribute('value','new default');");
            Check(page.Fs.Contains(field) && field.V == "edited" && field.D == "new default" && field.Rq && field.Ro && field.P == "Hint" && field.N == "renamed" &&
                page.Axs.Single(item => item.El == field.El).N == "Renamed field", "field state, defaults and accessibility update while keeping edited value");
            Code("field.removeAttribute('readonly');field.removeAttribute('required');field.removeAttribute('aria-label');field.removeAttribute('value');document.getElementById('flag').removeAttribute('disabled');");
            Check(!field.Ro && !field.Rq && field.V == "edited" && field.D == "" && !page.Fs.Single(item => item.El.Av("id") == "flag").Ds &&
                page.Axs.Single(item => item.El == field.El).N == "Original label", "boolean removals and accessibility fallback rebuild");
            Code("target.setAttribute('style','width: 210px; color: #168477');target.setAttribute('title','Literal title');");
            Check(page.Bs.Single(item => item.El == target).S.W == 210, "inline style projection rebuilds");
            Code("target.setAttribute('hidden','false');");
            Check(!page.Bs.Any(item => item.El == target), "hidden presence suppresses paint regardless of literal value");
            Code("target.removeAttribute('hidden');target.removeAttribute('style');");
            Check(page.Bs.Single(item => item.El == target).S.W == 120, "hidden and style removals restore paint");
            var details = engine.Id(page, "disclosure");
            Code("document.getElementById('disclosure').setAttribute('OPEN','false');");
            Check(details.Op && page.Bs.Any(item => item.El == engine.Id(page, "inside") && !item.Hd), "open presence updates disclosure state and visible contents");
            Code("document.getElementById('disclosure').removeAttribute('open');");
            Check(!details.Op && page.Bs.Any(item => item.El == engine.Id(page, "inside") && item.Hd), "open removal closes disclosure");
            Code("document.getElementById('link').setAttribute('href','about:blank#changed');");
            Check(page.Ls.Any(item => item.U.Fragment == "#changed"), "safe link targets rebuild from live attributes");
            Code("document.documentElement.setAttribute('data-root','yes');document.querySelector('head').setAttribute('data-head','yes');var child=document.getElementById('child');document.getElementById('parent').textContent='replacement';var rejected=0;for(var method of ['setAttribute','removeAttribute']){try{Reflect.apply(child[method],child,['x','v']);}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}if(rejected!==2)throw new Error('detached writes');");
            engine.Je = false;
            var denied = ex("target.setAttribute('x','denied');target.removeAttribute('id');");
            Check(denied.S == "off" && denied.N == 0 && target.Av("id") == "target" && !target.A.ContainsKey("x"), "permission revocation denies writers");
        }

        internal static async Task Native(string fixture, string output)
        {
            void Check(bool value, string name) { Program.Check(value, "native attribute edits: " + name); }
            using (var form = new Form { ClientSize = new Size(940, 820), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                async Task<Xr> Code(string code)
                {
                    var result = await view.JsA(code);
                    Check(result.Ok, "code: " + result.S + "; " + result.M + (result.Ok ? "" : "; " + (view.Xs as W)?.At.Sd));
                    return result;
                }
                form.Controls.Add(view);
                W.On(view, null, 5000);
                form.Show();
                var page = await view.GoA(fixture);
                var field = page.Fs.Single(item => item.El.Av("id") == "field");
                await Code("var field=document.getElementById('field');field.addEventListener('input',function(){document.getElementById('echo').textContent=this.getAttribute('data-state')+'|'+this.value;});");
                await View.Activate(view, "field");
                var editor = view.Controls.OfType<TextBox>().Single();
                editor.Text = "Elliot";
                await view.Xt;
                var width = editor.Width;
                var accessible = view.Ao.GetFocused();
                await Code("field.setAttribute('data-state','ready');field.setAttribute('aria-label','Edited field');document.getElementById('target').setAttribute('data-state','ready');document.getElementById('disclosure').setAttribute('open','');");
                Check(page.Fs.Contains(field) && field.V == "Elliot" && view.Controls.OfType<TextBox>().Single() == editor && editor.Visible && editor.Text == "Elliot",
                    "generic edits retain active field and editor identity");
                Check(field.S.W == 220 && editor.Width != width && editor.Width == view.Ab(view.Ax(field.El)).Width - 2, "attribute CSS reflows active editor bounds");
                Check(accessible.Name == "Edited field" && view.Ao.GetFocused().Name == "Edited field", "retained and fresh accessible objects follow aria-label");
                editor.Text = "Again";
                await view.Xt;
                Check(view.Id("echo").C[0].X == "ready|Again", "input events read changed attributes on retained wrapper");
                await Code("field.setAttribute('onclick',\"document.title='Attribute handler';\");");
                await View.Activate(view, "field");
                Check(view.T == "Attribute handler", "local inline handler follows newly written attribute");
                await Code("field.removeAttribute('onclick');document.title='Removed handler';");
                await View.Activate(view, "field");
                Check(view.T == "Removed handler", "removed inline handler is no longer activated");
                await View.Activate(view, "field");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 820);
                View.Paint(view, output == null ? null : output + "-narrow.png");
                Check(editor.Visible && view.ClientRectangle.Contains(editor.Bounds), "mutated editor fits narrow viewport");
                await Code("field.removeAttribute('data-state');field.removeAttribute('aria-label');document.getElementById('disclosure').removeAttribute('open');");
                Check(field.S.W == 160 && field.V == "Again" && accessible.Name == "Original label" && !view.Id("disclosure").Op,
                    "removal restores field layout, labels and disclosure without losing text");
                Check(view.Sa(field.El, "data-state", "ready") == 1 && field.S.W == 220 && view.Da(field.El, "missing") == 0 && field.V == "Again",
                    "compact view alias updates model and preserves editor text");
                Check(view.Da(field.El, "data-state") == 1 && field.S.W == 160 && view.Da(null, "x") == -1 && field.V == "Again",
                    "compact removal distinguishes missing, removed and invalid targets");
            }
        }
    }
}
