using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lea.Script
{
    internal sealed class Tq
    {
        private readonly Func<long> now;
        private readonly Dictionary<int, Ti> timers = new Dictionary<int, Ti>();
        private Ti current;
        private int next;
        private long order;

        internal Tq() : this(Clock()) { }
        internal Tq(Func<long> now) { this.now = now; }
        internal int N { get { return timers.Count; } }
        internal int Td
        {
            get
            {
                var timer = First();
                return timer == null ? -1 : (int)Math.Min(Int32.MaxValue, Math.Max(0, timer.Due - now()));
            }
        }

        internal int Ts(int delay, bool repeat)
        {
            if (delay < 0) throw new ArgumentOutOfRangeException("delay");
            if (timers.Count == 64 || next == Int32.MaxValue) throw new Ja.LimitException();
            var level = current == null ? 0 : current.Level;
            var timer = new Ti { Id = ++next, Delay = delay, Repeat = repeat };
            Schedule(timer, level);
            timers.Add(timer.Id, timer);
            return timer.Id;
        }

        internal void Tc(int id) { timers.Remove(id); }
        internal int Take()
        {
            if (current != null) throw new InvalidOperationException("A timer task is already active.");
            var timer = First();
            if (timer == null || timer.Due > now()) return 0;
            current = timer;
            return timer.Id;
        }

        internal void End()
        {
            var timer = current;
            current = null;
            Ti active;
            if (timer == null || !timers.TryGetValue(timer.Id, out active) || active != timer) return;
            if (timer.Repeat) Schedule(timer, timer.Level);
            else timers.Remove(timer.Id);
        }

        internal void Reset() { current = null; timers.Clear(); }
        private void Schedule(Ti timer, int level)
        {
            var delay = level > 5 ? Math.Max(4, timer.Delay) : timer.Delay;
            timer.Due = now() + delay;
            timer.Order = ++order;
            timer.Level = Math.Min(7, level + 1);
        }

        private Ti First()
        {
            Ti first = null;
            foreach (var timer in timers.Values)
                if (timer != current && (first == null || timer.Due < first.Due || timer.Due == first.Due && timer.Order < first.Order)) first = timer;
            return first;
        }

        private static Func<long> Clock()
        { var clock = Stopwatch.StartNew(); return () => clock.ElapsedMilliseconds; }
        private sealed class Ti
        {
            internal int Id, Delay, Level;
            internal long Due, Order;
            internal bool Repeat;
        }
    }
}
