using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

public static class PrivateClipboardStation
{
    [StructLayout(LayoutKind.Sequential)] private struct Attributes { public int Length; public IntPtr Descriptor; public int Inherit; }
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CreateWindowStationW(string name, uint flags, uint access, ref Attributes attributes);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CreateDesktopW(string name, IntPtr device, IntPtr mode, uint flags, uint access, ref Attributes attributes);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool SetProcessWindowStation(IntPtr station);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool SetThreadDesktop(IntPtr desktop);
    [DllImport("user32.dll")] private static extern IntPtr GetProcessWindowStation();
    [DllImport("user32.dll")] private static extern IntPtr GetThreadDesktop(uint thread);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern bool GetUserObjectInformationW(IntPtr handle, int index, IntPtr buffer, uint length, out uint needed);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr OpenWindowStationW(string name, bool inherit, uint access);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool CloseWindowStation(IntPtr station);
    // These are process-lifetime handles. Never reconnect to WinSta0: late OLE
    // clipboard cleanup must stay private. Parent probes object removal after exit.
    private static IntPtr station, desktop;
    public static string Name { get; private set; }
    public static bool Established { get; private set; }
    private static string ObjectName(IntPtr handle)
    {
        IntPtr buffer = Marshal.AllocHGlobal(1024);
        try { uint needed; if (!GetUserObjectInformationW(handle, 2, buffer, 1024, out needed)) throw new Win32Exception(); return Marshal.PtrToStringUni(buffer); }
        finally { Marshal.FreeHGlobal(buffer); }
    }
    public static void Initialize()
    {
        string original = ObjectName(GetProcessWindowStation());
        string sid = WindowsIdentity.GetCurrent().User.Value;
        var descriptor = new RawSecurityDescriptor("D:P(A;;GA;;;" + sid + ")(A;;GA;;;SY)");
        var bytes = new byte[descriptor.BinaryLength]; descriptor.GetBinaryForm(bytes, 0);
        IntPtr memory = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, memory, bytes.Length);
            var attributes = new Attributes { Length = Marshal.SizeOf(typeof(Attributes)), Descriptor = memory, Inherit = 0 };
            station = CreateWindowStationW("LeaClipboard-" + Guid.NewGuid().ToString("N"), 1, 0x2037f, ref attributes); // CWF_CREATE_ONLY; never reuse/elevate.
            if (station == IntPtr.Zero) throw new Win32Exception();
            Name = ObjectName(station);
            if (String.Equals(Name, original, StringComparison.OrdinalIgnoreCase) || String.Equals(Name, "WinSta0", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Clipboard station is not private");
            IntPtr flags = Marshal.AllocHGlobal(12);
            try { uint needed; if (!GetUserObjectInformationW(station, 1, flags, 12, out needed) || (Marshal.ReadInt32(flags, 8) & 1) != 0 || Marshal.ReadInt32(flags) != 0) throw new InvalidOperationException("Private station flags differ"); }
            finally { Marshal.FreeHGlobal(flags); }
            if (!SetProcessWindowStation(station)) throw new Win32Exception();
            desktop = CreateDesktopW("LeaClipboard", IntPtr.Zero, IntPtr.Zero, 0, 0x201ff, ref attributes);
            if (desktop == IntPtr.Zero || !SetThreadDesktop(desktop)) throw new Win32Exception();
            Established = true; AssertPrivate();
        }
        finally { Marshal.FreeHGlobal(memory); }
    }
    public static void AssertPrivate()
    {
        if (!Established || Name == null || !String.Equals(ObjectName(GetProcessWindowStation()), Name, StringComparison.Ordinal) ||
            !String.Equals(ObjectName(GetThreadDesktop(GetCurrentThreadId())), "LeaClipboard", StringComparison.Ordinal)) throw new InvalidOperationException("Private clipboard boundary lost");
    }
    public static int ProbeAfterExit(string name)
    {
        if (String.IsNullOrEmpty(name) || String.Equals(name, "WinSta0", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Private station name required");
        IntPtr handle = OpenWindowStationW(name, false, 0x80000000);
        if (handle == IntPtr.Zero) return Marshal.GetLastWin32Error();
        if (!CloseWindowStation(handle)) throw new Win32Exception();
        return 0;
    }
}
