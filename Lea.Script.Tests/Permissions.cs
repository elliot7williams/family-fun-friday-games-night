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
