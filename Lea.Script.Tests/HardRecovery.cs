using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Web.Script.Serialization;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class HardRecovery
    {
        internal static string Detail(Jp script, Jr result)
        {
            return result.S + "; " + result.M + "; startup=" + script.Sd +
                "; execution=" + script.Ed + "; pid=" + script.Pid;
        }

        internal static void Run(string fixture, string reportPath)
        {
            reportPath = Lea.Diagnostics.WorkerObservation.ValidateReport(reportPath);
            var rows = new List<object>();
            var clock = Stopwatch.StartNew();
            var engine = new E();
            var page = engine.Go(fixture);
            var runtime = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptWorker");
            var files = Directory.GetFiles(runtime);
            var identities = new HashSet<string>(StringComparer.Ordinal);
            Exception failure = null;
            var retired = false;
            Process survivor = null;
            var script = new Jp(engine, page);
            try
            {
                long elapsed = 0;
                Jr Code(string stage, string source)
                {
                    var priorPid = script.Pid;
                    var priorSid = script.Sid;
                    using (var prior = priorPid == 0 ? null : Process.GetProcessById(priorPid))
                    {
                        // Pin the old process before execution so a reused PID cannot prove retirement.
                        if (prior != null) { var handle = prior.Handle; }
                        var birth = prior == null ? "" : prior.StartTime.ToUniversalTime().ToString("O");
                        var before = clock.ElapsedMilliseconds;
                        var call = Stopwatch.StartNew();
                        var result = script.Ex(source);
                        elapsed = call.ElapsedMilliseconds;
                        var startup = script.Sd;
                        var execution = script.Ed;
                        var pid = script.Pid;
                        var sid = script.Sid;
                        if (!String.IsNullOrEmpty(sid)) identities.Add(sid);
                        var previousExited = prior == null ? (bool?)null :
                            pid == priorPid ? prior.HasExited : prior.WaitForExit(1000);
                        var leases = (ICollection<FileStream>)typeof(Jp).GetField("leases", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(script);
                        var io = typeof(Jp).GetField("io", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(script);
                        rows.Add(new { stage, beganMs = before, elapsedMs = elapsed, budgetMs = script.Tm,
                            status = result.S, changes = result.N, message = result.M, startup, execution,
                            previousPid = priorPid, previousSid = priorSid, previousBirthUtc = birth, previousExited,
                            pid, sid, activeWorker = pid != 0, isolatedMetadata = script.Ac, guardsMetadata = script.Mg,
                            noLoopbackMetadata = script.Ne, networkDeniedMetadata = script.Nd, fileDeniedMetadata = script.Fd,
                            ownedIo = io != null, readLeases = leases.Count, timers = script.Tn, frames = script.Rn, title = page.T });
                        Console.WriteLine("Hard recovery " + stage + ": " + Detail(script, result) + "; elapsed=" + elapsed);
                        Program.Check(io == null && leases.Count == (pid == 0 ? 0 : 8),
                            "hard recovery: idle I/O released and exact read leases match worker ownership: " + stage);
                        if (prior != null && pid != priorPid)
                            Program.Check(previousExited == true && !files.Any(file => HasSid(file, priorSid)),
                                "hard recovery: pinned old process exits and its exact ACL grants retire: " + stage);
                        return result;
                    }
                }

                var setup = Code("warmup", "1;");
                Program.Check(setup.Ok && script.Ac && script.Ne && script.Nd && script.Fd && script.Mg == 15,
                    "hard recovery: verified native warmup with unchanged isolation: " + Detail(script, setup));
                script.Tm = 25;
                var timeout = Code("regex-deadline", "/(a+)+$/.test('a'.repeat(120) + '!');");
                Deadline(script, timeout, elapsed, "native regex");
                script.Tm = 2000;
                var recovery = Code("regex-replacement", "document.title='recovered';");
                Program.Check(recovery.Ok && page.T == "recovered" && recovery.N == 1 && script.Pid != 0,
                    "hard recovery: fresh worker after native regex deadline: " + Detail(script, recovery));
                var warmup = Code("microtask-warmup", "queueMicrotask(function(){});");
                Program.Check(warmup.Ok, "hard recovery: microtask warmup: " + Detail(script, warmup));
                script.Tm = 25;
                timeout = Code("microtask-deadline", "queueMicrotask(function(){/(a+)+$/.test('a'.repeat(120)+'!');document.title='late microtask';});");
                Deadline(script, timeout, elapsed, "microtask regex");
                Program.Check(page.T == "recovered", "hard recovery: deadline prevents the late microtask mutation");
                script.Tm = 2000;
                recovery = Code("microtask-replacement", "document.title='recovered';");
                Program.Check(recovery.Ok && page.T == "recovered" && recovery.N == 1 && script.Pid != 0,
                    "hard recovery: fresh worker after microtask hard deadline: " + Detail(script, recovery));
                Program.Check(script.Sd.StartsWith("phase=ready;", StringComparison.Ordinal) &&
                    script.Ed.StartsWith("phase=done;", StringComparison.Ordinal) && script.Sd.Length <= 512 && script.Ed.Length <= 512,
                    "hard recovery: replacement records separate bounded startup and execution reports");
                Program.Check(script.Tm == 2000 && script.Cp == 2500 && script.Mm == 134217728,
                    "hard recovery: default page deadline and fixed CPU/memory limits preserved");
                survivor = Process.GetProcessById(script.Pid);
                var survivorHandle = survivor.Handle;
            }
            catch (Exception error) { failure = error; }
            finally
            {
                try
                {
                    script.Dispose();
                    retired = (survivor == null || survivor.WaitForExit(1000)) &&
                        identities.All(sid => files.All(file => !HasSid(file, sid)));
                    if (failure == null) Program.Check(retired, "hard recovery: disposal retires the final pinned process and all session ACLs");
                }
                catch (Exception error) { if (failure == null) failure = error; }
                finally
                {
                    if (survivor != null) survivor.Dispose();
                    using (var output = new FileStream(reportPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(output, new UTF8Encoding(false)))
                        writer.Write(new JavaScriptSerializer { MaxJsonLength = 1024 * 1024 }.Serialize(new {
                            kind = "hard-deadline-recovery", passed = failure == null, cleanupVerified = retired,
                            error = failure == null ? "" : Limit(failure.ToString(), 8192), calls = rows,
                            elapsedMs = clock.ElapsedMilliseconds, startupMs = 5000, executionMs = 2000, hardDeadlineMs = 25,
                            cpuRate = 2500, memoryBytes = 134217728, retries = 0, observerEnabled = false,
                            startupTraceEnabled = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE") == "1",
                            fullSuiteGate = false, coldCacheControlled = false, productionRuntimeChanged = false
                        }));
                }
            }
            if (failure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }

        private static void Deadline(Jp script, Jr result, long elapsed, string label)
        {
            Program.Check(result.S == "timeout" && result.M.StartsWith("Worker execution deadline exceeded. phase=", StringComparison.Ordinal) &&
                result.M.Length <= 512 && script.Ed.StartsWith("phase=", StringComparison.Ordinal) && script.Ed.Length <= 512 &&
                elapsed < 2000 && script.Pid == 0 && script.Tn == 0 && script.Rn == 0,
                "hard recovery: parent hard deadline retires " + label + " without a budget change: " + Detail(script, result));
        }

        private static bool HasSid(string path, string sid)
        {
            return File.GetAccessControl(path, AccessControlSections.Access)
                .GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>()
                .Any(rule => rule.IdentityReference.Value == sid);
        }

        private static string Limit(string value, int maximum) { return value.Length <= maximum ? value : value.Substring(0, maximum); }
    }
}
