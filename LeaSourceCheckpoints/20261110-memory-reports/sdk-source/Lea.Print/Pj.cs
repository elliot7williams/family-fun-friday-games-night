using System;
using System.Drawing.Printing;
using System.Threading;
using System.Windows.Forms;

namespace Lea.Print
{
    // Native selection only. A successful dialog never sends a job by itself.
    public sealed class Pj : IDisposable
    {
        private readonly int thread;
        private readonly PrintDialog dialog;
        private bool busy, accepted, disposed;

        public Pj(int maximumPages, PrinterSettings initial = null)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) throw new InvalidOperationException("STA required");
            if (maximumPages < 1 || maximumPages > Pw.MaxPages) throw new ArgumentOutOfRangeException("maximumPages");
            thread = Thread.CurrentThread.ManagedThreadId;
            var settings = initial == null ? new PrinterSettings() : (PrinterSettings)initial.Clone();
            settings.MinimumPage = 1; settings.MaximumPage = maximumPages;
            settings.FromPage = 1; settings.ToPage = maximumPages; settings.PrintRange = PrintRange.AllPages;
            dialog = new PrintDialog
            {
                PrinterSettings = settings, UseEXDialog = false,
                AllowSomePages = true, AllowSelection = false, AllowCurrentPage = false,
                AllowPrintToFile = false, ShowHelp = false
            };
        }
        public PrinterSettings St
        {
            get
            {
                Verify();
                if (busy || !accepted) throw new InvalidOperationException("No accepted printer selection");
                return (PrinterSettings)dialog.PrinterSettings.Clone();
            }
        }
        public bool Sh(Form owner)
        {
            Verify();
            if (busy) throw new InvalidOperationException("Printer selection is already open");
            if (owner == null) throw new ArgumentNullException("owner");
            if (owner.IsDisposed || owner.Disposing || !owner.IsHandleCreated || owner.InvokeRequired || !owner.Visible || !owner.Enabled)
                throw new InvalidOperationException("Live enabled owner on this UI thread required");
            accepted = false; busy = true;
            try
            {
                accepted = dialog.ShowDialog(owner) == DialogResult.OK;
                return accepted;
            }
            finally { busy = false; }
        }
        public Pk Cp(Po options)
        { return Pk.Cp(St, options); }
        public void Dispose()
        {
            if (Thread.CurrentThread.ManagedThreadId != thread) throw new InvalidOperationException("Owning UI thread required");
            if (disposed) return;
            if (busy) throw new InvalidOperationException("Close the printer dialog before disposal");
            disposed = true; accepted = false; dialog.Dispose();
        }
        private void Verify()
        {
            if (Thread.CurrentThread.ManagedThreadId != thread) throw new InvalidOperationException("Owning UI thread required");
            if (disposed) throw new ObjectDisposedException("Pj");
        }
    }
}
