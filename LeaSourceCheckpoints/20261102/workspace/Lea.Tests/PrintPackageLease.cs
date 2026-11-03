using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace Lea.PrintProbe
{
    [ComVisible(true), StructLayout(LayoutKind.Sequential)]
    public struct PackageStatus
    {
        public uint JobId;
        public int CurrentDocument, CurrentPage, CurrentPageTotal, Completion, PackageStatusCode;
    }
    [ComVisible(true), Guid("ed90c8ad-5c34-4d05-a1ec-0e8a9b3ad7af"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IStatusEvent
    {
        [DispId(1)] void PackageStatusUpdated([In] ref PackageStatus status);
    }
    [ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    public sealed class StatusSink : IStatusEvent
    {
        private readonly object gate = new object();
        private readonly List<PackageStatus> history = new List<PackageStatus>();
        private PackageStatus terminal;
        private bool complete, overflow;
        public void PackageStatusUpdated(ref PackageStatus status)
        {
            lock (gate)
            {
                if (history.Count < 64) history.Add(status); else overflow = true;
                if (!complete && status.Completion >= 1 && status.Completion <= 3)
                { terminal = status; complete = true; }
            }
        }
        public bool Complete { get { lock (gate) return complete; } }
        public bool Overflow { get { lock (gate) return overflow; } }
        public PackageStatus Terminal { get { lock (gate) return terminal; } }
        public PackageStatus[] History { get { lock (gate) return history.ToArray(); } }
    }
    [ComImport, Guid("d2959bf7-b31b-4a3d-9600-712eb1335ba4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPackageFactory
    {
        void CreateDocumentPackageTargetForPrintJob([MarshalAs(UnmanagedType.LPWStr)] string printer,
            [MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.Interface)] IStream output,
            [MarshalAs(UnmanagedType.Interface)] IStream ticket, out IPackageTarget target);
    }
    [ComImport, Guid("1b8efec4-3019-4c27-964e-367202156906"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPackageTarget
    {
        void GetPackageTargetTypes(out uint count, out IntPtr types);
        void GetPackageTarget([In] ref Guid type, [In] ref Guid iid, out IntPtr target);
        void Cancel();
    }
    [ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    public sealed class BoundedStream : IStream, IDisposable
    {
        private readonly object gate = new object();
        private readonly MemoryStream stream;
        private readonly bool writable;
        private const int Limit = 4 * 1024 * 1024;
        public BoundedStream(byte[] input)
        {
            if (input != null && input.Length > Limit) throw new ArgumentOutOfRangeException("input");
            writable = input == null;
            stream = input == null ? new MemoryStream() : new MemoryStream((byte[])input.Clone(), false);
        }
        public long Length { get { lock (gate) return stream.Length; } }
        public byte[] Snapshot() { lock (gate) { if (!stream.CanRead) throw new ObjectDisposedException("BoundedStream"); return stream.ToArray(); } }
        public void Read(byte[] bytes, int count, IntPtr read)
        { lock (gate) { if (read != IntPtr.Zero) Marshal.WriteInt32(read, 0); int n = stream.Read(bytes, 0, count); if (read != IntPtr.Zero) Marshal.WriteInt32(read, n); } }
        public void Write(byte[] bytes, int count, IntPtr written)
        {
            lock (gate)
            {
                if (written != IntPtr.Zero) Marshal.WriteInt32(written, 0);
                if (!writable || count < 0 || count > bytes.Length || count > Limit - stream.Position) throw new IOException("Bounded output stream required");
                stream.Write(bytes, 0, count); if (written != IntPtr.Zero) Marshal.WriteInt32(written, count);
            }
        }
        public void Seek(long offset, int origin, IntPtr position)
        {
            lock (gate)
            {
                long basis = origin == 0 ? 0 : origin == 1 ? stream.Position : origin == 2 ? stream.Length : -1;
                if (basis < 0 || offset > Limit - basis || offset < -basis) throw new IOException("Invalid bounded seek");
                stream.Position = basis + offset; if (position != IntPtr.Zero) Marshal.WriteInt64(position, stream.Position);
            }
        }
        public void SetSize(long length)
        { lock (gate) { if (!writable || length < 0 || length > Limit) throw new IOException("Invalid bounded length"); stream.SetLength(length); } }
        public void CopyTo(IStream target, long count, IntPtr read, IntPtr written)
        {
            if (read != IntPtr.Zero) Marshal.WriteInt64(read, 0);
            if (written != IntPtr.Zero) Marshal.WriteInt64(written, 0);
            if (target == null) throw new ArgumentNullException("target");
            if (Object.ReferenceEquals(target, this)) throw new IOException("Self copy is unsupported");
            if (count < 0) throw new IOException("Invalid copy size");
            long totalRead = 0, totalWritten = 0;
            var buffer = new byte[16384]; IntPtr transferred = Marshal.AllocHGlobal(4);
            try
            {
                // A request larger than the stream is legal: stop at EOF, not at the requested count.
                while (totalRead < count)
                {
                    int n; lock (gate) n = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, count - totalRead));
                    if (n == 0) break;
                    totalRead += n;
                    Marshal.WriteInt32(transferred, 0); target.Write(buffer, n, transferred);
                    int accepted = Marshal.ReadInt32(transferred);
                    if (accepted < 0 || accepted > n) throw new IOException("Invalid destination write count");
                    totalWritten += accepted;
                    if (accepted != n) throw new IOException("Partial stream copy");
                }
            }
            finally
            {
                if (read != IntPtr.Zero) Marshal.WriteInt64(read, totalRead);
                if (written != IntPtr.Zero) Marshal.WriteInt64(written, totalWritten);
                Marshal.FreeHGlobal(transferred);
            }
        }
        public void Commit(int flags) { }
        public void Revert() { throw new COMException("Unsupported stream operation", unchecked((int)0x80030001)); }
        public void LockRegion(long offset, long length, int kind) { throw new COMException("Unsupported stream lock", unchecked((int)0x80030001)); }
        public void UnlockRegion(long offset, long length, int kind) { throw new COMException("Unsupported stream unlock", unchecked((int)0x80030001)); }
        public void Stat(out STATSTG stat, int flags)
        { lock (gate) stat = new STATSTG { type = 2, cbSize = stream.Length, grfMode = writable ? 2 : 0 }; }
        public void Clone(out IStream clone) { clone = null; throw new COMException("Unsupported stream clone", unchecked((int)0x80030001)); }
        public void Dispose() { lock (gate) stream.Dispose(); }
    }
    public sealed class PrinterState
    {
        public readonly string Name;
        public readonly int? LegacyMode;
        public PrinterState(string name, int? legacyMode) { Name = name; LegacyMode = legacyMode; }
    }
    public static class PrinterSafety
    {
        public static PrinterState Capture()
        {
            uint size = 0;
            bool first = GetDefaultPrinter(null, ref size);
            int error = Marshal.GetLastWin32Error();
            if (first || error != 122 || size < 2 || size > 1024) throw new System.ComponentModel.Win32Exception(error, "Bounded current-user default printer required");
            var name = new System.Text.StringBuilder((int)size);
            if (!GetDefaultPrinter(name, ref size)) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            int? mode = null;
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Windows", false))
            {
                object value = key == null ? null : key.GetValue("LegacyDefaultPrinterMode");
                if (value is int) mode = (int)value;
            }
            return new PrinterState(name.ToString(), mode);
        }
        public static void RequireManualDefault(PrinterState state)
        {
            if (state == null || String.IsNullOrWhiteSpace(state.Name) || state.LegacyMode != 1)
                throw new InvalidOperationException("Virtual submission requires known manual default-printer management; user preference is never changed automatically");
        }
        public static bool Unchanged(PrinterState before, PrinterState after)
        {
            return before != null && after != null && String.Equals(before.Name, after.Name, StringComparison.Ordinal)
                && before.LegacyMode == after.LegacyMode;
        }
        [DllImport("winspool.drv", EntryPoint = "GetDefaultPrinterW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetDefaultPrinter(System.Text.StringBuilder name, ref uint size);
    }
    public sealed class PackageLease : IDisposable
    {
        public const string FactoryClass = "348ef17d-6c81-4982-92b4-ee188a43867a";
        private readonly int thread = Thread.CurrentThread.ManagedThreadId;
        private readonly BoundedStream output = new BoundedStream(null);
        private readonly StatusSink sink = new StatusSink();
        private IPackageFactory factory;
        private IPackageTarget target;
        private IConnectionPoint connection;
        private int cookie;
        private bool advised, disposed;
        private IntPtr transferLibrary;
        private TransferDocument transfer;
        private bool submissionAttempted;
        public bool CreationAttempted { get; private set; }
        public bool CreationReturned { get; private set; }
        public bool CancellationReturned { get; private set; }
        public string Name { get; private set; }
        public bool CanDispose { get { return !CreationAttempted || (CreationReturned && target != null && sink.Complete); } }
        public bool Disposed { get { return disposed; } }
        public long OutputBytes { get { return output.Length; } }
        public StatusSink Status { get { return sink; } }
        public PackageLease()
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) throw new InvalidOperationException("Retained STA owner required");
            Name = "Lea-empty-cancel-" + Guid.NewGuid().ToString("N");
        }
        public static Dictionary<string, object> Abi()
        {
            return new Dictionary<string, object> {
                {"factory", new Guid(FactoryClass).ToString("B").ToUpperInvariant()},
                {"factoryInterface", typeof(IPackageFactory).GUID.ToString("B").ToUpperInvariant()},
                {"target", typeof(IPackageTarget).GUID.ToString("B").ToUpperInvariant()},
                {"status", typeof(IStatusEvent).GUID.ToString("B").ToUpperInvariant()},
                {"statusSize", Marshal.SizeOf<PackageStatus>()},
                {"completionOffset", Marshal.OffsetOf<PackageStatus>("Completion").ToInt32()},
                {"hresultOffset", Marshal.OffsetOf<PackageStatus>("PackageStatusCode").ToInt32()},
                {"messageSize", Marshal.SizeOf<Message>()}, {"completed", 1}, {"canceled", 2}, {"failed", 3}
            };
        }
        public void Start()
        {
            Verify(); if (CreationAttempted) throw new InvalidOperationException("One creation attempt only");
            if (transfer != null) PrinterSafety.RequireManualDefault(PrinterSafety.Capture());
            factory = (IPackageFactory)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(FactoryClass), true));
            // Caller must retain this lease BEFORE entering the native creation call.
            CreationAttempted = true;
            factory.CreateDocumentPackageTargetForPrintJob("Microsoft XPS Document Writer", Name, output, null, out target);
            CreationReturned = true;
            if (target == null) throw new InvalidOperationException("No original package target returned");
            Guid events = typeof(IStatusEvent).GUID;
            ((IConnectionPointContainer)target).FindConnectionPoint(ref events, out connection);
            connection.Advise(sink, out cookie); advised = true;
        }
        public void Cancel()
        {
            Verify(); if (target == null) throw new InvalidOperationException("Original target is unavailable; retain owner");
            target.Cancel(); CancellationReturned = true;
        }
        public void LoadTransferBridge(string path)
        {
            Verify();
            if (CreationAttempted || transferLibrary != IntPtr.Zero) throw new InvalidOperationException("Load one bridge before target creation");
            if (!Path.IsPathRooted(path) || !File.Exists(path)) throw new ArgumentException("Absolute bridge path required", "path");
            transferLibrary = LoadLibraryEx(path, IntPtr.Zero, 0x1100);
            if (transferLibrary == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            IntPtr entry = GetProcAddress(transferLibrary, "LeaWriteXps");
            if (entry == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            transfer = (TransferDocument)Marshal.GetDelegateForFunctionPointer(entry, typeof(TransferDocument));
        }
        public uint WriteXps(byte[] bytes)
        {
            Verify();
            if (target == null || transfer == null || sink.Complete || submissionAttempted) throw new InvalidOperationException("One live target submission required");
            if (bytes == null || bytes.Length == 0 || bytes.Length > 4 * 1024 * 1024) throw new ArgumentException("Bounded XPS input required", "bytes");
            submissionAttempted = true;
            IntPtr original = Marshal.GetIUnknownForObject(target);
            try
            {
                uint pages;
                int hr = transfer(original, bytes, (uint)bytes.Length, out pages);
                Marshal.ThrowExceptionForHR(hr);
                return pages;
            }
            finally { Marshal.Release(original); }
        }
        public byte[] CompletedOutput()
        {
            Verify();
            if (!sink.Complete || sink.Terminal.Completion != 1 || sink.Terminal.PackageStatusCode < 0) throw new InvalidOperationException("Successful terminal completion required");
            return output.Snapshot();
        }
        public bool PumpUntilTerminal(int milliseconds)
        {
            Verify(); if (milliseconds < 0 || milliseconds > 5000) throw new ArgumentOutOfRangeException("milliseconds");
            var watch = System.Diagnostics.Stopwatch.StartNew();
            while (!sink.Complete && watch.ElapsedMilliseconds < milliseconds)
            {
                Message message;
                for (int i = 0; i < 64 && PeekMessage(out message, IntPtr.Zero, 0, 0, 1); i++)
                { TranslateMessage(ref message); DispatchMessage(ref message); }
                Thread.Sleep(10);
            }
            return sink.Complete;
        }
        public void Dispose()
        {
            if (Thread.CurrentThread.ManagedThreadId != thread) throw new InvalidOperationException("Original STA owner required");
            if (disposed) return;
            if (!CanDispose) throw new InvalidOperationException("Retain original target until terminal notification");
            if (advised) { connection.Unadvise(cookie); advised = false; }
            if (connection != null) { Marshal.FinalReleaseComObject(connection); connection = null; }
            if (target != null) { Marshal.FinalReleaseComObject(target); target = null; }
            if (factory != null) { Marshal.FinalReleaseComObject(factory); factory = null; }
            if (transferLibrary != IntPtr.Zero)
            {
                if (!FreeLibrary(transferLibrary)) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                transferLibrary = IntPtr.Zero; transfer = null;
            }
            output.Dispose(); disposed = true;
        }
        private void Verify()
        {
            if (Thread.CurrentThread.ManagedThreadId != thread) throw new InvalidOperationException("Original STA owner required");
            if (disposed) throw new ObjectDisposedException("PackageLease");
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct Message
        { public IntPtr Window; public uint Id; public UIntPtr WParam; public IntPtr LParam; public uint Time; public int X, Y; public uint Private; }
        [DllImport("user32.dll")] private static extern bool PeekMessage(out Message message, IntPtr window, uint first, uint last, uint flags);
        [DllImport("user32.dll")] private static extern bool TranslateMessage(ref Message message);
        [DllImport("user32.dll")] private static extern IntPtr DispatchMessage(ref Message message);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int TransferDocument(IntPtr original, [In] byte[] bytes, uint length, out uint pages);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)] private static extern IntPtr GetProcAddress(IntPtr module, string name);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool FreeLibrary(IntPtr module);
    }
}
