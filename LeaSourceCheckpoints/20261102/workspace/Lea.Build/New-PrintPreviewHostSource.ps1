function New-PrintPreviewHostSource {
    param([string]$Main,[string]$Targets)
    $originalMain=$Main.Replace("`r",'');$originalTargets=$Targets.Replace("`r",'')
    if($originalMain.Contains('If qpv() Then Return')){throw 'Preview source already routed'}
    $mainEdits=@(
        @('    Sub e3(ByVal sender As System.Object, ByVal e As EventArgs) Handles PP.Click',"    Sub e3(ByVal sender As System.Object, ByVal e As EventArgs) Handles PP.Click`n        If qpv() Then Return"),
        @("    Sub e4(ByVal sender As Object, ByVal e As EventArgs) Handles PP.MouseHover`n        If My.Settings.t = `"e`" Then", "    Sub e4(ByVal sender As Object, ByVal e As EventArgs) Handles PP.MouseHover`n        If My.Settings.t = `"e`" Then`n            If qpv() Then Return")
    )
    $updated=$originalMain
    foreach($edit in $mainEdits){
        if([regex]::Matches($updated,[regex]::Escape($edit[0])).Count -ne 1){throw 'Preview source anchors differ'}
        $updated=$updated.Replace($edit[0],$edit[1])
    }
    $inverse=$updated
    for($i=$mainEdits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($mainEdits[$i][1],$mainEdits[$i][0])}
    if($inverse -cne $originalMain){throw 'Preview main source inverse differs'}
    $anchor='  <ItemGroup Condition="''$(LeaScriptEnabled)'' == ''true''">'
    $addition=@'
  <ItemGroup>
    <Reference Include="Lea.Print">
      <HintPath>$(MSBuildThisFileDirectory)Lea.Print.dll</HintPath>
      <Private>True</Private>
    </Reference>
    <Reference Include="WindowsFormsIntegration" />
    <Reference Include="System.Xaml" />
    <Compile Include="$(MSBuildThisFileDirectory)..\PrintPreview.vb" />
  </ItemGroup>
'@
    if([regex]::Matches($originalTargets,[regex]::Escape($anchor)).Count -ne 1){throw 'Preview target anchor differs'}
    $targets=$originalTargets.Replace($anchor,$addition.TrimEnd("`n")+"`n"+$anchor)
    if($targets.Replace($addition.TrimEnd("`n")+"`n",'') -cne $originalTargets){throw 'Preview targets inverse differs'}
    $null=[xml]$targets
    @{main=$updated;targets=$targets;inverseVerified=$true;legacyPreviewRetained=$true;physicalPrintChanged=$false}
}
