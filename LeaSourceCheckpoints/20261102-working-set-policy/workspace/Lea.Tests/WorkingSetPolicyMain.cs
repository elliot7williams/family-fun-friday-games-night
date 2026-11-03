using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using Lea.Engine;
using Lea.Script;

internal static class WorkingSetPolicyMain
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetProcessWorkingSetSizeEx(IntPtr process, out UIntPtr minimum, out UIntPtr maximum, out uint flags);

    private static object Policy(Process process)
    {
        UIntPtr minimum, maximum;
        uint flags;
        if (!GetProcessWorkingSetSizeEx(process.Handle, out minimum, out maximum, out flags))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        return new Dictionary<string, object> {
            { "pid", process.Id }, { "minimumBytes", minimum.ToUInt64() }, { "maximumBytes", maximum.ToUInt64() },
            { "flags", flags }, { "hardMinimum", (flags & 1) != 0 }, { "hardMaximum", (flags & 4) != 0 }
        };
    }

    public static int Main(string[] args)
    {
        var report = new Dictionary<string, object>();
        using (var process = Process.GetCurrentProcess())
        {
            try
            {
                if (args.Length != 1) throw new ArgumentException("One fixture required.");
                var engine = new E { Jb = false };
                var page = engine.Go(args[0]);
                var changes = page.Jc;
                report["policyBefore"] = Policy(process);
                var cpu = process.TotalProcessorTime;
                var clock = Stopwatch.StartNew();
                using (var script = new Js(engine, page))
                {
                    var result = script.Ex("if(!document.contains(document))throw Error('document containment');");
                    report["ok"] = result.Ok && result.N == 0 && page.Jc == changes;
                    report["status"] = result.S;
                    report["message"] = result.M;
                    report["elapsedMs"] = clock.ElapsedMilliseconds;
                    report["cpuMs"] = (process.TotalProcessorTime - cpu).TotalMilliseconds;
                    report["policyAfter"] = Policy(process);
                }
            }
            catch (Exception error) { report["ok"] = false; report["exception"] = error.ToString(); }
        }
        Console.WriteLine("LEA_BOOTSTRAP_COMPARISON " + new JavaScriptSerializer().Serialize(report));
        return (bool)report["ok"] ? 0 : 1;
    }
}
