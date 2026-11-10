using System;
using System.IO;
using System.Collections.Generic;
namespace Lea.Script.Tests {
    internal static class ModernComponents {
        internal static Dictionary<string, object> Run(string name) {
            string hash;
            switch(name) {
                case "markers": hash = "51D4ED788B02D2F958EC985E584EDB94921E928B40DE0029E05743D94BC6D418"; break;
                case "frames": hash = "C21021CFAF56EE1AA89ABF11BECCC5355E8761D9CFCB00AF9F5403AE5AD8D5CA"; break;
                case "clock": hash = "F6705C79A5C9C20D9D9CFA261AE434DDF71C52D2E1BDC5C56B620146B633AC74"; break;
                case "wire": hash = "E99D2798C465D308ADBF748C1460A5A59B13CA07B526619ABD5303723990D6E6"; break;
                case "bootstrap": hash = "CAC5DE495DB20E2C7CA3E194914F900D9E0B48CB3ED15C8E84C04004C622EF1B"; break;
                default: throw new ArgumentException("Unknown component probe.");
            }
            var root = AppDomain.CurrentDomain.BaseDirectory;
            return CoreProbeBridge.Run(Path.Combine(root, "ComponentProbes", name), hash,
                Path.Combine(root, "ScriptWorker", "Lea.Script.Worker.dll"), Program.Check);
        }
        internal static Dictionary<string, object> Bootstrap(string path = null) {
            var worker = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker", "Lea.Script.Worker.dll");
            if (path != null && !String.Equals(Path.GetFullPath(path), worker, StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(Path.GetFullPath(path), Path.ChangeExtension(worker, ".exe"), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Bootstrap probe is bound to the selected candidate, not an alternate executable.");
            return Run("bootstrap");
        }
        internal static void WorkerEntry() {
            var root = AppDomain.CurrentDomain.BaseDirectory;
            WorkerEntryBridge.Run(Path.Combine(root, "WorkerEntryProbe"), "38EEB3BD77A6CA23751CDFB6ACC944080EA2DD246FA0629635495D671B2E0B44",
                Path.Combine(root, "ScriptWorker", "Lea.Script.Worker.dll"), Program.Check);
        }
    }
}