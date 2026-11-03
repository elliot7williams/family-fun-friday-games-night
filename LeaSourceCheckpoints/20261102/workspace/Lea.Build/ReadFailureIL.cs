using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

public static class ReadFailureIL
{
    private sealed class Instruction { internal int Offset; internal string Text; }
    private static string Name(MetadataReader metadata, int token)
    {
        if ((token & unchecked((int)0xff000000)) == 0x70000000)
        {
            var text = metadata.GetUserString(MetadataTokens.UserStringHandle(token & 0xffffff));
            return "\"" + (text.Length > 160 ? text.Substring(0, 160) : text) + "\"";
        }
        var handle = MetadataTokens.EntityHandle(token);
        if (handle.Kind == HandleKind.MethodDefinition)
            return metadata.GetString(metadata.GetMethodDefinition((MethodDefinitionHandle)handle).Name);
        if (handle.Kind == HandleKind.MemberReference)
            return metadata.GetString(metadata.GetMemberReference((MemberReferenceHandle)handle).Name);
        if (handle.Kind == HandleKind.FieldDefinition)
            return metadata.GetString(metadata.GetFieldDefinition((FieldDefinitionHandle)handle).Name);
        if (handle.Kind == HandleKind.TypeReference)
            return metadata.GetString(metadata.GetTypeReference((TypeReferenceHandle)handle).Name);
        return handle.Kind + ":0x" + token.ToString("X8");
    }

    public static string[] Read(string path, int methodToken, int failureOffset)
    {
        if ((methodToken & unchecked((int)0xff000000)) != 0x06000000 || failureOffset < 0)
            throw new InvalidDataException("Expected method-definition token and nonnegative IL offset.");
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var method = metadata.GetMethodDefinition(MetadataTokens.MethodDefinitionHandle(methodToken & 0xffffff));
        var il = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes();
        if (il == null || il.Length > 8388608 || failureOffset >= il.Length)
            throw new InvalidDataException("Failure IL offset/body bound.");
        var codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null))
            .ToDictionary(code => unchecked((ushort)code.Value));
        var rows = new List<Instruction>();
        for (int offset = 0; offset < il.Length;)
        {
            int start = offset;
            ushort key = il[offset++];
            if (key == 0xfe)
            {
                if (offset >= il.Length) throw new InvalidDataException("Truncated opcode.");
                key = (ushort)(0xfe00 | il[offset++]);
            }
            if (!codes.TryGetValue(key, out var code)) throw new InvalidDataException("Unknown opcode.");
            int size;
            switch (code.OperandType)
            {
                case OperandType.InlineNone: size = 0; break;
                case OperandType.ShortInlineI: case OperandType.ShortInlineVar: case OperandType.ShortInlineBrTarget: size = 1; break;
                case OperandType.InlineVar: size = 2; break;
                case OperandType.InlineI8: case OperandType.InlineR: size = 8; break;
                case OperandType.InlineSwitch:
                    if (offset > il.Length - 4) throw new InvalidDataException("Truncated switch.");
                    int count = BitConverter.ToInt32(il, offset);
                    if (count < 0 || count > (il.Length - offset - 4) / 4) throw new InvalidDataException("Switch bound.");
                    size = checked(4 + count * 4); break;
                default: size = 4; break;
            }
            if (offset > il.Length - size) throw new InvalidDataException("Truncated operand.");
            string operand = size == 0 ? "" : BitConverter.ToString(il, offset, size > 8 ? 8 : size);
            if (code.OperandType == OperandType.InlineMethod || code.OperandType == OperandType.InlineField ||
                code.OperandType == OperandType.InlineType || code.OperandType == OperandType.InlineTok || code.OperandType == OperandType.InlineString)
                operand = Name(metadata, BitConverter.ToInt32(il, offset));
            rows.Add(new Instruction { Offset = start, Text = start.ToString("X6") + " " + code.Name + " " + operand });
            offset += size;
        }
        int index = rows.FindLastIndex(row => row.Offset <= failureOffset);
        return new[] { "METHOD " + metadata.GetString(method.Name) + " failure=0x" + failureOffset.ToString("X6") + " bytes=" + il.Length }
            .Concat(rows.Skip(Math.Max(0, index - 22)).Take(34).Select(row => row.Text)).ToArray();
    }
}
