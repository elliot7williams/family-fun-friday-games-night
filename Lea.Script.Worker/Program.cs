using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Jint;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Lea.Script;

namespace Lea.Script.Worker
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 1 || args[0] != "--pipe") return 2;
            try
            {
                var guards = Lea.Image.Mg.Apply();
                using (var input = Console.OpenStandardInput())
                using (var output = Console.OpenStandardOutput())
                {
                    var init = Wire.Read(input);
                    Wire.Fields(init, "k", "p", "f");
                    if (Wire.Text(init, "k", 8) != "init") return 3;
                    var network = NetworkDenied(Wire.Number(init, "p"));
                    var file = FileDenied(Wire.Text(init, "f", 1024));
                    var id = 0;
                    var request = 0;
                    var vm = new Jint.Engine(options =>
                    {
                        options.MaxStatements(20000).LimitMemory(8 * 1024 * 1024)
                            .TimeoutInterval(TimeSpan.FromSeconds(5)).LimitRecursion(64)
                            .RegexTimeoutInterval(TimeSpan.FromMilliseconds(100)).MaxArraySize(65536);
                        options.Constraints.StackOverflowGuard = true;
                        options.Interop.Enabled = false;
                        options.Interop.AllowGetType = false;
                        options.Interop.AllowWrite = false;
                    });
                    vm.SetValue("__lea", new ClrFunction(vm, "__lea", (_, values) =>
                    {
                        if (++request > 256) throw new InvalidDataException("Script host-call budget exceeded.");
                        Wire.Write(output, new { k = "call", i = id, r = request, b = Codec.Arguments(values) });
                        var reply = Wire.Read(input);
                        Wire.Fields(reply, "k", "i", "r", "v");
                        if (Wire.Text(reply, "k", 8) != "reply" || Wire.Number(reply, "i") != id ||
                            Wire.Number(reply, "r") != request || !reply.ContainsKey("v"))
                            throw new InvalidDataException("Invalid script bridge reply.");
                        return Codec.Value(vm, reply["v"]);
                    }));
                    using (var stream = typeof(Program).Assembly.GetManifestResourceStream("Lea.Script.Dom.js"))
                    using (var reader = new StreamReader(stream)) vm.Execute(reader.ReadToEnd());
                    Wire.Write(output, new { k = "hello", g = guards, n = network, f = file ? 1 : 0 });
                    while (true)
                    {
                        var frame = Wire.Read(input);
                        Wire.Fields(frame, "k", "i", "c");
                        if (Wire.Text(frame, "k", 8) != "exec") return 3;
                        var next = Wire.Number(frame, "i");
                        if (next <= id) return 3;
                        id = next;
                        request = 0;
                        var code = Wire.Text(frame, "c", 16 * 1024);
                        try
                        {
                            vm.Execute(code);
                            Wire.Write(output, new { k = "end", i = id, s = "ok", m = "" });
                        }
                        catch (Exception error) when (error is JavaScriptException || error is JintException ||
                            error is Acornima.ParseErrorException || error is TimeoutException || error is InvalidDataException)
                        {
                            var limited = error is TimeoutException || error is StatementsCountOverflowException ||
                                error is MemoryLimitExceededException || error is RecursionDepthOverflowException ||
                                error is InvalidDataException;
                            var message = limited ? "Script budget exceeded: " + error.GetType().Name + "." : error.Message;
                            Wire.Write(output, new { k = "end", i = id, s = limited ? "limit" : "js",
                                m = message.Length <= 512 ? message : message.Substring(0, 512) });
                            return 0;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                try { Console.Error.WriteLine(error.GetType().Name + ": " + error.Message); } catch { }
                return 4;
            }
        }

        private static int NetworkDenied(int port)
        {
            if (port < 1 || port > 65535) return 0;
            using (var client = new TcpClient())
            {
                try
                {
                    var connect = client.BeginConnect(IPAddress.Loopback, port, null, null);
                    using (var wait = connect.AsyncWaitHandle)
                    {
                        // WFP can silently drop a denied connection rather than return AccessDenied.
                        // The parent independently verifies no exemptions and a working listener.
                        if (!wait.WaitOne(500)) return 2;
                        client.EndConnect(connect);
                        return 0;
                    }
                }
                catch (SocketException error)
                {
                    return error.SocketErrorCode == SocketError.AccessDenied ? 1 : 0;
                }
            }
        }

        private static bool FileDenied(string path)
        {
            try { using (var file = File.OpenRead(path)) file.ReadByte(); return false; }
            catch (UnauthorizedAccessException) { return true; }
        }
    }
}
