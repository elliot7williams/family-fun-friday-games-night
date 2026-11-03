using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class PermissionLifecycle
    {
        internal static void Run()
        {
            InvalidLimits();
            Retry();
            var profile = "Lea.Permission.Test." + Guid.NewGuid().ToString("N");
            var first = Path.Combine(Path.GetTempPath(), profile + ".first");
            var second = Path.Combine(Path.GetTempPath(), profile + ".second");
            IntPtr sid = IntPtr.Zero;
            var owned = false;
            try
            {
                var created = CreateAppContainerProfile(profile, profile, profile, IntPtr.Zero, 0, out sid);
                if (created != 0) throw new InvalidOperationException("Test profile creation failed: " + created);
                owned = true;
                var identity = new SecurityIdentifier(sid);
                Program.Check(true, "unique cleanup fixture profile created");
                File.WriteAllText(first, "first fixture");
                File.WriteAllText(second, "second fixture");
                var baseline = File.GetAccessControl(first, AccessControlSections.Access)
                    .GetSecurityDescriptorSddlForm(AccessControlSections.Access);
                var broad = File.GetAccessControl(first, AccessControlSections.Access);
                broad.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(first, broad);
                if (!Rp.FileAccessRule(second, identity, true)) throw new InvalidOperationException("Fixture grant failed.");
                Program.Check(!Rp.Release(profile, new[] { first, second }, identity), "failed revocation reports incomplete profile cleanup");
                var secondRules = File.GetAccessControl(second, AccessControlSections.Access)
                    .GetAccessRules(true, true, typeof(SecurityIdentifier));
                var remaining = false;
                foreach (FileSystemAccessRule rule in secondRules)
                    if (identity.Equals(rule.IdentityReference) && rule.AccessControlType == AccessControlType.Allow) remaining = true;
                Program.Check(!remaining, "cleanup continues past an unrevocable file");
                IntPtr duplicate = IntPtr.Zero;
                try
                {
                    var exists = CreateAppContainerProfile(profile, profile, profile, IntPtr.Zero, 0, out duplicate);
                    Program.Check(exists == unchecked((int)0x800700b7), "failed revocation preserves temporary profile");
                }
                finally { if (duplicate != IntPtr.Zero) FreeSid(duplicate); }
                var restored = new FileSecurity();
                restored.SetSecurityDescriptorSddlForm(baseline, AccessControlSections.Access);
                File.SetAccessControl(first, restored);
                Program.Check(Rp.Release(profile, new[] { first, second }, identity), "verified revocation allows profile deletion on retry");
            }
            finally
            {
                if (sid != IntPtr.Zero) FreeSid(sid);
                if (owned) DeleteAppContainerProfile(profile);
                if (File.Exists(first)) File.Delete(first);
                if (File.Exists(second)) File.Delete(second);
            }
        }

        private static void InvalidLimits()
        {
            foreach (var memory in new[] { 0L, -1L, Int64.MinValue })
                RejectLimit(memory, 2500, "memory");
            foreach (var cpu in new[] { Int32.MinValue, 0, 99, 10001, Int32.MaxValue })
                RejectLimit(128L * 1024 * 1024, cpu, "cpu");
            if (IntPtr.Size == 4) RejectLimit((long)UInt32.MaxValue + 1, 2500, "memory");
        }

        private static void RejectLimit(long memory, int cpu, string parameter)
        {
            try
            {
                using (Rp.Open(null, null, null, memory, cpu)) { }
                throw new InvalidOperationException("Invalid worker limit accepted.");
            }
            catch (ArgumentOutOfRangeException error)
            {
                Program.Check(error.ParamName == parameter, "invalid " + parameter + " rejected before worker setup");
            }
        }

        private static void Retry()
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var constructor = typeof(Rp).GetConstructors(flags)[0];
            using (var process = Process.GetCurrentProcess())
            {
                var endpoint = (Rp)constructor.Invoke(new object[] {
                    process, Stream.Null, Stream.Null, Stream.Null, null,
                    false, false, false, false, false
                });
                var callback = typeof(Rp).GetField("release", flags);
                var attempts = 0;
                callback.SetValue(endpoint, new Func<bool>(() => ++attempts >= 2));
                try
                {
                    endpoint.Dispose();
                    Program.Check(attempts == 0 && endpoint.CleanupPending, "live process cannot release its permission cleanup lease");
                    Program.Check(Object.ReferenceEquals(endpoint.P, process), "unconfirmed process retirement retains its process handle");
                    // Exercise retry accounting without pretending the live fixture exited.
                    typeof(Rp).GetProperty("P").GetSetMethod(true).Invoke(endpoint, new object[] { null });
                    endpoint.Dispose();
                    Program.Check(attempts == 1 && endpoint.CleanupPending, "failed cleanup retains its retry callback");
                    endpoint.Dispose();
                    Program.Check(attempts == 2 && !endpoint.CleanupPending, "verified cleanup retires its retry callback");
                    endpoint.Dispose();
                    Program.Check(attempts == 2, "completed cleanup is not invoked again");
                }
                finally
                {
                    callback.SetValue(endpoint, null);
                    typeof(Rp).GetProperty("P").GetSetMethod(true).Invoke(endpoint, new object[] { null });
                    endpoint.Dispose();
                }
            }
        }

        [DllImport("userenv.dll", CharSet = CharSet.Unicode)]
        private static extern int CreateAppContainerProfile(string name, string display, string description,
            IntPtr capabilities, uint count, out IntPtr sid);
        [DllImport("userenv.dll", CharSet = CharSet.Unicode)]
        private static extern int DeleteAppContainerProfile(string name);
        [DllImport("advapi32.dll")]
        private static extern IntPtr FreeSid(IntPtr sid);
    }
}
