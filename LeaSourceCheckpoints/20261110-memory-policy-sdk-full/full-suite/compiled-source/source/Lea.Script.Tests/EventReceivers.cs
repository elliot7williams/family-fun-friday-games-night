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
    internal static class EventReceivers
    {
        internal static void Run(string fixture, bool trustedOnly = false, string output = null, bool isolatedOnly = false)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            var target = engine.Id(page, "target");
            if (!isolatedOnly)
                using (var script = new Js(engine, page))
                    Suite(code => Task.FromResult(script.Ex(code)), () => Task.FromResult(script.Ev(target, "click")), "trusted").GetAwaiter().GetResult();
            if (trustedOnly) return;
            page = engine.Go(fixture);
            target = engine.Id(page, "target");
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(code => Task.FromResult(script.Ex(code)), () => Task.FromResult(script.Ev(target, "click")), "isolated").GetAwaiter().GetResult();
            if (isolatedOnly) return;
            View.Pump(() => Native(fixture, output));
        }

        private static async Task Suite(Func<string, Task<Jr>> ex, Func<Task<Jr>> fire, string mode)
        {
            async Task<Jr> Code(string code, string name)
            {
                var result = await ex(code);
                Program.Check(result.Ok, mode + " event receivers: " + name + "; " + result.S + "; " + result.M);
                return result;
            }
            async Task Fire(bool prevented = false)
            {
                var result = await fire();
                Program.Check(result.Ok && result.D == prevented, mode + " event receivers: dispatch/cancellation; " + result.S + "; " + result.M);
            }

            var constructors = await Code(@"var receiverConstructorCalls=0,receiverConstructorReads=0;
var receiverConstructorType={toString(){receiverConstructorReads++;return 'click';}};
function receiverConstructorCallback(){receiverConstructorCalls++;}
function receiverNonConstructor(method){
  if(Object.hasOwn(method,'prototype'))throw Error('event operation constructor prototype');
  var error;try{Reflect.construct(String,[receiverConstructorType],method);}catch(value){error=value;}
  if(!(error instanceof TypeError))throw Error('event operation newTarget');
  error=undefined;try{new method(receiverConstructorType,receiverConstructorCallback);}catch(value){error=value;}
  if(!(error instanceof TypeError))throw Error('event operation constructor target');
}
for(var name of ['addEventListener','removeEventListener']){
  var method=window[name];receiverNonConstructor(method);
  receiverNonConstructor(method.bind(document));
}
for(var name of ['onclick','oninput','onchange','onsubmit','onload']){
  var descriptor=Object.getOwnPropertyDescriptor(document,name);
  receiverNonConstructor(descriptor.get);receiverNonConstructor(descriptor.set);
}
if(receiverConstructorCalls||receiverConstructorReads)throw Error('event constructor argument conversion');",
                "shared methods and handler accessors are non-constructible, including bound methods, before conversion or callbacks");
            Program.Check(constructors.N == 0, mode + " event receivers: constructor rejection does not mutate the document");

            await Code(@"var receiverTarget=document.getElementById('target'),receiverOuter=document.getElementById('outer'),receiverTrace=[];
var receiverAdd=receiverOuter.addEventListener,receiverRemove=window.removeEventListener;
function receiverBorrowed(event){if(this!==receiverTarget||event.currentTarget!==this||event.target!==this||event.eventPhase!==2)throw Error('borrowed add receiver');receiverTrace.push('target');}
Reflect.apply(receiverAdd,receiverTarget,['click',receiverBorrowed,{once:true}]);", "borrowed add setup");
            await Fire();
            await Code(@"if(receiverTrace.join(',')!=='target')throw Error('borrowed add target');receiverTrace=[];
for(var node of [receiverOuter,receiverTarget,document,window])for(var name of ['addEventListener','removeEventListener']){
  var descriptor=Object.getOwnPropertyDescriptor(node,name);
  if(node[name]!==window[name]||descriptor.value.name!==name||descriptor.value.length!==2||descriptor.writable||descriptor.enumerable||descriptor.configurable)throw Error('shared method metadata');
}
for(var name of ['onclick','oninput','onchange','onsubmit','onload']){
  var descriptor=Object.getOwnPropertyDescriptor(receiverTarget,name);
  for(var node of [receiverOuter,document,window]){
    var other=Object.getOwnPropertyDescriptor(node,name);
    if(descriptor.get!==other.get||descriptor.set!==other.set||other.enumerable||other.configurable||other.get.length!==0||other.set.length!==1)throw Error('shared handler metadata');
  }
}", "shared methods and handler descriptors retain metadata");
            await Code(@"Reflect.apply(document.addEventListener,receiverTarget,['click',receiverBorrowed,true]);
Reflect.apply(receiverRemove,receiverTarget,['click',receiverBorrowed,true]);", "borrowed remove setup");
            await Fire();
            await Code(@"if(receiverTrace.length)throw Error('borrowed removal');
function receiverLocated(event){receiverTrace.push(this===receiverTarget?'target':this===receiverOuter?'outer':'wrong');}
Reflect.apply(window.addEventListener,receiverTarget,['click',receiverLocated,{once:true}]);
Reflect.apply(receiverTarget.addEventListener,receiverOuter,['click',receiverLocated,{once:true}]);", "same callback belongs to separate receiving targets");
            await Fire();
            await Code("if(receiverTrace.join(',')!=='target,outer')throw Error('receiver state isolation');receiverTrace=[];", "borrowed listeners retain target and bubble ordering");

            var invalid = await Code(@"var receiverReads=0,receiverType={toString(){receiverReads++;return 'click';}},receiverOptions={get capture(){receiverReads++;return false;}};
function receiverTypeError(action){var error;try{action();}catch(value){error=value;}if(!(error instanceof TypeError))throw Error('receiver TypeError required');}
for(var value of [{},[],1,true,'text',1n,Symbol('receiver'),Object.create(receiverTarget),new Proxy(receiverTarget,{get(){throw Error('receiver property read');}})]){
  for(var method of [receiverAdd,receiverRemove])receiverTypeError(()=>Reflect.apply(method,value,[receiverType,receiverBorrowed,receiverOptions]));
}
if(receiverReads)throw Error('receiver validation after conversion');", "illegal and proxy receivers reject before argument conversion");
            Program.Check(invalid.N == 0, mode + " event receivers: invalid receivers do not mutate the document");

            await Code(@"function receiverWindow(event){if(this!==window||event.currentTarget!==window||event.target!==receiverTarget||event.eventPhase!==3)throw Error('global fallback receiver');receiverTrace.push('window');}
var receiverDetached=receiverTarget.addEventListener;
receiverDetached('click',receiverWindow,{once:true});
for(var value of [null,undefined])Reflect.apply(receiverAdd,value,['click',function(event){receiverWindow(event);},{once:true}]);", "detached and nullish calls use the realm global");
            await Fire();
            await Code(@"if(receiverTrace.join(',')!=='window,window,window')throw Error('global fallback registration');receiverTrace=[];
receiverDetached('click',receiverWindow);Reflect.apply(receiverRemove,null,['click',receiverWindow]);
receiverDetached('click',receiverWindow);Reflect.apply(receiverRemove,undefined,['click',receiverWindow]);", "nullish removal uses global state");
            await Fire();
            await Code("if(receiverTrace.length)throw Error('global fallback removal');", "default-receiver removals leave no listeners");

            await Code(@"var receiverClick=Object.getOwnPropertyDescriptor(receiverOuter,'onclick');
function receiverProperty(event){if(this!==receiverTarget||event.currentTarget!==receiverTarget)throw Error('borrowed handler receiver');receiverTrace.push('property');return false;}
Reflect.apply(receiverClick.set,receiverTarget,[receiverProperty]);
if(Reflect.apply(receiverClick.get,receiverTarget,[])!==receiverProperty||receiverOuter.onclick!==null)throw Error('borrowed handler state');
for(var value of [{},Object.create(receiverTarget),new Proxy(receiverTarget,{})]){
  receiverTypeError(()=>Reflect.apply(receiverClick.get,value,[]));receiverTypeError(()=>Reflect.apply(receiverClick.set,value,[receiverProperty]));
}", "borrowed handler accessors bind and brand-check the receiver");
            await Fire(true);
            await Code(@"if(receiverTrace.join(',')!=='property')throw Error('borrowed handler invocation');receiverTrace=[];
Reflect.apply(Object.getOwnPropertyDescriptor(document,'onclick').set,receiverTarget,[null]);
if(receiverTarget.onclick!==null||receiverOuter.onclick!==null)throw Error('handler removal');
function receiverGlobalProperty(event){if(this!==window||event.currentTarget!==window)throw Error('global handler receiver');receiverTrace.push('global-property');return false;}
Reflect.apply(receiverClick.set,null,[receiverGlobalProperty]);
if(Reflect.apply(receiverClick.get,undefined,[])!==receiverGlobalProperty||receiverTarget.onclick!==null)throw Error('global handler state');", "borrowed clear and nullish handler accessors");
            await Fire(true);
            await Code(@"if(receiverTrace.join(',')!=='global-property')throw Error('global handler invocation');receiverTrace=[];
Reflect.apply(receiverClick.set,undefined,[null]);
for(var name of ['oninput','onchange','onsubmit','onload']){
  var descriptor=Object.getOwnPropertyDescriptor(receiverOuter,name);
  Reflect.apply(descriptor.set,receiverTarget,[receiverProperty]);
  if(Reflect.apply(descriptor.get,receiverTarget,[])!==receiverProperty||receiverOuter[name]!==null)throw Error('typed handler receiver');
  Reflect.apply(descriptor.set,receiverTarget,[null]);
}", "each shared handler type retains independent target state");

            await Code(@"var receiverGet=WeakMap.prototype.get,receiverSet=WeakMap.prototype.set,receiverApply=Reflect.apply,receiverError=TypeError;
try{
  WeakMap.prototype.get=function(){throw Error('public weak get reached');};WeakMap.prototype.set=function(){throw Error('public weak set reached');};
  Reflect.apply=function(){throw Error('public apply reached');};TypeError=undefined;
  var error;try{receiverApply(receiverAdd,{},['click',receiverBorrowed]);}catch(value){error=value;}
  if(!(error instanceof receiverError))throw Error('captured receiver error');
  receiverApply(receiverAdd,receiverTarget,['click',receiverBorrowed,{once:true}]);
}catch(error){WeakMap.prototype.get=receiverGet;WeakMap.prototype.set=receiverSet;Reflect.apply=receiverApply;TypeError=receiverError;throw error;}", "captured receiver lookup and invocation setup");
            await Fire();
            await Code(@"WeakMap.prototype.get=receiverGet;WeakMap.prototype.set=receiverSet;Reflect.apply=receiverApply;TypeError=receiverError;
if(receiverTrace.join(',')!=='target')throw Error('captured state lookup');receiverTrace=[];
if(window.onclick!==null||receiverTarget.onclick!==null)throw Error('handler cleanup');", "dispatch uses captured state lookup and leaves handler state clean");
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
                Program.Check(view.Xo != null && view.Xo.Ok, "native event receivers: local fixture load");
                await Suite(async code => Convert(await view.JsA(code)),
                    async () => Convert(await view.Xs.EvA(view.Id("target"), "click")), "native adapter");
                var result = await view.JsA(@"var receiverKeyboard=0,receiverCounter=document.getElementById('counter');
Reflect.apply(receiverAdd,receiverCounter,['click',function(event){if(this!==receiverCounter||event.currentTarget!==this)throw Error('keyboard borrowed receiver');receiverKeyboard++;},{once:true}]);");
                Program.Check(result.Ok, "native event receivers: borrowed keyboard callback setup");
                await View.Activate(view, "counter");
                result = await view.JsA("if(receiverKeyboard!==1||count!==1)throw Error('keyboard borrowed callback');");
                Program.Check(result.Ok && view.Id("result").C[0].X == "1", "native event receivers: real keyboard activation and repaint");
                var worker = ((W)view.Xs).At;
                Program.Check(worker.Ac && worker.Nd && worker.Fd && worker.Mg == 15 && worker.Cp == 2500 && worker.Mm == 128L * 1024 * 1024,
                    "native event receivers: native isolation and resource caps remain active");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 820);
                View.Paint(view, output == null ? null : output + "-narrow.png");
            }
        }
    }
}
