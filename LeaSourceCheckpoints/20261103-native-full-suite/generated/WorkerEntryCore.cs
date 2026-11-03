using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class WorkerEntryCore
{
    private const string WorkerHash = "F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0";
    private sealed class Row
    {
        internal bool Passed;
        internal string Name;
    }
    private static readonly List<Row> Rows = new List<Row>();
    private static void Check(bool passed, string name)
    {
        Rows.Add(new Row { Passed = passed, Name = name });
        if (!passed) throw new InvalidOperationException(name);
    }
    private static string Interpreters() => string.Join(",", AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetName().Name).Where(n => n == "Jint" || n == "Acornima").OrderBy(n => n, StringComparer.Ordinal));

    private static string[] Invoke(Assembly worker, string[] args)
    {
        var before = Interpreters();
        var oldOutput = Console.Out;
        var oldError = Console.Error;
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        using var error = new StringWriter(CultureInfo.InvariantCulture);
        try
        {
            Console.SetOut(output);
            Console.SetError(error);
            string result;
            try { result = Convert.ToString(worker.EntryPoint.Invoke(null, new object[] { args }), CultureInfo.InvariantCulture); }
            catch (TargetInvocationException failure) { result = failure.InnerException.GetType().FullName + ": " + failure.InnerException.Message; }
            catch (Exception failure) { result = failure.GetType().FullName + ": " + failure.Message; }
            if (output.GetStringBuilder().Length > 16384 || error.GetStringBuilder().Length > 16384)
                throw new InvalidDataException("Entry output exceeds fixture bound.");
            return new[] { result, output.ToString(), error.ToString(), before, Interpreters() };
        }
        finally { Console.SetOut(oldOutput); Console.SetError(oldError); }
    }

    private static int Main(string[] args)
    {
        var clock = Stopwatch.StartNew();
        var priorTrace = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");
        string error = null;
        string[] missing = null;
        try
        {
            if (args.Length != 1) throw new ArgumentException("Expected one pinned worker path.");
            var path = Path.GetFullPath(args[0]);
            if (Path.GetFileName(path) != "Lea.Script.Worker.dll" ||
                !string.Equals(Path.GetDirectoryName(path), Path.GetFullPath(AppContext.BaseDirectory).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Worker must be in the exact private fixture.");
            using (var input = File.OpenRead(path))
                if (Convert.ToHexString(SHA256.HashData(input)) != WorkerHash) throw new InvalidDataException("Worker identity differs.");
            using (var input = File.OpenRead(path))
            using (var pe = new PEReader(input))
            {
                var reader = pe.GetMetadataReader();
                foreach (var handle in reader.TypeDefinitions)
                {
                    var type = reader.GetTypeDefinition(handle);
                    if (reader.GetString(type.Name) != "<Module>") continue;
                    if (type.GetMethods().Any(h => reader.GetString(reader.GetMethodDefinition(h).Name) == ".cctor"))
                        throw new InvalidDataException("Unexpected worker module initializer.");
                }
            }
            var worker = Assembly.LoadFrom(path);
            Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");
            foreach (var invalid in new[] { Array.Empty<string>(), new[] { "--other" }, new[] { "--pipe", "extra" } })
            {
                var report = Invoke(worker, invalid);
                var label = "argc=" + invalid.Length;
                Check(report[0] == "2", "worker rejects invalid arguments before interpreter binding: " + label + "; " + report[0]);
                Check(report[1] == "" && report[2] == "", "invalid worker arguments do not enter startup or write output: " + label);
                Check(report[3] == "" && report[4] == "", "invalid worker arguments never load the interpreter: " + label);
            }
            var run = worker.EntryPoint.DeclaringType.GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic);
            Check(run != null && (run.GetMethodImplementationFlags() & MethodImplAttributes.NoInlining) != 0,
                "worker interpreter has a non-inlined startup boundary");
            bool framesValid;
            using (var input = File.OpenRead(path))
            using (var pe = new PEReader(input))
            {
                var reader = pe.GetMetadataReader();
                var frames = reader.MethodDefinitions.Select(h => reader.GetMethodDefinition(h))
                    .Where(m => reader.GetString(m.Name).StartsWith("<Run>g__Frames|", StringComparison.Ordinal)).ToArray();
                framesValid = frames.Length == 1;
                if (framesValid)
                {
                    var method = frames[0];
                    var signature = reader.GetBlobReader(method.Signature);
                    var header = signature.ReadSignatureHeader();
                    if (header.IsGeneric) signature.ReadCompressedInteger();
                    signature.ReadCompressedInteger();
                    framesValid = signature.ReadSignatureTypeCode() == SignatureTypeCode.Int32 &&
                        (method.ImplAttributes & MethodImplAttributes.NoInlining) != 0;
                }
            }
            Check(framesValid, "worker page processing has a non-inlined post-handshake boundary");
            Check(worker.EntryPoint.GetMethodBody().LocalVariables.All(v => v.LocalType.Namespace == "System" || v.LocalType.Namespace == "System.Diagnostics"),
                "worker entry locals do not require interpreter or closure types");
            missing = Invoke(worker, new[] { "--pipe" });
            Check(missing[0] == "4" && missing[1] == "" && missing[2].Contains("FileNotFoundException:", StringComparison.Ordinal) && missing[2].Contains("Jint", StringComparison.Ordinal),
                "worker handles a missing interpreter without an unhandled entry exception: " + missing[2]);
            Check(missing[2].IndexOf("startup entry", StringComparison.Ordinal) >= 0 &&
                missing[2].IndexOf("startup guards", StringComparison.Ordinal) > missing[2].IndexOf("startup entry", StringComparison.Ordinal) &&
                missing[2].IndexOf("FileNotFoundException:", StringComparison.Ordinal) > missing[2].IndexOf("startup guards", StringComparison.Ordinal) &&
                !missing[2].Contains("startup init", StringComparison.Ordinal),
                "worker applies guards before interpreter binding can fail");
        }
        catch (Exception failure) { error = failure.ToString(); }
        finally { Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", priorTrace); }
        using var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer))
        {
            json.WriteStartObject(); json.WriteBoolean("passed", error == null && Rows.Count == 14);
            json.WriteString("error", error); json.WriteString("workerSha256", WorkerHash);
            json.WriteNumber("elapsedMs", clock.ElapsedMilliseconds); json.WriteBoolean("pageCodeEvaluated", false);
            json.WriteStartArray("checks");
            foreach (var row in Rows) { json.WriteStartObject(); json.WriteBoolean("passed", row.Passed); json.WriteString("name", row.Name); json.WriteEndObject(); }
            json.WriteEndArray();
            if (missing != null) { json.WriteStartArray("missingDependencyReport"); foreach (var value in missing) json.WriteStringValue(value); json.WriteEndArray(); }
            json.WriteEndObject();
        }
        Console.WriteLine(Encoding.UTF8.GetString(buffer.ToArray()));
        return error == null && Rows.Count == 14 ? 0 : 1;
    }
}
