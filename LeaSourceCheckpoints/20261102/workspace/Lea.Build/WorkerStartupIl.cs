using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

public static class WorkerStartupIl
{
    public static Dictionary<string, object> Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var matches = metadata.TypeDefinitions.Where(handle =>
        {
            var type = metadata.GetTypeDefinition(handle);
            return metadata.GetString(type.Name) == "Program" &&
                metadata.GetString(type.Namespace) == "Lea.Script.Worker";
        }).ToArray();
        if (matches.Length != 1) throw new InvalidDataException("Expected one worker Program type.");
        var methods = metadata.GetTypeDefinition(matches[0]).GetMethods().Where(handle =>
            metadata.GetString(metadata.GetMethodDefinition(handle).Name) == "Run").ToArray();
        if (methods.Length != 1) throw new InvalidDataException("Expected one startup Run method.");
        var method = metadata.GetMethodDefinition(methods[0]);
        var length = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes().Length;
        if (length < 1 || length > 65536) throw new InvalidDataException("Startup body bound.");
        var token = MetadataTokens.GetToken(methods[0]);
        var rows = new SortedSet<string>(StringComparer.Ordinal);
        for (int offset = 0; offset < length; offset += 16)
            foreach (var row in ReadFailureIL.Read(path, token, offset))
                if (!row.StartsWith("METHOD ", StringComparison.Ordinal)) rows.Add(row);
        var lines = rows.ToArray();
        var boundaries = new List<string>();
        for (int index = 0; index < lines.Length; index++)
            if (lines[index].Contains("\"evaluate\"") || lines[index].Contains("\"debugger\"") ||
                lines[index].Contains("\"subscribed\"") || lines[index].Contains("get_Debugger") ||
                lines[index].Contains("BeforeEvaluate") || lines[index].Contains("callvirt Evaluate"))
                boundaries.AddRange(lines.Skip(Math.Max(0, index - 3)).Take(8));
        return new Dictionary<string, object>
        {
            ["methodToken"] = token, ["ilBytes"] = length, ["instructions"] = lines,
            ["boundaryInstructions"] = boundaries.Distinct().OrderBy(row => row, StringComparer.Ordinal).ToArray(),
            ["runtimeExecuted"] = false
        };
    }
}
