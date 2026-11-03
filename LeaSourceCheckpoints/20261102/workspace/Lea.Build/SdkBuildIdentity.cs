using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

public static class SdkBuildIdentity
{
    public static object Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length > 16 * 1024 * 1024) throw new InvalidDataException("Identity image size bound.");
        using var stream = new MemoryStream(bytes, false);
        using var pe = new PEReader(stream);
        if (!pe.HasMetadata)
        {
            var hash = Convert.ToHexString(SHA256.HashData(bytes));
            Array.Clear(bytes, checked(pe.PEHeaders.CoffHeaderStartOffset + 4), 4);
            return new { path, sha256 = hash, hasMetadata = false, moduleId = "", coffTimestamp = pe.PEHeaders.CoffHeader.TimeDateStamp,
                reproducibleDebugEntry = false, codeViewPaths = Array.Empty<string>(),
                timestampAndMvidMaskedSha256 = Convert.ToHexString(SHA256.HashData(bytes)),
                scope = "Native apphost: only the parsed COFF timestamp is masked; no managed module identity exists." };
        }
        var metadata = pe.GetMetadataReader();
        var id = metadata.GetGuid(metadata.GetModuleDefinition().Mvid);
        var guidBytes = id.ToByteArray();
        int matches = 0, location = -1;
        for (int i = 0; i <= bytes.Length - 16; i++)
        {
            bool equal = true;
            for (int j = 0; j < 16 && equal; j++) equal = bytes[i + j] == guidBytes[j];
            if (equal) { matches++; location = i; }
        }
        if (matches != 1) throw new InvalidDataException("Unique module identity occurrence required.");
        var paths = new List<string>();
        bool deterministic = false;
        foreach (var entry in pe.ReadDebugDirectory())
        {
            if (entry.Type == DebugDirectoryEntryType.Reproducible) deterministic = true;
            if (entry.Type == DebugDirectoryEntryType.CodeView) paths.Add(pe.ReadCodeViewDebugDirectoryData(entry).Path);
        }
        var original = Convert.ToHexString(SHA256.HashData(bytes));
        int timestamp = checked(pe.PEHeaders.CoffHeaderStartOffset + 4);
        Array.Clear(bytes, timestamp, 4);
        Array.Clear(bytes, location, 16);
        return new {
            path, sha256 = original, hasMetadata = true, moduleId = id.ToString(), coffTimestamp = pe.PEHeaders.CoffHeader.TimeDateStamp,
            reproducibleDebugEntry = deterministic, codeViewPaths = paths,
            timestampAndMvidMaskedSha256 = Convert.ToHexString(SHA256.HashData(bytes)),
            scope = "Only the parsed COFF timestamp and unique metadata MVID bytes are masked. Other metadata, debug paths, IL and native bytes remain exact; not semantic equivalence."
        };
    }
}
