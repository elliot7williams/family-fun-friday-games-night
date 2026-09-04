using System;
using System.Collections.Generic;

namespace Lea.Engine
{
    // S is Lea's compact computed style.
    public sealed class S
    {
        public const int Av = -65;

        public string F { get; internal set; }
        public string Bg { get; internal set; }
        public Bi Bi { get; internal set; }
        public Gi Gi { get; internal set; }
        public int Fs { get; internal set; }
        public string Ff { get; internal set; }
        public bool B { get; internal set; }
        public bool I { get; internal set; }
        public string Td { get; internal set; }
        public string Ta { get; internal set; }
        public string Tt { get; internal set; }
        public int Ls { get; internal set; }
        public int Wg { get; internal set; }
        public int Lh { get; internal set; }
        public int Lm { get; internal set; }
        public string Ws { get; internal set; }
        public string Wb { get; internal set; }
        public string Ov { get; internal set; }
        public string To { get; internal set; }
        public string Cu { get; internal set; }
        public bool Pe { get; internal set; }
        public string Of { get; internal set; }
        public int Ox { get; internal set; }
        public int Oy { get; internal set; }
        public bool Oxp { get; internal set; }
        public bool Oyp { get; internal set; }
        public string Vi { get; internal set; }
        public string Cp { get; internal set; }
        public int Op { get; internal set; }
        public string D { get; internal set; }
        public string Fd { get; internal set; }
        public string Jc { get; internal set; }
        public string Al { get; internal set; }
        public string Ji { get; internal set; }
        public string Ai { get; internal set; }
        public string Js { get; internal set; }
        public string As { get; internal set; }
        public int Gp { get; internal set; }
        public int Rg { get; internal set; }
        public string Gf { get; internal set; }
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
        public int Wp { get { return WidthValue(0, 0); } }
        public int Wo { get { return WidthValue(0, 1); } }
        public int Wu { get { return WidthValue(0, 2); } }
        public int H { get; internal set; }
        public int Hp { get { return HeightValue(0, 0); } }
        public int Ho { get { return HeightValue(0, 1); } }
        public int Hu { get { return HeightValue(0, 2); } }
        public int Wn { get; internal set; }
        public int Wnp { get { return WidthValue(1, 0); } }
        public int Wno { get { return WidthValue(1, 1); } }
        public int Wnu { get { return WidthValue(1, 2); } }
        public int Wx { get; internal set; }
        public int Wxp { get { return WidthValue(2, 0); } }
        public int Wxo { get { return WidthValue(2, 1); } }
        public int Wxu { get { return WidthValue(2, 2); } }
        public int Hn { get; internal set; }
        public int Hnp { get { return HeightValue(1, 0); } }
        public int Hno { get { return HeightValue(1, 1); } }
        public int Hnu { get { return HeightValue(1, 2); } }
        public int Hx { get; internal set; }
        public int Hxp { get { return HeightValue(2, 0); } }
        public int Hxo { get { return HeightValue(2, 1); } }
        public int Hxu { get { return HeightValue(2, 2); } }
        public double Rw { get; internal set; }
        public double Rh { get; internal set; }
        public bool Ra { get; internal set; }
        public int Bt { get; internal set; }
        public int Br { get; internal set; }
        public int Bb { get; internal set; }
        public int Bl { get; internal set; }
        public string Bc { get; internal set; }
        public string Bs { get; internal set; }
        public int Ow { get; internal set; }
        public string Os { get; internal set; }
        public string Oc { get; internal set; }
        public int Oo { get; internal set; }
        public int R0 { get; internal set; }
        public int R1 { get; internal set; }
        public int R2 { get; internal set; }
        public int R3 { get; internal set; }
        public int Q0 { get; internal set; }
        public int Q1 { get; internal set; }
        public int Q2 { get; internal set; }
        public int Q3 { get; internal set; }
        public IList<Sw> Sw { get; private set; }
        internal int Ao { get; set; }
        internal bool Zo { get; set; }
        internal bool Zs { get; set; }
        internal bool Zt { get; set; }
        internal Dictionary<string, string> Vr { get; set; }
        internal Dictionary<string, Uri> Vo { get; set; }
        internal int[] Gd { get; set; }
        internal int[] Rd { get; set; }
        internal int[] Gad { get; set; }
        internal int[] Rad { get; set; }
        internal Dictionary<string, int> Gnd { get; set; }
        internal Dictionary<string, int> Rnd { get; set; }
        internal Dictionary<string, int[]> Ad { get; set; }
        internal int[] Wd { get; set; }
        internal int[] Hd { get; set; }

        public int Vc { get { return Vr == null ? 0 : Vr.Count; } }
        public int Gc { get { return Gd == null ? 0 : Gd.Length; } }
        public int Rc { get { return Rd == null ? 0 : Rd.Length; } }
        public int Gac { get { return Gad == null ? 0 : Gad.Length; } }
        public int Rac { get { return Rad == null ? 0 : Rad.Length; } }
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

        public int Gav(int index)
        {
            return Gad != null && index >= 0 && index < Gad.Length ? Gad[index] : 0;
        }

        public int Rav(int index)
        {
            return Rad != null && index >= 0 && index < Rad.Length ? Rad[index] : 0;
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

        internal void SetWidth(int part, int percent, int offset, int unit)
        {
            if (part < 0 || part > 2) return;
            if (Wd == null && (percent != 0 || offset != 0 || unit != 0)) Wd = new int[9];
            if (Wd == null) return;
            Wd[part * 3] = percent;
            Wd[part * 3 + 1] = offset;
            Wd[part * 3 + 2] = unit;
            for (var index = 0; index < Wd.Length; index++) if (Wd[index] != 0) return;
            Wd = null;
        }

        internal void SetHeight(int part, int percent, int offset, int unit)
        {
            if (part < 0 || part > 2) return;
            if (Hd == null && (percent != 0 || offset != 0 || unit != 0)) Hd = new int[9];
            if (Hd == null) return;
            Hd[part * 3] = percent;
            Hd[part * 3 + 1] = offset;
            Hd[part * 3 + 2] = unit;
            for (var index = 0; index < Hd.Length; index++) if (Hd[index] != 0) return;
            Hd = null;
        }

        private int WidthValue(int part, int value)
        {
            return Wd == null ? 0 : Wd[part * 3 + value];
        }

        private int HeightValue(int part, int value)
        {
            return Hd == null ? 0 : Hd[part * 3 + value];
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
            Tt = "n";
            Ws = "";
            Wb = "";
            Ov = "";
            To = "c";
            Cu = "";
            Pe = true;
            Of = "f";
            Ox = 50;
            Oy = 50;
            Oxp = true;
            Oyp = true;
            Vi = "";
            Cp = "";
            Op = 100;
            Ao = 100;
            D = "";
            Fd = "r";
            Jc = "s";
            Al = "s";
            Ji = "t";
            Ai = "t";
            Js = "";
            As = "";
            Gf = "r";
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
            Ra = true;
            Pn = "";
            Bc = "";
            Bs = "";
            Os = "n";
            Oc = "";
            Sw = new List<Sw>();
        }

        internal S Inherit()
        {
            var style = new S { F = F, Fs = Fs, Ff = Ff, B = B, I = I, Td = Td, Ta = Ta, Tt = Tt, Ls = Ls, Wg = Wg, Lh = Lh, Lm = Lm, Ws = Ws, Wb = Wb, Cu = Cu, Pe = Pe, Vi = Vi, Op = Op, Ao = Op };
            style.Vr = Vr;
            style.Vo = Vo;
            return style;
        }
    }
}
