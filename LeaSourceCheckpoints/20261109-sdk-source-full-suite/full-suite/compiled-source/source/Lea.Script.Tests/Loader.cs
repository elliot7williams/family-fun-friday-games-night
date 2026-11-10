using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lea.Engine;
using Lea.Script;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class Loader
    {
        private static readonly string[] Names = { "clr.dll", "mscoree.dll", "clrjit.dll", "mscorjit.dll", "mscorlib.dll", "Jint.dll", "Acornima.dll" };

        internal static void Run(string fixture)
        {
            Console.WriteLine("Loader diagnostic: CLR=" + Environment.Version + "; parent64=" + Environment.Is64BitProcess);
            var runtime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var failures = 0;
            for (var sample = 0; sample < 2; sample++)
            {
                var engine = new E { Jb = false };
                var page = engine.Go(fixture);
                var profile = "Lea.Script.Loader." + Guid.NewGuid().ToString("N");
                var early = new string[0];
                string observationError = null;
                using (var script = new Jp(engine, page, runtime, files =>
                {
                    var endpoint = Rp.Open(files[0], "--pipe", runtime, 128L * 1024 * 1024, 2500, profile, files, true);
                    try
                    {
                        try { early = Modules(endpoint.P); }
                        catch (Win32Exception error) { observationError = error.ToString(); }
                        catch (InvalidOperationException error) { observationError = error.ToString(); }
                        return endpoint;
                    }
                    catch { endpoint.Dispose(); throw; }
                }))
                {
                    var result = script.Ex("1;");
                    Console.WriteLine("Loader native sample=" + sample + "; profile=" + profile + "; status=" + result.S + "; " + result.M);
                    Console.WriteLine("Loader startup: " + script.Sd);
                    foreach (var path in early) Console.WriteLine("Loader early module: " + path);
                    if (observationError != null) { failures++; Console.WriteLine("Loader observation error: " + observationError); }
                    if (!result.Ok) failures++;
                    else
                        using (var process = Process.GetProcessById(script.Pid))
                            foreach (var path in Modules(process)) Console.WriteLine("Loader ready module: " + path);
                }
            }
            var trustedEngine = new E { Jb = false };
            var trustedPage = trustedEngine.Go(fixture);
            using (var trusted = new Js(trustedEngine, trustedPage))
            {
                var result = trusted.Ex("1;");
                Console.WriteLine("Loader trusted status=" + result.S + "; " + result.M);
                if (!result.Ok) failures++;
            }
            using (var process = Process.GetCurrentProcess())
                foreach (var path in Modules(process)) Console.WriteLine("Loader parent module: " + path);
            Console.WriteLine("Loader diagnostic failures=" + failures + "; two native samples; no retries; observation is not a startup benchmark.");
            if (failures != 0) throw new InvalidOperationException("Runtime loader diagnostic did not complete every sample.");
        }

        private static string[] Modules(Process process)
        {
            return process.Modules.Cast<ProcessModule>().Where(module =>
                module.ModuleName.EndsWith(".ni.dll", StringComparison.OrdinalIgnoreCase) ||
                Names.Contains(module.ModuleName, StringComparer.OrdinalIgnoreCase))
                .Select(module => module.FileName).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        }
    }
}
