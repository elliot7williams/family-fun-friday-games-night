using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Lea.Tests
{
    internal static class ImageHandshake
    {
        private static void Main(string[] args)
        {
            using (var writer = new BinaryWriter(Console.OpenStandardOutput(), Encoding.UTF8))
            {
                var invalid = Array.IndexOf(args, "--invalid-hello") >= 0;
                writer.Write(invalid ? 0x3141454c : 0x3241454c);
                writer.Flush();
                Console.Error.WriteLine(invalid ? "image-invalid header flushed" : "image-stall header flushed");
                Thread.Sleep(60000);
            }
        }
    }
}
