using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Lea.Tests
{
    internal static class ImageHandshake
    {
        private static void Main()
        {
            using (var writer = new BinaryWriter(Console.OpenStandardOutput(), Encoding.UTF8))
            {
                writer.Write(0x3241454c);
                writer.Flush();
                Console.Error.WriteLine("image-stall header flushed");
                Thread.Sleep(60000);
            }
        }
    }
}
