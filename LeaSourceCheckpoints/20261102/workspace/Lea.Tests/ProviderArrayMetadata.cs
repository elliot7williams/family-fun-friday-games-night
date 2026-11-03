using System;
using System.Runtime.InteropServices;
using System.Windows.Automation.Provider;

internal static class ProviderArrayMetadata
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int Visible(IntPtr owner, out IntPtr array);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetIID(IntPtr array, out Guid id);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetVartype(IntPtr array, out ushort type);
    [DllImport("oleaut32.dll")] private static extern uint SafeArrayGetDim(IntPtr array);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetLBound(IntPtr array, uint dimension, out int bound);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayGetElement(IntPtr array, ref int index, IntPtr value);
    [DllImport("oleaut32.dll")] private static extern int SafeArrayDestroy(IntPtr array);
    internal static void Report(object provider, Type contract)
    {
        IntPtr pointer = Marshal.GetComInterfaceForObject(provider, contract), array = IntPtr.Zero;
        try
        {
            var call = (Visible)Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(pointer), 4 * IntPtr.Size), typeof(Visible));
            Marshal.ThrowExceptionForHR(call(pointer, out array));
            if (array == IntPtr.Zero || SafeArrayGetDim(array) != 1) throw new Exception("Metadata probe array shape");
            ushort kind; Marshal.ThrowExceptionForHR(SafeArrayGetVartype(array, out kind));
            if (kind != 13 && kind != 9) throw new Exception("Metadata probe requires interface elements");
            Guid id; int result = SafeArrayGetIID(array, out id);
            int first; Marshal.ThrowExceptionForHR(SafeArrayGetLBound(array, 1, out first));
            IntPtr buffer = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                Marshal.WriteIntPtr(buffer, IntPtr.Zero);
                Marshal.ThrowExceptionForHR(SafeArrayGetElement(array, ref first, buffer));
                IntPtr item = Marshal.ReadIntPtr(buffer), typed = IntPtr.Zero;
                if (item == IntPtr.Zero) throw new Exception("Metadata probe empty element");
                try
                {
                    Guid text = typeof(ITextRangeProvider).GUID;
                    Marshal.ThrowExceptionForHR(Marshal.QueryInterface(item, ref text, out typed));
                    Console.WriteLine("ARRAY_METADATA contract=" + contract.Name + " kind=" + kind + " features=" + ((ushort)Marshal.ReadInt16(array, 2)).ToString("X4") +
                        " IID-HRESULT=" + result.ToString("X8") + " IID=" + id + " element-is-text-interface=" + (item == typed));
                    Console.Out.Flush();
                }
                finally { if (typed != IntPtr.Zero) Marshal.Release(typed); Marshal.Release(item); }
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }
        finally { if (array != IntPtr.Zero) SafeArrayDestroy(array); Marshal.Release(pointer); GC.KeepAlive(provider); }
    }
}
