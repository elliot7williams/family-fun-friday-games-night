function New-AboutNavigationSource {
    param([string]$Original)
    $text=$Original.Replace("`r`n","`n")
    $cast='CType(j.k.SelectedTab.Controls.Item(0), WebBrowser).Navigate("http://firespiderinternet.weebly.com/")'
    $pattern='(?m)^(?<indent> +)N\(\)\n +'+[regex]::Escape($cast)+'$'
    if([regex]::Matches($text,$pattern).Count -ne 4){throw 'About paired navigation boundary differs'}
    $reverse=@{}
    $updated=[regex]::Replace($text,$pattern,{
        param($match)
        $new=$match.Groups['indent'].Value+'j.qln("http://firespiderinternet.weebly.com/")'
        $reverse[$new]=$match.Value
        return $new
    })
    $inverse=$updated
    foreach($new in $reverse.Keys){$inverse=[regex]::Replace($inverse,('(?m)^'+[regex]::Escape($new)+'$'),$reverse[$new])}
    if($inverse -cne $text){throw 'About navigation inverse differs'}
    return $updated
}

function Add-AboutNewTabRoute {
    param([string]$Original)
    $text=$Original.Replace("`r`n","`n")
    $anchor='    Private Function qleaNew(ByVal target As String) As Global.Lea.Win.V'
    $route=@'
    Friend Sub qln(ByVal target As String)
        Dim address As String = qleaInput(target, False)
        If String.IsNullOrWhiteSpace(address) Then Return
        qleaNew(address)
    End Sub

'@
    $route=$route.Replace("`r`n","`n")
    if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1 -or $text.Contains('Sub qln(')){throw 'About route anchor differs'}
    $updated=$text.Replace($anchor,$route+$anchor)
    if($updated.Replace($route,'') -cne $text){throw 'About route inverse differs'}
    return $updated
}
