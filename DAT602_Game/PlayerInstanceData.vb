''' <summary>
''' Data structure that allows the game form to more easily manage database id references.
''' </summary>
Public Structure PlayerInstanceData
    Public ReadOnly id As Integer
    Public ReadOnly name As String
    Public ReadOnly user As Integer
    Public ReadOnly game As Integer
    Public Sub New(ByVal prInstanceId As Integer)
        'Get instance's references to player and game information.
        Dim instanceInfo As tbl_player_game = MdDataAccess.getInstanceInfo(prInstanceId)
        'Set data
        id = prInstanceId
        name = MdDataAccess.getPlayerInfo(instanceInfo.pID).uname
        user = instanceInfo.pID
        game = instanceInfo.gID
    End Sub
End Structure
