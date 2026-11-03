using System;
using System.IO;
using System.Reflection.PortableExecutable;

public static class ReadReadyToRun
{
    public static bool IsX64(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var cor = pe.PEHeaders.CorHeader;
        if (cor == null || pe.PEHeaders.CoffHeader.Machine != Machine.Amd64) return false;
        var directory = cor.ManagedNativeHeaderDirectory;
        if (directory.Size < 16) return false;
        var reader = pe.GetSectionData(directory.RelativeVirtualAddress).GetReader(0, 16);
        return reader.ReadUInt32() == 0x00525452;
    }
}
