using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation.Provider;
using Lea.Win;

internal static class DocumentRangeApartmentChecks
{
    [DllImport("ole32.dll")] private static extern int CoInitializeEx(IntPtr reserved, uint flags);
    [DllImport("ole32.dll")] private static extern void CoUninitialize();
    [DllImport("ole32.dll")] private static extern int CoMarshalInterThreadInterfaceInStream(ref Guid id, IntPtr value, out IntPtr stream);
    [DllImport("ole32.dll")] private static extern int CoGetInterfaceAndReleaseStream(IntPtr stream, ref Guid id, out IntPtr value);
    [DllImport("ole32.dll")] private static extern int CoReleaseMarshalData(IntPtr stream);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int GetText(IntPtr range, int maximum, out IntPtr text);
    private static void Trace(string value) { Console.WriteLine("RANGE_APARTMENT " + value); Console.Out.Flush(); }

    internal static int Run(V view)
    {
        var root = (ITextProvider)typeof(V).GetProperty("UiRoot", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view, null);
        var ranges = root.GetVisibleRanges();
        if (ranges.Length == 0) throw new Exception("Apartment probe needs a visible range");
        var methods = typeof(ITextRangeProvider).GetMethods();
        Array.Sort(methods, (left, right) => left.MetadataToken.CompareTo(right.MetadataToken));
        if (methods.Length != 18 || methods[0].Name != "Clone" || methods[9].Name != "GetText") throw new Exception("Unexpected COM range method order");
        Guid id = typeof(ITextRangeProvider).GUID;
        IntPtr value = Marshal.GetComInterfaceForObject(ranges[0], typeof(ITextRangeProvider)), stream = IntPtr.Zero;
        try
        {
            try { Trace("marshal UI range"); Marshal.ThrowExceptionForHR(CoMarshalInterThreadInterfaceInStream(ref id, value, out stream)); }
            finally { Marshal.Release(value); }
            var task = Task.Run(() => {
                Marshal.ThrowExceptionForHR(CoInitializeEx(IntPtr.Zero, 0));
                IntPtr range = IntPtr.Zero;
                try
                {
                    IntPtr packet = Interlocked.Exchange(ref stream, IntPtr.Zero);
                    if (packet == IntPtr.Zero) throw new Exception("Range marshal packet unavailable");
                    Trace("unmarshal worker range");
                    Marshal.ThrowExceptionForHR(CoGetInterfaceAndReleaseStream(packet, ref id, out range));
                    if (range == IntPtr.Zero) throw new Exception("Worker range is null");
                    var read = (GetText)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(range), 12 * IntPtr.Size), typeof(GetText));
                    IntPtr text = IntPtr.Zero;
                    try
                    {
                        Trace("worker native GetText"); Marshal.ThrowExceptionForHR(read(range, -1, out text));
                        string result = text == IntPtr.Zero ? "" : Marshal.PtrToStringBSTR(text);
                        Trace("worker text length=" + result.Length);
                        if (!result.StartsWith("Alpha bravo", StringComparison.Ordinal) || result.Contains("SELECTION_SECRET")) throw new Exception("Worker range text/privacy differs");
                    }
                    finally { if (text != IntPtr.Zero) Marshal.FreeBSTR(text); }
                    return 1;
                }
                finally { try { if (range != IntPtr.Zero) Marshal.Release(range); } finally { CoUninitialize(); } }
            });
            var watch = Stopwatch.StartNew();
            while (!task.IsCompleted && watch.ElapsedMilliseconds < 5000) { System.Windows.Forms.Application.DoEvents(); Thread.Sleep(1); }
            if (!task.IsCompleted) throw new TimeoutException("Cross-apartment range check exceeded five seconds");
            int checks = task.GetAwaiter().GetResult(); Trace("complete"); return checks;
        }
        finally
        {
            IntPtr pending = Interlocked.Exchange(ref stream, IntPtr.Zero);
            if (pending != IntPtr.Zero) { try { Marshal.ThrowExceptionForHR(CoReleaseMarshalData(pending)); } finally { Marshal.Release(pending); } }
            GC.KeepAlive(ranges); GC.KeepAlive(root);
        }
    }
}
