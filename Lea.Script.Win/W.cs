using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lea.Engine;
using Lea.Win;

namespace Lea.Script.Win
{
    public sealed class W : Jx, Jt, Jf
    {
        private readonly E engine;
        private readonly P page;
        private bool loaded;
        private bool complete;
        private bool loadRequested;
        private bool dormant;
        private bool demanded;
        public Jp At { get; private set; }
        public int Td { get { return Local ? At.Td : -1; } }
        public int Rd { get { return Local ? At.Rd : -1; } }

        public async Task<Xr> RfA()
        {
            if (!Local) return new Xr("origin", 0, "Remote frame execution is disabled.");
            var result = await At.RfA();
            if (!result.Ok) { loaded = false; complete = false; }
            return Convert(result, 0);
        }

        public async Task<Xr> TkA()
        {
            if (!Local) return new Xr("origin", 0, "Remote timer execution is disabled.");
            var result = await At.TkA();
            if (!result.Ok) { loaded = false; complete = false; }
            return Convert(result, 0);
        }

        private W(E engine, P page, string directory, int milliseconds)
        {
            this.engine = engine;
            this.page = page;
            At = directory == null ? new Jp(engine, page) : new Jp(engine, page, directory);
            At.Tm = milliseconds;
        }

        public static void On(V view, string directory = null, int milliseconds = 2000)
        {
            if (view == null) throw new ArgumentNullException("view");
            view.Xf = (engine, page) => new W(engine, page, directory, milliseconds);
        }

        public static void Off(V view)
        {
            if (view == null) throw new ArgumentNullException("view");
            view.Xf = null;
        }

        private bool Local { get { return page.Og.Lc; } }

        public async Task<Xr> ExA(string code)
        {
            if (!Local) return new Xr("origin", 0, "Automatic and supplied scripts in this adapter are local-document only.");
            demanded = true;
            var start = await PgA();
            if (!start.Ok) return start;
            var result = await At.ExA(code);
            if (!result.Ok) { loaded = false; complete = false; }
            return Convert(result, start.N);
        }

        public async Task<Xr> EvA(N target, string type)
        {
            if (type != "click" && type != "input" && type != "change" && type != "submit" && type != "DOMContentLoaded" && type != "load")
                return new Xr("event", 0, "Unsupported event type.");
            if (target != null && !engine.Jn(page, target)) return new Xr("node", 0, "Event target is not connected to this page.");
            if (!Local) return new Xr("origin", 0, "Remote event execution is disabled.");
            var load = target == null && type == "load";
            if (load) loadRequested = true;
            var start = await PgA();
            if (!start.Ok) return start;
            if (dormant && !demanded) return start;
            if (load && complete) return start;
            var result = await At.EvA(target, type);
            if (!result.Ok) { loaded = false; complete = false; }
            else if (load) complete = true;
            return Convert(result, start.N);
        }

        public async Task<Xr> PgA()
        {
            if (!Local) return new Xr("origin", 0, "Remote inline scripts are disabled.");
            if (loaded || dormant && !demanded) return new Xr("ok", 0, "");
            loaded = true;
            dormant = false;
            complete = false;
            var count = 0;
            var handlers = false;
            var scripts = new List<string>();
            var nodes = new Stack<N>();
            nodes.Push(page.D);
            var scanned = 0;
            while (nodes.Count > 0)
            {
                var node = nodes.Pop();
                if (++scanned > 8192) { loaded = false; return new Xr("limit", 0, "Inline script scan exceeded its node limit."); }
                if (node.K == "template") continue;
                if (!handlers) handlers = node.Av("onclick").Length > 0 || node.Av("oninput").Length > 0 ||
                    node.Av("onchange").Length > 0 || node.Av("onsubmit").Length > 0 || node.Av("onload").Length > 0;
                if (node.K == "script" && node.Av("src").Length == 0)
                {
                    var type = node.Av("type").Trim().ToLowerInvariant();
                    if (type.Length == 0 || type == "text/javascript" || type == "application/javascript")
                    {
                        if (scripts.Count == 8) { loaded = false; return new Xr("limit", 0, "At most eight inline classic scripts are supported."); }
                        var code = new StringBuilder();
                        foreach (var child in node.C) if (child.K == "#text") code.Append(child.X);
                        if (code.Length > 16384) { loaded = false; return new Xr("code", 0, "Inline code must be at most 16 KiB per block."); }
                        scripts.Add(code.ToString());
                    }
                }
                for (var i = node.C.Count - 1; i >= 0; i--) nodes.Push(node.C[i]);
            }
            if (scripts.Count == 0 && !handlers && !demanded)
            { loaded = false; dormant = true; return new Xr("ok", 0, ""); }
            foreach (var code in scripts)
            {
                var result = await At.ExA(code);
                count += result.N;
                if (!result.Ok) { loaded = false; return Convert(result, count - result.N); }
            }
            foreach (var type in loadRequested ? new[] { "DOMContentLoaded", "load" } : new[] { "DOMContentLoaded" })
            {
                var result = await At.EvA(null, type);
                count += result.N;
                if (!result.Ok) { loaded = false; return Convert(result, count - result.N); }
                if (type == "load") complete = true;
            }
            return new Xr("ok", count, "");
        }

        private static Xr Convert(Jr result, int count)
        { return new Xr(result.S, count + result.N, result.M, result.D); }
        public void St() { At.St(); }
        public void Dispose() { At.Dispose(); }
    }
}
