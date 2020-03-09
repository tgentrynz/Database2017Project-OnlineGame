Module MdDataAccess
    Private db As GameDatabseEntities

    Public Function checkAccountLocked(ByVal prPlayer As Integer) As Boolean
        Dim o As Boolean = True
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        refreshConnection()

        l = db.intf_checkAccountLocked(prPlayer).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If

        Return o
    End Function

    Public Function checkAccountOnline(ByVal prPlayer As Integer) As Boolean
        Dim o As Boolean = False
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        refreshConnection()

        l = db.intf_checkAccountOnline(prPlayer).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If

        Return o
    End Function

    Public Function comparePassword(ByVal prPlayer As Integer, ByVal prPassword As String) As Boolean
        Dim o As Boolean = True
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        refreshConnection()

        l = db.intf_comparePassword(prPlayer, prPassword).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If
        saveChanges()

        Return o
    End Function

    Public Function createPlayer(ByVal prUsername As String, ByVal prPassword As String) As Integer?
        Dim o As Integer?
        Dim l As List(Of Integer?)
        refreshConnection()

        l = db.intf_createPlayer(prUsername, prPassword).ToList()
        If (l.Count > 0) Then
            o = l.First()
        End If
        saveChanges()

        Return o
    End Function

    Public Function deletePlayer(ByVal prPlayer As Integer) As Boolean
        Dim o As Boolean = False
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        refreshConnection()

        l = db.intf_deletePlayer(prPlayer).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If
        saveChanges()

        Return o
    End Function

    Public Function endGame(ByVal prGame As Integer) As Boolean
        Dim o As Boolean = False
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        refreshConnection()

        l = db.intf_endGame(prGame).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If
        saveChanges()

        Return o
    End Function

    ''' <summary>
    ''' Tries to find a player account using a username string and return the account's id.
    ''' </summary>
    ''' <param name="prUsername">The username to search for.</param>
    ''' <returns></returns>
    Public Function findPlayerAccount(ByVal prUsername As String) As Integer?
        Dim o As Integer? = Nothing
        Dim l As List(Of Integer?)
        refreshConnection()

        l = db.intf_findPlayerAccount(prUsername).ToList()
        If (l.Count > 0) Then
            o = l.First
        End If

        Return o
    End Function

    Public Function getCountPerAction(ByVal prGame As Integer) As List(Of intf_getCountPerAction_Result)
        Dim o As List(Of intf_getCountPerAction_Result)
        refreshConnection()

        o = db.intf_getCountPerAction(prGame).ToList()

        Return o
    End Function

    Public Function getGameInfo(ByVal prGame As Integer) As intf_getGameInfo_Result
        Dim o As intf_getGameInfo_Result = Nothing
        Dim l As List(Of intf_getGameInfo_Result)
        refreshConnection()

        l = db.intf_getGameInfo(prGame).ToList()
        If (l.Count > 0) Then
            o = l.First
        End If

        Return o
    End Function

    Public Function getGameState(ByVal prGame As Integer) As Byte
        Dim o As Byte
        Dim r As Byte?
        Dim l As List(Of Byte?)
        refreshConnection()

        l = db.intf_getGameState(prGame).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If

        Return o
    End Function

    Public Function getInstanceInfo(ByVal prInstance As Integer) As tbl_player_game
        Dim o As tbl_player_game
        Dim l As List(Of tbl_player_game)
        refreshConnection()

        l = db.intf_getInstanceInfo(prInstance).ToList()
        If (l.Count > 0) Then
            o = l.First()
        End If
        Return o
    End Function

    Public Function getInstanceSelection(ByVal prInstance As Integer) As Byte?
        Dim o As Byte? = New Byte?
        Dim l As List(Of Byte?)
        refreshConnection()

        l = db.intf_getInstanceSelection(prInstance).ToList()
        If (l.Count > 0) Then
            o = l.First()
        End If
        Return o
    End Function

    Public Function getPlayerGameMoveSelectionState(ByVal prGame As Integer) As List(Of ClsDisplayablePlayerGameMoveSelectionStateResult)
        Dim o As List(Of ClsDisplayablePlayerGameMoveSelectionStateResult) = New List(Of ClsDisplayablePlayerGameMoveSelectionStateResult)
        refreshConnection()
        For Each i As intf_getPlayerGameMoveSelectionState_Result In db.intf_getPlayerGameMoveSelectionState(prGame).ToList()
            o.Add(New ClsDisplayablePlayerGameMoveSelectionStateResult(i))
        Next
        Return o
    End Function

    Public Function getPlayerGameState(ByVal prGame As Integer) As List(Of ClsDisplayablePlayerGameStateResult)
        Dim o As List(Of ClsDisplayablePlayerGameStateResult) = New List(Of ClsDisplayablePlayerGameStateResult)
        refreshConnection()
        For Each i As intf_getPlayerGameState_Result In db.intf_getPlayerGameState(prGame).ToList()
            o.Add(New ClsDisplayablePlayerGameStateResult(i))
        Next
        Return o
    End Function

    ''' <summary>
    ''' Gets a list of all player accounts from the database.
    ''' </summary>
    ''' <returns></returns>
    Public Function getPlayerList() As List(Of tbl_player)
        refreshConnection()
        Return db.intf_getPlayerList().ToList()
    End Function

    ''' <summary>
    ''' Gets a list of all player accounts from the database, formatted for display in the admin panel.
    ''' </summary>
    ''' <returns></returns>
    Public Function getDisplayablePlayerList() As List(Of ClsDisplayablePlayer)
        Dim o As List(Of ClsDisplayablePlayer) = New List(Of ClsDisplayablePlayer)
        refreshConnection()

        For Each i In getPlayerList()
            o.Add(New ClsDisplayablePlayer(i))
        Next

        Return o
    End Function

    Public Function getPlayerInfo(prPlayer As Integer) As tbl_player
        Dim o As tbl_player = New tbl_player()
        refreshConnection()

        For Each i In db.intf_getPlayerInfo(prPlayer)
            o = i
        Next
        Return o
    End Function

    Public Function getRunningGames() As List(Of tbl_game)
        Dim o As List(Of tbl_game)
        refreshConnection()

        o = db.intf_getRunningGames().ToList()

        Return o
    End Function

    Public Function getDisplayableRunningGames() As List(Of ClsDisplayableGame)
        Dim o As List(Of ClsDisplayableGame) = New List(Of ClsDisplayableGame)
        refreshConnection()

        For Each i In getRunningGames()
            o.Add(New ClsDisplayableGame(i))
        Next

        Return o
    End Function

    Public Function initiateGame(ByVal prPlayer As Integer) As intf_initiateGame_Result
        Dim o As intf_initiateGame_Result = Nothing
        Dim l As List(Of intf_initiateGame_Result)
        refreshConnection()

        l = db.intf_initiateGame(prPlayer).ToList()
        If (l.Count > 0) Then
            o = l.First()
        End If
        saveChanges()

        Return o
    End Function

    Public Function isPlayerAdmin(ByVal prPlayer As Integer) As Boolean
        Dim o As Boolean = False
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        refreshConnection()

        l = db.intf_isPlayerAdmin(prPlayer).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If

        Return o
    End Function

    Public Function newPlayerInstance(ByVal prPlayer As Integer, ByVal prGame As Integer) As Integer?
        Dim o As Integer?
        Dim l As List(Of Integer?)
        refreshConnection()

        l = db.intf_newPlayerInstance(prPlayer, prGame).ToList()
        If (l.Count > 0) Then
            o = l.First()
        End If
        saveChanges()

        Return o
    End Function

    Public Function playerActionRegister(ByVal prPlayerInstance As Integer, ByVal prActionID As SByte) As Boolean
        Dim o As Boolean = False
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        refreshConnection()

        l = db.intf_playerActionRegister(prPlayerInstance, prActionID).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If
        saveChanges()

        Return o
    End Function

    Public Function playerDropout(ByVal prPlayerInstance As Integer) As Boolean
        Dim o As Boolean = False
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        refreshConnection()

        l = db.intf_playerDropout(prPlayerInstance).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If
        saveChanges()

        Return o
    End Function

    ''' <summary>
    ''' Attempts to log the specified player into the system and returns the result.
    ''' </summary>
    ''' <param name="prPlayer">The id number of the player.</param>
    ''' <param name="prPassword">The password.</param>
    ''' <returns></returns>
    Public Function playerLogin(ByVal prPlayer As Integer, ByVal prPassword As String) As Boolean
        Dim o As Boolean = False
        Dim l As List(Of Boolean?)
        Dim r As Boolean? = False
        refreshConnection()

        l = db.intf_playerLogin(prPlayer, prPassword).ToList
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If
        saveChanges()

        Return o
    End Function

    Public Function playerLogout(ByVal prPlayer As Integer) As Boolean
        Dim o As Boolean = False
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        refreshConnection()

        l = db.intf_playerLogout(prPlayer).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If
        saveChanges()

        Return o
    End Function

    Public Function updatePlayer(ByVal prPlayer As Integer, ByVal prUsername As String, ByVal prPassword As String, ByVal prCurrentWinStreak As Integer, ByVal prHighestWinStreak As Integer, ByVal prIsLocked As Boolean, ByVal prIsAdmin As Boolean) As Boolean
        Dim o As Boolean = False
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        Dim lcLocked As Byte = 0
        Dim lcAdmin As Byte = 0
        refreshConnection()

        If (prIsLocked) Then
            lcLocked = 1
        End If
        If (prIsAdmin) Then
            lcAdmin = 1
        End If

        l = db.intf_updatePlayer(prPlayer, prUsername, prPassword, prCurrentWinStreak, prHighestWinStreak, DirectCast(IIf(prIsLocked, 1, 0), Integer), DirectCast(IIf(prIsAdmin, 1, 0), Integer)).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If
        Console.WriteLine(o)
        saveChanges()

        Return o
    End Function

    Public Function updatePlayerActivityTime(ByVal prPlayer As Integer) As Boolean
        Dim o As Boolean = False
        Dim r As Boolean?
        Dim l As List(Of Boolean?)
        refreshConnection()

        l = db.intf_updatePlayerActivityTime(prPlayer).ToList()
        If (l.Count > 0) Then
            r = l.First()
            If (r.HasValue) Then
                o = r.Value
            End If
        End If
        saveChanges()

        Return o
    End Function
    Public Function maxUsernameLength() As Integer
        Return 16
    End Function

    Public Function maxPasswordLength() As Integer
        Return 16
    End Function
    Private Sub refreshConnection()
        If (Not IsNothing(db)) Then
            db.Dispose()
        End If
        db = New GameDatabseEntities()
    End Sub
    Private Sub saveChanges()
        If (Not IsNothing(db)) Then
            db.SaveChanges()
        End If
    End Sub
End Module
