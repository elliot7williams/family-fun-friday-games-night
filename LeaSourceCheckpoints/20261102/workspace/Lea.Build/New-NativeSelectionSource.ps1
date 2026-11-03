function New-NativeSelectionSource {
    param([Parameter(Mandatory)][string]$Original,[Parameter(Mandatory)][string]$Fragment)
    $marker='        private bool ReadyEdit(string code)'
    if([regex]::Matches($Original,[regex]::Escape($marker)).Count -ne 1 -or $Original.Contains('public string Sx()')){throw 'Native selection source anchor differs'}
    foreach($required in @('public string Sx()','return OnUi(delegate','Busy || editLock || !Ce("cp") ? String.Empty : ed.SelectedText','}) ?? String.Empty;')){
        if(!$Fragment.Contains($required)){throw 'Native selection policy fragment differs'}
    }
    if($Fragment.Contains('Clipboard.') -or $Fragment.Contains('ReadyEdit(') -or $Fragment.Contains('return ed.Text') -or $Fragment.Contains('Ec(')){throw 'Native selection policy fragment differs'}
    $updated=$Original.Replace($marker,$Fragment+$marker)
    if($updated.Replace($Fragment+$marker,$marker) -cne $Original){throw 'Native selection inverse differs'}
    @{text=$updated;inverseVerified=$true}
}
