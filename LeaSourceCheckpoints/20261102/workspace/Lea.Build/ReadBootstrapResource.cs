using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

public static class ReadBootstrapResource
{
    public static byte[] Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var resources = metadata.ManifestResources.Select(handle => metadata.GetManifestResource(handle)).ToArray();
        if (resources.Length != 1 || !resources[0].Implementation.IsNil ||
            metadata.GetString(resources[0].Name) != "Lea.Script.Dom.js")
            throw new InvalidDataException("Expected one embedded bootstrap resource.");
        var directory = pe.PEHeaders.CorHeader.ResourcesDirectory;
        int offset = checked((int)resources[0].Offset);
        if (offset < 0 || offset > directory.Size - 4) throw new InvalidDataException("Bootstrap offset bound.");
        var reader = pe.GetSectionData(directory.RelativeVirtualAddress).GetReader(offset, directory.Size - offset);
        int length = reader.ReadInt32();
        if (length < 1 || length > 131072 || length > reader.RemainingBytes)
            throw new InvalidDataException("Bootstrap size bound.");
        return reader.ReadBytes(length);
    }
}
