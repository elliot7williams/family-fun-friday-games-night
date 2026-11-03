namespace Lea.Engine
{
    // Cz is one bounded measured container-size record.
    public sealed class Cz
    {
        public Cz(N element, int width, int height)
        {
            E = element;
            W = width;
            H = height;
        }

        public N E { get; private set; }
        public int W { get; private set; }
        public int H { get; private set; }
    }
}
