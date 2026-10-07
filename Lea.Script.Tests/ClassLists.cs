using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Jint;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class ClassLists
    {
        internal static void Profile(string sourceFile)
        {
            var source = sourceFile == null
                ? (string)typeof(Js).GetField("bootstrap", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null)
                : System.IO.File.ReadAllText(sourceFile);
            var stages = new[] { "    let failed =", "    const realm =", "    const exceptions =", "    function DOMException(", "    DOMException.prototype =", "    const exceptionConstants =",
                "    Object.defineProperties(DOMException,", "    Object.defineProperties(DOMException.prototype, exceptionConstants)", "    define(DOMException.prototype, Symbol.toStringTag",
                "    function DOMTokenList()", "    DOMTokenList.prototype =", "    const listPrototype =", "    define(listPrototype, Symbol.iterator", "    const document =",
                "    eventTarget(document);", "    delete globalThis.__lea;", "    (function dispatch(" };
            for (var i = 0; i < stages.Length; i++)
            {
                Console.WriteLine("Trace point=" + i + "; " + stages[i].Trim());
                if (source.IndexOf(stages[i], StringComparison.Ordinal) < 0) continue;
                if (source.IndexOf(stages[i], StringComparison.Ordinal) != source.LastIndexOf(stages[i], StringComparison.Ordinal))
                    throw new InvalidOperationException("Ambiguous bootstrap trace point: " + stages[i]);
                source = source.Replace(stages[i], "    __mark('" + i + "');\n" + stages[i]);
            }
            for (var sample = 0; sample < 2; sample++)
            {
                var clock = System.Diagnostics.Stopwatch.StartNew();
                using (var process = System.Diagnostics.Process.GetCurrentProcess())
                {
                    var cpu = process.TotalProcessorTime;
                    var records = new System.Collections.Generic.List<string>();
                    // Diagnostic-only instrumentation; the unmodified Start probe remains the production-path gate.
                    var vm = new Jint.Engine(options =>
                    {
                        options.MaxStatements(20000).LimitMemory(8 * 1024 * 1024)
                            .TimeoutInterval(TimeSpan.FromSeconds(1)).LimitRecursion(64)
                            .RegexTimeoutInterval(TimeSpan.FromMilliseconds(100)).MaxArraySize(65536);
                        options.Constraints.StackOverflowGuard = true;
                        options.Interop.Enabled = false;
                        options.Interop.AllowGetType = false;
                        options.Interop.AllowWrite = false;
                    });
                    vm.SetValue("__mark", new Jint.Runtime.Interop.ClrFunction(vm, "__mark", (_, args) =>
                    {
                        records.Add("stage=" + args[0].AsString() + "; wall=" + clock.ElapsedMilliseconds + "; cpu=" + (long)(process.TotalProcessorTime - cpu).TotalMilliseconds);
                        return Jint.Native.JsValue.Undefined;
                    }));
                    vm.SetValue("__lea", Jint.Native.JsValue.Undefined);
                    vm.SetValue("__jobs", Jint.Native.JsValue.Undefined);
                    records.Add("VM ready; wall=" + clock.ElapsedMilliseconds + "; cpu=" + (long)(process.TotalProcessorTime - cpu).TotalMilliseconds);
                    string outcome;
                    try { vm.Evaluate(source); outcome = "completed"; }
                    catch (Exception error) { outcome = error.ToString(); }
                    Console.WriteLine("Profile sample=" + sample + "; wall=" + clock.ElapsedMilliseconds + "; cpu=" + (long)(process.TotalProcessorTime - cpu).TotalMilliseconds);
                    foreach (var record in records) Console.WriteLine(record);
                    Console.WriteLine(outcome);
                }
            }
        }

        internal static void Bootstrap(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            using (var script = new Js(engine, page))
            {
                var start = typeof(Js).GetMethod("Start", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var clock = System.Diagnostics.Stopwatch.StartNew();
                try { start.Invoke(script, null); }
                catch (System.Reflection.TargetInvocationException error)
                {
                    Console.WriteLine("Bootstrap failure: wall=" + clock.ElapsedMilliseconds + "; " + error.InnerException);
                    throw;
                }
                Console.WriteLine("Bootstrap completed: wall=" + clock.ElapsedMilliseconds);
            }
        }

        internal static void Run(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, "in-process");
            engine.Je = true;
            page = engine.Go(fixture);
            using (var script = new Jp(engine, page) { Tm = 5000 })
                Suite(engine, page, script.Ex, "isolated", () => script.Sd);
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, string mode, Func<string> diagnostics = null)
        {
            Jr Code(string code, string name)
            {
                var result = ex(code);
                Program.Check(result.Ok, mode + " classList: " + name + "; " + result.S + "; " + result.M +
                    (result.Ok || diagnostics == null ? "" : "; " + diagnostics()));
                return result;
            }
            var target = engine.Id(page, "target");
            Code("var target=document.getElementById('target'),list=target.classList;if(list.contains('')||list.contains(' '))throw Error('empty membership');", "empty membership is false");
            var absent = Code("list.add();list.remove('missing');if(target.hasAttribute('class'))throw Error('absent update');", "empty updates do not create an absent class");
            Program.Check(absent.N == 0, mode + " classList: absent updates do not rebuild");
            Code("if(list!==target.classList||!(list instanceof DOMTokenList)||Object.prototype.toString.call(list)!=='[object DOMTokenList]')throw Error('identity');", "stable branded identity");
            Code("for(var name of ['DOMTokenList','DOMException']){var d=Object.getOwnPropertyDescriptor(globalThis,name);if(!d.writable||!d.enumerable||!d.configurable)throw Error('global descriptor');d=Object.getOwnPropertyDescriptor(globalThis[name],'prototype');if(d.writable||d.enumerable||d.configurable)throw Error('prototype descriptor');}var d=Object.getOwnPropertyDescriptor(DOMTokenList.prototype,Symbol.iterator);if(!d.writable||d.enumerable||!d.configurable)throw Error('iterator descriptor');", "interface global and prototype descriptors");
            Code("for(var entry of [['item',1],['contains',1],['add',0],['remove',0],['toggle',1],['replace',2],['supports',1],['toString',0]]){var name=entry[0],d=Object.getOwnPropertyDescriptor(DOMTokenList.prototype,name);if(!d.enumerable||!d.writable||!d.configurable||d.value.name!==name||d.value.length!==entry[1])throw Error('method metadata '+name);}var d=Object.getOwnPropertyDescriptor(DOMTokenList.prototype,'value');if(d.get.name!=='get value'||d.set.name!=='set value'||!d.enumerable||!d.configurable)throw Error('accessor metadata');", "standard method names arity and descriptors");
            Code("target.className='  alpha\\tbeta\\nalpha\\fgamma\\r';if(list.length!==3||list[0]!=='alpha'||list.item(1)!=='beta'||list[3]!==undefined||list.item(3)!==null||list.value!==target.className||String(list)!==target.className)throw Error('live list');", "raw reflection and ordered unique ASCII words");
            Code("if(list.item(-1)!==null||list.item(1.9)!=='beta'||list.item(NaN)!=='alpha'||list.item(4294967296)!=='alpha')throw Error('unsigned item');", "unsigned item conversion");
            Code("if(list.add('alpha','delta','delta')!==undefined||list.value!=='alpha beta gamma delta')throw Error('variadic add');list.remove('beta','missing');if(list.value!=='alpha gamma delta')throw Error('variadic remove');", "atomic variadic add and remove");
            Code("target.className='Active active';list.remove('Active');if(list.value!=='active'||list.contains('Active')||!list.contains('active'))throw Error('case');list.add('Active');if(list.value!=='active Active')throw Error('case add');", "case-sensitive writes and membership");
            Code("if(list.toggle('active',true)!==true||list.toggle('active',false)!==false||list.toggle('active',false)!==false||list.toggle('active')!==true||list.toggle('active',undefined)!==false)throw Error('toggle');", "toggle force and return values");
            Code("target.className='a b c';if(!list.replace('a','c')||list.value!=='c b'||list.replace('missing','d')!==false||!list.replace('c','c'))throw Error('replace');target.className='a b c';list.replace('c','a');if(list.value!=='a b')throw Error('replace earlier');", "ordered-set replace and duplicate position");
            Code("target.className=' alpha alpha  ';list.add();if(list.value!=='alpha')throw Error('zero add');list.remove('missing');if(list.value!=='alpha')throw Error('zero remove');", "update normalizes present attributes");
            Code("list.value='  alpha alpha  ';", "prepare no-op raw class");
            var unchanged = Code("if(!list.toggle('alpha',true)||list.toggle('missing',false)||list.replace('missing','later')||list.value!=='  alpha alpha  ')throw Error('no-op serialization');", "forced toggle and missing replace preserve raw attributes");
            Program.Check(unchanged.N == 0, mode + " classList: forced no-op mutations do not rebuild");
            Code("target.className='';list.remove();if(!target.hasAttribute('class')||list.value!=='')throw Error('empty presence');", "empty class remains present");
            Code("list.value='raw raw\\fwords';if(list.length!==2||list.value!=='raw raw\\fwords')throw Error('value');target.classList='next next';if(list.value!=='next next'||list.length!==1)throw Error('forwarding');", "value setter and classList forwarding");
            Code("var forwarded={toString(){throw Error('early forwarding conversion');}},seenForward;Object.defineProperty(list,'value',{set(value){seenForward=value;},configurable:true});try{target.classList=forwarded;if(seenForward!==forwarded)throw Error('forwarded setter');}finally{delete list.value;}", "classList forwards without premature conversion to an overridden value setter");
            Code("list.value='x\\u00a0y x\\vy';if(list.length!==2||list.contains('x')||!list.contains('x\\u00a0y')||!list.contains('x\\vy'))throw Error('non ASCII');list.add('unicode-\\u2603','punctuation!?');", "non-ASCII whitespace and opaque tokens");
            Code("list.value='a b';var it=list.values();if(it.next().value!=='a')throw Error('iterator first');list.add('c');if(it.next().value!=='b'||it.next().value!=='c'||!it.next().done)throw Error('live iterator');list.add('d');if(!it.next().done)throw Error('completed iterator');", "live iterator and completed-state stability");
            Code("var keys=[...list.keys()],entries=[...list.entries()];if(keys.join(',')!=='0,1,2,3'||entries.map(x=>x.join(':')).join(',')!=='0:a,1:b,2:c,3:d'||[...list].join(',')!=='a,b,c,d'||list[Symbol.iterator]!==list.values)throw Error('iterable');", "keys entries and default iteration");
            Code("list.value='a b';var seen=[],receiver={};list.forEach(function(value,index,self){if(this!==receiver||self!==list)throw Error('callback');seen.push(index+':'+value);if(index===0)list.add('c');},receiver);if(seen.join(',')!=='0:a,1:b'||list.length!==3)throw Error('forEach live');", "forEach snapshot length receiver and arguments");
            Code("if(Object.keys(list).join(',')!=='0,1,2'||!('2' in list)||'3' in list||list['01']!==undefined)throw Error('keys');var d=Object.getOwnPropertyDescriptor(list,'0');if(d.value!=='a'||d.writable||!d.enumerable||!d.configurable)throw Error('descriptor');", "live indexed descriptors and enumeration");
            Code("if(Reflect.set(list,'0','x')||Reflect.set(list,'20','x')||Reflect.defineProperty(list,'0',{value:'x'})||Reflect.deleteProperty(list,'0')||Reflect.preventExtensions(list))throw Error('readonly index');list.extra=1;if(list.extra!==1||!Reflect.deleteProperty(list,'20'))throw Error('expando');delete list.extra;", "read-only virtual indices and extensibility");
            Code("function expects(fn,name,code){var error;try{fn();}catch(e){error=e;}if(!error||error.name!==name||(code!==undefined&&(!(error instanceof DOMException)||error.code!==code)))throw Error('expected '+name);}var raw=list.value;expects(()=>list.add('fresh',''),'SyntaxError',12);expects(()=>list.remove('a','bad name'),'InvalidCharacterError',5);expects(()=>list.toggle(''),'SyntaxError',12);expects(()=>list.replace('a','bad\\tname'),'InvalidCharacterError',5);if(list.value!==raw)throw Error('partial mutation');", "invalid tokens fail atomically with DOM errors");
            Code("expects(()=>list.replace('bad name',''),'SyntaxError',12);expects(()=>list.replace('','bad name'),'SyntaxError',12);expects(()=>list.add('bad name',''),'InvalidCharacterError',5);expects(()=>list.remove('bad name',''),'InvalidCharacterError',5);if(list.value!==raw)throw Error('validation partial');", "replace checks either empty argument before whitespace");
            Code("var error=new DOMException();if(error.name!=='Error'||error.message!==''||error.code!==0)throw Error('DOMException defaults');", "DOMException default construction and state");
            Code("if(!(error instanceof Error)||Object.getPrototypeOf(DOMException)!==Function.prototype||Object.getPrototypeOf(DOMException.prototype)!==Error.prototype||Object.prototype.toString.call(error)!=='[object DOMException]'||Object.hasOwn(error,'name')||Object.hasOwn(error,'message'))throw Error('DOMException branding');", "DOMException native Error branding and prototype chain");
            Code("var error=new DOMException(null,null);if(error.name!=='null'||error.message!=='null'||error.code!==0)throw Error('DOMException strings');", "DOMException nullable DOM string conversion");
            Code("var step='class';try{class DerivedException extends DOMException{}step='construct';var error=new DerivedException('message','SyntaxError');step='instance';if(!(error instanceof DerivedException))throw Error('instance');step='code';if(error.code!==12)throw Error('code');step='string';if(error.toString()!=='SyntaxError: message')throw Error('string');}catch(e){throw Error('DOMException subclass '+step+': '+e.message);}", "DOMException subclass construction and stringification");
            Code("var order=[],receiver={get name(){if(this!==receiver)throw Error('name receiver');order.push('N');return {toString(){order.push('C');return 'Name';}};},get message(){if(this!==receiver)throw Error('message receiver');order.push('M');return 'message';}};if(Error.prototype.toString.call(receiver)!=='Name: message'||order.join('')!=='NCM'||new TypeError('native').toString()!=='TypeError: native'||Error.prototype.toString.call({name:'',message:'only'})!=='only')throw Error('Error stringification');expects(()=>Error.prototype.toString.call(null),'TypeError');expects(()=>Error.prototype.toString.call(1),'TypeError');expects(()=>Error.prototype.toString.call({name:Symbol('x')}),'TypeError');", "Error stringification uses the original receiver and ordered DOM conversions");
            Code("var names=['IndexSizeError','','HierarchyRequestError','WrongDocumentError','InvalidCharacterError','','NoModificationAllowedError','NotFoundError','NotSupportedError','InUseAttributeError','InvalidStateError','SyntaxError','InvalidModificationError','NamespaceError','InvalidAccessError','','TypeMismatchError','SecurityError','NetworkError','AbortError','URLMismatchError','QuotaExceededError','TimeoutError','InvalidNodeTypeError','DataCloneError'];for(var i=0;i<names.length;i++)if(names[i]&&new DOMException('',names[i]).code!==i+1)throw Error('legacy code '+i);for(var name of ['DOMStringSizeError','NoDataAllowedError','ValidationError','syntaxerror','CustomError'])if(new DOMException('',name).code!==0)throw Error('unknown legacy code');", "DOMException standard and unassigned legacy codes");
            Code("var constants=['INDEX_SIZE_ERR','DOMSTRING_SIZE_ERR','HIERARCHY_REQUEST_ERR','WRONG_DOCUMENT_ERR','INVALID_CHARACTER_ERR','NO_DATA_ALLOWED_ERR','NO_MODIFICATION_ALLOWED_ERR','NOT_FOUND_ERR','NOT_SUPPORTED_ERR','INUSE_ATTRIBUTE_ERR','INVALID_STATE_ERR','SYNTAX_ERR','INVALID_MODIFICATION_ERR','NAMESPACE_ERR','INVALID_ACCESS_ERR','VALIDATION_ERR','TYPE_MISMATCH_ERR','SECURITY_ERR','NETWORK_ERR','ABORT_ERR','URL_MISMATCH_ERR','QUOTA_EXCEEDED_ERR','TIMEOUT_ERR','INVALID_NODE_TYPE_ERR','DATA_CLONE_ERR'];for(var i=0;i<constants.length;i++)for(var owner of [DOMException,DOMException.prototype]){var d=Object.getOwnPropertyDescriptor(owner,constants[i]);if(d.value!==i+1||d.writable||d.configurable||!d.enumerable)throw Error('constant descriptor');}for(var name of ['name','message','code']){var d=Object.getOwnPropertyDescriptor(DOMException.prototype,name);if(!d.enumerable||!d.configurable||d.set!==undefined||d.get.name!=='get '+name)throw Error('exception accessor');expects(()=>d.get.call({}),'TypeError');}expects(()=>DOMException(),'TypeError');expects(()=>new DOMException(Symbol('message')),'TypeError');expects(()=>new DOMException('',Symbol('name')),'TypeError');", "DOMException constants readonly accessors and illegal receivers");
            Code("for(var space of [' ','\\t','\\n','\\f','\\r'])expects(()=>list.add('a'+space+'b'),'InvalidCharacterError',5);expects(()=>list.contains(),'TypeError');expects(()=>list.toggle(),'TypeError');expects(()=>list.replace('a'),'TypeError');expects(()=>list.item(),'TypeError');expects(()=>list.forEach(null),'TypeError');expects(()=>list.supports('a'),'TypeError');expects(()=>new DOMTokenList(),'TypeError');", "ASCII validation required arguments and unsupported vocabulary");
            Code("var order=[];expects(()=>list.add('',{toString(){order.push('converted');return 'later';}}),'SyntaxError',12);if(order.join()!=='converted')throw Error('conversion order');list.value='before';list.add({toString(){list.value='after';return 'converted';}});if(list.value!=='after converted')throw Error('conversion state');", "all conversions precede token validation and live reads");
            Code("for(var method of ['add','remove','contains','toggle','supports'])expects(()=>list[method](Symbol('x')),'TypeError');expects(()=>list.replace('a',Symbol('x')),'TypeError');expects(()=>list.value=Symbol('x'),'TypeError');expects(()=>list.item(1n),'TypeError');expects(()=>list.contains.call({},'a'),'TypeError');expects(()=>Object.getOwnPropertyDescriptor(DOMTokenList.prototype,'value').get.call({}),'TypeError');", "Symbol conversion and illegal receivers");
            Code("var raw='x'.repeat(512);list.value=raw;expects(()=>list.add('overflow'),'TypeError');if(list.value!==raw)throw Error('size partial');list.value='a';list.add('y'.repeat(510));if(list.value.length!==512)throw Error('exact bound');", "existing 512-unit write bound is atomic");
            Code("list.value='';var tokens=[];for(var i=0;i<100;i++)tokens.push('c'+i);list.add(...tokens);if(list.length!==100||list[99]!=='c99'||list.value.length>512)throw Error('bulk tokens');", "100 distinct tokens fit the existing source and execution budgets");
            Code("list.value='a b';var oldString=String,oldIterator=Array.prototype[Symbol.iterator],oldIncludes=Array.prototype.includes,oldSplit=String.prototype.split,oldJoin=Array.prototype.join;try{String=undefined;Array.prototype[Symbol.iterator]=()=>{throw Error('iterator');};Array.prototype.includes=()=>{throw Error('includes');};oldString.prototype.split=()=>{throw Error('split');};Array.prototype.join=()=>{throw Error('join');};list.add('c','d');list.remove('a');if(!list.contains('d')||list.value!=='b c d')throw Error('captured');}finally{String=oldString;Array.prototype[Symbol.iterator]=oldIterator;Array.prototype.includes=oldIncludes;String.prototype.split=oldSplit;Array.prototype.join=oldJoin;}", "captured conversion and prototype-independent token operations");
            Code("list.value='';list.add(null,undefined,7n);if(list.value!=='null undefined 7')throw Error('conversion values');", "null undefined and BigInt DOM strings");
            Code("var oldIndex=Array.prototype.indexOf;try{Array.prototype.indexOf=()=>{throw Error('modified lookup');};list.add('captured');list.remove('captured');if(!list.contains('7'))throw Error('captured lookup');}finally{Array.prototype.indexOf=oldIndex;}", "native token lookup is captured before page prototype changes");
            Code("var OldTypeError=TypeError,OldDOMException=DOMException,OldWeakMap=WeakMap;try{TypeError=undefined;DOMException=undefined;WeakMap=undefined;var error;try{list.add(Symbol('x'));}catch(e){error=e;}if(!(error instanceof OldTypeError))throw Error('captured TypeError');try{list.add('');}catch(e){error=e;}if(!(error instanceof OldDOMException)||error.code!==12)throw Error('captured DOMException');try{list.value='x'.repeat(513);}catch(e){error=e;}if(!(error instanceof OldTypeError)||list.value!=='null undefined 7')throw Error('captured write rejection');}finally{TypeError=OldTypeError;DOMException=OldDOMException;WeakMap=OldWeakMap;}", "captured error constructors and private state survive global replacement");
            var field = page.Fs.Single(item => item.El.Av("id") == "field");
            Code("var field=document.getElementById('field');field.value='edited';field.classList.add('active','extra');field.classList.replace('extra','replacement');", "retained fields after bulk mutations");
            Program.Check(page.Fs.Contains(field) && field.V == "edited" && field.S.W == 190 && field.El.Av("class") == "active replacement", mode + " classList: field identity value and CSS retained");
            Code("var child=document.getElementById('child'),old=child.classList;document.getElementById('parent').textContent='replacement';expects(()=>old.add('detached'),'TypeError');", "detached mutation remains rejected");
            engine.Je = false;
            var denied = ex("list.add('denied');");
            Program.Check(denied.S == "off" && denied.N == 0 && target.Av("class") == "null undefined 7", mode + " classList: permission gate");
        }

        internal static async Task Native(string fixture, string output)
        {
            using (var form = new Form { ClientSize = new Size(940, 820), ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) })
            using (var view = new V { Dock = DockStyle.Fill })
            {
                form.Controls.Add(view);
                W.On(view, null, 5000);
                form.Show();
                var page = await view.GoA(fixture);
                var field = page.Fs.Single(item => item.El.Av("id") == "field");
                await View.Activate(view, "field");
                var editor = view.Controls.OfType<TextBox>().Single();
                editor.Text = "Elliot";
                await view.Xt;
                var result = await view.JsA("var field=document.getElementById('field'),list=field.classList;list.add('active','temporary');list.replace('temporary','ready');if(!list.toggle('active',true)||list.value!=='active ready'||[...list].join(',')!=='active,ready')throw Error('native list');");
                Program.Check(result.Ok, "native classList: code; " + result.S + "; " + result.M);
                Program.Check(page.Fs.Contains(field) && editor == view.Controls.OfType<TextBox>().Single() &&
                    editor.Visible && editor.Text == "Elliot" && field.V == "Elliot" && field.S.W == 190,
                    "native classList: bulk writes retain editor identity value and CSS");
                View.Paint(view, output == null ? null : output + "-desktop.png");
                form.ClientSize = new Size(420, 820);
                View.Paint(view, output == null ? null : output + "-narrow.png");
                Program.Check(editor.Visible && view.ClientRectangle.Contains(editor.Bounds), "native classList: narrow editor bounds");
            }
        }
    }
}
