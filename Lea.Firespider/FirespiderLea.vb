Imports System.IO

Partial Class j
    Private qleaMenu As ToolStripMenuItem

    Private Sub qleaInit(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        qleaMenu = New ToolStripMenuItem("New Lea Tab")
        qleaMenu.Image = My.Resources.F9
        qleaMenu.BackColor = Color.White
        qleaMenu.ForeColor = Color.Black
        qleaMenu.Padding = New Padding(0)
        qleaMenu.ShortcutKeys = Keys.Control Or Keys.Alt Or Keys.L
        AddHandler qleaMenu.Click, AddressOf qleaNewClick
        fl.DropDownItems.Insert(Math.Min(1, fl.DropDownItems.Count), qleaMenu)
        qleaWire()
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
    End Sub

    Private Function qleaAt() As Global.Lea.Win.V
        If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Return Nothing
        Return TryCast(k.SelectedTab.Controls(0), Global.Lea.Win.V)
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
        Dim tab As New TabPage("Lea")
        Dim host As New Global.Lea.Win.V()
        host.Dock = DockStyle.Fill
        tab.Tag = "lea"
        tab.Controls.Add(host)
        k.TabPages.Add(tab)
        k.SelectedTab = tab
        I += 1
        AddHandler host.Changed, AddressOf qleaChanged
        AddHandler host.Done, AddressOf qleaDone

        Dim closeMenu As New ContextMenuStrip()
        Dim closeItem As New ToolStripMenuItem("Close Lea Tab")
        AddHandler closeItem.Click, Sub() qleaClose(tab)
        closeMenu.Items.Add(closeItem)
        host.ContextMenuStrip = closeMenu
        If Visible Then qleaLoad(host, qleaStart()) Else host.Go(qleaStart())
    End Sub

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

    Private Sub qleaClose(ByVal tab As TabPage)
        If tab Is Nothing OrElse Not k.TabPages.Contains(tab) Then Return
        k.TabPages.Remove(tab)
        tab.Dispose()
        I = Math.Max(0, I - 1)
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
