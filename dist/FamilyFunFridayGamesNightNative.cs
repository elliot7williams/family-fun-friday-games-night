using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace FamilyFunFridayGamesNightNative
{
    internal static class Program
    {
        public static string LogPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Family Fun Friday Games Night.log"); }
        }

        [STAThread]
        private static void Main()
        {
            try
            {
                Log("Starting native app");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                TryEnableModernBrowserMode();
                Application.Run(new GamesNightForm());
                Log("App closed normally");
            }
            catch (Exception ex)
            {
                Log("Native wrapper failed: " + ex);
                MessageBox.Show("Family Fun Friday Games Night could not open. Details were written to:\n" + LogPath, "Family Fun Friday Games Night", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Log(string message)
        {
            try { File.AppendAllText(LogPath, DateTime.Now.ToString("s") + " " + message + Environment.NewLine); } catch { }
        }

        private static void TryEnableModernBrowserMode()
        {
            try
            {
                string exeName = Path.GetFileName(Application.ExecutablePath);
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    if (key != null) key.SetValue(exeName, 11001, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                Log("Browser emulation registry setup failed: " + ex.Message);
            }
        }
    }

    internal sealed class GamesNightForm : Form
    {
        private readonly WebBrowser browser;
        private readonly Panel loadingPanel;
        private readonly Timer startupTimer;
        private readonly string appDirectory;
        private readonly string startPage;

        public GamesNightForm()
        {
            Text = "Family Fun Friday Games Night";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1180;
            Height = 820;
            MinimumSize = new Size(720, 480);

            appDirectory = FindAppDirectory(AppDomain.CurrentDomain.BaseDirectory);
            startPage = Path.Combine(appDirectory, "index.html");
            Program.Log("Using app path: " + startPage);

            if (!File.Exists(startPage))
            {
                MessageBox.Show("Could not find index.html. Keep this executable next to the 'Family Fun Friday Games Night' folder.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            browser = new WebBrowser();
            browser.Dock = DockStyle.Fill;
            browser.ScriptErrorsSuppressed = true;
            browser.AllowWebBrowserDrop = false;
            browser.IsWebBrowserContextMenuEnabled = false;
            browser.WebBrowserShortcutsEnabled = true;
            browser.NewWindow += Browser_NewWindow;
            browser.Navigating += delegate(object sender, WebBrowserNavigatingEventArgs e) { Program.Log("Navigating: " + e.Url); };
            browser.DocumentCompleted += Browser_DocumentCompleted;

            loadingPanel = new Panel();
            loadingPanel.Dock = DockStyle.Fill;
            loadingPanel.BackColor = Color.FromArgb(14, 22, 34);

            Label title = new Label();
            title.AutoSize = false;
            title.Dock = DockStyle.Fill;
            title.TextAlign = ContentAlignment.MiddleCenter;
            title.ForeColor = Color.White;
            title.Font = new Font(FontFamily.GenericSansSerif, 18f, FontStyle.Bold);
            title.Text = "Loading Family Fun Friday Games Night...";
            loadingPanel.Controls.Add(title);

            startupTimer = new Timer();
            startupTimer.Interval = 900;
            startupTimer.Tick += delegate
            {
                startupTimer.Stop();
                NavigateStartPage();
            };

            Controls.Add(browser);
            Controls.Add(loadingPanel);
            loadingPanel.BringToFront();

            Shown += delegate { startupTimer.Start(); };
        }

        private void NavigateStartPage()
        {
            try
            {
                browser.Navigate(new Uri(startPage));
            }
            catch (Exception ex)
            {
                Program.Log("Initial navigation failed: " + ex);
                MessageBox.Show("Family Fun Friday Games Night could not load its start page. Details were written to:\n" + Program.LogPath, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Browser_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            try
            {
                HtmlElement active = browser.Document == null ? null : browser.Document.ActiveElement;
                string href = active == null ? null : active.GetAttribute("href");
                if (!String.IsNullOrEmpty(href)) browser.Navigate(href);
            }
            catch (Exception ex)
            {
                Program.Log("NewWindow handling failed: " + ex.Message);
            }
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Program.Log("Loaded: " + e.Url);
            if (browser.ReadyState == WebBrowserReadyState.Complete)
            {
                loadingPanel.Visible = false;
            }
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
    }
}
