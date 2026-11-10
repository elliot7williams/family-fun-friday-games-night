<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class f2
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(f2))
        Me.k1 = New System.Windows.Forms.Label()
        Me.l2 = New System.Windows.Forms.Label()
        Me.x1 = New System.Windows.Forms.ListBox()
        Me.pi1 = New System.Windows.Forms.PictureBox()
        Me.l1 = New System.Windows.Forms.Label()
        Me.u = New System.Windows.Forms.Button()
        Me.ll = New System.Windows.Forms.LinkLabel()
        Me.le = New System.Windows.Forms.Label()
        Me.A1 = New System.Windows.Forms.Timer(Me.components)
        CType(Me.pi1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'k1
        '
        Me.k1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.k1.AutoSize = True
        Me.k1.BackColor = System.Drawing.Color.Transparent
        Me.k1.Font = New System.Drawing.Font("Arial", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.k1.ForeColor = System.Drawing.Color.White
        Me.k1.Location = New System.Drawing.Point(61, 8)
        Me.k1.Margin = New System.Windows.Forms.Padding(0)
        Me.k1.Name = "k1"
        Me.k1.Size = New System.Drawing.Size(350, 24)
        Me.k1.TabIndex = 1
        Me.k1.Text = "Welcome to Firespider Internet 17"
        '
        'l2
        '
        Me.l2.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.l2.AutoSize = True
        Me.l2.BackColor = System.Drawing.Color.Transparent
        Me.l2.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.l2.ForeColor = System.Drawing.Color.White
        Me.l2.Location = New System.Drawing.Point(62, 41)
        Me.l2.Margin = New System.Windows.Forms.Padding(0)
        Me.l2.Name = "l2"
        Me.l2.Size = New System.Drawing.Size(103, 12)
        Me.l2.TabIndex = 2
        Me.l2.Text = "By: Oasis inc. 2017"
        '
        'x1
        '
        Me.x1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.x1.BackColor = System.Drawing.Color.Gray
        Me.x1.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.x1.Font = New System.Drawing.Font("Arial", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.x1.ForeColor = System.Drawing.Color.Black
        Me.x1.FormattingEnabled = True
        Me.x1.ItemHeight = 14
        Me.x1.Items.AddRange(New Object() {"About", "Firespider Internet v17 was created in 2019, December 29th.", "If you want more,", "", "Credits: Elliot Williams, Alan-Michael Steele, Jamie"})
        Me.x1.Location = New System.Drawing.Point(0, 64)
        Me.x1.Margin = New System.Windows.Forms.Padding(0)
        Me.x1.Name = "x1"
        Me.x1.SelectionMode = System.Windows.Forms.SelectionMode.None
        Me.x1.Size = New System.Drawing.Size(415, 70)
        Me.x1.TabIndex = 0
        '
        'pi1
        '
        Me.pi1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pi1.BackColor = System.Drawing.Color.Transparent
        Me.pi1.ErrorImage = Nothing
        Me.pi1.InitialImage = Nothing
        Me.pi1.Location = New System.Drawing.Point(2, 2)
        Me.pi1.Name = "pi1"
        Me.pi1.Size = New System.Drawing.Size(51, 51)
        Me.pi1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.pi1.TabIndex = 3
        Me.pi1.TabStop = False
        Me.pi1.WaitOnLoad = True
        '
        'l1
        '
        Me.l1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.l1.AutoSize = True
        Me.l1.BackColor = System.Drawing.Color.Transparent
        Me.l1.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.l1.ForeColor = System.Drawing.Color.White
        Me.l1.Location = New System.Drawing.Point(342, 42)
        Me.l1.Margin = New System.Windows.Forms.Padding(0)
        Me.l1.Name = "l1"
        Me.l1.Size = New System.Drawing.Size(67, 12)
        Me.l1.TabIndex = 5
        Me.l1.Text = "Build: 17017"
        '
        'u
        '
        Me.u.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.u.BackColor = System.Drawing.Color.Black
        Me.u.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.u.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.u.ForeColor = System.Drawing.Color.White
        Me.u.Location = New System.Drawing.Point(164, 39)
        Me.u.Name = "u"
        Me.u.Size = New System.Drawing.Size(125, 17)
        Me.u.TabIndex = 6
        Me.u.Text = "Check For Updates"
        Me.u.UseVisualStyleBackColor = False
        '
        'll
        '
        Me.ll.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ll.AutoSize = True
        Me.ll.BackColor = System.Drawing.Color.Gray
        Me.ll.DisabledLinkColor = System.Drawing.Color.Silver
        Me.ll.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ll.LinkColor = System.Drawing.Color.White
        Me.ll.Location = New System.Drawing.Point(91, 93)
        Me.ll.Name = "ll"
        Me.ll.Size = New System.Drawing.Size(58, 12)
        Me.ll.TabIndex = 8
        Me.ll.TabStop = True
        Me.ll.Text = "Click Here"
        '
        'le
        '
        Me.le.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.le.AutoSize = True
        Me.le.BackColor = System.Drawing.Color.Transparent
        Me.le.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.le.ForeColor = System.Drawing.Color.White
        Me.le.Location = New System.Drawing.Point(291, 42)
        Me.le.Margin = New System.Windows.Forms.Padding(0)
        Me.le.Name = "le"
        Me.le.Size = New System.Drawing.Size(50, 12)
        Me.le.TabIndex = 9
        Me.le.Text = "32/64 Bit"
        '
        'A1
        '
        Me.A1.Interval = 4000
        '
        'f2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Black
        Me.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(415, 134)
        Me.Controls.Add(Me.le)
        Me.Controls.Add(Me.ll)
        Me.Controls.Add(Me.u)
        Me.Controls.Add(Me.l1)
        Me.Controls.Add(Me.x1)
        Me.Controls.Add(Me.pi1)
        Me.Controls.Add(Me.l2)
        Me.Controls.Add(Me.k1)
        Me.DoubleBuffered = True
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ForeColor = System.Drawing.Color.White
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "f2"
        Me.Opacity = 0.97R
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "About"
        Me.TopMost = True
        CType(Me.pi1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents pi1 As System.Windows.Forms.PictureBox
    Public WithEvents k1 As System.Windows.Forms.Label
    Public WithEvents l2 As System.Windows.Forms.Label
    Public WithEvents x1 As System.Windows.Forms.ListBox
    Public WithEvents l1 As System.Windows.Forms.Label
    Public WithEvents u As System.Windows.Forms.Button
    Public WithEvents ll As System.Windows.Forms.LinkLabel
    Public WithEvents le As System.Windows.Forms.Label
    Public WithEvents A1 As System.Windows.Forms.Timer
End Class