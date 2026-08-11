using System;
using System.Collections.Generic;

namespace LeaForge
{
    public static class L
    {
        public static E E()
        {
            return new E();
        }
    }

    // Compact public API for small Lea programs.
    public sealed class E
    {
        private readonly ForgeEngine engine = new ForgeEngine();
        private readonly ForgeTab tab = new ForgeTab();

        public P At
        {
            get { return Wrap(tab, tab.Current); }
        }

        public P Go(string input)
        {
            return Wrap(tab, engine.Open(tab, input));
        }

        public P Bk(P tab)
        {
            return Wrap(tab.Tab, engine.Back(tab.Tab));
        }

        public P Fw(P tab)
        {
            return Wrap(tab.Tab, engine.Forward(tab.Tab));
        }

        public P Rl(P tab)
        {
            return Wrap(tab.Tab, engine.Reload(tab.Tab));
        }

        private static P Wrap(ForgeTab tab, ForgePage page)
        {
            return page == null ? null : new P(tab, page);
        }
    }

    public sealed class P
    {
        internal readonly ForgePage Page;
        internal readonly ForgeTab Tab;

        internal P(ForgeTab tab, ForgePage page)
        {
            Page = page;
            Tab = tab;
        }

        public Uri U { get { return Page.Url; } }
        public string T { get { return Page.Title; } }
        public string H { get { return Page.Html; } }
        public int N { get { return Page.Nodes.Count; } }
        public int Ls { get { return Page.Links.Count; } }
    }
}
