Public Class FrmLogin

    Private workingUsername As String 'The last username entered
    Private playerId As Integer 'The id of the player to log in.

    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        newScreen(LoginScreen.usernameEntry)
    End Sub


    ''' <summary>
    ''' Changes the form to allow a use to enter a username.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub gotoUsernameEntry(sender As Object, e As EventArgs)
        newScreen(LoginScreen.usernameEntry)
    End Sub

    ''' <summary>
    ''' Changes the form to allow a user to enter a password if their account is not locked.
    ''' </summary>
    Private Sub gotoLogin()
        Dim locked As Boolean = MdDataAccess.checkAccountLocked(playerId)
        If (Not locked) Then
            newScreen(LoginScreen.passwordEntry) 'Let the user enter their password.
        Else
            newScreen(LoginScreen.accountLocked) 'Show acount locked error message.
        End If
    End Sub

    ''' <summary>
    ''' Open lobby form after successful login
    ''' </summary>
    Private Sub gotoLobby()
        Dim lobby As FrmLobby = New FrmLobby(playerId)
        MdDataAccess.updatePlayerActivityTime(playerId)
        lobby.Show()
        Close()
    End Sub

    ''' <summary>
    ''' Handle the user's username input.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub usernameEntryBtnNext_Click(sender As Object, e As EventArgs)
        'Make sure text meets requirements
        workingUsername = txtUsernameEntry.Text
        If (MdDataValidation.checkUsername(workingUsername)) Then 'Make sure the username is valid.
            Dim i As Integer? = MdDataAccess.findPlayerAccount(workingUsername) 'Check if the account already exists.
            If (i.HasValue) Then 'If the account exists.
                playerId = i.Value
                If (MdDataAccess.checkAccountOnline(playerId)) Then
                    newScreen(LoginScreen.accountInUse) 'If the account is in use, the user can't log in.
                Else
                    gotoLogin() 'If the account is not already in-use proceed to the next part of the log in process.
                End If
            Else
                newScreen(LoginScreen.newUserConfirm) 'If the account doesn't exist, ask the user if they would like to create it.
            End If
        Else
            MessageBox.Show("The name entered was invalid.")
        End If

    End Sub

    ''' <summary>
    ''' Handle the user's password input.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub passwordEntryBtnNext_Click(sender As Object, e As EventArgs)
        If (MdDataAccess.playerLogin(playerId, txtPasswordEntry.Text)) Then
            gotoLobby() 'Successful login
        Else
            newScreen(LoginScreen.passwordFail) 'Failed login
        End If
    End Sub

    ''' <summary>
    ''' User has confirmed they would like to create a new account.
    ''' </summary>
    ''' <param name="SENDER"></param>
    ''' <param name="e"></param>
    Private Sub newUserConfirmBtnNext_Click(SENDER As Object, e As EventArgs)
        newScreen(LoginScreen.newUserEntry)
    End Sub

    ''' <summary>
    ''' Handle user's attempt to create an new account.
    ''' </summary>
    ''' <param name="SENDER"></param>
    ''' <param name="e"></param>
    Private Sub newUserEntryBtnNext_Click(SENDER As Object, e As EventArgs)
        Dim newUser As Integer?
        Dim err As Boolean = False
        If (txtUsernameEntry.Text.Equals(txtPasswordEntry.Text) And MdDataValidation.checkPassword(txtPasswordEntry.Text)) Then 'Check the passwords match and are valid values
            newUser = MdDataAccess.createPlayer(workingUsername, txtPasswordEntry.Text)
            If (newUser.HasValue) Then 'If the user was successfully created log them in
                playerId = newUser.Value
                If (MdDataAccess.playerLogin(playerId, txtPasswordEntry.Text)) Then
                    gotoLobby()
                Else
                    err = True
                End If
            Else
                err = True
            End If
        Else
            newScreen(LoginScreen.newUserPasswordMismatch) 'If the passwords didn't match, display an error.
        End If
        If (err) Then
            newScreen(LoginScreen.newUserFail) 'If the account couldn't be created, display an error.
        End If
    End Sub


    'This region just consists of auto-generated code moved into an select case statement, an enum to keep track of layouts this form can change to and all the control reference attributes.
