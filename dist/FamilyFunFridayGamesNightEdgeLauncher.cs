using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FamilyFunFridayGamesNightLauncher
{
    internal static class Program
    {
        private static string LogPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Family Fun Friday Games Night.log"); }
        }

        [STAThread]
        private static void Main()
        {
            LocalSiteServer server = null;

            try
            {
                string launcherDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string appDirectory = FindAppDirectory(launcherDirectory);
                string index = Path.Combine(appDirectory, "index.html");
                string extensionDirectory = FindExtensionDirectory(launcherDirectory);
                Log("Starting Edge app-mode launcher");
                Log("Using app path: " + index);
                Log("Using extension path: " + extensionDirectory);

                if (!File.Exists(index))
                {
                    MessageBox.Show("Could not find index.html. Keep this executable next to the 'Family Fun Friday Games Night' folder.", "Family Fun Friday Games Night", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string browserPath = FindBrowser();
                if (String.IsNullOrEmpty(browserPath))
                {
                    MessageBox.Show("Could not find Microsoft Edge or Google Chrome on this computer.", "Family Fun Friday Games Night", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                server = new LocalSiteServer(appDirectory);
                server.Start();
                string url = server.BaseUrl + "index.html";
                Log("Serving local app at: " + url);

                string profileDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "FamilyFunFridayGamesNightWebAppLocal");
                Directory.CreateDirectory(profileDir);

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = browserPath;
                startInfo.Arguments =
                    "--app=\"" + url + "\" " +
                    "--user-data-dir=\"" + profileDir + "\" " +
                    "--no-first-run " +
                    "--disable-features=TranslateUI";

                if (Directory.Exists(extensionDirectory))
                {
                    startInfo.Arguments += " --load-extension=\"" + extensionDirectory + "\"";
                }

                startInfo.UseShellExecute = false;
                Process browserProcess = Process.Start(startInfo);
                Log("Launched browser app mode: " + browserPath);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ServerLifetimeContext(server, browserProcess));
            }
            catch (Exception ex)
            {
                if (server != null) server.Stop();
                Log("Launcher failed: " + ex);
                MessageBox.Show("Family Fun Friday Games Night could not open. Details were written to:\n" + LogPath, "Family Fun Friday Games Night", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Log(string message)
        {
            try { File.AppendAllText(LogPath, DateTime.Now.ToString("s") + " " + message + Environment.NewLine); } catch { }
        }

        private static string FindBrowser()
        {
            string[] candidates = new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft\\Edge\\Application\\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft\\Edge\\Application\\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Edge\\Application\\msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google\\Chrome\\Application\\chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google\\Chrome\\Application\\chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\Application\\chrome.exe")
            };

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate)) return candidate;
            }

            return null;
        }

        private static string FindAppDirectory(string launcherDir)
        {
            string[] candidates = new string[]
            {
                Path.Combine(launcherDir, "Family Fun Friday Games Night"),
                Path.Combine(launcherDir, "..", "Family Fun Friday Games Night"),
                Path.Combine(Environment.CurrentDirectory, "Family Fun Friday Games Night")
            };

            foreach (string candidate in candidates)
            {
                string full = Path.GetFullPath(candidate);
                if (File.Exists(Path.Combine(full, "index.html"))) return full;
            }

            return Path.Combine(launcherDir, "Family Fun Friday Games Night");
        }

        private static string FindExtensionDirectory(string launcherDir)
        {
            string[] candidates = new string[]
            {
                Path.Combine(launcherDir, "games-night-back-extension"),
                Path.Combine(launcherDir, "..", "games-night-back-extension"),
                Path.Combine(Environment.CurrentDirectory, "games-night-back-extension")
            };

            foreach (string candidate in candidates)
            {
                string full = Path.GetFullPath(candidate);
                if (File.Exists(Path.Combine(full, "manifest.json"))) return full;
            }

            return Path.Combine(launcherDir, "games-night-back-extension");
        }
    }

    internal sealed class ServerLifetimeContext : ApplicationContext
    {
        private readonly LocalSiteServer server;
        private readonly Process browserProcess;
        private readonly System.Windows.Forms.Timer timer;

        public ServerLifetimeContext(LocalSiteServer server, Process browserProcess)
        {
            this.server = server;
            this.browserProcess = browserProcess;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 2500;
            timer.Tick += delegate
            {
                try
                {
                    if (browserProcess != null && browserProcess.HasExited)
                    {
                        Stop();
                    }
                }
                catch
                {
                }
            };
            timer.Start();
        }

        private void Stop()
        {
            timer.Stop();
            server.Stop();
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
                server.Stop();
            }
            base.Dispose(disposing);
        }
    }

    internal sealed class LocalSiteServer
    {
        private readonly string rootDirectory;
        private HttpListener listener;
        private Thread thread;
        private volatile bool running;

        public string BaseUrl { get; private set; }

        public LocalSiteServer(string rootDirectory)
        {
            this.rootDirectory = Path.GetFullPath(rootDirectory);
        }

        public void Start()
        {
            int port = FindFreePort();
            BaseUrl = "http://127.0.0.1:" + port + "/";
            listener = new HttpListener();
            listener.Prefixes.Add(BaseUrl);
            listener.Start();
            running = true;
            thread = new Thread(ListenLoop);
            thread.IsBackground = true;
            thread.Start();
        }

        public void Stop()
        {
            running = false;
            try { if (listener != null) listener.Stop(); } catch { }
            try { if (listener != null) listener.Close(); } catch { }
        }

        private void ListenLoop()
        {
            while (running)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(delegate { Serve(context); });
                }
                catch
                {
                    if (!running) return;
                }
            }
        }

        private void Serve(HttpListenerContext context)
        {
            try
            {
                string relative = Uri.UnescapeDataString(context.Request.Url.AbsolutePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (String.IsNullOrEmpty(relative)) relative = "index.html";

                string fullPath = Path.GetFullPath(Path.Combine(rootDirectory, relative));
                if (!fullPath.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase) || Directory.Exists(fullPath) || !File.Exists(fullPath))
                {
                    WriteText(context, 404, "Not found");
                    return;
                }

                byte[] bytes = File.ReadAllBytes(fullPath);
                context.Response.StatusCode = 200;
                context.Response.ContentType = GetContentType(fullPath);
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Close();
            }
            catch
            {
                try { WriteText(context, 500, "Server error"); } catch { }
            }
        }

        private static void WriteText(HttpListenerContext context, int status, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            context.Response.StatusCode = status;
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
        }

        private static int FindFreePort()
        {
            TcpListener tcp = new TcpListener(IPAddress.Loopback, 0);
            tcp.Start();
            int port = ((IPEndPoint)tcp.LocalEndpoint).Port;
            tcp.Stop();
            return port;
        }

        private static string GetContentType(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".html" || ext == ".htm") return "text/html; charset=utf-8";
            if (ext == ".css") return "text/css; charset=utf-8";
            if (ext == ".js") return "application/javascript; charset=utf-8";
            if (ext == ".json" || ext == ".webmanifest") return "application/manifest+json; charset=utf-8";
            if (ext == ".png") return "image/png";
            if (ext == ".jpg" || ext == ".jpeg") return "image/jpeg";
            if (ext == ".gif") return "image/gif";
            if (ext == ".svg") return "image/svg+xml";
            if (ext == ".ico") return "image/x-icon";
            if (ext == ".swf") return "application/x-shockwave-flash";
            if (ext == ".mp3") return "audio/mpeg";
            if (ext == ".mp4") return "video/mp4";
            return "application/octet-stream";
        }
    }
}
