using System;
using System.Threading;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    // One cold realm per invocation. No pre-evaluation, factory or bootstrap instrumentation.
    internal static class LazyMaps
    {
        private const string FirstScript = @"var lazyOriginalMap=Map,lazyOriginalGet=Map.prototype.get,lazyOriginalSet=Map.prototype.set,lazyOriginalDelete=Map.prototype.delete;
var lazyOriginalApply=Reflect.apply,lazyOriginalFunctionApply=Function.prototype.apply,lazyPoisonCalls=0;
var lazyPoison=function(){lazyPoisonCalls++;throw new Error('page Map/helper override invoked');};
var lazyRestore=function(){Map=lazyOriginalMap;lazyOriginalMap.prototype.get=lazyOriginalGet;lazyOriginalMap.prototype.set=lazyOriginalSet;lazyOriginalMap.prototype.delete=lazyOriginalDelete;Reflect.apply=lazyOriginalApply;Function.prototype.apply=lazyOriginalFunctionApply;};
try{
 Map=lazyPoison;lazyOriginalMap.prototype.get=lazyPoison;lazyOriginalMap.prototype.set=lazyPoison;lazyOriginalMap.prototype.delete=lazyPoison;Reflect.apply=lazyPoison;Function.prototype.apply=lazyPoison;
 // All cancellation/invalid-admission paths precede the first private-store use.
 clearTimeout(2147483647);clearInterval(2147483646);clearTimeout();clearInterval(null);
 cancelAnimationFrame(2147483647);cancelAnimationFrame(-1);cancelAnimationFrame(null);
 var lazyRejected=0;
 try{setTimeout();}catch(error){if(!(error instanceof TypeError))throw error;lazyRejected++;}
 try{requestAnimationFrame(null);}catch(error){if(!(error instanceof TypeError))throw error;lazyRejected++;}
 if(lazyRejected!==2||document.getElementById('lazy-map-missing')!==null)throw new Error('empty-store validation');
 var lazyNode=document.getElementById('target'),lazyOther=document.getElementById('other');
 if(!lazyNode||!lazyOther||lazyNode===lazyOther||lazyNode!==document.querySelector('#target')||lazyOther!==document.getElementById('other'))throw new Error('first wrapper identity');
 lazyNode.textContent='poisoned first use';
 var lazyCanceledTimer=setTimeout(function(){throw new Error('canceled timeout ran');},0);
 clearInterval(lazyCanceledTimer);
 var lazyCanceledInterval=setInterval(function(){throw new Error('canceled interval ran');},0);
 clearTimeout(lazyCanceledInterval);
 var lazyArgument={value:37},lazyTimerCalls=0,lazyFrameCalls=0,lazyTrace='';
 var lazyTimer=setTimeout(function(value){'use strict';
  if(this!==window||arguments.length!==1||value!==lazyArgument||lazyPoisonCalls!==0)throw new Error('timer callback identity/helpers');
  lazyTimerCalls++;lazyTrace='timer';lazyNode.textContent='timer callback';
  queueMicrotask(function(){lazyTrace+=',job';document.title=lazyTrace;});
 },0,lazyArgument);
 var lazyCanceledFrame=requestAnimationFrame(function(){throw new Error('canceled frame ran');});
 cancelAnimationFrame(lazyCanceledFrame);
 var lazyFrame=requestAnimationFrame(function(stamp){'use strict';
  if(this!==window||arguments.length!==1||!Number.isFinite(stamp)||stamp<0||stamp%1!==0||lazyPoisonCalls!==0)throw new Error('frame callback identity/helpers/time');
  lazyFrameCalls++;lazyTrace+=',frame';lazyNode.textContent='frame callback';document.title=lazyTrace;
 });
 if(!Number.isInteger(lazyTimer)||lazyTimer<=0||lazyTimer===lazyCanceledTimer||lazyTimer===lazyCanceledInterval||!Number.isInteger(lazyFrame)||lazyFrame<=lazyCanceledFrame)throw new Error('first scheduling handles');
 clearTimeout(lazyCanceledTimer);cancelAnimationFrame(lazyCanceledFrame);
 if(lazyPoisonCalls!==0)throw new Error('first-use poison invoked');
 document.title='poison installed';
}catch(error){lazyRestore();throw error;}";

        private const string VerifyScript = @"if(lazyTimerCalls!==1||lazyFrameCalls!==1||lazyTrace!=='timer,job,frame'||lazyPoisonCalls!==0)throw new Error('callback count/order');
if(Map!==lazyPoison||lazyOriginalMap.prototype.get!==lazyPoison||lazyOriginalMap.prototype.set!==lazyPoison||lazyOriginalMap.prototype.delete!==lazyPoison||Reflect.apply!==lazyPoison||Function.prototype.apply!==lazyPoison)throw new Error('poison restored early');
if(lazyNode!==document.getElementById('target')||lazyOther!==document.querySelector('#other'))throw new Error('callback wrapper identity');";

        private const string RestoreScript = @"lazyRestore();
if(Map!==lazyOriginalMap||lazyOriginalMap.prototype.get!==lazyOriginalGet||lazyOriginalMap.prototype.set!==lazyOriginalSet||lazyOriginalMap.prototype.delete!==lazyOriginalDelete||Reflect.apply!==lazyOriginalApply||Function.prototype.apply!==lazyOriginalFunctionApply||lazyPoisonCalls!==0)throw new Error('poison restoration');";

        internal static void Run(string fixture, string mode)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture); // Existing fixture contains no script elements.
            if (mode == "trusted")
            {
                // Standard Js path retains Vo.Of(1000); there is no Js.Tm override.
                using (var script = new Js(engine, page))
                    Suite(engine, page, script.Ex, script.Tk, script.Rf, () => script.Td, () => script.Tn,
                        () => script.Rd, () => script.Rn, () => "", mode);
            }
            else if (mode == "isolated")
            {
                using (var script = new Jp(engine, page))
                {
                    Program.Check(script.Tm == 2000 && script.Cp == 2500 && script.Mm == 134217728 && script.Pid == 0,
                        "isolated lazy maps: production execution/resource defaults; cold worker");
                    Suite(engine, page, script.Ex, script.Tk, script.Rf, () => script.Td, () => script.Tn,
                        () => script.Rd, () => script.Rn, () => script.Sd + "; " + script.Ed, mode);
                }
            }
            else throw new ArgumentException("Unknown lazy-map test mode.");
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, Func<Jr> tick, Func<Jr> frame,
            Func<int> timerDelay, Func<int> timerCount, Func<int> frameDelay, Func<int> frameCount,
            Func<string> diagnostics, string mode)
        {
            var live = false;
            Exception failure = null;
            void Assert(bool value, string name) { Program.Check(value, mode + " lazy maps: " + name); }
            Jr Operation(Func<Jr> action, string name)
            {
                Jr result;
                try { result = action(); }
                catch { live = false; throw; }
                live = result.Ok; // Production failures retire the poisoned realm; do not start a cleanup realm.
                Assert(result.Ok, name + "; " + result.S + "; " + result.M + (result.Ok ? "" : "; " + diagnostics()));
                return result;
            }
            try
            {
                Assert(timerCount() == 0 && timerDelay() == -1 && frameCount() == 0 && frameDelay() == -1,
                    "metadata is dormant before the first evaluation");
                Operation(() => ex(FirstScript), "cold first script poisons helpers before wrapper/timer/frame use");
                Assert(timerCount() == 1 && frameCount() == 1 && page.T == "poison installed" &&
                    engine.Id(page, "target").C[0].X == "poisoned first use",
                    "first scheduling defers exactly one timer and frame after canceled peers");
                Operation(tick, "actual first timer callback with poison retained");
                Assert(timerCount() == 0 && timerDelay() == -1 && frameCount() == 1 && page.T == "timer,job" &&
                    engine.Id(page, "target").C[0].X == "timer callback",
                    "timer arguments/this, microtask checkpoint and one-shot retirement");
                var delay = frameDelay();
                Assert(delay >= 0 && delay <= 16, "frame wait stays within the existing rendering opportunity");
                Thread.Sleep(delay); // Host wait only, after the first Ex startup/execution deadlines completed.
                Operation(frame, "actual first frame callback with poison retained");
                Assert(timerCount() == 0 && frameCount() == 0 && frameDelay() == -1 && page.T == "timer,job,frame" &&
                    engine.Id(page, "target").C[0].X == "frame callback",
                    "frame callback this/time, ordering and one-shot retirement");
                Operation(() => ex(VerifyScript), "wrapper identity and callback counts survive captured-helper poison");
                var idleTimer = Operation(tick, "idle timer dispatch after retirement");
                Assert(idleTimer.N == 0 && timerCount() == 0, "idle timer dispatch makes no model changes");
                var idleFrame = Operation(frame, "idle frame dispatch after retirement");
                Assert(idleFrame.N == 0 && frameCount() == 0, "idle frame dispatch makes no model changes");
                Operation(() => ex("clearTimeout(lazyTimer);clearInterval(2147483646);cancelAnimationFrame(lazyFrame);cancelAnimationFrame(2147483647);if(lazyPoisonCalls!==0)throw new Error('retired/missing cancellation');"),
                    "retired and missing IDs remain harmless under poison");
                Assert(timerCount() == 0 && frameCount() == 0, "all native scheduler metadata is empty");
            }
            catch (Exception error) { failure = error; throw; }
            finally
            {
                if (live)
                {
                    try
                    {
                        var restored = ex(RestoreScript);
                        if (!restored.Ok) throw new InvalidOperationException("FAIL " + mode + " lazy maps restoration; " + restored.S + "; " + restored.M + "; " + diagnostics());
                        if (failure == null) Assert(true, "original public constructor/prototype/helpers restored in finally");
                    }
                    catch (Exception cleanup)
                    {
                        if (failure != null) throw new AggregateException("Test and poison restoration both failed.", failure, cleanup);
                        throw;
                    }
                }
                // If an operation failed, Js/Jp already reset the realm; using.Dispose also retires it.
            }
        }
    }
}
