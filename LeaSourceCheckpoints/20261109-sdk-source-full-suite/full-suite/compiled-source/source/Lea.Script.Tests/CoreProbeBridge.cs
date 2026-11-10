using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

namespace Lea.Script.Tests
{
    internal static class CoreProbeBridge
    {
        private const string WorkerHash = "F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0";
        private static string Hash(string path)
        {
            using (var file = File.OpenRead(path)) using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
        }
        private static string Child(string root, string leaf)
        {
            if (String.IsNullOrEmpty(leaf) || Path.GetFileName(leaf) != leaf || leaf == "." || leaf == "..")
                throw new InvalidDataException("Probe manifest contains a non-leaf path.");
            var path = Path.Combine(root, leaf);
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Probe reparse point rejected.");
            return path;
        }
        internal static Dictionary<string, object> Run(string directory, string manifestHash, string selectedWorker, Action<bool, string> check)
        {
            if (check == null) throw new ArgumentNullException("check");
            var root = Path.GetFullPath(directory);
            if ((File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Probe directory reparse point rejected.");
            var manifestPath = Child(root, "manifest.json");
            if (Hash(manifestPath) != manifestHash || Hash(selectedWorker) != WorkerHash)
                throw new InvalidDataException("Probe manifest or selected candidate differs.");
            var serializer = new JavaScriptSerializer { MaxJsonLength = 1048576, RecursionLimit = 32 };
            var spec = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(manifestPath));
            var pins = (Dictionary<string, object>)spec["files"];
            var binaryName = (string)spec["binary"];
            var expectedCount = (int)spec["checks"];
            if (expectedCount <= 0 || expectedCount > 256 || !pins.ContainsKey(binaryName) ||
                !pins.ContainsKey("Lea.Script.Worker.dll") || !pins.ContainsKey("Jint.dll") || !pins.ContainsKey("Acornima.dll") ||
                !pins.ContainsKey(Path.GetFileNameWithoutExtension(binaryName) + ".runtimeconfig.json"))
                throw new InvalidDataException("Probe manifest is incomplete.");
            var leases = new List<FileStream>();
            try
            {
                leases.Add(File.Open(manifestPath, FileMode.Open, FileAccess.Read, FileShare.Read));
                leases.Add(File.Open(selectedWorker, FileMode.Open, FileAccess.Read, FileShare.Read));
                foreach (var pair in pins)
                {
                    var path = Child(root, pair.Key);
                    leases.Add(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                    if (Hash(path) != (string)pair.Value) throw new InvalidDataException("Probe input differs: " + pair.Key);
                }
                if ((string)pins["Lea.Script.Worker.dll"] != WorkerHash || Hash(manifestPath) != manifestHash || Hash(selectedWorker) != WorkerHash)
                    throw new InvalidDataException("Candidate binding changed before probe.");
                var binary = Child(root, binaryName);
                var host = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe");
                var start = new ProcessStartInfo(host, "\"" + binary + "\" \"" + Child(root, "Lea.Script.Worker.dll") + "\"") {
                    UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = root, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
                foreach (var variable in new[] { "DOTNET_STARTUP_HOOKS", "DOTNET_ADDITIONAL_DEPS", "DOTNET_SHARED_STORE", "CORECLR_ENABLE_PROFILING", "COR_ENABLE_PROFILING" })
                    start.EnvironmentVariables.Remove(variable);
                using (var process = new Process { StartInfo = start })
                {
                    process.Start(); process.StandardInput.Close();
                    var output = process.StandardOutput.ReadToEndAsync();
                    var error = process.StandardError.ReadToEndAsync();
                    if (!process.WaitForExit(60000))
                    {
                        process.Kill();
                        if (!process.WaitForExit(1000)) throw new InvalidOperationException("Probe termination unverified: " + process.Id);
                        throw new TimeoutException("Component probe deadline exceeded; oracle descendants require audit.");
                    }
                    if (!output.Wait(1000) || !error.Wait(1000)) throw new IOException("Probe streams did not drain.");
                    if (process.ExitCode != 0 || output.Result.Length > 1048576 || error.Result.Length != 0)
                        throw new InvalidOperationException("Component probe failed: " + error.Result + output.Result);
                    var report = serializer.Deserialize<Dictionary<string, object>>(output.Result);
                    if (!(bool)report["passed"] || (string)report["componentSha256"] != WorkerHash ||
                        (string)report["sourceSuiteSha256"] != (string)spec["sourceSuiteSha256"] ||
                        !(bool)report["componentOnly"] || (bool)report["workerEntryPointExecuted"] ||
                        (bool)report["pageCodeEvaluated"] || (bool)report["nativeIsolationTest"])
                        throw new InvalidDataException("Probe report scope or candidate differs.");
                    var rows = new List<Dictionary<string, object>>();
                    foreach (Dictionary<string, object> row in (IEnumerable)report["checks"]) rows.Add(row);
                    if (rows.Count != expectedCount) throw new InvalidDataException("Probe assertion coverage differs.");
                    foreach (Dictionary<string, object> row in rows)
                        if (!(bool)row["passed"] || String.IsNullOrEmpty((string)row["name"])) throw new InvalidDataException("Probe contains a failed assertion.");
                    foreach (var pair in pins) if (Hash(Child(root, pair.Key)) != (string)pair.Value) throw new InvalidDataException("Probe input changed.");
                    foreach (Dictionary<string, object> row in rows) check(true, (string)row["name"]);
                    return report;
                }
            }
            finally { foreach (var lease in leases) lease.Dispose(); }
        }
    }
}
