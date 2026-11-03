function New-LibraryNavigationSource {
    param([Parameter(Mandatory)][string]$Original,[Parameter(Mandatory)][ValidateSet('History','Bookmarks')][string]$Kind)
    $list=if($Kind -eq 'History'){'l0'}else{'x1'}
    $old='CType(j.k.SelectedTab.Controls.Item(0), WebBrowser).Navigate('+$list+'.SelectedItem)'
    $new='j.q1(CStr('+$list+'.SelectedItem))'
    if([regex]::Matches($Original,[regex]::Escape($old)).Count -ne 1 -or $Original.Contains($new)){throw 'Library navigation anchor differs'}
    $text=$Original.Replace($old,$new)
    if($text.Replace($new,$old) -cne $Original){throw 'Library navigation inverse differs'}
    @{text=$text;inverseVerified=$true;kind=$Kind;scope='One selected-item route to existing current-tab q1 dispatch; policy and legacy fallback retained'}
}
