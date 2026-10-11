using System;
using System.Threading;
using System.Threading.Tasks;
using Lea.Engine;

namespace Lea.Win
{
    public interface Jx : IDisposable
    {
        Task<Xr> ExA(string code);
        Task<Xr> EvA(N target, string type);
        Task<Xr> PgA();
        void St();
    }

    public interface Jt
    {
        int Td { get; }
        Task<Xr> TkA();
    }

    public interface Jf
    {
        int Rd { get; }
        Task<Xr> RfA();
    }

    public sealed class Xr
    {
        public Xr(string status, int count, string message, bool prevented = false)
        {
            S = status ?? "host";
            N = Math.Max(0, count);
            M = message == null ? "" : message.Length > 512 ? message.Substring(0, 512) : message;
            D = prevented;
        }
        public string S { get; private set; }
        public int N { get; private set; }
        public string M { get; private set; }
        public bool D { get; private set; }
        public bool Ok { get { return S == "ok"; } }
    }

    public sealed partial class V
    {
        private readonly int scriptThread = Thread.CurrentThread.ManagedThreadId;
        private Func<E, P, Jx> scriptFactory;
        private Xb scriptBinding;
        private bool scriptSetup;
        private Task activationTail = Task.FromResult(0);
        private int activationPending;
        private long activationEpoch;
        public event EventHandler Xd;
        public Xr Xo { get; private set; }
        public Jx Xs { get { return scriptBinding == null ? null : scriptBinding.Runner; } }
        public Task<Xr> Xt { get { return scriptBinding == null ? Task.FromResult(new Xr("none", 0, "")) : scriptBinding.Tail; } }
        public int Xn { get { return scriptBinding == null ? 0 : scriptBinding.Pending; } }

        public Func<E, P, Jx> Xf
        {
            get { return scriptFactory; }
            set
            {
                CheckScriptThread();
                CheckOpen();
                if (scriptFactory == value) return;
                RetireScript();
                scriptFactory = value;
                e.Jb = value == null;
            }
        }

        public Task<Xr> JsA(string code)
        {
            if (code == null || code.Length > 16384) return Task.FromResult(new Xr("code", 0, "Code must be at most 16 KiB."));
            return QueueScript(runner => runner.ExA(code));
        }

        public Task<Xr> Xe(N target, string type) { return QueueScript(runner => runner.EvA(target, type)); }
        public Task<Xr> Xl() { return QueueScript(runner => runner.PgA()); }

        private Task<Xr> QueueScript(Func<Jx, Task<Xr>> execute)
        {
            CheckScriptThread();
            if (closed || At == null) return Task.FromResult(new Xr("none", 0, ""));
            if (!e.Je) return Task.FromResult(new Xr("off", 0, "Scripting is disabled."));
            if (scriptFactory == null) return Task.FromResult(new Xr("none", 0, "No optional runtime is attached."));
            if (scriptSetup) return Task.FromResult(new Xr("busy", 0, "The script session is being created."));
            if (e.Busy) return Task.FromResult(new Xr("busy", 0, "The document is loading."));
            if (scriptBinding != null && (scriptBinding.Page != At || scriptBinding.Generation != e.Jg)) RetireScript();
            if (scriptBinding == null)
            {
                try
                {
                    var page = At;
                    var factory = scriptFactory;
                    var generation = e.Jg;
                    scriptSetup = true;
                    var runner = factory(e, page);
                    if (runner == null) return Task.FromResult(new Xr("setup", 0, "Runtime factory returned no session."));
                    if (closed || At != page || scriptFactory != factory || !e.Je || e.Jg != generation)
                    { runner.St(); runner.Dispose(); return Task.FromResult(new Xr("stale", 0, "Page or permission changed during setup.")); }
                    scriptBinding = new Xb(page, generation, runner);
                }
                catch (Exception error) { return Task.FromResult(new Xr("setup", 0, error.Message)); }
                finally { scriptSetup = false; }
            }
            var binding = scriptBinding;
            if (binding.Pending == 32) return Task.FromResult(new Xr("busy", 0, "Script event queue is full."));
            if (binding.Timer != null) binding.Timer.Stop();
            binding.Pending++;
            var previous = binding.Tail;
            var completion = new TaskCompletionSource<Xr>();
            binding.Tail = completion.Task;
            CompleteScript(binding, previous, execute, completion);
            return completion.Task;
        }

