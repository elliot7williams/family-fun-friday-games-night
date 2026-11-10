using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.Win32.SafeHandles;

internal static class WorkerEntryNativeHost
{
    [StructLayout(LayoutKind.Sequential)] private struct CpuPolicy { internal uint Flags, Rate; }
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool QueryInformationJobObject(IntPtr job, int kind, out CpuPolicy policy, uint size, IntPtr returned);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern uint GetProcessId(IntPtr process);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool GetExitCodeProcess(IntPtr process, out uint code);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool GetProcessTimes(IntPtr process, out long created, out long exited, out long kernel, out long user);
    private static string Hash(string path) { using(var f=File.OpenRead(path)) using(var s=SHA256.Create()) return BitConverter.ToString(s.ComputeHash(f)).Replace("-", ""); }
    private static string ReadBounded(Stream input)
    {
        using(var reader=new StreamReader(input,new UTF8Encoding(false,true),false,1024,true))
        {
            var result=new StringBuilder(); var buffer=new char[1024]; int n;
            while((n=reader.Read(buffer,0,buffer.Length))!=0) { if(result.Length+n>65536) throw new InvalidDataException("Entry report exceeds64Ki characters."); result.Append(buffer,0,n); }
            return result.ToString();
        }
    }
    private static object Property(object instance,string name) { return instance.GetType().GetProperty(name).GetValue(instance); }
    private static int Main(string[] args)
    {
        var json=new JavaScriptSerializer { MaxJsonLength=131072, RecursionLimit=32 };
        var leases=new List<FileStream>(); var before=new Dictionary<string,string>();
        var checks=new List<Dictionary<string,object>>(); var hashes=new Dictionary<string,string>();
        object endpoint=null; SafeProcessHandle retained=null; bool held=false, childTerminal=false, aclRestored=false, hashesUnchanged=false, nativeIdentity=false, guards=false;
        int childPid=0; long childCreated=0; uint nativeExit=259; string error=null, childError=null, output=null; object core=null; CpuPolicy policy=new CpuPolicy();
        Task<string> outputTask=null; bool outputComplete=false, cleanupTerminatedChild=false;
        var clock=Stopwatch.StartNew();
        try
        {
            string manifest;
            using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Lea.EntryFixture.json"))
            using(var reader=new StreamReader(stream)) manifest=reader.ReadToEnd();
            var spec=json.Deserialize<Dictionary<string,object>>(manifest);
            var root=Path.GetFullPath((string)spec["directory"]);
            if(args.Length!=1 || !String.Equals(root,Path.GetFullPath(args[0]),StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unexpected fixture path.");
            var files=(Dictionary<string,object>)spec["files"];
            if(files.Count!=7 || Directory.GetFiles(root).Length!=7 || Directory.GetDirectories(root).Length!=0 || File.Exists(Path.Combine(root,"Jint.dll")) || File.Exists(Path.Combine(root,"Acornima.dll"))) throw new InvalidDataException("Interpreter-free fixture inventory differs.");
            foreach(var file in files)
            {
                if(Path.GetFileName(file.Key)!=file.Key) throw new InvalidDataException("Non-leaf fixture entry.");
                var path=Path.Combine(root,file.Key); var walk=path;
                while(walk!=null) { if((File.GetAttributes(walk)&FileAttributes.ReparsePoint)!=0) throw new InvalidDataException("Fixture reparse point refused."); walk=Path.GetDirectoryName(walk); }
                leases.Add(File.Open(path,FileMode.Open,FileAccess.Read,FileShare.Read));
                if(Hash(path)!=(string)file.Value) throw new InvalidDataException("Fixture hash differs.");
                hashes.Add(path,(string)file.Value); before.Add(path,File.GetAccessControl(path).GetSecurityDescriptorSddlForm(System.Security.AccessControl.AccessControlSections.Access|System.Security.AccessControl.AccessControlSections.Owner|System.Security.AccessControl.AccessControlSections.Group));
            }
            checks.Add(new Dictionary<string,object>{{"passed",true},{"name","worker entry fixture has no interpreter dependencies (four CoreCLR worker files plus three companion files)"}});
            var parent=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Lea.Script.dll");
            if(Hash(parent)!="406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F") throw new InvalidDataException("Confinement parent differs.");
            var rp=Assembly.LoadFrom(parent).GetType("Lea.Win.Rp",true);
            var open=rp.GetMethods(BindingFlags.Static|BindingFlags.NonPublic).Single(m=>m.Name=="Open" && m.GetParameters().Length==8);
            var profile="Lea.Script.Entry."+Guid.NewGuid().ToString("N");
            endpoint=open.Invoke(null,new object[]{Path.Combine(root,"WorkerEntryCore.exe"),"\""+Path.Combine(root,"Lea.Script.Worker.dll")+"\"",root,134217728L,2500,profile,hashes.Keys.ToList(),true});
            guards=new[]{"R","L","S","A","M","C","N","F","B"}.All(n=>(bool)Property(endpoint,n));
            if(!guards) throw new InvalidOperationException("Native confinement flags differ.");
            var job=Property(endpoint,"J"); var jf=BindingFlags.NonPublic|BindingFlags.Instance;
            var gate=job.GetType().GetField("gate",jf).GetValue(job);
            lock(gate)
            {
                var handle=(IntPtr)job.GetType().GetField("handle",jf).GetValue(job);
                if(!QueryInformationJobObject(handle,15,out policy,8,IntPtr.Zero) || policy.Flags!=5 || policy.Rate!=2500) throw new InvalidOperationException("Original25% Job cap differs.");
            }
            retained=(SafeProcessHandle)rp.GetField("processHandle",jf).GetValue(endpoint); retained.DangerousAddRef(ref held);
            var native=retained.DangerousGetHandle(); long exited,kernel,user;
            if(!GetProcessTimes(native,out childCreated,out exited,out kernel,out user)) throw new InvalidOperationException("Original child time unavailable.");
            childPid=checked((int)GetProcessId(native)); var process=(Process)Property(endpoint,"P");
            nativeIdentity=childPid!=0 && process.Id==childPid && process.StartTime.ToUniversalTime().Ticks==DateTime.FromFileTimeUtc(childCreated).Ticks;
            if(!nativeIdentity) throw new InvalidOperationException("Original child identity differs.");
            var outputStream=(Stream)Property(endpoint,"O");
            outputTask=Task.Run(()=>ReadBounded(outputStream));
            ((Stream)Property(endpoint,"I")).Dispose();
            if(!process.WaitForExit(Math.Max(0,15000-(int)clock.ElapsedMilliseconds))) throw new TimeoutException("Original15-second entry-fixture budget exceeded.");
            if(!GetExitCodeProcess(native,out nativeExit) || nativeExit==259) throw new InvalidOperationException("Original child termination unavailable.");
            childTerminal=true;
            if(!outputTask.Wait(1000)) throw new IOException("Entry report did not drain.");
            output=outputTask.GetAwaiter().GetResult(); outputComplete=true; childError=(string)Property(endpoint,"E");
            core=json.Deserialize<Dictionary<string,object>>(output);
            var report=(Dictionary<string,object>)core;
            foreach(Dictionary<string,object> check in (IEnumerable)report["checks"]) checks.Add(check);
            if(nativeExit!=0 || !(bool)report["passed"] || (string)report["workerSha256"]!="F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0" || (bool)report["pageCodeEvaluated"] || checks.Count!=15 || checks.Any(c=>!(bool)c["passed"]) || childError!="") throw new InvalidDataException("Original entry assertions failed; retain native report.");
        }
        catch(Exception failure) { error=failure.ToString(); }
        finally
        {
            try
            {
                if(held) cleanupTerminatedChild=GetExitCodeProcess(retained.DangerousGetHandle(),out nativeExit) && nativeExit==259;
                if(endpoint!=null) ((IDisposable)endpoint).Dispose();
                if(outputTask!=null && !outputComplete)
                {
                    if(!outputTask.Wait(1000)) throw new IOException("Original entry report reader did not retire.");
                    output=outputTask.GetAwaiter().GetResult(); outputComplete=true;
                }
                if(held) childTerminal=GetExitCodeProcess(retained.DangerousGetHandle(),out nativeExit) && nativeExit!=259;
                aclRestored=before.Count==7 && before.All(p=>File.GetAccessControl(p.Key).GetSecurityDescriptorSddlForm(System.Security.AccessControl.AccessControlSections.Access|System.Security.AccessControl.AccessControlSections.Owner|System.Security.AccessControl.AccessControlSections.Group)==p.Value);
                hashesUnchanged=hashes.Count==7 && hashes.All(p=>Hash(p.Key)==p.Value);
            }
            catch(Exception failure) { error=(error??"")+" CLEANUP "+failure; }
            if(held) retained.DangerousRelease();
            foreach(var lease in leases) lease.Dispose();
        }
        var passed=error==null && checks.Count==15 && guards && nativeIdentity && childTerminal && outputComplete && !cleanupTerminatedChild && aclRestored && hashesUnchanged;
        Console.WriteLine(json.Serialize(new{passed,error,checks,core,output,childError,childPid,childCreated,nativeExit,childTerminal,nativeIdentityVerified=nativeIdentity,guardsVerified=guards,jobFlags=policy.Flags,cpuRateBasisPoints=policy.Rate,outputComplete,cleanupTerminatedChild,aclRestored,hashesUnchanged,elapsedMs=clock.ElapsedMilliseconds,profileDeletionProven=false,pageCodeEvaluated=false,productionChanged=false}));
        return passed?0:1;
    }
}
