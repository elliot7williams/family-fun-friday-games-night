using System;

namespace Lea.Engine
{
    // Er is Lea's compact typed loading failure.
    public sealed class Er : Exception
    {
        internal Er(string kind, Uri url, int status, bool retryable, string message, Exception inner)
            : base(message, inner)
        {
            K = kind;
            U = url;
            St = status;
            Rt = retryable;
        }

        public string K { get; private set; }
        public Uri U { get; private set; }
        public int St { get; private set; }
        public bool Rt { get; private set; }
    }
}
