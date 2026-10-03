namespace Lea.Script
{
    public sealed class Jr
    {
        internal Jr(string status, int count, string message)
        { S = status; N = count; M = message; }

        public string S { get; private set; }
        public int N { get; private set; }
        public string M { get; private set; }
        public bool Ok { get { return S == "ok"; } }
    }
}
