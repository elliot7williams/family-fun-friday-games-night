Imports System.Drawing
Partial Class j
    Dim Int As Long = 199979989911L
    Dim I As Integer = 0
    Dim z1 = CreateObject("SAPI.spvoice")
    Public mx As Integer = 8
    Public Const tm As Integer = 3
    Dim Draw As Boolean
    Dim Dcolo As Color = Color.Black
    Dim Dra As Integer = 7
    Dim Bmp As Bitmap
    Dim lu As String = ""
    Dim pv As Boolean = False
    Dim wo As Boolean = False
    Dim tl As ToolStripMenuItem
    Dim pts As Integer = 100
    Sub a1(ByVal sender As System.Object, ByVal e As EventArgs) Handles b.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoBack()
    End Sub
    Sub a2(ByVal sender As Object, ByVal e As EventArgs) Handles b.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoBack()
        End If
    End Sub
    Sub a3(ByVal sender As System.Object, ByVal e As EventArgs) Handles f.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoForward()
    End Sub
    Sub a4(ByVal sender As Object, ByVal e As EventArgs) Handles f.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoForward()
        End If
    End Sub
    Sub a5(ByVal sender As System.Object, ByVal e As EventArgs) Handles h.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(My.Settings.h)
    End Sub
    Sub a6(ByVal sender As Object, ByVal e As EventArgs) Handles h.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(My.Settings.h)
        End If
    End Sub
    Sub a7(ByVal sender As System.Object, ByVal e As EventArgs) Handles g.Click
        q1(c.Text)
    End Sub
    Sub a8(ByVal sender As Object, ByVal e As EventArgs) Handles g.MouseHover
        If My.Settings.t = "e" Then
            q1(c.Text)
        End If
    End Sub
    Sub a9(ByVal sender As System.Object, ByVal e As EventArgs) Handles r.Click
        If r.Image Is My.Resources.F7 Then
            r.Image = My.Resources.F8
        End If
        If r.Image Is My.Resources.F8 Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Refresh()
        Else
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Stop()
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).ScriptErrorsSuppressed = True
            On Error Resume Next
            LO.Size = New Size(1, 2)
            r.Image = My.Resources.F8
            su.Visible = False
            k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Text = ProductName & " - " & k.SelectedTab.Text
            c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
            f6.l0.Items.Add(c.Text)
            c.Items.Add(c.Text)
            If CType(k.SelectedTab.Controls.Item(0), WebBrowser).CanGoBack Then
                b.Enabled = True
                BT.Enabled = True
            Else
                b.Enabled = False
                BT.Enabled = False
            End If
            If CType(k.SelectedTab.Controls.Item(0), WebBrowser).CanGoForward Then
                f.Enabled = True
                FT.Enabled = True
            Else
                f.Enabled = False
                FT.Enabled = False
            End If
        End If
    End Sub
    Sub b1(ByVal sender As Object, ByVal e As EventArgs) Handles r.MouseHover
        If My.Settings.t = "e" Then
            If r.Image Is My.Resources.F7 Then
                r.Image = My.Resources.F8
            End If
            If r.Image Is My.Resources.F8 Then
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Refresh()
            Else
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Stop()
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).ScriptErrorsSuppressed = True
                On Error Resume Next
                LO.Size = New Size(1, 2)
                r.Image = My.Resources.F8
                su.Visible = False
                k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
                Text = ProductName & " - " & k.SelectedTab.Text
                c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
                f6.l0.Items.Add(c.Text)
                c.Items.Add(c.Text)
                If CType(k.SelectedTab.Controls.Item(0), WebBrowser).CanGoBack Then
                    b.Enabled = True
                    BT.Enabled = True
                Else
                    b.Enabled = False
                    BT.Enabled = False
                End If
                If CType(k.SelectedTab.Controls.Item(0), WebBrowser).CanGoForward Then
                    f.Enabled = True
                    FT.Enabled = True
                Else
                    f.Enabled = False
                    FT.Enabled = False
                End If
            End If
        End If
    End Sub
    Sub b3(ByVal sender As System.Object, ByVal e As EventArgs) Handles s.Click
        q2()
    End Sub
    Sub b4(ByVal sender As Object, ByVal e As EventArgs) Handles s.MouseHover
        Try
            If My.Settings.t = "e" Then
                q2()
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub b5(ByVal sender As System.Object, ByVal e As EventArgs) Handles BT.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoBack()
    End Sub
    Sub b6(ByVal sender As Object, ByVal e As EventArgs) Handles BT.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoBack()
        End If
    End Sub
    Sub b7(ByVal sender As System.Object, ByVal e As EventArgs) Handles FT.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoForward()
    End Sub
    Sub b8(ByVal sender As Object, ByVal e As EventArgs) Handles FT.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoForward()
        End If
    End Sub
    Sub b9(ByVal sender As System.Object, ByVal e As EventArgs) Handles ET.Click
        End
    End Sub
    Sub c1(ByVal sender As Object, ByVal e As EventArgs) Handles ET.MouseHover
        If My.Settings.t = "e" Then
            End
        End If
    End Sub
    Sub c2(ByVal sender As System.Object, ByVal e As EventArgs) Handles CW.Click
        Close()
    End Sub
    Sub c3(ByVal sender As System.Object, ByVal e As EventArgs) Handles NW.Click
        Dim f As New j
        f.Show()
    End Sub
    Sub c4(ByVal sender As Object, ByVal e As EventArgs) Handles NW.MouseHover
        If My.Settings.t = "e" Then
            Dim f As New j
            f.Show()
        End If
    End Sub
    Sub c5(ByVal sender As System.Object, ByVal e As EventArgs) Handles U.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Undo", True, vbNull)
    End Sub
    Sub c6(ByVal sender As Object, ByVal e As EventArgs) Handles U.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Undo", True, vbNull)
        End If
    End Sub
    Sub c7(ByVal sender As System.Object, ByVal e As EventArgs) Handles RD.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Redo", True, vbNull)
    End Sub
    Sub c8(ByVal sender As Object, ByVal e As EventArgs) Handles RD.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Redo", True, vbNull)
        End If
    End Sub
    Sub c9(ByVal sender As System.Object, ByVal e As EventArgs) Handles CO.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Copy", True, vbNull)
    End Sub
    Sub d1(ByVal sender As Object, ByVal e As EventArgs) Handles CO.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Copy", True, vbNull)
        End If
    End Sub
    Sub d2(ByVal sender As System.Object, ByVal e As EventArgs) Handles CU.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Cut", True, vbNull)
    End Sub
    Sub d3(ByVal sender As Object, ByVal e As EventArgs) Handles CU.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Cut", True, vbNull)
        End If
    End Sub
    Sub d4(ByVal sender As System.Object, ByVal e As EventArgs) Handles PA.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Paste", True, vbNull)
    End Sub
    Sub d5(ByVal sender As Object, ByVal e As EventArgs) Handles PA.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Paste", True, vbNull)
        End If
    End Sub
    Sub d6(ByVal sender As System.Object, ByVal e As EventArgs) Handles SA.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("SelectAll", True, vbNull)
    End Sub
    Sub d7(ByVal sender As Object, ByVal e As EventArgs) Handles SA.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("SelectAll", True, vbNull)
        End If
    End Sub
    Sub d8(ByVal sender As System.Object, ByVal e As EventArgs) Handles D.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Delete", True, vbNull)
    End Sub
    Sub d9(ByVal sender As Object, ByVal e As EventArgs) Handles D.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.ExecCommand("Delete", True, vbNull)
        End If
    End Sub
    Sub e1(ByVal sender As System.Object, ByVal e As EventArgs) Handles P.Click
        If qpr() Then Return
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).ShowPrintDialog()
    End Sub
    Sub e2(ByVal sender As Object, ByVal e As EventArgs) Handles P.MouseHover
        If My.Settings.t = "e" Then
            If qpr() Then Return
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).ShowPrintDialog()
        End If
    End Sub
    Sub e3(ByVal sender As System.Object, ByVal e As EventArgs) Handles PP.Click
        If qpv() Then Return
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).ShowPrintPreviewDialog()
    End Sub
    Sub e4(ByVal sender As Object, ByVal e As EventArgs) Handles PP.MouseHover
        If My.Settings.t = "e" Then
            If qpv() Then Return
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).ShowPrintPreviewDialog()
        End If
    End Sub
    Sub e5(ByVal sender As System.Object, ByVal e As EventArgs) Handles PS.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).ShowPageSetupDialog()
    End Sub
    Sub e6(ByVal sender As Object, ByVal e As EventArgs) Handles PS.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).ShowPageSetupDialog()
        End If
    End Sub
    Sub e7(ByVal sender As System.Object, ByVal e As EventArgs) Handles A.Click
        f2.Show()
    End Sub
    Sub e8(ByVal sender As Object, ByVal e As EventArgs) Handles A.MouseHover
        If My.Settings.t = "e" Then
            f2.Show()
        End If
    End Sub
    Sub e9(ByVal sender As System.Object, ByVal e As EventArgs) Handles pt.Click
        F3.Show()
        F3.RN.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentText
    End Sub
    Sub g1(ByVal sender As Object, ByVal e As EventArgs) Handles pt.MouseHover
        If My.Settings.t = "e" Then
            F3.Show()
            F3.RN.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentText
        End If
    End Sub
    Sub g2(ByVal sender As System.Object, ByVal e As EventArgs) Handles O.Click
        F4.Show()
    End Sub
    Sub g3(ByVal sender As Object, ByVal e As EventArgs) Handles O.MouseHover
        If My.Settings.t = "e" Then
            F4.Show()
        End If
    End Sub
    Sub g5(ByVal sender As System.Object, ByVal e As EventArgs) Handles HT.Click
        qleaNew("http://firespiderinternet.weebly.com/help.html")
    End Sub
    Sub f5(ByVal sender As Object, ByVal e As EventArgs) Handles HT.MouseHover
        If My.Settings.t = "e" Then
            g5(sender, e)
        End If
    End Sub
    Sub g6(ByVal sender As System.Object, ByVal e As EventArgs) Handles SH.Click
        f6.Show()
    End Sub
    Sub g7(ByVal sender As Object, ByVal e As EventArgs) Handles SH.MouseHover
        If My.Settings.t = "e" Then
            f6.Show()
        End If
    End Sub
    Sub g8(ByVal sender As System.Object, ByVal e As EventArgs) Handles SB.Click
        F7.Show()
    End Sub
    Sub g9(ByVal sender As Object, ByVal e As EventArgs) Handles SB.MouseHover
        If My.Settings.t = "e" Then
            F7.Show()
        End If
    End Sub
    Sub h1(ByVal sender As System.Object, ByVal e As EventArgs) Handles OP.Click
        Dim ox As New OpenFileDialog()
        ox.Title = "Open"
        ox.CheckFileExists = True
        ox.Filter = "All Files (*.*)|*.*"
        ox.ShowDialog()
        Try
            If ox.FileName = "" Then Exit Sub
            qad(ox.FileName)
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(ox.FileName)
        Catch ex As Exception
        End Try
    End Sub
    Sub h2(ByVal sender As Object, ByVal e As EventArgs) Handles OP.MouseHover
        If My.Settings.t = "e" Then
            Dim ox As New OpenFileDialog()
            ox.Title = "Open"
            ox.CheckFileExists = True
            ox.Filter = "All Files (*.*)|*.*"
            ox.ShowDialog()
            Try
                If ox.FileName = "" Then Exit Sub
                qad(ox.FileName)
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(ox.FileName)
            Catch ex As Exception
            End Try
        End If
    End Sub
    Sub h3(ByVal sender As System.Object, ByVal e As EventArgs) Handles SP.Click
        Dim SX As New SaveFileDialog()
            qsavefolder(SX)
        Dim mySW As System.IO.StreamWriter
        SX.FileName = (CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle)
        SX.Filter = "HTML [*.html*]|*.html|All Files [*.*]|*.*"
        SX.CheckPathExists = True
        SX.Title = "Save Page"
        SX.ShowDialog(Me)
        Try
            mySW = System.IO.File.AppendText(SX.FileName)
            mySW.Write(CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document)
            mySW.Flush()
            mySW.Close()
            qad(SX.FileName)
        Catch ex As Exception
        End Try
    End Sub
    Sub h4(ByVal sender As Object, ByVal e As EventArgs) Handles SP.MouseHover
        If My.Settings.t = "e" Then
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            Dim mySW As System.IO.StreamWriter
            SX.FileName = (CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle)
            SX.Filter = "HTML [*.html*]|*.html|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page"
            SX.ShowDialog(Me)
            Try
                mySW = System.IO.File.AppendText(SX.FileName)
                mySW.Write(CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document)
                mySW.Flush()
                mySW.Close()
                qad(SX.FileName)
            Catch ex As Exception
            End Try
        End If
    End Sub
    Sub nc(ByVal sender As System.Object, ByVal e As EventArgs) Handles N.Click
        Dim B As New WebBrowser
        k.TabPages.Add("New Tab")
        k.SelectTab(I)
        B.Dock = DockStyle.Fill
        k.SelectedTab.Controls.Add(B)
        AddHandler B.Navigating, AddressOf nn
        AddHandler B.Navigated, AddressOf nv
        AddHandler B.DocumentCompleted, AddressOf n0
        AddHandler B.ProgressChanged, AddressOf q14
        AddHandler B.NewWindow, AddressOf qpopup
        I = I + 1
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(qntu())
        Dim tz As Size = k.ItemSize
        If I <= 20 Then
            tz.Width -= 6
        ElseIf I >= 30 Then
            tz.Width += 8
        End If
        k.ItemSize = tz
    End Sub
    Sub nh(ByVal sender As Object, ByVal e As EventArgs) Handles N.MouseHover
        If My.Settings.t = "e" Then
            Dim B As New WebBrowser
            k.TabPages.Add("New Tab")
            k.SelectTab(I)
            B.Dock = DockStyle.Fill
            k.SelectedTab.Controls.Add(B)
            AddHandler B.Navigating, AddressOf nn
            AddHandler B.Navigated, AddressOf nv
            AddHandler B.DocumentCompleted, AddressOf n0
            AddHandler B.ProgressChanged, AddressOf q14
        AddHandler B.NewWindow, AddressOf qpopup
            I = I + 1
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(qntu())
            Dim tz As Size = k.ItemSize
            If I <= 20 Then
                tz.Width -= 6
            ElseIf I >= 30 Then
                tz.Width += 8
            End If
            k.ItemSize = tz
        End If
    End Sub
    Sub h5(ByVal sender As System.Object, ByVal e As EventArgs) Handles CT.Click
        If k.TabCount = 1 = False Then
            rca(CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
            k.TabPages.RemoveAt(k.SelectedIndex)
            I = I - 1
            Dim tz As Size = k.ItemSize
            tz.Width += 6
            k.ItemSize = tz
            k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Text = ProductName & " - " & k.SelectedTab.Text
            c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
        Else
            End
        End If
    End Sub
    Sub h6(ByVal sender As System.Object, ByVal e As EventArgs) Handles HO.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoHome()
    End Sub
    Sub h7(ByVal sender As Object, ByVal e As EventArgs) Handles HO.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoHome()
        End If
    End Sub
    Sub h8(ByVal sender As System.Object, ByVal e As EventArgs) Handles PB.Click
        Dim f As New j
        f.Show()
        f.Close()
        c.Items.Clear()
        f6.l0.Items.Clear()
        My.Settings.b0 = "f"
    End Sub
    Sub h9(ByVal sender As Object, ByVal e As EventArgs) Handles PB.MouseHover
        If My.Settings.t = "e" Then
            Dim f As New j
            f.Show()
            f.Close()
            c.Items.Clear()
            f6.l0.Items.Clear()
            My.Settings.b0 = "f"
        End If
    End Sub
    Sub i1(ByVal sender As System.Object, ByVal e As EventArgs) Handles CR.Click
        If MessageBox.Show("Clear all visible browsing history?", "Clear History", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
        c.Items.Clear()
        f6.l0.Items.Clear()
        su.Visible = True
        su.Text = "History cleared"
    End Sub
    Sub i2(ByVal sender As Object, ByVal e As EventArgs) Handles CR.MouseHover
        If My.Settings.t = "e" Then
            i1(sender, e)
        End If
    End Sub
    Sub i3(ByVal sender As System.Object, ByVal e As EventArgs) Handles re.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Refresh()
    End Sub
    Sub i4(ByVal sender As Object, ByVal e As EventArgs) Handles re.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Refresh()
        End If
    End Sub
    Sub i5(ByVal sender As System.Object, ByVal e As EventArgs) Handles st.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).Stop()
    End Sub
    Sub i6(ByVal sender As Object, ByVal e As EventArgs) Handles st.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Stop()
        End If
    End Sub
    Sub i7(ByVal sender As System.Object, ByVal e As EventArgs) Handles fs.Click
        If TopMost = False Then
            m.Visible = False
            k.Dock = DockStyle.Fill
            TopMost = True
            WindowState = FormWindowState.Normal
            FormBorderStyle = FormBorderStyle.None
            WindowState = FormWindowState.Maximized
            ShowIcon = False
            y2.Image = My.Resources.O0
            q.AutoSize = True
        Else
            m.Visible = True
            k.Dock = DockStyle.None
            TopMost = False
            FormBorderStyle = FormBorderStyle.Sizable
            WindowState = FormWindowState.Maximized
            ShowIcon = False
            y2.Image = My.Resources.F1
        End If
    End Sub
    Sub i8(ByVal sender As Object, ByVal e As EventArgs) Handles fs.MouseHover
        If My.Settings.t = "e" Then
            If TopMost = False Then
                m.Visible = False
                k.Dock = DockStyle.Fill
                TopMost = True
                WindowState = FormWindowState.Normal
                FormBorderStyle = FormBorderStyle.None
                WindowState = FormWindowState.Maximized
                ShowIcon = False
                y2.Image = My.Resources.O0
                q.AutoSize = True
            Else
                m.Visible = True
                k.Dock = DockStyle.None
                TopMost = False
                FormBorderStyle = FormBorderStyle.Sizable
                WindowState = FormWindowState.Maximized
                ShowIcon = False
                y2.Image = My.Resources.F1
            End If
        End If
    End Sub
    Sub i9(ByVal sender As Object, ByVal e As KeyEventArgs) Handles c.KeyDown
        If c.Text = "" Then
        Else
            If e.KeyCode = Keys.Enter Then
                q1(c.Text)
            End If
        End If
    End Sub
    Sub j1(ByVal sender As Object, ByVal e As KeyEventArgs) Handles x.KeyDown
        If x.Text = "search" Then
            x.Text = ""
        End If
        If x.Text = "" Then
        Else
            If e.KeyCode = Keys.Enter Then
                If x.Text = "search" Then
                Else
                    q2()
                End If
            End If
        End If
    End Sub
    Sub xc(ByVal sender As System.Object, ByVal e As EventArgs) Handles x.Click
        If x.Text = "search" Then
            x.Text = ""
            s.Enabled = False
        ElseIf x.Text = "" Then
            x.Text = "search"
            s.Enabled = False
        End If
    End Sub
    Sub tk(ByVal sender As System.Object, ByVal e As EventArgs) Handles t.Tick
        If k Is Nothing OrElse k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Exit Sub
        Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls.Item(0), WebBrowser)
        If wb IsNot Nothing Then su.Text = wb.StatusText
    End Sub
    Dim it As Integer = 0
    Sub pc(ByVal sender As System.Object, ByVal e As EventArgs) Handles BP.Click
        Try
            Dim url = qleaUrl(qleaSelectedTab())
            If url = "" Then Exit Sub
            My.Settings.bm.Add(url)
            My.Settings.Save()
            qrb()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub z2(ByVal sender As ToolStripMenuItem, ByVal ByVale As EventArgs)
        Try
            If sender.Tag Is Nothing Then Exit Sub
            q1(sender.Tag.ToString)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub jl(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Dim menuLayouts As New List(Of ToolStrip)()
        Try
            qleaMark("load-enter")
            mf = Me
            u8(mf)
            qawl()
            If My.Settings.ps = "e" Then pv = True
            qfeaturestartup()
            qleaMark("startup-settings")
            If My.Settings.i = Color.Black Then
            Else
                q.BackgroundImage = Nothing
                q.BackColor = My.Settings.i
                fl.BackgroundImage = Nothing
                fl.BackColor = My.Settings.i
                BackgroundImage = Nothing
                BackColor = My.Settings.i
                v.BackgroundImage = Nothing
                v.BackColor = My.Settings.i
                hi.BackgroundImage = Nothing
                hi.BackColor = My.Settings.i
                bo.BackgroundImage = Nothing
                bo.BackColor = My.Settings.i
                ot.BackgroundImage = Nothing
                ot.BackColor = My.Settings.i
                he.BackgroundImage = Nothing
                he.BackColor = My.Settings.i
                c.BackColor = My.Settings.i
                x.BackColor = My.Settings.i
            End If
            If My.Settings.ps = "e" Then
                pv = True
                My.Settings.b0 = "f"
            Else
                My.Settings.b0 = "e"
            End If
            KeyPreview = True
            qrb()
            For q As Integer = 0 To My.Settings.s.Count - 1
                If My.Settings.s.Item(q) = "" Then
                ElseIf My.Settings.s.Item(q) = "search" Then
                Else
                    x.Items.Add(My.Settings.s.Item(q))
                End If
            Next q
            qleaMark("menus-start")
            For Each root As ToolStripDropDownItem In New ToolStripDropDownItem() {m, fl, Me.e, v, hi, bo, ot, he}
                Dim layout = root.DropDown
                layout.SuspendLayout()
                menuLayouts.Add(layout)
            Next
            Dim du As New ToolStripMenuItem
            du.Text = "Duplicate Tab"
            du.Image = My.Resources.F9
            du.BackColor = Color.White
            du.ForeColor = Color.Black
            du.Padding = New Padding(0)
            AddHandler du.Click, AddressOf q4
            AddHandler du.MouseHover, AddressOf q5
            fl.DropDownItems.Insert(3, du)
            Dim ru As New ToolStripMenuItem
            ru.Text = "Reopen Closed Tab"
            ru.Image = My.Resources.F9
            ru.BackColor = Color.White
            ru.ForeColor = Color.Black
            ru.Padding = New Padding(0)
            AddHandler ru.Click, AddressOf q6
            AddHandler ru.MouseHover, AddressOf q7
            fl.DropDownItems.Insert(4, ru)
            Dim co As New ToolStripMenuItem
            co.Text = "Close Other Tabs"
            co.Image = My.Resources.F9
            co.BackColor = Color.White
            co.ForeColor = Color.Black
            co.Padding = New Padding(0)
            AddHandler co.Click, AddressOf q16
            AddHandler co.MouseHover, AddressOf q17
            fl.DropDownItems.Insert(5, co)
            Dim pm As New ToolStripMenuItem
            pm.Text = "Private Mode"
            pm.Image = My.Resources.F9
            pm.BackColor = Color.White
            pm.ForeColor = Color.Black
            pm.Padding = New Padding(0)
            AddHandler pm.Click, AddressOf q18
            AddHandler pm.MouseHover, AddressOf q19
            ot.DropDownItems.Insert(0, pm)
            Dim hm As New ToolStripMenuItem
            hm.Text = "Use Current Page As Home"
            hm.Image = My.Resources.F9
            hm.BackColor = Color.White
            hm.ForeColor = Color.Black
            hm.Padding = New Padding(0)
            AddHandler hm.Click, AddressOf q11
            AddHandler hm.MouseHover, AddressOf q12
            hi.DropDownItems.Insert(4, hm)
            Dim cp As New ToolStripMenuItem
            cp.Text = "Copy Page Link"
            cp.Image = My.Resources.F9
            cp.BackColor = Color.White
            cp.ForeColor = Color.Black
            cp.Padding = New Padding(0)
            AddHandler cp.Click, AddressOf q20
            AddHandler cp.MouseHover, AddressOf q21
            Me.e.DropDownItems.Insert(2, cp)
            Dim ra As New ToolStripMenuItem
            ra.Text = "Reload All Tabs"
            ra.Image = My.Resources.F9
            ra.BackColor = Color.White
            ra.ForeColor = Color.Black
            ra.Padding = New Padding(0)
            AddHandler ra.Click, AddressOf q22
            AddHandler ra.MouseHover, AddressOf q23
            v.DropDownItems.Insert(2, ra)
            tl = New ToolStripMenuItem
            tl.Text = "Tabs"
            tl.Image = My.Resources.F9
            tl.BackColor = Color.White
            tl.ForeColor = Color.Black
            tl.Padding = New Padding(0)
            AddHandler tl.DropDownOpening, AddressOf q24
            AddHandler tl.MouseHover, AddressOf q26
            hi.DropDownItems.Insert(5, tl)
            Dim ss As New ToolStripMenuItem
            ss.Text = "Save Screenshot"
            ss.Image = My.Resources.F9
            ss.BackColor = Color.White
            ss.ForeColor = Color.Black
            ss.Padding = New Padding(0)
            AddHandler ss.Click, AddressOf q27
            AddHandler ss.MouseHover, AddressOf q28
            fl.DropDownItems.Insert(8, ss)
            Dim cs As New ToolStripMenuItem
            cs.Text = "Clear Saved Session"
            cs.Image = My.Resources.F9
            cs.BackColor = Color.White
            cs.ForeColor = Color.Black
            cs.Padding = New Padding(0)
            AddHandler cs.Click, AddressOf q29
            AddHandler cs.MouseHover, AddressOf q30
            ot.DropDownItems.Insert(1, cs)
            Dim ba As New ToolStripMenuItem
            ba.Text = "Bookmark All Tabs"
            ba.Image = My.Resources.F9
            ba.BackColor = Color.White
            ba.ForeColor = Color.Black
            ba.Padding = New Padding(0)
            AddHandler ba.Click, AddressOf q31
            AddHandler ba.MouseHover, AddressOf q32
            bo.DropDownItems.Insert(1, ba)
            Dim cr As New ToolStripMenuItem
            cr.Text = "Close Tabs To The Right"
            cr.Image = My.Resources.F9
            cr.BackColor = Color.White
            cr.ForeColor = Color.Black
            cr.Padding = New Padding(0)
            AddHandler cr.Click, AddressOf q33
            AddHandler cr.MouseHover, AddressOf q34
            fl.DropDownItems.Insert(6, cr)
            Dim pi As New ToolStripMenuItem
            pi.Text = "Page Info"
            pi.Image = My.Resources.F9
            pi.BackColor = Color.White
            pi.ForeColor = Color.Black
            pi.Padding = New Padding(0)
            AddHandler pi.Click, AddressOf q35
            AddHandler pi.MouseHover, AddressOf q36
            he.DropDownItems.Insert(0, pi)
            Dim dl As New ToolStripMenuItem
            dl.Text = "Open Downloads Folder"
            dl.Image = My.Resources.F9
            dl.BackColor = Color.White
            dl.ForeColor = Color.Black
            dl.Padding = New Padding(0)
            AddHandler dl.Click, AddressOf q37
            AddHandler dl.MouseHover, AddressOf q38
            fl.DropDownItems.Insert(9, dl)
            Dim nw As New ToolStripMenuItem
            nw.Text = "Open Page In New Window"
            nw.Image = My.Resources.F9
            nw.BackColor = Color.White
            nw.ForeColor = Color.Black
            nw.Padding = New Padding(0)
            AddHandler nw.Click, AddressOf q39
            AddHandler nw.MouseHover, AddressOf q40
            fl.DropDownItems.Insert(7, nw)
            Dim ct As New ToolStripMenuItem
            ct.Text = "Copy Page Title"
            ct.Image = My.Resources.F9
            ct.BackColor = Color.White
            ct.ForeColor = Color.Black
            ct.Padding = New Padding(0)
            AddHandler ct.Click, AddressOf q41
            AddHandler ct.MouseHover, AddressOf q42
            Me.e.DropDownItems.Insert(3, ct)
            Dim rd As New ToolStripMenuItem
            rd.Text = "Remove Duplicate Bookmarks"
            rd.Image = My.Resources.F9
            rd.BackColor = Color.White
            rd.ForeColor = Color.Black
            rd.Padding = New Padding(0)
            AddHandler rd.Click, AddressOf q43
            AddHandler rd.MouseHover, AddressOf q44
            bo.DropDownItems.Insert(2, rd)
            Dim oa As New ToolStripMenuItem
            oa.Text = "Open All Bookmarks"
            oa.Image = My.Resources.F9
            oa.BackColor = Color.White
            oa.ForeColor = Color.Black
            oa.Padding = New Padding(0)
            AddHandler oa.Click, AddressOf q45
            AddHandler oa.MouseHover, AddressOf q46
            bo.DropDownItems.Insert(3, oa)
            Dim sa As New ToolStripMenuItem
            sa.Text = "Stop All Tabs"
            sa.Image = My.Resources.F9
            sa.BackColor = Color.White
            sa.ForeColor = Color.Black
            sa.Padding = New Padding(0)
            AddHandler sa.Click, AddressOf q47
            AddHandler sa.MouseHover, AddressOf q48
            v.DropDownItems.Insert(3, sa)
            Dim stl As New ToolStripMenuItem
            stl.Text = "Save All Tab Links"
            stl.Image = My.Resources.F9
            stl.BackColor = Color.White
            stl.ForeColor = Color.Black
            stl.Padding = New Padding(0)
            AddHandler stl.Click, AddressOf q49
            AddHandler stl.MouseHover, AddressOf q50
            fl.DropDownItems.Insert(10, stl)
            Dim ml As New ToolStripMenuItem
            ml.Text = "Move Tab Left"
            ml.Image = My.Resources.F9
            ml.BackColor = Color.White
            ml.ForeColor = Color.Black
            ml.Padding = New Padding(0)
            AddHandler ml.Click, AddressOf q51
            AddHandler ml.MouseHover, AddressOf q52
            hi.DropDownItems.Insert(6, ml)
            Dim mr As New ToolStripMenuItem
            mr.Text = "Move Tab Right"
            mr.Image = My.Resources.F9
            mr.BackColor = Color.White
            mr.ForeColor = Color.Black
            mr.Padding = New Padding(0)
            AddHandler mr.Click, AddressOf q53
            AddHandler mr.MouseHover, AddressOf q54
            hi.DropDownItems.Insert(7, mr)
            Dim ctl As New ToolStripMenuItem
            ctl.Text = "Copy Page Title And Link"
            ctl.Image = My.Resources.F9
            ctl.BackColor = Color.White
            ctl.ForeColor = Color.Black
            ctl.Padding = New Padding(0)
            AddHandler ctl.Click, AddressOf q55
            AddHandler ctl.MouseHover, AddressOf q56
            Me.e.DropDownItems.Insert(4, ctl)
            Dim osl As New ToolStripMenuItem
            osl.Text = "Open Saved Tab Links"
            osl.Image = My.Resources.F9
            osl.BackColor = Color.White
            osl.ForeColor = Color.Black
            osl.Padding = New Padding(0)
            AddHandler osl.Click, AddressOf q57
            AddHandler osl.MouseHover, AddressOf q58
            fl.DropDownItems.Insert(11, osl)
            Dim db As New ToolStripMenuItem
            db.Text = "Open In Default Browser"
            db.Image = My.Resources.F9
            db.BackColor = Color.White
            db.ForeColor = Color.Black
            db.Padding = New Padding(0)
            AddHandler db.Click, AddressOf q59
            AddHandler db.MouseHover, AddressOf q60
            fl.DropDownItems.Insert(8, db)
            Dim ca As New ToolStripMenuItem
            ca.Text = "Clear Address And Search Suggestions"
            ca.Image = My.Resources.F9
            ca.BackColor = Color.White
            ca.ForeColor = Color.Black
            ca.Padding = New Padding(0)
            AddHandler ca.Click, AddressOf q61
            AddHandler ca.MouseHover, AddressOf q62
            ot.DropDownItems.Insert(2, ca)
            Dim eb As New ToolStripMenuItem
            eb.Text = "Export Bookmarks"
            eb.Image = My.Resources.F9
            eb.BackColor = Color.White
            eb.ForeColor = Color.Black
            eb.Padding = New Padding(0)
            AddHandler eb.Click, AddressOf q63
            AddHandler eb.MouseHover, AddressOf q64
            bo.DropDownItems.Insert(4, eb)
            Dim ib As New ToolStripMenuItem
            ib.Text = "Import Bookmarks"
            ib.Image = My.Resources.F9
            ib.BackColor = Color.White
            ib.ForeColor = Color.Black
            ib.Padding = New Padding(0)
            AddHandler ib.Click, AddressOf q65
            AddHandler ib.MouseHover, AddressOf q66
            bo.DropDownItems.Insert(5, ib)
            Dim sbm As New ToolStripMenuItem
            sbm.Text = "Sort Bookmarks"
            sbm.Image = My.Resources.F9
            sbm.BackColor = Color.White
            sbm.ForeColor = Color.Black
            sbm.Padding = New Padding(0)
            AddHandler sbm.Click, AddressOf q67
            AddHandler sbm.MouseHover, AddressOf q68
            bo.DropDownItems.Insert(6, sbm)
            Dim cat As New ToolStripMenuItem
            cat.Text = "Copy All Tab Links"
            cat.Image = My.Resources.F9
            cat.BackColor = Color.White
            cat.ForeColor = Color.Black
            cat.Padding = New Padding(0)
            AddHandler cat.Click, AddressOf q69
            AddHandler cat.MouseHover, AddressOf q70
            Me.e.DropDownItems.Insert(5, cat)
            Dim cdt As New ToolStripMenuItem
            cdt.Text = "Close Duplicate Tabs"
            cdt.Image = My.Resources.F9
            cdt.BackColor = Color.White
            cdt.ForeColor = Color.Black
            cdt.Padding = New Padding(0)
            AddHandler cdt.Click, AddressOf q71
            AddHandler cdt.MouseHover, AddressOf q72
            fl.DropDownItems.Insert(12, cdt)
            Dim cttl As New ToolStripMenuItem
            cttl.Text = "Copy All Tab Titles And Links"
            cttl.Image = My.Resources.F9
            cttl.BackColor = Color.White
            cttl.ForeColor = Color.Black
            cttl.Padding = New Padding(0)
            AddHandler cttl.Click, AddressOf q73
            AddHandler cttl.MouseHover, AddressOf q74
            Me.e.DropDownItems.Insert(6, cttl)
            Dim stb As New ToolStripMenuItem
            stb.Text = "Sort Tabs By Title"
            stb.Image = My.Resources.F9
            stb.BackColor = Color.White
            stb.ForeColor = Color.Black
            stb.Padding = New Padding(0)
            AddHandler stb.Click, AddressOf q75
            AddHandler stb.MouseHover, AddressOf q76
            hi.DropDownItems.Insert(8, stb)
            Dim ssx As New ToolStripMenuItem
            ssx.Text = "Search Selected Text"
            ssx.Image = My.Resources.F9
            ssx.BackColor = Color.White
            ssx.ForeColor = Color.Black
            ssx.Padding = New Padding(0)
            AddHandler ssx.Click, AddressOf q77
            AddHandler ssx.MouseHover, AddressOf q78
            Me.e.DropDownItems.Insert(7, ssx)
            Dim daw As New ToolStripMenuItem
            daw.Text = "Duplicate All Tabs To New Window"
            daw.Image = My.Resources.F9
            daw.BackColor = Color.White
            daw.ForeColor = Color.Black
            daw.Padding = New Padding(0)
            AddHandler daw.Click, AddressOf q79
            AddHandler daw.MouseHover, AddressOf q80
            fl.DropDownItems.Insert(13, daw)
            Dim eh As New ToolStripMenuItem
            eh.Text = "Export History"
            eh.Image = My.Resources.F9
            eh.BackColor = Color.White
            eh.ForeColor = Color.Black
            eh.Padding = New Padding(0)
            AddHandler eh.Click, AddressOf q81
            AddHandler eh.MouseHover, AddressOf q82
            hi.DropDownItems.Insert(9, eh)
            Dim ih As New ToolStripMenuItem
            ih.Text = "Import History"
            ih.Image = My.Resources.F9
            ih.BackColor = Color.White
            ih.ForeColor = Color.Black
            ih.Padding = New Padding(0)
            AddHandler ih.Click, AddressOf q95
            AddHandler ih.MouseHover, AddressOf q96
            hi.DropDownItems.Insert(10, ih)
            Dim oh As New ToolStripMenuItem
            oh.Text = "Open History Items In Tabs"
            oh.Image = My.Resources.F9
            oh.BackColor = Color.White
            oh.ForeColor = Color.Black
            oh.Padding = New Padding(0)
            AddHandler oh.Click, AddressOf q97
            AddHandler oh.MouseHover, AddressOf q98
            hi.DropDownItems.Insert(11, oh)
            Dim sc As New ToolStripMenuItem
            sc.Text = "Startup Restore Session"
            sc.Image = My.Resources.F9
            sc.BackColor = Color.White
            sc.ForeColor = Color.Black
            sc.Padding = New Padding(0)
            AddHandler sc.Click, AddressOf q83
            AddHandler sc.MouseHover, AddressOf q84
            ot.DropDownItems.Insert(3, sc)
            Dim cl As New ToolStripMenuItem
            cl.Text = "Close Tabs Left Of Current"
            cl.Image = My.Resources.F9
            cl.BackColor = Color.White
            cl.ForeColor = Color.Black
            cl.Padding = New Padding(0)
            AddHandler cl.Click, AddressOf q85
            AddHandler cl.MouseHover, AddressOf q86
            fl.DropDownItems.Insert(14, cl)
            Dim spt As New ToolStripMenuItem
            spt.Text = "Save Current Page Text"
            spt.Image = My.Resources.F9
            spt.BackColor = Color.White
            spt.ForeColor = Color.Black
            spt.Padding = New Padding(0)
            AddHandler spt.Click, AddressOf q87
            AddHandler spt.MouseHover, AddressOf q88
            fl.DropDownItems.Insert(15, spt)
            Dim rt As New ToolStripMenuItem
            rt.Text = "Reverse Tab Order"
            rt.Image = My.Resources.F9
            rt.BackColor = Color.White
            rt.ForeColor = Color.Black
            rt.Padding = New Padding(0)
            AddHandler rt.Click, AddressOf q89
            AddHandler rt.MouseHover, AddressOf q90
            hi.DropDownItems.Insert(10, rt)
            Dim cpt As New ToolStripMenuItem
            cpt.Text = "Copy Current Page Text"
            cpt.Image = My.Resources.F9
            cpt.BackColor = Color.White
            cpt.ForeColor = Color.Black
            cpt.Padding = New Padding(0)
            AddHandler cpt.Click, AddressOf q91
            AddHandler cpt.MouseHover, AddressOf q92
            Me.e.DropDownItems.Insert(8, cpt)
            Dim sph As New ToolStripMenuItem
            sph.Text = "Save Current Page HTML"
            sph.Image = My.Resources.F9
            sph.BackColor = Color.White
            sph.ForeColor = Color.Black
            sph.Padding = New Padding(0)
            AddHandler sph.Click, AddressOf q93
            AddHandler sph.MouseHover, AddressOf q94
            fl.DropDownItems.Insert(16, sph)
            Dim cbm As New ToolStripMenuItem
            cbm.Text = "Clear Bookmarks"
            cbm.Image = My.Resources.F9
            cbm.BackColor = Color.White
            cbm.ForeColor = Color.Black
            cbm.Padding = New Padding(0)
            AddHandler cbm.Click, AddressOf q99
            AddHandler cbm.MouseHover, AddressOf q100
            bo.DropDownItems.Insert(7, cbm)
            Dim ohm As New ToolStripMenuItem
            ohm.Text = "Open Homepage In New Tab"
            ohm.Image = My.Resources.F9
            ohm.BackColor = Color.White
            ohm.ForeColor = Color.Black
            ohm.Padding = New Padding(0)
            AddHandler ohm.Click, AddressOf q101
            AddHandler ohm.MouseHover, AddressOf q102
            hi.DropDownItems.Insert(12, ohm)
            Dim chm As New ToolStripMenuItem
            chm.Text = "Copy Homepage Link"
            chm.Image = My.Resources.F9
            chm.BackColor = Color.White
            chm.ForeColor = Color.Black
            chm.Padding = New Padding(0)
            AddHandler chm.Click, AddressOf q103
            AddHandler chm.MouseHover, AddressOf q104
            hi.DropDownItems.Insert(13, chm)
            Dim rhm As New ToolStripMenuItem
            rhm.Text = "Reset Homepage To Default"
            rhm.Image = My.Resources.F9
            rhm.BackColor = Color.White
            rhm.ForeColor = Color.Black
            rhm.Padding = New Padding(0)
            AddHandler rhm.Click, AddressOf q105
            AddHandler rhm.MouseHover, AddressOf q106
            hi.DropDownItems.Insert(14, rhm)
            Dim sws As New ToolStripMenuItem
            sws.Text = "Save Window Snapshot"
            sws.Image = My.Resources.F9
            sws.BackColor = Color.White
            sws.ForeColor = Color.Black
            sws.Padding = New Padding(0)
            AddHandler sws.Click, AddressOf q107
            AddHandler sws.MouseHover, AddressOf q108
            fl.DropDownItems.Insert(17, sws)
            Dim csh As New ToolStripMenuItem
            csh.Text = "Clear Session And History"
            csh.Image = My.Resources.F9
            csh.BackColor = Color.White
            csh.ForeColor = Color.Black
            csh.Padding = New Padding(0)
            AddHandler csh.Click, AddressOf q109
            AddHandler csh.MouseHover, AddressOf q110
            ot.DropDownItems.Insert(4, csh)
            Dim cdm As New ToolStripMenuItem
            cdm.Text = "Copy Current Domain"
            cdm.Image = My.Resources.F9
            cdm.BackColor = Color.White
            cdm.ForeColor = Color.Black
            cdm.Padding = New Padding(0)
            AddHandler cdm.Click, AddressOf q111
            AddHandler cdm.MouseHover, AddressOf q112
            Me.e.DropDownItems.Insert(9, cdm)
            Dim ptf As New ToolStripMenuItem
            ptf.Text = "Pin Current Tab As First"
            ptf.Image = My.Resources.F9
            ptf.BackColor = Color.White
            ptf.ForeColor = Color.Black
            ptf.Padding = New Padding(0)
            AddHandler ptf.Click, AddressOf q113
            AddHandler ptf.MouseHover, AddressOf q114
            hi.DropDownItems.Insert(15, ptf)
            Dim ohd As New ToolStripMenuItem
            ohd.Text = "Open Homepage In Default Browser"
            ohd.Image = My.Resources.F9
            ohd.BackColor = Color.White
            ohd.ForeColor = Color.Black
            ohd.Padding = New Padding(0)
            AddHandler ohd.Click, AddressOf q115
            AddHandler ohd.MouseHover, AddressOf q116
            hi.DropDownItems.Insert(16, ohd)
            Dim css As New ToolStripMenuItem
            css.Text = "Copy Session Summary"
            css.Image = My.Resources.F9
            css.BackColor = Color.White
            css.ForeColor = Color.Black
            css.Padding = New Padding(0)
            AddHandler css.Click, AddressOf q117
            AddHandler css.MouseHover, AddressOf q118
            Me.e.DropDownItems.Insert(10, css)
            Dim rdt As New ToolStripMenuItem
            rdt.Text = "Reload Duplicate Tabs Only"
            rdt.Image = My.Resources.F9
            rdt.BackColor = Color.White
            rdt.ForeColor = Color.Black
            rdt.Padding = New Padding(0)
            AddHandler rdt.Click, AddressOf q119
            AddHandler rdt.MouseHover, AddressOf q120
            hi.DropDownItems.Insert(17, rdt)
            Dim cab As New ToolStripMenuItem
            cab.Text = "Copy All Bookmark Links"
            cab.Image = My.Resources.F9
            cab.BackColor = Color.White
            cab.ForeColor = Color.Black
            cab.Padding = New Padding(0)
            AddHandler cab.Click, AddressOf q121
            AddHandler cab.MouseHover, AddressOf q122
            bo.DropDownItems.Insert(8, cab)
            Dim odh As New ToolStripMenuItem
            odh.Text = "Quick Open Current Domain"
            odh.Image = My.Resources.F9
            odh.BackColor = Color.White
            odh.ForeColor = Color.Black
            odh.Padding = New Padding(0)
            AddHandler odh.Click, AddressOf q123
            AddHandler odh.MouseHover, AddressOf q124
            hi.DropDownItems.Insert(18, odh)
            Dim sbs As New ToolStripMenuItem
            sbs.Text = "Save Bookmark Snapshot"
            sbs.Image = My.Resources.F9
            sbs.BackColor = Color.White
            sbs.ForeColor = Color.Black
            sbs.Padding = New Padding(0)
            AddHandler sbs.Click, AddressOf q125
            AddHandler sbs.MouseHover, AddressOf q126
            bo.DropDownItems.Insert(9, sbs)
            Dim cah As New ToolStripMenuItem
            cah.Text = "Copy All History Links"
            cah.Image = My.Resources.F9
            cah.BackColor = Color.White
            cah.ForeColor = Color.Black
            cah.Padding = New Padding(0)
            AddHandler cah.Click, AddressOf q127
            AddHandler cah.MouseHover, AddressOf q128
            Me.e.DropDownItems.Insert(11, cah)
            Dim shd As New ToolStripMenuItem
            shd.Text = "Set Homepage To Current Domain"
            shd.Image = My.Resources.F9
            shd.BackColor = Color.White
            shd.ForeColor = Color.Black
            shd.Padding = New Padding(0)
            AddHandler shd.Click, AddressOf q129
            AddHandler shd.MouseHover, AddressOf q130
            hi.DropDownItems.Insert(19, shd)
            Dim olf As New ToolStripMenuItem
            olf.Text = "Open Current File In Folder"
            olf.Image = My.Resources.F9
            olf.BackColor = Color.White
            olf.ForeColor = Color.Black
            olf.Padding = New Padding(0)
            AddHandler olf.Click, AddressOf q131
            AddHandler olf.MouseHover, AddressOf q132
            fl.DropDownItems.Insert(18, olf)
            Dim bsh As New ToolStripMenuItem
            bsh.Text = "Bookmark Selected History Item"
            bsh.Image = My.Resources.F9
            bsh.BackColor = Color.White
            bsh.ForeColor = Color.Black
            bsh.Padding = New Padding(0)
            AddHandler bsh.Click, AddressOf q133
            AddHandler bsh.MouseHover, AddressOf q134
            bo.DropDownItems.Insert(10, bsh)
            Dim shs As New ToolStripMenuItem
            shs.Text = "Save History Snapshot"
            shs.Image = My.Resources.F9
            shs.BackColor = Color.White
            shs.ForeColor = Color.Black
            shs.Padding = New Padding(0)
            AddHandler shs.Click, AddressOf q135
            AddHandler shs.MouseHover, AddressOf q136
            hi.DropDownItems.Insert(20, shs)
            Dim cad As New ToolStripMenuItem
            cad.Text = "Clear All Browser Data"
            cad.Image = My.Resources.F9
            cad.BackColor = Color.White
            cad.ForeColor = Color.Black
            cad.Padding = New Padding(0)
            AddHandler cad.Click, AddressOf q137
            AddHandler cad.MouseHover, AddressOf q138
            ot.DropDownItems.Insert(5, cad)
            Dim odt As New ToolStripMenuItem
            odt.Text = "Open Current Domain In New Tab"
            odt.Image = My.Resources.F9
            odt.BackColor = Color.White
            odt.ForeColor = Color.Black
            odt.Padding = New Padding(0)
            AddHandler odt.Click, AddressOf q139
            AddHandler odt.MouseHover, AddressOf q140
            hi.DropDownItems.Insert(21, odt)
            Dim sds As New ToolStripMenuItem
            sds.Text = "Save All Browser Data Snapshot"
            sds.Image = My.Resources.F9
            sds.BackColor = Color.White
            sds.ForeColor = Color.Black
            sds.Padding = New Padding(0)
            AddHandler sds.Click, AddressOf q141
            AddHandler sds.MouseHover, AddressOf q142
            fl.DropDownItems.Insert(19, sds)
            Dim bcd As New ToolStripMenuItem
            bcd.Text = "Bookmark Current Domain"
            bcd.Image = My.Resources.F9
            bcd.BackColor = Color.White
            bcd.ForeColor = Color.Black
            bcd.Padding = New Padding(0)
            AddHandler bcd.Click, AddressOf q143
            AddHandler bcd.MouseHover, AddressOf q144
            bo.DropDownItems.Insert(11, bcd)
            Dim rsd As New ToolStripMenuItem
            rsd.Text = "Reload Tabs From Same Domain"
            rsd.Image = My.Resources.F9
            rsd.BackColor = Color.White
            rsd.ForeColor = Color.Black
            rsd.Padding = New Padding(0)
            AddHandler rsd.Click, AddressOf q145
            AddHandler rsd.MouseHover, AddressOf q146
            hi.DropDownItems.Insert(22, rsd)
            Dim csd As New ToolStripMenuItem
            csd.Text = "Copy Tabs From Same Domain"
            csd.Image = My.Resources.F9
            csd.BackColor = Color.White
            csd.ForeColor = Color.Black
            csd.Padding = New Padding(0)
            AddHandler csd.Click, AddressOf q147
            AddHandler csd.MouseHover, AddressOf q148
            Me.e.DropDownItems.Insert(12, csd)
            Dim cls As New ToolStripMenuItem
            cls.Text = "Close Tabs From Same Domain"
            cls.Image = My.Resources.F9
            cls.BackColor = Color.White
            cls.ForeColor = Color.Black
            cls.Padding = New Padding(0)
            AddHandler cls.Click, AddressOf q149
            AddHandler cls.MouseHover, AddressOf q150
            hi.DropDownItems.Insert(23, cls)
            Dim sdn As New ToolStripMenuItem
            sdn.Text = "Save Current Domain Snapshot"
            sdn.Image = My.Resources.F9
            sdn.BackColor = Color.White
            sdn.ForeColor = Color.Black
            sdn.Padding = New Padding(0)
            AddHandler sdn.Click, AddressOf q151
            AddHandler sdn.MouseHover, AddressOf q152
            fl.DropDownItems.Insert(20, sdn)
            Dim oct As New ToolStripMenuItem
            oct.Text = "Open Current URL In New Tab"
            oct.Image = My.Resources.F9
            oct.BackColor = Color.White
            oct.ForeColor = Color.Black
            oct.Padding = New Padding(0)
            AddHandler oct.Click, AddressOf q153
            AddHandler oct.MouseHover, AddressOf q154
            hi.DropDownItems.Insert(24, oct)
            Dim sss As New ToolStripMenuItem
            sss.Text = "Save Search Suggestions Snapshot"
            sss.Image = My.Resources.F9
            sss.BackColor = Color.White
            sss.ForeColor = Color.Black
            sss.Padding = New Padding(0)
            AddHandler sss.Click, AddressOf q155
            AddHandler sss.MouseHover, AddressOf q156
            fl.DropDownItems.Insert(21, sss)
            Dim cps As New ToolStripMenuItem
            cps.Text = "Copy Search Suggestions"
            cps.Image = My.Resources.F9
            cps.BackColor = Color.White
            cps.ForeColor = Color.Black
            cps.Padding = New Padding(0)
            AddHandler cps.Click, AddressOf q157
            AddHandler cps.MouseHover, AddressOf q158
            Me.e.DropDownItems.Insert(13, cps)
            Dim rse As New ToolStripMenuItem
            rse.Text = "Restore Default Search Engine"
            rse.Image = My.Resources.F9
            rse.BackColor = Color.White
            rse.ForeColor = Color.Black
            rse.Padding = New Padding(0)
            AddHandler rse.Click, AddressOf q159
            AddHandler rse.MouseHover, AddressOf q160
            ot.DropDownItems.Insert(6, rse)
            Dim cml As New ToolStripMenuItem
            cml.Text = "Copy Page As Markdown Link"
            cml.Image = My.Resources.F9
            cml.BackColor = Color.White
            cml.ForeColor = Color.Black
            cml.Padding = New Padding(0)
            AddHandler cml.Click, AddressOf q167
            AddHandler cml.MouseHover, AddressOf q168
            Me.e.DropDownItems.Insert(14, cml)
            Dim sth As New ToolStripMenuItem
            sth.Text = "Save Tab Links As HTML"
            sth.Image = My.Resources.F9
            sth.BackColor = Color.White
            sth.ForeColor = Color.Black
            sth.Padding = New Padding(0)
            AddHandler sth.Click, AddressOf q169
            AddHandler sth.MouseHover, AddressOf q170
            fl.DropDownItems.Insert(22, sth)
            Dim bsd As New ToolStripMenuItem
            bsd.Text = "Bookmark Tabs From Same Domain"
            bsd.Image = My.Resources.F9
            bsd.BackColor = Color.White
            bsd.ForeColor = Color.Black
            bsd.Padding = New Padding(0)
            AddHandler bsd.Click, AddressOf q171
            AddHandler bsd.MouseHover, AddressOf q172
            bo.DropDownItems.Insert(12, bsd)
            Dim odb As New ToolStripMenuItem
            odb.Text = "Open Current Domain In Default Browser"
            odb.Image = My.Resources.F9
            odb.BackColor = Color.White
            odb.ForeColor = Color.Black
            odb.Padding = New Padding(0)
            AddHandler odb.Click, AddressOf q173
            AddHandler odb.MouseHover, AddressOf q174
            hi.DropDownItems.Insert(25, odb)
            Dim cbt As New ToolStripMenuItem
            cbt.Text = "Close Blank Tabs"
            cbt.Image = My.Resources.F9
            cbt.BackColor = Color.White
            cbt.ForeColor = Color.Black
            cbt.Padding = New Padding(0)
            AddHandler cbt.Click, AddressOf q175
            AddHandler cbt.MouseHover, AddressOf q176
            fl.DropDownItems.Insert(23, cbt)
            Dim cal As New ToolStripMenuItem
            cal.Text = "Copy All Links On Current Page"
            cal.Image = My.Resources.F9
            cal.BackColor = Color.White
            cal.ForeColor = Color.Black
            cal.Padding = New Padding(0)
            AddHandler cal.Click, AddressOf q177
            AddHandler cal.MouseHover, AddressOf q178
            Me.e.DropDownItems.Insert(15, cal)
            Dim sal As New ToolStripMenuItem
            sal.Text = "Save All Links On Current Page"
            sal.Image = My.Resources.F9
            sal.BackColor = Color.White
            sal.ForeColor = Color.Black
            sal.Padding = New Padding(0)
            AddHandler sal.Click, AddressOf q179
            AddHandler sal.MouseHover, AddressOf q180
            fl.DropDownItems.Insert(24, sal)
            Dim bal As New ToolStripMenuItem
            bal.Text = "Bookmark All Links On Current Page"
            bal.Image = My.Resources.F9
            bal.BackColor = Color.White
            bal.ForeColor = Color.Black
            bal.Padding = New Padding(0)
            AddHandler bal.Click, AddressOf q181
            AddHandler bal.MouseHover, AddressOf q182
            bo.DropDownItems.Insert(13, bal)
            Dim pst As New ToolStripMenuItem
            pst.Text = "Scroll Page To Top"
            pst.Image = My.Resources.F9
            pst.BackColor = Color.White
            pst.ForeColor = Color.Black
            pst.Padding = New Padding(0)
            AddHandler pst.Click, AddressOf q183
            AddHandler pst.MouseHover, AddressOf q184
            v.DropDownItems.Insert(4, pst)
            Dim psb As New ToolStripMenuItem
            psb.Text = "Scroll Page To Bottom"
            psb.Image = My.Resources.F9
            psb.BackColor = Color.White
            psb.ForeColor = Color.Black
            psb.Padding = New Padding(0)
            AddHandler psb.Click, AddressOf q185
            AddHandler psb.MouseHover, AddressOf q186
            v.DropDownItems.Insert(5, psb)
            Dim cpi As New ToolStripMenuItem
            cpi.Text = "Copy Page Image Links"
            cpi.Image = My.Resources.F9
            cpi.BackColor = Color.White
            cpi.ForeColor = Color.Black
            cpi.Padding = New Padding(0)
            AddHandler cpi.Click, AddressOf q187
            AddHandler cpi.MouseHover, AddressOf q188
            Me.e.DropDownItems.Insert(16, cpi)
            Dim spi As New ToolStripMenuItem
            spi.Text = "Save Page Image Links"
            spi.Image = My.Resources.F9
            spi.BackColor = Color.White
            spi.ForeColor = Color.Black
            spi.Padding = New Padding(0)
            AddHandler spi.Click, AddressOf q189
            AddHandler spi.MouseHover, AddressOf q190
            fl.DropDownItems.Insert(25, spi)
            Dim cph As New ToolStripMenuItem
            cph.Text = "Copy Current Page HTML"
            cph.Image = My.Resources.F9
            cph.BackColor = Color.White
            cph.ForeColor = Color.Black
            cph.Padding = New Padding(0)
            AddHandler cph.Click, AddressOf q191
            AddHandler cph.MouseHover, AddressOf q192
            Me.e.DropDownItems.Insert(17, cph)
            Dim cpm As New ToolStripMenuItem
            cpm.Text = "Copy Page Metadata"
            cpm.Image = My.Resources.F9
            cpm.BackColor = Color.White
            cpm.ForeColor = Color.Black
            cpm.Padding = New Padding(0)
            AddHandler cpm.Click, AddressOf q193
            AddHandler cpm.MouseHover, AddressOf q194
            Me.e.DropDownItems.Insert(18, cpm)
            Dim osd As New ToolStripMenuItem
            osd.Text = "Open Same-Domain Page Links"
            osd.Image = My.Resources.F9
            osd.BackColor = Color.White
            osd.ForeColor = Color.Black
            osd.Padding = New Padding(0)
            AddHandler osd.Click, AddressOf q195
            AddHandler osd.MouseHover, AddressOf q196
            hi.DropDownItems.Insert(26, osd)
            Dim cspl As New ToolStripMenuItem
            cspl.Text = "Copy Same-Domain Page Links"
            cspl.Image = My.Resources.F9
            cspl.BackColor = Color.White
            cspl.ForeColor = Color.Black
            cspl.Padding = New Padding(0)
            AddHandler cspl.Click, AddressOf q197
            AddHandler cspl.MouseHover, AddressOf q198
            Me.e.DropDownItems.Insert(19, cspl)
            Dim sspl As New ToolStripMenuItem
            sspl.Text = "Save Same-Domain Page Links"
            sspl.Image = My.Resources.F9
            sspl.BackColor = Color.White
            sspl.ForeColor = Color.Black
            sspl.Padding = New Padding(0)
            AddHandler sspl.Click, AddressOf q199
            AddHandler sspl.MouseHover, AddressOf q200
            fl.DropDownItems.Insert(26, sspl)
            Dim oit As New ToolStripMenuItem
            oit.Text = "Open Page Image Links In Tabs"
            oit.Image = My.Resources.F9
            oit.BackColor = Color.White
            oit.ForeColor = Color.Black
            oit.Padding = New Padding(0)
            AddHandler oit.Click, AddressOf q201
            AddHandler oit.MouseHover, AddressOf q202
            hi.DropDownItems.Insert(27, oit)
            Dim cts As New ToolStripMenuItem
            cts.Text = "Copy Tab Summary"
            cts.Image = My.Resources.F9
            cts.BackColor = Color.White
            cts.ForeColor = Color.Black
            cts.Padding = New Padding(0)
            AddHandler cts.Click, AddressOf q203
            AddHandler cts.MouseHover, AddressOf q204
            Me.e.DropDownItems.Insert(20, cts)
            Dim spm As New ToolStripMenuItem
            spm.Text = "Save Page Metadata"
            spm.Image = My.Resources.F9
            spm.BackColor = Color.White
            spm.ForeColor = Color.Black
            spm.Padding = New Padding(0)
            AddHandler spm.Click, AddressOf q205
            AddHandler spm.MouseHover, AddressOf q206
            fl.DropDownItems.Insert(27, spm)
            Dim oat As New ToolStripMenuItem
            oat.Text = "Open All Tabs In Default Browser"
            oat.Image = My.Resources.F9
            oat.BackColor = Color.White
            oat.ForeColor = Color.Black
            oat.Padding = New Padding(0)
            AddHandler oat.Click, AddressOf q207
            AddHandler oat.MouseHover, AddressOf q208
            fl.DropDownItems.Insert(28, oat)
            Dim stc As New ToolStripMenuItem
            stc.Text = "Save All Tabs As CSV"
            stc.Image = My.Resources.F9
            stc.BackColor = Color.White
            stc.ForeColor = Color.Black
            stc.Padding = New Padding(0)
            AddHandler stc.Click, AddressOf q209
            AddHandler stc.MouseHover, AddressOf q210
            fl.DropDownItems.Insert(29, stc)
            Dim cbs As New ToolStripMenuItem
            cbs.Text = "Copy Bookmark Summary"
            cbs.Image = My.Resources.F9
            cbs.BackColor = Color.White
            cbs.ForeColor = Color.Black
            cbs.Padding = New Padding(0)
            AddHandler cbs.Click, AddressOf q211
            AddHandler cbs.MouseHover, AddressOf q212
            Me.e.DropDownItems.Insert(21, cbs)
            Dim orb As New ToolStripMenuItem
            orb.Text = "Open Random Bookmark"
            orb.Image = My.Resources.F9
            orb.BackColor = Color.White
            orb.ForeColor = Color.Black
            orb.Padding = New Padding(0)
            AddHandler orb.Click, AddressOf q213
            AddHandler orb.MouseHover, AddressOf q214
            bo.DropDownItems.Insert(14, orb)
            Dim ctp As New ToolStripMenuItem
            ctp.Text = "Copy Page Text Preview"
            ctp.Image = My.Resources.F9
            ctp.BackColor = Color.White
            ctp.ForeColor = Color.Black
            ctp.Padding = New Padding(0)
            AddHandler ctp.Click, AddressOf q215
            AddHandler ctp.MouseHover, AddressOf q216
            Me.e.DropDownItems.Insert(22, ctp)
            Dim stp As New ToolStripMenuItem
            stp.Text = "Save Page Text Preview"
            stp.Image = My.Resources.F9
            stp.BackColor = Color.White
            stp.ForeColor = Color.Black
            stp.Padding = New Padding(0)
            AddHandler stp.Click, AddressOf q217
            AddHandler stp.MouseHover, AddressOf q218
            fl.DropDownItems.Insert(30, stp)
            Dim hrf As New ToolStripMenuItem
            hrf.Text = "Hard Refresh Current Tab"
            hrf.Image = My.Resources.F9
            hrf.BackColor = Color.White
            hrf.ForeColor = Color.Black
            hrf.Padding = New Padding(0)
            AddHandler hrf.Click, AddressOf q219
            AddHandler hrf.MouseHover, AddressOf q220
            v.DropDownItems.Insert(6, hrf)
            Dim ctm As New ToolStripMenuItem
            ctm.Text = "Copy All Tabs As Markdown"
            ctm.Image = My.Resources.F9
            ctm.BackColor = Color.White
            ctm.ForeColor = Color.Black
            ctm.Padding = New Padding(0)
            AddHandler ctm.Click, AddressOf q221
            AddHandler ctm.MouseHover, AddressOf q222
            Me.e.DropDownItems.Insert(23, ctm)
            Dim sbh As New ToolStripMenuItem
            sbh.Text = "Save Bookmarks As HTML"
            sbh.Image = My.Resources.F9
            sbh.BackColor = Color.White
            sbh.ForeColor = Color.Black
            sbh.Padding = New Padding(0)
            AddHandler sbh.Click, AddressOf q223
            AddHandler sbh.MouseHover, AddressOf q224
            fl.DropDownItems.Insert(31, sbh)
            Dim chs As New ToolStripMenuItem
            chs.Text = "Copy History Summary"
            chs.Image = My.Resources.F9
            chs.BackColor = Color.White
            chs.ForeColor = Color.Black
            chs.Padding = New Padding(0)
            AddHandler chs.Click, AddressOf q225
            AddHandler chs.MouseHover, AddressOf q226
            Me.e.DropDownItems.Insert(24, chs)
            Dim std As New ToolStripMenuItem
            std.Text = "Search Current Domain"
            std.Image = My.Resources.F9
            std.BackColor = Color.White
            std.ForeColor = Color.Black
            std.Padding = New Padding(0)
            AddHandler std.Click, AddressOf q227
            AddHandler std.MouseHover, AddressOf q228
            hi.DropDownItems.Insert(28, std)
            Dim vat As New ToolStripMenuItem
            vat.Text = "View Current Page Source In New Tab"
            vat.Image = My.Resources.F9
            vat.BackColor = Color.White
            vat.ForeColor = Color.Black
            vat.Padding = New Padding(0)
            AddHandler vat.Click, AddressOf q229
            AddHandler vat.MouseHover, AddressOf q230
            hi.DropDownItems.Insert(29, vat)
            Dim cdh As New ToolStripMenuItem
            cdh.Text = "Remove Duplicate History Items"
            cdh.Image = My.Resources.F9
            cdh.BackColor = Color.White
            cdh.ForeColor = Color.Black
            cdh.Padding = New Padding(0)
            AddHandler cdh.Click, AddressOf q231
            AddHandler cdh.MouseHover, AddressOf q232
            hi.DropDownItems.Insert(30, cdh)
            Dim stm As New ToolStripMenuItem
            stm.Text = "Save All Tabs As Markdown"
            stm.Image = My.Resources.F9
            stm.BackColor = Color.White
            stm.ForeColor = Color.Black
            stm.Padding = New Padding(0)
            AddHandler stm.Click, AddressOf q233
            AddHandler stm.MouseHover, AddressOf q234
            fl.DropDownItems.Insert(32, stm)
            Dim hth As New ToolStripMenuItem
            hth.Text = "Save History As HTML"
            hth.Image = My.Resources.F9
            hth.BackColor = Color.White
            hth.ForeColor = Color.Black
            hth.Padding = New Padding(0)
            AddHandler hth.Click, AddressOf q235
            AddHandler hth.MouseHover, AddressOf q236
            fl.DropDownItems.Insert(33, hth)
            Dim plh As New ToolStripMenuItem
            plh.Text = "Copy Page Links As HTML"
            plh.Image = My.Resources.F9
            plh.BackColor = Color.White
            plh.ForeColor = Color.Black
            plh.Padding = New Padding(0)
            AddHandler plh.Click, AddressOf q237
            AddHandler plh.MouseHover, AddressOf q238
            Me.e.DropDownItems.Insert(25, plh)
            Dim ocl As New ToolStripMenuItem
            ocl.Text = "Open Clipboard Link In New Tab"
            ocl.Image = My.Resources.F9
            ocl.BackColor = Color.White
            ocl.ForeColor = Color.Black
            ocl.Padding = New Padding(0)
            AddHandler ocl.Click, AddressOf q239
            AddHandler ocl.MouseHover, AddressOf q240
            hi.DropDownItems.Insert(31, ocl)
            Dim sot As New ToolStripMenuItem
            sot.Text = "Search Open Tabs"
            sot.Image = My.Resources.F9
            sot.BackColor = Color.White
            sot.ForeColor = Color.Black
            sot.Padding = New Padding(0)
            AddHandler sot.Click, AddressOf q241
            AddHandler sot.MouseHover, AddressOf q242
            hi.DropDownItems.Insert(32, sot)
            Dim ctd As New ToolStripMenuItem
            ctd.Text = "Copy Open Tab Domains"
            ctd.Image = My.Resources.F9
            ctd.BackColor = Color.White
            ctd.ForeColor = Color.Black
            ctd.Padding = New Padding(0)
            AddHandler ctd.Click, AddressOf q243
            AddHandler ctd.MouseHover, AddressOf q244
            Me.e.DropDownItems.Insert(26, ctd)
            Dim plm As New ToolStripMenuItem
            plm.Text = "Copy Page Links As Markdown"
            plm.Image = My.Resources.F9
            plm.BackColor = Color.White
            plm.ForeColor = Color.Black
            plm.Padding = New Padding(0)
            AddHandler plm.Click, AddressOf q245
            AddHandler plm.MouseHover, AddressOf q246
            Me.e.DropDownItems.Insert(27, plm)
            Dim pim As New ToolStripMenuItem
            pim.Text = "Copy Page Images As Markdown"
            pim.Image = My.Resources.F9
            pim.BackColor = Color.White
            pim.ForeColor = Color.Black
            pim.Padding = New Padding(0)
            AddHandler pim.Click, AddressOf q247
            AddHandler pim.MouseHover, AddressOf q248
            Me.e.DropDownItems.Insert(28, pim)
            Dim sdc As New ToolStripMenuItem
            sdc.Text = "Save Open Tab Domains As CSV"
            sdc.Image = My.Resources.F9
            sdc.BackColor = Color.White
            sdc.ForeColor = Color.Black
            sdc.Padding = New Padding(0)
            AddHandler sdc.Click, AddressOf q249
            AddHandler sdc.MouseHover, AddressOf q250
            fl.DropDownItems.Insert(34, sdc)
            Dim rsn As New ToolStripMenuItem
            rsn.Text = "Reopen Saved Session In New Window"
            rsn.Image = My.Resources.F9
            rsn.BackColor = Color.White
            rsn.ForeColor = Color.Black
            rsn.Padding = New Padding(0)
            AddHandler rsn.Click, AddressOf q251
            AddHandler rsn.MouseHover, AddressOf q252
            fl.DropDownItems.Insert(35, rsn)
            Dim ctt As New ToolStripMenuItem
            ctt.Text = "Close Tabs Matching Text"
            ctt.Image = My.Resources.F9
            ctt.BackColor = Color.White
            ctt.ForeColor = Color.Black
            ctt.Padding = New Padding(0)
            AddHandler ctt.Click, AddressOf q253
            AddHandler ctt.MouseHover, AddressOf q254
            hi.DropDownItems.Insert(33, ctt)
            Dim bcl As New ToolStripMenuItem
            bcl.Text = "Bookmark Clipboard Link"
            bcl.Image = My.Resources.F9
            bcl.BackColor = Color.White
            bcl.ForeColor = Color.Black
            bcl.Padding = New Padding(0)
            AddHandler bcl.Click, AddressOf q255
            AddHandler bcl.MouseHover, AddressOf q256
            bo.DropDownItems.Insert(15, bcl)
            Dim cwc As New ToolStripMenuItem
            cwc.Text = "Copy Page Word Count"
            cwc.Image = My.Resources.F9
            cwc.BackColor = Color.White
            cwc.ForeColor = Color.Black
            cwc.Padding = New Padding(0)
            AddHandler cwc.Click, AddressOf q257
            AddHandler cwc.MouseHover, AddressOf q258
            Me.e.DropDownItems.Insert(29, cwc)
            Dim spo As New ToolStripMenuItem
            spo.Text = "Save Page Outline"
            spo.Image = My.Resources.F9
            spo.BackColor = Color.White
            spo.ForeColor = Color.Black
            spo.Padding = New Padding(0)
            AddHandler spo.Click, AddressOf q259
            AddHandler spo.MouseHover, AddressOf q260
            fl.DropDownItems.Insert(36, spo)
            Dim cpo As New ToolStripMenuItem
            cpo.Text = "Copy Page Outline"
            cpo.Image = My.Resources.F9
            cpo.BackColor = Color.White
            cpo.ForeColor = Color.Black
            cpo.Padding = New Padding(0)
            AddHandler cpo.Click, AddressOf q261
            AddHandler cpo.MouseHover, AddressOf q262
            Me.e.DropDownItems.Insert(30, cpo)
            Dim sotp As New ToolStripMenuItem
            sotp.Text = "Save Open Tabs As Plain List"
            sotp.Image = My.Resources.F9
            sotp.BackColor = Color.White
            sotp.ForeColor = Color.Black
            sotp.Padding = New Padding(0)
            AddHandler sotp.Click, AddressOf q263
            AddHandler sotp.MouseHover, AddressOf q264
            fl.DropDownItems.Insert(37, sotp)
            Dim rz As New ToolStripMenuItem
            rz.Text = "Reset Zoom To 100%"
            rz.Image = My.Resources.F9
            rz.BackColor = Color.White
            rz.ForeColor = Color.Black
            rz.Padding = New Padding(0)
            AddHandler rz.Click, AddressOf q265
            AddHandler rz.MouseHover, AddressOf q266
            v.DropDownItems.Insert(7, rz)
            Dim rm As New ToolStripMenuItem
            rm.Text = "Reading Mode"
            rm.Image = My.Resources.F9
            rm.BackColor = Color.White
            rm.ForeColor = Color.Black
            rm.Padding = New Padding(0)
            AddHandler rm.Click, AddressOf q267
            AddHandler rm.MouseHover, AddressOf q268
            v.DropDownItems.Insert(8, rm)
            Dim mt As New ToolStripMenuItem
            mt.Text = "Mute Page Audio"
            mt.Image = My.Resources.F9
            mt.BackColor = Color.White
            mt.ForeColor = Color.Black
            mt.Padding = New Padding(0)
            AddHandler mt.Click, AddressOf q269
            AddHandler mt.MouseHover, AddressOf q270
            v.DropDownItems.Insert(9, mt)
            Dim rnt As New ToolStripMenuItem
            rnt.Text = "Rename Current Tab"
            rnt.Image = My.Resources.F9
            rnt.BackColor = Color.White
            rnt.ForeColor = Color.Black
            rnt.Padding = New Padding(0)
            AddHandler rnt.Click, AddressOf q271
            AddHandler rnt.MouseHover, AddressOf q272
            hi.DropDownItems.Insert(34, rnt)
            Dim lkt As New ToolStripMenuItem
            lkt.Text = "Lock Current Tab"
            lkt.Image = My.Resources.F9
            lkt.BackColor = Color.White
            lkt.ForeColor = Color.Black
            lkt.Padding = New Padding(0)
            AddHandler lkt.Click, AddressOf q273
            AddHandler lkt.MouseHover, AddressOf q274
            hi.DropDownItems.Insert(35, lkt)
            Dim tct As New ToolStripMenuItem
            tct.Text = "Show Tab Count"
            tct.Image = My.Resources.F9
            tct.BackColor = Color.White
            tct.ForeColor = Color.Black
            tct.Padding = New Padding(0)
            AddHandler tct.Click, AddressOf q275
            AddHandler tct.MouseHover, AddressOf q276
            he.DropDownItems.Insert(1, tct)
            Dim qnt As New ToolStripMenuItem
            qnt.Text = "Quick Notes"
            qnt.Image = My.Resources.F9
            qnt.BackColor = Color.White
            qnt.ForeColor = Color.Black
            qnt.Padding = New Padding(0)
            AddHandler qnt.Click, AddressOf q277
            AddHandler qnt.MouseHover, AddressOf q278
            ot.DropDownItems.Insert(7, qnt)
            Dim nip As New ToolStripMenuItem
            nip.Text = "Navigate To IP Address"
            nip.Image = My.Resources.F9
            nip.BackColor = Color.White
            nip.ForeColor = Color.Black
            nip.Padding = New Padding(0)
            AddHandler nip.Click, AddressOf q279
            AddHandler nip.MouseHover, AddressOf q280
            fl.DropDownItems.Insert(38, nip)
            Dim bsn As New ToolStripMenuItem
            bsn.Text = "Bookmark Selected Text As Note"
            bsn.Image = My.Resources.F9
            bsn.BackColor = Color.White
            bsn.ForeColor = Color.Black
            bsn.Padding = New Padding(0)
            AddHandler bsn.Click, AddressOf q281
            AddHandler bsn.MouseHover, AddressOf q282
            bo.DropDownItems.Insert(16, bsn)
            Dim czl As New ToolStripMenuItem
            czl.Text = "Copy Current Zoom Level"
            czl.Image = My.Resources.F9
            czl.BackColor = Color.White
            czl.ForeColor = Color.Black
            czl.Padding = New Padding(0)
            AddHandler czl.Click, AddressOf q283
            AddHandler czl.MouseHover, AddressOf q284
            v.DropDownItems.Insert(10, czl)
            Dim ipt As New ToolStripMenuItem
            ipt.Text = "Invert Page Colors"
            ipt.Image = My.Resources.F9
            ipt.BackColor = Color.White
            ipt.ForeColor = Color.Black
            ipt.Padding = New Padding(0)
            AddHandler ipt.Click, AddressOf q285
            AddHandler ipt.MouseHover, AddressOf q286
            v.DropDownItems.Insert(11, ipt)
            Dim fsp As New ToolStripMenuItem
            fsp.Text = "Focus Page (Remove Distractions)"
            fsp.Image = My.Resources.F9
            fsp.BackColor = Color.White
            fsp.ForeColor = Color.Black
            fsp.Padding = New Padding(0)
            AddHandler fsp.Click, AddressOf q287
            AddHandler fsp.MouseHover, AddressOf q288
            v.DropDownItems.Insert(12, fsp)
            Dim cnt As New ToolStripMenuItem
            cnt.Text = "Copy Page Link Count"
            cnt.Image = My.Resources.F9
            cnt.BackColor = Color.White
            cnt.ForeColor = Color.Black
            cnt.Padding = New Padding(0)
            AddHandler cnt.Click, AddressOf q289
            AddHandler cnt.MouseHover, AddressOf q290
            Me.e.DropDownItems.Insert(31, cnt)
            Dim sfl As New ToolStripMenuItem
            sfl.Text = "Save Page Links As Text"
            sfl.Image = My.Resources.F9
            sfl.BackColor = Color.White
            sfl.ForeColor = Color.Black
            sfl.Padding = New Padding(0)
            AddHandler sfl.Click, AddressOf q291
            AddHandler sfl.MouseHover, AddressOf q292
            fl.DropDownItems.Insert(39, sfl)
            Dim ptp As New ToolStripMenuItem
            ptp.Text = "Pin Tab To Top Of List"
            ptp.Image = My.Resources.F9
            ptp.BackColor = Color.White
            ptp.ForeColor = Color.Black
            ptp.Padding = New Padding(0)
            AddHandler ptp.Click, AddressOf q293
            AddHandler ptp.MouseHover, AddressOf q294
            hi.DropDownItems.Insert(36, ptp)
            Dim bft As New ToolStripMenuItem
            bft.Text = "Bookmark All Filtered Tabs"
            bft.Image = My.Resources.F9
            bft.BackColor = Color.White
            bft.ForeColor = Color.Black
            bft.Padding = New Padding(0)
            AddHandler bft.Click, AddressOf q295
            AddHandler bft.MouseHover, AddressOf q296
            bo.DropDownItems.Insert(17, bft)
            Dim cpc As New ToolStripMenuItem
            cpc.Text = "Copy Page Character Count"
            cpc.Image = My.Resources.F9
            cpc.BackColor = Color.White
            cpc.ForeColor = Color.Black
            cpc.Padding = New Padding(0)
            AddHandler cpc.Click, AddressOf q297
            AddHandler cpc.MouseHover, AddressOf q298
            Me.e.DropDownItems.Insert(32, cpc)
            Dim osn As New ToolStripMenuItem
            osn.Text = "Open All Tabs From Same Domain In New Window"
            osn.Image = My.Resources.F9
            osn.BackColor = Color.White
            osn.ForeColor = Color.Black
            osn.Padding = New Padding(0)
            AddHandler osn.Click, AddressOf q299
            AddHandler osn.MouseHover, AddressOf q300
            hi.DropDownItems.Insert(37, osn)
            Dim aqn As New ToolStripMenuItem
            aqn.Text = "Append To Quick Notes"
            aqn.Image = My.Resources.F9
            aqn.BackColor = Color.White
            aqn.ForeColor = Color.Black
            aqn.Padding = New Padding(0)
            AddHandler aqn.Click, AddressOf q301
            AddHandler aqn.MouseHover, AddressOf q302
            ot.DropDownItems.Insert(8, aqn)
            Dim vbs As New ToolStripMenuItem
            vbs.Text = "View Browser Session Summary"
            vbs.Image = My.Resources.F9
            vbs.BackColor = Color.White
            vbs.ForeColor = Color.Black
            vbs.Padding = New Padding(0)
            AddHandler vbs.Click, AddressOf q303
            AddHandler vbs.MouseHover, AddressOf q304
            he.DropDownItems.Insert(2, vbs)
            Dim hlt As New ToolStripMenuItem
            hlt.Text = "Highlight Text On Page"
            hlt.Image = My.Resources.F9
            hlt.BackColor = Color.White
            hlt.ForeColor = Color.Black
            hlt.Padding = New Padding(0)
            AddHandler hlt.Click, AddressOf q305
            AddHandler hlt.MouseHover, AddressOf q306
            v.DropDownItems.Insert(13, hlt)
            Dim spj As New ToolStripMenuItem
            spj.Text = "Save Page As JSON Snapshot"
            spj.Image = My.Resources.F9
            spj.BackColor = Color.White
            spj.ForeColor = Color.Black
            spj.Padding = New Padding(0)
            AddHandler spj.Click, AddressOf q307
            AddHandler spj.MouseHover, AddressOf q308
            fl.DropDownItems.Insert(40, spj)
            Dim csl As New ToolStripMenuItem
            csl.Text = "Copy Selected Text As Link"
            csl.Image = My.Resources.F9
            csl.BackColor = Color.White
            csl.ForeColor = Color.Black
            csl.Padding = New Padding(0)
            AddHandler csl.Click, AddressOf q309
            AddHandler csl.MouseHover, AddressOf q310
            Me.e.DropDownItems.Insert(33, csl)
            Dim obn As New ToolStripMenuItem
            obn.Text = "Open Bookmark In New Tab"
            obn.Image = My.Resources.F9
            obn.BackColor = Color.White
            obn.ForeColor = Color.Black
            obn.Padding = New Padding(0)
            AddHandler obn.Click, AddressOf q311
            AddHandler obn.MouseHover, AddressOf q312
            bo.DropDownItems.Insert(18, obn)
            Dim ctc As New ToolStripMenuItem
            ctc.Text = "Close Tabs To The Left"
            ctc.Image = My.Resources.F9
            ctc.BackColor = Color.White
            ctc.ForeColor = Color.Black
            ctc.Padding = New Padding(0)
            AddHandler ctc.Click, AddressOf q313
            AddHandler ctc.MouseHover, AddressOf q314
            fl.DropDownItems.Insert(41, ctc)
            Dim snt As New ToolStripMenuItem
            snt.Text = "Show Notes In Status Bar"
            snt.Image = My.Resources.F9
            snt.BackColor = Color.White
            snt.ForeColor = Color.Black
            snt.Padding = New Padding(0)
            AddHandler snt.Click, AddressOf q315
            AddHandler snt.MouseHover, AddressOf q316
            ot.DropDownItems.Insert(9, snt)
            Dim cti As New ToolStripMenuItem
            cti.Text = "Copy Tab Index"
            cti.Image = My.Resources.F9
            cti.BackColor = Color.White
            cti.ForeColor = Color.Black
            cti.Padding = New Padding(0)
            AddHandler cti.Click, AddressOf q317
            AddHandler cti.MouseHover, AddressOf q318
            hi.DropDownItems.Insert(38, cti)
            Dim gft As New ToolStripMenuItem
            gft.Text = "Go To First Tab"
            gft.Image = My.Resources.F9
            gft.BackColor = Color.White
            gft.ForeColor = Color.Black
            gft.Padding = New Padding(0)
            AddHandler gft.Click, AddressOf q319
            AddHandler gft.MouseHover, AddressOf q320
            hi.DropDownItems.Insert(39, gft)
            Dim glt As New ToolStripMenuItem
            glt.Text = "Go To Last Tab"
            glt.Image = My.Resources.F9
            glt.BackColor = Color.White
            glt.ForeColor = Color.Black
            glt.Padding = New Padding(0)
            AddHandler glt.Click, AddressOf q321
            AddHandler glt.MouseHover, AddressOf q322
            hi.DropDownItems.Insert(40, glt)
            Dim sbp As New ToolStripMenuItem
            sbp.Text = "Save Bookmarks As Plain Text"
            sbp.Image = My.Resources.F9
            sbp.BackColor = Color.White
            sbp.ForeColor = Color.Black
            sbp.Padding = New Padding(0)
            AddHandler sbp.Click, AddressOf q323
            AddHandler sbp.MouseHover, AddressOf q324
            bo.DropDownItems.Insert(19, sbp)
            Dim dpn As New ToolStripMenuItem
            dpn.Text = "Duplicate Tab In New Window"
            dpn.Image = My.Resources.F9
            dpn.BackColor = Color.White
            dpn.ForeColor = Color.Black
            dpn.Padding = New Padding(0)
            AddHandler dpn.Click, AddressOf q325
            AddHandler dpn.MouseHover, AddressOf q326
            fl.DropDownItems.Insert(42, dpn)
            Dim scb As New ToolStripMenuItem
            scb.Text = "Scroll Page By Percentage"
            scb.Image = My.Resources.F9
            scb.BackColor = Color.White
            scb.ForeColor = Color.Black
            scb.Padding = New Padding(0)
            AddHandler scb.Click, AddressOf q327
            AddHandler scb.MouseHover, AddressOf q328
            v.DropDownItems.Insert(14, scb)
            Dim cep As New ToolStripMenuItem
            cep.Text = "Copy Page Email Addresses"
            cep.Image = My.Resources.F9
            cep.BackColor = Color.White
            cep.ForeColor = Color.Black
            cep.Padding = New Padding(0)
            AddHandler cep.Click, AddressOf q329
            AddHandler cep.MouseHover, AddressOf q330
            Me.e.DropDownItems.Insert(34, cep)
            Dim stk As New ToolStripMenuItem
            stk.Text = "Save Quick Notes To File"
            stk.Image = My.Resources.F9
            stk.BackColor = Color.White
            stk.ForeColor = Color.Black
            stk.Padding = New Padding(0)
            AddHandler stk.Click, AddressOf q331
            AddHandler stk.MouseHover, AddressOf q332
            ot.DropDownItems.Insert(10, stk)
            Dim swt As New ToolStripMenuItem
            swt.Text = "Swap Current Tab With Next"
            swt.Image = My.Resources.F9
            swt.BackColor = Color.White
            swt.ForeColor = Color.Black
            swt.Padding = New Padding(0)
            AddHandler swt.Click, AddressOf q333
            AddHandler swt.MouseHover, AddressOf q334
            hi.DropDownItems.Insert(41, swt)
            Dim cbj As New ToolStripMenuItem
            cbj.Text = "Copy Bookmarks As JSON"
            cbj.Image = My.Resources.F9
            cbj.BackColor = Color.White
            cbj.ForeColor = Color.Black
            cbj.Padding = New Padding(0)
            AddHandler cbj.Click, AddressOf q335
            AddHandler cbj.MouseHover, AddressOf q336
            bo.DropDownItems.Insert(20, cbj)
            Dim rql As New ToolStripMenuItem
            rql.Text = "Clear Quick Notes"
            rql.Image = My.Resources.F9
            rql.BackColor = Color.White
            rql.ForeColor = Color.Black
            rql.Padding = New Padding(0)
            AddHandler rql.Click, AddressOf q337
            AddHandler rql.MouseHover, AddressOf q338
            ot.DropDownItems.Insert(11, rql)
            Dim cth As New ToolStripMenuItem
            cth.Text = "Copy Page Headings"
            cth.Image = My.Resources.F9
            cth.BackColor = Color.White
            cth.ForeColor = Color.Black
            cth.Padding = New Padding(0)
            AddHandler cth.Click, AddressOf q339
            AddHandler cth.MouseHover, AddressOf q340
            Me.e.DropDownItems.Insert(35, cth)
            Dim dlt As New ToolStripMenuItem
            dlt.Text = "Delete Selected Bookmark"
            dlt.Image = My.Resources.F9
            dlt.BackColor = Color.White
            dlt.ForeColor = Color.Black
            dlt.Padding = New Padding(0)
            AddHandler dlt.Click, AddressOf q341
            AddHandler dlt.MouseHover, AddressOf q342
            bo.DropDownItems.Insert(21, dlt)
            Dim shc As New ToolStripMenuItem
            shc.Text = "Show Keyboard Shortcuts"
            shc.Image = My.Resources.F9
            shc.BackColor = Color.White
            shc.ForeColor = Color.Black
            shc.Padding = New Padding(0)
            AddHandler shc.Click, AddressOf q343
            AddHandler shc.MouseHover, AddressOf q344
            he.DropDownItems.Insert(3, shc)
            Dim rpt As New ToolStripMenuItem
            rpt.Text = "Reload Tab Every 30 Seconds"
            rpt.Image = My.Resources.F9
            rpt.BackColor = Color.White
            rpt.ForeColor = Color.Black
            rpt.Padding = New Padding(0)
            AddHandler rpt.Click, AddressOf q345
            AddHandler rpt.MouseHover, AddressOf q346
            v.DropDownItems.Insert(15, rpt)
            Dim sph2 As New ToolStripMenuItem
            sph2.Text = "Save History As Plain Text"
            sph2.Image = My.Resources.F9
            sph2.BackColor = Color.White
            sph2.ForeColor = Color.Black
            sph2.Padding = New Padding(0)
            AddHandler sph2.Click, AddressOf q347
            AddHandler sph2.MouseHover, AddressOf q348
            fl.DropDownItems.Insert(43, sph2)
            Dim cnp As New ToolStripMenuItem
            cnp.Text = "Copy Page Numbers"
            cnp.Image = My.Resources.F9
            cnp.BackColor = Color.White
            cnp.ForeColor = Color.Black
            cnp.Padding = New Padding(0)
            AddHandler cnp.Click, AddressOf q349
            AddHandler cnp.MouseHover, AddressOf q350
            Me.e.DropDownItems.Insert(36, cnp)
            Dim oln As New ToolStripMenuItem
            oln.Text = "Open Links From Clipboard In Tabs"
            oln.Image = My.Resources.F9
            oln.BackColor = Color.White
            oln.ForeColor = Color.Black
            oln.Padding = New Padding(0)
            AddHandler oln.Click, AddressOf q351
            AddHandler oln.MouseHover, AddressOf q352
            fl.DropDownItems.Insert(44, oln)
            Dim bkn As New ToolStripMenuItem
            bkn.Text = "Bookmark Current Tab With Note"
            bkn.Image = My.Resources.F9
            bkn.BackColor = Color.White
            bkn.ForeColor = Color.Black
            bkn.Padding = New Padding(0)
            AddHandler bkn.Click, AddressOf q353
            AddHandler bkn.MouseHover, AddressOf q354
            bo.DropDownItems.Insert(22, bkn)
            Dim cnu As New ToolStripMenuItem
            cnu.Text = "Copy Next Tab URL"
            cnu.Image = My.Resources.F9
            cnu.BackColor = Color.White
            cnu.ForeColor = Color.Black
            cnu.Padding = New Padding(0)
            AddHandler cnu.Click, AddressOf q355
            AddHandler cnu.MouseHover, AddressOf q356
            hi.DropDownItems.Insert(42, cnu)
            Dim ihn As New ToolStripMenuItem
            ihn.Text = "Import Quick Notes From File"
            ihn.Image = My.Resources.F9
            ihn.BackColor = Color.White
            ihn.ForeColor = Color.Black
            ihn.Padding = New Padding(0)
            AddHandler ihn.Click, AddressOf q357
            AddHandler ihn.MouseHover, AddressOf q358
            ot.DropDownItems.Insert(12, ihn)
            Dim cpv As New ToolStripMenuItem
            cpv.Text = "Copy Previous Tab URL"
            cpv.Image = My.Resources.F9
            cpv.BackColor = Color.White
            cpv.ForeColor = Color.Black
            cpv.Padding = New Padding(0)
            AddHandler cpv.Click, AddressOf q359
            AddHandler cpv.MouseHover, AddressOf q360
            hi.DropDownItems.Insert(43, cpv)
            Dim shp As New ToolStripMenuItem
            shp.Text = "Show Page Load Progress"
            shp.Image = My.Resources.F9
            shp.BackColor = Color.White
            shp.ForeColor = Color.Black
            shp.Padding = New Padding(0)
            AddHandler shp.Click, AddressOf q361
            AddHandler shp.MouseHover, AddressOf q362
            he.DropDownItems.Insert(4, shp)
            Dim sfb As New ToolStripMenuItem
            sfb.Text = "Search Bookmarks"
            sfb.Image = My.Resources.F9
            sfb.BackColor = Color.White
            sfb.ForeColor = Color.Black
            sfb.Padding = New Padding(0)
            AddHandler sfb.Click, AddressOf q363
            AddHandler sfb.MouseHover, AddressOf q364
            bo.DropDownItems.Insert(23, sfb)
            Dim chs2 As New ToolStripMenuItem
            chs2.Text = "Search History"
            chs2.Image = My.Resources.F9
            chs2.BackColor = Color.White
            chs2.ForeColor = Color.Black
            chs2.Padding = New Padding(0)
            AddHandler chs2.Click, AddressOf q365
            AddHandler chs2.MouseHover, AddressOf q366
            hi.DropDownItems.Insert(44, chs2)
            Dim pgs As New ToolStripMenuItem
            pgs.Text = "Print Page (Silent)"
            pgs.Image = My.Resources.F9
            pgs.BackColor = Color.White
            pgs.ForeColor = Color.Black
            pgs.Padding = New Padding(0)
            AddHandler pgs.Click, AddressOf q367
            AddHandler pgs.MouseHover, AddressOf q368
            fl.DropDownItems.Insert(45, pgs)
            Dim cbi As New ToolStripMenuItem
            cbi.Text = "Copy Page Image Count"
            cbi.Image = My.Resources.F9
            cbi.BackColor = Color.White
            cbi.ForeColor = Color.Black
            cbi.Padding = New Padding(0)
            AddHandler cbi.Click, AddressOf q369
            AddHandler cbi.MouseHover, AddressOf q370
            Me.e.DropDownItems.Insert(37, cbi)
            Dim ott As New ToolStripMenuItem
            ott.Text = "Open All History In Tabs"
            ott.Image = My.Resources.F9
            ott.BackColor = Color.White
            ott.ForeColor = Color.Black
            ott.Padding = New Padding(0)
            AddHandler ott.Click, AddressOf q371
            AddHandler ott.MouseHover, AddressOf q372
            hi.DropDownItems.Insert(45, ott)
            Dim sct As New ToolStripMenuItem
            sct.Text = "Set Custom Tab Color"
            sct.Image = My.Resources.F9
            sct.BackColor = Color.White
            sct.ForeColor = Color.Black
            sct.Padding = New Padding(0)
            AddHandler sct.Click, AddressOf q373
            AddHandler sct.MouseHover, AddressOf q374
            v.DropDownItems.Insert(16, sct)
            Dim cfl As New ToolStripMenuItem
            cfl.Text = "Copy Full Page Text As Lines"
            cfl.Image = My.Resources.F9
            cfl.BackColor = Color.White
            cfl.ForeColor = Color.Black
            cfl.Padding = New Padding(0)
            AddHandler cfl.Click, AddressOf q375
            AddHandler cfl.MouseHover, AddressOf q376
            Me.e.DropDownItems.Insert(38, cfl)
            Dim snw As New ToolStripMenuItem
            snw.Text = "Save Quick Notes With Timestamp"
            snw.Image = My.Resources.F9
            snw.BackColor = Color.White
            snw.ForeColor = Color.Black
            snw.Padding = New Padding(0)
            AddHandler snw.Click, AddressOf q377
            AddHandler snw.MouseHover, AddressOf q378
            ot.DropDownItems.Insert(13, snw)
            Dim obl As New ToolStripMenuItem
            obl.Text = "Open Bookmark By Number"
            obl.Image = My.Resources.F9
            obl.BackColor = Color.White
            obl.ForeColor = Color.Black
            obl.Padding = New Padding(0)
            AddHandler obl.Click, AddressOf q379
            AddHandler obl.MouseHover, AddressOf q380
            bo.DropDownItems.Insert(24, obl)
            Dim stt As New ToolStripMenuItem
            stt.Text = "Show Total Tabs Opened Count"
            stt.Image = My.Resources.F9
            stt.BackColor = Color.White
            stt.ForeColor = Color.Black
            stt.Padding = New Padding(0)
            AddHandler stt.Click, AddressOf q381
            AddHandler stt.MouseHover, AddressOf q382
            he.DropDownItems.Insert(5, stt)
            Dim cpl2 As New ToolStripMenuItem
            cpl2.Text = "Copy Page Last Modified"
            cpl2.Image = My.Resources.F9
            cpl2.BackColor = Color.White
            cpl2.ForeColor = Color.Black
            cpl2.Padding = New Padding(0)
            AddHandler cpl2.Click, AddressOf q383
            AddHandler cpl2.MouseHover, AddressOf q384
            Me.e.DropDownItems.Insert(39, cpl2)
            Dim gtn As New ToolStripMenuItem
            gtn.Text = "Go To Tab By Number"
            gtn.Image = My.Resources.F9
            gtn.BackColor = Color.White
            gtn.ForeColor = Color.Black
            gtn.Padding = New Padding(0)
            AddHandler gtn.Click, AddressOf q385
            AddHandler gtn.MouseHover, AddressOf q386
            hi.DropDownItems.Insert(46, gtn)
            Dim sph3 As New ToolStripMenuItem
            sph3.Text = "Save Page As Markdown"
            sph3.Image = My.Resources.F9
            sph3.BackColor = Color.White
            sph3.ForeColor = Color.Black
            sph3.Padding = New Padding(0)
            AddHandler sph3.Click, AddressOf q387
            AddHandler sph3.MouseHover, AddressOf q388
            fl.DropDownItems.Insert(46, sph3)
            Dim cpd As New ToolStripMenuItem
            cpd.Text = "Copy Page Description"
            cpd.Image = My.Resources.F9
            cpd.BackColor = Color.White
            cpd.ForeColor = Color.Black
            cpd.Padding = New Padding(0)
            AddHandler cpd.Click, AddressOf q389
            AddHandler cpd.MouseHover, AddressOf q390
            Me.e.DropDownItems.Insert(40, cpd)
            Dim trc As New ToolStripMenuItem
            trc.Text = "Translate Page (Google)"
            trc.Image = My.Resources.F9
            trc.BackColor = Color.White
            trc.ForeColor = Color.Black
            trc.Padding = New Padding(0)
            AddHandler trc.Click, AddressOf q391
            AddHandler trc.MouseHover, AddressOf q392
            v.DropDownItems.Insert(17, trc)
            Dim mbt As New ToolStripMenuItem
            mbt.Text = "Merge All Bookmarks To Clipboard"
            mbt.Image = My.Resources.F9
            mbt.BackColor = Color.White
            mbt.ForeColor = Color.Black
            mbt.Padding = New Padding(0)
            AddHandler mbt.Click, AddressOf q393
            AddHandler mbt.MouseHover, AddressOf q394
            bo.DropDownItems.Insert(25, mbt)
            Dim rnq As New ToolStripMenuItem
            rnq.Text = "Read Quick Notes Aloud"
            rnq.Image = My.Resources.F9
            rnq.BackColor = Color.White
            rnq.ForeColor = Color.Black
            rnq.Padding = New Padding(0)
            AddHandler rnq.Click, AddressOf q395
            AddHandler rnq.MouseHover, AddressOf q396
            ot.DropDownItems.Insert(14, rnq)
            Dim wce As New ToolStripMenuItem
            wce.Text = "Open Wayback Machine For Page"
            wce.Image = My.Resources.F9
            wce.BackColor = Color.White
            wce.ForeColor = Color.Black
            wce.Padding = New Padding(0)
            AddHandler wce.Click, AddressOf q397
            AddHandler wce.MouseHover, AddressOf q398
            hi.DropDownItems.Insert(47, wce)
            Dim cbk As New ToolStripMenuItem
            cbk.Text = "Copy Bookmark Count"
            cbk.Image = My.Resources.F9
            cbk.BackColor = Color.White
            cbk.ForeColor = Color.Black
            cbk.Padding = New Padding(0)
            AddHandler cbk.Click, AddressOf q399
            AddHandler cbk.MouseHover, AddressOf q400
            he.DropDownItems.Insert(6, cbk)
            Dim stf As New ToolStripMenuItem
            stf.Text = "Save All Tab Titles To File"
            stf.Image = My.Resources.F9
            stf.BackColor = Color.White
            stf.ForeColor = Color.Black
            stf.Padding = New Padding(0)
            AddHandler stf.Click, AddressOf q401
            AddHandler stf.MouseHover, AddressOf q402
            fl.DropDownItems.Insert(47, stf)
            Dim wsx As New ToolStripMenuItem
            wsx.Text = "Open Web Search For Selected Text"
            wsx.Image = My.Resources.F9
            wsx.BackColor = Color.White
            wsx.ForeColor = Color.Black
            wsx.Padding = New Padding(0)
            AddHandler wsx.Click, AddressOf q403
            AddHandler wsx.MouseHover, AddressOf q404
            Me.e.DropDownItems.Insert(41, wsx)
            Dim cpc2 As New ToolStripMenuItem
            cpc2.Text = "Copy Page Canonical URL"
            cpc2.Image = My.Resources.F9
            cpc2.BackColor = Color.White
            cpc2.ForeColor = Color.Black
            cpc2.Padding = New Padding(0)
            AddHandler cpc2.Click, AddressOf q405
            AddHandler cpc2.MouseHover, AddressOf q406
            Me.e.DropDownItems.Insert(42, cpc2)
            Dim spq As New ToolStripMenuItem
            spq.Text = "Share Page Via QR Code"
            spq.Image = My.Resources.F9
            spq.BackColor = Color.White
            spq.ForeColor = Color.Black
            spq.Padding = New Padding(0)
            AddHandler spq.Click, AddressOf q407
            AddHandler spq.MouseHover, AddressOf q408
            fl.DropDownItems.Insert(48, spq)
            Dim snt2 As New ToolStripMenuItem
            snt2.Text = "Sort History Alphabetically"
            snt2.Image = My.Resources.F9
            snt2.BackColor = Color.White
            snt2.ForeColor = Color.Black
            snt2.Padding = New Padding(0)
            AddHandler snt2.Click, AddressOf q409
            AddHandler snt2.MouseHover, AddressOf q410
            hi.DropDownItems.Insert(48, snt2)
            Dim cpk As New ToolStripMenuItem
            cpk.Text = "Copy Page Keywords"
            cpk.Image = My.Resources.F9
            cpk.BackColor = Color.White
            cpk.ForeColor = Color.Black
            cpk.Padding = New Padding(0)
            AddHandler cpk.Click, AddressOf q411
            AddHandler cpk.MouseHover, AddressOf q412
            Me.e.DropDownItems.Insert(43, cpk)
            Dim oar As New ToolStripMenuItem
            oar.Text = "Open Archive.org For Page"
            oar.Image = My.Resources.F9
            oar.BackColor = Color.White
            oar.ForeColor = Color.Black
            oar.Padding = New Padding(0)
            AddHandler oar.Click, AddressOf q413
            AddHandler oar.MouseHover, AddressOf q414
            hi.DropDownItems.Insert(49, oar)
            Dim sws2 As New ToolStripMenuItem
            sws2.Text = "Swap Tab With Previous"
            sws2.Image = My.Resources.F9
            sws2.BackColor = Color.White
            sws2.ForeColor = Color.Black
            sws2.Padding = New Padding(0)
            AddHandler sws2.Click, AddressOf q415
            AddHandler sws2.MouseHover, AddressOf q416
            hi.DropDownItems.Insert(50, sws2)
            Dim cnq As New ToolStripMenuItem
            cnq.Text = "Copy Quick Notes To Clipboard"
            cnq.Image = My.Resources.F9
            cnq.BackColor = Color.White
            cnq.ForeColor = Color.Black
            cnq.Padding = New Padding(0)
            AddHandler cnq.Click, AddressOf q417
            AddHandler cnq.MouseHover, AddressOf q418
            ot.DropDownItems.Insert(15, cnq)
            Dim rvb As New ToolStripMenuItem
            rvb.Text = "Reload All Tabs From Same Domain"
            rvb.Image = My.Resources.F9
            rvb.BackColor = Color.White
            rvb.ForeColor = Color.Black
            rvb.Padding = New Padding(0)
            AddHandler rvb.Click, AddressOf q419
            AddHandler rvb.MouseHover, AddressOf q420
            v.DropDownItems.Insert(18, rvb)
            Dim sbd As New ToolStripMenuItem
            sbd.Text = "Show Browser Statistics"
            sbd.Image = My.Resources.F9
            sbd.BackColor = Color.White
            sbd.ForeColor = Color.Black
            sbd.Padding = New Padding(0)
            AddHandler sbd.Click, AddressOf q421
            AddHandler sbd.MouseHover, AddressOf q422
            he.DropDownItems.Insert(7, sbd)
            Dim cfv As New ToolStripMenuItem
            cfv.Text = "Copy Favicon URL"
            cfv.Image = My.Resources.F9
            cfv.BackColor = Color.White
            cfv.ForeColor = Color.Black
            cfv.Padding = New Padding(0)
            AddHandler cfv.Click, AddressOf q423
            AddHandler cfv.MouseHover, AddressOf q424
            Me.e.DropDownItems.Insert(44, cfv)
            Dim rbm As New ToolStripMenuItem
            rbm.Text = "Reverse Bookmark Order"
            rbm.Image = My.Resources.F9
            rbm.BackColor = Color.White
            rbm.ForeColor = Color.Black
            rbm.Padding = New Padding(0)
            AddHandler rbm.Click, AddressOf q425
            AddHandler rbm.MouseHover, AddressOf q426
            bo.DropDownItems.Insert(26, rbm)
            Dim sph4 As New ToolStripMenuItem
            sph4.Text = "Save Page As CSV Table"
            sph4.Image = My.Resources.F9
            sph4.BackColor = Color.White
            sph4.ForeColor = Color.Black
            sph4.Padding = New Padding(0)
            AddHandler sph4.Click, AddressOf q427
            AddHandler sph4.MouseHover, AddressOf q428
            fl.DropDownItems.Insert(49, sph4)
            Dim opt As New ToolStripMenuItem
            opt.Text = "Open Page In New Private Tab"
            opt.Image = My.Resources.F9
            opt.BackColor = Color.White
            opt.ForeColor = Color.Black
            opt.Padding = New Padding(0)
            AddHandler opt.Click, AddressOf q429
            AddHandler opt.MouseHover, AddressOf q430
            fl.DropDownItems.Insert(50, opt)
            Dim cog As New ToolStripMenuItem
            cog.Text = "Copy Open Tab Count"
            cog.Image = My.Resources.F9
            cog.BackColor = Color.White
            cog.ForeColor = Color.Black
            cog.Padding = New Padding(0)
            AddHandler cog.Click, AddressOf q431
            AddHandler cog.MouseHover, AddressOf q432
            he.DropDownItems.Insert(8, cog)
            Dim shf As New ToolStripMenuItem
            shf.Text = "Shuffle Tab Order"
            shf.Image = My.Resources.F9
            shf.BackColor = Color.White
            shf.ForeColor = Color.Black
            shf.Padding = New Padding(0)
            AddHandler shf.Click, AddressOf q433
            AddHandler shf.MouseHover, AddressOf q434
            hi.DropDownItems.Insert(51, shf)
            Dim cpog As New ToolStripMenuItem
            cpog.Text = "Copy Open Graph Tags"
            cpog.Image = My.Resources.F9
            cpog.BackColor = Color.White
            cpog.ForeColor = Color.Black
            cpog.Padding = New Padding(0)
            AddHandler cpog.Click, AddressOf q435
            AddHandler cpog.MouseHover, AddressOf q436
            Me.e.DropDownItems.Insert(45, cpog)
            Dim srh As New ToolStripMenuItem
            srh.Text = "Speak Current Page Title"
            srh.Image = My.Resources.F9
            srh.BackColor = Color.White
            srh.ForeColor = Color.Black
            srh.Padding = New Padding(0)
            AddHandler srh.Click, AddressOf q437
            AddHandler srh.MouseHover, AddressOf q438
            v.DropDownItems.Insert(19, srh)
            Dim ipn As New ToolStripMenuItem
            ipn.Text = "Insert Page Note Into Quick Notes"
            ipn.Image = My.Resources.F9
            ipn.BackColor = Color.White
            ipn.ForeColor = Color.Black
            ipn.Padding = New Padding(0)
            AddHandler ipn.Click, AddressOf q439
            AddHandler ipn.MouseHover, AddressOf q440
            ot.DropDownItems.Insert(16, ipn)
            Dim chc As New ToolStripMenuItem
            chc.Text = "Copy History Count"
            chc.Image = My.Resources.F9
            chc.BackColor = Color.White
            chc.ForeColor = Color.Black
            chc.Padding = New Padding(0)
            AddHandler chc.Click, AddressOf q441
            AddHandler chc.MouseHover, AddressOf q442
            he.DropDownItems.Insert(9, chc)
            Dim ctt2 As New ToolStripMenuItem
            ctt2.Text = "Copy Current Tab Title"
            ctt2.Image = My.Resources.F9
            ctt2.BackColor = Color.White
            ctt2.ForeColor = Color.Black
            ctt2.Padding = New Padding(0)
            AddHandler ctt2.Click, AddressOf q443
            AddHandler ctt2.MouseHover, AddressOf q444
            Me.e.DropDownItems.Insert(45, ctt2)
            Dim opu As New ToolStripMenuItem
            opu.Text = "Open Parent URL"
            opu.Image = My.Resources.F9
            opu.BackColor = Color.White
            opu.ForeColor = Color.Black
            opu.Padding = New Padding(0)
            AddHandler opu.Click, AddressOf q445
            AddHandler opu.MouseHover, AddressOf q446
            hi.DropDownItems.Insert(52, opu)
            Dim sbf As New ToolStripMenuItem
            sbf.Text = "Sort Bookmarks By Domain"
            sbf.Image = My.Resources.F9
            sbf.BackColor = Color.White
            sbf.ForeColor = Color.Black
            sbf.Padding = New Padding(0)
            AddHandler sbf.Click, AddressOf q447
            AddHandler sbf.MouseHover, AddressOf q448
            bo.DropDownItems.Insert(27, sbf)
            Dim cpf As New ToolStripMenuItem
            cpf.Text = "Copy Page Font Families"
            cpf.Image = My.Resources.F9
            cpf.BackColor = Color.White
            cpf.ForeColor = Color.Black
            cpf.Padding = New Padding(0)
            AddHandler cpf.Click, AddressOf q449
            AddHandler cpf.MouseHover, AddressOf q450
            Me.e.DropDownItems.Insert(46, cpf)
            Dim spw As New ToolStripMenuItem
            spw.Text = "Speak Page URL"
            spw.Image = My.Resources.F9
            spw.BackColor = Color.White
            spw.ForeColor = Color.Black
            spw.Padding = New Padding(0)
            AddHandler spw.Click, AddressOf q451
            AddHandler spw.MouseHover, AddressOf q452
            v.DropDownItems.Insert(20, spw)
            Dim oht As New ToolStripMenuItem
            oht.Text = "Open All Tabs In Same Window (Reload)"
            oht.Image = My.Resources.F9
            oht.BackColor = Color.White
            oht.ForeColor = Color.Black
            oht.Padding = New Padding(0)
            AddHandler oht.Click, AddressOf q453
            AddHandler oht.MouseHover, AddressOf q454
            hi.DropDownItems.Insert(53, oht)
            Dim eqn As New ToolStripMenuItem
            eqn.Text = "Export Quick Notes As HTML"
            eqn.Image = My.Resources.F9
            eqn.BackColor = Color.White
            eqn.ForeColor = Color.Black
            eqn.Padding = New Padding(0)
            AddHandler eqn.Click, AddressOf q455
            AddHandler eqn.MouseHover, AddressOf q456
            ot.DropDownItems.Insert(17, eqn)
            Dim cph2 As New ToolStripMenuItem
            cph2.Text = "Copy Page HTTP Status Code"
            cph2.Image = My.Resources.F9
            cph2.BackColor = Color.White
            cph2.ForeColor = Color.Black
            cph2.Padding = New Padding(0)
            AddHandler cph2.Click, AddressOf q457
            AddHandler cph2.MouseHover, AddressOf q458
            he.DropDownItems.Insert(10, cph2)
            Dim sfl2 As New ToolStripMenuItem
            sfl2.Text = "Save Bookmark As QR Code"
            sfl2.Image = My.Resources.F9
            sfl2.BackColor = Color.White
            sfl2.ForeColor = Color.Black
            sfl2.Padding = New Padding(0)
            AddHandler sfl2.Click, AddressOf q459
            AddHandler sfl2.MouseHover, AddressOf q460
            bo.DropDownItems.Insert(28, sfl2)
            Dim flt As New ToolStripMenuItem
            flt.Text = "Find Longest Line On Page"
            flt.Image = My.Resources.F9
            flt.BackColor = Color.White
            flt.ForeColor = Color.Black
            flt.Padding = New Padding(0)
            AddHandler flt.Click, AddressOf q461
            AddHandler flt.MouseHover, AddressOf q462
            Me.e.DropDownItems.Insert(47, flt)
            Dim cpr As New ToolStripMenuItem
            cpr.Text = "Copy Page Reading Time"
            cpr.Image = My.Resources.F9
            cpr.BackColor = Color.White
            cpr.ForeColor = Color.Black
            cpr.Padding = New Padding(0)
            AddHandler cpr.Click, AddressOf q463
            AddHandler cpr.MouseHover, AddressOf q464
            Me.e.DropDownItems.Insert(48, cpr)
            Dim osc As New ToolStripMenuItem
            osc.Text = "Open Site:Search For Domain"
            osc.Image = My.Resources.F9
            osc.BackColor = Color.White
            osc.ForeColor = Color.Black
            osc.Padding = New Padding(0)
            AddHandler osc.Click, AddressOf q465
            AddHandler osc.MouseHover, AddressOf q466
            hi.DropDownItems.Insert(54, osc)
            Dim sbl As New ToolStripMenuItem
            sbl.Text = "Save Bookmark List As HTML"
            sbl.Image = My.Resources.F9
            sbl.BackColor = Color.White
            sbl.ForeColor = Color.Black
            sbl.Padding = New Padding(0)
            AddHandler sbl.Click, AddressOf q467
            AddHandler sbl.MouseHover, AddressOf q468
            bo.DropDownItems.Insert(29, sbl)
            Dim ctb As New ToolStripMenuItem
            ctb.Text = "Clone Tab To Background"
            ctb.Image = My.Resources.F9
            ctb.BackColor = Color.White
            ctb.ForeColor = Color.Black
            ctb.Padding = New Padding(0)
            AddHandler ctb.Click, AddressOf q469
            AddHandler ctb.MouseHover, AddressOf q470
            fl.DropDownItems.Insert(51, ctb)
            Dim cvt As New ToolStripMenuItem
            cvt.Text = "Copy All Tab URLs As Vertical List"
            cvt.Image = My.Resources.F9
            cvt.BackColor = Color.White
            cvt.ForeColor = Color.Black
            cvt.Padding = New Padding(0)
            AddHandler cvt.Click, AddressOf q471
            AddHandler cvt.MouseHover, AddressOf q472
            Me.e.DropDownItems.Insert(49, cvt)
            Dim shv As New ToolStripMenuItem
            shv.Text = "Show Page Viewport Size"
            shv.Image = My.Resources.F9
            shv.BackColor = Color.White
            shv.ForeColor = Color.Black
            shv.Padding = New Padding(0)
            AddHandler shv.Click, AddressOf q473
            AddHandler shv.MouseHover, AddressOf q474
            he.DropDownItems.Insert(11, shv)
            Dim rqn As New ToolStripMenuItem
            rqn.Text = "Replace Text In Quick Notes"
            rqn.Image = My.Resources.F9
            rqn.BackColor = Color.White
            rqn.ForeColor = Color.Black
            rqn.Padding = New Padding(0)
            AddHandler rqn.Click, AddressOf q475
            AddHandler rqn.MouseHover, AddressOf q476
            ot.DropDownItems.Insert(18, rqn)
            Dim jst As New ToolStripMenuItem
            jst.Text = "Jump To Next Tab With Same Domain"
            jst.Image = My.Resources.F9
            jst.BackColor = Color.White
            jst.ForeColor = Color.Black
            jst.Padding = New Padding(0)
            AddHandler jst.Click, AddressOf q477
            AddHandler jst.MouseHover, AddressOf q478
            hi.DropDownItems.Insert(55, jst)
            Dim dvt As New ToolStripMenuItem
            dvt.Text = "Dark Mode Toggle"
            dvt.Image = My.Resources.F9
            dvt.BackColor = Color.White
            dvt.ForeColor = Color.Black
            dvt.Padding = New Padding(0)
            AddHandler dvt.Click, AddressOf q479
            AddHandler dvt.MouseHover, AddressOf q480
            v.DropDownItems.Insert(21, dvt)
            Dim spd As New ToolStripMenuItem
            spd.Text = "Show Page Script Count"
            spd.Image = My.Resources.F9
            spd.BackColor = Color.White
            spd.ForeColor = Color.Black
            spd.Padding = New Padding(0)
            AddHandler spd.Click, AddressOf q481
            AddHandler spd.MouseHover, AddressOf q482
            he.DropDownItems.Insert(12, spd)
            Dim snr As New ToolStripMenuItem
            snr.Text = "Strip Numbers From Page Text"
            snr.Image = My.Resources.F9
            snr.BackColor = Color.White
            snr.ForeColor = Color.Black
            snr.Padding = New Padding(0)
            AddHandler snr.Click, AddressOf q483
            AddHandler snr.MouseHover, AddressOf q484
            Me.e.DropDownItems.Insert(50, snr)
            Dim oph As New ToolStripMenuItem
            oph.Text = "Open Page In Hex Viewer"
            oph.Image = My.Resources.F9
            oph.BackColor = Color.White
            oph.ForeColor = Color.Black
            oph.Padding = New Padding(0)
            AddHandler oph.Click, AddressOf q485
            AddHandler oph.MouseHover, AddressOf q486
            fl.DropDownItems.Insert(52, oph)
            Dim bth As New ToolStripMenuItem
            bth.Text = "Bookmark Tab History Item"
            bth.Image = My.Resources.F9
            bth.BackColor = Color.White
            bth.ForeColor = Color.Black
            bth.Padding = New Padding(0)
            AddHandler bth.Click, AddressOf q487
            AddHandler bth.MouseHover, AddressOf q488
            bo.DropDownItems.Insert(30, bth)
            Dim csd2 As New ToolStripMenuItem
            csd2.Text = "Copy Page Style Sheet Links"
            csd2.Image = My.Resources.F9
            csd2.BackColor = Color.White
            csd2.ForeColor = Color.Black
            csd2.Padding = New Padding(0)
            AddHandler csd2.Click, AddressOf q489
            AddHandler csd2.MouseHover, AddressOf q490
            Me.e.DropDownItems.Insert(51, csd2)
            Dim swm As New ToolStripMenuItem
            swm.Text = "Save Page Word Frequency"
            swm.Image = My.Resources.F9
            swm.BackColor = Color.White
            swm.ForeColor = Color.Black
            swm.Padding = New Padding(0)
            AddHandler swm.Click, AddressOf q491
            AddHandler swm.MouseHover, AddressOf q492
            fl.DropDownItems.Insert(53, swm)
            Dim tpg As New ToolStripMenuItem
            tpg.Text = "Tile All Tabs In New Window"
            tpg.Image = My.Resources.F9
            tpg.BackColor = Color.White
            tpg.ForeColor = Color.Black
            tpg.Padding = New Padding(0)
            AddHandler tpg.Click, AddressOf q493
            AddHandler tpg.MouseHover, AddressOf q494
            hi.DropDownItems.Insert(56, tpg)
            Dim qnl As New ToolStripMenuItem
            qnl.Text = "Show Quick Notes Line Count"
            qnl.Image = My.Resources.F9
            qnl.BackColor = Color.White
            qnl.ForeColor = Color.Black
            qnl.Padding = New Padding(0)
            AddHandler qnl.Click, AddressOf q495
            AddHandler qnl.MouseHover, AddressOf q496
            ot.DropDownItems.Insert(19, qnl)
            Dim ovd As New ToolStripMenuItem
            ovd.Text = "Open Page Video Links In Tabs"
            ovd.Image = My.Resources.F9
            ovd.BackColor = Color.White
            ovd.ForeColor = Color.Black
            ovd.Padding = New Padding(0)
            AddHandler ovd.Click, AddressOf q497
            AddHandler ovd.MouseHover, AddressOf q498
            hi.DropDownItems.Insert(57, ovd)
            Dim sgp As New ToolStripMenuItem
            sgp.Text = "Save Page As Numbered List"
            sgp.Image = My.Resources.F9
            sgp.BackColor = Color.White
            sgp.ForeColor = Color.Black
            sgp.Padding = New Padding(0)
            AddHandler sgp.Click, AddressOf q499
            AddHandler sgp.MouseHover, AddressOf q500
            fl.DropDownItems.Insert(54, sgp)
            Dim pzl As New ToolStripMenuItem
            pzl.Text = "Show Page Zoom As Percentage"
            pzl.Image = My.Resources.F9
            pzl.BackColor = Color.White
            pzl.ForeColor = Color.Black
            pzl.Padding = New Padding(0)
            AddHandler pzl.Click, AddressOf q501
            AddHandler pzl.MouseHover, AddressOf q502
            v.DropDownItems.Insert(22, pzl)
            Dim shm As New ToolStripMenuItem
            shm.Text = "Show History Item Count"
            shm.Image = My.Resources.F9
            shm.BackColor = Color.White
            shm.ForeColor = Color.Black
            shm.Padding = New Padding(0)
            AddHandler shm.Click, AddressOf q503
            AddHandler shm.MouseHover, AddressOf q504
            he.DropDownItems.Insert(13, shm)
            Dim cnt2 As New ToolStripMenuItem
            cnt2.Text = "Copy Page Link Titles"
            cnt2.Image = My.Resources.F9
            cnt2.BackColor = Color.White
            cnt2.ForeColor = Color.Black
            cnt2.Padding = New Padding(0)
            AddHandler cnt2.Click, AddressOf q505
            AddHandler cnt2.MouseHover, AddressOf q506
            Me.e.DropDownItems.Insert(52, cnt2)
            Dim ntu As New ToolStripMenuItem
            ntu.Text = "Set Custom New Tab URL"
            ntu.Image = My.Resources.F9
            ntu.BackColor = Color.White
            ntu.ForeColor = Color.Black
            ntu.Padding = New Padding(0)
            AddHandler ntu.Click, AddressOf q507
            AddHandler ntu.MouseHover, AddressOf q508
            ot.DropDownItems.Insert(20, ntu)
            Dim ctu As New ToolStripMenuItem
            ctu.Text = "Copy Page Table Data"
            ctu.Image = My.Resources.F9
            ctu.BackColor = Color.White
            ctu.ForeColor = Color.Black
            ctu.Padding = New Padding(0)
            AddHandler ctu.Click, AddressOf q509
            AddHandler ctu.MouseHover, AddressOf q510
            Me.e.DropDownItems.Insert(53, ctu)
            Dim fph As New ToolStripMenuItem
            fph.Text = "Find Text On Page And Count"
            fph.Image = My.Resources.F9
            fph.BackColor = Color.White
            fph.ForeColor = Color.Black
            fph.Padding = New Padding(0)
            AddHandler fph.Click, AddressOf q511
            AddHandler fph.MouseHover, AddressOf q512
            Me.e.DropDownItems.Insert(54, fph)
            Dim tit As New ToolStripMenuItem
            tit.Text = "Set Custom Window Title"
            tit.Image = My.Resources.F9
            tit.BackColor = Color.White
            tit.ForeColor = Color.Black
            tit.Padding = New Padding(0)
            AddHandler tit.Click, AddressOf q513
            AddHandler tit.MouseHover, AddressOf q514
            ot.DropDownItems.Insert(21, tit)
            Dim spu As New ToolStripMenuItem
            spu.Text = "Copy Page Protocol"
            spu.Image = My.Resources.F9
            spu.BackColor = Color.White
            spu.ForeColor = Color.Black
            spu.Padding = New Padding(0)
            AddHandler spu.Click, AddressOf q515
            AddHandler spu.MouseHover, AddressOf q516
            Me.e.DropDownItems.Insert(55, spu)
            Dim tbn As New ToolStripMenuItem
            tbn.Text = "Open All Bookmarks In New Window"
            tbn.Image = My.Resources.F9
            tbn.BackColor = Color.White
            tbn.ForeColor = Color.Black
            tbn.Padding = New Padding(0)
            AddHandler tbn.Click, AddressOf q517
            AddHandler tbn.MouseHover, AddressOf q518
            bo.DropDownItems.Insert(31, tbn)
            Dim shl As New ToolStripMenuItem
            shl.Text = "Show Page Load Time"
            shl.Image = My.Resources.F9
            shl.BackColor = Color.White
            shl.ForeColor = Color.Black
            shl.Padding = New Padding(0)
            AddHandler shl.Click, AddressOf q519
            AddHandler shl.MouseHover, AddressOf q520
            he.DropDownItems.Insert(14, shl)
            Dim obu As New ToolStripMenuItem
            obu.Text = "Open URL From Selected Text"
            obu.Image = My.Resources.F9
            obu.BackColor = Color.White
            obu.ForeColor = Color.Black
            obu.Padding = New Padding(0)
            AddHandler obu.Click, AddressOf q521
            AddHandler obu.MouseHover, AddressOf q522
            Me.e.DropDownItems.Insert(56, obu)
            Dim ctp2 As New ToolStripMenuItem
            ctp2.Text = "Copy Page Path"
            ctp2.Image = My.Resources.F9
            ctp2.BackColor = Color.White
            ctp2.ForeColor = Color.Black
            ctp2.Padding = New Padding(0)
            AddHandler ctp2.Click, AddressOf q523
            AddHandler ctp2.MouseHover, AddressOf q524
            Me.e.DropDownItems.Insert(57, ctp2)
            Dim cpq As New ToolStripMenuItem
            cpq.Text = "Copy Page Query String"
            cpq.Image = My.Resources.F9
            cpq.BackColor = Color.White
            cpq.ForeColor = Color.Black
            cpq.Padding = New Padding(0)
            AddHandler cpq.Click, AddressOf q525
            AddHandler cpq.MouseHover, AddressOf q526
            Me.e.DropDownItems.Insert(58, cpq)
            Dim opf As New ToolStripMenuItem
            opf.Text = "Open Page In New Tab (Force Reload)"
            opf.Image = My.Resources.F9
            opf.BackColor = Color.White
            opf.ForeColor = Color.Black
            opf.Padding = New Padding(0)
            AddHandler opf.Click, AddressOf q527
            AddHandler opf.MouseHover, AddressOf q528
            fl.DropDownItems.Insert(55, opf)
            Dim cul As New ToolStripMenuItem
            cul.Text = "Copy Page URL Without Query"
            cul.Image = My.Resources.F9
            cul.BackColor = Color.White
            cul.ForeColor = Color.Black
            cul.Padding = New Padding(0)
            AddHandler cul.Click, AddressOf q529
            AddHandler cul.MouseHover, AddressOf q530
            Me.e.DropDownItems.Insert(59, cul)
            Dim sbl2 As New ToolStripMenuItem
            sbl2.Text = "Save Bookmark As Markdown Link"
            sbl2.Image = My.Resources.F9
            sbl2.BackColor = Color.White
            sbl2.ForeColor = Color.Black
            sbl2.Padding = New Padding(0)
            AddHandler sbl2.Click, AddressOf q531
            AddHandler sbl2.MouseHover, AddressOf q532
            bo.DropDownItems.Insert(32, sbl2)
            Dim cht As New ToolStripMenuItem
            cht.Text = "Copy All History As Markdown"
            cht.Image = My.Resources.F9
            cht.BackColor = Color.White
            cht.ForeColor = Color.Black
            cht.Padding = New Padding(0)
            AddHandler cht.Click, AddressOf q533
            AddHandler cht.MouseHover, AddressOf q534
            hi.DropDownItems.Insert(58, cht)
            Dim spb As New ToolStripMenuItem
            spb.Text = "Show Page Broken Links"
            spb.Image = My.Resources.F9
            spb.BackColor = Color.White
            spb.ForeColor = Color.Black
            spb.Padding = New Padding(0)
            AddHandler spb.Click, AddressOf q535
            AddHandler spb.MouseHover, AddressOf q536
            he.DropDownItems.Insert(15, spb)
            Dim osb As New ToolStripMenuItem
            osb.Text = "Open Search In New Tab"
            osb.Image = My.Resources.F9
            osb.BackColor = Color.White
            osb.ForeColor = Color.Black
            osb.Padding = New Padding(0)
            AddHandler osb.Click, AddressOf q537
            AddHandler osb.MouseHover, AddressOf q538
            fl.DropDownItems.Insert(56, osb)
            Dim rnb As New ToolStripMenuItem
            rnb.Text = "Rename Selected Bookmark"
            rnb.Image = My.Resources.F9
            rnb.BackColor = Color.White
            rnb.ForeColor = Color.Black
            rnb.Padding = New Padding(0)
            AddHandler rnb.Click, AddressOf q539
            AddHandler rnb.MouseHover, AddressOf q540
            bo.DropDownItems.Insert(33, rnb)
            Dim sph5 As New ToolStripMenuItem
            sph5.Text = "Save Page Links As HTML"
            sph5.Image = My.Resources.F9
            sph5.BackColor = Color.White
            sph5.ForeColor = Color.Black
            sph5.Padding = New Padding(0)
            AddHandler sph5.Click, AddressOf q541
            AddHandler sph5.MouseHover, AddressOf q542
            fl.DropDownItems.Insert(57, sph5)
            Dim cwt As New ToolStripMenuItem
            cwt.Text = "Count Open Tabs By Domain"
            cwt.Image = My.Resources.F9
            cwt.BackColor = Color.White
            cwt.ForeColor = Color.Black
            cwt.Padding = New Padding(0)
            AddHandler cwt.Click, AddressOf q543
            AddHandler cwt.MouseHover, AddressOf q544
            he.DropDownItems.Insert(16, cwt)
            Dim cbr As New ToolStripMenuItem
            cbr.Text = "Copy Browser Version Info"
            cbr.Image = My.Resources.F9
            cbr.BackColor = Color.White
            cbr.ForeColor = Color.Black
            cbr.Padding = New Padding(0)
            AddHandler cbr.Click, AddressOf q545
            AddHandler cbr.MouseHover, AddressOf q546
            he.DropDownItems.Insert(17, cbr)
            Dim oph2 As New ToolStripMenuItem
            oph2.Text = "Open History Item By Number"
            oph2.Image = My.Resources.F9
            oph2.BackColor = Color.White
            oph2.ForeColor = Color.Black
            oph2.Padding = New Padding(0)
            AddHandler oph2.Click, AddressOf q547
            AddHandler oph2.MouseHover, AddressOf q548
            hi.DropDownItems.Insert(59, oph2)
            Dim tbs As New ToolStripMenuItem
            tbs.Text = "Copy Tab Titles As Bullet List"
            tbs.Image = My.Resources.F9
            tbs.BackColor = Color.White
            tbs.ForeColor = Color.Black
            tbs.Padding = New Padding(0)
            AddHandler tbs.Click, AddressOf q549
            AddHandler tbs.MouseHover, AddressOf q550
            Me.e.DropDownItems.Insert(60, tbs)
            Dim ias As New ToolStripMenuItem
            ias.Text = "Insert Address Bar Text Into Quick Notes"
            ias.Image = My.Resources.F9
            ias.BackColor = Color.White
            ias.ForeColor = Color.Black
            ias.Padding = New Padding(0)
            AddHandler ias.Click, AddressOf q551
            AddHandler ias.MouseHover, AddressOf q552
            ot.DropDownItems.Insert(22, ias)
            Dim stx As New ToolStripMenuItem
            stx.Text = "Copy Clean URL"
            stx.Image = My.Resources.F9
            stx.BackColor = Color.White
            stx.ForeColor = Color.Black
            stx.Padding = New Padding(0)
            AddHandler stx.Click, AddressOf q553
            AddHandler stx.MouseHover, AddressOf q554
            Me.e.DropDownItems.Insert(61, stx)
            Dim cpi2 As New ToolStripMenuItem
            cpi2.Text = "Copy Page Input Fields"
            cpi2.Image = My.Resources.F9
            cpi2.BackColor = Color.White
            cpi2.ForeColor = Color.Black
            cpi2.Padding = New Padding(0)
            AddHandler cpi2.Click, AddressOf q555
            AddHandler cpi2.MouseHover, AddressOf q556
            Me.e.DropDownItems.Insert(62, cpi2)
            Dim opu2 As New ToolStripMenuItem
            opu2.Text = "Open Page In New Tab And Bookmark"
            opu2.Image = My.Resources.F9
            opu2.BackColor = Color.White
            opu2.ForeColor = Color.Black
            opu2.Padding = New Padding(0)
            AddHandler opu2.Click, AddressOf q557
            AddHandler opu2.MouseHover, AddressOf q558
            fl.DropDownItems.Insert(58, opu2)
            Dim shq As New ToolStripMenuItem
            shq.Text = "Show Quick Notes Word Count"
            shq.Image = My.Resources.F9
            shq.BackColor = Color.White
            shq.ForeColor = Color.Black
            shq.Padding = New Padding(0)
            AddHandler shq.Click, AddressOf q559
            AddHandler shq.MouseHover, AddressOf q560
            ot.DropDownItems.Insert(23, shq)
            Dim cma As New ToolStripMenuItem
            cma.Text = "Copy All Bookmark Domains"
            cma.Image = My.Resources.F9
            cma.BackColor = Color.White
            cma.ForeColor = Color.Black
            cma.Padding = New Padding(0)
            AddHandler cma.Click, AddressOf q561
            AddHandler cma.MouseHover, AddressOf q562
            bo.DropDownItems.Insert(34, cma)
            Dim sph6 As New ToolStripMenuItem
            sph6.Text = "Save Page As Plain Text (Clean)"
            sph6.Image = My.Resources.F9
            sph6.BackColor = Color.White
            sph6.ForeColor = Color.Black
            sph6.Padding = New Padding(0)
            AddHandler sph6.Click, AddressOf q563
            AddHandler sph6.MouseHover, AddressOf q564
            fl.DropDownItems.Insert(59, sph6)
            Dim otn As New ToolStripMenuItem
            otn.Text = "Open Next History Item"
            otn.Image = My.Resources.F9
            otn.BackColor = Color.White
            otn.ForeColor = Color.Black
            otn.Padding = New Padding(0)
            AddHandler otn.Click, AddressOf q565
            AddHandler otn.MouseHover, AddressOf q566
            hi.DropDownItems.Insert(60, otn)
            Dim otp As New ToolStripMenuItem
            otp.Text = "Open Previous History Item"
            otp.Image = My.Resources.F9
            otp.BackColor = Color.White
            otp.ForeColor = Color.Black
            otp.Padding = New Padding(0)
            AddHandler otp.Click, AddressOf q567
            AddHandler otp.MouseHover, AddressOf q568
            hi.DropDownItems.Insert(61, otp)
            Dim cpn As New ToolStripMenuItem
            cpn.Text = "Copy Page As Plain Text Summary"
            cpn.Image = My.Resources.F9
            cpn.BackColor = Color.White
            cpn.ForeColor = Color.Black
            cpn.Padding = New Padding(0)
            AddHandler cpn.Click, AddressOf q569
            AddHandler cpn.MouseHover, AddressOf q570
            Me.e.DropDownItems.Insert(63, cpn)
            Dim rvh As New ToolStripMenuItem
            rvh.Text = "Reverse History Order"
            rvh.Image = My.Resources.F9
            rvh.BackColor = Color.White
            rvh.ForeColor = Color.Black
            rvh.Padding = New Padding(0)
            AddHandler rvh.Click, AddressOf q571
            AddHandler rvh.MouseHover, AddressOf q572
            hi.DropDownItems.Insert(62, rvh)
            Dim spu2 As New ToolStripMenuItem
            spu2.Text = "Save Page URL History To File"
            spu2.Image = My.Resources.F9
            spu2.BackColor = Color.White
            spu2.ForeColor = Color.Black
            spu2.Padding = New Padding(0)
            AddHandler spu2.Click, AddressOf q573
            AddHandler spu2.MouseHover, AddressOf q574
            fl.DropDownItems.Insert(60, spu2)
            Dim tgj As New ToolStripMenuItem
            tgj.Text = "Copy Page As JSON Summary"
            tgj.Image = My.Resources.F9
            tgj.BackColor = Color.White
            tgj.ForeColor = Color.Black
            tgj.Padding = New Padding(0)
            AddHandler tgj.Click, AddressOf q575
            AddHandler tgj.MouseHover, AddressOf q576
            Me.e.DropDownItems.Insert(64, tgj)
            Dim bho As New ToolStripMenuItem
            bho.Text = "Bookmark History Range"
            bho.Image = My.Resources.F9
            bho.BackColor = Color.White
            bho.ForeColor = Color.Black
            bho.Padding = New Padding(0)
            AddHandler bho.Click, AddressOf q577
            AddHandler bho.MouseHover, AddressOf q578
            bo.DropDownItems.Insert(35, bho)
            Dim stz As New ToolStripMenuItem
            stz.Text = "Set Zoom Level"
            stz.Image = My.Resources.F9
            stz.BackColor = Color.White
            stz.ForeColor = Color.Black
            stz.Padding = New Padding(0)
            AddHandler stz.Click, AddressOf q579
            AddHandler stz.MouseHover, AddressOf q580
            v.DropDownItems.Insert(23, stz)
            Dim cpac As New ToolStripMenuItem
            cpac.Text = "Copy Page Anchor IDs"
            cpac.Image = My.Resources.F9
            cpac.BackColor = Color.White
            cpac.ForeColor = Color.Black
            cpac.Padding = New Padding(0)
            AddHandler cpac.Click, AddressOf q581
            AddHandler cpac.MouseHover, AddressOf q582
            Me.e.DropDownItems.Insert(65, cpac)
            Dim qns As New ToolStripMenuItem
            qns.Text = "Search Within Quick Notes"
            qns.Image = My.Resources.F9
            qns.BackColor = Color.White
            qns.ForeColor = Color.Black
            qns.Padding = New Padding(0)
            AddHandler qns.Click, AddressOf q583
            AddHandler qns.MouseHover, AddressOf q584
            ot.DropDownItems.Insert(24, qns)
            Dim cpmt As New ToolStripMenuItem
            cpmt.Text = "Copy Page Meta Tags"
            cpmt.Image = My.Resources.F9
            cpmt.BackColor = Color.White
            cpmt.ForeColor = Color.Black
            cpmt.Padding = New Padding(0)
            AddHandler cpmt.Click, AddressOf q585
            AddHandler cpmt.MouseHover, AddressOf q586
            Me.e.DropDownItems.Insert(66, cpmt)
            Dim ohu As New ToolStripMenuItem
            ohu.Text = "Open Homepage In Default Browser"
            ohu.Image = My.Resources.F9
            ohu.BackColor = Color.White
            ohu.ForeColor = Color.Black
            ohu.Padding = New Padding(0)
            AddHandler ohu.Click, AddressOf q587
            AddHandler ohu.MouseHover, AddressOf q588
            hi.DropDownItems.Insert(63, ohu)
            Dim scp As New ToolStripMenuItem
            scp.Text = "Save Tab Session As JSON"
            scp.Image = My.Resources.F9
            scp.BackColor = Color.White
            scp.ForeColor = Color.Black
            scp.Padding = New Padding(0)
            AddHandler scp.Click, AddressOf q589
            AddHandler scp.MouseHover, AddressOf q590
            fl.DropDownItems.Insert(61, scp)
            Dim lts As New ToolStripMenuItem
            lts.Text = "Load Tab Session From JSON"
            lts.Image = My.Resources.F9
            lts.BackColor = Color.White
            lts.ForeColor = Color.Black
            lts.Padding = New Padding(0)
            AddHandler lts.Click, AddressOf q591
            AddHandler lts.MouseHover, AddressOf q592
            fl.DropDownItems.Insert(62, lts)
            Dim cph3 As New ToolStripMenuItem
            cph3.Text = "Copy Page Highlight Text"
            cph3.Image = My.Resources.F9
            cph3.BackColor = Color.White
            cph3.ForeColor = Color.Black
            cph3.Padding = New Padding(0)
            AddHandler cph3.Click, AddressOf q593
            AddHandler cph3.MouseHover, AddressOf q594
            Me.e.DropDownItems.Insert(67, cph3)
            Dim obn2 As New ToolStripMenuItem
            obn2.Text = "Open All Bookmarks With Keyword"
            obn2.Image = My.Resources.F9
            obn2.BackColor = Color.White
            obn2.ForeColor = Color.Black
            obn2.Padding = New Padding(0)
            AddHandler obn2.Click, AddressOf q595
            AddHandler obn2.MouseHover, AddressOf q596
            bo.DropDownItems.Insert(36, obn2)
            Dim spf2 As New ToolStripMenuItem
            spf2.Text = "Show Page Frame Count"
            spf2.Image = My.Resources.F9
            spf2.BackColor = Color.White
            spf2.ForeColor = Color.Black
            spf2.Padding = New Padding(0)
            AddHandler spf2.Click, AddressOf q597
            AddHandler spf2.MouseHover, AddressOf q598
            he.DropDownItems.Insert(18, spf2)
            Dim cqs As New ToolStripMenuItem
            cqs.Text = "Copy Quick Notes Last Line"
            cqs.Image = My.Resources.F9
            cqs.BackColor = Color.White
            cqs.ForeColor = Color.Black
            cqs.Padding = New Padding(0)
            AddHandler cqs.Click, AddressOf q599
            AddHandler cqs.MouseHover, AddressOf q600
            ot.DropDownItems.Insert(25, cqs)
            Dim tpn As New ToolStripMenuItem
            tpn.Text = "Toggle Status Bar"
            tpn.Image = My.Resources.F9
            tpn.BackColor = Color.White
            tpn.ForeColor = Color.Black
            tpn.Padding = New Padding(0)
            AddHandler tpn.Click, AddressOf q601
            AddHandler tpn.MouseHover, AddressOf q602
            v.DropDownItems.Insert(24, tpn)
            Dim opr As New ToolStripMenuItem
            opr.Text = "Open Page In Google Cache"
            opr.Image = My.Resources.F9
            opr.BackColor = Color.White
            opr.ForeColor = Color.Black
            opr.Padding = New Padding(0)
            AddHandler opr.Click, AddressOf q603
            AddHandler opr.MouseHover, AddressOf q604
            hi.DropDownItems.Insert(64, opr)
            Dim cbm2 As New ToolStripMenuItem
            cbm2.Text = "Copy Bookmarks As CSV"
            cbm2.Image = My.Resources.F9
            cbm2.BackColor = Color.White
            cbm2.ForeColor = Color.Black
            cbm2.Padding = New Padding(0)
            AddHandler cbm2.Click, AddressOf q605
            AddHandler cbm2.MouseHover, AddressOf q606
            bo.DropDownItems.Insert(37, cbm2)
            Dim sth2 As New ToolStripMenuItem
            sth2.Text = "Show Tab Loading Order"
            sth2.Image = My.Resources.F9
            sth2.BackColor = Color.White
            sth2.ForeColor = Color.Black
            sth2.Padding = New Padding(0)
            AddHandler sth2.Click, AddressOf q607
            AddHandler sth2.MouseHover, AddressOf q608
            he.DropDownItems.Insert(19, sth2)
            Dim osp As New ToolStripMenuItem
            osp.Text = "Open Page Source In Default Editor"
            osp.Image = My.Resources.F9
            osp.BackColor = Color.White
            osp.ForeColor = Color.Black
            osp.Padding = New Padding(0)
            AddHandler osp.Click, AddressOf q609
            AddHandler osp.MouseHover, AddressOf q610
            fl.DropDownItems.Insert(63, osp)
            Dim cpl3 As New ToolStripMenuItem
            cpl3.Text = "Copy Page Link At Position"
            cpl3.Image = My.Resources.F9
            cpl3.BackColor = Color.White
            cpl3.ForeColor = Color.Black
            cpl3.Padding = New Padding(0)
            AddHandler cpl3.Click, AddressOf q611
            AddHandler cpl3.MouseHover, AddressOf q612
            Me.e.DropDownItems.Insert(68, cpl3)
            Dim qnp As New ToolStripMenuItem
            qnp.Text = "Prepend Text To Quick Notes"
            qnp.Image = My.Resources.F9
            qnp.BackColor = Color.White
            qnp.ForeColor = Color.Black
            qnp.Padding = New Padding(0)
            AddHandler qnp.Click, AddressOf q613
            AddHandler qnp.MouseHover, AddressOf q614
            ot.DropDownItems.Insert(26, qnp)
            Dim spv As New ToolStripMenuItem
            spv.Text = "Save Page As PDF Via Print"
            spv.Image = My.Resources.F9
            spv.BackColor = Color.White
            spv.ForeColor = Color.Black
            spv.Padding = New Padding(0)
            AddHandler spv.Click, AddressOf q615
            AddHandler spv.MouseHover, AddressOf q616
            fl.DropDownItems.Insert(64, spv)
            Dim cbu As New ToolStripMenuItem
            cbu.Text = "Copy Base URL"
            cbu.Image = My.Resources.F9
            cbu.BackColor = Color.White
            cbu.ForeColor = Color.Black
            cbu.Padding = New Padding(0)
            AddHandler cbu.Click, AddressOf q617
            AddHandler cbu.MouseHover, AddressOf q618
            Me.e.DropDownItems.Insert(69, cbu)
            Dim oqr As New ToolStripMenuItem
            oqr.Text = "Open QR Code Scanner Page"
            oqr.Image = My.Resources.F9
            oqr.BackColor = Color.White
            oqr.ForeColor = Color.Black
            oqr.Padding = New Padding(0)
            AddHandler oqr.Click, AddressOf q619
            AddHandler oqr.MouseHover, AddressOf q620
            fl.DropDownItems.Insert(65, oqr)
            Dim tbs2 As New ToolStripMenuItem
            tbs2.Text = "Copy Tab URLs Matching Keyword"
            tbs2.Image = My.Resources.F9
            tbs2.BackColor = Color.White
            tbs2.ForeColor = Color.Black
            tbs2.Padding = New Padding(0)
            AddHandler tbs2.Click, AddressOf q621
            AddHandler tbs2.MouseHover, AddressOf q622
            Me.e.DropDownItems.Insert(70, tbs2)
            Dim sbd2 As New ToolStripMenuItem
            sbd2.Text = "Save Bookmarks As JSON"
            sbd2.Image = My.Resources.F9
            sbd2.BackColor = Color.White
            sbd2.ForeColor = Color.Black
            sbd2.Padding = New Padding(0)
            AddHandler sbd2.Click, AddressOf q623
            AddHandler sbd2.MouseHover, AddressOf q624
            bo.DropDownItems.Insert(38, sbd2)
            Dim lbj As New ToolStripMenuItem
            lbj.Text = "Load Bookmarks From JSON"
            lbj.Image = My.Resources.F9
            lbj.BackColor = Color.White
            lbj.ForeColor = Color.Black
            lbj.Padding = New Padding(0)
            AddHandler lbj.Click, AddressOf q625
            AddHandler lbj.MouseHover, AddressOf q626
            bo.DropDownItems.Insert(39, lbj)
            Dim cpr2 As New ToolStripMenuItem
            cpr2.Text = "Copy Page Response Headers"
            cpr2.Image = My.Resources.F9
            cpr2.BackColor = Color.White
            cpr2.ForeColor = Color.Black
            cpr2.Padding = New Padding(0)
            AddHandler cpr2.Click, AddressOf q627
            AddHandler cpr2.MouseHover, AddressOf q628
            he.DropDownItems.Insert(20, cpr2)
            Dim ott2 As New ToolStripMenuItem
            ott2.Text = "Open All Tabs From History"
            ott2.Image = My.Resources.F9
            ott2.BackColor = Color.White
            ott2.ForeColor = Color.Black
            ott2.Padding = New Padding(0)
            AddHandler ott2.Click, AddressOf q629
            AddHandler ott2.MouseHover, AddressOf q630
            hi.DropDownItems.Insert(65, ott2)
            Dim spn As New ToolStripMenuItem
            spn.Text = "Speak Page Text Aloud"
            spn.Image = My.Resources.F9
            spn.BackColor = Color.White
            spn.ForeColor = Color.Black
            spn.Padding = New Padding(0)
            AddHandler spn.Click, AddressOf q631
            AddHandler spn.MouseHover, AddressOf q632
            v.DropDownItems.Insert(25, spn)
            Dim qne As New ToolStripMenuItem
            qne.Text = "Export Quick Notes As CSV"
            qne.Image = My.Resources.F9
            qne.BackColor = Color.White
            qne.ForeColor = Color.Black
            qne.Padding = New Padding(0)
            AddHandler qne.Click, AddressOf q633
            AddHandler qne.MouseHover, AddressOf q634
            ot.DropDownItems.Insert(27, qne)
            Dim cph4 As New ToolStripMenuItem
            cph4.Text = "Copy Page Paragraph Count"
            cph4.Image = My.Resources.F9
            cph4.BackColor = Color.White
            cph4.ForeColor = Color.Black
            cph4.Padding = New Padding(0)
            AddHandler cph4.Click, AddressOf q635
            AddHandler cph4.MouseHover, AddressOf q636
            Me.e.DropDownItems.Insert(71, cph4)
            Dim tbf As New ToolStripMenuItem
            tbf.Text = "Close All Tabs Except First"
            tbf.Image = My.Resources.F9
            tbf.BackColor = Color.White
            tbf.ForeColor = Color.Black
            tbf.Padding = New Padding(0)
            AddHandler tbf.Click, AddressOf q637
            AddHandler tbf.MouseHover, AddressOf q638
            fl.DropDownItems.Insert(66, tbf)
            Dim opu3 As New ToolStripMenuItem
            opu3.Text = "Open URL From Quick Notes"
            opu3.Image = My.Resources.F9
            opu3.BackColor = Color.White
            opu3.ForeColor = Color.Black
            opu3.Padding = New Padding(0)
            AddHandler opu3.Click, AddressOf q639
            AddHandler opu3.MouseHover, AddressOf q640
            ot.DropDownItems.Insert(28, opu3)
            Dim cts2 As New ToolStripMenuItem
            cts2.Text = "Copy Tab URLs As JSON Array"
            cts2.Image = My.Resources.F9
            cts2.BackColor = Color.White
            cts2.ForeColor = Color.Black
            cts2.Padding = New Padding(0)
            AddHandler cts2.Click, AddressOf q641
            AddHandler cts2.MouseHover, AddressOf q642
            Me.e.DropDownItems.Insert(72, cts2)
            Dim rbn As New ToolStripMenuItem
            rbn.Text = "Reload Tab By Number"
            rbn.Image = My.Resources.F9
            rbn.BackColor = Color.White
            rbn.ForeColor = Color.Black
            rbn.Padding = New Padding(0)
            AddHandler rbn.Click, AddressOf q643
            AddHandler rbn.MouseHover, AddressOf q644
            hi.DropDownItems.Insert(66, rbn)
            Dim cpt2 As New ToolStripMenuItem
            cpt2.Text = "Copy Page Title And Domain"
            cpt2.Image = My.Resources.F9
            cpt2.BackColor = Color.White
            cpt2.ForeColor = Color.Black
            cpt2.Padding = New Padding(0)
            AddHandler cpt2.Click, AddressOf q645
            AddHandler cpt2.MouseHover, AddressOf q646
            Me.e.DropDownItems.Insert(73, cpt2)
            Dim oln2 As New ToolStripMenuItem
            oln2.Text = "Open All Links On Page In Tabs"
            oln2.Image = My.Resources.F9
            oln2.BackColor = Color.White
            oln2.ForeColor = Color.Black
            oln2.Padding = New Padding(0)
            AddHandler oln2.Click, AddressOf q647
            AddHandler oln2.MouseHover, AddressOf q648
            hi.DropDownItems.Insert(67, oln2)
            Dim spt2 As New ToolStripMenuItem
            spt2.Text = "Save Quick Notes As Markdown"
            spt2.Image = My.Resources.F9
            spt2.BackColor = Color.White
            spt2.ForeColor = Color.Black
            spt2.Padding = New Padding(0)
            AddHandler spt2.Click, AddressOf q649
            AddHandler spt2.MouseHover, AddressOf q650
            ot.DropDownItems.Insert(29, spt2)
            Dim cbk2 As New ToolStripMenuItem
            cbk2.Text = "Close Tabs Below Current"
            cbk2.Image = My.Resources.F9
            cbk2.BackColor = Color.White
            cbk2.ForeColor = Color.Black
            cbk2.Padding = New Padding(0)
            AddHandler cbk2.Click, AddressOf q651
            AddHandler cbk2.MouseHover, AddressOf q652
            fl.DropDownItems.Insert(67, cbk2)
            Dim opr2 As New ToolStripMenuItem
            opr2.Text = "Open Random History Item"
            opr2.Image = My.Resources.F9
            opr2.BackColor = Color.White
            opr2.ForeColor = Color.Black
            opr2.Padding = New Padding(0)
            AddHandler opr2.Click, AddressOf q653
            AddHandler opr2.MouseHover, AddressOf q654
            hi.DropDownItems.Insert(68, opr2)
            Dim cpb As New ToolStripMenuItem
            cpb.Text = "Copy Page Bold Text"
            cpb.Image = My.Resources.F9
            cpb.BackColor = Color.White
            cpb.ForeColor = Color.Black
            cpb.Padding = New Padding(0)
            AddHandler cpb.Click, AddressOf q655
            AddHandler cpb.MouseHover, AddressOf q656
            Me.e.DropDownItems.Insert(74, cpb)
            Dim shb As New ToolStripMenuItem
            shb.Text = "Show Bookmark At Position"
            shb.Image = My.Resources.F9
            shb.BackColor = Color.White
            shb.ForeColor = Color.Black
            shb.Padding = New Padding(0)
            AddHandler shb.Click, AddressOf q657
            AddHandler shb.MouseHover, AddressOf q658
            bo.DropDownItems.Insert(40, shb)
            Dim tgl As New ToolStripMenuItem
            tgl.Text = "Toggle Tab Strip Visibility"
            tgl.Image = My.Resources.F9
            tgl.BackColor = Color.White
            tgl.ForeColor = Color.Black
            tgl.Padding = New Padding(0)
            AddHandler tgl.Click, AddressOf q659
            AddHandler tgl.MouseHover, AddressOf q660
            v.DropDownItems.Insert(26, tgl)
            Dim shl2 As New ToolStripMenuItem
            shl2.Text = "Save History As JSON"
            shl2.Image = My.Resources.F9
            shl2.BackColor = Color.White
            shl2.ForeColor = Color.Black
            shl2.Padding = New Padding(0)
            AddHandler shl2.Click, AddressOf q661
            AddHandler shl2.MouseHover, AddressOf q662
            fl.DropDownItems.Insert(68, shl2)
            Dim lhj As New ToolStripMenuItem
            lhj.Text = "Load History From JSON"
            lhj.Image = My.Resources.F9
            lhj.BackColor = Color.White
            lhj.ForeColor = Color.Black
            lhj.Padding = New Padding(0)
            AddHandler lhj.Click, AddressOf q663
            AddHandler lhj.MouseHover, AddressOf q664
            fl.DropDownItems.Insert(69, lhj)
            Dim ctu2 As New ToolStripMenuItem
            ctu2.Text = "Copy Current Tab URL Encoded"
            ctu2.Image = My.Resources.F9
            ctu2.BackColor = Color.White
            ctu2.ForeColor = Color.Black
            ctu2.Padding = New Padding(0)
            AddHandler ctu2.Click, AddressOf q665
            AddHandler ctu2.MouseHover, AddressOf q666
            Me.e.DropDownItems.Insert(75, ctu2)
            Dim obn3 As New ToolStripMenuItem
            obn3.Text = "Open Next Bookmark"
            obn3.Image = My.Resources.F9
            obn3.BackColor = Color.White
            obn3.ForeColor = Color.Black
            obn3.Padding = New Padding(0)
            AddHandler obn3.Click, AddressOf q667
            AddHandler obn3.MouseHover, AddressOf q668
            bo.DropDownItems.Insert(41, obn3)
            Dim spw2 As New ToolStripMenuItem
            spw2.Text = "Show Page External Link Count"
            spw2.Image = My.Resources.F9
            spw2.BackColor = Color.White
            spw2.ForeColor = Color.Black
            spw2.Padding = New Padding(0)
            AddHandler spw2.Click, AddressOf q669
            AddHandler spw2.MouseHover, AddressOf q670
            he.DropDownItems.Insert(21, spw2)
            Dim qna As New ToolStripMenuItem
            qna.Text = "Append Page Info To Quick Notes"
            qna.Image = My.Resources.F9
            qna.BackColor = Color.White
            qna.ForeColor = Color.Black
            qna.Padding = New Padding(0)
            AddHandler qna.Click, AddressOf q671
            AddHandler qna.MouseHover, AddressOf q672
            ot.DropDownItems.Insert(30, qna)
            Dim sth3 As New ToolStripMenuItem
            sth3.Text = "Stop Speaking"
            sth3.Image = My.Resources.F9
            sth3.BackColor = Color.White
            sth3.ForeColor = Color.Black
            sth3.Padding = New Padding(0)
            AddHandler sth3.Click, AddressOf q673
            AddHandler sth3.MouseHover, AddressOf q674
            v.DropDownItems.Insert(27, sth3)
            Dim cpi3 As New ToolStripMenuItem
            cpi3.Text = "Copy Page Internal Links Only"
            cpi3.Image = My.Resources.F9
            cpi3.BackColor = Color.White
            cpi3.ForeColor = Color.Black
            cpi3.Padding = New Padding(0)
            AddHandler cpi3.Click, AddressOf q675
            AddHandler cpi3.MouseHover, AddressOf q676
            Me.e.DropDownItems.Insert(76, cpi3)
            Dim cpe As New ToolStripMenuItem
            cpe.Text = "Copy Page External Links Only"
            cpe.Image = My.Resources.F9
            cpe.BackColor = Color.White
            cpe.ForeColor = Color.Black
            cpe.Padding = New Padding(0)
            AddHandler cpe.Click, AddressOf q677
            AddHandler cpe.MouseHover, AddressOf q678
            Me.e.DropDownItems.Insert(77, cpe)
            Dim snt3 As New ToolStripMenuItem
            snt3.Text = "Save Notes With Page Context"
            snt3.Image = My.Resources.F9
            snt3.BackColor = Color.White
            snt3.ForeColor = Color.Black
            snt3.Padding = New Padding(0)
            AddHandler snt3.Click, AddressOf q679
            AddHandler snt3.MouseHover, AddressOf q680
            ot.DropDownItems.Insert(31, snt3)
            Dim opb As New ToolStripMenuItem
            opb.Text = "Open Previous Bookmark"
            opb.Image = My.Resources.F9
            opb.BackColor = Color.White
            opb.ForeColor = Color.Black
            opb.Padding = New Padding(0)
            AddHandler opb.Click, AddressOf q681
            AddHandler opb.MouseHover, AddressOf q682
            bo.DropDownItems.Insert(42, opb)
            Dim ctu3 As New ToolStripMenuItem
            ctu3.Text = "Copy Tab Count As Text"
            ctu3.Image = My.Resources.F9
            ctu3.BackColor = Color.White
            ctu3.ForeColor = Color.Black
            ctu3.Padding = New Padding(0)
            AddHandler ctu3.Click, AddressOf q683
            AddHandler ctu3.MouseHover, AddressOf q684
            he.DropDownItems.Insert(22, ctu3)
            Dim osp2 As New ToolStripMenuItem
            osp2.Text = "Open Search Engine Settings"
            osp2.Image = My.Resources.F9
            osp2.BackColor = Color.White
            osp2.ForeColor = Color.Black
            osp2.Padding = New Padding(0)
            AddHandler osp2.Click, AddressOf q685
            AddHandler osp2.MouseHover, AddressOf q686
            ot.DropDownItems.Insert(32, osp2)
            Dim sph7 As New ToolStripMenuItem
            sph7.Text = "Save Page As XML Snapshot"
            sph7.Image = My.Resources.F9
            sph7.BackColor = Color.White
            sph7.ForeColor = Color.Black
            sph7.Padding = New Padding(0)
            AddHandler sph7.Click, AddressOf q687
            AddHandler sph7.MouseHover, AddressOf q688
            fl.DropDownItems.Insert(70, sph7)
            Dim tbn2 As New ToolStripMenuItem
            tbn2.Text = "Go To Tab To The Right"
            tbn2.Image = My.Resources.F9
            tbn2.BackColor = Color.White
            tbn2.ForeColor = Color.Black
            tbn2.Padding = New Padding(0)
            AddHandler tbn2.Click, AddressOf q689
            AddHandler tbn2.MouseHover, AddressOf q690
            hi.DropDownItems.Insert(69, tbn2)
            Dim tbp As New ToolStripMenuItem
            tbp.Text = "Go To Tab To The Left"
            tbp.Image = My.Resources.F9
            tbp.BackColor = Color.White
            tbp.ForeColor = Color.Black
            tbp.Padding = New Padding(0)
            AddHandler tbp.Click, AddressOf q691
            AddHandler tbp.MouseHover, AddressOf q692
            hi.DropDownItems.Insert(70, tbp)
            Dim cpn2 As New ToolStripMenuItem
            cpn2.Text = "Copy Page Navigation Timing"
            cpn2.Image = My.Resources.F9
            cpn2.BackColor = Color.White
            cpn2.ForeColor = Color.Black
            cpn2.Padding = New Padding(0)
            AddHandler cpn2.Click, AddressOf q693
            AddHandler cpn2.MouseHover, AddressOf q694
            he.DropDownItems.Insert(23, cpn2)
            Dim sbm2 As New ToolStripMenuItem
            sbm2.Text = "Shuffle Bookmark Order"
            sbm2.Image = My.Resources.F9
            sbm2.BackColor = Color.White
            sbm2.ForeColor = Color.Black
            sbm2.Padding = New Padding(0)
            AddHandler sbm2.Click, AddressOf q695
            AddHandler sbm2.MouseHover, AddressOf q696
            bo.DropDownItems.Insert(43, sbm2)
            Dim cph5 As New ToolStripMenuItem
            cph5.Text = "Copy Page As Tab Separated Data"
            cph5.Image = My.Resources.F9
            cph5.BackColor = Color.White
            cph5.ForeColor = Color.Black
            cph5.Padding = New Padding(0)
            AddHandler cph5.Click, AddressOf q697
            AddHandler cph5.MouseHover, AddressOf q698
            Me.e.DropDownItems.Insert(78, cph5)
            Dim spw3 As New ToolStripMenuItem
            spw3.Text = "Save Page Images List To File"
            spw3.Image = My.Resources.F9
            spw3.BackColor = Color.White
            spw3.ForeColor = Color.Black
            spw3.Padding = New Padding(0)
            AddHandler spw3.Click, AddressOf q699
            AddHandler spw3.MouseHover, AddressOf q700
            fl.DropDownItems.Insert(71, spw3)
            Dim oht2 As New ToolStripMenuItem
            oht2.Text = "Open Bookmark Folder In Tabs"
            oht2.Image = My.Resources.F9
            oht2.BackColor = Color.White
            oht2.ForeColor = Color.Black
            oht2.Padding = New Padding(0)
            AddHandler oht2.Click, AddressOf q701
            AddHandler oht2.MouseHover, AddressOf q702
            bo.DropDownItems.Insert(44, oht2)
            Dim cpt3 As New ToolStripMenuItem
            cpt3.Text = "Copy Page Text Upper Case"
            cpt3.Image = My.Resources.F9
            cpt3.BackColor = Color.White
            cpt3.ForeColor = Color.Black
            cpt3.Padding = New Padding(0)
            AddHandler cpt3.Click, AddressOf q703
            AddHandler cpt3.MouseHover, AddressOf q704
            Me.e.DropDownItems.Insert(79, cpt3)
            Dim cptl As New ToolStripMenuItem
            cptl.Text = "Copy Page Text Lower Case"
            cptl.Image = My.Resources.F9
            cptl.BackColor = Color.White
            cptl.ForeColor = Color.Black
            cptl.Padding = New Padding(0)
            AddHandler cptl.Click, AddressOf q705
            AddHandler cptl.MouseHover, AddressOf q706
            Me.e.DropDownItems.Insert(80, cptl)
            Dim sph8 As New ToolStripMenuItem
            sph8.Text = "Save Page As Numbered HTML List"
            sph8.Image = My.Resources.F9
            sph8.BackColor = Color.White
            sph8.ForeColor = Color.Black
            sph8.Padding = New Padding(0)
            AddHandler sph8.Click, AddressOf q707
            AddHandler sph8.MouseHover, AddressOf q708
            fl.DropDownItems.Insert(72, sph8)
            Dim opd As New ToolStripMenuItem
            opd.Text = "Open Page On DuckDuckGo"
            opd.Image = My.Resources.F9
            opd.BackColor = Color.White
            opd.ForeColor = Color.Black
            opd.Padding = New Padding(0)
            AddHandler opd.Click, AddressOf q709
            AddHandler opd.MouseHover, AddressOf q710
            hi.DropDownItems.Insert(71, opd)
            Dim cpf2 As New ToolStripMenuItem
            cpf2.Text = "Copy Page First Heading"
            cpf2.Image = My.Resources.F9
            cpf2.BackColor = Color.White
            cpf2.ForeColor = Color.Black
            cpf2.Padding = New Padding(0)
            AddHandler cpf2.Click, AddressOf q711
            AddHandler cpf2.MouseHover, AddressOf q712
            Me.e.DropDownItems.Insert(81, cpf2)
            Dim stq As New ToolStripMenuItem
            stq.Text = "Save All Tab URLs With Titles"
            stq.Image = My.Resources.F9
            stq.BackColor = Color.White
            stq.ForeColor = Color.Black
            stq.Padding = New Padding(0)
            AddHandler stq.Click, AddressOf q713
            AddHandler stq.MouseHover, AddressOf q714
            fl.DropDownItems.Insert(73, stq)
            Dim qnd As New ToolStripMenuItem
            qnd.Text = "Delete Quick Notes Line By Number"
            qnd.Image = My.Resources.F9
            qnd.BackColor = Color.White
            qnd.ForeColor = Color.Black
            qnd.Padding = New Padding(0)
            AddHandler qnd.Click, AddressOf q715
            AddHandler qnd.MouseHover, AddressOf q716
            ot.DropDownItems.Insert(33, qnd)
            Dim opu4 As New ToolStripMenuItem
            opu4.Text = "Open Page URL In New Private Window"
            opu4.Image = My.Resources.F9
            opu4.BackColor = Color.White
            opu4.ForeColor = Color.Black
            opu4.Padding = New Padding(0)
            AddHandler opu4.Click, AddressOf q717
            AddHandler opu4.MouseHover, AddressOf q718
            fl.DropDownItems.Insert(74, opu4)
            Dim cpj2 As New ToolStripMenuItem
            cpj2.Text = "Copy Bookmarks As Markdown List"
            cpj2.Image = My.Resources.F9
            cpj2.BackColor = Color.White
            cpj2.ForeColor = Color.Black
            cpj2.Padding = New Padding(0)
            AddHandler cpj2.Click, AddressOf q719
            AddHandler cpj2.MouseHover, AddressOf q720
            bo.DropDownItems.Insert(45, cpj2)
            Dim shm2 As New ToolStripMenuItem
            shm2.Text = "Show Page Memory Usage"
            shm2.Image = My.Resources.F9
            shm2.BackColor = Color.White
            shm2.ForeColor = Color.Black
            shm2.Padding = New Padding(0)
            AddHandler shm2.Click, AddressOf q721
            AddHandler shm2.MouseHover, AddressOf q722
            he.DropDownItems.Insert(24, shm2)
            Dim opr3 As New ToolStripMenuItem
            opr3.Text = "Open Page On Bing"
            opr3.Image = My.Resources.F9
            opr3.BackColor = Color.White
            opr3.ForeColor = Color.Black
            opr3.Padding = New Padding(0)
            AddHandler opr3.Click, AddressOf q723
            AddHandler opr3.MouseHover, AddressOf q724
            hi.DropDownItems.Insert(72, opr3)
            Dim qnm As New ToolStripMenuItem
            qnm.Text = "Merge Clipboard Into Quick Notes"
            qnm.Image = My.Resources.F9
            qnm.BackColor = Color.White
            qnm.ForeColor = Color.Black
            qnm.Padding = New Padding(0)
            AddHandler qnm.Click, AddressOf q725
            AddHandler qnm.MouseHover, AddressOf q726
            ot.DropDownItems.Insert(34, qnm)
            Dim cts3 As New ToolStripMenuItem
            cts3.Text = "Copy Tab At Position Info"
            cts3.Image = My.Resources.F9
            cts3.BackColor = Color.White
            cts3.ForeColor = Color.Black
            cts3.Padding = New Padding(0)
            AddHandler cts3.Click, AddressOf q727
            AddHandler cts3.MouseHover, AddressOf q728
            Me.e.DropDownItems.Insert(82, cts3)
            Dim opp As New ToolStripMenuItem
            opp.Text = "Open Page On Google Maps"
            opp.Image = My.Resources.F9
            opp.BackColor = Color.White
            opp.ForeColor = Color.Black
            opp.Padding = New Padding(0)
            AddHandler opp.Click, AddressOf q729
            AddHandler opp.MouseHover, AddressOf q730
            hi.DropDownItems.Insert(73, opp)
            Dim sbs2 As New ToolStripMenuItem
            sbs2.Text = "Save Bookmark Range To File"
            sbs2.Image = My.Resources.F9
            sbs2.BackColor = Color.White
            sbs2.ForeColor = Color.Black
            sbs2.Padding = New Padding(0)
            AddHandler sbs2.Click, AddressOf q731
            AddHandler sbs2.MouseHover, AddressOf q732
            bo.DropDownItems.Insert(46, sbs2)
            Dim cph6 As New ToolStripMenuItem
            cph6.Text = "Copy Page Link And Image Count"
            cph6.Image = My.Resources.F9
            cph6.BackColor = Color.White
            cph6.ForeColor = Color.Black
            cph6.Padding = New Padding(0)
            AddHandler cph6.Click, AddressOf q733
            AddHandler cph6.MouseHover, AddressOf q734
            he.DropDownItems.Insert(25, cph6)
            Dim cpc7 As New ToolStripMenuItem
            cpc7.Text = "Copy Page Citation"
            cpc7.Image = My.Resources.F9
            cpc7.BackColor = Color.White
            cpc7.ForeColor = Color.Black
            cpc7.Padding = New Padding(0)
            AddHandler cpc7.Click, AddressOf q735
            AddHandler cpc7.MouseHover, AddressOf q736
            Me.e.DropDownItems.Insert(83, cpc7)
            Dim pag As New ToolStripMenuItem
            pag.Text = "Paste And Go"
            pag.Image = My.Resources.F9
            pag.BackColor = Color.White
            pag.ForeColor = Color.Black
            pag.Padding = New Padding(0)
            AddHandler pag.Click, AddressOf q737
            AddHandler pag.MouseHover, AddressOf q738
            Me.e.DropDownItems.Insert(88, pag)
            Dim cbd As New ToolStripMenuItem
            cbd.Text = "Copy Browser Diagnostics"
            cbd.Image = My.Resources.F9
            cbd.BackColor = Color.White
            cbd.ForeColor = Color.Black
            cbd.Padding = New Padding(0)
            AddHandler cbd.Click, AddressOf q739
            AddHandler cbd.MouseHover, AddressOf q740
            he.DropDownItems.Insert(18, cbd)
            Dim sns As New ToolStripMenuItem
            sns.Text = "Save Named Session"
            sns.Image = My.Resources.F9
            sns.BackColor = Color.White
            sns.ForeColor = Color.Black
            sns.Padding = New Padding(0)
            AddHandler sns.Click, AddressOf q741
            AddHandler sns.MouseHover, AddressOf q742
            fl.DropDownItems.Add(sns)
            Dim ons As New ToolStripMenuItem
            ons.Text = "Open Named Session"
            ons.Image = My.Resources.F9
            ons.BackColor = Color.White
            ons.ForeColor = Color.Black
            ons.Padding = New Padding(0)
            AddHandler ons.Click, AddressOf q743
            AddHandler ons.MouseHover, AddressOf q744
            fl.DropDownItems.Add(ons)
            Dim rvd As New ToolStripMenuItem
            rvd.Text = "Recently Visited Domains"
            rvd.Image = My.Resources.F9
            rvd.BackColor = Color.White
            rvd.ForeColor = Color.Black
            rvd.Padding = New Padding(0)
            AddHandler rvd.DropDownOpening, AddressOf q745
            hi.DropDownItems.Add(rvd)
            Dim rlw As New ToolStripMenuItem
            rlw.Text = "Reopen Last Closed Window"
            rlw.Image = My.Resources.F9
            rlw.BackColor = Color.White
            rlw.ForeColor = Color.Black
            rlw.Padding = New Padding(0)
            AddHandler rlw.Click, AddressOf q748
            AddHandler rlw.MouseHover, AddressOf q749
            fl.DropDownItems.Add(rlw)
            Dim dsd As New ToolStripMenuItem
            dsd.Text = "Duplicate Tabs From Same Domain"
            dsd.Image = My.Resources.F9
            dsd.BackColor = Color.White
            dsd.ForeColor = Color.Black
            dsd.Padding = New Padding(0)
            AddHandler dsd.Click, AddressOf q750
            AddHandler dsd.MouseHover, AddressOf q751
            fl.DropDownItems.Add(dsd)
            Dim smb As New ToolStripMenuItem
            smb.Text = "Stop Media Or Busy Page"
            smb.Image = My.Resources.F9
            smb.BackColor = Color.White
            smb.ForeColor = Color.Black
            smb.Padding = New Padding(0)
            AddHandler smb.Click, AddressOf q752
            AddHandler smb.MouseHover, AddressOf q753
            v.DropDownItems.Add(smb)
            Dim bmf As New ToolStripMenuItem
            bmf.Text = "Bookmark Current Page In Folder"
            bmf.Image = My.Resources.F9
            bmf.BackColor = Color.White
            bmf.ForeColor = Color.Black
            bmf.Padding = New Padding(0)
            AddHandler bmf.Click, AddressOf q754
            AddHandler bmf.MouseHover, AddressOf q755
            bo.DropDownItems.Add(bmf)
            Dim ebd As New ToolStripMenuItem
            ebd.Text = "Export All Browser Data"
            ebd.Image = My.Resources.F9
            ebd.BackColor = Color.White
            ebd.ForeColor = Color.Black
            ebd.Padding = New Padding(0)
            AddHandler ebd.Click, AddressOf q756
            AddHandler ebd.MouseHover, AddressOf q757
            fl.DropDownItems.Add(ebd)
            Dim ibd As New ToolStripMenuItem
            ibd.Text = "Import All Browser Data"
            ibd.Image = My.Resources.F9
            ibd.BackColor = Color.White
            ibd.ForeColor = Color.Black
            ibd.Padding = New Padding(0)
            AddHandler ibd.Click, AddressOf q758
            AddHandler ibd.MouseHover, AddressOf q759
            fl.DropDownItems.Add(ibd)
            Dim rtv As New ToolStripMenuItem
            rtv.Text = "Reader Text View"
            rtv.Image = My.Resources.F9
            rtv.BackColor = Color.White
            rtv.ForeColor = Color.Black
            rtv.Padding = New Padding(0)
            AddHandler rtv.Click, AddressOf q760
            AddHandler rtv.MouseHover, AddressOf q761
            v.DropDownItems.Add(rtv)
            Dim spb7 As New ToolStripMenuItem
            spb7.Text = "Save Page Bundle"
            spb7.Image = My.Resources.F9
            spb7.BackColor = Color.White
            spb7.ForeColor = Color.Black
            spb7.Padding = New Padding(0)
            AddHandler spb7.Click, AddressOf q762
            AddHandler spb7.MouseHover, AddressOf q763
            fl.DropDownItems.Add(spb7)
            Dim pgl As New ToolStripMenuItem
            pgl.Text = "Save Page Images As HTML Gallery"
            pgl.Image = My.Resources.F9
            pgl.BackColor = Color.White
            pgl.ForeColor = Color.Black
            pgl.Padding = New Padding(0)
            AddHandler pgl.Click, AddressOf q764
            AddHandler pgl.MouseHover, AddressOf q765
            fl.DropDownItems.Add(pgl)
            Dim pss As New ToolStripMenuItem
            pss.Text = "Private Mode On Startup"
            pss.Image = My.Resources.F9
            pss.BackColor = Color.White
            pss.ForeColor = Color.Black
            pss.Padding = New Padding(0)
            AddHandler pss.Click, AddressOf q766
            AddHandler pss.MouseHover, AddressOf q767
            ot.DropDownItems.Add(pss)
            Dim sep As New ToolStripMenuItem
            sep.Text = "Search Engine Presets"
            sep.Image = My.Resources.F9
            sep.BackColor = Color.White
            sep.ForeColor = Color.Black
            sep.Padding = New Padding(0)
            Dim seb As New ToolStripMenuItem("Bing")
            seb.Tag = "https://www.bing.com/search?q="
            AddHandler seb.Click, AddressOf q768
            AddHandler seb.MouseHover, AddressOf q769
            sep.DropDownItems.Add(seb)
            Dim seg As New ToolStripMenuItem("Google")
            seg.Tag = "https://www.google.com/search?q="
            AddHandler seg.Click, AddressOf q770
            AddHandler seg.MouseHover, AddressOf q771
            sep.DropDownItems.Add(seg)
            Dim sed As New ToolStripMenuItem("DuckDuckGo")
            sed.Tag = "https://duckduckgo.com/?q="
            AddHandler sed.Click, AddressOf q772
            AddHandler sed.MouseHover, AddressOf q773
            sep.DropDownItems.Add(sed)
            Dim sec As New ToolStripMenuItem("Custom Search URL")
            AddHandler sec.Click, AddressOf q774
            AddHandler sec.MouseHover, AddressOf q775
            sep.DropDownItems.Add(sec)
            ot.DropDownItems.Add(sep)
            Dim dls As New ToolStripMenuItem
            dls.Text = "Downloads List"
            dls.Image = My.Resources.F9
            dls.BackColor = Color.White
            dls.ForeColor = Color.Black
            dls.Padding = New Padding(0)
            AddHandler dls.DropDownOpening, AddressOf q776
            fl.DropDownItems.Add(dls)
            Dim cdl As New ToolStripMenuItem
            cdl.Text = "Clear Downloads List"
            cdl.Image = My.Resources.F9
            cdl.BackColor = Color.White
            cdl.ForeColor = Color.Black
            cdl.Padding = New Padding(0)
            AddHandler cdl.Click, AddressOf q779
            AddHandler cdl.MouseHover, AddressOf q780
            fl.DropDownItems.Add(cdl)
            Dim scu As New ToolStripMenuItem
            scu.Text = "Save Current Page Shortcut"
            scu.Image = My.Resources.F9
            scu.BackColor = Color.White
            scu.ForeColor = Color.Black
            scu.Padding = New Padding(0)
            AddHandler scu.Click, AddressOf q781
            AddHandler scu.MouseHover, AddressOf q782
            fl.DropDownItems.Add(scu)
            Dim its As New ToolStripMenuItem
            its.Text = "Increase Page Text Size"
            its.Image = My.Resources.F9
            its.BackColor = Color.White
            its.ForeColor = Color.Black
            its.Padding = New Padding(0)
            AddHandler its.Click, AddressOf q783
            AddHandler its.MouseHover, AddressOf q784
            v.DropDownItems.Add(its)
            Dim dts As New ToolStripMenuItem
            dts.Text = "Decrease Page Text Size"
            dts.Image = My.Resources.F9
            dts.BackColor = Color.White
            dts.ForeColor = Color.Black
            dts.Padding = New Padding(0)
            AddHandler dts.Click, AddressOf q785
            AddHandler dts.MouseHover, AddressOf q786
            v.DropDownItems.Add(dts)
            Dim hcp As New ToolStripMenuItem
            hcp.Text = "High Contrast Page"
            hcp.Image = My.Resources.F9
            hcp.BackColor = Color.White
            hcp.ForeColor = Color.Black
            hcp.Padding = New Padding(0)
            AddHandler hcp.Click, AddressOf q787
            AddHandler hcp.MouseHover, AddressOf q788
            v.DropDownItems.Add(hcp)
            Dim crs As New ToolStripMenuItem
            crs.Text = "Copy Readable Page Summary"
            crs.Image = My.Resources.F9
            crs.BackColor = Color.White
            crs.ForeColor = Color.Black
            crs.Padding = New Padding(0)
            AddHandler crs.Click, AddressOf q789
            AddHandler crs.MouseHover, AddressOf q790
            Me.e.DropDownItems.Add(crs)
            Dim ttw As New ToolStripMenuItem
            ttw.Text = "Tile Two Windows"
            ttw.Image = My.Resources.F9
            ttw.BackColor = Color.White
            ttw.ForeColor = Color.Black
            ttw.Padding = New Padding(0)
            AddHandler ttw.Click, AddressOf q791
            AddHandler ttw.MouseHover, AddressOf q792
            v.DropDownItems.Add(ttw)
            Dim ctm7 As New ToolStripMenuItem
            ctm7.Text = "Compact Toolbar Mode"
            ctm7.Image = My.Resources.F9
            ctm7.BackColor = Color.White
            ctm7.ForeColor = Color.Black
            ctm7.Padding = New Padding(0)
            AddHandler ctm7.Click, AddressOf q793
            AddHandler ctm7.MouseHover, AddressOf q794
            ot.DropDownItems.Add(ctm7)
            Dim rwp As New ToolStripMenuItem
            rwp.Text = "Remember Window Size"
            rwp.Image = My.Resources.F9
            rwp.BackColor = Color.White
            rwp.ForeColor = Color.Black
            rwp.Padding = New Padding(0)
            AddHandler rwp.Click, AddressOf q795
            AddHandler rwp.MouseHover, AddressOf q796
            ot.DropDownItems.Add(rwp)
            Dim rwl As New ToolStripMenuItem
            rwl.Text = "Reset Window Layout"
            rwl.Image = My.Resources.F9
            rwl.BackColor = Color.White
            rwl.ForeColor = Color.Black
            rwl.Padding = New Padding(0)
            AddHandler rwl.Click, AddressOf q797
            AddHandler rwl.MouseHover, AddressOf q798
            ot.DropDownItems.Add(rwl)
            Dim hss As New ToolStripMenuItem
            hss.Text = "Search Suggestions From History"
            hss.Image = My.Resources.F9
            hss.BackColor = Color.White
            hss.ForeColor = Color.Black
            hss.Padding = New Padding(0)
            AddHandler hss.Click, AddressOf q799
            AddHandler hss.MouseHover, AddressOf q800
            ot.DropDownItems.Add(hss)
            Dim soe As New ToolStripMenuItem
            soe.Text = "Search With Selected Engine Once"
            soe.Image = My.Resources.F9
            soe.BackColor = Color.White
            soe.ForeColor = Color.Black
            soe.Padding = New Padding(0)
            AddHandler soe.Click, AddressOf q801
            AddHandler soe.MouseHover, AddressOf q802
            ot.DropDownItems.Add(soe)
            Dim sel As New ToolStripMenuItem
            sel.Text = "Save Error Log"
            sel.Image = My.Resources.F9
            sel.BackColor = Color.White
            sel.ForeColor = Color.Black
            sel.Padding = New Padding(0)
            AddHandler sel.Click, AddressOf q803
            AddHandler sel.MouseHover, AddressOf q804
            ot.DropDownItems.Add(sel)
            Dim coh As New ToolStripMenuItem
            coh.Text = "Clear Old History Items"
            coh.Image = My.Resources.F9
            coh.BackColor = Color.White
            coh.ForeColor = Color.Black
            coh.Padding = New Padding(0)
            AddHandler coh.Click, AddressOf q805
            AddHandler coh.MouseHover, AddressOf q806
            hi.DropDownItems.Add(coh)
            Dim vss As New ToolStripMenuItem
            vss.Text = "Validate Saved Settings"
            vss.Image = My.Resources.F9
            vss.BackColor = Color.White
            vss.ForeColor = Color.Black
            vss.Padding = New Padding(0)
            AddHandler vss.Click, AddressOf q807
            AddHandler vss.MouseHover, AddressOf q808
            ot.DropDownItems.Add(vss)
            qleaMark("menus-base")
            qfeatures()
            qleaMark("menus-ready")
            qleaStartup()
            qleaMark("lea-started")
            Dim tz As Size = k.ItemSize
            tz.Width = 240
            tz.Height += 3
            k.ItemSize = tz
            If My.Settings.si = "" Then
                F4.HSB1.Value = 1
                si.Visible = False
                T1.Visible = False
                c.Size = New Size(CInt(Size.Width * 0.69), c.Size.Width)
                F4.O2.ForeColor = Color.Black
                F4.O1.ForeColor = Color.White
                F4.F1.Text = "Turn Siera On"
            Else
                c.Size = New Size(CInt(Size.Width * 0.659), c.Size.Width)
            End If
            qacm()
            qfeature3loaded()
            g.Enabled = False
            qleaMark("load-exit")
        Catch ex As Exception
            qlog(ex)
        Finally
            For index As Integer = menuLayouts.Count - 1 To 0 Step -1
                menuLayouts(index).ResumeLayout(True)
            Next
            qleaMark("menus-resumed")
        End Try
    End Sub
    Sub ki(ByVal sender As Object, ByVal e As DrawItemEventArgs) Handles k.DrawItem
        Dim z5 As TabPage = k.TabPages(e.Index)
        Dim z6 As Rectangle = k.GetTabRect(e.Index)
        Dim z7 As New SolidBrush(Color.Black)
        Dim z8 As New SolidBrush(Color.Black)
        Dim sf As New StringFormat
        sf.Alignment = StringAlignment.Center
        sf.LineAlignment = StringAlignment.Center
        If CBool(e.State And DrawItemState.Selected) Then
            z7.Color = Color.Black
            z8.Color = Color.Black
            z6.Inflate(1, 1)
        End If
        If k.Alignment = TabAlignment.Left Or k.Alignment = TabAlignment.Right Then
            Dim RotateAngle As Single = 90
            If k.Alignment = TabAlignment.Left Then RotateAngle = 270
            Dim cp As New PointF(CSng(z6.Left + (z6.Width / 2)), z6.Top + (z6.Height \ 2))
            e.Graphics.TranslateTransform(cp.X, cp.Y)
            e.Graphics.RotateTransform(RotateAngle)
            z6 = New Rectangle(-(z6.Height \ 2), -(z6.Width \ 2), z6.Height, z6.Width)
        End If
        Dim v1 As Rectangle = k.GetTabRect(e.Index)
        Dim z9 As New RectangleF(
            v1.Left + tm,
            v1.Y + tm,
            v1.Width - 4 * tm,
            v1.Height - 2 * tm)
        Dim v2 As New StringFormat
        e.Graphics.DrawImage(My.Resources.F2, z6)
        Dim v3 As New Font(Font, FontStyle.Bold)
        v2.Alignment = StringAlignment.Center
        v2.LineAlignment = StringAlignment.Center
        e.Graphics.DrawString(
            k.TabPages(e.Index).Text,
            v3,
            z7,
            z9,
            v2)
        e.Graphics.DrawImage(My.Resources.FX, z9.Right - mx - -1, z9.Top + mx - 5, 8, 8)
        e.Graphics.ResetTransform()
        z7.Dispose()
        z8.Dispose()
    End Sub
    Sub kd(ByVal sender As Object, ByVal e As MouseEventArgs) Handles k.MouseDown
        Try
            For s As Integer = 0 To k.TabPages.Count - 1
                k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
                Text = ProductName & " - " & k.SelectedTab.Text
                c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
                If CType(k.SelectedTab.Controls.Item(0), WebBrowser).CanGoBack Then
                    b.Enabled = True
                    BT.Enabled = True
                Else
                    b.Enabled = False
                    BT.Enabled = False
                End If
                If CType(k.SelectedTab.Controls.Item(0), WebBrowser).CanGoForward Then
                    f.Enabled = True
                    FT.Enabled = True
                Else
                    f.Enabled = False
                    FT.Enabled = False
                End If
                LO.Size = New Size(1, 2)
                Dim v1 As Rectangle = k.GetTabRect(s)
                Dim z9 As New RectangleF(
                    v1.Left + tm,
                    v1.Y + tm,
                    v1.Width - 4 * tm,
                    v1.Height - 4 * tm)
                If e.X >= z9.Right - mx AndAlso
                   e.X <= z9.Right AndAlso
                   e.Y >= z9.Top AndAlso
                   e.Y <= z9.Top + mx _
                Then
                    If k.TabCount = 1 = False Then
                        Debug.WriteLine("Tab" & s)
                        rca(CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
                        k.TabPages.RemoveAt(k.SelectedIndex)
                        I = I - 1
                        Dim tz As Size = k.ItemSize
                        tz.Width += 6
                        k.ItemSize = tz
                        k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
                        Text = ProductName & " - " & k.SelectedTab.Text
                        c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
                        If CType(k.SelectedTab.Controls.Item(0), WebBrowser).CanGoBack Then
                            b.Enabled = True
                            BT.Enabled = True
                        Else
                            b.Enabled = False
                            BT.Enabled = False
                        End If
                        If CType(k.SelectedTab.Controls.Item(0), WebBrowser).CanGoForward Then
                            f.Enabled = True
                            FT.Enabled = True
                        Else
                            f.Enabled = False
                            FT.Enabled = False
                        End If
                        Exit Sub
                    Else
                        Close()
                        Exit Sub
                    End If
                End If
            Next s
            Dim pt As Point = New Point(e.X, e.Y)
            Dim tp As TabPage = z4(pt)
            If Not tp Is Nothing Then
                DoDragDrop(tp, DragDropEffects.All)
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub nv(ByVal sender As Object, ByVal e As WebBrowserNavigatedEventArgs)
        Dim browser = TryCast(sender, WebBrowser)
        Dim tab = qleaLegacyTab(browser)
        If tab Is Nothing Then Return
        browser.ScriptErrorsSuppressed = True
        If tab IsNot k.SelectedTab Then Return
        On Error Resume Next
        LO.Size = New Size(1, 2)
        r.Image = My.Resources.F8
        k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
        Dim pn As String = If(My.Settings.th <> "", My.Settings.th, ProductName)
        Text = pn & " - " & k.SelectedTab.Text
        c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
        If My.Settings.b0 = "e" And pv = False Then
            f6.l0.Items.Add(c.Text)
            c.Items.Add(c.Text)
            qash(c.Text)
            qash(CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle)
        ElseIf My.Settings.b0 = "f" Then
        End If
        If CType(k.SelectedTab.Controls.Item(0), WebBrowser).CanGoBack Then
            b.Enabled = True
            BT.Enabled = True
        Else
            b.Enabled = False
            BT.Enabled = False
        End If
        If CType(k.SelectedTab.Controls.Item(0), WebBrowser).CanGoForward Then
            f.Enabled = True
            FT.Enabled = True
        Else
            f.Enabled = False
            FT.Enabled = False
        End If
        g.Enabled = False
    End Sub
    Sub nn(ByVal sender As Object, ByVal e As WebBrowserNavigatingEventArgs)
        Dim browser = TryCast(sender, WebBrowser)
        Dim tab = qleaLegacyTab(browser)
        If tab Is Nothing Then
            e.Cancel = True
            Return
        End If
        browser.ScriptErrorsSuppressed = True
        If qnavigationblocked(e) Then Exit Sub
        If wo = True Then
            e.Cancel = True
            su.Visible = True
            su.Text = "Work offline - navigation blocked"
            Exit Sub
        End If
        If CStr(tab.Tag) = "locked" Then
            e.Cancel = True
            su.Visible = True
            su.Text = "Tab is locked - navigation blocked"
            Exit Sub
        End If
        If tab IsNot k.SelectedTab Then Return
        On Error Resume Next
        Text = ProductName & " Loading..."
        k.SelectedTab.Text = "Please Wait..."
        LO.Size = New Size(Size.Width, 2)
        r.Image = My.Resources.F7
        su.Visible = True
        g.Enabled = False
    End Sub
    Sub pk(ByVal sender As System.Object, ByVal e As EventArgs) Handles PI.Click
        CType(k.SelectedTab.Controls.Item(0), WebBrowser).ShowPropertiesDialog()
    End Sub
    Sub zk(ByVal sender As System.Object, ByVal e As EventArgs) Handles zx.Click
        Try
            Dim B As New WebBrowser
            k.TabPages.Add("New Tab")
            k.SelectTab(I)
            B.Dock = DockStyle.Fill
            k.SelectedTab.Controls.Add(B)
            AddHandler B.Navigating, AddressOf nn
            AddHandler B.Navigated, AddressOf nv
            AddHandler B.DocumentCompleted, AddressOf n0
            AddHandler B.ProgressChanged, AddressOf q14
        AddHandler B.NewWindow, AddressOf qpopup
            I = I + 1
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(qntu())
            Dim tz As Size = k.ItemSize
            If I <= 20 Then
                tz.Width -= 6
            ElseIf I >= 30 Then
                tz.Width += 8
            End If
            k.ItemSize = tz
        Catch ex As Exception
        End Try
    End Sub
    Sub zh(ByVal sender As Object, ByVal e As EventArgs) Handles zx.MouseHover
        If My.Settings.t = "e" Then
            Try
                Dim B As New WebBrowser
                k.TabPages.Add("New Tab")
                k.SelectTab(I)
                B.Dock = DockStyle.Fill
                k.SelectedTab.Controls.Add(B)
                AddHandler B.Navigating, AddressOf nn
                AddHandler B.Navigated, AddressOf nv
                AddHandler B.DocumentCompleted, AddressOf n0
                AddHandler B.ProgressChanged, AddressOf q14
        AddHandler B.NewWindow, AddressOf qpopup
                I = I + 1
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(qntu())
                Dim tz As Size = k.ItemSize
                If I <= 20 Then
                    tz.Width -= 6
                ElseIf I >= 30 Then
                    tz.Width += 8
                End If
                k.ItemSize = tz
            Catch ex As Exception
            End Try
        End If
    End Sub
    Function z4(ByVal pt As Point) As TabPage
        Dim tp As TabPage = Nothing
        For i As Integer = 0 To k.TabPages.Count - 1
            If k.GetTabRect(i).Contains(pt) Then
                tp = k.TabPages(i)
                Exit For
            End If
        Next i
        Return tp
    End Function
    Function z3(ByVal page As TabPage) As Integer
        For i As Integer = 0 To k.TabPages.Count - 1
            If k.TabPages(i) Is page Then
                Return i
            End If
        Next i
        Return -1
    End Function
    Sub l2(ByVal sender As Object, ByVal e As DragEventArgs) Handles k.DragOver
        Dim pt As New Point(e.X, e.Y)
        pt = k.PointToClient(pt)
        Dim ht1 As TabPage = z4(pt)
        If Not ht1 Is Nothing Then
            If e.Data.GetDataPresent(GetType(TabPage)) Then
                e.Effect = DragDropEffects.Link
                Dim dt As TabPage = CType(e.Data.GetData(GetType(TabPage)), TabPage)
                Dim idi As Integer = z3(dt)
                Dim dli As Integer = z3(ht1)
                If idi <> dli Then
                    Dim pages As New ArrayList()
                    For i As Integer = 0 To k.TabPages.Count - 1
                        If i <> idi Then
                            pages.Add(k.TabPages(i))
                        End If
                    Next i
                    pages.Insert(dli, dt)
                    k.TabPages.Clear()
                    k.TabPages.AddRange(CType(pages.ToArray(GetType(TabPage)), TabPage()))
                    k.SelectedTab = dt
                End If
            End If
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub
    Sub j2(ByVal sender As System.Object, ByVal e As EventArgs) Handles y2.Click
        If TopMost = False Then
            m.Visible = False
            k.Dock = DockStyle.Fill
            TopMost = True
            WindowState = FormWindowState.Normal
            FormBorderStyle = FormBorderStyle.None
            WindowState = FormWindowState.Maximized
            ShowIcon = False
            y2.Image = My.Resources.O0
            q.AutoSize = True
        Else
            m.Visible = True
            k.Dock = DockStyle.None
            TopMost = False
            FormBorderStyle = FormBorderStyle.Sizable
            WindowState = FormWindowState.Maximized
            ShowIcon = False
            y2.Image = My.Resources.F1
        End If
    End Sub
    Sub j3(ByVal sender As Object, ByVal e As EventArgs) Handles y2.MouseHover
        If My.Settings.t = "e" Then
            If TopMost = False Then
                m.Visible = False
                k.Dock = DockStyle.Fill
                TopMost = True
                WindowState = FormWindowState.Normal
                FormBorderStyle = FormBorderStyle.None
                WindowState = FormWindowState.Maximized
                ShowIcon = False
                y2.Image = My.Resources.O0
                q.AutoSize = True
            Else
                m.Visible = True
                k.Dock = DockStyle.None
                TopMost = False
                FormBorderStyle = FormBorderStyle.Sizable
                WindowState = FormWindowState.Maximized
                ShowIcon = False
                y2.Image = My.Resources.F1
            End If
        End If
    End Sub
    Sub j4(ByVal sender As Object, ByVal e As EventArgs) Handles m.MouseHover
        If My.Settings.t = "e" Then
            If m Is DropDown Then
                m.DropDown.Hide()
            Else
                m.DropDown.Show()
            End If
        End If
    End Sub
    Sub j5(ByVal sender As System.Object, ByVal e As EventArgs) Handles si.Click
        If T1.Text = "" Then
            c.Size = New Size(CInt(Size.Width * 0.61), c.Size.Width)
            T1.Text = "Listening..."
            My.Computer.Audio.Play(My.Resources.sr, AudioPlayMode.Background)
            si.Image = My.Resources.O1
            y1.Start()
        Else
            c.Size = New Size(CInt(Size.Width * 0.659), c.Size.Width)
            T1.Text = "Looking"
            My.Computer.Audio.Play(My.Resources.so, AudioPlayMode.Background)
            si.Image = My.Resources.F0
            y1.Stop()
            z1.Speak("Looking")
            ls()
        End If
    End Sub
    Sub j6(ByVal sender As System.Object, ByVal e As EventArgs) Handles y1.Tick
        c.Size = New Size(CInt(Size.Width * 0.659), c.Size.Width)
        T1.Text = ""
        My.Computer.Audio.Play(My.Resources.so, AudioPlayMode.Background)
        si.Image = My.Resources.F0
        y1.Stop()
        z1.Speak("Looking")
        ls()
    End Sub
    Sub j7(ByVal sender As Object, ByVal e As EventArgs) Handles si.MouseHover
        If My.Settings.t = "e" Then
            If T1.Text = "" Then
                c.Size = New Size(CInt(Size.Width * 0.61), c.Size.Width)
                T1.Text = "Listening..."
                My.Computer.Audio.Play(My.Resources.sr, AudioPlayMode.Background)
                si.Image = My.Resources.O1
                y1.Start()
            Else
                c.Size = New Size(CInt(Size.Width * 0.659), c.Size.Width)
                T1.Text = "Looking"
                My.Computer.Audio.Play(My.Resources.so, AudioPlayMode.Background)
                si.Image = My.Resources.F0
                y1.Stop()
                z1.Speak("Looking")
                ls()
            End If
        End If
    End Sub
    Public Sub ls()
        If c.Text = ("http://www.bing.com/maps/?v=2&cp=44.162140~-77.377258&lvl=12&dir=0&sty=b&where1=Belleville, ON&form=LMLTCC#Y3A9NDQuMTYyMTQwfi03Ny4zNzcyNTgmbHZsPTEyJnN0eT1iJnE9QmVsbGV2aWxsZSUyNTJDJTI1MjBPTg==") Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate("http://www.bing.com/maps/?v=2&cp=44.232766~-76.487865&lvl=11&dir=0&sty=b&where1=Kingston, ON&form=LMLTCC&adlt=strict#Y3A9NDQuMjMyNzY2fi03Ni40ODc4NjUmbHZsPTExJnN0eT1iJnE9S2luZ3N0b24lMjUyQyUyNTIwT04=")
            T1.Text = ""
            z1.Speak("Here's Kingston Ontario")
        ElseIf c.Text = ("http://www.bing.com/maps/?v=2&cp=44.232766~-76.487865&lvl=11&dir=0&sty=b&where1=Kingston, ON&form=LMLTCC&adlt=strict#Y3A9NDQuMjMyNzY2fi03Ni40ODc4NjUmbHZsPTExJnN0eT1iJnE9S2luZ3N0b24lMjUyQyUyNTIwT04=") Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate("http://www.bing.com/maps/?v=2&cp=43.654170~-79.382698&lvl=9&dir=0&sty=b&where1=Toronto, ON&form=LMLTCC#Y3A9NDMuNjU0MTcwfi03OS4zODI2OTgmbHZsPTkmc3R5PWImcT1Ub3JvbnRvJTI1MkMlMjUyME9O")
            T1.Text = ""
            z1.Speak("Here's Toronto Ontario")
        ElseIf c.Text = ("http://www.bing.com/maps/?v=2&cp=43.654170~-79.382698&lvl=9&dir=0&sty=b&where1=Toronto, ON&form=LMLTCC#Y3A9NDMuNjU0MTcwfi03OS4zODI2OTgmbHZsPTkmc3R5PWImcT1Ub3JvbnRvJTI1MkMlMjUyME9O") Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate("http://binged.it/RdYfKM")
            T1.Text = ""
            z1.Speak("Here's Belleville Ontario")
        Else
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate("http://binged.it/RdYfKM")
            T1.Text = ""
            z1.Speak("Here's Belleville Ontario")
        End If
    End Sub
    Sub j8(ByVal sender As System.Object, ByVal e As EventArgs) Handles ti.Tick
        If k Is Nothing OrElse k.SelectedTab Is Nothing Then Return
        If k.SelectedTab.Text = "(1) Facebook" Then
            My.Computer.Audio.Play(My.Resources.sr, AudioPlayMode.Background)
            si.Image = My.Resources.F2
        End If
    End Sub
    Sub j9(ByVal sender As Object, ByVal e As EventArgs) Handles PI.MouseHover
        If My.Settings.t = "e" Then
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).ShowPropertiesDialog()
        End If
    End Sub
    Sub k1(ByVal sender As Object, ByVal e As KeyEventArgs) Handles x.KeyUp
        If x.Text = "" Then
            s.Enabled = False
        ElseIf x.Text = "search" Then
            s.Enabled = False
        Else
            s.Enabled = True
        End If
    End Sub
    Sub cm2(sender As Object, e As EventArgs) Handles x.MouseEnter
        If x.Text = "" Then
            s.Enabled = False
        ElseIf x.Text = "search" Then
            s.Enabled = False
        Else
            s.Enabled = True
        End If
    End Sub
    Sub oi2(ByVal sender As Object, ByVal e As EventArgs) Handles x.SelectedIndexChanged
        If x.Text = "" Then
            s.Enabled = False
        ElseIf x.Text = "search" Then
            s.Enabled = False
        Else
            s.Enabled = True
        End If
    End Sub
    Sub k2(ByVal sender As Object, ByVal e As KeyEventArgs) Handles c.KeyUp
        If c.Text = "" Then
            g.Enabled = False
        Else
            g.Enabled = True
        End If
    End Sub
    Sub cm1(sender As Object, e As EventArgs) Handles c.MouseEnter
        If c.Text = "" Then
            g.Enabled = False
        Else
            g.Enabled = True
        End If
    End Sub
    Sub oi1(ByVal sender As Object, ByVal e As EventArgs) Handles c.SelectedIndexChanged
        If c.Text = "" Then
            g.Enabled = False
        Else
            g.Enabled = True
        End If
    End Sub
    Sub po(ByVal sender As Object, ByVal e As EventArgs) Handles BP.MouseHover
        If My.Settings.t = "e" Then
            Try
                pc(sender, e)
            Catch ex As Exception
                qlog(ex)
            End Try
        End If
    End Sub
    Sub k3(sender As System.Object, e As EventArgs) Handles W.Click
        Try
            wo = Not wo
            su.Visible = True
            If wo = True Then
                su.Text = "Work offline on"
            Else
                su.Text = "Work offline off"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub k4(sender As System.Object, e As EventArgs) Handles SL.Click
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim u As String = wb.Url.ToString
            Process.Start("mailto:?subject=" & Uri.EscapeDataString(wb.DocumentTitle) & "&body=" & Uri.EscapeDataString(u))
            su.Visible = True
            su.Text = "Send link opened"
        Catch ex As Exception
        End Try
    End Sub
    Sub k5(sender As System.Object, e As EventArgs) Handles zi.Click
        Try
            Dim doc As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.ScrollRectangle.Size
            doc = Size.Add(CType(doc, Drawing.Size), New Size(50, 50))
            Dim wb As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Size
            Dim factor As String = (Math.Round(Math.Max(wb.Width / doc.Width, wb.Height / doc.Width), 2) * 500).ToString & "%"
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.Style &= ";zoom:" & factor
        Catch ex As Exception
        End Try
    End Sub
    Sub k6(sender As Object, e As EventArgs) Handles zi.MouseHover
        If My.Settings.t = "e" Then
            Try
                Dim doc As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.ScrollRectangle.Size
                doc = Size.Add(CType(doc, Drawing.Size), New Size(50, 50))
                Dim wb As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Size
                Dim factor As String = (Math.Round(Math.Max(wb.Width / doc.Width, wb.Height / doc.Width), 2) * 500).ToString & "%"
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.Style &= ";zoom:" & factor
            Catch ex As Exception
            End Try
        End If
    End Sub
    Sub k7(sender As System.Object, e As EventArgs) Handles zo.Click
        Try
            Dim doc As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.ScrollRectangle.Size
            doc = Size.Add(CType(doc, Drawing.Size), New Size(150, 150))
            Dim wb As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Size
            Dim factor As String = (Math.Round(Math.Min(wb.Width / doc.Width, wb.Height / doc.Width), 2) * 100).ToString & "%"
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.Style &= ";zoom:" & factor
        Catch ex As Exception
        End Try
    End Sub
    Sub k8(sender As Object, e As EventArgs) Handles zo.MouseHover
        If My.Settings.t = "e" Then
            Try
                Dim doc As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.ScrollRectangle.Size
                doc = Size.Add(CType(doc, Drawing.Size), New Size(150, 150))
                Dim wb As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Size
                Dim factor As String = (Math.Round(Math.Min(wb.Width / doc.Width, wb.Height / doc.Width), 2) * 100).ToString & "%"
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.Style &= ";zoom:" & factor
            Catch ex As Exception
            End Try
        End If
    End Sub
    Sub k9(sender As System.Object, e As EventArgs) Handles es.Click
        Try
            Dim doc As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.ScrollRectangle.Size
            doc = Size.Add(CType(doc, Drawing.Size), New Size(150, 150))
            Dim wb As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Size
            Dim factor As String = (Math.Round(Math.Max(wb.Width / doc.Width, wb.Height / doc.Width), 2) * 200).ToString & "%"
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.Style &= ";zoom:" & factor
        Catch ex As Exception
        End Try
    End Sub
    Sub l1(sender As Object, e As EventArgs) Handles es.MouseHover
        If My.Settings.t = "e" Then
            Try
                Dim doc As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.ScrollRectangle.Size
                doc = Size.Add(CType(doc, Drawing.Size), New Size(150, 150))
                Dim wb As Size = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Size
                Dim factor As String = (Math.Round(Math.Max(wb.Width / doc.Width, wb.Height / doc.Width), 2) * 150).ToString & "%"
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.Style &= ";zoom:" & factor
            Catch ex As Exception
            End Try
        End If
    End Sub
    Public Sub u8(ByVal fn1 As Form)
        Try
            If Screen.PrimaryScreen.WorkingArea.Width() = 800 Then
                fn1.Scale(0.55)
                k.Dock = DockStyle.Fill
            ElseIf Screen.PrimaryScreen.WorkingArea.Width() = 1024 Then
                fn1.Scale(0.77)
                k.Dock = DockStyle.Fill
            ElseIf Screen.PrimaryScreen.WorkingArea.Width() = 1360 Then
                fn1.Scale(1.15)
                k.Dock = DockStyle.Fill
            ElseIf Screen.PrimaryScreen.WorkingArea.Width = 1366 Then
                fn1.Scale(1.02)
                k.Dock = DockStyle.Fill
            End If
        Catch ex As Exception
        End Try
    End Sub
    Public Sub u9(sender As System.Object, e As EventArgs) Handles k.MouseDown
        Try
            If MouseButtons = MouseButtons.Middle Then
                If k.TabCount = 1 = False Then
                    rca(CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
                    k.TabPages.RemoveAt(k.SelectedIndex)
                    I = I - 1
                    Dim tz As Size = k.ItemSize
                    tz.Width += 6
                    k.ItemSize = tz
                    k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
                    Text = ProductName & " - " & k.SelectedTab.Text
                    c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
                Else
                    End
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub
    Public Sub c0(sender As Object, e As EventArgs) Handles c.Click
        Try
        Catch ex As Exception
        End Try
    End Sub
    Sub n0(ByVal sender As Object, ByVal e As WebBrowserDocumentCompletedEventArgs)
        Dim browser = TryCast(sender, WebBrowser)
        Dim tab = qleaLegacyTab(browser)
        If tab Is Nothing Then Return
        browser.ScriptErrorsSuppressed = True
        On Error Resume Next
        tab.Text = browser.DocumentTitle
        If pv = False Then
            If browser.Url IsNot Nothing Then qash(browser.Url.ToString())
            qash(tab.Text)
        End If
        If tab Is k.SelectedTab Then
            LO.Size = New Size(1, 2)
            r.Image = My.Resources.F8
            su.Visible = False
            Text = ProductName & " - " & tab.Text
            If browser.Url IsNot Nothing Then c.Text = browser.Url.ToString()
            g.Enabled = False
        End If
        qapplydomain(CType(sender, WebBrowser))
        qapplyusercontent(CType(sender, WebBrowser))
        qapplycontentfilters(CType(sender, WebBrowser))
        qapplypermissions(CType(sender, WebBrowser))
        qapplytabgroup(CType(sender, WebBrowser))
        qapplyannotations(CType(sender, WebBrowser))
        qrestoreform(CType(sender, WebBrowser))
        If CType(sender, WebBrowser).Url = e.Url Then
            qanalyticsvisit(CType(sender, WebBrowser))
            If k.SelectedTab IsNot Nothing AndAlso k.SelectedTab.Controls.Count > 0 AndAlso k.SelectedTab.Controls(0) Is sender Then qapplysitechrome(CType(sender, WebBrowser))
        End If
    End Sub
    Sub y5(sender As Object, e As EventArgs) Handles y3.Click
        y9.Show()
        Bmp = New Bitmap(y9.Width, y9.Height)
        y9.Image = Bmp
    End Sub
    Sub y4(sender As Object, e As EventArgs) Handles y3.MouseHover
        If My.Settings.t = "e" Then
            y9.Show()
            Bmp = New Bitmap(y9.Width, y9.Height)
            y9.Image = Bmp
        End If
    End Sub
    Sub PaintBrush(X As Integer, Y As Integer)
        Using y5 As Graphics = Graphics.FromImage(y9.Image)
            y5.FillRectangle(New SolidBrush(Dcolo), New Rectangle(X, Y, Dra, Dra))
        End Using
        y9.Refresh()
    End Sub
    Sub y6(sender As Object, e As MouseEventArgs) Handles y9.MouseDown
        Draw = True
        PaintBrush(e.X, e.Y)
    End Sub
    Sub y7(sender As Object, e As MouseEventArgs) Handles y9.MouseMove
        If Draw = True Then
            PaintBrush(e.X, e.Y)
        End If
    End Sub
    Sub y8(sender As Object, e As MouseEventArgs) Handles y9.MouseUp
        Draw = False
    End Sub
    Sub q1(ByVal u As String)
        Try
            If qleaDo("go", u) Then Return
            Dim aliasAddress As String = ""
            If qresolvealias(u, aliasAddress) Then u = aliasAddress
            Dim address As Uri = Nothing
            If Uri.TryCreate(u, UriKind.Absolute, address) AndAlso (address.Scheme = Uri.UriSchemeHttp OrElse address.Scheme = Uri.UriSchemeHttps OrElse address.Scheme = Uri.UriSchemeFile OrElse address.Scheme = "about") Then
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(address.AbsoluteUri)
            ElseIf u = "" Then
            ElseIf u.Contains(" ") Or u.Contains(".") = False Then
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(My.Settings.se & Uri.EscapeDataString(u))
            ElseIf u.Contains("://") Or u.StartsWith("about:") Or u.StartsWith("file:") Then
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(u)
            Else
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate("http://" & u)
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q2()
        Try
            If x.Text = "" Then
            ElseIf x.Text = "search" Then
            Else
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(My.Settings.se & Uri.EscapeDataString(x.Text))
                x.Items.Add(x.Text)
                My.Settings.s.Add(x.Text)
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q3(ByVal u As String)
        Try
            Dim B As New WebBrowser
            k.TabPages.Add("New Tab")
            k.SelectTab(I)
            B.Dock = DockStyle.Fill
            k.SelectedTab.Controls.Add(B)
            AddHandler B.Navigating, AddressOf nn
            AddHandler B.Navigated, AddressOf nv
            AddHandler B.DocumentCompleted, AddressOf n0
            AddHandler B.ProgressChanged, AddressOf q14
        AddHandler B.NewWindow, AddressOf qpopup
            I = I + 1
            q1(u)
            Dim tz As Size = k.ItemSize
            If I <= 20 Then
                tz.Width -= 6
            ElseIf I >= 30 Then
                tz.Width += 8
            End If
            k.ItemSize = tz
        Catch ex As Exception
        End Try
    End Sub
    Sub q4(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaDuplicate()
        Catch ex As Exception
        End Try
    End Sub
    Sub q5(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q4(sender, e)
        End If
    End Sub
    Sub q6(ByVal sender As Object, ByVal e As EventArgs)
        qleaReopen()
    End Sub
    Sub q7(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q6(sender, e)
        End If
    End Sub
    Sub q8(ByVal sender As Object, ByVal e As KeyEventArgs) Handles Me.KeyDown
        Try
            If qcustomshortcut(e) Then
                Exit Sub
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.T Then
                q6(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.C Then
                q20(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.R Then
                q22(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.L Then
                q167(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.B Then
                q171(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.U Then
                q177(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.I Then
                q187(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.S Then
                q203(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.E Then
                q209(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.M Then
                q211(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.D Then
                q227(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.K Then
                q221(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.V Then
                q239(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.F Then
                q241(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.Shift And e.KeyCode = Keys.P Then
                q809(sender, e)
                e.SuppressKeyPress = True
            ElseIf e.Control And e.KeyCode = Keys.L Then
                c.Focus()
                c.SelectAll()
                e.SuppressKeyPress = True
            ElseIf e.Control And e.KeyCode = Keys.T Then
                N.PerformClick()
                e.SuppressKeyPress = True
            ElseIf e.Control And e.KeyCode = Keys.W Then
                CT.PerformClick()
                e.SuppressKeyPress = True
            ElseIf e.Control And e.KeyCode = Keys.R Then
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Refresh()
                e.SuppressKeyPress = True
            ElseIf e.Control And e.KeyCode = Keys.D Then
                BP.PerformClick()
                e.SuppressKeyPress = True
            ElseIf e.Control And e.KeyCode = Keys.H Then
                f6.Show()
                e.SuppressKeyPress = True
            ElseIf e.Control And e.KeyCode = Keys.B Then
                F7.Show()
                e.SuppressKeyPress = True
            ElseIf e.KeyCode = Keys.F5 Then
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Refresh()
                e.SuppressKeyPress = True
            ElseIf e.Alt And e.KeyCode = Keys.Left Then
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoBack()
                e.SuppressKeyPress = True
            ElseIf e.Alt And e.KeyCode = Keys.Right Then
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).GoForward()
                e.SuppressKeyPress = True
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q9(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            qfeatureclosing()
            If pv = False Then
                My.Settings.ls.Clear()
                My.Settings.lcw.Clear()
                For q As Integer = 0 To k.TabPages.Count - 1
                    If k.TabPages(q).Controls.Count > 0 Then
                        Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                        Dim savedUrl As String = qpersistedtaburl(k.TabPages(q), wb)
                        If savedUrl <> "" Then
                            My.Settings.ls.Add(savedUrl)
                            My.Settings.lcw.Add(savedUrl)
                        End If
                    End If
                Next q
            End If
            If My.Settings.rws = "e" And FormBorderStyle <> FormBorderStyle.None Then
                Dim savedBounds As Rectangle = If(WindowState = FormWindowState.Normal, Bounds, RestoreBounds)
                My.Settings.wl = savedBounds.Left.ToString
                My.Settings.wt = savedBounds.Top.ToString
                My.Settings.ww = savedBounds.Width.ToString
                My.Settings.wh = savedBounds.Height.ToString
                My.Settings.ws = If(WindowState = FormWindowState.Maximized, "Maximized", "Normal")
            End If
            qclearonexit()
            My.Settings.Save()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q10(ByVal sender As System.Object, ByVal e As EventArgs) Handles FI.Click
        Try
            Dim term As String = InputBox("Enter text to find on this page:", "Find In Current Page", "")
            If term = "" Then Exit Sub
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Document Is Nothing Or wb.Document.Body Is Nothing Then
                su.Visible = True
                su.Text = "No page match found"
                Exit Sub
            End If
            Dim pageRange As Object = wb.Document.Body.DomElement.createTextRange()
            If CBool(pageRange.findText(term)) Then
                pageRange.select()
                pageRange.scrollIntoView(True)
                su.Visible = True
                su.Text = "Page match found"
            Else
                su.Visible = True
                su.Text = "No page match found"
            End If
        Catch ex As Exception
            su.Visible = True
            su.Text = "No page match found"
        End Try
    End Sub
    Sub q11(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim target = qleaSessionTarget(qleaUrl(qleaSelectedTab()))
            If target = "" Then Return
            My.Settings.h = target
            My.Settings.Save()
            su.Visible = True
            su.Text = "Home page saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q12(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q11(sender, e)
        End If
    End Sub
    Sub q13(ByVal sender As Object, ByVal e As EventArgs) Handles FI.MouseHover
        If My.Settings.t = "e" Then
            q10(sender, e)
        End If
    End Sub
    Sub q14(ByVal sender As Object, ByVal e As WebBrowserProgressChangedEventArgs)
        Dim tab = qleaLegacyTab(TryCast(sender, WebBrowser))
        If tab Is Nothing OrElse tab IsNot k.SelectedTab Then Return
        Try
            If e.MaximumProgress > 0 And e.CurrentProgress > 0 Then
                Dim sz As Integer = CInt((e.CurrentProgress / e.MaximumProgress) * Size.Width)
                If sz < 1 Then sz = 1
                If sz > Size.Width Then sz = Size.Width
                LO.Size = New Size(sz, 2)
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q16(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaCloseGroup("a")
        Catch ex As Exception
        End Try
    End Sub
    Sub q17(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q16(sender, e)
        End If
    End Sub
    Sub q18(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If pv = False Then
                pv = True
                My.Settings.b0 = "f"
                f6.l0.Items.Clear()
                c.Items.Clear()
                su.Visible = True
                su.Text = "Private mode on"
                Text = ProductName & " - Private"
            Else
                pv = False
                My.Settings.b0 = "e"
                su.Visible = True
                su.Text = "Private mode off"
                Text = ProductName & " - " & k.SelectedTab.Text
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q19(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q18(sender, e)
        End If
    End Sub
    Sub q20(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim target = qleaUrl(qleaSelectedTab())
            If target = "" Then Return
            Clipboard.SetText(target)
            su.Visible = True
            su.Text = "Page link copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q21(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q20(sender, e)
        End If
    End Sub
    Sub q22(ByVal sender As Object, ByVal e As EventArgs)
        Try
            For Each tab As TabPage In k.TabPages
                If tab.Controls.Count = 0 OrElse suspendedGroupUrls.ContainsKey(tab) Then Continue For
                Dim host = qleaHost(tab)
                If host IsNot Nothing Then
                    qleaReload(host)
                Else
                    Dim browser = TryCast(tab.Controls(0), WebBrowser)
                    If browser IsNot Nothing Then browser.Refresh()
                End If
            Next
        Catch ex As Exception
        End Try
    End Sub
    Sub q23(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q22(sender, e)
        End If
    End Sub
    Sub q24(ByVal sender As Object, ByVal e As EventArgs)
        Try
            tl.DropDownItems.Clear()
            For q As Integer = 0 To k.TabPages.Count - 1
                Dim ti As New ToolStripMenuItem
                ti.Text = k.TabPages(q).Text
                If ti.Text = "" Then ti.Text = "New Tab"
                ti.Tag = k.TabPages(q)
                ti.Image = My.Resources.F9
                ti.BackColor = Color.White
                ti.ForeColor = Color.Black
                ti.Padding = New Padding(0)
                AddHandler ti.Click, AddressOf q25
                AddHandler ti.MouseHover, AddressOf q25
                tl.DropDownItems.Add(ti)
            Next q
        Catch ex As Exception
        End Try
    End Sub
    Sub q25(ByVal sender As ToolStripMenuItem, ByVal e As EventArgs)
        Try
            Dim tab = TryCast(sender.Tag, TabPage)
            If tab Is Nothing OrElse tab.IsDisposed OrElse Not k.TabPages.Contains(tab) Then Return
            k.SelectedTab = tab
            qleaTabChanged(k, EventArgs.Empty)
        Catch ex As Exception
        End Try
    End Sub
    Sub q26(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q24(sender, e)
            tl.DropDown.Show()
        End If
    End Sub
    Sub q27(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            SX.Filter = "PNG Image [*.png*]|*.png|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Screenshot"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim bm As New Bitmap(wb.Width, wb.Height)
            Dim pt As Point = wb.PointToScreen(New Point(0, 0))
            Using gr As Graphics = Graphics.FromImage(bm)
                gr.CopyFromScreen(pt, New Point(0, 0), wb.Size)
            End Using
            bm.Save(SX.FileName, Imaging.ImageFormat.Png)
            bm.Dispose()
            su.Visible = True
            su.Text = "Screenshot saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q28(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q27(sender, e)
        End If
    End Sub
    Sub q29(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If MessageBox.Show("Clear the saved tab session?", "Clear Saved Session", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
            qleaClearSession()
            su.Visible = True
            su.Text = "Saved session cleared"
        Catch ex As Exception
            qlog(ex)
            su.Visible = True
            su.Text = "Saved session could not be cleared"
        End Try
    End Sub
    Sub q30(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q29(sender, e)
        End If
    End Sub
    Sub q31(ByVal sender As Object, ByVal e As EventArgs)
        Try
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim url = qleaUrl(k.TabPages(q))
                    If url <> "" Then
                        My.Settings.bm.Add(url)
                        Dim fv As New ToolStripMenuItem
                        fv.Text = k.TabPages(q).Text
                        If fv.Text = "" Then fv.Text = url
                        fv.Tag = url
                        fv.Image = My.Resources.F9
                        fv.BackgroundImage = My.Resources.F2
                        fv.BackgroundImageLayout = ImageLayout.Stretch
                        fv.ForeColor = Color.White
                        AddHandler fv.Click, AddressOf z2
                        AddHandler fv.MouseHover, AddressOf z2
                        RB.DropDownItems.Add(fv)
                    End If
                End If
            Next q
            My.Settings.Save()
            su.Visible = True
            su.Text = "All tabs bookmarked"
        Catch ex As Exception
        End Try
    End Sub
    Sub q32(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q31(sender, e)
        End If
    End Sub
    Sub q33(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaCloseGroup("r")
        Catch ex As Exception
        End Try
    End Sub
    Sub q34(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q33(sender, e)
        End If
    End Sub
    Sub q35(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim msg As String = "Title: " & wb.DocumentTitle & vbCrLf &
                "Address: " & wb.Url.ToString & vbCrLf &
                "Status: " & wb.StatusText & vbCrLf &
                "Can Go Back: " & wb.CanGoBack.ToString & vbCrLf &
                "Can Go Forward: " & wb.CanGoForward.ToString
            MessageBox.Show(msg, "Page Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
        End Try
    End Sub
    Sub q36(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q35(sender, e)
        End If
    End Sub
    Sub q37(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) & "\Downloads")
        Catch ex As Exception
        End Try
    End Sub
    Sub q38(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q37(sender, e)
        End If
    End Sub
    Sub q39(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim f As New j
            f.Show()
            f.q1(CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
        Catch ex As Exception
        End Try
    End Sub
    Sub q40(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q39(sender, e)
        End If
    End Sub
    Sub q41(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Clipboard.SetText(CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle)
            su.Visible = True
            su.Text = "Page title copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q42(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q41(sender, e)
        End If
    End Sub
    Sub q43(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As New ArrayList
            For q As Integer = 0 To My.Settings.bm.Count - 1
                Dim u As String = My.Settings.bm.Item(q)
                If u = "" Then
                ElseIf ar.Contains(u) Then
                Else
                    ar.Add(u)
                End If
            Next q
            My.Settings.bm.Clear()
            RB.DropDownItems.Clear()
            For q As Integer = 0 To ar.Count - 1
                My.Settings.bm.Add(ar.Item(q).ToString)
                Dim fv As New ToolStripMenuItem
                fv.Text = ar.Item(q).ToString
                fv.Tag = ar.Item(q).ToString
                fv.Image = My.Resources.F9
                fv.BackgroundImage = My.Resources.F2
                fv.BackgroundImageLayout = ImageLayout.Stretch
                fv.ForeColor = Color.White
                AddHandler fv.Click, AddressOf z2
                AddHandler fv.MouseHover, AddressOf z2
                RB.DropDownItems.Add(fv)
            Next q
            My.Settings.Save()
            su.Visible = True
            su.Text = "Duplicate bookmarks removed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q44(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q43(sender, e)
        End If
    End Sub
    Sub q45(ByVal sender As Object, ByVal e As EventArgs)
        Try
            For q As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(q) = "" Then
                Else
                    q3(My.Settings.bm.Item(q))
                End If
            Next q
        Catch ex As Exception
        End Try
    End Sub
    Sub q46(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q45(sender, e)
        End If
    End Sub
    Sub q47(ByVal sender As Object, ByVal e As EventArgs)
        Try
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    CType(k.TabPages(q).Controls.Item(0), WebBrowser).Stop()
                End If
            Next q
            LO.Size = New Size(1, 2)
            r.Image = My.Resources.F8
            su.Visible = False
        Catch ex As Exception
        End Try
    End Sub
    Sub q48(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q47(sender, e)
        End If
    End Sub
    Sub q49(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Tabs.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save All Tab Links"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        mySW.WriteLine(wb.Url.ToString)
                    End If
                End If
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Tab links saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q50(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q49(sender, e)
        End If
    End Sub
    Sub q51(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.SelectedIndex > 0 Then
                Dim ix As Integer = k.SelectedIndex
                Dim tp As TabPage = k.SelectedTab
                k.TabPages.RemoveAt(ix)
                k.TabPages.Insert(ix - 1, tp)
                k.SelectedTab = tp
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q52(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q51(sender, e)
        End If
    End Sub
    Sub q53(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.SelectedIndex < k.TabPages.Count - 1 Then
                Dim ix As Integer = k.SelectedIndex
                Dim tp As TabPage = k.SelectedTab
                k.TabPages.RemoveAt(ix)
                k.TabPages.Insert(ix + 1, tp)
                k.SelectedTab = tp
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q54(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q53(sender, e)
        End If
    End Sub
    Sub q55(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Clipboard.SetText(wb.DocumentTitle & vbCrLf & wb.Url.ToString)
            su.Visible = True
            su.Text = "Page title and link copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q56(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q55(sender, e)
        End If
    End Sub
    Sub q57(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ox As New OpenFileDialog()
            ox.Title = "Open Saved Tab Links"
            ox.CheckFileExists = True
            ox.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            ox.ShowDialog()
            If ox.FileName = "" Then Exit Sub
            Dim ar() As String = System.IO.File.ReadAllLines(ox.FileName)
            For q As Integer = 0 To ar.Length - 1
                If ar(q) = "" Then
                Else
                    q3(ar(q))
                End If
            Next q
        Catch ex As Exception
        End Try
    End Sub
    Sub q58(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q57(sender, e)
        End If
    End Sub
    Sub q59(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Process.Start(CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
        Catch ex As Exception
        End Try
    End Sub
    Sub q60(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q59(sender, e)
        End If
    End Sub
    Sub q61(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If MessageBox.Show("Clear address and search suggestions?", "Clear Suggestions", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
            c.Items.Clear()
            x.Items.Clear()
            My.Settings.s.Clear()
            su.Visible = True
            su.Text = "Address and search suggestions cleared"
        Catch ex As Exception
        End Try
    End Sub
    Sub q62(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q61(sender, e)
        End If
    End Sub
    Sub q63(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Bookmarks.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Export Bookmarks"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            For q As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(q) = "" Then
                Else
                    mySW.WriteLine(My.Settings.bm.Item(q))
                End If
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Bookmarks exported"
        Catch ex As Exception
        End Try
    End Sub
    Sub q64(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q63(sender, e)
        End If
    End Sub
    Sub q65(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ox As New OpenFileDialog()
            ox.Title = "Import Bookmarks"
            ox.CheckFileExists = True
            ox.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            ox.ShowDialog()
            If ox.FileName = "" Then Exit Sub
            Dim ar() As String = System.IO.File.ReadAllLines(ox.FileName)
            For q As Integer = 0 To ar.Length - 1
                If ar(q) = "" Then
                Else
                    My.Settings.bm.Add(ar(q))
                    Dim fv As New ToolStripMenuItem
                    fv.Text = ar(q)
                    fv.Tag = ar(q)
                    fv.Image = My.Resources.F9
                    fv.BackgroundImage = My.Resources.F2
                    fv.BackgroundImageLayout = ImageLayout.Stretch
                    fv.ForeColor = Color.White
                    AddHandler fv.Click, AddressOf z2
                    AddHandler fv.MouseHover, AddressOf z2
                    RB.DropDownItems.Add(fv)
                End If
            Next q
            My.Settings.Save()
            su.Visible = True
            su.Text = "Bookmarks imported"
        Catch ex As Exception
        End Try
    End Sub
    Sub q66(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q65(sender, e)
        End If
    End Sub
    Sub q67(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As New ArrayList
            For q As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(q) = "" Then
                Else
                    ar.Add(My.Settings.bm.Item(q))
                End If
            Next q
            ar.Sort()
            My.Settings.bm.Clear()
            RB.DropDownItems.Clear()
            For q As Integer = 0 To ar.Count - 1
                My.Settings.bm.Add(ar.Item(q).ToString)
                Dim fv As New ToolStripMenuItem
                fv.Text = ar.Item(q).ToString
                fv.Tag = ar.Item(q).ToString
                fv.Image = My.Resources.F9
                fv.BackgroundImage = My.Resources.F2
                fv.BackgroundImageLayout = ImageLayout.Stretch
                fv.ForeColor = Color.White
                AddHandler fv.Click, AddressOf z2
                AddHandler fv.MouseHover, AddressOf z2
                RB.DropDownItems.Add(fv)
            Next q
            My.Settings.Save()
            su.Visible = True
            su.Text = "Bookmarks sorted"
        Catch ex As Exception
        End Try
    End Sub
    Sub q68(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q67(sender, e)
        End If
    End Sub
    Sub q69(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = ""
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        tx &= wb.Url.ToString & vbCrLf
                    End If
                End If
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "All tab links copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q70(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q69(sender, e)
        End If
    End Sub
    Sub q71(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As New ArrayList
            For q As Integer = k.TabPages.Count - 1 To 0 Step -1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        If ar.Contains(wb.Url.ToString) Then
                            rca(wb.Url.ToString)
                            k.TabPages.RemoveAt(q)
                            I = I - 1
                            Dim tz As Size = k.ItemSize
                            tz.Width += 6
                            k.ItemSize = tz
                        Else
                            ar.Add(wb.Url.ToString)
                        End If
                    End If
                End If
            Next q
            k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Text = ProductName & " - " & k.SelectedTab.Text
            c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
            su.Visible = True
            su.Text = "Duplicate tabs closed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q72(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q71(sender, e)
        End If
    End Sub
    Sub q73(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = ""
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        tx &= wb.DocumentTitle & " - " & wb.Url.ToString & vbCrLf
                    End If
                End If
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "All tab titles and links copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q74(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q73(sender, e)
        End If
    End Sub
    Sub q75(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As New ArrayList
            For q As Integer = 0 To k.TabPages.Count - 1
                ar.Add(k.TabPages(q))
            Next q
            For q As Integer = 0 To ar.Count - 2
                For w As Integer = q + 1 To ar.Count - 1
                    If String.Compare(CType(ar(q), TabPage).Text, CType(ar(w), TabPage).Text) > 0 Then
                        Dim tp As TabPage = CType(ar(q), TabPage)
                        ar(q) = ar(w)
                        ar(w) = tp
                    End If
                Next w
            Next q
            Dim st As TabPage = k.SelectedTab
            k.TabPages.Clear()
            For q As Integer = 0 To ar.Count - 1
                k.TabPages.Add(CType(ar(q), TabPage))
            Next q
            k.SelectedTab = st
            su.Visible = True
            su.Text = "Tabs sorted"
        Catch ex As Exception
        End Try
    End Sub
    Sub q76(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q75(sender, e)
        End If
    End Sub
    Sub q77(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.InvokeScript("eval", New Object() {"document.selection ? document.selection.createRange().text : window.getSelection().toString();"}).ToString
            If tx = "" Then
            Else
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(My.Settings.se & Uri.EscapeDataString(tx))
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q78(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q77(sender, e)
        End If
    End Sub
    Sub q79(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim f As New j
            f.Show()
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        If q = 0 Then
                            f.q1(wb.Url.ToString)
                        Else
                            f.q3(wb.Url.ToString)
                        End If
                    End If
                End If
            Next q
        Catch ex As Exception
        End Try
    End Sub
    Sub q80(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q79(sender, e)
        End If
    End Sub
    Sub q81(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider History.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Export History"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            For q As Integer = 0 To f6.l0.Items.Count - 1
                mySW.WriteLine(f6.l0.Items.Item(q).ToString)
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "History exported"
        Catch ex As Exception
        End Try
    End Sub
    Sub q82(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q81(sender, e)
        End If
    End Sub
    Sub q83(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.rs = "e" Then
                My.Settings.rs = ""
                su.Visible = True
                su.Text = "Startup will open homepage"
            Else
                My.Settings.rs = "e"
                su.Visible = True
                su.Text = "Startup will restore session"
            End If
            My.Settings.Save()
        Catch ex As Exception
        End Try
    End Sub
    Sub q84(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q83(sender, e)
        End If
    End Sub
    Sub q85(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaCloseGroup("l")
        Catch ex As Exception
        End Try
    End Sub
    Sub q86(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q85(sender, e)
        End If
    End Sub
    Sub q87(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle & ".txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Current Page Text"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim tx As String = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.InnerText
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.Write(tx)
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Page text saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q88(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q87(sender, e)
        End If
    End Sub
    Sub q89(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim st As TabPage = k.SelectedTab
            Dim ar As New ArrayList
            For q As Integer = k.TabPages.Count - 1 To 0 Step -1
                ar.Add(k.TabPages(q))
            Next q
            k.TabPages.Clear()
            For q As Integer = 0 To ar.Count - 1
                k.TabPages.Add(CType(ar(q), TabPage))
            Next q
            k.SelectedTab = st
            su.Visible = True
            su.Text = "Tabs reversed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q90(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q89(sender, e)
        End If
    End Sub
    Sub q91(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Clipboard.SetText(CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.InnerText)
            su.Visible = True
            su.Text = "Page text copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q92(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q91(sender, e)
        End If
    End Sub
    Sub q93(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle & ".html"
            SX.Filter = "HTML [*.html*]|*.html|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Current Page HTML"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.Write(CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentText)
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Page HTML saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q94(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q93(sender, e)
        End If
    End Sub
    Sub q95(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ox As New OpenFileDialog()
            ox.Title = "Import History"
            ox.CheckFileExists = True
            ox.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            ox.ShowDialog()
            If ox.FileName = "" Then Exit Sub
            Dim ar() As String = System.IO.File.ReadAllLines(ox.FileName)
            For q As Integer = 0 To ar.Length - 1
                If ar(q) = "" Then
                Else
                    f6.l0.Items.Add(ar(q))
                    c.Items.Add(ar(q))
                End If
            Next q
            su.Visible = True
            su.Text = "History imported"
        Catch ex As Exception
        End Try
    End Sub
    Sub q96(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q95(sender, e)
        End If
    End Sub
    Sub q97(ByVal sender As Object, ByVal e As EventArgs)
        Try
            For q As Integer = 0 To f6.l0.Items.Count - 1
                If f6.l0.Items.Item(q).ToString = "" Then
                Else
                    q3(f6.l0.Items.Item(q).ToString)
                End If
            Next q
        Catch ex As Exception
        End Try
    End Sub
    Sub q98(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q97(sender, e)
        End If
    End Sub
    Sub q99(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If MessageBox.Show("Clear all bookmarks?", "Clear Bookmarks", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
            My.Settings.bm.Clear()
            RB.DropDownItems.Clear()
            My.Settings.Save()
            su.Visible = True
            su.Text = "Bookmarks cleared"
        Catch ex As Exception
        End Try
    End Sub
    Sub q100(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q99(sender, e)
        End If
    End Sub
    Sub q101(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaNew(qleaHome())
        Catch ex As Exception
        End Try
    End Sub
    Sub q102(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q101(sender, e)
        End If
    End Sub
    Sub q103(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Clipboard.SetText(My.Settings.h)
            su.Visible = True
            su.Text = "Homepage link copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q104(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q103(sender, e)
        End If
    End Sub
    Sub q105(ByVal sender As Object, ByVal e As EventArgs)
        Try
            My.Settings.h = "http://fsin.glitch.me/"
            My.Settings.Save()
            su.Visible = True
            su.Text = "Homepage reset"
        Catch ex As Exception
        End Try
    End Sub
    Sub q106(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q105(sender, e)
        End If
    End Sub
    Sub q107(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Window Snapshot.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Window Snapshot"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider Window Snapshot")
            mySW.WriteLine("Selected Tab: " & k.SelectedIndex.ToString)
            mySW.WriteLine("Homepage: " & My.Settings.h)
            mySW.WriteLine("Restore Session: " & My.Settings.rs)
            mySW.WriteLine("Private Mode: " & pv.ToString)
            mySW.WriteLine("")
            For q As Integer = 0 To k.TabPages.Count - 1
                mySW.WriteLine("Tab " & q.ToString)
                mySW.WriteLine("Title: " & k.TabPages(q).Text)
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then mySW.WriteLine("URL: " & wb.Url.ToString)
                End If
                mySW.WriteLine("")
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Window snapshot saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q108(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q107(sender, e)
        End If
    End Sub
    Sub q109(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If MessageBox.Show("Clear the saved session and visible history?", "Clear Session And History", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
            My.Settings.ls.Clear()
            f6.l0.Items.Clear()
            c.Items.Clear()
            My.Settings.Save()
            su.Visible = True
            su.Text = "Session and history cleared"
        Catch ex As Exception
        End Try
    End Sub
    Sub q110(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q109(sender, e)
        End If
    End Sub
    Sub q111(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim address = qleaDomain()
            If address Is Nothing Then Return
            Clipboard.SetText(address.Host)
            su.Visible = True
            su.Text = "Current domain copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q112(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q111(sender, e)
        End If
    End Sub
    Sub q113(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.SelectedIndex > 0 Then
                Dim tp As TabPage = k.SelectedTab
                k.TabPages.Remove(tp)
                k.TabPages.Insert(0, tp)
                k.SelectedTab = tp
                su.Visible = True
                su.Text = "Tab pinned first"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q114(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q113(sender, e)
        End If
    End Sub
    Sub q115(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Process.Start(My.Settings.h)
        Catch ex As Exception
        End Try
    End Sub
    Sub q116(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q115(sender, e)
        End If
    End Sub
    Sub q117(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = "Firespider Session Summary" & vbCrLf
            tx &= "Selected Tab: " & k.SelectedIndex.ToString & vbCrLf
            tx &= "Homepage: " & My.Settings.h & vbCrLf
            tx &= "Restore Session: " & My.Settings.rs & vbCrLf
            tx &= "Private Mode: " & pv.ToString & vbCrLf & vbCrLf
            For q As Integer = 0 To k.TabPages.Count - 1
                tx &= "Tab " & q.ToString & ": " & k.TabPages(q).Text & vbCrLf
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then tx &= wb.Url.ToString & vbCrLf
                End If
                tx &= vbCrLf
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "Session summary copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q118(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q117(sender, e)
        End If
    End Sub
    Sub q119(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As New ArrayList
            Dim ct As New ArrayList
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        Dim u As String = wb.Url.ToString
                        If ar.Contains(u) Then
                            ct(ar.IndexOf(u)) = CInt(ct(ar.IndexOf(u))) + 1
                        Else
                            ar.Add(u)
                            ct.Add(1)
                        End If
                    End If
                End If
            Next q
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        If CInt(ct(ar.IndexOf(wb.Url.ToString))) > 1 Then
                            wb.Refresh()
                        End If
                    End If
                End If
            Next q
            su.Visible = True
            su.Text = "Duplicate tabs reloaded"
        Catch ex As Exception
        End Try
    End Sub
    Sub q120(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q119(sender, e)
        End If
    End Sub
    Sub q121(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = ""
            For q As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(q) = "" Then
                Else
                    tx &= My.Settings.bm.Item(q) & vbCrLf
                End If
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "Bookmark links copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q122(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q121(sender, e)
        End If
    End Sub
    Sub q123(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim address = qleaDomain()
            If address IsNot Nothing Then q1(address.AbsoluteUri)
        Catch ex As Exception
        End Try
    End Sub
    Sub q124(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q123(sender, e)
        End If
    End Sub
    Sub q125(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Bookmark Snapshot.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Bookmark Snapshot"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider Bookmark Snapshot")
            mySW.WriteLine("Bookmark Count: " & My.Settings.bm.Count.ToString)
            mySW.WriteLine("")
            For q As Integer = 0 To My.Settings.bm.Count - 1
                mySW.WriteLine((q + 1).ToString & ". " & My.Settings.bm.Item(q))
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Bookmark snapshot saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q126(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q125(sender, e)
        End If
    End Sub
    Sub q127(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = ""
            For q As Integer = 0 To f6.l0.Items.Count - 1
                tx &= f6.l0.Items.Item(q).ToString & vbCrLf
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "History links copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q128(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q127(sender, e)
        End If
    End Sub
    Sub q129(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim address = qleaDomain()
            If address Is Nothing Then Return
            My.Settings.h = address.AbsoluteUri
            My.Settings.Save()
            su.Visible = True
            su.Text = "Homepage set to current domain"
        Catch ex As Exception
        End Try
    End Sub
    Sub q130(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q129(sender, e)
        End If
    End Sub
    Sub q131(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim u As Uri = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url
            If u.IsFile Then
                Process.Start(System.IO.Path.GetDirectoryName(u.LocalPath))
                su.Visible = True
                su.Text = "Current file folder opened"
            Else
                su.Visible = True
                su.Text = "Current page is not a local file"
            End If
        Catch ex As Exception
            su.Visible = True
            su.Text = "Current page is not a local file"
        End Try
    End Sub
    Sub q132(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q131(sender, e)
        End If
    End Sub
    Sub q133(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.SelectedItem Is Nothing Then Exit Sub
            Dim u As String = f6.l0.SelectedItem.ToString
            My.Settings.bm.Add(u)
            Dim fv As New ToolStripMenuItem
            fv.Text = u
            fv.Tag = u
            fv.Image = My.Resources.F9
            fv.BackgroundImage = My.Resources.F2
            fv.BackgroundImageLayout = ImageLayout.Stretch
            fv.ForeColor = Color.White
            AddHandler fv.Click, AddressOf z2
            AddHandler fv.MouseHover, AddressOf z2
            RB.DropDownItems.Add(fv)
            My.Settings.Save()
            su.Visible = True
            su.Text = "History item bookmarked"
        Catch ex As Exception
        End Try
    End Sub
    Sub q134(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q133(sender, e)
        End If
    End Sub
    Sub q135(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider History Snapshot.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save History Snapshot"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider History Snapshot")
            mySW.WriteLine("History Count: " & f6.l0.Items.Count.ToString)
            mySW.WriteLine("")
            For q As Integer = 0 To f6.l0.Items.Count - 1
                mySW.WriteLine((q + 1).ToString & ". " & f6.l0.Items.Item(q).ToString)
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "History snapshot saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q136(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q135(sender, e)
        End If
    End Sub
    Sub q137(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If MessageBox.Show("Clear all saved browser data?", "Clear All Browser Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
            My.Settings.ls.Clear()
            My.Settings.bm.Clear()
            My.Settings.s.Clear()
            My.Settings.ns.Clear()
            My.Settings.dl.Clear()
            My.Settings.lcw.Clear()
            My.Settings.rc.Clear()
            f6.l0.Items.Clear()
            c.Items.Clear()
            x.Items.Clear()
            RB.DropDownItems.Clear()
            My.Settings.Save()
            su.Visible = True
            su.Text = "All browser data cleared"
        Catch ex As Exception
        End Try
    End Sub
    Sub q138(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q137(sender, e)
        End If
    End Sub
    Sub q139(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim address = qleaDomain()
            If address IsNot Nothing Then qleaNew(address.AbsoluteUri)
        Catch ex As Exception
        End Try
    End Sub
    Sub q140(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q139(sender, e)
        End If
    End Sub
    Sub q141(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Browser Data Snapshot.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save All Browser Data Snapshot"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider Browser Data Snapshot")
            mySW.WriteLine("Homepage: " & My.Settings.h)
            mySW.WriteLine("Restore Session: " & My.Settings.rs)
            mySW.WriteLine("Private Mode: " & pv.ToString)
            mySW.WriteLine("Open Tabs: " & k.TabPages.Count.ToString)
            mySW.WriteLine("Bookmarks: " & My.Settings.bm.Count.ToString)
            mySW.WriteLine("History: " & f6.l0.Items.Count.ToString)
            mySW.WriteLine("")
            mySW.WriteLine("Open Tabs")
            For q As Integer = 0 To k.TabPages.Count - 1
                mySW.WriteLine((q + 1).ToString & ". " & k.TabPages(q).Text)
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then mySW.WriteLine("   " & wb.Url.ToString)
                End If
            Next q
            mySW.WriteLine("")
            mySW.WriteLine("Bookmarks")
            For q As Integer = 0 To My.Settings.bm.Count - 1
                mySW.WriteLine((q + 1).ToString & ". " & My.Settings.bm.Item(q))
            Next q
            mySW.WriteLine("")
            mySW.WriteLine("History")
            For q As Integer = 0 To f6.l0.Items.Count - 1
                mySW.WriteLine((q + 1).ToString & ". " & f6.l0.Items.Item(q).ToString)
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Browser data snapshot saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q142(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q141(sender, e)
        End If
    End Sub
    Sub q143(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim address As Uri = qleaDomain()
            If address Is Nothing Then Return
            My.Settings.bm.Add(address.AbsoluteUri)
            My.Settings.Save()
            qrb()
            su.Visible = True
            su.Text = "Current domain bookmarked"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q144(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q143(sender, e)
        End If
    End Sub
    Sub q145(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaDomainAction("rl")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q146(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q145(sender, e)
        End If
    End Sub
    Sub q147(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim text As String = qleaDomainText(False)
            If text = "" Then Return
            Clipboard.SetText(text)
            su.Visible = True
            su.Text = "Same-domain tabs copied"
        Catch ex As Exception
            qlog(ex)
            su.Visible = True
            su.Text = "Same-domain tabs could not be copied"
        End Try
    End Sub
    Sub q148(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q147(sender, e)
        End If
    End Sub
    Sub q149(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaDomainAction("cl")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q150(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q149(sender, e)
        End If
    End Sub
    Sub q151(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim text As String = qleaDomainText(True)
            If text = "" Then Return
            Using dialog As New SaveFileDialog()
                qsavefolder(dialog)
                dialog.FileName = "Firespider Domain Snapshot.txt"
                dialog.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
                dialog.CheckPathExists = True
                dialog.Title = "Save Current Domain Snapshot"
                If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                System.IO.File.WriteAllText(dialog.FileName, text, System.Text.Encoding.UTF8)
            End Using
            su.Visible = True
            su.Text = "Domain snapshot saved"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q152(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q151(sender, e)
        End If
    End Sub
    Sub q153(ByVal sender As Object, ByVal e As EventArgs)
        Try
            q3(CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
        Catch ex As Exception
        End Try
    End Sub
    Sub q154(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q153(sender, e)
        End If
    End Sub
    Sub q155(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Search Suggestions Snapshot.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Search Suggestions Snapshot"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider Search Suggestions Snapshot")
            mySW.WriteLine("Search Engine: " & My.Settings.se)
            mySW.WriteLine("")
            mySW.WriteLine("Address Suggestions")
            For q As Integer = 0 To c.Items.Count - 1
                mySW.WriteLine((q + 1).ToString & ". " & c.Items.Item(q).ToString)
            Next q
            mySW.WriteLine("")
            mySW.WriteLine("Search Suggestions")
            For q As Integer = 0 To x.Items.Count - 1
                mySW.WriteLine((q + 1).ToString & ". " & x.Items.Item(q).ToString)
            Next q
            mySW.WriteLine("")
            mySW.WriteLine("Saved Search Suggestions")
            For q As Integer = 0 To My.Settings.s.Count - 1
                mySW.WriteLine((q + 1).ToString & ". " & My.Settings.s.Item(q))
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Search suggestions snapshot saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q156(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q155(sender, e)
        End If
    End Sub
    Sub q157(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = "Search Engine: " & My.Settings.se & vbCrLf & vbCrLf
            tx &= "Address Suggestions" & vbCrLf
            For q As Integer = 0 To c.Items.Count - 1
                tx &= c.Items.Item(q).ToString & vbCrLf
            Next q
            tx &= vbCrLf & "Search Suggestions" & vbCrLf
            For q As Integer = 0 To x.Items.Count - 1
                tx &= x.Items.Item(q).ToString & vbCrLf
            Next q
            tx &= vbCrLf & "Saved Search Suggestions" & vbCrLf
            For q As Integer = 0 To My.Settings.s.Count - 1
                tx &= My.Settings.s.Item(q) & vbCrLf
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "Search suggestions copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q158(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q157(sender, e)
        End If
    End Sub
    Sub q159(ByVal sender As Object, ByVal e As EventArgs)
        Try
            My.Settings.se = "www.bing.com/search?q="
            My.Settings.Save()
            su.Visible = True
            su.Text = "Default search engine restored"
        Catch ex As Exception
        End Try
    End Sub
    Sub q160(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q159(sender, e)
        End If
    End Sub
    Sub rca(ByVal u As String)
        Try
            If pv Then Return
            u = qleaSessionTarget(u)
            If u = "" Then
            Else
                lu = u
                If My.Settings.rc.Contains(u) Then
                    My.Settings.rc.Remove(u)
                End If
                My.Settings.rc.Insert(0, u)
                While My.Settings.rc.Count > 25
                    My.Settings.rc.RemoveAt(My.Settings.rc.Count - 1)
                End While
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q161(ByVal sender As Object, ByVal e As EventArgs) Handles RC.DropDownOpening
        Try
            RC.DropDownItems.Clear()
            For q As Integer = 0 To My.Settings.rc.Count - 1
                Dim ti As New ToolStripMenuItem
                ti.Text = My.Settings.rc.Item(q)
                ti.Tag = My.Settings.rc.Item(q)
                ti.Image = My.Resources.F9
                ti.BackColor = Color.White
                ti.ForeColor = Color.Black
                ti.Padding = New Padding(0)
                AddHandler ti.Click, AddressOf q163
                AddHandler ti.MouseHover, AddressOf q163
                RC.DropDownItems.Add(ti)
            Next q
        Catch ex As Exception
        End Try
    End Sub
    Sub q162(ByVal sender As Object, ByVal e As EventArgs) Handles RC.Click
        q161(sender, e)
        If RC.DropDownItems.Count > 0 Then RC.ShowDropDown()
    End Sub
    Sub q163(ByVal sender As ToolStripMenuItem, ByVal ByVale As EventArgs)
        Dim target = qleaSessionTarget(TryCast(sender.Tag, String))
        If target <> "" Then qleaNew(target)
    End Sub
    Sub q164(ByVal sender As Object, ByVal e As EventArgs) Handles RC.MouseHover
        If My.Settings.t = "e" Then
            q162(sender, e)
        End If
    End Sub
    Sub q165(ByVal sender As Object, ByVal e As EventArgs) Handles RP.Click
        Try
            qleaPreviousSession()
        Catch ex As Exception
        End Try
    End Sub
    Sub q166(ByVal sender As Object, ByVal e As EventArgs) Handles RP.MouseHover
        If My.Settings.t = "e" Then
            q165(sender, e)
        End If
    End Sub
    Function qh(ByVal tx As String) As String
        Try
            tx = tx.Replace("&", "&amp;")
            tx = tx.Replace("<", "&lt;")
            tx = tx.Replace(">", "&gt;")
            tx = tx.Replace("""", "&quot;")
            Return tx
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Sub q167(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim tt As String = wb.DocumentTitle
            If tt = "" Then tt = wb.Url.ToString
            tt = tt.Replace("[", "\[")
            tt = tt.Replace("]", "\]")
            Clipboard.SetText("[" & tt & "](" & wb.Url.ToString & ")")
            su.Visible = True
            su.Text = "Markdown link copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q168(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q167(sender, e)
        End If
    End Sub
    Sub q169(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Tabs.html"
            SX.Filter = "HTML File [*.html*]|*.html|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Tab Links As HTML"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("<!doctype html>")
            mySW.WriteLine("<html>")
            mySW.WriteLine("<head><meta charset=""utf-8""><title>Firespider Tabs</title></head>")
            mySW.WriteLine("<body>")
            mySW.WriteLine("<h1>Firespider Tabs</h1>")
            mySW.WriteLine("<ol>")
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        Dim tt As String = k.TabPages(q).Text
                        If tt = "" Then tt = wb.Url.ToString
                        mySW.WriteLine("<li><a href=""" & qh(wb.Url.ToString) & """>" & qh(tt) & "</a></li>")
                    End If
                End If
            Next q
            mySW.WriteLine("</ol>")
            mySW.WriteLine("</body>")
            mySW.WriteLine("</html>")
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Tab links HTML saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q170(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q169(sender, e)
        End If
    End Sub
    Sub q171(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaDomainAction("bm")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q172(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q171(sender, e)
        End If
    End Sub
    Sub q173(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim u As Uri = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url
            Process.Start(u.Scheme & "://" & u.Host & "/")
            su.Visible = True
            su.Text = "Domain opened in default browser"
        Catch ex As Exception
        End Try
    End Sub
    Sub q174(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q173(sender, e)
        End If
    End Sub
    Sub q175(ByVal sender As Object, ByVal e As EventArgs)
        Try
            For q As Integer = k.TabPages.Count - 1 To 0 Step -1
                If k.TabPages.Count = 1 Then Exit For
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wb.Url Is Nothing Or k.TabPages(q).Text = "" Or k.TabPages(q).Text = "New Tab" Or k.TabPages(q).Text = "New Page" Then
                        k.TabPages.RemoveAt(q)
                        I = I - 1
                        Dim tz As Size = k.ItemSize
                        tz.Width += 6
                        k.ItemSize = tz
                    End If
                End If
            Next q
            k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Text = ProductName & " - " & k.SelectedTab.Text
            c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
            su.Visible = True
            su.Text = "Blank tabs closed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q176(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q175(sender, e)
        End If
    End Sub
    Function ql() As ArrayList
        Dim ar As New ArrayList
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            For Each he As HtmlElement In wb.Document.Links
                Dim u As String = he.GetAttribute("href")
                If u = "" Then
                ElseIf ar.Contains(u) Then
                Else
                    ar.Add(u)
                End If
            Next
        Catch ex As Exception
        End Try
        Return ar
    End Function
    Sub q177(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = ql()
            Dim tx As String = ""
            For q As Integer = 0 To ar.Count - 1
                tx &= ar.Item(q).ToString & vbCrLf
            Next q
            If tx = "" Then
                su.Visible = True
                su.Text = "No page links found"
            Else
                Clipboard.SetText(tx)
                su.Visible = True
                su.Text = "Page links copied"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q178(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q177(sender, e)
        End If
    End Sub
    Sub q179(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = ql()
            If ar.Count = 0 Then
                su.Visible = True
                su.Text = "No page links found"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Page Links.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save All Links On Current Page"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider Page Links")
            mySW.WriteLine("Page: " & CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
            mySW.WriteLine("Links: " & ar.Count.ToString)
            mySW.WriteLine("")
            For q As Integer = 0 To ar.Count - 1
                mySW.WriteLine(ar.Item(q).ToString)
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Page links saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q180(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q179(sender, e)
        End If
    End Sub
    Sub q181(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = ql()
            For q As Integer = 0 To ar.Count - 1
                Dim u As String = ar.Item(q).ToString
                My.Settings.bm.Add(u)
                Dim fv As New ToolStripMenuItem
                fv.Text = u
                fv.Tag = u
                fv.Image = My.Resources.F9
                fv.BackgroundImage = My.Resources.F2
                fv.BackgroundImageLayout = ImageLayout.Stretch
                fv.ForeColor = Color.White
                AddHandler fv.Click, AddressOf z2
                AddHandler fv.MouseHover, AddressOf z2
                RB.DropDownItems.Add(fv)
            Next q
            My.Settings.Save()
            su.Visible = True
            su.Text = "Page links bookmarked"
        Catch ex As Exception
        End Try
    End Sub
    Sub q182(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q181(sender, e)
        End If
    End Sub
    Sub q183(ByVal sender As Object, ByVal e As EventArgs)
        Try
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Window.ScrollTo(0, 0)
            su.Visible = True
            su.Text = "Scrolled to top"
        Catch ex As Exception
        End Try
    End Sub
    Sub q184(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q183(sender, e)
        End If
    End Sub
    Sub q185(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim y As Integer = wb.Document.Body.ScrollRectangle.Height
            wb.Document.Window.ScrollTo(0, y)
            su.Visible = True
            su.Text = "Scrolled to bottom"
        Catch ex As Exception
        End Try
    End Sub
    Sub q186(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q185(sender, e)
        End If
    End Sub
    Function qi() As ArrayList
        Dim ar As New ArrayList
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            For Each he As HtmlElement In wb.Document.Images
                Dim u As String = he.GetAttribute("src")
                If u = "" Then
                ElseIf ar.Contains(u) Then
                Else
                    ar.Add(u)
                End If
            Next
        Catch ex As Exception
        End Try
        Return ar
    End Function
    Sub q187(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = qi()
            Dim tx As String = ""
            For q As Integer = 0 To ar.Count - 1
                tx &= ar.Item(q).ToString & vbCrLf
            Next q
            If tx = "" Then
                su.Visible = True
                su.Text = "No page images found"
            Else
                Clipboard.SetText(tx)
                su.Visible = True
                su.Text = "Page image links copied"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q188(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q187(sender, e)
        End If
    End Sub
    Sub q189(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = qi()
            If ar.Count = 0 Then
                su.Visible = True
                su.Text = "No page images found"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Page Images.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page Image Links"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider Page Image Links")
            mySW.WriteLine("Page: " & CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
            mySW.WriteLine("Images: " & ar.Count.ToString)
            mySW.WriteLine("")
            For q As Integer = 0 To ar.Count - 1
                mySW.WriteLine(ar.Item(q).ToString)
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Page image links saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q190(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q189(sender, e)
        End If
    End Sub
    Sub q191(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Clipboard.SetText(CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentText)
            su.Visible = True
            su.Text = "Page HTML copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q192(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q191(sender, e)
        End If
    End Sub
    Sub q193(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim links As ArrayList = ql()
            Dim imgs As ArrayList = qi()
            Dim tx As String = ""
            tx &= "Title: " & wb.DocumentTitle & vbCrLf
            tx &= "Address: " & wb.Url.ToString & vbCrLf
            tx &= "Domain: " & wb.Url.Host & vbCrLf
            tx &= "Status: " & wb.StatusText & vbCrLf
            tx &= "Links: " & links.Count.ToString & vbCrLf
            tx &= "Images: " & imgs.Count.ToString & vbCrLf
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "Page metadata copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q194(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q193(sender, e)
        End If
    End Sub
    Sub q195(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim u As Uri = wb.Url
            Dim ar As ArrayList = ql()
            Dim opened As Integer = 0
            For q As Integer = 0 To ar.Count - 1
                Dim lu As New Uri(ar.Item(q).ToString)
                If lu.Host = u.Host Then
                    q3(lu.ToString)
                    opened += 1
                End If
            Next q
            su.Visible = True
            su.Text = opened.ToString & " same-domain links opened"
        Catch ex As Exception
        End Try
    End Sub
    Sub q196(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q195(sender, e)
        End If
    End Sub
    Function qs() As ArrayList
        Dim ar As New ArrayList
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim u As Uri = wb.Url
            Dim al As ArrayList = ql()
            For q As Integer = 0 To al.Count - 1
                Dim lu As New Uri(al.Item(q).ToString)
                If lu.Host = u.Host Then ar.Add(lu.ToString)
            Next q
        Catch ex As Exception
        End Try
        Return ar
    End Function
    Function qm() As String
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim links As ArrayList = ql()
            Dim imgs As ArrayList = qi()
            Dim tx As String = ""
            tx &= "Title: " & wb.DocumentTitle & vbCrLf
            tx &= "Address: " & wb.Url.ToString & vbCrLf
            tx &= "Domain: " & wb.Url.Host & vbCrLf
            tx &= "Status: " & wb.StatusText & vbCrLf
            tx &= "Links: " & links.Count.ToString & vbCrLf
            tx &= "Same-Domain Links: " & qs().Count.ToString & vbCrLf
            tx &= "Images: " & imgs.Count.ToString & vbCrLf
            tx &= "Can Go Back: " & wb.CanGoBack.ToString & vbCrLf
            tx &= "Can Go Forward: " & wb.CanGoForward.ToString & vbCrLf
            Return tx
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Sub q197(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = qs()
            Dim tx As String = ""
            For q As Integer = 0 To ar.Count - 1
                tx &= ar.Item(q).ToString & vbCrLf
            Next q
            If tx = "" Then
                su.Visible = True
                su.Text = "No same-domain links found"
            Else
                Clipboard.SetText(tx)
                su.Visible = True
                su.Text = "Same-domain links copied"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q198(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q197(sender, e)
        End If
    End Sub
    Sub q199(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = qs()
            If ar.Count = 0 Then
                su.Visible = True
                su.Text = "No same-domain links found"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Same Domain Links.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Same-Domain Page Links"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider Same-Domain Page Links")
            mySW.WriteLine("Page: " & CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
            mySW.WriteLine("Links: " & ar.Count.ToString)
            mySW.WriteLine("")
            For q As Integer = 0 To ar.Count - 1
                mySW.WriteLine(ar.Item(q).ToString)
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Same-domain links saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q200(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q199(sender, e)
        End If
    End Sub
    Sub q201(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = qi()
            For q As Integer = 0 To ar.Count - 1
                q3(ar.Item(q).ToString)
            Next q
            su.Visible = True
            su.Text = ar.Count.ToString & " image links opened"
        Catch ex As Exception
        End Try
    End Sub
    Sub q202(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q201(sender, e)
        End If
    End Sub
    Sub q203(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = ""
            tx &= "Open Tabs: " & k.TabPages.Count.ToString & vbCrLf
            tx &= "Selected Tab: " & k.SelectedIndex.ToString & vbCrLf
            tx &= "" & vbCrLf
            For q As Integer = 0 To k.TabPages.Count - 1
                tx &= (q + 1).ToString & ". " & k.TabPages(q).Text & vbCrLf
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then tx &= "   " & wb.Url.ToString & vbCrLf
                End If
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "Tab summary copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q204(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q203(sender, e)
        End If
    End Sub
    Sub q205(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Page Metadata.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page Metadata"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.Write(qm())
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Page metadata saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q206(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q205(sender, e)
        End If
    End Sub
    Sub q207(ByVal sender As Object, ByVal e As EventArgs)
        Try
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then Process.Start(wb.Url.ToString)
                End If
            Next q
            su.Visible = True
            su.Text = "All tabs opened in default browser"
        Catch ex As Exception
        End Try
    End Sub
    Sub q208(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q207(sender, e)
        End If
    End Sub
    Function qc(ByVal tx As String) As String
        Try
            tx = tx.Replace("""", """""")
            Return """" & tx & """"
        Catch ex As Exception
            Return """"""
        End Try
    End Function
    Function qp() As String
        Try
            Dim tx As String = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.InnerText
            If tx.Length > 1000 Then tx = tx.Substring(0, 1000)
            Return tx
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Sub q209(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Tabs.csv"
            SX.Filter = "CSV File [*.csv*]|*.csv|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save All Tabs As CSV"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Number,Title,Address,Domain")
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        mySW.WriteLine(qc((q + 1).ToString) & "," & qc(k.TabPages(q).Text) & "," & qc(wb.Url.ToString) & "," & qc(wb.Url.Host))
                    End If
                End If
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Tabs CSV saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q210(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q209(sender, e)
        End If
    End Sub
    Sub q211(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = ""
            tx &= "Bookmarks: " & My.Settings.bm.Count.ToString & vbCrLf
            tx &= "" & vbCrLf
            For q As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(q) = "" Then
                Else
                    tx &= (q + 1).ToString & ". " & My.Settings.bm.Item(q) & vbCrLf
                End If
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "Bookmark summary copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q212(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q211(sender, e)
        End If
    End Sub
    Sub q213(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As New ArrayList
            For q As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(q) = "" Then
                Else
                    ar.Add(My.Settings.bm.Item(q))
                End If
            Next q
            If ar.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks"
            Else
                Dim rn As New Random
                q3(ar.Item(rn.Next(0, ar.Count)).ToString)
                su.Visible = True
                su.Text = "Random bookmark opened"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q214(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q213(sender, e)
        End If
    End Sub
    Sub q215(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = qp()
            If tx = "" Then
                su.Visible = True
                su.Text = "No page text"
            Else
                Clipboard.SetText(tx)
                su.Visible = True
                su.Text = "Page text preview copied"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q216(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q215(sender, e)
        End If
    End Sub
    Sub q217(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = qp()
            If tx = "" Then
                su.Visible = True
                su.Text = "No page text"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle & " Preview.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page Text Preview"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.Write(tx)
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Page text preview saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q218(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q217(sender, e)
        End If
    End Sub
    Sub q219(ByVal sender As Object, ByVal e As EventArgs)
        Try
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Refresh(WebBrowserRefreshOption.Completely)
            su.Visible = True
            su.Text = "Current tab hard refreshed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q220(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q219(sender, e)
        End If
    End Sub
    Function qmk(ByVal tx As String) As String
        Try
            tx = tx.Replace("[", "\[")
            tx = tx.Replace("]", "\]")
            Return tx
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Sub q221(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = ""
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        Dim tt As String = k.TabPages(q).Text
                        If tt = "" Then tt = wb.Url.ToString
                        tx &= "- [" & qmk(tt) & "](" & wb.Url.ToString & ")" & vbCrLf
                    End If
                End If
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "Tabs copied as Markdown"
        Catch ex As Exception
        End Try
    End Sub
    Sub q222(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q221(sender, e)
        End If
    End Sub
    Sub q223(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Bookmarks.html"
            SX.Filter = "HTML File [*.html*]|*.html|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Bookmarks As HTML"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("<!doctype html>")
            mySW.WriteLine("<html>")
            mySW.WriteLine("<head><meta charset=""utf-8""><title>Firespider Bookmarks</title></head>")
            mySW.WriteLine("<body>")
            mySW.WriteLine("<h1>Firespider Bookmarks</h1>")
            mySW.WriteLine("<ol>")
            For q As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(q) = "" Then
                Else
                    mySW.WriteLine("<li><a href=""" & qh(My.Settings.bm.Item(q)) & """>" & qh(My.Settings.bm.Item(q)) & "</a></li>")
                End If
            Next q
            mySW.WriteLine("</ol>")
            mySW.WriteLine("</body>")
            mySW.WriteLine("</html>")
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Bookmarks HTML saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q224(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q223(sender, e)
        End If
    End Sub
    Sub q225(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = ""
            tx &= "History Items: " & f6.l0.Items.Count.ToString & vbCrLf
            tx &= "" & vbCrLf
            For q As Integer = 0 To f6.l0.Items.Count - 1
                tx &= (q + 1).ToString & ". " & f6.l0.Items.Item(q).ToString & vbCrLf
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "History summary copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q226(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q225(sender, e)
        End If
    End Sub
    Sub q227(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim tx As String = InputBox("Search " & wb.Url.Host, "Search Current Domain")
            If tx = "" Then Exit Sub
            q3(My.Settings.se & Uri.EscapeDataString("site:" & wb.Url.Host & " " & tx))
            su.Visible = True
            su.Text = "Domain search opened"
        Catch ex As Exception
        End Try
    End Sub
    Sub q228(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q227(sender, e)
        End If
    End Sub
    Sub q229(ByVal sender As Object, ByVal e As EventArgs)
        Try
            q3("view-source:" & CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
            su.Visible = True
            su.Text = "Page source opened"
        Catch ex As Exception
        End Try
    End Sub
    Sub q230(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q229(sender, e)
        End If
    End Sub
    Sub q231(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As New ArrayList
            For q As Integer = 0 To f6.l0.Items.Count - 1
                Dim u As String = f6.l0.Items.Item(q).ToString
                If u = "" Then
                ElseIf ar.Contains(u) Then
                Else
                    ar.Add(u)
                End If
            Next q
            f6.l0.Items.Clear()
            For q As Integer = 0 To ar.Count - 1
                f6.l0.Items.Add(ar.Item(q).ToString)
            Next q
            su.Visible = True
            su.Text = "Duplicate history items removed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q232(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q231(sender, e)
        End If
    End Sub
    Sub q233(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Tabs.md"
            SX.Filter = "Markdown File [*.md*]|*.md|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save All Tabs As Markdown"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("# Firespider Tabs")
            mySW.WriteLine("")
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        Dim tt As String = k.TabPages(q).Text
                        If tt = "" Then tt = wb.Url.ToString
                        mySW.WriteLine("- [" & qmk(tt) & "](" & wb.Url.ToString & ")")
                    End If
                End If
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Tabs Markdown saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q234(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q233(sender, e)
        End If
    End Sub
    Sub q235(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider History.html"
            SX.Filter = "HTML File [*.html*]|*.html|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save History As HTML"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("<!doctype html>")
            mySW.WriteLine("<html>")
            mySW.WriteLine("<head><meta charset=""utf-8""><title>Firespider History</title></head>")
            mySW.WriteLine("<body>")
            mySW.WriteLine("<h1>Firespider History</h1>")
            mySW.WriteLine("<ol>")
            For q As Integer = 0 To f6.l0.Items.Count - 1
                Dim u As String = f6.l0.Items.Item(q).ToString
                If u = "" Then
                Else
                    mySW.WriteLine("<li><a href=""" & qh(u) & """>" & qh(u) & "</a></li>")
                End If
            Next q
            mySW.WriteLine("</ol>")
            mySW.WriteLine("</body>")
            mySW.WriteLine("</html>")
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "History HTML saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q236(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q235(sender, e)
        End If
    End Sub
    Sub q237(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = ql()
            Dim tx As String = "<ol>" & vbCrLf
            For q As Integer = 0 To ar.Count - 1
                tx &= "<li><a href=""" & qh(ar.Item(q).ToString) & """>" & qh(ar.Item(q).ToString) & "</a></li>" & vbCrLf
            Next q
            tx &= "</ol>"
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "Page links HTML copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q238(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q237(sender, e)
        End If
    End Sub
    Sub q239(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaPaste(Clipboard.GetText(), False)
        Catch ex As Exception
        End Try
    End Sub
    Sub q240(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q239(sender, e)
        End If
    End Sub
    Sub q241(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = InputBox("Find tab title or address", "Search Open Tabs")
            If tx = "" Then Exit Sub
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Text.ToLower.Contains(tx.ToLower) Then
                    k.SelectedTab = k.TabPages(q)
                    su.Visible = True
                    su.Text = "Matching tab selected"
                    Exit Sub
                ElseIf k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        If wb.Url.ToString.ToLower.Contains(tx.ToLower) Then
                            k.SelectedTab = k.TabPages(q)
                            su.Visible = True
                            su.Text = "Matching tab selected"
                            Exit Sub
                        End If
                    End If
                End If
            Next q
            su.Visible = True
            su.Text = "No matching tab"
        Catch ex As Exception
        End Try
    End Sub
    Sub q242(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q241(sender, e)
        End If
    End Sub
    Sub q243(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As New ArrayList
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        If ar.Contains(wb.Url.Host) Then
                        Else
                            ar.Add(wb.Url.Host)
                        End If
                    End If
                End If
            Next q
            Dim tx As String = ""
            For q As Integer = 0 To ar.Count - 1
                tx &= ar.Item(q).ToString & vbCrLf
            Next q
            Clipboard.SetText(tx)
            su.Visible = True
            su.Text = "Open tab domains copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q244(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q243(sender, e)
        End If
    End Sub
    Sub q245(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = ql()
            Dim tx As String = ""
            For q As Integer = 0 To ar.Count - 1
                tx &= "- [" & qmk(ar.Item(q).ToString) & "](" & ar.Item(q).ToString & ")" & vbCrLf
            Next q
            If tx = "" Then
                su.Visible = True
                su.Text = "No page links found"
            Else
                Clipboard.SetText(tx)
                su.Visible = True
                su.Text = "Page links copied as Markdown"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q246(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q245(sender, e)
        End If
    End Sub
    Sub q247(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = qi()
            Dim tx As String = ""
            For q As Integer = 0 To ar.Count - 1
                tx &= "![](" & ar.Item(q).ToString & ")" & vbCrLf
            Next q
            If tx = "" Then
                su.Visible = True
                su.Text = "No page images found"
            Else
                Clipboard.SetText(tx)
                su.Visible = True
                su.Text = "Page images copied as Markdown"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q248(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q247(sender, e)
        End If
    End Sub
    Sub q249(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Tab Domains.csv"
            SX.Filter = "CSV File [*.csv*]|*.csv|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Open Tab Domains As CSV"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim ar As New ArrayList
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Domain,Title,Address")
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        If ar.Contains(wb.Url.Host) Then
                        Else
                            ar.Add(wb.Url.Host)
                            mySW.WriteLine(qc(wb.Url.Host) & "," & qc(k.TabPages(q).Text) & "," & qc(wb.Url.ToString))
                        End If
                    End If
                End If
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Open tab domains CSV saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q250(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q249(sender, e)
        End If
    End Sub
    Sub q251(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim f As New j
            f.Show()
            If My.Settings.ls.Count = 0 Then
                f.q3(My.Settings.h)
            Else
                CType(f.k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(My.Settings.ls.Item(0))
                For q As Integer = 1 To My.Settings.ls.Count - 1
                    f.q3(My.Settings.ls.Item(q))
                Next q
            End If
            su.Visible = True
            su.Text = "Saved session opened in new window"
        Catch ex As Exception
        End Try
    End Sub
    Sub q252(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q251(sender, e)
        End If
    End Sub
    Sub q253(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = InputBox("Close tabs with title or address containing", "Close Tabs Matching Text")
            If tx = "" Then Exit Sub
            Dim closed As Integer = 0
            For q As Integer = k.TabPages.Count - 1 To 0 Step -1
                Dim ok As Boolean = k.TabPages(q).Text.ToLower.Contains(tx.ToLower)
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        If wb.Url.ToString.ToLower.Contains(tx.ToLower) Then ok = True
                    End If
                End If
                If ok = True And k.TabPages.Count > 1 Then
                    k.TabPages.RemoveAt(q)
                    I = I - 1
                    closed += 1
                End If
            Next q
            su.Visible = True
            su.Text = closed.ToString & " matching tabs closed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q254(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q253(sender, e)
        End If
    End Sub
    Sub q255(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim u As String = Clipboard.GetText.Trim
            If u = "" Then
                su.Visible = True
                su.Text = "Clipboard is empty"
                Exit Sub
            End If
            If qvl(u) = False Then
                su.Visible = True
                su.Text = "Clipboard is not a link"
                Exit Sub
            End If
            My.Settings.bm.Add(u)
            My.Settings.Save()
            qrb()
            su.Visible = True
            su.Text = "Clipboard link bookmarked"
        Catch ex As Exception
        End Try
    End Sub
    Sub q256(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q255(sender, e)
        End If
    End Sub
    Sub q257(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim tx As String = wb.Document.Body.InnerText
            Dim ar() As String = tx.Split(New Char() {" "c, ControlChars.Tab, ControlChars.Cr, ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
            Clipboard.SetText("Title: " & wb.DocumentTitle & vbCrLf & "Address: " & wb.Url.ToString & vbCrLf & "Words: " & ar.Length.ToString)
            su.Visible = True
            su.Text = "Page word count copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q258(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q257(sender, e)
        End If
    End Sub
    Function qo() As String
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim tx As String = ""
            tx &= "Firespider Page Outline" & vbCrLf
            tx &= "Title: " & wb.DocumentTitle & vbCrLf
            tx &= "Address: " & wb.Url.ToString & vbCrLf
            tx &= "" & vbCrLf
            For Each he As HtmlElement In wb.Document.All
                Dim nm As String = he.TagName.ToUpper
                If nm = "H1" Or nm = "H2" Or nm = "H3" Then
                    Dim t As String = he.InnerText
                    If t = "" Then
                    Else
                        tx &= nm & ": " & t & vbCrLf
                    End If
                End If
            Next
            Return tx
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Sub q259(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = qo()
            If tx = "" Then
                su.Visible = True
                su.Text = "No page outline found"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Page Outline.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page Outline"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.Write(tx)
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Page outline saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q260(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q259(sender, e)
        End If
    End Sub
    Sub q261(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tx As String = qo()
            If tx = "" Then
                su.Visible = True
                su.Text = "No page outline found"
            Else
                Clipboard.SetText(tx)
                su.Visible = True
                su.Text = "Page outline copied"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q262(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q261(sender, e)
        End If
    End Sub
    Sub q263(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Open Tabs.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Open Tabs As Plain List"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If Not wb.Url Is Nothing Then
                        mySW.WriteLine(k.TabPages(q).Text)
                        mySW.WriteLine(wb.Url.ToString)
                        mySW.WriteLine("")
                    End If
                End If
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Open tabs list saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q264(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q263(sender, e)
        End If
    End Sub
    ' --- Reset Zoom To 100% ---
    Sub q265(ByVal sender As Object, ByVal e As EventArgs)
        Try
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Document.Body.Style &= ";zoom:100%"
            My.Settings.zl = "100"
            su.Visible = True
            su.Text = "Zoom reset to 100%"
        Catch ex As Exception
        End Try
    End Sub
    Sub q266(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q265(sender, e)
        End If
    End Sub
    ' --- Reading Mode ---
    Sub q267(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim script As String =
                "var b=document.body;" &
                "if(b.getAttribute('data-fsrd')){" &
                "b.style.cssText=b.getAttribute('data-fsrd-orig')||'';" &
                "b.removeAttribute('data-fsrd');" &
                "b.removeAttribute('data-fsrd-orig');" &
                "}else{" &
                "b.setAttribute('data-fsrd-orig',b.style.cssText||'');" &
                "b.style.cssText='max-width:750px;margin:40px auto;font-size:18px;line-height:1.7;font-family:Georgia,serif;background:#fffdf5;color:#222;padding:20px;';" &
                "b.setAttribute('data-fsrd','1');" &
                "}"
            wb.Document.InvokeScript("eval", New Object() {script})
            su.Visible = True
            su.Text = "Reading mode toggled"
        Catch ex As Exception
        End Try
    End Sub
    Sub q268(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q267(sender, e)
        End If
    End Sub
    ' --- Mute Page Audio ---
    Sub q269(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim script As String =
                "var els=document.querySelectorAll('audio,video');" &
                "for(var i=0;i<els.length;i++){" &
                "els[i].muted=!els[i].muted;" &
                "}"
            wb.Document.InvokeScript("eval", New Object() {script})
            su.Visible = True
            su.Text = "Page audio mute toggled"
        Catch ex As Exception
        End Try
    End Sub
    Sub q270(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q269(sender, e)
        End If
    End Sub
    ' --- Rename Current Tab ---
    Sub q271(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim nm As String = InputBox("Enter new tab name:", "Rename Tab", k.SelectedTab.Text)
            If nm = "" Then Exit Sub
            k.SelectedTab.Text = nm
            Text = ProductName & " - " & nm
            su.Visible = True
            su.Text = "Tab renamed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q272(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q271(sender, e)
        End If
    End Sub
    ' --- Lock / Unlock Current Tab ---
    Sub q273(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tag As String = k.SelectedTab.Tag
            If tag = "locked" Then
                k.SelectedTab.Tag = ""
                su.Visible = True
                su.Text = "Tab unlocked"
                If k.SelectedTab.Text.StartsWith("[L] ") Then
                    k.SelectedTab.Text = k.SelectedTab.Text.Substring(4)
                    Text = ProductName & " - " & k.SelectedTab.Text
                End If
            Else
                k.SelectedTab.Tag = "locked"
                If Not k.SelectedTab.Text.StartsWith("[L] ") Then
                    k.SelectedTab.Text = "[L] " & k.SelectedTab.Text
                    Text = ProductName & " - " & k.SelectedTab.Text
                End If
                su.Visible = True
                su.Text = "Tab locked - navigation blocked"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q274(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q273(sender, e)
        End If
    End Sub
    ' --- Show Tab Count ---
    Sub q275(ByVal sender As Object, ByVal e As EventArgs)
        Try
            su.Visible = True
            su.Text = "Open tabs: " & k.TabPages.Count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q276(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q275(sender, e)
        End If
    End Sub
    ' --- Quick Notes ---
    Sub q277(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim note As String = InputBox("Quick Notes (changes are saved):", "Quick Notes", My.Settings.qn)
            If note = "" AndAlso My.Settings.qn = "" Then Exit Sub
            My.Settings.qn = note
            My.Settings.Save()
            su.Visible = True
            su.Text = "Quick notes saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q278(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q277(sender, e)
        End If
    End Sub
    ' --- Navigate To IP Address ---
    Sub q279(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ip As String = InputBox("Enter IP address or local address to navigate to:", "Navigate To IP Address", "192.168.")
            If ip = "" Then Exit Sub
            If Not ip.StartsWith("http") Then
                ip = "http://" & ip
            End If
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(ip)
        Catch ex As Exception
        End Try
    End Sub
    Sub q280(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q279(sender, e)
        End If
    End Sub
    ' --- Bookmark Selected Text As Note ---
    Sub q281(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sel As String = wb.Document.GetElementsByTagName("body")(0).InnerText
            Dim selected As String = ""
            Try
                selected = wb.Document.GetElementFromPoint(
                    wb.PointToClient(Cursor.Position)).InnerText
            Catch
            End Try
            If selected = "" OrElse selected Is Nothing Then
                selected = InputBox("No text detected. Enter note text to bookmark:", "Bookmark Note", "")
            End If
            If selected = "" Then Exit Sub
            Dim note As String = "[NOTE] " & selected.Substring(0, Math.Min(80, selected.Length))
            Dim fv As New ToolStripMenuItem
            fv.Text = note
            fv.Tag = wb.Url
            fv.Image = My.Resources.F9
            fv.BackColor = Color.White
            fv.ForeColor = Color.Black
            fv.Padding = New Padding(0)
            AddHandler fv.Click, AddressOf z2
            AddHandler fv.MouseHover, AddressOf z2
            RB.DropDownItems.Add(fv)
            My.Settings.bm.Add(fv.Tag.ToString)
            My.Settings.Save()
            su.Visible = True
            su.Text = "Note bookmarked"
        Catch ex As Exception
        End Try
    End Sub
    Sub q282(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q281(sender, e)
        End If
    End Sub
    ' --- Copy Current Zoom Level ---
    Sub q283(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim zv As String = "unknown"
            Try
                Dim raw As Object = wb.Document.InvokeScript("eval",
                    New Object() {"(function(){var s=document.body.style.cssText;" &
                    "var m=s.match(/zoom:([^;]+)/);return m?m[1]:'100%';})();"})
                If raw IsNot Nothing Then zv = raw.ToString
            Catch
            End Try
            Clipboard.SetText("Zoom: " & zv)
            su.Visible = True
            su.Text = "Zoom level copied: " & zv
        Catch ex As Exception
        End Try
    End Sub
    Sub q284(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q283(sender, e)
        End If
    End Sub
    ' --- Invert Page Colors ---
    Sub q285(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim script As String =
                "(function(){" &
                "var h=document.documentElement;" &
                "if(h.getAttribute('data-fsinv')){" &
                "h.style.filter='';" &
                "h.removeAttribute('data-fsinv');" &
                "}else{" &
                "h.style.filter='invert(1) hue-rotate(180deg)';" &
                "h.setAttribute('data-fsinv','1');" &
                "}" &
                "})();"
            wb.Document.InvokeScript("eval", New Object() {script})
            su.Visible = True
            su.Text = "Page color invert toggled"
        Catch ex As Exception
        End Try
    End Sub
    Sub q286(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q285(sender, e)
        End If
    End Sub
    ' --- Focus Page (Remove Distractions) ---
    Sub q287(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim script As String =
                "(function(){" &
                "var h=document.documentElement;" &
                "if(h.getAttribute('data-fsfocus')){" &
                "var old=h.getAttribute('data-fsfocus-orig');" &
                "document.body.style.cssText=old||'';" &
                "h.removeAttribute('data-fsfocus');" &
                "h.removeAttribute('data-fsfocus-orig');" &
                "}else{" &
                "h.setAttribute('data-fsfocus-orig',document.body.style.cssText||'');" &
                "var ads=document.querySelectorAll(" &
                "'aside,nav,[id*=""ad""],[class*=""ad""]," &
                "[id*=""banner""],[class*=""banner""]," &
                "[id*=""sidebar""],[class*=""sidebar""]," &
                "[id*=""popup""],[class*=""popup""]')" &
                ";for(var i=0;i<ads.length;i++)ads[i].style.display='none';" &
                "document.body.style.maxWidth='800px';" &
                "document.body.style.margin='20px auto';" &
                "h.setAttribute('data-fsfocus','1');" &
                "}" &
                "})();"
            wb.Document.InvokeScript("eval", New Object() {script})
            su.Visible = True
            su.Text = "Focus mode toggled"
        Catch ex As Exception
        End Try
    End Sub
    Sub q288(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q287(sender, e)
        End If
    End Sub
    ' --- Copy Page Link Count ---
    Sub q289(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim count As Integer = 0
            For Each el As HtmlElement In wb.Document.Links
                count += 1
            Next
            Clipboard.SetText(count.ToString)
            su.Visible = True
            su.Text = "Link count copied: " & count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q290(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q289(sender, e)
        End If
    End Sub
    ' --- Save Page Links As Text ---
    Sub q291(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Page Links.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page Links As Text"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            For Each el As HtmlElement In wb.Document.Links
                Dim href As String = el.GetAttribute("href")
                If href <> "" Then
                    mySW.WriteLine(el.InnerText & vbTab & href)
                End If
            Next
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Page links saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q292(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q291(sender, e)
        End If
    End Sub
    ' --- Pin Tab To Top Of List ---
    Sub q293(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim tp As TabPage = k.SelectedTab
            Dim idx As Integer = k.TabPages.IndexOf(tp)
            If idx = 0 Then
                su.Visible = True
                su.Text = "Tab is already first"
                Exit Sub
            End If
            k.TabPages.Remove(tp)
            k.TabPages.Insert(0, tp)
            k.SelectedTab = tp
            su.Visible = True
            su.Text = "Tab pinned to top"
        Catch ex As Exception
        End Try
    End Sub
    Sub q294(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q293(sender, e)
        End If
    End Sub
    ' --- Bookmark All Filtered Tabs ---
    Sub q295(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim filter As String = InputBox("Enter keyword to filter tabs by title or URL:", "Bookmark Filtered Tabs", "")
            If filter = "" Then Exit Sub
            Dim added As Integer = 0
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wb.Url Is Nothing Then Continue For
                    Dim title As String = k.TabPages(q).Text
                    Dim url As String = wb.Url.ToString
                    If title.ToLower.Contains(filter.ToLower) Or url.ToLower.Contains(filter.ToLower) Then
                        Dim fv As New ToolStripMenuItem
                        fv.Text = If(title = "", url, title)
                        fv.Tag = url
                        fv.Image = My.Resources.F9
                        fv.BackColor = Color.White
                        fv.ForeColor = Color.Black
                        fv.Padding = New Padding(0)
                        AddHandler fv.Click, AddressOf z2
                        AddHandler fv.MouseHover, AddressOf z2
                        RB.DropDownItems.Add(fv)
                        My.Settings.bm.Add(url)
                        added += 1
                    End If
                End If
            Next q
            My.Settings.Save()
            su.Visible = True
            su.Text = added.ToString & " filtered tab(s) bookmarked"
        Catch ex As Exception
        End Try
    End Sub
    Sub q296(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q295(sender, e)
        End If
    End Sub
    ' --- Copy Page Character Count ---
    Sub q297(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim txt As String = wb.Document.Body.InnerText
            Dim chars As Integer = If(txt Is Nothing, 0, txt.Length)
            Clipboard.SetText(chars.ToString)
            su.Visible = True
            su.Text = "Character count copied: " & chars.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q298(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q297(sender, e)
        End If
    End Sub
    ' --- Open All Tabs From Same Domain In New Window ---
    Sub q299(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaDomainAction("wn")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q300(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q299(sender, e)
        End If
    End Sub
    ' --- Append To Quick Notes ---
    Sub q301(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim pageref As String = ""
            Try
                pageref = wb.DocumentTitle & " - " & wb.Url.ToString
            Catch
            End Try
            Dim append As String = InputBox("Text to append to Quick Notes:" & vbCrLf & "(Current page will be added as reference)", "Append To Quick Notes", "")
            If append = "" Then Exit Sub
            Dim sep As String = If(My.Settings.qn = "", "", vbCrLf & "---" & vbCrLf)
            My.Settings.qn = My.Settings.qn & sep & append & If(pageref = "", "", vbCrLf & "[" & pageref & "]")
            My.Settings.Save()
            su.Visible = True
            su.Text = "Appended to quick notes"
        Catch ex As Exception
        End Try
    End Sub
    Sub q302(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q301(sender, e)
        End If
    End Sub
    ' --- View Browser Session Summary ---
    Sub q303(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("=== Firespider Session Summary ===")
            sb.AppendLine("Open tabs:      " & k.TabPages.Count.ToString)
            sb.AppendLine("Bookmarks:      " & My.Settings.bm.Count.ToString)
            sb.AppendLine("History items:  " & f6.l0.Items.Count.ToString)
            sb.AppendLine("Search history: " & x.Items.Count.ToString)
            sb.AppendLine("Homepage:       " & My.Settings.h)
            sb.AppendLine("Search engine:  " & My.Settings.se)
            sb.AppendLine("Private mode:   " & If(pv, "On", "Off"))
            sb.AppendLine("Work offline:   " & If(wo, "On", "Off"))
            sb.AppendLine("Hover mode:     " & If(My.Settings.t = "e", "On", "Off"))
            sb.AppendLine("Siera:          " & If(My.Settings.si = "e", "On", "Off"))
            sb.AppendLine("Quick notes:    " & If(My.Settings.qn = "", "(empty)", My.Settings.qn.Substring(0, Math.Min(60, My.Settings.qn.Length)) & "..."))
            Try
                sb.AppendLine("Current page:   " & wb.DocumentTitle)
                sb.AppendLine("Current URL:    " & wb.Url.ToString)
            Catch
            End Try
            MsgBox(sb.ToString, MsgBoxStyle.Information, "Session Summary")
        Catch ex As Exception
        End Try
    End Sub
    Sub q304(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q303(sender, e)
        End If
    End Sub
    ' --- Highlight Text On Page ---
    Sub q305(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim term As String = InputBox("Enter text to highlight on this page:", "Highlight Text", "")
            If term = "" Then Exit Sub
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim escaped As String = term.Replace("'", "\'")
            Dim script As String =
                "(function(){" &
                "var body=document.body.innerHTML;" &
                "body=body.replace(/<mark[^>]*>/gi,'').replace(/<\/mark>/gi,'');" &
                "var re=new RegExp('(" & escaped & ")','gi');" &
                "document.body.innerHTML=body.replace(re,'<mark style=""background:#ff0;color:#000"">$1</mark>');" &
                "})();"
            wb.Document.InvokeScript("eval", New Object() {script})
            su.Visible = True
            su.Text = "Text highlighted: " & term
        Catch ex As Exception
        End Try
    End Sub
    Sub q306(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q305(sender, e)
        End If
    End Sub
    ' --- Save Page As JSON Snapshot ---
    Sub q307(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = wb.DocumentTitle & ".json"
            SX.Filter = "JSON File [*.json*]|*.json|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page As JSON Snapshot"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim title As String = wb.DocumentTitle.Replace("""", "\""")
            Dim url As String = ""
            Try
                url = wb.Url.ToString.Replace("""", "\""")
            Catch
            End Try
            Dim body As String = ""
            Try
                body = wb.Document.Body.InnerText.Replace("""", "\""").Replace(vbCrLf, "\n").Replace(vbLf, "\n").Replace(vbCr, "\n")
            Catch
            End Try
            Dim links As New System.Text.StringBuilder
            links.Append("[")
            Dim first As Boolean = True
            For Each el As HtmlElement In wb.Document.Links
                Dim href As String = el.GetAttribute("href").Replace("""", "\""")
                If href <> "" Then
                    If Not first Then links.Append(",")
                    links.Append("""" & href & """")
                    first = False
                End If
            Next
            links.Append("]")
            Dim json As New System.Text.StringBuilder
            json.AppendLine("{")
            json.AppendLine("  ""title"": """ & title & """,")
            json.AppendLine("  ""url"": """ & url & """,")
            json.AppendLine("  ""links"": " & links.ToString & ",")
            json.AppendLine("  ""text"": """ & body.Substring(0, Math.Min(2000, body.Length)) & """")
            json.AppendLine("}")
            System.IO.File.WriteAllText(SX.FileName, json.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Page JSON snapshot saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q308(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q307(sender, e)
        End If
    End Sub
    ' --- Copy Selected Text As Link ---
    Sub q309(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sel As String = ""
            Try
                sel = wb.Document.InvokeScript("eval",
                    New Object() {"window.getSelection().toString();"}).ToString.Trim
            Catch
            End Try
            If sel = "" Then
                su.Visible = True
                su.Text = "No text selected on page"
                Exit Sub
            End If
            Dim url As String = wb.Url.ToString
            Clipboard.SetText("[" & sel & "](" & url & ")")
            su.Visible = True
            su.Text = "Selected text copied as markdown link"
        Catch ex As Exception
        End Try
    End Sub
    Sub q310(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q309(sender, e)
        End If
    End Sub
    ' --- Open Bookmark In New Tab ---
    Sub q311(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim items() As String = New String(My.Settings.bm.Count - 1) {}
            For i As Integer = 0 To My.Settings.bm.Count - 1
                items(i) = My.Settings.bm.Item(i)
            Next i
            Dim pick As String = InputBox("Enter bookmark URL or number (1-" & My.Settings.bm.Count & "):" &
                vbCrLf & String.Join(vbCrLf, items.Take(10)), "Open Bookmark In New Tab", "1")
            If pick = "" Then Exit Sub
            Dim url As String = pick
            Dim idx As Integer = 0
            If Integer.TryParse(pick, idx) Then
                idx = idx - 1
                If idx >= 0 AndAlso idx < My.Settings.bm.Count Then
                    url = My.Settings.bm.Item(idx)
                End If
            End If
            q3(url)
        Catch ex As Exception
        End Try
    End Sub
    Sub q312(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q311(sender, e)
        End If
    End Sub
    ' --- Close Tabs To The Left ---
    Sub q313(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim cur As Integer = k.SelectedIndex
            If cur <= 0 Then
                su.Visible = True
                su.Text = "No tabs to the left"
                Exit Sub
            End If
            qleaCloseGroup("l")
            su.Visible = True
            su.Text = "Tabs to the left closed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q314(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q313(sender, e)
        End If
    End Sub
    ' --- Show Notes In Status Bar ---
    Sub q315(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
            Else
                su.Visible = True
                su.Text = My.Settings.qn.Substring(0, Math.Min(120, My.Settings.qn.Length))
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q316(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q315(sender, e)
        End If
    End Sub
    ' --- Copy Tab Index ---
    Sub q317(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim idx As Integer = k.SelectedIndex + 1
            Clipboard.SetText(idx.ToString)
            su.Visible = True
            su.Text = "Tab index copied: " & idx.ToString & " of " & k.TabPages.Count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q318(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q317(sender, e)
        End If
    End Sub
    ' --- Go To First Tab ---
    Sub q319(ByVal sender As Object, ByVal e As EventArgs)
        Try
            k.SelectedIndex = 0
            k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Text = ProductName & " - " & k.SelectedTab.Text
            c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q320(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q319(sender, e)
        End If
    End Sub
    ' --- Go To Last Tab ---
    Sub q321(ByVal sender As Object, ByVal e As EventArgs)
        Try
            k.SelectedIndex = k.TabPages.Count - 1
            k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Text = ProductName & " - " & k.SelectedTab.Text
            c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q322(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q321(sender, e)
        End If
    End Sub
    ' --- Save Bookmarks As Plain Text ---
    Sub q323(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks to save"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Bookmarks.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Bookmarks As Plain Text"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider Internet Bookmarks")
            mySW.WriteLine(New String("="c, 40))
            mySW.WriteLine("")
            For i As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(i) <> "" Then
                    mySW.WriteLine((i + 1).ToString & ". " & My.Settings.bm.Item(i))
                End If
            Next i
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Bookmarks saved as plain text"
        Catch ex As Exception
        End Try
    End Sub
    Sub q324(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q323(sender, e)
        End If
    End Sub
    ' --- Duplicate Tab In New Window ---
    Sub q325(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim nw As New j
            nw.Show()
            CType(nw.k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(wb.Url.ToString)
            su.Visible = True
            su.Text = "Tab duplicated in new window"
        Catch ex As Exception
        End Try
    End Sub
    Sub q326(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q325(sender, e)
        End If
    End Sub
    ' --- Scroll Page By Percentage ---
    Sub q327(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim pct As String = InputBox("Enter scroll percentage (0-100):", "Scroll Page By Percentage", "50")
            If pct = "" Then Exit Sub
            Dim n As Integer = 50
            If Not Integer.TryParse(pct, n) Then
                su.Visible = True
                su.Text = "Invalid percentage entered"
                Exit Sub
            End If
            n = Math.Max(0, Math.Min(100, n))
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim script As String =
                "(function(){" &
                "var h=document.documentElement.scrollHeight||document.body.scrollHeight;" &
                "window.scrollTo(0,Math.round(h*" & n & "/100));" &
                "})();"
            wb.Document.InvokeScript("eval", New Object() {script})
            su.Visible = True
            su.Text = "Scrolled to " & n.ToString & "%"
        Catch ex As Exception
        End Try
    End Sub
    Sub q328(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q327(sender, e)
        End If
    End Sub
    ' --- Copy Page Email Addresses ---
    Sub q329(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim emails As New List(Of String)
            Dim body As String = ""
            Try
                body = wb.Document.Body.InnerText
            Catch
            End Try
            Dim pattern As String = "[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}"
            Dim rx As New System.Text.RegularExpressions.Regex(pattern)
            For Each m As System.Text.RegularExpressions.Match In rx.Matches(body)
                If Not emails.Contains(m.Value.ToLower) Then
                    emails.Add(m.Value.ToLower)
                End If
            Next
            For Each el As HtmlElement In wb.Document.Links
                Dim href As String = el.GetAttribute("href")
                If href.StartsWith("mailto:") Then
                    Dim addr As String = href.Substring(7).ToLower
                    If Not emails.Contains(addr) Then emails.Add(addr)
                End If
            Next
            If emails.Count = 0 Then
                su.Visible = True
                su.Text = "No email addresses found on page"
                Exit Sub
            End If
            Clipboard.SetText(String.Join(vbCrLf, emails))
            su.Visible = True
            su.Text = emails.Count.ToString & " email address(es) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q330(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q329(sender, e)
        End If
    End Sub
    ' --- Save Quick Notes To File ---
    Sub q331(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Quick Notes.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Quick Notes To File"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            System.IO.File.WriteAllText(SX.FileName, My.Settings.qn, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Quick notes saved to file"
        Catch ex As Exception
        End Try
    End Sub
    Sub q332(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q331(sender, e)
        End If
    End Sub
    ' --- Swap Current Tab With Next ---
    Sub q333(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim idx As Integer = k.SelectedIndex
            If idx >= k.TabPages.Count - 1 Then
                su.Visible = True
                su.Text = "No tab to the right to swap with"
                Exit Sub
            End If
            Dim tp As TabPage = k.SelectedTab
            k.TabPages.Remove(tp)
            k.TabPages.Insert(idx + 1, tp)
            k.SelectedTab = tp
            su.Visible = True
            su.Text = "Tab swapped with next"
        Catch ex As Exception
        End Try
    End Sub
    Sub q334(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q333(sender, e)
        End If
    End Sub
    ' --- Copy Bookmarks As JSON ---
    Sub q335(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks to copy"
                Exit Sub
            End If
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("[")
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim url As String = My.Settings.bm.Item(i).Replace("""", "\""")
                If url = "" Then Continue For
                Dim comma As String = If(i < My.Settings.bm.Count - 1, ",", "")
                sb.AppendLine("  { ""url"": """ & url & """ }" & comma)
            Next i
            sb.AppendLine("]")
            Clipboard.SetText(sb.ToString)
            su.Visible = True
            su.Text = "Bookmarks copied as JSON"
        Catch ex As Exception
        End Try
    End Sub
    Sub q336(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q335(sender, e)
        End If
    End Sub
    ' --- Clear Quick Notes ---
    Sub q337(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim confirm As MsgBoxResult = MsgBox("Clear all quick notes?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Clear Quick Notes")
            If confirm = MsgBoxResult.Yes Then
                My.Settings.qn = ""
                My.Settings.Save()
                su.Visible = True
                su.Text = "Quick notes cleared"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q338(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q337(sender, e)
        End If
    End Sub
    ' --- Copy Page Headings ---
    Sub q339(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            For Each tag As String In {"h1", "h2", "h3", "h4", "h5", "h6"}
                For Each el As HtmlElement In wb.Document.GetElementsByTagName(tag)
                    Dim txt As String = el.InnerText
                    If txt IsNot Nothing AndAlso txt.Trim <> "" Then
                        sb.AppendLine("[" & tag.ToUpper & "] " & txt.Trim)
                    End If
                Next
            Next
            If sb.Length = 0 Then
                su.Visible = True
                su.Text = "No headings found on page"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString)
            su.Visible = True
            su.Text = "Page headings copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q340(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q339(sender, e)
        End If
    End Sub
    ' --- Delete Selected Bookmark ---
    Sub q341(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim items() As String = New String(Math.Min(My.Settings.bm.Count, 15) - 1) {}
            For i As Integer = 0 To items.Length - 1
                items(i) = (i + 1).ToString & ". " & My.Settings.bm.Item(i)
            Next i
            Dim pick As String = InputBox("Enter bookmark number to delete:" & vbCrLf & String.Join(vbCrLf, items), "Delete Bookmark", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= My.Settings.bm.Count Then
                su.Visible = True
                su.Text = "Bookmark number out of range"
                Exit Sub
            End If
            Dim removed As String = My.Settings.bm.Item(idx)
            My.Settings.bm.RemoveAt(idx)
            My.Settings.Save()
            For i As Integer = RB.DropDownItems.Count - 1 To 0 Step -1
                Dim mi As ToolStripMenuItem = TryCast(RB.DropDownItems(i), ToolStripMenuItem)
                If mi IsNot Nothing AndAlso mi.Tag IsNot Nothing AndAlso mi.Tag.ToString = removed Then
                    RB.DropDownItems.RemoveAt(i)
                    Exit For
                End If
            Next i
            su.Visible = True
            su.Text = "Bookmark deleted"
        Catch ex As Exception
        End Try
    End Sub
    Sub q342(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q341(sender, e)
        End If
    End Sub
    ' --- Show Keyboard Shortcuts ---
    Sub q343(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("=== Firespider Keyboard Shortcuts ===")
            sb.AppendLine("")
            sb.AppendLine("Ctrl+T          New Tab")
            sb.AppendLine("Ctrl+W          Close Tab")
            sb.AppendLine("Ctrl+R / F5     Refresh")
            sb.AppendLine("Ctrl+L          Focus Address Bar")
            sb.AppendLine("Ctrl+D          Bookmark Page")
            sb.AppendLine("Ctrl+H          Show History")
            sb.AppendLine("Ctrl+B          Show Bookmarks")
            sb.AppendLine("Alt+Left        Go Back")
            sb.AppendLine("Alt+Right       Go Forward")
            sb.AppendLine("")
            sb.AppendLine("Ctrl+Shift+T    Reopen Closed Tab")
            sb.AppendLine("Ctrl+Shift+C    Copy Page Link")
            sb.AppendLine("Ctrl+Shift+R    Reload All Tabs")
            sb.AppendLine("Ctrl+Shift+L    Copy Page As Markdown Link")
            sb.AppendLine("Ctrl+Shift+B    Bookmark Tabs From Same Domain")
            sb.AppendLine("Ctrl+Shift+U    Copy All Links On Page")
            sb.AppendLine("Ctrl+Shift+I    Copy Page Image Links")
            sb.AppendLine("Ctrl+Shift+S    Copy Tab Summary")
            sb.AppendLine("Ctrl+Shift+E    Save All Tabs As CSV")
            sb.AppendLine("Ctrl+Shift+M    Copy Bookmark Summary")
            sb.AppendLine("Ctrl+Shift+D    Search Current Domain")
            sb.AppendLine("Ctrl+Shift+K    Copy All Tabs As Markdown")
            sb.AppendLine("Ctrl+Shift+V    Open Clipboard Link In New Tab")
            sb.AppendLine("Ctrl+Shift+F    Search Open Tabs")
            sb.AppendLine("Ctrl+Shift+P    Copy Page Links As Markdown")
            MsgBox(sb.ToString, MsgBoxStyle.Information, "Keyboard Shortcuts")
        Catch ex As Exception
        End Try
    End Sub
    Sub q344(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q343(sender, e)
        End If
    End Sub
    ' --- Reload Tab Every 30 Seconds ---
    Sub q345(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If ti.Interval = 1 Then
                ti.Interval = 30000
                ti.Start()
                su.Visible = True
                su.Text = "Auto-reload every 30s ON"
            Else
                ti.Interval = 1
                ti.Stop()
                su.Visible = True
                su.Text = "Auto-reload OFF"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q346(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q345(sender, e)
        End If
    End Sub
    ' --- Save History As Plain Text ---
    Sub q347(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider History.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save History As Plain Text"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider Internet History")
            mySW.WriteLine(New String("="c, 40))
            mySW.WriteLine("")
            For i As Integer = 0 To f6.l0.Items.Count - 1
                mySW.WriteLine((i + 1).ToString & ". " & f6.l0.Items(i).ToString)
            Next i
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "History saved as plain text"
        Catch ex As Exception
        End Try
    End Sub
    Sub q348(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q347(sender, e)
        End If
    End Sub
    ' --- Copy Page Numbers ---
    Sub q349(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim body As String = ""
            Try
                body = wb.Document.Body.InnerText
            Catch
            End Try
            Dim rx As New System.Text.RegularExpressions.Regex("\b\d+(?:[.,]\d+)*\b")
            Dim nums As New List(Of String)
            For Each m As System.Text.RegularExpressions.Match In rx.Matches(body)
                If Not nums.Contains(m.Value) Then nums.Add(m.Value)
            Next
            If nums.Count = 0 Then
                su.Visible = True
                su.Text = "No numbers found on page"
                Exit Sub
            End If
            Clipboard.SetText(String.Join(vbCrLf, nums))
            su.Visible = True
            su.Text = nums.Count.ToString & " unique number(s) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q350(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q349(sender, e)
        End If
    End Sub
    ' --- Open Links From Clipboard In Tabs ---
    Sub q351(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaPaste(Clipboard.GetText(), True)
        Catch ex As Exception
        End Try
    End Sub
    Sub q352(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q351(sender, e)
        End If
    End Sub
    ' --- Bookmark Current Tab With Note ---
    Sub q353(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim note As String = InputBox("Enter a note for this bookmark:", "Bookmark With Note", wb.DocumentTitle)
            If note = "" Then Exit Sub
            Dim label As String = "[" & note & "] " & wb.DocumentTitle
            Dim fv As New ToolStripMenuItem
            fv.Text = label.Substring(0, Math.Min(80, label.Length))
            fv.Tag = wb.Url
            fv.Image = My.Resources.F9
            fv.BackColor = Color.White
            fv.ForeColor = Color.Black
            fv.Padding = New Padding(0)
            AddHandler fv.Click, AddressOf z2
            AddHandler fv.MouseHover, AddressOf z2
            RB.DropDownItems.Add(fv)
            My.Settings.bm.Add(wb.Url.ToString)
            My.Settings.Save()
            su.Visible = True
            su.Text = "Bookmarked with note"
        Catch ex As Exception
        End Try
    End Sub
    Sub q354(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q353(sender, e)
        End If
    End Sub
    ' --- Copy Next Tab URL ---
    Sub q355(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim idx As Integer = k.SelectedIndex + 1
            If idx >= k.TabPages.Count Then
                su.Visible = True
                su.Text = "No tab to the right"
                Exit Sub
            End If
            Dim wb As WebBrowser = CType(k.TabPages(idx).Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "Next tab has no URL"
                Exit Sub
            End If
            Clipboard.SetText(wb.Url.ToString)
            su.Visible = True
            su.Text = "Next tab URL copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q356(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q355(sender, e)
        End If
    End Sub
    ' --- Import Quick Notes From File ---
    Sub q357(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ox As New OpenFileDialog()
            ox.Title = "Import Quick Notes From File"
            ox.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            ox.CheckFileExists = True
            ox.ShowDialog()
            If ox.FileName = "" Then Exit Sub
            Dim imported As String = System.IO.File.ReadAllText(ox.FileName, System.Text.Encoding.UTF8)
            If My.Settings.qn = "" Then
                My.Settings.qn = imported
            Else
                My.Settings.qn = My.Settings.qn & vbCrLf & "---" & vbCrLf & imported
            End If
            My.Settings.Save()
            su.Visible = True
            su.Text = "Quick notes imported from file"
        Catch ex As Exception
        End Try
    End Sub
    Sub q358(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q357(sender, e)
        End If
    End Sub
    ' --- Copy Previous Tab URL ---
    Sub q359(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim idx As Integer = k.SelectedIndex - 1
            If idx < 0 Then
                su.Visible = True
                su.Text = "No tab to the left"
                Exit Sub
            End If
            Dim wb As WebBrowser = CType(k.TabPages(idx).Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "Previous tab has no URL"
                Exit Sub
            End If
            Clipboard.SetText(wb.Url.ToString)
            su.Visible = True
            su.Text = "Previous tab URL copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q360(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q359(sender, e)
        End If
    End Sub
    ' --- Show Page Load Progress ---
    Sub q361(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim loaded As Integer = CInt((LO.Width / Size.Width) * 100)
            If LO.Width <= 1 Then
                su.Visible = True
                su.Text = "Page fully loaded (100%)"
            Else
                su.Visible = True
                su.Text = "Page load progress: " & loaded.ToString & "%"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q362(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q361(sender, e)
        End If
    End Sub
    ' --- Search Bookmarks ---
    Sub q363(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim term As String = InputBox("Enter search term:", "Search Bookmarks", "")
            If term = "" Then Exit Sub
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim raw As String = My.Settings.bm.Item(i)
                Dim url As String = qbu(raw)
                If raw.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 Then
                    q1(url)
                    su.Visible = True
                    su.Text = "Bookmark match opened"
                    Exit Sub
                End If
            Next i
            su.Visible = True
            su.Text = "No bookmark match found"
        Catch ex As Exception
        End Try
    End Sub
    Sub q364(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q363(sender, e)
        End If
    End Sub
    ' --- Search History ---
    Sub q365(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim term As String = InputBox("Enter search term:", "Search History", "")
            If term = "" Then Exit Sub
            Dim results As New System.Text.StringBuilder
            Dim found As Integer = 0
            For i As Integer = 0 To f6.l0.Items.Count - 1
                Dim url As String = f6.l0.Items(i).ToString
                If url.ToLower.Contains(term.ToLower) Then
                    results.AppendLine((i + 1).ToString & ". " & url)
                    found += 1
                End If
            Next i
            If found = 0 Then
                su.Visible = True
                su.Text = "No history matches: " & term
            Else
                MsgBox(results.ToString, MsgBoxStyle.Information, found.ToString & " History Item(s) Found")
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q366(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q365(sender, e)
        End If
    End Sub
    ' --- Print Page Silent ---
    Sub q367(ByVal sender As Object, ByVal e As EventArgs)
        Try
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Print()
            su.Visible = True
            su.Text = "Page sent to printer"
        Catch ex As Exception
        End Try
    End Sub
    Sub q368(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q367(sender, e)
        End If
    End Sub
    ' --- Copy Page Image Count ---
    Sub q369(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim count As Integer = wb.Document.GetElementsByTagName("img").Count
            Clipboard.SetText(count.ToString)
            su.Visible = True
            su.Text = "Image count copied: " & count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q370(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q369(sender, e)
        End If
    End Sub
    ' --- Open All History In Tabs ---
    Sub q371(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim confirm As MsgBoxResult = MsgBox("Open all " & f6.l0.Items.Count & " history items in tabs?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Open All History In Tabs")
            If confirm <> MsgBoxResult.Yes Then Exit Sub
            For i As Integer = 0 To f6.l0.Items.Count - 1
                q3(f6.l0.Items(i).ToString)
            Next i
            su.Visible = True
            su.Text = f6.l0.Items.Count.ToString & " history item(s) opened"
        Catch ex As Exception
        End Try
    End Sub
    Sub q372(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q371(sender, e)
        End If
    End Sub
    ' --- Set Custom Tab Color ---
    Sub q373(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim co As New ColorDialog
            co.AnyColor = True
            co.FullOpen = True
            co.ShowDialog()
            k.SelectedTab.BackColor = co.Color
            su.Visible = True
            su.Text = "Tab color set"
        Catch ex As Exception
        End Try
    End Sub
    Sub q374(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q373(sender, e)
        End If
    End Sub
    ' --- Copy Full Page Text As Lines ---
    Sub q375(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            If raw = "" Then
                su.Visible = True
                su.Text = "No text found on page"
                Exit Sub
            End If
            Dim lines() As String = raw.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim sb As New System.Text.StringBuilder
            For Each ln As String In lines
                Dim trimmed As String = ln.Trim
                If trimmed <> "" Then sb.AppendLine(trimmed)
            Next
            Clipboard.SetText(sb.ToString)
            su.Visible = True
            su.Text = lines.Length.ToString & " line(s) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q376(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q375(sender, e)
        End If
    End Sub
    ' --- Save Quick Notes With Timestamp ---
    Sub q377(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim stamp As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
            Dim sep As String = If(My.Settings.qn = "", "", vbCrLf & "---" & vbCrLf)
            Dim note As String = InputBox("Enter note to save with timestamp:", "Save Quick Note With Timestamp", "")
            If note = "" Then Exit Sub
            My.Settings.qn = My.Settings.qn & sep & "[" & stamp & "] " & note
            My.Settings.Save()
            su.Visible = True
            su.Text = "Timestamped note saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q378(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q377(sender, e)
        End If
    End Sub
    ' --- Open Bookmark By Number ---
    Sub q379(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim pick As String = InputBox("Enter bookmark number to open (1-" & My.Settings.bm.Count & "):", "Open Bookmark By Number", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= My.Settings.bm.Count Then
                su.Visible = True
                su.Text = "Number out of range"
                Exit Sub
            End If
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(My.Settings.bm.Item(idx))
        Catch ex As Exception
        End Try
    End Sub
    Sub q380(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q379(sender, e)
        End If
    End Sub
    ' --- Show Total Tabs Opened Count ---
    Sub q381(ByVal sender As Object, ByVal e As EventArgs)
        Try
            su.Visible = True
            su.Text = "Total tabs opened this session: " & I.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q382(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q381(sender, e)
        End If
    End Sub
    ' --- Copy Page Last Modified ---
    Sub q383(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim lastMod As Object = wb.Document.InvokeScript("eval",
                New Object() {"document.lastModified;"})
            Dim result As String = If(lastMod IsNot Nothing, lastMod.ToString, "Not available")
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = "Last modified copied: " & result
        Catch ex As Exception
        End Try
    End Sub
    Sub q384(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q383(sender, e)
        End If
    End Sub
    ' --- Go To Tab By Number ---
    Sub q385(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim pick As String = InputBox("Enter tab number to go to (1-" & k.TabPages.Count & "):", "Go To Tab By Number", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= k.TabPages.Count Then
                su.Visible = True
                su.Text = "Tab number out of range"
                Exit Sub
            End If
            k.SelectedIndex = idx
            k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Text = ProductName & " - " & k.SelectedTab.Text
            c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q386(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q385(sender, e)
        End If
    End Sub
    ' --- Save Page As Markdown ---
    Sub q387(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = wb.DocumentTitle & ".md"
            SX.Filter = "Markdown File [*.md*]|*.md|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page As Markdown"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("# " & wb.DocumentTitle)
            sb.AppendLine("")
            sb.AppendLine("> " & wb.Url.ToString)
            sb.AppendLine("")
            Try
                Dim body As String = wb.Document.Body.InnerText
                If body IsNot Nothing Then
                    For Each ln As String In body.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
                        Dim t As String = ln.Trim
                        If t <> "" Then sb.AppendLine(t & "  ")
                    Next
                End If
            Catch
            End Try
            sb.AppendLine("")
            sb.AppendLine("## Links")
            sb.AppendLine("")
            For Each el As HtmlElement In wb.Document.Links
                Dim href As String = el.GetAttribute("href")
                Dim txt As String = If(el.InnerText IsNot Nothing, el.InnerText.Trim, href)
                If href <> "" Then sb.AppendLine("- [" & txt & "](" & href & ")")
            Next
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Page saved as Markdown"
        Catch ex As Exception
        End Try
    End Sub
    Sub q388(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q387(sender, e)
        End If
    End Sub
    ' --- Copy Page Description ---
    Sub q389(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim desc As String = ""
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("meta")
                Dim name As String = el.GetAttribute("name")
                If name IsNot Nothing AndAlso name.ToLower = "description" Then
                    desc = el.GetAttribute("content")
                    Exit For
                End If
            Next
            If desc = "" Then
                su.Visible = True
                su.Text = "No meta description found on page"
                Exit Sub
            End If
            Clipboard.SetText(desc)
            su.Visible = True
            su.Text = "Page description copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q390(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q389(sender, e)
        End If
    End Sub
    ' --- Translate Page (Google) ---
    Sub q391(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim encoded As String = Uri.EscapeDataString(wb.Url.ToString)
            q3("https://translate.google.com/translate?u=" & encoded)
            su.Visible = True
            su.Text = "Opening in Google Translate"
        Catch ex As Exception
        End Try
    End Sub
    Sub q392(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q391(sender, e)
        End If
    End Sub
    ' --- Merge All Bookmarks To Clipboard ---
    Sub q393(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks to merge"
                Exit Sub
            End If
            Dim sb As New System.Text.StringBuilder
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim url As String = My.Settings.bm.Item(i)
                If url <> "" Then sb.AppendLine(url)
            Next i
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = My.Settings.bm.Count.ToString & " bookmark(s) merged to clipboard"
        Catch ex As Exception
        End Try
    End Sub
    Sub q394(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q393(sender, e)
        End If
    End Sub
    ' --- Read Quick Notes Aloud ---
    Sub q395(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            z1.Speak(My.Settings.qn)
            su.Visible = True
            su.Text = "Reading quick notes aloud"
        Catch ex As Exception
        End Try
    End Sub
    Sub q396(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q395(sender, e)
        End If
    End Sub
    ' --- Open Wayback Machine For Page ---
    Sub q397(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            q3("https://web.archive.org/web/*/" & wb.Url.ToString)
            su.Visible = True
            su.Text = "Opening Wayback Machine"
        Catch ex As Exception
        End Try
    End Sub
    Sub q398(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q397(sender, e)
        End If
    End Sub
    ' --- Copy Bookmark Count ---
    Sub q399(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim count As Integer = My.Settings.bm.Count
            Clipboard.SetText(count.ToString)
            su.Visible = True
            su.Text = "Bookmark count copied: " & count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q400(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q399(sender, e)
        End If
    End Sub
    ' --- Save All Tab Titles To File ---
    Sub q401(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Tab Titles.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save All Tab Titles To File"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Firespider Internet Tab Titles")
            mySW.WriteLine(New String("="c, 40))
            mySW.WriteLine("")
            For q As Integer = 0 To k.TabPages.Count - 1
                Dim title As String = k.TabPages(q).Text
                Dim url As String = ""
                Try
                    Dim wbq As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wbq.Url IsNot Nothing Then url = wbq.Url.ToString
                Catch
                End Try
                mySW.WriteLine((q + 1).ToString & ". " & title)
                If url <> "" Then mySW.WriteLine("   " & url)
                mySW.WriteLine("")
            Next q
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = "Tab titles saved to file"
        Catch ex As Exception
        End Try
    End Sub
    Sub q402(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q401(sender, e)
        End If
    End Sub
    ' --- Open Web Search For Selected Text ---
    Sub q403(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sel As String = ""
            Try
                Dim raw As Object = wb.Document.InvokeScript("eval",
                    New Object() {"window.getSelection().toString();"})
                If raw IsNot Nothing Then sel = raw.ToString.Trim
            Catch
            End Try
            If sel = "" Then
                su.Visible = True
                su.Text = "No text selected on page"
                Exit Sub
            End If
            q3(My.Settings.se & Uri.EscapeDataString(sel))
            su.Visible = True
            su.Text = "Searching: " & sel.Substring(0, Math.Min(40, sel.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q404(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q403(sender, e)
        End If
    End Sub
    ' --- Copy Page Canonical URL ---
    Sub q405(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim canonical As String = ""
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("link")
                If el.GetAttribute("rel") IsNot Nothing AndAlso
                   el.GetAttribute("rel").ToLower = "canonical" Then
                    canonical = el.GetAttribute("href")
                    Exit For
                End If
            Next
            If canonical = "" Then
                canonical = wb.Url.ToString
                su.Visible = True
                su.Text = "No canonical tag - copied current URL"
            Else
                su.Visible = True
                su.Text = "Canonical URL copied"
            End If
            Clipboard.SetText(canonical)
        Catch ex As Exception
        End Try
    End Sub
    Sub q406(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q405(sender, e)
        End If
    End Sub
    ' --- Share Page Via QR Code ---
    Sub q407(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim encoded As String = Uri.EscapeDataString(wb.Url.ToString)
            q3("https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=" & encoded)
            su.Visible = True
            su.Text = "QR code opened in new tab"
        Catch ex As Exception
        End Try
    End Sub
    Sub q408(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q407(sender, e)
        End If
    End Sub
    ' --- Sort History Alphabetically ---
    Sub q409(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim items As New List(Of String)
            For i As Integer = 0 To f6.l0.Items.Count - 1
                items.Add(f6.l0.Items(i).ToString)
            Next i
            items.Sort(StringComparer.OrdinalIgnoreCase)
            f6.l0.Items.Clear()
            c.Items.Clear()
            For Each item As String In items
                f6.l0.Items.Add(item)
                c.Items.Add(item)
            Next
            su.Visible = True
            su.Text = "History sorted alphabetically"
        Catch ex As Exception
        End Try
    End Sub
    Sub q410(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q409(sender, e)
        End If
    End Sub
    ' --- Copy Page Keywords ---
    Sub q411(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim keywords As String = ""
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("meta")
                Dim name As String = el.GetAttribute("name")
                If name IsNot Nothing AndAlso name.ToLower = "keywords" Then
                    keywords = el.GetAttribute("content")
                    Exit For
                End If
            Next
            If keywords = "" Then
                su.Visible = True
                su.Text = "No keywords meta tag found on page"
                Exit Sub
            End If
            Clipboard.SetText(keywords)
            su.Visible = True
            su.Text = "Page keywords copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q412(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q411(sender, e)
        End If
    End Sub
    ' --- Open Archive.org For Page ---
    Sub q413(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            q3("https://archive.org/search?query=" & Uri.EscapeDataString(wb.Url.ToString))
            su.Visible = True
            su.Text = "Opening Archive.org search"
        Catch ex As Exception
        End Try
    End Sub
    Sub q414(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q413(sender, e)
        End If
    End Sub
    ' --- Swap Tab With Previous ---
    Sub q415(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim idx As Integer = k.SelectedIndex
            If idx = 0 Then
                su.Visible = True
                su.Text = "No tab to the left to swap with"
                Exit Sub
            End If
            Dim tp As TabPage = k.SelectedTab
            k.TabPages.Remove(tp)
            k.TabPages.Insert(idx - 1, tp)
            k.SelectedTab = tp
            su.Visible = True
            su.Text = "Tab swapped with previous"
        Catch ex As Exception
        End Try
    End Sub
    Sub q416(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q415(sender, e)
        End If
    End Sub
    ' --- Copy Quick Notes To Clipboard ---
    Sub q417(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Clipboard.SetText(My.Settings.qn)
            su.Visible = True
            su.Text = "Quick notes copied to clipboard"
        Catch ex As Exception
        End Try
    End Sub
    Sub q418(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q417(sender, e)
        End If
    End Sub
    ' --- Reload All Tabs From Same Domain ---
    Sub q419(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaDomainAction("rl")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q420(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q419(sender, e)
        End If
    End Sub
    ' --- Show Browser Statistics ---
    Sub q421(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("=== Firespider Browser Statistics ===")
            sb.AppendLine("")
            sb.AppendLine("Open tabs:            " & k.TabPages.Count.ToString)
            sb.AppendLine("Total tabs opened:    " & I.ToString)
            sb.AppendLine("Bookmarks saved:      " & My.Settings.bm.Count.ToString)
            sb.AppendLine("History items:        " & f6.l0.Items.Count.ToString)
            sb.AppendLine("Search suggestions:   " & x.Items.Count.ToString)
            sb.AppendLine("Closed tab URLs saved:" & My.Settings.rc.Count.ToString)
            sb.AppendLine("Session URLs saved:   " & My.Settings.ls.Count.ToString)
            sb.AppendLine("Quick notes length:   " & My.Settings.qn.Length.ToString & " chars")
            sb.AppendLine("")
            Dim domCount As New Dictionary(Of String, Integer)
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Try
                        Dim wbq As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                        If wbq.Url IsNot Nothing Then
                            Dim host As String = wbq.Url.Host
                            If domCount.ContainsKey(host) Then
                                domCount(host) += 1
                            Else
                                domCount.Add(host, 1)
                            End If
                        End If
                    Catch
                    End Try
                End If
            Next q
            sb.AppendLine("Unique domains open:  " & domCount.Count.ToString)
            If domCount.Count > 0 Then
                sb.AppendLine("")
                sb.AppendLine("Domains:")
                For Each kvp As KeyValuePair(Of String, Integer) In domCount
                    sb.AppendLine("  " & kvp.Key & " (" & kvp.Value.ToString & " tab(s))")
                Next
            End If
            MsgBox(sb.ToString, MsgBoxStyle.Information, "Browser Statistics")
        Catch ex As Exception
        End Try
    End Sub
    Sub q422(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q421(sender, e)
        End If
    End Sub
    ' --- Copy Favicon URL ---
    Sub q423(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim favicon As String = ""
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("link")
                Dim rel As String = el.GetAttribute("rel")
                If rel IsNot Nothing AndAlso (rel.ToLower.Contains("icon")) Then
                    favicon = el.GetAttribute("href")
                    Exit For
                End If
            Next
            If favicon = "" Then
                Try
                    favicon = wb.Url.Scheme & "://" & wb.Url.Host & "/favicon.ico"
                Catch
                End Try
            End If
            If favicon = "" Then
                su.Visible = True
                su.Text = "No favicon found"
                Exit Sub
            End If
            If favicon.StartsWith("//") Then favicon = "https:" & favicon
            If favicon.StartsWith("/") AndAlso wb.Url IsNot Nothing Then
                favicon = wb.Url.Scheme & "://" & wb.Url.Host & favicon
            End If
            Clipboard.SetText(favicon)
            su.Visible = True
            su.Text = "Favicon URL copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q424(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q423(sender, e)
        End If
    End Sub
    ' --- Reverse Bookmark Order ---
    Sub q425(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count < 2 Then
                su.Visible = True
                su.Text = "Not enough bookmarks to reverse"
                Exit Sub
            End If
            Dim items As New List(Of String)
            For i As Integer = 0 To My.Settings.bm.Count - 1
                items.Add(My.Settings.bm.Item(i))
            Next i
            items.Reverse()
            My.Settings.bm.Clear()
            RB.DropDownItems.Clear()
            For Each url As String In items
                If url <> "" Then
                    My.Settings.bm.Add(url)
                    Dim fv As New ToolStripMenuItem
                    fv.Text = url.Substring(0, Math.Min(60, url.Length))
                    fv.Tag = url
                    fv.Image = My.Resources.F9
                    fv.BackColor = Color.White
                    fv.ForeColor = Color.Black
                    fv.Padding = New Padding(0)
                    AddHandler fv.Click, AddressOf z2
                    AddHandler fv.MouseHover, AddressOf z2
                    RB.DropDownItems.Add(fv)
                End If
            Next
            My.Settings.Save()
            su.Visible = True
            su.Text = "Bookmark order reversed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q426(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q425(sender, e)
        End If
    End Sub
    ' --- Save Page As CSV Table ---
    Sub q427(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim tables As HtmlElementCollection = wb.Document.GetElementsByTagName("table")
            If tables.Count = 0 Then
                su.Visible = True
                su.Text = "No tables found on page"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = wb.DocumentTitle & ".csv"
            SX.Filter = "CSV File [*.csv*]|*.csv|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page As CSV Table"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            For Each tbl As HtmlElement In tables
                For Each row As HtmlElement In tbl.GetElementsByTagName("tr")
                    Dim cells As New List(Of String)
                    For Each cell As HtmlElement In row.GetElementsByTagName("td")
                        Dim txt As String = If(cell.InnerText IsNot Nothing, cell.InnerText.Trim.Replace("""", """"""), "")
                        cells.Add("""" & txt & """")
                    Next
                    For Each cell As HtmlElement In row.GetElementsByTagName("th")
                        Dim txt As String = If(cell.InnerText IsNot Nothing, cell.InnerText.Trim.Replace("""", """"""), "")
                        cells.Add("""" & txt & """")
                    Next
                    If cells.Count > 0 Then sb.AppendLine(String.Join(",", cells))
                Next
                sb.AppendLine("")
            Next
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = tables.Count.ToString & " table(s) saved as CSV"
        Catch ex As Exception
        End Try
    End Sub
    Sub q428(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q427(sender, e)
        End If
    End Sub
    ' --- Open Page In New Private Tab ---
    Sub q429(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim url As String = wb.Url.ToString
            Dim oldPv As Boolean = pv
            pv = True
            My.Settings.b0 = "f"
            q3(url)
            pv = oldPv
            If Not pv Then My.Settings.b0 = "e"
            su.Visible = True
            su.Text = "Page opened in private tab"
        Catch ex As Exception
        End Try
    End Sub
    Sub q430(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q429(sender, e)
        End If
    End Sub
    ' --- Copy Open Tab Count ---
    Sub q431(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Clipboard.SetText(k.TabPages.Count.ToString)
            su.Visible = True
            su.Text = "Open tab count copied: " & k.TabPages.Count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q432(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q431(sender, e)
        End If
    End Sub
    ' --- Shuffle Tab Order ---
    Sub q433(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.TabPages.Count < 2 Then
                su.Visible = True
                su.Text = "Not enough tabs to shuffle"
                Exit Sub
            End If
            Dim pages As New List(Of TabPage)
            For i As Integer = 0 To k.TabPages.Count - 1
                pages.Add(k.TabPages(i))
            Next i
            Dim rng As New Random
            For i As Integer = pages.Count - 1 To 1 Step -1
                Dim j As Integer = rng.Next(0, i + 1)
                Dim tmp As TabPage = pages(i)
                pages(i) = pages(j)
                pages(j) = tmp
            Next i
            k.TabPages.Clear()
            For Each tp As TabPage In pages
                k.TabPages.Add(tp)
            Next
            k.SelectedIndex = 0
            su.Visible = True
            su.Text = "Tab order shuffled"
        Catch ex As Exception
        End Try
    End Sub
    Sub q434(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q433(sender, e)
        End If
    End Sub
    ' --- Copy Open Graph Tags ---
    Sub q435(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("meta")
                Dim prop As String = el.GetAttribute("property")
                If prop IsNot Nothing AndAlso prop.ToLower.StartsWith("og:") Then
                    Dim content As String = el.GetAttribute("content")
                    If content IsNot Nothing Then
                        sb.AppendLine(prop & ": " & content)
                    End If
                End If
            Next
            If sb.Length = 0 Then
                su.Visible = True
                su.Text = "No Open Graph tags found on page"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString)
            su.Visible = True
            su.Text = "Open Graph tags copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q436(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q435(sender, e)
        End If
    End Sub
    ' --- Speak Current Page Title ---
    Sub q437(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim title As String = wb.DocumentTitle
            If title = "" Then title = "Untitled page"
            Dim domain As String = ""
            If wb.Url IsNot Nothing Then domain = wb.Url.Host
            z1.Speak(title & If(domain = "", "", ". " & domain))
            su.Visible = True
            su.Text = "Page title spoken"
        Catch ex As Exception
        End Try
    End Sub
    Sub q438(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q437(sender, e)
        End If
    End Sub
    ' --- Insert Page Note Into Quick Notes ---
    Sub q439(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim stamp As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
            Dim title As String = wb.DocumentTitle
            Dim url As String = ""
            Try
                url = wb.Url.ToString
            Catch
            End Try
            Dim note As String = InputBox("Enter note for this page:", "Insert Page Note", "")
            If note = "" Then Exit Sub
            Dim entry As String = "[" & stamp & "] " & title & vbCrLf &
                                  "URL: " & url & vbCrLf &
                                  "Note: " & note
            Dim sep As String = If(My.Settings.qn = "", "", vbCrLf & "---" & vbCrLf)
            My.Settings.qn = My.Settings.qn & sep & entry
            My.Settings.Save()
            su.Visible = True
            su.Text = "Page note inserted into quick notes"
        Catch ex As Exception
        End Try
    End Sub
    Sub q440(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q439(sender, e)
        End If
    End Sub
    ' --- Copy History Count ---
    Sub q441(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim count As Integer = f6.l0.Items.Count
            Clipboard.SetText(count.ToString)
            su.Visible = True
            su.Text = "History count copied: " & count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q442(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q441(sender, e)
        End If
    End Sub
    ' --- Copy Current Tab Title ---
    Sub q443(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim title As String = k.SelectedTab.Text
            If title = "" Then title = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Clipboard.SetText(title)
            su.Visible = True
            su.Text = "Tab title copied: " & title.Substring(0, Math.Min(50, title.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q444(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q443(sender, e)
        End If
    End Sub
    ' --- Open Parent URL ---
    Sub q445(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim url As String = wb.Url.ToString.TrimEnd("/"c)
            Dim lastSlash As Integer = url.LastIndexOf("/")
            Dim schemeEnd As Integer = url.IndexOf("://")
            If lastSlash > schemeEnd + 2 Then
                Dim parent As String = url.Substring(0, lastSlash)
                CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(parent)
                su.Visible = True
                su.Text = "Navigating to parent URL"
            Else
                su.Visible = True
                su.Text = "Already at root URL"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q446(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q445(sender, e)
        End If
    End Sub
    ' --- Sort Bookmarks By Domain ---
    Sub q447(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count < 2 Then
                su.Visible = True
                su.Text = "Not enough bookmarks to sort"
                Exit Sub
            End If
            Dim items As New List(Of String)
            For i As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(i) <> "" Then items.Add(My.Settings.bm.Item(i))
            Next i
            items.Sort(Function(a, b)
                           Try
                               Dim da As String = New Uri(a).Host
                               Dim db As String = New Uri(b).Host
                               Return String.Compare(da, db, StringComparison.OrdinalIgnoreCase)
                           Catch
                               Return String.Compare(a, b, StringComparison.OrdinalIgnoreCase)
                           End Try
                       End Function)
            My.Settings.bm.Clear()
            RB.DropDownItems.Clear()
            For Each url As String In items
                My.Settings.bm.Add(url)
                Dim fv As New ToolStripMenuItem
                fv.Text = url.Substring(0, Math.Min(60, url.Length))
                fv.Tag = url
                fv.Image = My.Resources.F9
                fv.BackColor = Color.White
                fv.ForeColor = Color.Black
                fv.Padding = New Padding(0)
                AddHandler fv.Click, AddressOf z2
                AddHandler fv.MouseHover, AddressOf z2
                RB.DropDownItems.Add(fv)
            Next
            My.Settings.Save()
            su.Visible = True
            su.Text = "Bookmarks sorted by domain"
        Catch ex As Exception
        End Try
    End Sub
    Sub q448(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q447(sender, e)
        End If
    End Sub
    ' --- Copy Page Font Families ---
    Sub q449(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim fonts As New List(Of String)
            Dim raw As Object = wb.Document.InvokeScript("eval",
                New Object() {
                    "(function(){" &
                    "var els=document.querySelectorAll('*');" &
                    "var seen={};" &
                    "var result=[];" &
                    "for(var i=0;i<els.length;i++){" &
                    "var f=window.getComputedStyle(els[i]).fontFamily;" &
                    "if(f&&!seen[f]){seen[f]=1;result.push(f);}" &
                    "}" &
                    "return result.join('\n');" &
                    "})();"})
            Dim result As String = If(raw IsNot Nothing, raw.ToString.Trim, "")
            If result = "" Then
                su.Visible = True
                su.Text = "No font families detected"
                Exit Sub
            End If
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = "Page font families copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q450(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q449(sender, e)
        End If
    End Sub
    ' --- Speak Page URL ---
    Sub q451(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL to speak"
                Exit Sub
            End If
            Dim url As String = wb.Url.ToString
            z1.Speak(url)
            su.Visible = True
            su.Text = "Speaking URL"
        Catch ex As Exception
        End Try
    End Sub
    Sub q452(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q451(sender, e)
        End If
    End Sub
    ' --- Open All Tabs In Same Window (Reload) ---
    Sub q453(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.TabPages.Count = 0 Then Exit Sub
            Dim confirm As MsgBoxResult = MsgBox("Reload all " & k.TabPages.Count & " tabs?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Reload All Tabs")
            If confirm <> MsgBoxResult.Yes Then Exit Sub
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    CType(k.TabPages(q).Controls.Item(0), WebBrowser).Refresh()
                End If
            Next q
            su.Visible = True
            su.Text = "All tabs reloaded"
        Catch ex As Exception
        End Try
    End Sub
    Sub q454(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q453(sender, e)
        End If
    End Sub
    ' --- Export Quick Notes As HTML ---
    Sub q455(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Quick Notes.html"
            SX.Filter = "HTML File [*.html*]|*.html|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Export Quick Notes As HTML"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("<!DOCTYPE html>")
            sb.AppendLine("<html><head><meta charset=""utf-8"">")
            sb.AppendLine("<title>Firespider Quick Notes</title>")
            sb.AppendLine("<style>body{font-family:Georgia,serif;max-width:800px;margin:40px auto;line-height:1.7;background:#fffdf5;color:#222;}h1{color:#333;}hr{border:1px solid #ccc;}pre{white-space:pre-wrap;word-break:break-all;}</style>")
            sb.AppendLine("</head><body>")
            sb.AppendLine("<h1>Firespider Quick Notes</h1>")
            sb.AppendLine("<p><em>Exported: " & Now.ToString("yyyy-MM-dd HH:mm:ss") & "</em></p><hr>")
            Dim lines() As String = My.Settings.qn.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.None)
            For Each ln As String In lines
                Dim escaped As String = ln.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                sb.AppendLine("<p>" & escaped & "</p>")
            Next
            sb.AppendLine("</body></html>")
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Quick notes exported as HTML"
        Catch ex As Exception
        End Try
    End Sub
    Sub q456(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q455(sender, e)
        End If
    End Sub
    ' --- Copy Page HTTP Status Code ---
    Sub q457(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim req As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(wb.Url), System.Net.HttpWebRequest)
            req.Method = "HEAD"
            req.Timeout = 5000
            req.AllowAutoRedirect = False
            Dim resp As System.Net.HttpWebResponse = CType(req.GetResponse(), System.Net.HttpWebResponse)
            Dim code As String = CInt(resp.StatusCode).ToString & " " & resp.StatusDescription
            resp.Close()
            Clipboard.SetText(code)
            su.Visible = True
            su.Text = "HTTP status copied: " & code
        Catch ex As System.Net.WebException
            Dim code As String = "Error: " & ex.Message.Substring(0, Math.Min(60, ex.Message.Length))
            Clipboard.SetText(code)
            su.Visible = True
            su.Text = code
        Catch ex As Exception
        End Try
    End Sub
    Sub q458(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q457(sender, e)
        End If
    End Sub
    ' --- Save Bookmark As QR Code ---
    Sub q459(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim pick As String = InputBox("Enter bookmark number to open as QR code (1-" & My.Settings.bm.Count & "):", "Save Bookmark As QR Code", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= My.Settings.bm.Count Then
                su.Visible = True
                su.Text = "Number out of range"
                Exit Sub
            End If
            Dim url As String = My.Settings.bm.Item(idx)
            Dim encoded As String = Uri.EscapeDataString(url)
            q3("https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=" & encoded)
            su.Visible = True
            su.Text = "Bookmark QR code opened in new tab"
        Catch ex As Exception
        End Try
    End Sub
    Sub q460(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q459(sender, e)
        End If
    End Sub
    ' --- Find Longest Line On Page ---
    Sub q461(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            If raw = "" Then
                su.Visible = True
                su.Text = "No text found on page"
                Exit Sub
            End If
            Dim lines() As String = raw.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim longest As String = ""
            For Each ln As String In lines
                If ln.Trim.Length > longest.Length Then
                    longest = ln.Trim
                End If
            Next
            Clipboard.SetText(longest)
            su.Visible = True
            su.Text = "Longest line copied (" & longest.Length.ToString & " chars)"
        Catch ex As Exception
        End Try
    End Sub
    Sub q462(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q461(sender, e)
        End If
    End Sub
    ' --- Copy Page Reading Time ---
    Sub q463(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            Dim wordCount As Integer = 0
            If raw <> "" Then
                Dim words() As String = raw.Split(New Char() {" "c, vbTab(0), vbCr(0), vbLf(0)}, StringSplitOptions.RemoveEmptyEntries)
                wordCount = words.Length
            End If
            Dim minutes As Integer = CInt(Math.Ceiling(wordCount / 200.0))
            Dim result As String = wordCount.ToString & " words — ~" & minutes.ToString & " min read"
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q464(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q463(sender, e)
        End If
    End Sub
    ' --- Open Site:Search For Domain ---
    Sub q465(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim host As String = wb.Url.Host
            q3(My.Settings.se & Uri.EscapeDataString("site:" & host))
            su.Visible = True
            su.Text = "Site search opened for: " & host
        Catch ex As Exception
        End Try
    End Sub
    Sub q466(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q465(sender, e)
        End If
    End Sub
    ' --- Save Bookmark List As HTML ---
    Sub q467(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks to save"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Bookmarks.html"
            SX.Filter = "HTML File [*.html*]|*.html|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Bookmark List As HTML"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("<!DOCTYPE html>")
            sb.AppendLine("<html><head><meta charset=""utf-8"">")
            sb.AppendLine("<title>Firespider Bookmarks</title>")
            sb.AppendLine("<style>body{font-family:Arial,sans-serif;max-width:900px;margin:40px auto;line-height:1.8;background:#fff;color:#222;}a{color:#0066cc;}li{margin:4px 0;}</style>")
            sb.AppendLine("</head><body>")
            sb.AppendLine("<h1>Firespider Bookmarks</h1>")
            sb.AppendLine("<p><em>Exported: " & Now.ToString("yyyy-MM-dd HH:mm:ss") & " &mdash; " & My.Settings.bm.Count.ToString & " bookmark(s)</em></p>")
            sb.AppendLine("<ol>")
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim url As String = My.Settings.bm.Item(i)
                If url <> "" Then
                    Dim escaped As String = url.Replace("&", "&amp;").Replace("""", "&quot;")
                    sb.AppendLine("  <li><a href=""" & escaped & """>" & escaped & "</a></li>")
                End If
            Next i
            sb.AppendLine("</ol>")
            sb.AppendLine("</body></html>")
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Bookmark list saved as HTML"
        Catch ex As Exception
        End Try
    End Sub
    Sub q468(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q467(sender, e)
        End If
    End Sub
    ' --- Clone Tab To Background ---
    Sub q469(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim url As String = wb.Url.ToString
            Dim currentIdx As Integer = k.SelectedIndex
            Dim B As New WebBrowser
            k.TabPages.Add("Loading...")
            Dim newIdx As Integer = k.TabPages.Count - 1
            k.TabPages(newIdx).Controls.Add(B)
            B.Dock = DockStyle.Fill
            AddHandler B.Navigating, AddressOf nn
            AddHandler B.Navigated, AddressOf nv
            AddHandler B.DocumentCompleted, AddressOf n0
            AddHandler B.ProgressChanged, AddressOf q14
        AddHandler B.NewWindow, AddressOf qpopup
            I = I + 1
            Dim tz As Size = k.ItemSize
            If I <= 20 Then tz.Width -= 6
            k.ItemSize = tz
            B.Navigate(url)
            k.SelectedIndex = currentIdx
            su.Visible = True
            su.Text = "Tab cloned to background"
        Catch ex As Exception
        End Try
    End Sub
    Sub q470(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q469(sender, e)
        End If
    End Sub
    ' --- Copy All Tab URLs As Vertical List ---
    Sub q471(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim sb As New System.Text.StringBuilder
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wbq As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wbq.Url IsNot Nothing Then
                        sb.AppendLine(wbq.Url.ToString)
                    End If
                End If
            Next q
            If sb.Length = 0 Then
                su.Visible = True
                su.Text = "No tab URLs to copy"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "All tab URLs copied as vertical list"
        Catch ex As Exception
        End Try
    End Sub
    Sub q472(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q471(sender, e)
        End If
    End Sub
    ' --- Show Page Viewport Size ---
    Sub q473(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As Object = wb.Document.InvokeScript("eval",
                New Object() {"window.innerWidth + 'x' + window.innerHeight;"})
            Dim result As String = If(raw IsNot Nothing, raw.ToString, wb.Width.ToString & "x" & wb.Height.ToString)
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = "Viewport size copied: " & result
        Catch ex As Exception
        End Try
    End Sub
    Sub q474(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q473(sender, e)
        End If
    End Sub
    ' --- Replace Text In Quick Notes ---
    Sub q475(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim findTerm As String = InputBox("Find text:", "Replace In Quick Notes", "")
            If findTerm = "" Then Exit Sub
            Dim replaceTerm As String = InputBox("Replace with:", "Replace In Quick Notes", "")
            Dim count As Integer = 0
            Dim original As String = My.Settings.qn
            Dim updated As String = original
            Do While updated.ToLower.Contains(findTerm.ToLower)
                Dim idx As Integer = updated.ToLower.IndexOf(findTerm.ToLower)
                updated = updated.Substring(0, idx) & replaceTerm & updated.Substring(idx + findTerm.Length)
                count += 1
            Loop
            If count = 0 Then
                su.Visible = True
                su.Text = "Text not found in quick notes"
                Exit Sub
            End If
            My.Settings.qn = updated
            My.Settings.Save()
            su.Visible = True
            su.Text = count.ToString & " replacement(s) made in quick notes"
        Catch ex As Exception
        End Try
    End Sub
    Sub q476(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q475(sender, e)
        End If
    End Sub
    ' --- Jump To Next Tab With Same Domain ---
    Sub q477(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaDomainAction("nx")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q478(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q477(sender, e)
        End If
    End Sub
    ' --- Dark Mode Toggle ---
    Sub q479(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim script As String =
                "(function(){" &
                "var h=document.documentElement;" &
                "if(h.getAttribute('data-fsdark')){" &
                "h.style.filter='';" &
                "h.removeAttribute('data-fsdark');" &
                "}else{" &
                "h.style.filter='invert(1) hue-rotate(180deg)';" &
                "var imgs=document.querySelectorAll('img,video,canvas');" &
                "for(var i=0;i<imgs.length;i++)imgs[i].style.filter='invert(1) hue-rotate(180deg)';" &
                "h.setAttribute('data-fsdark','1');" &
                "}" &
                "})();"
            wb.Document.InvokeScript("eval", New Object() {script})
            su.Visible = True
            su.Text = "Dark mode toggled"
        Catch ex As Exception
        End Try
    End Sub
    Sub q480(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q479(sender, e)
        End If
    End Sub
    ' --- Show Page Script Count ---
    Sub q481(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim scripts As Integer = wb.Document.GetElementsByTagName("script").Count
            Dim styles As Integer = wb.Document.GetElementsByTagName("link").Count +
                                    wb.Document.GetElementsByTagName("style").Count
            Dim iframes As Integer = wb.Document.GetElementsByTagName("iframe").Count
            Dim result As String = "Scripts: " & scripts.ToString &
                                   "  Stylesheets: " & styles.ToString &
                                   "  iframes: " & iframes.ToString
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q482(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q481(sender, e)
        End If
    End Sub
    ' --- Strip Numbers From Page Text ---
    Sub q483(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            If raw = "" Then
                su.Visible = True
                su.Text = "No text found on page"
                Exit Sub
            End If
            Dim rx As New System.Text.RegularExpressions.Regex("\d+")
            Dim stripped As String = rx.Replace(raw, "").Trim
            Clipboard.SetText(stripped)
            su.Visible = True
            su.Text = "Page text copied with numbers removed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q484(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q483(sender, e)
        End If
    End Sub
    ' --- Open Page In Hex Viewer ---
    Sub q485(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            q3("https://hexed.it/?url=" & Uri.EscapeDataString(wb.Url.ToString))
            su.Visible = True
            su.Text = "Opening page in hex viewer"
        Catch ex As Exception
        End Try
    End Sub
    Sub q486(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q485(sender, e)
        End If
    End Sub
    ' --- Bookmark Tab History Item ---
    Sub q487(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim pick As String = InputBox("Enter history item number to bookmark (1-" & f6.l0.Items.Count & "):", "Bookmark History Item", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= f6.l0.Items.Count Then
                su.Visible = True
                su.Text = "Number out of range"
                Exit Sub
            End If
            Dim url As String = f6.l0.Items(idx).ToString
            Dim fv As New ToolStripMenuItem
            fv.Text = url.Substring(0, Math.Min(60, url.Length))
            fv.Tag = url
            fv.Image = My.Resources.F9
            fv.BackColor = Color.White
            fv.ForeColor = Color.Black
            fv.Padding = New Padding(0)
            AddHandler fv.Click, AddressOf z2
            AddHandler fv.MouseHover, AddressOf z2
            RB.DropDownItems.Add(fv)
            My.Settings.bm.Add(url)
            My.Settings.Save()
            su.Visible = True
            su.Text = "History item bookmarked"
        Catch ex As Exception
        End Try
    End Sub
    Sub q488(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q487(sender, e)
        End If
    End Sub
    ' --- Copy Page Style Sheet Links ---
    Sub q489(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("link")
                Dim rel As String = el.GetAttribute("rel")
                If rel IsNot Nothing AndAlso rel.ToLower.Contains("stylesheet") Then
                    Dim href As String = el.GetAttribute("href")
                    If href <> "" Then sb.AppendLine(href)
                End If
            Next
            If sb.Length = 0 Then
                su.Visible = True
                su.Text = "No stylesheets found on page"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Stylesheet links copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q490(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q489(sender, e)
        End If
    End Sub
    ' --- Save Page Word Frequency ---
    Sub q491(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            If raw = "" Then
                su.Visible = True
                su.Text = "No text found on page"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Word Frequency.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page Word Frequency"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim freq As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Dim rx As New System.Text.RegularExpressions.Regex("[a-zA-Z]+")
            For Each m As System.Text.RegularExpressions.Match In rx.Matches(raw)
                Dim w As String = m.Value.ToLower
                If freq.ContainsKey(w) Then
                    freq(w) += 1
                Else
                    freq.Add(w, 1)
                End If
            Next
            Dim sorted As New List(Of KeyValuePair(Of String, Integer))(freq)
            sorted.Sort(Function(a, b) b.Value.CompareTo(a.Value))
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine("Word Frequency - " & wb.DocumentTitle)
            mySW.WriteLine(New String("="c, 40))
            mySW.WriteLine("")
            For Each kvp As KeyValuePair(Of String, Integer) In sorted
                mySW.WriteLine(kvp.Value.ToString.PadLeft(6) & "  " & kvp.Key)
            Next
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = freq.Count.ToString & " unique words saved"
        Catch ex As Exception
        End Try
    End Sub
    Sub q492(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q491(sender, e)
        End If
    End Sub
    ' --- Tile All Tabs In New Window ---
    Sub q493(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.TabPages.Count = 0 Then Exit Sub
            Dim urls As New List(Of String)
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wbq As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wbq.Url IsNot Nothing Then urls.Add(wbq.Url.ToString)
                End If
            Next q
            If urls.Count = 0 Then
                su.Visible = True
                su.Text = "No URLs to tile"
                Exit Sub
            End If
            Dim nw As New j
            nw.Show()
            For i As Integer = 1 To urls.Count - 1
                nw.q3(urls(i))
            Next i
            If urls.Count > 0 Then
                CType(nw.k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(urls(0))
            End If
            su.Visible = True
            su.Text = urls.Count.ToString & " tab(s) opened in new window"
        Catch ex As Exception
        End Try
    End Sub
    Sub q494(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q493(sender, e)
        End If
    End Sub
    ' --- Show Quick Notes Line Count ---
    Sub q495(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim lines() As String = My.Settings.qn.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim words() As String = My.Settings.qn.Split(New Char() {" "c, vbTab(0), vbCr(0), vbLf(0)}, StringSplitOptions.RemoveEmptyEntries)
            Dim result As String = "Lines: " & lines.Length.ToString &
                                   "  Words: " & words.Length.ToString &
                                   "  Chars: " & My.Settings.qn.Length.ToString
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q496(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q495(sender, e)
        End If
    End Sub
    ' --- Open Page Video Links In Tabs ---
    Sub q497(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim count As Integer = 0
            Dim videoExts() As String = {".mp4", ".webm", ".ogg", ".avi", ".mov", ".mkv"}
            Dim videoHosts() As String = {"youtube.com", "youtu.be", "vimeo.com", "dailymotion.com", "twitch.tv"}
            For Each el As HtmlElement In wb.Document.Links
                Dim href As String = el.GetAttribute("href").ToLower
                Dim isVideo As Boolean = False
                For Each ext As String In videoExts
                    If href.EndsWith(ext) Then isVideo = True : Exit For
                Next
                If Not isVideo Then
                    For Each host As String In videoHosts
                        If href.Contains(host) Then isVideo = True : Exit For
                    Next
                End If
                If isVideo Then
                    q3(el.GetAttribute("href"))
                    count += 1
                End If
            Next
            If count = 0 Then
                su.Visible = True
                su.Text = "No video links found on page"
            Else
                su.Visible = True
                su.Text = count.ToString & " video link(s) opened"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q498(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q497(sender, e)
        End If
    End Sub
    ' --- Save Page As Numbered List ---
    Sub q499(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            If raw = "" Then
                su.Visible = True
                su.Text = "No text found on page"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = wb.DocumentTitle & " List.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page As Numbered List"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim lines() As String = raw.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim mySW As New System.IO.StreamWriter(SX.FileName, False)
            mySW.WriteLine(wb.DocumentTitle)
            mySW.WriteLine(New String("="c, 40))
            mySW.WriteLine("")
            Dim n As Integer = 1
            For Each ln As String In lines
                Dim t As String = ln.Trim
                If t <> "" Then
                    mySW.WriteLine(n.ToString & ". " & t)
                    n += 1
                End If
            Next
            mySW.Flush()
            mySW.Close()
            su.Visible = True
            su.Text = (n - 1).ToString & " lines saved as numbered list"
        Catch ex As Exception
        End Try
    End Sub
    Sub q500(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q499(sender, e)
        End If
    End Sub
    ' --- Show Page Zoom As Percentage ---
    Sub q501(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim zv As String = "100%"
            Try
                Dim raw As Object = wb.Document.InvokeScript("eval",
                    New Object() {"(function(){var s=document.body.style.cssText;" &
                    "var m=s.match(/zoom:([^;]+)/);return m?m[1]:'100%';})();"})
                If raw IsNot Nothing Then zv = raw.ToString.Trim
            Catch
            End Try
            Clipboard.SetText(zv)
            su.Visible = True
            su.Text = "Page zoom: " & zv
        Catch ex As Exception
        End Try
    End Sub
    Sub q502(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q501(sender, e)
        End If
    End Sub
    ' --- Show History Item Count ---
    Sub q503(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim count As Integer = f6.l0.Items.Count
            Dim result As String = "History items: " & count.ToString
            Clipboard.SetText(count.ToString)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q504(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q503(sender, e)
        End If
    End Sub
    ' --- New Tab Start URL helper ---
    Function qntu() As String
        If My.Settings.dashboard = "e" Then
            Return qdashboardurl()
        ElseIf My.Settings.nf <> "" Then
            Return My.Settings.nf
        Else
            Dim home As String = If(My.Settings.h, "").Trim()
            If home = "" OrElse String.Equals(home, "http://fsin.glitch.me/", StringComparison.OrdinalIgnoreCase) Then
                Return qleaStart()
            End If
            Return home
        End If
    End Function
    Dim nt As Integer = -1119769811
    ' --- Copy Page Link Titles ---
    Sub q505(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            For Each el As HtmlElement In wb.Document.Links
                Dim title As String = el.GetAttribute("title")
                Dim inner As String = If(el.InnerText IsNot Nothing, el.InnerText.Trim, "")
                Dim href As String = el.GetAttribute("href")
                Dim lbl As String = If(title <> "", title, inner)
                If lbl <> "" AndAlso href <> "" Then
                    sb.AppendLine(lbl & vbTab & href)
                End If
            Next
            If sb.Length = 0 Then
                su.Visible = True
                su.Text = "No titled links found on page"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Page link titles copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q506(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q505(sender, e)
        End If
    End Sub
    ' --- Set Custom New Tab URL ---
    Sub q507(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim current As String = If(My.Settings.nf = "", My.Settings.h, My.Settings.nf)
            Dim url As String = InputBox("Enter URL to open in every new tab:" & vbCrLf & "(Leave blank to use the homepage)", "Set Custom New Tab URL", current)
            If url Is Nothing Then Exit Sub
            My.Settings.nf = url.Trim
            My.Settings.Save()
            su.Visible = True
            If My.Settings.nf = "" Then
                su.Text = "New tab URL reset to homepage"
            Else
                su.Text = "New tab URL set to: " & My.Settings.nf.Substring(0, Math.Min(60, My.Settings.nf.Length))
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q508(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q507(sender, e)
        End If
    End Sub
    ' --- Copy Page Table Data ---
    Sub q509(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim tables As HtmlElementCollection = wb.Document.GetElementsByTagName("table")
            If tables.Count = 0 Then
                su.Visible = True
                su.Text = "No tables found on page"
                Exit Sub
            End If
            Dim sb As New System.Text.StringBuilder
            Dim tIdx As Integer = 0
            For Each tbl As HtmlElement In tables
                tIdx += 1
                sb.AppendLine("Table " & tIdx.ToString)
                sb.AppendLine(New String("-"c, 30))
                For Each row As HtmlElement In tbl.GetElementsByTagName("tr")
                    Dim cells As New List(Of String)
                    For Each cell As HtmlElement In row.GetElementsByTagName("th")
                        Dim txt As String = If(cell.InnerText IsNot Nothing, cell.InnerText.Trim, "")
                        cells.Add("[" & txt & "]")
                    Next
                    For Each cell As HtmlElement In row.GetElementsByTagName("td")
                        Dim txt As String = If(cell.InnerText IsNot Nothing, cell.InnerText.Trim, "")
                        cells.Add(txt)
                    Next
                    If cells.Count > 0 Then sb.AppendLine(String.Join(vbTab, cells))
                Next
                sb.AppendLine("")
            Next
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = tables.Count.ToString & " table(s) copied as text"
        Catch ex As Exception
        End Try
    End Sub
    Sub q510(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q509(sender, e)
        End If
    End Sub
    ' --- Find Text On Page And Count ---
    Sub q511(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim term As String = InputBox("Enter text to find and count on this page:", "Find Text On Page", "")
            If term = "" Then Exit Sub
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim body As String = ""
            Try
                body = wb.Document.Body.InnerText
            Catch
            End Try
            If body = "" Then
                su.Visible = True
                su.Text = "No page text to search"
                Exit Sub
            End If
            Dim count As Integer = 0
            Dim idx As Integer = 0
            Do
                idx = body.ToLower.IndexOf(term.ToLower, idx)
                If idx = -1 Then Exit Do
                count += 1
                idx += 1
            Loop
            Dim escaped As String = term.Replace("'", "\'")
            Dim script As String =
                "(function(){" &
                "var body=document.body.innerHTML;" &
                "body=body.replace(/<mark[^>]*data-fsfc[^>]*>/gi,'').replace(/<\/mark>/gi,'');" &
                "var re=new RegExp('(" & escaped & ")','gi');" &
                "document.body.innerHTML=body.replace(re,'<mark style=""background:#ffe066;color:#000"" data-fsfc=""1"">$1</mark>');" &
                "})();"
            Try
                wb.Document.InvokeScript("eval", New Object() {script})
            Catch
            End Try
            Dim result As String = """" & term & """ found " & count.ToString & " time(s) on page"
            Clipboard.SetText(count.ToString)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q512(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q511(sender, e)
        End If
    End Sub
    ' --- Set Custom Window Title ---
    Sub q513(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim current As String = If(My.Settings.th = "", ProductName, My.Settings.th)
            Dim title As String = InputBox("Enter custom window title prefix:" & vbCrLf & "(Leave blank to restore default)", "Set Custom Window Title", current)
            If title Is Nothing Then Exit Sub
            My.Settings.th = title.Trim
            My.Settings.Save()
            If My.Settings.th = "" Then
                Text = ProductName & " - " & k.SelectedTab.Text
                su.Visible = True
                su.Text = "Window title restored to default"
            Else
                Text = My.Settings.th & " - " & k.SelectedTab.Text
                su.Visible = True
                su.Text = "Window title set to: " & My.Settings.th
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q514(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q513(sender, e)
        End If
    End Sub
    ' --- Copy Page Protocol ---
    Sub q515(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim protocol As String = wb.Url.Scheme
            Clipboard.SetText(protocol)
            su.Visible = True
            su.Text = "Protocol copied: " & protocol
        Catch ex As Exception
        End Try
    End Sub
    Sub q516(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q515(sender, e)
        End If
    End Sub
    ' --- Open All Bookmarks In New Window ---
    Sub q517(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim valid As New List(Of String)
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim u As String = My.Settings.bm.Item(i)
                If u <> "" Then valid.Add(u)
            Next i
            If valid.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim confirm As MsgBoxResult = MsgBox("Open all " & valid.Count & " bookmark(s) in a new window?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Open All Bookmarks In New Window")
            If confirm <> MsgBoxResult.Yes Then Exit Sub
            Dim nw As New j
            nw.Show()
            CType(nw.k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(valid(0))
            For i As Integer = 1 To valid.Count - 1
                nw.q3(valid(i))
            Next i
            su.Visible = True
            su.Text = valid.Count.ToString & " bookmark(s) opened in new window"
        Catch ex As Exception
        End Try
    End Sub
    Sub q518(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q517(sender, e)
        End If
    End Sub
    ' --- Show Page Load Time ---
    Sub q519(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As Object = Nothing
            Try
                raw = wb.Document.InvokeScript("eval",
                    New Object() {
                        "(function(){" &
                        "var t=window.performance&&window.performance.timing;" &
                        "if(!t)return 'Not available';" &
                        "var load=t.loadEventEnd-t.navigationStart;" &
                        "var dom=t.domContentLoadedEventEnd-t.navigationStart;" &
                        "return 'Load: '+load+'ms | DOM ready: '+dom+'ms';" &
                        "})();"})
            Catch
            End Try
            Dim result As String = If(raw IsNot Nothing, raw.ToString, "Timing not available")
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q520(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q519(sender, e)
        End If
    End Sub
    ' --- Open URL From Selected Text ---
    Sub q521(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sel As String = ""
            Try
                Dim raw As Object = wb.Document.InvokeScript("eval",
                    New Object() {"window.getSelection().toString();"})
                If raw IsNot Nothing Then sel = raw.ToString.Trim
            Catch
            End Try
            If sel = "" Then
                su.Visible = True
                su.Text = "No text selected on page"
                Exit Sub
            End If
            Dim url As String = sel
            If Not url.StartsWith("http") AndAlso Not url.StartsWith("file:") AndAlso Not url.StartsWith("about:") Then
                If url.Contains(".") AndAlso Not url.Contains(" ") Then
                    url = "http://" & url
                Else
                    url = My.Settings.se & Uri.EscapeDataString(url)
                End If
            End If
            q3(url)
            su.Visible = True
            su.Text = "Opening: " & sel.Substring(0, Math.Min(60, sel.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q522(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q521(sender, e)
        End If
    End Sub
    ' --- Copy Page Path ---
    Sub q523(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim path As String = wb.Url.AbsolutePath
            Clipboard.SetText(path)
            su.Visible = True
            su.Text = "Page path copied: " & path.Substring(0, Math.Min(60, path.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q524(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q523(sender, e)
        End If
    End Sub
    ' --- Copy Page Query String ---
    Sub q525(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim qs As String = wb.Url.Query
            If qs = "" Then
                su.Visible = True
                su.Text = "No query string in current URL"
                Exit Sub
            End If
            Clipboard.SetText(qs)
            su.Visible = True
            su.Text = "Query string copied: " & qs.Substring(0, Math.Min(60, qs.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q526(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q525(sender, e)
        End If
    End Sub
    ' --- Open Page In New Tab (Force Reload) ---
    Sub q527(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim url As String = wb.Url.ToString
            q3(url)
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Refresh(WebBrowserRefreshOption.Completely)
            su.Visible = True
            su.Text = "Page opened in new tab with force reload"
        Catch ex As Exception
        End Try
    End Sub
    Sub q528(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q527(sender, e)
        End If
    End Sub
    ' --- Copy Page URL Without Query ---
    Sub q529(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim u As Uri = wb.Url
            Dim clean As String = u.Scheme & "://" & u.Host & u.AbsolutePath
            Clipboard.SetText(clean)
            su.Visible = True
            su.Text = "URL without query copied: " & clean.Substring(0, Math.Min(70, clean.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q530(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q529(sender, e)
        End If
    End Sub
    ' --- Save Bookmark As Markdown Link ---
    Sub q531(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim items() As String = New String(Math.Min(My.Settings.bm.Count, 15) - 1) {}
            For i As Integer = 0 To items.Length - 1
                items(i) = (i + 1).ToString & ". " & My.Settings.bm.Item(i)
            Next i
            Dim pick As String = InputBox("Enter bookmark number to copy as Markdown link (1-" & My.Settings.bm.Count & "):" & vbCrLf & String.Join(vbCrLf, items), "Save Bookmark As Markdown Link", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= My.Settings.bm.Count Then
                su.Visible = True
                su.Text = "Number out of range"
                Exit Sub
            End If
            Dim url As String = My.Settings.bm.Item(idx)
            Dim md As String = "[" & qmk(url) & "](" & url & ")"
            Clipboard.SetText(md)
            su.Visible = True
            su.Text = "Bookmark copied as Markdown link"
        Catch ex As Exception
        End Try
    End Sub
    Sub q532(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q531(sender, e)
        End If
    End Sub
    ' --- Copy All History As Markdown ---
    Sub q533(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim sb As New System.Text.StringBuilder
            For i As Integer = 0 To f6.l0.Items.Count - 1
                Dim url As String = f6.l0.Items(i).ToString
                If url <> "" Then
                    sb.AppendLine("- [" & qmk(url) & "](" & url & ")")
                End If
            Next i
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = f6.l0.Items.Count.ToString & " history item(s) copied as Markdown"
        Catch ex As Exception
        End Try
    End Sub
    Sub q534(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q533(sender, e)
        End If
    End Sub
    ' --- Show Page Broken Links ---
    Sub q535(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim links As ArrayList = ql()
            If links.Count = 0 Then
                su.Visible = True
                su.Text = "No links found on page"
                Exit Sub
            End If
            Dim broken As New System.Text.StringBuilder
            Dim count As Integer = 0
            For Each href As String In links
                Try
                    Dim req As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(href), System.Net.HttpWebRequest)
                    req.Method = "HEAD"
                    req.Timeout = 3000
                    req.AllowAutoRedirect = True
                    Dim resp As System.Net.HttpWebResponse = CType(req.GetResponse(), System.Net.HttpWebResponse)
                    Dim code As Integer = CInt(resp.StatusCode)
                    resp.Close()
                    If code >= 400 Then
                        broken.AppendLine(code.ToString & " - " & href)
                        count += 1
                    End If
                Catch ex2 As System.Net.WebException
                    broken.AppendLine("ERR - " & href)
                    count += 1
                Catch
                End Try
            Next
            If count = 0 Then
                su.Visible = True
                su.Text = "No broken links found (" & links.Count.ToString & " checked)"
            Else
                Clipboard.SetText(broken.ToString.TrimEnd)
                MsgBox(broken.ToString, MsgBoxStyle.Information, count.ToString & " Broken Link(s) Found — Copied To Clipboard")
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q536(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q535(sender, e)
        End If
    End Sub
    ' --- Open Search In New Tab ---
    Sub q537(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim term As String = InputBox("Enter search term to open in a new tab:", "Open Search In New Tab", "")
            If term = "" Then Exit Sub
            q3(My.Settings.se & Uri.EscapeDataString(term))
            su.Visible = True
            su.Text = "Search opened in new tab: " & term.Substring(0, Math.Min(50, term.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q538(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q537(sender, e)
        End If
    End Sub
    ' --- Rename Selected Bookmark ---
    Sub q539(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim items() As String = New String(Math.Min(My.Settings.bm.Count, 15) - 1) {}
            For i As Integer = 0 To items.Length - 1
                items(i) = (i + 1).ToString & ". " & My.Settings.bm.Item(i)
            Next i
            Dim pick As String = InputBox("Enter bookmark number to rename (1-" & My.Settings.bm.Count & "):" & vbCrLf & String.Join(vbCrLf, items), "Rename Bookmark", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= My.Settings.bm.Count Then
                su.Visible = True
                su.Text = "Number out of range"
                Exit Sub
            End If
            Dim oldUrl As String = My.Settings.bm.Item(idx)
            Dim newUrl As String = InputBox("Enter new URL for this bookmark:", "Rename Bookmark", oldUrl)
            If newUrl = "" OrElse newUrl = oldUrl Then Exit Sub
            My.Settings.bm.Item(idx) = newUrl
            My.Settings.Save()
            For i As Integer = 0 To RB.DropDownItems.Count - 1
                Dim mi As ToolStripMenuItem = TryCast(RB.DropDownItems(i), ToolStripMenuItem)
                If mi IsNot Nothing AndAlso mi.Tag IsNot Nothing AndAlso mi.Tag.ToString = oldUrl Then
                    mi.Text = newUrl.Substring(0, Math.Min(60, newUrl.Length))
                    mi.Tag = newUrl
                    Exit For
                End If
            Next i
            su.Visible = True
            su.Text = "Bookmark updated"
        Catch ex As Exception
        End Try
    End Sub
    Sub q540(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q539(sender, e)
        End If
    End Sub
    ' --- Save Page Links As HTML ---
    Sub q541(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim links As ArrayList = ql()
            If links.Count = 0 Then
                su.Visible = True
                su.Text = "No links found on page"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = wb.DocumentTitle & " Links.html"
            SX.Filter = "HTML File [*.html*]|*.html|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page Links As HTML"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("<!DOCTYPE html>")
            sb.AppendLine("<html><head><meta charset=""utf-8"">")
            sb.AppendLine("<title>Links — " & qh(wb.DocumentTitle) & "</title>")
            sb.AppendLine("<style>body{font-family:Arial,sans-serif;max-width:900px;margin:40px auto;line-height:1.8;}a{color:#0055cc;}li{margin:3px 0;}</style>")
            sb.AppendLine("</head><body>")
            sb.AppendLine("<h1>Links on " & qh(wb.DocumentTitle) & "</h1>")
            sb.AppendLine("<p><em>Source: <a href=""" & qh(wb.Url.ToString) & """>" & qh(wb.Url.ToString) & "</a> &mdash; " & links.Count.ToString & " link(s)</em></p>")
            sb.AppendLine("<ol>")
            For Each href As String In links
                sb.AppendLine("  <li><a href=""" & qh(href) & """>" & qh(href) & "</a></li>")
            Next
            sb.AppendLine("</ol>")
            sb.AppendLine("</body></html>")
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = links.Count.ToString & " page link(s) saved as HTML"
        Catch ex As Exception
        End Try
    End Sub
    Sub q542(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q541(sender, e)
        End If
    End Sub
    ' --- Count Open Tabs By Domain ---
    Sub q543(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim domCount As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wb.Url IsNot Nothing Then
                        Dim host As String = wb.Url.Host
                        If domCount.ContainsKey(host) Then
                            domCount(host) += 1
                        Else
                            domCount.Add(host, 1)
                        End If
                    End If
                End If
            Next q
            If domCount.Count = 0 Then
                su.Visible = True
                su.Text = "No domains found in open tabs"
                Exit Sub
            End If
            Dim sorted As New List(Of KeyValuePair(Of String, Integer))(domCount)
            sorted.Sort(Function(a, b) b.Value.CompareTo(a.Value))
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("Open tabs by domain (" & k.TabPages.Count.ToString & " total):")
            sb.AppendLine("")
            For Each kvp As KeyValuePair(Of String, Integer) In sorted
                sb.AppendLine(kvp.Value.ToString.PadLeft(4) & "  " & kvp.Key)
            Next
            Clipboard.SetText(sb.ToString.TrimEnd)
            MsgBox(sb.ToString, MsgBoxStyle.Information, "Tabs By Domain — Copied To Clipboard")
        Catch ex As Exception
        End Try
    End Sub
    Sub q544(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q543(sender, e)
        End If
    End Sub
    ' --- Copy Browser Version Info ---
    Sub q545(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim ua As String = ""
            Try
                Dim raw As Object = wb.Document.InvokeScript("eval",
                    New Object() {"navigator.userAgent;"})
                If raw IsNot Nothing Then ua = raw.ToString
            Catch
            End Try
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("Firespider Internet " & Application.ProductVersion)
            sb.AppendLine("WebBrowser Control (IE " & wb.Version.ToString & ")")
            sb.AppendLine("OS: " & Environment.OSVersion.ToString)
            sb.AppendLine(".NET: " & Environment.Version.ToString)
            sb.AppendLine("Platform: " & Environment.OSVersion.Platform.ToString)
            If ua <> "" Then sb.AppendLine("UserAgent: " & ua)
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Browser version info copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q546(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q545(sender, e)
        End If
    End Sub
    ' --- Open History Item By Number ---
    Sub q547(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim pick As String = InputBox("Enter history item number to open (1-" & f6.l0.Items.Count & "):", "Open History Item By Number", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= f6.l0.Items.Count Then
                su.Visible = True
                su.Text = "Number out of range"
                Exit Sub
            End If
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(f6.l0.Items(idx).ToString)
            su.Visible = True
            su.Text = "Navigating to history item " & (idx + 1).ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q548(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q547(sender, e)
        End If
    End Sub
    ' --- Copy Tab Titles As Bullet List ---
    Sub q549(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim sb As New System.Text.StringBuilder
            For q As Integer = 0 To k.TabPages.Count - 1
                Dim title As String = k.TabPages(q).Text
                If title = "" Then title = "New Tab"
                sb.AppendLine("• " & title)
            Next q
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = k.TabPages.Count.ToString & " tab title(s) copied as bullet list"
        Catch ex As Exception
        End Try
    End Sub
    Sub q550(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q549(sender, e)
        End If
    End Sub
    ' --- Insert Address Bar Text Into Quick Notes ---
    Sub q551(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim txt As String = c.Text.Trim
            If txt = "" Then
                su.Visible = True
                su.Text = "Address bar is empty"
                Exit Sub
            End If
            Dim stamp As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
            Dim sep As String = If(My.Settings.qn = "", "", vbCrLf & "---" & vbCrLf)
            My.Settings.qn = My.Settings.qn & sep & "[" & stamp & "] " & txt
            My.Settings.Save()
            su.Visible = True
            su.Text = "Address bar text inserted into quick notes"
        Catch ex As Exception
        End Try
    End Sub
    Sub q552(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q551(sender, e)
        End If
    End Sub
    ' --- Copy Clean URL ---
    Sub q553(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim trackers() As String = {
                "utm_source", "utm_medium", "utm_campaign", "utm_term", "utm_content",
                "utm_id", "utm_source_platform", "fbclid", "gclid", "gclsrc",
                "dclid", "msclkid", "mc_eid", "yclid", "twclid",
                "ref", "referrer", "source", "_ga", "_gl"
            }
            Dim u As Uri = wb.Url
            Dim qs As String = u.Query
            If qs = "" Then
                Clipboard.SetText(u.ToString)
                su.Visible = True
                su.Text = "Clean URL copied"
                Exit Sub
            End If
            Dim parts As String() = qs.TrimStart("?"c).Split("&"c)
            Dim kept As New List(Of String)
            Dim removed As Integer = 0
            For Each part As String In parts
                Dim key As String = part.Split("="c)(0).ToLower
                Dim isTracker As Boolean = False
                For Each t As String In trackers
                    If key = t.ToLower Then isTracker = True : Exit For
                Next
                If isTracker Then
                    removed += 1
                Else
                    kept.Add(part)
                End If
            Next
            Dim cleanUrl As String = u.GetLeftPart(UriPartial.Path)
            If kept.Count > 0 Then cleanUrl &= "?" & String.Join("&", kept)
            If u.Fragment <> "" Then cleanUrl &= u.Fragment
            Clipboard.SetText(cleanUrl)
            su.Visible = True
            su.Text = removed.ToString & " tracking param(s) stripped — clean URL copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q554(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q553(sender, e)
        End If
    End Sub
    ' --- Copy Page Input Fields ---
    Sub q555(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            Dim count As Integer = 0
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("input")
                Dim itype As String = If(el.GetAttribute("type") <> "", el.GetAttribute("type"), "text")
                Dim iname As String = If(el.GetAttribute("name") <> "", el.GetAttribute("name"), "(unnamed)")
                Dim iid As String = If(el.GetAttribute("id") <> "", el.GetAttribute("id"), "(no id)")
                Dim iph As String = If(el.GetAttribute("placeholder") <> "", el.GetAttribute("placeholder"), "")
                sb.AppendLine("type=" & itype & "  name=" & iname & "  id=" & iid & If(iph <> "", "  placeholder=" & iph, ""))
                count += 1
            Next
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("textarea")
                Dim iname As String = If(el.GetAttribute("name") <> "", el.GetAttribute("name"), "(unnamed)")
                Dim iid As String = If(el.GetAttribute("id") <> "", el.GetAttribute("id"), "(no id)")
                sb.AppendLine("type=textarea  name=" & iname & "  id=" & iid)
                count += 1
            Next
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("select")
                Dim iname As String = If(el.GetAttribute("name") <> "", el.GetAttribute("name"), "(unnamed)")
                Dim iid As String = If(el.GetAttribute("id") <> "", el.GetAttribute("id"), "(no id)")
                sb.AppendLine("type=select  name=" & iname & "  id=" & iid)
                count += 1
            Next
            If count = 0 Then
                su.Visible = True
                su.Text = "No input fields found on page"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = count.ToString & " input field(s) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q556(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q555(sender, e)
        End If
    End Sub
    ' --- Open Page In New Tab And Bookmark ---
    Sub q557(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim url As String = wb.Url.ToString
            Dim title As String = wb.DocumentTitle
            q3(url)
            My.Settings.bm.Add(url)
            Dim fv As New ToolStripMenuItem
            fv.Text = If(title <> "", title, url)
            fv.Tag = url
            fv.Image = My.Resources.F9
            fv.BackgroundImage = My.Resources.F2
            fv.BackgroundImageLayout = ImageLayout.Stretch
            fv.ForeColor = Color.White
            AddHandler fv.Click, AddressOf z2
            AddHandler fv.MouseHover, AddressOf z2
            RB.DropDownItems.Add(fv)
            My.Settings.Save()
            su.Visible = True
            su.Text = "Page opened in new tab and bookmarked"
        Catch ex As Exception
        End Try
    End Sub
    Sub q558(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q557(sender, e)
        End If
    End Sub
    ' --- Show Quick Notes Word Count ---
    Sub q559(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim words() As String = My.Settings.qn.Split(New Char() {" "c, vbTab(0), vbCr(0), vbLf(0)}, StringSplitOptions.RemoveEmptyEntries)
            Dim lines() As String = My.Settings.qn.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim result As String = "Words: " & words.Length.ToString & "  Lines: " & lines.Length.ToString & "  Chars: " & My.Settings.qn.Length.ToString
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q560(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q559(sender, e)
        End If
    End Sub
    ' --- Copy All Bookmark Domains ---
    Sub q561(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim domains As New List(Of String)
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim url As String = My.Settings.bm.Item(i)
                If url = "" Then Continue For
                Try
                    Dim host As String = New Uri(url).Host
                    If Not domains.Contains(host) Then domains.Add(host)
                Catch
                End Try
            Next i
            If domains.Count = 0 Then
                su.Visible = True
                su.Text = "No valid domains found in bookmarks"
                Exit Sub
            End If
            domains.Sort(StringComparer.OrdinalIgnoreCase)
            Clipboard.SetText(String.Join(vbCrLf, domains))
            su.Visible = True
            su.Text = domains.Count.ToString & " unique bookmark domain(s) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q562(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q561(sender, e)
        End If
    End Sub
    ' --- Save Page As Plain Text (Clean) ---
    Sub q563(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            If raw = "" Then
                su.Visible = True
                su.Text = "No text found on page"
                Exit Sub
            End If
            Dim lines() As String = raw.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim sb As New System.Text.StringBuilder
            For Each ln As String In lines
                Dim t As String = ln.Trim
                If t <> "" Then sb.AppendLine(t)
            Next
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = wb.DocumentTitle & ".txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page As Plain Text (Clean)"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            System.IO.File.WriteAllText(SX.FileName, sb.ToString.TrimEnd, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Page saved as clean plain text"
        Catch ex As Exception
        End Try
    End Sub
    Sub q564(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q563(sender, e)
        End If
    End Sub
    ' --- Open Next History Item ---
    Sub q565(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim cur As String = If(wb.Url IsNot Nothing, wb.Url.ToString, "")
            Dim idx As Integer = -1
            For i As Integer = 0 To f6.l0.Items.Count - 1
                If f6.l0.Items(i).ToString = cur Then
                    idx = i
                    Exit For
                End If
            Next i
            Dim next_idx As Integer = If(idx = -1 OrElse idx >= f6.l0.Items.Count - 1, 0, idx + 1)
            wb.Navigate(f6.l0.Items(next_idx).ToString)
            su.Visible = True
            su.Text = "Opening history item " & (next_idx + 1).ToString & " of " & f6.l0.Items.Count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q566(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q565(sender, e)
        End If
    End Sub
    ' --- Open Previous History Item ---
    Sub q567(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim cur As String = If(wb.Url IsNot Nothing, wb.Url.ToString, "")
            Dim idx As Integer = -1
            For i As Integer = 0 To f6.l0.Items.Count - 1
                If f6.l0.Items(i).ToString = cur Then
                    idx = i
                    Exit For
                End If
            Next i
            Dim prev_idx As Integer = If(idx <= 0, f6.l0.Items.Count - 1, idx - 1)
            wb.Navigate(f6.l0.Items(prev_idx).ToString)
            su.Visible = True
            su.Text = "Opening history item " & (prev_idx + 1).ToString & " of " & f6.l0.Items.Count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q568(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q567(sender, e)
        End If
    End Sub
    ' --- Copy Page As Plain Text Summary ---
    Sub q569(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("=== Page Summary ===")
            sb.AppendLine("Title:   " & wb.DocumentTitle)
            Try
                sb.AppendLine("URL:     " & wb.Url.ToString)
                sb.AppendLine("Domain:  " & wb.Url.Host)
            Catch
            End Try
            Dim links As ArrayList = ql()
            Dim imgs As ArrayList = qi()
            sb.AppendLine("Links:   " & links.Count.ToString)
            sb.AppendLine("Images:  " & imgs.Count.ToString)
            Try
                Dim bodyText As String = wb.Document.Body.InnerText
                Dim words() As String = bodyText.Split(New Char() {" "c, vbTab(0), vbCr(0), vbLf(0)}, StringSplitOptions.RemoveEmptyEntries)
                sb.AppendLine("Words:   " & words.Length.ToString)
                sb.AppendLine("Chars:   " & bodyText.Length.ToString)
                Dim mins As Integer = CInt(Math.Ceiling(words.Length / 200.0))
                sb.AppendLine("Read:    ~" & mins.ToString & " min")
            Catch
            End Try
            sb.AppendLine("Status:  " & wb.StatusText)
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Page plain text summary copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q570(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q569(sender, e)
        End If
    End Sub
    ' --- Reverse History Order ---
    Sub q571(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count < 2 Then
                su.Visible = True
                su.Text = "Not enough history items to reverse"
                Exit Sub
            End If
            Dim items As New List(Of String)
            For i As Integer = 0 To f6.l0.Items.Count - 1
                items.Add(f6.l0.Items(i).ToString)
            Next i
            items.Reverse()
            f6.l0.Items.Clear()
            c.Items.Clear()
            For Each item As String In items
                f6.l0.Items.Add(item)
                c.Items.Add(item)
            Next
            su.Visible = True
            su.Text = "History order reversed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q572(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q571(sender, e)
        End If
    End Sub
    ' --- Save Page URL History To File ---
    Sub q573(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider History " & Now.ToString("yyyy-MM-dd") & ".txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page URL History To File"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("Firespider Internet — URL History")
            sb.AppendLine("Exported: " & Now.ToString("yyyy-MM-dd HH:mm:ss"))
            sb.AppendLine("Items: " & f6.l0.Items.Count.ToString)
            sb.AppendLine(New String("="c, 40))
            sb.AppendLine("")
            For i As Integer = 0 To f6.l0.Items.Count - 1
                sb.AppendLine((i + 1).ToString.PadLeft(4) & ".  " & f6.l0.Items(i).ToString)
            Next i
            System.IO.File.WriteAllText(SX.FileName, sb.ToString.TrimEnd, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "URL history saved to file"
        Catch ex As Exception
        End Try
    End Sub
    Sub q574(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q573(sender, e)
        End If
    End Sub
    ' --- Copy Page As JSON Summary ---
    Sub q575(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim links As ArrayList = ql()
            Dim imgs As ArrayList = qi()
            Dim title As String = wb.DocumentTitle.Replace("""", "\""")
            Dim url As String = ""
            Dim host As String = ""
            Try
                url = wb.Url.ToString.Replace("""", "\""")
                host = wb.Url.Host.Replace("""", "\""")
            Catch
            End Try
            Dim wordCount As Integer = 0
            Dim charCount As Integer = 0
            Try
                Dim body As String = wb.Document.Body.InnerText
                charCount = body.Length
                wordCount = body.Split(New Char() {" "c, vbTab(0), vbCr(0), vbLf(0)}, StringSplitOptions.RemoveEmptyEntries).Length
            Catch
            End Try
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("{")
            sb.AppendLine("  ""title"": """ & title & """,")
            sb.AppendLine("  ""url"": """ & url & """,")
            sb.AppendLine("  ""domain"": """ & host & """,")
            sb.AppendLine("  ""links"": " & links.Count.ToString & ",")
            sb.AppendLine("  ""images"": " & imgs.Count.ToString & ",")
            sb.AppendLine("  ""words"": " & wordCount.ToString & ",")
            sb.AppendLine("  ""chars"": " & charCount.ToString & ",")
            sb.AppendLine("  ""status"": """ & wb.StatusText.Replace("""", "\""") & """")
            sb.AppendLine("}")
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Page JSON summary copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q576(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q575(sender, e)
        End If
    End Sub
    ' --- Bookmark History Range ---
    Sub q577(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim fromStr As String = InputBox("Bookmark history items from number:", "Bookmark History Range", "1")
            If fromStr = "" Then Exit Sub
            Dim toStr As String = InputBox("To number (max " & f6.l0.Items.Count & "):", "Bookmark History Range", f6.l0.Items.Count.ToString)
            If toStr = "" Then Exit Sub
            Dim fromIdx As Integer = 0
            Dim toIdx As Integer = 0
            If Not Integer.TryParse(fromStr, fromIdx) OrElse Not Integer.TryParse(toStr, toIdx) Then
                su.Visible = True
                su.Text = "Invalid numbers entered"
                Exit Sub
            End If
            fromIdx -= 1
            toIdx -= 1
            fromIdx = Math.Max(0, fromIdx)
            toIdx = Math.Min(f6.l0.Items.Count - 1, toIdx)
            If fromIdx > toIdx Then
                su.Visible = True
                su.Text = "Start must be less than or equal to end"
                Exit Sub
            End If
            Dim added As Integer = 0
            For i As Integer = fromIdx To toIdx
                Dim url As String = f6.l0.Items(i).ToString
                If url <> "" Then
                    My.Settings.bm.Add(url)
                    Dim fv As New ToolStripMenuItem
                    fv.Text = url.Substring(0, Math.Min(60, url.Length))
                    fv.Tag = url
                    fv.Image = My.Resources.F9
                    fv.BackgroundImage = My.Resources.F2
                    fv.BackgroundImageLayout = ImageLayout.Stretch
                    fv.ForeColor = Color.White
                    AddHandler fv.Click, AddressOf z2
                    AddHandler fv.MouseHover, AddressOf z2
                    RB.DropDownItems.Add(fv)
                    added += 1
                End If
            Next i
            My.Settings.Save()
            su.Visible = True
            su.Text = added.ToString & " history item(s) bookmarked"
        Catch ex As Exception
        End Try
    End Sub
    Sub q578(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q577(sender, e)
        End If
    End Sub
    ' --- Set Zoom Level ---
    Sub q579(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim current As String = If(My.Settings.zl <> "", My.Settings.zl, "100")
            Dim pct As String = InputBox("Enter zoom percentage (e.g. 75, 100, 125, 150):", "Set Zoom Level", current)
            If pct = "" Then Exit Sub
            Dim n As Integer = 100
            If Not Integer.TryParse(pct.Replace("%", "").Trim, n) Then
                su.Visible = True
                su.Text = "Invalid percentage entered"
                Exit Sub
            End If
            n = Math.Max(10, Math.Min(500, n))
            wb.Document.Body.Style &= ";zoom:" & n.ToString & "%"
            My.Settings.zl = n.ToString
            su.Visible = True
            su.Text = "Zoom set to " & n.ToString & "%"
        Catch ex As Exception
        End Try
    End Sub
    Sub q580(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q579(sender, e)
        End If
    End Sub
    ' --- Copy Page Anchor IDs ---
    Sub q581(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            Dim count As Integer = 0
            For Each el As HtmlElement In wb.Document.All
                Dim aid As String = el.GetAttribute("id")
                If aid <> "" Then
                    sb.AppendLine("#" & aid)
                    count += 1
                End If
            Next
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("a")
                Dim aname As String = el.GetAttribute("name")
                If aname <> "" Then
                    sb.AppendLine("#" & aname & "  (name anchor)")
                    count += 1
                End If
            Next
            If count = 0 Then
                su.Visible = True
                su.Text = "No anchor IDs found on page"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = count.ToString & " anchor ID(s) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q582(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q581(sender, e)
        End If
    End Sub
    ' --- Search Within Quick Notes ---
    Sub q583(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim term As String = InputBox("Enter search term:", "Search Within Quick Notes", "")
            If term = "" Then Exit Sub
            Dim lines() As String = My.Settings.qn.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim results As New System.Text.StringBuilder
            Dim found As Integer = 0
            For i As Integer = 0 To lines.Length - 1
                If lines(i).ToLower.Contains(term.ToLower) Then
                    results.AppendLine("Line " & (i + 1).ToString & ": " & lines(i).Trim)
                    found += 1
                End If
            Next i
            If found = 0 Then
                su.Visible = True
                su.Text = "No matches found for: " & term
            Else
                Clipboard.SetText(results.ToString.TrimEnd)
                MsgBox(results.ToString, MsgBoxStyle.Information, found.ToString & " Match(es) Found — Copied To Clipboard")
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q584(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q583(sender, e)
        End If
    End Sub
    ' --- Copy Page Meta Tags ---
    Sub q585(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            Dim count As Integer = 0
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("meta")
                Dim name As String = el.GetAttribute("name")
                Dim prop As String = el.GetAttribute("property")
                Dim httpEq As String = el.GetAttribute("http-equiv")
                Dim content As String = el.GetAttribute("content")
                Dim charset As String = el.GetAttribute("charset")
                If charset <> "" Then
                    sb.AppendLine("charset=" & charset)
                    count += 1
                ElseIf name <> "" Then
                    sb.AppendLine("name=" & name & "  content=" & content)
                    count += 1
                ElseIf prop <> "" Then
                    sb.AppendLine("property=" & prop & "  content=" & content)
                    count += 1
                ElseIf httpEq <> "" Then
                    sb.AppendLine("http-equiv=" & httpEq & "  content=" & content)
                    count += 1
                End If
            Next
            If count = 0 Then
                su.Visible = True
                su.Text = "No meta tags found on page"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = count.ToString & " meta tag(s) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q586(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q585(sender, e)
        End If
    End Sub
    ' --- Open Homepage In Default Browser ---
    Sub q587(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Process.Start(My.Settings.h)
            su.Visible = True
            su.Text = "Homepage opened in default browser"
        Catch ex As Exception
        End Try
    End Sub
    Sub q588(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q587(sender, e)
        End If
    End Sub
    ' --- Save Tab Session As JSON ---
    Sub q589(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Session " & Now.ToString("yyyy-MM-dd") & ".json"
            SX.Filter = "JSON File [*.json*]|*.json|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Tab Session As JSON"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("{")
            sb.AppendLine("  ""exported"": """ & Now.ToString("yyyy-MM-dd HH:mm:ss") & """,")
            sb.AppendLine("  ""tabCount"": " & k.TabPages.Count.ToString & ",")
            sb.AppendLine("  ""selectedTab"": " & k.SelectedIndex.ToString & ",")
            sb.AppendLine("  ""homepage"": """ & My.Settings.h.Replace("""", "\""") & """,")
            sb.AppendLine("  ""tabs"": [")
            For q As Integer = 0 To k.TabPages.Count - 1
                Dim title As String = k.TabPages(q).Text.Replace("""", "\""")
                Dim url As String = ""
                Try
                    Dim wbq As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wbq.Url IsNot Nothing Then url = wbq.Url.ToString.Replace("""", "\""")
                Catch
                End Try
                Dim comma As String = If(q < k.TabPages.Count - 1, ",", "")
                sb.AppendLine("    { ""index"": " & q.ToString & ", ""title"": """ & title & """, ""url"": """ & url & """ }" & comma)
            Next q
            sb.AppendLine("  ]")
            sb.AppendLine("}")
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Tab session saved as JSON"
        Catch ex As Exception
        End Try
    End Sub
    Sub q590(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q589(sender, e)
        End If
    End Sub
    ' --- Load Tab Session From JSON ---
    Sub q591(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ox As New OpenFileDialog()
            ox.Title = "Load Tab Session From JSON"
            ox.CheckFileExists = True
            ox.Filter = "JSON File [*.json*]|*.json|All Files [*.*]|*.*"
            ox.ShowDialog()
            If ox.FileName = "" Then Exit Sub
            Dim raw As String = System.IO.File.ReadAllText(ox.FileName, System.Text.Encoding.UTF8)
            Dim rx As New System.Text.RegularExpressions.Regex("""url""\s*:\s*""([^""]+)""")
            Dim matches As System.Text.RegularExpressions.MatchCollection = rx.Matches(raw)
            If matches.Count = 0 Then
                su.Visible = True
                su.Text = "No URLs found in JSON file"
                Exit Sub
            End If
            Dim first As Boolean = True
            For Each m As System.Text.RegularExpressions.Match In matches
                Dim url As String = m.Groups(1).Value
                If url = "" Then Continue For
                If first Then
                    CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(url)
                    first = False
                Else
                    q3(url)
                End If
            Next
            su.Visible = True
            su.Text = matches.Count.ToString & " tab(s) loaded from JSON session"
        Catch ex As Exception
        End Try
    End Sub
    Sub q592(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q591(sender, e)
        End If
    End Sub
    ' --- Copy Page Highlight Text ---
    Sub q593(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            Dim count As Integer = 0
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("mark")
                Dim txt As String = If(el.InnerText IsNot Nothing, el.InnerText.Trim, "")
                If txt <> "" Then
                    sb.AppendLine(txt)
                    count += 1
                End If
            Next
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("em")
                Dim txt As String = If(el.InnerText IsNot Nothing, el.InnerText.Trim, "")
                If txt <> "" Then
                    sb.AppendLine("[em] " & txt)
                    count += 1
                End If
            Next
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("strong")
                Dim txt As String = If(el.InnerText IsNot Nothing, el.InnerText.Trim, "")
                If txt <> "" Then
                    sb.AppendLine("[strong] " & txt)
                    count += 1
                End If
            Next
            If count = 0 Then
                su.Visible = True
                su.Text = "No highlighted or emphasised text found"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = count.ToString & " highlighted/emphasised text block(s) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q594(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q593(sender, e)
        End If
    End Sub
    ' --- Open All Bookmarks With Keyword ---
    Sub q595(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim term As String = InputBox("Enter keyword to filter bookmarks:", "Open All Bookmarks With Keyword", "")
            If term = "" Then Exit Sub
            Dim opened As Integer = 0
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim url As String = My.Settings.bm.Item(i)
                If url <> "" AndAlso url.ToLower.Contains(term.ToLower) Then
                    q3(url)
                    opened += 1
                End If
            Next i
            If opened = 0 Then
                su.Visible = True
                su.Text = "No bookmarks match: " & term
            Else
                su.Visible = True
                su.Text = opened.ToString & " matching bookmark(s) opened"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q596(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q595(sender, e)
        End If
    End Sub
    ' --- Show Page Frame Count ---
    Sub q597(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim frames As Integer = wb.Document.GetElementsByTagName("iframe").Count +
                                    wb.Document.GetElementsByTagName("frame").Count +
                                    wb.Document.GetElementsByTagName("frameset").Count
            Dim result As String = "Frames on page: " & frames.ToString &
                                   " (iframe: " & wb.Document.GetElementsByTagName("iframe").Count.ToString &
                                   ", frame: " & wb.Document.GetElementsByTagName("frame").Count.ToString & ")"
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q598(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q597(sender, e)
        End If
    End Sub
    ' --- Copy Quick Notes Last Line ---
    Sub q599(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim lines() As String = My.Settings.qn.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim last As String = lines(lines.Length - 1).Trim
            If last = "" Then
                su.Visible = True
                su.Text = "Last line is empty"
                Exit Sub
            End If
            Clipboard.SetText(last)
            su.Visible = True
            su.Text = "Last quick note line copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q600(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q599(sender, e)
        End If
    End Sub
    ' --- Toggle Status Bar ---
    Sub q601(ByVal sender As Object, ByVal e As EventArgs)
        Try
            su.Visible = Not su.Visible
            su.Text = If(su.Visible, "Status bar visible", "")
        Catch ex As Exception
        End Try
    End Sub
    Sub q602(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q601(sender, e)
        End If
    End Sub
    ' --- Open Page In Google Cache ---
    Sub q603(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            q3("https://webcache.googleusercontent.com/search?q=cache:" & Uri.EscapeDataString(wb.Url.ToString))
            su.Visible = True
            su.Text = "Opening Google Cache for page"
        Catch ex As Exception
        End Try
    End Sub
    Sub q604(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q603(sender, e)
        End If
    End Sub
    ' --- Copy Bookmarks As CSV ---
    Sub q605(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("Number,URL,Domain")
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim url As String = My.Settings.bm.Item(i)
                If url = "" Then Continue For
                Dim host As String = ""
                Try
                    host = New Uri(url).Host
                Catch
                End Try
                sb.AppendLine(qc((i + 1).ToString) & "," & qc(url) & "," & qc(host))
            Next i
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Bookmarks copied as CSV"
        Catch ex As Exception
        End Try
    End Sub
    Sub q606(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q605(sender, e)
        End If
    End Sub
    ' --- Show Tab Loading Order ---
    Sub q607(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("=== Tab Loading Order ===")
            sb.AppendLine("Total tabs: " & k.TabPages.Count.ToString)
            sb.AppendLine("Selected:   Tab " & (k.SelectedIndex + 1).ToString)
            sb.AppendLine("")
            For q As Integer = 0 To k.TabPages.Count - 1
                Dim title As String = k.TabPages(q).Text
                Dim url As String = ""
                Dim locked As String = If(k.TabPages(q).Tag = "locked", " [LOCKED]", "")
                Try
                    Dim wbq As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wbq.Url IsNot Nothing Then url = wbq.Url.ToString
                Catch
                End Try
                Dim marker As String = If(q = k.SelectedIndex, " <<", "")
                sb.AppendLine((q + 1).ToString.PadLeft(3) & ". " & title & locked & marker)
                If url <> "" Then sb.AppendLine("     " & url)
            Next q
            Clipboard.SetText(sb.ToString.TrimEnd)
            MsgBox(sb.ToString, MsgBoxStyle.Information, "Tab Loading Order — Copied To Clipboard")
        Catch ex As Exception
        End Try
    End Sub
    Sub q608(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q607(sender, e)
        End If
    End Sub
    ' --- Open Page Source In Default Editor ---
    Sub q609(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.DocumentText = "" Then
                su.Visible = True
                su.Text = "No page source to open"
                Exit Sub
            End If
            Dim tmp As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "firespider_source_" & Now.Ticks.ToString & ".html")
            System.IO.File.WriteAllText(tmp, wb.DocumentText, System.Text.Encoding.UTF8)
            Process.Start(tmp)
            su.Visible = True
            su.Text = "Page source opened in default editor"
        Catch ex As Exception
        End Try
    End Sub
    Sub q610(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q609(sender, e)
        End If
    End Sub
    ' --- Copy Page Link At Position ---
    Sub q611(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim links As ArrayList = ql()
            If links.Count = 0 Then
                su.Visible = True
                su.Text = "No links found on page"
                Exit Sub
            End If
            Dim pick As String = InputBox("Enter link number to copy (1-" & links.Count & "):", "Copy Page Link At Position", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= links.Count Then
                su.Visible = True
                su.Text = "Number out of range"
                Exit Sub
            End If
            Clipboard.SetText(links(idx).ToString)
            su.Visible = True
            su.Text = "Link " & (idx + 1).ToString & " copied: " & links(idx).ToString.Substring(0, Math.Min(60, links(idx).ToString.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q612(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q611(sender, e)
        End If
    End Sub
    ' --- Prepend Text To Quick Notes ---
    Sub q613(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim stamp As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
            Dim text As String = InputBox("Enter text to prepend to Quick Notes:", "Prepend Text To Quick Notes", "")
            If text = "" Then Exit Sub
            Dim entry As String = "[" & stamp & "] " & text
            Dim sep As String = If(My.Settings.qn = "", "", vbCrLf & "---" & vbCrLf)
            My.Settings.qn = entry & sep & My.Settings.qn
            My.Settings.Save()
            su.Visible = True
            su.Text = "Text prepended to quick notes"
        Catch ex As Exception
        End Try
    End Sub
    Sub q614(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q613(sender, e)
        End If
    End Sub
    ' --- Save Page As PDF Via Print ---
    Sub q615(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            su.Visible = True
            su.Text = "Opening print dialog — choose a PDF printer to save"
            wb.ShowPrintDialog()
        Catch ex As Exception
        End Try
    End Sub
    Sub q616(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q615(sender, e)
        End If
    End Sub
    ' --- Copy Base URL ---
    Sub q617(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim base As String = wb.Url.Scheme & "://" & wb.Url.Host
            Clipboard.SetText(base)
            su.Visible = True
            su.Text = "Base URL copied: " & base
        Catch ex As Exception
        End Try
    End Sub
    Sub q618(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q617(sender, e)
        End If
    End Sub
    ' --- Open QR Code Scanner Page ---
    Sub q619(ByVal sender As Object, ByVal e As EventArgs)
        Try
            q3("https://webqr.com/")
            su.Visible = True
            su.Text = "QR code scanner page opened"
        Catch ex As Exception
        End Try
    End Sub
    Sub q620(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q619(sender, e)
        End If
    End Sub
    ' --- Copy Tab URLs Matching Keyword ---
    Sub q621(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim term As String = InputBox("Enter keyword to filter tab URLs:", "Copy Tab URLs Matching Keyword", "")
            If term = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            Dim count As Integer = 0
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wb.Url IsNot Nothing Then
                        Dim url As String = wb.Url.ToString
                        Dim title As String = k.TabPages(q).Text
                        If url.ToLower.Contains(term.ToLower) OrElse title.ToLower.Contains(term.ToLower) Then
                            sb.AppendLine(url)
                            count += 1
                        End If
                    End If
                End If
            Next q
            If count = 0 Then
                su.Visible = True
                su.Text = "No tabs match: " & term
            Else
                Clipboard.SetText(sb.ToString.TrimEnd)
                su.Visible = True
                su.Text = count.ToString & " matching tab URL(s) copied"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q622(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q621(sender, e)
        End If
    End Sub
    ' --- Save Bookmarks As JSON ---
    Sub q623(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks to save"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Bookmarks " & Now.ToString("yyyy-MM-dd") & ".json"
            SX.Filter = "JSON File [*.json*]|*.json|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Bookmarks As JSON"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("{")
            sb.AppendLine("  ""exported"": """ & Now.ToString("yyyy-MM-dd HH:mm:ss") & """,")
            sb.AppendLine("  ""count"": " & My.Settings.bm.Count.ToString & ",")
            sb.AppendLine("  ""bookmarks"": [")
            Dim valid As New List(Of String)
            For i As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(i) <> "" Then valid.Add(My.Settings.bm.Item(i))
            Next i
            For i As Integer = 0 To valid.Count - 1
                Dim url As String = valid(i).Replace("""", "\""")
                Dim host As String = ""
                Try
                    host = New Uri(valid(i)).Host.Replace("""", "\""")
                Catch
                End Try
                Dim comma As String = If(i < valid.Count - 1, ",", "")
                sb.AppendLine("    { ""index"": " & i.ToString & ", ""url"": """ & url & """, ""domain"": """ & host & """ }" & comma)
            Next i
            sb.AppendLine("  ]")
            sb.AppendLine("}")
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Bookmarks saved as JSON"
        Catch ex As Exception
        End Try
    End Sub
    Sub q624(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q623(sender, e)
        End If
    End Sub
    ' --- Load Bookmarks From JSON ---
    Sub q625(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ox As New OpenFileDialog()
            ox.Title = "Load Bookmarks From JSON"
            ox.CheckFileExists = True
            ox.Filter = "JSON File [*.json*]|*.json|All Files [*.*]|*.*"
            ox.ShowDialog()
            If ox.FileName = "" Then Exit Sub
            Dim raw As String = System.IO.File.ReadAllText(ox.FileName, System.Text.Encoding.UTF8)
            Dim rx As New System.Text.RegularExpressions.Regex("""url""\s*:\s*""([^""]+)""")
            Dim matches As System.Text.RegularExpressions.MatchCollection = rx.Matches(raw)
            If matches.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmark URLs found in JSON file"
                Exit Sub
            End If
            Dim added As Integer = 0
            For Each m As System.Text.RegularExpressions.Match In matches
                Dim url As String = m.Groups(1).Value
                If url = "" Then Continue For
                My.Settings.bm.Add(url)
                Dim fv As New ToolStripMenuItem
                fv.Text = url.Substring(0, Math.Min(60, url.Length))
                fv.Tag = url
                fv.Image = My.Resources.F9
                fv.BackgroundImage = My.Resources.F2
                fv.BackgroundImageLayout = ImageLayout.Stretch
                fv.ForeColor = Color.White
                AddHandler fv.Click, AddressOf z2
                AddHandler fv.MouseHover, AddressOf z2
                RB.DropDownItems.Add(fv)
                added += 1
            Next
            My.Settings.Save()
            su.Visible = True
            su.Text = added.ToString & " bookmark(s) loaded from JSON"
        Catch ex As Exception
        End Try
    End Sub
    Sub q626(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q625(sender, e)
        End If
    End Sub
    ' --- Copy Page Response Headers ---
    Sub q627(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim req As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(wb.Url), System.Net.HttpWebRequest)
            req.Method = "HEAD"
            req.Timeout = 5000
            req.AllowAutoRedirect = False
            Dim resp As System.Net.HttpWebResponse = CType(req.GetResponse(), System.Net.HttpWebResponse)
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("HTTP " & CInt(resp.StatusCode).ToString & " " & resp.StatusDescription)
            sb.AppendLine("URL: " & wb.Url.ToString)
            sb.AppendLine("")
            For Each key As String In resp.Headers.AllKeys
                sb.AppendLine(key & ": " & resp.Headers(key))
            Next
            resp.Close()
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Response headers copied (" & resp.Headers.Count.ToString & " headers)"
        Catch ex As System.Net.WebException
            Dim msg As String = "Error: " & ex.Message.Substring(0, Math.Min(80, ex.Message.Length))
            su.Visible = True
            su.Text = msg
        Catch ex As Exception
        End Try
    End Sub
    Sub q628(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q627(sender, e)
        End If
    End Sub
    ' --- Open All Tabs From History ---
    Sub q629(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim fromStr As String = InputBox("Open history items from number:", "Open All Tabs From History", "1")
            If fromStr = "" Then Exit Sub
            Dim toStr As String = InputBox("To number (max " & f6.l0.Items.Count & "):", "Open All Tabs From History", Math.Min(10, f6.l0.Items.Count).ToString)
            If toStr = "" Then Exit Sub
            Dim fromIdx As Integer = 0
            Dim toIdx As Integer = 0
            If Not Integer.TryParse(fromStr, fromIdx) OrElse Not Integer.TryParse(toStr, toIdx) Then
                su.Visible = True
                su.Text = "Invalid numbers entered"
                Exit Sub
            End If
            fromIdx -= 1
            toIdx -= 1
            fromIdx = Math.Max(0, fromIdx)
            toIdx = Math.Min(f6.l0.Items.Count - 1, toIdx)
            If fromIdx > toIdx Then
                su.Visible = True
                su.Text = "Start must be less than or equal to end"
                Exit Sub
            End If
            Dim opened As Integer = 0
            For i As Integer = fromIdx To toIdx
                Dim url As String = f6.l0.Items(i).ToString
                If url <> "" Then
                    q3(url)
                    opened += 1
                End If
            Next i
            su.Visible = True
            su.Text = opened.ToString & " history item(s) opened in tabs"
        Catch ex As Exception
        End Try
    End Sub
    Sub q630(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q629(sender, e)
        End If
    End Sub
    ' --- Speak Page Text Aloud ---
    Sub q631(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            If raw = "" Then
                su.Visible = True
                su.Text = "No page text to speak"
                Exit Sub
            End If
            Dim preview As String = raw.Substring(0, Math.Min(500, raw.Length))
            z1.Speak(preview)
            su.Visible = True
            su.Text = "Speaking first 500 chars of page text"
        Catch ex As Exception
        End Try
    End Sub
    Sub q632(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q631(sender, e)
        End If
    End Sub
    ' --- Export Quick Notes As CSV ---
    Sub q633(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Quick Notes.csv"
            SX.Filter = "CSV File [*.csv*]|*.csv|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Export Quick Notes As CSV"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim lines() As String = My.Settings.qn.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("Line,Text")
            For i As Integer = 0 To lines.Length - 1
                Dim t As String = lines(i).Trim
                If t <> "" Then
                    sb.AppendLine(qc((i + 1).ToString) & "," & qc(t))
                End If
            Next i
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Quick notes exported as CSV"
        Catch ex As Exception
        End Try
    End Sub
    Sub q634(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q633(sender, e)
        End If
    End Sub
    ' --- Copy Page Paragraph Count ---
    Sub q635(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim pCount As Integer = wb.Document.GetElementsByTagName("p").Count
            Dim divCount As Integer = wb.Document.GetElementsByTagName("div").Count
            Dim liCount As Integer = wb.Document.GetElementsByTagName("li").Count
            Dim result As String = "Paragraphs: " & pCount.ToString &
                                   "  Divs: " & divCount.ToString &
                                   "  List items: " & liCount.ToString
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q636(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q635(sender, e)
        End If
    End Sub
    ' --- Close All Tabs Except First ---
    Sub q637(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.TabPages.Count <= 1 Then
                su.Visible = True
                su.Text = "Only one tab is open"
                Exit Sub
            End If
            Dim confirm As MsgBoxResult = MsgBox("Close all tabs except the first?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Close All Tabs Except First")
            If confirm <> MsgBoxResult.Yes Then Exit Sub
            Dim closed As Integer = 0
            For q As Integer = k.TabPages.Count - 1 To 1 Step -1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wb.Url IsNot Nothing Then rca(wb.Url.ToString)
                End If
                k.TabPages.RemoveAt(q)
                I = I - 1
                Dim tz As Size = k.ItemSize
                tz.Width += 6
                k.ItemSize = tz
                closed += 1
            Next q
            k.SelectedIndex = 0
            k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Text = ProductName & " - " & k.SelectedTab.Text
            c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
            su.Visible = True
            su.Text = closed.ToString & " tab(s) closed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q638(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q637(sender, e)
        End If
    End Sub
    ' --- Open URL From Quick Notes ---
    Sub q639(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim rx As New System.Text.RegularExpressions.Regex("https?://[^\s""'<>\]\)]+")
            Dim matches As System.Text.RegularExpressions.MatchCollection = rx.Matches(My.Settings.qn)
            If matches.Count = 0 Then
                su.Visible = True
                su.Text = "No URLs found in quick notes"
                Exit Sub
            End If
            If matches.Count = 1 Then
                q3(matches(0).Value)
                su.Visible = True
                su.Text = "URL opened from quick notes"
                Exit Sub
            End If
            Dim sb As New System.Text.StringBuilder
            For i As Integer = 0 To Math.Min(matches.Count - 1, 14)
                sb.AppendLine((i + 1).ToString & ". " & matches(i).Value)
            Next i
            Dim pick As String = InputBox("Enter URL number to open (1-" & Math.Min(matches.Count, 15) & "):" & vbCrLf & sb.ToString, "Open URL From Quick Notes", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= matches.Count Then
                su.Visible = True
                su.Text = "Number out of range"
                Exit Sub
            End If
            q3(matches(idx).Value)
            su.Visible = True
            su.Text = "URL opened from quick notes"
        Catch ex As Exception
        End Try
    End Sub
    Sub q640(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q639(sender, e)
        End If
    End Sub
    ' --- Copy Tab URLs As JSON Array ---
    Sub q641(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("[")
            Dim urls As New List(Of String)
            For q As Integer = 0 To k.TabPages.Count - 1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wb.Url IsNot Nothing Then urls.Add(wb.Url.ToString)
                End If
            Next q
            For i As Integer = 0 To urls.Count - 1
                Dim comma As String = If(i < urls.Count - 1, ",", "")
                sb.AppendLine("  """ & urls(i).Replace("""", "\""") & """" & comma)
            Next i
            sb.AppendLine("]")
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = urls.Count.ToString & " tab URL(s) copied as JSON array"
        Catch ex As Exception
        End Try
    End Sub
    Sub q642(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q641(sender, e)
        End If
    End Sub
    ' --- Reload Tab By Number ---
    Sub q643(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim pick As String = InputBox("Enter tab number to reload (1-" & k.TabPages.Count & "):", "Reload Tab By Number", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= k.TabPages.Count Then
                su.Visible = True
                su.Text = "Tab number out of range"
                Exit Sub
            End If
            If k.TabPages(idx).Controls.Count > 0 Then
                CType(k.TabPages(idx).Controls.Item(0), WebBrowser).Refresh()
                su.Visible = True
                su.Text = "Tab " & (idx + 1).ToString & " reloaded"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q644(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q643(sender, e)
        End If
    End Sub
    ' --- Copy Page Title And Domain ---
    Sub q645(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim title As String = wb.DocumentTitle
            Dim domain As String = ""
            Try
                domain = wb.Url.Host
            Catch
            End Try
            Dim result As String = title & vbCrLf & domain
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = "Title and domain copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q646(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q645(sender, e)
        End If
    End Sub
    ' --- Open All Links On Page In Tabs ---
    Sub q647(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ar As ArrayList = ql()
            If ar.Count = 0 Then
                su.Visible = True
                su.Text = "No links found on page"
                Exit Sub
            End If
            Dim confirm As MsgBoxResult = MsgBox("Open all " & ar.Count & " link(s) in new tabs?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Open All Links On Page In Tabs")
            If confirm <> MsgBoxResult.Yes Then Exit Sub
            For Each href As String In ar
                q3(href)
            Next
            su.Visible = True
            su.Text = ar.Count.ToString & " link(s) opened in tabs"
        Catch ex As Exception
        End Try
    End Sub
    Sub q648(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q647(sender, e)
        End If
    End Sub
    ' --- Save Quick Notes As Markdown ---
    Sub q649(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Quick Notes.md"
            SX.Filter = "Markdown File [*.md*]|*.md|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Quick Notes As Markdown"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("# Firespider Quick Notes")
            sb.AppendLine("")
            sb.AppendLine("> Exported: " & Now.ToString("yyyy-MM-dd HH:mm:ss"))
            sb.AppendLine("")
            Dim lines() As String = My.Settings.qn.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.None)
            For Each ln As String In lines
                If ln.Trim = "---" Then
                    sb.AppendLine("")
                    sb.AppendLine("---")
                    sb.AppendLine("")
                Else
                    sb.AppendLine(ln)
                End If
            Next
            System.IO.File.WriteAllText(SX.FileName, sb.ToString.TrimEnd, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Quick notes saved as Markdown"
        Catch ex As Exception
        End Try
    End Sub
    Sub q650(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q649(sender, e)
        End If
    End Sub
    ' --- Close Tabs Below Current ---
    Sub q651(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim cur As Integer = k.SelectedIndex
            If cur >= k.TabPages.Count - 1 Then
                su.Visible = True
                su.Text = "No tabs below current tab"
                Exit Sub
            End If
            Dim closed As Integer = 0
            For q As Integer = k.TabPages.Count - 1 To cur + 1 Step -1
                If k.TabPages(q).Controls.Count > 0 Then
                    Dim wb As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wb.Url IsNot Nothing Then rca(wb.Url.ToString)
                End If
                k.TabPages.RemoveAt(q)
                I = I - 1
                Dim tz As Size = k.ItemSize
                tz.Width += 6
                k.ItemSize = tz
                closed += 1
            Next q
            su.Visible = True
            su.Text = closed.ToString & " tab(s) below current closed"
        Catch ex As Exception
        End Try
    End Sub
    Sub q652(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q651(sender, e)
        End If
    End Sub
    ' --- Open Random History Item ---
    Sub q653(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim rng As New Random
            Dim idx As Integer = rng.Next(0, f6.l0.Items.Count)
            Dim url As String = f6.l0.Items(idx).ToString
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(url)
            su.Visible = True
            su.Text = "Random history item opened: item " & (idx + 1).ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q654(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q653(sender, e)
        End If
    End Sub
    ' --- Copy Page Bold Text ---
    Sub q655(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim sb As New System.Text.StringBuilder
            Dim count As Integer = 0
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("b")
                Dim txt As String = If(el.InnerText IsNot Nothing, el.InnerText.Trim, "")
                If txt <> "" Then
                    sb.AppendLine(txt)
                    count += 1
                End If
            Next
            For Each el As HtmlElement In wb.Document.GetElementsByTagName("strong")
                Dim txt As String = If(el.InnerText IsNot Nothing, el.InnerText.Trim, "")
                If txt <> "" Then
                    sb.AppendLine(txt)
                    count += 1
                End If
            Next
            If count = 0 Then
                su.Visible = True
                su.Text = "No bold text found on page"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = count.ToString & " bold text block(s) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q656(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q655(sender, e)
        End If
    End Sub
    ' --- Show Bookmark At Position ---
    Sub q657(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim pick As String = InputBox("Enter bookmark number to view (1-" & My.Settings.bm.Count & "):", "Show Bookmark At Position", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= My.Settings.bm.Count Then
                su.Visible = True
                su.Text = "Number out of range"
                Exit Sub
            End If
            Dim url As String = My.Settings.bm.Item(idx)
            Dim host As String = ""
            Try
                host = New Uri(url).Host
            Catch
            End Try
            MsgBox("Bookmark " & (idx + 1).ToString & " of " & My.Settings.bm.Count.ToString & vbCrLf & vbCrLf &
                   "URL: " & url & vbCrLf &
                   "Domain: " & host, MsgBoxStyle.Information, "Bookmark Details")
            Clipboard.SetText(url)
            su.Visible = True
            su.Text = "Bookmark " & (idx + 1).ToString & " shown and copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q658(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q657(sender, e)
        End If
    End Sub
    ' --- Toggle Tab Strip Visibility ---
    Sub q659(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.Appearance = TabAppearance.Normal Then
                k.Appearance = TabAppearance.FlatButtons
                k.ItemSize = New Size(0, 1)
                su.Visible = True
                su.Text = "Tab strip hidden"
            Else
                k.Appearance = TabAppearance.Normal
                Dim tz As Size = k.ItemSize
                tz.Height = 20
                tz.Width = 240
                k.ItemSize = tz
                su.Visible = True
                su.Text = "Tab strip visible"
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q660(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q659(sender, e)
        End If
    End Sub
    ' --- Save History As JSON ---
    Sub q661(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If f6.l0.Items.Count = 0 Then
                su.Visible = True
                su.Text = "History is empty"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider History " & Now.ToString("yyyy-MM-dd") & ".json"
            SX.Filter = "JSON File [*.json*]|*.json|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save History As JSON"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("{")
            sb.AppendLine("  ""exported"": """ & Now.ToString("yyyy-MM-dd HH:mm:ss") & """,")
            sb.AppendLine("  ""count"": " & f6.l0.Items.Count.ToString & ",")
            sb.AppendLine("  ""history"": [")
            For i As Integer = 0 To f6.l0.Items.Count - 1
                Dim url As String = f6.l0.Items(i).ToString.Replace("""", "\""")
                Dim comma As String = If(i < f6.l0.Items.Count - 1, ",", "")
                sb.AppendLine("    { ""index"": " & i.ToString & ", ""url"": """ & url & """ }" & comma)
            Next i
            sb.AppendLine("  ]")
            sb.AppendLine("}")
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "History saved as JSON"
        Catch ex As Exception
        End Try
    End Sub
    Sub q662(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q661(sender, e)
        End If
    End Sub
    ' --- Load History From JSON ---
    Sub q663(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim ox As New OpenFileDialog()
            ox.Title = "Load History From JSON"
            ox.CheckFileExists = True
            ox.Filter = "JSON File [*.json*]|*.json|All Files [*.*]|*.*"
            ox.ShowDialog()
            If ox.FileName = "" Then Exit Sub
            Dim raw As String = System.IO.File.ReadAllText(ox.FileName, System.Text.Encoding.UTF8)
            Dim rx As New System.Text.RegularExpressions.Regex("""url""\s*:\s*""([^""]+)""")
            Dim matches As System.Text.RegularExpressions.MatchCollection = rx.Matches(raw)
            If matches.Count = 0 Then
                su.Visible = True
                su.Text = "No history URLs found in JSON file"
                Exit Sub
            End If
            Dim added As Integer = 0
            For Each m As System.Text.RegularExpressions.Match In matches
                Dim url As String = m.Groups(1).Value
                If url = "" Then Continue For
                f6.l0.Items.Add(url)
                c.Items.Add(url)
                added += 1
            Next
            su.Visible = True
            su.Text = added.ToString & " history item(s) loaded from JSON"
        Catch ex As Exception
        End Try
    End Sub
    Sub q664(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q663(sender, e)
        End If
    End Sub
    ' --- Copy Current Tab URL Encoded ---
    Sub q665(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim encoded As String = Uri.EscapeDataString(wb.Url.ToString)
            Clipboard.SetText(encoded)
            su.Visible = True
            su.Text = "URL-encoded address copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q666(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q665(sender, e)
        End If
    End Sub
    ' --- Open Next Bookmark ---
    Sub q667(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim valid As New List(Of String)
            For i As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(i) <> "" Then valid.Add(My.Settings.bm.Item(i))
            Next i
            If valid.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim cur As String = If(wb.Url IsNot Nothing, wb.Url.ToString, "")
            Dim idx As Integer = -1
            For i As Integer = 0 To valid.Count - 1
                If valid(i) = cur Then
                    idx = i
                    Exit For
                End If
            Next i
            Dim next_idx As Integer = If(idx = -1 OrElse idx >= valid.Count - 1, 0, idx + 1)
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(valid(next_idx))
            su.Visible = True
            su.Text = "Navigating to bookmark " & (next_idx + 1).ToString & " of " & valid.Count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q668(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q667(sender, e)
        End If
    End Sub
    ' --- Show Page External Link Count ---
    Sub q669(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim host As String = wb.Url.Host
            Dim total As Integer = 0
            Dim external As Integer = 0
            Dim internal As Integer = 0
            For Each el As HtmlElement In wb.Document.Links
                Dim href As String = el.GetAttribute("href")
                If href = "" Then Continue For
                total += 1
                Try
                    Dim lu As New Uri(href)
                    If lu.Host <> "" AndAlso lu.Host <> host Then
                        external += 1
                    Else
                        internal += 1
                    End If
                Catch
                    internal += 1
                End Try
            Next
            Dim result As String = "Total: " & total.ToString &
                                   "  External: " & external.ToString &
                                   "  Internal: " & internal.ToString
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q670(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q669(sender, e)
        End If
    End Sub
    ' --- Append Page Info To Quick Notes ---
    Sub q671(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim stamp As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
            Dim title As String = wb.DocumentTitle
            Dim url As String = ""
            Try
                url = wb.Url.ToString
            Catch
            End Try
            Dim entry As String = "[" & stamp & "]" & vbCrLf &
                                  "Title: " & title & vbCrLf &
                                  "URL:   " & url
            Dim sep As String = If(My.Settings.qn = "", "", vbCrLf & "---" & vbCrLf)
            My.Settings.qn = My.Settings.qn & sep & entry
            My.Settings.Save()
            su.Visible = True
            su.Text = "Page info appended to quick notes"
        Catch ex As Exception
        End Try
    End Sub
    Sub q672(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q671(sender, e)
        End If
    End Sub
    ' --- Stop Speaking ---
    Sub q673(ByVal sender As Object, ByVal e As EventArgs)
        Try
            z1.Skip("Sentence", 100)
            su.Visible = True
            su.Text = "Speech stopped"
        Catch ex As Exception
            Try
                z1 = CreateObject("SAPI.spvoice")
                su.Visible = True
                su.Text = "Speech reset"
            Catch
            End Try
        End Try
    End Sub
    Sub q674(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q673(sender, e)
        End If
    End Sub
    ' --- Copy Page Internal Links Only ---
    Sub q675(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim host As String = wb.Url.Host
            Dim sb As New System.Text.StringBuilder
            Dim count As Integer = 0
            For Each el As HtmlElement In wb.Document.Links
                Dim href As String = el.GetAttribute("href")
                If href = "" Then Continue For
                Try
                    Dim lu As New Uri(href)
                    If lu.Host = "" OrElse lu.Host = host Then
                        sb.AppendLine(href)
                        count += 1
                    End If
                Catch
                    sb.AppendLine(href)
                    count += 1
                End Try
            Next
            If count = 0 Then
                su.Visible = True
                su.Text = "No internal links found on page"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = count.ToString & " internal link(s) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q676(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q675(sender, e)
        End If
    End Sub
    ' --- Copy Page External Links Only ---
    Sub q677(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then
                su.Visible = True
                su.Text = "No URL loaded"
                Exit Sub
            End If
            Dim host As String = wb.Url.Host
            Dim sb As New System.Text.StringBuilder
            Dim count As Integer = 0
            For Each el As HtmlElement In wb.Document.Links
                Dim href As String = el.GetAttribute("href")
                If href = "" Then Continue For
                Try
                    Dim lu As New Uri(href)
                    If lu.Host <> "" AndAlso lu.Host <> host Then
                        sb.AppendLine(href)
                        count += 1
                    End If
                Catch
                End Try
            Next
            If count = 0 Then
                su.Visible = True
                su.Text = "No external links found on page"
                Exit Sub
            End If
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = count.ToString & " external link(s) copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q678(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q677(sender, e)
        End If
    End Sub
    ' --- Save Notes With Page Context ---
    Sub q679(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim note As String = InputBox("Enter note to save with current page context:", "Save Notes With Page Context", "")
            If note = "" Then Exit Sub
            Dim stamp As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
            Dim title As String = wb.DocumentTitle
            Dim url As String = ""
            Try
                url = wb.Url.ToString
            Catch
            End Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Note " & Now.ToString("yyyy-MM-dd") & ".txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Notes With Page Context"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("Firespider Page Note")
            sb.AppendLine(New String("="c, 40))
            sb.AppendLine("Date:   " & stamp)
            sb.AppendLine("Title:  " & title)
            sb.AppendLine("URL:    " & url)
            sb.AppendLine("")
            sb.AppendLine("Note:")
            sb.AppendLine(note)
            System.IO.File.WriteAllText(SX.FileName, sb.ToString.TrimEnd, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Note saved with page context"
        Catch ex As Exception
        End Try
    End Sub
    Sub q680(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q679(sender, e)
        End If
    End Sub
    ' --- Open Previous Bookmark ---
    Sub q681(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim valid As New List(Of String)
            For i As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(i) <> "" Then valid.Add(My.Settings.bm.Item(i))
            Next i
            If valid.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim cur As String = If(wb.Url IsNot Nothing, wb.Url.ToString, "")
            Dim idx As Integer = -1
            For i As Integer = 0 To valid.Count - 1
                If valid(i) = cur Then
                    idx = i
                    Exit For
                End If
            Next i
            Dim prev_idx As Integer = If(idx <= 0, valid.Count - 1, idx - 1)
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(valid(prev_idx))
            su.Visible = True
            su.Text = "Navigating to bookmark " & (prev_idx + 1).ToString & " of " & valid.Count.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q682(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q681(sender, e)
        End If
    End Sub
    ' --- Copy Tab Count As Text ---
    Sub q683(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim result As String = "Open tabs: " & k.TabPages.Count.ToString &
                                   " | Selected: " & (k.SelectedIndex + 1).ToString &
                                   " | Total opened: " & I.ToString
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q684(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q683(sender, e)
        End If
    End Sub
    ' --- Open Search Engine Settings ---
    Sub q685(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim current As String = My.Settings.se
            Dim newSe As String = InputBox("Enter search engine URL (include ?q= or ?p= at end):" & vbCrLf & vbCrLf &
                "Examples:" & vbCrLf &
                "www.bing.com/search?q=" & vbCrLf &
                "www.google.ca/search?q=" & vbCrLf &
                "duckduckgo.com/?q=" & vbCrLf &
                "search.yahoo.com/search?p=", "Open Search Engine Settings", current)
            If newSe = "" OrElse newSe = current Then Exit Sub
            My.Settings.se = newSe.Trim
            My.Settings.Save()
            su.Visible = True
            su.Text = "Search engine set to: " & My.Settings.se.Substring(0, Math.Min(60, My.Settings.se.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q686(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q685(sender, e)
        End If
    End Sub
    ' --- Save Page As XML Snapshot ---
    Sub q687(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = wb.DocumentTitle & ".xml"
            SX.Filter = "XML File [*.xml*]|*.xml|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page As XML Snapshot"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim links As ArrayList = ql()
            Dim imgs As ArrayList = qi()
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("<?xml version=""1.0"" encoding=""utf-8""?>")
            sb.AppendLine("<page>")
            sb.AppendLine("  <title>" & qh(wb.DocumentTitle) & "</title>")
            Dim url As String = ""
            Try
                url = wb.Url.ToString
            Catch
            End Try
            sb.AppendLine("  <url>" & qh(url) & "</url>")
            sb.AppendLine("  <domain>" & qh(If(wb.Url IsNot Nothing, wb.Url.Host, "")) & "</domain>")
            sb.AppendLine("  <exported>" & Now.ToString("yyyy-MM-dd HH:mm:ss") & "</exported>")
            sb.AppendLine("  <links count=""" & links.Count.ToString & """>")
            For Each href As String In links
                sb.AppendLine("    <link>" & qh(href) & "</link>")
            Next
            sb.AppendLine("  </links>")
            sb.AppendLine("  <images count=""" & imgs.Count.ToString & """>")
            For Each src As String In imgs
                sb.AppendLine("    <image>" & qh(src) & "</image>")
            Next
            sb.AppendLine("  </images>")
            sb.AppendLine("</page>")
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Page saved as XML snapshot"
        Catch ex As Exception
        End Try
    End Sub
    Sub q688(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q687(sender, e)
        End If
    End Sub
    ' --- Go To Tab To The Right ---
    Sub q689(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim idx As Integer = k.SelectedIndex + 1
            If idx >= k.TabPages.Count Then idx = 0
            k.SelectedIndex = idx
            k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Text = ProductName & " - " & k.SelectedTab.Text
            c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q690(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q689(sender, e)
        End If
    End Sub
    ' --- Go To Tab To The Left ---
    Sub q691(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim idx As Integer = k.SelectedIndex - 1
            If idx < 0 Then idx = k.TabPages.Count - 1
            k.SelectedIndex = idx
            k.SelectedTab.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).DocumentTitle
            Text = ProductName & " - " & k.SelectedTab.Text
            c.Text = CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString
        Catch ex As Exception
        End Try
    End Sub
    Sub q692(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q691(sender, e)
        End If
    End Sub
    ' --- Copy Page Navigation Timing ---
    Sub q693(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As Object = Nothing
            Try
                raw = wb.Document.InvokeScript("eval",
                    New Object() {
                        "(function(){" &
                        "var t=window.performance&&window.performance.timing;" &
                        "if(!t)return 'Not available';" &
                        "return 'DNS: '+(t.domainLookupEnd-t.domainLookupStart)+'ms | " &
                        "Connect: '+(t.connectEnd-t.connectStart)+'ms | " &
                        "TTFB: '+(t.responseStart-t.requestStart)+'ms | " &
                        "Download: '+(t.responseEnd-t.responseStart)+'ms | " &
                        "DOM: '+(t.domContentLoadedEventEnd-t.navigationStart)+'ms | " &
                        "Load: '+(t.loadEventEnd-t.navigationStart)+'ms';" &
                        "})();"})
            Catch
            End Try
            Dim result As String = If(raw IsNot Nothing, raw.ToString, "Timing data not available")
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = "Navigation timing copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q694(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q693(sender, e)
        End If
    End Sub
    ' --- Shuffle Bookmark Order ---
    Sub q695(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim valid As New List(Of String)
            For i As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(i) <> "" Then valid.Add(My.Settings.bm.Item(i))
            Next i
            If valid.Count < 2 Then
                su.Visible = True
                su.Text = "Not enough bookmarks to shuffle"
                Exit Sub
            End If
            Dim rng As New Random
            For i As Integer = valid.Count - 1 To 1 Step -1
                Dim j As Integer = rng.Next(0, i + 1)
                Dim tmp As String = valid(i)
                valid(i) = valid(j)
                valid(j) = tmp
            Next i
            My.Settings.bm.Clear()
            RB.DropDownItems.Clear()
            For Each url As String In valid
                My.Settings.bm.Add(url)
                Dim fv As New ToolStripMenuItem
                fv.Text = url.Substring(0, Math.Min(60, url.Length))
                fv.Tag = url
                fv.Image = My.Resources.F9
                fv.BackgroundImage = My.Resources.F2
                fv.BackgroundImageLayout = ImageLayout.Stretch
                fv.ForeColor = Color.White
                AddHandler fv.Click, AddressOf z2
                AddHandler fv.MouseHover, AddressOf z2
                RB.DropDownItems.Add(fv)
            Next
            My.Settings.Save()
            su.Visible = True
            su.Text = "Bookmark order shuffled"
        Catch ex As Exception
        End Try
    End Sub
    Sub q696(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q695(sender, e)
        End If
    End Sub
    ' --- Copy Page As Tab Separated Data ---
    Sub q697(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim links As ArrayList = ql()
            Dim imgs As ArrayList = qi()
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("Field" & vbTab & "Value")
            sb.AppendLine("Title" & vbTab & wb.DocumentTitle)
            Try
                sb.AppendLine("URL" & vbTab & wb.Url.ToString)
                sb.AppendLine("Domain" & vbTab & wb.Url.Host)
                sb.AppendLine("Protocol" & vbTab & wb.Url.Scheme)
                sb.AppendLine("Path" & vbTab & wb.Url.AbsolutePath)
            Catch
            End Try
            sb.AppendLine("Links" & vbTab & links.Count.ToString)
            sb.AppendLine("Images" & vbTab & imgs.Count.ToString)
            Try
                Dim body As String = wb.Document.Body.InnerText
                Dim words() As String = body.Split(New Char() {" "c, vbTab(0), vbCr(0), vbLf(0)}, StringSplitOptions.RemoveEmptyEntries)
                sb.AppendLine("Words" & vbTab & words.Length.ToString)
                sb.AppendLine("Chars" & vbTab & body.Length.ToString)
            Catch
            End Try
            sb.AppendLine("Status" & vbTab & wb.StatusText)
            sb.AppendLine("Can Back" & vbTab & wb.CanGoBack.ToString)
            sb.AppendLine("Can Forward" & vbTab & wb.CanGoForward.ToString)
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Page data copied as tab-separated values"
        Catch ex As Exception
        End Try
    End Sub
    Sub q698(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q697(sender, e)
        End If
    End Sub
    ' --- Save Page Images List To File ---
    Sub q699(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim imgs As ArrayList = qi()
            If imgs.Count = 0 Then
                su.Visible = True
                su.Text = "No images found on page"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = wb.DocumentTitle & " Images.txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page Images List To File"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("Firespider Page Images")
            sb.AppendLine("Page:   " & wb.DocumentTitle)
            Try
                sb.AppendLine("URL:    " & wb.Url.ToString)
            Catch
            End Try
            sb.AppendLine("Images: " & imgs.Count.ToString)
            sb.AppendLine("Saved:  " & Now.ToString("yyyy-MM-dd HH:mm:ss"))
            sb.AppendLine(New String("="c, 40))
            sb.AppendLine("")
            For i As Integer = 0 To imgs.Count - 1
                sb.AppendLine((i + 1).ToString.PadLeft(4) & ".  " & imgs(i).ToString)
            Next i
            System.IO.File.WriteAllText(SX.FileName, sb.ToString.TrimEnd, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = imgs.Count.ToString & " image(s) saved to file"
        Catch ex As Exception
        End Try
    End Sub
    Sub q700(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q699(sender, e)
        End If
    End Sub
    ' --- Open Bookmark Folder In Tabs ---
    Sub q701(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim domain As String = InputBox("Enter domain keyword to open all matching bookmarks in tabs:" & vbCrLf & "(e.g. github, youtube, wikipedia)", "Open Bookmark Folder In Tabs", "")
            If domain = "" Then Exit Sub
            Dim opened As Integer = 0
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim url As String = My.Settings.bm.Item(i)
                If url = "" Then Continue For
                Dim host As String = ""
                Try
                    host = New Uri(url).Host
                Catch
                End Try
                If host.ToLower.Contains(domain.ToLower) OrElse url.ToLower.Contains(domain.ToLower) Then
                    q3(url)
                    opened += 1
                End If
            Next i
            If opened = 0 Then
                su.Visible = True
                su.Text = "No bookmarks match domain: " & domain
            Else
                su.Visible = True
                su.Text = opened.ToString & " bookmark(s) opened for: " & domain
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub q702(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q701(sender, e)
        End If
    End Sub
    ' --- Copy Page Text Upper Case ---
    Sub q703(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            If raw = "" Then
                su.Visible = True
                su.Text = "No page text found"
                Exit Sub
            End If
            Clipboard.SetText(raw.ToUpper)
            su.Visible = True
            su.Text = "Page text copied in upper case"
        Catch ex As Exception
        End Try
    End Sub
    Sub q704(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q703(sender, e)
        End If
    End Sub
    ' --- Copy Page Text Lower Case ---
    Sub q705(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            If raw = "" Then
                su.Visible = True
                su.Text = "No page text found"
                Exit Sub
            End If
            Clipboard.SetText(raw.ToLower)
            su.Visible = True
            su.Text = "Page text copied in lower case"
        Catch ex As Exception
        End Try
    End Sub
    Sub q706(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q705(sender, e)
        End If
    End Sub
    ' --- Save Page As Numbered HTML List ---
    Sub q707(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As String = ""
            Try
                raw = wb.Document.Body.InnerText
            Catch
            End Try
            If raw = "" Then
                su.Visible = True
                su.Text = "No page text found"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = wb.DocumentTitle & " List.html"
            SX.Filter = "HTML File [*.html*]|*.html|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Page As Numbered HTML List"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim lines() As String = raw.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("<!DOCTYPE html>")
            sb.AppendLine("<html><head><meta charset=""utf-8"">")
            sb.AppendLine("<title>" & qh(wb.DocumentTitle) & "</title>")
            sb.AppendLine("<style>body{font-family:Arial,sans-serif;max-width:900px;margin:40px auto;line-height:1.8;}li{margin:2px 0;}</style>")
            sb.AppendLine("</head><body>")
            sb.AppendLine("<h1>" & qh(wb.DocumentTitle) & "</h1>")
            Try
                sb.AppendLine("<p><em><a href=""" & qh(wb.Url.ToString) & """>" & qh(wb.Url.ToString) & "</a></em></p>")
            Catch
            End Try
            sb.AppendLine("<ol>")
            For Each ln As String In lines
                Dim t As String = ln.Trim
                If t <> "" Then sb.AppendLine("  <li>" & qh(t) & "</li>")
            Next
            sb.AppendLine("</ol>")
            sb.AppendLine("</body></html>")
            System.IO.File.WriteAllText(SX.FileName, sb.ToString, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = "Page saved as numbered HTML list"
        Catch ex As Exception
        End Try
    End Sub
    Sub q708(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q707(sender, e)
        End If
    End Sub
    ' --- Open Page On DuckDuckGo ---
    Sub q709(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            q3("https://duckduckgo.com/?q=" & Uri.EscapeDataString(wb.Url.ToString))
            su.Visible = True
            su.Text = "Page URL searched on DuckDuckGo"
        Catch ex As Exception
        End Try
    End Sub
    Sub q710(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q709(sender, e)
        End If
    End Sub
    ' --- Copy Page First Heading ---
    Sub q711(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim heading As String = ""
            For Each tag As String In {"h1", "h2", "h3"}
                Dim els As HtmlElementCollection = wb.Document.GetElementsByTagName(tag)
                If els.Count > 0 Then
                    Dim txt As String = If(els(0).InnerText IsNot Nothing, els(0).InnerText.Trim, "")
                    If txt <> "" Then
                        heading = "[" & tag.ToUpper & "] " & txt
                        Exit For
                    End If
                End If
            Next
            If heading = "" Then
                su.Visible = True
                su.Text = "No headings found on page"
                Exit Sub
            End If
            Clipboard.SetText(heading)
            su.Visible = True
            su.Text = "First heading copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q712(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q711(sender, e)
        End If
    End Sub
    ' --- Save All Tab URLs With Titles ---
    Sub q713(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Tabs " & Now.ToString("yyyy-MM-dd") & ".txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save All Tab URLs With Titles"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("Firespider Internet — Open Tabs")
            sb.AppendLine("Saved: " & Now.ToString("yyyy-MM-dd HH:mm:ss"))
            sb.AppendLine("Count: " & k.TabPages.Count.ToString)
            sb.AppendLine(New String("="c, 40))
            sb.AppendLine("")
            For q As Integer = 0 To k.TabPages.Count - 1
                Dim title As String = k.TabPages(q).Text
                Dim url As String = ""
                Try
                    Dim wbq As WebBrowser = CType(k.TabPages(q).Controls.Item(0), WebBrowser)
                    If wbq.Url IsNot Nothing Then url = wbq.Url.ToString
                Catch
                End Try
                sb.AppendLine((q + 1).ToString.PadLeft(3) & ". " & title)
                If url <> "" Then sb.AppendLine("     " & url)
                sb.AppendLine("")
            Next q
            System.IO.File.WriteAllText(SX.FileName, sb.ToString.TrimEnd, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = k.TabPages.Count.ToString & " tab(s) saved with titles"
        Catch ex As Exception
        End Try
    End Sub
    Sub q714(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q713(sender, e)
        End If
    End Sub
    ' --- Delete Quick Notes Line By Number ---
    Sub q715(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.qn = "" Then
                su.Visible = True
                su.Text = "Quick notes are empty"
                Exit Sub
            End If
            Dim lines As New List(Of String)(My.Settings.qn.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.None))
            Dim nonEmpty As New List(Of String)
            For Each ln As String In lines
                If ln.Trim <> "" Then nonEmpty.Add(ln)
            Next
            If nonEmpty.Count = 0 Then
                su.Visible = True
                su.Text = "No lines found in quick notes"
                Exit Sub
            End If
            Dim preview As New System.Text.StringBuilder
            For i As Integer = 0 To Math.Min(nonEmpty.Count - 1, 14)
                preview.AppendLine((i + 1).ToString & ". " & nonEmpty(i).Trim.Substring(0, Math.Min(60, nonEmpty(i).Trim.Length)))
            Next i
            Dim pick As String = InputBox("Enter line number to delete (1-" & nonEmpty.Count & "):" & vbCrLf & preview.ToString, "Delete Quick Notes Line", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= nonEmpty.Count Then
                su.Visible = True
                su.Text = "Number out of range"
                Exit Sub
            End If
            nonEmpty.RemoveAt(idx)
            My.Settings.qn = String.Join(vbCrLf, nonEmpty)
            My.Settings.Save()
            su.Visible = True
            su.Text = "Quick notes line " & (idx + 1).ToString & " deleted"
        Catch ex As Exception
        End Try
    End Sub
    Sub q716(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q715(sender, e)
        End If
    End Sub
    ' --- Open Page URL In New Private Window ---
    Sub q717(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim url As String = wb.Url.ToString
            Dim nw As New j
            nw.Show()
            Dim oldPv As Boolean = nw.pv
            nw.pv = True
            nw.My_Settings_b0_set("f")
            CType(nw.k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(url)
            nw.su.Visible = True
            nw.su.Text = "Private window opened"
            su.Visible = True
            su.Text = "Page opened in new private window"
        Catch ex As Exception
            Try
                Dim nw As New j
                nw.Show()
                CType(nw.k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(CType(k.SelectedTab.Controls.Item(0), WebBrowser).Url.ToString)
                su.Visible = True
                su.Text = "Page opened in new window"
            Catch
            End Try
        End Try
    End Sub
    Sub q718(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q717(sender, e)
        End If
    End Sub
    Public Sub My_Settings_b0_set(ByVal v As String)
        My.Settings.b0 = v
    End Sub
    ' --- Copy Bookmarks As Markdown List ---
    Sub q719(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim sb As New System.Text.StringBuilder
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim url As String = My.Settings.bm.Item(i)
                If url = "" Then Continue For
                sb.AppendLine("- [" & qmk(url) & "](" & url & ")")
            Next i
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Bookmarks copied as Markdown list"
        Catch ex As Exception
        End Try
    End Sub
    Sub q720(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q719(sender, e)
        End If
    End Sub
    ' --- Show Page Memory Usage ---
    Sub q721(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim raw As Object = Nothing
            Try
                raw = wb.Document.InvokeScript("eval",
                    New Object() {
                        "(function(){" &
                        "var m=window.performance&&window.performance.memory;" &
                        "if(!m)return 'Memory API not available';" &
                        "function fmt(b){return (b/1048576).toFixed(2)+'MB';}" &
                        "return 'Used: '+fmt(m.usedJSHeapSize)+" &
                        "' | Total: '+fmt(m.totalJSHeapSize)+" &
                        "' | Limit: '+fmt(m.jsHeapSizeLimit);" &
                        "})();"})
            Catch
            End Try
            Dim proc As String = ""
            Try
                Dim mem As Long = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64
                proc = "  |  Process: " & (mem \ 1048576).ToString & "MB"
            Catch
            End Try
            Dim result As String = If(raw IsNot Nothing, raw.ToString, "JS memory not available") & proc
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result.Substring(0, Math.Min(120, result.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q722(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q721(sender, e)
        End If
    End Sub
    ' --- Open Page On Bing ---
    Sub q723(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            q3("https://www.bing.com/search?q=" & Uri.EscapeDataString(wb.Url.ToString))
            su.Visible = True
            su.Text = "Page URL searched on Bing"
        Catch ex As Exception
        End Try
    End Sub
    Sub q724(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q723(sender, e)
        End If
    End Sub
    ' --- Merge Clipboard Into Quick Notes ---
    Sub q725(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim clip As String = Clipboard.GetText.Trim
            If clip = "" Then
                su.Visible = True
                su.Text = "Clipboard is empty"
                Exit Sub
            End If
            Dim stamp As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
            Dim sep As String = If(My.Settings.qn = "", "", vbCrLf & "---" & vbCrLf)
            My.Settings.qn = My.Settings.qn & sep & "[Clipboard " & stamp & "]" & vbCrLf & clip
            My.Settings.Save()
            su.Visible = True
            su.Text = "Clipboard content merged into quick notes"
        Catch ex As Exception
        End Try
    End Sub
    Sub q726(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q725(sender, e)
        End If
    End Sub
    ' --- Copy Tab At Position Info ---
    Sub q727(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim pick As String = InputBox("Enter tab number to inspect (1-" & k.TabPages.Count & "):", "Copy Tab At Position Info", "1")
            If pick = "" Then Exit Sub
            Dim idx As Integer = 0
            If Not Integer.TryParse(pick, idx) Then
                su.Visible = True
                su.Text = "Invalid number entered"
                Exit Sub
            End If
            idx = idx - 1
            If idx < 0 OrElse idx >= k.TabPages.Count Then
                su.Visible = True
                su.Text = "Tab number out of range"
                Exit Sub
            End If
            Dim title As String = k.TabPages(idx).Text
            Dim url As String = ""
            Dim locked As String = If(k.TabPages(idx).Tag = "locked", " [LOCKED]", "")
            Try
                Dim wbq As WebBrowser = CType(k.TabPages(idx).Controls.Item(0), WebBrowser)
                If wbq.Url IsNot Nothing Then url = wbq.Url.ToString
            Catch
            End Try
            Dim result As String = "Tab " & (idx + 1).ToString & " of " & k.TabPages.Count.ToString & locked & vbCrLf &
                                   "Title: " & title & vbCrLf &
                                   "URL:   " & url
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = "Tab " & (idx + 1).ToString & " info copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q728(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q727(sender, e)
        End If
    End Sub
    ' --- Open Page On Google Maps ---
    Sub q729(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim query As String = ""
            If wb.Url IsNot Nothing Then
                query = wb.DocumentTitle
                If query = "" Then query = wb.Url.Host
            End If
            Dim term As String = InputBox("Enter location or address to find on Google Maps:", "Open Page On Google Maps", query)
            If term = "" Then Exit Sub
            q3("https://www.google.com/maps/search/" & Uri.EscapeDataString(term))
            su.Visible = True
            su.Text = "Google Maps opened for: " & term.Substring(0, Math.Min(50, term.Length))
        Catch ex As Exception
        End Try
    End Sub
    Sub q730(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q729(sender, e)
        End If
    End Sub
    ' --- Save Bookmark Range To File ---
    Sub q731(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.bm.Count = 0 Then
                su.Visible = True
                su.Text = "No bookmarks saved"
                Exit Sub
            End If
            Dim fromStr As String = InputBox("Save bookmarks from number:", "Save Bookmark Range To File", "1")
            If fromStr = "" Then Exit Sub
            Dim toStr As String = InputBox("To number (max " & My.Settings.bm.Count & "):", "Save Bookmark Range To File", My.Settings.bm.Count.ToString)
            If toStr = "" Then Exit Sub
            Dim fromIdx As Integer = 0
            Dim toIdx As Integer = 0
            If Not Integer.TryParse(fromStr, fromIdx) OrElse Not Integer.TryParse(toStr, toIdx) Then
                su.Visible = True
                su.Text = "Invalid numbers entered"
                Exit Sub
            End If
            fromIdx -= 1
            toIdx -= 1
            fromIdx = Math.Max(0, fromIdx)
            toIdx = Math.Min(My.Settings.bm.Count - 1, toIdx)
            If fromIdx > toIdx Then
                su.Visible = True
                su.Text = "Start must be less than or equal to end"
                Exit Sub
            End If
            Dim SX As New SaveFileDialog()
            qsavefolder(SX)
            SX.FileName = "Firespider Bookmarks " & (fromIdx + 1) & "-" & (toIdx + 1) & ".txt"
            SX.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            SX.CheckPathExists = True
            SX.Title = "Save Bookmark Range To File"
            SX.ShowDialog(Me)
            If SX.FileName = "" Then Exit Sub
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine("Firespider Bookmarks " & (fromIdx + 1).ToString & " to " & (toIdx + 1).ToString)
            sb.AppendLine("Saved: " & Now.ToString("yyyy-MM-dd HH:mm:ss"))
            sb.AppendLine(New String("="c, 40))
            sb.AppendLine("")
            For i As Integer = fromIdx To toIdx
                Dim url As String = My.Settings.bm.Item(i)
                If url <> "" Then sb.AppendLine((i + 1).ToString.PadLeft(4) & ".  " & url)
            Next i
            System.IO.File.WriteAllText(SX.FileName, sb.ToString.TrimEnd, System.Text.Encoding.UTF8)
            su.Visible = True
            su.Text = (toIdx - fromIdx + 1).ToString & " bookmark(s) saved to file"
        Catch ex As Exception
        End Try
    End Sub
    Sub q732(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q731(sender, e)
        End If
    End Sub
    ' --- Copy Page Link And Image Count ---
    Sub q733(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim linkCount As Integer = wb.Document.Links.Count
            Dim imgCount As Integer = wb.Document.GetElementsByTagName("img").Count
            Dim scriptCount As Integer = wb.Document.GetElementsByTagName("script").Count
            Dim result As String = "Links: " & linkCount.ToString &
                                   "  Images: " & imgCount.ToString &
                                   "  Scripts: " & scriptCount.ToString
            Clipboard.SetText(result)
            su.Visible = True
            su.Text = result
        Catch ex As Exception
        End Try
    End Sub
    Sub q734(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q733(sender, e)
        End If
    End Sub
    ' --- Copy Page Citation ---
    Sub q735(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim title As String = wb.DocumentTitle
            If title = "" Then title = wb.Url.ToString
            Clipboard.SetText(title & " - " & wb.Url.ToString & " - Accessed " & Now.ToString("yyyy-MM-dd"))
            su.Visible = True
            su.Text = "Page citation copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q736(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q735(sender, e)
        End If
    End Sub
    ' --- Paste And Go ---
    Sub q737(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If Clipboard.ContainsText() = False Then Exit Sub
            Dim text As String = Clipboard.GetText.Trim
            If text = "" Then Exit Sub
            c.Text = text
            q1(text)
            su.Visible = True
            su.Text = "Clipboard text opened"
        Catch ex As Exception
        End Try
    End Sub
    Sub q738(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q737(sender, e)
        End If
    End Sub
    ' --- Copy Browser Diagnostics ---
    Sub q739(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            Dim title As String = wb.DocumentTitle
            Dim url As String = ""
            If wb.Url IsNot Nothing Then url = wb.Url.ToString
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine(ProductName & " " & Application.ProductVersion)
            sb.AppendLine("Title: " & title)
            sb.AppendLine("URL: " & url)
            sb.AppendLine("Tabs: " & k.TabPages.Count.ToString)
            sb.AppendLine("Homepage: " & My.Settings.h)
            sb.AppendLine("Search engine: " & My.Settings.se)
            sb.AppendLine("Private mode: " & If(pv, "On", "Off"))
            sb.AppendLine("Restore session: " & If(My.Settings.rs = "e", "On", "Off"))
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Browser diagnostics copied"
        Catch ex As Exception
        End Try
    End Sub
    Sub q740(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            q739(sender, e)
        End If
    End Sub
    Sub qlog(ByVal ex As Exception)
        Try
            If My.Settings.el <> "e" Then Exit Sub
            Dim path As String = My.Settings.ep
            If path = "" Then
                Dim folder As String = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firespider Internet")
                System.IO.Directory.CreateDirectory(folder)
                path = System.IO.Path.Combine(folder, "Firespider Error Log.txt")
                My.Settings.ep = path
                My.Settings.Save()
            End If
            System.IO.File.AppendAllText(path, Now.ToString("yyyy-MM-dd HH:mm:ss") & " - " & ex.Message & vbCrLf, System.Text.Encoding.UTF8)
        Catch
        End Try
    End Sub
    Function qvl(ByVal text As String) As Boolean
        Try
            text = text.Trim
            If text = "" OrElse text.Contains(" ") Then Return False
            If text.Contains(".") = False AndAlso text.Contains("://") = False AndAlso text.StartsWith("file:") = False Then Return False
            Dim candidate As String = text
            If candidate.Contains("://") = False AndAlso candidate.StartsWith("file:") = False Then candidate = "http://" & candidate
            Dim parsed As Uri = Nothing
            If Uri.TryCreate(candidate, UriKind.Absolute, parsed) = False Then Return False
            Return parsed.Scheme = Uri.UriSchemeHttp OrElse parsed.Scheme = Uri.UriSchemeHttps OrElse parsed.Scheme = Uri.UriSchemeFile
        Catch ex As Exception
            Return False
        End Try
    End Function
    Function qvurl(ByVal text As String) As Boolean
        Try
            If qvl(text) Then Return True
            Return text.StartsWith("about:", StringComparison.OrdinalIgnoreCase)
        Catch ex As Exception
            Return False
        End Try
    End Function
    Function qbu(ByVal raw As String) As String
        Try
            Dim split As Integer = raw.IndexOf("|"c)
            If split > 0 AndAlso split < raw.Length - 1 Then
                Dim possible As String = raw.Substring(split + 1)
                If qvurl(possible) Then Return possible
            End If
            Return raw
        Catch ex As Exception
            Return raw
        End Try
    End Function
    Function qbf(ByVal raw As String) As String
        Try
            Dim split As Integer = raw.IndexOf("|"c)
            If split > 0 AndAlso split < raw.Length - 1 Then
                Dim possible As String = raw.Substring(split + 1)
                If qvurl(possible) Then Return raw.Substring(0, split)
            End If
            Return ""
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Sub qbh(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then
            z2(CType(sender, ToolStripMenuItem), e)
        End If
    End Sub
    Sub qrb()
        Try
            RB.DropDownItems.Clear()
            Dim folders As New Dictionary(Of String, ToolStripMenuItem)(StringComparer.OrdinalIgnoreCase)
            For i As Integer = 0 To My.Settings.bm.Count - 1
                Dim raw As String = My.Settings.bm.Item(i)
                If raw = "" Then Continue For
                Dim url As String = qbu(raw)
                If qvurl(url) = False Then Continue For
                Dim folder As String = qbf(raw)
                Dim item As New ToolStripMenuItem
                item.Text = url
                item.Tag = url
                item.Image = My.Resources.F9
                item.BackgroundImage = My.Resources.F2
                item.BackgroundImageLayout = ImageLayout.Stretch
                item.ForeColor = Color.White
                AddHandler item.Click, AddressOf z2
                AddHandler item.MouseHover, AddressOf qbh
                If folder = "" Then
                    RB.DropDownItems.Add(item)
                Else
                    Dim parent As ToolStripMenuItem = Nothing
                    If folders.TryGetValue(folder, parent) = False Then
                        parent = New ToolStripMenuItem
                        parent.Text = folder
                        parent.Image = My.Resources.F9
                        parent.BackColor = Color.White
                        parent.ForeColor = Color.Black
                        parent.Padding = New Padding(0)
                        folders.Add(folder, parent)
                        RB.DropDownItems.Add(parent)
                    End If
                    parent.DropDownItems.Add(item)
                End If
            Next i
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub qash(ByVal text As String)
        Try
            text = text.Trim
            If text = "" OrElse text = "search" Then Exit Sub
            If x.Items.Contains(text) = False Then x.Items.Add(text)
            If My.Settings.s.Contains(text) = False Then
                My.Settings.s.Add(text)
                My.Settings.Save()
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub qad(ByVal path As String)
        Try
            If path = "" Then Exit Sub
            path = System.IO.Path.GetFullPath(path)
            If My.Settings.dl.Contains(path) Then My.Settings.dl.Remove(path)
            My.Settings.dl.Add(path)
            While My.Settings.dl.Count > 20
                My.Settings.dl.RemoveAt(0)
            End While
            My.Settings.Save()
            qtrackdownload(path)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub qacm()
        Try
            Dim compact As Boolean = My.Settings.cm = "e"
            x.Visible = Not compact
            s.Visible = Not compact
            y3.Visible = Not compact
            If compact Then
                si.Visible = False
                T1.Visible = False
                c.Size = New Size(CInt(Size.Width * 0.85), c.Height)
            ElseIf My.Settings.si <> "" Then
                si.Visible = True
                T1.Visible = True
                c.Size = New Size(CInt(Size.Width * 0.659), c.Height)
            Else
                c.Size = New Size(CInt(Size.Width * 0.69), c.Height)
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub qawl()
        Try
            If My.Settings.rws <> "e" OrElse My.Settings.ww = "" Then Exit Sub
            Dim leftValue As Integer = 0
            Dim topValue As Integer = 0
            Dim widthValue As Integer = 0
            Dim heightValue As Integer = 0
            If Integer.TryParse(My.Settings.wl, leftValue) = False OrElse Integer.TryParse(My.Settings.wt, topValue) = False OrElse Integer.TryParse(My.Settings.ww, widthValue) = False OrElse Integer.TryParse(My.Settings.wh, heightValue) = False Then Exit Sub
            If widthValue < 400 OrElse heightValue < 300 Then Exit Sub
            Dim saved As New Rectangle(leftValue, topValue, widthValue, heightValue)
            Dim visible As Boolean = False
            For Each display As Screen In Screen.AllScreens
                If display.WorkingArea.IntersectsWith(saved) Then visible = True
            Next display
            If visible = False Then Exit Sub
            StartPosition = FormStartPosition.Manual
            WindowState = FormWindowState.Normal
            Bounds = saved
            If My.Settings.ws = "Maximized" Then WindowState = FormWindowState.Maximized
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Function qsf(ByVal name As String) As String
        Try
            For Each bad As Char In System.IO.Path.GetInvalidFileNameChars()
                name = name.Replace(bad, "_"c)
            Next bad
            name = name.Trim
            If name = "" Then name = "Firespider Page"
            Return name
        Catch ex As Exception
            Return "Firespider Page"
        End Try
    End Function
    Function qsca(ByVal values As System.Collections.Specialized.StringCollection, ByVal value As String) As Boolean
        If value = "" OrElse values.Contains(value) Then Return False
        values.Add(value)
        Return True
    End Function
    ' --- Save Named Session ---
    Sub q741(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim name As String = InputBox("Enter a name for this tab session:", "Save Named Session", "Session " & Now.ToString("yyyy-MM-dd"))
            name = name.Trim.Replace("|", "-")
            If name = "" Then Exit Sub
            qsavesession(name)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q742(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q741(sender, e)
    End Sub
    ' --- Open Named Session ---
    Sub q743(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim names As New List(Of String)
            For Each entry As String In My.Settings.ns
                Dim split As Integer = entry.IndexOf("|"c)
                If split > 0 Then
                    Dim name As String = entry.Substring(0, split)
                    If names.Contains(name) = False Then names.Add(name)
                End If
            Next entry
            If names.Count = 0 Then
                su.Visible = True
                su.Text = "No named sessions saved"
                Exit Sub
            End If
            Dim prompt As String = "Enter a saved session name:" & vbCrLf & String.Join(vbCrLf, names.ToArray)
            Dim selected As String = InputBox(prompt, "Open Named Session", names(0))
            If selected = "" Then Exit Sub
            qopensession(selected)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q744(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q743(sender, e)
    End Sub
    ' --- Recently Visited Domains ---
    Sub q745(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim menu As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
            menu.DropDownItems.Clear()
            Dim found As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For i As Integer = f6.l0.Items.Count - 1 To 0 Step -1
                Dim parsed As Uri = Nothing
                If Uri.TryCreate(f6.l0.Items(i).ToString, UriKind.Absolute, parsed) AndAlso (parsed.Scheme = Uri.UriSchemeHttp OrElse parsed.Scheme = Uri.UriSchemeHttps) Then
                    If found.Add(parsed.Host) Then
                        Dim item As New ToolStripMenuItem
                        item.Text = parsed.Host
                        item.Tag = parsed.GetLeftPart(UriPartial.Authority) & "/"
                        item.Image = My.Resources.F9
                        item.BackColor = Color.White
                        item.ForeColor = Color.Black
                        item.Padding = New Padding(0)
                        AddHandler item.Click, AddressOf q746
                        AddHandler item.MouseHover, AddressOf q747
                        menu.DropDownItems.Add(item)
                    End If
                End If
            Next i
            If menu.DropDownItems.Count = 0 Then
                Dim empty As New ToolStripMenuItem("No recent domains")
                empty.Enabled = False
                menu.DropDownItems.Add(empty)
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q746(ByVal sender As Object, ByVal e As EventArgs)
        Try
            q1(CType(sender, ToolStripMenuItem).Tag.ToString)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q747(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q746(sender, e)
    End Sub
    ' --- Reopen Last Closed Window ---
    Sub q748(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim nw As j = qleaWindow(My.Settings.lcw.Cast(Of String)())
            su.Visible = True
            su.Text = If(nw Is Nothing, "No supported closed-window URLs", nw.k.TabCount.ToString & " Lea tab(s) reopened in a new window")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q749(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q748(sender, e)
    End Sub
    ' --- Duplicate Tabs From Same Domain ---
    Sub q750(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaDomainAction("dp")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q751(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q750(sender, e)
    End Sub
    ' --- Stop Media Or Busy Page ---
    Sub q752(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            wb.Stop()
            Try
                wb.Document.InvokeScript("eval", New Object() {"(function(){var t=['audio','video'];for(var a=0;a<t.length;a++){var m=document.getElementsByTagName(t[a]);for(var i=0;i<m.length;i++){try{m[i].pause();}catch(e){}}}})();"})
            Catch
            End Try
            su.Visible = True
            su.Text = "Page stopped and media paused"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q753(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q752(sender, e)
    End Sub
    ' --- Bookmark Current Page In Folder ---
    Sub q754(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim folder As String = InputBox("Enter bookmark folder name:", "Bookmark Folder", "General").Trim.Replace("|", "-")
            If folder = "" Then Exit Sub
            My.Settings.bm.Add(folder & "|" & wb.Url.ToString)
            My.Settings.Save()
            qrb()
            su.Visible = True
            su.Text = "Bookmark saved in " & folder
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q755(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q754(sender, e)
    End Sub
    ' --- Export All Browser Data ---
    Sub q756(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim save As New SaveFileDialog
            qsavefolder(save)
            save.FileName = "Firespider Browser Data.txt"
            save.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            save.Title = "Export All Browser Data"
            If save.ShowDialog(Me) <> DialogResult.OK OrElse save.FileName = "" Then Exit Sub
            Dim data As New System.Text.StringBuilder
            data.AppendLine("FIRESPIDER_BROWSER_DATA_V1")
            data.AppendLine("[Homepage]")
            data.AppendLine(My.Settings.h)
            data.AppendLine("[SearchEngine]")
            data.AppendLine(My.Settings.se)
            data.AppendLine("[RestoreSession]")
            data.AppendLine(My.Settings.rs)
            data.AppendLine("[Session]")
            For Each value As String In My.Settings.ls
                If value <> "" Then data.AppendLine(value)
            Next value
            data.AppendLine("[History]")
            For i As Integer = 0 To f6.l0.Items.Count - 1
                If f6.l0.Items(i).ToString <> "" Then data.AppendLine(f6.l0.Items(i).ToString)
            Next i
            data.AppendLine("[Bookmarks]")
            For Each value As String In My.Settings.bm
                If value <> "" Then data.AppendLine(value)
            Next value
            data.AppendLine("[Suggestions]")
            For Each value As String In My.Settings.s
                If value <> "" Then data.AppendLine(value)
            Next value
            data.AppendLine("[NamedSessions]")
            For Each value As String In My.Settings.ns
                If value <> "" Then data.AppendLine(value)
            Next value
            data.AppendLine("[Downloads]")
            For Each value As String In My.Settings.dl
                If value <> "" Then data.AppendLine(value)
            Next value
            data.AppendLine("[End]")
            System.IO.File.WriteAllText(save.FileName, data.ToString, System.Text.Encoding.UTF8)
            qad(save.FileName)
            su.Visible = True
            su.Text = "All browser data exported"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q757(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q756(sender, e)
    End Sub
    ' --- Import All Browser Data ---
    Sub q758(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim open As New OpenFileDialog
            open.Filter = "Text File [*.txt*]|*.txt|All Files [*.*]|*.*"
            open.Title = "Import All Browser Data"
            If open.ShowDialog(Me) <> DialogResult.OK OrElse open.FileName = "" Then Exit Sub
            Dim lines As String() = System.IO.File.ReadAllLines(open.FileName)
            If lines.Length = 0 OrElse lines(0).Trim <> "FIRESPIDER_BROWSER_DATA_V1" Then
                su.Visible = True
                su.Text = "Browser data file is not valid"
                Exit Sub
            End If
            Dim section As String = ""
            Dim imported As Integer = 0
            For i As Integer = 1 To lines.Length - 1
                Dim line As String = lines(i).Trim
                If line.StartsWith("[") AndAlso line.EndsWith("]") Then
                    section = line
                    Continue For
                End If
                If line = "" Then Continue For
                Select Case section
                    Case "[Homepage]"
                        If qvurl(line) Then My.Settings.h = line : imported += 1
                    Case "[SearchEngine]"
                        If line.Contains(" ") = False Then My.Settings.se = line : imported += 1
                    Case "[RestoreSession]"
                        If line = "e" OrElse line = "" Then My.Settings.rs = line : imported += 1
                    Case "[Session]"
                        If qvurl(line) AndAlso qsca(My.Settings.ls, line) Then imported += 1
                    Case "[History]"
                        If qvurl(line) AndAlso f6.l0.Items.Contains(line) = False Then
                            f6.l0.Items.Add(line)
                            c.Items.Add(line)
                            imported += 1
                        End If
                    Case "[Bookmarks]"
                        If qvurl(qbu(line)) AndAlso qsca(My.Settings.bm, line) Then imported += 1
                    Case "[Suggestions]"
                        If qsca(My.Settings.s, line) Then imported += 1
                    Case "[NamedSessions]"
                        Dim split As Integer = line.IndexOf("|"c)
                        If split > 0 AndAlso qvurl(line.Substring(split + 1)) AndAlso qsca(My.Settings.ns, line) Then imported += 1
                    Case "[Downloads]"
                        If System.IO.Path.IsPathRooted(line) AndAlso qsca(My.Settings.dl, line) Then imported += 1
                End Select
            Next i
            My.Settings.Save()
            qrb()
            For Each suggestion As String In My.Settings.s
                If suggestion <> "" AndAlso x.Items.Contains(suggestion) = False Then x.Items.Add(suggestion)
            Next suggestion
            qad(open.FileName)
            su.Visible = True
            su.Text = imported.ToString & " browser data item(s) imported"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q759(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q758(sender, e)
    End Sub
    ' --- Reader Text View ---
    Sub q760(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Document Is Nothing OrElse wb.Document.Body Is Nothing Then Exit Sub
            Dim title As String = If(wb.DocumentTitle = "", "Reader Text", wb.DocumentTitle)
            Dim url As String = If(wb.Url Is Nothing, "", wb.Url.ToString)
            Dim html As String = "<!doctype html><html><head><meta charset=""utf-8""><title>" & qh(title) & "</title>" &
                                 "<style>body{max-width:760px;margin:40px auto;padding:0 24px;font:18px/1.65 Georgia,serif;color:#222}pre{white-space:pre-wrap;font:inherit}a{color:#0645ad}</style></head><body>" &
                                 "<h1>" & qh(title) & "</h1><p><a href=""" & qh(url) & """>" & qh(url) & "</a></p><pre>" & qh(wb.Document.Body.InnerText) & "</pre></body></html>"
            Dim path As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Firespider Reader " & Guid.NewGuid().ToString("N") & ".html")
            System.IO.File.WriteAllText(path, html, System.Text.Encoding.UTF8)
            q3(New Uri(path).AbsoluteUri)
            su.Visible = True
            su.Text = "Reader text view opened"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q761(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q760(sender, e)
    End Sub
    ' --- Save Page Bundle ---
    Sub q762(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Document Is Nothing Then Exit Sub
            Dim folder As New FolderBrowserDialog
            qsavefolder(folder)
            folder.Description = "Choose a folder for the page bundle"
            If folder.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
            Dim defaultName As String = qsf(If(wb.DocumentTitle = "", "Firespider Page", wb.DocumentTitle))
            Dim baseName As String = InputBox("Enter the page bundle base filename:", "Save Page Bundle", defaultName)
            If baseName = "" Then Exit Sub
            baseName = qsf(baseName)
            Dim htmlPath As String = System.IO.Path.Combine(folder.SelectedPath, baseName & ".html")
            Dim textPath As String = System.IO.Path.Combine(folder.SelectedPath, baseName & ".txt")
            Dim linksPath As String = System.IO.Path.Combine(folder.SelectedPath, baseName & ".links.txt")
            System.IO.File.WriteAllText(htmlPath, wb.DocumentText, System.Text.Encoding.UTF8)
            Dim bodyText As String = If(wb.Document.Body Is Nothing, "", wb.Document.Body.InnerText)
            System.IO.File.WriteAllText(textPath, bodyText, System.Text.Encoding.UTF8)
            Dim links As New System.Text.StringBuilder
            For Each element As HtmlElement In wb.Document.Links
                Dim href As String = element.GetAttribute("href")
                If href <> "" Then links.AppendLine(href)
            Next element
            System.IO.File.WriteAllText(linksPath, links.ToString, System.Text.Encoding.UTF8)
            qad(htmlPath)
            qad(textPath)
            qad(linksPath)
            su.Visible = True
            su.Text = "Page bundle saved"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q763(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q762(sender, e)
    End Sub
    ' --- Save Page Images As HTML Gallery ---
    Sub q764(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Document Is Nothing Then Exit Sub
            Dim save As New SaveFileDialog
            qsavefolder(save)
            save.FileName = qsf(If(wb.DocumentTitle = "", "Firespider Image Gallery", wb.DocumentTitle)) & " Gallery.html"
            save.Filter = "HTML File [*.html*]|*.html|All Files [*.*]|*.*"
            save.Title = "Save Page Images As HTML Gallery"
            If save.ShowDialog(Me) <> DialogResult.OK OrElse save.FileName = "" Then Exit Sub
            Dim html As New System.Text.StringBuilder
            html.AppendLine("<!doctype html><html><head><meta charset=""utf-8""><title>Firespider Image Gallery</title>")
            html.AppendLine("<style>body{font-family:Segoe UI,Arial;background:#222;color:#fff;padding:24px}.gallery{display:flex;flex-wrap:wrap;gap:16px}.gallery a{background:#fff;padding:8px}.gallery img{display:block;max-width:240px;max-height:180px}</style></head><body>")
            html.AppendLine("<h1>" & qh(wb.DocumentTitle) & "</h1><div class=""gallery"">")
            Dim count As Integer = 0
            For Each element As HtmlElement In wb.Document.GetElementsByTagName("img")
                Dim src As String = element.GetAttribute("src")
                If src <> "" Then
                    html.AppendLine("<a href=""" & qh(src) & """><img src=""" & qh(src) & """ alt=""Page image""></a>")
                    count += 1
                End If
            Next element
            html.AppendLine("</div></body></html>")
            System.IO.File.WriteAllText(save.FileName, html.ToString, System.Text.Encoding.UTF8)
            qad(save.FileName)
            su.Visible = True
            su.Text = count.ToString & " image(s) saved in HTML gallery"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q765(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q764(sender, e)
    End Sub
    ' --- Private Mode On Startup ---
    Sub q766(ByVal sender As Object, ByVal e As EventArgs)
        Try
            My.Settings.ps = If(My.Settings.ps = "e", "", "e")
            My.Settings.Save()
            su.Visible = True
            su.Text = "Private mode on startup " & If(My.Settings.ps = "e", "enabled", "disabled")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q767(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q766(sender, e)
    End Sub
    Sub qse(ByVal url As String, ByVal label As String)
        Try
            My.Settings.se = url
            My.Settings.Save()
            su.Visible = True
            su.Text = label & " is now the default search engine"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q768(ByVal sender As Object, ByVal e As EventArgs)
        qse("https://www.bing.com/search?q=", "Bing")
    End Sub
    Sub q769(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q768(sender, e)
    End Sub
    Sub q770(ByVal sender As Object, ByVal e As EventArgs)
        qse("https://www.google.com/search?q=", "Google")
    End Sub
    Sub q771(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q770(sender, e)
    End Sub
    Sub q772(ByVal sender As Object, ByVal e As EventArgs)
        qse("https://duckduckgo.com/?q=", "DuckDuckGo")
    End Sub
    Sub q773(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q772(sender, e)
    End Sub
    Sub q774(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim url As String = InputBox("Enter a custom search URL with the query added at the end:", "Custom Search URL", My.Settings.se).Trim
            If url = "" Then Exit Sub
            If url.Contains(" ") Then
                su.Visible = True
                su.Text = "Custom search URL is not valid"
                Exit Sub
            End If
            If url.Contains("://") = False Then url = "https://" & url
            qse(url, "Custom search")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q775(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q774(sender, e)
    End Sub
    ' --- Downloads List ---
    Sub q776(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim menu As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
            menu.DropDownItems.Clear()
            For i As Integer = My.Settings.dl.Count - 1 To 0 Step -1
                Dim path As String = My.Settings.dl.Item(i)
                If path = "" Then Continue For
                Dim item As New ToolStripMenuItem
                item.Text = System.IO.Path.GetFileName(path)
                If item.Text = "" Then item.Text = path
                item.ToolTipText = path
                item.Tag = path
                item.Image = My.Resources.F9
                item.BackColor = Color.White
                item.ForeColor = Color.Black
                item.Padding = New Padding(0)
                AddHandler item.Click, AddressOf q777
                AddHandler item.MouseHover, AddressOf q778
                menu.DropDownItems.Add(item)
            Next i
            If menu.DropDownItems.Count = 0 Then
                Dim empty As New ToolStripMenuItem("No recent downloads")
                empty.Enabled = False
                menu.DropDownItems.Add(empty)
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q777(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim path As String = CType(sender, ToolStripMenuItem).Tag.ToString
            If System.IO.File.Exists(path) OrElse System.IO.Directory.Exists(path) Then
                Process.Start(path)
                su.Visible = True
                su.Text = "Download opened"
            Else
                su.Visible = True
                su.Text = "Downloaded file was not found"
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q778(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q777(sender, e)
    End Sub
    ' --- Clear Downloads List ---
    Sub q779(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If MessageBox.Show("Clear the downloads list? Downloaded files will not be deleted.", "Clear Downloads List", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
            My.Settings.dl.Clear()
            My.Settings.Save()
            su.Visible = True
            su.Text = "Downloads list cleared"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q780(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q779(sender, e)
    End Sub
    ' --- Save Current Page Shortcut ---
    Sub q781(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Url Is Nothing Then Exit Sub
            Dim save As New SaveFileDialog
            qsavefolder(save)
            save.FileName = qsf(If(wb.DocumentTitle = "", "Firespider Page", wb.DocumentTitle)) & ".url"
            save.Filter = "Internet Shortcut [*.url*]|*.url|All Files [*.*]|*.*"
            save.Title = "Save Current Page Shortcut"
            If save.ShowDialog(Me) <> DialogResult.OK OrElse save.FileName = "" Then Exit Sub
            System.IO.File.WriteAllText(save.FileName, "[InternetShortcut]" & vbCrLf & "URL=" & wb.Url.ToString & vbCrLf, System.Text.Encoding.Default)
            qad(save.FileName)
            su.Visible = True
            su.Text = "Page shortcut saved"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q782(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q781(sender, e)
    End Sub
    ' --- Increase Page Text Size ---
    Sub q783(ByVal sender As Object, ByVal e As EventArgs)
        Try
            pts = Math.Min(250, pts + 10)
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            wb.Document.Body.Style &= ";font-size:" & pts.ToString & "% !important;"
            su.Visible = True
            su.Text = "Page text size " & pts.ToString & "%"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q784(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q783(sender, e)
    End Sub
    ' --- Decrease Page Text Size ---
    Sub q785(ByVal sender As Object, ByVal e As EventArgs)
        Try
            pts = Math.Max(50, pts - 10)
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            wb.Document.Body.Style &= ";font-size:" & pts.ToString & "% !important;"
            su.Visible = True
            su.Text = "Page text size " & pts.ToString & "%"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q786(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q785(sender, e)
    End Sub
    ' --- High Contrast Page ---
    Sub q787(ByVal sender As Object, ByVal e As EventArgs)
        Try
            contrastOn = True
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Document Is Nothing OrElse wb.Document.Body Is Nothing Then Exit Sub
            wb.Document.Body.Style &= ";background-color:#000000 !important;color:#ffffff !important;"
            For Each link As HtmlElement In wb.Document.Links
                link.Style &= ";color:#ffff00 !important;"
            Next link
            su.Visible = True
            su.Text = "High contrast page enabled"
        Catch ex As Exception
            qlog(ex)
            su.Visible = True
            su.Text = "High contrast is not supported on this page"
        End Try
    End Sub
    Sub q788(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q787(sender, e)
    End Sub
    ' --- Copy Readable Page Summary ---
    Sub q789(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls.Item(0), WebBrowser)
            If wb.Document Is Nothing OrElse wb.Document.Body Is Nothing Then Exit Sub
            Dim text As String = wb.Document.Body.InnerText
            Dim words As String() = text.Split(New Char() {" "c, ChrW(9), ChrW(10), ChrW(13)}, StringSplitOptions.RemoveEmptyEntries)
            Dim headings As New List(Of String)
            For level As Integer = 1 To 6
                For Each heading As HtmlElement In wb.Document.GetElementsByTagName("h" & level.ToString)
                    If heading.InnerText IsNot Nothing AndAlso heading.InnerText.Trim <> "" AndAlso headings.Count < 6 Then headings.Add(heading.InnerText.Trim)
                Next heading
            Next level
            Dim preview As String = text.Replace(vbCrLf, " ").Replace(vbLf, " ").Replace(vbCr, " ").Trim
            If preview.Length > 500 Then preview = preview.Substring(0, 500)
            Dim sb As New System.Text.StringBuilder
            sb.AppendLine(If(wb.DocumentTitle = "", "Untitled page", wb.DocumentTitle))
            sb.AppendLine(If(wb.Url Is Nothing, "", wb.Url.ToString))
            sb.AppendLine("Word count: " & words.Length.ToString)
            If headings.Count > 0 Then
                sb.AppendLine("Headings:")
                For Each heading As String In headings
                    sb.AppendLine("- " & heading)
                Next heading
            End If
            sb.AppendLine("Preview: " & preview)
            Clipboard.SetText(sb.ToString.TrimEnd)
            su.Visible = True
            su.Text = "Readable page summary copied"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q790(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q789(sender, e)
    End Sub
    ' --- Tile Two Windows ---
    Sub q791(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim area As Rectangle = Screen.FromControl(Me).WorkingArea
            Dim leftWidth As Integer = area.Width \ 2
            StartPosition = FormStartPosition.Manual
            WindowState = FormWindowState.Normal
            Bounds = New Rectangle(area.Left, area.Top, leftWidth, area.Height)
            Dim nw As New j
            nw.StartPosition = FormStartPosition.Manual
            nw.Show()
            nw.WindowState = FormWindowState.Normal
            nw.Bounds = New Rectangle(area.Left + leftWidth, area.Top, area.Width - leftWidth, area.Height)
            su.Visible = True
            su.Text = "Two browser windows tiled"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q792(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q791(sender, e)
    End Sub
    ' --- Compact Toolbar Mode ---
    Sub q793(ByVal sender As Object, ByVal e As EventArgs)
        Try
            My.Settings.cm = If(My.Settings.cm = "e", "", "e")
            My.Settings.Save()
            qacm()
            su.Visible = True
            su.Text = "Compact toolbar " & If(My.Settings.cm = "e", "enabled", "disabled")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q794(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q793(sender, e)
    End Sub
    ' --- Remember Window Size ---
    Sub q795(ByVal sender As Object, ByVal e As EventArgs)
        Try
            My.Settings.rws = If(My.Settings.rws = "e", "", "e")
            My.Settings.Save()
            su.Visible = True
            su.Text = "Remember window size " & If(My.Settings.rws = "e", "enabled", "disabled")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q796(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q795(sender, e)
    End Sub
    ' --- Reset Window Layout ---
    Sub q797(ByVal sender As Object, ByVal e As EventArgs)
        Try
            My.Settings.wl = ""
            My.Settings.wt = ""
            My.Settings.ww = ""
            My.Settings.wh = ""
            My.Settings.ws = ""
            My.Settings.Save()
            StartPosition = FormStartPosition.WindowsDefaultLocation
            WindowState = FormWindowState.Maximized
            su.Visible = True
            su.Text = "Window layout reset"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q798(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q797(sender, e)
    End Sub
    ' --- Search Suggestions From History ---
    Sub q799(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim before As Integer = My.Settings.s.Count
            For i As Integer = 0 To f6.l0.Items.Count - 1
                qash(f6.l0.Items(i).ToString)
            Next i
            su.Visible = True
            su.Text = (My.Settings.s.Count - before).ToString & " history suggestion(s) added"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q800(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q799(sender, e)
    End Sub
    ' --- Search With Selected Engine Once ---
    Sub q801(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim term As String = InputBox("Enter search text:", "Search With Selected Engine Once", "")
            If term = "" Then Exit Sub
            Dim engine As String = InputBox("Choose Bing, Google, or DuckDuckGo:", "Select Search Engine", "DuckDuckGo").Trim.ToLower
            Dim url As String = ""
            If engine = "bing" OrElse engine = "b" Then
                url = "https://www.bing.com/search?q="
            ElseIf engine = "google" OrElse engine = "g" Then
                url = "https://www.google.com/search?q="
            ElseIf engine = "duckduckgo" OrElse engine = "duck" OrElse engine = "d" Then
                url = "https://duckduckgo.com/?q="
            Else
                su.Visible = True
                su.Text = "Search engine was not recognized"
                Exit Sub
            End If
            CType(k.SelectedTab.Controls.Item(0), WebBrowser).Navigate(url & Uri.EscapeDataString(term))
            su.Visible = True
            su.Text = "One-time search opened"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q802(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q801(sender, e)
    End Sub
    ' --- Save Error Log ---
    Sub q803(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.el = "e" Then
                My.Settings.el = ""
                My.Settings.Save()
                su.Visible = True
                su.Text = "Error logging disabled"
                Exit Sub
            End If
            If My.Settings.ep = "" Then
                Dim folder As String = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firespider Internet")
                System.IO.Directory.CreateDirectory(folder)
                My.Settings.ep = System.IO.Path.Combine(folder, "Firespider Error Log.txt")
            End If
            My.Settings.el = "e"
            My.Settings.Save()
            Try
                Throw New ApplicationException("Error logging enabled test")
            Catch testError As Exception
                qlog(testError)
            End Try
            su.Visible = True
            su.Text = "Error logging enabled"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q804(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q803(sender, e)
    End Sub
    ' --- Clear Old History Items ---
    Sub q805(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim answer As String = InputBox("How many recent history items should be kept?", "Clear Old History Items", "25")
            If answer = "" Then Exit Sub
            Dim keep As Integer = 0
            If Integer.TryParse(answer, keep) = False OrElse keep < 0 Then
                su.Visible = True
                su.Text = "History keep count is not valid"
                Exit Sub
            End If
            Dim removed As Integer = 0
            While f6.l0.Items.Count > keep
                f6.l0.Items.RemoveAt(f6.l0.Items.Count - 1)
                removed += 1
            End While
            c.Items.Clear()
            For i As Integer = 0 To f6.l0.Items.Count - 1
                c.Items.Add(f6.l0.Items(i).ToString)
            Next i
            su.Visible = True
            su.Text = removed.ToString & " old history item(s) cleared"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q806(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q805(sender, e)
    End Sub
    ' --- Validate Saved Settings ---
    Sub q807(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim badBookmarks As New List(Of Integer)
            Dim badSession As New List(Of Integer)
            Dim badLastWindow As New List(Of Integer)
            Dim badSuggestions As New List(Of Integer)
            Dim badNamed As New List(Of Integer)
            Dim badDownloads As New List(Of Integer)
            For i As Integer = 0 To My.Settings.bm.Count - 1
                If My.Settings.bm.Item(i).Trim = "" OrElse qvurl(qbu(My.Settings.bm.Item(i))) = False Then badBookmarks.Add(i)
            Next i
            For i As Integer = 0 To My.Settings.ls.Count - 1
                If qvurl(My.Settings.ls.Item(i)) = False Then badSession.Add(i)
            Next i
            For i As Integer = 0 To My.Settings.lcw.Count - 1
                If qvurl(My.Settings.lcw.Item(i)) = False Then badLastWindow.Add(i)
            Next i
            For i As Integer = 0 To My.Settings.s.Count - 1
                If My.Settings.s.Item(i).Trim = "" Then badSuggestions.Add(i)
            Next i
            For i As Integer = 0 To My.Settings.ns.Count - 1
                Dim entry As String = My.Settings.ns.Item(i)
                Dim split As Integer = entry.IndexOf("|"c)
                If split <= 0 OrElse split = entry.Length - 1 OrElse qvurl(entry.Substring(split + 1)) = False Then badNamed.Add(i)
            Next i
            For i As Integer = 0 To My.Settings.dl.Count - 1
                If My.Settings.dl.Item(i).Trim = "" OrElse System.IO.Path.IsPathRooted(My.Settings.dl.Item(i)) = False Then badDownloads.Add(i)
            Next i
            Dim badHomepage As Boolean = qvurl(My.Settings.h) = False
            Dim badSearch As Boolean = My.Settings.se.Trim = "" OrElse My.Settings.se.Contains(" ")
            Dim count As Integer = badBookmarks.Count + badSession.Count + badLastWindow.Count + badSuggestions.Count + badNamed.Count + badDownloads.Count + If(badHomepage, 1, 0) + If(badSearch, 1, 0)
            If count = 0 Then
                su.Visible = True
                su.Text = "Saved settings are valid"
                Exit Sub
            End If
            If MessageBox.Show("Remove or reset " & count.ToString & " invalid saved setting item(s)?", "Validate Saved Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
            For i As Integer = badBookmarks.Count - 1 To 0 Step -1
                My.Settings.bm.RemoveAt(badBookmarks(i))
            Next i
            For i As Integer = badSession.Count - 1 To 0 Step -1
                My.Settings.ls.RemoveAt(badSession(i))
            Next i
            For i As Integer = badLastWindow.Count - 1 To 0 Step -1
                My.Settings.lcw.RemoveAt(badLastWindow(i))
            Next i
            For i As Integer = badSuggestions.Count - 1 To 0 Step -1
                My.Settings.s.RemoveAt(badSuggestions(i))
            Next i
            For i As Integer = badNamed.Count - 1 To 0 Step -1
                My.Settings.ns.RemoveAt(badNamed(i))
            Next i
            For i As Integer = badDownloads.Count - 1 To 0 Step -1
                My.Settings.dl.RemoveAt(badDownloads(i))
            Next i
            If badHomepage Then My.Settings.h = "http://fsin.glitch.me/"
            If badSearch Then My.Settings.se = "https://www.bing.com/search?q="
            My.Settings.Save()
            qrb()
            su.Visible = True
            su.Text = count.ToString & " invalid setting item(s) cleaned"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q808(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q807(sender, e)
    End Sub
End Class
