using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

internal static class UiaComponentInventory
{
    private static void Report(string kind, string path)
    {
        string hash;
        using (var file = File.OpenRead(path))
        using (var sha = SHA256.Create()) hash = BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "");
        Console.WriteLine("UIA_COMPONENT|" + kind + "|" + path + "|" + FileVersionInfo.GetVersionInfo(path).FileVersion + "|" + hash);
    }

    internal static void Capture()
    {
        Console.WriteLine("UIA_COMPONENT_POINTER_BYTES|" + IntPtr.Size);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            if (assembly.GetName().Name.StartsWith("UIAutomation", StringComparison.Ordinal))
                Report("managed", assembly.Location);
        bool core = false;
        using (var process = Process.GetCurrentProcess())
            foreach (ProcessModule module in process.Modules)
                if (String.Equals(module.ModuleName, "UIAutomationCore.dll", StringComparison.OrdinalIgnoreCase))
                { Report("native", module.FileName); core = true; }
        if (!core) throw new Exception("Loaded UIAutomationCore inventory missing");
        Console.WriteLine("UIA_COMPONENT_INVENTORY_COMPLETE"); Console.Out.Flush();
    }
}
