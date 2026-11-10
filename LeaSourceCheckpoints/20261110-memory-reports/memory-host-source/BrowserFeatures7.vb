Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Partial Class j
    Sub qfeatures7()
        bo.DropDownItems.Add(qnewitem("Research Collection Manager", AddressOf q931, AddressOf q932))
        bo.DropDownItems.Add(qnewitem("Versioned Page Archive Library", AddressOf q933, AddressOf q934))
    End Sub

    ' --- Research Collections ---
    Sub q931(ByVal sender As Object, ByVal e As EventArgs)
        Try
            q7ensurestorage()
            Dim action As String = InputBox(q7researchsummary() & vbCrLf & vbCrLf & "Enter CREATE, ADD, OPEN, RENAME, DUPLICATE, EXPORT, or DELETE:", "Research Collection Manager", "CREATE").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            If action = "CREATE" Then
                Dim name As String = InputBox("Enter a research collection name:", "Create Research Collection", "Research " & Now.ToString("yyyy-MM-dd")).Trim
                If name = "" Then Exit Sub
                If q7collectionbyname(name) IsNot Nothing Then
                    su.Visible = True
                    su.Text = "A research collection with that name already exists"
                    Exit Sub
                End If
                Dim record As New ResearchCollectionRecord
                record.Id = Guid.NewGuid().ToString("N")
                record.Name = name
                record.Created = Now.ToString("o")
                My.Settings.research.Add(q7collectionraw(record))
                My.Settings.Save()
                su.Visible = True
                su.Text = "Research collection created: " & name
            Else
                Dim collection As ResearchCollectionRecord = q7choosecollection("Choose Research Collection")
                If collection Is Nothing Then Exit Sub
                If action = "ADD" Then
                    q7addresearchitem(collection)
                ElseIf action = "OPEN" Then
                    Dim urls As List(Of String) = q7researchitems(collection.Id).Select(Function(item) item.Url).Where(Function(url) qvurl(url)).Distinct(StringComparer.OrdinalIgnoreCase).ToList
                    If urls.Count = 0 Then
                        su.Visible = True
                        su.Text = "This research collection has no web pages to open"
                    Else
                        Dim opened As Integer = qlazyopenurls(urls)
                        su.Visible = True
                        su.Text = opened.ToString & " Lea research tab(s) restored"
                        If opened < urls.Count Then su.Text &= "; " & (urls.Count - opened).ToString & " URL(s) skipped by Lea limits"
                    End If
                ElseIf action = "RENAME" Then
                    Dim name As String = InputBox("Enter the new collection name:", "Rename Research Collection", collection.Name).Trim
                    If name = "" Then Exit Sub
                    Dim existing As ResearchCollectionRecord = q7collectionbyname(name)
                    If existing IsNot Nothing AndAlso existing.Id <> collection.Id Then
                        su.Visible = True
                        su.Text = "A research collection with that name already exists"
                        Exit Sub
                    End If
                    collection.Name = name
                    q7replacecollection(collection)
                    My.Settings.Save()
                    su.Visible = True
                    su.Text = "Research collection renamed"
                ElseIf action = "DUPLICATE" Then
                    Dim name As String = InputBox("Enter the duplicate collection name:", "Duplicate Research Collection", collection.Name & " Copy").Trim
                    If name = "" OrElse q7collectionbyname(name) IsNot Nothing Then Exit Sub
                    Dim duplicate As New ResearchCollectionRecord
                    duplicate.Id = Guid.NewGuid().ToString("N")
                    duplicate.Name = name
                    duplicate.Created = Now.ToString("o")
                    My.Settings.research.Add(q7collectionraw(duplicate))
                    For Each item As ResearchItemRecord In q7researchitems(collection.Id)
                        item.CollectionId = duplicate.Id
                        item.Id = Guid.NewGuid().ToString("N")
                        item.Added = Now.ToString("o")
                        My.Settings.research.Add(q7itemraw(item))
                    Next item
                    My.Settings.Save()
                    su.Visible = True
                    su.Text = "Research collection duplicated"
                ElseIf action = "EXPORT" Then
                    q7exportcollection(collection)
                ElseIf action = "DELETE" Then
                    If MessageBox.Show("Delete the collection '" & collection.Name & "' and its references?" & vbCrLf & vbCrLf & "Bookmarks, annotations, reading-list entries, sessions, and archive files will not be deleted.", "Delete Research Collection", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
                    For i As Integer = My.Settings.research.Count - 1 To 0 Step -1
                        Dim raw As String = My.Settings.research(i)
                        Dim savedCollection As ResearchCollectionRecord = q7parsecollection(raw)
                        Dim savedItem As ResearchItemRecord = q7parseitem(raw)
                        If (savedCollection IsNot Nothing AndAlso savedCollection.Id = collection.Id) OrElse (savedItem IsNot Nothing AndAlso savedItem.CollectionId = collection.Id) Then My.Settings.research.RemoveAt(i)
                    Next i
                    My.Settings.Save()
                    su.Visible = True
                    su.Text = "Research collection deleted; source records were kept"
                End If
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q932(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q931(sender, e)
    End Sub

    Sub q7ensurestorage()
        If My.Settings.research Is Nothing Then My.Settings.research = New System.Collections.Specialized.StringCollection
        If My.Settings.archiveVersions Is Nothing Then My.Settings.archiveVersions = New System.Collections.Specialized.StringCollection
        If My.Settings.archiveRetention Is Nothing Then My.Settings.archiveRetention = New System.Collections.Specialized.StringCollection
    End Sub

    Friend Function q7researchcollections() As List(Of ResearchCollectionRecord)
        q7ensurestorage()
        Dim records As New List(Of ResearchCollectionRecord)
        For Each raw As String In My.Settings.research
            Dim record As ResearchCollectionRecord = q7parsecollection(raw)
            If record IsNot Nothing Then records.Add(record)
        Next raw
        Return records.OrderBy(Function(item) item.Name).ToList
    End Function

    Friend Function q7researchitems(ByVal collectionId As String) As List(Of ResearchItemRecord)
        q7ensurestorage()
        Dim records As New List(Of ResearchItemRecord)
        For Each raw As String In My.Settings.research
            Dim record As ResearchItemRecord = q7parseitem(raw)
            If record IsNot Nothing AndAlso (collectionId = "" OrElse record.CollectionId = collectionId) Then records.Add(record)
        Next raw
        Return records.OrderByDescending(Function(item) item.Added).ToList
    End Function

    Function q7parsecollection(ByVal raw As String) As ResearchCollectionRecord
        Try
            Dim fields As String() = raw.Split("|"c)
            If fields.Length <> 5 OrElse fields(0) <> "v1" OrElse fields(1) <> "C" Then Return Nothing
            Dim record As New ResearchCollectionRecord
            record.Id = qunb64(fields(2))
            record.Name = qunb64(fields(3))
            record.Created = qunb64(fields(4))
            If record.Id = "" OrElse record.Name = "" Then Return Nothing
            Return record
        Catch
            Return Nothing
        End Try
    End Function

    Function q7parseitem(ByVal raw As String) As ResearchItemRecord
        Try
            Dim fields As String() = raw.Split("|"c)
            If fields.Length <> 9 OrElse fields(0) <> "v1" OrElse fields(1) <> "I" Then Return Nothing
            Dim record As New ResearchItemRecord
            record.Id = qunb64(fields(2))
            record.CollectionId = qunb64(fields(3))
            record.Kind = qunb64(fields(4))
            record.Title = qunb64(fields(5))
            record.Url = qunb64(fields(6))
            record.Reference = qunb64(fields(7))
            record.Added = qunb64(fields(8))
            If record.Id = "" OrElse record.CollectionId = "" Then Return Nothing
            Return record
        Catch
            Return Nothing
        End Try
    End Function

    Function q7collectionraw(ByVal record As ResearchCollectionRecord) As String
        Return "v1|C|" & qb64(record.Id) & "|" & qb64(record.Name) & "|" & qb64(record.Created)
    End Function

    Function q7itemraw(ByVal record As ResearchItemRecord) As String
        Return "v1|I|" & qb64(record.Id) & "|" & qb64(record.CollectionId) & "|" & qb64(record.Kind) & "|" & qb64(record.Title) & "|" & qb64(record.Url) & "|" & qb64(record.Reference) & "|" & qb64(record.Added)
    End Function

    Function q7collectionbyname(ByVal name As String) As ResearchCollectionRecord
        Return q7researchcollections().FirstOrDefault(Function(item) item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
    End Function

    Function q7choosecollection(ByVal title As String) As ResearchCollectionRecord
        Dim records As List(Of ResearchCollectionRecord) = q7researchcollections()
        If records.Count = 0 Then
            su.Visible = True
            su.Text = "Create a research collection first"
            Return Nothing
        End If
        Dim lines As New List(Of String)
        For i As Integer = 0 To records.Count - 1
            lines.Add((i + 1).ToString & ". " & records(i).Name & " (" & q7researchitems(records(i).Id).Count.ToString & " item(s))")
        Next i
        Dim answer As String = InputBox(String.Join(vbCrLf, lines.ToArray) & vbCrLf & vbCrLf & "Enter a collection number or exact name:", title, "1").Trim
        Dim index As Integer = 0
        If Integer.TryParse(answer, index) Then
            index -= 1
            If index >= 0 AndAlso index < records.Count Then Return records(index)
        End If
        Return records.FirstOrDefault(Function(item) item.Name.Equals(answer, StringComparison.OrdinalIgnoreCase))
    End Function

    Function q7researchsummary() As String
        Dim records As List(Of ResearchCollectionRecord) = q7researchcollections()
        If records.Count = 0 Then Return "Research collections: none"
        Dim lines As New List(Of String)
        For Each record As ResearchCollectionRecord In records.Take(15)
            lines.Add(record.Name & " — " & q7researchitems(record.Id).Count.ToString & " item(s)")
        Next record
        Return "Research collections:" & vbCrLf & String.Join(vbCrLf, lines.ToArray)
    End Function

    Sub q7replacecollection(ByVal record As ResearchCollectionRecord)
        For i As Integer = 0 To My.Settings.research.Count - 1
            Dim existing As ResearchCollectionRecord = q7parsecollection(My.Settings.research(i))
            If existing IsNot Nothing AndAlso existing.Id = record.Id Then
                My.Settings.research(i) = q7collectionraw(record)
                Exit Sub
            End If
        Next i
    End Sub

    Sub q7addresearchitem(ByVal collection As ResearchCollectionRecord)
        Dim kinds As String = "CURRENT, BOOKMARK, ANNOTATION, READING, ARCHIVE, or SESSION"
        Dim kind As String = InputBox("Enter " & kinds & ":", "Add To " & collection.Name, "CURRENT").Trim.ToUpperInvariant
        If kind = "" Then Exit Sub
        Dim choices As New List(Of ResearchItemRecord)
        If kind = "CURRENT" Then
            If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Exit Sub
            Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
            Dim url As String = qpersistedtaburl(k.SelectedTab, wb)
            If url = "" Then Exit Sub
            choices.Add(q7newresearchitem(collection.Id, "page", If(wb Is Nothing OrElse wb.DocumentTitle = "", url, wb.DocumentTitle), url, url))
        ElseIf kind = "BOOKMARK" Then
            For Each raw As String In My.Settings.bm
                Dim url As String = qbu(raw)
                If qvurl(url) Then choices.Add(q7newresearchitem(collection.Id, "bookmark", url, url, raw))
            Next raw
        ElseIf kind = "ANNOTATION" Then
            For Each record As AnnotationRecord In qannotationrecords()
                choices.Add(q7newresearchitem(collection.Id, "annotation", If(record.Note = "", record.Text, record.Note), record.Url, record.DateSaved & "|" & record.Text))
            Next record
        ElseIf kind = "READING" Then
            For Each raw As String In My.Settings.rl
                Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
                If fields.Length = 4 Then choices.Add(q7newresearchitem(collection.Id, "reading", fields(2), fields(3), raw))
            Next raw
        ElseIf kind = "ARCHIVE" Then
            For Each record As ArchiveRecord In qarchiverecords()
                choices.Add(q7newresearchitem(collection.Id, "archive", record.Title, record.Url, record.HtmlPath))
            Next record
            For Each record As VersionedArchiveRecord In q7archiveversions()
                choices.Add(q7newresearchitem(collection.Id, "archive-version", record.Title, record.Url, record.HtmlPath))
            Next record
        ElseIf kind = "SESSION" Then
            For Each name As String In qsessions()
                Dim urls As New List(Of String)
                For Each raw As String In My.Settings.ns
                    Dim split As Integer = raw.IndexOf("|"c)
                    If split > 0 AndAlso raw.Substring(0, split).Equals(name, StringComparison.OrdinalIgnoreCase) Then urls.Add(raw.Substring(split + 1))
                Next raw
                choices.Add(q7newresearchitem(collection.Id, "session", name, If(urls.Count = 0, "", urls(0)), String.Join(vbLf, urls.ToArray)))
            Next name
        Else
            su.Visible = True
            su.Text = "Unknown research item type"
            Exit Sub
        End If
        If choices.Count = 0 Then
            su.Visible = True
            su.Text = "No matching items are available"
            Exit Sub
        End If
        Dim selected As ResearchItemRecord = choices(0)
        If choices.Count > 1 Then
            Dim lines As New List(Of String)
            For i As Integer = 0 To Math.Min(choices.Count, 30) - 1
                lines.Add((i + 1).ToString & ". " & If(choices(i).Title.Length > 80, choices(i).Title.Substring(0, 80) & "...", choices(i).Title))
            Next i
            Dim answer As String = InputBox(String.Join(vbCrLf, lines.ToArray) & vbCrLf & vbCrLf & "Enter the item number:", "Add Research Item", "1")
            Dim index As Integer = 0
            If Integer.TryParse(answer, index) = False Then Exit Sub
            index -= 1
            If index < 0 OrElse index >= choices.Count Then Exit Sub
            selected = choices(index)
        End If
        If q7researchitems(collection.Id).Any(Function(item) item.Kind = selected.Kind AndAlso item.Reference.Equals(selected.Reference, StringComparison.OrdinalIgnoreCase)) Then
            su.Visible = True
            su.Text = "That item is already in the research collection"
            Exit Sub
        End If
        My.Settings.research.Add(q7itemraw(selected))
        My.Settings.Save()
        su.Visible = True
        su.Text = "Added " & selected.Kind & " to " & collection.Name
    End Sub

    Function q7newresearchitem(ByVal collectionId As String, ByVal kind As String, ByVal title As String, ByVal url As String, ByVal reference As String) As ResearchItemRecord
        Dim record As New ResearchItemRecord
        record.Id = Guid.NewGuid().ToString("N")
        record.CollectionId = collectionId
        record.Kind = kind
        record.Title = If(title, "").Trim
        If record.Title.Length > 500 Then record.Title = record.Title.Substring(0, 500)
        record.Url = If(url, "").Trim
        record.Reference = If(reference, "")
        record.Added = Now.ToString("o")
        Return record
    End Function

    Sub q7exportcollection(ByVal collection As ResearchCollectionRecord)
        Dim dialog As New SaveFileDialog
        qsavefolder(dialog)
        dialog.FileName = qsf(collection.Name) & " Research Collection.txt"
        dialog.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
        If dialog.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        Dim data As New StringBuilder
        data.AppendLine("Firespider Research Collection")
        data.AppendLine("Name: " & collection.Name)
        data.AppendLine("Created: " & collection.Created)
        data.AppendLine("Exported: " & Now.ToString("o"))
        data.AppendLine()
        For Each item As ResearchItemRecord In q7researchitems(collection.Id)
            data.AppendLine("[" & item.Kind.ToUpperInvariant & "] " & item.Title)
            If item.Url <> "" Then data.AppendLine(item.Url)
            data.AppendLine("Added: " & item.Added)
            data.AppendLine()
        Next item
        File.WriteAllText(dialog.FileName, data.ToString, Encoding.UTF8)
        su.Visible = True
        su.Text = "Research collection exported"
    End Sub

    ' --- Versioned Page Archives ---
    Sub q933(ByVal sender As Object, ByVal e As EventArgs)
        Try
            q7ensurestorage()
            Dim action As String = InputBox("Versioned archives: " & My.Settings.archiveVersions.Count.ToString & vbCrLf & vbCrLf & "Enter CAPTURE, TIMELINE, COMPARE, OPEN, DELETE, or RETENTION:", "Versioned Page Archive Library", "CAPTURE").Trim.ToUpperInvariant
            If action = "CAPTURE" Then
                q7captureversion()
            ElseIf action = "TIMELINE" Then
                q7showtimeline(False)
            ElseIf action = "COMPARE" Then
                q7showtimeline(True)
            ElseIf action = "OPEN" Then
                Dim record As VersionedArchiveRecord = q7choosearchiveversion("Open Archive Version")
                If record IsNot Nothing Then q3(New Uri(record.HtmlPath).AbsoluteUri)
            ElseIf action = "DELETE" Then
                Dim record As VersionedArchiveRecord = q7choosearchiveversion("Delete Archive Version")
                If record Is Nothing OrElse MessageBox.Show("Delete this saved archive version and its local files?", "Delete Archive Version", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
                q7deletearchiveversion(record, True)
                My.Settings.Save()
                su.Visible = True
                su.Text = "Archive version deleted"
            ElseIf action = "RETENTION" Then
                q7setretention()
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q934(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q933(sender, e)
    End Sub

    Friend Function q7archiveversions() As List(Of VersionedArchiveRecord)
        q7ensurestorage()
        Dim records As New List(Of VersionedArchiveRecord)
        For Each raw As String In My.Settings.archiveVersions
            Dim record As VersionedArchiveRecord = q7parsearchiveversion(raw)
            If record IsNot Nothing Then records.Add(record)
        Next raw
        Return records.OrderByDescending(Function(item) item.DateSaved).ToList
    End Function

    Function q7parsearchiveversion(ByVal raw As String) As VersionedArchiveRecord
        Try
            Dim fields As String() = raw.Split("|"c)
            If fields.Length <> 10 OrElse fields(0) <> "v1" Then Return Nothing
            Dim record As New VersionedArchiveRecord
            record.Id = qunb64(fields(1))
            record.DateSaved = qunb64(fields(2))
            record.Title = qunb64(fields(3))
            record.Url = qunb64(fields(4))
            record.CanonicalUrl = qunb64(fields(5))
            record.HtmlPath = qunb64(fields(6))
            record.TextPath = qunb64(fields(7))
            record.Hash = fields(8)
            record.ParentId = qunb64(fields(9))
            If record.Id = "" OrElse record.CanonicalUrl = "" Then Return Nothing
            Return record
        Catch
            Return Nothing
        End Try
    End Function

    Function q7archiveversionraw(ByVal record As VersionedArchiveRecord) As String
        Return "v1|" & qb64(record.Id) & "|" & qb64(record.DateSaved) & "|" & qb64(record.Title) & "|" & qb64(record.Url) & "|" & qb64(record.CanonicalUrl) & "|" & qb64(record.HtmlPath) & "|" & qb64(record.TextPath) & "|" & record.Hash & "|" & qb64(record.ParentId)
    End Function

    Function q7canonicalurl(ByVal value As String) As String
        Try
            Dim address As New Uri(value)
            Dim builder As New UriBuilder(address)
            builder.Fragment = ""
            If builder.Path = "" Then builder.Path = "/"
            Return builder.Uri.AbsoluteUri
        Catch
            Return value
        End Try
    End Function

    Sub q7captureversion()
        If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Exit Sub
        Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
        If wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing Then Exit Sub
        Dim canonical As String = q7canonicalurl(wb.Url.ToString)
        Dim text As String = If(wb.Document.Body Is Nothing, "", wb.Document.Body.InnerText)
        Dim normalized As String = q7normalizetext(text)
        Dim hash As String = q7sha256text(normalized)
        Dim existing As List(Of VersionedArchiveRecord) = q7archiveversions().Where(Function(item) item.CanonicalUrl.Equals(canonical, StringComparison.OrdinalIgnoreCase)).OrderByDescending(Function(item) item.DateSaved).ToList
        If existing.Count > 0 AndAlso existing(0).Hash = hash Then
            su.Visible = True
            su.Text = "No content change; newest archive version was kept"
            Exit Sub
        End If
        Dim folder As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firespider Internet", "Archive", "Versions")
        Directory.CreateDirectory(folder)
        Dim title As String = If(wb.DocumentTitle = "", wb.Url.Host, wb.DocumentTitle)
        Dim stem As String = qsf(title) & " " & Now.ToString("yyyyMMdd HHmmss fff")
        Dim record As New VersionedArchiveRecord
        record.Id = Guid.NewGuid().ToString("N")
        record.DateSaved = Now.ToString("o")
        record.Title = title
        record.Url = wb.Url.ToString
        record.CanonicalUrl = canonical
        record.HtmlPath = Path.Combine(folder, stem & ".html")
        record.TextPath = Path.Combine(folder, stem & ".txt")
        record.Hash = hash
        record.ParentId = If(existing.Count = 0, "", existing(0).Id)
        File.WriteAllText(record.HtmlPath, wb.DocumentText, Encoding.UTF8)
        File.WriteAllText(record.TextPath, text, Encoding.UTF8)
        My.Settings.archiveVersions.Add(q7archiveversionraw(record))
        q7pruneversions(canonical)
        My.Settings.Save()
        su.Visible = True
        su.Text = "New page version archived"
    End Sub

    Function q7normalizetext(ByVal value As String) As String
        If value Is Nothing Then Return ""
        Dim lines As New List(Of String)
        For Each line As String In value.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(ControlChars.Lf)
            Dim clean As String = System.Text.RegularExpressions.Regex.Replace(line, "\s+", " ").Trim
            If clean <> "" Then lines.Add(clean)
        Next line
        Return String.Join(vbLf, lines.ToArray)
    End Function

    Function q7sha256text(ByVal value As String) As String
        Using sha As SHA256 = SHA256.Create()
            Return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant
        End Using
    End Function

    Function q7choosearchiveversion(ByVal title As String) As VersionedArchiveRecord
        Dim records As List(Of VersionedArchiveRecord) = q7archiveversions()
        If records.Count = 0 Then
            su.Visible = True
            su.Text = "No versioned page archives are saved"
            Return Nothing
        End If
        Dim lines As New List(Of String)
        For i As Integer = 0 To Math.Min(records.Count, 40) - 1
            lines.Add((i + 1).ToString & ". " & records(i).DateSaved & " — " & records(i).Title)
        Next i
        Dim answer As String = InputBox(String.Join(vbCrLf, lines.ToArray) & vbCrLf & vbCrLf & "Enter an archive number:", title, "1")
        Dim index As Integer = 0
        If Integer.TryParse(answer, index) = False Then Return Nothing
        index -= 1
        If index < 0 OrElse index >= records.Count Then Return Nothing
        Return records(index)
    End Function

    Sub q7showtimeline(ByVal compare As Boolean)
        Dim records As List(Of VersionedArchiveRecord) = q7archiveversions()
        Dim urls As List(Of String) = records.Select(Function(item) item.CanonicalUrl).Distinct(StringComparer.OrdinalIgnoreCase).ToList
        If urls.Count = 0 Then
            su.Visible = True
            su.Text = "No versioned page archives are saved"
            Exit Sub
        End If
        Dim lines As New List(Of String)
        For i As Integer = 0 To Math.Min(urls.Count, 30) - 1
            lines.Add((i + 1).ToString & ". " & urls(i) & " (" & records.Where(Function(item) item.CanonicalUrl.Equals(urls(i), StringComparison.OrdinalIgnoreCase)).Count.ToString & ")")
        Next i
        Dim answer As String = InputBox(String.Join(vbCrLf, lines.ToArray) & vbCrLf & vbCrLf & "Enter a page number:", If(compare, "Compare Archive Versions", "Archive Timeline"), "1")
        Dim index As Integer = 0
        If Integer.TryParse(answer, index) = False Then Exit Sub
        index -= 1
        If index < 0 OrElse index >= urls.Count Then Exit Sub
        Dim versions As List(Of VersionedArchiveRecord) = records.Where(Function(item) item.CanonicalUrl.Equals(urls(index), StringComparison.OrdinalIgnoreCase)).OrderByDescending(Function(item) item.DateSaved).ToList
        If compare Then
            If versions.Count < 2 Then
                su.Visible = True
                su.Text = "At least two versions are required for comparison"
                Exit Sub
            End If
            q7compareversions(versions(1), versions(0))
        Else
            Dim report As New StringBuilder
            report.AppendLine("Archive timeline: " & urls(index))
            report.AppendLine()
            For i As Integer = 0 To versions.Count - 1
                report.AppendLine((i + 1).ToString & ". " & versions(i).DateSaved & " — " & versions(i).Title & " — " & versions(i).Hash.Substring(0, Math.Min(12, versions(i).Hash.Length)))
            Next i
            MessageBox.Show(report.ToString, "Versioned Page Archive Timeline", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Sub q7compareversions(ByVal older As VersionedArchiveRecord, ByVal newer As VersionedArchiveRecord)
        If File.Exists(older.TextPath) = False OrElse File.Exists(newer.TextPath) = False Then
            su.Visible = True
            su.Text = "One or both archive text files are missing"
            Exit Sub
        End If
        Dim oldLines As String() = q7normalizetext(File.ReadAllText(older.TextPath, Encoding.UTF8)).Split(ControlChars.Lf)
        Dim newLines As String() = q7normalizetext(File.ReadAllText(newer.TextPath, Encoding.UTF8)).Split(ControlChars.Lf)
        Dim oldSet As New HashSet(Of String)(oldLines, StringComparer.Ordinal)
        Dim newSet As New HashSet(Of String)(newLines, StringComparer.Ordinal)
        Dim html As New StringBuilder("<!doctype html><html><head><meta charset=""utf-8""><title>Archive Comparison</title><style>body{max-width:1000px;margin:30px auto;padding:0 24px;font:15px Segoe UI,Arial}.add{background:#e6ffed}.remove{background:#ffeef0}.same{color:#666}p{padding:6px;margin:3px 0}</style></head><body>")
        html.Append("<h1>Archive Comparison</h1><p>Older: " & qh(older.DateSaved) & "<br>Newer: " & qh(newer.DateSaved) & "<br>Page: " & qh(newer.Url) & "</p><h2>Removed</h2>")
        For Each line As String In oldLines.Where(Function(value) value <> "" AndAlso newSet.Contains(value) = False)
            html.Append("<p class=""remove"">− " & qh(line) & "</p>")
        Next line
        html.Append("<h2>Added</h2>")
        For Each line As String In newLines.Where(Function(value) value <> "" AndAlso oldSet.Contains(value) = False)
            html.Append("<p class=""add"">+ " & qh(line) & "</p>")
        Next line
        html.Append("</body></html>")
        Dim path As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Firespider Archive Comparison " & Guid.NewGuid().ToString("N") & ".html")
        File.WriteAllText(path, html.ToString, Encoding.UTF8)
        q3(New Uri(path).AbsoluteUri)
        su.Visible = True
        su.Text = "Archive comparison opened"
    End Sub

    Sub q7setretention()
        Dim url As String = ""
        If k.SelectedTab IsNot Nothing AndAlso k.SelectedTab.Controls.Count > 0 Then
            Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
            If wb IsNot Nothing AndAlso wb.Url IsNot Nothing Then url = q7canonicalurl(wb.Url.ToString)
        End If
        url = InputBox("Enter the page URL whose version retention should change:", "Archive Version Retention", url).Trim
        If url = "" Then Exit Sub
        url = q7canonicalurl(url)
        Dim mode As String = InputBox("Enter ALL, 5, or 10:", "Archive Version Retention", q7retention(url)).Trim.ToLowerInvariant
        If mode <> "all" AndAlso mode <> "5" AndAlso mode <> "10" Then Exit Sub
        For i As Integer = My.Settings.archiveRetention.Count - 1 To 0 Step -1
            Dim fields As String() = My.Settings.archiveRetention(i).Split("|"c)
            If fields.Length = 2 AndAlso qunb64(fields(0)).Equals(url, StringComparison.OrdinalIgnoreCase) Then My.Settings.archiveRetention.RemoveAt(i)
        Next i
        My.Settings.archiveRetention.Add(qb64(url) & "|" & mode)
        q7pruneversions(url)
        My.Settings.Save()
        su.Visible = True
        su.Text = "Archive retention set to " & mode
    End Sub

    Function q7retention(ByVal url As String) As String
        For Each raw As String In My.Settings.archiveRetention
            Dim fields As String() = raw.Split("|"c)
            If fields.Length = 2 AndAlso qunb64(fields(0)).Equals(url, StringComparison.OrdinalIgnoreCase) Then Return fields(1)
        Next raw
        Return "all"
    End Function

    Sub q7pruneversions(ByVal url As String)
        Dim mode As String = q7retention(url)
        If mode = "all" Then Exit Sub
        Dim keep As Integer = 0
        If Integer.TryParse(mode, keep) = False OrElse keep < 1 Then Exit Sub
        Dim records As List(Of VersionedArchiveRecord) = q7archiveversions().Where(Function(item) item.CanonicalUrl.Equals(url, StringComparison.OrdinalIgnoreCase)).OrderByDescending(Function(item) item.DateSaved).ToList
        For Each record As VersionedArchiveRecord In records.Skip(keep).ToArray
            q7deletearchiveversion(record, True)
        Next record
    End Sub

    Sub q7deletearchiveversion(ByVal record As VersionedArchiveRecord, ByVal deleteFiles As Boolean)
        For i As Integer = My.Settings.archiveVersions.Count - 1 To 0 Step -1
            Dim existing As VersionedArchiveRecord = q7parsearchiveversion(My.Settings.archiveVersions(i))
            If existing IsNot Nothing AndAlso existing.Id = record.Id Then My.Settings.archiveVersions.RemoveAt(i)
        Next i
        If deleteFiles Then
            Try
                If File.Exists(record.HtmlPath) Then File.Delete(record.HtmlPath)
                If File.Exists(record.TextPath) Then File.Delete(record.TextPath)
            Catch ex As Exception
                qlog(ex)
            End Try
        End If
    End Sub
End Class

Friend Class ResearchCollectionRecord
    Public Id As String = ""
    Public Name As String = ""
    Public Created As String = ""
    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class

Friend Class ResearchItemRecord
    Public Id As String = ""
    Public CollectionId As String = ""
    Public Kind As String = ""
    Public Title As String = ""
    Public Url As String = ""
    Public Reference As String = ""
    Public Added As String = ""
End Class

Friend Class VersionedArchiveRecord
    Public Id As String = ""
    Public DateSaved As String = ""
    Public Title As String = ""
    Public Url As String = ""
    Public CanonicalUrl As String = ""
    Public HtmlPath As String = ""
    Public TextPath As String = ""
    Public Hash As String = ""
    Public ParentId As String = ""
End Class
