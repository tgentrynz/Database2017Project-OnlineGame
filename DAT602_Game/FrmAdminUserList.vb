Public Class FrmAdminUserList

    Private playerId As Integer

    Private Sub FrmAdminUserList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        updateList()
    End Sub

    Private Function checkInput() As Boolean
        Return MdDataValidation.checkUsername(txtUsername.Text) And MdDataValidation.checkPassword(txtPassword.Text)
    End Function

    Public Sub New(prPlayer As Integer)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.playerId = prPlayer
    End Sub

    Private Sub updateList()
        lstUsers.DataSource = MdDataAccess.getDisplayablePlayerList
    End Sub

    Private Sub updateSelectedDetails()
        Dim user As tbl_player
        user = MdDataAccess.getPlayerInfo(DirectCast(lstUsers.SelectedItem, tbl_player).id)
        lblSelectedUser.Text = "User: " & user.id
        txtUsername.Text = user.uname
        txtPassword.Text = user.pword
        numCurrentWins.Value = user.currentWinStreak
        numHighestWins.Value = user.highestWinStreak
        rbLocked.Checked = user.isLocked
        rbAdmin.Checked = user.isAdmin
        btnLogout.Enabled = user.isOnline
        enableEditControls(True)
    End Sub

    Private Sub enableEditControls(input As Boolean)
        If (input = False) Then
            lblSelectedUser.Text = "User:"
            txtUsername.Text = ""
            txtPassword.Text = ""
            numCurrentWins.Value = 0
            numHighestWins.Value = 0
            rbLocked.Checked = False
            rbAdmin.Checked = False
            btnLogout.Enabled = False
        End If
        txtUsername.Enabled = input
        txtPassword.Enabled = input
        numCurrentWins.Enabled = input
        numHighestWins.Enabled = input
        rbLocked.Enabled = input
        rbAdmin.Enabled = input
    End Sub

    Private Sub lbUsers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstUsers.SelectedIndexChanged
        If (lstUsers.SelectedItem IsNot Nothing) Then
            updateSelectedDetails()
        Else
            enableEditControls(False)
        End If
    End Sub

    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        If (checkInput() And lstUsers.SelectedItem IsNot Nothing) Then
            If (MdDataAccess.updatePlayer(DirectCast(lstUsers.SelectedItem, tbl_player).id, txtUsername.Text, txtPassword.Text, numCurrentWins.Value, numHighestWins.Value, rbLocked.Checked, rbAdmin.Checked)) Then
                MessageBox.Show("Account updated.")
            End If
        End If
        updateList()
    End Sub

    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        Dim user AS tbl_player
        If (lstUsers.SelectedItem IsNot Nothing) Then
            user = DirectCast(lstUsers.SelectedItem, tbl_player)
            If (user.id <> playerId) Then
                MdDataAccess.playerLogout(user.id)
                updateSelectedDetails()
            Else
                MessageBox.Show("Don't log yourself out using this menu.")
            End If

        End If
    End Sub

    ''' <summary>
    ''' Manual handling of the click so that unchecking rbLocked doesn't autocheck rbAdmin.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub rbLocked_Click(sender As Object, e As EventArgs) Handles rbLocked.Click
        rbLocked.Checked = Not rbLocked.Checked
    End Sub

    ''' <summary>
    ''' Manual handling of the click so that unchecking rbAdmin doesn't autocheck rbLocked.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub rbAdmin_Click(sender As Object, e As EventArgs) Handles rbAdmin.Click
        rbAdmin.Checked = Not rbAdmin.Checked
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Close()
    End Sub

    Private Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        Dim newForm As FrmAdminNewUser = New FrmAdminNewUser()
        newForm.ShowDialog()
        updateList()
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        Dim deleteForm As FrmAdminDeleteUser
        Dim user As tbl_player
        If (lstUsers.SelectedItem IsNot Nothing) Then
            user = DirectCast(lstUsers.SelectedItem, tbl_player)
            If (user.id <> playerId) Then
                deleteForm = New FrmAdminDeleteUser(user.id, playerId)
                deleteForm.ShowDialog()
            Else
                MessageBox.Show("Don't delete yourself using this menu.")
            End If
            updateList()
        End If

    End Sub
End Class