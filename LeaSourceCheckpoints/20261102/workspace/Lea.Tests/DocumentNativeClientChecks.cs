using System;
using System.Runtime.InteropServices;

internal static class DocumentNativeClientChecks
{
    [DllImport("ole32.dll")] private static extern int CoInitializeEx(IntPtr reserved, uint flags);
    [DllImport("ole32.dll")] private static extern void CoUninitialize();
    [DllImport("ole32.dll")] private static extern int CoCreateInstance(ref Guid cls, IntPtr outer, uint context, ref Guid id, out IntPtr value);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int FromHandle(IntPtr owner, IntPtr handle, out IntPtr value);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int PatternAs(IntPtr owner, int pattern, ref Guid id, out IntPtr value);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int GetObject(IntPtr owner, out IntPtr value);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int GetLength(IntPtr owner, out int length);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int GetElement(IntPtr owner, int index, out IntPtr value);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int GetText(IntPtr owner, int maximum, out IntPtr value);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetVartype(IntPtr array, out ushort type);
    [DllImport("oleaut32.dll")] private static extern uint SafeArrayGetDim(IntPtr array);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetLBound(IntPtr array, uint dimension, out int bound);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetUBound(IntPtr array, uint dimension, out int bound);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayAccessData(IntPtr array, out IntPtr data);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayUnaccessData(IntPtr array);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayDestroy(IntPtr array);

