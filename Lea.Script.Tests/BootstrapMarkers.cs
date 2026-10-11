using System;
using System.IO;
using System.Reflection;
using Jint.Native;

namespace Lea.Script.Tests
{
    internal static class BootstrapMarkers
    {
        internal static void Contract()
        {
            Suite(typeof(Lea.Script.Js).Assembly, "trusted");
            Suite(Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.exe")), "worker");
        }

        private static void Suite(Assembly assembly, string name)
        {
            var type = assembly.GetType("Lea.Script.Bm", true);
            var read = type.GetMethod("Of", BindingFlags.Instance | BindingFlags.NonPublic);
            var end = type.GetMethod("End", BindingFlags.Instance | BindingFlags.NonPublic);
            object Create() => Activator.CreateInstance(type, true);
            string Read(object state, params JsValue[] values) => (string)read.Invoke(state, new object[] { values });
            void Reject(Action action, string reason)
            {
                var rejected = false;
                try { action(); }
                catch (TargetInvocationException error) when (error.InnerException is InvalidDataException) { rejected = true; }
                Program.Check(rejected, name + " bootstrap markers: " + reason);
            }
            var markers = Create();
            Program.Check(Read(markers) == null, name + " bootstrap markers: ordinary microtask checkpoint stays separate");
            Reject(() => end.Invoke(markers, null), "incomplete initialization rejected");
            Reject(() => Read(markers, (JsValue)1), "non-string stage rejected");
            Reject(() => Read(markers, (JsValue)"declarations", (JsValue)"scope"), "extra arguments rejected");
            Reject(() => Read(markers, (JsValue)"unknown"), "unknown stage rejected");
            Reject(() => Read(markers, (JsValue)"scope"), "out-of-order stage rejected");
            var stages = new[] { "declarations", "scope", "events", "interfaces", "elements", "document" };
            foreach (var stage in stages)
            {
                Program.Check(Read(markers, (JsValue)stage) == stage, name + " bootstrap markers: ordered " + stage);
                Reject(() => Read(markers, (JsValue)stage), "duplicate " + stage + " rejected without advancing");
            }
            end.Invoke(markers, null);
            Program.Check(Read(markers) == null, name + " bootstrap markers: page microtasks work after initialization");
            Reject(() => Read(markers, (JsValue)"declarations"), "page execution cannot restart markers");
            Program.Check(Read(Create(), (JsValue)"declarations") == "declarations", name + " bootstrap markers: fresh realms own independent state");
        }
    }
}
