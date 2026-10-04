using System;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class Microtasks
    {
        internal static void Run(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            using (var script = new Js(engine, page))
                Suite(engine, page, script.Ex, script.Ev, "in-process");
            page = engine.Go(fixture);
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(engine, page, script.Ex, script.Ev, "isolated");
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, Func<N, string, Jr> ev, string mode)
        {
            void Check(bool condition, string name) { Program.Check(condition, mode + " microtasks: " + name); }
            void Code(string code) { var result = ex(code); Check(result.Ok, "code: " + result.S + "; " + result.M); }
            var target = engine.Id(page, "counter");
            Code("var target=document.getElementById('counter');target.onclick=function(){Promise.resolve().then(function(){document.title='Event microtask';});};");
            var eventResult = ev(target, "click");
            Check(eventResult.Ok && eventResult.N == 1 && page.T == "Event microtask", "native event checkpoint includes Promise DOM writes");

            Code(@"var nativePromise=Promise,nativeThen=Promise.prototype.then,nativeResolve=Promise.resolve,nativeApply=Reflect.apply;
Promise=undefined;nativePromise.resolve=undefined;nativePromise.prototype.then=undefined;Reflect.apply=undefined;
queueMicrotask(function(){document.title='Native async scheduler';});
Promise=nativePromise;nativePromise.resolve=nativeResolve;nativePromise.prototype.then=nativeThen;Reflect.apply=nativeApply;");
            Check(page.T == "Native async scheduler", "first scheduler use is independent of replaced public Promise/Reflect helpers");

            Code(@"var trace=['sync'];var returned=queueMicrotask(function(){'use strict';
if(this!==undefined||arguments.length!==0)throw new Error('callback');trace.push('a');queueMicrotask(function(){trace.push('c');});});
Promise.resolve().then(function(){trace.push('b');});trace.push('end');
if(returned!==undefined||queueMicrotask.name!=='queueMicrotask'||queueMicrotask.length!==1||typeof __jobs!=='undefined')throw new Error('surface');");
            Code("document.title=trace.join(',');");
            Check(page.T == "sync,end,a,b,c", "deferred FIFO, nested jobs, Promise interleaving, callback identity and hidden checkpoint");

            Code("target.onclick=function(e){Promise.resolve().then(function(){if(e.eventPhase!==0||e.currentTarget!==null||e.composedPath().length!==0)throw new Error('cleanup');e.preventDefault();document.title='Late cancellation';});};");
            eventResult = ev(target, "click");
            Check(eventResult.Ok && !eventResult.D && page.T == "Late cancellation", "event cleanup precedes checkpoint and late cancellation cannot change dispatch result");
            Code("target.onclick=function(e){e.preventDefault();queueMicrotask(function(){document.title='Canceled checkpoint';});};");
            eventResult = ev(target, "click");
            Check(eventResult.Ok && eventResult.D && page.T == "Canceled checkpoint", "synchronous cancellation survives checkpoint");
            Code("target.onclick=async function(){await Promise.resolve();document.title='Awaited event';};");
            eventResult = ev(target, "click");
            Check(eventResult.Ok && eventResult.N == 1 && page.T == "Awaited event", "async event continuation completes in native turn");
            Code("Promise.resolve().then(function(){throw new Error('caught');}).catch(function(e){document.title=e.message;});");
            Check(page.T == "caught", "handled Promise rejection uses Jint semantics");
            Code("var invalid=[undefined,null,{},'code',0,{handleEvent:function(){}}],rejected=0;for(var value of invalid){try{queueMicrotask(value);}catch(e){if(!(e instanceof TypeError))throw e;rejected++;}}if(rejected!==6)throw new Error('validation');");
            Code("queueMicrotask(function(){Promise=undefined;});queueMicrotask(function(){document.title='Captured scheduler';});");
            Check(page.T == "Captured scheduler", "scheduler does not depend on a replaced public Promise binding");

            var failure = ex("var oldRealm=17;queueMicrotask(function(){document.title='Partial microtask';throw new Error('expected');});queueMicrotask(function(){document.getElementById('result').textContent='Following job';});");
            Check(failure.S == "js" && failure.N == 2 && page.T == "Partial microtask" && engine.Id(page, "result").C[0].X == "Following job",
                "uncaught callback reports failure after draining following jobs and retains completed writes");
            Code("if(typeof oldRealm!=='undefined')throw new Error('realm');document.title='Fresh microtasks';");
            Code("var target=document.getElementById('counter');target.onclick=function(){queueMicrotask(function(){document.title='Event partial';throw new Error('first callback');});queueMicrotask(function(){throw new Error('second callback');});queueMicrotask(function(){document.getElementById('result').textContent='Event following';});};");
            failure = ev(target, "click");
            Check(failure.S == "js" && failure.N == 2 && failure.M.Contains("first callback") && page.T == "Event partial" && engine.Id(page, "result").C[0].X == "Event following",
                "native uncaught callback failure drains later jobs and reports first error");
            Code("var discarded=0;var target=document.getElementById('counter');target.onclick=function(){queueMicrotask(function(){discarded++;document.title='Unexpected job';});throw new Error('synchronous event');};");
            Check(ev(target, "click").S == "js" && page.T == "Event partial", "synchronous event failure discards queued realm before checkpoint");
            Code("if(typeof discarded!=='undefined')throw new Error('queued realm survived');");
            failure = ex("queueMicrotask(function(){throw null;});");
            Check(failure.S == "js" && failure.M.Length <= 512, "non-Error thrown values are bounded failures");
            Code("var admitted=0;for(var i=0;i<128;i++)queueMicrotask(function(){admitted++;});");
            Code("if(admitted!==128)throw new Error('count');for(var i=0;i<128;i++)queueMicrotask(function(){admitted++;});");
            Code("if(admitted!==256)throw new Error('reset');");
            Check(ex("for(var i=0;i<129;i++)queueMicrotask(function(){});").S == "limit", "128-job admission limit and per-turn reset");
            Check(ex("function spin(){queueMicrotask(spin);}queueMicrotask(spin);").S == "limit", "recursive queueMicrotask cannot bypass admission budget");
            Code("function warm(){Promise.resolve().then(function(){});}warm();");
            Check(ex("function spin(){Promise.resolve().then(spin);}spin();").S == "limit", "recursive Promise jobs cannot bypass execution constraints");
            Code("var target=document.getElementById('counter');target.onclick=function(){for(var i=0;i<50;i++)Promise.resolve().then(function(){for(var j=0;j<1000;j++){var n=j;}});};");
            Check(ev(target, "click").S == "limit", "event checkpoint enforces aggregate job execution budget");
            Code("var settle;new Promise(function(resolve){settle=resolve;}).then(function(){document.title='Stale promise';});");
            engine.Je = false;
            engine.Je = true;
            Code("if(typeof settle!=='undefined')throw new Error('permission realm');document.title='Permission fresh';");
            Check(page.T == "Permission fresh", "permission generation discards pending Promise realm");
        }
    }
}
