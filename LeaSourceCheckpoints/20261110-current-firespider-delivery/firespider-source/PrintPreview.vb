Partial Class j
    Private qpb As Boolean

    Private Function qpv() As Boolean
        If IsDisposed OrElse Disposing Then Return True
        If InvokeRequired Then Throw New InvalidOperationException("Preview requires the browser UI thread")
        Dim tab As TabPage = qleaSelectedTab()
        If tab Is Nothing OrElse tab.Controls.Count = 0 Then Return True
        Dim view As Global.Lea.Win.V = qleaHost(tab)
        If view Is Nothing Then
            If TypeOf tab.Controls(0) Is WebBrowser Then Return False
            su.Visible = True
            su.Text = "Preview unavailable for this tab"
            Return True
        End If
        If qpb Then Return True
        qpb = True
        Try
            Dim options As New Global.Lea.Print.Po(False, 0.75, 100, False, True, True, False)
            Dim geometry = Global.Lea.Print.Pg.Cp(options, 850, 1100, New Global.Lea.Print.Pb(0, 0, 850, 1100))
            Dim capture = Global.Lea.Print.Pc.Cs(view, geometry)
            Using plan As Global.Lea.Print.Pw = Global.Lea.Print.Pw.Dc(capture.Document, geometry)
                If plan.UnresolvedImages <> 0 Then Throw New InvalidOperationException("Page images are not ready for preview")
                Dim address As String = ""
                Dim uri As Uri = Nothing
                If Uri.TryCreate(capture.Document.Source, UriKind.Absolute, uri) AndAlso (uri.Scheme = "http" OrElse uri.Scheme = "https") Then address = capture.Document.Source
                Dim pages As New Global.Lea.Print.Pp(plan, capture.Document.Title, address)
                Using preview As New Global.Lea.Print.Ph(pages)
                    preview.ShowDialog(Me)
                End Using
            End Using
            su.Visible = True
            su.Text = "Preview closed"
        Catch ex As Exception
            qlog(ex)
            su.Visible = True
            su.Text = "Preview unavailable for this page"
        Finally
            qpb = False
        End Try
        Return True
    End Function
End Class
