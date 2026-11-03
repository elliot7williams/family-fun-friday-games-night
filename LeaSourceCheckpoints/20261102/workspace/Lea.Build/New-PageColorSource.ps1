function New-PageColorSource {
    param([Parameter(Mandatory)][string]$Theme,[Parameter(Mandatory)][string]$Routes,[Parameter(Mandatory)][string]$Helper)
    $nl=[Environment]::NewLine
    $Theme=$Theme.Replace(([string][char]13+[char]10),[string][char]10).Replace([string][char]10,$nl)
    $Routes=$Routes.Replace(([string][char]13+[char]10),[string][char]10).Replace([string][char]10,$nl)
    $Helper=$Helper.Replace(([string][char]13+[char]10),[string][char]10).Replace([string][char]10,$nl)
    $marker='    Friend Function qsitechromecolor(ByVal browser As WebBrowser) As Color'
    $old=$marker+$nl+'        Try'+$nl
    $new=$Helper+$marker+$nl+'        Try'+$nl+'            If browser Is Nothing AndAlso qleaAt() IsNot Nothing Then Return qleapagecolor(qleaAt())'+$nl
    $done='        qleaStylesDone(host)'
    $updated=$done+$nl+'        If host IsNot Nothing AndAlso host Is qleaAt() Then qapplysitechrome(qthemebrowser())'
    if([regex]::Matches($Theme,[regex]::Escape($old)).Count -ne 1 -or
       [regex]::Matches($Routes,[regex]::Escape($done)).Count -ne 1 -or
       $Theme.Contains('qleapagecolor') -or $Routes.Contains($updated)){throw 'Page-color source anchors differ'}
    $themeOut=$Theme.Replace($old,$new);$routesOut=$Routes.Replace($done,$updated)
    if($themeOut.Replace($new,$old) -cne $Theme -or $routesOut.Replace($updated,$done) -cne $Routes){throw 'Page-color inverse differs'}
    @{theme=$themeOut;routes=$routesOut;inverseVerified=$true}
}
