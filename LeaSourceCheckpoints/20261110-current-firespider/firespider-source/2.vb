Class f2
    Dim Int As Integer = -77539
    Sub l3(ByVal sender As System.Object, ByVal e As EventArgs) Handles u.Click
        u.Text = "Checking For Updates"
        M()
    End Sub
    Sub M()
        Try
            Dim f As String = Application.StartupPath & "/v"
            Dim m As String = ("17.0.0.0")
            If My.Computer.FileSystem.FileExists(f) Then
                My.Computer.FileSystem.DeleteFile(f)
            End If
            My.Computer.Network.DownloadFile("http://firespiderinternet.weebly.com/files/theme/v", f)
            Dim x As String = My.Computer.FileSystem.ReadAllText(f)
            If m = x Then
                u.Text = "None Available"
                A1.Start()
            ElseIf m IsNot x Then
                u.Text = "New Update Available!"
                My.Computer.Network.DownloadFile("http://firespiderinternet.weebly.com/files/theme/FirespiderInternet.exe", "Firespider Internet 17.exe")
                Application.Restart()
            End If
        Catch ex As Exception
        End Try
    End Sub
    Sub l4(ByVal sender As System.Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles ll.LinkClicked
        j.qln("http://firespiderinternet.weebly.com/")
    End Sub
    Sub N()
        j.N.PerformClick()
    End Sub
    Sub l5(ByVal sender As System.Object, ByVal e As EventArgs) Handles A1.Tick
        u.Text = "Check For Updates"
    End Sub
    Dim It As Integer = 0
    Sub l6(ByVal sender As Object, ByVal e As EventArgs) Handles u.MouseHover
        If My.Settings.t = "e" Then
            u.Text = "Checking For Updates"
            M()
        End If
    End Sub
    Sub l7(ByVal sender As System.Object, ByVal e As EventArgs) Handles MyBase.Load
        pi1.Image = My.Resources.O3
    End Sub
    Sub l8(ByVal sender As System.Object, ByVal e As EventArgs) Handles pi1.Click
        j.qln("http://firespiderinternet.weebly.com/")
    End Sub
    Sub l9(ByVal sender As Object, ByVal e As EventArgs) Handles pi1.MouseHover
        If My.Settings.t = "e" Then
            j.qln("http://firespiderinternet.weebly.com/")
        End If
    End Sub
    Sub m1(ByVal sender As Object, ByVal e As EventArgs) Handles ll.MouseHover
        If My.Settings.t = "e" Then
            j.qln("http://firespiderinternet.weebly.com/")
        End If
    End Sub
    Dim nt As Integer = -1119769811
End Class