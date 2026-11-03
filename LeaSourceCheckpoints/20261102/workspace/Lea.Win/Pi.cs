using System;
using System.IO;

namespace Lea.Win
{
    // Pi validates bounded picture structure before bytes reach the native decoder.
    internal sealed class Pi
    {
        public const int Axis = 8192;
        public const int Pixels = 8 * 1024 * 1024;
        public const int Frames = 64;
        public const int HeaderBytes = 256 * 1024;
        public const long WorkBytes = 64L * 1024 * 1024;

        public readonly string K;
        public readonly int W;
        public readonly int H;
        public readonly int F;
        public readonly long P;
        public readonly long R;

        private Pi(string kind, int width, int height, int frames, long workPixels, long rawBytes)
        {
            var pixels = (long)width * height;
            if (width < 1 || height < 1 || width > Axis || height > Axis || pixels > Pixels)
                throw Bad("Image dimensions exceed Lea's preflight limit.");
            if (frames < 1 || frames > Frames || workPixels < pixels || workPixels > Pixels)
                throw Bad("Image frames exceed Lea's preflight limit.");
            var retained = pixels * 4;
            var reserve = retained + Math.Max(workPixels * 4, rawBytes);
            if (reserve < 1 || reserve > WorkBytes) throw Bad("Image decode estimate exceeds Lea's preflight limit.");
            K = kind;
            W = width;
            H = height;
            F = frames;
            P = pixels;
            R = reserve;
        }

        public static Pi Rd(byte[] data)
        {
            if (data == null || data.Length < 10) throw Bad("Image resource is empty or truncated.");
            if (PngSignature(data, 0, data.Length)) return Png(data);
            if (data[0] == 0xff && data[1] == 0xd8) return Jpeg(data);
            if (Ascii(data, 0, "GIF87a") || Ascii(data, 0, "GIF89a")) return Gif(data);
            if (data[0] == (byte)'B' && data[1] == (byte)'M') return Bmp(data);
            if (L2(data, 0) == 0 && L2(data, 2) == 1) return Icon(data);
            throw Bad("Unsupported image format.");
        }

        private static Pi Png(byte[] data)
        {
            if (data.Length < 33 || B4(data, 8) != 13 || !Ascii(data, 12, "IHDR")) throw Bad("PNG header is invalid.");
            var width = Positive(B4(data, 16));
            var height = Positive(B4(data, 20));
            var depth = data[24];
            var color = data[25];
            var channels = color == 0 ? 1 : color == 2 ? 3 : color == 3 ? 1 : color == 4 ? 2 : color == 6 ? 4 : 0;
            var validDepth = color == 0 ? depth == 1 || depth == 2 || depth == 4 || depth == 8 || depth == 16 :
                color == 2 || color == 4 || color == 6 ? depth == 8 || depth == 16 : color == 3 && (depth == 1 || depth == 2 || depth == 4 || depth == 8);
            if (channels == 0 || !validDepth || data[26] != 0 || data[27] != 0 || data[28] > 1) throw Bad("PNG image settings are invalid.");
            var frames = 1;
            var ended = false;
            var pixelsFound = false;
            var at = 8;
            while (at <= data.Length - 12)
            {
                var size = B4(data, at);
                if (size > Int32.MaxValue || size > data.Length - at - 12) throw Bad("PNG chunk is truncated.");
                if (Ascii(data, at + 4, "acTL"))
                {
                    if (size != 8) throw Bad("PNG animation header is invalid.");
                    frames = Positive(B4(data, at + 8));
                }
                if (Ascii(data, at + 4, "IDAT")) pixelsFound = true;
                if (Ascii(data, at + 4, "IEND"))
                {
                    if (size != 0) throw Bad("PNG end chunk is invalid.");
                    ended = true;
                    break;
                }
                at += 12 + (int)size;
            }
            if (!ended || !pixelsFound) throw Bad("PNG image has no bounded pixel data and end chunk.");
            var row = ((long)width * channels * depth + 7) / 8 + 1;
            return new Pi("p", width, height, frames, (long)width * height * frames, row * height * frames);
        }

