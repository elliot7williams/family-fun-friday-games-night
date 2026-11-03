$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$workspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root = Join-Path $workspace ('dist/.archive-copy-selftest-' + [Guid]::NewGuid().ToString('N'))
$directory = Join-Path $root 'extracted'
$null = New-Item -ItemType Directory -Path $directory
$archive = Join-Path $root 'fixture.zip'
$fixture = Join-Path $directory 'file.txt'
[IO.File]::WriteAllText($fixture, 'trusted fixture')
[IO.Compression.ZipFile]::CreateFromDirectory($directory, $archive)
$results = [Collections.Generic.List[string]]::new()
function Invoke-FixtureComparison {
    & (Join-Path $PSScriptRoot 'Test-ArchiveCopy.ps1') -Archive $archive -Directory $directory | ConvertFrom-Json
}
$same = Invoke-FixtureComparison
if (!$same.contentMatches -or $same.comparedFiles -ne 1 -or $same.filesystemModified) { throw 'Matching case failed.' }
$results.Add('identical file matches without mutation')
[IO.File]::WriteAllText($fixture, 'altered fixture')
if ((Invoke-FixtureComparison).contentMatches) { throw 'Content mismatch accepted.' }
$results.Add('same-length changed content rejected')
[IO.File]::WriteAllText($fixture, 'trusted fixture')
[IO.File]::WriteAllText((Join-Path $directory 'extra.txt'), 'extra')
$extra = Invoke-FixtureComparison
if ($extra.contentMatches -or 'Extra: extra.txt' -notin $extra.differences) { throw 'Extra file accepted.' }
$results.Add('extra file rejected')
[IO.File]::WriteAllText(($fixture + ':LeaFixture'), 'metadata')
$stream = Invoke-FixtureComparison
if ($stream.fileStreamsPreserved -or !@($stream.streamIssues | Where-Object { $_ -like '*LeaFixture' }).Count) {
    throw 'Unarchived stream accepted.'
}
$results.Add('alternate stream reported')
if ($stream.deletionAuthorized) { throw 'Read-only comparison authorized deletion.' }
$results.Add('comparison never authorizes deletion')
$longDirectory = Join-Path (Join-Path (Join-Path $directory ('x' * 70)) ('y' * 70)) ('z' * 70)
$null = [IO.Directory]::CreateDirectory($longDirectory)
[IO.File]::WriteAllText((Join-Path $longDirectory 'long.txt'), 'long path fixture')
$long = Invoke-FixtureComparison
if (@($long.streamIssues | Where-Object { $_ -like 'Stream audit failed:*' }).Count) { throw 'Long path stream audit failed.' }
$results.Add('long path stream audit completes')
$directoryStream = Join-Path $directory 'directory-with-stream'
$null = [IO.Directory]::CreateDirectory($directoryStream)
[IO.File]::WriteAllText(($directoryStream + ':LeaDirectoryFixture'), 'directory metadata')
$directoryResult = Invoke-FixtureComparison
if ($directoryResult.fileStreamsPreserved -or
    !@($directoryResult.streamIssues | Where-Object { $_ -eq 'Unarchived stream: directory-with-stream:LeaDirectoryFixture' }).Count) {
    throw 'Unarchived directory stream accepted.'
}
$results.Add('directory alternate stream reported')
[IO.File]::WriteAllText(($directory + ':LeaRootFixture'), 'root metadata')
$rootResult = Invoke-FixtureComparison
if ($rootResult.fileStreamsPreserved -or 'Unarchived stream: .:LeaRootFixture' -notin $rootResult.streamIssues) {
    throw 'Unarchived root directory stream accepted.'
}
$results.Add('root directory alternate stream reported')
[pscustomobject]@{passed=$true;checks=$results.ToArray();fixturesRetained=$root} | ConvertTo-Json -Depth 3
