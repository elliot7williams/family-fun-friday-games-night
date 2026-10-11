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
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class Clock
    {
        internal static void Run(string fixture, string mode)
        {
            if (mode == "core") { Core(fixture); return; }
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            if (mode == "trusted")
                using (var script = new Js(engine, page))
                    Suite(engine, page, script.Ex, script.Rf, () => script.Rd, () => script.Nw, () => script.To, () => "", mode);
            else if (mode == "isolated")
                using (var script = new Jp(engine, page))
                {
                    var origin = script.To; var now = script.Nw;
                    Check(script.Pid == 0 && now >= 0 && origin > 0, "clock aliases do not start an isolated worker");
                    Suite(engine, page, script.Ex, script.Rf, () => script.Rd, () => script.Nw, () => script.To, () => script.Sd, mode);
                    Check(script.Pid == 0, "clock permission revocation leaves no isolated worker");
                }
            else throw new ArgumentException("Unknown clock test mode.");
        }

        private static void Core(string fixture)
        {
            double now = 0;
            var queue = new Fq(() => now, 1791750000000.75);
            Check(queue.To == 1791750000000 && queue.Nw == 0, "clock origin is fixed and coarsened to whole milliseconds");
            foreach (var value in new[] { 0.25, 0.99, 1.25, 16.25, 123.999 })
            {
                now = value;
                Check(queue.Nw == Math.Floor(value), "clock sample never exposes a fractional millisecond: " + value);
            }
            queue.Rs(); now += 16; queue.Take();
            Check(queue.Time == queue.Nw, "frame timestamps and performance reads share one coarsened clock");
            queue.Reset(); now += 8;
            Check(queue.N == 0 && queue.Nw == Math.Floor(now) && queue.To == 1791750000000,
                "realm reset clears frames without resetting the clock or origin");
            try { new Fq(null); throw new Exception("Null clock accepted"); }
            catch (ArgumentNullException) { Check(true, "clock source is required"); }
            foreach (var value in new[] { Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, 9007199254740992d, -9007199254740992d })
            {
                try { new Fq(() => 0, value); throw new Exception("Bad origin accepted"); }
                catch (ArgumentOutOfRangeException) { Check(true, "clock origin rejects nonfinite or unbounded values"); }
            }
            Check(new Fq(() => 0, -1.25).To == -2, "pre-epoch clock origins remain signed and coarsened");
            var worker = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe"));
            using (var vm = new Jint.Engine())
                foreach (var assembly in new[] { typeof(Jp).Assembly, worker })
                {
                    var codec = assembly.GetType("Lea.Script.Codec", true).GetMethod("Value", BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (var value in new object[] { 0d, 16.25d, -16.25d, 1791750000000L, -1791750000000L, 17.5m, 9007199254740991d })
                    {
                        Jint.Native.JsValue actual = null;
                        try { actual = (Jint.Native.JsValue)codec.Invoke(null, new[] { (object)vm, value }); }
                        catch (TargetInvocationException error) when (error.InnerException is InvalidDataException) { }
                        Check(actual != null && actual.IsNumber() && actual.AsNumber() == Convert.ToDouble(value),
                            "bounded bridge number accepted in " + assembly.GetName().Name + ": " + value);
                    }
                    foreach (var value in new object[] { Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity,
                        9007199254740992d, -9007199254740992d, Int64.MaxValue, Decimal.MaxValue, 1.25f })
                    {
                        var rejected = false;
                        try { codec.Invoke(null, new[] { (object)vm, value }); }
                        catch (TargetInvocationException error) when (error.InnerException is InvalidDataException) { rejected = true; }
                        Check(rejected, "invalid bridge number rejected in " + assembly.GetName().Name);
                    }
                }
            var engine = new E { Jb = false }; var page = engine.Go(fixture);
            var bridge = new Ja(engine, page); bridge.Begin();
            Check(bridge.Call(new object[] { "nw" }) is double && bridge.Call(new object[] { "to" }) is double &&
                bridge.Calls == 2 && bridge.Changes == 0 && bridge.Frames.N == 0 && bridge.Timers.N == 0,
                "two-letter clock bridge is read-only and consumes the normal call budget");
            foreach (var op in new[] { "nw", "to" })
            {
                try { bridge.Call(new object[] { op, 1 }); throw new Exception("Extra clock argument accepted"); }
                catch (InvalidDataException) { Check(true, "clock bridge rejects extra arguments: " + op); }
            }
            bridge.Begin();
            for (var i = 0; i < 256; i++) bridge.Call(new object[] { "nw" });
            try { bridge.Call(new object[] { "nw" }); throw new Exception("Clock quota bypassed"); }
            catch (Ja.LimitException) { Check(true, "clock bridge preserves the 256-call ceiling"); }
            var creations = 0;
            var trusted = new Js(engine, page, options => { creations++; return new Jint.Engine(options); });
            var isolated = new Jp(engine, page);
            foreach (var type in new[] { typeof(Js), typeof(Jp) })
                foreach (var name in new[] { "Nw", "To" })
                {
                    var property = type.GetProperty(name);
                    Check(property != null && property.PropertyType == typeof(double) && property.GetSetMethod() == null,
                        "read-only short clock alias " + type.Name + "." + name);
                }
            try
            {
                var origin = trusted.To; var stamp = trusted.Nw;
                Thread.Sleep(5);
                Check(trusted.Nw >= stamp && trusted.To == origin && isolated.Nw >= 0 && isolated.To > 0 &&
                    creations == 0 && isolated.Pid == 0, "host clock aliases advance without interpreter or worker creation");
                Exception cross = null;
                var thread = new Thread(() => { try { var ignored = isolated.Nw; } catch (Exception error) { cross = error; } });
                thread.Start(); thread.Join();
                Check(cross is InvalidOperationException, "host clock preserves isolated owner-thread affinity");
            }
            finally { trusted.Dispose(); isolated.Dispose(); }
            try { var ignored = trusted.To; throw new Exception("Disposed trusted clock read"); }
            catch (ObjectDisposedException) { Check(true, "disposed trusted clock alias rejected"); }
            try { var ignored = isolated.Nw; throw new Exception("Disposed isolated clock read"); }
            catch (ObjectDisposedException) { Check(true, "disposed isolated clock alias rejected"); }
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, Func<Jr> frame, Func<int> delay,
            Func<double> now, Func<double> origin, Func<string> diagnostics, string mode)
        {
            void Assert(bool value, string name) { Check(value, mode + " clock: " + name); }
            void Code(string code) { var result = ex(code); Assert(result.Ok, "code: " + result.S + "; " + result.M + "; " + diagnostics()); }
            var firstOrigin = origin();
            Code(@"var clock=performance,descriptor=Object.getOwnPropertyDescriptor(window,'performance');
if(clock!==window.performance||!(clock instanceof Performance)||Object.prototype.toString.call(clock)!=='[object Performance]'||clock.now.length!==0||clock.toJSON.length!==0)throw Error('interface');
var origin=clock.timeOrigin,last=clock.now();if(!Number.isFinite(origin)||!Number.isInteger(origin)||!Number.isInteger(last)||last<0||typeof origin!=='number')throw Error('values');
var json=clock.toJSON();if(Object.keys(json).join(',')!=='timeOrigin'||json.timeOrigin!==origin||JSON.stringify(clock)!==JSON.stringify(json)||json===clock.toJSON())throw Error('json');
if(clock.now('ignored',Symbol())<last)throw Error('extra arguments');");
            Assert(origin() == firstOrigin && now() >= 0, "native and page origin share the same parent clock");
            Code(@"var rejected=0;var getOrigin=Object.getOwnPropertyDescriptor(Performance.prototype,'timeOrigin').get;
for(var test of [function(){Performance();},function(){new Performance();},function(){clock.now.call(null);},function(){clock.now.call({});},function(){clock.now.call(window);},function(){clock.toJSON.call({});},function(){getOrigin.call({});},function(){clock.now.call(Performance.prototype);},function(){descriptor.get.call({});}]){try{test();}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}if(rejected!==9)throw Error('brand');
var readonly=false;try{(function(){'use strict';clock.timeOrigin=0;})();}catch(e){readonly=e instanceof TypeError;}if(!readonly||clock.timeOrigin!==origin)throw Error('readonly');");
            Code(@"var oldDate=Date.now;Date.now=function(){return -1e18;};var current=clock.now();Date.now=oldDate;if(current<last||clock.timeOrigin!==origin)throw Error('date dependency');last=current;");
            Thread.Sleep(5);
            Code("if(clock.now()<last||clock.timeOrigin!==origin)throw Error('continuity');last=clock.now();");
            Code(@"var oldApply=Reflect.apply,oldDefine=Object.defineProperty,oldType=TypeError;
Reflect.apply=function(){throw Error('apply');};Object.defineProperty=function(){throw Error('define');};TypeError=function(){throw Error('type');};
var caught=false;try{clock.now.call({});}catch(e){caught=e instanceof oldType;}if(!caught||clock.now()<last)throw Error('captured helpers');
performance=7;if(window.performance!==7||descriptor.get.call(window)!==clock||clock.timeOrigin!==origin)throw Error('replaceable');
oldDefine(window,'performance',descriptor);Reflect.apply=oldApply;Object.defineProperty=oldDefine;TypeError=oldType;");
            Code("requestAnimationFrame(function(t){if(!Number.isInteger(t)||t<0||t>performance.now()||performance.timeOrigin!==origin)throw Error('frame clock');document.title='Clock frame';});");
            Thread.Sleep(Math.Max(0, delay()));
            var batch = frame();
            Assert(batch.Ok && page.T == "Clock frame", "animation timestamps and page clock remain comparable: " + batch.S + "; " + batch.M);
            var failure = ex("var discardedClockRealm=1;throw Error('clock realm reset');");
            Assert(failure.S == "js", "cooperative error retires the realm");
            Assert(origin() == firstOrigin && now() >= 0, "realm retirement does not reset host clock metadata");
            Code("if(typeof discardedClockRealm!=='undefined'||typeof clock!=='undefined')throw Error('old realm');if(performance.timeOrigin!==" + firstOrigin.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + ")throw Error('new origin');document.title=String(performance.now());");
            Assert(Double.TryParse(page.T, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value) && value >= 0,
                "fresh realm recovers clock access without origin drift");
            engine.Je = false;
            Assert(ex("performance.now();").S == "off", "disabled scripting cannot read the clock through page code");
            Assert(origin() == firstOrigin && now() >= 0, "host metadata remains readable while scripting is disabled");
        }

        internal static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 680), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view); W.On(view); form.Show();
                await view.GoA(fixture);
                Check(view.Xo != null && view.Xo.Ok, "native clock fixture loads");
                var runtime = (W)view.Xs; var origin = runtime.At.To;
                var result = await view.JsA("var began=performance.now(),origin=performance.timeOrigin;requestAnimationFrame(function(t){var now=performance.now();if(!Number.isInteger(t)||!Number.isInteger(now)||t>now||performance.timeOrigin!==origin)throw Error('clock');document.getElementById('result').textContent=String(now-began)+' ms';document.title='Native clock';});");
                Check(result.Ok, "native clock callback scheduled: " + result.S + "; " + result.M);
                var wait = Stopwatch.StartNew();
                while ((view.T != "Native clock" || view.Xn != 0) && wait.ElapsedMilliseconds < 5000) await Task.Delay(10);
                Check(view.T == "Native clock" && view.Xn == 0 && runtime.At.Rn == 0, "native frame pump repaints a monotonic clock measurement");
                Check(runtime.At.Ac && runtime.At.Ne && runtime.At.Nd && runtime.At.Fd && runtime.At.Mg == 15 &&
                    runtime.At.Cp == 2500 && runtime.At.Mm == 134217728 && runtime.At.To == origin, "clock retains verified isolation and resource limits");
                var text = view.Id("result").C[0].X;
                Check(text.EndsWith(" ms", StringComparison.Ordinal) && Int32.TryParse(text.Substring(0, text.Length - 3), out var elapsed) && elapsed >= 0,
                    "native clock measurement is a nonnegative integer duration");
                foreach (var width in new[] { 940, 420 })
                {
                    form.ClientSize = new Size(width, 680); form.PerformLayout();
                    using (var bitmap = new Bitmap(view.Width, view.Height))
                    {
                        view.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                        var colors = new HashSet<int>();
                        for (var y = 0; y < bitmap.Height; y += 7) for (var x = 0; x < bitmap.Width; x += 7) colors.Add(bitmap.GetPixel(x, y).ToArgb());
                        Check(colors.Count > 8 && view.Id("result").C[0].X == text, "native clock repaint remains nonblank at " + width);
                        if (output != null) bitmap.Save(output + "-" + width + ".png");
                    }
                }
                view.Visible = false; var before = runtime.At.Nw; await Task.Delay(40);
                Check(runtime.At.Nw >= before && runtime.At.To == origin, "hidden view does not reset or pause its clock");
                var pid = runtime.At.Pid; view.Dispose(); await Task.Delay(100);
                try { var ignored = runtime.At.Nw; throw new Exception("Disposed native clock accepted"); }
                catch (ObjectDisposedException) { Check(true, "native disposal rejects stale host clock reads"); }
                Check(Exited(pid), "native disposal retires the exact clock worker");
            }
        }

        private static bool Exited(int pid)
        { try { using (var process = Process.GetProcessById(pid)) return process.WaitForExit(1000); } catch (ArgumentException) { return true; } }
        private static void Check(bool condition, string name) { Program.Check(condition, name); }
    }
}
