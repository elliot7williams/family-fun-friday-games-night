$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $workspace 'continuity-evidence/ordered-flow-v6/source/E.cs'
$fragment=Join-Path $PSScriptRoot 'PrintStyles.fragment.cs'
$out=Join-Path $workspace 'continuity-evidence/print-style-core-source-v1'
if(Test-Path $out){throw 'Preserve style source preparation'}
if((Get-FileHash $source).Hash -cne 'E07F8014A89FD7758A927C2EC36DE379BF8097E93DA18D23968AABA4EB60A905'){throw 'Mature E differs'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-Exact([string]$Before,[string]$After,[int]$Count=1){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne $Count){throw ('Anchor count differs: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After,$Count))
}
$anchor='        // Dg captures bounded page and engine diagnostics for developer tools.'
Replace-Exact $anchor (([IO.File]::ReadAllText($fragment).Replace("`r",''))+"`n"+$anchor)
$before=@'
            public bool On(int width, int height, int density, char theme, char motion, char contrast, char scripting,
                char preferredData, char pointer, char hover, char anyPointer, char anyHover)
'@
Replace-Exact $before ($before.TrimEnd("`r","`n").Replace('char anyHover)','char anyHover, bool printing = false)')) 2
Replace-Exact "pointer, hover, anyPointer, anyHover) &&`n                    B.On" "pointer, hover, anyPointer, anyHover, printing) &&`n                    B.On"
Replace-Exact "pointer, hover, anyPointer, anyHover);`n                var matches = Mt != 'p' && Mt != 'x' &&" "pointer, hover, anyPointer, anyHover, printing);`n                var matches = Mt != 'x' && (Mt == 'p' ? printing : Mt != 's' || !printing) &&"
Replace-Exact "pointer, hover, anyPointer, anyHover)) return true;`n                return false;`n            }`n        }`n`n        private sealed class Ic" "pointer, hover, anyPointer, anyHover, printing)) return true;`n                return false;`n            }`n        }`n`n        private sealed class Ic"
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Projection inverse differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText((Join-Path $out 'E.cs'),$text,[Text.UTF8Encoding]::new($false))
@{inverseVerified=$true;source=(Get-FileHash $source);fragment=(Get-FileHash $fragment);candidate=(Get-FileHash (Join-Path $out 'E.cs'));edits=$edits}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json')
Get-FileHash (Join-Path $out 'E.cs')
