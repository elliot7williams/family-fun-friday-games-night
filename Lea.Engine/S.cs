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
