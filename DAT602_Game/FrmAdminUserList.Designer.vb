<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmAdminUserList
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
        Me.lstUsers = New System.Windows.Forms.ListBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblSelectedUser = New System.Windows.Forms.Label()
        Me.txtUsername = New System.Windows.Forms.TextBox()
        Me.txtPassword = New System.Windows.Forms.TextBox()
        Me.numCurrentWins = New System.Windows.Forms.NumericUpDown()
        Me.numHighestWins = New System.Windows.Forms.NumericUpDown()
        Me.rbLocked = New System.Windows.Forms.RadioButton()
        Me.rbAdmin = New System.Windows.Forms.RadioButton()
        Me.btnUpdate = New System.Windows.Forms.Button()
        Me.btnLogout = New System.Windows.Forms.Button()
        Me.btnDelete = New System.Windows.Forms.Button()
        Me.btnBack = New System.Windows.Forms.Button()
        Me.btnNew = New System.Windows.Forms.Button()
        CType(Me.numCurrentWins, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numHighestWins, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lstUsers
        '
        Me.lstUsers.FormattingEnabled = True
        Me.lstUsers.Location = New System.Drawing.Point(12, 25)
        Me.lstUsers.Name = "lstUsers"
        Me.lstUsers.Size = New System.Drawing.Size(138, 186)
        Me.lstUsers.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(34, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Users"
        '
        'lblSelectedUser
        '
        Me.lblSelectedUser.AutoSize = True
        Me.lblSelectedUser.Location = New System.Drawing.Point(164, 9)
        Me.lblSelectedUser.Name = "lblSelectedUser"
        Me.lblSelectedUser.Size = New System.Drawing.Size(32, 13)
        Me.lblSelectedUser.TabIndex = 2
        Me.lblSelectedUser.Text = "User:"
        '
        'txtUsername
        '
        Me.txtUsername.Location = New System.Drawing.Point(227, 25)
        Me.txtUsername.Name = "txtUsername"
        Me.txtUsername.Size = New System.Drawing.Size(100, 20)
        Me.txtUsername.TabIndex = 3
        '
        'txtPassword
        '
        Me.txtPassword.Location = New System.Drawing.Point(227, 51)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.Size = New System.Drawing.Size(100, 20)
        Me.txtPassword.TabIndex = 4
        '
        'numCurrentWins
        '
        Me.numCurrentWins.Location = New System.Drawing.Point(227, 77)
        Me.numCurrentWins.Name = "numCurrentWins"
        Me.numCurrentWins.Size = New System.Drawing.Size(100, 20)
        Me.numCurrentWins.TabIndex = 5
        '
        'numHighestWins
        '
        Me.numHighestWins.Location = New System.Drawing.Point(227, 103)
        Me.numHighestWins.Name = "numHighestWins"
        Me.numHighestWins.Size = New System.Drawing.Size(100, 20)
        Me.numHighestWins.TabIndex = 6
        '
        'rbLocked
        '
        Me.rbLocked.AutoCheck = False
        Me.rbLocked.AutoSize = True
        Me.rbLocked.Location = New System.Drawing.Point(167, 129)
        Me.rbLocked.Name = "rbLocked"
        Me.rbLocked.RightToLeft = System.Windows.Forms.RightToLeft.Yes
        Me.rbLocked.Size = New System.Drawing.Size(61, 17)
        Me.rbLocked.TabIndex = 7
        Me.rbLocked.TabStop = True
        Me.rbLocked.Text = "Locked"
        Me.rbLocked.UseVisualStyleBackColor = True
        '
        'rbAdmin
        '
        Me.rbAdmin.AutoCheck = False
        Me.rbAdmin.AutoSize = True
        Me.rbAdmin.Location = New System.Drawing.Point(273, 129)
        Me.rbAdmin.Name = "rbAdmin"
        Me.rbAdmin.RightToLeft = System.Windows.Forms.RightToLeft.Yes
        Me.rbAdmin.Size = New System.Drawing.Size(54, 17)
        Me.rbAdmin.TabIndex = 8
        Me.rbAdmin.TabStop = True
        Me.rbAdmin.Text = "Admin"
        Me.rbAdmin.UseVisualStyleBackColor = True
        '
        'btnUpdate
        '
        Me.btnUpdate.Location = New System.Drawing.Point(167, 152)
        Me.btnUpdate.Name = "btnUpdate"
        Me.btnUpdate.Size = New System.Drawing.Size(75, 23)
        Me.btnUpdate.TabIndex = 9
        Me.btnUpdate.Text = "Update"
        Me.btnUpdate.UseVisualStyleBackColor = True
        '
        'btnLogout
        '
        Me.btnLogout.Location = New System.Drawing.Point(167, 182)
        Me.btnLogout.Name = "btnLogout"
        Me.btnLogout.Size = New System.Drawing.Size(75, 23)
        Me.btnLogout.TabIndex = 10
        Me.btnLogout.Text = "Logout"
        Me.btnLogout.UseVisualStyleBackColor = True
        '
        'btnDelete
        '
        Me.btnDelete.Location = New System.Drawing.Point(249, 182)
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.Size = New System.Drawing.Size(75, 23)
        Me.btnDelete.TabIndex = 11
        Me.btnDelete.Text = "Delete"
        Me.btnDelete.UseVisualStyleBackColor = True
        '
        'btnBack
        '
        Me.btnBack.Location = New System.Drawing.Point(12, 226)
        Me.btnBack.Name = "btnBack"
        Me.btnBack.Size = New System.Drawing.Size(75, 23)
        Me.btnBack.TabIndex = 12
        Me.btnBack.Text = "Close"
        Me.btnBack.UseVisualStyleBackColor = True
        '
        'btnNew
        '
        Me.btnNew.Location = New System.Drawing.Point(94, 225)
        Me.btnNew.Name = "btnNew"
        Me.btnNew.Size = New System.Drawing.Size(75, 23)
        Me.btnNew.TabIndex = 13
        Me.btnNew.Text = "New"
        Me.btnNew.UseVisualStyleBackColor = True
        '
        'FrmAdminUserList
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(339, 261)
        Me.Controls.Add(Me.btnNew)
        Me.Controls.Add(Me.btnBack)
        Me.Controls.Add(Me.btnDelete)
        Me.Controls.Add(Me.btnLogout)
        Me.Controls.Add(Me.btnUpdate)
        Me.Controls.Add(Me.rbAdmin)
        Me.Controls.Add(Me.rbLocked)
        Me.Controls.Add(Me.numHighestWins)
        Me.Controls.Add(Me.numCurrentWins)
        Me.Controls.Add(Me.txtPassword)
        Me.Controls.Add(Me.txtUsername)
        Me.Controls.Add(Me.lblSelectedUser)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lstUsers)
        Me.Name = "FrmAdminUserList"
        Me.Text = "Users"
        CType(Me.numCurrentWins, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numHighestWins, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lstUsers As ListBox
    Friend WithEvents Label1 As Label
    Friend WithEvents lblSelectedUser As Label
    Friend WithEvents txtUsername As TextBox
    Friend WithEvents txtPassword As TextBox
    Friend WithEvents numCurrentWins As NumericUpDown
    Friend WithEvents numHighestWins As NumericUpDown
    Friend WithEvents rbLocked As RadioButton
    Friend WithEvents rbAdmin As RadioButton
    Friend WithEvents btnUpdate As Button
    Friend WithEvents btnLogout As Button
    Friend WithEvents btnDelete As Button
    Friend WithEvents btnBack As Button
    Friend WithEvents btnNew As Button
End Class
