$ErrorActionPreference='Stop'
if($PSVersionTable.PSEdition -ne 'Desktop'){throw 'Framework metadata audit requires Windows PowerShell'}
$root=Split-Path $PSScriptRoot -Parent
$path='C:\Windows\Microsoft.NET\assembly\GAC_MSIL\UIAutomationClient\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationClient.dll'
$expected='8035D3870294D43DBF0C1FAA4FBD1FB3CB958D1766C249D1929D6FD3B923D4E6'
if((Get-FileHash $path).Hash -cne $expected){throw 'Captured framework client differs'}
$output=Join-Path $root 'continuity-evidence/wpf-cold-legacy-provider-v2/framework-declarations.json'
if(Test-Path $output){throw 'Preserve prior declarations audit'}
$assembly=[Reflection.Assembly]::LoadFrom($path)
$type=$assembly.GetType('MS.Internal.Automation.UiaCoreApi',$true)
$methods=@(foreach($method in $type.GetMethods([Reflection.BindingFlags]'Static,NonPublic')|Where-Object Name -in @('RawTextPattern_GetVisibleRanges','RawUiaHTextRangeFromVariant','RawTextRange_GetText')){
    [pscustomobject]@{method=$method.ToString();import=@($method.GetCustomAttributes([Runtime.InteropServices.DllImportAttribute],$false)|Select-Object Value,EntryPoint,CallingConvention);parameters=@($method.GetParameters()|ForEach-Object {
        [pscustomobject]@{name=$_.Name;type=$_.ParameterType.FullName;attributes=$_.Attributes.ToString();marshal=@($_.GetCustomAttributes([Runtime.InteropServices.MarshalAsAttribute],$false)|Select-Object Value,SafeArraySubType)}
    })}
})
if($methods.Count -ne 3 -or (Get-FileHash $path).Hash -cne $expected){throw 'Declaration audit incomplete or input changed'}
$audit=@{assemblySha256=$expected;runnerSha256=(Get-FileHash $PSCommandPath).Hash;methods=$methods;metadataOnly=$true;legacyGatePassed=$false}
[IO.File]::WriteAllText($output,($audit|ConvertTo-Json -Depth 8))
if((Get-Content $output -Raw|ConvertFrom-Json).methods.Count -ne 3){throw 'Declaration readback differs'}
Write-Output 'FRAMEWORK_UIA_DECLARATIONS_CAPTURED 3; legacy gate unchanged'
