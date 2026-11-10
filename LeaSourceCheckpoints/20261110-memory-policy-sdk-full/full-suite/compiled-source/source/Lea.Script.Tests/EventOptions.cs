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
    internal static class EventOptions
    {
        internal static void Run(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            var target = engine.Id(page, "target");
            using (var script = new Js(engine, page))
                Suite(code => Task.FromResult(script.Ex(code)), () => Task.FromResult(script.Ev(target, "click")), "trusted").GetAwaiter().GetResult();
            page = engine.Go(fixture);
            target = engine.Id(page, "target");
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(code => Task.FromResult(script.Ex(code)), () => Task.FromResult(script.Ev(target, "click")), "isolated").GetAwaiter().GetResult();
            View.Pump(() => Native(fixture));
        }

        private static async Task Suite(Func<string, Task<Jr>> ex, Func<Task<Jr>> fire, string mode)
        {
            async Task Code(string code, string name)
            {
                var result = await ex(code);
                Program.Check(result.Ok, mode + " event options: " + name + "; " + result.S + "; " + result.M);
            }
            async Task Fire()
            {
                var result = await fire();
                Program.Check(result.Ok, mode + " event options: dispatch; " + result.S + "; " + result.M);
            }

            await Code(@"var optionOuter=document.getElementById('outer'),optionTrace=[],optionCallbacks=[];
function optionCallback(event){optionTrace.push(event.eventPhase);}
function optionTypeError(action){var error;try{action();}catch(value){error=value;}if(!(error instanceof TypeError))throw Error('TypeError required');}
for(var value of [true,1,-1,'capture',1n,Symbol('capture')]){
  var callback=function(event){optionTrace.push(event.eventPhase);};optionCallbacks.push(callback);
  optionOuter.addEventListener('click',callback,value);
}", "truthy primitive capture setup");
            await Fire();
            await Code("if(optionTrace.join(',')!=='1,1,1,1,1,1')throw Error('primitive capture: '+optionTrace);for(var callback of optionCallbacks)optionOuter.removeEventListener('click',callback,true);optionCallbacks=[];optionTrace=[];", "truthy primitives use boolean union branch");
            await Code(@"for(var value of [undefined,null,false,0,0n,NaN,'',new Boolean(true),new Number(1),[]]){
  var callback=function(event){optionTrace.push(event.eventPhase);};optionCallbacks.push(callback);
  optionOuter.addEventListener('click',callback,value);
}", "falsy primitives and boxed dictionaries setup");
            await Fire();
            await Code("if(optionTrace.join(',')!=='3,3,3,3,3,3,3,3,3,3')throw Error('default capture');for(var callback of optionCallbacks)optionOuter.removeEventListener('click',callback,false);optionTrace=[];", "null defaults and objects remain dictionaries");
            await Code(@"var reads=[];
var optionType={toString(){reads.push('type');return 'click';}};
var optionFields=new Proxy({}, {get(target,key){reads.push(key);return key==='signal'?undefined:key==='capture';}});
optionOuter.addEventListener(optionType,optionCallback,optionFields);
if(reads.join(',')!=='type,capture,once,passive,signal')throw Error('add conversion order: '+reads);
reads=[];optionOuter.removeEventListener(optionType,optionCallback,optionFields);
if(reads.join(',')!=='type,capture')throw Error('remove conversion order: '+reads);", "single ordered type and dictionary reads");
            await Code(@"reads=[];window.removeEventListener(optionType,optionCallback);
if(reads.join(',')!=='type')throw Error('empty-list type conversion');
var functionOptions=function(){};Object.defineProperty(functionOptions,'capture',{get(){reads.push('capture');return true;}});
reads=[];optionOuter.addEventListener('click',optionCallback,functionOptions);
if(reads.join(',')!=='capture')throw Error('function dictionary');", "empty removal converts once and functions are dictionaries");
            await Fire();
            await Code("if(optionTrace.join(',')!=='1')throw Error('function capture');optionOuter.removeEventListener('click',optionCallback,1);optionTrace=[];", "primitive removal matches captured listener");
            await Code(@"var primitiveDescriptors={};
try{
  for(var key of ['capture','once','passive','signal']){
    primitiveDescriptors[key]=Object.getOwnPropertyDescriptor(String.prototype,key);
    Object.defineProperty(String.prototype,key,{get(){throw Error('boxed primitive options');},configurable:true});
  }
  optionOuter.addEventListener('click',optionCallback,'capture');
  optionOuter.removeEventListener('click',optionCallback,'capture');
}finally{for(var key of ['capture','once','passive','signal']){var descriptor=primitiveDescriptors[key];if(descriptor)Object.defineProperty(String.prototype,key,descriptor);else delete String.prototype[key];}}
", "primitive branch does not read boxed prototype members");
            await Code(@"reads=[];optionOuter.addEventListener('click',optionCallback,{capture:true,once:true});
optionOuter.addEventListener('click',optionCallback,optionFields);
if(reads.join(',')!=='capture,once,passive,signal')throw Error('duplicate skipped conversion');", "duplicate registration still converts options");
            await Fire();
            await Fire();
            await Code("if(optionTrace.join(',')!=='1')throw Error('duplicate changed once');optionTrace=[];", "duplicate retains original once flag");
            await Code(@"reads=[];optionOuter.addEventListener('click',null,optionFields);
if(reads.join(',')!=='capture,once,passive,signal')throw Error('null skipped conversion');
reads=[];optionOuter.addEventListener('click',undefined,optionFields);
if(reads.join(',')!=='capture,once,passive,signal')throw Error('undefined skipped conversion');
for(var method of ['addEventListener','removeEventListener']){
  optionTypeError(()=>optionOuter[method]());optionTypeError(()=>optionOuter[method]('click'));
  for(var callback of [1,'callback',true,1n,Symbol('callback')]){
    reads=[];optionTypeError(()=>optionOuter[method](optionType,callback,optionFields));
    if(reads.join(',')!=='type')throw Error('invalid callback conversion order');
  }
}", "required arguments nullable callbacks and callback conversion order");
            await Code(@"var sentinel={};
for(var member of ['capture','once','passive','signal']){
  var fields={};Object.defineProperty(fields,member,{get(){throw sentinel;}});
  var error;try{optionOuter.addEventListener('click',optionCallback,fields);}catch(value){error=value;}
  if(error!==sentinel)throw Error('dictionary error changed');
}
for(var signal of [null,false,0,'',1,{},Symbol('signal'),function(){}])
  optionTypeError(()=>optionOuter.addEventListener('click',optionCallback,{signal}));
optionTypeError(()=>optionOuter.addEventListener('click',null,{signal:null}));
", "getter errors propagate and unsupported signals fail closed");
            await Fire();
            await Code("if(optionTrace.length)throw Error('failed conversion installed listener');", "failed conversion cannot partially register");
            await Code(@"var oldString=String,oldTypeError=TypeError;
try{
  String=undefined;TypeError=undefined;
  var rejected;try{optionOuter.addEventListener(Symbol('type'),optionCallback);}catch(error){rejected=error;}
  if(!(rejected instanceof oldTypeError))throw Error('captured type error');
  optionOuter.addEventListener('click',optionCallback,true);
  optionOuter.removeEventListener({toString(){return 'click';}},optionCallback,true);
}finally{String=oldString;TypeError=oldTypeError;}
var onlyCapture={capture:true,get once(){throw Error('remove read once');},get passive(){throw Error('remove read passive');},get signal(){throw Error('remove read signal');}};
optionOuter.addEventListener('click',optionCallback,true);optionOuter.removeEventListener('click',optionCallback,onlyCapture);
", "captured conversions and remove-only capture dictionary");
            await Fire();
            await Code("if(optionTrace.length)throw Error('removal did not match');", "converted removals take effect");
            await Code(@"for(var method of ['addEventListener','removeEventListener']){
  if(optionOuter[method].name!==method||optionOuter[method].length!==2)throw Error('method metadata');
}
", "standard method names and required arity");
        }

        private static Jr Convert(Xr result) { return new Jr(result.S, result.N, result.M, result.D); }

        private static async Task Native(string fixture)
        {
            using (var form = new Form { ClientSize = new Size(940, 820), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                W.On(view, null, 5000);
                form.Show();
                await view.GoA(fixture);
                Program.Check(view.Xo != null && view.Xo.Ok, "native event options: local fixture load");
                await Suite(async code => Convert(await view.JsA(code)),
                    async () => Convert(await view.Xs.EvA(view.Id("target"), "click")), "native adapter");
                var result = await view.JsA("optionTrace=[];document.body.addEventListener('click',optionCallback,1);");
                Program.Check(result.Ok, "native event options: keyboard capture setup");
                await View.Activate(view, "counter");
                result = await view.JsA("if(optionTrace.join(',')!=='1')throw Error('keyboard capture');document.body.removeEventListener('click',optionCallback,1);");
                Program.Check(result.Ok, "native event options: real keyboard activation uses converted capture");
                var worker = ((W)view.Xs).At;
                Program.Check(worker.Ac && worker.Nd && worker.Fd && worker.Mg == 15,
                    "native event options: native isolation remains active");
                View.Paint(view, null);
                form.ClientSize = new Size(420, 820);
                View.Paint(view, null);
            }
        }
    }
}
