$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Build-SuitePhaseTiming.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '0ADCEC50B98688491AD4CA9EBE1986A0C6E4B1661AB47259EB0B55C1349A0EF5'){throw 'Base builder changed'}
$target=Join-Path $PSScriptRoot 'Build-RelocatedSuitePhase.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve generated builder'}
$text=[IO.File]::ReadAllText($base).Replace("`r`n","`n")
function ReplaceOnce([string]$old,[string]$new){
    if($script:text.IndexOf($old,[StringComparison]::Ordinal) -lt 0 -or $script:text.IndexOf($old,[StringComparison]::Ordinal) -ne $script:text.LastIndexOf($old,[StringComparison]::Ordinal)){throw ('Ambiguous substitution: '+$old)}
    $script:text=$script:text.Replace($old,$new)
}
ReplaceOnce "'suite-phase-build-v1'" "'suite-phase-relocated-v1'"
ReplaceOnce 'Space;$null=New-Item -ItemType Directory $outRoot' 'Space;if((Get-PSDrive C).Free -lt 600MB){throw ''Relocation reserve required''};$null=New-Item -ItemType Directory $outRoot'
$helper=Join-Path $PSScriptRoot 'Initialize-SuiteRelocation.ps1';$helperHash=(Get-FileHash -LiteralPath $helper).Hash
$insert=@'
    $relocationHelper=Join-Path $workspace 'Lea.Build/Initialize-SuiteRelocation.ps1'
    Hold $relocationHelper 'HELPER_HASH'
    . $relocationHelper
    $result.relocation=Initialize-SuiteRelocation $build $base $outRoot $workspace
    $result.runtime=$result.relocation.runtime
'@
$insert=$insert.Replace('HELPER_HASH',$helperHash)
ReplaceOnce '    $overlayPath=Join-Path' ($insert+"`n    `$overlayPath=Join-Path")
$old='            $to=Join-Path (Join-Path $outRoot ''source'') $relative'
$insert=@'
            if($relative -ceq 'Lea.Script.Tests\ModernComponents.cs'){
                $original=[IO.File]::ReadAllText($from)
                if([regex]::Matches($original,$result.relocation.oldManifestSha256).Count -ne 1){throw 'Expected one entry dispatcher binding'}
                $updated=$original.Replace($result.relocation.oldManifestSha256,$result.relocation.newManifestSha256)
                if($updated.Replace($result.relocation.newManifestSha256,$result.relocation.oldManifestSha256) -cne $original){throw 'Dispatcher inverse failed'}
                $selected=Join-Path $outRoot 'ModernComponents.overlay.cs'
                [IO.File]::WriteAllText($selected,$updated,[Text.UTF8Encoding]::new($false))
                Hold $selected (Get-FileHash -LiteralPath $selected).Hash
                $changed=$true
            }
'@
ReplaceOnce $old ($insert+"`n"+$old)
ReplaceOnce '@($result.sources|Where-Object changed).Count -ne 1' '@($result.sources|Where-Object changed).Count -ne 2'
foreach($name in @('Lea.Script.Tests.exe','Lea.Script.Tests.pdb')){
    $text=$text.Replace(('Join-Path $outRoot '''+$name+''''),('Join-Path $result.runtime '''+$name+''''))
}
$old='            if($line.StartsWith(''/reference:'')){$path=$line.Substring(11).Trim(''"'');Hold $path (OriginalHash $path)}'
$new=@'
            if($line.StartsWith('/reference:')){
                $path=$line.Substring(11).Trim('"');$expected=OriginalHash $path
                $newPath=$path.Replace($build.candidate,$result.runtime)
                Hold $newPath $expected
                $line='/reference:"'+$newPath+'"'
            }
'@
ReplaceOnce $old $new
ReplaceOnce '$result.sha256=(Get-FileHash -LiteralPath $result.binary).Hash;$result.passed=$true' @'
$result.sha256=(Get-FileHash -LiteralPath $result.binary).Hash
    $result.files=@(Get-ChildItem -LiteralPath $result.runtime -File -Recurse|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
    if($result.files.Count -ne 65){throw 'Expected65 relocated runtime files'}
    $result.fixtureSource=$build.fixtureSource;$result.fixtureFiles=$build.fixtureFiles
    $result.passed=$true
'@
$tokens=$null;$errors=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ('Generated syntax invalid: '+($errors.Message -join '; '))}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
[pscustomobject]@{generated=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;relocationHelperSha256=$helperHash;syntaxValidated=$true;executed=$false}|ConvertTo-Json
