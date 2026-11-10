using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace Lea.Script.Tests
{
    internal static class AclBaseline
    {
        internal static bool Initialize(string path)
        {
            var security = File.GetAccessControl(path, AccessControlSections.Access);
            var before = security.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
            security.SetSecurityDescriptorSddlForm(before, AccessControlSections.Access);
            File.SetAccessControl(path, security);
            var after = Read(path);
            Validate(before, after);
            return before != after;
        }

        private static string Read(string path)
        { return File.GetAccessControl(path, AccessControlSections.Access).GetSecurityDescriptorSddlForm(AccessControlSections.Access); }

        private static void Validate(string original, string initialized)
        {
            var before = new RawSecurityDescriptor(original);
            var after = new RawSecurityDescriptor(initialized);
            if (before.DiscretionaryAcl == null || after.DiscretionaryAcl == null)
                throw new InvalidDataException("ACL baseline requires a non-null file DACL.");
            var oldAcl = new byte[before.DiscretionaryAcl.BinaryLength];
            var newAcl = new byte[after.DiscretionaryAcl.BinaryLength];
            before.DiscretionaryAcl.GetBinaryForm(oldAcl, 0);
            after.DiscretionaryAcl.GetBinaryForm(newAcl, 0);
            if (!oldAcl.SequenceEqual(newAcl) ||
                ((before.ControlFlags ^ after.ControlFlags) & ~ControlFlags.DiscretionaryAclAutoInherited) != 0)
                throw new InvalidDataException("ACL baseline initialization changed permissions or other control flags.");
        }

        internal static void Contract()
        {
            const string rule = "(A;;0x1200a9;;;WD)";
            const string other = "(A;;0x1200a9;;;AU)";
            var plain = "D:" + rule;
            Validate(plain, plain);
            Program.Check(true, "ACL baseline: identical descriptors accepted");
            Validate(plain, "D:AI" + rule);
            Program.Check(true, "ACL baseline: auto-inherited metadata alone accepted before snapshot");
            Reject(plain, "D:P" + rule, "protection-flag change rejected");
            Reject(plain, "D:(A;;FA;;;WD)", "changed access mask rejected");
            Reject(plain, "D:" + rule + other, "additional permission ACE rejected");
            Reject("D:" + rule + other, plain, "missing permission ACE rejected");
            Reject("D:" + rule + other, "D:" + other + rule, "reordered permission ACEs rejected");
            Reject(plain, "O:WD", "missing DACL rejected");

            var root = Path.Combine(Path.GetFullPath(Path.GetTempPath()), "Lea-acl-baseline-" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(root, "probe.bin");
            if (Directory.Exists(root)) throw new IOException("ACL baseline fixture already exists.");
            Directory.CreateDirectory(root);
            var passed = false;
            try
            {
                File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
                var before = Read(path);
                Initialize(path);
                var after = Read(path);
                Validate(before, after);
                Program.Check(!Initialize(path) && Read(path) == after && File.ReadAllBytes(path).SequenceEqual(new byte[] { 1, 2, 3 }),
                    "ACL baseline: private NTFS fixture is idempotent without changing permissions or data");
                passed = true;
            }
            finally
            {
                if (passed) { File.Delete(path); Directory.Delete(root, false); }
                else Console.WriteLine("Retained ACL baseline fixture: " + root);
            }
        }

        private static void Reject(string before, string after, string name)
        {
            var rejected = false;
            try { Validate(before, after); }
            catch (InvalidDataException) { rejected = true; }
            Program.Check(rejected, "ACL baseline: " + name);
        }
    }
}
