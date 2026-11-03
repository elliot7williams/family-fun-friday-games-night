$ErrorActionPreference='Stop'
$names=@('DecoderOwner.fragment.cs','Build-DecoderOwnerView.generated.ps1','Test-DecoderOwner.generated.ps1')
foreach($name in $names){if(Test-Path -LiteralPath (Join-Path $PSScriptRoot $name)){throw 'Preserve owner diagnostic preparation'}}
$fragment=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'DecoderQueueTrace.fragment.cs'))
$fields=@'
        private sealed class DecoderOwner
        {
            internal readonly long Id;
            internal readonly string Args;
            internal readonly CancellationToken Token;
            internal readonly Stopwatch Clock = Stopwatch.StartNew();
            internal string Phase = "entry";
            internal DecoderOwner(string args, CancellationToken token)
            {
                Id = Interlocked.Increment(ref ownerSequence);
                Args = args == "--image-fake" ? "fake" : args == "--pipe --delay=6000" ? "delayed" : "other";
                Token = token;
            }
        }
        private static long ownerSequence;
        private static DecoderOwner activeOwner;
        private static void MarkOwner(string phase)
        {
            var owner = Interlocked.CompareExchange(ref activeOwner, null, null);
            if (owner != null) Interlocked.Exchange(ref owner.Phase, phase);
        }
        private static string OwnerSnapshot()
        {
            var owner = Interlocked.CompareExchange(ref activeOwner, null, null);
            return owner == null ? "; owner=none" : "; owner=" + owner.Id +
                ":" + owner.Args + ":" + Interlocked.CompareExchange(ref owner.Phase, null, null) +
                ":age=" + owner.Clock.ElapsedMilliseconds + ":cancel=" + (owner.Token.IsCancellationRequested ? "1" : "0");
        }

'@
$old='                var message = text + detail;'
if(!$fragment.Contains($old)){throw 'Owner report anchor differs'}
$fragment=$fragment.Replace($old,"                detail += OwnerSnapshot();`r`n                var message = (text.Length > 512 - detail.Length ? text.Substring(0, 512 - detail.Length) : text) + detail;")
$old=@'
                    entered = true;
                    return Read(data, path, args, token, reserve, limit, hostMemory,
                        started, admission, measured, killed, report);
'@
$new=@'
                    entered = true;
                    var owner = new DecoderOwner(args, token);
                    Interlocked.Exchange(ref activeOwner, owner);
                    try
                    {
                        return Read(data, path, args, token, reserve, limit, hostMemory,
                            started, admission, measured, killed, report);
                    }
                    finally { Interlocked.CompareExchange(ref activeOwner, null, owner); }
'@
# Normalize only the replacement matching; generated source is retained for review.
$fragment=$fragment.Replace("`r`n","`n");$old=$old.Replace("`r`n","`n");$new=$new.Replace("`r`n","`n")
if([regex]::Matches($fragment,[regex]::Escape($old)).Count -ne 1){throw 'Owner decode anchor differs'}
$fragment=$fields+"`r`n"+$fragment.Replace($old,$new)
[IO.File]::WriteAllText((Join-Path $PSScriptRoot $names[0]),$fragment,[Text.UTF8Encoding]::new($false))
$builder=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-DecoderQueueTraceView.generated.ps1'))
$builder=$builder.Replace('New-DecoderQueueTraceSource.ps1','New-DecoderOwnerSource.ps1').Replace('DecoderQueueTrace.fragment.cs','DecoderOwner.fragment.cs')
[IO.File]::WriteAllText((Join-Path $PSScriptRoot $names[1]),$builder,[Text.UTF8Encoding]::new($false))
$runner=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderRetirement.generated.ps1'))
$runner=$runner.Replace('decoder-retirement-v1','decoder-owner-v1').Replace('Build-DecoderQueueTraceView.generated','Build-DecoderOwnerView.generated')
[IO.File]::WriteAllText((Join-Path $PSScriptRoot $names[2]),$runner,[Text.UTF8Encoding]::new($false))
foreach($name in $names[1..2]){
    $tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot $name),[ref]$tokens,[ref]$errors)
    if($errors.Count){throw ($errors|Out-String)}
}
Get-FileHash -LiteralPath ($names|ForEach-Object {Join-Path $PSScriptRoot $_})
