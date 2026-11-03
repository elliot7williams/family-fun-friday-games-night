using System;
using System.IO;
using Lea.Engine;
using Lea.Script;

internal static class IsConnectedMain
{
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
    private sealed class Session : IDisposable
    {
        private readonly Js trusted;
        private readonly Jp worker;
        internal Session(E engine, P page, bool isolated)
        {
            if (isolated) worker = new Jp(engine, page) { Tm = 5000 };
            else trusted = new Js(engine, page);
        }
        internal Jr Ex(string code) { return worker == null ? trusted.Ex(code) : worker.Ex(code); }
        internal void Guards()
        {
            if (worker != null) Require(worker.Ac && worker.Nd && worker.Ne && worker.Fd && worker.Cp == 2500 && worker.Mm == 128L * 1024 * 1024, "worker isolation guards remain active");
        }
        public void Dispose()
        {
            if (trusted != null) trusted.Dispose();
            if (worker != null) { worker.Dispose(); Require(!worker.Pc && worker.Pid == 0, "worker retirement must complete"); }
        }
    }
    public static int Main(string[] args)
    {
        try
        {
            bool isolated = args.Length == 3 && args[2] == "isolated";
            var engine = new E { Jb = false };
            var page = engine.Go(args[0]);
            using (var script = new Session(engine, page, isolated))
            {
                var changes = page.Jc;
                var read = script.Ex("if(!document.isConnected||!document.getElementById('child').isConnected)throw Error('connected read');");
                Require(read.Ok && read.N == 0 && page.Jc == changes, "connectivity read must not mutate: " + read.S + "; " + read.M);
                script.Guards();
                var semantics = script.Ex(File.ReadAllText(args[1]));
                Require(semantics.Ok && semantics.N == 1, "connectivity semantics and one removal: " + semantics.S + "; " + semantics.M);
                engine.Je = false;
                Require(script.Ex("document.isConnected;").S == "off", "revoked scripting remains off");
            }
            engine = new E { Jb = false };
            page = engine.Go(args[0]);
            using (var script = new Session(engine, page, isolated))
            {
                var group = engine.Id(page, "group");
                var child = engine.Id(page, "child");
                var up = typeof(N).GetProperty("Up");
                var original = group.Up;
                up.SetValue(group, child, null);
                try { Require(script.Ex("document.getElementById('child').isConnected;").S == "limit", "cyclic native ancestry must remain bounded"); }
                finally { up.SetValue(group, original, null); }
                Require(script.Ex("if(!document.getElementById('child').isConnected)throw Error('recovery');").Ok, "typed limit must permit recovery");
                script.Guards();
            }
            Console.WriteLine("PASS Lea isConnected " + (isolated ? "isolated" : "trusted") + ": nonmutating read, descriptors, brands, read-only, detached reads/write guard, revoked permission, cycle limit and recovery.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
