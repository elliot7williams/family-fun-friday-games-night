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
        public string Tc { get; internal set; }
        public string Tds { get; internal set; }
        public int Tw { get; internal set; }
        public int Uo { get; internal set; }
        public bool Uop { get; internal set; }
        public string Ta { get; internal set; }
        public string Dr { get; internal set; }
        public string Tt { get; internal set; }
        public int Ti { get; internal set; }
        public bool Tip { get; internal set; }
        public Sw Ts { get; internal set; }
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
        public int Wk { get { return FunctionKind(false, 0); } }
        public int H { get; internal set; }
        public int Hp { get { return HeightValue(0, 0); } }
        public int Ho { get { return HeightValue(0, 1); } }
        public int Hu { get { return HeightValue(0, 2); } }
        public int Hk { get { return FunctionKind(true, 0); } }
        public int Wn { get; internal set; }
        public int Wnp { get { return WidthValue(1, 0); } }
        public int Wno { get { return WidthValue(1, 1); } }
        public int Wnu { get { return WidthValue(1, 2); } }
        public int Wnk { get { return FunctionKind(false, 1); } }
        public int Wx { get; internal set; }
        public int Wxp { get { return WidthValue(2, 0); } }
        public int Wxo { get { return WidthValue(2, 1); } }
        public int Wxu { get { return WidthValue(2, 2); } }
        public int Wxk { get { return FunctionKind(false, 2); } }
        public int Hn { get; internal set; }
        public int Hnp { get { return HeightValue(1, 0); } }
        public int Hno { get { return HeightValue(1, 1); } }
        public int Hnu { get { return HeightValue(1, 2); } }
        public int Hnk { get { return FunctionKind(true, 1); } }
        public int Hx { get; internal set; }
        public int Hxp { get { return HeightValue(2, 0); } }
        public int Hxo { get { return HeightValue(2, 1); } }
        public int Hxu { get { return HeightValue(2, 2); } }
        public int Hxk { get { return FunctionKind(true, 2); } }
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
        internal int[] Wfd { get; set; }
        internal int[] Hfd { get; set; }
        internal string[] Be { get; set; }

        public int Vc { get { return Vr == null ? 0 : Vr.Count; } }
        public int Gc { get { return Gd == null ? 0 : Gd.Length; } }
        public int Rc { get { return Rd == null ? 0 : Rd.Length; } }
        public int Gac { get { return Gad == null ? 0 : Gad.Length; } }
        public int Rac { get { return Rad == null ? 0 : Rad.Length; } }
        public int Bec { get { return Be == null ? 0 : Be.Length; } }
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

        public string Bsv(int edge)
        {
            return Be != null && edge >= 0 && edge < 4 ? Be[edge] : Bs;
        }

        public string Bcv(int edge)
        {
            return Be != null && edge >= 0 && edge < 4 ? Be[edge + 4] : Bc;
        }

        internal void SetBorderStyle(int edge, string value)
        {
            value = value ?? "";
            if (edge < 0)
            {
                Bs = value;
                if (Be != null) for (var index = 0; index < 4; index++) Be[index] = value;
            }
            else if (edge < 4)
            {
                EnsureBorderEdges();
                Be[edge] = value;
            }
            CompactBorderEdges();
        }

        internal void SetBorderColor(int edge, string value)
        {
            value = value ?? "";
            if (edge < 0)
            {
                Bc = value;
                if (Be != null) for (var index = 4; index < 8; index++) Be[index] = value;
            }
            else if (edge < 4)
            {
                EnsureBorderEdges();
                Be[edge + 4] = value;
            }
            CompactBorderEdges();
        }

        private void EnsureBorderEdges()
        {
            if (Be != null) return;
            Be = new string[8];
            for (var index = 0; index < 4; index++)
            {
                Be[index] = Bs;
                Be[index + 4] = Bc;
            }
        }

        private void CompactBorderEdges()
        {
            if (Be == null) return;
            for (var index = 0; index < 4; index++)
                if (Be[index] != Bs || Be[index + 4] != Bc) return;
            Be = null;
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

        public int Wq(int part)
        {
            return FunctionCount(false, part);
        }

        public int Hq(int part)
        {
            return FunctionCount(true, part);
        }

        public int Wv(int part, int argument, int value)
        {
            return FunctionValue(false, part, argument, value);
        }

        public int Hv(int part, int argument, int value)
        {
            return FunctionValue(true, part, argument, value);
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

        internal void SetFunction(bool height, int part, int kind, int[] values)
        {
            if (part < 0 || part > 2 || kind < 0 || kind > 3) return;
            var data = height ? Hfd : Wfd;
            if (data == null && kind != 0) data = new int[42];
            if (data == null) return;
            var start = part * 14;
            for (var index = 0; index < 14; index++) data[start + index] = 0;
            if (kind != 0 && values != null && (values.Length == 8 || values.Length == 12))
            {
                data[start] = kind;
                data[start + 1] = values.Length / 4;
                Array.Copy(values, 0, data, start + 2, values.Length);
            }
            var any = false;
            for (var index = 0; index < data.Length; index++) if (data[index] != 0) { any = true; break; }
            if (height) Hfd = any ? data : null;
            else Wfd = any ? data : null;
        }

        internal void CopyFunction(bool height, int part, S inherited)
        {
            if (inherited == null) { SetFunction(height, part, 0, null); return; }
            var kind = inherited.FunctionKind(height, part);
            var count = inherited.FunctionCount(height, part);
            if (kind == 0 || count < 2 || count > 3) { SetFunction(height, part, 0, null); return; }
            var values = new int[count * 4];
            for (var argument = 0; argument < count; argument++)
                for (var value = 0; value < 4; value++)
                    values[argument * 4 + value] = inherited.FunctionValue(height, part, argument, value);
            SetFunction(height, part, kind, values);
        }

        private int WidthValue(int part, int value)
        {
            return Wd == null ? 0 : Wd[part * 3 + value];
        }

        private int HeightValue(int part, int value)
        {
            return Hd == null ? 0 : Hd[part * 3 + value];
        }

        private int FunctionKind(bool height, int part)
        {
            var data = height ? Hfd : Wfd;
            return data == null || part < 0 || part > 2 ? 0 : data[part * 14];
        }

        private int FunctionCount(bool height, int part)
        {
            var data = height ? Hfd : Wfd;
            return data == null || part < 0 || part > 2 ? 0 : data[part * 14 + 1];
        }

        private int FunctionValue(bool height, int part, int argument, int value)
        {
            var data = height ? Hfd : Wfd;
            return data == null || part < 0 || part > 2 || argument < 0 || argument > 2 || value < 0 || value > 3 ?
                0 : data[part * 14 + 2 + argument * 4 + value];
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
            Tc = "";
            Tds = "";
            Ta = "";
            Dr = "l";
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
            var style = new S { F = F, Fs = Fs, Ff = Ff, B = B, I = I, Td = Td, Tc = Tc, Tds = Tds, Tw = Tw, Uo = Uo, Uop = Uop, Ta = Ta, Dr = Dr, Tt = Tt, Ti = Ti, Tip = Tip, Ts = Ts, Ls = Ls, Wg = Wg, Lh = Lh, Lm = Lm, Ws = Ws, Wb = Wb, Cu = Cu, Pe = Pe, Vi = Vi, Op = Op, Ao = Op };
            style.Vr = Vr;
            style.Vo = Vo;
            return style;
        }
    }
}
