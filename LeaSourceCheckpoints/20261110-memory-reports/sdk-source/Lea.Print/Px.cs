using System;
using System.IO;
using System.IO.Packaging;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;

namespace Lea.Print
{
    public static class Px
    {
        // Caller exclusively owns the empty destination; failures retain partial output.
        public static int Wr(Pp pages, Stream destination)
        { return Write(pages, destination); }

        public static int Rg(Pr pages, Stream destination)
        { return Write(pages, destination); }

        private static int Write(DocumentPaginator pages, Stream destination)
        {
            if (pages == null) throw new ArgumentNullException("pages");
            int count = pages.PageCount;
            if (destination == null) throw new ArgumentNullException("destination");
            if (!destination.CanRead || !destination.CanWrite || !destination.CanSeek)
                throw new ArgumentException("Readable writable seekable destination required", "destination");
            if (destination.Length != 0 || destination.Position != 0)
                throw new ArgumentException("Empty destination at position zero required", "destination");
            using (var package = Package.Open(new BorrowedStream(destination), FileMode.Create, FileAccess.ReadWrite))
            using (var document = new XpsDocument(package, CompressionOption.Maximum))
                XpsDocument.CreateXpsDocumentWriter(document).Write(pages);
            destination.Flush();
            return count;
        }

        private sealed class BorrowedStream : Stream
        {
            private readonly Stream stream;
            internal BorrowedStream(Stream stream) { this.stream = stream; }
            public override bool CanRead { get { return stream.CanRead; } }
            public override bool CanWrite { get { return stream.CanWrite; } }
            public override bool CanSeek { get { return stream.CanSeek; } }
            public override long Length { get { return stream.Length; } }
            public override long Position { get { return stream.Position; } set { stream.Position = value; } }
            public override void Flush() { stream.Flush(); }
            public override int Read(byte[] buffer, int offset, int count) { return stream.Read(buffer, offset, count); }
            public override void Write(byte[] buffer, int offset, int count) { stream.Write(buffer, offset, count); }
            public override long Seek(long offset, SeekOrigin origin) { return stream.Seek(offset, origin); }
            public override void SetLength(long value) { stream.SetLength(value); }
            protected override void Dispose(bool disposing) { base.Dispose(disposing); }
        }
    }
}
