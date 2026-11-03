using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Lea.Engine;
using Lea.Script;
using Lea.Win;
using Microsoft.Win32.SafeHandles;

internal static class StartupStallProbe
{
    private sealed class WithheldHandshake : Stream
    {
        private readonly ManualResetEvent release = new ManualResetEvent(false);
        internal readonly ManualResetEvent Finished = new ManualResetEvent(false);
        internal bool Entered;
        public override int Read(byte[] buffer, int offset, int count)
        {
            Entered = true;
            try { release.WaitOne(); throw new IOException("Injected withheld handshake retired."); }
            finally { Finished.Set(); }
        }
        protected override void Dispose(bool disposing) { release.Set(); base.Dispose(disposing); }
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }
        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
    }

    private static Jp owner;
    private static SafeProcessHandle original;
    private static Stream actualOutput;
    private static WithheldHandshake blocked;
    private static int Main(string[] args)
    {
        try
        {
            var engine = new E { Jb = false };
            var page = engine.Go(args[0]);
            var title = page.T;
            var directory = Path.GetFullPath(args[1]);
            owner = new Jp(engine, page, directory, files => {
                var profile = (string)typeof(Jp).GetField("profile", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                var endpoint = Rp.Open(files[0], "--pipe", directory, owner.Mm, owner.Cp, profile, files, true);
                typeof(Jp).GetField("worker", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, endpoint);
                if (!endpoint.C || !endpoint.N || !endpoint.R || !endpoint.L || !endpoint.F || !endpoint.M)
                    throw new InvalidOperationException("Native confinement verification failed.");
                var handle = (SafeProcessHandle)typeof(Rp).GetField("processHandle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(endpoint);
                IntPtr copy;
                if (!DuplicateHandle(GetCurrentProcess(), handle, GetCurrentProcess(), out copy, 0, false, 2))
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                original = new SafeProcessHandle(copy, true);
                actualOutput = endpoint.O;
                blocked = new WithheldHandshake();
                typeof(Rp).GetProperty("O").SetValue(endpoint, blocked, null);
                return endpoint;
            });
            var result = owner.Ex("document.title='MUST NOT EXECUTE';");
            if (result.Ok || result.S != "timeout" || !owner.Sd.Contains("phase=hello; child=running") || page.T != title)
                throw new InvalidOperationException("Unexpected withheld-handshake outcome: " + result.S + "; " + result.M + "; " + owner.Sd);
            Console.WriteLine("PASS live restricted worker deadline: " + result.S + "; " + owner.Sd);
            owner.Dispose();
            if (owner.Pc || !blocked.Entered || !blocked.Finished.WaitOne(2000) || WaitForSingleObject(original, 0) != 0)
                throw new InvalidOperationException("Reader retirement, cleanup or original process exit unverified.");
            Console.WriteLine("PASS timeout cleanup: no page execution; blocked read retired; exact child exit observed; parent cleanup complete.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally
        {
            if (owner != null) owner.Dispose();
            if (blocked != null) blocked.Dispose();
            if ((owner != null && owner.Pc) || (blocked != null && blocked.Entered && !blocked.Finished.WaitOne(2000))) {
                Console.Error.WriteLine("Cleanup pending; retaining owner.");
                Thread.Sleep(Timeout.Infinite);
            }
            if (actualOutput != null) actualOutput.Dispose();
            if (original != null) original.Dispose();
        }
    }

    [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool DuplicateHandle(IntPtr source, SafeProcessHandle handle, IntPtr target, out IntPtr duplicate, uint access, bool inherit, uint options);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern uint WaitForSingleObject(SafeProcessHandle handle, uint milliseconds);
}
