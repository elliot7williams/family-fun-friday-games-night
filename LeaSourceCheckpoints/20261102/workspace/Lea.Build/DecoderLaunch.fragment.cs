        private sealed class DecoderOwner
        {
            internal readonly long Id;
            internal readonly string Args;
            internal readonly CancellationToken Token;
            internal readonly Stopwatch Clock = Stopwatch.StartNew();
            internal string Phase = "entry";
            internal string Trace = "";
            internal DecoderOwner(string args, CancellationToken token)
            {
                Id = Interlocked.Increment(ref ownerSequence);
                Args = args == "--image-fake" ? "fake" : args == "--pipe --delay=6000" ? "delayed" : "other";
                Token = token;
            }
        }
        private static long ownerSequence;
        private static DecoderOwner activeOwner;
        private static void MarkOwner(string phase)
        {
            var owner = Interlocked.CompareExchange(ref activeOwner, null, null);
            if (owner != null)
            {
                Interlocked.Exchange(ref owner.Phase, phase);
                var entry = phase + "=" + owner.Clock.ElapsedMilliseconds + ";";
                string previous;
                do
                {
                    previous = Interlocked.CompareExchange(ref owner.Trace, null, null);
                    if (previous.Length + entry.Length > 180) break;
                } while (Interlocked.CompareExchange(ref owner.Trace, previous + entry, previous) != previous);
            }
        }
        private static string OwnerSnapshot()
        {
            var owner = Interlocked.CompareExchange(ref activeOwner, null, null);
            return owner == null ? "; owner=none" : "; owner=" + owner.Id +
                ":" + owner.Args + ":" + Interlocked.CompareExchange(ref owner.Phase, null, null) +
                ":age=" + owner.Clock.ElapsedMilliseconds + ":s=" + Interlocked.CompareExchange(ref owner.Trace, null, null) + ":cancel=" + (owner.Token.IsCancellationRequested ? "1" : "0");
        }

        public static async Task<Image> Rd(byte[] data, string path, string args, CancellationToken token,
            long reserve, long limit, Func<long> hostMemory,
            Action<int, bool, bool, bool, bool, bool, bool, bool, bool, bool, int, int> started,
            Action<long, long, long, long, bool> admission,
            Action<long, long, long, long> measured, Action killed, Action<string> diagnostics)
        {
            var entered = false;
            var canceledAtEntry = token.IsCancellationRequested;
            var clock = Stopwatch.StartNew();
            string queuePhase = "wait";
            long admittedAt = -1, dispatchAt = -1, threadAt = -1;
            Action<string> observe = diagnostics == null ? null : new Action<string>(phase =>
            {
                var elapsed = clock.ElapsedMilliseconds;
                if (phase == "admit") Interlocked.Exchange(ref admittedAt, elapsed);
                else if (phase == "dispatch") Interlocked.Exchange(ref dispatchAt, elapsed);
                else if (phase == "thread") Interlocked.Exchange(ref threadAt, elapsed);
                Interlocked.Exchange(ref queuePhase, phase);
            });
            Action<string> report = diagnostics == null ? null : new Action<string>(text =>
            {
                var detail = "; queue=" + Interlocked.CompareExchange(ref queuePhase, null, null) +
                    "; admitMs=" + Interlocked.Read(ref admittedAt) + "; dispatchMs=" + Interlocked.Read(ref dispatchAt) +
                    "; threadMs=" + Interlocked.Read(ref threadAt);
                detail += OwnerSnapshot();
                if (detail.Length > 448) detail = detail.Substring(0, 448);
                var message = (text.Length > 512 - detail.Length ? text.Substring(0, 512 - detail.Length) : text) + detail;
                diagnostics(message.Length <= 512 ? message : message.Substring(0, 512));
            });
            try
            {
                return await decodeQueue.RunObserved(() =>
                {
                    entered = true;
                    var owner = new DecoderOwner(args, token);
                    Interlocked.Exchange(ref activeOwner, owner);
                    try
                    {
                        return Read(data, path, args, token, reserve, limit, hostMemory,
                            started, admission, measured, killed, report);
                    }
                    finally { Interlocked.CompareExchange(ref activeOwner, null, owner); }
                }, token, observe).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (!entered && !canceledAtEntry && report != null)
                    try { report("phase=queue; result=cancel; worker=0; wall=" + clock.ElapsedMilliseconds +
                        "; ready=0; kind=OperationCanceledException; steps=queue=0;"); }
                    catch (Exception) { }
                throw;
            }
        }

        internal sealed class DecodeQueue
        {
            private readonly SemaphoreSlim gate;

            internal DecodeQueue() : this(new SemaphoreSlim(1, 1)) { }
            internal DecodeQueue(SemaphoreSlim gate)
            { this.gate = gate ?? throw new ArgumentNullException(nameof(gate)); }

            internal Task<T> Run<T>(Func<T> decode, CancellationToken token)
            { return RunObserved(decode, token, null); }

            private static void Observe(Action<string> observer, string phase)
            {
                if (observer != null) try { observer(phase); } catch (Exception) { }
            }

            internal async Task<T> RunObserved<T>(Func<T> decode, CancellationToken token, Action<string> observer)
            {
                Observe(observer, "wait");
                await gate.WaitAsync(token).ConfigureAwait(false);
                Task<T> work;
                try
                {
                    Observe(observer, "admit");
                    token.ThrowIfCancellationRequested();
                    Observe(observer, "dispatch");
                    work = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            Observe(observer, "thread");
                            token.ThrowIfCancellationRequested();
                            return decode();
                        }
                        finally { gate.Release(); }
                    }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
                catch { gate.Release(); throw; }
                return await work.ConfigureAwait(false);
            }
        }

