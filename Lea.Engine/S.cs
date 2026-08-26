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
        public int Gl { get; internal set; }
        public int Gs { get; internal set; }
        public int Rl { get; internal set; }
        public int Rs { get; internal set; }
        public string Gn { get; internal set; }
        public string Ge { get; internal set; }
        public string Rn { get; internal set; }
        public string Re { get; internal set; }
        public string An { get; internal set; }
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
        internal int[] Gd { get; set; }
        internal int[] Rd { get; set; }
        internal Dictionary<string, int> Gnd { get; set; }
        internal Dictionary<string, int> Rnd { get; set; }
        internal Dictionary<string, int[]> Ad { get; set; }

        public int Vc { get { return Vr == null ? 0 : Vr.Count; } }
        public int Gc { get { return Gd == null ? 0 : Gd.Length; } }
        public int Rc { get { return Rd == null ? 0 : Rd.Length; } }
        public int Gnc { get { return Gnd == null ? 0 : Gnd.Count; } }
        public int Rnc { get { return Rnd == null ? 0 : Rnd.Count; } }
        public int Aa { get { return Ad == null ? 0 : Ad.Count; } }
        public int Ar { get; internal set; }
        public int Ac { get; internal set; }

        public int Gv(int index)
        {
            return Gd != null && index >= 0 && index < Gd.Length ? Gd[index] : 0;
        }

        public int Rv(int index)
        {
            return Rd != null && index >= 0 && index < Rd.Length ? Rd[index] : 0;
        }

        public int Gx(string name)
        {
            return GridLine(name, false);
        }

        public int Rx(string name)
        {
            return GridLine(name, true);
        }

        public int Ag(string name, int part)
        {
            int[] area;
            return !String.IsNullOrEmpty(name) && part >= 0 && part < 4 && Ad != null && Ad.TryGetValue(name, out area) ? area[part] : 0;
        }

        public string Cv(string name)
        {
            string value;
            return !String.IsNullOrEmpty(name) && Vr != null && Vr.TryGetValue(name, out value) ? value : "";
        }

        private int GridLine(string name, bool rows)
        {
            if (String.IsNullOrEmpty(name)) return 0;
            var names = rows ? Rnd : Gnd;
            int line;
            if (names != null && names.TryGetValue(name, out line)) return line;
            var start = name.EndsWith("-start", StringComparison.Ordinal);
            var end = name.EndsWith("-end", StringComparison.Ordinal);
            if (!start && !end) return 0;
            var suffix = start ? 6 : 4;
            int[] area;
            if (name.Length <= suffix || Ad == null || !Ad.TryGetValue(name.Substring(0, name.Length - suffix), out area)) return 0;
            var areaStart = rows ? area[0] : area[1];
            return end ? areaStart + (rows ? area[2] : area[3]) : areaStart;
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
            Gs = 1;
            Rs = 1;
            Gn = "";
            Ge = "";
            Rn = "";
            Re = "";
            An = "";
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
