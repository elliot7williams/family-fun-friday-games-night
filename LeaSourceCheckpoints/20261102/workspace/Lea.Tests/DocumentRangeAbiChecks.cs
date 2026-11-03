using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Automation.Provider;
using Lea.Win;

internal static class DocumentRangeAbiChecks
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetRanges(IntPtr provider, out IntPtr ranges);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetText(IntPtr range, int maximum, out IntPtr text);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetVartype(IntPtr array, out ushort type);
    [DllImport("oleaut32.dll")] private static extern uint SafeArrayGetDim(IntPtr array);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetLBound(IntPtr array, uint dimension, out int bound);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetUBound(IntPtr array, uint dimension, out int bound);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetElement(IntPtr array, ref int index, IntPtr value);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayDestroy(IntPtr array);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetIID(IntPtr array, out Guid id);
    private static void Trace(string value) { Console.WriteLine("RANGE_ABI " + value); Console.Out.Flush(); }

    internal static int Run(V view)
    {
        object root = typeof(V).GetProperty("UiRoot", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view, null);
        IntPtr provider = Marshal.GetComInterfaceForObject(root, typeof(ITextProvider));
        IntPtr array = IntPtr.Zero;
        try
        {
            // IUnknown occupies slots 0..2; GetVisibleRanges follows GetSelection.
            var call = (GetRanges)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(provider), 4 * IntPtr.Size), typeof(GetRanges));
            Trace("call visible ranges");
            Marshal.ThrowExceptionForHR(call(provider, out array));
            if (array == IntPtr.Zero) throw new Exception("Native range array is null");
            ushort type; Marshal.ThrowExceptionForHR(SafeArrayGetVartype(array, out type));
            uint dimensions = SafeArrayGetDim(array); Trace("type=" + type + " dimensions=" + dimensions);
            Guid arrayId; int arrayIdResult = SafeArrayGetIID(array, out arrayId);
            Trace("features=" + ((ushort)Marshal.ReadInt16(array, 2)).ToString("X4") + " IID-HRESULT=" + arrayIdResult.ToString("X8") + " IID=" + arrayId);
            if (dimensions != 1 || (type != 13 && type != 9)) throw new Exception("Native range array is not an interface SAFEARRAY");
            int first, last;
            Marshal.ThrowExceptionForHR(SafeArrayGetLBound(array, 1, out first));
            Marshal.ThrowExceptionForHR(SafeArrayGetUBound(array, 1, out last));
            Trace("bounds=" + first + ".." + last);
            if ((long)last - first + 1 < 1 || (long)last - first + 1 > 2048) throw new Exception("Native range array bounds");
            IntPtr buffer = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                int index = first; Marshal.WriteIntPtr(buffer, IntPtr.Zero);
                Marshal.ThrowExceptionForHR(SafeArrayGetElement(array, ref index, buffer));
                IntPtr item = Marshal.ReadIntPtr(buffer), range = IntPtr.Zero;
                if (item == IntPtr.Zero) throw new Exception("Native first range pointer is null");
                try
                {
                    Guid id = typeof(ITextRangeProvider).GUID;
                    int result = Marshal.QueryInterface(item, ref id, out range);
                    Trace("first QueryInterface HRESULT=" + result.ToString("X8") + " nonnull=" + (range != IntPtr.Zero));
                    Trace("element-is-text-interface=" + (item == range));
                    Marshal.ThrowExceptionForHR(result);
                    if (range == IntPtr.Zero) throw new Exception("Native range interface is null");
                    var methods = typeof(ITextRangeProvider).GetMethods();
                    Array.Sort(methods, (left, right) => left.MetadataToken.CompareTo(right.MetadataToken));
                    if (methods.Length != 18 || methods[0].Name != "Clone" || methods[9].Name != "GetText") throw new Exception("Unexpected text range COM method order");
                    var read = (GetText)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(range), 12 * IntPtr.Size), typeof(GetText));
                    IntPtr value = IntPtr.Zero;
                    try
                    {
                        Trace("first native GetText slot=12");
                        Marshal.ThrowExceptionForHR(read(range, -1, out value));
                        string text = value == IntPtr.Zero ? "" : Marshal.PtrToStringBSTR(value);
                        Trace("first native GetText length=" + text.Length);
                        if (!text.StartsWith("Alpha bravo", StringComparison.Ordinal) || text.Contains("SELECTION_SECRET")) throw new Exception("Native range text payload/privacy differs");
                    }
                    finally { if (value != IntPtr.Zero) Marshal.FreeBSTR(value); }
                }
                finally { if (range != IntPtr.Zero) Marshal.Release(range); Marshal.Release(item); }
            }
            finally { Marshal.FreeHGlobal(buffer); }
            Trace("complete"); return 4;
        }
        finally
        {
            try { if (array != IntPtr.Zero) Marshal.ThrowExceptionForHR(SafeArrayDestroy(array)); }
            finally { Marshal.Release(provider); GC.KeepAlive(root); }
        }
    }
}
