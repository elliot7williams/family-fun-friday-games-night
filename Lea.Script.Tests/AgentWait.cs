using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jint;
using Jint.Runtime;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class AgentWait
    {
        internal static void Control()
        {
            var options = Vo.Of(1000);
            using (var vm = new Jint.Engine(options))
            {
                Check(vm.Evaluate("typeof SharedArrayBuffer + ':' + typeof Atomics.wait").AsString() == "function:function",
                    "atomic wait control uses exposed shared buffers and wait function");
                vm.Execute("var buffer=new Int32Array(new SharedArrayBuffer(4));Atomics.store(buffer,0,0);");
                Check(vm.Evaluate("Atomics.load(buffer,0)").AsNumber() == 0, "atomic wait control buffer is initialized");
                var clock = Stopwatch.StartNew();
                var value = ""; var rejected = false;
                try { value = vm.Evaluate("Atomics.wait(buffer,0,0,300)").AsString(); }
                catch (JavaScriptException error) { rejected = error.Error.AsObject().Get("name").AsString() == "TypeError"; }
                Console.WriteLine("Atomic wait control: canSuspend=" + options.AgentCanSuspend + "; elapsed=" + clock.ElapsedMilliseconds + "; value=" + value + "; rejected=" + rejected);
                Check(options.AgentCanSuspend ? !rejected && value == "timed-out" && clock.ElapsedMilliseconds >= 250 : rejected,
                    "finite control distinguishes blocking default from nonblocking window configuration");
            }
        }

        private static void Bound()
        {
            Check(!Vo.Of(1000).AgentCanSuspend, "trusted window configuration forbids suspension before wait tests");
            var worker = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe"));
            var factory = worker.GetType("Lea.Script.Vo", true).GetMethod("Of", BindingFlags.Static | BindingFlags.NonPublic);
            Check(!((Options)factory.Invoke(null, new object[] { 5000 })).AgentCanSuspend,
                "compiled isolated window configuration forbids suspension before wait tests");
        }

        internal static void Run(string fixture, string mode)
        {
            if (mode == "control") { Control(); return; }
            Bound();
            if (mode == "core")
            {
                foreach (var milliseconds in new[] { 1000, 5000 })
                {
                    var options = Vo.Of(milliseconds);
                    Check(!options.AgentCanSuspend && options.Constraints.StackOverflowGuard && !options.Interop.Enabled && !options.Interop.AllowGetType && !options.Interop.AllowWrite,
                        "nonblocking option preserves stack and CLR guards: " + milliseconds);
                    options.AgentCanSuspend = true; Vo.At(options, milliseconds);
                    Check(!options.AgentCanSuspend, "callback configuration restores nonblocking semantics: " + milliseconds);
                }
                Control();
                return;
            }
            var engine = new E { Jb = false }; var page = engine.Go(fixture);
            if (mode == "trusted")
                using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, script.Tk, () => script.Td, script.Rf, () => script.Rd, mode);
            else if (mode == "isolated")
                using (var script = new Jp(engine, page))
                {
                    Suite(engine, page, script.Ex, script.Tk, () => script.Td, script.Rf, () => script.Rd, mode);
                    Check(script.Ac && script.Mg == 15 && script.Cp == 2500 && script.Mm == 134217728,
                        "nonblocking wait retains native isolation and resource limits");
                }
            else throw new ArgumentException("Unknown agent wait test mode.");
        }

        private const string Reject = @"function rejectWait(action){var rejected=false;try{action();}catch(e){rejected=e instanceof TypeError;}if(!rejected)throw Error('blocking wait accepted');}";
        private const string Memory = "var memory=new Int32Array(new SharedArrayBuffer(16)),wide=new BigInt64Array(new SharedArrayBuffer(16));";

        private static void Suite(E engine, P page, Func<string, Jr> ex, Func<Jr> timer, Func<int> timerDelay,
            Func<Jr> frame, Func<int> frameDelay, string mode)
        {
            void Assert(bool value, string name) { Check(value, mode + " agent: " + name); }
            void Code(string code) { var result = ex(code); Assert(result.Ok, "code: " + result.S + "; " + result.M); }
            Code(Reject + Memory + "Atomics.store(memory,0,7);if(Atomics.add(memory,0,2)!==7||Atomics.compareExchange(memory,0,9,11)!==9||Atomics.load(memory,0)!==11||Atomics.notify(memory,0)!==0)throw Error('atomic operations');");
            Code("for(var duration of [0,-1,300,Infinity,NaN,undefined])rejectWait(function(){Atomics.wait(memory,0,11,duration);});rejectWait(function(){Atomics.wait(memory,0,99,0);});rejectWait(function(){Atomics.wait(wide,0,0n,Infinity);});");
            Code("rejectWait(function(){Atomics.wait(new Int32Array(4),0,0,0);});rejectWait(function(){Atomics.wait(new Float32Array(new SharedArrayBuffer(16)),0,0,0);});var range=false;try{Atomics.wait(memory,99,11,0);}catch(e){range=e instanceof RangeError;}if(!range)throw Error('argument validation');");
            Code("var order='';queueMicrotask(function(){rejectWait(function(){Atomics.wait(memory,0,11,Infinity);});order+='m';});Promise.resolve().then(function(){rejectWait(function(){Atomics.wait(memory,0,11,NaN);});order+='p';});");
            Code("if(order!=='mp')throw Error('microtask progress');var immediate=Atomics.waitAsync(memory,0,99,Infinity),zero=Atomics.waitAsync(memory,0,11,0);if(immediate.async!==false||immediate.value!=='not-equal'||zero.async!==false||zero.value!=='timed-out')throw Error('waitAsync');var wake=Atomics.waitAsync(memory,0,11,Infinity);if(!wake.async||!(wake.value instanceof Promise)||Atomics.notify(memory,0,1)!==1)throw Error('notify');wake.value.then(function(value){if(value!=='ok')throw Error('wake');order+='a';});");
            Code("if(order!=='mpa')throw Error('async wake progress');setTimeout(function(){rejectWait(function(){Atomics.wait(memory,0,11,Infinity);});document.title='Wait timer';},0);");
            Thread.Sleep(Math.Max(0, timerDelay()));
            var task = timer(); Assert(task.Ok && page.T == "Wait timer", "timer callback continues after rejecting a blocking wait: " + task.S + "; " + task.M);
            Code("requestAnimationFrame(function(){rejectWait(function(){Atomics.wait(memory,0,11,Infinity);});document.title='Wait frame';});");
            Thread.Sleep(Math.Max(0, frameDelay()));
            task = frame(); Assert(task.Ok && page.T == "Wait frame", "animation continues after rejecting a blocking wait: " + task.S + "; " + task.M);
            var failure = ex("var discardedAgent=1;Atomics.wait(memory,0,11,Infinity);");
            Assert(failure.S == "js", "uncaught forbidden wait is a JavaScript error, not a hard deadline");
            Code("if(typeof discardedAgent!=='undefined')throw Error('retired realm');var fresh=new Int32Array(new SharedArrayBuffer(4));var rejected=false;try{Atomics.wait(fresh,0,0,Infinity);}catch(e){rejected=e instanceof TypeError;}if(!rejected)throw Error('fresh realm guard');document.title='Wait recovered';");
            Assert(page.T == "Wait recovered", "fresh realm remains nonblocking after recovery");
            engine.Je = false; Assert(ex("Atomics.wait(new Int32Array(new SharedArrayBuffer(4)),0,0,Infinity);").S == "off", "permission revocation still prevents execution");
        }

        internal static async Task Native(string fixture, string output)
        {
            Bound();
            using (var form = new Form { ClientSize = new Size(940, 680), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); W.On(view); form.Show(); await view.GoA(fixture);
                Check(view.Xo != null && view.Xo.Ok, "native nonblocking fixture loads");
                var runtime = (W)view.Xs;
                var result = await view.JsA(Reject + Memory + "setTimeout(function(){rejectWait(function(){Atomics.wait(memory,0,0,Infinity);});document.getElementById('result').textContent='Ready';requestAnimationFrame(function(){rejectWait(function(){Atomics.wait(memory,0,0,Infinity);});document.title='Lea responsive';});},0);");
                Check(result.Ok, "native timer and frame wait rejection scheduled: " + result.S + "; " + result.M);
                var clock = Stopwatch.StartNew();
                while ((view.T != "Lea responsive" || view.Xn != 0) && clock.ElapsedMilliseconds < 5000) await Task.Delay(10);
                Check(view.T == "Lea responsive" && view.Id("result").C[0].X == "Ready" && runtime.At.Tn == 0 && runtime.At.Rn == 0 && view.Xn == 0,
                    "native timer/frame pump remains responsive after forbidden waits");
                Check(runtime.At.Ac && runtime.At.Ne && runtime.At.Nd && runtime.At.Fd && runtime.At.Mg == 15 && runtime.At.Cp == 2500 && runtime.At.Mm == 134217728,
                    "nonblocking native page keeps verified isolation and resource limits");
                foreach (var width in new[] { 940, 420 })
                {
                    form.ClientSize = new Size(width, 680); form.PerformLayout();
                    using (var bitmap = new Bitmap(view.Width, view.Height))
                    {
                        view.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                        var colors = new HashSet<int>();
                        for (var y = 0; y < bitmap.Height; y += 7) for (var x = 0; x < bitmap.Width; x += 7) colors.Add(bitmap.GetPixel(x, y).ToArgb());
                        Check(colors.Count > 8 && view.Id("result").C[0].X == "Ready", "nonblocking page remains nonblank at " + width);
                        if (output != null) bitmap.Save(output + "-" + width + ".png");
                    }
                }
                var pid = runtime.At.Pid; using (var worker = Process.GetProcessById(pid))
                {
                    var handle = worker.Handle; view.Dispose(); await Task.Delay(100);
                    Check(worker.WaitForExit(1000), "native nonblocking session disposal retires the pinned worker");
                }
            }
        }

        private static void Check(bool condition, string name) { Program.Check(condition, name); }
    }
}
