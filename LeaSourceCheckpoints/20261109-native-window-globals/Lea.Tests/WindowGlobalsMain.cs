using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Jint;
using Jint.Native;
using Lea.Script;

internal static class WindowGlobalsMain
{
    private static int checks;
    private static readonly Type installer = typeof(Js).Assembly.GetType("Lea.Script.Gd", true);
    private static readonly MethodInfo bind = installer.GetMethod("Bind", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo install = installer.GetMethod("Install", BindingFlags.Instance | BindingFlags.NonPublic);
    private static Exception Unwrap(Exception error)
    {
        while (error is TargetInvocationException && error.InnerException != null) error = error.InnerException;
        return error;
    }
    private static void Check(bool condition, string name)
    { if (!condition) throw new Exception(name); checks++; }
    private static void Reject<T>(Action action, string name) where T : Exception
    {
        Exception failure = null;
        try { action(); } catch (Exception error) { failure = Unwrap(error); }
        Check(failure is T, name);
    }
    private static Jint.Engine Vm()
    {
        return new Jint.Engine(options => {
            options.MaxStatements(20000).LimitMemory(8 * 1024 * 1024)
                .TimeoutInterval(TimeSpan.FromMilliseconds(1000)).LimitRecursion(64)
                .RegexTimeoutInterval(TimeSpan.FromMilliseconds(100)).MaxArraySize(65536);
            options.Constraints.StackOverflowGuard = true;
            options.AgentCanSuspend = false;
            options.Interop.Enabled = false; options.Interop.AllowGetType = false; options.Interop.AllowWrite = false;
        });
    }
    private static int Main()
    {
        try
        {
            using (var vm = Vm())
            using (var foreign = Vm())
            {
                var instance = Activator.CreateInstance(installer, true);
                Reject<ArgumentNullException>(() => bind.Invoke(instance, new object[] { null }), "null realm rejected");
                bind.Invoke(instance, new object[] { vm });
                Reject<InvalidOperationException>(() => bind.Invoke(instance, new object[] { vm }), "rebinding rejected");
                Reject<InvalidDataException>(() => install.Invoke(instance, new object[] { JsValue.Undefined, new JsValue[0] }), "short initialization rejected");
                Reject<InvalidDataException>(() => install.Invoke(instance, new object[] { JsValue.Undefined, new JsValue[7] }), "long initialization rejected");
                var names = new[] { "setTimeout", "setInterval", "clearTimeout", "clearInterval", "requestAnimationFrame", "cancelAnimationFrame" };
                var parameters = new[] { "a,b=0", "a,b=0", "a=0", "a=0", "a", "a" };
                var functions = new JsValue[6];
                for (int i = 0; i < functions.Length; i++) functions[i] = vm.Evaluate("(function " + names[i] + "(" + parameters[i] + "){})");
                var invalid = (JsValue[])functions.Clone(); invalid[5] = JsValue.Null;
                Reject<InvalidDataException>(() => install.Invoke(instance, new object[] { JsValue.Undefined, invalid }), "invalid final function rejected before writes");
                Check(vm.Evaluate("typeof setTimeout==='undefined'&&typeof cancelAnimationFrame==='undefined'").AsBoolean(), "invalid arguments made no partial installation");
                var other = (JsValue[])functions.Clone(); other[3] = foreign.Evaluate("(function clearInterval(a=0){})");
                Reject<InvalidDataException>(() => install.Invoke(instance, new object[] { JsValue.Undefined, other }), "foreign function realm rejected");
                vm.Invoke(vm.GetValue("__wg"), JsValue.Undefined, functions);
                for (int i = 0; i < functions.Length; i++) Check(ReferenceEquals(vm.GetValue(names[i]), functions[i]), "original function identity: " + names[i]);
                Check(vm.Evaluate(@"(function(){
const names=['setTimeout','setInterval','clearTimeout','clearInterval','requestAnimationFrame','cancelAnimationFrame'];
const lengths=[1,1,0,0,1,1];
for(let i=0;i<names.length;i++){
 const d=Object.getOwnPropertyDescriptor(globalThis,names[i]);
 if(!d.writable||!d.enumerable||!d.configurable||d.value.name!==names[i]||d.value.length!==lengths[i])return false;
}
return Reflect.ownKeys(globalThis).filter(k=>names.includes(k)).join('|')===names.join('|');
}())").AsBoolean(), "public descriptor/name/arity/order contract");
                Reject<InvalidDataException>(() => install.Invoke(instance, new object[] { JsValue.Undefined, functions }), "duplicate installation rejected");
                Exception threadFailure = null;
                var thread = new Thread(() => { try { install.Invoke(instance, new object[] { JsValue.Undefined, functions }); } catch (Exception error) { threadFailure = Unwrap(error); } });
                thread.IsBackground = true; thread.Start();
                Check(thread.Join(2000), "wrong-thread control retired");
                Check(threadFailure is InvalidOperationException, "wrong-thread installation rejected");
                Check(vm.Evaluate("delete globalThis.__wg;typeof __wg==='undefined'&&!('__wg' in globalThis)").AsBoolean(), "installer helper removable and hidden");
                Check(vm.Evaluate("typeof System==='undefined'&&typeof importNamespace==='undefined'").AsBoolean(), "no CLR bridge exposure");
            }
            Console.WriteLine("WINDOW_GLOBALS_CONTRACT PASS " + checks);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(Unwrap(error)); return 1; }
    }
}
