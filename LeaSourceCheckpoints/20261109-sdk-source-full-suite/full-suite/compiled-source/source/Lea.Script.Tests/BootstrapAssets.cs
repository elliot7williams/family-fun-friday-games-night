using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class BootstrapAssets
    {
        internal static void Run(string root)
        {
            var source = File.ReadAllBytes(Path.Combine(root, "Lea.Script", "Dom.js"));
            var compact = File.ReadAllBytes(Path.Combine(root, "Lea.Script", "Dom.compact.js"));
            var worker = ModernComponents.Bootstrap();
            Program.Check(source.Length > compact.Length, "readable bootstrap is retained alongside the smaller generated asset");
            Program.Check(Resource(typeof(Js).Assembly).SequenceEqual(compact), "trusted runtime embeds the exact generated bootstrap");
            Program.Check(Convert.FromBase64String((string)worker["resourceBase64"]).SequenceEqual(compact), "native worker embeds the exact same generated bootstrap");
            Program.Check(typeof(Js).Assembly.GetManifestResourceNames().SequenceEqual(new[] { "Lea.Script.Dom.js" }) &&
                ((System.Collections.IEnumerable)worker["resourceNames"]).Cast<string>().SequenceEqual(new[] { "Lea.Script.Dom.js" }),
                "bootstrap resource identity is unchanged and the readable copy is not also embedded");
        }

        private static byte[] Resource(Assembly assembly)
        {
            using (var stream = assembly.GetManifestResourceStream("Lea.Script.Dom.js"))
            using (var output = new MemoryStream())
            {
                if (stream == null) throw new InvalidOperationException("Missing bootstrap resource: " + assembly.FullName);
                stream.CopyTo(output);
                return output.ToArray();
            }
        }
    }
}
