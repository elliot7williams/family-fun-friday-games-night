using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Lea.Script;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class StackTarget
    {
        internal static void Run(string reportPath)
        {
            reportPath = Lea.Diagnostics.WorkerObservation.ValidateReport(reportPath);
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var names = (string[])typeof(Jp).GetField("Names", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var files = names.Select(name => Path.Combine(directory, name)).ToArray();
            var rules = files.Select(Rules).ToArray();
            using (var worker = Rp.Open(files[0], "--pipe", directory, 134217728, 2500,
                "Lea.Script.StackTarget." + Guid.NewGuid().ToString("N"), files, true))
            {
                Program.Check(worker.R && worker.L && worker.S && worker.A && worker.M && worker.C && worker.N && worker.F && worker.B,
                    "stack target: original restricted native launch protections");
                using (var parent = Process.GetCurrentProcess())
                using (var output = new FileStream(reportPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(output, new UTF8Encoding(false)))
                    writer.Write(new JavaScriptSerializer().Serialize(new { pid = worker.P.Id, parentPid = parent.Id,
                        createdUtcTicks = worker.P.StartTime.ToUniversalTime().Ticks, worker = files[0],
                        kind = "controlled-init-wait", startupProtocolSent = false, holdMs = 12000 }));
                Thread.Sleep(12000);
            }
            Program.Check(files.Select(Rules).SequenceEqual(rules), "stack target: worker file grants retire after controlled init wait");
        }

        private static string Rules(string path) => File.GetAccessControl(path, AccessControlSections.Access).GetSecurityDescriptorSddlForm(AccessControlSections.Access);
    }
}