        private async void CompleteScript(Xb binding, Task<Xr> previous, Func<Jx, Task<Xr>> execute, TaskCompletionSource<Xr> completion)
        {
            try { completion.SetResult(await RunScript(binding, previous, execute)); }
            catch (Exception error) { completion.SetException(error); }
        }

        private async Task<Xr> RunScript(Xb binding, Task<Xr> previous, Func<Jx, Task<Xr>> execute)
        {
            try
            {
                await previous;
                if (!CurrentScript(binding)) return new Xr("stale", 0, "The script session has been retired.");
                var before = binding.Page.Jc;
                Xr result;
                try { result = await execute(binding.Runner) ?? new Xr("host", 0, "Runtime returned no result."); }
                catch (Exception error) { result = new Xr("host", Math.Max(0, binding.Page.Jc - before), error.Message); }
                if (CurrentScript(binding))
                {
                    Xo = result;
                    try
                    {
                        if (result.N > 0) ScriptChanged(result.N);
                        if (CurrentScript(binding) && Xd != null) Xd(this, EventArgs.Empty);
                    }
                    catch (Exception error) { result = new Xr("host", result.N, error.Message, result.D); if (CurrentScript(binding)) Xo = result; }
                }
                return result;
            }
            finally
            {
                binding.Pending--;
                if (binding.Retired && binding.Pending == 0) DisposeScript(binding);
                else if (binding.Pending == 0) ArmScriptTimer(binding);
            }
        }

