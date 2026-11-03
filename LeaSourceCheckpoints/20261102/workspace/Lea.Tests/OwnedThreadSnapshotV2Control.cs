using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Lea.Build;

internal static class OwnedThreadSnapshotV2Control
{
    private static int checks;
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }
    private static readonly MethodInfo parse = typeof(OwnedThreadSnapshotV2).GetMethod("Parse", BindingFlags.Static | BindingFlags.NonPublic);
    private static OwnedThreadSnapshotV2.ThreadData[] Parse(IntPtr buffer, int used, uint pid)
    { return (OwnedThreadSnapshotV2.ThreadData[])parse.Invoke(null, new object[] { buffer, used, pid }); }
    private static void Rejected(Action action, string message)
    {
        try { action(); }
        catch (TargetInvocationException error) { if (error.InnerException is InvalidDataException || error.InnerException is OverflowException) { checks++; return; } throw; }
        catch (ArgumentException) { checks++; return; }
        catch (InvalidDataException) { checks++; return; }
        throw new Exception("Accepted " + message);
    }
    private static int Main()
    {
        try
        {
            Check(OwnedThreadSnapshotV2.LayoutSupported(), "layout");
            const int unrelatedSpan = 256 + 4097 * 80, targetSpan = 256 + 80;
            int length = unrelatedSpan + targetSpan;
            IntPtr buffer = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.Copy(new byte[length], 0, buffer, length);
                Marshal.WriteInt32(buffer, 0, unrelatedSpan); Marshal.WriteInt32(buffer, 4, 4097); Marshal.WriteInt64(buffer, 80, 7);
                Marshal.WriteInt32(buffer, unrelatedSpan + 4, 1); Marshal.WriteInt64(buffer, unrelatedSpan + 80, 9);
                Marshal.WriteInt64(buffer, unrelatedSpan + 256 + 16, 1);
                Marshal.WriteInt64(buffer, unrelatedSpan + 256 + 40, 9); Marshal.WriteInt64(buffer, unrelatedSpan + 256 + 48, 11);
                var result = Parse(buffer, length, 9);
                Check(result.Length == 1 && result[0].Id == 11, "large unrelated record must not block target");
                Rejected(() => Parse(buffer, length, 7), "oversized target");
                Marshal.WriteInt32(buffer, 4, 4098); Rejected(() => Parse(buffer, length, 9), "unrelated thread region exceeds its span");
                Marshal.WriteInt32(buffer, 4, 4097); Marshal.WriteInt32(buffer, 0, unrelatedSpan - 1);
                Rejected(() => Parse(buffer, length, 9), "misaligned next offset");
                Marshal.WriteInt32(buffer, 0, unrelatedSpan);
                Rejected(() => Parse(buffer, length - 1, 9), "truncated target thread");
                Rejected(() => Parse(buffer, 255, 9), "truncated header");
                Marshal.WriteInt64(buffer, unrelatedSpan + 256 + 40, 8); Rejected(() => Parse(buffer, length, 9), "wrong thread owner");
                Marshal.WriteInt64(buffer, unrelatedSpan + 256 + 40, 9); Marshal.WriteInt32(buffer, unrelatedSpan + 256 + 68, 10);
                Rejected(() => Parse(buffer, length, 9), "invalid thread state");
            }
            finally { Marshal.FreeHGlobal(buffer); }
            using (var process = Process.GetCurrentProcess())
            {
                long birth, exit, kernel, user;
                if (!GetProcessTimes(process.Handle, out birth, out exit, out kernel, out user)) throw new Exception("process times");
                var live = OwnedThreadSnapshotV2.Read(process.Handle, (uint)process.Id, birth);
                Check(live.OriginalProcessHandleVerified && live.Threads.Length > 0, "live original-handle snapshot");
                Rejected(() => OwnedThreadSnapshotV2.Read(process.Handle, (uint)process.Id, birth + 1), "wrong creation time");
                Rejected(() => OwnedThreadSnapshotV2.Read(process.Handle, (uint)process.Id + 1, birth), "wrong process identity");
                Rejected(() => OwnedThreadSnapshotV2.Read(IntPtr.Zero, (uint)process.Id, birth), "null handle");
            }
            Console.WriteLine("PASS THREAD SNAPSHOT V2 " + checks);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetProcessTimes(IntPtr handle, out long birth, out long exit, out long kernel, out long user);
}
