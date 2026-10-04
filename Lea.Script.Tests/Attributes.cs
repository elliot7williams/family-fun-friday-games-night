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
    internal static class Attributes
    {
        internal static void Run(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, "in-process");
            engine.Je = true;
            page = engine.Go(fixture);
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(engine, page, script.Ex, "isolated", () => script.Sd);
        }

        internal static async Task Native(string fixture, string output)
        {
            void Check(bool value, string name) { Program.Check(value, "native attributes: " + name); }
            using (var form = new Form { ClientSize = new Size(940, 820), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                async Task<Xr> Code(string code)
                {
                    var result = await view.JsA(code);
                    var runtime = view.Xs as W;
                    Check(result.Ok, "code: " + result.S + "; " + result.M +
                        (result.Ok || runtime == null ? "" : "; " + runtime.At.Sd));
                    return result;
                }
                form.Controls.Add(view);
                W.On(view, null, 5000);
                form.Show();
                var page = await view.GoA(fixture);
                var target = view.Id("target");
                var field = page.Fs.Single(item => item.El.Av("id") == "field");
                await Code("var field=document.getElementById('field');field.addEventListener('input',function(){document.getElementById('echo').textContent=this.id+'|'+this.className+'|'+this.value;});field.onclick=function(){document.title=this.id+'|'+this.className;};");
                await View.Activate(view, "field");
                var editor = view.Controls.OfType<TextBox>().Single();
                var accessible = view.Ao.GetFocused();
                Check(editor.Visible && accessible != null && accessible.Name == "Original label", "focused editor and original accessible label");
                editor.Text = "Elliot";
                await view.Xt;
                var oldWidth = editor.Width;
                var result = await Code("var target=document.getElementById('target');target.id='renamed';target.className='active';field.id='field-new';field.className='active';if(document.getElementById('field')!==null||document.getElementById('field-new')!==field)throw new Error('identity');");
                Check(result.N == 4 && view.Id("renamed") == target && page.Fs.Contains(field) &&
                    view.Controls.OfType<TextBox>().Single() == editor && editor.Visible && editor.Text == "Elliot" && field.V == "Elliot",
                    "metadata writes retain node, field, editor and user text");
                Check(field.S.W == 190 && editor.Width != oldWidth && editor.Width == view.Ab(view.Ax(field.El)).Width - 2,
                    "class replacement reflows active editor bounds");
                Check(accessible.Name == "Updated label" && view.Ao.GetFocused().Name == "Updated label",
                    "retained and fresh native accessible objects follow renamed label association");
                editor.Text = "Again";
                await view.Xt;
                Check(view.Id("echo").C[0].X == "field-new|active|Again", "input event exposes live metadata on the retained wrapper");
                await View.Activate(view, "field-new");
                Check(view.T == "field-new|active", "native activation finds renamed element and retains its handler");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 820);
                View.Paint(view, output == null ? null : output + "-narrow.png");
                Check(editor.Visible && view.ClientRectangle.Contains(editor.Bounds), "renamed active editor remains within narrow viewport");
                await Code("field.classList.remove('active');if(field.className!=='')throw new Error('class reflection');field.id='field';");
                Check(view.Id("field") == field.El && accessible.Name == "Original label" && field.V == "Again",
                    "classList and ID restoration refresh native state without losing edits");
            }
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, string mode, Func<string> diagnostics = null)
        {
            void Check(bool value, string name) { Program.Check(value, mode + " attributes: " + name); }
            Jr Code(string code)
            {
                var result = ex(code);
                var details = result.Ok ? "" : "; " + (diagnostics == null ? "" : diagnostics()) + "; code=" +
                    code.Substring(0, Math.Min(256, code.Length));
                Check(result.Ok, "code: " + result.S + "; " + result.M + details);
                return result;
            }
            var target = engine.Id(page, "target");
            var field = page.Fs.Single(item => item.El.Av("id") == "field");
            Code("var target=document.getElementById('target');if(target.id!=='target'||target.className!==''||target.hasAttribute('class')||!target.hasAttribute('ID'))throw new Error('attribute reflection');");
            var changed = Code("target.id='renamed';target.className='active';if(target!==document.getElementById('renamed')||document.getElementById('target')!==null||target!==document.querySelector('.active'))throw new Error('identity');");
            Check(changed.N == 2 && engine.Id(page, "renamed") == target && target.Av("class") == "active", "renaming retains wrapper and node identity");
            var block = page.Bs.Single(item => item.El == target);
            Check(block.S.F == "#168477" && block.S.W == 190, "ID removal and class replacement rebuild the CSS cascade");
            Code(@"target.className='  active\tactive\nextra\f';if(target.className!=='  active\tactive\nextra\f'||target.getAttribute('CLASS')!==target.className||!target.classList.contains('extra'))throw new Error('raw class');");
            Check(target.Av("class") == "  active\tactive\nextra\f", "class reflection preserves raw whitespace and duplicate tokens");
            Code("target.classList.add('later');if(!target.className.includes('later'))throw new Error('live className');target.classList.remove('later');");
            Code("target.id='';target.className='';if(!target.hasAttribute('id')||!target.hasAttribute('class')||target.id!==''||target.className!==''||target.getAttribute('id')!==''||target.getAttribute('class')!=='')throw new Error('empty attributes');");
            Check(target.A.ContainsKey("id") && target.A.ContainsKey("class") && engine.Id(page, "renamed") == null, "empty reflected attributes remain present");
            Code("target.id=null;target.className=undefined;if(target.id!=='null'||target.className!=='undefined')throw new Error('DOM strings');target.id=37n;if(target.id!=='37')throw new Error('BigInt string');");
            Code("target.id='${literal}';target.className='${literal}';");
            Check(target.Av("id") == "${literal}" && target.Av("class") == "${literal}", "typed attribute values are not Lea commands or interpolated variables");
            changed = Code("target.id={toString:function(){document.title='Coerced';return 'converted';}};target.className={toString:function(){return 'active';}};");
            Check(changed.N == 3 && page.T == "Coerced" && target.Av("id") == "converted", "value conversion runs in the realm before literal parent mutation");
            Code("var oldString=String;String=undefined;try{target.id='captured';target.className='active';if(target.getAttribute('ID')!=='captured'||!target.hasAttribute('CLASS'))throw new Error('captured string');}finally{String=oldString;}");
            changed = Code("var rejected=0;for(var name of ['id','className']){try{target[name]=Symbol('value');}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}if(rejected!==2)throw new Error('Symbol conversion');");
            Check(changed.N == 0 && target.Av("id") == "captured" && target.Av("class") == "active", "Symbol conversion fails before mutation");
            Code("var rejected=0;for(var name of ['getAttribute','hasAttribute']){try{target[name](Symbol('name'));}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}if(rejected!==2)throw new Error('Symbol attribute name');");
            changed = Code("var rejected=0;for(var name of ['id','className']){try{target[name]='x'.repeat(513);}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}if(rejected!==2)throw new Error('attribute bound');");
            Check(changed.N == 0 && target.Av("id") == "captured" && target.Av("class") == "active", "oversized writes are atomic failures");
            Code("target.id='x'.repeat(512);target.className='y'.repeat(512);if(document.getElementById('x'.repeat(512))!==target)throw new Error('limit identity');");
            Check(target.Av("id").Length == 512 && target.Av("class").Length == 512, "both reflected values accept the exact bound");
            Code("document.documentElement.id='root-new';document.documentElement.className='root-class';document.querySelector('head').id='head-new';if(document.getElementById('root-new')!==document.documentElement)throw new Error('root attributes');");
            Check(page.D.C.Any(item => item.K == "html" && item.Av("class") == "root-class"), "metadata applies to root and nonpainted elements");
            Code("var field=document.getElementById('field');field.value='edited';field.id='field-new';field.className='active';if(document.getElementById('field-new')!==field||field.value!=='edited')throw new Error('field identity');");
            Check(page.Fs.Contains(field) && field.V == "edited" && field.El.Av("id") == "field-new", "native field identity and edited value survive metadata changes");
            Check(page.Axs.Single(item => item.El == field.El).N == "Updated label", "ID changes rebuild accessible label references");
            Code("var flag=document.getElementById('flag');if(!flag.hasAttribute('disabled')||flag.getAttribute('disabled')!==''||flag.hasAttribute('missing'))throw new Error('presence');");
            Code("var child=document.getElementById('child');document.getElementById('parent').textContent='replacement';var rejected=false;try{child.id='detached';}catch(e){rejected=e instanceof TypeError;}if(!rejected)throw new Error('detached mutation');");
            engine.Je = false;
            var denied = ex("target.className='denied';");
            Check(denied.S == "off" && denied.N == 0 && target.Av("class").Length == 512, "permission revocation prevents attribute writes");
        }
    }
}
