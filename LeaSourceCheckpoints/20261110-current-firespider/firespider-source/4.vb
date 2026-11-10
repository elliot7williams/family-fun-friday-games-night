Public Class F4
    Dim Int As Integer = -77539
    Private mx As Integer = 8
    Private Const TM As Integer = 3
    Sub m2(ByVal sender As System.Object, ByVal e As EventArgs) Handles B1.Click
        j.q.BackgroundImage = CType(None, Image)
        Dim Co As New ColorDialog
        Co.ShowDialog()
        j.q.BackgroundImage = Nothing
        j.q.BackColor = Co.Color
        j.fl.BackgroundImage = Nothing
        j.fl.BackColor = Co.Color
        j.e.BackgroundImage = Nothing
        j.e.BackColor = Co.Color
        j.v.BackgroundImage = Nothing
        j.v.BackColor = Co.Color
        j.hi.BackgroundImage = Nothing
        j.hi.BackColor = Co.Color
        j.bo.BackgroundImage = Nothing
        j.bo.BackColor = Co.Color
        j.ot.BackgroundImage = Nothing
        j.ot.BackColor = Co.Color
        j.he.BackgroundImage = Nothing
        j.he.BackColor = Co.Color
        j.c.BackColor = Co.Color
        j.x.BackColor = Co.Color
        ''j.SH.BackgroundImage = Nothing
        ''j.SH.BackColor = Co.Color
        My.Settings.i = Co.Color
    End Sub
    Sub m3(ByVal sender As System.Object, ByVal e As ScrollEventArgs) Handles HSB1.Scroll
        If HSB1.Value = 1 Then
            j.si.Visible = False
            j.T1.Visible = False
            j.c.Size = New Size(CInt(j.Size.Width * 0.6884375), j.c.Size.Width)
            O2.ForeColor = Color.Black
            O1.ForeColor = Color.White
            F1.Text = "Turn Siera On"
            My.Settings.si = ""
        ElseIf HSB1.Value = 0 Then
            j.si.Visible = True
            j.T1.Visible = True
            j.c.Size = New Size(CInt(j.Size.Width * 0.66734375), j.c.Size.Width)
            O1.ForeColor = Color.Black
            O2.ForeColor = Color.White
            F1.Text = "Turn Siera Off"
            My.Settings.si = "e"
        End If
    End Sub
    Sub m4(ByVal sender As Object, ByVal e As DrawItemEventArgs) Handles TB1.DrawItem
        Dim CurrentTab As TabPage = TB1.TabPages(e.Index)
        Dim ItemRect As Rectangle = TB1.GetTabRect(e.Index)
        Dim FBr As New SolidBrush(Color.Black)
        Dim TBr As New SolidBrush(Color.Black)
        Dim sf As New StringFormat
        sf.Alignment = StringAlignment.Center
        sf.LineAlignment = StringAlignment.Center
        If CBool(e.State And DrawItemState.Selected) Then
            FBr.Color = Color.Black
            TBr.Color = Color.Black
            ItemRect.Inflate(1, 1)
        End If
        If TB1.Alignment = TabAlignment.Left Or TB1.Alignment = TabAlignment.Right Then
            Dim RotateAngle As Single = 90
            If TB1.Alignment = TabAlignment.Left Then RotateAngle = 270
            Dim cp As New PointF(CSng(ItemRect.Left + (ItemRect.Width / 2)), ItemRect.Top + (ItemRect.Height \ 2))
            e.Graphics.TranslateTransform(cp.X, cp.Y)
            e.Graphics.RotateTransform(RotateAngle)
            ItemRect = New Rectangle(-(ItemRect.Height \ 2), -(ItemRect.Width \ 2), ItemRect.Height, ItemRect.Width)
        End If
        Dim tab_rect As Rectangle = TB1.GetTabRect(e.Index)
        Dim layout_rect As New RectangleF(
            tab_rect.Left + TM,
            tab_rect.Y + TM,
            tab_rect.Width - 4 * TM,
            tab_rect.Height - 2 * TM)
        Dim string_format As New StringFormat
        e.Graphics.DrawImage(My.Resources.F2, ItemRect)
        Dim big_font As New Font(Font, FontStyle.Bold)
        string_format.Alignment = StringAlignment.Center
        string_format.LineAlignment = StringAlignment.Center
        e.Graphics.DrawString(
            TB1.TabPages(e.Index).Text,
            big_font,
            FBr,
            layout_rect,
            string_format)
        e.Graphics.ResetTransform()
        FBr.Dispose()
        TBr.Dispose()
    End Sub
    Sub m5(ByVal sender As System.Object, ByVal e As ScrollEventArgs) Handles HSB2.Scroll
        If HSB2.Value = 1 Then
            My.Settings.t = ""
            L4.ForeColor = Color.Black
            L3.ForeColor = Color.White
            L2.Text = "Turn Hover Technology On"
        ElseIf HSB2.Value = 0 Then
            My.Settings.t = "e"
            L3.ForeColor = Color.Black
            L4.ForeColor = Color.White
            L2.Text = "Turn Hover Technology Off"
        End If
    End Sub
    Sub m6(ByVal sender As Object, ByVal e As EventArgs) Handles B1.MouseHover
        If My.Settings.t = "e" Then
            j.q.BackgroundImage = CType(None, Image)
            Dim Co As New ColorDialog
            Co.ShowDialog()
            j.q.BackColor = Co.Color
            j.fl.BackColor = Co.Color
            j.e.BackColor = Co.Color
            j.v.BackColor = Co.Color
            j.hi.BackColor = Co.Color
            j.bo.BackColor = Co.Color
            j.ot.BackColor = Co.Color
            j.he.BackColor = Co.Color
            j.c.BackColor = Co.Color
            j.x.BackColor = Co.Color
            j.SH.BackColor = Co.Color
            j.FC.BackColor = Co.Color
            j.RP.BackColor = Co.Color
            j.RC.BackColor = Co.Color
            j.RW.BackColor = Co.Color
            j.PI.BackColor = Co.Color
            j.PB.BackColor = Co.Color
            j.CR.BackColor = Co.Color
            j.O.BackColor = Co.Color
            j.A.BackColor = Co.Color
            j.HT.BackColor = Co.Color
            j.N.BackColor = Co.Color
            j.NW.BackColor = Co.Color
            j.OP.BackColor = Co.Color
            j.CT.BackColor = Co.Color
            j.CW.BackColor = Co.Color
            j.SP.BackColor = Co.Color
            j.SL.BackColor = Co.Color
            j.PS.BackColor = Co.Color
            j.PP.BackColor = Co.Color
            j.P.BackColor = Co.Color
            j.W.BackColor = Co.Color
            j.ET.BackColor = Co.Color
            j.U.BackColor = Co.Color
            j.RD.BackColor = Co.Color
            j.CU.BackColor = Co.Color
            j.CO.BackColor = Co.Color
            j.PA.BackColor = Co.Color
            j.D.BackColor = Co.Color
            j.SA.BackColor = Co.Color
            j.FI.BackColor = Co.Color
            j.z.BackColor = Co.Color
            j.zi.BackColor = Co.Color
            j.zo.BackColor = Co.Color
            j.es.BackColor = Co.Color
            j.zt.BackColor = Co.Color
            j.ts.BackColor = Co.Color
            j.ce.BackColor = Co.Color
            j.pt.BackColor = Co.Color
            j.fs.BackColor = Co.Color
            j.st.BackColor = Co.Color
            j.re.BackColor = Co.Color
            j.SB.BackColor = Co.Color
            j.BP.BackColor = Co.Color
            j.TP.BackColor = Co.Color
            j.OB.BackColor = Co.Color
            j.RB.BackColor = Co.Color
            j.RT.BackColor = Co.Color
            My.Settings.i = Co.Color
        End If
    End Sub
    Dim It As Integer = 0
    Sub m7(ByVal sender As System.Object, ByVal e As EventArgs) Handles L3.Click
        HSB2.Value = 1
        My.Settings.t = ""
        L4.ForeColor = Color.Black
        L3.ForeColor = Color.White
        L2.Text = "Turn Hover Technology On"
    End Sub
    Sub m8(ByVal sender As System.Object, ByVal e As EventArgs) Handles L4.Click
        HSB2.Value = 0
        My.Settings.t = "e"
        L3.ForeColor = Color.Black
        L4.ForeColor = Color.White
        L2.Text = "Turn Hover Technology Off"
    End Sub
    Sub m9(ByVal sender As System.Object, ByVal e As EventArgs) Handles O2.Click
        HSB1.Value = 0
        j.si.Visible = True
        j.T1.Visible = True
        j.c.Size = New Size(CInt(j.Size.Width * 0.66734375), j.c.Size.Width)
        O1.ForeColor = Color.Black
        O2.ForeColor = Color.White
        F1.Text = "Turn Siera Off"
        My.Settings.si = "e"
    End Sub
    Sub n1(ByVal sender As System.Object, ByVal e As EventArgs) Handles O1.Click
        HSB1.Value = 1
        j.si.Visible = False
        j.T1.Visible = False
        j.c.Size = New Size(CInt(j.Size.Width * 0.6884375), j.c.Size.Width)
        O2.ForeColor = Color.Black
        O1.ForeColor = Color.White
        F1.Text = "Turn Siera On"
        My.Settings.si = ""
    End Sub
    Sub n2(ByVal sender As System.Object, ByVal e As EventArgs) Handles MyBase.Load
        If My.Settings.t = "" Then
            HSB2.Value = 1
            L4.ForeColor = Color.Black
            L3.ForeColor = Color.White
            L2.Text = "Turn Hover Technology On"
        End If
    End Sub
    Sub n3(ByVal sender As System.Object, ByVal e As EventArgs) Handles Search.SelectedIndexChanged
        If Search.SelectedItem Is "Bing" Then
            My.Settings.se = "www.bing.com/search?q="
        ElseIf Search.SelectedItem Is "Google" Then
            My.Settings.se = "www.google.ca/search?q="
        ElseIf Search.SelectedItem Is "Yahoo" Then
            My.Settings.se = "search.yahoo.com/search?p="
        ElseIf Search.SelectedItem Is "Ask" Then
            My.Settings.se = "www.ask.com/search?q="
        ElseIf Search.SelectedItem Is "DuckDuckGo" Then
            My.Settings.se = "duckduckgo.com/?q="
        End If
    End Sub
    Sub n4(ByVal sender As Object, ByVal e As KeyEventArgs) Handles CB1.KeyUp
        My.Settings.h = CB1.Text
    End Sub
    Sub n5(ByVal sender As Object, ByVal e As EventArgs) Handles O1.MouseHover
        If My.Settings.t = "e" Then
            HSB1.Value = 1
            j.si.Visible = False
            j.T1.Visible = False
            j.c.Size = New Size(CInt(j.Size.Width * 0.6884375), j.c.Size.Width)
            O2.ForeColor = Color.Black
            O1.ForeColor = Color.White
            F1.Text = "Turn Siera On"
            My.Settings.si = ""
        End If
    End Sub
    Sub n6(ByVal sender As Object, ByVal e As EventArgs) Handles O2.MouseHover
        If My.Settings.t = "e" Then
            HSB1.Value = 0
            j.si.Visible = True
            j.T1.Visible = True
            j.c.Size = New Size(CInt(j.Size.Width * 0.66734375), j.c.Size.Width)
            O1.ForeColor = Color.Black
            O2.ForeColor = Color.White
            F1.Text = "Turn Siera Off"
            My.Settings.si = "e"
        End If
    End Sub
    Sub n7(ByVal sender As Object, ByVal e As EventArgs) Handles L3.MouseHover
        If My.Settings.t = "e" Then
            HSB2.Value = 1
            My.Settings.t = ""
            L4.ForeColor = Color.Black
            L3.ForeColor = Color.White
            L2.Text = "Turn Hover Technology On"
        End If
    End Sub
    Sub n8(ByVal sender As Object, ByVal e As EventArgs) Handles L4.MouseHover
        If My.Settings.t = "e" Then
            HSB2.Value = 0
            My.Settings.t = "e"
            L3.ForeColor = Color.Black
            L4.ForeColor = Color.White
            L2.Text = "Turn Hover Technology Off"
        End If
    End Sub
    Dim nt As Integer = -1119769811
End Class