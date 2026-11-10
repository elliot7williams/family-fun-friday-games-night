Imports System.ComponentModel
Imports System.Text

Partial Class j
    Friend fav As ToolStripMenuItem
    Friend recoveryRequested As Boolean = False
    Dim recoveryTimer As New Timer
    Dim contrastOn As Boolean = False
    Dim featuresBuilt As Boolean = False
    Dim lastWindowClosing As Boolean = False
    Shared featureSessionStarted As Boolean = False
    Shared featureWindows As Integer = 0

    Sub qfeaturestartup()
        Try
            featureWindows += 1
            If featureSessionStarted = False Then
                If Not pv AndAlso My.Settings.recoveryEnabled = "e" AndAlso My.Settings.cleanExit <> "e" AndAlso My.Settings.recovery.Count > 0 Then
                    recoveryRequested = True
                End If
                featureSessionStarted = True
                My.Settings.cleanExit = ""
                My.Settings.Save()
            End If
            recoveryTimer.Interval = 30000
            AddHandler recoveryTimer.Tick, AddressOf qrecoverytick
            If My.Settings.recoveryEnabled = "e" Then recoveryTimer.Start()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qfeatures()
        If featuresBuilt Then Exit Sub
        featuresBuilt = True
        Try
            he.DropDownItems.Add(qnewitem("Command Palette (Ctrl+Shift+P)", AddressOf q809, AddressOf q810))
            fl.DropDownItems.Add(qnewitem("Named Session Manager", AddressOf q814, AddressOf q815))
            Dim closedWindows As ToolStripMenuItem = qnewitem("Recently Closed Windows", Nothing, Nothing)
            AddHandler closedWindows.DropDownOpening, AddressOf q820
            fl.DropDownItems.Add(closedWindows)
            fl.DropDownItems.Add(qnewitem("Remove Missing Downloads", AddressOf q818, AddressOf q819))
            bo.DropDownItems.Add(qnewitem("Bookmark Folder Manager", AddressOf q816, AddressOf q817))
            bo.DropDownItems.Add(qnewitem("Add Current Page To Reading List", AddressOf q825, AddressOf q826))
            Dim reading As ToolStripMenuItem = qnewitem("Reading List", Nothing, Nothing)
            AddHandler reading.DropDownOpening, AddressOf q827
            bo.DropDownItems.Add(reading)
            Me.e.DropDownItems.Add(qnewitem("Unified Browser Search", AddressOf q838, AddressOf q839))
            v.DropDownItems.Add(qnewitem("Open Start Page Dashboard", AddressOf q834, AddressOf q835))
            v.DropDownItems.Add(qnewitem("Save Current Domain Preferences", AddressOf q840, AddressOf q841))
            v.DropDownItems.Add(qnewitem("Clear Current Domain Preferences", AddressOf q842, AddressOf q843))
            ot.DropDownItems.Add(qnewitem("Automatic Crash Recovery", AddressOf q823, AddressOf q824))
            ot.DropDownItems.Add(qnewitem("Use Dashboard For New Tabs", AddressOf q836, AddressOf q837))
            ot.DropDownItems.Add(qnewitem("Choose Default Save Folder", AddressOf q844, AddressOf q845))
            ot.DropDownItems.Add(qnewitem("Reset Default Save Folder", AddressOf q846, AddressOf q847))
            ot.DropDownItems.Add(qnewitem("Clear Data On Exit", AddressOf q848, AddressOf q849))
            ot.DropDownItems.Add(qnewitem("Block Current Site", AddressOf q850, AddressOf q851))
            ot.DropDownItems.Add(qnewitem("Manage Blocked Sites", AddressOf q852, AddressOf q853))
            ot.DropDownItems.Add(qnewitem("Pop-up Policy", AddressOf q854, AddressOf q855))
            ot.DropDownItems.Add(qnewitem("External Protocol Confirmation", AddressOf q856, AddressOf q857))
            qfeatures2()
            qleaMark("menus-features")
            qremoveitem(fl, "Reopen Last Closed Window")
            qdedupe()
            qleaMark("menus-deduped")
            qorganizemenus()
            qleaMark("menus-organized")
            fav = qnewitem("Favorites", Nothing, Nothing)
            AddHandler fav.DropDownOpening, AddressOf q811
            m.DropDownItems.Insert(0, fav)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Function qnewitem(ByVal text As String, ByVal clickHandler As EventHandler, ByVal hoverHandler As EventHandler) As ToolStripMenuItem
        Dim item As New ToolStripMenuItem
        item.Text = text
        item.Image = My.Resources.F9
        item.BackColor = Color.White
        item.ForeColor = Color.Black
        item.Padding = New Padding(0)
        If clickHandler IsNot Nothing Then AddHandler item.Click, clickHandler
        If hoverHandler IsNot Nothing Then AddHandler item.MouseHover, hoverHandler
        Return item
    End Function

    Sub qremoveitem(ByVal root As ToolStripMenuItem, ByVal text As String)
        For i As Integer = root.DropDownItems.Count - 1 To 0 Step -1
            If root.DropDownItems(i).Text.Replace("&", "").Equals(text, StringComparison.OrdinalIgnoreCase) Then root.DropDownItems.RemoveAt(i)
        Next i
    End Sub

    Sub qdedupe()
        For Each root As ToolStripMenuItem In New ToolStripMenuItem() {fl, Me.e, v, hi, bo, ot, he}
            Dim found As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For i As Integer = root.DropDownItems.Count - 1 To 0 Step -1
                Dim item As ToolStripMenuItem = TryCast(root.DropDownItems(i), ToolStripMenuItem)
                If item Is Nothing Then Continue For
                Dim key As String = item.Text.Replace("&", "").Trim
                If found.Add(key) = False Then root.DropDownItems.RemoveAt(i)
            Next i
        Next root
    End Sub

    Sub qorganizemenus()
        qorganizeroot(fl, New String() {"Tabs And Windows", "Sessions And Recovery", "Open And Import", "Save And Export", "Downloads And Files", "Other"})
        qorganizeroot(Me.e, New String() {"Clipboard And Copy", "Page Text", "Links And Metadata", "Search And Find", "Other"})
        qorganizeroot(v, New String() {"Page Display", "Accessibility", "Window And Layout", "Reload And Stop", "Other"})
        qorganizeroot(hi, New String() {"Navigation", "Domains And History", "Tab History", "Search", "Other"})
        qorganizeroot(bo, New String() {"Save And Organize", "Open Bookmarks", "Reading", "Import And Export", "Other"})
        qorganizeroot(ot, New String() {"Startup And Recovery", "Privacy And Safety", "Downloads And Files", "Appearance", "Data And Maintenance", "Other"})
        qorganizeroot(he, New String() {"Commands And Help", "Diagnostics", "Page Information", "Other"})
    End Sub

    Sub qorganizeroot(ByVal root As ToolStripMenuItem, ByVal categories As String())
        Dim oldItems As New List(Of ToolStripItem)
        For Each oldItem As ToolStripItem In root.DropDownItems
            If TypeOf oldItem Is ToolStripMenuItem Then oldItems.Add(oldItem)
        Next oldItem
        root.DropDownItems.Clear()
        Dim parents As New Dictionary(Of String, ToolStripMenuItem)(StringComparer.OrdinalIgnoreCase)
        Try
            For Each category As String In categories
                Dim parent As New ToolStripMenuItem(category)
                parent.Image = My.Resources.F9
                parent.BackColor = Color.White
                parent.ForeColor = Color.Black
                parent.Padding = New Padding(0)
                parents.Add(category, parent)
                parent.DropDown.SuspendLayout()
                root.DropDownItems.Add(parent)
            Next category
            For Each item As ToolStripMenuItem In oldItems
                Dim category As String = qmenucategory(root.Text, item.Text)
                If parents.ContainsKey(category) = False Then category = "Other"
                parents(category).DropDownItems.Add(item)
            Next item
            For i As Integer = root.DropDownItems.Count - 1 To 0 Step -1
                Dim parent As ToolStripMenuItem = CType(root.DropDownItems(i), ToolStripMenuItem)
                If parent.DropDownItems.Count = 0 Then root.DropDownItems.RemoveAt(i)
            Next i
        Finally
            For Each parent As ToolStripMenuItem In parents.Values
                parent.DropDown.ResumeLayout(True)
            Next
        End Try
    End Sub

    Function qmenucategory(ByVal root As String, ByVal text As String) As String
        Dim value As String = text.ToLowerInvariant
        root = root.Replace("&", "").ToLowerInvariant
        If root.Contains("file") Then
            If value.Contains("download") OrElse value.Contains("shortcut") Then Return "Downloads And Files"
            If value.Contains("session") OrElse value.Contains("workspace") OrElse value.Contains("recover") OrElse value.Contains("closed window") OrElse value.Contains("snapshot") Then Return "Sessions And Recovery"
            If value.Contains("save") OrElse value.Contains("archive") OrElse value.Contains("export") OrElse value.Contains("print") Then Return "Save And Export"
            If value.Contains("open") OrElse value.Contains("restore") OrElse value.Contains("migrate") OrElse value.Contains("import") OrElse value.Contains("load") Then Return "Open And Import"
            If value.Contains("tab") OrElse value.Contains("window") OrElse value.Contains("close") OrElse value.Contains("duplicate") Then Return "Tabs And Windows"
        ElseIf root.Contains("edit") Then
            If value.Contains("search") OrElse value.Contains("find") Then Return "Search And Find"
            If value.Contains("link") OrElse value.Contains("url") OrElse value.Contains("domain") OrElse value.Contains("meta") OrElse value.Contains("json") OrElse value.Contains("html") Then Return "Links And Metadata"
            If value.Contains("text") OrElse value.Contains("title") OrElse value.Contains("readable") OrElse value.Contains("word") Then Return "Page Text"
            If value.Contains("copy") OrElse value.Contains("paste") OrElse value.Contains("clipboard") OrElse value.Contains("cut") Then Return "Clipboard And Copy"
        ElseIf root.Contains("view") Then
            If value.Contains("contrast") OrElse value.Contains("text size") OrElse value.Contains("reader") OrElse value.Contains("access") OrElse value.Contains("domain preference") Then Return "Accessibility"
            If value.Contains("window") OrElse value.Contains("tab overview") OrElse value.Contains("tab group") OrElse value.Contains("hibernate") OrElse value.Contains("hibernation") OrElse value.Contains("vertical") OrElse value.Contains("side-by-side") OrElse value.Contains("split") OrElse value.Contains("toolbar") OrElse value.Contains("full screen") OrElse value.Contains("tile") Then Return "Window And Layout"
            If value.Contains("reload") OrElse value.Contains("stop") OrElse value.Contains("refresh") Then Return "Reload And Stop"
            If value.Contains("zoom") OrElse value.Contains("image") OrElse value.Contains("style") OrElse value.Contains("dashboard") OrElse value.Contains("page") Then Return "Page Display"
        ElseIf root.Contains("history") Then
            If value.Contains("search") Then Return "Search"
            If value.Contains("domain") OrElse value.Contains("history") OrElse value.Contains("visited") Then Return "Domains And History"
            If value.Contains("tab") Then Return "Tab History"
            If value.Contains("back") OrElse value.Contains("forward") OrElse value.Contains("home") OrElse value.Contains("reload") OrElse value.Contains("open") Then Return "Navigation"
        ElseIf root.Contains("bookmark") Then
            If value.Contains("reading") OrElse value.Contains("feed") OrElse value.Contains("watchlist") OrElse value.Contains("versioned") Then Return "Reading"
            If value.Contains("import") OrElse value.Contains("export") OrElse value.Contains("html") Then Return "Import And Export"
            If value.Contains("open") OrElse value.Contains("visit") OrElse value.Contains("smart") OrElse value.Contains("archive library") Then Return "Open Bookmarks"
            If value.Contains("save") OrElse value.Contains("annotation") OrElse value.Contains("intelligence") OrElse value.Contains("collection") OrElse value.Contains("add") OrElse value.Contains("remove") OrElse value.Contains("clear") OrElse value.Contains("folder") OrElse value.Contains("bookmark") OrElse value.Contains("vault") Then Return "Save And Organize"
        ElseIf root.Contains("option") Then
            If value.Contains("privacy") OrElse value.Contains("permission") OrElse value.Contains("filter") OrElse value.Contains("autofill") OrElse value.Contains("script") OrElse value.Contains("style") OrElse value.Contains("block") OrElse value.Contains("protocol") OrElse value.Contains("pop-up") OrElse value.Contains("private") OrElse value.Contains("offline") Then Return "Privacy And Safety"
            If value.Contains("download") OrElse value.Contains("save folder") OrElse value.Contains("file") Then Return "Downloads And Files"
            If value.Contains("startup") OrElse value.Contains("identity") OrElse value.Contains("recovery") OrElse value.Contains("session") OrElse value.Contains("hibernate") OrElse value.Contains("hibernation") OrElse value.Contains("lazy") OrElse value.Contains("new tab") OrElse value.Contains("home") Then Return "Startup And Recovery"
            If value.Contains("color") OrElse value.Contains("theme") OrElse value.Contains("toolbar") OrElse value.Contains("appearance") Then Return "Appearance"
            If value.Contains("clear") OrElse value.Contains("automation") OrElse value.Contains("alias") OrElse value.Contains("backup") OrElse value.Contains("validate") OrElse value.Contains("import") OrElse value.Contains("export") OrElse value.Contains("log") OrElse value.Contains("data") Then Return "Data And Maintenance"
        ElseIf root.Contains("help") Then
            If value.Contains("command") OrElse value.Contains("help") OrElse value.Contains("shortcut") OrElse value.Contains("about") Then Return "Commands And Help"
            If value.Contains("error") OrElse value.Contains("diagnostic") OrElse value.Contains("validate") OrElse value.Contains("status") Then Return "Diagnostics"
            If value.Contains("page") OrElse value.Contains("identity") OrElse value.Contains("resource") OrElse value.Contains("document") OrElse value.Contains("browser") Then Return "Page Information"
        End If
        Return "Other"
    End Function

    Friend Function qcommandlist() As List(Of FirespiderCommand)
        Dim commands As New List(Of FirespiderCommand)
        For Each root As ToolStripItem In m.DropDownItems
            Dim menu As ToolStripMenuItem = TryCast(root, ToolStripMenuItem)
            If menu Is Nothing OrElse menu Is fav Then Continue For
            qcollectcommands(menu, menu.Text.Replace("&", ""), commands)
        Next root
        Return commands
    End Function

    Sub qcollectcommands(ByVal menu As ToolStripMenuItem, ByVal path As String, ByVal commands As List(Of FirespiderCommand))
        For Each child As ToolStripItem In menu.DropDownItems
            Dim item As ToolStripMenuItem = TryCast(child, ToolStripMenuItem)
            If item Is Nothing Then Continue For
            Dim childPath As String = path & " > " & item.Text.Replace("&", "")
            If item.DropDownItems.Count > 0 Then
                qcollectcommands(item, childPath, commands)
            ElseIf item.Enabled Then
                commands.Add(New FirespiderCommand(qcommandid(childPath), childPath, item))
            End If
        Next child
    End Sub

    Function qcommandid(ByVal value As String) As String
        Dim result As New StringBuilder
        For Each letter As Char In value.ToLowerInvariant
            If Char.IsLetterOrDigit(letter) Then result.Append(letter)
        Next letter
        Return result.ToString
    End Function

    Friend Sub qtogglefavorite(ByVal id As String)
        If My.Settings.fc.Contains(id) Then
            My.Settings.fc.Remove(id)
        Else
            My.Settings.fc.Add(id)
        End If
        My.Settings.Save()
        qfavoriteitems()
    End Sub

    Sub qfavoriteitems()
        If fav Is Nothing Then Exit Sub
        fav.DropDownItems.Clear()
        Dim byId As New Dictionary(Of String, FirespiderCommand)
        For Each command As FirespiderCommand In qcommandlist()
            If byId.ContainsKey(command.Id) = False Then byId.Add(command.Id, command)
        Next command
        For Each id As String In My.Settings.fc
            If byId.ContainsKey(id) Then
                Dim item As ToolStripMenuItem = qnewitem(byId(id).Label, AddressOf q812, AddressOf q813)
                item.Tag = byId(id).Item
                fav.DropDownItems.Add(item)
            End If
        Next id
        If fav.DropDownItems.Count = 0 Then
            Dim empty As New ToolStripMenuItem("Pin commands from the Command Palette")
            empty.Enabled = False
            fav.DropDownItems.Add(empty)
        End If
    End Sub

    Sub q809(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim palette As New CommandPaletteForm(Me)
            palette.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q810(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q809(sender, e)
    End Sub
    Sub q811(ByVal sender As Object, ByVal e As EventArgs)
        qfavoriteitems()
    End Sub
    Sub q812(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim original As ToolStripMenuItem = TryCast(CType(sender, ToolStripMenuItem).Tag, ToolStripMenuItem)
            If original IsNot Nothing Then original.PerformClick()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q813(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q812(sender, e)
    End Sub

    Sub q814(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim names As List(Of String) = qsessions()
            Dim action As String = InputBox("Enter OPEN, RENAME, REPLACE, or DELETE." & vbCrLf & "Saved sessions: " & If(names.Count = 0, "none", String.Join(", ", names.ToArray)), "Named Session Manager", "OPEN").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            Dim selected As String = InputBox("Enter the session name:", "Named Session Manager", If(names.Count = 0, "", names(0))).Trim
            If selected = "" Then Exit Sub
            If action = "OPEN" Then
                qopensession(selected)
            ElseIf action = "DELETE" Then
                For i As Integer = My.Settings.ns.Count - 1 To 0 Step -1
                    If My.Settings.ns(i).StartsWith(selected & "|", StringComparison.OrdinalIgnoreCase) Then My.Settings.ns.RemoveAt(i)
                Next i
                My.Settings.Save()
                su.Text = "Named session deleted"
            ElseIf action = "RENAME" Then
                Dim newName As String = InputBox("Enter the new name:", "Rename Named Session", selected).Trim.Replace("|", "-")
                If newName = "" Then Exit Sub
                For i As Integer = 0 To My.Settings.ns.Count - 1
                    Dim split As Integer = My.Settings.ns(i).IndexOf("|"c)
                    If split > 0 AndAlso My.Settings.ns(i).Substring(0, split).Equals(selected, StringComparison.OrdinalIgnoreCase) Then My.Settings.ns(i) = newName & My.Settings.ns(i).Substring(split)
                Next i
                My.Settings.Save()
                su.Text = "Named session renamed"
            ElseIf action = "REPLACE" Then
                qsavesession(selected)
            End If
            su.Visible = True
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q815(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q814(sender, e)
    End Sub

    Function qsessions() As List(Of String)
        Dim names As New List(Of String)
        For Each entry As String In My.Settings.ns
            Dim split As Integer = entry.IndexOf("|"c)
            If split > 0 Then
                Dim name As String = entry.Substring(0, split)
                If names.Contains(name) = False Then names.Add(name)
            End If
        Next entry
        Return names
    End Function

    Sub qsavesession(ByVal name As String)
        name = If(name, "").Trim.Replace("|", "-")
        If name = "" Then Return
        For i As Integer = My.Settings.ns.Count - 1 To 0 Step -1
            If My.Settings.ns(i).StartsWith(name & "|", StringComparison.OrdinalIgnoreCase) Then My.Settings.ns.RemoveAt(i)
        Next i
        Dim saved As Integer = 0
        For Each tab As TabPage In qalltabs()
            If tab.Controls.Count = 0 Then Continue For
            Dim url As String = qleaUrl(tab)
            If url <> "" Then
                My.Settings.ns.Add(name.Replace("|", "-") & "|" & url)
                saved += 1
            End If
        Next tab
        My.Settings.Save()
        su.Visible = True
        su.Text = saved.ToString & " tab(s) saved as " & name
    End Sub

    Sub qopensession(ByVal name As String)
        name = If(name, "").Trim()
        Dim urls As New List(Of String)
        For Each entry As String In My.Settings.ns
            Dim split As Integer = entry.IndexOf("|"c)
            If split > 0 AndAlso entry.Substring(0, split).Equals(name, StringComparison.OrdinalIgnoreCase) Then urls.Add(entry.Substring(split + 1))
        Next entry
        If urls.Count = 0 Then
            su.Visible = True
            su.Text = "Named session not found"
            Exit Sub
        End If
        Dim opened = qleaRestoreSaved(urls, True)
        su.Visible = True
        su.Text = opened.ToString & " Lea tab(s) opened from " & name
        If opened < urls.Count Then su.Text &= "; " & (urls.Count - opened).ToString & " URL(s) skipped by Lea limits"
    End Sub

    Sub q816(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim action As String = InputBox("Enter CREATE, RENAME, MOVE, or DELETE.", "Bookmark Folder Manager", "RENAME").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            If action = "CREATE" Then
                Dim folder As String = InputBox("Enter the new folder name. The current page will be saved in it:", "Create Bookmark Folder", "General").Trim.Replace("|", "-")
                Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
                If folder <> "" AndAlso wb.Url IsNot Nothing Then My.Settings.bm.Add(folder & "|" & wb.Url.ToString)
            ElseIf action = "RENAME" Then
                Dim oldName As String = InputBox("Enter the folder to rename:", "Rename Bookmark Folder", "General").Trim
                Dim newName As String = InputBox("Enter the new folder name:", "Rename Bookmark Folder", oldName).Trim.Replace("|", "-")
                If oldName = "" OrElse newName = "" Then Exit Sub
                For i As Integer = 0 To My.Settings.bm.Count - 1
                    If qbf(My.Settings.bm(i)).Equals(oldName, StringComparison.OrdinalIgnoreCase) Then My.Settings.bm(i) = newName & "|" & qbu(My.Settings.bm(i))
                Next i
            ElseIf action = "DELETE" Then
                Dim folder As String = InputBox("Enter the folder to delete with all bookmarks in it:", "Delete Bookmark Folder", "General").Trim
                If folder = "" OrElse MessageBox.Show("Delete folder " & folder & " and all of its bookmarks?", "Bookmark Folder Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
                For i As Integer = My.Settings.bm.Count - 1 To 0 Step -1
                    If qbf(My.Settings.bm(i)).Equals(folder, StringComparison.OrdinalIgnoreCase) Then My.Settings.bm.RemoveAt(i)
                Next i
            ElseIf action = "MOVE" Then
                Dim match As String = InputBox("Enter all or part of the bookmark URL:", "Move Bookmark", "").Trim
                Dim folder As String = InputBox("Enter the destination folder:", "Move Bookmark", "General").Trim.Replace("|", "-")
                If match = "" OrElse folder = "" Then Exit Sub
                For i As Integer = 0 To My.Settings.bm.Count - 1
                    If qbu(My.Settings.bm(i)).IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0 Then
                        My.Settings.bm(i) = folder & "|" & qbu(My.Settings.bm(i))
                        Exit For
                    End If
                Next i
            End If
            My.Settings.Save()
            qrb()
            su.Visible = True
            su.Text = "Bookmark folders updated"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q817(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q816(sender, e)
    End Sub

    Sub q818(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim removed As Integer = 0
            Dim removedPaths As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For i As Integer = My.Settings.dl.Count - 1 To 0 Step -1
                If System.IO.File.Exists(My.Settings.dl(i)) = False AndAlso System.IO.Directory.Exists(My.Settings.dl(i)) = False Then
                    removedPaths.Add(My.Settings.dl(i))
                    My.Settings.dl.RemoveAt(i)
                    removed += 1
                End If
            Next i
            For i As Integer = My.Settings.dq.Count - 1 To 0 Step -1
                Dim record As DownloadQueueRecord = qparsedownload(My.Settings.dq(i))
                If record Is Nothing OrElse (System.IO.File.Exists(record.Path) = False AndAlso System.IO.Directory.Exists(record.Path) = False) Then
                    If record IsNot Nothing AndAlso removedPaths.Add(record.Path) Then removed += 1
                    My.Settings.dq.RemoveAt(i)
                End If
            Next i
            My.Settings.Save()
            su.Visible = True
            su.Text = removed.ToString & " missing download(s) removed"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q819(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q818(sender, e)
    End Sub

    Sub q820(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim menu As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
            menu.DropDownItems.Clear()
            For Each raw As String In My.Settings.cwh
                Dim values As String() = raw.Split(ControlChars.Tab)
                If values.Length < 2 Then Continue For
                Dim item As ToolStripMenuItem = qnewitem(values(0) & " (" & (values.Length - 1).ToString & " tab(s))", AddressOf q821, AddressOf q822)
                item.Tag = raw
                menu.DropDownItems.Add(item)
            Next raw
            If menu.DropDownItems.Count = 0 Then
                Dim empty As New ToolStripMenuItem("No recently closed windows")
                empty.Enabled = False
                menu.DropDownItems.Add(empty)
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q821(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim values As String() = CType(sender, ToolStripMenuItem).Tag.ToString.Split(ControlChars.Tab)
            Dim window As j = qleaWindow(values.Skip(1))
            su.Visible = True
            su.Text = If(window Is Nothing, "No supported closed-window URLs", window.k.TabCount.ToString & " Lea tab(s) reopened in a new window")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q822(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q821(sender, e)
    End Sub

    Sub q823(ByVal sender As Object, ByVal e As EventArgs)
        My.Settings.recoveryEnabled = If(My.Settings.recoveryEnabled = "e", "", "e")
        If My.Settings.recoveryEnabled = "e" Then
            recoveryTimer.Start()
            qrecoverysnapshot()
        Else
            recoveryTimer.Stop()
        End If
        My.Settings.Save()
        su.Visible = True
        su.Text = "Automatic crash recovery " & If(My.Settings.recoveryEnabled = "e", "enabled", "disabled")
    End Sub
    Sub q824(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q823(sender, e)
    End Sub

    Sub q825(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim url As String = wb.Url.ToString
            For Each raw As String In My.Settings.rl
                Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
                If fields.Length = 4 AndAlso fields(3).Equals(url, StringComparison.OrdinalIgnoreCase) Then
                    su.Visible = True
                    su.Text = "Page is already in the reading list"
                    Exit Sub
                End If
            Next raw
            Dim title As String = If(wb.DocumentTitle = "", url, wb.DocumentTitle).Replace("|", "-")
            My.Settings.rl.Add("unread|" & Now.ToString("yyyy-MM-dd") & "|" & title & "|" & url)
            My.Settings.Save()
            su.Visible = True
            su.Text = "Page added to reading list"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q826(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q825(sender, e)
    End Sub

    Sub q827(ByVal sender As Object, ByVal e As EventArgs)
        Dim menu As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        menu.DropDownItems.Clear()
        For i As Integer = 0 To My.Settings.rl.Count - 1
            Dim fields As String() = My.Settings.rl(i).Split(New Char() {"|"c}, 4)
            If fields.Length <> 4 Then Continue For
            Dim parent As ToolStripMenuItem = qnewitem(If(fields(0) = "unread", "● ", "✓ ") & fields(2) & " - " & fields(1), Nothing, Nothing)
            parent.Tag = i
            Dim openItem As ToolStripMenuItem = qnewitem("Open", AddressOf q828, AddressOf q829)
            openItem.Tag = i
            Dim markItem As ToolStripMenuItem = qnewitem(If(fields(0) = "unread", "Mark Read", "Mark Unread"), AddressOf q830, AddressOf q831)
            markItem.Tag = i
            Dim removeItem As ToolStripMenuItem = qnewitem("Remove", AddressOf q832, AddressOf q833)
            removeItem.Tag = i
            parent.DropDownItems.Add(openItem)
            parent.DropDownItems.Add(markItem)
            parent.DropDownItems.Add(removeItem)
            menu.DropDownItems.Add(parent)
        Next i
        If menu.DropDownItems.Count = 0 Then
            Dim empty As New ToolStripMenuItem("Reading list is empty")
            empty.Enabled = False
            menu.DropDownItems.Add(empty)
        End If
    End Sub
    Sub q828(ByVal sender As Object, ByVal e As EventArgs)
        Dim index As Integer = CInt(CType(sender, ToolStripMenuItem).Tag)
        If index < 0 OrElse index >= My.Settings.rl.Count Then Exit Sub
        Dim fields As String() = My.Settings.rl(index).Split(New Char() {"|"c}, 4)
        If fields.Length = 4 Then q1(fields(3))
    End Sub
    Sub q829(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q828(sender, e)
    End Sub
    Sub q830(ByVal sender As Object, ByVal e As EventArgs)
        Dim index As Integer = CInt(CType(sender, ToolStripMenuItem).Tag)
        If index < 0 OrElse index >= My.Settings.rl.Count Then Exit Sub
        Dim fields As String() = My.Settings.rl(index).Split(New Char() {"|"c}, 4)
        If fields.Length = 4 Then My.Settings.rl(index) = If(fields(0) = "unread", "read", "unread") & "|" & fields(1) & "|" & fields(2) & "|" & fields(3)
        My.Settings.Save()
    End Sub
    Sub q831(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q830(sender, e)
    End Sub
    Sub q832(ByVal sender As Object, ByVal e As EventArgs)
        Dim index As Integer = CInt(CType(sender, ToolStripMenuItem).Tag)
        If index >= 0 AndAlso index < My.Settings.rl.Count Then My.Settings.rl.RemoveAt(index)
        My.Settings.Save()
    End Sub
    Sub q833(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q832(sender, e)
    End Sub

    Sub q834(ByVal sender As Object, ByVal e As EventArgs)
        q1(qdashboardurl())
    End Sub
    Sub q835(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q834(sender, e)
    End Sub
    Sub q836(ByVal sender As Object, ByVal e As EventArgs)
        My.Settings.dashboard = If(My.Settings.dashboard = "e", "", "e")
        My.Settings.Save()
        su.Visible = True
        su.Text = "Dashboard for new tabs " & If(My.Settings.dashboard = "e", "enabled", "disabled")
    End Sub
    Sub q837(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q836(sender, e)
    End Sub

    Friend Function qdashboardurl() As String
        Try
            Dim html As New StringBuilder
            html.AppendLine("<!doctype html><html><head><meta charset=""utf-8""><title>Firespider Dashboard</title><style>body{font:16px Segoe UI,Arial;background:#20242b;color:#eee;max-width:1050px;margin:35px auto;padding:0 24px}h1{color:#ff6b35}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(280px,1fr));gap:18px}.card{background:#2b3039;border-radius:8px;padding:18px}a{color:#79b8ff;word-break:break-all}li{margin:7px 0}.status{color:#a7f3a7}</style></head><body>")
            html.AppendLine("<h1>Firespider Dashboard</h1><p class=""status"">Recovery: " & If(My.Settings.recoveryEnabled = "e", "enabled", "disabled") & If(My.Settings.recoveryTime = "", "", " · last snapshot " & qh(My.Settings.recoveryTime)) & "</p><div class=""grid"">")
            html.AppendLine("<section class=""card""><h2>Bookmarks</h2><ul>")
            For Each raw As String In My.Settings.bm
                Dim url As String = qbu(raw)
                If qvurl(url) Then html.AppendLine("<li>[" & qh(qbf(raw)) & "] <a href=""" & qh(url) & """>" & qh(url) & "</a></li>")
            Next raw
            html.AppendLine("</ul></section><section class=""card""><h2>Named Sessions</h2><ul>")
            For Each name As String In qsessions()
                html.AppendLine("<li>" & qh(name) & "</li>")
            Next name
            html.AppendLine("</ul></section><section class=""card""><h2>Workspaces</h2><ul>")
            For Each name As String In qworkspacenames()
                html.AppendLine("<li>" & qh(name) & "</li>")
            Next name
            html.AppendLine("</ul></section><section class=""card""><h2>Reading List</h2><ul>")
            For Each raw As String In My.Settings.rl
                Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
                If fields.Length = 4 Then html.AppendLine("<li>" & If(fields(0) = "unread", "● ", "✓ ") & "<a href=""" & qh(fields(3)) & """>" & qh(fields(2)) & "</a></li>")
            Next raw
            html.AppendLine("</ul></section><section class=""card""><h2>Recent Domains</h2><ul>")
            Dim domains As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For i As Integer = f6.l0.Items.Count - 1 To 0 Step -1
                Dim parsed As Uri = Nothing
                If Uri.TryCreate(f6.l0.Items(i).ToString, UriKind.Absolute, parsed) AndAlso domains.Add(parsed.Host) Then html.AppendLine("<li><a href=""" & qh(parsed.GetLeftPart(UriPartial.Authority)) & """>" & qh(parsed.Host) & "</a></li>")
                If domains.Count >= 12 Then Exit For
            Next i
            html.AppendLine("</ul></section><section class=""card""><h2>Local Archives</h2><ul>")
            For Each archive As ArchiveRecord In qarchiverecords().OrderByDescending(Function(record) record.DateSaved).Take(12)
                Dim target As String = If(System.IO.File.Exists(archive.HtmlPath), New Uri(archive.HtmlPath).AbsoluteUri, archive.Url)
                html.AppendLine("<li><a href=""" & qh(target) & """>" & qh(archive.Title) & "</a> · " & qh(archive.DateSaved) & "</li>")
            Next archive
            For Each archive As VersionedArchiveRecord In q7archiveversions().Take(12)
                Dim target As String = If(System.IO.File.Exists(archive.HtmlPath), New Uri(archive.HtmlPath).AbsoluteUri, archive.Url)
                html.AppendLine("<li>[version] <a href=""" & qh(target) & """>" & qh(archive.Title) & "</a> · " & qh(archive.DateSaved) & "</li>")
            Next archive
            html.AppendLine("</ul></section><section class=""card""><h2>Research Collections</h2><ul>")
            For Each collection As ResearchCollectionRecord In q7researchcollections().Take(12)
                Dim items As List(Of ResearchItemRecord) = q7researchitems(collection.Id)
                html.AppendLine("<li><strong>" & qh(collection.Name) & "</strong> · " & items.Count.ToString & " item(s)</li>")
                For Each item As ResearchItemRecord In items.Where(Function(value) qvurl(value.Url)).Take(3)
                    html.AppendLine("<li>↳ <a href=""" & qh(item.Url) & """>" & qh(item.Title) & "</a></li>")
                Next item
            Next collection
            html.AppendLine("</ul></section><section class=""card""><h2>Watchlist</h2><ul>")
            For Each item As Feature8WatchItem In q8watchitems().Where(Function(value) value.Unread).OrderByDescending(Function(value) value.Discovered).Take(12)
                html.AppendLine("<li>● <a href=""" & qh(item.Url) & """>" & qh(item.Title) & "</a></li>")
            Next item
            html.AppendLine("</ul></section></div></body></html>")
            Dim folder As String = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firespider Internet")
            System.IO.Directory.CreateDirectory(folder)
            Dim path As String = System.IO.Path.Combine(folder, "Dashboard.html")
            System.IO.File.WriteAllText(path, html.ToString, Encoding.UTF8)
            Return New Uri(path).AbsoluteUri
        Catch ex As Exception
            qlog(ex)
            Return My.Settings.h
        End Try
    End Function

    Sub q838(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim search As New UnifiedSearchForm(Me)
            search.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q839(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q838(sender, e)
    End Sub

    Friend Function qsearchresults(ByVal query As String) As List(Of BrowserSearchResult)
        Dim results As New List(Of BrowserSearchResult)
        query = query.Trim
        For i As Integer = 0 To k.TabPages.Count - 1
            If k.TabPages(i).Controls.Count = 0 Then Continue For
            Dim wb As WebBrowser = TryCast(k.TabPages(i).Controls(0), WebBrowser)
            If wb IsNot Nothing Then qaddsearch(results, query, "Open tab", k.TabPages(i).Text, qpersistedtaburl(k.TabPages(i), wb), i)
        Next i
        For Each raw As String In My.Settings.bm
            qaddsearch(results, query, "Bookmark", If(qbf(raw) = "", qbu(raw), qbf(raw)), qbu(raw), -1)
        Next raw
        For i As Integer = f6.l0.Items.Count - 1 To 0 Step -1
            qaddsearch(results, query, "History", f6.l0.Items(i).ToString, f6.l0.Items(i).ToString, -1)
        Next i
        For Each name As String In qsessions()
            qaddsearch(results, query, "Named session", name, name, -1)
        Next name
        For Each name As String In qworkspacenames()
            qaddsearch(results, query, "Workspace", name, name, -1)
        Next name
        For Each archive As ArchiveRecord In qarchiverecords()
            Dim target As String = If(System.IO.File.Exists(archive.HtmlPath), New Uri(archive.HtmlPath).AbsoluteUri, archive.Url)
            qaddsearch(results, query, "Local archive", archive.Title, target, -1)
        Next archive
        For Each archive As VersionedArchiveRecord In q7archiveversions()
            Dim target As String = If(System.IO.File.Exists(archive.HtmlPath), New Uri(archive.HtmlPath).AbsoluteUri, archive.Url)
            qaddsearch(results, query, "Archive version", archive.Title & " — " & archive.DateSaved, target, -1)
        Next archive
        For Each collection As ResearchCollectionRecord In q7researchcollections()
            qaddsearch(results, query, "Research collection", collection.Name, collection.Id, -1)
            For Each item As ResearchItemRecord In q7researchitems(collection.Id)
                If qvurl(item.Url) Then qaddsearch(results, query, "Research item", collection.Name & " — " & item.Title, item.Url, -1)
            Next item
        Next collection
        For Each item As Feature8WatchItem In q8watchitems()
            qaddsearch(results, query, "Watch item", item.Title, item.SourceId & "|" & item.ItemId, -1)
        Next item
        For Each path As String In My.Settings.dl
            qaddsearch(results, query, "Download", System.IO.Path.GetFileName(path), path, -1)
        Next path
        For Each line As String In My.Settings.qn.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
            qaddsearch(results, query, "Quick note", line.Trim, line.Trim, -1)
        Next line
        Return results
    End Function

    Sub qaddsearch(ByVal results As List(Of BrowserSearchResult), ByVal query As String, ByVal kind As String, ByVal label As String, ByVal value As String, ByVal tabIndex As Integer)
        If query = "" OrElse label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 OrElse value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 Then results.Add(New BrowserSearchResult(kind, label, value, tabIndex))
    End Sub

    Friend Sub qopensearchresult(ByVal result As BrowserSearchResult)
        Try
            If result.Kind = "Open tab" AndAlso result.TabIndex >= 0 AndAlso result.TabIndex < k.TabPages.Count Then
                k.SelectedIndex = result.TabIndex
            ElseIf result.Kind = "Named session" Then
                qopensession(result.Value)
            ElseIf result.Kind = "Workspace" Then
                qopenworkspace(result.Value)
            ElseIf result.Kind = "Research collection" Then
                Dim urls As List(Of String) = q7researchitems(result.Value).Select(Function(item) item.Url).Where(Function(url) qvurl(url)).Distinct(StringComparer.OrdinalIgnoreCase).ToList
                If urls.Count > 0 Then qlazyopenurls(urls)
            ElseIf result.Kind = "Watch item" Then
                Dim fields As String() = result.Value.Split(New Char() {"|"c}, 2)
                If fields.Length = 2 Then q8openwatchitem(fields(0), fields(1))
            ElseIf result.Kind = "Download" Then
                If System.IO.File.Exists(result.Value) OrElse System.IO.Directory.Exists(result.Value) Then Process.Start(result.Value)
            ElseIf result.Kind = "Quick note" Then
                Clipboard.SetText(result.Value)
                su.Visible = True
                su.Text = "Quick note copied"
            Else
                q1(result.Value)
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub q840(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            If wb.Url Is Nothing OrElse wb.Url.Host = "" Then Exit Sub
            Dim host As String = wb.Url.Host.ToLowerInvariant
            For i As Integer = My.Settings.dp.Count - 1 To 0 Step -1
                If My.Settings.dp(i).StartsWith(host & "|", StringComparison.OrdinalIgnoreCase) Then My.Settings.dp.RemoveAt(i)
            Next i
            My.Settings.dp.Add(host & "|" & My.Settings.zl & "|" & pts.ToString & "|" & If(contrastOn, "e", ""))
            My.Settings.Save()
            su.Visible = True
            su.Text = "Reading preferences saved for " & host
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q841(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q840(sender, e)
    End Sub
    Sub q842(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim host As String = wb.Url.Host
            For i As Integer = My.Settings.dp.Count - 1 To 0 Step -1
                If My.Settings.dp(i).StartsWith(host & "|", StringComparison.OrdinalIgnoreCase) Then My.Settings.dp.RemoveAt(i)
            Next i
            My.Settings.Save()
            su.Visible = True
            su.Text = "Reading preferences cleared for " & host
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q843(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q842(sender, e)
    End Sub

    Friend Sub qapplydomain(ByVal wb As WebBrowser)
        Try
            If wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing OrElse wb.Document.Body Is Nothing Then Exit Sub
            For Each raw As String In My.Settings.dp
                Dim fields As String() = raw.Split("|"c)
                If fields.Length >= 4 AndAlso fields(0).Equals(wb.Url.Host, StringComparison.OrdinalIgnoreCase) Then
                    Dim zoom As Integer = 100
                    Dim textSize As Integer = 100
                    Integer.TryParse(fields(1), zoom)
                    Integer.TryParse(fields(2), textSize)
                    wb.Document.Body.Style &= ";zoom:" & zoom.ToString & "%;font-size:" & textSize.ToString & "% !important;"
                    If fields(3) = "e" Then wb.Document.Body.Style &= ";background-color:#000000 !important;color:#ffffff !important;"
                    Exit For
                End If
            Next raw
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub q844(ByVal sender As Object, ByVal e As EventArgs)
        Dim folder As New FolderBrowserDialog
        folder.Description = "Choose the default folder for saved browser files"
        If System.IO.Directory.Exists(My.Settings.df) Then folder.SelectedPath = My.Settings.df
        If folder.ShowDialog(Me) = DialogResult.OK Then
            My.Settings.df = folder.SelectedPath
            My.Settings.Save()
            su.Visible = True
            su.Text = "Default save folder updated"
        End If
    End Sub
    Sub q845(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q844(sender, e)
    End Sub
    Sub q846(ByVal sender As Object, ByVal e As EventArgs)
        My.Settings.df = ""
        My.Settings.Save()
        su.Visible = True
        su.Text = "Default save folder reset"
    End Sub
    Sub q847(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q846(sender, e)
    End Sub
    Friend Sub qsavefolder(ByVal dialog As SaveFileDialog)
        If System.IO.Directory.Exists(My.Settings.df) Then dialog.InitialDirectory = My.Settings.df
    End Sub
    Friend Sub qsavefolder(ByVal dialog As FolderBrowserDialog)
        If System.IO.Directory.Exists(My.Settings.df) Then dialog.SelectedPath = My.Settings.df
    End Sub

    Sub q848(ByVal sender As Object, ByVal e As EventArgs)
        Dim current As String = String.Join(",", My.Settings.ce.Cast(Of String)().ToArray)
        Dim answer As String = InputBox("Enter categories separated by commas: history, suggestions, sessions, downloads, notes, watchlist, analytics, or all. Leave blank to disable.", "Clear Data On Exit", current).Trim.ToLowerInvariant
        My.Settings.ce.Clear()
        For Each value As String In answer.Split(","c)
            value = value.Trim
            If value <> "" AndAlso My.Settings.ce.Contains(value) = False Then My.Settings.ce.Add(value)
        Next value
        My.Settings.Save()
        su.Visible = True
        su.Text = If(My.Settings.ce.Count = 0, "Clear data on exit disabled", "Clear data on exit updated")
    End Sub
    Sub q849(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q848(sender, e)
    End Sub

    Sub q850(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            If wb.Url Is Nothing OrElse wb.Url.Host = "" Then Exit Sub
            Dim host As String = qdomain(wb.Url.Host)
            If My.Settings.bs.Contains(host) = False Then My.Settings.bs.Add(host)
            My.Settings.Save()
            su.Visible = True
            su.Text = host & " added to blocked sites"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q851(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q850(sender, e)
    End Sub
    Sub q852(ByVal sender As Object, ByVal e As EventArgs)
        Dim action As String = InputBox("Blocked sites:" & vbCrLf & String.Join(vbCrLf, My.Settings.bs.Cast(Of String)().ToArray) & vbCrLf & vbCrLf & "Enter a domain to remove, CLEAR to remove all, or a new domain to add:", "Manage Blocked Sites", "").Trim.ToLowerInvariant
        If action = "" Then Exit Sub
        If action = "clear" Then
            My.Settings.bs.Clear()
        Else
            action = qdomain(action)
            If My.Settings.bs.Contains(action) Then My.Settings.bs.Remove(action) Else My.Settings.bs.Add(action)
        End If
        My.Settings.Save()
        su.Visible = True
        su.Text = "Blocked sites updated"
    End Sub
    Sub q853(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q852(sender, e)
    End Sub

    Function qdomain(ByVal value As String) As String
        value = value.Trim.ToLowerInvariant
        Dim parsed As Uri = Nothing
        If Uri.TryCreate(value, UriKind.Absolute, parsed) Then value = parsed.Host
        If value.StartsWith("www.") Then value = value.Substring(4)
        Return value.Trim("."c)
    End Function

    Sub q854(ByVal sender As Object, ByVal e As EventArgs)
        My.Settings.popup = If(My.Settings.popup = "tab", "block", "tab")
        My.Settings.Save()
        su.Visible = True
        su.Text = "Pop-ups will be " & If(My.Settings.popup = "tab", "redirected to Firespider tabs", "blocked")
    End Sub
    Sub q855(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q854(sender, e)
    End Sub
    Sub q856(ByVal sender As Object, ByVal e As EventArgs)
        My.Settings.externalConfirm = If(My.Settings.externalConfirm = "e", "", "e")
        My.Settings.Save()
        su.Visible = True
        su.Text = "External protocol confirmation " & If(My.Settings.externalConfirm = "e", "enabled", "disabled")
    End Sub
    Sub q857(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q856(sender, e)
    End Sub

    Friend Function qnavigationblocked(ByVal e As WebBrowserNavigatingEventArgs) As Boolean
        Try
            If e.Url Is Nothing Then Return False
            If e.Url.Scheme = Uri.UriSchemeHttp OrElse e.Url.Scheme = Uri.UriSchemeHttps Then
                Dim host As String = qdomain(e.Url.Host)
                For Each blocked As String In My.Settings.bs
                    blocked = qdomain(blocked)
                    If blocked <> "" AndAlso (host.Equals(blocked, StringComparison.OrdinalIgnoreCase) OrElse host.EndsWith("." & blocked, StringComparison.OrdinalIgnoreCase)) Then
                        e.Cancel = True
                        su.Visible = True
                        su.Text = "Navigation blocked by blocked-sites list: " & blocked
                        Return True
                    End If
                Next blocked
                If qcontentnavigationblocked(e.Url) Then
                    e.Cancel = True
                    su.Visible = True
                    su.Text = "Navigation blocked by content filter: " & e.Url.Host
                    Return True
                End If
            End If
            Dim normal As Boolean = e.Url.Scheme = Uri.UriSchemeHttp OrElse e.Url.Scheme = Uri.UriSchemeHttps OrElse e.Url.Scheme = Uri.UriSchemeFile OrElse e.Url.Scheme = "about"
            Dim dangerousFile As Boolean = e.Url.Scheme = Uri.UriSchemeFile AndAlso New String() {".exe", ".msi", ".bat", ".cmd", ".com", ".scr"}.Contains(System.IO.Path.GetExtension(e.Url.LocalPath).ToLowerInvariant)
            If My.Settings.externalConfirm = "e" AndAlso (normal = False OrElse dangerousFile) Then
                If MessageBox.Show("Allow this external target?" & vbCrLf & e.Url.ToString, "External Protocol Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
                    e.Cancel = True
                    su.Visible = True
                    su.Text = "External target cancelled"
                    Return True
                End If
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
        Return False
    End Function

    Friend Sub qpopup(ByVal sender As Object, ByVal e As CancelEventArgs)
        Try
            e.Cancel = True
            Dim url As String = ""
            Dim wb As WebBrowser = TryCast(sender, WebBrowser)
            If wb IsNot Nothing Then
                url = wb.StatusText
                If qvurl(url) = False AndAlso wb.Document IsNot Nothing AndAlso wb.Document.ActiveElement IsNot Nothing Then url = wb.Document.ActiveElement.GetAttribute("href")
            End If
            If My.Settings.popup = "tab" AndAlso qvurl(url) Then
                q3(url)
                su.Text = "Pop-up redirected to a new Firespider tab"
            Else
                su.Text = "Pop-up blocked"
            End If
            su.Visible = True
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qrecoverytick(ByVal sender As Object, ByVal e As EventArgs)
        qrecoverysnapshot()
    End Sub
    Sub qrecoverysnapshot()
        Try
            If pv Then Exit Sub
            My.Settings.recovery.Clear()
            For Each url As String In qleaSavedUrls()
                My.Settings.recovery.Add(url)
            Next
            My.Settings.recoveryTime = Now.ToString("yyyy-MM-dd HH:mm:ss")
            My.Settings.Save()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qfeatureclosing()
        Try
            recoveryTimer.Stop()
            featureWindows = Math.Max(0, featureWindows - 1)
            lastWindowClosing = featureWindows = 0
            My.Settings.cleanExit = If(lastWindowClosing, "e", "")
            qfeature3closing(lastWindowClosing)
            If pv = False Then
                Dim urls As List(Of String) = qleaSavedUrls()
                My.Settings.recovery.Clear()
                If urls.Count > 0 Then
                    My.Settings.cwh.Insert(0, Now.ToString("yyyy-MM-dd HH:mm") & ControlChars.Tab & String.Join(ControlChars.Tab, urls.ToArray))
                    While My.Settings.cwh.Count > 5
                        My.Settings.cwh.RemoveAt(My.Settings.cwh.Count - 1)
                    End While
                    For Each url As String In urls
                        My.Settings.recovery.Add(url)
                    Next url
                End If
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qclearonexit()
        If lastWindowClosing = False OrElse My.Settings.ce.Count = 0 Then Exit Sub
        Dim all As Boolean = My.Settings.ce.Contains("all")
        If all OrElse My.Settings.ce.Contains("history") Then
            f6.l0.Items.Clear()
            c.Items.Clear()
        End If
        If all OrElse My.Settings.ce.Contains("suggestions") Then
            My.Settings.s.Clear()
            x.Items.Clear()
        End If
        If all OrElse My.Settings.ce.Contains("sessions") Then
            My.Settings.ls.Clear()
            My.Settings.ns.Clear()
            My.Settings.lcw.Clear()
            My.Settings.cwh.Clear()
            My.Settings.recovery.Clear()
        End If
        If all OrElse My.Settings.ce.Contains("downloads") Then
            My.Settings.dl.Clear()
            My.Settings.dq.Clear()
            My.Settings.downloadSafety.Clear()
        End If
        If all OrElse My.Settings.ce.Contains("notes") Then My.Settings.qn = ""
        If all OrElse My.Settings.ce.Contains("watchlist") Then My.Settings.watchItems.Clear()
        If all OrElse My.Settings.ce.Contains("analytics") Then My.Settings.analytics.Clear()
    End Sub

    Friend Function qopenurls(ByVal urls As List(Of String)) As Integer
        Return qleaRestoreUrls(urls, True)
    End Function
End Class

Friend Class FirespiderCommand
    Public Id As String
    Public Label As String
    Public Item As ToolStripMenuItem
    Sub New(ByVal commandId As String, ByVal commandLabel As String, ByVal commandItem As ToolStripMenuItem)
        Id = commandId
        Label = commandLabel
        Item = commandItem
    End Sub
    Public Overrides Function ToString() As String
        Return Label
    End Function
End Class

Friend Class CommandPaletteForm
    Inherits Form
    Dim ownerForm As j
    Dim searchBox As New TextBox
    Dim resultList As New ListBox
    Dim runButton As New Button
    Dim pinButton As New Button

    Sub New(ByVal browser As j)
        ownerForm = browser
        Text = "Firespider Command Palette"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(680, 470)
        MinimumSize = New Size(480, 320)
        searchBox.Dock = DockStyle.Top
        searchBox.Font = New Font("Segoe UI", 12.0!)
        searchBox.Height = 34
        resultList.Dock = DockStyle.Fill
        resultList.Font = New Font("Segoe UI", 10.0!)
        Dim bottom As New Panel
        bottom.Dock = DockStyle.Bottom
        bottom.Height = 44
        runButton.Text = "Run"
        runButton.Width = 100
        runButton.Dock = DockStyle.Right
        pinButton.Text = "Pin / Unpin"
        pinButton.Width = 120
        pinButton.Dock = DockStyle.Right
        bottom.Controls.Add(runButton)
        bottom.Controls.Add(pinButton)
        Controls.Add(resultList)
        Controls.Add(searchBox)
        Controls.Add(bottom)
        AddHandler searchBox.TextChanged, AddressOf qfilter
        AddHandler searchBox.KeyDown, AddressOf qkey
        AddHandler resultList.DoubleClick, AddressOf qrun
        AddHandler resultList.KeyDown, AddressOf qkey
        AddHandler runButton.Click, AddressOf qrun
        AddHandler pinButton.Click, AddressOf qpin
        qfilter(Nothing, EventArgs.Empty)
        AddHandler Shown, Sub() searchBox.Focus()
    End Sub
    Sub qfilter(ByVal sender As Object, ByVal e As EventArgs)
        Dim query As String = searchBox.Text.Trim
        Dim selected As Object = resultList.SelectedItem
        resultList.Items.Clear()
        For Each command As FirespiderCommand In ownerForm.qcommandlist()
            If query = "" OrElse command.Label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 Then resultList.Items.Add(command)
        Next command
        If resultList.Items.Count > 0 Then resultList.SelectedIndex = 0
    End Sub
    Sub qkey(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            qrun(sender, e)
            e.SuppressKeyPress = True
        ElseIf e.KeyCode = Keys.Down AndAlso sender Is searchBox AndAlso resultList.Items.Count > 0 Then
            resultList.Focus()
            resultList.SelectedIndex = Math.Min(resultList.Items.Count - 1, Math.Max(0, resultList.SelectedIndex))
            e.SuppressKeyPress = True
        ElseIf e.KeyCode = Keys.Escape Then
            Close()
        End If
    End Sub
    Sub qrun(ByVal sender As Object, ByVal e As EventArgs)
        Dim command As FirespiderCommand = TryCast(resultList.SelectedItem, FirespiderCommand)
        If command Is Nothing Then Exit Sub
        Close()
        command.Item.PerformClick()
    End Sub
    Sub qpin(ByVal sender As Object, ByVal e As EventArgs)
        Dim command As FirespiderCommand = TryCast(resultList.SelectedItem, FirespiderCommand)
        If command Is Nothing Then Exit Sub
        ownerForm.qtogglefavorite(command.Id)
    End Sub
End Class

Friend Class BrowserSearchResult
    Public Kind As String
    Public Label As String
    Public Value As String
    Public TabIndex As Integer
    Sub New(ByVal resultKind As String, ByVal resultLabel As String, ByVal resultValue As String, ByVal index As Integer)
        Kind = resultKind
        Label = resultLabel
        Value = resultValue
        TabIndex = index
    End Sub
    Public Overrides Function ToString() As String
        Return "[" & Kind & "] " & Label & If(Value = Label OrElse Value = "", "", " — " & Value)
    End Function
End Class

Friend Class UnifiedSearchForm
    Inherits Form
    Dim ownerForm As j
    Dim searchBox As New TextBox
    Dim resultList As New ListBox
    Dim openButton As New Button
    Sub New(ByVal browser As j)
        ownerForm = browser
        Text = "Unified Browser Search"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(760, 500)
        MinimumSize = New Size(520, 340)
        searchBox.Dock = DockStyle.Top
        searchBox.Font = New Font("Segoe UI", 12.0!)
        resultList.Dock = DockStyle.Fill
        resultList.Font = New Font("Segoe UI", 10.0!)
        openButton.Text = "Open Selected Result"
        openButton.Dock = DockStyle.Bottom
        openButton.Height = 38
        Controls.Add(resultList)
        Controls.Add(searchBox)
        Controls.Add(openButton)
        AddHandler searchBox.TextChanged, AddressOf qfilter
        AddHandler searchBox.KeyDown, AddressOf qkey
        AddHandler resultList.KeyDown, AddressOf qkey
        AddHandler resultList.DoubleClick, AddressOf qopen
        AddHandler openButton.Click, AddressOf qopen
        qfilter(Nothing, EventArgs.Empty)
        AddHandler Shown, Sub() searchBox.Focus()
    End Sub
    Sub qfilter(ByVal sender As Object, ByVal e As EventArgs)
        resultList.Items.Clear()
        For Each result As BrowserSearchResult In ownerForm.qsearchresults(searchBox.Text)
            resultList.Items.Add(result)
        Next result
        If resultList.Items.Count > 0 Then resultList.SelectedIndex = 0
    End Sub
    Sub qkey(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            qopen(sender, e)
            e.SuppressKeyPress = True
        ElseIf e.KeyCode = Keys.Escape Then
            Close()
        End If
    End Sub
    Sub qopen(ByVal sender As Object, ByVal e As EventArgs)
        Dim result As BrowserSearchResult = TryCast(resultList.SelectedItem, BrowserSearchResult)
        If result IsNot Nothing Then ownerForm.qopensearchresult(result)
    End Sub
End Class
