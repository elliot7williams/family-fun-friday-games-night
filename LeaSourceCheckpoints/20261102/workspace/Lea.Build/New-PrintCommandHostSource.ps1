function New-PrintCommandHostSource {
    param([string]$Main,[string]$Targets)
    $main=$Main.Replace("`r",'');$targets=$Targets.Replace("`r",'')
    if($main.Contains('If qpr() Then Return')){throw 'Print command already routed'}
    $edits=@(
        @('    Sub e1(ByVal sender As System.Object, ByVal e As EventArgs) Handles P.Click',"    Sub e1(ByVal sender As System.Object, ByVal e As EventArgs) Handles P.Click`n        If qpr() Then Return"),
        @("    Sub e2(ByVal sender As Object, ByVal e As EventArgs) Handles P.MouseHover`n        If My.Settings.t = `"e`" Then", "    Sub e2(ByVal sender As Object, ByVal e As EventArgs) Handles P.MouseHover`n        If My.Settings.t = `"e`" Then`n            If qpr() Then Return")
    )
    $updated=$main
    foreach($edit in $edits){
        if([regex]::Matches($updated,[regex]::Escape($edit[0])).Count -ne 1){throw 'Print command anchor differs'}
        $updated=$updated.Replace($edit[0],$edit[1])
    }
    $inverse=$updated
    for($index=$edits.Count-1;$index -ge 0;$index--){$inverse=$inverse.Replace($edits[$index][1],$edits[$index][0])}
    if($inverse -cne $main){throw 'Print command inverse differs'}
    $anchor='    <Compile Include="$(MSBuildThisFileDirectory)..\PrintPreview.vb" />'
    $addition=@'
    <Compile Include="$(MSBuildThisFileDirectory)..\PrintCommand.vb" />
    <Reference Include="System.Printing" />
    <Reference Include="ReachFramework" />
'@
    if([regex]::Matches($targets,[regex]::Escape($anchor)).Count -ne 1){throw 'Print target anchor differs'}
    $newTargets=$targets.Replace($anchor,$anchor+"`n"+$addition.TrimEnd("`n"))
    if($newTargets.Replace("`n"+$addition.TrimEnd("`n"),'') -cne $targets){throw 'Print target inverse differs'}
    $null=[xml]$newTargets
    @{main=$updated;targets=$newTargets;inverseVerified=$true}
}