#Region "Controls and Runtime Layout"
    ''' <summary>
    ''' This sub allows the form to change its layout at runtime.
    ''' </summary>
    ''' <param name="prScreen">The screen, from the LoginScreen enum, for the form to switch to.</param>
    Private Sub newScreen(ByVal prScreen As LoginScreen)
        Me.SuspendLayout()
        For Each i As Control In Controls
            i.Dispose()
        Next
        Controls.Clear()
        Select Case prScreen
            Case LoginScreen.usernameEntry
                'Initialise controls
                txtUsernameEntry = New TextBox()
                btnNext = New Button()
                btnBack = New Button()
                lblUsername = New Label()

                'Set controls
                txtUsernameEntry.Location = New System.Drawing.Point(68, 111)
                txtUsernameEntry.Name = "txtUsernameEntry"
                txtUsernameEntry.Size = New System.Drawing.Size(146, 20)
                txtUsernameEntry.TabIndex = 0
                txtUsernameEntry.Text = workingUsername

                btnNext.Location = New System.Drawing.Point(139, 137)
                btnNext.Name = "btnNext"
                btnNext.Size = New System.Drawing.Size(75, 23)
                btnNext.TabIndex = 1
                btnNext.Text = "Go"
                btnNext.UseVisualStyleBackColor = True
                AddHandler btnNext.Click, AddressOf usernameEntryBtnNext_Click

                btnBack.Location = New System.Drawing.Point(13, 226)
                btnBack.Name = "btnBack"
                btnBack.Size = New System.Drawing.Size(75, 23)
                btnBack.TabIndex = 4
                btnBack.Text = "Exit"
                btnBack.UseVisualStyleBackColor = True
                AddHandler btnBack.Click, AddressOf Close

                lblUsername.AutoSize = True
                lblUsername.Location = New System.Drawing.Point(65, 95)
                lblUsername.Name = "lblUsername"
                lblUsername.Size = New System.Drawing.Size(93, 13)
                lblUsername.TabIndex = 2
                lblUsername.Text = "Enter a username."

                'Add controls to list
                Controls.Add(txtUsernameEntry)
                Controls.Add(btnNext)
                Controls.Add(btnBack)
                Controls.Add(lblUsername)

            Case LoginScreen.passwordEntry
                'Initialise Controls
                lblUsername = New Label()
                lblPassword = New Label()
                txtPasswordEntry = New TextBox()
                btnNext = New Button()
                btnBack = New Button()

                'Set controls
                lblUsername.AutoSize = True
                lblUsername.Location = New System.Drawing.Point(65, 39)
                lblUsername.Name = "lblUsername"
                lblUsername.Size = New System.Drawing.Size(58, 26)
                lblUsername.TabIndex = 0
                lblUsername.Text = "Username:" & vbCrLf & " " & workingUsername

                lblPassword.AutoSize = True
                lblPassword.Location = New System.Drawing.Point(65, 88)
                lblPassword.Name = "lblPassword"
                lblPassword.Size = New System.Drawing.Size(81, 13)
                lblPassword.TabIndex = 1
                lblPassword.Text = "Enter Password"

                txtPasswordEntry.Location = New System.Drawing.Point(68, 104)
                txtPasswordEntry.Name = "txtPasswordEntry"
                txtPasswordEntry.Size = New System.Drawing.Size(146, 20)
                txtPasswordEntry.TabIndex = 2
                AddHandler btnNext.Click, AddressOf passwordEntryBtnNext_Click

                btnNext.Location = New System.Drawing.Point(139, 130)
                btnNext.Name = "btnNext"
                btnNext.Size = New System.Drawing.Size(75, 23)
                btnNext.TabIndex = 3
                btnNext.Text = "Go"
                btnNext.UseVisualStyleBackColor = True

                btnBack.Location = New System.Drawing.Point(13, 226)
                btnBack.Name = "btnBack"
                btnBack.Size = New System.Drawing.Size(75, 23)
                btnBack.TabIndex = 4
                btnBack.Text = "Cancel"
                btnBack.UseVisualStyleBackColor = True
                AddHandler btnBack.Click, AddressOf gotoUsernameEntry

                'Add controls to list
                Controls.Add(txtPasswordEntry)
                Controls.Add(lblUsername)
                Controls.Add(lblPassword)
                Controls.Add(btnNext)
                Controls.Add(btnBack)
            Case LoginScreen.passwordFail
                'Initialise controls
                lblInfo = New Label()
                btnBack = New Button()

                'Set controls
                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(65, 39)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(58, 26)
                lblInfo.TabIndex = 0
                lblInfo.Text = "The password entered" & vbCrLf & "was incorrect."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                btnBack.Location = New System.Drawing.Point(13, 226)
                btnBack.Name = "btnBack"
                btnBack.Size = New System.Drawing.Size(75, 23)
                btnBack.TabIndex = 4
                btnBack.Text = "Return"
                btnBack.UseVisualStyleBackColor = True
                AddHandler btnBack.Click, AddressOf gotoLogin

                'Add controls to list
                Controls.Add(lblInfo)
                Controls.Add(btnBack)
            Case LoginScreen.newUserConfirm
                'Initialise controls
                lblInfo = New Label()
                btnNext = New Button()
                btnBack = New Button()

                'Set controls
                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(65, 39)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(58, 26)
                lblInfo.TabIndex = 0
                lblInfo.Text = "User account does not exist." & vbCrLf & "Would you like to create a" & vbCrLf & "new account?"
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                btnNext.Location = New System.Drawing.Point(143, 185)
                btnNext.Name = "btnNext"
                btnNext.Size = New System.Drawing.Size(75, 23)
                btnNext.TabIndex = 1
                btnNext.Text = "Yes"
                btnNext.UseVisualStyleBackColor = True
                AddHandler btnNext.Click, AddressOf newUserConfirmBtnNext_Click

                btnBack.Location = New System.Drawing.Point(62, 185)
                btnBack.Name = "btnBack"
                btnBack.Size = New System.Drawing.Size(75, 23)
                btnBack.TabIndex = 0
                btnBack.Text = "Cancel"
                btnBack.UseVisualStyleBackColor = True
                AddHandler btnBack.Click, AddressOf gotoUsernameEntry

                'Add controls to list
                Controls.Add(lblInfo)
                Controls.Add(btnNext)
                Controls.Add(btnBack)
            Case LoginScreen.newUserEntry
                'Initialise Controls
                lblUsername = New Label()
                lblPassword = New Label()
                lblInfo = New Label() 'Reusing lblInfo to save adding another reference to the class.
                txtUsernameEntry = New TextBox() 'Reusing this as a password entry to save adding another reference to the class.
                txtPasswordEntry = New TextBox()
                btnNext = New Button()
                btnBack = New Button()

                'Set controls
                lblUsername.AutoSize = True
                lblUsername.Location = New System.Drawing.Point(65, 39)
                lblUsername.Name = "lblUsername"
                lblUsername.Size = New System.Drawing.Size(58, 26)
                lblUsername.TabIndex = 0
                lblUsername.Text = "Username:" & vbCrLf & " " & workingUsername

                lblPassword.AutoSize = True
                lblPassword.Location = New System.Drawing.Point(65, 88)
                lblPassword.Name = "lblPassword"
                lblPassword.Size = New System.Drawing.Size(81, 13)
                lblPassword.TabIndex = 1
                lblPassword.Text = "Enter Password"

                txtUsernameEntry.Location = New System.Drawing.Point(68, 104)
                txtUsernameEntry.Name = "txtPasswordEntry"
                txtUsernameEntry.Size = New System.Drawing.Size(146, 20)
                txtUsernameEntry.TabIndex = 2

                txtPasswordEntry.Location = New System.Drawing.Point(68, 136)
                txtPasswordEntry.Name = "txtPasswordConfirm"
                txtPasswordEntry.Size = New System.Drawing.Size(146, 20)
                txtPasswordEntry.TabIndex = 2

                btnNext.Location = New System.Drawing.Point(139, 171)
                btnNext.Name = "btnNext"
                btnNext.Size = New System.Drawing.Size(75, 23)
                btnNext.TabIndex = 3
                btnNext.Text = "Go"
                btnNext.UseVisualStyleBackColor = True
                AddHandler btnNext.Click, AddressOf newUserEntryBtnNext_Click

                btnBack.Location = New System.Drawing.Point(13, 226)
                btnBack.Name = "btnBack"
                btnBack.Size = New System.Drawing.Size(75, 23)
                btnBack.TabIndex = 4
                btnBack.Text = "Cancel"
                btnBack.UseVisualStyleBackColor = True
                AddHandler btnBack.Click, AddressOf gotoUsernameEntry

                'Add controls to list
                Controls.Add(txtUsernameEntry)
                Controls.Add(txtPasswordEntry)
                Controls.Add(lblUsername)
                Controls.Add(lblPassword)
                Controls.Add(btnNext)
                Controls.Add(btnBack)
            Case LoginScreen.newUserPasswordMismatch
                'Initialise controls
                lblInfo = New Label()
                btnNext = New Button()
                btnBack = New Button()

                'Set controls
                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(65, 39)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(58, 26)
                lblInfo.TabIndex = 0
                lblInfo.Text = "The passwords entered" & vbCrLf & "did not match or had incorrect values." & vbCrLf & "Passwords must include" & vbCrLf & "at least one number" & vbCrLf & "have no spaces."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                btnNext.Location = New System.Drawing.Point(143, 185)
                btnNext.Name = "btnNext"
                btnNext.Size = New System.Drawing.Size(75, 23)
                btnNext.TabIndex = 1
                btnNext.Text = "Retry"
                btnNext.UseVisualStyleBackColor = True
                AddHandler btnNext.Click, AddressOf newUserConfirmBtnNext_Click

                btnBack.Location = New System.Drawing.Point(62, 185)
                btnBack.Name = "btnBack"
                btnBack.Size = New System.Drawing.Size(75, 23)
                btnBack.TabIndex = 0
                btnBack.Text = "Cancel"
                btnBack.UseVisualStyleBackColor = True
                AddHandler btnBack.Click, AddressOf gotoUsernameEntry

                'Add controls to list
                Controls.Add(lblInfo)
                Controls.Add(btnNext)
                Controls.Add(btnBack)
            Case LoginScreen.newUserFail
                'Initialise controls
                lblInfo = New Label()
                btnBack = New Button()

                'Set controls
                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(65, 39)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(58, 26)
                lblInfo.TabIndex = 0
                lblInfo.Text = "There was an error creating the account."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                btnBack.Location = New System.Drawing.Point(13, 226)
                btnBack.Name = "btnBack"
                btnBack.Size = New System.Drawing.Size(75, 23)
                btnBack.TabIndex = 4
                btnBack.Text = "Cancel"
                btnBack.UseVisualStyleBackColor = True
                AddHandler btnBack.Click, AddressOf gotoUsernameEntry

                'Add controls to list
                Controls.Add(lblInfo)
                Controls.Add(btnBack)
            Case LoginScreen.accountLocked
                'Initialise controls
                lblInfo = New Label()
                btnBack = New Button()

                'Set controls
                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(65, 39)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(58, 26)
                lblInfo.TabIndex = 0
                lblInfo.Text = "This account has been locked." & vbCrLf & "Please contact an administrator."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                btnBack.Location = New System.Drawing.Point(13, 226)
                btnBack.Name = "btnBack"
                btnBack.Size = New System.Drawing.Size(75, 23)
                btnBack.TabIndex = 4
                btnBack.Text = "Return"
                btnBack.UseVisualStyleBackColor = True
                AddHandler btnBack.Click, AddressOf gotoUsernameEntry

                'Add controls to list
                Controls.Add(lblInfo)
                Controls.Add(btnBack)
            Case LoginScreen.accountInUse
                'Initialise controls
                lblInfo = New Label()
                btnBack = New Button()

                'Set controls
                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(65, 39)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(58, 26)
                lblInfo.TabIndex = 0
                lblInfo.Text = "This account is already logged in."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                btnBack.Location = New System.Drawing.Point(13, 226)
                btnBack.Name = "btnBack"
                btnBack.Size = New System.Drawing.Size(75, 23)
                btnBack.TabIndex = 4
                btnBack.Text = "Return"
                btnBack.UseVisualStyleBackColor = True
                AddHandler btnBack.Click, AddressOf gotoUsernameEntry

                'Add controls to list
                Controls.Add(lblInfo)
                Controls.Add(btnBack)
        End Select
        ResumeLayout(True)
    End Sub
    ''' <summary>
    ''' This enum defines the layouts that the form can change to at runtime.
    ''' </summary>
    Private Enum LoginScreen
        usernameEntry
        passwordEntry
        passwordFail
        newUserConfirm
        newUserEntry
        newUserPasswordMismatch
        newUserFail
        accountLocked
        accountInUse
    End Enum
#Region "Navigation Buttons"
    Friend WithEvents btnNext As Button
    Friend WithEvents btnBack As Button
#End Region
    Friend WithEvents lblUsername As Label
    Friend WithEvents lblPassword As Label
    Friend WithEvents lblInfo As Label
    Friend WithEvents txtUsernameEntry As TextBox
    Friend WithEvents txtPasswordEntry As TextBox
#End Region
End Class