Partial Class j
    Private qps As System.Drawing.Printing.PrinterSettings

    Private Function qpr() As Boolean
        If IsDisposed OrElse Disposing Then Return True
        If InvokeRequired Then Throw New InvalidOperationException("Printing requires the browser UI thread")
        If qpb Then Return True
        Dim tab As TabPage = qleaSelectedTab()
        If tab Is Nothing OrElse tab.Controls.Count = 0 Then Return True
        Dim view As Global.Lea.Win.V = qleaHost(tab)
        If view Is Nothing Then
            If TypeOf tab.Controls(0) Is WebBrowser Then Return False
            su.Visible = True
            su.Text = "Printing unavailable for this tab"
            Return True
        End If
        qpb = True
        Dim submissionAttempted As Boolean = False
        Try
            If view.IsDisposed OrElse Not view.IsHandleCreated OrElse view.Busy OrElse view.At Is Nothing Then Throw New InvalidOperationException("Page is not ready for printing")
            Dim page = view.At
            Using selection As New Global.Lea.Print.Pj(Global.Lea.Print.Pw.MaxPages, qps)
                If Not selection.Sh(Me) Then
                    su.Visible = True
                    su.Text = "Printing canceled"
                    Return True
                End If
                If k.SelectedTab IsNot tab OrElse qleaHost(tab) IsNot view OrElse view.IsDisposed OrElse Not Object.ReferenceEquals(view.At, page) Then Throw New InvalidOperationException("Page changed during printer selection")
                qps = selection.St
                Dim options As New Global.Lea.Print.Po(False, 0.75, 100, False, True, True, False)
                Using prepared = selection.Cp(options)
                    Dim capture = Global.Lea.Print.Pc.Cs(view, prepared.Geometry)
                    Using plan As Global.Lea.Print.Pw = Global.Lea.Print.Pw.Dc(capture.Document, prepared.Geometry)
                        Dim address As String = ""
                        Dim uri As Uri = Nothing
                        If Uri.TryCreate(capture.Document.Source, UriKind.Absolute, uri) AndAlso (uri.Scheme = "http" OrElse uri.Scheme = "https") Then address = capture.Document.Source
                        Dim pages As New Global.Lea.Print.Pp(plan, capture.Document.Title, address)
                        prepared.Rg(pages)
                        submissionAttempted = True
                        prepared.Wr(pages)
                    End Using
                End Using
            End Using
            su.Visible = True
            su.Text = "Print job submitted"
        Catch ex As Exception
            qlog(ex)
            su.Visible = True
            su.Text = If(submissionAttempted, "Print status uncertain; check printer queue", "Print unavailable for this page")
        Finally
            qpb = False
        End Try
        Return True
    End Function
End Class
