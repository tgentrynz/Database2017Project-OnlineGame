Public Class FrmAdminDeleteUser

    Private playerId
    Private adminId

    Public Sub New(prPlayerId As Integer, prAdminId As Integer)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        playerId = prPlayerId
        adminId = prAdminId
    End Sub

    Private Sub FrmAdminDeleteUser_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblUsername.Text = "You are deleting user: " & MdDataAccess.getPlayerInfo(playerId).uname
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        If (MdDataAccess.comparePassword(adminId, txtPassword.Text)) Then
            If (MdDataAccess.deletePlayer(playerId)) Then
                Close()
            Else
                MessageBox.Show("There was an error deleting the user account.")
            End If
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Close()
    End Sub
End Class