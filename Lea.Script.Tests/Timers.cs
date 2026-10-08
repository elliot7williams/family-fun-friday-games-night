using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class Timers
    {
        internal const string FirstScript = @"if(setTimeout.length!==1||setInterval.length!==1||clearTimeout.length!==0||clearInterval.length!==0)throw new Error('IDL arity');
var trace=['sync'],argument={value:37};
var one=setTimeout(function(value){'use strict';if(this!==window||value!==argument)throw new Error('callback identity');trace.push('one');queueMicrotask(function(){trace.push('job');document.title=trace.join(',');});},-1,argument);
var two=setTimeout(function(){trace.push('two');document.title=trace.join(',');});
Promise.resolve().then(function(){trace.push('micro');});document.title='deferred';
if(!Number.isInteger(one)||one<=0||two===one)throw new Error('handle');";

        internal static void Run(string fixture)
        {
            Scheduler();
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            using (var script = new Js(engine, page))
                Suite(engine, page, script.Ex, script.Tk, () => script.Td, () => script.Tn, () => "", "in-process");
            page = engine.Go(fixture);
            using (var script = new Jp(engine, page) { Tm = 5000 })
            {
                Check(script.Tk().Ok && script.Pid == 0, "idle timer dispatch leaves isolated runtime dormant");
                Suite(engine, page, script.Ex, script.Tk, () => script.Td, () => script.Tn, () => script.Sd, "isolated");
            }
            Native(fixture);
        }

        private static void Scheduler()
        {
            long clock = 0;
            var queue = new Tq(() => clock);
            Check(queue.Td == -1 && queue.N == 0 && queue.Take() == 0, "empty monotonic timer queue");
            var later = queue.Ts(30, false);
            var first = queue.Ts(0, false);
            Check(queue.Take() == first, "timer queue chooses earliest deadline");
            queue.End();
            clock = 29;
            Check(queue.Td == 1 && queue.Take() == 0, "timer queue cannot execute before its deadline");
            clock = 30;
            Check(queue.Take() == later && queue.N == 1, "firing timer retains admission until callback ends");
            queue.End();
            Check(queue.N == 0, "one-shot timer retires after callback");
            var repeat = queue.Ts(0, true);
            first = queue.Ts(0, false);
            Check(queue.Take() == repeat, "equal deadlines retain creation order");
            queue.End();
            Check(queue.Take() == first, "rearmed interval goes behind an already ready timeout");
            queue.End();
            for (var index = 0; index < 5; index++) { Check(queue.Take() == repeat, "unclamped interval nesting: " + index); queue.End(); }
            Check(queue.Td == 4 && queue.Take() == 0, "interval nesting greater than five clamps to four milliseconds");
            clock += 4;
            Check(queue.Take() == repeat, "clamped interval executes after due time");
            var nested = queue.Ts(0, false);
            Check(queue.Td == 4, "synchronous nested timeout inherits timer task level");
            queue.End();
            var microtask = queue.Ts(0, false);
            Check(queue.Td == 0 && queue.Take() == microtask, "microtask scheduling is not timer-task nesting");
            queue.End();
            queue.Tc(repeat);
            queue.Tc(nested);
            Check(queue.N == 0, "shared timer handles support cancellation");
            repeat = queue.Ts(0, true);
            queue.Take();
            queue.Tc(repeat);
            queue.End();
            Check(queue.N == 0, "interval can cancel itself without rearming");
            for (var index = 0; index < 64; index++) queue.Ts(Int32.MaxValue, false);
            Check(queue.N == 64 && queue.Td == Int32.MaxValue, "64 active timers and full signed-long delay bound");
            try { queue.Ts(0, false); throw new Exception("timer limit accepted"); }
            catch (Ja.LimitException) { Check(true, "timer admission ceiling enforced by parent"); }
            queue.Reset();
            Check(queue.Td == -1 && queue.N == 0, "realm reset clears all timer metadata");
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, Func<Jr> tick,
            Func<int> delay, Func<int> count, Func<string> diagnostics, string mode)
        {
            void Assert(bool condition, string name) { Check(condition, mode + " timers: " + name); }
            void Code(string code) { var result = ex(code); Assert(result.Ok, "code: " + result.S + "; " + result.M + "; " + diagnostics()); }
            Jr Tick() { var result = tick(); Assert(result.Ok, "task: " + result.S + "; " + result.M + "; " + diagnostics()); return result; }
            Code(FirstScript);
            Assert(count() == 2 && delay() == 0 && page.T == "deferred", "timers are deferred until a distinct native task");
            var task = Tick();
            Assert(task.N == 1 && page.T == "sync,micro,one,job" && count() == 1, "callback arguments/this and microtask checkpoint precede next timer");
            Tick();
            Assert(page.T == "sync,micro,one,job,two" && count() == 0 && delay() == -1, "timeout FIFO and one-shot retirement");
            Code("var a=setTimeout(function(){document.title='wrong';});var b=setInterval(function(){document.title='wrong';});clearInterval(a);clearTimeout(b);clearTimeout();clearInterval(null);");
            Assert(count() == 0 && Tick().N == 0, "clearTimeout and clearInterval share IDs and missing IDs are no-ops");
            Code("var repeats=0;var interval=setInterval(function(){repeats++;document.title=String(repeats);if(repeats===3)clearTimeout(interval);},0);");
            Tick(); Tick(); Tick();
            Assert(page.T == "3" && count() == 0, "interval persists in realm and cancels inside callback");
            Code("var wrong=setTimeout(function(){document.title='wrong';},1000);setTimeout(function(){clearInterval(wrong);document.title='canceled peer';});");
            Tick();
            Assert(page.T == "canceled peer" && count() == 0, "callback can cancel another pending timer");
            Code("setTimeout({toString:function(){setTimeout(\"document.title='coercion first';\");return \"var stringGlobal=42;document.title='string callback';\";}});");
            Tick(); Tick();
            Code("if(stringGlobal!==42)throw new Error('classic global scope');");
            Assert(page.T == "string callback" && count() == 0, "string handlers compile in global scope after argument coercion");
            Code("setTimeout({valueOf:function(){return 42;},toString:function(){return \"document.title='String hint';\";}});");
            Tick();
            Assert(page.T == "String hint", "handler conversion uses string hint instead of valueOf");
            Code("(async function(){document.title='Await start';await new Promise(function(resolve){setTimeout(resolve);});document.title='Await timer';})();");
            Tick();
            Assert(page.T == "Await timer" && count() == 0, "await can resume from a timer Promise in the same checkpoint");
            Code("setTimeout(function(){document.title='wrapped delay';},2147483648);");
            Tick();
            Assert(page.T == "wrapped delay", "delay uses signed 32-bit conversion before negative clamp");
            Code(@"var rejected=0;for(var test of [function(){setTimeout();},function(){setTimeout(Symbol('code'));},function(){setTimeout(function(){},1n);},function(){clearTimeout(1n);}]){
try{test();}catch(error){if(!(error instanceof TypeError))throw error;rejected++;}}if(rejected!==4)throw new Error('conversion');");
            Code(@"var oldApply=Reflect.apply,oldFunctionApply=Function.prototype.apply,oldGet=Map.prototype.get,oldSet=Map.prototype.set,oldDelete=Map.prototype.delete;
Reflect.apply=undefined;Function.prototype.apply=undefined;Map.prototype.get=undefined;Map.prototype.set=undefined;Map.prototype.delete=undefined;
setTimeout(function(){Reflect.apply=oldApply;Function.prototype.apply=oldFunctionApply;Map.prototype.get=oldGet;Map.prototype.set=oldSet;Map.prototype.delete=oldDelete;document.title='Captured timer helpers';});");
            Tick();
            Assert(page.T == "Captured timer helpers", "timer admission and dispatch use captured helpers");
            Code("var oldEval=eval;eval=undefined;setTimeout(\"eval=oldEval;document.title='Captured classic eval';\");");
            Tick();
            Assert(page.T == "Captured classic eval", "string compilation does not depend on replaced public eval");
            Code("var peer=setInterval(function(){document.title='Promise cancellation';Promise.resolve().then(function(){clearInterval(peer);});},0);");
            Tick();
            Assert(count() == 0 && page.T == "Promise cancellation", "interval rearming precedes checkpoint and Promise can cancel it");
            Code("var many=[];for(var i=0;i<64;i++)many.push(setTimeout(function(){},2147483647));for(var id of many)clearTimeout(id);");
            Assert(count() == 0, "all admitted timer handles can be cleared in one turn");
            var limited = ex("for(var i=0;i<65;i++)setTimeout(function(){},2147483647);");
            Assert(limited.S == "limit" && count() == 0, "parent timer admission limit retires realm and metadata");
            Code(@"var arguments64=[function(){if(arguments.length!==64||arguments[63]!==39)throw new Error('arguments');document.title='64 arguments';},0];
for(var i=0;i<64;i++)arguments64.push(i===63?39:undefined);setTimeout.apply(null,arguments64);");
            Tick();
            Assert(page.T == "64 arguments", "callback argument list stays in realm and retains 64 entries");
            Code("var tooMany=[function(){},0];for(var i=0;i<65;i++)tooMany.push(i);var rejected=false;try{setTimeout.apply(null,tooMany);}catch(error){rejected=error instanceof RangeError;}if(!rejected)throw new Error('argument limit');");
            Code("var oldTimerRealm=17;setTimeout(function(){document.title='Partial timer';queueMicrotask(function(){document.getElementById('result').textContent='Following timer job';});throw new Error('first timer failure');});setTimeout(function(){document.title='Discarded peer';});");
            var failed = tick();
            Assert(failed.S == "js" && failed.N == 2 && failed.M.Contains("first timer failure") && page.T == "Partial timer" &&
                engine.Id(page,"result").C[0].X == "Following timer job" && count() == 0,
                "callback failure drains microtasks, retains writes, and discards peer timers");
            Code("if(typeof oldTimerRealm!=='undefined')throw new Error('failed realm');setTimeout(function(){while(true){};});");
            failed = tick();
            Assert(failed.S == "limit" && count() == 0, "timer callbacks retain cooperative execution budget");
            Code("setTimeout('while(true){};');");
            failed = tick();
            Assert(failed.S == "limit" && count() == 0, "string timer code retains cooperative execution budget");
            Code("setTimeout(function(){document.title='Revoked timer';},0);");
            engine.Je = false;
            engine.Je = true;
            Assert(delay() == -1 && Tick().N == 0 && count() == 0 && page.T == "Partial timer", "permission generation discards timers before execution");
            Code("setTimeout(function(){document.title='Old page timer';},0);");
            engine.Go(page.U.AbsoluteUri);
            failed = tick();
            Assert(failed.S == "stale" && count() == 0 && page.T == "Partial timer", "navigation invalidates a bound timer realm");
        }

        private static void Native(string fixture)
        {
            var previous = SynchronizationContext.Current;
            Exception failure = null;
            using (var context = new ApplicationContext())
            using (var starter = new System.Windows.Forms.Timer { Interval = 5 })
            {
                starter.Tick += async (sender, args) =>
                {
                    starter.Stop();
                    try { await NativeCore(fixture); }
                    catch (Exception error) { failure = error; }
                    finally { context.ExitThread(); }
                };
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                starter.Start();
                try { Application.Run(context); }
                finally { SynchronizationContext.SetSynchronizationContext(previous); }
            }
            if (failure != null) throw failure;
        }

        private static async Task NativeCore(string fixture)
        {
            using (var form = new Form { ClientSize = new Size(940, 820), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                W.On(view, null, 5000);
                form.Show();
                await view.GoA(fixture);
                Check(view.Xo != null && view.Xo.Ok, "native timer fixture initialized");
                var changes = 0;
                view.Xd += (sender, args) => changes++;
                await Code(view, "setTimeout(function(){document.title='Native timer';document.getElementById('result').textContent='timer painted';},60);");
                await Wait(() => view.T == "Native timer" && view.Xn == 0, "native timeout dispatched automatically");
                Check(changes > 0 && view.Id("result").C[0].X == "timer painted" && ((W)view.Xs).At.Tn == 0, "timer completion repaints DOM and notifies host");
                foreach (var width in new[] { 940, 420 })
                {
                    form.ClientSize = new Size(width, 820);
                    form.PerformLayout();
                    var height = view.ClientSize.Height;
                    Check(view.ClientSize.Width == width && height > 0, "native timer viewport dimensions: " + width + "x" + height);
                    using (var bitmap = new Bitmap(width, height))
                    {
                        view.DrawToBitmap(bitmap, new Rectangle(0, 0, width, height));
                        var colors = new HashSet<int>();
                        for (var y = 0; y < height; y += 7) for (var x = 0; x < width; x += 7) colors.Add(bitmap.GetPixel(x, y).ToArgb());
                        Check(colors.Count > 8, "timer-painted native page is nonblank at " + width);
                        bitmap.Save(Path.Combine(Path.GetTempPath(), "Lea-timers-native-" + width + ".png"));
                    }
                }
                for (var index = 0; index <= view.Kn && (view.Ke == null || view.Ke.Av("id") != "field"); index++) view.Kf();
                await view.Ka();
                var editor = view.Controls.OfType<TextBox>().Single();
                Check(editor.Visible, "native editor active before timer writes");
                await Code(view, "setTimeout(function(){queueMicrotask(function(){document.getElementById('field').value='timer editor value';});},60);");
                await Wait(() => editor.Text == "timer editor value" && view.Xn == 0, "timer microtask synchronizes retained native editor");
                await Code(view, "var timerCount=0;var timerId=setInterval(function(){document.title='Interval '+(++timerCount);if(timerCount===3)clearInterval(timerId);},10);");
                await Wait(() => view.T == "Interval 3" && view.Xn == 0, "native interval completes and self-cancels");
                var runtime = (W)view.Xs;
                await Code(view, "setTimeout(function(){document.title='Stopped timer';},300);");
                Check(!view.Busy && runtime.At.Tn == 1, "waiting timer does not mark view busy");
                view.St();
                await Task.Delay(400);
                Check(view.T != "Stopped timer" && runtime.At.Tn == 0 && view.Xs == null, "Stop retires timer metadata and native pump");
                await Code(view, "setTimeout(function(){document.title='Old page timer';},300);");
                runtime = (W)view.Xs;
                await view.GoA("about:blank");
                await Task.Delay(400);
                Check(view.T != "Old page timer" && runtime.At.Tn == 0 && ((W)view.Xs).At.Pid == 0, "navigation clears timers and leaves script-free replacement dormant");
                await Code(view, "setTimeout(function(){document.title='Revoked native timer';},300);");
                runtime = (W)view.Xs;
                view.Je = false;
                await Task.Delay(400);
                Check(view.T != "Revoked native timer" && runtime.At.Tn == 0 && runtime.At.Pid == 0, "permission revocation retires pending native timers");
                view.Je = true;
                await Code(view, "document.title='Before timer deadline';setTimeout(function(){/(a+)+$/.test('a'.repeat(120)+'!');document.title='Late native timer';},200);");
                runtime = (W)view.Xs;
                var pid = runtime.At.Pid;
                runtime.At.Tm = 25;
                var ticks = 0;
                using (var timer = new System.Windows.Forms.Timer { Interval = 5 })
                {
                    timer.Tick += (sender, args) => ticks++;
                    timer.Start();
                    await Wait(() => view.Xo != null && view.Xo.S == "timeout" && view.Xn == 0, "native timer hard deadline returns bounded failure");
                    timer.Stop();
                }
                Check(ticks > 0 && runtime.At.Pid == 0 && Exited(pid) && runtime.At.Tn == 0 && view.T == "Before timer deadline",
                    "timer hard deadline keeps UI responsive, kills worker, and prevents late writes");
                runtime.At.Tm = 5000;
                await Code(view, "setTimeout(function(){document.title='Disposed native timer';},1000);");
                runtime = (W)view.Xs;
                pid = runtime.At.Pid;
                view.Dispose();
                await Task.Delay(200);
                Check(runtime.At.Tn == 0 && Exited(pid), "view disposal retires timer pump and isolated process");
            }
        }

        private static async Task Code(V view, string code)
        {
            var result = await view.JsA(code);
            Check(result.Ok, "native timer code: " + result.S + "; " + result.M + "; " + (view.Xs is W ? ((W)view.Xs).At.Sd : ""));
        }

        private static async Task Wait(Func<bool> condition, string name)
        {
            var clock = Stopwatch.StartNew();
            while (!condition() && clock.ElapsedMilliseconds < 5000) await Task.Delay(10);
            Check(condition(), name);
        }

        private static bool Exited(int pid)
        {
            try { using (var process = Process.GetProcessById(pid)) return process.WaitForExit(1000); }
            catch (ArgumentException) { return true; }
        }

        private static void Check(bool condition, string name) { Program.Check(condition, name); }
    }
}
