using System;
using System.Collections;
using System.Collections.Generic;

namespace Lea.Script.Reference
{
    internal static class AppSettings
    {
        internal static int MaxJsonDeserializerMembers { get { return Int32.MaxValue; } }
        internal static bool AllowRelaxedUnicodeDecoding { get { return false; } }
    }

    internal sealed partial class JavaScriptSerializer
    {
        internal const string ServerTypeFieldName = "__type";
        internal const long DatetimeMinTimeTicks = 621355968000000000L;
    }

    internal sealed class JavaScriptTypeResolver
    {
        internal string ResolveTypeId(Type type) { throw new NotSupportedException(); }
    }

    internal sealed class JavaScriptConverter
    {
        internal IDictionary<string, object> Serialize(object value, JavaScriptSerializer serializer) { throw new NotSupportedException(); }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal sealed class ScriptIgnoreAttribute : Attribute
    {
        public bool ApplyToOverrides { get; set; }
    }

    internal static class SecurityUtils
    {
        internal static object FieldInfoGetValue(System.Reflection.FieldInfo field, object value) { return field.GetValue(value); }
        internal static object MethodInfoInvoke(System.Reflection.MethodInfo method, object value, object[] args) { return method.Invoke(value, args); }
    }

    internal static class CompatibilityFormat
    {
        internal static string Single(float value)
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var text = value.ToString("G7", culture);
            float parsed;
            if (!System.Single.TryParse(text, System.Globalization.NumberStyles.Float, culture, out parsed) || parsed != value)
                text = value.ToString("G9", culture);
            return text;
        }
    }

    internal static class SR
    {
        internal const string CollectionCountExceeded_JavaScriptObjectDeserializer = "JSON member limit.";
        internal static string GetString(string message, int count) { return message; }
    }

    internal static class ObjectConverter
    {
        internal static object ConvertObjectToType(IDictionary<string, object> value, Type type, JavaScriptSerializer serializer)
        {
            var hint = value[JavaScriptSerializer.ServerTypeFieldName];
            if (hint == null) return value;
            // Preserve the observed failure without constructing arbitrary CLR objects.
            if (hint is IDictionary<string, object>) throw new MissingMethodException("Invalid script type hint.");
            throw new ArgumentException("Script type resolution is unavailable.");
        }
    }

    internal static class Decoder
    {
        internal static object Read(string json, int depth)
        {
            return Normalize(JavaScriptObjectDeserializer.BasicDeserialize(json, depth, new JavaScriptSerializer()));
        }

        private static object Normalize(object value)
        {
            var map = value as IDictionary<string, object>;
            if (map != null)
            {
                foreach (var key in new List<string>(map.Keys)) map[key] = Normalize(map[key]);
                return map;
            }
            var list = value as ArrayList;
            if (list == null) return value;
            var items = new object[list.Count];
            for (var index = 0; index < items.Length; index++) items[index] = Normalize(list[index]);
            return items;
        }
    }
}
