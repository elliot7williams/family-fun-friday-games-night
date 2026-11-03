param(
    [Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedRunReceipt,
    [Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedProcessReceipt
)
$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'Audit-OwnedFullScriptOutcome.ps1'
$target=Join-Path $PSScriptRoot 'Audit-OwnedNativeSelectionScriptOutcome.generated.ps1'
if(Test-Path $target){throw 'Preserve generated outcome audit'}
if((Get-FileHash $source).Hash -cne '97C67BC1832369B0B90AACE6830CC9B9E5BFF80344EE02F32A3EEE3660332A3B'){throw 'Original read-only script outcome audit differs'}
$original=[IO.File]::ReadAllText($source)
$edits=@(
    @{old="'current-native-script-owned-run-v1'";new="'native-selection-script-owned-run-v1'";count=1},
    @{old='current-native-script-suite-v8/';new='current-native-script-suite-v9/';count=2},
    @{old='334A45678D717608E7B05C8722EA52C7928B51460658BBAE15740018DB54699D';new=$ExpectedRunReceipt;count=1},
    @{old='E01640B4C0C3467A97431A9C74EA3E2EFAB54344AA9679F1589BF76356A4B248';new=$ExpectedProcessReceipt;count=1},
    @{old='FC4AD1E5E9249348B6A4A8D70609E3BB90ABD38E3458A52D5E52FDAB2F6543C1';new='5EE6B967BAF69F336C393F6AED5E90BE8D2C69D203FA1A24B13B64A92BC53537';count=1},
    @{old='054089709A22C07E7D9C027AC8D03D26A5206D8A1C5B6BBD3B23634D326B689D';new='C5C64B012DDF6738FB95D586B06F77B260F78DAF00EF3A58039D0DEC1D415653';count=1}
)
$text=$original
foreach($edit in $edits){
    if([regex]::Matches($text,[regex]::Escape($edit.old)).Count -ne $edit.count){throw 'Read-only outcome binding anchor differs'}
    $text=$text.Replace($edit.old,$edit.new)
}
$inverse=$text
for($index=$edits.Count-1;$index -ge 0;$index--){$inverse=$inverse.Replace($edits[$index].new,$edits[$index].old)}
if($inverse -cne $original){throw 'Read-only outcome inverse differs'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
& $target
