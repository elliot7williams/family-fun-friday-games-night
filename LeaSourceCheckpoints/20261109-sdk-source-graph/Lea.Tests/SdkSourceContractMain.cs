using System;
using System.Linq;
using System.Runtime.Versioning;
using Lea.Engine;
using Lea.Script;
using Lea.Script.Win;
using Lea.Win;

internal static class SdkSourceContractMain
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; }

    [STAThread]
    public static int Main()
    {
        try
        {
            foreach (var type in new[] { typeof(E), typeof(V), typeof(Js), typeof(W) })
            {
                var attributes = type.Assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute), false);
                Check(attributes.Length == 1 && ((TargetFrameworkAttribute)attributes[0]).FrameworkName == ".NETFramework,Version=v4.8", type.Name + " framework metadata");
            }
            foreach (var type in new[] { typeof(V), typeof(Js), typeof(W) }) Reference(type, typeof(E));
            Reference(typeof(W), typeof(V));
            Reference(typeof(W), typeof(Js));
            bool onRejected = false, offRejected = false;
            try { W.On(null); } catch (ArgumentNullException) { onRejected = true; }
            try { W.Off(null); } catch (ArgumentNullException) { offRejected = true; }
            Check(onRejected, "null attach rejected");
            Check(offRejected, "null detach rejected");
            using (var view = new V())
            {
                Check(view.Xf == null, "adapter initially detached");
                W.On(view);
                Check(view.Xf != null, "adapter attaches to rebuilt view");
                W.Off(view);
                Check(view.Xf == null, "adapter detaches from rebuilt view");
            }
            Console.WriteLine("PASS SDK SOURCE CONTRACT " + checks);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void Reference(Type owner, Type dependency)
    {
        var expected = dependency.Assembly.GetName();
        var references = owner.Assembly.GetReferencedAssemblies().Where(n => n.Name == expected.Name).ToArray();
        Check(references.Length == 1 && references[0].Version == expected.Version, owner.Name + " matching " + expected.Name);
    }
}
