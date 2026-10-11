using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class Frames
    {
        internal static void Run(string fixture, string mode)
        {
            if (mode == "scheduler") { Scheduler(); Protocol(); return; }
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            if (mode == "trusted")
                using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, script.Rf, () => script.Rd, () => script.Rn, () => "", mode);
            else if (mode == "isolated")
                using (var script = new Jp(engine, page))
                {
                    Check(script.Rf().Ok && script.Pid == 0, "idle frame leaves native worker dormant");
                    Suite(engine, page, script.Ex, script.Rf, () => script.Rd, () => script.Rn, () => script.Sd, mode);
                }
            else throw new ArgumentException("Unknown frame test mode.");
        }

        private static void Scheduler()
        {
            double now = 0;
            var queue = new Fq(() => now);
            Check(queue.N == 0 && queue.Rd == -1 && queue.Take() == null, "empty frame queue is dormant");
            var first = queue.Rs(); var peer = queue.Rs();
            Check(first == 1 && peer == 2 && queue.N == 2 && queue.Rd == 16, "frame handles and one bounded rendering deadline");
            now = 15.5;
            Check(queue.Rd == 1 && queue.Take() == null, "frame delay rounds up and cannot execute early");
            now = 16.25;
            var keys = queue.Take();
            Check(keys.Length == 2 && (int)keys[0] == first && (int)keys[1] == peer && queue.Time == Math.Floor(now), "frame snapshot preserves registration order and one coarsened timestamp");
            try { queue.Take(); throw new Exception("Concurrent frame accepted"); }
            catch (InvalidOperationException) { Check(true, "frame batches cannot overlap"); }
            queue.Rc(first); queue.Rc(peer);
            var nested = queue.Rs();
            queue.End();
            Check(queue.N == 1 && queue.Rd == 16 && queue.Take() == null, "new callback survives batch retirement but waits for next opportunity");
            now = 32.25;
            keys = queue.Take();
            Check(keys.Length == 1 && (int)keys[0] == nested && queue.Time == Math.Floor(now), "subsequent frame uses increasing coarsened monotonic time");
            queue.End();
            Check(queue.N == 0 && queue.Rd == -1, "frame callbacks are one shot");
            for (var i = 0; i < 64; i++) queue.Rs();
            Check(queue.N == 64, "64 parent-owned frame admissions");
            try { queue.Rs(); throw new Exception("Frame ceiling accepted"); }
            catch (Ja.LimitException) { Check(true, "frame admission ceiling fails closed"); }
            queue.Reset();
            Check(queue.N == 0 && queue.Take() == null && queue.Rd == -1, "realm reset retires all frame metadata");
            Check(queue.Rs() > nested, "reset does not reuse retired frame handles");
        }

        private static void Protocol()
        {
            var worker = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe"));
            foreach (var assembly in new[] { typeof(Jp).Assembly, worker })
            {
                var method = assembly.GetType("Lea.Script.Wire", true).GetMethod("Time", BindingFlags.NonPublic | BindingFlags.Static);
                Check(method != null, "frame timestamp validator compiled into " + assembly.GetName().Name);
                foreach (var value in new object[] { 0, 16.25, 17L, 17.5m })
                    Check((double)method.Invoke(null, new object[] { new Dictionary<string, object> { { "t", value } } }) == Convert.ToDouble(value), "frame timestamp admits bounded numeric representation");
                foreach (var value in new object[] { null, "16", true, -1, Double.NaN, Double.PositiveInfinity, 9007199254740992d })
                {
                    var rejected = false;
                    try { method.Invoke(null, new object[] { new Dictionary<string, object> { { "t", value } } }); }
                    catch (TargetInvocationException error) when (error.InnerException is InvalidDataException) { rejected = true; }
                    Check(rejected, "frame timestamp rejects nonnumeric, negative and unbounded values");
                }
                var keys = assembly.GetType("Lea.Script.Wire", true).GetMethod("Rk", BindingFlags.NonPublic | BindingFlags.Static);
                Check(keys != null, "frame key validator compiled into " + assembly.GetName().Name);
                var accepted = new object[] { 1, 2, Int32.MaxValue };
                Check(Object.ReferenceEquals(accepted, keys.Invoke(null, new object[] { new Dictionary<string, object> { { "p", accepted } } })), "frame keys preserve strictly ordered bounded handles");
                foreach (var value in new object[] { null, "keys", new object[0], new object[65], new object[] { 0 }, new object[] { -1 },
                    new object[] { 1, 1 }, new object[] { 2, 1 }, new object[] { 1L }, new object[] { true } })
                {
                    var rejected = false;
                    try { keys.Invoke(null, new object[] { new Dictionary<string, object> { { "p", value } } }); }
                    catch (TargetInvocationException error) when (error.InnerException is InvalidDataException) { rejected = true; }
                    Check(rejected, "frame keys reject malformed, excessive, duplicate and unordered handles");
                }
            }
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, Func<Jr> frame,
            Func<int> delay, Func<int> count, Func<string> diagnostics, string mode)
        {
            void Assert(bool value, string name) { Check(value, mode + " frames: " + name); }
            void Code(string code) { var result = ex(code); Assert(result.Ok, "code: " + result.S + "; " + result.M + "; " + diagnostics()); }
            Jr Frame() { Thread.Sleep(Math.Max(0, delay())); var result = frame(); Assert(result.Ok, "batch: " + result.S + "; " + result.M + "; " + diagnostics()); return result; }
            Code(@"if(requestAnimationFrame.length!==1||cancelAnimationFrame.length!==1)throw new Error('arity');
var trace=[],stamp,next=0;var a=requestAnimationFrame(function(t){'use strict';if(this!==window||arguments.length!==1||!Number.isFinite(t)||t<0)throw new Error('callback');stamp=t;trace.push('one');queueMicrotask(function(){trace.push('job');});next=requestAnimationFrame(function(u){if(u<=stamp)throw new Error('time');trace.push('next');document.title=trace.join(',');});});
var b=requestAnimationFrame(function(t){if(t!==stamp)throw new Error('shared time');trace.push('two');document.title=trace.join(',');});
if(!Number.isInteger(a)||a<=0||b<=a)throw new Error('handles');document.title='Frames deferred';");
            Assert(count() == 2 && page.T == "Frames deferred", "callbacks remain deferred until host rendering opportunity");
            Frame();
            Assert(page.T == "one,job,two" && count() == 1, "FIFO, per-callback microtasks, shared timestamp and nested deferral");
            Frame();
            Assert(page.T == "one,job,two,next" && count() == 0 && delay() == -1, "nested callback runs once in a later batch");
            Code("var peer;requestAnimationFrame(function(){Promise.resolve().then(function(){cancelAnimationFrame(peer);document.title='Frame peer canceled';});});peer=requestAnimationFrame(function(){document.title='wrong';});");
            Frame();
            Assert(page.T == "Frame peer canceled" && count() == 0, "microtask cancels later callback in the same snapshot");
            Code(@"var rejected=0;for(var test of [function(){requestAnimationFrame();},function(){requestAnimationFrame(null);},function(){requestAnimationFrame('code');},function(){cancelAnimationFrame();},function(){cancelAnimationFrame(1n);},function(){cancelAnimationFrame(Symbol());},function(){requestAnimationFrame.call({},function(){});},function(){cancelAnimationFrame.call({},1);}]){try{test();}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}if(rejected!==8)throw new Error('conversion');cancelAnimationFrame(-1);cancelAnimationFrame(null);cancelAnimationFrame(undefined);");
            Code("var id=requestAnimationFrame(function(){document.title='wrong';});cancelAnimationFrame({valueOf:function(){return id+4294967296;}});");
            Assert(count() == 0 && frame().N == 0, "unsigned-long cancellation conversion and idle no-op");
            Code(@"var oldApply=Reflect.apply,oldGet=Map.prototype.get,oldSet=Map.prototype.set,oldDelete=Map.prototype.delete;
Reflect.apply=undefined;Map.prototype.get=undefined;Map.prototype.set=undefined;Map.prototype.delete=undefined;
requestAnimationFrame(function(){Reflect.apply=oldApply;Map.prototype.get=oldGet;Map.prototype.set=oldSet;Map.prototype.delete=oldDelete;document.title='Captured frame helpers';});");
            Frame();
            Assert(page.T == "Captured frame helpers", "admission and dispatch retain captured helpers");
            Code("var many=[];for(var i=0;i<64;i++)many.push(requestAnimationFrame(function(){}));for(var id of many)cancelAnimationFrame(id);");
            Assert(count() == 0, "all admitted callbacks can be canceled");
            var failure = ex("for(var i=0;i<65;i++)requestAnimationFrame(function(){});");
            Assert(failure.S == "limit" && count() == 0, "parent frame ceiling retires realm");
            Code("var oldFrameRealm=17;requestAnimationFrame(function(){document.title='Partial frame';throw new Error('frame failure');});requestAnimationFrame(function(){document.getElementById('result').textContent='peer completed';});");
            Thread.Sleep(Math.Max(0, delay())); failure = frame();
            Assert(failure.S == "js" && failure.N == 2 && failure.M.Contains("frame failure") && page.T == "Partial frame" &&
                engine.Id(page, "result").C[0].X == "peer completed" && count() == 0, "callback exception permits snapshot peers, retains writes, then retires realm");
            Code("if(typeof oldFrameRealm!=='undefined')throw new Error('realm');requestAnimationFrame(function(){while(true){};});");
            Thread.Sleep(Math.Max(0, delay())); failure = frame();
            Assert(failure.S == "limit" && count() == 0, "frame callback retains cooperative budget");
            Code("requestAnimationFrame(function(){document.title='Revoked frame';});");
            engine.Je = false; engine.Je = true;
            Assert(delay() == -1 && frame().N == 0 && count() == 0 && page.T == "Partial frame", "permission generation invalidates frames before execution");
            Code("requestAnimationFrame(function(){document.title='Stale frame';});");
            engine.Go(page.U.AbsoluteUri);
            failure = frame();
            Assert(failure.S == "stale" && count() == 0 && page.T == "Partial frame", "navigation invalidates frame realm");
        }

        internal static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 680), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); W.On(view); form.Show();
                await view.GoA(fixture);
                Check(view.Xo != null && view.Xo.Ok, "native frame fixture loads");
                async Task Code(string code) { var result = await view.JsA(code); Check(result.Ok, "native frame code: " + result.S + "; " + result.M); }
                await Code("var n=0;function animate(t){document.getElementById('result').textContent=String(++n);document.title='Frame '+n;if(n<3)requestAnimationFrame(animate);}requestAnimationFrame(animate);");
                await Wait(() => view.T == "Frame 3" && view.Xn == 0, "native view dispatches three distinct rendering opportunities");
                var runtime = (W)view.Xs;
                Check(runtime.At.Rn == 0 && runtime.At.Ac && runtime.At.Cp == 2500 && runtime.At.Mm == 134217728, "frame pump retains isolation and resource limits");
                foreach (var width in new[] { 940, 420 })
                {
                    form.ClientSize = new Size(width, 680); form.PerformLayout();
                    using (var bitmap = new Bitmap(view.Width, view.Height))
                    {
                        view.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                        var colors = new HashSet<int>();
                        for (var y = 0; y < bitmap.Height; y += 7) for (var x = 0; x < bitmap.Width; x += 7) colors.Add(bitmap.GetPixel(x, y).ToArgb());
                        Check(colors.Count > 8 && view.Id("result").C[0].X == "3", "frame repaint is nonblank and updated at " + width);
                        if (output != null) bitmap.Save(output + "-" + width + ".png");
                    }
                }
                await Code("requestAnimationFrame(function(){document.title='Visible frame';});setTimeout(function(){document.title='Hidden timer';},40);");
                view.Visible = false;
                await Task.Delay(150);
                Check(view.T == "Hidden timer" && runtime.At.Rn == 1 && view.Xn == 0, "hidden view pauses frames without pausing ordinary timers");
                view.Visible = true;
                await Wait(() => view.T == "Visible frame" && runtime.At.Rn == 0 && view.Xn == 0, "showing view resumes pending frame");
                await Code("requestAnimationFrame(function(){document.title='Before frame deadline';while(true){};});");
                var pid = runtime.At.Pid;
                await Wait(() => runtime.At.Pid == 0 && view.Xn == 0, "bounded frame failure retires exact worker");
                Check(view.T == "Before frame deadline" && runtime.At.Rn == 0 && view.Xo != null && !view.Xo.Ok && Exited(pid), "failed frame prevents late writes and clears metadata");
                await Code("requestAnimationFrame(function(){document.title='Disposed frame';});");
                runtime = (W)view.Xs; pid = runtime.At.Pid;
                view.Dispose();
                await Task.Delay(100);
                Check(runtime.At.Rn == 0 && Exited(pid), "native view disposal retires frames and worker");
            }
        }

        private static async Task Wait(Func<bool> predicate, string name)
        { var clock = Stopwatch.StartNew(); while (!predicate() && clock.ElapsedMilliseconds < 5000) await Task.Delay(10); Check(predicate(), name); }
        private static bool Exited(int pid)
        { try { using (var process = Process.GetProcessById(pid)) return process.WaitForExit(1000); } catch (ArgumentException) { return true; } }
        private static void Check(bool value, string name) { Program.Check(value, name); }
    }
}
