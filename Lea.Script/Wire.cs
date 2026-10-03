using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace Lea.Script
{
    internal static class Wire
    {
        private const int Magic = 0x5341454c;
        internal const int MaxFrame = 128 * 1024;
        internal const int Version = 2;
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

        internal static void Write(Stream stream, object value)
        {
            byte[] bytes;
            try { bytes = Utf8.GetBytes(Serializer().Serialize(value)); }
            catch (Exception error) when (error is ArgumentException || error is InvalidOperationException)
            { throw new InvalidDataException("Invalid outgoing script frame.", error); }
            if (bytes.Length > MaxFrame) throw new InvalidDataException("Script frame is too large.");
            var header = new byte[12];
            Put(header, 0, Magic);
            Put(header, 4, Version);
            Put(header, 8, bytes.Length);
            stream.Write(header, 0, header.Length);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        internal static Dictionary<string, object> Read(Stream stream)
        {
            var header = ReadBytes(stream, 12);
            var length = Get(header, 8);
            if (Get(header, 0) != Magic || Get(header, 4) != Version || length < 2 || length > MaxFrame)
                throw new InvalidDataException("Invalid script frame header.");
            try
            {
                var value = Serializer().DeserializeObject(Utf8.GetString(ReadBytes(stream, length))) as Dictionary<string, object>;
                if (value == null || value.Count > 8) throw new InvalidDataException("Invalid script frame object.");
                return value;
            }
            catch (Exception error) when (error is ArgumentException || error is InvalidOperationException)
            { throw new InvalidDataException("Invalid script frame JSON.", error); }
        }

        internal static void Fields(Dictionary<string, object> frame, params string[] names)
        {
            if (frame.Count != names.Length) throw new InvalidDataException("Unexpected script frame fields.");
            foreach (var name in names)
                if (!frame.ContainsKey(name)) throw new InvalidDataException("Missing script frame field.");
        }

        internal static string Step(Dictionary<string, object> frame, int execution, ref int request)
        {
            if (Number(frame, "i") != execution) throw new InvalidDataException("Unexpected script execution ID.");
            var kind = Text(frame, "k", 8);
            if (kind == "end")
            {
                Fields(frame, "k", "i", "s", "m", "d");
                var status = Text(frame, "s", 8);
                if (status != "ok" && status != "js" && status != "limit")
                    throw new InvalidDataException("Unexpected worker result.");
                Text(frame, "m", 512);
                if (!(frame["d"] is bool)) throw new InvalidDataException("Invalid event cancellation result.");
            }
            else
            {
                Fields(frame, "k", "i", "r", "b");
                if (kind != "call" || request >= 256 || Number(frame, "r") != request + 1 || !(frame["b"] is object[]))
                    throw new InvalidDataException("Invalid script request sequence or arguments.");
                request++;
            }
            return kind;
        }

        internal static string Text(Dictionary<string, object> frame, string key, int max)
        {
            object value;
            if (!frame.TryGetValue(key, out value) || !(value is string) || ((string)value).Length > max)
                throw new InvalidDataException("Invalid script frame string.");
            return (string)value;
        }

        internal static int Number(Dictionary<string, object> frame, string key)
        {
            object value;
            if (!frame.TryGetValue(key, out value) || !(value is int)) throw new InvalidDataException("Invalid script frame number.");
            return (int)value;
        }

        private static JavaScriptSerializer Serializer()
        { return new JavaScriptSerializer { MaxJsonLength = MaxFrame, RecursionLimit = 8 }; }

        private static byte[] ReadBytes(Stream stream, int length)
        {
            var bytes = new byte[length];
            var at = 0;
            while (at < bytes.Length)
            {
                var count = stream.Read(bytes, at, bytes.Length - at);
                if (count == 0) throw new EndOfStreamException("Script worker pipe ended.");
                at += count;
            }
            return bytes;
        }

        private static void Put(byte[] bytes, int at, int value)
        { for (var index = 0; index < 4; index++) bytes[at + index] = (byte)(value >> (index * 8)); }
        private static int Get(byte[] bytes, int at)
        { return bytes[at] | bytes[at + 1] << 8 | bytes[at + 2] << 16 | bytes[at + 3] << 24; }
    }
}
