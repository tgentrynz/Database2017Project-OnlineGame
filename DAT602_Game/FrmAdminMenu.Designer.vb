<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmAdminMenu
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
        Me.btnUser = New System.Windows.Forms.Button()
        Me.btnGames = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.lbl = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'btnUser
        '
        Me.btnUser.Location = New System.Drawing.Point(105, 57)
        Me.btnUser.Name = "btnUser"
        Me.btnUser.Size = New System.Drawing.Size(75, 23)
        Me.btnUser.TabIndex = 0
        Me.btnUser.Text = "Users"
        Me.btnUser.UseVisualStyleBackColor = True
        '
        'btnGames
        '
        Me.btnGames.Location = New System.Drawing.Point(105, 86)
        Me.btnGames.Name = "btnGames"
        Me.btnGames.Size = New System.Drawing.Size(75, 23)
        Me.btnGames.TabIndex = 1
        Me.btnGames.Text = "Games"
        Me.btnGames.UseVisualStyleBackColor = True
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(12, 129)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(75, 23)
        Me.btnClose.TabIndex = 2
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'lbl
        '
        Me.lbl.AutoSize = True
        Me.lbl.Location = New System.Drawing.Point(90, 22)
        Me.lbl.Name = "lbl"
        Me.lbl.Size = New System.Drawing.Size(102, 13)
        Me.lbl.TabIndex = 3
        Me.lbl.Text = "Administration Menu"
        '
        'FrmAdminMenu
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 164)
        Me.Controls.Add(Me.lbl)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnGames)
        Me.Controls.Add(Me.btnUser)
        Me.Name = "FrmAdminMenu"
        Me.Text = "FrmAdminMenu"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnUser As Button
    Friend WithEvents btnGames As Button
    Friend WithEvents btnClose As Button
    Friend WithEvents lbl As Label
End Class
