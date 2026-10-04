using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Lea.Script.Worker
{
    internal sealed class NetworkProbe : IDisposable
    {
        private TcpClient client;
        private IAsyncResult connect;
        private Stopwatch clock;
        private int denied;

        internal NetworkProbe(int port)
        {
            if (port < 1 || port > 65535) return;
            try
            {
                client = new TcpClient();
                connect = client.BeginConnect(IPAddress.Loopback, port, null, null);
                clock = Stopwatch.StartNew();
            }
            catch (SocketException error)
            { denied = error.SocketErrorCode == SocketError.AccessDenied ? 1 : 0; }
            catch { Dispose(); throw; }
        }

        internal int Denied()
        {
            if (connect == null) return denied;
            try
            {
                using (var wait = connect.AsyncWaitHandle)
                {
                    // WFP may silently drop a denied connection. The parent checks
                    // no exemptions and a working listener before trusting this result.
                    var remaining = (int)Math.Max(0, 500 - clock.ElapsedMilliseconds);
                    if (!wait.WaitOne(remaining)) return 2;
                    client.EndConnect(connect);
                    return 0;
                }
            }
            catch (SocketException error)
            { return error.SocketErrorCode == SocketError.AccessDenied ? 1 : 0; }
        }

        public void Dispose() { if (client != null) client.Close(); }
    }
}