    // Slots/IIDs follow Microsoft's win32metadata UIAutomationClient.h, not provider interfaces.
    private static T Method<T>(IntPtr owner, int slot) where T : class
    {
        if (owner == IntPtr.Zero) throw new Exception("Native UIA interface is null");
        return (T)(object)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(owner), slot * IntPtr.Size), typeof(T));
    }
    private static void Trace(string value) { Console.WriteLine("NATIVE_UIA " + value); Console.Out.Flush(); }
    private static string Read(IntPtr range)
    {
        IntPtr value = IntPtr.Zero;
        try { Marshal.ThrowExceptionForHR(Method<GetText>(range, 12)(range, -1, out value)); return value == IntPtr.Zero ? "" : Marshal.PtrToStringBSTR(value); }
        finally { if (value != IntPtr.Zero) Marshal.FreeBSTR(value); }
    }
    private static void CheckRectangles(IntPtr range)
    {
        IntPtr array = IntPtr.Zero;
        try
        {
            Marshal.ThrowExceptionForHR(Method<GetObject>(range, 10)(range, out array));
            if (array == IntPtr.Zero) throw new Exception("Native UIA rectangles are null");
            ushort type; Marshal.ThrowExceptionForHR(SafeArrayGetVartype(array, out type));
            if (type != 5 || SafeArrayGetDim(array) != 1) throw new Exception("Native UIA rectangle array shape");
            int first, last; Marshal.ThrowExceptionForHR(SafeArrayGetLBound(array, 1, out first)); Marshal.ThrowExceptionForHR(SafeArrayGetUBound(array, 1, out last));
            long length = (long)last - first + 1;
            if (length < 4 || length > 8192 || length % 4 != 0) throw new Exception("Native UIA rectangle count");
            IntPtr data; Marshal.ThrowExceptionForHR(SafeArrayAccessData(array, out data));
            try
            {
                var values = new double[(int)length]; Marshal.Copy(data, values, 0, values.Length);
                foreach (double value in values) if (Double.IsNaN(value) || Double.IsInfinity(value)) throw new Exception("Native UIA rectangle not finite");
                for (int i = 0; i < values.Length; i += 4) if (values[i + 2] <= 0 || values[i + 3] <= 0) throw new Exception("Native UIA rectangle not visible");
            }
            finally { Marshal.ThrowExceptionForHR(SafeArrayUnaccessData(array)); }
        }
        finally { if (array != IntPtr.Zero) Marshal.ThrowExceptionForHR(SafeArrayDestroy(array)); }
    }
    internal static int Run(IntPtr handle, bool selection = false, int expectedSelection = 1)
    {
        if (expectedSelection < 0 || expectedSelection > 2 || (selection && expectedSelection != 1)) throw new ArgumentOutOfRangeException("expectedSelection");
        Marshal.ThrowExceptionForHR(CoInitializeEx(IntPtr.Zero, 0));
        IntPtr automation = IntPtr.Zero, element = IntPtr.Zero, pattern = IntPtr.Zero, document = IntPtr.Zero, ranges = IntPtr.Zero;
        try
        {
            var cls = new Guid("ff48dba4-60ef-4201-aa87-54103eef594e");
            var iid = new Guid("30cbe57d-d9d0-452a-ab13-7ac5ac4825ee");
            Trace("create"); Marshal.ThrowExceptionForHR(CoCreateInstance(ref cls, IntPtr.Zero, 1, ref iid, out automation));
            Trace("element"); Marshal.ThrowExceptionForHR(Method<FromHandle>(automation, 6)(automation, handle, out element));
            iid = new Guid("32eba289-3583-42c9-9c59-3b6d9a1e9b6a");
            Marshal.ThrowExceptionForHR(Method<PatternAs>(element, 14)(element, 10014, ref iid, out pattern));
            int mode; Marshal.ThrowExceptionForHR(Method<GetLength>(pattern, 8)(pattern, out mode));
            if (mode != expectedSelection) throw new Exception("Native UIA selection mode differs from exact fixture contract");
            Trace("document"); Marshal.ThrowExceptionForHR(Method<GetObject>(pattern, 7)(pattern, out document));
            if (!Read(document).Contains("Alpha bravo")) throw new Exception("Native UIA document text differs");
            Trace("visible array"); Marshal.ThrowExceptionForHR(Method<GetObject>(pattern, 6)(pattern, out ranges));
            int length; Marshal.ThrowExceptionForHR(Method<GetLength>(ranges, 3)(ranges, out length));
            Trace("visible count=" + length);
            if (length < 1 || length > 2048) throw new Exception("Native UIA range count differs");
            for (int i = 0; i < length; i++)
            {
                IntPtr range = IntPtr.Zero;
                try
                {
                    Trace("visible text index=" + i);
                    Marshal.ThrowExceptionForHR(Method<GetElement>(ranges, 4)(ranges, i, out range));
                    string text = Read(range);
                    if (text.Length == 0 || text.Contains("SELECTION_SECRET")) throw new Exception("Native UIA visible text/privacy differs");
                    Trace("visible rectangles index=" + i); CheckRectangles(range);
                }
                finally { if (range != IntPtr.Zero) Marshal.Release(range); }
            }
            if (selection)
            {
                IntPtr selected = IntPtr.Zero, range = IntPtr.Zero;
                try
                {
                    Trace("selection"); Marshal.ThrowExceptionForHR(Method<GetObject>(pattern, 5)(pattern, out selected));
                    int count; Marshal.ThrowExceptionForHR(Method<GetLength>(selected, 3)(selected, out count));
                    if (count != 1) throw new Exception("Native UIA selected range count");
                    Marshal.ThrowExceptionForHR(Method<GetElement>(selected, 4)(selected, 0, out range));
                    if (Read(range) != "Alpha") throw new Exception("Native UIA selected text differs");
                    CheckRectangles(range);
                }
                finally { if (range != IntPtr.Zero) Marshal.Release(range); if (selected != IntPtr.Zero) Marshal.Release(selected); }
            }
            Trace("complete"); return 3 + 2 * length + (selection ? 3 : 0);
        }
        finally
        {
            try
            {
                foreach (var value in new[] { ranges, document, pattern, element, automation }) if (value != IntPtr.Zero) Marshal.Release(value);
            }
            finally { CoUninitialize(); }
        }
    }
}
