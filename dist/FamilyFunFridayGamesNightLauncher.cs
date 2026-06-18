using System;
using System.Diagnostics;
using System.IO;

namespace FamilyFunFridayGamesNightLauncher
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            string launcherDir = AppDomain.CurrentDomain.BaseDirectory;
            string appDir = FindAppDirectory(launcherDir);
            string appFile = Path.Combine(appDir, "index.html");

            if (!File.Exists(appFile))
            {
                System.Windows.Forms.MessageBox.Show(
                    "Could not find the Family Fun Friday Games Night web files. Keep this launcher next to the 'Family Fun Friday Games Night' folder.",
                    "Family Fun Friday Games Night",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            string appUrl = new Uri(appFile).AbsoluteUri + "?v=" + DateTime.Now.Ticks.ToString();
            string browser = FindBrowser();

            if (!String.IsNullOrEmpty(browser))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = browser,
                    Arguments = "--app=\"" + appUrl + "\" --user-data-dir=\"" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FamilyFunFridayGamesNightWebView") + "\"",
                    UseShellExecute = false
                });
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = appFile,
                    UseShellExecute = true
                });
            }
        }

        private static string FindAppDirectory(string launcherDir)
        {
            string[] candidates = new string[]
            {
                Path.Combine(launcherDir, "Family Fun Friday Games Night"),
                Path.Combine(launcherDir, "..", "Family Fun Friday Games Night"),
                Path.Combine(launcherDir, "GamesNightApp", "Family Fun Friday Games Night"),
                Path.Combine(Environment.CurrentDirectory, "Family Fun Friday Games Night")
            };

            foreach (string candidate in candidates)
            {
                string full = Path.GetFullPath(candidate);
                if (File.Exists(Path.Combine(full, "index.html")))
                {
                    return full;
                }
            }

            return Path.Combine(launcherDir, "Family Fun Friday Games Night");
        }

        private static string FindBrowser()
        {
            string[] candidates = new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe")
            };

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate)) return candidate;
            }

            return null;
        }
    }
}
