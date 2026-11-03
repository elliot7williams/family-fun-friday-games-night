using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Automation;

internal static class DocumentLegacyHandleChecks
{
    [DllImport("UIAutomationCore.dll", EntryPoint = "TextPattern_GetVisibleRanges")]
    private static extern int Visible(IntPtr pattern, out IntPtr ranges);
    [DllImport("UIAutomationCore.dll", EntryPoint = "TextPattern_get_DocumentRange")]
    private static extern int Document(IntPtr pattern, out IntPtr range);
    [DllImport("UIAutomationCore.dll", EntryPoint = "UiaHTextRangeFromVariant")]
    private static extern int ConvertRange([MarshalAs(UnmanagedType.Struct)] ref object value, out IntPtr range);
    [DllImport("UIAutomationCore.dll", EntryPoint = "UiaHTextRangeFromVariant")]
    private static extern int ConvertOriginalVariant(IntPtr value, out IntPtr range);
    [DllImport("UIAutomationCore.dll", EntryPoint = "TextRange_GetText")]
    private static extern int Read(IntPtr range, int maximum, out IntPtr text);
    [DllImport("UIAutomationCore.dll", EntryPoint = "UiaTextRangeRelease")]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ReleaseRange(IntPtr range);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetVartype(IntPtr array, out ushort type);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetLBound(IntPtr array, uint dimension, out int bound);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetUBound(IntPtr array, uint dimension, out int bound);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetElement(IntPtr array, ref int index, IntPtr value);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayDestroy(IntPtr array);
    [DllImport("oleaut32.dll")] private static extern int VariantClear(IntPtr value);
    private static void Trace(string value) { Console.WriteLine("LEGACY_HANDLE " + value); Console.Out.Flush(); }

    internal static int Run(TextPattern pattern, bool originalVariant = false)
    {
        SafeHandle owner = null;
        for (Type type = pattern.GetType(); type != null && owner == null; type = type.BaseType)
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                if (field.FieldType.Name == "SafePatternHandle") owner = (SafeHandle)field.GetValue(pattern);
        if (owner == null || owner.IsInvalid) throw new Exception("Legacy pattern handle missing");
        bool held = false;
        IntPtr array = IntPtr.Zero, range = IntPtr.Zero, document = IntPtr.Zero, buffer = IntPtr.Zero, text = IntPtr.Zero;
        try
        {
            owner.DangerousAddRef(ref held);
            Trace("native document call");
            Marshal.ThrowExceptionForHR(Document(owner.DangerousGetHandle(), out document));
            Marshal.ThrowExceptionForHR(Read(document, -1, out text));
            string documentText = text == IntPtr.Zero ? "" : Marshal.PtrToStringBSTR(text);
            Trace("native document text length=" + documentText.Length);
            if (!documentText.Contains("Alpha bravo")) throw new Exception("Legacy native document payload differs");
            if (text != IntPtr.Zero) { Marshal.FreeBSTR(text); text = IntPtr.Zero; }
            Trace("native visible call");
            Marshal.ThrowExceptionForHR(Visible(owner.DangerousGetHandle(), out array));
            ushort kind; Marshal.ThrowExceptionForHR(SafeArrayGetVartype(array, out kind));
            Trace("array variant=" + kind);
            if (kind != 12) throw new Exception("Legacy native API did not return VARIANT array");
            int first, last;
            Marshal.ThrowExceptionForHR(SafeArrayGetLBound(array, 1, out first));
            Marshal.ThrowExceptionForHR(SafeArrayGetUBound(array, 1, out last));
            if (last < first || (long)last - first > 2047) throw new Exception("Legacy native array bounds");
            buffer = Marshal.AllocCoTaskMem(24);
            for (int i = 0; i < 24; i++) Marshal.WriteByte(buffer, i, 0);
            Marshal.ThrowExceptionForHR(SafeArrayGetElement(array, ref first, buffer));
            object value = Marshal.GetObjectForNativeVariant(buffer);
            Trace("element native=" + Marshal.ReadInt16(buffer) + " managed=" + (value == null ? "null" : value.GetType().FullName));
            if (originalVariant)
            {
                Trace("conversion uses original native VARIANT buffer; no object remarshal");
                Marshal.ThrowExceptionForHR(ConvertOriginalVariant(buffer, out range));
            }
            else Marshal.ThrowExceptionForHR(ConvertRange(ref value, out range));
            if (range == IntPtr.Zero) throw new Exception("Legacy native range conversion is null");
            Trace("converted nonnull; native text call");
            Marshal.ThrowExceptionForHR(Read(range, -1, out text));
            string result = text == IntPtr.Zero ? "" : Marshal.PtrToStringBSTR(text);
            Trace("native text length=" + result.Length);
            if (!result.StartsWith("Alpha bravo", StringComparison.Ordinal) || result.Contains("SELECTION_SECRET"))
                throw new Exception("Legacy native range payload/privacy differs");
            GC.KeepAlive(value);
            return 3;
        }
        finally
        {
            if (text != IntPtr.Zero) Marshal.FreeBSTR(text);
            if (range != IntPtr.Zero) ReleaseRange(range);
            if (document != IntPtr.Zero) ReleaseRange(document);
            if (buffer != IntPtr.Zero) { VariantClear(buffer); Marshal.FreeCoTaskMem(buffer); }
            if (array != IntPtr.Zero) SafeArrayDestroy(array);
            if (held) owner.DangerousRelease();
        }
    }
}
