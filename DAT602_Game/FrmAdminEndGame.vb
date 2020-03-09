Public Class FrmAdminEndGame

    Private gameId As Integer
    Private adminId As Integer

    Public Sub New(prGameID As Integer, prAdminID As Integer)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        gameId = prGameID
        adminId = prAdminID
    End Sub

    Private Sub FrmAdminDeleteGame_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblHost.Text = "You are deleting the game hosted by: " + MdDataAccess.getGameInfo(gameId).host_name
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        If (MdDataAccess.comparePassword(adminId, txtPassword.Text)) Then
            If (MdDataAccess.endGame(gameId)) Then
                Close()
            Else
                MessageBox.Show("There was an error ending the game.")
            End If
        End If
    End Sub
End Class