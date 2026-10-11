using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lea.Script
{
    internal sealed class Fq
    {
        private readonly Func<double> now;
        private readonly HashSet<int> pending = new HashSet<int>();
        private object[] batch;
        private int next;
        private double due;

        internal Fq() : this(Clock()) { }
        internal Fq(Func<double> now) { this.now = now; }
        internal int N { get { return pending.Count; } }
        internal double Time { get; private set; }
        internal int Rd { get { return N == 0 ? -1 : (int)Math.Min(Int32.MaxValue, Math.Ceiling(Math.Max(0, due - now()))); } }

        internal int Rs()
        {
            if (N == 64 || next == Int32.MaxValue) throw new Ja.LimitException();
            if (N == 0 && batch == null) due = now() + 16;
            var id = ++next;
            pending.Add(id);
            return id;
        }

        internal void Rc(int id) { pending.Remove(id); }
        internal object[] Take()
        {
            if (batch != null) throw new InvalidOperationException("An animation frame is already active.");
            var time = now();
            if (N == 0 || time < due) return null;
            Time = time;
            due = time + 16;
            var keys = new List<int>(pending);
            keys.Sort();
            batch = new object[keys.Count];
            for (var index = 0; index < keys.Count; index++) batch[index] = keys[index];
            return batch;
        }

        internal void End()
        {
            if (batch != null) foreach (var id in batch) pending.Remove((int)id);
            batch = null;
        }

        internal void Reset() { pending.Clear(); batch = null; }
        private static Func<double> Clock()
        { var clock = Stopwatch.StartNew(); return () => clock.ElapsedTicks * (1000.0 / Stopwatch.Frequency); }
    }
}
