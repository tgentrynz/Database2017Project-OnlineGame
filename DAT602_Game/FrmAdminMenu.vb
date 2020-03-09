Public Class FrmAdminMenu

    Private playerId As Integer

    Public Sub New(prPlayer As Integer)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.playerId = prPlayer
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Close()
    End Sub

    Private Sub btnUser_Click(sender As Object, e As EventArgs) Handles btnUser.Click
        Dim form As FrmAdminUserList = New FrmAdminUserList(playerId)
        form.ShowDialog()
    End Sub

    Private Sub btnGames_Click(sender As Object, e As EventArgs) Handles btnGames.Click
        Dim form As FrmAdminGameList = New FrmAdminGameList(playerId)
        form.ShowDialog()
    End Sub
End Class