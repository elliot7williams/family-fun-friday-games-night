using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class Permissions
    {
        internal static void Run()
        {
            var path = Path.Combine(Path.GetTempPath(), "Lea-acl-test-" + Guid.NewGuid().ToString("N") + ".txt");
            File.WriteAllText(path, "trusted test fixture");
            var original = Rules(path);
            var identities = Enumerable.Range(1, 12).Select(index =>
                new SecurityIdentifier("S-1-15-2-100-" + index)).ToArray();
            try
            {
                ExactContract(path, identities[0], identities[1]);
                for (var round = 0; round < 8; round++)
                {
                    Together(8, index => Rp.FileAccessRule(path, identities[index], true));
                    Program.Check(identities.Take(8).All(identity => Has(path, identity)), "concurrent exact grants coexist: " + round);
                    Together(8, index => index < 4 ? Rp.FileAccessRule(path, identities[index], false) :
                        Rp.FileAccessRule(path, identities[index + 4], true));
                    Program.Check(identities.Take(4).All(identity => !Has(path, identity)) &&
                        identities.Skip(4).All(identity => Has(path, identity)), "concurrent cleanup preserves other grants: " + round);
                    Together(8, index => Rp.FileAccessRule(path, identities[index + 4], false));
                    Program.Check(Rules(path).SequenceEqual(original), "concurrent grants retire without unrelated ACL changes: " + round);
                }
            }
            finally
            {
                foreach (var identity in identities) Rp.FileAccessRule(path, identity, false);
                File.Delete(path);
            }
        }

        private static void ExactContract(string path, SecurityIdentifier identity, SecurityIdentifier other)
        {
            var baseline = File.GetAccessControl(path, AccessControlSections.Access)
                .GetSecurityDescriptorSddlForm(AccessControlSections.Access);
            try
            {
                var broad = File.GetAccessControl(path, AccessControlSections.Access);
                broad.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(path, broad);
                var before = Rules(path);
                Program.Check(!Rp.FileAccessRule(path, identity, true), "broader file grant is not accepted as least privilege");
                Program.Check(Rules(path).SequenceEqual(before), "rejected broader grant does not modify permissions");
                Program.Check(!Rp.FileAccessRule(path, identity, false), "cleanup does not report broader access as revoked");
                Program.Check(Rules(path).SequenceEqual(before), "cleanup preserves unowned broader permissions");

                Restore(path, baseline);
                var denied = File.GetAccessControl(path, AccessControlSections.Access);
                denied.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.ReadAndExecute, AccessControlType.Deny));
                File.SetAccessControl(path, denied);
                before = Rules(path);
                Program.Check(!Rp.FileAccessRule(path, identity, true), "denied identity is not reported as granted");
                Program.Check(Rules(path).SequenceEqual(before), "rejected denied grant preserves permissions");

                Restore(path, baseline);
                var foreign = File.GetAccessControl(path, AccessControlSections.Access);
                foreign.AddAccessRule(new FileSystemAccessRule(other, FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(path, foreign);
                before = Rules(path);
                Program.Check(Rp.FileAccessRule(path, identity, true), "exact grant coexists with another identity");
                var granted = Rules(path);
                Program.Check(Rp.FileAccessRule(path, identity, true) && Rules(path).SequenceEqual(granted), "exact grant remains idempotent");
                Program.Check(Rp.FileAccessRule(path, identity, false) && Rules(path).SequenceEqual(before), "verified exact cleanup preserves another identity");
                Program.Check(Rp.FileAccessRule(path, identity, false) && Rules(path).SequenceEqual(before), "verified empty cleanup remains idempotent");
                InheritedContract(identity);
            }
            finally { Restore(path, baseline); }
        }

        private static void Restore(string path, string descriptor)
        {
            var security = new FileSecurity();
            security.SetSecurityDescriptorSddlForm(descriptor, AccessControlSections.Access);
            File.SetAccessControl(path, security);
            if (File.GetAccessControl(path, AccessControlSections.Access).GetSecurityDescriptorSddlForm(AccessControlSections.Access) != descriptor)
                throw new InvalidOperationException("Permission fixture was not restored.");
        }

        private static void InheritedContract(SecurityIdentifier identity)
        {
            var directory = Path.Combine(Path.GetTempPath(), "Lea-acl-parent-" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(directory, "inherited.txt");
            Directory.CreateDirectory(directory);
            try
            {
                var security = Directory.GetAccessControl(directory, AccessControlSections.Access);
                security.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.ReadAndExecute,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(directory, security);
                File.WriteAllText(path, "inherited permission fixture");
                var before = Rules(path);
                Program.Check(File.GetAccessControl(path, AccessControlSections.Access).GetAccessRules(true, true, typeof(SecurityIdentifier))
                    .Cast<FileSystemAccessRule>().Any(rule => identity.Equals(rule.IdentityReference) && rule.IsInherited), "inherited permission fixture is effective");
                Program.Check(!Rp.FileAccessRule(path, identity, true), "inherited grant is not accepted as an owned lease");
                Program.Check(Rules(path).SequenceEqual(before), "inherited grant rejection preserves permissions");
                Program.Check(!Rp.FileAccessRule(path, identity, false), "inherited access is not reported as revoked");
                Program.Check(Rules(path).SequenceEqual(before), "inherited cleanup preserves the parent grant");
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
                Directory.Delete(directory);
            }
        }

        private static void Together(int count, Func<int, bool> action)
        {
            using (var start = new ManualResetEventSlim())
            {
                var tasks = Enumerable.Range(0, count).Select(index => Task.Factory.StartNew(() =>
                {
                    start.Wait();
                    if (!action(index)) throw new InvalidOperationException("File grant read-back failed.");
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray();
                start.Set();
                Task.WaitAll(tasks);
            }
        }

        private static bool Has(string path, SecurityIdentifier identity)
        {
            return File.GetAccessControl(path, AccessControlSections.Access)
                .GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>()
                .Any(rule => identity.Equals(rule.IdentityReference) && rule.AccessControlType == AccessControlType.Allow &&
                    (rule.FileSystemRights & FileSystemRights.ReadAndExecute) == FileSystemRights.ReadAndExecute);
        }

        private static string[] Rules(string path)
        {
            return File.GetAccessControl(path, AccessControlSections.Access)
                .GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>()
                .Select(rule => rule.IdentityReference.Value + ":" + rule.FileSystemRights + ":" + rule.AccessControlType + ":" +
                    rule.IsInherited + ":" + rule.InheritanceFlags + ":" + rule.PropagationFlags).OrderBy(rule => rule, StringComparer.Ordinal).ToArray();
        }
    }
}
