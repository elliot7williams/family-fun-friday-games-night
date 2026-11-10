Public Class F7
    Dim Int As Integer = -77539
    Sub o5(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        x1.DataSource = My.Settings.bm
    End Sub
    Sub o6(ByVal sender As Object, ByVal e As EventArgs) Handles E.Click
        Try
            Close()
        Catch ex As Exception
        End Try
    End Sub
    Sub o7(ByVal sender As Object, ByVal e As EventArgs) Handles E.MouseHover
        If My.Settings.t = "e" Then
            Try
                Close()
            Catch ex As Exception
            End Try
        End If
    End Sub
    Sub o8(ByVal sender As Object, ByVal e As EventArgs) Handles re.Click
        Try
            x1.SelectedItems.Clear()
        Catch ex As Exception
        End Try
    End Sub
    Sub o9(ByVal sender As Object, ByVal e As EventArgs) Handles re.MouseHover
        If My.Settings.t = "e" Then
            Try
                x1.SelectedItems.Clear()
            Catch ex As Exception
            End Try
        End If
    End Sub
    Sub p1(ByVal sender As Object, ByVal e As EventArgs) Handles x1.DoubleClick
        Try
            j.q1(CStr(x1.SelectedItem))
        Catch ex As Exception
        End Try
    End Sub
    Dim It As Integer = 0
    Sub p2(ByVal sender As Object, ByVal e As EventArgs) Handles ca.Click
        Try
            My.Settings.bm.Clear()
            Dim f As New F7
            f.Show()
            Close()
        Catch ex As Exception
        End Try
    End Sub
    Sub p3(ByVal sender As Object, ByVal e As EventArgs) Handles ca.MouseHover
        If My.Settings.t = "e" Then
            Try
                My.Settings.bm.Clear()
                Dim f As New F7
                f.Show()
                Close()
            Catch ex As Exception
            End Try
        End If
    End Sub
    Dim nt As Integer = -1119769811
End Class