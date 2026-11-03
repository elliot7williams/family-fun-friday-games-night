function New-DecoderQueueTraceSource {
    param([string]$Original,[string]$Fragment)
    $start=$Original.IndexOf('        public static async Task<Image> Rd(')
    $end=$Original.IndexOf('        private static Image Read(',$start)
    if($start -lt 0 -or $end -le $start -or !$Original.Contains('internal async Task<T> Run<T>')){throw 'Decoder source shape differs'}
    $old=$Original.Substring($start,$end-$start)
    if($Original.Contains($Fragment)){throw 'Decoder overlay already applied'}
    $updated=$Original.Remove($start,$end-$start).Insert($start,$Fragment)
    if($updated.Replace($Fragment,$old) -cne $Original){throw 'Decoder overlay inverse differs'}
    return $updated
}
