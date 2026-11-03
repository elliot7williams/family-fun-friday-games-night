using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

internal static class WpfTextProviderServerMain
{
    [STAThread]
    private static int Main(string[] args)
    {
        Guid nonce;
        if (args.Length != 4 || args[0] != "--server" || !Guid.TryParseExact(args[3], "N", out nonce))
            throw new ArgumentException("WPF server handshake arguments");
        var text = new TextBox { Text = "Alpha bravo minimal", IsReadOnly = true, AcceptsReturn = true };
        var window = new Window { Width = 400, Height = 180, Title = "WPF UIA baseline", Content = text };
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(25) };
        window.ContentRendered += delegate {
            if (text.Text != "Alpha bravo minimal") throw new Exception("WPF fixture text differs");
            long handle = new WindowInteropHelper(window).Handle.ToInt64();
            string payload = "{\"handle\":" + handle + ",\"pid\":" + Process.GetCurrentProcess().Id + ",\"nonce\":\"" + nonce.ToString("N") + "\"}";
            using (var file = new FileStream(args[1] + ".tmp", FileMode.CreateNew))
            using (var writer = new StreamWriter(file)) writer.Write(payload);
            File.Move(args[1] + ".tmp", args[1]);
            Console.WriteLine("WPF_UIA SERVER_READY " + nonce.ToString("N")); Console.Out.Flush();
        };
        timer.Tick += delegate {
            if (File.Exists(args[2]) && new FileInfo(args[2]).Length == 32 && File.ReadAllText(args[2]) == nonce.ToString("N"))
            { timer.Stop(); window.Close(); }
        };
        window.Closed += delegate { timer.Stop(); };
        timer.Start();
        new Application().Run(window);
        Console.WriteLine("WPF_UIA SERVER_STOPPED");
        return 0;
    }
}
