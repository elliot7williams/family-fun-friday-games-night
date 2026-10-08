using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class ListenerCalls
    {
        internal static void Run(string fixture, bool trustedOnly = false, string output = null)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            var target = engine.Id(page, "target");
            using (var script = new Js(engine, page))
                Suite(code => Task.FromResult(script.Ex(code)), () => Task.FromResult(script.Ev(target, "click")), "trusted").GetAwaiter().GetResult();
            if (trustedOnly) return;
            page = engine.Go(fixture);
            target = engine.Id(page, "target");
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(code => Task.FromResult(script.Ex(code)), () => Task.FromResult(script.Ev(target, "click")), "isolated").GetAwaiter().GetResult();
            View.Pump(() => Native(fixture, output));
        }

        private static async Task Suite(Func<string, Task<Jr>> ex, Func<Task<Jr>> fire, string mode)
        {
            async Task Code(string code, string name)
            {
                var result = await ex(code);
                Program.Check(result.Ok, mode + " listener calls: " + name + "; " + result.S + "; " + result.M);
            }
            async Task Fire(bool prevented = false)
            {
                var result = await fire();
                Program.Check(result.Ok && result.D == prevented, mode + " listener calls: dispatch/cancellation; " + result.S + "; " + result.M);
            }

            await Code(@"var callTarget=document.getElementById('target'),callTrace=[],callReads=0,callEvent;
function callListener(event){
  if(this!==callTarget||event.currentTarget!==this||event.target!==this||arguments.length!==1||event.eventPhase!==2)throw Error('function receiver/arguments');
  callEvent=event;callTrace.push('function');
}
Object.defineProperty(callListener,'call',{get(){callReads++;throw Error('public call getter reached');}});
callTarget.addEventListener('click',callListener,{once:true});", "function with throwing own call getter setup");
            await Fire();
            await Code("if(callReads!==0||callTrace.join(',')!=='function'||callEvent.currentTarget!==null||callEvent.eventPhase!==0)throw Error('own call lookup');callTrace=[];", "function invoked without reading own call and event state restored");
            await Fire();
            await Code(@"if(callTrace.length)throw Error('once listener retained');
for(var value of [null,0,{},function(){callTrace.push('fake');}]){
  var callback=function(event){if(this!==callTarget||arguments.length!==1)throw Error('receiver');callTrace.push('value');};
  callback.call=value;callTarget.addEventListener('click',callback,{once:true});
}", "noncallable and redirecting own call properties setup");
            await Fire();
            await Code("if(callTrace.join(',')!=='value,value,value,value')throw Error('own call redirected callback');callTrace=[];", "own call values do not replace listeners");
            await Code(@"var callable=function(event){if(this!==callTarget)throw Error('callable receiver');callTrace.push('callable');};
Object.defineProperty(callable,'handleEvent',{get(){throw Error('callable handleEvent read');}});
var proxy=new Proxy(callable,{apply(target,receiver,args){if(receiver!==callTarget||args.length!==1)throw Error('proxy arguments');callTrace.push('proxy');return Reflect.apply(target,receiver,args);}});
Object.defineProperty(proxy,'call',{get(){throw Error('proxy call read');}});
callTarget.addEventListener('click',proxy,{once:true});", "callable proxy setup");
            await Fire();
            await Code("if(callTrace.join(',')!=='proxy,callable')throw Error('callable operation');callTrace=[];", "callable listener uses internal call and ignores handleEvent");
            await Code(@"var property=function(event){if(this!==callTarget||event.currentTarget!==this||arguments.length!==1)throw Error('DOM0 receiver');callEvent=event;callTrace.push('property');return false;};
Object.defineProperty(property,'call',{get(){callReads++;throw Error('DOM0 call read');}});
callTarget.onclick=property;", "DOM0 handler setup");
            await Fire(true);
            await Code("if(callReads||callTrace.join(',')!=='property'||!callEvent.defaultPrevented||callTarget.onclick!==property)throw Error('DOM0 invocation');callTarget.onclick=null;callTrace=[];", "DOM0 receiver return-false and property identity preserved");
            await Code(@"var objectReads=0,objectListener={get handleEvent(){
  objectReads++;if(this!==objectListener)throw Error('getter receiver');
  var callback=function(event){if(this!==objectListener||event.currentTarget!==callTarget||arguments.length!==1)throw Error('object receiver');callTrace.push('object');return false;};
  Object.defineProperty(callback,'call',{get(){throw Error('object callback call read');}});return callback;
}};
callTarget.addEventListener('click',objectListener,{once:true});
if(objectReads)throw Error('eager handleEvent read');", "object listener defers operation lookup until dispatch");
            await Fire();
            await Code("if(objectReads!==1||callTrace.join(',')!=='object')throw Error('object operation reads');callTrace=[];", "single handleEvent getter read original receiver and ignored false result");
            await Code(@"var mutable={handleEvent:function(){throw Error('stale operation');}};
callTarget.addEventListener('click',mutable,{once:true});
mutable.handleEvent=function(event){if(this!==mutable||event.currentTarget!==callTarget)throw Error('mutable receiver');callTrace.push('latest');};
mutable.handleEvent.call=null;", "object operation can change after registration");
            await Fire();
            await Code("if(callTrace.join(',')!=='latest')throw Error('operation not live');callTrace=[];", "dispatch resolves the current callable operation");
            await Code(@"var savedCall=Function.prototype.call,savedOwn=Object.prototype.hasOwnProperty,savedApply=Reflect.apply;
callTarget.addEventListener('click',callListener,{once:true});
callTarget.onclick=function(event){if(this!==callTarget)throw Error('private ownership');callTrace.push('intrinsic');};
Function.prototype.call=function(){throw Error('public Function.call reached');};
Object.prototype.hasOwnProperty=function(){throw Error('public ownership helper reached');};
Reflect.apply=function(){throw Error('public Reflect.apply reached');};", "page replaces public invocation and ownership helpers");
            await Fire();
            await Code(@"Function.prototype.call=savedCall;Object.prototype.hasOwnProperty=savedOwn;Reflect.apply=savedApply;
if(callTrace.join(',')!=='function,intrinsic'||callReads)throw Error('captured intrinsics');
callTarget.onclick=null;callTrace=[];", "captured helpers survive public prototype replacement");
            await Code(@"try{Reflect.apply({},null,[]);throw Error('noncallable accepted');}
catch(error){if(!(error instanceof TypeError)||error.message!=='Reflect.apply requires the first argument to be a function')throw Error('interpreter callable error');}
var failedCallRealm=1;
callTarget.addEventListener('click',{handleEvent:{call(){document.getElementById('target').textContent='fake operation';}}},{once:true});
callTarget.textContent='before invalid operation';", "noncallable operation with fake call setup");
            var failed = await fire();
            Program.Check(failed.S == "js" && !failed.D && failed.N == 0 && failed.M == "Reflect.apply requires the first argument to be a function",
                mode + " listener calls: noncallable operation fails without fake call; " + failed.S + "; " + failed.M);
            await Code("if(typeof failedCallRealm!=='undefined'||document.getElementById('target').textContent!=='before invalid operation')throw Error('failed realm or fake mutation');", "existing failure reset semantics and no fake mutation");
            await Code(@"var getterCallRealm=1;
document.getElementById('target').addEventListener('click',{get handleEvent(){throw Error('listener getter sentinel');}},{once:true});", "throwing operation getter setup");
            failed = await fire();
            Program.Check(failed.S == "js" && !failed.D && failed.N == 0 && failed.M == "listener getter sentinel",
                mode + " listener calls: operation getter exception remains unchanged");
            await Code("if(typeof getterCallRealm!=='undefined')throw Error('getter failure realm survived');", "getter failure retains existing realm reset");
        }

        private static Jr Convert(Xr result) { return new Jr(result.S, result.N, result.M, result.D); }

        private static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 820), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                W.On(view, null, 5000);
                form.Show();
                await view.GoA(fixture);
                Program.Check(view.Xo != null && view.Xo.Ok, "native listener calls: local fixture load");
                await Suite(async code => Convert(await view.JsA(code)),
                    async () => Convert(await view.Xs.EvA(view.Id("target"), "click")), "native adapter");
                var result = await view.JsA(@"var nativeCallCount=0;
var nativeCallback=function(event){if(this!==document.getElementById('counter')||arguments.length!==1)throw Error('native receiver');nativeCallCount++;};
Object.defineProperty(nativeCallback,'call',{get(){throw Error('native own call read');}});
document.getElementById('counter').addEventListener('click',nativeCallback,{once:true});
var savedNativeCall=Function.prototype.call;Function.prototype.call=function(){throw Error('native public call reached');};");
                Program.Check(result.Ok, "native listener calls: keyboard callback setup");
                await View.Activate(view, "counter");
                result = await view.JsA("Function.prototype.call=savedNativeCall;if(nativeCallCount!==1||count!==1)throw Error('native dispatch');");
                Program.Check(result.Ok && view.Id("result").C[0].X == "1", "native listener calls: real keyboard activation and repaint");
                var worker = ((W)view.Xs).At;
                Program.Check(worker.Ac && worker.Nd && worker.Fd && worker.Mg == 15,
                    "native listener calls: native isolation remains active");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 820);
                View.Paint(view, output == null ? null : output + "-narrow.png");
            }
        }
    }
}
