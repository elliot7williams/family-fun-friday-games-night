$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$path=Join-Path $workspace 'Lea.Script.Worker/Program.cs'
$text=[IO.File]::ReadAllText($path)
$start=$text.IndexOf('        private static void Trace(')
$end=$text.IndexOf('        private static bool FileDenied(',$start)
if($start -lt 0 -or $end -le $start){throw 'Trace method shape changed'}
$method=$text.Substring($start,$end-$start)
Add-Type -TypeDefinition ('using System; using System.Diagnostics; public static class LeaOrdinaryTraceContract {'+$method+'}')
$trace=[LeaOrdinaryTraceContract].GetMethod('Trace',[Reflection.BindingFlags]'NonPublic,Static')
$original=[Console]::Error;$output=[IO.StringWriter]::new()
try{
    [Console]::SetError($output)
    $trace.Invoke($null,@('debugger',[Diagnostics.Stopwatch]::StartNew(),$false))
    $trace.Invoke($null,@('subscribed',[Diagnostics.Stopwatch]::StartNew(),$false))
}finally{[Console]::SetError($original)}
if($output.ToString() -cne ('startup debugger'+[Environment]::NewLine+'startup subscribed'+[Environment]::NewLine)){throw 'Ordinary trace leaks timing or changes fixed labels'}
$output.Dispose();$output=[IO.StringWriter]::new()
try{[Console]::SetError($output);$trace.Invoke($null,@('debugger',[Diagnostics.Stopwatch]::StartNew(),$true))}finally{[Console]::SetError($original)}
if($output.ToString() -cnotmatch '^startup debugger: wall=\d+; cpu=\d+ ms\r?\n$'){throw 'Explicit timing trace changed'}
$output.Dispose()
[pscustomobject]@{passed=$true;checks=2;sourceSha256=(Get-FileHash -LiteralPath $path).Hash;scope='Extracted actual Trace method checks; not rebuilt worker or full-suite pass'}|ConvertTo-Json
