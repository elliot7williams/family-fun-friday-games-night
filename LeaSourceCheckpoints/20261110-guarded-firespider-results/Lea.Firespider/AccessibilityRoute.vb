    Private Function qleaaccessroute() As Boolean
        Dim tab = qleaSelectedTab()
        If tab IsNot Nothing AndAlso tab.Controls.Count > 0 AndAlso TypeOf tab.Controls(0) Is WebBrowser Then Return False
        Dim host = qleaAt()
        If host Is Nothing OrElse host.IsDisposed OrElse host.Disposing OrElse host.Busy OrElse host.Le IsNot Nothing OrElse host.At Is Nothing Then
            su.Visible = True
            su.Text = "Select a ready Lea page for Accessibility Inspector"
            Return True
        End If
        Dim issues As Integer = 0
        Dim html As String = qleaaccesshtml(host.At, issues)
        Dim reportTab As New TabPage("Accessibility Inspector")
        Dim report As Global.Lea.Win.V = qleaMount(reportTab, pv OrElse host.Pv)
        Dim previous = tab
        Try
            k.TabPages.Add(reportTab)
            I = k.TabPages.Count
            report.Ht(html)
            k.SelectedTab = reportTab
            qleaState(report)
        Catch
            If k.TabPages.Contains(reportTab) Then k.TabPages.Remove(reportTab)
            qleaRelease(report)
            reportTab.Dispose()
            I = k.TabPages.Count
            If previous IsNot Nothing AndAlso k.TabPages.Contains(previous) Then k.SelectedTab = previous
            Throw
        End Try
        su.Visible = True
        su.Text = issues.ToString() & " potential accessibility issue(s) reported"
        Return True
    End Function