        private static Pi Jpeg(byte[] data)
        {
            var at = 2;
            var limit = Math.Min(data.Length, HeaderBytes);
            while (at < limit)
            {
                if (data[at++] != 0xff) throw Bad("JPEG marker stream is invalid.");
                while (at < limit && data[at] == 0xff) at++;
                if (at >= limit) break;
                var marker = data[at++];
                if (marker == 0xd9 || marker == 0xda) break;
                if (marker == 0x01 || marker >= 0xd0 && marker <= 0xd8) continue;
                if (at + 2 > limit) break;
                var size = B2(data, at);
                if (size < 2 || at + size > data.Length) throw Bad("JPEG segment is truncated.");
                if (Sof(marker))
                {
                    if (size < 11 || at + 8 > data.Length) throw Bad("JPEG frame header is invalid.");
                    var height = B2(data, at + 3);
                    var width = B2(data, at + 5);
                    var components = data[at + 7];
                    if (components < 1 || components > 4 || size < 8 + components * 3) throw Bad("JPEG components are invalid.");
                    var pixels = (long)width * height;
                    return new Pi("j", width, height, 1, pixels, pixels * 4);
                }
                at += size;
            }
            throw Bad("JPEG dimensions were not found within the header limit.");
        }

        private static bool Sof(int marker)
        {
            return marker >= 0xc0 && marker <= 0xcf && marker != 0xc4 && marker != 0xc8 && marker != 0xcc;
        }

        private static Pi Gif(byte[] data)
        {
            var width = L2(data, 6);
            var height = L2(data, 8);
            var at = 13;
            if ((data[10] & 0x80) != 0) at = Add(at, 3L << ((data[10] & 7) + 1), data.Length);
            var frames = 0;
            long framePixels = 0;
            var ended = false;
            while (at < data.Length)
            {
                var kind = data[at++];
                if (kind == 0x3b) { ended = true; break; }
                if (kind == 0x21)
                {
                    if (at >= data.Length) throw Bad("GIF extension is truncated.");
                    at++;
                    at = Blocks(data, at);
                    continue;
                }
                if (kind != 0x2c || at + 9 > data.Length) throw Bad("GIF block is invalid.");
                var left = L2(data, at);
                var top = L2(data, at + 2);
                var frameWidth = L2(data, at + 4);
                var frameHeight = L2(data, at + 6);
                var packed = data[at + 8];
                at += 9;
                if (frameWidth < 1 || frameHeight < 1 || left + (long)frameWidth > width || top + (long)frameHeight > height)
                    throw Bad("GIF frame dimensions are invalid.");
                frames++;
                framePixels += (long)frameWidth * frameHeight;
                if (frames > Frames || framePixels > Pixels) throw Bad("GIF animation exceeds Lea's frame limit.");
                if ((packed & 0x80) != 0) at = Add(at, 3L << ((packed & 7) + 1), data.Length);
                if (at >= data.Length || data[at++] < 2) throw Bad("GIF image data is invalid.");
                at = Blocks(data, at);
            }
            if (!ended || frames == 0) throw Bad("GIF image is incomplete.");
            var canvas = (long)width * height;
            return new Pi("g", width, height, frames, Math.Max(canvas, framePixels), Math.Max(canvas, framePixels) * 4);
        }

        private static int Blocks(byte[] data, int at)
        {
            while (at < data.Length)
            {
                var size = data[at++];
                if (size == 0) return at;
                at = Add(at, size, data.Length);
            }
            throw Bad("Image data blocks are truncated.");
        }

        private static Pi Bmp(byte[] data)
        {
            if (data.Length < 26) throw Bad("BMP header is truncated.");
            var fileSize = L4(data, 2);
            var pixelsAt = L4(data, 10);
            var dib = L4(data, 14);
            if (fileSize != 0 && fileSize > data.Length || pixelsAt >= data.Length) throw Bad("BMP file bounds are invalid.");
            int width;
            int height;
            int planes;
            int depth;
            if (dib == 12)
            {
                width = L2(data, 18);
                height = L2(data, 20);
                planes = L2(data, 22);
                depth = L2(data, 24);
            }
            else
            {
                if (dib < 40 || dib > data.Length - 14 || data.Length < 54) throw Bad("BMP information header is invalid.");
                width = Absolute(I4(data, 18));
                height = Absolute(I4(data, 22));
                planes = L2(data, 26);
                depth = L2(data, 28);
                var compression = L4(data, 30);
                if (compression != 0 && compression != 1 && compression != 2 && compression != 3 && compression != 6)
                    throw Bad("BMP compression is unsupported.");
            }
            if (planes != 1 || depth != 1 && depth != 4 && depth != 8 && depth != 16 && depth != 24 && depth != 32)
                throw Bad("BMP pixel layout is invalid.");
            var count = (long)width * height;
            return new Pi("b", width, height, 1, count, count * 4);
        }

