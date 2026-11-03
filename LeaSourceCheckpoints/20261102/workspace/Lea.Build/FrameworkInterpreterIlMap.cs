using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

public static class FrameworkInterpreterIlMap
{
    private static string TypeName(MetadataReader metadata, TypeDefinitionHandle handle)
    {
        var type = metadata.GetTypeDefinition(handle);
        string name = metadata.GetString(type.Name);
        var parent = type.GetDeclaringType();
        return parent.IsNil ? metadata.GetString(type.Namespace) + "." + name : TypeName(metadata, parent) + "+" + name;
    }
    public static object Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var assembly = metadata.GetAssemblyDefinition();
        var types = new List<object>();
        int count = 0;
        foreach (var handle in metadata.TypeDefinitions)
        {
            var type = metadata.GetTypeDefinition(handle);
            string name = metadata.GetString(type.Name), space = metadata.GetString(type.Namespace);
            bool engine = space == "Jint" && name == "Engine";
            bool options = name == "ConstraintOptions";
            bool parent = space == "Lea.Script" && (name == "Vo" || name == "Js");
            bool extensions = space == "Jint" && name == "OptionsExtensions";
            if (!engine && !options && !parent && !extensions && name != "StackGuard" && name != "TimeConstraint" && name != "OperationDeadlineConstraint" && name != "MemoryLimitConstraint" && name != "GCPolyfills") continue;
            var methods = new List<object>();
            foreach (var methodHandle in type.GetMethods())
            {
                var method = metadata.GetMethodDefinition(methodHandle);
                string methodName = metadata.GetString(method.Name);
                if (engine && methodName != "ExecuteWithConstraints" && methodName != "CheckConstraints" && methodName != "CheckExactConstraints" && methodName != "CheckAmortizedConstraints" && methodName != "ResetConstraints") continue;
                if (options && methodName != ".ctor" && methodName != "get_MaxExecutionStackCount" && methodName != "set_MaxExecutionStackCount") continue;
                if (parent && name == "Js" && methodName != ".ctor" && methodName != "Start") continue;
                if (extensions && methodName != "LimitRecursion") continue;
                if (method.RelativeVirtualAddress == 0) continue;
                int length = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes().Length;
                if (length < 1 || length > 65536 || ++count > 128) throw new InvalidDataException("Interpreter method inventory bound");
                int token = MetadataTokens.GetToken(methodHandle);
                var rows = new SortedSet<string>(StringComparer.Ordinal);
                for (int offset = 0; offset < length; offset += 16)
                    foreach (string row in ReadFailureIL.Read(path, token, offset))
                        if (!row.StartsWith("METHOD ", StringComparison.Ordinal)) rows.Add(row);
                methods.Add(new { name = methodName, token = token.ToString("X8"), bytes = length, instructions = rows.ToArray() });
            }
            types.Add(new { name = TypeName(metadata, handle), methods });
        }
        return new { assembly = metadata.GetString(assembly.Name), version = assembly.Version.ToString(), methodCount = count, types, runtimeExecuted = false, causeProven = false };
    }
}
