using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lea.Script.Tests
{
    internal static class WorkerEntry
    {
        internal static void Run()
        {
            var runtime = AppDomain.CurrentDomain.BaseDirectory;
            var root = Path.Combine(Path.GetTempPath(), "Lea-worker-entry-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var worker = Path.Combine(root, "Lea.Script.Worker.exe");
            var tests = Path.Combine(root, Path.GetFileName(typeof(WorkerEntry).Assembly.Location));
            var files = new[] { worker, worker + ".config", tests };
            AppDomain domain = null;
            var passed = false;
            try
            {
                File.Copy(Path.Combine(runtime, "ScriptWorker", Path.GetFileName(worker)), worker);
                File.Copy(Path.Combine(runtime, "ScriptWorker", Path.GetFileName(worker) + ".config"), worker + ".config");
                File.Copy(typeof(WorkerEntry).Assembly.Location, tests);
                Program.Check(Directory.GetFiles(root).Length == 3 && !File.Exists(Path.Combine(root, "Jint.dll")) &&
                    !File.Exists(Path.Combine(root, "Acornima.dll")), "worker entry fixture has no interpreter dependencies");
                domain = AppDomain.CreateDomain("Lea worker entry", null, new AppDomainSetup
                { ApplicationBase = root, ConfigurationFile = worker + ".config" });
                var probe = (WorkerEntryProbe)domain.CreateInstanceFromAndUnwrap(tests, typeof(WorkerEntryProbe).FullName);
                foreach (var args in new[] { new string[0], new[] { "--other" }, new[] { "--pipe", "extra" } })
                {
                    var report = probe.Invoke(worker, args);
                    var name = "argc=" + args.Length;
                    Program.Check(report[0] == "2", "worker rejects invalid arguments before interpreter binding: " + name + "; " + report[0]);
                    Program.Check(report[1] == "" && report[2] == "", "invalid worker arguments do not enter startup or write output: " + name);
                    Program.Check(report[3] == "" && report[4] == "", "invalid worker arguments never load the interpreter: " + name);
                }
                var assembly = Assembly.LoadFrom(Path.Combine(runtime, "ScriptWorker", Path.GetFileName(worker)));
                var run = assembly.EntryPoint.DeclaringType.GetMethod("Run", BindingFlags.Static | BindingFlags.NonPublic);
                Program.Check(run != null && (run.GetMethodImplementationFlags() & MethodImplAttributes.NoInlining) != 0,
                    "worker interpreter has a non-inlined startup boundary");
                var frames = assembly.GetTypes().SelectMany(type => type.GetMethods(
                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic))
                    .SingleOrDefault(method => method.Name.StartsWith("<Run>g__Frames|", StringComparison.Ordinal));
                Program.Check(frames != null && frames.ReturnType == typeof(int) &&
                    (frames.GetMethodImplementationFlags() & MethodImplAttributes.NoInlining) != 0,
                    "worker page processing has a non-inlined post-handshake boundary");
                Program.Check(assembly.EntryPoint.GetMethodBody().LocalVariables.All(variable =>
                    variable.LocalType.Namespace == "System" || variable.LocalType.Namespace == "System.Diagnostics"),
                    "worker entry locals do not require interpreter or closure types");
                using (var process = Process.Start(new ProcessStartInfo(worker, "--pipe")
                {
                    WorkingDirectory = root, UseShellExecute = false, CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardInput = true,
                    RedirectStandardOutput = true, RedirectStandardError = true
                }))
                {
                    process.StandardInput.Close();
                    var output = process.StandardOutput.ReadToEndAsync();
                    var error = process.StandardError.ReadToEndAsync();
                    if (!process.WaitForExit(15000))
                    {
                        process.Kill(); process.WaitForExit();
                        throw new InvalidOperationException("Missing-dependency worker did not exit.");
                    }
                    var stderr = error.GetAwaiter().GetResult();
                    Program.Check(process.ExitCode == 4 && output.GetAwaiter().GetResult() == "" &&
                        stderr.Contains("FileNotFoundException:") && stderr.Contains("Jint"),
                        "worker handles a missing interpreter without an unhandled entry exception: " + stderr);
                    Program.Check(stderr.IndexOf("startup entry", StringComparison.Ordinal) >= 0 &&
                        stderr.IndexOf("startup guards", StringComparison.Ordinal) > stderr.IndexOf("startup entry", StringComparison.Ordinal) &&
                        stderr.IndexOf("FileNotFoundException:", StringComparison.Ordinal) > stderr.IndexOf("startup guards", StringComparison.Ordinal) &&
                        !stderr.Contains("startup init"), "worker applies guards before interpreter binding can fail");
                }
                passed = true;
            }
            finally
            {
                if (domain != null) AppDomain.Unload(domain);
                if (passed)
                {
                    // Remove only this probe's copied files, after the isolated domain releases them.
                    foreach (var file in files) File.Delete(file);
                    Directory.Delete(root, false);
                }
                else Console.WriteLine("Retained worker entry fixture: " + root);
            }
        }
    }

    public sealed class WorkerEntryProbe : MarshalByRefObject
    {
        public string[] Invoke(string worker, string[] args)
        {
            var before = InterpreterAssemblies();
            var oldOutput = Console.Out;
            var oldError = Console.Error;
            using (var output = new StringWriter())
            using (var error = new StringWriter())
            {
                Console.SetOut(output);
                Console.SetError(error);
                string result;
                try
                {
                    try { result = Assembly.LoadFrom(worker).EntryPoint.Invoke(null, new object[] { args }).ToString(); }
                    catch (TargetInvocationException failure)
                    { result = failure.InnerException.GetType().FullName + ": " + failure.InnerException.Message; }
                    catch (Exception failure) { result = failure.GetType().FullName + ": " + failure.Message; }
                    return new[] { result, output.ToString(), error.ToString(), before, InterpreterAssemblies() };
                }
                finally { Console.SetOut(oldOutput); Console.SetError(oldError); }
            }
        }

        private static string InterpreterAssemblies()
        {
            return String.Join(",", AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetName().Name)
                .Where(name => name == "Jint" || name == "Acornima").OrderBy(name => name));
        }
    }
}
