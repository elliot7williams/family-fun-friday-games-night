using System;
using System.Reflection;
using Lea.Engine;
using Lea.Script;

internal static class LazyMapTrustedMain
{
    private static int checks;
    private static void Require(bool value, string message) { checks++; if (!value) throw new Exception(message); }
    private const string FirstUse = @"'use strict';
var lateMap=WeakMap,lateGet=WeakMap.prototype.get,lateSet=WeakMap.prototype.set,lateApply=Reflect.apply;
var lateTraps=0,lateFake=new Proxy({}, {get(){lateTraps++;throw Error('receiver trap');}});
function lateReject(action){var error;try{action();}catch(caught){error=caught;}if(!(error instanceof TypeError))throw Error('invalid receiver');}
lateReject(()=>Object.getOwnPropertyDescriptor(DOMTokenList.prototype,'length').get.call(lateFake));
lateReject(()=>Object.getOwnPropertyDescriptor(DOMException.prototype,'name').get.call(lateFake));
if(lateTraps!==0)throw Error('proxy trap invoked');
if(Function('return typeof StateMap')()!=='undefined')throw Error('private constructor exposed');
try{
 WeakMap=function(){throw Error('late public constructor');};
 lateMap.prototype.get=lateMap.prototype.set=Reflect.apply=function(){throw Error('late public intrinsic');};
 var lateList=document.getElementById('target').classList;
 if(lateList.length!==0||lateList.contains('missing'))throw Error('first class list');
 var lateException=new DOMException('late state','SyntaxError');
 if(lateException.name!=='SyntaxError'||lateException.message!=='late state'||lateException.code!==12)throw Error('first exception state');
 var lateFieldList=document.getElementById('field').classList;
 if(lateFieldList.length!==0||lateList!==document.getElementById('target').classList)throw Error('retained map state');
 if(new DOMException('second','TypeMismatchError').code!==17)throw Error('second exception state');
}finally{WeakMap=lateMap;lateMap.prototype.get=lateGet;lateMap.prototype.set=lateSet;Reflect.apply=lateApply;}";

    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 3 || (args[1] != "ClassLists" && args[1] != "FirstUse"))
                throw new ArgumentException("Supply original tests, trusted case and fixture.");
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
                    checks = (int)assembly.GetType("Lea.Script.Tests.Program", true)
                        .GetField("checks", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                    if (checks <= 0) throw new Exception("No original assertions executed.");
                }
                else
                {
                    var changes = page.Jc;
                    var result = script.Ex(FirstUse);
                    Require(result.Ok && result.N == 0 && page.Jc == changes,
                        "Cold captured-constructor/receiver probe must pass without mutation: " + result.S + "; " + result.M);
                    engine.Je = false;
                    Require(script.Ex("document.isConnected;").S == "off", "Revocation must remain enforced.");
                }
            }
            Console.WriteLine("PASS Lea lazy-map trusted " + args[1] + ": " + checks + " checks.");
            return 0;
        }
        catch (TargetInvocationException error) { Console.Error.WriteLine(error.InnerException ?? error); return 1; }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
