using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

// Diagnostic only: same SDK IID/order, explicit element type on the two arrays.
[ComVisible(true), Guid("3589c92c-63f3-4367-99bb-ada653b77cf2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface TypedTextProviderContract
{
#if NATIVE_TEXT_ARRAYS
    [PreserveSig] int GetSelection(out System.IntPtr array);
    [PreserveSig] int GetVisibleRanges(out System.IntPtr array);
#else
    [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN, SafeArrayUserDefinedSubType = typeof(ITextRangeProvider))]
    ITextRangeProvider[] GetSelection();
    [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN, SafeArrayUserDefinedSubType = typeof(ITextRangeProvider))]
    ITextRangeProvider[] GetVisibleRanges();
#endif
    ITextRangeProvider RangeFromChild(IRawElementProviderSimple child);
    ITextRangeProvider RangeFromPoint(Point point);
    ITextRangeProvider DocumentRange { get; }
    SupportedTextSelection SupportedTextSelection { get; }
}
