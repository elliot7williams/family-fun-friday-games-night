using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Interop;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class LazyEvents
    {
        internal static void Run(string fixture, string mode)
        {
            if (mode == "core") { Core(fixture); return; }
            if (mode != "trusted" && mode != "isolated") throw new ArgumentException("Use core, trusted, isolated or native lazy-event mode.");
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            if (mode == "trusted")
            {
                using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, script.Ev, mode);
            }
            else
            {
                using (var script = new Jp(engine, page))
                {
                    Suite(engine, page, script.Ex, script.Ev, mode);
                    Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15 && script.Cp == 2500 && script.Mm == 134217728,
                        "isolated lazy events retain verified policy and resource limits");
                    using (var process = Process.GetProcessById(script.Pid))
                    {
                        var handle = process.Handle; script.Dispose();
                        Check(process.WaitForExit(2000), "isolated disposal retires the pinned exact worker");
                    }
                }
            }
        }

        private static void Core(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            var states = new List<ObjectInstance>();
            var configured = false;
            int Count(string field) => states.Count(state => state.Get(field).IsObject());
            // Allocation observation is a five-second CLR diagnostic, not a trusted/native startup gate.
            using (var vm = new Jint.Engine(Vo.Of(5000)))
            {
                configured = !Vo.Of(5000).AgentCanSuspend;
                var prototype = vm.GetValue("WeakMap").AsObject().Get("prototype").AsObject();
                var original = prototype.Get("set");
                prototype.Set("set", (JsValue)new ClrFunction(vm, "set", (receiver, args) =>
                {
                    if (args.Length == 2 && args[0].IsObject() && args[1].IsObject())
                    {
                        var state = args[1].AsObject();
                        var node = state.Get("node");
                        if (node.IsObject() && ReferenceEquals(node.AsObject(), args[0].AsObject()) &&
                            (state.Get("listeners").IsNull() || state.Get("listeners").IsObject())) states.Add(state);
                    }
                    return vm.Invoke(original, receiver, args);
                }), true);
                var bridge = new Ja(engine, page);
                var markers = new Bm();
                vm.SetValue("__lea", (JsValue)new ClrFunction(vm, "__lea", (_, args) => Codec.Value(vm, bridge.Call(Codec.Arguments(args)))));
                vm.SetValue("__jobs", (JsValue)new ClrFunction(vm, "__jobs", (_, args) =>
                {
                    if (markers.Of(args) == null) vm.Advanced.ProcessTasks();
                    return JsValue.Undefined;
                }));
                string source;
                using (var stream = typeof(Js).Assembly.GetManifestResourceStream("Lea.Script.Dom.js"))
                using (var reader = new StreamReader(stream)) source = reader.ReadToEnd();
                var dispatch = vm.Evaluate(source);
                markers.End();
                Console.WriteLine("Lazy event allocation diagnostic: interpreterBudgetMs=5000; nativeStartupGate=false; trustedStartupGate=false; productionBudgetsUnchanged=true");
                void Code(string code, string label)
                {
                    bridge.Begin(); vm.Execute(code); vm.Invoke(dispatch, JsValue.Undefined, new[] { (JsValue)"mt", Codec.Value(vm, new object[0]) });
                    Check(true, "core " + label);
                }
                bool Fire(string id)
                { bridge.Begin(); return vm.Invoke(dispatch, JsValue.Undefined, new[] { (JsValue)"click", Codec.Value(vm, bridge.Path(engine.Id(page, id))) }).AsBoolean(); }
                void Empty(string label)
                { Check(new[] { "listeners", "properties", "inline", "handlers" }.All(field => Count(field) == 0), "core " + label); }
                Code("1;", "starts the ordinary bootstrap");
                Check(configured && states.Count == 2, "core observes document/window state with explicit nonblocking diagnostic options");
                Empty("bootstrap creates no listener arrays or handler maps");
                Code("var rows=document.querySelectorAll('.sample');if(rows.length!==16)throw Error('count');for(var row of rows){if(!row.id)throw Error('id');}", "wraps sixteen ordinary elements");
                Check(states.Count == 18, "core records exactly sixteen element and two realm states");
                Empty("eighteen untouched targets allocate zero event containers");
                Code("for(var row of rows){if(row.onclick!==null)throw Error('handler');row.removeEventListener('click',null);row.addEventListener('click',null);}", "empty getters removals and null callbacks stay valid");
                Check(!Fire("target"), "core listener-free dispatch stays uncanceled");
                Empty("empty dispatch and no-op APIs keep storage dormant");
                Code("var target=rows[0];target.addEventListener('click',function(){document.title='Once';},{once:true});", "registers the first ordinary listener");
                Check(Count("listeners") == 1 && Count("properties") + Count("inline") + Count("handlers") == 0,
                    "core first listener needs one array and no handler maps");
                var stateTarget = states.Single(state => state.Get("key").IsNumber() && state.Get("listeners").IsObject());
                var list = stateTarget.Get("listeners");
                Check(!Fire("target") && page.T == "Once", "core once listener executes");
                Check(!Fire("target") && ReferenceEquals(list, stateTarget.Get("listeners")) &&
                    Count("listeners") == 1 && Count("handlers") == 0, "core retired listeners retain their array without creating handler maps");
                Code("target.onclick=null;", "explicit null masks inline handlers");
                Check(Count("properties") == 1 && Count("handlers") == 0 && Count("inline") == 0, "core null assignment needs only its property map");
                Code("target.onclick=function(){document.title='Handler';return false;};", "activates a DOM0 handler");
                Check(Fire("target") && page.T == "Handler" && Count("handlers") == 1 && Count("properties") == 1 && Count("inline") == 0,
                    "core DOM0 handler allocates its slot map and preserves cancellation");
                Code("target.onclick=null;target.onclick=function(){};var inlineNode=rows[1];inlineNode.setAttribute('onclick',\"document.title='Inline';\");if(typeof inlineNode.onclick!=='function')throw Error('inline');", "reactivates a handler and first reads authored inline code");
                Check(Count("listeners") == 2 && Count("properties") == 1 && Count("inline") == 1 && Count("handlers") == 2 && ReferenceEquals(list, stateTarget.Get("listeners")),
                    "core storage remains per-target and existing containers are reused");
                Check(!Fire("other") && page.T == "Inline", "core lazy inline cache remains executable");
                Code("var create=Object.create,get=WeakMap.prototype.get,set=WeakMap.prototype.set;try{Object.create=WeakMap.prototype.get=WeakMap.prototype.set=function(){throw Error('public intrinsic');};rows[2].onclick=function(){};}finally{Object.create=create;WeakMap.prototype.get=get;WeakMap.prototype.set=set;}", "lazy maps and receiver state use captured intrinsics");
                Check(Count("listeners") == 3 && Count("properties") == 2 && Count("handlers") == 3 && Count("inline") == 1,
                    "core first use touches only the requested target containers");
                Code("for(var node of [target,inlineNode,document,window])for(var name of ['listeners','properties','inline','handlers'])if(Object.hasOwn(node,name))throw Error('private state');", "event containers remain private");
                Console.WriteLine("Lazy event storage: targets=" + states.Count + "; arrays=" + Count("listeners") + "; propertyMaps=" + Count("properties") + "; inlineMaps=" + Count("inline") + "; handlerMaps=" + Count("handlers"));
            }
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, Func<N, string, Jr> ev, string mode)
        {
            void Code(string code, string label) { var result = ex(code); Check(result.Ok, mode + " " + label + "; " + result.S + "; " + result.M); }
            Jr Fire(string id) { var result = ev(engine.Id(page, id), "click"); Check(result.Ok, mode + " event dispatch: " + id + "; " + result.S + "; " + result.M); return result; }
            Code("var nodes=document.querySelectorAll('.sample'),target=nodes[0],other=nodes[1],log=[];for(var node of nodes){if(node.onclick!==null)throw Error('empty');node.removeEventListener('click',null);node.addEventListener('click',null);}", "unused targets and no-op event calls");
            Check(!Fire("target").D, mode + " empty route remains uncanceled");
            Code("var cap=function(){log.push('C');},once=function(){log.push('O');},bubble=function(){log.push('B');};document.addEventListener('click',cap,true);target.addEventListener('click',once,{once:true});window.addEventListener('click',bubble);", "first listeners on document element and window");
            Fire("target"); Fire("target");
            Code("if(log.join('')!=='COBCB')throw Error('order');document.removeEventListener('click',cap,true);window.removeEventListener('click',bubble);", "capture bubble once ordering and exact removal");
            Code("other.setAttribute('onclick',\"document.title='Inline';return false;\");", "authored handler installed after wrapping");
            Check(Fire("other").D && page.T == "Inline", mode + " inline handler initializes lazily and cancels");
            Code("other.onclick=null;document.title='Masked';", "explicit null overrides the inline attribute");
            Check(!Fire("other").D && page.T == "Masked", mode + " masked inline handler cannot run");
            Code("other.onclick=function(){document.title='DOM0';};other.onclick=null;other.onclick=function(){document.title='Reused';};", "DOM0 slots deactivate and reactivate");
            Check(!Fire("other").D && page.T == "Reused", mode + " reactivated slot runs without stale inline code");
            Code("var p=function(e){e.preventDefault();};target.addEventListener('click',p,{passive:true});", "first passive listener after empty dispatch");
            Check(!Fire("target").D, mode + " passive listener cannot cancel");
            Code("target.removeEventListener('click',p);var descriptor=Object.getOwnPropertyDescriptor(target,'onclick'),bad=0;for(var operation of [()=>target.addEventListener('click'),()=>target.removeEventListener('click'),()=>descriptor.get.call({}),()=>descriptor.set.call({},null)]){try{operation();}catch(error){if(error instanceof TypeError)bad++;}}if(bad!==4||target.addEventListener!==document.addEventListener||document.addEventListener!==window.addEventListener||Object.hasOwn(target.addEventListener,'prototype'))throw Error('metadata');", "required arguments shared descriptors and receiver brands remain enforced");
            Code("var saved=Object.create;try{Object.create=function(){throw Error('public factory');};nodes[2].onclick=function(){document.title='Captured';};}finally{Object.create=saved;}", "deferred maps retain the private constructor");
            Check(Fire("cell-2").Ok && page.T == "Captured", mode + " deferred handler stays bound to its element");
            Code("for(var i=0;i<126;i++)window.addEventListener('click',function(){});", "listener ceiling includes active DOM0 slots");
            Check(ex("window.onclick=function(){};").S == "js", mode + " unchanged 128-slot ceiling retires the realm");
            Code("if(typeof nodes!=='undefined')throw Error('stale realm');document.title='Fresh events';", "replacement realm discards old listener state");
        }

        internal static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 700), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); W.On(view); form.Show();
                var page = await view.GoA(fixture);
                Check(view.Xo != null && view.Xo.Ok && view.T == "Lazy events", "native lifecycle is ready");
                await View.Activate(view, "add");
                Check(view.T == "Count 1" && view.Id("result").C[0].X == "Count 1", "native ordinary listener");
                await View.Activate(view, "handler");
                Check(view.T == "DOM0" && view.Xo.Ok, "native DOM0 handler");
                await View.Activate(view, "inline");
                Check(view.T == "Inline" && view.Id("result").C[0].X == "Inline", "native authored inline handler");
                var field = page.Fs.Single(item => item.El.Av("id") == "field");
                await View.Activate(view, "field"); var editor = view.Controls.OfType<TextBox>().Single(); editor.Text = "Elliot"; await view.Xt;
                Check(editor.Visible && field.V == "Elliot", "native retained field input");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 700); View.Paint(view, output == null ? null : output + "-narrow.png");
                var runtime = ((W)view.Xs).At;
                Check(editor.Visible && view.ClientRectangle.Contains(editor.Bounds) && runtime.Ac && runtime.Ne && runtime.Nd && runtime.Fd && runtime.Mm == 134217728 && runtime.Cp == 2500,
                    "native narrow field and isolation remain intact");
                using (var process = Process.GetProcessById(runtime.Pid))
                {
                    var handle = process.Handle; view.Dispose(); Check(process.WaitForExit(2000), "native exact worker retired");
                }
            }
        }

        private static void Check(bool condition, string label) { Program.Check(condition, "lazy events: " + label); }
    }
}
