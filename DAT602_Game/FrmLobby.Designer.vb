<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmLobby
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
        Me.btnLogout = New System.Windows.Forms.Button()
        Me.lstGameList = New System.Windows.Forms.ListBox()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.btnJoin = New System.Windows.Forms.Button()
        Me.btnCreate = New System.Windows.Forms.Button()
        Me.tmrRefresh = New System.Windows.Forms.Timer(Me.components)
        Me.btnAdmin = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'btnLogout
        '
        Me.btnLogout.Location = New System.Drawing.Point(13, 226)
        Me.btnLogout.Name = "btnLogout"
        Me.btnLogout.Size = New System.Drawing.Size(75, 23)
        Me.btnLogout.TabIndex = 0
        Me.btnLogout.Text = "Logout"
        Me.btnLogout.UseVisualStyleBackColor = True
        '
        'lstGameList
        '
        Me.lstGameList.FormattingEnabled = True
        Me.lstGameList.Location = New System.Drawing.Point(13, 39)
        Me.lstGameList.Name = "lstGameList"
        Me.lstGameList.Size = New System.Drawing.Size(200, 173)
        Me.lstGameList.TabIndex = 1
        '
        'lblInfo
        '
        Me.lblInfo.AutoSize = True
        Me.lblInfo.Location = New System.Drawing.Point(13, 13)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.Size = New System.Drawing.Size(69, 13)
        Me.lblInfo.TabIndex = 2
        Me.lblInfo.Text = "Open Games"
        '
        'btnJoin
        '
        Me.btnJoin.Location = New System.Drawing.Point(222, 39)
        Me.btnJoin.Name = "btnJoin"
        Me.btnJoin.Size = New System.Drawing.Size(75, 23)
        Me.btnJoin.TabIndex = 3
        Me.btnJoin.Text = "Join"
        Me.btnJoin.UseVisualStyleBackColor = True
        '
        'btnCreate
        '
        Me.btnCreate.Location = New System.Drawing.Point(222, 69)
        Me.btnCreate.Name = "btnCreate"
        Me.btnCreate.Size = New System.Drawing.Size(75, 23)
        Me.btnCreate.TabIndex = 4
        Me.btnCreate.Text = "Create"
        Me.btnCreate.UseVisualStyleBackColor = True
        '
        'tmrRefresh
        '
        Me.tmrRefresh.Interval = 1000
        '
        'btnAdmin
        '
        Me.btnAdmin.Location = New System.Drawing.Point(95, 226)
        Me.btnAdmin.Name = "btnAdmin"
        Me.btnAdmin.Size = New System.Drawing.Size(75, 23)
        Me.btnAdmin.TabIndex = 5
        Me.btnAdmin.Text = "Admin"
        Me.btnAdmin.UseVisualStyleBackColor = True
        '
        'FrmLobby
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(309, 261)
        Me.Controls.Add(Me.btnAdmin)
        Me.Controls.Add(Me.btnCreate)
        Me.Controls.Add(Me.btnJoin)
        Me.Controls.Add(Me.lblInfo)
        Me.Controls.Add(Me.lstGameList)
        Me.Controls.Add(Me.btnLogout)
        Me.Name = "FrmLobby"
        Me.Text = "FrmLobby"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnLogout As Button
    Friend WithEvents lstGameList As ListBox
    Friend WithEvents lblInfo As Label
    Friend WithEvents btnJoin As Button
    Friend WithEvents btnCreate As Button
    Friend WithEvents tmrRefresh As Timer
    Friend WithEvents btnAdmin As Button
End Class
