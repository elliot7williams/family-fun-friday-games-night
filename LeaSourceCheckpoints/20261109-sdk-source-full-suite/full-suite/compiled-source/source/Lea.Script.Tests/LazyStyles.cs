using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime.Interop;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class LazyStyles
    {
        internal static void Run(string fixture, string mode)
        {
            if (mode == "core") { Core(fixture); return; }
            if (mode != "trusted" && mode != "isolated") throw new ArgumentException("Use core, trusted, isolated or native lazy-style mode.");
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            if (mode == "trusted")
            {
                using (var script = new Js(engine, page))
                    Suite(id => engine.Id(page, id), code => Task.FromResult(script.Ex(code)), mode).GetAwaiter().GetResult();
            }
            else
            {
                using (var script = new Jp(engine, page))
                {
                    Suite(id => engine.Id(page, id), code => Task.FromResult(script.Ex(code)), mode).GetAwaiter().GetResult();
                    Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15 && script.Cp == 2500 && script.Mm == 134217728,
                        "isolated styles retain verified policy and resource limits");
                    var process = System.Diagnostics.Process.GetProcessById(script.Pid);
                    using (process)
                    {
                        var handle = process.Handle;
                        script.Dispose();
                        Check(process.WaitForExit(1000), "isolated disposal retires the pinned exact worker");
                    }
                }
            }
        }

        private static void Core(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            var definitions = 0;
            var objects = new List<object>();
            var configured = false;
            using (var script = new Js(engine, page, configure =>
            {
                var options = new Options(); configure(options);
                configured = VmOptions.Snapshot(options) == VmOptions.Snapshot(Vo.Of(1000)) && !options.AgentCanSuspend;
                var vm = new Jint.Engine(options);
                var constructor = vm.GetValue("Object").AsObject();
                var original = constructor.Get("defineProperty");
                constructor.Set("defineProperty", (JsValue)new ClrFunction(vm, "defineProperty", (_, args) =>
                {
                    if (args.Length >= 3 && args[1].IsString() && args[2].IsObject())
                    {
                        var name = args[1].AsString(); var descriptor = args[2].AsObject();
                        var enumerable = descriptor.Get("enumerable");
                        if ((name == "color" || name == "backgroundColor" || name == "width" || name == "height" || name == "display" || name == "fontSize") &&
                            descriptor.Get("set") is Function && descriptor.Get("get").IsUndefined() && enumerable.IsBoolean() && enumerable.AsBoolean())
                        {
                            definitions++;
                            if (!objects.Exists(value => ReferenceEquals(value, args[0]))) objects.Add(args[0]);
                        }
                    }
                    return vm.Invoke(original, constructor, args);
                }), true);
                return vm;
            }))
            {
                void Code(string code, string label)
                { var result = script.Ex(code); Check(result.Ok, label + "; " + result.S + "; " + result.M); }
                Code("var rows=document.querySelectorAll('.sample');if(rows.length!==16)throw Error('count');for(var row of rows){if(!row.id)throw Error('id');}if(typeof makeStyle!=='undefined')throw Error('private factory');document.title='Wrapped';", "core wraps sixteen elements without style reads");
                Console.WriteLine("Lazy style allocation control: definitions=" + definitions + "; objects=" + objects.Count + "; wrapped=16");
                Check(configured && definitions == 0 && objects.Count == 0, "core wrapping allocates no style setters or style objects under unchanged options");
                Code("var first=rows[0],style=first.style;if(Object.keys(style).join(',')!=='color,backgroundColor,width,height,display,fontSize')throw Error('keys');", "core first style read creates the supported six-property facade");
                Check(definitions == 6 && objects.Count == 1, "core exactly one style object and six setters are created on first read");
                Code("if(first.style!==style||document.getElementById('target').style!==style)throw Error('identity');", "core repeated lookup preserves style identity");
                Check(definitions == 6 && objects.Count == 1, "core repeated reads do not rebuild style setters");
                Code("var other=rows[1],otherStyle=other.style;if(otherStyle===style)throw Error('shared style');style.color='#123456';otherStyle.color='#b84235';", "core independent elements retain bound setters");
                Check(definitions == 12 && objects.Count == 2 && engine.Id(page, "target").Av("style").Contains("#123456") && engine.Id(page, "other").Av("style").Contains("#b84235"), "core each requested element owns one independent style object");
                Code("for(var row of document.querySelectorAll('.sample')){row.textContent;row.id;}document.title='Still wrapped';", "core later non-style reads keep unused facades dormant");
                Check(definitions == 12 && objects.Count == 2, "core fourteen untouched elements still allocate no style setters");
                Code("var savedDefine=Object.defineProperty,savedGet=WeakMap.prototype.get,savedSet=WeakMap.prototype.set;try{Object.defineProperty=function(){throw Error('public define');};WeakMap.prototype.get=function(){throw Error('public get');};WeakMap.prototype.set=function(){throw Error('public set');};rows[2].style.backgroundColor='#e7eaf0';}finally{Object.defineProperty=savedDefine;WeakMap.prototype.get=savedGet;WeakMap.prototype.set=savedSet;}", "core lazy creation uses captured definition and receiver-state intrinsics");
                Check(definitions == 18 && objects.Count == 3 && engine.Id(page, "cell-2").Av("style").Contains("#e7eaf0"), "core third requested style adds only its six setters");
                Code("globalThis.makeStyle=function(){throw Error('page factory');};globalThis.styleNames=[];rows[3].style.color='#0b7a3e';", "core page collisions cannot replace the private lazy factory");
                Check(definitions == 24 && objects.Count == 4 && engine.Id(page, "cell-3").Av("style").Contains("#0b7a3e"), "core four requested styles allocate twenty-four setters instead of ninety-six");
            }
        }

        private static async Task Suite(Func<string, N> node, Func<string, Task<Jr>> ex, string mode)
        {
            async Task Code(string code, string label)
            { var result = await ex(code); Check(result.Ok, mode + ": " + label + "; " + result.S + "; " + result.M); }
            await Code("var first=document.getElementById('target'),other=document.getElementById('other'),meta=Object.getOwnPropertyDescriptor(first,'style');if(typeof meta.get!=='function'||meta.set!==undefined||meta.enumerable||meta.configurable||'value' in meta)throw Error('style metadata');var style=first.style;", "style is an own read-only lazy accessor");
            await Code("'use strict';if(first.style!==style||meta.get.call(first)!==style||meta.get.call(other)!==other.style||style===other.style)throw Error('style identity');for(var receiver of [{},document,window,null]){var failed=false;try{meta.get.call(receiver);}catch(error){failed=error instanceof TypeError;}if(!failed)throw Error('style receiver');}var failed=false;try{first.style={};}catch(error){failed=error instanceof TypeError;}if(!failed||first.style!==style)throw Error('style overwrite');", "same-object, borrowed getter, illegal receivers and strict overwrite behavior");
            await Code("if(Object.keys(style).join(',')!=='color,backgroundColor,width,height,display,fontSize')throw Error('style keys');for(var name of Object.keys(style)){var slot=Object.getOwnPropertyDescriptor(style,name);if(typeof slot.set!=='function'||slot.get!==undefined||!slot.enumerable||slot.configurable)throw Error('setter metadata');}style.extra=7;if(style.extra!==7||style.width!==undefined)throw Error('facade compatibility');", "write-only setter metadata and ordinary expandos remain intact");
            await Code("style.color='#123456';style.backgroundColor='#eef6ff';style.width='120px';style.height='24px';style.display='block';style.fontSize='14px';", "all six existing style aliases still write");
            var text = node("target").Av("style");
            Check(text.Contains("#123456") && text.Contains("#eef6ff") && text.Contains("120px") && text.Contains("24px") && text.Contains("block") && text.Contains("14px"), mode + ": all six writes reach the bound model");
            await Code("first.textContent='Retained style';style.color='#b84235';if(first.style!==style)throw Error('retained style');", "retained facade survives other DOM mutations");
            Check(node("target").Av("style").Contains("#b84235") && !node("other").Av("style").Contains("#b84235"), mode + ": retained writes do not target another element");
            await Code("Object.getOwnPropertyDescriptor(style,'width').set.call(other.style,'130px');", "existing setter closures remain bound even with a foreign this");
            Check(node("target").Av("style").Contains("130px") && !node("other").Av("style").Contains("130px"), mode + ": borrowed setter preserves the original bound-target behavior");
            await Code("var third=document.getElementById('cell-2'),savedDefine=Object.defineProperty,savedGet=WeakMap.prototype.get,savedSet=WeakMap.prototype.set;try{Object.defineProperty=function(){throw Error('public define');};WeakMap.prototype.get=function(){throw Error('public get');};WeakMap.prototype.set=function(){throw Error('public set');};third.style.backgroundColor='#e7eaf0';}finally{Object.defineProperty=savedDefine;WeakMap.prototype.get=savedGet;WeakMap.prototype.set=savedSet;}", "lazy construction survives page intrinsic overrides");
            Check(node("cell-2").Av("style").Contains("#e7eaf0"), mode + ": captured lazy construction still writes the model");
            await Code("if(typeof makeStyle!=='undefined'||'makeStyle' in globalThis||Function('return typeof makeStyle')()!=='undefined')throw Error('private factory');globalThis.makeStyle=37;globalThis.__lea=function(){throw Error('page bridge');};document.getElementById('cell-3').style.color='#0b7a3e';", "factory remains private despite page-global collisions");
            Check(node("cell-3").Av("style").Contains("#0b7a3e"), mode + ": private factory and bridge remain connected");
            await Code("var detached=document.getElementById('cell-4'),staleStyle=detached.style;detached.remove();", "lazy style can be retained before element removal");
            var stale = await ex("staleStyle.color='#ffffff';");
            Check(stale.S == "js" && stale.N == 0, mode + ": removed-element style write fails without mutation");
            await Code("if(typeof first!=='undefined'||typeof staleStyle!=='undefined')throw Error('old realm');document.getElementById('target').style.color='#148677';document.title='Lazy styles';", "explicit recovery creates fresh wrappers and lazy styles");
            Check(node("target").Av("style").Contains("#148677"), mode + ": fresh realm remains bound to the current page");
        }

        internal static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 680), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); W.On(view); form.Show();
                await view.GoA(fixture);
                Check(view.Xo != null && view.Xo.Ok, "native lazy-style fixture loads");
                await Suite(view.Id, async code => { var result = await view.JsA(code); return new Jr(result.S, result.N, result.M); }, "native");
                var runtime = (W)view.Xs;
                Check(runtime.At.Ac && runtime.At.Ne && runtime.At.Nd && runtime.At.Fd && runtime.At.Mg == 15 && runtime.At.Cp == 2500 && runtime.At.Mm == 134217728, "native lazy styles preserve isolation and limits");
                foreach (var width in new[] { 940, 420 })
                {
                    form.ClientSize = new Size(width, 680); form.PerformLayout();
                    using (var bitmap = new Bitmap(view.Width, view.Height))
                    {
                        view.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                        var colors = new HashSet<int>();
                        for (var y = 0; y < bitmap.Height; y += 7) for (var x = 0; x < bitmap.Width; x += 7) colors.Add(bitmap.GetPixel(x, y).ToArgb());
                        Check(colors.Count > 8 && view.T == "Lazy styles", "native lazy styles remain nonblank at " + width);
                        if (output != null) bitmap.Save(output + "-" + width + ".png");
                    }
                }
                using (var process = System.Diagnostics.Process.GetProcessById(runtime.At.Pid))
                {
                    var handle = process.Handle; view.Dispose();
                    Check(process.WaitForExit(1000), "native disposal retires the pinned exact style worker");
                }
            }
        }

        private static void Check(bool condition, string label) { Program.Check(condition, "lazy styles: " + label); }
    }
}
