using System;
using System.Collections.Generic;

namespace Lea.Engine
{
    // S is Lea's compact computed style.
    public sealed class S
    {
        public string F { get; internal set; }
        public string Bg { get; internal set; }
        public int Fs { get; internal set; }
        public string Ff { get; internal set; }
        public bool B { get; internal set; }
        public bool I { get; internal set; }
        public string Td { get; internal set; }
        public string Ta { get; internal set; }
        public int Lh { get; internal set; }
        public int Lm { get; internal set; }
        public string Ws { get; internal set; }
        public string Wb { get; internal set; }
        public string Ov { get; internal set; }
        public string Vi { get; internal set; }
        public int Op { get; internal set; }
        public string D { get; internal set; }
        public string Fd { get; internal set; }
        public string Jc { get; internal set; }
        public string Ai { get; internal set; }
        public string As { get; internal set; }
        public int Gp { get; internal set; }
        public int Rg { get; internal set; }
        public string Wr { get; internal set; }
        public int Fg { get; internal set; }
        public int Sh { get; internal set; }
        public int Fb { get; internal set; }
        public int Or { get; internal set; }
        public string Pn { get; internal set; }
        public int? Tp { get; internal set; }
        public int? Rt { get; internal set; }
        public int? Bm { get; internal set; }
        public int? Lt { get; internal set; }
        public int Zi { get; internal set; }
        public bool Za { get; internal set; }
        public bool Zc { get; internal set; }
        public int Tx { get; internal set; }
        public int Ty { get; internal set; }
        public int Mt { get; internal set; }
        public int Mr { get; internal set; }
        public int Mb { get; internal set; }
        public int Ml { get; internal set; }
        public int Pt { get; internal set; }
        public int Pr { get; internal set; }
        public int Pb { get; internal set; }
        public int Pl { get; internal set; }
        public string Bz { get; internal set; }
        public int W { get; internal set; }
        public int H { get; internal set; }
        public int Wn { get; internal set; }
        public int Wx { get; internal set; }
        public int Hn { get; internal set; }
        public int Hx { get; internal set; }
        public int Bt { get; internal set; }
        public int Br { get; internal set; }
        public int Bb { get; internal set; }
        public int Bl { get; internal set; }
        public string Bc { get; internal set; }
        public string Bs { get; internal set; }
        internal int Ao { get; set; }
        internal bool Zo { get; set; }
        internal bool Zs { get; set; }
        internal bool Zt { get; set; }
        internal Dictionary<string, string> Vr { get; set; }

        public int Vc { get { return Vr == null ? 0 : Vr.Count; } }

        public string Cv(string name)
        {
            string value;
            return !String.IsNullOrEmpty(name) && Vr != null && Vr.TryGetValue(name, out value) ? value : "";
        }

        internal S()
        {
            F = "";
            Bg = "";
            Ff = "";
            Td = "";
            Ta = "";
            Ws = "";
            Wb = "";
            Ov = "";
            Vi = "";
            Op = 100;
            Ao = 100;
            D = "";
            Fd = "r";
            Jc = "s";
            Ai = "t";
            As = "";
            Wr = "n";
            Sh = 1;
            Fb = -1;
            Bz = "c";
            Pn = "";
            Bc = "";
            Bs = "";
        }

        internal S Inherit()
        {
            var style = new S { F = F, Fs = Fs, Ff = Ff, B = B, I = I, Td = Td, Ta = Ta, Lh = Lh, Lm = Lm, Ws = Ws, Wb = Wb, Vi = Vi, Op = Op, Ao = Op };
            style.Vr = Vr;
            return style;
        }
    }
}
