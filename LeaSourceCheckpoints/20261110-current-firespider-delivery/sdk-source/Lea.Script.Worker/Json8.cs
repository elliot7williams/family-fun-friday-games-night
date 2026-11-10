using System;

namespace Lea.Script
{
    internal sealed class Json8
    {
        internal int MaxJsonLength { get; set; }
        internal int RecursionLimit { get; set; }

        internal string Serialize(object value)
        {
            return new Reference.JavaScriptSerializer {
                MaxJsonLength = MaxJsonLength, RecursionLimit = RecursionLimit
            }.Serialize(value);
        }

        internal object DeserializeObject(string json)
        {
            if (json.Length > MaxJsonLength) throw new ArgumentException("JSON length limit.");
            return Reference.Decoder.Read(json, RecursionLimit);
        }
    }
}
