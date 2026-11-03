using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Lea.Build
{
    // Diagnostic ownership only; the production worker's restricted job is unchanged.
    public sealed class OwnedTestProcess : IDisposable
    {
        private readonly object gate = new object();
        private KernelHandle job, process;
        private bool disposed;
        public StreamReader StandardOutput { get; private set; }
        public StreamReader StandardError { get; private set; }
        public string StartError { get; private set; }
        public uint Id { get; private set; }
        public bool Started { get; private set; }
        public bool AssignedBeforeResume { get; private set; }
        public bool TerminationRequested { get; private set; }
        public long CreationFileTime { get; private set; }

        public static OwnedTestProcess Start(string application, string arguments, string directory, bool failBeforeResume = false)
        {
            if (!Path.IsPathFullyQualified(application) || !File.Exists(application) || application.Contains("\""))
                throw new ArgumentException("An existing absolute application path is required.");
            if (!Path.IsPathFullyQualified(directory) || !Directory.Exists(directory))
                throw new ArgumentException("An existing absolute working directory is required.");
            var command = new StringBuilder("\"" + application + "\" " + arguments);
            if (command.Length >= 32767) throw new ArgumentException("Command line exceeds the Windows bound.");
            var owner = new OwnedTestProcess();
            owner.job = CreateJobObject(IntPtr.Zero, null);
            if (owner.job.IsInvalid)
            {
                var failure = new Win32Exception(Marshal.GetLastWin32Error(), "CreateJobObject");
                owner.job.Dispose();
                throw failure;
            }
            SafeFileHandle readOut = null, writeOut = null, readError = null, writeError = null, input = null;
            IntPtr attributes = IntPtr.Zero, handles = IntPtr.Zero, jobs = IntPtr.Zero, environment = IntPtr.Zero;
            bool initialized = false;
            var info = new ProcessInformation();
            try
            {
                var limits = new ExtendedLimits();
                limits.Basic.Flags = 0x2000; // Kill on last close; no breakaway or resource-limit override.
                if (!SetInformationJobObject(owner.job, 9, ref limits, (uint)Marshal.SizeOf<ExtendedLimits>())) Fail("SetInformationJobObject");
                owner.VerifyLimits();
                var security = new SecurityAttributes { Size = Marshal.SizeOf<SecurityAttributes>(), Inherit = true };
                if (!CreatePipe(out readOut, out writeOut, ref security, 0)) Fail("CreatePipe stdout");
                if (!CreatePipe(out readError, out writeError, ref security, 0)) Fail("CreatePipe stderr");
                if (!SetHandleInformation(readOut, 1, 0) || !SetHandleInformation(readError, 1, 0)) Fail("SetHandleInformation");
                input = CreateFile("NUL", 0x80000000, 3, ref security, 3, 0x80, IntPtr.Zero);
                if (input.IsInvalid) Fail("CreateFile NUL");
                UIntPtr size = UIntPtr.Zero;
                InitializeProcThreadAttributeList(IntPtr.Zero, 2, 0, ref size);
                if (size.ToUInt64() == 0 || size.ToUInt64() > 65536) throw new InvalidOperationException("Invalid attribute-list size.");
                attributes = Marshal.AllocHGlobal(checked((int)size.ToUInt64()));
                if (!InitializeProcThreadAttributeList(attributes, 2, 0, ref size)) Fail("InitializeProcThreadAttributeList");
                initialized = true;
                handles = Marshal.AllocHGlobal(IntPtr.Size * 3);
                Marshal.WriteIntPtr(handles, 0, input.DangerousGetHandle());
                Marshal.WriteIntPtr(handles, IntPtr.Size, writeOut.DangerousGetHandle());
                Marshal.WriteIntPtr(handles, IntPtr.Size * 2, writeError.DangerousGetHandle());
                jobs = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(jobs, owner.job.DangerousGetHandle());
                if (!UpdateProcThreadAttribute(attributes, 0, new UIntPtr(0x20002), handles, new UIntPtr((uint)(IntPtr.Size * 3)), IntPtr.Zero, IntPtr.Zero)) Fail("Handle list");
                if (!UpdateProcThreadAttribute(attributes, 0, new UIntPtr(0x2000d), jobs, new UIntPtr((uint)IntPtr.Size), IntPtr.Zero, IntPtr.Zero)) Fail("Job list");
                environment = Marshal.StringToHGlobalUni(CleanEnvironment());
                var startup = new StartupInfoEx();
                startup.Info.Size = Marshal.SizeOf<StartupInfoEx>();
                startup.Info.Flags = 0x100;
                startup.Info.Input = input.DangerousGetHandle();
                startup.Info.Output = writeOut.DangerousGetHandle();
                startup.Info.Error = writeError.DangerousGetHandle();
                startup.Attributes = attributes;
                // JOB_LIST assigns atomically; suspension permits verification/fault injection before user code.
                if (!CreateProcess(application, command, IntPtr.Zero, IntPtr.Zero, true,
                    0x08000000 | 0x00080000 | 0x00000400 | 0x00000004, environment, directory, ref startup, out info)) Fail("CreateProcessW");
                owner.process = new KernelHandle(info.Process); info.Process = IntPtr.Zero;
                owner.Id = info.Id;
                bool assigned;
                if (!IsProcessInJob(owner.process, owner.job, out assigned) || !assigned) Fail("IsProcessInJob");
                owner.AssignedBeforeResume = true;
                if (owner.ActiveProcesses != 1 || owner.WaitForExit(0)) throw new InvalidOperationException("Suspended owned process state differs.");
                long created, exited, kernel, user;
                if (!GetProcessTimes(owner.process, out created, out exited, out kernel, out user)) Fail("GetProcessTimes");
                owner.CreationFileTime = created;
                owner.StandardOutput = Reader(ref readOut);
                owner.StandardError = Reader(ref readError);
                if (failBeforeResume) throw new InvalidOperationException("Controlled post-create/pre-resume failure.");
                if (ResumeThread(info.Thread) != 1) Fail("ResumeThread");
                owner.Started = true;
            }
            catch (Exception failure) { owner.StartError = failure.ToString(); }
            finally
            {
                if (info.Thread != IntPtr.Zero) CloseHandle(info.Thread);
                if (info.Process != IntPtr.Zero) CloseHandle(info.Process);
                if (initialized) DeleteProcThreadAttributeList(attributes);
                if (attributes != IntPtr.Zero) Marshal.FreeHGlobal(attributes);
                if (handles != IntPtr.Zero) Marshal.FreeHGlobal(handles);
                if (jobs != IntPtr.Zero) Marshal.FreeHGlobal(jobs);
                if (environment != IntPtr.Zero) Marshal.FreeHGlobal(environment);
                if (readOut != null) readOut.Dispose();
                if (readError != null) readError.Dispose();
                if (writeOut != null) writeOut.Dispose();
                if (writeError != null) writeError.Dispose();
                if (input != null) input.Dispose();
                GC.KeepAlive(owner.job);
            }
            return owner; // Even failed starts retain their job/original process for explicit retirement.
        }

        public uint ActiveProcesses
        {
            get
            {
                lock (gate)
                {
                    RequireOpen();
                    var accounting = new Accounting();
                    if (!QueryAccounting(job, 1, ref accounting, (uint)Marshal.SizeOf<Accounting>(), IntPtr.Zero)) Fail("Query job accounting");
                    return accounting.Active;
                }
            }
        }

        public bool WaitForExit(int milliseconds)
        {
            if (milliseconds < 0 || milliseconds > 300000) throw new ArgumentOutOfRangeException(nameof(milliseconds));
            lock (gate)
            {
                RequireOpen();
                if (process == null) return true;
                var state = WaitForSingleObject(process, (uint)milliseconds);
                if (state == 0xffffffff) Fail("WaitForSingleObject");
                return state == 0;
            }
        }

        public uint ExitCode
        {
            get
            {
                lock (gate)
                {
                    RequireOpen();
                    if (process == null || !WaitForExit(0)) throw new InvalidOperationException("Original process exit not observed.");
                    uint code;
                    if (!GetExitCodeProcess(process, out code)) Fail("GetExitCodeProcess");
                    return code;
                }
            }
        }

        public bool TerminateAndWait(int retirementMilliseconds)
        {
            if (retirementMilliseconds < 0 || retirementMilliseconds > 5000) throw new ArgumentOutOfRangeException(nameof(retirementMilliseconds));
            lock (gate)
            {
                RequireOpen();
                var clock = Stopwatch.StartNew();
                if (ActiveProcesses != 0)
                {
                    TerminationRequested = true;
                    if (!TerminateJobObject(job, 0xdead)) Fail("TerminateJobObject");
                }
                do
                {
                    if (WaitForExit(0) && ActiveProcesses == 0) return true;
                    if (clock.ElapsedMilliseconds >= retirementMilliseconds) return false;
                    Thread.Sleep((int)Math.Max(0, Math.Min(10, retirementMilliseconds - clock.ElapsedMilliseconds)));
                } while (true);
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                if (disposed) return;
                if (ActiveProcesses != 0 || !WaitForExit(0)) throw new InvalidOperationException("Retain owner: original process/job retirement is unverified.");
                if (StandardOutput != null) StandardOutput.Dispose();
                if (StandardError != null) StandardError.Dispose();
                if (process != null) process.Dispose();
                job.Dispose();
                disposed = true;
            }
        }

        private void RequireOpen()
        {
            if (disposed || job == null || job.IsInvalid || job.IsClosed) throw new ObjectDisposedException(nameof(OwnedTestProcess));
        }
        private void VerifyLimits()
        {
            var limits = new ExtendedLimits();
            if (!QueryLimits(job, 9, ref limits, (uint)Marshal.SizeOf<ExtendedLimits>(), IntPtr.Zero)) Fail("Query job limits");
            if (limits.Basic.Flags != 0x2000) throw new InvalidOperationException("Owned test job policy differs.");
        }
        private static StreamReader Reader(ref SafeFileHandle handle)
        {
            var stream = new FileStream(handle, FileAccess.Read, 4096, false);
            handle = null;
            try { return new StreamReader(stream, Encoding.UTF8, true, 4096); }
            catch { stream.Dispose(); throw; }
        }
        private static string CleanEnvironment()
        {
            var values = new System.Collections.Generic.SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables()) values[(string)entry.Key] = (string)entry.Value;
            foreach (var name in new[] { "LEA_SCRIPT_STARTUP_TRACE", "LEA_FIRESPIDER_PROFILE", "LEA_FIRESPIDER_SESSION", "COR_ENABLE_PROFILING", "COR_PROFILER", "COR_PROFILER_PATH", "COR_PROFILER_PATH_32", "COR_PROFILER_PATH_64", "CORECLR_ENABLE_PROFILING", "CORECLR_PROFILER", "CORECLR_PROFILER_PATH", "CORECLR_PROFILER_PATH_32", "CORECLR_PROFILER_PATH_64", "DOTNET_STARTUP_HOOKS", "DOTNET_ADDITIONAL_DEPS", "DOTNET_SHARED_STORE" }) values.Remove(name);
            var block = new StringBuilder();
            foreach (var pair in values) block.Append(pair.Key).Append('=').Append(pair.Value).Append('\0');
            return block.Append('\0').ToString();
        }
        private static void Fail(string api) { throw new Win32Exception(Marshal.GetLastWin32Error(), api); }
        private sealed class KernelHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public KernelHandle() : base(true) { }
            public KernelHandle(IntPtr value) : base(true) { SetHandle(value); }
            protected override bool ReleaseHandle() { return CloseHandle(handle); }
        }
        [StructLayout(LayoutKind.Sequential)] private struct SecurityAttributes { public int Size; public IntPtr Descriptor; [MarshalAs(UnmanagedType.Bool)] public bool Inherit; }
        [StructLayout(LayoutKind.Sequential)] private struct StartupInfo { public int Size; public IntPtr Reserved, Desktop, Title; public uint X, Y, Width, Height, XChars, YChars, Fill, Flags; public ushort Show, ReservedLength; public IntPtr ReservedBytes, Input, Output, Error; }
        [StructLayout(LayoutKind.Sequential)] private struct StartupInfoEx { public StartupInfo Info; public IntPtr Attributes; }
        [StructLayout(LayoutKind.Sequential)] private struct ProcessInformation { public IntPtr Process, Thread; public uint Id, ThreadId; }
        [StructLayout(LayoutKind.Sequential)] private struct BasicLimits { public long ProcessTime, JobTime; public uint Flags; public UIntPtr Minimum, Maximum; public uint ActiveLimit; public UIntPtr Affinity; public uint Priority, Scheduling; }
        [StructLayout(LayoutKind.Sequential)] private struct Io { public ulong ReadOps, WriteOps, OtherOps, ReadBytes, WriteBytes, OtherBytes; }
        [StructLayout(LayoutKind.Sequential)] private struct ExtendedLimits { public BasicLimits Basic; public Io Io; public UIntPtr ProcessMemory, JobMemory, PeakProcessMemory, PeakJobMemory; }
        [StructLayout(LayoutKind.Sequential)] private struct Accounting { public long User, Kernel, PeriodUser, PeriodKernel; public uint Faults, Total, Active, Terminated; }
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern KernelHandle CreateJobObject(IntPtr security, string name);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetInformationJobObject(KernelHandle job, int kind, ref ExtendedLimits limits, uint size);
        [DllImport("kernel32.dll", EntryPoint = "QueryInformationJobObject", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool QueryLimits(KernelHandle job, int kind, ref ExtendedLimits limits, uint size, IntPtr returned);
        [DllImport("kernel32.dll", EntryPoint = "QueryInformationJobObject", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool QueryAccounting(KernelHandle job, int kind, ref Accounting accounting, uint size, IntPtr returned);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CreatePipe(out SafeFileHandle read, out SafeFileHandle write, ref SecurityAttributes security, uint size);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetHandleInformation(SafeFileHandle handle, uint mask, uint flags);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateFileW")] private static extern SafeFileHandle CreateFile(string name, uint access, uint share, ref SecurityAttributes security, uint creation, uint flags, IntPtr template);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool InitializeProcThreadAttributeList(IntPtr list, int count, int flags, ref UIntPtr size);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool UpdateProcThreadAttribute(IntPtr list, uint flags, UIntPtr attribute, IntPtr value, UIntPtr size, IntPtr previous, IntPtr returned);
        [DllImport("kernel32.dll")] private static extern void DeleteProcThreadAttributeList(IntPtr list);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateProcessW")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CreateProcess(string application, StringBuilder command, IntPtr processSecurity, IntPtr threadSecurity, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint flags, IntPtr environment, string directory, ref StartupInfoEx startup, out ProcessInformation info);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsProcessInJob(KernelHandle process, KernelHandle job, [MarshalAs(UnmanagedType.Bool)] out bool assigned);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetProcessTimes(KernelHandle process, out long created, out long exited, out long kernel, out long user);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint ResumeThread(IntPtr thread);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(KernelHandle process, uint milliseconds);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetExitCodeProcess(KernelHandle process, out uint code);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool TerminateJobObject(KernelHandle job, uint exitCode);
        [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CloseHandle(IntPtr handle);
    }
}
