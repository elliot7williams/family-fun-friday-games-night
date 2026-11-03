using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

public static class CombinedScriptPhaseMap
{
    private static string TypeName(MetadataReader metadata, TypeDefinitionHandle handle)
    {
        var type = metadata.GetTypeDefinition(handle);
        var parent = type.GetDeclaringType();
        return parent.IsNil ? metadata.GetString(type.Namespace) + "." + metadata.GetString(type.Name) :
            TypeName(metadata, parent) + "+" + metadata.GetString(type.Name);
    }
    private static string MethodName(MetadataReader metadata, EntityHandle handle, int depth = 0)
    {
        if (depth > 4) throw new InvalidDataException("Generic method nesting bound.");
        if (handle.Kind == HandleKind.MethodDefinition) return metadata.GetString(metadata.GetMethodDefinition((MethodDefinitionHandle)handle).Name);
        if (handle.Kind == HandleKind.MemberReference) return metadata.GetString(metadata.GetMemberReference((MemberReferenceHandle)handle).Name);
        if (handle.Kind == HandleKind.MethodSpecification) return MethodName(metadata, metadata.GetMethodSpecification((MethodSpecificationHandle)handle).Method, depth + 1) + "<spec>";
        throw new InvalidDataException("Unexpected generic method target.");
    }
    public static object Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var assembly = metadata.GetAssemblyDefinition();
        var methods = new List<object>();
        foreach (var typeHandle in metadata.TypeDefinitions)
        {
            string typeName = TypeName(metadata, typeHandle);
            foreach (var methodHandle in metadata.GetTypeDefinition(typeHandle).GetMethods())
            {
                var method = metadata.GetMethodDefinition(methodHandle);
                string name = metadata.GetString(method.Name);
                bool engine = typeName == "Jint.Engine" && (name == "Execute" || name == "ExecuteForCompletion" || name == "ExecuteWithConstraints" || name == "ScriptEvaluation" || name == "ResetConstraints");
                bool lambda = typeName.StartsWith("Jint.Engine+", StringComparison.Ordinal) && name.StartsWith("<ExecuteForCompletion>", StringComparison.Ordinal);
                bool constraint = typeName == "Jint.Constraints.TimeConstraint" || typeName == "Jint.Constraints.OperationDeadlineConstraint";
                bool parent = typeName == "Lea.Script.Js" && (name == "Run" || name == "Start") || typeName == "Lea.Script.Vo" && name == "At";
                if ((!engine && !lambda && !constraint && !parent) || method.RelativeVirtualAddress == 0) continue;
                int bytes = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes().Length;
                if (bytes < 1 || bytes > 65536 || methods.Count >= 64) throw new InvalidDataException("Phase map method/body bound.");
                int token = MetadataTokens.GetToken(methodHandle);
                var rows = new SortedSet<string>(StringComparer.Ordinal);
                for (int offset = 0; offset < bytes; offset += 16)
                    foreach (string row in ReadFailureIL.Read(path, token, offset))
                        if (!row.StartsWith("METHOD ", StringComparison.Ordinal))
                            rows.Add(Regex.Replace(row, "MethodSpecification:0x([0-9A-Fa-f]{8})", match =>
                                MethodName(metadata, MetadataTokens.EntityHandle(Convert.ToInt32(match.Groups[1].Value, 16)))));
                var blob = metadata.GetBlobReader(method.Signature);
                var header = blob.ReadSignatureHeader();
                if (header.IsGeneric) blob.ReadCompressedInteger();
                int parameters = blob.ReadCompressedInteger();
                methods.Add(new { type = typeName, name, token = token.ToString("X8"), parameters, bytes, instructions = rows });
            }
        }
        return new { assembly = metadata.GetString(assembly.Name), version = assembly.Version.ToString(), methods, runtimeExecuted = false };
    }
}
