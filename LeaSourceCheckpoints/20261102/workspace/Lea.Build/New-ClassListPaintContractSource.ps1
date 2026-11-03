function New-ClassListPaintContractSource {
    param([string]$View,[string]$Classes,[string]$RootView)
    $start='        internal static void PaintAuthored(V view, string output)'
    $end='        private static void Check(bool condition, string name) { Program.Check(condition, name); }'
    if([regex]::Matches($RootView,[regex]::Escape($start)).Count -ne 1 -or [regex]::Matches($RootView,[regex]::Escape($end)).Count -ne 1 -or [regex]::Matches($View,[regex]::Escape($end)).Count -ne 1){throw 'Paint helper anchors differ'}
    $a=$RootView.IndexOf($start,[StringComparison]::Ordinal);$b=$RootView.IndexOf($end,$a,[StringComparison]::Ordinal)
    if($b -le $a -or $View.Contains($start)){throw 'Paint helper range differs'}
    $helper=$RootView.Substring($a,$b-$a)
    $updated=$View.Replace($end,$helper+$end)
    if($updated.Replace($helper+$end,$end) -cne $View){throw 'Paint source inverse differs'}
    $old='View.Paint(view, output == null ? null : output + '
    $new='View.PaintAuthored(view, output == null ? null : output + '
    if([regex]::Matches($Classes,[regex]::Escape($old)).Count -ne 2 -or $Classes.Contains($new)){throw 'Expected exactly two classList paint calls'}
    $calls=$Classes.Replace($old,$new)
    if($calls.Replace($new,$old) -cne $Classes){throw 'ClassList inverse differs'}
    @{view=$updated;classes=$calls;inverseVerified=$true}
}