        private void ArmScriptTimer(Xb binding)
        {
            if (!CurrentScript(binding)) return;
            var timed = binding.Runner as Jt;
            var framed = binding.Runner as Jf;
            if (timed == null && framed == null) return;
            try
            {
                var delay = timed == null ? -1 : timed.Td;
                var frameDelay = framed == null || !Visible ? -1 : framed.Rd;
                if (frameDelay >= 0 && (delay < 0 || frameDelay < delay)) delay = frameDelay;
                if (delay < 0) { if (binding.Timer != null) binding.Timer.Stop(); return; }
                if (binding.Timer == null)
                {
                    binding.Timer = new System.Windows.Forms.Timer();
                    binding.Timer.Tick += async (sender, args) =>
                    {
                        if (binding.Timer != null) binding.Timer.Stop();
                        if (!CurrentScript(binding)) { if (scriptBinding == binding) RetireScript(); return; }
                        try
                        {
                            await QueueScript(runner =>
                            {
                                var frame = runner as Jf;
                                if (Visible && frame != null && frame.Rd == 0) return frame.RfA();
                                var timer = runner as Jt;
                                return timer != null && timer.Td == 0 ? timer.TkA() : Task.FromResult(new Xr("ok", 0, ""));
                            });
                        }
                        catch (Exception error) { if (CurrentScript(binding)) Xo = new Xr("host", 0, error.Message); }
                    };
                }
                binding.Timer.Interval = Math.Max(1, delay);
                binding.Timer.Start();
            }
            catch (Exception error)
            {
                if (binding.Timer != null) binding.Timer.Stop();
                if (CurrentScript(binding)) Xo = new Xr("host", 0, error.Message);
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (scriptBinding != null && scriptBinding.Pending == 0) ArmScriptTimer(scriptBinding);
        }

        private bool CurrentScript(Xb binding)
        {
            return !closed && !binding.Retired && scriptBinding == binding && At == binding.Page && e.Je && e.Jg == binding.Generation;
        }

        private void RetireScript()
        {
            activationEpoch++;
            activationTail = Task.FromResult(0);
            var binding = scriptBinding;
            scriptBinding = null;
            Xo = null;
            if (binding == null) return;
            binding.Retired = true;
            if (binding.Timer != null) { binding.Timer.Dispose(); binding.Timer = null; }
            binding.Runner.St();
            if (binding.Pending == 0) DisposeScript(binding);
        }

        private static void DisposeScript(Xb binding)
        {
            if (binding.Disposed) return;
            binding.Disposed = true;
            binding.Runner.Dispose();
        }

        private void ScriptChanged(int count)
        {
            if (edit != null && !At.Fs.Contains(edit)) EndEdit(false);
            if (heldEdit != null && !At.Fs.Contains(heldEdit)) ForgetEdit();
            if (edit != null && ed.Text != edit.V)
            {
                var start = ed.SelectionStart;
                var length = ed.SelectionLength;
                editLock = true;
                try { ed.Text = edit.V; ed.Select(Math.Min(start, ed.TextLength), Math.Min(length, Math.Max(0, ed.TextLength - start))); }
                finally { editLock = false; }
                editStart = edit.V;
                ForgetEdit();
            }
            Commands(count);
            if (closed) return;
            EnsureLayout();
            PlaceEditor();
        }

        private Task<bool> ScriptClick(N target) { return ScriptDefault(target, "click"); }

        private async Task<bool> ScriptDefault(N target, string type)
        {
            var page = At;
            var factory = scriptFactory;
            var epoch = activationEpoch;
            if (target != null && target.K == "#text") target = target.Up;
            var result = await Xe(target, type);
            return closed || At != page || scriptFactory != factory || activationEpoch != epoch || result.D ||
                target != null && !e.Jn(page, target) ||
                result.S != "ok" && result.S != "none" && result.S != "off" && result.S != "origin";
        }

        private async void ShownScripts(P page, Uri address)
        {
            try
            {
                var epoch = activationEpoch;
                var scripts = await Xl();
                if (closed || At != page || page.U != address) return;
                await PicsA(page);
                if (closed || At != page || page.U != address) return;
                if (scripts.Ok && scriptFactory != null && epoch == activationEpoch) await Xe(null, "load");
                if (closed || At != page || page.U != address) return;
                if (Done != null) Done(this, EventArgs.Empty);
                if (!closed && At == page && loadOwner == null) GoRequest(page);
            }
            catch (Exception error) { if (!closed && At == page) Xo = new Xr("host", 0, error.Message); }
        }

        private async void ScriptEvent(N target, string type)
        {
            if (scriptFactory == null || closed) return;
            var page = At;
            try { await Xe(target, type); }
            catch (Exception error) { if (!closed && At == page) Xo = new Xr("host", 0, error.Message); }
        }

        private async Task FieldEvents(Fi field)
        {
            var page = At;
            if (scriptFactory == null || closed || page == null || !page.Fs.Contains(field)) return;
            await Xe(field.El, "input");
            if (!closed && At == page && page.Fs.Contains(field)) await Xe(field.El, "change");
        }

        private async void ScriptFields(Fi field)
        {
            var page = At;
            try { await FieldEvents(field); }
            catch (Exception error) { if (!closed && At == page) Xo = new Xr("host", 0, error.Message); }
        }

        // Serialize native default actions as well as dispatch, so rapid checkbox
        // clicks cannot preactivate the field ahead of an earlier listener.
        private Task Activation(Func<Task> action, N target)
        {
            if (scriptFactory == null) return action();
            if (activationPending == 32) return Task.FromResult(0);
            var page = At;
            var factory = scriptFactory;
            var epoch = activationEpoch;
            var previous = activationTail;
            var completion = new TaskCompletionSource<int>();
            activationTail = completion.Task;
            activationPending++;
            CompleteActivation();
            return completion.Task;

            async void CompleteActivation()
            {
                Exception failure = null;
                try
                {
                    try { await previous; } catch { }
                    if (!closed && At == page && scriptFactory == factory && epoch == activationEpoch && e.Jn(page, target)) await action();
                }
                catch (Exception error) { failure = error; }
                finally { activationPending--; }
                if (failure == null) completion.SetResult(0); else completion.SetException(failure);
            }
        }

        private void CheckScriptThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != scriptThread)
                throw new InvalidOperationException("Use the Lea view's script API on its owning thread.");
        }

        private sealed class Xb
        {
            internal readonly P Page;
            internal readonly long Generation;
            internal readonly Jx Runner;
            internal Task<Xr> Tail = Task.FromResult(new Xr("ok", 0, ""));
            internal int Pending;
            internal bool Retired;
            internal bool Disposed;
            internal System.Windows.Forms.Timer Timer;
            internal Xb(P page, long generation, Jx runner) { Page = page; Generation = generation; Runner = runner; }
        }
    }
}
