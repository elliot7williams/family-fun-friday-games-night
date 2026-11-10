using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;

public static class ReadEmbeddedEntryResource
{
    public static string Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var r = pe.GetMetadataReader();
        var rows = r.ManifestResources.Select(h => r.GetManifestResource(h)).ToArray();
        if (rows.Length != 1 || !rows[0].Implementation.IsNil || r.GetString(rows[0].Name) != "Lea.EntryFixture.json")
            throw new InvalidDataException("Unexpected resources");
        var directory = pe.PEHeaders.CorHeader.ResourcesDirectory;
        var reader = pe.GetSectionData(directory.RelativeVirtualAddress).GetReader(checked((int)rows[0].Offset), checked(directory.Size - (int)rows[0].Offset));
        int length = reader.ReadInt32();
        if (length < 0 || length > 65536) throw new InvalidDataException("Entry resource bound");
        return new UTF8Encoding(false, true).GetString(reader.ReadBytes(length));
    }
}
