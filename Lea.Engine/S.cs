namespace Lea.Engine
{
    // S is Lea's compact computed style.
    public sealed class S
    {
        public string F { get; internal set; }
        public string Bg { get; internal set; }
        public int Fs { get; internal set; }
        public bool B { get; internal set; }
        public bool I { get; internal set; }
        public string Ta { get; internal set; }
        public int Lh { get; internal set; }
        public int Lm { get; internal set; }
        public string D { get; internal set; }
        public int Mt { get; internal set; }
        public int Mr { get; internal set; }
        public int Mb { get; internal set; }
        public int Ml { get; internal set; }
        public int Pt { get; internal set; }
        public int Pr { get; internal set; }
        public int Pb { get; internal set; }
        public int Pl { get; internal set; }
        public int W { get; internal set; }
        public int H { get; internal set; }
        public int Bt { get; internal set; }
        public int Br { get; internal set; }
        public int Bb { get; internal set; }
        public int Bl { get; internal set; }
        public string Bc { get; internal set; }
        public string Bs { get; internal set; }

        internal S()
        {
            F = "";
            Bg = "";
            Ta = "";
            D = "";
            Bc = "";
            Bs = "";
        }

        internal S Inherit()
        {
            return new S { F = F, Fs = Fs, B = B, I = I, Ta = Ta, Lh = Lh, Lm = Lm };
        }
    }
}
