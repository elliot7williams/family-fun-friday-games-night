using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Web.Script.Serialization;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class BootstrapCostObserver
    {
        internal static Jr Run(Js script, string code)
        {
            var stageField = typeof(Js).GetField("startupStage", BindingFlags.Instance | BindingFlags.NonPublic);
            if (stageField == null) throw new InvalidOperationException("Expected startup stage field");
            int owner = Thread.CurrentThread.ManagedThreadId;
            int count = 0, otherThreadCount = 0, callbackErrors = 0;
            bool entered = false, overflow = false;
            var types = new Dictionary<Type, int>();
            var examples = new List<object>();
            var watch = Stopwatch.StartNew();
            EventHandler<FirstChanceExceptionEventArgs> handler = (sender, args) =>
            {
                if (Thread.CurrentThread.ManagedThreadId != owner)
                {
                    Interlocked.Increment(ref otherThreadCount);
                    return;
                }
                if (entered) return;
                entered = true;
                try
                {
                    count++;
                    if (count > 10000) { overflow = true; return; }
                    Type type = args.Exception.GetType();
                    int previous;
                    if (types.TryGetValue(type, out previous)) types[type] = previous + 1;
                    else if (types.Count < 16) types.Add(type, 1);
                    else overflow = true;
                    if (examples.Count < 24)
                    {
                        var site = args.Exception.TargetSite;
                        examples.Add(new { wallMs = watch.ElapsedMilliseconds, type = type.FullName,
                            stage = (string)stageField.GetValue(script),
                            site = site == null ? null : site.DeclaringType.FullName + "." + site.Name });
                    }
                }
                catch { callbackErrors++; }
                finally { entered = false; }
            };
            Jr result = null;
            using (var process = Process.GetCurrentProcess())
            {
                var before = NativeProcessSample.Read(process);
                AppDomain.CurrentDomain.FirstChanceException += handler;
                try { result = script.Ex(code); return result; }
                finally
                {
                    AppDomain.CurrentDomain.FirstChanceException -= handler;
                    var after = NativeProcessSample.Read(process);
                    var counts = new List<object>();
                    foreach (var pair in types) counts.Add(new { type = pair.Key.FullName, count = pair.Value });
                    var report = new { wallMs = watch.ElapsedMilliseconds, count, otherThreadCount, callbackErrors,
                        overflow, handlerRemoved = true, counts, examples, before, after,
                        executionOk = result != null && result.Ok, mutations = result == null ? -1 : result.N,
                        status = result == null ? null : result.S, message = result == null ? null : result.M,
                        budgetChanged = false, fullSuitePassed = false,
                        scope = "Owning-thread first-chance exceptions during first computed call only; test observer adds overhead and warming" };
                    Console.WriteLine("LEA_BOOTSTRAP_COST " + new JavaScriptSerializer().Serialize(report));
                    if (overflow || callbackErrors != 0) throw new InvalidOperationException("Exception observation incomplete");
                }
            }
        }
    }
}
