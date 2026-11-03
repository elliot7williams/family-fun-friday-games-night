using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

internal static class DecoderRetirementMain
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; }

    private static int Main(string[] args)
    {
        var assembly = Assembly.LoadFrom(args[0]);
        var manager = assembly.GetType("Lea.Win.Di", true);
        var endpoint = assembly.GetType("Lea.Win.Rp", true);
        if (args.Length > 2 && args[2] == "--shutdown-race") return ShutdownRace(assembly, args[1]);
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var worker = manager.GetField("worker", flags);
        var drop = manager.GetMethod("Drop", flags);
        var release = endpoint.GetField("release", BindingFlags.Instance | BindingFlags.NonPublic);
        var key = manager.GetField("workerKey", flags);
        Check(worker.GetValue(null) == null, "isolated manager starts empty");
        // No process, pipe, profile or ACL is created. Inject only the cleanup callback.
        var fake = FormatterServices.GetUninitializedObject(endpoint);
        int attempts = 0;
        bool succeed = false;
        release.SetValue(fake, new Func<bool>(() => { attempts++; return succeed; }));
        worker.SetValue(null, fake); key.SetValue(null, "retained-test");
        try
        {
            for (int retry = 0; retry < 2; retry++)
            {
                bool refused = false;
                try { drop.Invoke(null, null); }
                catch (TargetInvocationException e) { refused = e.InnerException is IOException; }
                Check(refused, "pending cleanup refuses completed retirement");
                Check(Object.ReferenceEquals(worker.GetValue(null), fake), "same endpoint retained");
                Check(release.GetValue(fake) != null, "cleanup callback retained");
                Check((string)key.GetValue(null) == "retained-test", "identity retained until cleanup");
            }
            Check(attempts == 2, "retry calls original cleanup exactly once");
            succeed = true; drop.Invoke(null, null);
            Check(attempts == 3, "successful retry uses original callback");
            Check(worker.GetValue(null) == null, "successful retirement clears endpoint");
            Check(release.GetValue(fake) == null, "successful cleanup clears callback");
            Check((string)key.GetValue(null) == "", "successful retirement clears key");
            drop.Invoke(null, null);
            Check(attempts == 3, "empty retirement is idempotent");
            var failedOpen = manager.GetMethod("RetireFailedOpen", flags);
            failedOpen.Invoke(null, new object[] { null });
            Check(worker.GetValue(null) == null, "failure before endpoint creation is harmless");
            succeed = false;
            release.SetValue(fake, new Func<bool>(() => { attempts++; return succeed; }));
            bool pendingOpen = false;
            try { failedOpen.Invoke(null, new object[] { fake }); }
            catch (TargetInvocationException e) { pendingOpen = e.InnerException is IOException; }
            Check(pendingOpen, "failed open reports pending retirement");
            Check(Object.ReferenceEquals(worker.GetValue(null), fake), "failed open publishes endpoint before cleanup");
            Check((string)key.GetValue(null) == "", "failed open is never reusable by prior key");
            Check(release.GetValue(fake) != null, "failed open retains cleanup callback");
            succeed = true; drop.Invoke(null, null);
            Check(worker.GetValue(null) == null && release.GetValue(fake) == null, "failed open cleanup retry succeeds");
            release.SetValue(fake, new Func<bool>(() => { throw new IOException("injected cleanup failure"); }));
            bool cleanupThrew = false;
            try { failedOpen.Invoke(null, new object[] { fake }); }
            catch (TargetInvocationException e) { cleanupThrew = e.InnerException is IOException; }
            Check(cleanupThrew, "cleanup exception propagated");
            Check(Object.ReferenceEquals(worker.GetValue(null), fake), "throwing cleanup retains failed-open endpoint");
            Check(release.GetValue(fake) != null, "throwing cleanup retains callback");
            release.SetValue(fake, new Func<bool>(() => true));
            drop.Invoke(null, null);
            Check(worker.GetValue(null) == null, "throwing cleanup can recover on retry");
            var setupFailure = endpoint.GetNestedType("SetupFailure", BindingFlags.NonPublic);
            var cause = new IOException("injected startup failure");
            var failure = (Exception)Activator.CreateInstance(setupFailure, BindingFlags.Instance | BindingFlags.NonPublic,
                null, new object[] { cause, fake }, null);
            Check(Object.ReferenceEquals(failure.InnerException, cause), "setup failure preserves original cause");
            Check(Object.ReferenceEquals(setupFailure.GetProperty("Endpoint", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(failure, null), fake), "setup failure carries exact endpoint");
            Check(failure is InvalidOperationException, "setup failure retains public exception contract");
            var retireOpenFailure = manager.GetMethod("RetireOpenFailure", flags);
            release.SetValue(fake, new Func<bool>(() => false));
            Exception reported = null;
            try { retireOpenFailure.Invoke(null, new object[] { null, failure }); }
            catch (TargetInvocationException e) { reported = e.InnerException; }
            Check(reported != null && reported.GetType() == setupFailure, "manager preserves pending setup-failure type");
            var aggregate = reported.InnerException as AggregateException;
            Check(aggregate != null && Object.ReferenceEquals(aggregate.InnerExceptions[0], failure), "manager preserves complete original failure chain");
            Check(aggregate.InnerExceptions[1] is IOException, "manager preserves cleanup failure alongside original");
            Check(Object.ReferenceEquals(worker.GetValue(null), fake), "manager extracts retained endpoint from setup failure");
            Check(Object.ReferenceEquals(setupFailure.GetProperty("Endpoint", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(reported, null), fake), "reported recovery error carries same endpoint");
            release.SetValue(fake, new Func<bool>(() => true)); drop.Invoke(null, null);
            Check(worker.GetValue(null) == null, "manager setup-failure recovery clears endpoint");
            if (args.Length > 1) RealWorkers(endpoint, args[1]);
            var shutdown = manager.GetMethod("Shutdown", flags);
            release.SetValue(fake, new Func<bool>(() => false)); worker.SetValue(null, fake);
            Check(!(bool)shutdown.Invoke(null, null), "shutdown reports pending cleanup");
            Check(Object.ReferenceEquals(worker.GetValue(null), fake), "shutdown retains pending endpoint");
            release.SetValue(fake, new Func<bool>(() => true));
            Check((bool)shutdown.Invoke(null, null), "shutdown cleanup can retry");
            Check(worker.GetValue(null) == null, "shutdown retry clears endpoint");
            Check((bool)shutdown.Invoke(null, null), "shutdown is idempotent");
            Check((int)manager.GetField("shuttingDown", flags).GetValue(null) == 1, "shutdown prevents future launches");
            Console.WriteLine("Decoder retirement passed: " + checks);
            return 0;
        }
        finally { if (worker.GetValue(null) != null) { succeed = true; drop.Invoke(null, null); } }
    }

    private static bool HasGrant(string path, string sid)
    {
        foreach (FileSystemAccessRule rule in File.GetAccessControl(path).GetAccessRules(true, true, typeof(SecurityIdentifier)))
            if (rule.IdentityReference.Value == sid && rule.AccessControlType == AccessControlType.Allow) return true;
        return false;
    }

    private static int ShutdownRace(Assembly assembly, string path)
    {
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var manager = assembly.GetType("Lea.Win.Di", true);
        var endpoint = assembly.GetType("Lea.Win.Rp", true);
        var worker = manager.GetField("worker", flags);
        var state = manager.GetField("state", flags).GetValue(null);
        var shutdown = manager.GetMethod("Shutdown", flags);
        string before = File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All);
        using (var admitted = new ManualResetEventSlim())
        using (var continueDecode = new ManualResetEventSlim())
        using (var stateHeld = new ManualResetEventSlim())
        using (var releaseState = new ManualResetEventSlim())
        {
            Task<Image> decode = null;
            Task holder = null;
            try
            {
                byte[] png;
                using (var bitmap = new Bitmap(2, 1))
                using (var data = new MemoryStream())
                { bitmap.Save(data, System.Drawing.Imaging.ImageFormat.Png); png = data.ToArray(); }
                Action<int, bool, bool, bool, bool, bool, bool, bool, bool, bool, int, int> started =
                    (pid, job, restricted, low, suspended, aslr, mitigation, container, capabilities, acl, cpu, guards) =>
                    { admitted.Set(); if (!continueDecode.Wait(5000)) throw new TimeoutException("Decode fixture rendezvous timed out"); };
                decode = (Task<Image>)manager.GetMethod("Rd").Invoke(null, new object[] {
                    png, path, "--pipe", CancellationToken.None, (long)png.Length + 8L, 512L * 1024 * 1024,
                    new Func<long>(() => { using (var process = System.Diagnostics.Process.GetCurrentProcess()) return process.PrivateMemorySize64; }),
                    started, null, null, null, null });
                Check(admitted.Wait(5000), "real decode reaches admitted pre-request rendezvous");
                object original = worker.GetValue(null);
                Check(original != null, "admitted decode retains real endpoint");
                holder = Task.Run(() => {
                    lock (state) { stateHeld.Set(); if (!releaseState.Wait(5000)) throw new TimeoutException("State fixture rendezvous timed out"); }
                });
                Check(stateHeld.Wait(2000), "second thread owns state lock");
                Check(!(bool)shutdown.Invoke(null, null), "busy-lock shutdown reports pending");
                Check(Object.ReferenceEquals(worker.GetValue(null), original), "busy shutdown preserves exact live endpoint");
                Check((bool)endpoint.GetProperty("CleanupPending", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original, null),
                    "busy shutdown preserves real cleanup callback");
                releaseState.Set();
                Check(holder.Wait(2000), "state-lock owner retires");
                Check((bool)shutdown.Invoke(null, null), "shutdown retry retires admitted worker");
                Check(worker.GetValue(null) == null, "shutdown retry clears shared endpoint");
                continueDecode.Set();
                Exception stopped = null;
                try { using (var unexpected = decode.GetAwaiter().GetResult()) { } }
                catch (Exception error) { stopped = error; }
                Check(stopped is IOException || stopped is ObjectDisposedException || stopped is InvalidOperationException,
                    "admitted decode terminates with closed-endpoint error");
                Check(decode.IsCompleted, "admitted task completed after shutdown");
                Check(worker.GetValue(null) == null, "decode failure does not resurrect worker");
                Exception rejected = null;
                try { manager.GetMethod("Open", flags).Invoke(null, new object[] { path, "--pipe" }); }
                catch (TargetInvocationException error) { rejected = error.InnerException; }
                Check(rejected is InvalidOperationException && rejected.Message == "Image decoder is shutting down.",
                    "new launch rejected after shutdown");
                Check(File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All) == before,
                    "concurrent shutdown restores original descriptor");
                Console.WriteLine("Decoder shutdown race passed: " + checks);
                return 0;
            }
            finally
            {
                releaseState.Set(); continueDecode.Set();
                if (holder != null) holder.Wait(2000);
                shutdown.Invoke(null, null);
                if (decode != null)
                    try { if (decode.Wait(2000) && decode.Status == TaskStatus.RanToCompletion) decode.Result.Dispose(); }
                    catch (AggregateException) { }
            }
        }
    }

    private static void RealWorkers(Type endpoint, string path)
    {
        var open = endpoint.GetMethod("Open", BindingFlags.Public | BindingFlags.Static, null,
            new Type[] { typeof(string), typeof(string), typeof(string), typeof(long), typeof(int) }, null);
        var sidProperty = endpoint.GetProperty("Sid");
        var pending = endpoint.GetProperty("CleanupPending", BindingFlags.Instance | BindingFlags.NonPublic);
        var before = File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All);
        object first = null, second = null;
        try
        {
            first = open.Invoke(null, new object[] { path, "--pipe", Path.GetDirectoryName(path), 192L * 1024 * 1024, 2500 });
            second = open.Invoke(null, new object[] { path, "--pipe", Path.GetDirectoryName(path), 192L * 1024 * 1024, 2500 });
            string a = (string)sidProperty.GetValue(first, null), b = (string)sidProperty.GetValue(second, null);
            Console.WriteLine("DECODER_OWNERS first=" + a + " second=" + b);
            Check(a != b, "overlapping decoder profiles have unique identities");
            Check(HasGrant(path, a) && HasGrant(path, b), "both concurrent workers retain access");
            foreach (object item in new object[] { first, second })
            {
                foreach (string flag in new string[] { "R", "L", "C", "N", "F", "B" })
                    Check((bool)endpoint.GetProperty(flag).GetValue(item, null), "real worker isolation " + flag);
                Check((bool)pending.GetValue(item, null), "live worker owns eventual cleanup");
            }
            ((IDisposable)first).Dispose();
            Check(!(bool)pending.GetValue(first, null), "first decoder cleanup completed");
            Check(!HasGrant(path, a), "first decoder grant revoked");
            Check(HasGrant(path, b), "first retirement preserves second decoder access");
            Check(!((System.Diagnostics.Process)endpoint.GetProperty("P").GetValue(second, null)).HasExited,
                "second decoder remains live after first retirement");
            ((IDisposable)second).Dispose();
            Check(!(bool)pending.GetValue(second, null), "second decoder cleanup completed");
            Check(!HasGrant(path, b), "second decoder grant revoked");
            Check(File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All) == before,
                "real decoder retirement restores original descriptor");
            Console.WriteLine("DECODER_OWNERS acl-restored; both cleanup callbacks completed");
            string invalid = Path.Combine(Path.GetDirectoryName(path), "invalid-decoder.exe");
            using (var file = new FileStream(invalid, FileMode.CreateNew)) file.WriteByte(0);
            string invalidAcl = File.GetAccessControl(invalid).GetSecurityDescriptorSddlForm(AccessControlSections.All);
            Exception startupError = null;
            try { open.Invoke(null, new object[] { invalid, "--pipe", Path.GetDirectoryName(path), 192L * 1024 * 1024, 2500 }); }
            catch (TargetInvocationException e) { startupError = e.InnerException; }
            Check(startupError is System.ComponentModel.Win32Exception, "invalid native image fails startup without pending cleanup");
            Check(File.GetAccessControl(invalid).GetSecurityDescriptorSddlForm(AccessControlSections.All) == invalidAcl,
                "native startup failure restores original file descriptor");
            Console.WriteLine("DECODER_STARTUP invalid-image failure cleaned");
            PostCreateFailure(endpoint, path, before);
            FilesystemCleanupRetry(endpoint, path, before);
            DecodePixels(endpoint.Assembly, path, before);
        }
        finally
        {
            if (first != null) ((IDisposable)first).Dispose();
            if (second != null) ((IDisposable)second).Dispose();
        }
    }

    private static void PostCreateFailure(Type endpoint, string path, string before)
    {
        var observer = endpoint.GetField("StageObserver", BindingFlags.Static | BindingFlags.NonPublic);
        var previous = observer.GetValue(null);
        var stages = new List<string>();
        var open = endpoint.GetMethod("Open", BindingFlags.Public | BindingFlags.Static, null,
            new Type[] { typeof(string), typeof(string), typeof(string), typeof(long), typeof(int) }, null);
        Exception error = null;
        try
        {
            observer.SetValue(null, new Action<string>(stage => stages.Add(stage)));
            try { open.Invoke(null, new object[] { path, "--pipe", Path.GetDirectoryName(path), 192L * 1024 * 1024, 99 }); }
            catch (TargetInvocationException failure) { error = failure.InnerException; }
        }
        finally { observer.SetValue(null, previous); }
        Check(error is ArgumentOutOfRangeException && ((ArgumentOutOfRangeException)error).ParamName == "cpu",
            "post-create Job validation error preserved after cleanup");
        Check(stages.IndexOf("create") >= 0 && stages.IndexOf("token") > stages.IndexOf("create") &&
            stages.IndexOf("job") > stages.IndexOf("token"), "failure occurs after native creation and token verification");
        Check(!stages.Contains("resume") && !stages.Contains("returned"), "rejected child is never resumed or published");
        Check(File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All) == before,
            "post-create failure restores original descriptor");
        Console.WriteLine("DECODER_POST_CREATE stages=" + String.Join(",", stages.ToArray()) + "; acl-restored");
    }

    private static void FilesystemCleanupRetry(Type endpoint, string path, string before)
    {
        string sidecar = Path.Combine(Path.GetDirectoryName(path), "cleanup-retry.sidecar");
        using (var file = new FileStream(sidecar, FileMode.CreateNew)) file.WriteByte(42);
        string sidecarAcl = File.GetAccessControl(sidecar).GetSecurityDescriptorSddlForm(AccessControlSections.All);
        var open = endpoint.GetMethod("Open", BindingFlags.Static | BindingFlags.NonPublic);
        var pending = endpoint.GetProperty("CleanupPending", BindingFlags.Instance | BindingFlags.NonPublic);
        object worker = null;
        try
        {
            worker = open.Invoke(null, new object[] { path, "--pipe", Path.GetDirectoryName(path), 192L * 1024 * 1024, 2500,
                "Lea.Image.Decoder.Retry." + Guid.NewGuid().ToString("N"), new List<string> { path, sidecar }, true });
            string sid = (string)endpoint.GetProperty("Sid").GetValue(worker, null);
            Check(HasGrant(path, sid) && HasGrant(sidecar, sid), "multi-file worker owns both grants");
            File.Delete(sidecar);
            ((IDisposable)worker).Dispose();
            Check((bool)pending.GetValue(worker, null), "real missing-file cleanup failure retains callback");
            Check(endpoint.GetProperty("P").GetValue(worker, null) == null, "process retired before pending ACL retry");
            Check(!HasGrant(path, sid), "partial cleanup revokes existing executable grant");
            using (var file = new FileStream(sidecar, FileMode.CreateNew)) file.WriteByte(42);
            ((IDisposable)worker).Dispose();
            Check(!(bool)pending.GetValue(worker, null), "restored filesystem allows cleanup retry");
            Check(!HasGrant(sidecar, sid), "retry leaves no sidecar grant");
            Check(File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All) == before,
                "cleanup retry preserves original executable descriptor");
            Check(File.GetAccessControl(sidecar).GetSecurityDescriptorSddlForm(AccessControlSections.All) == sidecarAcl,
                "cleanup retry preserves recreated sidecar descriptor");
            Console.WriteLine("DECODER_RETRY real missing-sidecar failure retained; retry completed");
        }
        finally
        {
            if (!File.Exists(sidecar)) using (var file = new FileStream(sidecar, FileMode.CreateNew)) file.WriteByte(42);
            if (worker != null) ((IDisposable)worker).Dispose();
        }
    }

    private static void DecodePixels(Assembly assembly, string path, string originalAcl)
    {
        var manager = assembly.GetType("Lea.Win.Di", true);
        var drop = manager.GetMethod("Drop", BindingFlags.Static | BindingFlags.NonPublic);
        byte[] png;
        using (var bitmap = new Bitmap(2, 1))
        using (var data = new MemoryStream())
        {
            bitmap.SetPixel(0, 0, Color.Red); bitmap.SetPixel(1, 0, Color.Blue);
            bitmap.Save(data, System.Drawing.Imaging.ImageFormat.Png); png = data.ToArray();
        }
        try
        {
            var task = (Task<Image>)manager.GetMethod("Rd").Invoke(null, new object[] {
                png, path, "--pipe", CancellationToken.None, (long)png.Length + 8L, 512L * 1024 * 1024,
                new Func<long>(() => { using (var process = System.Diagnostics.Process.GetCurrentProcess()) return process.PrivateMemorySize64; }),
                null, null, null, null, null });
            using (var image = task.GetAwaiter().GetResult())
            using (var pixels = new Bitmap(image))
            {
                Check(pixels.Width == 2 && pixels.Height == 1, "temporary-profile decoder returns image dimensions");
                Check(pixels.GetPixel(0, 0).ToArgb() == Color.Red.ToArgb(), "real decoder returns red pixel");
                Check(pixels.GetPixel(1, 0).ToArgb() == Color.Blue.ToArgb(), "real decoder returns blue pixel");
            }
        }
        finally { drop.Invoke(null, null); }
        Check(File.GetAccessControl(path).GetSecurityDescriptorSddlForm(AccessControlSections.All) == originalAcl,
            "image decode retirement restores original descriptor");
        Console.WriteLine("DECODER_PIXELS red-blue PNG decoded; acl-restored");
    }
}
