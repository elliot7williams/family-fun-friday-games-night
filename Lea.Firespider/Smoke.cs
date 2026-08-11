using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Lea.Win;

namespace Lea.Firespider
{
    internal static class Smoke
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                var root = args.Length > 0 ? args[0] : AppDomain.CurrentDomain.BaseDirectory;
                var assembly = Assembly.LoadFrom(Path.Combine(root, "Firespider Internet 19.exe"));
                var type = assembly.GetType("Firespider_Internet.j", true);
                using (var form = (Form)Activator.CreateInstance(type, true))
                {
                    form.CreateControl();
                    var init = type.GetMethod("qleaInit", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(init != null, "Lea adapter initialization");
                    init.Invoke(form, new object[] { form, EventArgs.Empty });

                    var file = Member<ToolStripMenuItem>(form, "fl");
                    var found = false;
                    foreach (ToolStripItem item in file.DropDownItems)
                        if (item.Text == "New Lea Tab") found = true;
                    Check(found, "New Lea Tab menu");

                    var open = type.GetMethod("qleaNewClick", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(open != null, "Lea tab command");
                    open.Invoke(form, new object[] { form, EventArgs.Empty });

                    var tabs = Member<TabControl>(form, "k");
                    var host = tabs.SelectedTab == null || tabs.SelectedTab.Controls.Count == 0 ? null : tabs.SelectedTab.Controls[0] as V;

                    Check(host != null, "Lea view in selected tab");
                    Check(host.At != null && host.T == "Lea in Firespider", "bundled Lea start page");
                    var timer = Member<Timer>(form, "t");
                    var tick = typeof(Timer).GetMethod("OnTick", BindingFlags.Instance | BindingFlags.NonPublic);
                    Check(tick != null, "Firespider timer tick");
                    tick.Invoke(timer, new object[] { EventArgs.Empty });
                    var start = Path.Combine(root, "Lea", "start.html");
                    host.Go(start);
                    Check(host.CanBk, "Lea history before host Back");
                    Member<ToolStripItem>(form, "b").PerformClick();
                    Check(host.CanFw, "Firespider Back routed to Lea");
                }

                Console.WriteLine("Firespider Lea smoke passed: menu, tab, timer, start page, and host navigation.");
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("Firespider Lea smoke failed: " + error);
                return 1;
            }
        }

        private static T Member<T>(object target, string name) where T : class
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var property = target.GetType().GetProperty(name, flags);
            var value = property == null ? null : property.GetValue(target, null);
            if (value == null)
            {
                var field = target.GetType().GetField(name, flags);
                value = field == null ? null : field.GetValue(target);
            }
            var result = value as T;
            if (result == null) throw new InvalidOperationException("Missing host member: " + name);
            return result;
        }

        private static void Check(bool value, string name)
        {
            if (!value) throw new InvalidOperationException(name);
        }

    }
}
