<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F3
    Inherits System.Windows.Forms.Form
    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F3))
        Me.RN = New System.Windows.Forms.RichTextBox()
        Me.SuspendLayout()
        '
        'RN
        '
        Me.RN.BackColor = System.Drawing.Color.Black
        Me.RN.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.RN.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RN.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RN.ForeColor = System.Drawing.Color.DimGray
        Me.RN.Location = New System.Drawing.Point(0, 0)
        Me.RN.Name = "RN"
        Me.RN.ReadOnly = True
        Me.RN.Size = New System.Drawing.Size(765, 435)
        Me.RN.TabIndex = 0
        Me.RN.Text = ""
        '
        'F3
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Black
        Me.ClientSize = New System.Drawing.Size(765, 435)
        Me.Controls.Add(Me.RN)
        Me.ForeColor = System.Drawing.Color.White
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimizeBox = False
        Me.Name = "F3"
        Me.Opacity = 0.97R
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Page Source"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents RN As System.Windows.Forms.RichTextBox
End Class