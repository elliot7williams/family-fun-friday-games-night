using System;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Printing;
using System.Printing.Interop;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Documents;
using Microsoft.Win32.SafeHandles;

namespace Lea.Print
{
    // Prepared selection only; the queue's user/default settings are never committed.
    public sealed class Pk : IDisposable
    {
        private readonly int thread = Thread.CurrentThread.ManagedThreadId;
        private PrintServer server;
        private PrintQueue queue;
        private PrintTicket ticket;
        private Pg geometry;
        private string name;
        private int first, last;
        private bool all, disposed, attempted;
        private Pk() { }

        internal static Pk Cp(PrinterSettings settings, Po options)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) throw new InvalidOperationException("STA required");
            if (settings == null) throw new ArgumentNullException("settings");
            if (options == null) throw new ArgumentNullException("options");
            if (options.SelectionOnly) throw new NotSupportedException("Document selection printing is not available");
            if (settings.PrintToFile) throw new NotSupportedException("Print-to-file requires a separate destination policy");
            if (settings.PrintRange != System.Drawing.Printing.PrintRange.AllPages && settings.PrintRange != System.Drawing.Printing.PrintRange.SomePages)
                throw new NotSupportedException("Only all pages or an explicit range is supported");
            if (!settings.IsValid || settings.Copies < 1) throw new InvalidOperationException("Valid selected printer and copies required");
            var result = new Pk();
            try
            {
                result.name = settings.PrinterName;
                string queueName = result.name;
                if (queueName.StartsWith(@"\\", StringComparison.Ordinal))
                {
                    var uri = new Uri(queueName, UriKind.Absolute);
                    queueName = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
                    if (!uri.IsUnc || queueName.Length == 0 || queueName.IndexOfAny(new[] { '/', '\\' }) >= 0)
                        throw new ArgumentException("Invalid network printer name");
                    result.server = new PrintServer(@"\\" + uri.Host);
                }
                else result.server = new LocalPrintServer();
                result.queue = result.server.GetPrintQueue(queueName);
                PrintTicket requested;
                using (var converter = new PrintTicketConverter(result.name, 1))
                    requested = converter.ConvertDevModeToPrintTicket(DevMode(settings), PrintTicketScope.JobScope);
                requested.CopyCount = settings.Copies;
                requested.Collation = settings.Collate ? Collation.Collated : Collation.Uncollated;
                var baseline = result.queue.DefaultPrintTicket;
                if (baseline == null) throw new InvalidOperationException("Printer has no default ticket for validation");
                result.ticket = result.queue.MergeAndValidatePrintTicket(baseline, requested).ValidatedPrintTicket;
                if (result.ticket == null || result.ticket.CopyCount != settings.Copies)
                    throw new InvalidOperationException("Driver could not preserve the selected copy count");
                if (settings.Copies > 1 && result.ticket.Collation != requested.Collation)
                    throw new InvalidOperationException("Driver could not preserve selected collation");
                if (result.ticket.PageScalingFactor.HasValue && result.ticket.PageScalingFactor.Value != 100)
                    throw new NotSupportedException("Use Lea scaling instead of additional driver scaling");
                var orientation = result.ticket.PageOrientation;
                bool landscape = orientation == PageOrientation.Landscape || orientation == PageOrientation.ReverseLandscape;
                bool gray = options.Grayscale || result.ticket.OutputColor == OutputColor.Grayscale || result.ticket.OutputColor == OutputColor.Monochrome;
                var effective = new Po(landscape, options.MarginInches, options.ScalePercent, gray, options.Header, options.Footer, false);
                result.geometry = Pd.Cp(effective, result.queue.GetPrintCapabilities(result.ticket));
                result.all = settings.PrintRange == System.Drawing.Printing.PrintRange.AllPages;
                result.first = settings.FromPage; result.last = settings.ToPage;
                if (!result.all && (result.first < 1 || result.last < result.first || result.last > Pw.MaxPages))
                    throw new ArgumentException("Selected page range exceeds Lea bounds");
                return result;
            }
            catch { result.Dispose(); throw; }
        }
        public string Name { get { Verify(); return name; } }
        public Pg Geometry { get { Verify(); return geometry; } }
        public PrintTicket Tk { get { Verify(); return ticket.Clone(); } }
        public Pr Rg(Pp pages)
        {
            Verify();
            if (pages == null) throw new ArgumentNullException("pages");
            if (!Object.ReferenceEquals(pages.Geometry, geometry)) throw new InvalidOperationException("Capture pages using this selection's geometry");
            if (pages.HasImages) throw new InvalidOperationException("Page images are not ready for printing");
            return new Pr(pages, all ? 1 : first, all ? pages.PageCount : last);
        }
        public void Wr(Pp pages)
        {
            Pr selected = Rg(pages);
            if (attempted) throw new InvalidOperationException("Do not retry an uncertain submission; select again");
            attempted = true;
            queue.CurrentJobSettings.Description = "Firespider Internet";
            PrintQueue.CreateXpsDocumentWriter(queue).Write(selected, ticket);
        }
        public void Dispose()
        {
            if (Thread.CurrentThread.ManagedThreadId != thread) throw new InvalidOperationException("Owning UI thread required");
            if (disposed) return;
            disposed = true;
            try { if (queue != null) queue.Dispose(); }
            finally { if (server != null) server.Dispose(); }
        }
        private void Verify()
        {
            if (Thread.CurrentThread.ManagedThreadId != thread) throw new InvalidOperationException("Owning UI thread required");
            if (disposed) throw new ObjectDisposedException("Pk");
        }
        private static byte[] DevMode(PrinterSettings settings)
        {
            using (var memory = new DevModeMemory(settings.GetHdevmode()))
            {
                ulong allocated = GlobalSize(memory).ToUInt64();
                if (memory.IsInvalid || allocated < (ulong)Marshal.SizeOf(typeof(DevModeHeader)) || allocated > 1024 * 1024)
                    throw new InvalidOperationException("Invalid or oversized printer settings buffer");
                IntPtr pointer = GlobalLock(memory);
                if (pointer == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
                try
                {
                    var header = (DevModeHeader)Marshal.PtrToStructure(pointer, typeof(DevModeHeader));
                    int length = checked(header.Size + header.Extra);
                    if (header.Size < Marshal.SizeOf(typeof(DevModeHeader)) || (ulong)length > allocated)
                        throw new InvalidOperationException("Incomplete printer settings buffer");
                    var bytes = new byte[length]; Marshal.Copy(pointer, bytes, 0, length); return bytes;
                }
                finally { GlobalUnlock(memory); }
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        private struct DevModeHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)] public string DeviceName;
            public ushort SpecVersion, DriverVersion, Size, Extra;
        }
        private sealed class DevModeMemory : SafeHandleZeroOrMinusOneIsInvalid
        {
            internal DevModeMemory(IntPtr value) : base(true) { SetHandle(value); }
            protected override bool ReleaseHandle() { return GlobalFree(handle) == IntPtr.Zero; }
        }
        [DllImport("kernel32.dll", SetLastError=true)] private static extern UIntPtr GlobalSize(DevModeMemory handle);
        [DllImport("kernel32.dll", SetLastError=true)] private static extern IntPtr GlobalLock(DevModeMemory handle);
        [DllImport("kernel32.dll", SetLastError=true)] private static extern bool GlobalUnlock(DevModeMemory handle);
        [DllImport("kernel32.dll")] private static extern IntPtr GlobalFree(IntPtr handle);
    }
}
