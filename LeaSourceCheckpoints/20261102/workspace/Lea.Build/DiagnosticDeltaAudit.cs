using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

public static class DiagnosticDeltaAudit
{
    private sealed class Snapshot
    {
        public Dictionary<string, string> Methods = new();
        public Dictionary<string, string> Bodies = new();
        public Dictionary<string, string> Headers = new();
        public string[] Fields;
    }

    private static Snapshot Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var reader = pe.GetMetadataReader();
        // Reuse the pinned resolver rather than invent another IL token parser.
        var type = typeof(TemporalResolvedIl).GetNestedType("Names", BindingFlags.NonPublic);
        var names = Activator.CreateInstance(type, new object[] { reader });
        string Entity(EntityHandle handle) => (string)type.GetMethod("Entity").Invoke(names, new object[] { handle });
        string Token(int token) => (string)type.GetMethod("Token").Invoke(names, new object[] { token });
        var header = typeof(TemporalResolvedIl).GetMethod("Header", BindingFlags.NonPublic | BindingFlags.Static);
        var result = new Snapshot();
        foreach (var handle in reader.MethodDefinitions)
        {
            var method = reader.GetMethodDefinition(handle);
            var import = method.GetImport();
            string imported = import.Module.IsNil ? "" : reader.GetString(reader.GetModuleReference(import.Module).Name) + ":" + reader.GetString(import.Name) + ":" + (int)import.Attributes;
            var identity = Entity(handle);
            result.Methods.Add(identity, (int)method.Attributes + ":" + (int)method.ImplAttributes + ":" + imported);
            if (method.RelativeVirtualAddress == 0) continue;
            var body = pe.GetMethodBody(method.RelativeVirtualAddress);
            result.Bodies.Add(identity, TemporalInstructionShape.Decode(body.GetILBytes(), false, Token));
            result.Headers.Add(identity, (string)header.Invoke(null, new object[] { body, names }));
        }
        result.Fields = reader.FieldDefinitions.Select(h => {
            var field = reader.GetFieldDefinition(h);
            return Entity(h) + ":" + (int)field.Attributes + ":" + field.GetOffset();
        }).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        return result;
    }

    private static Snapshot MapKnownClosures(Snapshot source)
    {
        string Map(string text) => text.Replace("self:Lea.Win/Rp+/<>c__DisplayClass92_0", "self:Lea.Win/Rp+/<>c__DisplayClass96_0")
            .Replace("self:Lea.Win/Rp+/<>c__DisplayClass95_0", "self:Lea.Win/Rp+/<>c__DisplayClass99_0");
        Dictionary<string,string> MapRows(Dictionary<string,string> rows) => rows.ToDictionary(x => Map(x.Key), x => Map(x.Value), StringComparer.Ordinal);
        return new Snapshot { Methods=MapRows(source.Methods), Bodies=MapRows(source.Bodies), Headers=MapRows(source.Headers), Fields=source.Fields.Select(Map).OrderBy(x => x, StringComparer.Ordinal).ToArray() };
    }

    public static object Compare(string baseline, string candidate) => CompareCore(Read(baseline), Read(candidate));
    public static object CompareKnownClosures(string baseline, string candidate) => CompareCore(MapKnownClosures(Read(baseline)), Read(candidate));

    private static object CompareCore(Snapshot a, Snapshot b)
    {
        string[] Changed(Dictionary<string,string> x, Dictionary<string,string> y) => x.Keys.Intersect(y.Keys).Where(k => x[k] != y[k]).OrderBy(k => k, StringComparer.Ordinal).ToArray();
        return new {
            originalMethods = a.Methods.Count,
            candidateMethods = b.Methods.Count,
            added = b.Methods.Keys.Except(a.Methods.Keys).OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            removed = a.Methods.Keys.Except(b.Methods.Keys).ToArray(),
            methodMetadataChanges = Changed(a.Methods, b.Methods),
            instructionChanges = Changed(a.Bodies, b.Bodies),
            headerLocalExceptionChanges = Changed(a.Headers, b.Headers),
            bodyPresenceChanges = a.Methods.Keys.Intersect(b.Methods.Keys).Where(k => a.Bodies.ContainsKey(k) != b.Bodies.ContainsKey(k)).ToArray(),
            fieldsIdentical = a.Fields.SequenceEqual(b.Fields),
            scope = "Resolved method IL/header/local/EH, method/import metadata and field identities only; not complete PE metadata or runtime validation"
        };
    }
}
