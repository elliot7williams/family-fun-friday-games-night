$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Add-Type -Path (Join-Path $root 'Lea.Tests/PrintPackageLease.cs')
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
public sealed class ShortPrintStream : IStream {
    public int Accepted;
    public void Write(byte[] b, int n, IntPtr p) { Accepted = n - 1; if (p != IntPtr.Zero) Marshal.WriteInt32(p, Accepted); }
    public void Read(byte[] b, int n, IntPtr p) { throw new NotSupportedException(); }
    public void Seek(long o, int k, IntPtr p) { throw new NotSupportedException(); }
    public void SetSize(long n) { throw new NotSupportedException(); }
    public void CopyTo(IStream s, long n, IntPtr r, IntPtr w) { throw new NotSupportedException(); }
    public void Commit(int f) { throw new NotSupportedException(); }
    public void Revert() { throw new NotSupportedException(); }
    public void LockRegion(long o, long n, int k) { throw new NotSupportedException(); }
    public void UnlockRegion(long o, long n, int k) { throw new NotSupportedException(); }
    public void Stat(out STATSTG s, int f) { throw new NotSupportedException(); }
    public void Clone(out IStream s) { throw new NotSupportedException(); }
}
'@
$checks = 0
function Check($condition, $name) {
    if (!$condition) { throw "Failed: $name" }
    $script:checks++
}
function Reject([scriptblock]$action, $name) {
    $rejected = $false
    try { & $action } catch { $rejected = $true }
    Check $rejected $name
}
$n = [Runtime.InteropServices.Marshal]::AllocHGlobal(8)
$written = [Runtime.InteropServices.Marshal]::AllocHGlobal(8)
$output = [Lea.PrintProbe.BoundedStream]::new($null)
$destination = [Lea.PrintProbe.BoundedStream]::new($null)
$inputStream = $null
try {
    $bytes = [byte[]](1, 2, 3)
    $inputStream = [Lea.PrintProbe.BoundedStream]::new($bytes)
    $bytes[0] = 99
    Check ($inputStream.Snapshot()[0] -eq 1) 'input is defensively copied'
    Reject { [Lea.PrintProbe.BoundedStream]::new([byte[]]::new(4194305)) } 'oversized input'
    $output.Write([byte[]](4, 5, 6), 3, $n)
    Check ([Runtime.InteropServices.Marshal]::ReadInt32($n) -eq 3) 'write count'
    $copy = $output.Snapshot(); $copy[0] = 77
    Check ($output.Snapshot()[0] -eq 4) 'snapshot is detached'
    $output.Seek(0, 0, $n)
    Check ([Runtime.InteropServices.Marshal]::ReadInt64($n) -eq 0) 'seek position'
    $buffer = [byte[]]::new(8)
    $output.Read($buffer, 8, $n)
    Check ([Runtime.InteropServices.Marshal]::ReadInt32($n) -eq 3) 'short read count'
    Check (($buffer[0] -eq 4) -and ($buffer[2] -eq 6)) 'read bytes'
    $output.Read($buffer, 8, $n)
    Check ([Runtime.InteropServices.Marshal]::ReadInt32($n) -eq 0) 'EOF count'
    Reject { $output.Seek(-1, 0, $n) } 'negative seek'
    Reject { $output.Seek(4194305, 0, $n) } 'oversized seek'
    Reject { $output.SetSize(4194305) } 'oversized length'
    $output.Seek(4194304, 0, $n)
    Reject { $output.Write([byte[]](1), 1, $n) } 'write beyond cap'
    Check ([Runtime.InteropServices.Marshal]::ReadInt32($n) -eq 0) 'failed write zero count'
    Check ($output.Length -eq 3) 'rejected writes preserve length'
    Reject { $inputStream.Write([byte[]](1), 1, $n) } 'read-only input'
    $inputStream.CopyTo($destination, [long]::MaxValue, $n, $written)
    Check ([Runtime.InteropServices.Marshal]::ReadInt64($n) -eq 3) 'copy through EOF read count'
    Check ([Runtime.InteropServices.Marshal]::ReadInt64($written) -eq 3) 'copy through EOF write count'
    Check (($destination.Length -eq 3) -and ($destination.Snapshot()[0] -eq 1)) 'copy bytes'
    $inputStream.CopyTo($destination, 1, $n, $written)
    Check (([Runtime.InteropServices.Marshal]::ReadInt64($n) -eq 0) -and ([Runtime.InteropServices.Marshal]::ReadInt64($written) -eq 0)) 'copy EOF resets counts'
    $inputStream.Seek(0, 0, [IntPtr]::Zero)
    $short = [ShortPrintStream]::new()
    Reject { $inputStream.CopyTo($short, 3, $n, $written) } 'partial destination write rejected'
    Check ([Runtime.InteropServices.Marshal]::ReadInt64($n) -eq 3) 'partial copy read count'
    Check ([Runtime.InteropServices.Marshal]::ReadInt64($written) -eq 2) 'partial copy write count'
    Reject { $inputStream.CopyTo($inputStream, 3, $n, $written) } 'self copy rejected'
    Reject { $inputStream.CopyTo($destination, -1, $n, $written) } 'negative copy rejected'
    Check (([Runtime.InteropServices.Marshal]::ReadInt64($n) -eq 0) -and ([Runtime.InteropServices.Marshal]::ReadInt64($written) -eq 0)) 'invalid copy resets counts'
    $output.Dispose()
    $destination.Dispose()
    Reject { $output.Snapshot() } 'disposed snapshot'
    [pscustomobject]@{ checks = $checks; passed = $true; nativeTargetCreated = $false;
        sourceSha256 = (Get-FileHash (Join-Path $root 'Lea.Tests/PrintPackageLease.cs')).Hash }
} finally {
    $output.Dispose()
    $destination.Dispose()
    if ($null -ne $inputStream) { $inputStream.Dispose() }
    [Runtime.InteropServices.Marshal]::FreeHGlobal($n)
    [Runtime.InteropServices.Marshal]::FreeHGlobal($written)
}
