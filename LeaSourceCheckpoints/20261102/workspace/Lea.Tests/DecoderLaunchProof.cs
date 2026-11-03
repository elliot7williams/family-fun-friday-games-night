using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

internal static class DecoderLaunchProof
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new InvalidOperationException(name); checks++; Console.WriteLine("PASS " + name); }
    private static int Main(string[] args)
    {
        try
        {
            Check((int)typeof(DecoderOwnerProof).GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { args }) == 0, "all prior 71 decoder controls retained");
            var assembly = Assembly.LoadFrom(args[0]);
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var launcher = assembly.GetType("Lea.Win.Rp", true);
            var observer = launcher.GetField("StageObserver", flags);
            var observe = launcher.GetMethod("ObserveStage", flags);
            Check(observer.IsDefined(typeof(ThreadStaticAttribute), false), "launch observer is thread-local");
            try
            {
                observer.SetValue(null, new Action<string>(stage => { throw new ApplicationException("observer control"); }));
                observe.Invoke(null, new object[] { "sid" });
                Check(true, "launch observer exception is isolated");
                string seen = "";
                var callback = new Action<string>(stage => seen = stage);
                observer.SetValue(null, callback);
                observe.Invoke(null, new object[] { "acl" });
                Check(seen == "acl", "launch observer receives same-thread stage");
                bool isolated = false;
                var work = Task.Factory.StartNew(() => {
                    isolated = observer.GetValue(null) == null;
                    observe.Invoke(null, new object[] { "create" });
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                if (!((IAsyncResult)work).AsyncWaitHandle.WaitOne(5000)) throw new TimeoutException("Observer isolation control.");
                work.GetAwaiter().GetResult();
                Check(isolated && seen == "acl", "other thread cannot invoke caller launch observer");
                Check(Object.ReferenceEquals(observer.GetValue(null), callback), "caller retains exact observer identity");
            }
            finally { observer.SetValue(null, null); }
            var decoder = assembly.GetType("Lea.Win.Di", true);
            var active = decoder.GetField("activeOwner", flags);
            var mark = decoder.GetMethod("MarkOwner", flags);
            var snapshot = decoder.GetMethod("OwnerSnapshot", flags);
            var ownerType = decoder.GetNestedType("DecoderOwner", BindingFlags.NonPublic);
            try
            {
                var owner = Activator.CreateInstance(ownerType, BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new object[] { "--pipe --delay=6000", CancellationToken.None }, null);
                active.SetValue(null, owner);
                mark.Invoke(null, new object[] { "prep" });
                string trace = (string)ownerType.GetField("Trace", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                Check(trace.StartsWith("prep=") && trace.EndsWith(";"), "owner stores launch stage with elapsed time");
                mark.Invoke(null, new object[] { "token" });
                trace = (string)ownerType.GetField("Trace", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                Check(trace.Contains("token=") && !trace.Contains("guards="), "parent token stage does not imply helper handshake guards");
                for (int n = 0; n < 1000; n++) mark.Invoke(null, new object[] { "create" });
                mark.Invoke(null, new object[] { "returned" });
                trace = (string)ownerType.GetField("Trace", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                Check(trace.Length <= 180 && trace.StartsWith("prep="), "launch stage trace retains bounded prefix");
                string report = (string)snapshot.Invoke(null, null);
                Check(report.Contains(":delayed:returned:"), "latest phase remains live after trace fills");
                Check(report.Length < 320, "owner stage snapshot remains bounded");
            }
            finally { active.SetValue(null, null); }
            Check((string)snapshot.Invoke(null, null) == "; owner=none", "launch control restores idle owner");
            Console.WriteLine("Decoder launch: checks=" + checks + "; failures=0; no native worker launched");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
