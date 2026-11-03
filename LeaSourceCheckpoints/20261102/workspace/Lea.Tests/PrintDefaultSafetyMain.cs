using System;
using System.IO;
using System.Runtime.InteropServices;
using Lea.PrintProbe;

[ComVisible(true), Guid("1b8efec4-3019-4c27-964e-367202156906"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IRejectedLeaPackageTarget
{
    [PreserveSig] int GetPackageTargetTypes(out uint count, out IntPtr types);
    [PreserveSig] int GetPackageTarget(ref Guid type, ref Guid iid, out IntPtr target);
    [PreserveSig] int Cancel();
}
[ComVisible(true), ClassInterface(ClassInterfaceType.None)]
public sealed class RejectedLeaPackageTarget : IRejectedLeaPackageTarget
{
    public int TargetCalls, TypeCalls, CancelCalls;
    public int GetPackageTargetTypes(out uint count, out IntPtr types) { TypeCalls++; count = 0; types = IntPtr.Zero; return unchecked((int)0x80070005); }
    public int GetPackageTarget(ref Guid type, ref Guid iid, out IntPtr target) { TargetCalls++; target = IntPtr.Zero; return unchecked((int)0x80070005); }
    public int Cancel() { CancelCalls++; return 0; }
}

internal static class PrintDefaultSafetyMain
{
    private static int checks;
    private static void Check(bool value) { if (!value) throw new InvalidOperationException("Check " + (checks + 1)); checks++; }
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int WriteDocument(IntPtr target, [In] byte[] bytes, uint length, IntPtr pages);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr module, string name);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr module);
    private static void BridgeFailures(string path)
    {
        IntPtr library = LoadLibraryEx(path, IntPtr.Zero, 0x1100);
        if (library == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        IntPtr pages = IntPtr.Zero, unrelated = IntPtr.Zero, rejectedPointer = IntPtr.Zero;
        try
        {
            IntPtr entry = GetProcAddress(library, "LeaWriteXps");
            if (entry == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            var write = (WriteDocument)Marshal.GetDelegateForFunctionPointer(entry, typeof(WriteDocument));
            byte[] bytes = new byte[] { 1, 2, 3 };
            const int invalid = unchecked((int)0x80070057), pointer = unchecked((int)0x80004003), noInterface = unchecked((int)0x80004002);
            pages = Marshal.AllocHGlobal(4);
            unrelated = Marshal.GetIUnknownForObject(new StatusSink());
            Check(write(IntPtr.Zero, bytes, 3, IntPtr.Zero) == pointer);
            Marshal.WriteInt32(pages, 99);
            Check(write(IntPtr.Zero, bytes, 3, pages) == invalid && Marshal.ReadInt32(pages) == 0);
            Marshal.WriteInt32(pages, 99);
            Check(write(unrelated, bytes, 3, pages) == noInterface && Marshal.ReadInt32(pages) == 0);
            Marshal.WriteInt32(pages, 99);
            Check(write(unrelated, null, 3, pages) == invalid && Marshal.ReadInt32(pages) == 0);
            Marshal.WriteInt32(pages, 99);
            Check(write(unrelated, bytes, 0, pages) == invalid && Marshal.ReadInt32(pages) == 0);
            Marshal.WriteInt32(pages, 99);
            Check(write(unrelated, bytes, 4194305, pages) == invalid && Marshal.ReadInt32(pages) == 0);
            // Managed COM fixture only: no Windows factory, spooler job or printer is created.
            var rejected = new RejectedLeaPackageTarget();
            rejectedPointer = Marshal.GetIUnknownForObject(rejected);
            Marshal.WriteInt32(pages, 99);
            Check(write(rejectedPointer, bytes, 3, pages) == unchecked((int)0x80070005) && Marshal.ReadInt32(pages) == 0);
            Check(rejected.TargetCalls == 1 && rejected.TypeCalls == 0 && rejected.CancelCalls == 0);
        }
        finally
        {
            if (unrelated != IntPtr.Zero) Marshal.Release(unrelated);
            if (rejectedPointer != IntPtr.Zero) Marshal.Release(rejectedPointer);
            if (pages != IntPtr.Zero) Marshal.FreeHGlobal(pages);
            if (!FreeLibrary(library)) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2) throw new ArgumentException("Bridge and output directory required");
            PrinterState before = PrinterSafety.Capture();
            foreach (int? mode in new int?[] { null, 0, -1, 2, Int32.MaxValue })
            {
                bool denied = false;
                try { PrinterSafety.RequireManualDefault(new PrinterState("fixture", mode)); }
                catch (InvalidOperationException) { denied = true; }
                Check(denied);
            }
            PrinterSafety.RequireManualDefault(new PrinterState("fixture", 1)); Check(true);
            Check(!PrinterSafety.Unchanged(before, new PrinterState("not-the-current-printer", before.LegacyMode)));
            Check(Marshal.SizeOf(typeof(PackageStatus)) == 24);
            using (var stream = new BoundedStream(new byte[] { 1, 2, 3 }))
            using (var destination = new BoundedStream(null))
            {
                stream.CopyTo(destination, Int64.MaxValue, IntPtr.Zero, IntPtr.Zero);
                Check(destination.Length == 3 && destination.Snapshot()[2] == 3);
            }
            using (var lease = new PackageLease())
            {
                lease.LoadTransferBridge(args[0]);
                Check(!lease.CreationAttempted && lease.CanDispose);
            }
            BridgeFailures(args[0]);
            Check(PrinterSafety.Unchanged(before, PrinterSafety.Capture()));
            string json = "{\"passed\":true,\"checks\":" + checks + ",\"printTargetCreated\":false,\"bridgeLoadUnload\":true,\"runtime\":\"" + Environment.Version + "\"}";
            using (var file = new FileStream(Path.Combine(args[1], "safety.json"), FileMode.CreateNew))
            using (var writer = new StreamWriter(file)) writer.Write(json);
            Console.WriteLine(json);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
