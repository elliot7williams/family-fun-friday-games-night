function New-AllStartupTraceSource {
    param([Parameter(Mandatory)][string]$Original)
    if($Original.Contains('LEA_ALL_STARTUP_TRACE')){throw 'All-startup trace already applied'}
    $nl=if($Original.Contains("`r`n")){"`r`n"}else{"`n"}
    $header='        private static int Main(string[] args)'+$nl+'        {'
    $root='                var root = args.Length == 0 ? Directory.GetCurrentDirectory() : Path.GetFullPath(args[0]);'
    $tail='                return 1;'+$nl+'            }'+$nl+'        }'+$nl+$nl+'        private static readonly'
    $finally=@'
                return 1;
            }
            finally
            {
                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", previousStartupTrace);
                var restored = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE") == previousStartupTrace;
                Console.Error.WriteLine("LEA_ALL_STARTUP_TRACE restored=" + (restored ? "true" : "false"));
                if (!restored) throw new InvalidOperationException("Diagnostic startup trace was not restored.");
            }
        }

        private static readonly
'@
    $finally=$finally.Replace("`r`n","`n").Replace("`n",$nl)
    $edits=@(
        @($header,($header+$nl+'            var previousStartupTrace = Environment.GetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE");')),
        @($root,('                Environment.SetEnvironmentVariable("LEA_SCRIPT_STARTUP_TRACE", "1");'+$nl+'                Console.Error.WriteLine("LEA_ALL_STARTUP_TRACE enabled");'+$nl+$root)),
        @($tail,$finally)
    )
    $text=$Original
    foreach($pair in $edits){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'All-startup trace anchor differs'};$text=$text.Replace($pair[0],$pair[1])}
    $inverse=$text
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'All-startup trace inverse differs'}
    @{text=$text;inverseVerified=$true;scope='Diagnostic Main enables existing worker tracing and restores environment in finally. Original assertions, phases and budgets preserved; deliberately sanitized child probes retain their own policy.'}
}
