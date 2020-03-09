''' <summary>
''' This class exists so that the tbl_game entity created in the GameDatabase.edmx model can be modified outside of the GameDatabase context. Primarily for use in display of the game lobby.
''' </summary>
Public Class ClsDisplayableGame : Inherits tbl_game

    Public Sub New(i As tbl_game)
        'Because this class exists outside of the EDM, the imported functions can not easily return it.
        'So the properties need to be manually set using an instance of tbl_game
        id = i.id
        gameState = i.gameState
        resetNumber = i.resetNumber
        roundNumber = i.roundNumber
        stateStartTime = i.stateStartTime
        gameStartTime = i.gameStartTime
        gameEndTime = i.gameEndTime
    End Sub

    Public Overrides Function ToString() As String
        Dim r As intf_getGameInfo_Result = MdDataAccess.getGameInfo(id)

        Return "Host: " & r.host_name & " | Player Count: " & r.player_count
    End Function

End Class
