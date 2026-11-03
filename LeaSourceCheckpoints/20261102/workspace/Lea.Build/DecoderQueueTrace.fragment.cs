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
                var message = text + detail;
                diagnostics(message.Length <= 512 ? message : message.Substring(0, 512));
            });
            try
            {
                return await decodeQueue.RunObserved(() =>
                {
                    entered = true;
                    return Read(data, path, args, token, reserve, limit, hostMemory,
                        started, admission, measured, killed, report);
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

