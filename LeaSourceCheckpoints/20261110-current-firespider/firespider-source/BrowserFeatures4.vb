Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports System.Text

Partial Class j
    Dim automationTimer As New Timer
    Dim analyticsTimer As New Timer
    Shared automationStartupRun As Boolean = False

    Sub qfeatures4()
        fl.DropDownItems.Add(qnewitem("Restore Or Migrate Backup", AddressOf q892, AddressOf q893))
        v.DropDownItems.Add(qnewitem("Visual Tab Overview", AddressOf q894, AddressOf q895))
        bo.DropDownItems.Add(qnewitem("Bookmark Intelligence Manager", AddressOf q896, AddressOf q897))
        bo.DropDownItems.Add(qnewitem("Add Selection Annotation", AddressOf q898, AddressOf q899))
        bo.DropDownItems.Add(qnewitem("Annotations And Highlights Library", AddressOf q911, AddressOf q912))
        Dim smart As ToolStripMenuItem = qnewitem("Smart Bookmark Folders", Nothing, Nothing)
        AddHandler smart.DropDownOpening, AddressOf q908
        bo.DropDownItems.Add(smart)
        ot.DropDownItems.Add(qnewitem("Browser Identity Manager", AddressOf q900, AddressOf q901))
        he.DropDownItems.Add(qnewitem("Network Diagnostics Center", AddressOf q902, AddressOf q903))
        ot.DropDownItems.Add(qnewitem("Browser Automation Rules", AddressOf q904, AddressOf q905))
        hi.DropDownItems.Add(qnewitem("Local Browser Analytics", AddressOf q906, AddressOf q907))
        qfeatures5()
    End Sub

    Sub qfeature4loaded()
        Try
            automationTimer.Interval = 60000
            analyticsTimer.Interval = 60000
            AddHandler automationTimer.Tick, AddressOf qautomationtick
            AddHandler analyticsTimer.Tick, AddressOf qanalyticstick
            automationTimer.Start()
            If My.Settings.analyticsEnabled = "e" AndAlso pv = False Then analyticsTimer.Start()
            If My.Settings.portableMode = "e" AndAlso My.Settings.idp.Count = 0 Then
                qloadportable()
                If My.Settings.idp.Count > 0 Then qloadidentity(My.Settings.activeIdentity)
            End If
            If automationStartupRun = False Then
                automationStartupRun = True
                qrunautomations(True)
            End If
            qfeature6loaded()
            qfeature8loaded()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qfeature4closing(ByVal finalWindow As Boolean)
        automationTimer.Stop()
        analyticsTimer.Stop()
        qfeature6closing(finalWindow)
        qfeature8closing(finalWindow)
        If finalWindow Then
            qsaveidentity(My.Settings.activeIdentity)
            If My.Settings.portableMode = "e" Then qwriteportable()
        End If
    End Sub

    ' --- Backup Restore And Migration Wizard ---
    Sub q892(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wizard As New BackupRestoreForm(Me)
            wizard.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q893(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q892(sender, e)
    End Sub

    Friend Function qparsebackup(ByVal path As String) As Dictionary(Of String, BackupSettingData)
        Dim result As New Dictionary(Of String, BackupSettingData)(StringComparer.OrdinalIgnoreCase)
        If System.IO.File.Exists(path) = False Then Return result
        Dim lines As String() = System.IO.File.ReadAllLines(path, Encoding.UTF8)
        If lines.Length = 0 OrElse lines(0).Trim <> "FIRESPIDER_SETTINGS_BACKUP_V1" Then Return result
        For Each line As String In lines.Skip(2)
            Dim fields As String() = line.Split(New Char() {"|"c}, 3)
            If fields.Length <> 3 OrElse New String() {"S", "C", "E"}.Contains(fields(0)) = False Then Continue For
            Dim name As String = qunb64(fields(1))
            Dim value As String = qunb64(fields(2))
            If My.Settings.Properties(name) Is Nothing Then Continue For
            If result.ContainsKey(name) = False Then result.Add(name, New BackupSettingData(name, fields(0) = "C" OrElse fields(0) = "E"))
            If fields(0) <> "E" Then result(name).Values.Add(value)
        Next line
        Return result
    End Function

    Friend Function qrestorebackup(ByVal path As String, ByVal names As List(Of String)) As String
        Dim parsed As Dictionary(Of String, BackupSettingData) = qparsebackup(path)
        If parsed.Count = 0 Then Return "The selected file is not a valid Firespider backup."
        qperformbackup()
        Dim restored As Integer = 0
        For Each name As String In names
            If parsed.ContainsKey(name) = False OrElse My.Settings.Properties(name) Is Nothing Then Continue For
            Try
                Dim current As Object = My.Settings(name)
                If TypeOf current Is System.Collections.Specialized.StringCollection Then
                    Dim collection As System.Collections.Specialized.StringCollection = CType(current, System.Collections.Specialized.StringCollection)
                    collection.Clear()
                    For Each value As String In parsed(name).Values
                        collection.Add(value)
                    Next value
                ElseIf name = "i" AndAlso parsed(name).Values.Count > 0 Then
                    My.Settings(name) = New ColorConverter().ConvertFromString(parsed(name).Values(0))
                ElseIf parsed(name).Values.Count > 0 Then
                    My.Settings(name) = parsed(name).Values(0)
                End If
                restored += 1
            Catch ex As Exception
                qlog(ex)
            End Try
        Next name
        My.Settings.Save()
        qrb()
        qacm()
        qverticaltabs()
        Return restored.ToString & " setting group(s) restored. A rollback backup was created first. Restart Firespider to apply startup-level changes."
    End Function

    ' --- Visual Tab Overview ---
    Sub q894(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim overview As New TabOverviewForm(Me, qoverviewitems())
            overview.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q895(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q894(sender, e)
    End Sub

    Friend Function qoverviewitems() As List(Of TabOverviewRecord)
        Dim records As New List(Of TabOverviewRecord)
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each tab As TabPage In qalltabs()
            If tab.Controls.Count = 0 Then Continue For
            Dim wb As WebBrowser = TryCast(tab.Controls(0), WebBrowser)
            Dim url As String = qpersistedtaburl(tab, wb)
            Dim duplicate As Boolean = url <> "" AndAlso seen.Add(url) = False
            Dim estimate As Long = 0
            Try
                estimate = CLng(wb.DocumentText.Length) * 2L
                If wb.Document IsNot Nothing Then estimate += CLng(wb.Document.GetElementsByTagName("img").Count) * 32768L
            Catch
            End Try
            records.Add(New TabOverviewRecord(tab, If(tab.Text = "", url, tab.Text), url, duplicate, estimate, qtabthumbnail(wb, tab.Text)))
        Next tab
        Return records
    End Function

    Function qtabthumbnail(ByVal wb As WebBrowser, ByVal label As String) As Bitmap
        Dim image As New Bitmap(300, 170)
        Try
            wb.DrawToBitmap(image, New Rectangle(0, 0, image.Width, image.Height))
        Catch
            Using graphics As Graphics = Graphics.FromImage(image)
                graphics.Clear(Color.WhiteSmoke)
                graphics.DrawString(label, New Font("Segoe UI", 12.0!), Brushes.DimGray, New RectangleF(15, 20, 270, 130))
            End Using
        End Try
        Return image
    End Function

    Friend Sub qoverviewclose(ByVal records As List(Of TabOverviewRecord))
        If k.TabPages.Cast(Of TabPage)().All(Function(tab) records.Any(Function(record) record.Tab Is tab)) Then qleaNew(qleaStart())
        For Each record As TabOverviewRecord In records
            qleaClose(record.Tab)
        Next record
        I = k.TabPages.Count
        My.Settings.Save()
    End Sub
    Friend Sub qoverviewreload(ByVal records As List(Of TabOverviewRecord))
        For Each record As TabOverviewRecord In records
            If record.Tab.IsDisposed Then Continue For
            If suspendedGroupUrls.ContainsKey(record.Tab) Then
                qfeature6resumetab(record.Tab)
                Continue For
            End If
            Dim host As Global.Lea.Win.V = qleaHost(record.Tab)
            If host IsNot Nothing Then
                qleaReload(host)
                Continue For
            End If
            If record.Tab.Controls.Count > 0 Then
                Dim wb As WebBrowser = TryCast(record.Tab.Controls(0), WebBrowser)
                If wb IsNot Nothing Then
                    If suspendedGroupUrls.ContainsKey(record.Tab) Then
                        Dim url As String = suspendedGroupUrls(record.Tab)
                        suspendedGroupUrls.Remove(record.Tab)
                        wb.Navigate(url)
                    Else
                        wb.Refresh()
                    End If
                End If
            End If
        Next record
    End Sub
    Friend Sub qoverviewsuspend(ByVal records As List(Of TabOverviewRecord))
        For Each record As TabOverviewRecord In records
            If Not record.Tab.IsDisposed Then qsuspendtab(record.Tab)
        Next record
    End Sub
    Friend Sub qoverviewarchive(ByVal records As List(Of TabOverviewRecord))
        For Each record As TabOverviewRecord In records
            If record.Tab.Controls.Count > 0 AndAlso suspendedGroupUrls.ContainsKey(record.Tab) = False Then qarchivebrowser(TryCast(record.Tab.Controls(0), WebBrowser), record.Url)
        Next record
    End Sub
    Friend Sub qoverviewmergeduplicates()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim remove As New List(Of TabOverviewRecord)
        For Each record As TabOverviewRecord In qoverviewitems()
            If record.Url <> "" AndAlso seen.Add(record.Url) = False Then remove.Add(record)
        Next record
        qoverviewclose(remove)
    End Sub

    ' --- Bookmark Tags, Smart Folders, Checks, And Archive Associations ---
    Sub q896(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim action As String = InputBox("Enter TAG, UNTAG, MERGE, CHECK, or ASSOCIATE.", "Bookmark Intelligence Manager", "TAG").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            Dim currentUrl As String = If(wb.Url Is Nothing, "", wb.Url.ToString)
            If action = "MERGE" Then
                Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                Dim removed As Integer = 0
                For i As Integer = My.Settings.bm.Count - 1 To 0 Step -1
                    Dim url As String = qbu(My.Settings.bm(i))
                    If seen.Add(url) = False Then My.Settings.bm.RemoveAt(i) : removed += 1
                Next i
                My.Settings.Save()
                qrb()
                su.Text = removed.ToString & " duplicate bookmark(s) merged"
            ElseIf action = "CHECK" Then
                If MessageBox.Show("Check up to 50 HTTP bookmarks now? This sends a short HEAD request to each site.", "Broken Bookmark Check", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
                Dim broken As New List(Of String)
                For Each raw As String In My.Settings.bm.Cast(Of String)().Take(50)
                    Dim url As String = qbu(raw)
                    If qbookmarkreachable(url) = False Then broken.Add(url)
                Next raw
                If broken.Count > 0 Then Clipboard.SetText(String.Join(vbCrLf, broken.ToArray))
                MessageBox.Show(broken.Count.ToString & " potentially broken bookmark(s)." & If(broken.Count > 0, " Their URLs were copied to the clipboard.", ""), "Bookmark Check", MessageBoxButtons.OK, If(broken.Count = 0, MessageBoxIcon.Information, MessageBoxIcon.Warning))
            Else
                Dim url As String = InputBox("Enter the bookmark URL:", "Bookmark Intelligence Manager", currentUrl).Trim
                If url = "" Then Exit Sub
                If My.Settings.bm.Cast(Of String)().Any(Function(raw) qbu(raw).Equals(url, StringComparison.OrdinalIgnoreCase)) = False Then My.Settings.bm.Add("General|" & url)
                Dim meta As BookmarkMetaRecord = qbookmarkmeta(url)
                If action = "TAG" Then
                    meta.Tags = InputBox("Enter comma-separated tags:", "Bookmark Tags", meta.Tags).Trim.ToLowerInvariant
                ElseIf action = "UNTAG" Then
                    meta.Tags = ""
                ElseIf action = "ASSOCIATE" Then
                    Dim archive As ArchiveRecord = qarchiverecords().Where(Function(item) item.Url.Equals(url, StringComparison.OrdinalIgnoreCase)).OrderByDescending(Function(item) item.DateSaved).FirstOrDefault
                    If archive Is Nothing Then
                        MessageBox.Show("No local archive matches this bookmark URL.", "Archive Association", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Else
                        meta.ArchivePath = archive.HtmlPath
                    End If
                End If
                qsavebookmarkmeta(meta)
                My.Settings.Save()
                qrb()
                su.Text = "Bookmark metadata updated"
            End If
            su.Visible = True
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q897(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q896(sender, e)
    End Sub
    Function qbookmarkreachable(ByVal url As String) As Boolean
        Try
            Dim parsed As Uri = Nothing
            If Uri.TryCreate(url, UriKind.Absolute, parsed) = False OrElse (parsed.Scheme <> Uri.UriSchemeHttp AndAlso parsed.Scheme <> Uri.UriSchemeHttps) Then Return True
            Dim request As HttpWebRequest = CType(WebRequest.Create(parsed), HttpWebRequest)
            request.Method = "HEAD"
            request.Timeout = 3500
            request.AllowAutoRedirect = True
            request.UserAgent = "Firespider Internet Bookmark Check"
            Using response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
                Return CInt(response.StatusCode) < 400
            End Using
        Catch ex As WebException
            Dim response As HttpWebResponse = TryCast(ex.Response, HttpWebResponse)
            If response IsNot Nothing Then Return CInt(response.StatusCode) < 500
            Return False
        Catch
            Return False
        End Try
    End Function
    Function qbookmarkmeta(ByVal url As String) As BookmarkMetaRecord
        For Each raw As String In My.Settings.bmt
            Dim fields As String() = raw.Split("|"c)
            If fields.Length = 3 AndAlso qunb64(fields(0)).Equals(url, StringComparison.OrdinalIgnoreCase) Then Return New BookmarkMetaRecord(url, qunb64(fields(1)), qunb64(fields(2)))
        Next raw
        Return New BookmarkMetaRecord(url, "", "")
    End Function
    Sub qsavebookmarkmeta(ByVal meta As BookmarkMetaRecord)
        For i As Integer = My.Settings.bmt.Count - 1 To 0 Step -1
            Dim fields As String() = My.Settings.bmt(i).Split("|"c)
            If fields.Length = 3 AndAlso qunb64(fields(0)).Equals(meta.Url, StringComparison.OrdinalIgnoreCase) Then My.Settings.bmt.RemoveAt(i)
        Next i
        If meta.Tags <> "" OrElse meta.ArchivePath <> "" Then My.Settings.bmt.Add(qb64(meta.Url) & "|" & qb64(meta.Tags) & "|" & qb64(meta.ArchivePath))
    End Sub
    Sub q908(ByVal sender As Object, ByVal e As EventArgs)
        Dim menu As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        menu.DropDownItems.Clear()
        Dim tags As New SortedDictionary(Of String, ToolStripMenuItem)(StringComparer.OrdinalIgnoreCase)
        For Each raw As String In My.Settings.bm
            Dim url As String = qbu(raw)
            Dim meta As BookmarkMetaRecord = qbookmarkmeta(url)
            For Each tag As String In meta.Tags.Split(","c).Select(Function(value) value.Trim).Where(Function(value) value <> "")
                If tags.ContainsKey(tag) = False Then
                    Dim parent As ToolStripMenuItem = qnewitem(tag, Nothing, Nothing)
                    tags.Add(tag, parent)
                    menu.DropDownItems.Add(parent)
                End If
                Dim item As ToolStripMenuItem = qnewitem(If(meta.ArchivePath = "", url, "◆ " & url), AddressOf q909, AddressOf q910)
                item.Tag = If(meta.ArchivePath <> "" AndAlso System.IO.File.Exists(meta.ArchivePath), New Uri(meta.ArchivePath).AbsoluteUri, url)
                tags(tag).DropDownItems.Add(item)
            Next tag
        Next raw
        If menu.DropDownItems.Count = 0 Then
            Dim empty As New ToolStripMenuItem("No tagged bookmarks")
            empty.Enabled = False
            menu.DropDownItems.Add(empty)
        End If
    End Sub
    Sub q909(ByVal sender As Object, ByVal e As EventArgs)
        q1(CType(sender, ToolStripMenuItem).Tag.ToString)
    End Sub
    Sub q910(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q909(sender, e)
    End Sub

    ' --- Persistent Annotations And Highlights ---
    Sub q898(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            If wb.Url Is Nothing OrElse wb.Document Is Nothing Then Exit Sub
            Dim selected As String = ""
            Try
                Dim value As Object = wb.Document.InvokeScript("eval", New Object() {"(window.getSelection?window.getSelection().toString():document.selection.createRange().text)"})
                If value IsNot Nothing Then selected = value.ToString.Trim
            Catch
            End Try
            If selected = "" Then
                Dim library As New AnnotationLibraryForm(Me)
                library.Show(Me)
                Exit Sub
            End If
            If selected.Length > 1000 Then selected = selected.Substring(0, 1000)
            Dim note As String = InputBox("Add an optional note for this highlight:", "Page Annotation", "")
            Dim color As String = InputBox("Enter a highlight color name or HTML color:", "Page Annotation", "Yellow").Trim
            Dim archivePath As String = ""
            Dim archive As ArchiveRecord = qarchiverecords().Where(Function(item) item.Url.Equals(wb.Url.ToString, StringComparison.OrdinalIgnoreCase)).OrderByDescending(Function(item) item.DateSaved).FirstOrDefault
            If archive IsNot Nothing Then archivePath = archive.HtmlPath
            My.Settings.an.Add(qb64(wb.Url.ToString) & "|" & Now.ToString("o") & "|" & qb64(selected) & "|" & qb64(note) & "|" & qb64(color) & "|" & qb64(archivePath))
            If My.Settings.rl.Cast(Of String)().Any(Function(raw) raw.Split(New Char() {"|"c}, 4).Last.Equals(wb.Url.ToString, StringComparison.OrdinalIgnoreCase)) = False AndAlso MessageBox.Show("Also add this annotated page to the reading list?", "Page Annotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then q825(sender, e)
            My.Settings.Save()
            qapplyannotations(wb)
            su.Visible = True
            su.Text = "Page annotation saved"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q899(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q898(sender, e)
    End Sub
    Sub q911(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim library As New AnnotationLibraryForm(Me)
            library.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q912(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q911(sender, e)
    End Sub
    Friend Function qannotationrecords() As List(Of AnnotationRecord)
        Dim records As New List(Of AnnotationRecord)
        For i As Integer = 0 To My.Settings.an.Count - 1
            Dim fields As String() = My.Settings.an(i).Split("|"c)
            If fields.Length = 6 Then records.Add(New AnnotationRecord(i, qunb64(fields(0)), fields(1), qunb64(fields(2)), qunb64(fields(3)), qunb64(fields(4)), qunb64(fields(5))))
        Next i
        Return records
    End Function
    Friend Sub qapplyannotations(ByVal wb As WebBrowser)
        Try
            If wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing OrElse wb.Document.Body Is Nothing Then Exit Sub
            For Each record As AnnotationRecord In qannotationrecords().Where(Function(item) item.Url.Equals(wb.Url.ToString, StringComparison.OrdinalIgnoreCase))
                Try
                    Dim range As Object = wb.Document.Body.DomElement.createTextRange()
                    If CBool(range.findText(record.Text)) Then range.execCommand("BackColor", False, ColorTranslator.ToHtml(qgroupcolor(record.Color)))
                Catch
                End Try
            Next record
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Friend Sub qremoveannotation(ByVal record As AnnotationRecord)
        If record.Index >= 0 AndAlso record.Index < My.Settings.an.Count Then My.Settings.an.RemoveAt(record.Index)
        My.Settings.Save()
    End Sub

    ' --- Browser Identities And Portable Profiles ---
    Sub q900(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim names As List(Of String) = qidentitynames()
            Dim action As String = InputBox("Active: " & My.Settings.activeIdentity & vbCrLf & "Identities: " & String.Join(", ", names.ToArray) & vbCrLf & vbCrLf & "Enter CREATE, SWITCH, RENAME, DELETE, or PORTABLE:", "Browser Identity Manager", "CREATE").Trim.ToUpperInvariant
            If action = "PORTABLE" Then
                My.Settings.portableMode = If(My.Settings.portableMode = "e", "", "e")
                If My.Settings.portableMode = "e" Then qwriteportable()
                My.Settings.Save()
                MessageBox.Show("Portable profile export is " & If(My.Settings.portableMode = "e", "enabled", "disabled") & ". Encrypted vault and autofill blobs remain protected to this Windows user.", "Portable Profile", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If
            If action = "CREATE" Then
                Dim name As String = InputBox("Enter the new identity name:", "Create Browser Identity", "Identity " & (names.Count + 1).ToString).Trim.Replace("|", "-")
                If name = "" Then Exit Sub
                Dim mode As String = InputBox("Enter CLONE to copy current data or EMPTY for a blank identity:", "Create Browser Identity", "EMPTY").Trim.ToUpperInvariant
                qsaveidentity(My.Settings.activeIdentity)
                qdeleteidentity(name)
                My.Settings.idp.Add(name & "|I|" & qb64("identity"))
                If mode = "CLONE" Then qsaveidentity(name)
                qloadidentity(name)
            Else
                Dim name As String = InputBox("Enter the identity name:", "Browser Identity Manager", If(names.Count = 0, "Default", names(0))).Trim
                If name = "" Then Exit Sub
                If action = "SWITCH" Then
                    qsaveidentity(My.Settings.activeIdentity)
                    qloadidentity(name)
                ElseIf action = "RENAME" Then
                    Dim replacement As String = InputBox("Enter the new identity name:", "Rename Browser Identity", name).Trim.Replace("|", "-")
                    If replacement = "" Then Exit Sub
                    For i As Integer = 0 To My.Settings.idp.Count - 1
                        Dim split As Integer = My.Settings.idp(i).IndexOf("|"c)
                        If split > 0 AndAlso My.Settings.idp(i).Substring(0, split).Equals(name, StringComparison.OrdinalIgnoreCase) Then My.Settings.idp(i) = replacement & My.Settings.idp(i).Substring(split)
                    Next i
                    If My.Settings.activeIdentity.Equals(name, StringComparison.OrdinalIgnoreCase) Then My.Settings.activeIdentity = replacement
                ElseIf action = "DELETE" AndAlso name <> "Default" AndAlso name <> My.Settings.activeIdentity Then
                    qdeleteidentity(name)
                End If
            End If
            My.Settings.Save()
            su.Visible = True
            su.Text = "Browser identities updated"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q901(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q900(sender, e)
    End Sub
    Function qidentitycollections() As String()
        Return New String() {"s", "bm", "rc", "ns", "cwh", "rl", "dp", "bs", "perm", "wp", "dl", "dq", "vault", "archives", "af", "us", "sc", "cf", "cfx", "tg", "fr", "bmt", "an", "analytics", "au", "hibernateExceptions", "research", "archiveVersions", "archiveRetention", "watchSources", "watchItems", "aliases", "downloadSafety", "trustedHashes"}
    End Function
    Function qidentityscalars() As String()
        Return New String() {"h", "se", "nf", "th", "ps", "rs", "cfEnabled", "frEnabled", "dashboard", "hibernateEnabled", "hibernateMinutes", "lazyRestoreMode", "watchEnabled", "quarantineFolder"}
    End Function
    Function qidentitynames() As List(Of String)
        Dim names As New List(Of String) From {"Default"}
        For Each raw As String In My.Settings.idp
            Dim split As Integer = raw.IndexOf("|"c)
            If split > 0 AndAlso names.Contains(raw.Substring(0, split)) = False Then names.Add(raw.Substring(0, split))
        Next raw
        Return names
    End Function
    Sub qdeleteidentity(ByVal name As String)
        For i As Integer = My.Settings.idp.Count - 1 To 0 Step -1
            If My.Settings.idp(i).StartsWith(name & "|", StringComparison.OrdinalIgnoreCase) Then My.Settings.idp.RemoveAt(i)
        Next i
    End Sub
    Sub qsaveidentity(ByVal name As String)
        If name = "" Then name = "Default"
        qdeleteidentity(name)
        My.Settings.idp.Add(name & "|I|" & qb64("identity"))
        For Each settingName As String In qidentitycollections()
            Dim values As System.Collections.Specialized.StringCollection = TryCast(My.Settings(settingName), System.Collections.Specialized.StringCollection)
            If values Is Nothing Then Continue For
            For Each value As String In values
                My.Settings.idp.Add(name & "|" & settingName & "|" & qb64(value))
            Next value
        Next settingName
        For Each settingName As String In qidentityscalars()
            My.Settings.idp.Add(name & "|@" & settingName & "|" & qb64(If(My.Settings(settingName) Is Nothing, "", My.Settings(settingName).ToString)))
        Next settingName
        For Each tab As TabPage In qalltabs()
            If tab.Controls.Count > 0 Then
                Dim url As String = qpersistedtaburl(tab, TryCast(tab.Controls(0), WebBrowser))
                If url <> "" Then My.Settings.idp.Add(name & "|tabs|" & qb64(url))
            End If
        Next tab
        My.Settings.Save()
    End Sub
    Sub qloadidentity(ByVal name As String)
        Dim records As String() = My.Settings.idp.Cast(Of String)().Where(Function(raw) raw.StartsWith(name & "|", StringComparison.OrdinalIgnoreCase)).ToArray
        If records.Length = 0 AndAlso name <> "Default" Then Exit Sub
        For Each settingName As String In qidentitycollections()
            Dim values As System.Collections.Specialized.StringCollection = TryCast(My.Settings(settingName), System.Collections.Specialized.StringCollection)
            If values IsNot Nothing Then values.Clear()
        Next settingName
        Dim scalarDefaults As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {{"h", "http://fsin.glitch.me/"}, {"se", "https://www.bing.com/search?q="}, {"nf", ""}, {"th", ""}, {"ps", ""}, {"rs", "e"}, {"cfEnabled", "e"}, {"frEnabled", "e"}, {"dashboard", ""}, {"hibernateEnabled", ""}, {"hibernateMinutes", "30"}, {"lazyRestoreMode", "off"}, {"watchEnabled", ""}, {"quarantineFolder", ""}}
        For Each settingName As String In qidentityscalars()
            My.Settings(settingName) = scalarDefaults(settingName)
        Next settingName
        Dim urls As New List(Of String)
        For Each raw As String In records
            Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
            If fields.Length <> 3 Then Continue For
            Dim value As String = qunb64(fields(2))
            If fields(1) = "tabs" Then
                If qvurl(value) Then urls.Add(value)
            ElseIf fields(1).StartsWith("@") AndAlso qidentityscalars().Contains(fields(1).Substring(1)) Then
                My.Settings(fields(1).Substring(1)) = value
            ElseIf qidentitycollections().Contains(fields(1)) Then
                Dim values As System.Collections.Specialized.StringCollection = TryCast(My.Settings(fields(1)), System.Collections.Specialized.StringCollection)
                If values IsNot Nothing Then values.Add(value)
            End If
        Next raw
        If My.Settings.ps = "e" Then pv = True
        If qleaRestoreSaved(urls, True, True) = 0 Then
            qleaRestoreSaved(New String() {New Uri(qleaStart()).AbsoluteUri}, True, True)
        End If
        hiddenGroupTabs.Clear()
        My.Settings.activeIdentity = name
        My.Settings.Save()
        c.Items.Clear()
        f6.l0.Items.Clear()
        x.Items.Clear()
        For Each suggestion As String In My.Settings.s
            If suggestion <> "" AndAlso x.Items.Contains(suggestion) = False Then x.Items.Add(suggestion)
        Next suggestion
        qrb()
        qapplyalltabgroups()
        If feature6Loaded Then
            If My.Settings.hibernateEnabled = "e" AndAlso pv = False Then feature6HibernateTimer.Start() Else feature6HibernateTimer.Stop()
        End If
        If q8watchTimerReady Then
            If My.Settings.watchEnabled = "e" AndAlso pv = False Then q8watchTimer.Start() Else q8watchTimer.Stop()
        End If
    End Sub
    Function qportablepath() As String
        Dim besideApp As String = System.IO.Path.Combine(Application.StartupPath, "Firespider Portable Profile.txt")
        If System.IO.File.Exists(besideApp) Then Return besideApp
        Dim documents As String = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Firespider Portable Profile.txt")
        If System.IO.File.Exists(documents) Then Return documents
        Return besideApp
    End Function
    Sub qwriteportable()
        Try
            qsaveidentity(My.Settings.activeIdentity)
            Dim data As New StringBuilder("FIRESPIDER_PORTABLE_PROFILE_V1" & vbCrLf)
            data.AppendLine("A|" & qb64(My.Settings.activeIdentity))
            For Each raw As String In My.Settings.idp
                data.AppendLine("I|" & qb64(raw))
            Next raw
            Dim path As String = qportablepath()
            Try
                System.IO.File.WriteAllText(path, data.ToString, Encoding.UTF8)
            Catch ex As UnauthorizedAccessException
                path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Firespider Portable Profile.txt")
                System.IO.File.WriteAllText(path, data.ToString, Encoding.UTF8)
            End Try
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub qloadportable()
        Try
            Dim path As String = qportablepath()
            If System.IO.File.Exists(path) = False Then Exit Sub
            For Each line As String In System.IO.File.ReadAllLines(path, Encoding.UTF8).Skip(1)
                Dim fields As String() = line.Split(New Char() {"|"c}, 2)
                If fields.Length <> 2 Then Continue For
                If fields(0) = "A" Then My.Settings.activeIdentity = qunb64(fields(1))
                If fields(0) = "I" Then My.Settings.idp.Add(qunb64(fields(1)))
            Next line
            My.Settings.Save()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    ' --- Network Diagnostics ---
    Sub q902(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            Dim target As String = InputBox("Enter a URL or hostname to diagnose:", "Network Diagnostics Center", If(wb.Url Is Nothing, "https://www.example.com/", wb.Url.ToString)).Trim
            If target = "" Then Exit Sub
            Dim uri As Uri = Nothing
            If Uri.TryCreate(target, UriKind.Absolute, uri) = False Then Uri.TryCreate("https://" & target, UriKind.Absolute, uri)
            If uri Is Nothing Then Exit Sub
            Dim report As New StringBuilder
            report.AppendLine("<!doctype html><html><head><meta charset=""utf-8""><title>Network Diagnostics</title><style>body{max-width:850px;margin:35px auto;padding:0 24px;font:16px/1.55 Consolas,monospace}dt{font-weight:bold;margin-top:15px}dd{white-space:pre-wrap}</style></head><body><h1>Network Diagnostics Center</h1><dl>")
            report.AppendLine("<dt>Target</dt><dd>" & qh(uri.ToString) & "</dd><dt>Browser offline mode</dt><dd>" & If(wo, "Enabled", "Disabled") & "</dd><dt>System network available</dt><dd>" & If(NetworkInterface.GetIsNetworkAvailable(), "Yes", "No") & "</dd>")
            Dim watch As Stopwatch = Stopwatch.StartNew()
            Try
                Dim addresses As IPAddress() = Dns.GetHostAddresses(uri.Host)
                watch.Stop()
                report.AppendLine("<dt>DNS</dt><dd>" & watch.ElapsedMilliseconds.ToString & " ms · " & qh(String.Join(", ", addresses.Select(Function(address) address.ToString).ToArray)) & "</dd>")
            Catch ex As Exception
                report.AppendLine("<dt>DNS</dt><dd>Failed: " & qh(ex.Message) & "</dd>")
            End Try
            Try
                Dim proxy As IWebProxy = WebRequest.DefaultWebProxy
                report.AppendLine("<dt>Proxy</dt><dd>" & If(proxy Is Nothing OrElse proxy.IsBypassed(uri), "Direct", qh(proxy.GetProxy(uri).ToString)) & "</dd>")
            Catch ex As Exception
                report.AppendLine("<dt>Proxy</dt><dd>Unavailable: " & qh(ex.Message) & "</dd>")
            End Try
            Try
                watch.Restart()
                Dim request As HttpWebRequest = CType(WebRequest.Create(uri), HttpWebRequest)
                request.Method = "HEAD"
                request.Timeout = 7000
                request.AllowAutoRedirect = True
                request.UserAgent = "Firespider Internet Diagnostics"
                Using response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
                    watch.Stop()
                    report.AppendLine("<dt>HTTP response</dt><dd>" & CInt(response.StatusCode).ToString & " " & qh(response.StatusDescription) & " · " & watch.ElapsedMilliseconds.ToString & " ms · final " & qh(response.ResponseUri.ToString) & "</dd>")
                End Using
            Catch ex As Exception
                report.AppendLine("<dt>HTTP response</dt><dd>Failed: " & qh(ex.Message) & "</dd>")
            End Try
            report.AppendLine("</dl><h2>Troubleshooting</h2><ol><li>Confirm Work Offline is disabled.</li><li>Compare the resolved addresses with another device.</li><li>Check the proxy destination and authentication.</li><li>Retry HTTP and HTTPS separately when the server supports both.</li><li>Review blocked-site and content-filter rules for this host.</li></ol></body></html>")
            Dim path As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Firespider Network " & Guid.NewGuid().ToString("N") & ".html")
            System.IO.File.WriteAllText(path, report.ToString, Encoding.UTF8)
            q3(New Uri(path).AbsoluteUri)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q903(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q902(sender, e)
    End Sub

    ' --- Rule-Driven Automations ---
    Sub q904(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim action As String = InputBox(qautomationsummary() & vbCrLf & vbCrLf & "Enter ADD, TOGGLE, DELETE, RUN, or CLEAR:", "Browser Automation Rules", "ADD").Trim.ToUpperInvariant
            If action = "ADD" Then
                Dim name As String = InputBox("Enter an automation name:", "Add Automation", "Automation " & (My.Settings.au.Count + 1).ToString).Trim.Replace("|", "-")
                Dim kind As String = InputBox("Enter WORKSPACE, ARCHIVE, CLEAR, or STYLE:", "Add Automation", "ARCHIVE").Trim.ToUpperInvariant
                Dim interval As String = InputBox("Enter STARTUP, HOURLY, DAILY, or WEEKLY:", "Add Automation", "DAILY").Trim.ToUpperInvariant
                If name = "" OrElse New String() {"WORKSPACE", "ARCHIVE", "CLEAR", "STYLE"}.Contains(kind) = False OrElse New String() {"STARTUP", "HOURLY", "DAILY", "WEEKLY"}.Contains(interval) = False Then Exit Sub
                Dim payload As String = ""
                If kind = "WORKSPACE" Then payload = InputBox("Enter the workspace profile name:", "Automation Payload", If(qworkspacenames().Count = 0, "", qworkspacenames()(0)))
                If kind = "CLEAR" Then payload = InputBox("Enter categories: history,suggestions,downloads,notes,watchlist,analytics:", "Automation Payload", "history")
                If kind = "STYLE" Then payload = InputBox("Enter the saved user-style item number:", "Automation Payload", "1")
                My.Settings.au.Add(name & "|" & kind & "|" & interval & "|0|e|" & qb64(payload))
            ElseIf action = "CLEAR" Then
                My.Settings.au.Clear()
            Else
                Dim index As Integer = qautomationindex()
                If index < 0 Then Exit Sub
                Dim fields As String() = My.Settings.au(index).Split(New Char() {"|"c}, 6)
                If action = "DELETE" Then My.Settings.au.RemoveAt(index)
                If action = "TOGGLE" AndAlso fields.Length = 6 Then My.Settings.au(index) = fields(0) & "|" & fields(1) & "|" & fields(2) & "|" & fields(3) & "|" & If(fields(4) = "e", "", "e") & "|" & fields(5)
                If action = "RUN" Then qrunautomation(index)
            End If
            My.Settings.Save()
            su.Visible = True
            su.Text = "Browser automations updated"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q905(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q904(sender, e)
    End Sub
    Function qautomationsummary() As String
        Dim lines As New List(Of String)
        For i As Integer = 0 To My.Settings.au.Count - 1
            Dim fields As String() = My.Settings.au(i).Split(New Char() {"|"c}, 6)
            If fields.Length = 6 Then lines.Add((i + 1).ToString & ". " & fields(0) & " · " & fields(1) & " · " & fields(2) & " · " & If(fields(4) = "e", "on", "off"))
        Next i
        Return "Automation rules:" & vbCrLf & If(lines.Count = 0, "none", String.Join(vbCrLf, lines.ToArray))
    End Function
    Function qautomationindex() As Integer
        Dim value As Integer = 0
        If Integer.TryParse(InputBox(qautomationsummary() & vbCrLf & vbCrLf & "Enter the rule number:", "Browser Automation Rules", "1"), value) = False Then Return -1
        value -= 1
        If value < 0 OrElse value >= My.Settings.au.Count Then Return -1
        Return value
    End Function
    Sub qautomationtick(ByVal sender As Object, ByVal e As EventArgs)
        qrunautomations(False)
    End Sub
    Sub qrunautomations(ByVal startup As Boolean)
        For i As Integer = 0 To My.Settings.au.Count - 1
            Dim fields As String() = My.Settings.au(i).Split(New Char() {"|"c}, 6)
            If fields.Length <> 6 OrElse fields(4) <> "e" Then Continue For
            If startup AndAlso fields(2) = "STARTUP" Then qrunautomation(i) : Continue For
            If startup OrElse fields(2) = "STARTUP" Then Continue For
            Dim lastTicks As Long = 0
            Long.TryParse(fields(3), lastTicks)
            Dim elapsed As TimeSpan = If(lastTicks = 0, TimeSpan.MaxValue, Now.Subtract(New DateTime(lastTicks)))
            If (fields(2) = "HOURLY" AndAlso elapsed.TotalHours >= 1) OrElse (fields(2) = "DAILY" AndAlso elapsed.TotalDays >= 1) OrElse (fields(2) = "WEEKLY" AndAlso elapsed.TotalDays >= 7) Then qrunautomation(i)
        Next i
    End Sub
    Sub qrunautomation(ByVal index As Integer)
        If index < 0 OrElse index >= My.Settings.au.Count Then Exit Sub
        Dim fields As String() = My.Settings.au(index).Split(New Char() {"|"c}, 6)
        If fields.Length <> 6 Then Exit Sub
        Dim payload As String = qunb64(fields(5))
        If fields(1) = "WORKSPACE" Then qopenworkspace(payload)
        If fields(1) = "ARCHIVE" AndAlso k.SelectedTab IsNot Nothing AndAlso k.SelectedTab.Controls.Count > 0 Then qarchivebrowser(TryCast(k.SelectedTab.Controls(0), WebBrowser), "")
        If fields(1) = "CLEAR" Then qclearcategories(payload)
        If fields(1) = "STYLE" Then
            Dim styleIndex As Integer = 0
            If Integer.TryParse(payload, styleIndex) Then
                styleIndex -= 1
                If styleIndex >= 0 AndAlso styleIndex < My.Settings.us.Count Then
                    Dim styleFields As String() = My.Settings.us(styleIndex).Split(New Char() {"|"c}, 4)
                    If styleFields.Length = 4 AndAlso styleFields(1) = "style" Then qpreviewusercontent(styleIndex)
                End If
            End If
        End If
        fields(3) = Now.Ticks.ToString
        My.Settings.au(index) = String.Join("|", fields)
        My.Settings.Save()
    End Sub
    Sub qclearcategories(ByVal raw As String)
        For Each category As String In raw.ToLowerInvariant.Split(","c).Select(Function(value) value.Trim)
            If category = "history" Then f6.l0.Items.Clear() : c.Items.Clear()
            If category = "suggestions" Then My.Settings.s.Clear() : x.Items.Clear()
            If category = "downloads" Then My.Settings.dl.Clear() : My.Settings.dq.Clear() : My.Settings.downloadSafety.Clear()
            If category = "notes" Then My.Settings.qn = ""
            If category = "watchlist" Then My.Settings.watchItems.Clear()
            If category = "analytics" Then My.Settings.analytics.Clear()
        Next category
        My.Settings.Save()
    End Sub

    ' --- Local Browser Analytics ---
    Sub q906(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim action As String = InputBox("Local analytics: " & If(My.Settings.analyticsEnabled = "e", "enabled", "disabled") & vbCrLf & "Records: " & My.Settings.analytics.Count.ToString & vbCrLf & vbCrLf & "Enter REPORT, TOGGLE, CLEAR, or EXPORT:", "Local Browser Analytics", "REPORT").Trim.ToUpperInvariant
            If action = "TOGGLE" Then
                My.Settings.analyticsEnabled = If(My.Settings.analyticsEnabled = "e", "", "e")
                If My.Settings.analyticsEnabled = "e" AndAlso pv = False Then analyticsTimer.Start() Else analyticsTimer.Stop()
            ElseIf action = "CLEAR" Then
                If MessageBox.Show("Clear all local browser analytics?", "Local Browser Analytics", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then My.Settings.analytics.Clear()
            ElseIf action = "EXPORT" Then
                Dim save As New SaveFileDialog
                qsavefolder(save)
                save.FileName = "Firespider Local Analytics.csv"
                save.Filter = "CSV File [*.csv*]|*.csv|All Files [*.*]|*.*"
                If save.ShowDialog(Me) = DialogResult.OK Then System.IO.File.WriteAllText(save.FileName, qanalyticscsv(), Encoding.UTF8)
            ElseIf action = "REPORT" Then
                qanalyticsreport()
            End If
            My.Settings.Save()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q907(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q906(sender, e)
    End Sub
    Sub qanalyticstick(ByVal sender As Object, ByVal e As EventArgs)
        If pv OrElse My.Settings.analyticsEnabled <> "e" OrElse ContainsFocus = False OrElse WindowState = FormWindowState.Minimized OrElse k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Exit Sub
        Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
        If wb IsNot Nothing AndAlso wb.Url IsNot Nothing AndAlso wb.Url.Host <> "" Then qupdateanalytics(wb.Url.Host, 60, 0)
    End Sub
    Friend Sub qanalyticsvisit(ByVal wb As WebBrowser)
        If pv OrElse My.Settings.analyticsEnabled <> "e" OrElse wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Url.Host = "" Then Exit Sub
        qupdateanalytics(wb.Url.Host, 0, 1)
    End Sub
    Sub qupdateanalytics(ByVal domain As String, ByVal seconds As Integer, ByVal visits As Integer)
        Dim hour As String = Now.ToString("yyyy-MM-dd HH")
        For i As Integer = 0 To My.Settings.analytics.Count - 1
            Dim fields As String() = My.Settings.analytics(i).Split("|"c)
            If fields.Length = 4 AndAlso fields(0) = hour AndAlso qunb64(fields(1)).Equals(domain, StringComparison.OrdinalIgnoreCase) Then
                Dim existingSeconds As Integer = 0
                Dim existingVisits As Integer = 0
                Integer.TryParse(fields(2), existingSeconds)
                Integer.TryParse(fields(3), existingVisits)
                My.Settings.analytics(i) = hour & "|" & qb64(domain) & "|" & (existingSeconds + seconds).ToString & "|" & (existingVisits + visits).ToString
                My.Settings.Save()
                Exit Sub
            End If
        Next i
        My.Settings.analytics.Add(hour & "|" & qb64(domain) & "|" & seconds.ToString & "|" & visits.ToString)
        While My.Settings.analytics.Count > 2000
            My.Settings.analytics.RemoveAt(0)
        End While
        My.Settings.Save()
    End Sub
    Function qanalyticscsv() As String
        Dim data As New StringBuilder("Hour,Domain,Seconds,Visits" & vbCrLf)
        For Each raw As String In My.Settings.analytics
            Dim fields As String() = raw.Split("|"c)
            If fields.Length = 4 Then
                Dim quote As String = ChrW(34)
                Dim domain As String = qunb64(fields(1)).Replace(quote, quote & quote)
                data.AppendLine(quote & fields(0) & quote & "," & quote & domain & quote & "," & fields(2) & "," & fields(3))
            End If
        Next raw
        Return data.ToString
    End Function
    Sub qanalyticsreport()
        Dim domainSeconds As New Dictionary(Of String, Long)(StringComparer.OrdinalIgnoreCase)
        Dim domainVisits As New Dictionary(Of String, Long)(StringComparer.OrdinalIgnoreCase)
        Dim hours As New Dictionary(Of String, Long)
        For Each raw As String In My.Settings.analytics
            Dim fields As String() = raw.Split("|"c)
            If fields.Length <> 4 Then Continue For
            Dim domain As String = qunb64(fields(1))
            Dim seconds As Long = 0
            Dim visits As Long = 0
            Long.TryParse(fields(2), seconds)
            Long.TryParse(fields(3), visits)
            If domainSeconds.ContainsKey(domain) = False Then domainSeconds(domain) = 0 : domainVisits(domain) = 0
            domainSeconds(domain) += seconds
            domainVisits(domain) += visits
            Dim hour As String = fields(0).Substring(Math.Max(0, fields(0).Length - 2)) & ":00"
            If hours.ContainsKey(hour) = False Then hours(hour) = 0
            hours(hour) += seconds
        Next raw
        Dim archiveBytes As Long = qarchiverecords().Sum(Function(record) If(System.IO.File.Exists(record.HtmlPath), New System.IO.FileInfo(record.HtmlPath).Length, 0L) + If(System.IO.File.Exists(record.TextPath), New System.IO.FileInfo(record.TextPath).Length, 0L))
        Dim backupBytes As Long = 0
        If System.IO.Directory.Exists(qbackupfolder()) Then
            For Each file As String In System.IO.Directory.GetFiles(qbackupfolder(), "Firespider Backup *.txt")
                backupBytes += New System.IO.FileInfo(file).Length
            Next file
        End If
        Dim archiveMonths As New Dictionary(Of String, Integer)
        For Each record As ArchiveRecord In qarchiverecords()
            Dim month As String = If(record.DateSaved.Length >= 7, record.DateSaved.Substring(0, 7), "Unknown")
            If archiveMonths.ContainsKey(month) = False Then archiveMonths(month) = 0
            archiveMonths(month) += 1
        Next record
        Dim html As New StringBuilder("<!doctype html><html><head><meta charset=""utf-8""><title>Local Browser Analytics</title><style>body{max-width:900px;margin:35px auto;padding:0 24px;font:16px Segoe UI,Arial}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ccc;padding:8px;text-align:left}</style></head><body><h1>Local Browser Analytics</h1><p>Stored only in this Firespider profile. Private windows are excluded.</p><p>Archive storage: " & archiveBytes.ToString("N0") & " bytes · Backup storage: " & backupBytes.ToString("N0") & " bytes · Archive records: " & My.Settings.archives.Count.ToString & "</p><h2>Top Domains</h2><table><tr><th>Domain</th><th>Reading time</th><th>Visits</th></tr>")
        For Each pair As KeyValuePair(Of String, Long) In domainSeconds.OrderByDescending(Function(item) item.Value).Take(30)
            html.Append("<tr><td>" & qh(pair.Key) & "</td><td>" & TimeSpan.FromSeconds(pair.Value).ToString() & "</td><td>" & domainVisits(pair.Key).ToString & "</td></tr>")
        Next pair
        html.Append("</table><h2>Active Hours</h2><table><tr><th>Hour</th><th>Time</th></tr>")
        For Each pair As KeyValuePair(Of String, Long) In hours.OrderByDescending(Function(item) item.Value)
            html.Append("<tr><td>" & pair.Key & "</td><td>" & TimeSpan.FromSeconds(pair.Value).ToString & "</td></tr>")
        Next pair
        html.Append("</table><h2>Archive Growth</h2><table><tr><th>Month</th><th>Pages archived</th></tr>")
        For Each pair As KeyValuePair(Of String, Integer) In archiveMonths.OrderBy(Function(item) item.Key)
            html.Append("<tr><td>" & pair.Key & "</td><td>" & pair.Value.ToString & "</td></tr>")
        Next pair
        html.Append("</table></body></html>")
        Dim path As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Firespider Analytics " & Guid.NewGuid().ToString("N") & ".html")
        System.IO.File.WriteAllText(path, html.ToString, Encoding.UTF8)
        q3(New Uri(path).AbsoluteUri)
    End Sub

    Sub qarchivebrowser(ByVal wb As WebBrowser, ByVal savedUrl As String)
        Try
            If wb Is Nothing OrElse wb.Document Is Nothing Then Exit Sub
            Dim folder As String = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firespider Internet", "Archive")
            System.IO.Directory.CreateDirectory(folder)
            Dim title As String = If(wb.DocumentTitle = "", "Archived Page", wb.DocumentTitle)
            Dim stem As String = qsf(title) & " " & Now.ToString("yyyyMMdd HHmmss fff")
            Dim htmlPath As String = System.IO.Path.Combine(folder, stem & ".html")
            Dim textPath As String = System.IO.Path.Combine(folder, stem & ".txt")
            System.IO.File.WriteAllText(htmlPath, wb.DocumentText, Encoding.UTF8)
            System.IO.File.WriteAllText(textPath, If(wb.Document.Body Is Nothing, "", wb.Document.Body.InnerText), Encoding.UTF8)
            Dim url As String = If(savedUrl <> "", savedUrl, If(wb.Url Is Nothing, "", wb.Url.ToString))
            My.Settings.archives.Add(Now.ToString("yyyy-MM-dd HH:mm:ss") & "|" & qb64(title) & "|" & qb64(url) & "|" & qb64(htmlPath) & "|" & qb64(textPath))
            My.Settings.Save()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
End Class

Friend Class BackupSettingData
    Public Name As String
    Public IsCollection As Boolean
    Public Values As New List(Of String)
    Sub New(ByVal settingName As String, ByVal collection As Boolean)
        Name = settingName
        IsCollection = collection
    End Sub
    Public Overrides Function ToString() As String
        Return Name & " — " & Values.Count.ToString & If(IsCollection, " item(s)", " value")
    End Function
End Class

Friend Class BackupRestoreForm
    Inherits Form
    Dim ownerForm As j
    Dim pathBox As New TextBox
    Dim browseButton As New Button
    Dim settingsList As New CheckedListBox
    Dim validateButton As New Button
    Dim restoreButton As New Button
    Dim parsed As New Dictionary(Of String, BackupSettingData)
    Sub New(ByVal browser As j)
        ownerForm = browser
        Text = "Backup Restore And Selective Migration"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(760, 560)
        Dim top As New Panel With {.Dock = DockStyle.Top, .Height = 38}
        browseButton.Text = "Browse"
        browseButton.Dock = DockStyle.Right
        browseButton.Width = 100
        pathBox.Dock = DockStyle.Fill
        top.Controls.Add(pathBox)
        top.Controls.Add(browseButton)
        settingsList.Dock = DockStyle.Fill
        settingsList.CheckOnClick = True
        Dim bottom As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 45}
        validateButton.Text = "Validate"
        restoreButton.Text = "Restore Selected"
        restoreButton.Width = 140
        bottom.Controls.Add(validateButton)
        bottom.Controls.Add(restoreButton)
        Controls.Add(settingsList)
        Controls.Add(top)
        Controls.Add(bottom)
        AddHandler browseButton.Click, AddressOf qbrowse
        AddHandler validateButton.Click, AddressOf qvalidate
        AddHandler restoreButton.Click, AddressOf qrestore
    End Sub
    Sub qbrowse(ByVal sender As Object, ByVal e As EventArgs)
        Dim open As New OpenFileDialog
        open.Filter = "Firespider Backup [*.txt*]|*.txt|All Files [*.*]|*.*"
        open.InitialDirectory = ownerForm.qbackupfolder()
        If open.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        pathBox.Text = open.FileName
        parsed = ownerForm.qparsebackup(open.FileName)
        settingsList.Items.Clear()
        For Each item As BackupSettingData In parsed.Values.OrderBy(Function(value) value.Name)
            settingsList.Items.Add(item, True)
        Next item
    End Sub
    Sub qvalidate(ByVal sender As Object, ByVal e As EventArgs)
        MessageBox.Show(If(parsed.Count = 0, "No valid backup is loaded.", parsed.Count.ToString & " recognized setting group(s); " & parsed.Values.Sum(Function(item) item.Values.Count).ToString & " serialized value(s). Unknown or malformed records are ignored."), "Backup Validation", MessageBoxButtons.OK, If(parsed.Count = 0, MessageBoxIcon.Warning, MessageBoxIcon.Information))
    End Sub
    Sub qrestore(ByVal sender As Object, ByVal e As EventArgs)
        If parsed.Count = 0 Then Exit Sub
        Dim names As New List(Of String)
        For Each checked As Object In settingsList.CheckedItems
            names.Add(CType(checked, BackupSettingData).Name)
        Next checked
        If names.Count = 0 OrElse MessageBox.Show("Restore " & names.Count.ToString & " selected setting group(s)? A rollback backup will be created first.", "Restore Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
        MessageBox.Show(ownerForm.qrestorebackup(pathBox.Text, names), "Backup Restore", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class

Friend Class TabOverviewRecord
    Public Tab As TabPage
    Public Title As String
    Public Url As String
    Public Duplicate As Boolean
    Public EstimatedBytes As Long
    Public Thumbnail As Bitmap
    Sub New(ByVal page As TabPage, ByVal label As String, ByVal address As String, ByVal duplicateUrl As Boolean, ByVal estimate As Long, ByVal image As Bitmap)
        Tab = page
        Title = label
        Url = address
        Duplicate = duplicateUrl
        EstimatedBytes = estimate
        Thumbnail = image
    End Sub
End Class

Friend Class TabOverviewForm
    Inherits Form
    Dim ownerForm As j
    Dim records As List(Of TabOverviewRecord)
    Dim tabs As New ListView
    Dim images As New ImageList
    Dim details As New Label
    Sub New(ByVal browser As j, ByVal items As List(Of TabOverviewRecord))
        ownerForm = browser
        records = items
        Text = "Visual Tab Overview"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(1050, 700)
        images.ImageSize = New Size(240, 136)
        images.ColorDepth = ColorDepth.Depth32Bit
        tabs.Dock = DockStyle.Fill
        tabs.View = View.LargeIcon
        tabs.CheckBoxes = True
        tabs.MultiSelect = True
        tabs.LargeImageList = images
        For i As Integer = 0 To records.Count - 1
            images.Images.Add(records(i).Thumbnail)
            Dim item As New ListViewItem(If(records(i).Duplicate, "[Duplicate] ", "") & records(i).Title, i)
            item.Tag = records(i)
            tabs.Items.Add(item)
        Next i
        details.Dock = DockStyle.Bottom
        details.Height = 46
        details.TextAlign = ContentAlignment.MiddleLeft
        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 44}
        For Each action As String In New String() {"Reload", "Suspend", "Archive", "Close", "Merge Duplicates"}
            Dim button As New Button With {.Text = action, .AutoSize = True, .Tag = action}
            AddHandler button.Click, AddressOf qaction
            buttons.Controls.Add(button)
        Next action
        Controls.Add(tabs)
        Controls.Add(details)
        Controls.Add(buttons)
        AddHandler tabs.SelectedIndexChanged, AddressOf qdetails
    End Sub
    Function qchecked() As List(Of TabOverviewRecord)
        Return tabs.CheckedItems.Cast(Of ListViewItem)().Select(Function(item) CType(item.Tag, TabOverviewRecord)).ToList
    End Function
    Sub qdetails(ByVal sender As Object, ByVal e As EventArgs)
        If tabs.SelectedItems.Count = 0 Then Exit Sub
        Dim record As TabOverviewRecord = CType(tabs.SelectedItems(0).Tag, TabOverviewRecord)
        details.Text = record.Url & " · estimated DOM/image footprint " & record.EstimatedBytes.ToString("N0") & " bytes" & If(record.Duplicate, " · duplicate URL", "")
    End Sub
    Sub qaction(ByVal sender As Object, ByVal e As EventArgs)
        Dim action As String = CType(sender, Button).Tag.ToString
        Dim selected As List(Of TabOverviewRecord) = qchecked()
        If action = "Merge Duplicates" Then ownerForm.qoverviewmergeduplicates() : Close() : Exit Sub
        If selected.Count = 0 Then Exit Sub
        If action = "Reload" Then ownerForm.qoverviewreload(selected)
        If action = "Suspend" Then ownerForm.qoverviewsuspend(selected)
        If action = "Archive" Then ownerForm.qoverviewarchive(selected)
        If action = "Close" Then ownerForm.qoverviewclose(selected) : Close()
    End Sub
End Class

Friend Class BookmarkMetaRecord
    Public Url As String
    Public Tags As String
    Public ArchivePath As String
    Sub New(ByVal address As String, ByVal tagList As String, ByVal archive As String)
        Url = address
        Tags = tagList
        ArchivePath = archive
    End Sub
End Class

Friend Class AnnotationRecord
    Public Index As Integer
    Public Url As String
    Public DateSaved As String
    Public Text As String
    Public Note As String
    Public Color As String
    Public ArchivePath As String
    Sub New(ByVal recordIndex As Integer, ByVal address As String, ByVal saved As String, ByVal selectedText As String, ByVal annotationNote As String, ByVal highlightColor As String, ByVal archive As String)
        Index = recordIndex
        Url = address
        DateSaved = saved
        Text = selectedText
        Note = annotationNote
        Color = highlightColor
        ArchivePath = archive
    End Sub
    Public Overrides Function ToString() As String
        Return DateSaved & " — " & If(Note = "", Text, Note)
    End Function
End Class

Friend Class AnnotationLibraryForm
    Inherits Form
    Dim ownerForm As j
    Dim search As New TextBox
    Dim results As New ListBox
    Dim details As New TextBox
    Sub New(ByVal browser As j)
        ownerForm = browser
        Text = "Annotations And Highlights"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(850, 560)
        search.Dock = DockStyle.Top
        results.Dock = DockStyle.Left
        results.Width = 390
        details.Dock = DockStyle.Fill
        details.Multiline = True
        details.ReadOnly = True
        details.ScrollBars = ScrollBars.Vertical
        Dim buttons As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 44}
        For Each action As String In New String() {"Open Page", "Open Archive", "Copy Note", "Delete"}
            Dim button As New Button With {.Text = action, .AutoSize = True, .Tag = action}
            AddHandler button.Click, AddressOf qaction
            buttons.Controls.Add(button)
        Next action
        Controls.Add(details)
        Controls.Add(results)
        Controls.Add(search)
        Controls.Add(buttons)
        AddHandler search.TextChanged, AddressOf qfilter
        AddHandler results.SelectedIndexChanged, AddressOf qdetails
        qfilter(Nothing, EventArgs.Empty)
    End Sub
    Sub qfilter(ByVal sender As Object, ByVal e As EventArgs)
        results.Items.Clear()
        For Each record As AnnotationRecord In ownerForm.qannotationrecords()
            If search.Text = "" OrElse (record.Url & " " & record.Text & " " & record.Note).IndexOf(search.Text, StringComparison.OrdinalIgnoreCase) >= 0 Then results.Items.Add(record)
        Next record
        If results.Items.Count > 0 Then results.SelectedIndex = 0
    End Sub
    Function qselected() As AnnotationRecord
        Return TryCast(results.SelectedItem, AnnotationRecord)
    End Function
    Sub qdetails(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As AnnotationRecord = qselected()
        If record IsNot Nothing Then details.Text = "Page: " & record.Url & vbCrLf & "Saved: " & record.DateSaved & vbCrLf & "Highlight: " & record.Text & vbCrLf & "Note: " & record.Note & vbCrLf & "Color: " & record.Color & vbCrLf & "Archive: " & If(record.ArchivePath = "", "none", record.ArchivePath)
    End Sub
    Sub qaction(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As AnnotationRecord = qselected()
        If record Is Nothing Then Exit Sub
        Dim action As String = CType(sender, Button).Tag.ToString
        If action = "Open Page" Then ownerForm.q1(record.Url)
        If action = "Open Archive" AndAlso System.IO.File.Exists(record.ArchivePath) Then ownerForm.q3(New Uri(record.ArchivePath).AbsoluteUri)
        If action = "Copy Note" Then Clipboard.SetText(If(record.Note = "", record.Text, record.Note))
        If action = "Delete" Then ownerForm.qremoveannotation(record) : qfilter(Nothing, EventArgs.Empty)
    End Sub
End Class
