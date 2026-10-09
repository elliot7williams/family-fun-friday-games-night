using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class ElementReceivers
    {
        internal static void Run(string fixture, bool isolatedOnly = false, string output = null)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            if (!isolatedOnly)
                using (var script = new Js(engine, page))
                    Suite(code => Task.FromResult(script.Ex(code)), "trusted").GetAwaiter().GetResult();
            page = engine.Go(fixture);
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(code => Task.FromResult(script.Ex(code)), "isolated", () => script.Sd).GetAwaiter().GetResult();
            if (!isolatedOnly) View.Pump(() => Native(fixture, output));
        }

        private static async Task Suite(Func<string, Task<Jr>> ex, string mode, Func<string> diagnostics = null)
        {
            async Task<Jr> Code(string code, string name)
            {
                var result = await ex(code);
                Program.Check(result.Ok, mode + " element receivers: " + name + "; " + result.S + "; " + result.M +
                    (result.Ok || diagnostics == null ? "" : "; " + diagnostics()));
                return result;
            }
            var read = await Code(@"var elementSource=document.getElementById('target'),elementDestination=document.getElementById('field');
var elementGet=elementSource.getAttribute,elementHas=elementSource.hasAttribute;
if(Reflect.apply(elementGet,elementDestination,['id'])!=='field'||Reflect.apply(elementGet,elementSource,['id'])!=='target'||!Reflect.apply(elementHas,elementDestination,['value'])||Reflect.apply(elementHas,elementSource,['value']))throw Error('borrowed attribute target');",
                "borrowed attribute methods use the receiving element");
            Program.Check(read.N == 0, mode + " element receivers: borrowed reads do not mutate the document");
            await Code(@"for(var name of ['getAttribute','hasAttribute']){
 var descriptor=Object.getOwnPropertyDescriptor(elementSource,name),other=Object.getOwnPropertyDescriptor(elementDestination,name);
 if(descriptor.value!==other.value||descriptor.value.name!==name||descriptor.value.length!==1||Object.prototype.hasOwnProperty.call(descriptor.value,'prototype')||descriptor.writable||descriptor.enumerable||descriptor.configurable)throw Error('attribute method metadata');
 var constructed=false;try{Reflect.construct(Object,[],descriptor.value);constructed=true;}catch(error){if(!(error instanceof TypeError))throw error;}if(constructed)throw Error('constructible attribute method');
}
var elementClass=Object.getOwnPropertyDescriptor(elementSource,'classList'),elementOtherClass=Object.getOwnPropertyDescriptor(elementDestination,'classList');
if(elementClass.get!==elementOtherClass.get||elementClass.set!==elementOtherClass.set||elementClass.get.length!==0||elementClass.set.length!==1||elementClass.enumerable||elementClass.configurable)throw Error('class accessor metadata');
if(Reflect.apply(elementClass.get,elementDestination,[])!==elementDestination.classList||Reflect.apply(elementClass.get,elementSource,[])!==elementSource.classList||elementSource.classList===elementDestination.classList)throw Error('borrowed class getter');",
                "shared method/accessor metadata and per-element token-list identity");
            var write = await Code(@"var elementOriginalClass=elementDestination.className;
Reflect.apply(elementClass.set,elementDestination,['receiver receiver extra']);
if(elementSource.className!==''||elementDestination.className!=='receiver receiver extra'||Reflect.apply(elementClass.get,elementDestination,[]).value!=='receiver receiver extra'||Reflect.apply(elementGet,elementDestination,['CLASS'])!=='receiver receiver extra')throw Error('borrowed class setter');
elementDestination.className=elementOriginalClass;", "borrowed class setter forwards to the receiving token list");
            Program.Check(write.N == 2, mode + " element receivers: borrowed setter and restoration each perform one mutation");
            var invalid = await Code(@"var elementReads=0,elementName={toString(){elementReads++;return 'id';}};
function elementTypeError(action){var error;try{action();}catch(value){error=value;}if(!(error instanceof TypeError))throw Error('Element TypeError required');}
for(var receiver of [null,undefined,window,document,{},[],1,true,'text',1n,Symbol('receiver'),Object.create(elementSource),new Proxy(elementSource,{get(){throw Error('receiver property read');}})]){
 for(var method of [elementGet,elementHas])elementTypeError(()=>Reflect.apply(method,receiver,[elementName]));
 elementTypeError(()=>Reflect.apply(elementClass.get,receiver,[]));elementTypeError(()=>Reflect.apply(elementClass.set,receiver,[elementName]));
}
if(elementReads)throw Error('Element receiver checked after conversion');
for(var method of [elementGet,elementHas]){
 elementTypeError(()=>Reflect.apply(method,elementSource,[]));
 elementTypeError(()=>Reflect.apply(method,elementSource,[Symbol('name')]));
 if(Reflect.apply(method,elementSource,[null])!==(method===elementGet?null:false))throw Error('null attribute conversion');
}
elementTypeError(()=>elementGet('id'));elementTypeError(()=>elementHas('id'));",
                "invalid receivers reject before conversion; missing and Symbol names reject");
            Program.Check(invalid.N == 0, mode + " element receivers: rejected access does not mutate the document");
            await Code(@"elementReads=0;
if(Reflect.apply(elementGet,elementDestination,[elementName])!=='field'||elementReads!==1)throw Error('attribute conversion count');
elementReads=0;if(!Reflect.apply(elementHas,elementDestination,[elementName])||elementReads!==1)throw Error('attribute presence conversion count');
var elementStableList=elementDestination.classList;elementDestination.className='live extra';
if(Reflect.apply(elementClass.get,elementDestination,[])!==elementStableList||elementStableList.value!=='live extra'||!elementStableList.contains('live'))throw Error('live borrowed list');
elementDestination.className=elementOriginalClass;", "single name conversion and live token-list identity after metadata writes");
            await Code(@"var elementWeakGet=WeakMap.prototype.get,elementWeakSet=WeakMap.prototype.set,elementApply=Reflect.apply,elementError=TypeError,elementString=String;
try{
 WeakMap.prototype.get=function(){throw Error('public weak get');};WeakMap.prototype.set=function(){throw Error('public weak set');};Reflect.apply=function(){throw Error('public apply');};TypeError=undefined;String=undefined;
 if(elementApply(elementGet,elementDestination,['id'])!=='field'||!elementApply(elementHas,elementDestination,['value']))throw Error('captured attribute lookup');
 if(elementApply(elementClass.get,elementDestination,[])!==elementStableList)throw Error('captured class lookup');
 elementApply(elementClass.set,elementDestination,['captured']);
 var error;try{elementApply(elementGet,{},['id']);}catch(value){error=value;}if(!(error instanceof elementError))throw Error('captured Element error');
}finally{WeakMap.prototype.get=elementWeakGet;WeakMap.prototype.set=elementWeakSet;Reflect.apply=elementApply;TypeError=elementError;String=elementString;}
if(elementDestination.className!=='captured')throw Error('captured setter');elementDestination.className=elementOriginalClass;",
                "captured receiver lookup, conversion and error constructors survive public poisoning");
            await Code(@"Object.defineProperty(Object.prototype,'classTokens',{get(){throw Error('inherited token cache');},set(){throw Error('inherited token cache');},configurable:true});
try{var elementChild=document.getElementById('child');if(elementChild.classList!==Reflect.apply(elementClass.get,elementChild,[])||elementChild.classList.value!=='')throw Error('own token cache');}finally{delete Object.prototype.classTokens;}
if(elementSource.className!==''||elementDestination.className!==elementOriginalClass)throw Error('element cleanup');",
                "new wrappers own their lazy token cache despite inherited accessors");
            var failure = await ex("elementGet.call({},'id');");
            Program.Check(failure.S == "js" && failure.N == 0, mode + " element receivers: uncaught brand failure remains a JavaScript error");
            await Code(@"if(typeof elementSource!=='undefined')throw Error('failed Element realm survived');
var recoveredElement=document.getElementById('field');if(recoveredElement.getAttribute('id')!=='field'||recoveredElement.classList.value!=='')throw Error('fresh Element realm');",
                "failure retires the realm and fresh receiver state recovers");
        }

        private static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 820), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                W.On(view, null, 5000);
                form.Show();
                var page = await view.GoA(fixture);
                Program.Check(page != null && view.Id("field") != null, "native element receivers: local fixture load");
                await Suite(async code => { var r = await view.JsA(code); return new Jr(r.S, r.N, r.M, r.D); },
                    "native adapter", () => ((W)view.Xs).At.Sd);
                await View.Activate(view, "field");
                var editor = view.Controls.OfType<TextBox>().Single();
                var field = page.Fs.Single(item => item.El.Av("id") == "field");
                var accessible = view.Ao.GetFocused();
                editor.Text = "Elliot";
                await view.Xt;
                var result = await view.JsA(@"var nativeElement=document.getElementById('field'),nativeElementClass=Object.getOwnPropertyDescriptor(document.getElementById('target'),'classList');
Reflect.apply(nativeElementClass.set,nativeElement,['active']);nativeElement.id='field-new';
if(Reflect.apply(document.getElementById('target').getAttribute,nativeElement,['id'])!=='field-new')throw Error('native borrowed read');");
                Program.Check(result.Ok && result.N == 2 && view.Id("field-new") == field.El && field.S.W == 190 &&
                    editor.Visible && editor.Text == "Elliot" && field.V == "Elliot" && view.Controls.OfType<TextBox>().Single() == editor,
                    "native element receivers: borrowed setter reflows the retained edited field");
                Program.Check(accessible != null && accessible.Name == "Updated label" && view.Ao.GetFocused().Name == "Updated label",
                    "native element receivers: borrowed writes retain focus and update accessible association");
                var worker = ((W)view.Xs).At;
                Program.Check(worker.Ac && worker.Nd && worker.Fd && worker.Mg == 15 && worker.Cp == 2500 && worker.Mm == 128L * 1024 * 1024,
                    "native element receivers: isolation and resource limits remain active");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 820);
                View.Paint(view, output == null ? null : output + "-narrow.png");
                Program.Check(editor.Visible && view.ClientRectangle.Contains(editor.Bounds),
                    "native element receivers: borrowed-write field fits the narrow viewport");
            }
        }
    }
}
