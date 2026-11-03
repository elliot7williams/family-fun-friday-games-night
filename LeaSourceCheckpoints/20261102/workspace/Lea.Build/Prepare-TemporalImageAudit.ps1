param([ValidateSet('lazy','control')][string]$Variant='lazy')
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$base=Join-Path $workspace 'continuity-evidence/permission-contract-v1/Audit-FirespiderCleanupImageAcl.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne 'D4241C17F51D2D4F661E538D8EBFACE9C56F0586DC888552F27122D169E6197B'){throw 'Classifier base changed'}
$text=[IO.File]::ReadAllText($base)
$target=Join-Path (Split-Path $base) ('Audit-Temporal-'+$Variant+'-ImageAcl.generated.ps1')
if(Test-Path -LiteralPath $target){throw 'Preserve classifier'}
foreach($pair in @(
    @('firespider-cleanup-host-v1',('../temporal-startup-v1/host-'+$Variant+'-v1')),
    @('90A93BAF2C96587465C7F60A7A058F509615C2F82D5F9CECD1BE871525AE5FCB','E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B'),
    @('ACD68BF8D6472BF5659D878D26FC611DC54F0241D97EBD7F010FA129B827E008',$(if($Variant -eq 'control'){'0F63063292774C788B5C9A054862BEE1B5BEDB7308AA4EB9CB0A0AA34784A221'}else{'A5B520E4A9D117854F49A105E2496DDF31796463851B373ACF87D6B571E86DE2'}))
)){
    if(!$text.Contains($pair[0])){throw 'Classifier anchor missing'}
    $text=$text.Replace($pair[0],$pair[1])
}
$tokens=$null;$errors=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Classifier syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
[pscustomobject]@{baseSha256=(Get-FileHash -LiteralPath $base).Hash;generated=$target;sha256=(Get-FileHash -LiteralPath $target).Hash}|ConvertTo-Json
