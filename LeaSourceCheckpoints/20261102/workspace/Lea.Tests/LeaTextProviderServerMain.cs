using System;
using System.Diagnostics;
using System.IO;
using Lea.Win;
using Forms = System.Windows.Forms;

internal static class LeaTextProviderServerMain
{
    [STAThread]
    private static int Main(string[] args)
    {
        Guid nonce;
        if (args.Length != 4 || args[0] != "--server" || !Guid.TryParseExact(args[3], "N", out nonce)) throw new ArgumentException("Server handshake arguments");
        string fixture = Path.Combine(Path.GetDirectoryName(args[1]), "lea-pair.html");
        using (var file = new FileStream(fixture, FileMode.CreateNew))
        using (var writer = new StreamWriter(file)) writer.Write("<!doctype html><html><body><p>Alpha bravo minimal</p></body></html>");
        using (var form = new Forms.Form { Width = 600, Height = 300, Text = "Exact Lea UIA server" })
        using (var view = new V { Dock = Forms.DockStyle.Fill })
        using (var timer = new Forms.Timer { Interval = 25 })
        {
            bool ready = false;
            form.Controls.Add(view); form.Shown += delegate { view.Go(fixture); };
            timer.Tick += delegate {
                if (!ready && !view.Busy && view.At != null)
                {
                    if (view.Le != null) throw new Exception("Lea fixture navigation failed");
                    view.Refresh();
                    if (view.Dtx() != "Alpha bravo minimal") throw new Exception("Lea fixture document text differs: " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(view.Dtx())));
                    view.Sel(0, 5);
                    if (view.Sds() != "Alpha") throw new Exception("Lea fixture selection differs");
                    string payload = "{\"handle\":" + view.Handle.ToInt64() + ",\"pid\":" + Process.GetCurrentProcess().Id + ",\"nonce\":\"" + nonce.ToString("N") + "\"}";
                    using (var file = new FileStream(args[1] + ".tmp", FileMode.CreateNew))
                    using (var writer = new StreamWriter(file)) writer.Write(payload);
                    File.Move(args[1] + ".tmp", args[1]); ready = true;
                    Console.WriteLine("LEA_UIA SERVER_READY " + nonce.ToString("N")); Console.Out.Flush();
                }
                if (File.Exists(args[2]) && new FileInfo(args[2]).Length == 32 && File.ReadAllText(args[2]) == nonce.ToString("N"))
                { timer.Stop(); form.Close(); }
            };
            timer.Start(); Forms.Application.Run(form);
            Console.WriteLine("LEA_UIA SERVER_STOPPED"); return 0;
        }
    }
}
