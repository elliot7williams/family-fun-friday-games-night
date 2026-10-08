using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class RuntimeStart
    {
        internal static void Run(string fixture, bool split)
        {
            Console.WriteLine(split ? "Reflective Start diagnostic; phase boundary differs from normal Ex; no retries." :
                "Production Ex diagnostic; unmodified limits; no retries.");
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            var records = new List<string>();
            using (var process = Process.GetCurrentProcess())
            using (var script = new Js(engine, page))
            {
                var cpu = process.TotalProcessorTime;
                var clock = Stopwatch.StartNew();
                Action<string> mark = phase => records.Add(phase + "; wall=" + clock.ElapsedMilliseconds +
                    "; cpu=" + (long)(process.TotalProcessorTime - cpu).TotalMilliseconds + " ms");
                Jint.Engine vm = null;
                Jint.Runtime.Debugger.DebugHandler.BeforeEvaluateEventHandler parsed = null;
                var phase = "bootstrap";
                try
                {
                    if (split)
                    {
                        mark("bootstrap begin");
                        var start = typeof(Js).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
                        try { start.Invoke(script, null); }
                        catch (TargetInvocationException error)
                        {
                            mark("bootstrap failed");
                            Console.WriteLine("Bootstrap exception: " + error.InnerException);
                            throw;
                        }
                        mark("bootstrap ready");
                        vm = (Jint.Engine)typeof(Js).GetField("vm", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(script);
                        parsed = (sender, ast) => mark("page script parsed");
                        vm.Debugger.BeforeEvaluate += parsed;
                    }
                    phase = split ? "page script" : "production Ex";
                    mark(phase + " begin");
                    var result = script.Ex(Timers.FirstScript);
                    mark(phase + " returned");
                    Console.WriteLine("First timer script: status=" + result.S + "; changes=" + result.N + "; " + result.M);
                    Program.Check(result.Ok, phase + " completes the exact first timer script");
                    Program.Check(script.Tn == 2 && script.Td == 0 && page.T == "deferred",
                        "first timer script retains two deferred tasks and the expected title");
                }
                finally
                {
                    if (vm != null && parsed != null) vm.Debugger.BeforeEvaluate -= parsed;
                    foreach (var record in records) Console.WriteLine(record);
                    Console.WriteLine("Runtime start diagnostic ended in " + phase + "; samples=1; no retries; not a cold-cache benchmark.");
                }
            }
        }
    }
}