        private static Pi Icon(byte[] data)
        {
            var frames = L2(data, 4);
            if (frames < 1 || frames > Frames || 6L + frames * 16L > data.Length) throw Bad("ICO directory is invalid.");
            var maxWidth = 0;
            var maxHeight = 0;
            long pixels = 0;
            for (var index = 0; index < frames; index++)
            {
                var at = 6 + index * 16;
                var width = data[at] == 0 ? 256 : data[at];
                var height = data[at + 1] == 0 ? 256 : data[at + 1];
                var size = L4(data, at + 8);
                var offset = L4(data, at + 12);
                if (size < 16 || offset > data.Length || size > data.Length - offset) throw Bad("ICO frame bounds are invalid.");
                var start = (int)offset;
                if (PngSignature(data, start, (int)size))
                {
                    if (size < 24) throw Bad("ICO PNG frame is truncated.");
                    width = Positive(B4(data, start + 16));
                    height = Positive(B4(data, start + 20));
                }
                else
                {
                    var dib = L4(data, start);
                    if (dib < 40 || size < 16 || dib > size) throw Bad("ICO bitmap frame is invalid.");
                    width = Absolute(I4(data, start + 4));
                    var storedHeight = Absolute(I4(data, start + 8));
                    if ((storedHeight & 1) != 0) throw Bad("ICO bitmap height is invalid.");
                    height = storedHeight / 2;
                }
                if (width < 1 || height < 1 || width > Axis || height > Axis) throw Bad("ICO frame dimensions exceed Lea's limit.");
                pixels += (long)width * height;
                if (pixels > Pixels) throw Bad("ICO frames exceed Lea's pixel limit.");
                maxWidth = Math.Max(maxWidth, width);
                maxHeight = Math.Max(maxHeight, height);
            }
            return new Pi("i", maxWidth, maxHeight, frames, pixels, pixels * 4);
        }

        private static bool PngSignature(byte[] data, int at, int count)
        {
            return count >= 24 && at >= 0 && at <= data.Length - count &&
                data[at] == 0x89 && data[at + 1] == 0x50 && data[at + 2] == 0x4e && data[at + 3] == 0x47 &&
                data[at + 4] == 0x0d && data[at + 5] == 0x0a && data[at + 6] == 0x1a && data[at + 7] == 0x0a;
        }

        private static bool Ascii(byte[] data, int at, string value)
        {
            if (at < 0 || at + value.Length > data.Length) return false;
            for (var index = 0; index < value.Length; index++) if (data[at + index] != (byte)value[index]) return false;
            return true;
        }

        private static int Add(int at, long count, int length)
        {
            if (count < 0 || count > length - at) throw Bad("Image structure exceeds its input bounds.");
            return at + (int)count;
        }

        private static int Positive(uint value)
        {
            if (value == 0 || value > Int32.MaxValue) throw Bad("Image dimension is invalid.");
            return (int)value;
        }

        private static int Absolute(int value)
        {
            if (value == 0 || value == Int32.MinValue) throw Bad("Image dimension is invalid.");
            return Math.Abs(value);
        }

        private static int B2(byte[] data, int at)
        {
            if (at < 0 || at > data.Length - 2) throw Bad("Image header is truncated.");
            return data[at] << 8 | data[at + 1];
        }

        private static uint B4(byte[] data, int at)
        {
            if (at < 0 || at > data.Length - 4) throw Bad("Image header is truncated.");
            return (uint)(data[at] << 24 | data[at + 1] << 16 | data[at + 2] << 8 | data[at + 3]);
        }

        private static int L2(byte[] data, int at)
        {
            if (at < 0 || at > data.Length - 2) throw Bad("Image header is truncated.");
            return data[at] | data[at + 1] << 8;
        }

        private static uint L4(byte[] data, int at)
        {
            if (at < 0 || at > data.Length - 4) throw Bad("Image header is truncated.");
            return (uint)(data[at] | data[at + 1] << 8 | data[at + 2] << 16 | data[at + 3] << 24);
        }

        private static int I4(byte[] data, int at)
        {
            return unchecked((int)L4(data, at));
        }

        private static InvalidDataException Bad(string message)
        {
            return new InvalidDataException(message);
        }
    }
}
