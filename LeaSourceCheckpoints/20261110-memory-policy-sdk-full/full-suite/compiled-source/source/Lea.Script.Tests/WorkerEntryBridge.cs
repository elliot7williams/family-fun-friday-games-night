using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Lea.Script.Tests
{
    internal static class WorkerEntryBridge
    {
        private const string WorkerHash="F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0";
        private static string Hash(string path) { using(var f=File.OpenRead(path)) using(var s=SHA256.Create()) return BitConverter.ToString(s.ComputeHash(f)).Replace("-", ""); }
        private static string Child(string root,string name)
        {
            if(String.IsNullOrEmpty(name) || Path.GetFileName(name)!=name || name=="." || name=="..") throw new InvalidDataException("Non-leaf entry probe path.");
            var path=Path.Combine(root,name); var walk=path;
            while(walk!=null) { if((File.GetAttributes(walk)&FileAttributes.ReparsePoint)!=0) throw new InvalidDataException("Entry probe reparse point refused."); walk=Path.GetDirectoryName(walk); }
            return path;
        }
        private static string ReadBounded(Stream stream)
        {
            using(var reader=new StreamReader(stream,new UTF8Encoding(false,true),false,1024,true))
            {
                var text=new StringBuilder(); var buffer=new char[1024]; int n;
                while((n=reader.Read(buffer,0,buffer.Length))!=0) { if(text.Length+n>65536) throw new InvalidDataException("Entry probe output exceeds64Ki characters."); text.Append(buffer,0,n); }
                return text.ToString();
            }
        }
        internal static void Run(string root,string manifestHash,string selectedWorker,Action<bool,string> check)
        {
            if(check==null) throw new ArgumentNullException("check");
            root=Path.GetFullPath(root);
            var fixture=Path.Combine(Path.GetDirectoryName(root),"WorkerEntryFixture");
            var manifest=Child(root,"manifest.json");
            if(Hash(manifest)!=manifestHash || Hash(selectedWorker)!=WorkerHash) throw new InvalidDataException("Entry manifest/selected worker differs.");
            var json=new JavaScriptSerializer { MaxJsonLength=131072,RecursionLimit=32 };
            var spec=json.Deserialize<Dictionary<string,object>>(File.ReadAllText(manifest));
            var files=(Dictionary<string,object>)spec["files"];
            var fixtureFiles=(Dictionary<string,object>)spec["fixtureFiles"];
            if(files.Count!=3 || fixtureFiles.Count!=7 || !files.ContainsKey("WorkerEntryNativeHost.exe") || !fixtureFiles.ContainsKey("Lea.Script.Worker.dll") || (string)fixtureFiles["Lea.Script.Worker.dll"]!=WorkerHash) throw new InvalidDataException("Entry fixture manifest incomplete.");
            var leases=new List<FileStream>(); var pins=new Dictionary<string,string>();
            Action<string,string> lease=(path,hash)=>{leases.Add(File.Open(path,FileMode.Open,FileAccess.Read,FileShare.Read));if(Hash(path)!=hash) throw new InvalidDataException("Entry input differs.");pins.Add(path,hash);};
            try
            {
                lease(manifest,manifestHash); lease(selectedWorker,WorkerHash);
                foreach(var p in files) lease(Child(root,p.Key),(string)p.Value);
                foreach(var p in fixtureFiles) lease(Child(fixture,p.Key),(string)p.Value);
                var start=new ProcessStartInfo(Child(root,"WorkerEntryNativeHost.exe"),"\""+fixture+"\"") {UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,WorkingDirectory=root,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true};
                foreach(var variable in new[]{"LEA_SCRIPT_STARTUP_TRACE","DOTNET_STARTUP_HOOKS","DOTNET_ADDITIONAL_DEPS","DOTNET_SHARED_STORE","CORECLR_ENABLE_PROFILING","COR_ENABLE_PROFILING","COR_PROFILER","CORECLR_PROFILER","COR_PROFILER_PATH","CORECLR_PROFILER_PATH"}) start.EnvironmentVariables.Remove(variable);
                using(var process=new Process {StartInfo=start})
                {
                    if(!process.Start() || process.Handle==IntPtr.Zero) throw new InvalidOperationException("Original entry host handle unavailable.");
                    process.StandardInput.Close();
                    var output=Task.Run(()=>ReadBounded(process.StandardOutput.BaseStream));
                    var error=Task.Run(()=>ReadBounded(process.StandardError.BaseStream));
                    if(!process.WaitForExit(30000))
                    {
                        process.Kill();
                        if(!process.WaitForExit(5000)) throw new InvalidOperationException("Original entry host termination unverified; nested child audit required.");
                        throw new TimeoutException("Entry fixture host exceeded30s; nested child audit required.");
                    }
                    if(!Task.WaitAll(new Task[]{output,error},1000)) throw new IOException("Entry host streams did not retire.");
                    var report=json.Deserialize<Dictionary<string,object>>(output.GetAwaiter().GetResult());
                    if(process.ExitCode!=0 || error.GetAwaiter().GetResult()!="" || !(bool)report["passed"] || !(bool)report["childTerminal"] || !(bool)report["nativeIdentityVerified"] || !(bool)report["guardsVerified"] || !(bool)report["outputComplete"] || (bool)report["cleanupTerminatedChild"] || !(bool)report["aclRestored"] || !(bool)report["hashesUnchanged"] || (int)report["cpuRateBasisPoints"]!=2500 || (int)report["jobFlags"]!=5 || (bool)report["pageCodeEvaluated"]) throw new InvalidDataException("Confined entry fixture failed: "+output.Result);
                    var rows=((IEnumerable)report["checks"]).Cast<Dictionary<string,object>>().ToArray();
                    if(rows.Length!=15 || rows.Any(r=>!(bool)r["passed"] || String.IsNullOrEmpty((string)r["name"]))) throw new InvalidDataException("Original15 entry assertions incomplete.");
                    foreach(var p in pins) if(Hash(p.Key)!=p.Value) throw new InvalidDataException("Entry input changed during execution.");
                    foreach(var row in rows) check(true,(string)row["name"]);
                }
            }
            finally { foreach(var file in leases) file.Dispose(); }
        }
    }
}
