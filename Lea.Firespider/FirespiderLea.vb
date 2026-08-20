Imports System.IO
Imports System.Collections.Generic

Partial Class j
    Private qleaMenu As ToolStripMenuItem
    Private Shared qleaSessionRestored As Boolean

    Private Sub qleaInit(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If qleaMenu IsNot Nothing Then Return
        qleaMenu = New ToolStripMenuItem("New Lea Tab")
        qleaMenu.Image = My.Resources.F9
        qleaMenu.BackColor = Color.White
        qleaMenu.ForeColor = Color.Black
        qleaMenu.Padding = New Padding(0)
        qleaMenu.ShortcutKeys = Keys.Control Or Keys.Alt Or Keys.L
        AddHandler qleaMenu.Click, AddressOf qleaNewClick
        fl.DropDownItems.Insert(Math.Min(1, fl.DropDownItems.Count), qleaMenu)
        qleaWire()
        If Not qleaSessionRestored Then
            qleaSessionRestored = True
            qleaRestoreSession()
        End If
    End Sub

    Private Sub qleaWire()
        RemoveHandler b.Click, AddressOf a1
        RemoveHandler b.MouseHover, AddressOf a2
        RemoveHandler f.Click, AddressOf a3
        RemoveHandler f.MouseHover, AddressOf a4
        RemoveHandler h.Click, AddressOf a5
        RemoveHandler h.MouseHover, AddressOf a6
        RemoveHandler g.Click, AddressOf a7
        RemoveHandler g.MouseHover, AddressOf a8
        RemoveHandler r.Click, AddressOf a9
        RemoveHandler r.MouseHover, AddressOf b1
        RemoveHandler s.Click, AddressOf b3
        RemoveHandler s.MouseHover, AddressOf b4
        RemoveHandler BT.Click, AddressOf b5
        RemoveHandler BT.MouseHover, AddressOf b6
        RemoveHandler FT.Click, AddressOf b7
        RemoveHandler FT.MouseHover, AddressOf b8
        RemoveHandler HO.Click, AddressOf h6
        RemoveHandler HO.MouseHover, AddressOf h7
        RemoveHandler re.Click, AddressOf i3
        RemoveHandler re.MouseHover, AddressOf i4
        RemoveHandler st.Click, AddressOf i5
        RemoveHandler st.MouseHover, AddressOf i6
        RemoveHandler t.Tick, AddressOf tk
        RemoveHandler CT.Click, AddressOf h5
        RemoveHandler pt.Click, AddressOf e9
        RemoveHandler pt.MouseHover, AddressOf g1
        RemoveHandler c.KeyDown, AddressOf i9
        RemoveHandler x.KeyDown, AddressOf j1
        RemoveHandler OP.Click, AddressOf h1
        RemoveHandler OP.MouseHover, AddressOf h2
        RemoveHandler SP.Click, AddressOf h3
        RemoveHandler SP.MouseHover, AddressOf h4
        RemoveHandler SL.Click, AddressOf k4
        RemoveHandler Me.FormClosing, AddressOf q9

        AddHandler b.Click, AddressOf qleaBackClick
        AddHandler b.MouseHover, AddressOf qleaBackHover
        AddHandler f.Click, AddressOf qleaForwardClick
        AddHandler f.MouseHover, AddressOf qleaForwardHover
        AddHandler h.Click, AddressOf qleaHomeClick
        AddHandler h.MouseHover, AddressOf qleaHomeHover
        AddHandler g.Click, AddressOf qleaGoClick
        AddHandler g.MouseHover, AddressOf qleaGoHover
        AddHandler r.Click, AddressOf qleaReloadClick
        AddHandler r.MouseHover, AddressOf qleaReloadHover
        AddHandler s.Click, AddressOf qleaSearchClick
        AddHandler s.MouseHover, AddressOf qleaSearchHover
        AddHandler BT.Click, AddressOf qleaBackMenuClick
        AddHandler BT.MouseHover, AddressOf qleaBackMenuHover
        AddHandler FT.Click, AddressOf qleaForwardMenuClick
        AddHandler FT.MouseHover, AddressOf qleaForwardMenuHover
        AddHandler HO.Click, AddressOf qleaHomeMenuClick
        AddHandler HO.MouseHover, AddressOf qleaHomeMenuHover
        AddHandler re.Click, AddressOf qleaReloadMenuClick
        AddHandler re.MouseHover, AddressOf qleaReloadMenuHover
        AddHandler st.Click, AddressOf qleaStopClick
        AddHandler st.MouseHover, AddressOf qleaStopHover
        AddHandler t.Tick, AddressOf qleaTick
        AddHandler CT.Click, AddressOf qleaCloseMenuClick
        AddHandler pt.Click, AddressOf qleaSourceClick
        AddHandler pt.MouseHover, AddressOf qleaSourceHover
        AddHandler c.KeyDown, AddressOf qleaAddressKeyDown
        AddHandler x.KeyDown, AddressOf qleaSearchKeyDown
        AddHandler OP.Click, AddressOf qleaOpenFileClick
        AddHandler OP.MouseHover, AddressOf qleaOpenFileHover
        AddHandler SP.Click, AddressOf qleaSavePageClick
        AddHandler SP.MouseHover, AddressOf qleaSavePageHover
        AddHandler SL.Click, AddressOf qleaSendLinkClick
        AddHandler k.MouseDown, AddressOf qleaTabMouseDown
        AddHandler Me.FormClosing, AddressOf qleaClosing
    End Sub

    Private Function qleaAt() As Global.Lea.Win.V
        If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Return Nothing
        Dim host = TryCast(k.SelectedTab.Controls(0), Global.Lea.Win.V)
        If host IsNot Nothing Then host.Pv = pv
        Return host
    End Function

    Private Function qleaHost(ByVal tab As TabPage) As Global.Lea.Win.V
        If tab Is Nothing OrElse tab.Controls.Count = 0 Then Return Nothing
        Return TryCast(tab.Controls(0), Global.Lea.Win.V)
    End Function

    Private Function qleaDo(ByVal command As String, Optional ByVal value As String = "") As Boolean
        Dim host = qleaAt()
        If host Is Nothing Then Return False
        Select Case command
            Case "bk"
                host.Bk()
            Case "fw"
                host.Fw()
            Case "go"
                qleaOpen(host, value, False)
            Case "search"
                qleaOpen(host, value, True)
            Case "home"
                qleaLoad(host, qleaStart())
            Case "reload"
                If host.Busy Then host.St() Else qleaReload(host)
            Case "stop"
                host.St()
        End Select
        qleaState(host)
        Return True
    End Function

    Private Function qleaHover(ByVal command As String, Optional ByVal value As String = "") As Boolean
        If qleaAt() Is Nothing Then Return False
        If My.Settings.t = "e" Then qleaDo(command, value)
        Return True
    End Function

    Private Sub qleaNewClick(ByVal sender As Object, ByVal e As EventArgs)
        qleaNew(qleaStart())
    End Sub

    Private Function qleaNew(ByVal target As String) As Global.Lea.Win.V
        Dim tab As New TabPage("Lea")
        Dim host As New Global.Lea.Win.V()
        host.Pv = pv
        host.Dock = DockStyle.Fill
        tab.Tag = "lea"
        tab.Controls.Add(host)
        k.TabPages.Add(tab)
        k.SelectedTab = tab
        I += 1
        AddHandler host.Changed, AddressOf qleaChanged
        AddHandler host.Done, AddressOf qleaDone
        AddHandler host.Al, AddressOf qleaAlert

        Dim closeMenu As New ContextMenuStrip()
        Dim closeItem As New ToolStripMenuItem("Close Lea Tab")
        AddHandler closeItem.Click, Sub() qleaClose(tab)
        closeMenu.Items.Add(closeItem)
        host.ContextMenuStrip = closeMenu
        If String.IsNullOrWhiteSpace(target) Then target = qleaStart()
        If Visible Then qleaLoad(host, target) Else host.Go(target)
        Return host
    End Function

    Private Function qleaStart() As String
        Dim localPage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lea", "start.html")
        If File.Exists(localPage) Then Return localPage
        If Not String.IsNullOrWhiteSpace(My.Settings.h) Then Return My.Settings.h
        Return "https://example.com"
    End Function

    Private Async Sub qleaOpen(ByVal host As Global.Lea.Win.V, ByVal input As String, ByVal searchOnly As Boolean)
        Dim value = If(input, "").Trim()
        Dim aliasAddress As String = ""
        If qresolvealias(value, aliasAddress) Then value = aliasAddress
        If value.Length = 0 Then Return
        If searchOnly OrElse value.Contains(" ") OrElse Not value.Contains(".") Then
            value = My.Settings.se & Uri.EscapeDataString(value)
        ElseIf Not value.Contains("://") AndAlso Not value.StartsWith("about:") AndAlso Not value.StartsWith("file:") Then
            value = "http://" & value
        End If
        Await qleaLoadA(host, value)
    End Sub

    Private Async Sub qleaLoad(ByVal host As Global.Lea.Win.V, ByVal target As String)
        Await qleaLoadA(host, target)
    End Sub

    Private Async Function qleaLoadA(ByVal host As Global.Lea.Win.V, ByVal target As String) As Threading.Tasks.Task
        Try
            su.Visible = True
            su.Text = "Lea loading"
            r.Image = My.Resources.F7
            Await host.GoA(target)
        Catch ex As Exception
            su.Visible = True
            su.Text = "Lea: " & ex.Message
            r.Image = My.Resources.F8
        End Try
    End Function

    Private Async Sub qleaReload(ByVal host As Global.Lea.Win.V)
        Try
            su.Visible = True
            su.Text = "Lea loading"
            r.Image = My.Resources.F7
            Await host.RlA()
        Catch ex As Exception
            su.Visible = True
            su.Text = "Lea: " & ex.Message
            r.Image = My.Resources.F8
        End Try
    End Sub

    Private Sub qleaChanged(ByVal sender As Object, ByVal e As EventArgs)
        qleaState(TryCast(sender, Global.Lea.Win.V))
    End Sub

    Private Sub qleaDone(ByVal sender As Object, ByVal e As EventArgs)
        r.Image = My.Resources.F8
        su.Visible = False
        qleaState(TryCast(sender, Global.Lea.Win.V))
    End Sub

    Private Sub qleaAlert(ByVal sender As Object, ByVal e As EventArgs)
        Dim host = TryCast(sender, Global.Lea.Win.V)
        If host Is Nothing Then Return
        Dim message = If(host.Am, "")
        If message.Length > 160 Then message = message.Substring(0, 160)
        su.Visible = True
        su.Text = "Lea: " & message
    End Sub

    Private Sub qleaState(ByVal host As Global.Lea.Win.V)
        If host Is Nothing Then Return
        Dim tab = TryCast(host.Parent, TabPage)
        If tab IsNot Nothing Then tab.Text = If(String.IsNullOrWhiteSpace(host.T), "Lea", host.T)
        If k.SelectedTab IsNot tab Then Return
        c.Text = If(host.U Is Nothing, "", host.U.ToString())
        Text = ProductName & " - " & If(tab Is Nothing, "Lea", tab.Text)
        b.Enabled = host.CanBk
        BT.Enabled = host.CanBk
        f.Enabled = host.CanFw
        FT.Enabled = host.CanFw
    End Sub

    Private Sub qleaTabChanged(ByVal sender As Object, ByVal e As EventArgs) Handles k.SelectedIndexChanged
        Dim host = qleaAt()
        If host IsNot Nothing Then
            qleaState(host)
            Return
        End If
        If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Return
        Dim browser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
        If browser Is Nothing Then Return
        k.SelectedTab.Text = browser.DocumentTitle
        c.Text = If(browser.Url Is Nothing, "", browser.Url.ToString())
        b.Enabled = browser.CanGoBack
        BT.Enabled = browser.CanGoBack
        f.Enabled = browser.CanGoForward
        FT.Enabled = browser.CanGoForward
    End Sub

    Private Sub qleaTick(ByVal sender As Object, ByVal e As EventArgs)
        Dim host = qleaAt()
        If host Is Nothing Then
            tk(sender, e)
        Else
            su.Text = If(host.Busy, "Lea loading", "Lea ready")
            qleaState(host)
        End If
    End Sub

    Private Sub qleaCloseMenuClick(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            h5(sender, e)
            Return
        End If
        qleaClose(k.SelectedTab)
    End Sub

    Private Sub qleaTabMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If qleaAt() Is Nothing Then Return
        If e.Button = MouseButtons.Middle Then
            qleaClose(k.SelectedTab)
            Return
        End If
        If e.Button <> MouseButtons.Left OrElse k.SelectedIndex < 0 Then Return
        Dim tabRect = k.GetTabRect(k.SelectedIndex)
        Dim closeRect As New RectangleF(tabRect.Left + tm, tabRect.Top + tm, tabRect.Width - 4 * tm, tabRect.Height - 4 * tm)
        If e.X >= closeRect.Right - mx AndAlso e.X <= closeRect.Right AndAlso e.Y >= closeRect.Top AndAlso e.Y <= closeRect.Top + mx Then qleaClose(k.SelectedTab)
    End Sub

    Private Function qleaSourceText() As String
        Dim host = qleaAt()
        If host Is Nothing OrElse host.At Is Nothing Then Return ""
        Return If(host.H, "")
    End Function

    Private Sub qleaSourceClick(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            e9(sender, e)
            Return
        End If
        Dim source = qleaSourceText()
        If source.Length = 0 Then
            su.Visible = True
            su.Text = "No Lea page source to show"
            Return
        End If
        F3.Show()
        F3.RN.Text = source
        su.Visible = True
        su.Text = "Lea page source opened"
    End Sub

    Private Sub qleaSourceHover(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            g1(sender, e)
        ElseIf My.Settings.t = "e" Then
            qleaSourceClick(sender, e)
        End If
    End Sub

    Private Sub qleaAddressKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If qleaAt() Is Nothing Then
            i9(sender, e)
            Return
        End If
        If e.KeyCode <> Keys.Enter OrElse String.IsNullOrWhiteSpace(c.Text) Then Return
        qleaDo("go", c.Text)
        e.Handled = True
        e.SuppressKeyPress = True
    End Sub

    Private Sub qleaSearchKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If qleaAt() Is Nothing Then
            j1(sender, e)
            Return
        End If
        If e.KeyCode <> Keys.Enter OrElse String.IsNullOrWhiteSpace(x.Text) OrElse x.Text = "search" Then Return
        qleaDo("search", x.Text)
        e.Handled = True
        e.SuppressKeyPress = True
    End Sub

    Private Function qleaOpenPath(ByVal path As String) As Boolean
        Dim host = qleaAt()
        If host Is Nothing OrElse String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then Return False
        qleaLoad(host, System.IO.Path.GetFullPath(path))
        Return True
    End Function

    Private Sub qleaOpenFileClick(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            h1(sender, e)
            Return
        End If
        Using dialog As New OpenFileDialog()
            dialog.Title = "Open"
            dialog.CheckFileExists = True
            dialog.Filter = "Web pages (*.html;*.htm)|*.html;*.htm|Text files (*.txt)|*.txt|All files (*.*)|*.*"
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            If qleaOpenPath(dialog.FileName) Then
                qad(dialog.FileName)
                su.Visible = True
                su.Text = "Lea file opened"
            End If
        End Using
    End Sub

    Private Sub qleaOpenFileHover(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            h2(sender, e)
        ElseIf My.Settings.t = "e" Then
            qleaOpenFileClick(sender, e)
        End If
    End Sub

    Private Function qleaSafeName(ByVal title As String) As String
        Dim name = If(title, "").Trim()
        If name.Length = 0 Then name = "Lea page"
        For Each invalid As Char In Path.GetInvalidFileNameChars()
            name = name.Replace(invalid, "_"c)
        Next
        Return name
    End Function

    Private Function qleaSavePath(ByVal path As String) As Boolean
        Dim source = qleaSourceText()
        If source.Length = 0 OrElse String.IsNullOrWhiteSpace(path) Then Return False
        File.WriteAllText(path, source, New System.Text.UTF8Encoding(False))
        Return True
    End Function

    Private Sub qleaSavePageClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim host = qleaAt()
        If host Is Nothing Then
            h3(sender, e)
            Return
        End If
        Using dialog As New SaveFileDialog()
            qsavefolder(dialog)
            dialog.FileName = qleaSafeName(host.T) & ".html"
            dialog.Filter = "HTML (*.html)|*.html|All files (*.*)|*.*"
            dialog.CheckPathExists = True
            dialog.Title = "Save Lea Page"
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                If qleaSavePath(dialog.FileName) Then
                    qad(dialog.FileName)
                    su.Visible = True
                    su.Text = "Lea page saved"
                End If
            Catch ex As Exception
                su.Visible = True
                su.Text = "Lea: " & ex.Message
            End Try
        End Using
    End Sub

    Private Sub qleaSavePageHover(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            h4(sender, e)
        ElseIf My.Settings.t = "e" Then
            qleaSavePageClick(sender, e)
        End If
    End Sub

    Private Function qleaMailLink() As String
        Dim host = qleaAt()
        If host Is Nothing OrElse host.U Is Nothing Then Return ""
        Return "mailto:?subject=" & Uri.EscapeDataString(If(host.T, "")) & "&body=" & Uri.EscapeDataString(host.U.AbsoluteUri)
    End Function

    Private Sub qleaSendLinkClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim link = qleaMailLink()
        If link.Length = 0 Then
            k4(sender, e)
            Return
        End If
        Try
            Process.Start(link)
            su.Visible = True
            su.Text = "Send Lea link opened"
        Catch ex As Exception
            su.Visible = True
            su.Text = "Lea: " & ex.Message
        End Try
    End Sub

    Private Function qleaSessionPath() As String
        Dim custom = Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION")
        If Not String.IsNullOrWhiteSpace(custom) Then Return custom
        Dim folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firespider Internet")
        Return Path.Combine(folder, "Lea.session")
    End Function

    Private Sub qleaSaveSession()
        Dim sessionFile = qleaSessionPath()
        If pv Then
            If File.Exists(sessionFile) Then File.Delete(sessionFile)
            Return
        End If
        Dim urls As New List(Of String)()
        For Each tab As TabPage In k.TabPages
            Dim host = qleaHost(tab)
            If host IsNot Nothing AndAlso host.U IsNot Nothing Then urls.Add(host.U.AbsoluteUri)
        Next
        If urls.Count = 0 Then
            If File.Exists(sessionFile) Then File.Delete(sessionFile)
            Return
        End If
        Dim folder = System.IO.Path.GetDirectoryName(sessionFile)
        If Not String.IsNullOrWhiteSpace(folder) Then Directory.CreateDirectory(folder)
        File.WriteAllLines(sessionFile, urls.ToArray(), New System.Text.UTF8Encoding(False))
    End Sub

    Private Sub qleaRestoreSession()
        If pv Then Return
        Dim path = qleaSessionPath()
        If Not File.Exists(path) Then Return
        Dim restored = 0
        For Each line In File.ReadAllLines(path)
            If restored >= 12 Then Exit For
            Dim target = If(line, "").Trim()
            Dim address As Uri = Nothing
            If Not Uri.TryCreate(target, UriKind.Absolute, address) Then Continue For
            If address.Scheme <> Uri.UriSchemeHttp AndAlso address.Scheme <> Uri.UriSchemeHttps AndAlso address.Scheme <> Uri.UriSchemeFile Then Continue For
            qleaNew(address.AbsoluteUri)
            restored += 1
        Next
        If restored > 0 Then
            su.Visible = True
            su.Text = "Restored " & restored.ToString() & " Lea tab(s)"
        End If
    End Sub

    Private Sub qleaClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs)
        Try
            qleaSaveSession()
            qfeatureclosing()
            If pv = False Then
                My.Settings.ls.Clear()
                My.Settings.lcw.Clear()
                For Each tab As TabPage In k.TabPages
                    If tab.Controls.Count = 0 Then Continue For
                    Dim browser = TryCast(tab.Controls(0), WebBrowser)
                    If browser Is Nothing Then Continue For
                    Dim savedUrl As String = qpersistedtaburl(tab, browser)
                    If savedUrl <> "" Then
                        My.Settings.ls.Add(savedUrl)
                        My.Settings.lcw.Add(savedUrl)
                    End If
                Next
            End If
            If My.Settings.rws = "e" And FormBorderStyle <> FormBorderStyle.None Then
                Dim savedBounds As Rectangle = If(WindowState = FormWindowState.Normal, Bounds, RestoreBounds)
                My.Settings.wl = savedBounds.Left.ToString()
                My.Settings.wt = savedBounds.Top.ToString()
                My.Settings.ww = savedBounds.Width.ToString()
                My.Settings.wh = savedBounds.Height.ToString()
                My.Settings.ws = If(WindowState = FormWindowState.Maximized, "Maximized", "Normal")
            End If
            qclearonexit()
            My.Settings.Save()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Private Sub qleaClose(ByVal tab As TabPage)
        If tab Is Nothing OrElse Not k.TabPages.Contains(tab) Then Return
        If k.TabCount = 1 Then
            Close()
            Return
        End If
        k.TabPages.Remove(tab)
        tab.Dispose()
        I = Math.Max(0, I - 1)
        qleaTabChanged(k, EventArgs.Empty)
    End Sub

    Private Sub qleaBackClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("bk") Then a1(s, e)
    End Sub
    Private Sub qleaBackHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("bk") Then a2(s, e)
    End Sub
    Private Sub qleaForwardClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("fw") Then a3(s, e)
    End Sub
    Private Sub qleaForwardHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("fw") Then a4(s, e)
    End Sub
    Private Sub qleaHomeClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("home") Then a5(s, e)
    End Sub
    Private Sub qleaHomeHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("home") Then a6(s, e)
    End Sub
    Private Sub qleaGoClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("go", c.Text) Then a7(s, e)
    End Sub
    Private Sub qleaGoHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("go", c.Text) Then a8(s, e)
    End Sub
    Private Sub qleaSearchClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("search", x.Text) Then b3(s, e)
    End Sub
    Private Sub qleaSearchHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("search", x.Text) Then b4(s, e)
    End Sub
    Private Sub qleaReloadClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("reload") Then a9(s, e)
    End Sub
    Private Sub qleaReloadHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("reload") Then b1(s, e)
    End Sub
    Private Sub qleaBackMenuClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("bk") Then b5(s, e)
    End Sub
    Private Sub qleaBackMenuHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("bk") Then b6(s, e)
    End Sub
    Private Sub qleaForwardMenuClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("fw") Then b7(s, e)
    End Sub
    Private Sub qleaForwardMenuHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("fw") Then b8(s, e)
    End Sub
    Private Sub qleaHomeMenuClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("home") Then h6(s, e)
    End Sub
    Private Sub qleaHomeMenuHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("home") Then h7(s, e)
    End Sub
    Private Sub qleaReloadMenuClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("reload") Then i3(s, e)
    End Sub
    Private Sub qleaReloadMenuHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("reload") Then i4(s, e)
    End Sub
    Private Sub qleaStopClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("stop") Then i5(s, e)
    End Sub
    Private Sub qleaStopHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("stop") Then i6(s, e)
    End Sub
End Class
