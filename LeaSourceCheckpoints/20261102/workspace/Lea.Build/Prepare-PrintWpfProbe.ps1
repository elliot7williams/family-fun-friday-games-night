$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-PrintFontProbe.generated.ps1'
$target=Join-Path $PSScriptRoot 'Test-PrintWpfProbe.generated.ps1'
$out=Join-Path $workspace 'continuity-evidence/print-wpf-preparation-v1'
if((Test-Path $target) -or (Test-Path $out)){throw 'Preserve WPF preparation'}
if((Get-FileHash $source).Hash -cne '03ECC2F4F2EC2DE97D267C9F6C759D501CE2FC3DB549C3875305E64411676D6C'){throw 'Original runner differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-One([string]$Before,[string]$After){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne 1){throw ('Unique anchor required: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
Replace-One 'print-font-probe-v1' 'print-wpf-probe-v1'
Replace-One 'Owned font diagnostic only: compare native GDI+ and GDI font fallback. Does not run foundation suite, qualify renderer or change production engine' 'Owned WPF vector glyph fallback diagnostic only; no foundation rerun, pagination, actual printer, renderer adoption or host qualification'
Replace-One "'Lea.Tests/PrintFontProbe.cs','Lea.Print/Lea.Print.csproj'" "'Lea.Tests/PrintFontProbe.cs','Lea.Tests/PrintWpfProbe.cs','Lea.Print/Lea.Print.csproj'"
Replace-One '/main:PrintFontProbe' '/main:PrintWpfProbe'
Replace-One "('`"'+(Join-Path `$out 'PrintFontProbe.cs')+'`"')" "('`"'+(Join-Path `$out 'PrintFontProbe.cs')+'`"'),('`"'+(Join-Path `$out 'PrintWpfProbe.cs')+'`"')"
Replace-One 'PASS Lea print font diagnostic generated' 'PASS Lea WPF glyph diagnostic generated'
Replace-One "    `$references.Add('/reference:`"'+`$core+'`"')" @'
    $references.Add('/reference:"'+$core+'"')
    foreach($name in @('PresentationCore.dll')){
        $reference=Join-Path ${env:ProgramFiles(x86)} ('Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/'+$name)
        Hold $reference
        $references.Add('/reference:"'+$reference+'"')
    }
'@
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Projection inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
$receipt=@{inverseVerified=$true;syntaxVerified=$true;original=(Get-FileHash $source);generated=(Get-FileHash $target);edits=$edits}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($receipt|ConvertTo-Json -Depth 6))
$receipt|Select-Object inverseVerified,syntaxVerified,generated|ConvertTo-Json
