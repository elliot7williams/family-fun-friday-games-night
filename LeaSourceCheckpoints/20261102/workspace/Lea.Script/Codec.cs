using System;
using System.IO;
using Jint;
using Jint.Native;

namespace Lea.Script
{
    internal static class Codec
    {
        internal static object[] Arguments(JsValue[] values)
        {
            if (values.Length > 4) throw new InvalidDataException("Too many bridge arguments.");
            var result = new object[values.Length];
            for (var index = 0; index < result.Length; index++)
            {
                var value = values[index];
                if (value.IsNull()) result[index] = null;
                else if (value.IsString()) result[index] = value.AsString();
                else if (value.IsNumber())
                {
                    var number = value.AsNumber();
                    if (number < 0 || number > Int32.MaxValue || number != Math.Truncate(number))
                        throw new InvalidDataException("Invalid bridge number.");
                    result[index] = (int)number;
                }
                else throw new InvalidDataException("Invalid bridge value.");
            }
            return result;
        }

        internal static JsValue Value(Jint.Engine engine, object value)
        {
            if (value == null) return JsValue.Null;
            if (value is string) return (string)value;
            if (value is int) return (int)value;
            if (value is bool) return (bool)value;
            var array = value as object[];
            if (array == null || array.Length > 256) throw new InvalidDataException("Invalid bridge response.");
            var items = new JsValue[array.Length];
            for (var index = 0; index < items.Length; index++)
            {
                if (!(array[index] is int)) throw new InvalidDataException("Invalid bridge handle list.");
                items[index] = (int)array[index];
            }
            return new JsArray(engine, items);
        }
    }
}
