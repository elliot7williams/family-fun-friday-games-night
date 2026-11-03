using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;

namespace Lea.Script.Tests
{
    internal static class Program
    {
        private static readonly List<object> rows = new List<object>();
        internal static void Check(bool condition, string name)
        {
            rows.Add(new { passed = condition, name });
            if (!condition) throw new InvalidOperationException("FAIL " + name);
        }
        private static int Main(string[] args)
        {
            string error = null;
            try
            {
                if (args.Length != 1) throw new ArgumentException("One pinned component required.");
                var path = Path.GetFullPath(args[0]);
                string hash;
                using (var file = File.OpenRead(path)) using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
                if (hash != "F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0") throw new InvalidDataException("Component differs.");
                using (var file = File.OpenRead(path)) using (var pe = new PEReader(file))
                {
                    var reader = pe.GetMetadataReader();
                    foreach (var handle in reader.TypeDefinitions)
                    {
                        var type = reader.GetTypeDefinition(handle);
                        if (reader.GetString(type.Name) != "<Module>") continue;
                        foreach (var method in type.GetMethods())
                            if (reader.GetString(reader.GetMethodDefinition(method).Name) == ".cctor") throw new InvalidOperationException("Unexpected module initializer.");
                    }
                }
                System.Web.Script.Serialization.JavaScriptSerializer.OracleHash = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "oracle.sha256")).Trim();
                WireFrames.Run(Assembly.LoadFrom(path));
                if (rows.Count != 93) throw new InvalidOperationException("Original wire coverage differs.");
            }
            catch (Exception failure) { error = failure.ToString(); }
            Console.WriteLine(JsonSerializer.Serialize(new { passed = error == null, error, checks = rows,
                componentSha256 = "F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0",
                sourceSuiteSha256 = "8270A23339C9CBEF63166430DCE12351CF6B198908AF52ED6F66B52D5B9E1F0E",
                componentOnly = true, compiledWireMethodsInvoked = true, workerEntryPointExecuted = false,
                pageCodeEvaluated = false, nativeIsolationTest = false }));
            return error == null ? 0 : 1;
        }
    }
}
