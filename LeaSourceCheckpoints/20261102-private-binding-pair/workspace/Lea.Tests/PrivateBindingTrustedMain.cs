using System;
using System.Reflection;
using Lea.Engine;
using Lea.Script;

internal static class PrivateBindingTrustedMain
{
    private static int checks;
    private static void Require(bool value, string message) { checks++; if (!value) throw new Exception(message); }
    private const string Setup = @"'use strict';
var savedMap=WeakMap,savedGet=WeakMap.prototype.get,savedSet=WeakMap.prototype.set,savedApply=Reflect.apply;
var traps=0,fake=new Proxy({}, {get(){traps++;throw Error('receiver trap');},getPrototypeOf(){traps++;throw Error('prototype trap');}});
function reject(action){var error;try{action();}catch(caught){error=caught;}if(!(error instanceof TypeError))throw Error('invalid receiver');}
var add=document.addEventListener,remove=document.removeEventListener,click=Object.getOwnPropertyDescriptor(document,'onclick');
for(var bad of [{},fake,1,'x',true,Symbol('x')]){
 reject(()=>add.call(bad,'click',()=>{}));reject(()=>remove.call(bad,'click',()=>{}));
 reject(()=>click.get.call(bad));reject(()=>click.set.call(bad,()=>{}));
}
reject(()=>Object.getOwnPropertyDescriptor(DOMTokenList.prototype,'length').get.call(fake));
reject(()=>Object.getOwnPropertyDescriptor(DOMException.prototype,'name').get.call(fake));
if(traps!==0)throw Error('proxy trap');
for(var name of ['targetState','windowTarget','documentTarget','StateMap'])if(Function('return typeof '+name)()!=='undefined')throw Error('private state exposed');
WeakMap=function(){throw Error('late constructor');};
savedMap.prototype.get=savedMap.prototype.set=Reflect.apply=function(){throw Error('late intrinsic');};
var route=[];
window.onload=function(event){if(this!==window||event.target!==window)throw Error('load receiver');route.push('load');};
window.addEventListener('click',function(){route.push('window');},true);
document.addEventListener('click',function(){route.push('document');},true);
var target=document.getElementById('target');
var list=target.classList;
if(list.length!==0||list!==target.classList||!target.isConnected)throw Error('first wrapped element');
if(new DOMException('first','SyntaxError').code!==12)throw Error('exception state');
if(document.getElementById('field').classList.length!==0)throw Error('second wrapper');
target.addEventListener('click',function(event){if(this!==target||event.target!==target)throw Error('element receiver');route.push('once');},{once:true});
target.onclick=function(){route.push('handler');return false;};";

    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3 || (args[1] != "ClassLists" && args[1] != "FirstUse")) throw new ArgumentException("Supply original tests, case and fixture.");
            var engine = new E { Jb = false };
            var page = engine.Go(args[2]);
            using (var script = new Js(engine, page))
            {
                if (args[1] == "ClassLists")
                {
                    var assembly = Assembly.LoadFrom(args[0]);
                    var type = assembly.GetType("Lea.Script.Tests.ClassLists", true);
                    var suite = type.GetMethod("Suite", BindingFlags.NonPublic | BindingFlags.Static, null,
                        new[] { typeof(E), typeof(P), typeof(Func<string, Jr>), typeof(string), typeof(Func<string>) }, null);
                    if (suite == null) throw new Exception("Original class-list suite shape differs.");
                    suite.Invoke(null, new object[] { engine, page, new Func<string, Jr>(script.Ex), "in-process", null });
                    checks = (int)assembly.GetType("Lea.Script.Tests.Program", true).GetField("checks", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    if (checks <= 0) throw new Exception("No original assertions executed.");
                }
                else
                {
                    var changes = page.Jc;
                    var setup = script.Ex(Setup);
                    Require(setup.Ok && setup.N == 0 && page.Jc == changes, "Cold first-use probe: " + setup.S + "; " + setup.M);
                    Require(script.Ev(null, "load").Ok, "Direct window event state after poisoned intrinsics.");
                    var target = engine.Q(page, "#target");
                    Require(script.Ev(target, "click").Ok, "First captured element event dispatch.");
                    Require(script.Ev(target, "click").Ok, "Second dispatch with once listener removed.");
                    var verify = script.Ex("if(route.join('|')!=='load|window|document|once|handler|window|document|handler')throw Error('route '+route);WeakMap=savedMap;savedMap.prototype.get=savedGet;savedMap.prototype.set=savedSet;Reflect.apply=savedApply;");
                    Require(verify.Ok && verify.N == 0 && page.Jc == changes, "Event ordering and nonmutation: " + verify.S + "; " + verify.M);
                    engine.Je = false;
                    Require(script.Ex("document.isConnected;").S == "off", "Revocation must remain enforced.");
                }
            }
            Console.WriteLine("PASS Lea private-binding trusted " + args[1] + ": " + checks + " checks.");
            return 0;
        }
        catch (TargetInvocationException error) { Console.Error.WriteLine(error.InnerException ?? error); return 1; }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
