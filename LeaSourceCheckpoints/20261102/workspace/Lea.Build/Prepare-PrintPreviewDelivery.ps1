param([ValidateSet('Package','Extraction')][string]$Stage='Package',[switch]$Command)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$prefix=if($Command){'print-command'}else{'print-preview'}
$out=Join-Path $workspace ('continuity-evidence/'+$prefix+'-delivery-preparation-'+$Stage.ToLowerInvariant()+'-v1')
$source=Join-Path $PSScriptRoot $(if($Stage -eq 'Package'){'Package-Runtime8033Development.generated.ps1'}else{'Test-Runtime8033Extraction.generated.ps1'})
$target=Join-Path $PSScriptRoot $(if($Stage -eq 'Package'){if($Command){'Package-PrintCommandDevelopment.generated.ps1'}else{'Package-PrintPreviewDevelopment.generated.ps1'}}else{if($Command){'Test-PrintCommandExtraction.generated.ps1'}else{'Test-PrintPreviewExtraction.generated.ps1'}})
$pin=if($Stage -eq 'Package'){'B6842B460B6DFDFE1BD81C2F4F8F39AF4279DD45FBBFD139A246744400816900'}else{'D41912733AD0B6DB636B22FEA2EE4390E5988097F296F10B27985B924AE9F5B2'}
if((Test-Path -LiteralPath $out) -or (Test-Path -LiteralPath $target)){throw 'Preserve delivery preparation'}
if((Get-FileHash -LiteralPath $source).Hash -cne $pin){throw 'Original delivery helper changed'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'');$text=$original
$edits=[Collections.Generic.List[object]]::new()
function Replace-Exact([string]$Before,[string]$After,[int]$Count=1){
    if([regex]::Matches($script:text,[regex]::Escape($Before)).Count -ne $Count){throw ('Anchor count differs: '+$Before)}
    $script:text=$script:text.Replace($Before,$After);$edits.Add(@($Before,$After))
}
$campaign=if($Command){'host-print-command-owned-v1'}else{'host-print-preview-owned-v1'}
$campaignHash=if($Command){'82C0C3CFCEECC781622FE265AD90EBA540F7F2BCD230F29ADB988F5DFA4BFC1A'}else{'B721DF1664B22445A09DA73AD2B5F06DD449D8CFD58EA927A547E140AA7AA4DA'}
$zipName=if($Command){'Firespider-Lea-print-development-20261028.zip'}else{'Firespider-Lea-preview-development-20261027.zip'}
$hostHash=if($Command){'FCFAA635B4403BC4F449761C49BA17F6ED9CFEF443D7164C56C924DDB09FA7C7'}else{'F5EBF8F4593FACD4CA33083DEF1B854951DC21232674EAB6C9F3DC0361DD8FBE'}
Replace-Exact 'host-runtime8033-owned-v1' $campaign
Replace-Exact 'A1B1A96C9358525A7704BE283B434AD946D4906C47025E1E01D2C768BD0A12CA' $campaignHash $(if($Stage -eq 'Package'){1}else{2})
Replace-Exact 'Firespider-Lea-runtime8033-development-20261027.zip' $zipName
Replace-Exact 'FBBD913CF0B0C654706844DAF078B01B1DFC4A06DF8687678296DE261B24E818' $hostHash
if($Stage -eq 'Package'){
    Replace-Exact 'runtime-8033-development-package-v1' ($prefix+'-development-package-v1')
    Replace-Exact ' -ne 398' ' -ne 399' 2
    Replace-Exact ' -ne 400' ' -ne 401'
    Replace-Exact 'runtimeFiles=398' 'runtimeFiles=399'
    $sourceReference=if($Command){"@{name='print-command-source-20261028.zip';sha256='7B56572A2318AE3B491320DADAF9EC3AB3B58D57EE90207BCB97B5E4314BCB0F'}"}else{"@{name='print-preview-host-source-20261027.zip';sha256='3BE58454C829D6A3D8E17FE5B762206E47F030F0501C3206E0EDFA6DA3488524'}"}
    Replace-Exact "@{name='runtime-8033-matched-source-20261027.zip';sha256='84179CBDF6C3674E43C886FCF72B5C048056425B2D029E2CE3DBBE4CD719AEB9'}" $sourceReference
    Replace-Exact "@{name='native-selection-host-source-20261027.zip';sha256='D4F8FB82153418B2584C27E6D6A50C9D99499119B28DE7A64C755DACD8D0FC80'}" "@{name='runtime-8033-matched-source-20261027.zip';sha256='84179CBDF6C3674E43C886FCF72B5C048056425B2D029E2CE3DBBE4CD719AEB9'}"
    $printHash=if($Command){'3C014359204CEEDD11E8164A28467D46CB3BA83E5DF6A81802C0F3A7D3AF1795'}else{'FC867957DF7CD7029BF15455C5163027755A79EA3CAECF5EF55963A6F3FE8A6A'}
    Replace-Exact "        'Lea.Engine.dll'=" ("        'Lea.Print.dll'='"+$printHash+"'`n        'Lea.Engine.dll'=")
    $tokens=$null;$errors=$null
    $ast=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
    $notes=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.AssignmentStatementAst] -and $n.Left.Extent.Text -ceq '$note'},$true))
    if($errors.Count -or $notes.Count -ne 1){throw 'Unique package note assignment required'}
    $noteName=if($Command){'PrintCommandDevelopmentNote.txt'}else{'PrintPreviewDevelopmentNote.txt'}
    $note=[IO.File]::ReadAllText((Join-Path $PSScriptRoot $noteName)).Replace("`r",'').TrimEnd("`n")
    if($note.Contains("`n'@")){throw 'Invalid package note terminator'}
    Replace-Exact $notes[0].Extent.Text ("`$note=@'`n"+$note+"`n'@")
}else{
    Replace-Exact 'runtime-8033-package-extraction-v1' ($prefix+'-package-extraction-v1')
    Replace-Exact ' -ne 398' ' -ne 399' 2
    Replace-Exact ' -ne 400' ' -ne 401' 2
    Replace-Exact '$result.before.Count -eq 400' '$result.before.Count -eq 401'
    $package=Get-Content (Join-Path $workspace ('continuity-evidence/'+$prefix+'-development-package-v1/result.json')) -Raw|ConvertFrom-Json
    if(!$package.verified -or $package.runtimeFiles -ne 399 -or $package.hostReceiptSha256 -cne $campaignHash -or (Get-FileHash -LiteralPath $package.archive).Hash -cne $package.sha256){throw 'New verified package required'}
    Replace-Exact '97CC8B8EAC78FC8A07D909786D780C5FC0824F58AA19D1FA2287070DC7A9CC9A' $package.sha256
}
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Delivery projection inverse differs'}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Delivery projection syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
foreach($path in @($PSCommandPath,$target)){Copy-Item -LiteralPath $path -Destination (Join-Path $out ([IO.Path]::GetFileName($path)))}
@{passed=$true;stage=$Stage;inverseVerified=$true;syntaxVerified=$true;sourceSha256=$pin;targetSha256=(Get-FileHash -LiteralPath $target).Hash;runtimeExecuted=$false;scope=('Exact399-file '+$prefix+' development package and original three extraction smokes; no gate waiver or installed replacement')}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $out 'result.json')
Get-Content -LiteralPath (Join-Path $out 'result.json')
