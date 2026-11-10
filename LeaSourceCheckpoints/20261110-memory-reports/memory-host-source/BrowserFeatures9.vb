Imports System.Collections.Specialized
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Partial Class j
    Const qf9DownloadRecordPrefix As String = "FIRESPIDER_DOWNLOAD_SAFETY"
    Const qf9IdentityPackagePrefix As String = "FIRESPIDER_IDENTITY_PACKAGE"
    Const qf9IdentityPayloadPrefix As String = "FIRESPIDER_IDENTITY_PAYLOAD"
    Const qf9ProtectedTransferPrefix As String = "FIRESPIDER_REWRAPPED_DPAPI_V1|"
    Const qf9IdentityPackageIterations As Integer = 120000

    Sub qfeatures9()
        fl.DropDownItems.Add(qnewitem("Download Safety Center", AddressOf q941, AddressOf q942))
        ot.DropDownItems.Add(qnewitem("Password-Protected Identity Transfer", AddressOf q943, AddressOf q944))
    End Sub

    ' --- Download Safety Center ---
    Sub q941(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If qf9settingcollection("downloadSafety") Is Nothing OrElse qf9settingcollection("trustedHashes") Is Nothing Then
                MessageBox.Show("The Download Safety settings are not available in this build.", "Download Safety Center", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If
            Dim center As New DownloadSafetyCenterForm(Me)
            center.Show(Me)
        Catch ex As Exception
            qlog(ex)
            MessageBox.Show("The Download Safety Center could not be opened.", "Download Safety Center", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Sub q942(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q941(sender, e)
    End Sub

    Friend Function qf9settingcollection(ByVal settingName As String) As StringCollection
        If My.Settings.Properties(settingName) Is Nothing Then Return Nothing
        Dim values As StringCollection = TryCast(My.Settings(settingName), StringCollection)
        If values Is Nothing Then
            values = New StringCollection
            My.Settings(settingName) = values
        End If
        Return values
    End Function

    Friend Function qf9safetyrecords() As List(Of DownloadSafetyRecord)
        Dim records As New List(Of DownloadSafetyRecord)
        Dim values As StringCollection = qf9settingcollection("downloadSafety")
        If values Is Nothing Then Return records
        For Each raw As String In values
            Dim record As DownloadSafetyRecord = qf9parsesafetyrecord(raw)
            If record IsNot Nothing Then records.Add(record)
        Next raw
        Return records
    End Function

    Function qf9parsesafetyrecord(ByVal raw As String) As DownloadSafetyRecord
        Try
            Dim fields As String() = raw.Split("|"c)
            If fields.Length <> 11 OrElse fields(0) <> qf9DownloadRecordPrefix OrElse fields(1) <> "1" Then Return Nothing
            Dim baselineSize As Long = -1
            Dim lastSize As Long = -1
            Dim lastWriteTicks As Long = 0
            If Long.TryParse(fields(5), baselineSize) = False Then Return Nothing
            If Long.TryParse(fields(8), lastSize) = False Then Return Nothing
            If Long.TryParse(fields(10), lastWriteTicks) = False Then Return Nothing
            Dim record As New DownloadSafetyRecord
            record.Path = qunb64(fields(2))
            record.Source = qunb64(fields(3))
            record.BaselineHash = fields(4).ToLowerInvariant
            record.BaselineSize = baselineSize
            record.FirstSeen = qunb64(fields(6))
            record.LastHash = fields(7).ToLowerInvariant
            record.LastSize = lastSize
            record.LastChecked = qunb64(fields(9))
            record.LastWriteUtcTicks = lastWriteTicks
            If record.Path = "" Then Return Nothing
            Return record
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Function qf9safetyraw(ByVal record As DownloadSafetyRecord) As String
        Return qf9DownloadRecordPrefix & "|1|" & qb64(record.Path) & "|" & qb64(record.Source) & "|" & record.BaselineHash & "|" & record.BaselineSize.ToString & "|" & qb64(record.FirstSeen) & "|" & record.LastHash & "|" & record.LastSize.ToString & "|" & qb64(record.LastChecked) & "|" & record.LastWriteUtcTicks.ToString
    End Function

    Friend Sub qf9savesafetyrecords(ByVal records As IEnumerable(Of DownloadSafetyRecord))
        Dim values As StringCollection = qf9settingcollection("downloadSafety")
        If values Is Nothing Then Throw New InvalidOperationException("The downloadSafety setting is unavailable.")
        values.Clear()
        Dim paths As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each record As DownloadSafetyRecord In records.OrderBy(Function(item) item.FirstSeen).ThenBy(Function(item) item.Path)
            If record.Path <> "" AndAlso paths.Add(record.Path) Then values.Add(qf9safetyraw(record))
        Next record
        My.Settings.Save()
    End Sub

    Friend Function qf9syncsafety() As List(Of DownloadSafetyRecord)
        Dim byPath As New Dictionary(Of String, DownloadSafetyRecord)(StringComparer.OrdinalIgnoreCase)
        For Each record As DownloadSafetyRecord In qf9safetyrecords()
            If byPath.ContainsKey(record.Path) = False Then byPath.Add(record.Path, record)
        Next record
        For Each tracked As DownloadQueueRecord In qdownloadrecords()
            If tracked Is Nothing OrElse tracked.Path = "" Then Continue For
            Dim record As DownloadSafetyRecord = Nothing
            If byPath.TryGetValue(tracked.Path, record) = False Then
                record = New DownloadSafetyRecord
                record.Path = tracked.Path
                record.BaselineSize = -1
                record.LastSize = -1
                record.FirstSeen = If(tracked.DateSaved = "", Now.ToString("o"), tracked.DateSaved)
                byPath.Add(record.Path, record)
            End If
            If record.Source = "" Then record.Source = tracked.Source
            qf9inspectdownload(record, False)
        Next tracked
        Dim result As List(Of DownloadSafetyRecord) = byPath.Values.ToList
        qf9savesafetyrecords(result)
        Return result
    End Function

    Friend Sub qf9rechecksafety(ByVal record As DownloadSafetyRecord)
        qf9inspectdownload(record, True)
        Dim records As List(Of DownloadSafetyRecord) = qf9safetyrecords()
        Dim replaced As Boolean = False
        For i As Integer = 0 To records.Count - 1
            If records(i).Path.Equals(record.Path, StringComparison.OrdinalIgnoreCase) Then
                records(i) = record
                replaced = True
                Exit For
            End If
        Next i
        If replaced = False Then records.Add(record)
        qf9savesafetyrecords(records)
    End Sub

    Sub qf9inspectdownload(ByVal record As DownloadSafetyRecord, ByVal force As Boolean)
        record.LastError = ""
        If File.Exists(record.Path) = False Then
            record.LastSize = -1
            record.LastWriteUtcTicks = 0
            record.LastChecked = Now.ToString("o")
            Exit Sub
        End If
        Try
            Dim information As New FileInfo(record.Path)
            Dim requiresHash As Boolean = force OrElse record.BaselineHash = "" OrElse information.Length <> record.LastSize OrElse information.LastWriteTimeUtc.Ticks <> record.LastWriteUtcTicks
            If requiresHash = False Then Exit Sub
            Dim currentHash As String = qf9sha256(record.Path)
            If record.BaselineHash = "" Then
                record.BaselineHash = currentHash
                record.BaselineSize = information.Length
            End If
            record.LastHash = currentHash
            record.LastSize = information.Length
            record.LastWriteUtcTicks = information.LastWriteTimeUtc.Ticks
            record.LastChecked = Now.ToString("o")
            If record.FirstSeen = "" Then record.FirstSeen = Now.ToString("o")
        Catch ex As Exception
            record.LastError = ex.Message
            record.LastChecked = Now.ToString("o")
        End Try
    End Sub

    Function qf9sha256(ByVal path As String) As String
        Using input As FileStream = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
            Using sha As SHA256 = SHA256.Create()
                Return BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLowerInvariant
            End Using
        End Using
    End Function

    Friend Function qf9istrusted(ByVal hash As String) As Boolean
        If hash = "" Then Return False
        Dim values As StringCollection = qf9settingcollection("trustedHashes")
        If values Is Nothing Then Return False
        For Each raw As String In values
            Dim candidate As String = raw.Trim.ToLowerInvariant
            If candidate.StartsWith("sha256|") Then candidate = candidate.Substring(7)
            If candidate = hash.ToLowerInvariant Then Return True
        Next raw
        Return False
    End Function

    Friend Sub qf9trusthash(ByVal hash As String)
        Dim normalized As String = hash.Trim.ToLowerInvariant
        If normalized.Length <> 64 OrElse normalized.Any(Function(character) Uri.IsHexDigit(character) = False) Then Throw New InvalidDataException("The selected file does not have a valid SHA-256 hash.")
        Dim values As StringCollection = qf9settingcollection("trustedHashes")
        If values Is Nothing Then Throw New InvalidOperationException("The trustedHashes setting is unavailable.")
        If qf9istrusted(normalized) = False Then values.Add(normalized)
        My.Settings.Save()
    End Sub

    Friend Function qf9downloadwarnings(ByVal record As DownloadSafetyRecord) As List(Of String)
        Dim warnings As New List(Of String)
        Dim fileName As String = Path.GetFileName(record.Path)
        Dim extension As String = Path.GetExtension(fileName).ToLowerInvariant
        Dim executableExtensions As String() = New String() {".exe", ".com", ".scr", ".bat", ".cmd", ".msi", ".msp", ".ps1", ".vbs", ".vbe", ".js", ".jse", ".wsf", ".wsh", ".hta", ".cpl", ".dll", ".sys", ".jar", ".reg", ".lnk", ".pif", ".application", ".gadget", ".appref-ms"}
        If executableExtensions.Contains(extension) Then warnings.Add("Executable-type file (" & extension & "); do not run it unless its source and hash are trusted.")
        Dim stem As String = Path.GetFileNameWithoutExtension(fileName)
        Dim earlierExtension As String = Path.GetExtension(stem)
        If extension <> "" AndAlso earlierExtension <> "" Then warnings.Add("Double extension (" & earlierExtension.ToLowerInvariant & extension & ") may disguise the actual file type.")
        Try
            Dim source As Uri = Nothing
            If Uri.TryCreate(record.Source, UriKind.Absolute, source) AndAlso (source.Scheme = Uri.UriSchemeHttp OrElse source.Scheme = Uri.UriSchemeHttps) Then
                Dim sourceExtension As String = Path.GetExtension(source.AbsolutePath).ToLowerInvariant
                If sourceExtension <> "" AndAlso extension <> "" AndAlso sourceExtension <> extension AndAlso New String() {".htm", ".html", ".php", ".asp", ".aspx", ".jsp"}.Contains(sourceExtension) = False Then warnings.Add("Saved extension " & extension & " differs from the source URL extension " & sourceExtension & ".")
            End If
        Catch
        End Try
        Return warnings
    End Function

    Friend Sub qf9removesafetyrecord(ByVal record As DownloadSafetyRecord)
        If record Is Nothing Then Exit Sub
        Dim values As StringCollection = qf9settingcollection("downloadSafety")
        If values IsNot Nothing Then
            For i As Integer = values.Count - 1 To 0 Step -1
                Dim saved As DownloadSafetyRecord = qf9parsesafetyrecord(values(i))
                If saved IsNot Nothing AndAlso saved.Path.Equals(record.Path, StringComparison.OrdinalIgnoreCase) Then values.RemoveAt(i)
            Next i
        End If
        For Each tracked As DownloadQueueRecord In qdownloadrecords().ToArray
            If tracked.Path.Equals(record.Path, StringComparison.OrdinalIgnoreCase) Then qupdatedownload(tracked, True)
        Next tracked
        My.Settings.dl.Remove(record.Path)
        My.Settings.Save()
    End Sub

    Friend Function qf9downloadstatus(ByVal record As DownloadSafetyRecord, ByVal allRecords As IEnumerable(Of DownloadSafetyRecord)) As String
        Dim states As New List(Of String)
        If File.Exists(record.Path) = False Then
            states.Add("Missing")
        ElseIf record.LastError <> "" Then
            states.Add("Check failed")
        ElseIf record.BaselineHash <> "" AndAlso record.LastHash <> "" AndAlso record.BaselineHash.Equals(record.LastHash, StringComparison.OrdinalIgnoreCase) = False Then
            states.Add("Changed")
        Else
            states.Add("Verified")
        End If
        If qf9downloadwarnings(record).Count > 0 Then states.Add("Warning")
        If record.LastHash <> "" AndAlso allRecords.Count(Function(item) item.Path.Equals(record.Path, StringComparison.OrdinalIgnoreCase) = False AndAlso item.LastHash.Equals(record.LastHash, StringComparison.OrdinalIgnoreCase)) > 0 Then states.Add("Duplicate hash")
        If qf9istrusted(record.LastHash) Then states.Add("Trusted hash")
        Return String.Join(" · ", states.ToArray)
    End Function

    Friend Function qf9duplicatepaths(ByVal record As DownloadSafetyRecord, ByVal allRecords As IEnumerable(Of DownloadSafetyRecord)) As String()
        If record.LastHash = "" Then Return New String() {}
        Return allRecords.Where(Function(item) item.Path.Equals(record.Path, StringComparison.OrdinalIgnoreCase) = False AndAlso item.LastHash.Equals(record.LastHash, StringComparison.OrdinalIgnoreCase)).Select(Function(item) item.Path).ToArray
    End Function

    Friend Function qf9quarantinefolder() As String
        If My.Settings.Properties("quarantineFolder") IsNot Nothing Then
            Dim configured As Object = My.Settings("quarantineFolder")
            If configured IsNot Nothing AndAlso configured.ToString.Trim <> "" Then Return configured.ToString
        End If
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Firespider Quarantine")
    End Function

    Friend Sub qf9setquarantinefolder(ByVal folder As String)
        If My.Settings.Properties("quarantineFolder") Is Nothing Then Throw New InvalidOperationException("The quarantineFolder setting is unavailable.")
        My.Settings("quarantineFolder") = folder
        My.Settings.Save()
    End Sub

    Friend Function qf9quarantinedownload(ByVal record As DownloadSafetyRecord, ByRef destination As String, ByRef detail As String) As Boolean
        destination = ""
        detail = ""
        If record Is Nothing OrElse File.Exists(record.Path) = False Then
            detail = "The selected file no longer exists."
            Return False
        End If
        Dim originalPath As String = record.Path
        Dim folder As String = qf9quarantinefolder()
        Try
            Directory.CreateDirectory(folder)
            If Path.GetFullPath(Path.GetDirectoryName(originalPath)).TrimEnd(Path.DirectorySeparatorChar).Equals(Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) Then
                detail = "The file is already in the quarantine folder."
                Return False
            End If
            destination = qf9uniquefile(Path.Combine(folder, Path.GetFileName(originalPath)))
            File.Move(originalPath, destination)
        Catch ex As Exception
            detail = ex.Message
            Return False
        End Try
        Try
            Dim records As List(Of DownloadSafetyRecord) = qf9safetyrecords()
            Dim found As Boolean = False
            For Each saved As DownloadSafetyRecord In records
                If saved.Path.Equals(originalPath, StringComparison.OrdinalIgnoreCase) Then
                    saved.Path = destination
                    saved.LastWriteUtcTicks = New FileInfo(destination).LastWriteTimeUtc.Ticks
                    record = saved
                    found = True
                    Exit For
                End If
            Next saved
            If found = False Then
                record.Path = destination
                records.Add(record)
            End If
            qf9savesafetyrecords(records)
            For Each tracked As DownloadQueueRecord In qdownloadrecords().ToArray
                If tracked.Path.Equals(originalPath, StringComparison.OrdinalIgnoreCase) Then
                    qupdatedownload(tracked, True)
                    tracked.Path = destination
                    tracked.Status = "quarantined"
                    qupdatedownload(tracked, False)
                End If
            Next tracked
            detail = "The file was moved without being opened or executed."
        Catch ex As Exception
            qlog(ex)
            detail = "The file was moved, but its tracking record could not be fully updated."
        End Try
        Return True
    End Function

    Function qf9uniquefile(ByVal requestedPath As String) As String
        If File.Exists(requestedPath) = False AndAlso Directory.Exists(requestedPath) = False Then Return requestedPath
        Dim folder As String = Path.GetDirectoryName(requestedPath)
        Dim stem As String = Path.GetFileNameWithoutExtension(requestedPath)
        Dim extension As String = Path.GetExtension(requestedPath)
        Dim index As Integer = 2
        Do
            Dim candidate As String = Path.Combine(folder, stem & " (" & index.ToString & ")" & extension)
            If File.Exists(candidate) = False AndAlso Directory.Exists(candidate) = False Then Return candidate
            index += 1
        Loop
    End Function

    Friend Function qf9formatbytes(ByVal size As Long) As String
        If size < 0 Then Return "Unavailable"
        If size < 1024 Then Return size.ToString("N0") & " bytes"
        If size < 1048576 Then Return (size / 1024.0R).ToString("N1") & " KB"
        If size < 1073741824 Then Return (size / 1048576.0R).ToString("N1") & " MB"
        Return (size / 1073741824.0R).ToString("N2") & " GB"
    End Function

    ' --- Password-Protected Identity Transfer ---
    Sub q943(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim action As String = InputBox("Export packages contain selected serialized identity categories in one encrypted file." & vbCrLf & "Import validates integrity before showing a preview or changing settings." & vbCrLf & vbCrLf & "Enter EXPORT or IMPORT:", "Password-Protected Identity Transfer", "EXPORT").Trim.ToUpperInvariant
            If action = "EXPORT" Then
                qf9exportidentity()
            ElseIf action = "IMPORT" Then
                qf9importidentity()
            ElseIf action <> "" Then
                MessageBox.Show("Enter EXPORT or IMPORT.", "Identity Transfer", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            qlog(ex)
            MessageBox.Show("The identity transfer could not be completed.", "Identity Transfer", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Sub q944(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q943(sender, e)
    End Sub

    Sub qf9exportidentity()
        Dim names As List(Of String) = qidentitynames()
        Dim enteredName As String = InputBox("Enter the identity to export, or CURRENT:" & vbCrLf & String.Join(", ", names.ToArray), "Export Identity", If(My.Settings.activeIdentity = "", "Default", My.Settings.activeIdentity)).Trim
        If enteredName = "" Then Exit Sub
        If enteredName.Equals("CURRENT", StringComparison.OrdinalIgnoreCase) Then enteredName = If(My.Settings.activeIdentity = "", "Default", My.Settings.activeIdentity)
        Dim identityName As String = qf9resolvedidentity(enteredName)
        If identityName = "" Then
            MessageBox.Show("That identity does not exist.", "Export Identity", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        If identityName.Equals(My.Settings.activeIdentity, StringComparison.OrdinalIgnoreCase) Then qsaveidentity(identityName)
        Dim allRecords As List(Of String) = My.Settings.idp.Cast(Of String)().Where(Function(raw) qf9recordidentity(raw).Equals(identityName, StringComparison.OrdinalIgnoreCase)).ToList
        If allRecords.Count = 0 Then
            MessageBox.Show("The selected identity has no serialized records to export.", "Export Identity", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        Dim available As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each raw As String In allRecords
            Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
            If fields.Length = 3 Then available.Add(fields(1))
        Next raw
        Dim categoryAnswer As String = InputBox("Available category IDs:" & vbCrLf & String.Join(", ", available.OrderBy(Function(value) value).ToArray) & vbCrLf & vbCrLf & "Enter ALL or comma-separated category IDs. Identity metadata is always included.", "Export Identity Categories", "ALL").Trim
        If categoryAnswer = "" Then Exit Sub
        Dim selectedCategories As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If categoryAnswer.Equals("ALL", StringComparison.OrdinalIgnoreCase) Then
            selectedCategories.UnionWith(available)
        Else
            For Each category As String In categoryAnswer.Split(","c)
                Dim normalized As String = category.Trim
                If available.Contains(normalized) Then selectedCategories.Add(normalized)
            Next category
        End If
        If available.Contains("I") Then selectedCategories.Add("I")
        Dim selectedRecords As List(Of String) = allRecords.Where(Function(raw) selectedCategories.Contains(qf9recordcategory(raw))).ToList
        If selectedRecords.Count = 0 Then
            MessageBox.Show("No matching categories were selected.", "Export Identity", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        Dim protectedCount As Integer = 0
        Dim protectedFailure As String = ""
        Dim packagedRecords As List(Of String) = qf9prepareprotectedexport(selectedRecords, protectedCount, protectedFailure)
        If packagedRecords Is Nothing Then
            MessageBox.Show("A DPAPI-protected vault or autofill record could not be decrypted for this Windows user, so no package was written." & If(protectedFailure = "", "", vbCrLf & vbCrLf & protectedFailure), "Identity Export Stopped", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        Dim dialog As New SaveFileDialog
        qsavefolder(dialog)
        dialog.Filter = "Firespider Encrypted Identity (*.fsiidentity)|*.fsiidentity|All Files (*.*)|*.*"
        dialog.DefaultExt = "fsiidentity"
        dialog.AddExtension = True
        dialog.FileName = "Firespider Identity - " & qf9safefilename(identityName) & ".fsiidentity"
        If dialog.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        Dim password As String = qf9passwordprompt("Export Identity Password", "Enter and confirm a package password of at least 8 characters. The password is not stored.", True)
        If password = "" Then Exit Sub
        Dim payload As String = qf9buildidentitypayload(identityName, selectedCategories, packagedRecords)
        Dim encrypted As String = qf9encryptidentitypackage(payload, password)
        File.WriteAllText(dialog.FileName, encrypted, New UTF8Encoding(False))
        password = ""
        payload = ""
        encrypted = ""
        packagedRecords.Clear()
        su.Visible = True
        su.Text = selectedRecords.Count.ToString & " identity record(s) exported"
        MessageBox.Show(selectedRecords.Count.ToString & " record(s) from " & selectedCategories.Count.ToString & " category ID(s) were exported to one authenticated encrypted package." & vbCrLf & protectedCount.ToString & " vault/autofill record(s) were decrypted only in memory and rewrapped for the destination Windows user.", "Identity Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Sub qf9importidentity()
        Dim dialog As New OpenFileDialog
        dialog.Filter = "Firespider Encrypted Identity (*.fsiidentity)|*.fsiidentity|All Files (*.*)|*.*"
        dialog.CheckFileExists = True
        If Directory.Exists(My.Settings.df) Then dialog.InitialDirectory = My.Settings.df
        If dialog.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        If New FileInfo(dialog.FileName).Length > 52428800 Then
            MessageBox.Show("The selected package is too large.", "Import Identity", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        Dim serialized As String = File.ReadAllText(dialog.FileName, Encoding.UTF8)
        Dim password As String = qf9passwordprompt("Import Identity Password", "Enter the package password. Wrong passwords and integrity failures make no changes.", False)
        If password = "" Then Exit Sub
        Dim failure As String = ""
        Dim plaintext As String = qf9decryptidentitypackage(serialized, password, failure)
        password = ""
        serialized = ""
        If plaintext Is Nothing Then
            MessageBox.Show("The password is wrong, the package was changed, or the file is invalid. No settings were changed." & If(failure = "", "", vbCrLf & vbCrLf & failure), "Identity Package Rejected", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        Dim payload As IdentityTransferPayload = qf9parseidentitypayload(plaintext, failure)
        plaintext = ""
        If payload Is Nothing Then
            MessageBox.Show("The decrypted identity payload is invalid. No settings were changed." & If(failure = "", "", vbCrLf & vbCrLf & failure), "Identity Package Rejected", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        Dim preview As String = "Identity: " & payload.SourceName & vbCrLf & "Exported: " & payload.Exported & vbCrLf & "Category IDs: " & String.Join(", ", payload.Categories.OrderBy(Function(value) value).ToArray) & vbCrLf & "Serialized records: " & payload.Records.Count.ToString & vbCrLf & "Portable protected records: " & qf9protectedrecordcount(payload.Records).ToString & vbCrLf & vbCrLf & "Authenticated package integrity was verified before this preview. Continue to choose NEW or MERGE?"
        If MessageBox.Show(preview, "Identity Import Preview", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) <> DialogResult.OK Then Exit Sub
        Dim mode As String = InputBox("Enter NEW to create a separate identity, or MERGE to add non-duplicate records to an existing identity." & vbCrLf & "MERGE keeps existing scalar values when conflicts occur.", "Identity Import Mode", "NEW").Trim.ToUpperInvariant
        If mode <> "NEW" AndAlso mode <> "MERGE" Then Exit Sub
        Dim targetName As String = ""
        If mode = "NEW" Then
            Dim requested As String = qf9cleanidentityname(InputBox("Enter the name for the imported identity:", "New Imported Identity", payload.SourceName).Trim)
            If requested = "" Then Exit Sub
            targetName = qf9uniqueidentityname(requested)
            If targetName <> requested Then MessageBox.Show("That name already exists. The package will be imported as " & targetName & ".", "Identity Name Conflict", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            Dim existingNames As List(Of String) = qidentitynames()
            Dim requested As String = InputBox("Enter the existing identity to merge into, or CURRENT:" & vbCrLf & String.Join(", ", existingNames.ToArray), "Merge Imported Identity", If(My.Settings.activeIdentity = "", "Default", My.Settings.activeIdentity)).Trim
            If requested = "" Then Exit Sub
            If requested.Equals("CURRENT", StringComparison.OrdinalIgnoreCase) Then requested = If(My.Settings.activeIdentity = "", "Default", My.Settings.activeIdentity)
            targetName = qf9resolvedidentity(requested)
            If targetName = "" Then
                MessageBox.Show("The merge target does not exist.", "Import Identity", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If
        End If
        Dim finalWarning As String = "Import " & payload.Records.Count.ToString & " record(s) using " & mode & " into '" & targetName & "'?" & vbCrLf & vbCrLf & "A rotating settings backup will be required before any records are changed."
        If targetName.Equals(My.Settings.activeIdentity, StringComparison.OrdinalIgnoreCase) Then finalWarning &= vbCrLf & "The active identity and its tabs will be reloaded after the merge."
        If mode = "MERGE" Then finalWarning &= vbCrLf & vbCrLf & qf9mergepreview(payload.Records, targetName)
        If MessageBox.Show(finalWarning, "Confirm Identity Import", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
        Dim summary As String = ""
        If qf9applyidentityimport(payload, mode, targetName, summary, failure) = False Then
            MessageBox.Show("The import failed and the identity records were rolled back." & If(failure = "", "", vbCrLf & vbCrLf & failure), "Identity Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        su.Visible = True
        su.Text = summary
        MessageBox.Show(summary & vbCrLf & vbCrLf & "A rollback backup was created first. Portable vault/autofill values were protected for this Windows user before being saved.", "Identity Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Function qf9passwordprompt(ByVal title As String, ByVal prompt As String, ByVal confirm As Boolean) As String
        Using dialog As New IdentityTransferPasswordForm(title, prompt, confirm)
            If dialog.ShowDialog(Me) = DialogResult.OK Then Return dialog.PasswordValue
        End Using
        Return ""
    End Function

    Function qf9prepareprotectedexport(ByVal records As IEnumerable(Of String), ByRef protectedCount As Integer, ByRef failure As String) As List(Of String)
        protectedCount = 0
        failure = ""
        Dim result As New List(Of String)
        Try
            For Each raw As String In records
                Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
                If fields.Length <> 3 Then Throw New InvalidDataException("An identity record is malformed.")
                If fields(1).Equals("vault", StringComparison.OrdinalIgnoreCase) OrElse fields(1).Equals("af", StringComparison.OrdinalIgnoreCase) Then
                    Dim encryptedValue As String = qunb64(fields(2))
                    Dim plaintext As String = qvaultunprotect(encryptedValue)
                    fields(2) = qb64(qf9ProtectedTransferPrefix & qb64(plaintext))
                    plaintext = ""
                    encryptedValue = ""
                    protectedCount += 1
                    result.Add(String.Join("|", fields))
                Else
                    result.Add(raw)
                End If
            Next raw
            Return result
        Catch ex As Exception
            failure = ex.Message
            result.Clear()
            Return Nothing
        End Try
    End Function

    Function qf9prepareprotectedimport(ByVal records As IEnumerable(Of String), ByRef protectedCount As Integer, ByRef failure As String) As List(Of String)
        protectedCount = 0
        failure = ""
        Dim result As New List(Of String)
        Try
            For Each raw As String In records
                Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
                If fields.Length <> 3 Then Throw New InvalidDataException("An identity record is malformed.")
                Dim value As String = qunb64(fields(2))
                If (fields(1).Equals("vault", StringComparison.OrdinalIgnoreCase) OrElse fields(1).Equals("af", StringComparison.OrdinalIgnoreCase)) AndAlso value.StartsWith(qf9ProtectedTransferPrefix, StringComparison.Ordinal) Then
                    Dim plaintext As String = ""
                    If qf9trydecode64(value.Substring(qf9ProtectedTransferPrefix.Length), plaintext) = False Then Throw New InvalidDataException("A portable protected value is malformed.")
                    fields(2) = qb64(qvaultprotect(plaintext))
                    plaintext = ""
                    protectedCount += 1
                    result.Add(String.Join("|", fields))
                Else
                    result.Add(raw)
                End If
                value = ""
            Next raw
            Return result
        Catch ex As Exception
            failure = ex.Message
            result.Clear()
            Return Nothing
        End Try
    End Function

    Function qf9protectedrecordcount(ByVal records As IEnumerable(Of String)) As Integer
        Dim count As Integer = 0
        For Each raw As String In records
            Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
            If fields.Length = 3 AndAlso qunb64(fields(2)).StartsWith(qf9ProtectedTransferPrefix, StringComparison.Ordinal) Then count += 1
        Next raw
        Return count
    End Function

    Function qf9mergepreview(ByVal incoming As IEnumerable(Of String), ByVal targetName As String) As String
        Dim existingKeys As New HashSet(Of String)(StringComparer.Ordinal)
        Dim existingScalars As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each raw As String In My.Settings.idp.Cast(Of String)().Where(Function(value) qf9recordidentity(value).Equals(targetName, StringComparison.OrdinalIgnoreCase))
            Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
            If fields.Length <> 3 Then Continue For
            existingKeys.Add(qf9logicalrecordkey(raw))
            If fields(1).StartsWith("@") AndAlso existingScalars.ContainsKey(fields(1)) = False Then existingScalars.Add(fields(1), fields(2))
        Next raw
        Dim duplicates As Integer = 0
        Dim scalarConflicts As Integer = 0
        Dim additions As Integer = 0
        For Each raw As String In incoming
            Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
            If fields.Length <> 3 Then Continue For
            If fields(1).StartsWith("@") AndAlso existingScalars.ContainsKey(fields(1)) Then
                If existingScalars(fields(1)) = fields(2) Then duplicates += 1 Else scalarConflicts += 1
            ElseIf existingKeys.Contains(qf9logicalrecordkey(raw)) Then
                duplicates += 1
            Else
                additions += 1
            End If
        Next raw
        Return "Merge preview: " & additions.ToString & " addition(s), " & duplicates.ToString & " duplicate(s), and " & scalarConflicts.ToString & " scalar conflict(s). Existing scalar values win."
    End Function

    Function qf9logicalrecordkey(ByVal raw As String) As String
        Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
        If fields.Length <> 3 Then Return raw
        Dim category As String = fields(1).ToLowerInvariant
        If category = "vault" OrElse category = "af" Then
            Try
                Dim value As String = qunb64(fields(2))
                Dim plaintext As String = ""
                If value.StartsWith(qf9ProtectedTransferPrefix, StringComparison.Ordinal) Then
                    If qf9trydecode64(value.Substring(qf9ProtectedTransferPrefix.Length), plaintext) = False Then Return category & "|" & fields(2)
                Else
                    plaintext = qvaultunprotect(value)
                End If
                Dim key As String = category & "|protected|" & q8sha256(plaintext)
                plaintext = ""
                value = ""
                Return key
            Catch
            End Try
        End If
        Return category & "|" & fields(2)
    End Function

    Function qf9buildidentitypayload(ByVal identityName As String, ByVal categories As IEnumerable(Of String), ByVal records As IEnumerable(Of String)) As String
        Dim data As New StringBuilder
        data.AppendLine(qf9IdentityPayloadPrefix & "|1")
        data.AppendLine("N|" & qb64(identityName))
        data.AppendLine("T|" & qb64(Now.ToString("o")))
        For Each category As String In categories.OrderBy(Function(value) value)
            data.AppendLine("C|" & qb64(category))
        Next category
        For Each raw As String In records
            data.AppendLine("R|" & qb64(raw))
        Next raw
        Return data.ToString
    End Function

    Function qf9parseidentitypayload(ByVal plaintext As String, ByRef failure As String) As IdentityTransferPayload
        failure = ""
        Try
            Dim lines As String() = plaintext.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(New Char() {ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
            If lines.Length < 4 OrElse lines.Length > 100000 OrElse lines(0) <> qf9IdentityPayloadPrefix & "|1" Then Throw New InvalidDataException("Unsupported identity payload version.")
            Dim payload As New IdentityTransferPayload
            Dim listedCategories As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For i As Integer = 1 To lines.Length - 1
                Dim fields As String() = lines(i).Split(New Char() {"|"c}, 2)
                If fields.Length <> 2 Then Throw New InvalidDataException("A payload line is malformed.")
                Dim decoded As String = ""
                If qf9trydecode64(fields(1), decoded) = False Then Throw New InvalidDataException("A payload value is not valid Base64.")
                If fields(0) = "N" Then
                    If payload.SourceName <> "" Then Throw New InvalidDataException("The payload contains more than one identity name.")
                    payload.SourceName = decoded
                ElseIf fields(0) = "T" Then
                    payload.Exported = decoded
                ElseIf fields(0) = "C" Then
                    If decoded <> "" Then listedCategories.Add(decoded)
                ElseIf fields(0) = "R" Then
                    payload.Records.Add(decoded)
                Else
                    Throw New InvalidDataException("The payload contains an unknown record type.")
                End If
            Next i
            If payload.SourceName = "" OrElse payload.Records.Count = 0 Then Throw New InvalidDataException("The payload does not contain an identity and records.")
            Dim actualCategories As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each raw As String In payload.Records
                Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
                If fields.Length <> 3 OrElse fields(0).Equals(payload.SourceName, StringComparison.OrdinalIgnoreCase) = False OrElse fields(1) = "" Then Throw New InvalidDataException("A serialized identity record is invalid.")
                Dim ignored As String = ""
                If qf9trydecode64(fields(2), ignored) = False Then Throw New InvalidDataException("A serialized identity value is invalid.")
                actualCategories.Add(fields(1))
            Next raw
            For Each category As String In listedCategories
                If actualCategories.Contains(category) = False Then Throw New InvalidDataException("The category manifest does not match the serialized records.")
            Next category
            payload.Categories.AddRange(actualCategories.OrderBy(Function(value) value))
            Return payload
        Catch ex As Exception
            failure = ex.Message
            Return Nothing
        End Try
    End Function

    Function qf9encryptidentitypackage(ByVal plaintext As String, ByVal password As String) As String
        Dim salt(31) As Byte
        Dim iv(15) As Byte
        Dim derived As Byte() = Nothing
        Dim plainBytes As Byte() = Nothing
        Dim cipherBytes As Byte() = Nothing
        Try
            Using random As RandomNumberGenerator = RandomNumberGenerator.Create()
                random.GetBytes(salt)
                random.GetBytes(iv)
            End Using
            Using derivation As New Rfc2898DeriveBytes(password, salt, qf9IdentityPackageIterations)
                derived = derivation.GetBytes(64)
            End Using
            plainBytes = New UTF8Encoding(False, True).GetBytes(plaintext)
            Using aes As Aes = Aes.Create()
                aes.KeySize = 256
                aes.BlockSize = 128
                aes.Mode = CipherMode.CBC
                aes.Padding = PaddingMode.PKCS7
                aes.Key = derived.Take(32).ToArray
                aes.IV = iv
                Using output As New MemoryStream
                    Using crypto As New CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write)
                        crypto.Write(plainBytes, 0, plainBytes.Length)
                        crypto.FlushFinalBlock()
                        cipherBytes = output.ToArray
                    End Using
                End Using
            End Using
            Dim signedPart As String = qf9IdentityPackagePrefix & "|1|" & qf9IdentityPackageIterations.ToString & "|" & Convert.ToBase64String(salt) & "|" & Convert.ToBase64String(iv) & "|" & Convert.ToBase64String(cipherBytes)
            Dim authentication As Byte()
            Using hmac As New HMACSHA256(derived.Skip(32).Take(32).ToArray)
                authentication = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPart))
            End Using
            Return signedPart & "|" & Convert.ToBase64String(authentication)
        Finally
            If derived IsNot Nothing Then Array.Clear(derived, 0, derived.Length)
            If plainBytes IsNot Nothing Then Array.Clear(plainBytes, 0, plainBytes.Length)
            If cipherBytes IsNot Nothing Then Array.Clear(cipherBytes, 0, cipherBytes.Length)
            Array.Clear(salt, 0, salt.Length)
            Array.Clear(iv, 0, iv.Length)
        End Try
    End Function

    Function qf9decryptidentitypackage(ByVal serialized As String, ByVal password As String, ByRef failure As String) As String
        failure = ""
        Dim derived As Byte() = Nothing
        Dim plainBytes As Byte() = Nothing
        Dim cipherBytes As Byte() = Nothing
        Try
            Dim fields As String() = serialized.Split("|"c)
            If fields.Length <> 7 OrElse fields(0) <> qf9IdentityPackagePrefix OrElse fields(1) <> "1" Then Throw New InvalidDataException("Unsupported identity package version.")
            Dim iterations As Integer = 0
            If Integer.TryParse(fields(2), iterations) = False OrElse iterations < 10000 OrElse iterations > 2000000 Then Throw New InvalidDataException("Invalid password derivation settings.")
            Dim salt As Byte() = Convert.FromBase64String(fields(3))
            Dim iv As Byte() = Convert.FromBase64String(fields(4))
            cipherBytes = Convert.FromBase64String(fields(5))
            Dim suppliedAuthentication As Byte() = Convert.FromBase64String(fields(6))
            If salt.Length < 16 OrElse iv.Length <> 16 OrElse cipherBytes.Length = 0 OrElse cipherBytes.Length Mod 16 <> 0 OrElse suppliedAuthentication.Length <> 32 Then Throw New InvalidDataException("Invalid encrypted package fields.")
            Using derivation As New Rfc2898DeriveBytes(password, salt, iterations)
                derived = derivation.GetBytes(64)
            End Using
            Dim signedPart As String = String.Join("|", fields.Take(6).ToArray)
            Dim expectedAuthentication As Byte()
            Using hmac As New HMACSHA256(derived.Skip(32).Take(32).ToArray)
                expectedAuthentication = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPart))
            End Using
            If qf9constanttimeequals(expectedAuthentication, suppliedAuthentication) = False Then Throw New CryptographicException("Authentication failed.")
            Using aes As Aes = Aes.Create()
                aes.KeySize = 256
                aes.BlockSize = 128
                aes.Mode = CipherMode.CBC
                aes.Padding = PaddingMode.PKCS7
                aes.Key = derived.Take(32).ToArray
                aes.IV = iv
                Using input As New MemoryStream(cipherBytes)
                    Using crypto As New CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read)
                        Using output As New MemoryStream
                            crypto.CopyTo(output)
                            plainBytes = output.ToArray
                        End Using
                    End Using
                End Using
            End Using
            Return New UTF8Encoding(False, True).GetString(plainBytes)
        Catch ex As Exception
            failure = "Password or HMAC-SHA256 integrity verification failed."
            Return Nothing
        Finally
            If derived IsNot Nothing Then Array.Clear(derived, 0, derived.Length)
            If plainBytes IsNot Nothing Then Array.Clear(plainBytes, 0, plainBytes.Length)
            If cipherBytes IsNot Nothing Then Array.Clear(cipherBytes, 0, cipherBytes.Length)
        End Try
    End Function

    Function qf9constanttimeequals(ByVal left As Byte(), ByVal right As Byte()) As Boolean
        If left Is Nothing OrElse right Is Nothing OrElse left.Length <> right.Length Then Return False
        Dim difference As Integer = 0
        For i As Integer = 0 To left.Length - 1
            difference = difference Or (CInt(left(i)) Xor CInt(right(i)))
        Next i
        Return difference = 0
    End Function

    Function qf9trydecode64(ByVal encoded As String, ByRef decoded As String) As Boolean
        Try
            decoded = New UTF8Encoding(False, True).GetString(Convert.FromBase64String(encoded))
            Return True
        Catch ex As Exception
            decoded = ""
            Return False
        End Try
    End Function

    Function qf9applyidentityimport(ByVal payload As IdentityTransferPayload, ByVal mode As String, ByVal targetName As String, ByRef summary As String, ByRef failure As String) As Boolean
        summary = ""
        failure = ""
        Dim originalRecords As String() = My.Settings.idp.Cast(Of String)().ToArray
        Dim synchronizedRecords As String() = Nothing
        Dim previousActive As String = If(My.Settings.activeIdentity = "", "Default", My.Settings.activeIdentity)
        Dim reloadStarted As Boolean = False
        Try
            Dim protectedImported As Integer = 0
            Dim importRecords As List(Of String) = qf9prepareprotectedimport(payload.Records, protectedImported, failure)
            If importRecords Is Nothing Then Throw New CryptographicException(If(failure = "", "A portable protected value could not be rewrapped for this Windows user.", failure))
            qsaveidentity(previousActive)
            synchronizedRecords = My.Settings.idp.Cast(Of String)().ToArray
            If qf9createimportbackup() = False Then Throw New IOException("A rollback backup could not be created, so the import was stopped.")
            Dim added As Integer = 0
            Dim duplicates As Integer = 0
            Dim conflicts As Integer = 0
            If mode = "NEW" Then
                Dim hasMarker As Boolean = False
                For Each raw As String In importRecords
                    Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
                    If fields(1).Equals("I", StringComparison.OrdinalIgnoreCase) Then hasMarker = True
                    My.Settings.idp.Add(targetName & "|" & fields(1) & "|" & fields(2))
                    added += 1
                Next raw
                If hasMarker = False Then
                    My.Settings.idp.Add(targetName & "|I|" & qb64("identity"))
                    added += 1
                End If
            Else
                Dim existingValues As New HashSet(Of String)(StringComparer.Ordinal)
                Dim existingScalars As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                Dim hasMarker As Boolean = False
                For Each raw As String In My.Settings.idp.Cast(Of String)().Where(Function(value) qf9recordidentity(value).Equals(targetName, StringComparison.OrdinalIgnoreCase))
                    Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
                    If fields.Length <> 3 Then Continue For
                    existingValues.Add(qf9logicalrecordkey(raw))
                    If fields(1).StartsWith("@") AndAlso existingScalars.ContainsKey(fields(1)) = False Then existingScalars.Add(fields(1), fields(2))
                    If fields(1).Equals("I", StringComparison.OrdinalIgnoreCase) Then hasMarker = True
                Next raw
                For Each raw As String In importRecords
                    Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
                    Dim key As String = qf9logicalrecordkey(raw)
                    If fields(1).Equals("I", StringComparison.OrdinalIgnoreCase) AndAlso hasMarker Then
                        duplicates += 1
                    ElseIf fields(1).StartsWith("@") AndAlso existingScalars.ContainsKey(fields(1)) Then
                        If existingScalars(fields(1)) = fields(2) Then duplicates += 1 Else conflicts += 1
                    ElseIf existingValues.Contains(key) Then
                        duplicates += 1
                    Else
                        My.Settings.idp.Add(targetName & "|" & fields(1) & "|" & fields(2))
                        existingValues.Add(key)
                        If fields(1).StartsWith("@") Then existingScalars(fields(1)) = fields(2)
                        If fields(1).Equals("I", StringComparison.OrdinalIgnoreCase) Then hasMarker = True
                        added += 1
                    End If
                Next raw
                If hasMarker = False Then
                    My.Settings.idp.Add(targetName & "|I|" & qb64("identity"))
                    added += 1
                End If
            End If
            My.Settings.Save()
            If targetName.Equals(previousActive, StringComparison.OrdinalIgnoreCase) Then
                reloadStarted = True
                qloadidentity(targetName)
            End If
            summary = "Imported " & added.ToString & " record(s) into '" & targetName & "'"
            If mode = "MERGE" Then summary &= "; skipped " & duplicates.ToString & " duplicate(s) and kept " & conflicts.ToString & " existing scalar conflict(s)"
            If protectedImported > 0 Then summary &= "; reprotected " & protectedImported.ToString & " vault/autofill record(s) for this Windows user"
            importRecords.Clear()
            Return True
        Catch ex As Exception
            failure = ex.Message
            Try
                My.Settings.idp.Clear()
                Dim rollbackRecords As String() = If(synchronizedRecords, originalRecords)
                For Each raw As String In rollbackRecords
                    My.Settings.idp.Add(raw)
                Next raw
                My.Settings.activeIdentity = previousActive
                My.Settings.Save()
                If reloadStarted Then qloadidentity(previousActive)
            Catch rollbackException As Exception
                qlog(rollbackException)
                failure &= vbCrLf & "Automatic in-memory rollback also failed; use the rotating backup created before import."
            End Try
            qlog(ex)
            Return False
        End Try
    End Function

    Function qf9createimportbackup() As Boolean
        Try
            Dim folder As String = qbackupfolder()
            Dim before As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            If Directory.Exists(folder) Then before.UnionWith(Directory.GetFiles(folder, "Firespider Backup *.txt"))
            qperformbackup()
            If Directory.Exists(folder) = False Then Return False
            For Each file As String In Directory.GetFiles(folder, "Firespider Backup *.txt")
                If before.Contains(file) = False Then Return True
            Next file
            Return False
        Catch ex As Exception
            qlog(ex)
            Return False
        End Try
    End Function

    Function qf9recordidentity(ByVal raw As String) As String
        Dim split As Integer = raw.IndexOf("|"c)
        If split <= 0 Then Return ""
        Return raw.Substring(0, split)
    End Function

    Function qf9recordcategory(ByVal raw As String) As String
        Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
        If fields.Length <> 3 Then Return ""
        Return fields(1)
    End Function

    Function qf9resolvedidentity(ByVal requested As String) As String
        For Each name As String In qidentitynames()
            If name.Equals(requested, StringComparison.OrdinalIgnoreCase) Then Return name
        Next name
        Return ""
    End Function

    Function qf9cleanidentityname(ByVal value As String) As String
        Return value.Replace("|", "-").Replace(vbCr, " ").Replace(vbLf, " ").Replace(ControlChars.Tab, " ").Trim
    End Function

    Function qf9uniqueidentityname(ByVal requested As String) As String
        Dim clean As String = qf9cleanidentityname(requested)
        If qf9resolvedidentity(clean) = "" Then Return clean
        Dim index As Integer = 2
        Do
            Dim candidate As String = clean & " (Imported " & index.ToString & ")"
            If qf9resolvedidentity(candidate) = "" Then Return candidate
            index += 1
        Loop
    End Function

    Function qf9safefilename(ByVal value As String) As String
        Dim result As String = value
        For Each character As Char In Path.GetInvalidFileNameChars()
            result = result.Replace(character, "-"c)
        Next character
        If result.Trim = "" Then Return "Identity"
        Return result.Trim
    End Function
End Class

Friend Class DownloadSafetyRecord
    Public Path As String = ""
    Public Source As String = ""
    Public BaselineHash As String = ""
    Public BaselineSize As Long = -1
    Public FirstSeen As String = ""
    Public LastHash As String = ""
    Public LastSize As Long = -1
    Public LastChecked As String = ""
    Public LastWriteUtcTicks As Long = 0
    Public LastError As String = ""
End Class

Friend Class IdentityTransferPayload
    Public SourceName As String = ""
    Public Exported As String = ""
    Public Categories As New List(Of String)
    Public Records As New List(Of String)
End Class

Friend Class IdentityTransferPasswordForm
    Inherits Form

    Dim passwordBox As New TextBox
    Dim confirmationBox As New TextBox
    Dim requireConfirmation As Boolean
    Public PasswordValue As String = ""

    Sub New(ByVal windowTitle As String, ByVal prompt As String, ByVal confirm As Boolean)
        requireConfirmation = confirm
        Text = windowTitle
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ClientSize = New Size(520, If(confirm, 205, 155))
        Dim message As New Label With {.Text = prompt, .Left = 12, .Top = 12, .Width = 495, .Height = 45}
        passwordBox.Left = 12
        passwordBox.Top = 62
        passwordBox.Width = 495
        passwordBox.UseSystemPasswordChar = True
        confirmationBox.Left = 12
        confirmationBox.Top = 110
        confirmationBox.Width = 495
        confirmationBox.UseSystemPasswordChar = True
        Dim confirmLabel As New Label With {.Text = "Confirm password:", .Left = 12, .Top = 92, .Width = 200, .Height = 18}
        Dim okButton As New Button With {.Text = "OK", .Left = 326, .Top = If(confirm, 158, 108), .Width = 85}
        Dim cancelButton As New Button With {.Text = "Cancel", .Left = 422, .Top = If(confirm, 158, 108), .Width = 85, .DialogResult = DialogResult.Cancel}
        Controls.Add(message)
        Controls.Add(passwordBox)
        If confirm Then Controls.Add(confirmLabel) : Controls.Add(confirmationBox)
        Controls.Add(okButton)
        Controls.Add(cancelButton)
        AcceptButton = okButton
        CancelButton = cancelButton
        AddHandler okButton.Click, AddressOf qaccept
    End Sub

    Sub qaccept(ByVal sender As Object, ByVal e As EventArgs)
        If passwordBox.Text = "" Then Exit Sub
        If requireConfirmation AndAlso passwordBox.Text.Length < 8 Then
            MessageBox.Show(Me, "Use a password of at least 8 characters.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        If requireConfirmation AndAlso passwordBox.Text <> confirmationBox.Text Then
            MessageBox.Show(Me, "The passwords do not match.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            confirmationBox.Clear()
            confirmationBox.Focus()
            Exit Sub
        End If
        PasswordValue = passwordBox.Text
        passwordBox.Clear()
        confirmationBox.Clear()
        DialogResult = DialogResult.OK
        Close()
    End Sub
End Class

Friend Class DownloadSafetyCenterForm
    Inherits Form

    Dim ownerForm As j
    Dim files As New ListView
    Dim details As New TextBox
    Dim recheckButton As New Button
    Dim trustButton As New Button
    Dim revealButton As New Button
    Dim quarantineButton As New Button
    Dim folderButton As New Button
    Dim removeButton As New Button
    Dim records As New List(Of DownloadSafetyRecord)

    Sub New(ByVal browser As j)
        ownerForm = browser
        Text = "Download Safety Center"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(1040, 610)
        MinimumSize = New Size(820, 480)
        files.Dock = DockStyle.Left
        files.Width = 570
        files.View = View.Details
        files.FullRowSelect = True
        files.HideSelection = False
        files.MultiSelect = False
        files.Columns.Add("File", 230)
        files.Columns.Add("Safety", 205)
        files.Columns.Add("Size", 95)
        files.Columns.Add("First seen", 140)
        details.Dock = DockStyle.Fill
        details.Multiline = True
        details.ReadOnly = True
        details.ScrollBars = ScrollBars.Vertical
        details.BackColor = SystemColors.Window
        Dim buttons As New FlowLayoutPanel
        buttons.Dock = DockStyle.Bottom
        buttons.Height = 48
        buttons.Padding = New Padding(5)
        recheckButton.Text = "Recheck"
        trustButton.Text = "Trust Hash"
        revealButton.Text = "Reveal Folder"
        quarantineButton.Text = "Quarantine"
        folderButton.Text = "Quarantine Folder"
        removeButton.Text = "Remove Record"
        folderButton.Width = 125
        buttons.Controls.AddRange(New Control() {recheckButton, trustButton, revealButton, quarantineButton, folderButton, removeButton})
        Controls.Add(details)
        Controls.Add(files)
        Controls.Add(buttons)
        AddHandler files.SelectedIndexChanged, AddressOf qf9showdetails
        AddHandler recheckButton.Click, AddressOf qf9recheck
        AddHandler trustButton.Click, AddressOf qf9trust
        AddHandler revealButton.Click, AddressOf qf9reveal
        AddHandler quarantineButton.Click, AddressOf qf9quarantine
        AddHandler folderButton.Click, AddressOf qf9choosefolder
        AddHandler removeButton.Click, AddressOf qf9remove
        qf9reload("")
    End Sub

    Function qf9selected() As DownloadSafetyRecord
        If files.SelectedItems.Count = 0 Then Return Nothing
        Return TryCast(files.SelectedItems(0).Tag, DownloadSafetyRecord)
    End Function

    Sub qf9reload(ByVal selectPath As String)
        Try
            Cursor = Cursors.WaitCursor
            records = ownerForm.qf9syncsafety()
            files.BeginUpdate()
            files.Items.Clear()
            Dim selectedItem As ListViewItem = Nothing
            For Each record As DownloadSafetyRecord In records.OrderBy(Function(entry) System.IO.Path.GetFileName(entry.Path))
                Dim label As String = System.IO.Path.GetFileName(record.Path)
                If label = "" Then label = record.Path
                Dim item As New ListViewItem(label)
                item.SubItems.Add(ownerForm.qf9downloadstatus(record, records))
                item.SubItems.Add(ownerForm.qf9formatbytes(record.LastSize))
                item.SubItems.Add(record.FirstSeen)
                item.Tag = record
                files.Items.Add(item)
                If record.Path.Equals(selectPath, StringComparison.OrdinalIgnoreCase) Then selectedItem = item
            Next record
            files.EndUpdate()
            If selectedItem IsNot Nothing Then
                selectedItem.Selected = True
                selectedItem.EnsureVisible()
            ElseIf files.Items.Count > 0 Then
                files.Items(0).Selected = True
            Else
                details.Text = "No tracked downloads or saved files are available."
            End If
        Catch ex As Exception
            ownerForm.qlog(ex)
            MessageBox.Show(Me, "Safety records could not be refreshed.", "Download Safety Center", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    Sub qf9showdetails(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadSafetyRecord = qf9selected()
        If record Is Nothing Then Exit Sub
        Dim warningLines As List(Of String) = ownerForm.qf9downloadwarnings(record)
        Dim duplicatePaths As String() = ownerForm.qf9duplicatepaths(record, records)
        Dim changed As Boolean = record.BaselineHash <> "" AndAlso record.LastHash <> "" AndAlso record.BaselineHash.Equals(record.LastHash, StringComparison.OrdinalIgnoreCase) = False
        Dim text As New StringBuilder
        text.AppendLine("Status: " & ownerForm.qf9downloadstatus(record, records))
        text.AppendLine("File: " & record.Path)
        text.AppendLine("Exists: " & If(File.Exists(record.Path), "Yes", "No"))
        text.AppendLine("Source: " & If(record.Source = "", "Unavailable", record.Source))
        text.AppendLine("Size: " & ownerForm.qf9formatbytes(record.LastSize))
        text.AppendLine("First seen: " & record.FirstSeen)
        text.AppendLine("Last checked: " & record.LastChecked)
        text.AppendLine("Original SHA-256: " & If(record.BaselineHash = "", "Unavailable", record.BaselineHash))
        text.AppendLine("Current SHA-256: " & If(record.LastHash = "", "Unavailable", record.LastHash))
        text.AppendLine("Changed since first hash: " & If(changed, "YES", "No"))
        text.AppendLine("Trusted current hash: " & If(ownerForm.qf9istrusted(record.LastHash), "Yes", "No"))
        If record.LastError <> "" Then text.AppendLine("Check error: " & record.LastError)
        text.AppendLine()
        If warningLines.Count = 0 Then
            text.AppendLine("Filename/type warnings: None")
        Else
            text.AppendLine("Filename/type warnings:")
            For Each warning As String In warningLines
                text.AppendLine("• " & warning)
            Next warning
        End If
        If duplicatePaths.Length > 0 Then
            text.AppendLine()
            text.AppendLine("Other tracked paths with the same SHA-256:")
            For Each path As String In duplicatePaths
                text.AppendLine("• " & path)
            Next path
        End If
        text.AppendLine()
        text.AppendLine("Quarantine folder: " & ownerForm.qf9quarantinefolder())
        text.AppendLine("This center never executes downloaded files.")
        details.Text = text.ToString
    End Sub

    Sub qf9recheck(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadSafetyRecord = qf9selected()
        If record Is Nothing Then Exit Sub
        Try
            Cursor = Cursors.WaitCursor
            ownerForm.qf9rechecksafety(record)
            qf9reload(record.Path)
        Catch ex As Exception
            ownerForm.qlog(ex)
            MessageBox.Show(Me, "The file could not be rechecked.", "Download Safety Center", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    Sub qf9trust(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadSafetyRecord = qf9selected()
        If record Is Nothing Then Exit Sub
        Try
            ownerForm.qf9rechecksafety(record)
            If record.LastHash = "" Then Throw New InvalidDataException("A SHA-256 hash is not available for this file.")
            If MessageBox.Show(Me, "Trust this exact SHA-256 hash?" & vbCrLf & vbCrLf & record.LastHash & vbCrLf & vbCrLf & "Trust does not execute the file and does not hide changed-file history.", "Trust Download Hash", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
            ownerForm.qf9trusthash(record.LastHash)
            qf9reload(record.Path)
        Catch ex As Exception
            ownerForm.qlog(ex)
            MessageBox.Show(Me, ex.Message, "Trust Download Hash", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Sub qf9reveal(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadSafetyRecord = qf9selected()
        If record Is Nothing Then Exit Sub
        Try
            If File.Exists(record.Path) Then
                Process.Start("explorer.exe", "/select,""" & record.Path & """")
            Else
                Dim folder As String = Path.GetDirectoryName(record.Path)
                If Directory.Exists(folder) Then Process.Start(folder) Else MessageBox.Show(Me, "The containing folder no longer exists.", "Reveal Download", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            ownerForm.qlog(ex)
            MessageBox.Show(Me, "The containing folder could not be revealed.", "Reveal Download", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Sub qf9choosefolder(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim dialog As New FolderBrowserDialog
            dialog.Description = "Choose the folder used to quarantine downloaded files"
            If Directory.Exists(ownerForm.qf9quarantinefolder()) Then dialog.SelectedPath = ownerForm.qf9quarantinefolder()
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                ownerForm.qf9setquarantinefolder(dialog.SelectedPath)
                qf9showdetails(Nothing, EventArgs.Empty)
            End If
        Catch ex As Exception
            ownerForm.qlog(ex)
            MessageBox.Show(Me, "The quarantine folder could not be updated.", "Download Safety Center", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Sub qf9quarantine(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadSafetyRecord = qf9selected()
        If record Is Nothing Then Exit Sub
        If File.Exists(record.Path) = False Then
            MessageBox.Show(Me, "The selected file no longer exists.", "Quarantine Download", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If
        Dim message As String = "Move this file to the configured quarantine folder?" & vbCrLf & vbCrLf & record.Path & vbCrLf & "→ " & ownerForm.qf9quarantinefolder() & vbCrLf & vbCrLf & "The file will not be opened or executed."
        If MessageBox.Show(Me, message, "Confirm Quarantine Move", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
        Dim destination As String = ""
        Dim detail As String = ""
        If ownerForm.qf9quarantinedownload(record, destination, detail) Then
            MessageBox.Show(Me, detail & vbCrLf & vbCrLf & destination, "Download Quarantined", MessageBoxButtons.OK, MessageBoxIcon.Information)
            qf9reload(destination)
        Else
            MessageBox.Show(Me, detail, "Quarantine Download", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Sub qf9remove(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadSafetyRecord = qf9selected()
        If record Is Nothing Then Exit Sub
        If MessageBox.Show(Me, "Remove this download tracking and safety record?" & vbCrLf & vbCrLf & record.Path & vbCrLf & vbCrLf & "The file itself will not be deleted or opened.", "Remove Download Record", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
        ownerForm.qf9removesafetyrecord(record)
        qf9reload("")
    End Sub
End Class
