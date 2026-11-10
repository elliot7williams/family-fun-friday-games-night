using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Lea.Engine;
using Lea.Script;

internal static class SdkScriptWorkerMain
{
    private static int checks;
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; Console.WriteLine("PASS " + name); }
    private static bool HasSid(string path, string sid)
    {
        return File.GetAccessControl(path, AccessControlSections.Access).GetAccessRules(true, true, typeof(SecurityIdentifier))
            .Cast<FileSystemAccessRule>().Any(rule => rule.IdentityReference.Value == sid);
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) return 2;
            var runtime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var files = Directory.GetFiles(runtime);
            var original = files.ToDictionary(path => path, path => File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All));
            var engine = new E(); var page = engine.Go(args[0]);
            Check(page.T == "SDK local script", "worker page has no automatic execution");
            var script = new Jp(engine, page);
            Microsoft.Win32.SafeHandles.SafeProcessHandle handle = null;
            bool retained = false;
            try
            {
                var result = script.Ex("var retained = 9; document.title='Worker '+retained; document.getElementById('result').textContent='Isolated';");
                Console.WriteLine("WORKER_START " + result.S + "; " + result.M + "; " + script.Sd);
                Check(result.Ok, "cold isolated execution");
                Check(typeof(Jp).GetField("io", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(script) == null,
                    "completed execution retains no owned parent I/O dispatcher");
                Check(page.T == "Worker 9", "worker computed title");
                Check(engine.Q(page, "#result").C[0].X == "Isolated", "worker DOM mutation");
                Check(script.Pid != 0 && script.Pid != Process.GetCurrentProcess().Id, "worker separate process");
                Check(script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15, "worker reported isolation and probe flags");
                Check(script.Tm == 2000 && script.Cp == 2500 && script.Mm == 128L * 1024 * 1024, "original worker budgets");
                Check(script.Ex("if(retained!==9) throw Error('state');").Ok, "worker persistent realm");
                Check(typeof(Jp).GetField("io", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(script) == null,
                    "later execution retains realm without idle parent I/O dispatcher");
                Check(script.Ex("if(typeof System!=='undefined'||typeof fetch!=='undefined'||typeof __lea!=='undefined'||typeof document.GetType!=='undefined')throw Error('host exposure');").Ok, "worker no CLR or network API");
                var sid = script.Sid;
                Check(!string.IsNullOrEmpty(sid), "worker session SID recorded");
                Check(files.All(path => HasSid(path, sid)), "worker exact file session grants");
                Check(!HasSid(runtime, sid) && !HasSid(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lea.Script.dll"), sid), "worker no folder or parent grant");
                bool locked = false;
                try { using (File.Open(files[0], FileMode.Open, FileAccess.Write, FileShare.ReadWrite)) { } }
                catch (IOException) { locked = true; }
                Check(locked, "worker runtime bytes locked");
                var endpoint = typeof(Jp).GetField("worker", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(script);
                var process = (Process)endpoint.GetType().GetProperty("P").GetValue(endpoint, null);
                handle = process.SafeHandle; handle.DangerousAddRef(ref retained);
                var nativeHandle = handle.DangerousGetHandle();
                Check(retained && nativeHandle != IntPtr.Zero && WaitForSingleObject(nativeHandle, 0) == 258, "original worker handle retained alive");
                Check(script.Ex(new string(' ', 16385)).S == "code", "worker source size bound");
                Exception wrongThread = null;
                var thread = new Thread(() => { try { script.Ex("1;"); } catch (Exception error) { wrongThread = error; } });
                thread.Start(); Check(thread.Join(2000) && wrongThread is InvalidOperationException, "worker thread affinity");
                script.St();
                Check(WaitForSingleObject(nativeHandle, 5000) == 0, "original handle observes stopped worker exit");
                Check(script.Tk().S == "cancel" && script.Pid == 0, "stopped timer drains endpoint and clears process");
                Check(files.All(path => !HasSid(path, sid)), "worker stop removes session grants");
                Check(script.Rf().S == "cancel" && script.Pid == 0, "stopped frames cannot launch worker");
                Check(!script.Pc, "worker retirement is not pending");
            }
            finally { script.Dispose(); if (retained) handle.DangerousRelease(); }
            bool disposed = false;
            try { script.Ex("1;"); } catch (ObjectDisposedException) { disposed = true; }
            Check(disposed, "disposed worker session rejected");
            Check(files.All(path => original[path] == File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All)), "worker ACL exact restoration");
            Console.WriteLine("PASS SDK WORKER " + checks); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
