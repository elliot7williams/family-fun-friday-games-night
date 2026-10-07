using System;
using System.Threading;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class BootstrapScopes
    {
        internal static void Run(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, script.Tk, "in-process");
            page = engine.Go(fixture);
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(engine, page, script.Ex, script.Tk, "isolated", () => script.Sd);
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, Func<Jr> tick, string mode, Func<string> diagnostics = null)
        {
            Jr Code(string code, string name)
            {
                var result = ex(code);
                Program.Check(result.Ok, mode + " bootstrap scope: " + name + "; " + result.S + "; " + result.M +
                    (result.Ok || diagnostics == null ? "" : "; " + diagnostics()));
                return result;
            }
            Code("var names=['__lea','__jobs','call','checkpoint','failed','failure','microtaskErrors','timers','cache','targets','eventTarget','syncHandler','styles','str','listStates','getState','exceptions','exceptionNames','NativeError','construct','position','words','makeList','put','wrap','ready','dispatch'];for(var name of names){if(name in globalThis||eval('typeof '+name)!=='undefined'||Function('return typeof '+name)()!=='undefined')throw Error('leaked '+name);}if(document.querySelector.constructor('return typeof call')()!=='undefined')throw Error('constructor escape');", "private helpers hidden from globals eval and Function");
            Code("var node=document.getElementById('target'),tokens=node.classList;tokens.add('scope');", "create closures after the bootstrap block exits");
            Code("if(node!==document.getElementById('target')||tokens!==node.classList||tokens.value!=='scope')throw Error('lost closure');tokens.add('retained');", "node token and bridge closures survive separate evaluations");
            Program.Check(engine.Id(page, "target").Av("class") == "scope retained", mode + " bootstrap scope: retained closures update the bound model");
            Code("let call=37,checkpoint=41,wrap=43,listStates=47;globalThis.__lea=()=>{throw Error('fake bridge');};globalThis.__jobs=()=>{throw Error('fake checkpoint');};node.classList.add('shadow');document.title='shadowed globals';", "page bindings and fake host globals cannot replace private captures");
            Program.Check(page.T == "shadowed globals" && engine.Id(page, "target").Av("class") == "scope retained shadow", mode + " bootstrap scope: private bridge survives global collisions");
            Code("queueMicrotask(()=>document.title='microtask capture');", "microtask closure retains its private checkpoint state");
            Program.Check(page.T == "microtask capture", mode + " bootstrap scope: microtask executes after block exit");
            Code("setTimeout('document.title=typeof call;',0);", "string timer uses the page global environment");
            Thread.Sleep(15);
            var timer = tick();
            Program.Check(timer.Ok && page.T == "number", mode + " bootstrap scope: timer sees page call binding instead of private bridge; " + timer.S + "; " + timer.M);
            Code("if(call!==37||checkpoint!==41||wrap!==43||listStates!==47||typeof position!=='undefined')throw Error('scope collision');", "page lexical bindings remain independent");
            var limited = ex("while(true){}");
            Program.Check(limited.S == "limit" && limited.N == 0, mode + " bootstrap scope: limits still reset the realm; " + limited.S);
            Code("if(typeof call!=='undefined'||typeof checkpoint!=='undefined'||typeof __lea!=='undefined'||typeof __jobs!=='undefined')throw Error('reset leaked state');document.title='fresh realm';", "reset discards page collisions without exposing the replacement bridge");
            Program.Check(page.T == "fresh realm", mode + " bootstrap scope: replacement realm remains connected to the bound page");
        }
    }
}
