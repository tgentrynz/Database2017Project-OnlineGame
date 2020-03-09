Public Class ClsDisplayablePlayerGameMoveSelectionStateResult : Inherits intf_getPlayerGameMoveSelectionState_Result

    Public Sub New(i As intf_getPlayerGameMoveSelectionState_Result)
        'Because this class exists outside of the EDM, the imported functions can not easily return it.
        'So the properties need to be manually set using an instance of intf_getPlayerGameState_Result
        player_name = i.player_name
        player_state = i.player_state
        player_hasMoved = i.player_hasMoved
    End Sub

    Public Overrides Function ToString() As String
        Return player_name & ": " & IIf(player_state = 0, "Out", IIf(player_hasMoved, "Waiting", "Working"))
    End Function
End Class
