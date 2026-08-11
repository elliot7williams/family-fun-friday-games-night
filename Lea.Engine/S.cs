namespace Lea.Engine
{
    // S is Lea's compact computed style.
    public sealed class S
    {
        public string F { get; internal set; }
        public string Bg { get; internal set; }
        public int Fs { get; internal set; }
        public bool B { get; internal set; }
        public string D { get; internal set; }
        public int Mt { get; internal set; }
        public int Mr { get; internal set; }
        public int Mb { get; internal set; }
        public int Ml { get; internal set; }
        public int Pt { get; internal set; }
        public int Pr { get; internal set; }
        public int Pb { get; internal set; }
        public int Pl { get; internal set; }

        internal S()
        {
            F = "";
            Bg = "";
            D = "";
        }

        internal S Inherit()
        {
            return new S { F = F, Fs = Fs, B = B };
        }
    }
}
