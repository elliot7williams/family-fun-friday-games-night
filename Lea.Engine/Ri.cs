using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Lea.Engine
{
    // Limits both encoded input and expanded bytes before they enter retained buffers.
    internal sealed class Ri : Stream
    {
        private readonly Stream input;
        private readonly Uri url;
        private readonly int limit;
        private readonly CancellationToken stop;
        private long read;

        private Ri(Stream input, Uri url, int limit, CancellationToken stop)
        {
            this.input = input;
            this.url = url;
            this.limit = limit;
            this.stop = stop;
        }

        internal static void Check(Uri url, long size, int limit)
        {
            if (size > limit)
                throw new Er("large", url, 0, false, "Lea response exceeds its " + limit + " byte input limit.", null);
        }

        private static Stream Decoded(Ri wire, string encoding)
        {
            var kind = (encoding ?? "").Trim().ToLowerInvariant();
            if (kind == "" || kind == "identity") return wire;
            Stream decoder;
            if (kind == "gzip") decoder = new GZipStream(wire, CompressionMode.Decompress);
            else if (kind == "deflate") decoder = new DeflateStream(wire, CompressionMode.Decompress);
            else throw new InvalidDataException("Unsupported response content encoding.");
            return new Ri(decoder, wire.url, wire.limit, wire.stop);
        }

        internal static byte[] Bytes(Stream source, Uri url, long length, string encoding, int limit, CancellationToken stop)
        {
            stop.ThrowIfCancellationRequested();
            Check(url, length, limit);
            using (var wire = new Ri(source, url, limit, stop))
            using (var decoded = Decoded(wire, encoding))
            using (var output = new MemoryStream())
            {
                var buffer = new byte[32768];
                int count;
                while ((count = decoded.Read(buffer, 0, buffer.Length)) != 0) output.Write(buffer, 0, count);
                return output.ToArray();
            }
        }

        internal static async Task<byte[]> BytesA(Stream source, Uri url, long length, string encoding, int limit, CancellationToken stop)
        {
            stop.ThrowIfCancellationRequested();
            Check(url, length, limit);
            using (var wire = new Ri(source, url, limit, stop))
            using (var decoded = Decoded(wire, encoding))
            using (var output = new MemoryStream())
            {
                var buffer = new byte[32768];
                int count;
                while ((count = await decoded.ReadAsync(buffer, 0, buffer.Length, stop).ConfigureAwait(false)) != 0)
                    output.Write(buffer, 0, count);
                return output.ToArray();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            stop.ThrowIfCancellationRequested();
            var size = input.Read(buffer, offset, (int)Math.Min(count, limit - read + 1));
            return Count(size);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            stop.ThrowIfCancellationRequested();
            var size = await input.ReadAsync(buffer, offset, (int)Math.Min(count, limit - read + 1), cancellationToken).ConfigureAwait(false);
            return Count(size);
        }

        private int Count(int size)
        {
            stop.ThrowIfCancellationRequested();
            read += size;
            Check(url, read, limit);
            return size;
        }

        public override bool CanRead { get { return input.CanRead; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }
        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        protected override void Dispose(bool disposing)
        {
            if (disposing) input.Dispose();
            base.Dispose(disposing);
        }
    }
}
