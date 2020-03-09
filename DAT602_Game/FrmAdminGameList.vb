Public Class FrmAdminGameList

    Private playerId

    Public Sub New(prPlayerID As Integer)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        playerId = prPlayerID
    End Sub

    Private Sub FrmAdminGameList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        tmrRefresh.Enabled = True
    End Sub

    Private Sub tmrRefresh_Tick(sender As Object, e As EventArgs) Handles tmrRefresh.Tick
        Dim selected As Boolean
        Dim selectedId As Integer
        'If the user has selected a game store its id value
        If (lstGameList.SelectedItem IsNot Nothing) Then
            selected = True
            selectedId = DirectCast(lstGameList.SelectedItem, ClsDisplayableGame).id
        End If
        'Refresh the list
        lstGameList.DataSource = Nothing
        lstGameList.DataSource = MdDataAccess.getDisplayableRunningGames()
        'If the game the user had selected still exists select it again
        If (selected) Then
            For Each i As ClsDisplayableGame In lstGameList.Items
                If (i.id = selectedId) Then
                    lstGameList.SelectedIndex = lstGameList.Items.IndexOf(i)
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Sub btnEndGame_Click(sender As Object, e As EventArgs) Handles btnEndGame.Click
        Dim endForm As FrmAdminEndGame
        If (lstGameList.SelectedItem IsNot Nothing) Then
            endForm = New FrmAdminEndGame(DirectCast(lstGameList.SelectedItem, tbl_game).id, playerId)
            endForm.ShowDialog()
        End If
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Close()
    End Sub
End Class