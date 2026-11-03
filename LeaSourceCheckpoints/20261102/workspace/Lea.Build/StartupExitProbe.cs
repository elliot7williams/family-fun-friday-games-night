using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Lea.Engine;
using Lea.Script;
using Lea.Win;
using Microsoft.Win32.SafeHandles;

internal static class StartupExitProbe
{
    private static Jp owner;
    private static SafeProcessHandle original;
    private static int Main(string[] args)
    {
        try
        {
            var engine = new E { Jb = false };
            var page = engine.Go(args[0]);
            var title = page.T;
            var directory = Path.GetFullPath(args[1]);
            bool injected = false;
            owner = new Jp(engine, page, directory, files => {
                var profile = (string)typeof(Jp).GetField("profile", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                var endpoint = Rp.Open(files[0], "--pipe", directory, owner.Mm, owner.Cp, profile, files, true);
                // Publish ownership before any test assertion can fail.
                typeof(Jp).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, endpoint);
                if (!endpoint.C || !endpoint.N || !endpoint.R || !endpoint.L || !endpoint.F || !endpoint.M)
                    throw new InvalidOperationException("Native confinement verification failed.");
                var handle = (SafeProcessHandle)typeof(Rp).GetField("processHandle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(endpoint);
                IntPtr copy;
                if (!DuplicateHandle(GetCurrentProcess(), handle, GetCurrentProcess(), out copy, 0, false, 2))
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                original = new SafeProcessHandle(copy, true);
                if (!TerminateProcess(original, 23) || WaitForSingleObject(original, 5000) != 0)
                    throw new InvalidOperationException("Injected child exit not observed.");
                injected = true;
                if (Rp.ProcessState(original) != "exited=23") throw new InvalidOperationException("Original child state mismatch.");
                return endpoint;
            });
            var result = owner.Ex("document.title='MUST NOT EXECUTE';");
            if (!injected || result.Ok || result.S != "setup" || !owner.Sd.Contains("child=exited=23") || page.T != title)
                throw new InvalidOperationException("Unexpected failed-start outcome: " + result.S + "; " + result.M + "; " + owner.Sd);
            Console.WriteLine("PASS injected original restricted worker exit: " + result.S + "; " + owner.Sd);
            owner.Dispose();
            if (owner.Pc || WaitForSingleObject(original, 0) != 0)
                throw new InvalidOperationException("Cleanup or original process exit unverified.");
            Console.WriteLine("PASS failed startup: no page execution; original exit observed; parent cleanup complete.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally
        {
            if (owner != null) owner.Dispose();
            if (owner != null && owner.Pc) {
                Console.Error.WriteLine("Cleanup pending; retaining owner.");
                Thread.Sleep(Timeout.Infinite);
            }
            if (original != null) original.Dispose();
        }
    }

    [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool DuplicateHandle(IntPtr source, SafeProcessHandle handle, IntPtr target, out IntPtr duplicate, uint access, bool inherit, uint options);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool TerminateProcess(SafeProcessHandle handle, uint code);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern uint WaitForSingleObject(SafeProcessHandle handle, uint milliseconds);
}
