$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Build-ArchivedParentView.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '5F48FB7E813C3F0F5C923BCE8C588CDA636D2F24C42F6BF2BB4ECE792EADD665'){throw 'Base builder changed'}
$target=Join-Path $PSScriptRoot 'Build-ArchivedNative.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve builder'}
$text=[IO.File]::ReadAllText($base)
foreach($pair in @(
    @('archived-parent-view-build-v3','archived-native-build-v1'),
    @('tested-parent-view-source-v1.zip','tested-native-source-v1.zip'),
    @('F6B770D3AB46AFC1FD6D9DF4D65CD272FA365D6494B9B647DD0DED77A5F12599','A5DAC85239EB06335323454696E7B8F9292859217934599989799E8752B8B158'),
    @("@('parent','view')","@('Lea.Engine','Lea.Image')"),
    @('Relocated archived parent/view compilation only; binary equivalence and runtime NOT verified','Relocated archived engine/image compilation with inventoried current references; historical reference identity, equivalence and runtime NOT verified')
)){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Ambiguous builder substitution'}
    $text=$text.Replace($pair[0],$pair[1])
}
$anchor='    foreach($module in $manifest.modules){'
$addition=@'
    foreach($entry in $manifest.modules){
        $dependencies=@(
            foreach($source in $entry.sources){
                @{kind='source';originalPath=$source.originalPath;archivePath=$source.archivePath;sha256=$source.sha256}
            }
            foreach($reference in $entry.references){
                @{kind='reference';originalPath=$reference.path;sha256=$reference.currentSha256}
            }
        )
        $entry|Add-Member -NotePropertyName dependencies -NotePropertyValue $dependencies
    }
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Manifest anchor changed'}
$text=$text.Replace($anchor,$addition+"`r`n"+$anchor)
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Invalid generated syntax'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
