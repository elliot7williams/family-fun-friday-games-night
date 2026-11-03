using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Lea.Engine;
using Lea.Script;
using Microsoft.Win32.SafeHandles;

internal static class SdkSessionIoBenchmarkMain
{
    private static int checks;
    private static readonly FieldInfo QueueField = typeof(Jp).GetField("io", BindingFlags.Instance | BindingFlags.NonPublic);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2 || (args[1] != "retain" && args[1] != "retire")) return 2;
            var retain = args[1] == "retain";
            var runtime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var files = Directory.GetFiles(runtime);
            var acl = files.ToDictionary(path => path, path => File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All));
            var engine = new E(); var page = engine.Go(args[0]);
            var script = new Jp(engine, page);
            var samples = new long[32]; long cold;
            SafeProcessHandle handle = null; bool retained = false;
            try
            {
                var start = Stopwatch.GetTimestamp();
                var result = script.Ex("var count=0;");
                cold = Stopwatch.GetTimestamp() - start;
                Check(result.Ok, "first execution failed: " + result.S + "; " + result.M);
                Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15 && script.Tm == 2000 && script.Cp == 2500 && script.Mm == 134217728,
                    "original isolation and resource limits");
                var queue = QueueField.GetValue(script);
                Check(retain ? queue != null : queue == null, "initial queue policy differs");
                var endpoint = typeof(Jp).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(script);
                var process = (Process)endpoint.GetType().GetProperty("P").GetValue(endpoint, null);
                handle = process.SafeHandle; handle.DangerousAddRef(ref retained);
                if (!retained || WaitForSingleObject(handle.DangerousGetHandle(), 0) != 258) throw new Exception("Original live worker handle required");
                for (var i = 0; i < samples.Length; i++)
                {
                    start = Stopwatch.GetTimestamp();
                    result = script.Ex("count++;document.title=String(count);");
                    samples[i] = Stopwatch.GetTimestamp() - start;
                    var current = QueueField.GetValue(script);
                    Check(result.Ok && page.T == (i + 1).ToString(CultureInfo.InvariantCulture) && (retain ? Object.ReferenceEquals(queue, current) : current == null),
                        "measured request failed or queue policy changed: " + i + "; " + result.S + "; " + result.M);
                }
                script.Dispose();
                Check(WaitForSingleObject(handle.DangerousGetHandle(), 5000) == 0 && !script.Pc, "original worker retirement unverified");
            }
            finally { script.Dispose(); if (retained) handle.DangerousRelease(); }
            Check(files.All(path => File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All) == acl[path]), "worker ACLs differ");
            Console.WriteLine("BENCHMARK " + args[1] + " " + Stopwatch.Frequency + " " + samples.Length);
            Console.WriteLine("FIRST " + cold);
            for (var i = 0; i < samples.Length; i++) Console.WriteLine("SAMPLE " + i + " " + samples[i]);
            Console.WriteLine("PASS SDK IO BENCHMARK " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
