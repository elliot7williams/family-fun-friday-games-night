using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;

internal static partial class MinimalTextProviderMain
{
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint process);
    private static int Server(string[] args)
    {
        Guid nonce;
        if (args.Length != 4 || !Guid.TryParseExact(args[3], "N", out nonce)) throw new ArgumentException("Server handshake arguments");
        using (var form = new Forms.Form { Width = 400, Height = 180, Text = "Independent UIA server" })
        using (var surface = new Surface { Dock = Forms.DockStyle.Fill })
        using (var timer = new Forms.Timer { Interval = 25 })
        {
            form.Controls.Add(surface);
            form.Shown += delegate {
#if COLD_LEGACY_CLIENT
                Console.WriteLine("ARRAY_METADATA bypassed for cold client comparison");
#else
#if TYPED_TEXT_ARRAYS
                ProviderArrayMetadata.Report(surface.Provider, typeof(TypedTextProviderContract));
#else
                ProviderArrayMetadata.Report(surface.Provider, typeof(System.Windows.Automation.Provider.ITextProvider));
#endif
#endif
                string payload = "{\"handle\":" + surface.Handle.ToInt64() + ",\"pid\":" + Process.GetCurrentProcess().Id + ",\"nonce\":\"" + nonce.ToString("N") + "\"}";
                using (var file = new FileStream(args[1] + ".tmp", FileMode.CreateNew))
                using (var writer = new StreamWriter(file)) writer.Write(payload);
                File.Move(args[1] + ".tmp", args[1]);
                Console.WriteLine("MINIMAL_UIA SERVER_READY " + nonce.ToString("N")); Console.Out.Flush();
            };
            timer.Tick += delegate {
                if (File.Exists(args[2]) && new FileInfo(args[2]).Length == 32 && File.ReadAllText(args[2]) == nonce.ToString("N"))
                { timer.Stop(); form.Close(); }
            };
            timer.Start(); Forms.Application.Run(form);
            Console.WriteLine("MINIMAL_UIA SERVER_STOPPED");
            return 0;
        }
    }
}
