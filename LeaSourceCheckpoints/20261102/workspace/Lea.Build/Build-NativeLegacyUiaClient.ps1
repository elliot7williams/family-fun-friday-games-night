# Runs within the retained minimal-provider owner; uses its Hold/input/run ledger.
$manifestPath=Join-Path $root 'continuity-evidence/print-package-cancel-v3/compiler-kit-manifest.json'
Hold $manifestPath 'A7BF4C74CBD4EAC8E4D6099835B9B9520E1DDA59CCB952CD8C8DE14420EE4D19'
$manifest=@(Get-Content $manifestPath -Raw|ConvertFrom-Json)
if($manifest.Count -ne 56){throw 'Native compiler inventory differs'}
foreach($file in $manifest){Hold $file.source $file.sha256;Hold $file.path $file.sha256}
$nativeCompiler=Join-Path $root 'continuity-evidence/print-package-cancel-v3/compiler-kit/cl.exe'
$vc='C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VC\Tools\MSVC\14.11.25503'
$sdk='C:\Program Files (x86)\Windows Kits\10'
$nativeSource=Join-Path $root 'Lea.Tests/NativeLegacyUiaClient.cpp';Hold $nativeSource
$copy=Join-Path $out 'NativeLegacyUiaClient.cpp';Copy-Item $nativeSource $copy;Hold $copy
$serverExe=$exe;$exe=Join-Path $out 'NativeLegacyUiaClient.exe'
$nativeArguments=@('/nologo','/EHsc','/W4','/WX','/MT',('/I"'+$vc+'\include"'))
if($NativeEdit){$nativeArguments+='/DNATIVE_EDIT_SERVER'}
foreach($part in @('ucrt','shared','um','winrt')){$nativeArguments+=('/I"'+$sdk+'\Include\10.0.16299.0\'+$part+'"')}
$nativeArguments+=@(('/Fe:"'+$exe+'"'),('/Fo:"'+(Join-Path $out 'native-client.obj')+'"'),('"'+$copy+'"'),'/link',('/LIBPATH:"'+$vc+'\lib\x64"'),('/LIBPATH:"'+$sdk+'\Lib\10.0.16299.0\um\x64"'),('/LIBPATH:"'+$sdk+'\Lib\10.0.16299.0\ucrt\x64"'),'uiautomationcore.lib','ole32.lib','oleaut32.lib','user32.lib')
$previousPath=$env:PATH
try{
    $env:PATH=(Split-Path $nativeCompiler)+';'+$previousPath
    $nativeBuild=Invoke-OwnedTestRun -Name 'compile-native-uia-client' -Application $nativeCompiler -Arguments $nativeArguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$nativeBuild.passed){throw 'Native UIA client compilation failed'}
}finally{$env:PATH=$previousPath}
Hold $exe
if($NativeEdit){$serverExe=$exe}
