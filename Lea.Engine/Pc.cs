using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Lea.Engine
{
    // Pc is Lea's optional bounded persistent representation cache.
    internal sealed class Pc
    {
        private const int Magic = 0x3141454C;
        private const int Version = 1;
        private const int MaxEntries = 128;
        private const long MaxBytes = 128L * 1024 * 1024;
        private const int MaxKey = 4096;
        private const int MaxType = 256;
        private const int MaxEncoding = 64;
        private const int MaxValidator = 1024;
        private const int MaxBody = 6 * 1024 * 1024;
        private const int MaxFile = MaxBody + 16 * 1024;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        private readonly object gate = new object();
        private string path = "";
        private int mx = 32;
        private long mb = 16L * 1024 * 1024;
        private int hits;
        private int writes;
        private int recovers;
        private int evicts;

        public string P
        {
            get { lock (gate) return path; }
            set
            {
                var next = (value ?? "").Trim();
                if (next.Length > 0)
                {
                    next = Path.GetFullPath(next);
                    if (File.Exists(next)) throw new ArgumentException("Lea requires a cache directory.", "value");
                }
                lock (gate)
                {
                    path = next;
                    Trim();
                }
            }
        }

        public int Mx
        {
            get { lock (gate) return mx; }
            set { lock (gate) { mx = Math.Max(0, Math.Min(MaxEntries, value)); Trim(); } }
        }

        public long Mb
        {
            get { lock (gate) return mb; }
            set { lock (gate) { mb = Math.Max(0, Math.Min(MaxBytes, value)); Trim(); } }
        }

        public bool On { get { lock (gate) return path.Length > 0 && mx > 0 && mb > 0; } }
        public int N { get { lock (gate) return Files().Count; } }
        public long B { get { lock (gate) return Size(Files()); } }
        public int H { get { lock (gate) return hits; } }
        public int W { get { lock (gate) return writes; } }
        public int R { get { lock (gate) return recovers; } }
        public int E { get { lock (gate) return evicts; } }

        // Gt returns 1 for a decoded hit, 2 for a recovered corrupt entry, or 0.
        public int Gt(string key, out Pr item)
        {
            item = null;
            lock (gate)
            {
                if (path.Length == 0 || mx == 0 || mb == 0) return 0;
                string file;
                try { file = FileFor(key); }
                catch (Exception error)
                {
                    if (!Optional(error)) throw;
                    return 0;
                }
                if (!File.Exists(file)) return 0;
                try
                {
                    item = Read(file, key);
                    try { File.SetLastWriteTimeUtc(file, DateTime.UtcNow); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                    hits++;
                    return 1;
                }
                catch (InvalidDataException) { return Recover(file); }
                catch (EndOfStreamException) { return Recover(file); }
                catch (DecoderFallbackException) { return Recover(file); }
                catch (FormatException) { return Recover(file); }
                catch (IOException) { return 0; }
                catch (UnauthorizedAccessException) { return 0; }
            }
        }

        public bool Pt(string key, Pr item)
        {
            if (item == null) return false;
            lock (gate)
            {
                if (path.Length == 0 || mx == 0 || mb == 0) return false;
                string file = null;
                string temp = null;
                try
                {
                    Directory.CreateDirectory(path);
                    file = FileFor(key);
                    temp = Path.Combine(path, ".lea-" + Guid.NewGuid().ToString("N") + ".tmp");
                    Write(temp, key, item);
                    if (new FileInfo(temp).Length > mb)
                    {
                        Delete(temp);
                        Rm(key);
                        return false;
                    }
                    if (File.Exists(file)) File.Replace(temp, file, null, true);
                    else
                    {
                        try { File.Move(temp, file); }
                        catch (IOException)
                        {
                            if (!File.Exists(file)) throw;
                            File.Replace(temp, file, null, true);
                        }
                    }
                    writes++;
                    Trim();
                    return File.Exists(file);
                }
                catch (InvalidDataException) { Delete(temp); return false; }
                catch (IOException) { Delete(temp); return false; }
                catch (UnauthorizedAccessException) { Delete(temp); return false; }
                catch (NotSupportedException) { Delete(temp); return false; }
                catch (ArgumentException) { Delete(temp); return false; }
            }
        }

        public void Rm(string key)
        {
            lock (gate)
            {
                if (path.Length == 0) return;
                try { Delete(FileFor(key)); }
                catch (Exception error) { if (!Optional(error)) throw; }
            }
        }

        public void Cl()
        {
            lock (gate)
                foreach (var file in Files()) Delete(file.FullName);
        }

        private int Recover(string file)
        {
            recovers++;
            Delete(file);
            return 2;
        }

        private void Trim()
        {
            var files = Files();
            files.Sort(delegate(FileInfo left, FileInfo right)
            {
                var order = left.LastWriteTimeUtc.CompareTo(right.LastWriteTimeUtc);
                return order != 0 ? order : StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
            });
            var bytes = Size(files);
            while (files.Count > 0 && (files.Count > mx || bytes > mb))
            {
                var file = files[0];
                files.RemoveAt(0);
                var length = SafeLength(file);
                if (!Delete(file.FullName)) continue;
                bytes = Math.Max(0, bytes - length);
                evicts++;
            }
        }

        private List<FileInfo> Files()
        {
            var result = new List<FileInfo>();
            if (path.Length == 0 || !Directory.Exists(path)) return result;
            try
            {
                foreach (var name in Directory.GetFiles(path, "*.lea", SearchOption.TopDirectoryOnly))
                {
                    var stem = Path.GetFileNameWithoutExtension(name);
                    if (stem.Length != 64 || !Hex(stem)) continue;
                    result.Add(new FileInfo(name));
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return result;
        }

        private static bool Hex(string value)
        {
            foreach (var ch in value)
                if (!((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F'))) return false;
            return true;
        }

        private static long Size(IEnumerable<FileInfo> files)
        {
            long total = 0;
            foreach (var file in files)
            {
                var length = SafeLength(file);
                total = length > Int64.MaxValue - total ? Int64.MaxValue : total + length;
            }
            return total;
        }

        private static long SafeLength(FileInfo file)
        {
            try { return Math.Max(0, file.Length); }
            catch (IOException) { return 0; }
            catch (UnauthorizedAccessException) { return 0; }
        }

        private string FileFor(string key)
        {
            using (var hash = SHA256.Create())
            {
                var bytes = hash.ComputeHash(Utf8.GetBytes(key ?? ""));
                var name = new StringBuilder(64);
                foreach (var value in bytes) name.Append(value.ToString("x2"));
                return Path.Combine(path, name + ".lea");
            }
        }

        private static void Write(string file, string key, Pr item)
        {
            var raw = Utf8.GetBytes(item.H ?? "");
            if (raw.Length > MaxBody) throw new InvalidDataException("Lea cache body is too large.");
            byte[] packed;
            using (var output = new MemoryStream())
            {
                using (var zip = new GZipStream(output, CompressionMode.Compress, true)) zip.Write(raw, 0, raw.Length);
                packed = output.ToArray();
            }
            using (var stream = new FileStream(file, FileMode.CreateNew, FileAccess.Write, FileShare.None, 32768, FileOptions.WriteThrough))
            using (var writer = new BinaryWriter(stream, Utf8, true))
            {
                writer.Write(Magic);
                writer.Write(Version);
                WriteText(writer, key, MaxKey);
                WriteText(writer, item.U.AbsoluteUri, MaxKey);
                writer.Write(item.St);
                writer.Write(item.Sz);
                writer.Write(item.Fr.ToUniversalTime().Ticks);
                WriteText(writer, item.Ct, MaxType);
                WriteText(writer, item.Ce, MaxEncoding);
                WriteText(writer, item.Ch, MaxEncoding);
                WriteText(writer, item.Et, MaxValidator);
                WriteText(writer, item.Lm, MaxValidator);
                writer.Write(raw.Length);
                writer.Write(packed.Length);
                writer.Write(packed);
                writer.Flush();
                stream.Flush(true);
            }
        }

        private static Pr Read(string file, string expectedKey)
        {
            var info = new FileInfo(file);
            if (info.Length < 32 || info.Length > MaxFile) throw new InvalidDataException("Lea cache entry size is invalid.");
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 32768, FileOptions.SequentialScan))
            using (var reader = new BinaryReader(stream, Utf8, true))
            {
                if (reader.ReadInt32() != Magic || reader.ReadInt32() != Version) throw new InvalidDataException("Lea cache entry version is invalid.");
                var key = ReadText(reader, MaxKey);
                if (!String.Equals(key, expectedKey, StringComparison.Ordinal)) throw new InvalidDataException("Lea cache key is invalid.");
                var url = new Uri(ReadText(reader, MaxKey), UriKind.Absolute);
                if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps) throw new InvalidDataException("Lea cache URL is invalid.");
                var status = reader.ReadInt32();
                var size = reader.ReadInt32();
                var ticks = reader.ReadInt64();
                if (status < 200 || status >= 300 || size < 0 || size > 2 * 1024 * 1024 || ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                    throw new InvalidDataException("Lea cache metadata is invalid.");
                var type = ReadText(reader, MaxType);
                var encoding = ReadText(reader, MaxEncoding);
                var charset = ReadText(reader, MaxEncoding);
                var etag = ReadText(reader, MaxValidator);
                var modified = ReadText(reader, MaxValidator);
                var rawLength = reader.ReadInt32();
                var packedLength = reader.ReadInt32();
                if (rawLength < 0 || rawLength > MaxBody || packedLength < 0 || packedLength > MaxFile || packedLength > stream.Length - stream.Position)
                    throw new InvalidDataException("Lea cache body length is invalid.");
                var packed = Exact(reader, packedLength);
                if (stream.Position != stream.Length) throw new InvalidDataException("Lea cache entry has trailing data.");
                var raw = Unpack(packed, rawLength);
                return new Pr(url, Utf8.GetString(raw), status, type, encoding, charset, size,
                    new DateTime(ticks, DateTimeKind.Utc), etag, modified);
            }
        }

        private static byte[] Unpack(byte[] packed, int expected)
        {
            using (var input = new MemoryStream(packed, false))
            using (var zip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream(Math.Min(expected, 32768)))
            {
                var buffer = new byte[32768];
                int read;
                while ((read = zip.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (output.Length + read > MaxBody || output.Length + read > expected) throw new InvalidDataException("Lea cache body expands past its bound.");
                    output.Write(buffer, 0, read);
                }
                if (output.Length != expected) throw new InvalidDataException("Lea cache body is incomplete.");
                return output.ToArray();
            }
        }

        private static void WriteText(BinaryWriter writer, string value, int max)
        {
            var bytes = Utf8.GetBytes(value ?? "");
            if (bytes.Length > max) throw new InvalidDataException("Lea cache text is too large.");
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadText(BinaryReader reader, int max)
        {
            var length = reader.ReadInt32();
            if (length < 0 || length > max) throw new InvalidDataException("Lea cache text length is invalid.");
            return Utf8.GetString(Exact(reader, length));
        }

        private static byte[] Exact(BinaryReader reader, int length)
        {
            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException();
            return bytes;
        }

        private static bool Delete(string file)
        {
            if (String.IsNullOrEmpty(file)) return false;
            try
            {
                if (!File.Exists(file)) return false;
                File.Delete(file);
                return true;
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        private static bool Optional(Exception error)
        {
            return error is IOException || error is UnauthorizedAccessException || error is NotSupportedException || error is ArgumentException;
        }
    }

    internal sealed class Pr
    {
        public readonly Uri U;
        public readonly string H;
        public readonly int St;
        public readonly string Ct;
        public readonly string Ce;
        public readonly string Ch;
        public readonly int Sz;
        public readonly DateTime Fr;
        public readonly string Et;
        public readonly string Lm;

        public Pr(Uri url, string html, int status, string contentType, string contentEncoding, string charset, int size, DateTime freshUntil, string etag, string lastModified)
        {
            U = url;
            H = html ?? "";
            St = status;
            Ct = contentType ?? "";
            Ce = contentEncoding ?? "";
            Ch = charset ?? "";
            Sz = size;
            Fr = freshUntil;
            Et = etag ?? "";
            Lm = lastModified ?? "";
        }
    }
}
