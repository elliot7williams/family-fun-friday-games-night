Public Class f6
    Dim Int As Integer = -77539
    Sub n9(ByVal sender As System.Object, ByVal e As EventArgs) Handles z.Click
        l0.Items.Clear()
        j.c.Items.Clear()
    End Sub
    Sub o1(ByVal sender As System.Object, ByVal e As EventArgs) Handles b.Click
        Close()
    End Sub
    Sub o2(ByVal sender As Object, ByVal e As EventArgs) Handles b.MouseHover
        If My.Settings.t = "e" Then
            Close()
        End If
    End Sub
    Dim It As Integer = 0
    Sub o3(ByVal sender As Object, ByVal e As EventArgs) Handles z.MouseHover
        If My.Settings.t = "e" Then
            l0.Items.Clear()
            j.c.Items.Clear()
        End If
    End Sub
    Sub o4(ByVal sender As Object, ByVal e As EventArgs) Handles l0.DoubleClick
        Try
            j.q1(CStr(l0.SelectedItem))
        Catch ex As Exception
        End Try
    End Sub
    Dim nt As Integer = -1119769811
End Class