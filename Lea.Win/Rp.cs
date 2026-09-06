using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Lea.Win
{
    // Rp launches one restricted image worker and owns its exact inherited pipes.
    internal sealed class Rp : IDisposable
    {
        private const uint Inherit = 0x00000001;
        private const uint Suspended = 0x00000004;
        private const uint NoWindow = 0x08000000;
        private const uint ExtendedStartup = 0x00080000;
        private static readonly IntPtr HandleList = new IntPtr(0x00020002);
        private static readonly IntPtr MitigationPolicy = new IntPtr(0x00020007);
        private static readonly IntPtr SecurityCapabilities = new IntPtr(0x00020009);
        // DEP, SEHOP, heap termination, and force/bottom-up/high-entropy ASLR.
        private const long StartupPolicy = 0x00111105L;
        private const string ContainerName = "Lea.Image.Decoder";
        private const int AlreadyExists = unchecked((int)0x800700b7);

        public Process P { get; private set; }
        public Stream I { get; private set; }
        public Stream O { get; private set; }
        public Jb J { get; private set; }
        public bool R { get; private set; }
        public bool L { get; private set; }
        public bool S { get; private set; }
        public bool A { get; private set; }
        public bool M { get; private set; }
        public bool C { get; private set; }
        public bool N { get; private set; }
        public bool F { get; private set; }
        public bool Ready { get; set; }
        public int G { get; set; }

        private Stream error;
        private readonly object errorGate = new object();
        private readonly StringBuilder errorText = new StringBuilder();

        public string E { get { lock (errorGate) return errorText.ToString(); } }

        private Rp(Process process, Stream input, Stream output, Stream errors, Jb job, bool restricted,
            bool low, bool container, bool noCapabilities, bool fileAcl)
        {
            P = process;
            I = input;
            O = output;
            error = errors;
            J = job;
            R = restricted;
            L = low;
            S = true;
            A = true;
            M = true;
            C = container;
            N = noCapabilities;
            F = fileAcl;
            Task.Factory.StartNew(() => Drain(errors), default(System.Threading.CancellationToken),
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public static Rp Open(string path, string args, string directory, long memory, int cpu)
        {
            IntPtr attributes = IntPtr.Zero;
            IntPtr handles = IntPtr.Zero;
            IntPtr mitigation = IntPtr.Zero;
            IntPtr appSid = IntPtr.Zero;
            IntPtr capabilities = IntPtr.Zero;
            IntPtr childInput = IntPtr.Zero;
            IntPtr parentInput = IntPtr.Zero;
            IntPtr parentOutput = IntPtr.Zero;
            IntPtr childOutput = IntPtr.Zero;
            IntPtr parentError = IntPtr.Zero;
            IntPtr childError = IntPtr.Zero;
            var info = new ProcessInfo();
            var created = false;
            Process process = null;
            FileStream input = null;
            FileStream output = null;
            FileStream errors = null;
            Jb job = null;
            try
            {
                appSid = ContainerSid();
                var fileAcl = GrantFile(path, appSid);

                var security = new SecurityAttributes { Length = Marshal.SizeOf(typeof(SecurityAttributes)), Inherit = true };
                Pipe(ref security, out childInput, out parentInput, true);
                Pipe(ref security, out parentOutput, out childOutput, false);
                Pipe(ref security, out parentError, out childError, false);

                IntPtr attributeSize = IntPtr.Zero;
                InitializeProcThreadAttributeList(IntPtr.Zero, 3, 0, ref attributeSize);
                if (attributeSize == IntPtr.Zero) throw Error("Lea could not size its worker handle list.");
                attributes = Marshal.AllocHGlobal(attributeSize);
                if (!InitializeProcThreadAttributeList(attributes, 3, 0, ref attributeSize))
                    throw Error("Lea could not create its worker handle list.");
                handles = Marshal.AllocHGlobal(IntPtr.Size * 3);
                Marshal.WriteIntPtr(handles, 0, childInput);
                Marshal.WriteIntPtr(handles, IntPtr.Size, childOutput);
                Marshal.WriteIntPtr(handles, IntPtr.Size * 2, childError);
                if (!UpdateProcThreadAttribute(attributes, 0, HandleList, handles, new IntPtr(IntPtr.Size * 3),
                    IntPtr.Zero, IntPtr.Zero)) throw Error("Lea could not restrict inherited worker handles.");
                mitigation = Marshal.AllocHGlobal(8);
                Marshal.WriteInt64(mitigation, StartupPolicy);
                if (!UpdateProcThreadAttribute(attributes, 0, MitigationPolicy, mitigation, new IntPtr(8),
                    IntPtr.Zero, IntPtr.Zero)) throw Error("Lea could not set image-worker startup mitigations.");
                var emptyCapabilities = new CapabilitySet { AppContainerSid = appSid };
                capabilities = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CapabilitySet)));
                Marshal.StructureToPtr(emptyCapabilities, capabilities, false);
                if (!UpdateProcThreadAttribute(attributes, 0, SecurityCapabilities, capabilities,
                    new IntPtr(Marshal.SizeOf(typeof(CapabilitySet))), IntPtr.Zero, IntPtr.Zero))
                    throw Error("Lea could not set image-worker security capabilities.");

                var startup = new StartupInfoEx();
                startup.Startup.Length = Marshal.SizeOf(typeof(StartupInfoEx));
                startup.Startup.Flags = 0x00000100;
                startup.Startup.StandardInput = childInput;
                startup.Startup.StandardOutput = childOutput;
                startup.Startup.StandardError = childError;
                startup.Attributes = attributes;
                var command = new StringBuilder("\"" + path.Replace("\"", "") + "\"" +
                    (String.IsNullOrWhiteSpace(args) ? "" : " " + args));
                if (!CreateProcessAsUser(IntPtr.Zero, path, command, IntPtr.Zero, IntPtr.Zero, true,
                    Suspended | NoWindow | ExtendedStartup, IntPtr.Zero, directory, ref startup, out info))
                    throw Error("Lea could not start its restricted image worker.");
                created = true;
                bool noCapabilities;
                bool restricted;
                bool low;
                if (!AppContainer(info.Process, appSid, out noCapabilities, out restricted, out low) ||
                    !noCapabilities || !restricted || !low)
                    throw new InvalidOperationException("Lea image worker did not enter its restricted zero-capability AppContainer.");
                var aslr = Aslr(info.Process);
                if ((aslr & 3) != 3)
                    throw new InvalidOperationException("Lea image-worker startup ASLR was not applied: policy=" + aslr + ".");
                Close(ref childInput);
                Close(ref childOutput);
                Close(ref childError);

                job = Jb.Add(info.Process, memory, cpu);
                input = File(parentInput, FileAccess.Write);
                parentInput = IntPtr.Zero;
                output = File(parentOutput, FileAccess.Read);
                parentOutput = IntPtr.Zero;
                errors = File(parentError, FileAccess.Read);
                parentError = IntPtr.Zero;
                process = Process.GetProcessById(info.ProcessId);
                if (ResumeThread(info.Thread) == UInt32.MaxValue) throw Error("Lea could not resume its bounded image worker.");
                Close(ref info.Thread);
                Close(ref info.Process);
                return new Rp(process, input, output, errors, job, restricted, low, true, noCapabilities, fileAcl);
            }
            catch
            {
                if (job != null) job.Dispose();
                else if (created && info.Process != IntPtr.Zero) TerminateProcess(info.Process, 1);
                if (process != null) process.Dispose();
                if (input != null) input.Dispose();
                if (output != null) output.Dispose();
                if (errors != null) errors.Dispose();
                throw;
            }
            finally
            {
                Close(ref info.Thread);
                Close(ref info.Process);
                Close(ref childInput);
                Close(ref parentInput);
                Close(ref parentOutput);
                Close(ref childOutput);
                Close(ref parentError);
                Close(ref childError);
                if (attributes != IntPtr.Zero) { DeleteProcThreadAttributeList(attributes); Marshal.FreeHGlobal(attributes); }
                if (handles != IntPtr.Zero) Marshal.FreeHGlobal(handles);
                if (mitigation != IntPtr.Zero) Marshal.FreeHGlobal(mitigation);
                if (capabilities != IntPtr.Zero) Marshal.FreeHGlobal(capabilities);
                if (appSid != IntPtr.Zero) FreeSid(appSid);
            }
        }

        public void Dispose()
        {
            if (I != null) try { I.Dispose(); } catch (IOException) { }
            if (O != null) try { O.Dispose(); } catch (IOException) { }
            if (error != null) try { error.Dispose(); } catch (IOException) { }
            if (J != null) J.Dispose();
            if (P != null) P.Dispose();
            I = null;
            O = null;
            error = null;
            J = null;
            P = null;
        }

        private void Drain(Stream stream)
        {
            var buffer = new byte[1024];
            try
            {
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    lock (errorGate)
                        if (errorText.Length < 1024)
                            errorText.Append(Encoding.UTF8.GetString(buffer, 0, Math.Min(count, 1024 - errorText.Length)));
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }
        }

        private static FileStream File(IntPtr handle, FileAccess access)
        {
            return new FileStream(new SafeFileHandle(handle, true), access, 4096, false);
        }

        private static void Pipe(ref SecurityAttributes security, out IntPtr read, out IntPtr write, bool parentWrites)
        {
            if (!CreatePipe(out read, out write, ref security, 0)) throw Error("Lea could not create an image-worker pipe.");
            var parent = parentWrites ? write : read;
            if (!SetHandleInformation(parent, Inherit, 0)) throw Error("Lea could not restrict an image-worker pipe.");
        }

        private static IntPtr TokenInfo(IntPtr token, int kind)
        {
            uint size = 0;
            GetTokenInformation(token, kind, IntPtr.Zero, 0, out size);
            if (size == 0) throw Error("Lea could not size image-worker token data.");
            var data = Marshal.AllocHGlobal((int)size);
            if (!GetTokenInformation(token, kind, data, size, out size))
            {
                Marshal.FreeHGlobal(data);
                throw Error("Lea could not read image-worker token data.");
            }
            return data;
        }

        private static int Integrity(IntPtr token)
        {
            var data = TokenInfo(token, 25);
            try
            {
                var sid = Marshal.PtrToStructure<SidAttribute>(data).Sid;
                var count = Marshal.ReadByte(GetSidSubAuthorityCount(sid));
                return count == 0 ? Int32.MaxValue : Marshal.ReadInt32(GetSidSubAuthority(sid, count - 1));
            }
            finally { Marshal.FreeHGlobal(data); }
        }

        private static bool PrivilegesStripped(IntPtr token)
        {
            var data = TokenInfo(token, 3);
            try
            {
                var count = Marshal.ReadInt32(data);
                var enabled = 0;
                for (var index = 0; index < count; index++)
                    if ((Marshal.ReadInt32(data, 4 + index * 12 + 8) & 2) != 0) enabled++;
                return enabled <= 1;
            }
            finally { Marshal.FreeHGlobal(data); }
        }

        private static bool AppContainer(IntPtr process, IntPtr expectedSid, out bool noCapabilities,
            out bool restricted, out bool low)
        {
            noCapabilities = false;
            restricted = false;
            low = false;
            IntPtr token;
            if (!OpenProcessToken(process, 0x0008, out token)) throw Error("Lea could not inspect its image-worker token.");
            try
            {
                var state = TokenInfo(token, 29);
                var identity = IntPtr.Zero;
                var groups = IntPtr.Zero;
                try
                {
                    if (Marshal.ReadInt32(state) == 0) return false;
                    identity = TokenInfo(token, 31);
                    var sid = Marshal.ReadIntPtr(identity);
                    if (sid == IntPtr.Zero || !EqualSid(sid, expectedSid)) return false;
                    groups = TokenInfo(token, 30);
                    noCapabilities = Marshal.ReadInt32(groups) == 0;
                    restricted = PrivilegesStripped(token);
                    low = Integrity(token) == 4096;
                    return true;
                }
                finally
                {
                    Marshal.FreeHGlobal(state);
                    if (identity != IntPtr.Zero) Marshal.FreeHGlobal(identity);
                    if (groups != IntPtr.Zero) Marshal.FreeHGlobal(groups);
                }
            }
            finally { Close(ref token); }
        }

        private static bool GrantFile(string path, IntPtr appSid)
        {
            var identity = new SecurityIdentifier(appSid);
            var rights = FileSystemRights.ReadAndExecute;
            var security = System.IO.File.GetAccessControl(path, AccessControlSections.Access);
            foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(SecurityIdentifier)))
                if (identity.Equals(rule.IdentityReference) && rule.AccessControlType == AccessControlType.Allow &&
                    (rule.FileSystemRights & rights) == rights) return true;
            security.AddAccessRule(new FileSystemAccessRule(identity, rights, AccessControlType.Allow));
            System.IO.File.SetAccessControl(path, security);
            security = System.IO.File.GetAccessControl(path, AccessControlSections.Access);
            foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(SecurityIdentifier)))
                if (identity.Equals(rule.IdentityReference) && rule.AccessControlType == AccessControlType.Allow &&
                    (rule.FileSystemRights & rights) == rights) return true;
            return false;
        }

        private static uint Aslr(IntPtr process)
        {
            uint policy;
            if (!GetProcessMitigationPolicy(process, 1, out policy, new UIntPtr(4)))
                throw Error("Lea could not verify image-worker startup ASLR.");
            return policy;
        }

        private static IntPtr ContainerSid()
        {
            IntPtr sid;
            var result = CreateAppContainerProfile(ContainerName, ContainerName, ContainerName,
                IntPtr.Zero, 0, out sid);
            if (result == 0) return sid;
            if (result == AlreadyExists)
            {
                result = DeriveAppContainerSidFromAppContainerName(ContainerName, out sid);
                if (result == 0) return sid;
            }
            Marshal.ThrowExceptionForHR(result);
            throw new InvalidOperationException("Lea could not create its image-worker AppContainer profile.");
        }

        private static void Close(ref IntPtr handle)
        {
            var value = handle;
            handle = IntPtr.Zero;
            if (value != IntPtr.Zero && value != new IntPtr(-1)) CloseHandle(value);
        }

        private static Win32Exception Error(string message)
        {
            return new Win32Exception(Marshal.GetLastWin32Error(), message);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SidAttribute { public IntPtr Sid; public uint Attributes; }

        [StructLayout(LayoutKind.Sequential)]
        private struct CapabilitySet
        {
            public IntPtr AppContainerSid;
            public IntPtr Capabilities;
            public uint CapabilityCount;
            public uint Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SecurityAttributes
        {
            public int Length;
            public IntPtr SecurityDescriptor;
            [MarshalAs(UnmanagedType.Bool)] public bool Inherit;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct StartupInfo
        {
            public int Length;
            public string Reserved;
            public string Desktop;
            public string Title;
            public int X;
            public int Y;
            public int XSize;
            public int YSize;
            public int XChars;
            public int YChars;
            public int Fill;
            public int Flags;
            public short Show;
            public short ReservedBytes;
            public IntPtr ReservedData;
            public IntPtr StandardInput;
            public IntPtr StandardOutput;
            public IntPtr StandardError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct StartupInfoEx { public StartupInfo Startup; public IntPtr Attributes; }

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessInfo
        {
            public IntPtr Process;
            public IntPtr Thread;
            public int ProcessId;
            public int ThreadId;
        }

        [DllImport("advapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);
        [DllImport("advapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetTokenInformation(IntPtr token, int kind, IntPtr data, uint length, out uint needed);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateProcessAsUserW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateProcessAsUser(IntPtr token, string application, StringBuilder command,
            IntPtr processAttributes, IntPtr threadAttributes, [MarshalAs(UnmanagedType.Bool)] bool inherit,
            uint flags, IntPtr environment, string directory, ref StartupInfoEx startup, out ProcessInfo info);
        [DllImport("userenv.dll", CharSet = CharSet.Unicode)]
        private static extern int DeriveAppContainerSidFromAppContainerName(string name, out IntPtr sid);
        [DllImport("userenv.dll", CharSet = CharSet.Unicode)]
        private static extern int CreateAppContainerProfile(string name, string displayName, string description,
            IntPtr capabilities, uint capabilityCount, out IntPtr sid);
        [DllImport("advapi32.dll")] private static extern IntPtr FreeSid(IntPtr sid);
        [DllImport("advapi32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EqualSid(IntPtr first, IntPtr second);
        [DllImport("advapi32.dll")] private static extern IntPtr GetSidSubAuthorityCount(IntPtr sid);
        [DllImport("advapi32.dll")] private static extern IntPtr GetSidSubAuthority(IntPtr sid, int index);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreatePipe(out IntPtr read, out IntPtr write, ref SecurityAttributes security, uint size);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetHandleInformation(IntPtr handle, uint mask, uint flags);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InitializeProcThreadAttributeList(IntPtr list, int count, int flags, ref IntPtr size);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UpdateProcThreadAttribute(IntPtr list, uint flags, IntPtr attribute, IntPtr value,
            IntPtr size, IntPtr previous, IntPtr returned);
        [DllImport("kernel32.dll")] private static extern void DeleteProcThreadAttributeList(IntPtr list);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint ResumeThread(IntPtr thread);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessMitigationPolicy(IntPtr process, int kind, out uint policy, UIntPtr length);
        [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(IntPtr process, uint exitCode);
        [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr value);
    }
}
