function New-DecoderQueueTraceSource {
    param([string]$Original,[string]$Fragment)
    $start=$Original.IndexOf('        public static async Task<Image> Rd(')
    $end=$Original.IndexOf('        private static Image Read(',$start)
    if($start -lt 0 -or $end -le $start -or !$Original.Contains('internal async Task<T> Run<T>')){throw 'Owner decoder source shape differs'}
    $old=$Original.Substring($start,$end-$start)
    $updated=$Original.Remove($start,$end-$start).Insert($start,$Fragment)
    $pairs=@(
        @('                phase = value;', "                phase = value;`r`n                MarkOwner(value);"),
        @('                    if (Stop(current) && killed != null) killed();', "                    MarkOwner(`"stop`" );`r`n                    if (Stop(current) && killed != null) killed();`r`n                    MarkOwner(`"stopped`" );")
    )
    foreach($pair in $pairs){
        if($pair.Count -ne 2 -or [regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw 'Owner stage anchor differs'}
        $updated=$updated.Replace($pair[0],$pair[1])
    }
    $inverse=$updated
    foreach($pair in $pairs){$inverse=$inverse.Replace($pair[1],$pair[0])}
    if($inverse.Replace($Fragment,$old) -cne $Original){throw 'Owner overlay inverse differs'}
    return $updated
}
