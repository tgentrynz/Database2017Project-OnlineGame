Public Class FrmLobby
    Private playerId As Integer 'The logged in player
    Private _openGames As List(Of FrmGame) = New List(Of FrmGame) 'A list of all game windows the application has open.

    Public ReadOnly Property openGames As List(Of FrmGame)
        Get
            Return _openGames
        End Get
    End Property

    Public Sub New(prPlayerId As Integer)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.playerId = prPlayerId
    End Sub

    Private Sub FrmLobby_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        tmrRefresh.Start()
        'Hide the admin button from regular users.
        If (Not MdDataAccess.isPlayerAdmin(playerId)) Then
            btnAdmin.Enabled = False
            btnAdmin.Hide()
        End If

    End Sub

    Private Sub FrmLobby_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Dim loginForm As FrmLogin = New FrmLogin()
        Dim closeForm As FrmGame
        MdDataAccess.playerLogout(playerId) 'Log the player out.
        'Close all open game windows.
        For i As Integer = _openGames.Count - 1 To 0 Step -1
            Console.WriteLine(i)
            closeForm = _openGames(i)
            closeForm.dropoutOnClose = False
            closeForm.Close()
        Next
        'Return to the login form.
        loginForm.Show()
    End Sub

    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        Close()
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

    ''' <summary>
    ''' Handle user joining a game.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnJoin_Click(sender As Object, e As EventArgs) Handles btnJoin.Click
        Dim gameForm As FrmGame
        Dim gameId As Integer
        Dim instanceId As Integer?
        Dim isAlreadyOpen As Boolean = False
        'Make sure player hasn't been logged out
        If (MdDataAccess.checkAccountOnline(playerId)) Then
            If (lstGameList.SelectedItem IsNot Nothing) Then
                'Get the game id
                gameId = DirectCast(lstGameList.SelectedItem, tbl_game).id
                'Try add the player
                instanceId = MdDataAccess.newPlayerInstance(playerId, gameId)
                If (instanceId.HasValue) Then
                    'Check if the player is already in this game.
                    For Each i As FrmGame In _openGames
                        If (i.instanceId = instanceId) Then
                            gameForm = i
                            isAlreadyOpen = True
                            Exit For
                        End If
                    Next
                    If (isAlreadyOpen) Then 'If the player already has the game open bring the window to focus.
                        gameForm.WindowState = FormWindowState.Normal 'The value of isAlreadyOpen will only evaluate to true if gameForm has been given a value. Visual Studio just can't see that.
                        gameForm.Focus()
                    Else 'Open a new game window.
                        gameForm = New FrmGame(instanceId.Value, Me)
                        openGames.Add(gameForm)
                        gameForm.Show()
                    End If

                Else 'Failed to join game
                    MessageBox.Show("An error occurred while joining the game.", "Game Join Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk)
                End If
            End If
        Else
            unexpectedLogout()
        End If
    End Sub

    ''' <summary>
    ''' Handle user creating a game.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnCreate_Click(sender As Object, e As EventArgs) Handles btnCreate.Click
        Dim gameForm As FrmGame
        'Make sure player hasn't been logged out
        If (MdDataAccess.checkAccountOnline(playerId)) Then
            Dim newGame As intf_initiateGame_Result = MdDataAccess.initiateGame(playerId)
            If (newGame IsNot Nothing) Then
                gameForm = New FrmGame(newGame.instance_id, Me)
                openGames.Add(gameForm)
                gameForm.Show()
            Else
                MessageBox.Show("An error occurred in the game creation.", "Game Creation Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk)
            End If
        Else
            unexpectedLogout()
        End If
    End Sub

    ''' <summary>
    ''' Handle user accessing the admin menu.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnAdmin_Click(sender As Object, e As EventArgs) Handles btnAdmin.Click
        Dim adminMenu As FrmAdminMenu
        If (MdDataAccess.checkAccountOnline(playerId)) Then
            adminMenu = New FrmAdminMenu(playerId)
            adminMenu.ShowDialog()
        Else
            unexpectedLogout()
        End If
    End Sub

    ''' <summary>
    ''' Lets the user know the system has logged their account out.
    ''' </summary>
    Private Sub unexpectedLogout()
        MessageBox.Show("Your account has been logged out. Please re-enter you account details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Close()
    End Sub

End Class