''' <summary>
''' This class exists so that the tbl_player entity created in the GameDatabase.edmx model can be modified outside of the GameDatabase context. Primarily for use in display of player details.
''' </summary>
Public Class ClsDisplayablePlayer : Inherits tbl_player

    Public Sub New(i As tbl_player)
        'Because this class exists outside of the EDM, the imported functions can not easily return it.
        'So the properties need to be manually set using an instance of tbl_player
        id = i.id
        uname = i.uname
        pword = i.pword
        currentWinStreak = i.currentWinStreak
        highestWinStreak = i.highestWinStreak
        isOnline = i.isOnline
        isLocked = i.isLocked
        isAdmin = i.isAdmin
    End Sub

    Public Overrides Function ToString() As String
        Return id & vbTab & uname & IIf(uname.Length > 11, vbTab, vbTab & vbTab) & IIf(isOnline, "Y", "N") & vbTab & currentWinStreak
    End Function

End Class
