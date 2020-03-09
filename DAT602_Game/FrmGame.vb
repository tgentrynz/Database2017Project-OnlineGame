Public Class FrmGame
    Private lobby As FrmLobby
    Private instance As PlayerInstanceData
    Private storedGameScreen As GameScreen
    Private storedGameState As Byte
    Private _dropoutOnClose = True

    Public ReadOnly Property instanceId As Integer
        Get
            Return instance.id
        End Get
    End Property

    ''' <summary>
    ''' Call dropout script when closing.
    ''' </summary>
    Public WriteOnly Property dropoutOnClose As Boolean
        'If the lobby form is closing this one due to the logout script running, it can set dropoutOnClose to false so that the dropout script isn't called mulitple times.
        Set(value As Boolean)
            _dropoutOnClose = value
        End Set
    End Property

    Public Sub New(prInstanceId As Integer, prLobby As FrmLobby)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        instance = New PlayerInstanceData(prInstanceId)
        lobby = prLobby
    End Sub

    Private Sub FrmGame_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        storedGameState = MdDataAccess.getGameState(instance.game)
        Text = instance.name
        newScreen(getScreenToUse(storedGameState))
        tmrRefresh.Enabled = True
    End Sub

    Private Sub FrmGame_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Dim dropped As Boolean
        If (_dropoutOnClose) Then 'If the user needs to be removed from the game in the database, then do that here.
            dropped = MdDataAccess.playerDropout(instance.id)
            If (dropped) Then
                lobby.openGames.Remove(Me) 'Remove from list of open games.
            Else
                e.Cancel = True
            End If
        Else
            lobby.openGames.Remove(Me) 'Remove from list of open games.
        End If
    End Sub

    Private Sub tmrRefresh_Tick(sender As Object, e As EventArgs) Handles tmrRefresh.Tick
        Dim currentGameState As Byte
        If (MdDataAccess.checkAccountOnline(instance.user)) Then 'Check user is still online.
            currentGameState = MdDataAccess.getGameState(instance.game)
            If (currentGameState <> storedGameState) Then
                'Handle potential changes to player's username by admins
                If (MdDataAccess.getPlayerInfo(instance.user).uname <> instance.name) Then
                    instance = New PlayerInstanceData(instance.id)
                End If
                'Update game screen
                storedGameState = currentGameState
                newScreen(getScreenToUse(currentGameState))
            End If
            UpdatePlayerList()
        Else
            newScreen(GameScreen.loggedOut) 'Show the user they have been logged out.
        End If

    End Sub

    ''' <summary>
    ''' Handles user making a selection in-game.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub selectionMade(sender As Object, e As EventArgs)
        Dim selection As Byte = 0
        Select Case (DirectCast(sender, Control).Name)
            Case "btnRock"
                selection = 0
            Case "btnPaper"
                selection = 1
            Case "btnScissors"
                selection = 2
        End Select
        If (MdDataAccess.checkAccountOnline(instance.user)) Then 'Only register the move if the player is online.
            If (MdDataAccess.playerActionRegister(instance.id, selection)) Then
                newScreen(GameScreen.moveSelected)
            End If
        End If
    End Sub

    Private Sub UserLeave(sender As Object, e As EventArgs)
        Close()
    End Sub

    ''' <summary>
    ''' Determines which layout needs to be loaded based on the game's state.
    ''' </summary>
    ''' <param name="prState"></param>
    ''' <returns></returns>
    Private Function getScreenToUse(prState As Byte) As GameScreen
        Dim result As GameScreen = GameScreen.gameOver
        Select Case prState
            Case 0
                result = GameScreen.waiting
            Case 1
                result = GameScreen.gameRestarting
            Case 2
                If (MdDataAccess.getInstanceInfo(instanceId).playerState = 1) Then
                    'This LINQ runs through all the players that have made a move this round and finds if this player instance is amongst them
                    Dim moved = Aggregate i In MdDataAccess.getPlayerGameMoveSelectionState(instance.game)
                                    Where i.player_hasMoved = True And
                                        i.player_name = instance.name
                                    Into Count()
                    If (moved > 0) Then 'If the player has made a move this round.
                        result = GameScreen.moveSelected
                    Else 'If the player still needs to make a move.
                        result = GameScreen.moveSelection
                    End If
                Else 'If the player is out.
                    result = GameScreen.outSelection
                End If
            Case 3
                'While the game is calculating the results, show players their choice.
                result = GameScreen.moveSelected
            Case 4
                'Show the results to the players.
                result = GameScreen.roundResults
            Case 5
                'Show player if they won or lost the game.
                Select Case (MdDataAccess.getInstanceInfo(instance.id).playerState)
                    Case 0
                        result = GameScreen.gameResultsLost
                    Case 1
                        result = GameScreen.gameResultsWon
                End Select
            Case 6
                'Show players the game has ended.
                result = GameScreen.gameOver
        End Select
        Return result
    End Function

    ''' <summary>
    ''' Fills the listbox with relevent data types depending on the current screen.
    ''' </summary>
    Private Sub UpdatePlayerList()
        Select Case (storedGameScreen)
            Case GameScreen.moveSelection
                lstPlayerList.DataSource = MdDataAccess.getPlayerGameMoveSelectionState(instance.game)
            Case GameScreen.moveSelected
                lstPlayerList.DataSource = MdDataAccess.getPlayerGameMoveSelectionState(instance.game)
            Case GameScreen.outSelection
                lstPlayerList.DataSource = MdDataAccess.getPlayerGameMoveSelectionState(instance.game)
            Case GameScreen.roundResults
                lstPlayerList.DataSource = MdDataAccess.getPlayerGameState(instance.game)
        End Select
    End Sub

    ''' <summary>
    ''' Returns the player's selection represented as a string.
    ''' </summary>
    ''' <param name="prSelection"></param>
    ''' <returns></returns>
    Private Function formatPlayerSelection(ByVal prSelection As Byte?) As String
        If (prSelection.HasValue) Then
            Select Case prSelection.Value
                Case 0
                    Return "Rock"
                Case 1
                    Return "Paper"
                Case 2
                    Return "Scissors"
                Case Else
                    Return "Error"
            End Select
        Else
            Return "Nothing"
        End If
    End Function

    ''' <summary>
    ''' Fills the result boxes with the count of each action.
    ''' </summary>
    Private Sub formatActionCount()
        Dim counts As List(Of intf_getCountPerAction_Result) = MdDataAccess.getCountPerAction(instance.game)
        For Each i As intf_getCountPerAction_Result In counts
            Select Case i.Action
                Case 0
                    txtRock.Text = i.Count
                Case 1
                    txtPaper.Text = i.Count
                Case 2
                    txtScissors.Text = i.Count
            End Select
        Next





    End Sub

    'This region just consists of auto-generated code moved into an select case statement, an enum to keep track of layouts this form can change to and all the control reference attributes.
#Region "Controls and Runtime Layout"
    ''' <summary>
    ''' This sub allows the form to change its layout at runti
    ''' </summary>
    ''' <param name="prScreen">The screen, from the GameScreen enum, for the form to switch to.</param>
    Private Sub newScreen(ByVal prScreen As GameScreen)
        SuspendLayout()
        For Each i As Control In Controls
            i.Dispose()
        Next
        Controls.Clear()
        Select Case prScreen
            Case GameScreen.waiting
                'Initialise controls
                grpGameControls = New GroupBox()
                btnRock = New Button()
                btnPaper = New Button()
                btnScissors = New Button()
                lblInfo = New Label()
                btnLeave = New Button()
                grpGameControls.SuspendLayout()

                'Set Controls
                grpGameControls.Controls.Add(btnScissors)
                grpGameControls.Controls.Add(btnPaper)
                grpGameControls.Controls.Add(btnRock)
                grpGameControls.Location = New System.Drawing.Point(12, 25)
                grpGameControls.Name = "grpGameControls"
                grpGameControls.Size = New System.Drawing.Size(260, 87)
                grpGameControls.TabIndex = 0
                grpGameControls.TabStop = False

                btnRock.Location = New System.Drawing.Point(9, 19)
                btnRock.Name = "btnRock"
                btnRock.Size = New System.Drawing.Size(75, 23)
                btnRock.TabIndex = 0
                btnRock.Text = "Rock"
                btnRock.UseVisualStyleBackColor = True

                btnPaper.Location = New System.Drawing.Point(90, 19)
                btnPaper.Name = "btnPaper"
                btnPaper.Size = New System.Drawing.Size(75, 23)
                btnPaper.TabIndex = 1
                btnPaper.Text = "Paper"
                btnPaper.UseVisualStyleBackColor = True

                btnScissors.Location = New System.Drawing.Point(171, 19)
                btnScissors.Name = "btnScissors"
                btnScissors.Size = New System.Drawing.Size(75, 23)
                btnScissors.TabIndex = 2
                btnScissors.Text = "Scissors"
                btnScissors.UseVisualStyleBackColor = True

                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(67, 9)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(154, 13)
                lblInfo.TabIndex = 1
                lblInfo.Text = "Waiting for more players to join."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                btnLeave.Location = New System.Drawing.Point(12, 226)
                btnLeave.Name = "btnLeave"
                btnLeave.Size = New System.Drawing.Size(75, 23)
                btnLeave.TabIndex = 2
                btnLeave.Text = "Leave"
                btnLeave.UseVisualStyleBackColor = True

                'Add controls
                Controls.Add(btnLeave)
                Controls.Add(lblInfo)
                Controls.Add(grpGameControls)
                grpGameControls.ResumeLayout(False)
            Case GameScreen.moveSelection
                'Initialise controls
                grpGameControls = New System.Windows.Forms.GroupBox()
                btnScissors = New System.Windows.Forms.Button()
                btnPaper = New System.Windows.Forms.Button()
                btnRock = New System.Windows.Forms.Button()
                btnLeave = New System.Windows.Forms.Button()
                lblInfo = New System.Windows.Forms.Label()
                lstPlayerList = New System.Windows.Forms.ListBox()
                lblPlayerList = New System.Windows.Forms.Label()
                grpGameControls.SuspendLayout()

                'Set controls
                grpGameControls.Controls.Add(btnScissors)
                grpGameControls.Controls.Add(btnPaper)
                grpGameControls.Controls.Add(btnRock)
                grpGameControls.Location = New System.Drawing.Point(12, 26)
                grpGameControls.Name = "grpGameControls"
                grpGameControls.Size = New System.Drawing.Size(260, 87)
                grpGameControls.TabIndex = 3
                grpGameControls.TabStop = False

                btnScissors.Location = New System.Drawing.Point(171, 19)
                btnScissors.Name = "btnScissors"
                btnScissors.Size = New System.Drawing.Size(75, 23)
                btnScissors.TabIndex = 2
                btnScissors.Text = "Scissors"
                btnScissors.UseVisualStyleBackColor = True
                AddHandler btnScissors.Click, AddressOf selectionMade

                btnPaper.Location = New System.Drawing.Point(90, 19)
                btnPaper.Name = "btnPaper"
                btnPaper.Size = New System.Drawing.Size(75, 23)
                btnPaper.TabIndex = 1
                btnPaper.Text = "Paper"
                btnPaper.UseVisualStyleBackColor = True
                AddHandler btnPaper.Click, AddressOf selectionMade

                btnRock.Location = New System.Drawing.Point(9, 19)
                btnRock.Name = "btnRock"
                btnRock.Size = New System.Drawing.Size(75, 23)
                btnRock.TabIndex = 0
                btnRock.Text = "Rock"
                btnRock.UseVisualStyleBackColor = True
                AddHandler btnRock.Click, AddressOf selectionMade

                btnLeave.Location = New System.Drawing.Point(12, 227)
                btnLeave.Name = "btnLeave"
                btnLeave.Size = New System.Drawing.Size(75, 23)
                btnLeave.TabIndex = 5
                btnLeave.Text = "Leave"
                btnLeave.UseVisualStyleBackColor = True

                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(99, 10)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(91, 13)
                lblInfo.TabIndex = 4
                lblInfo.Text = "Make a selection."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                lstPlayerList.FormattingEnabled = True
                lstPlayerList.Location = New System.Drawing.Point(102, 141)
                lstPlayerList.Name = "lstPlayerList"
                lstPlayerList.Size = New System.Drawing.Size(170, 108)
                lstPlayerList.TabIndex = 6

                lblPlayerList.AutoSize = True
                lblPlayerList.Location = New System.Drawing.Point(150, 122)
                lblPlayerList.Name = "lblPlayerList"
                lblPlayerList.Size = New System.Drawing.Size(55, 13)
                lblPlayerList.TabIndex = 7
                lblPlayerList.Text = "Player List"

                'Add controls
                Controls.Add(lblPlayerList)
                Controls.Add(lstPlayerList)
                Controls.Add(grpGameControls)
                Controls.Add(btnLeave)
                Controls.Add(lblInfo)
                grpGameControls.ResumeLayout(False)
            Case GameScreen.moveSelected
                'Initialise controls
                grpGameControls = New System.Windows.Forms.GroupBox()
                btnSelection = New System.Windows.Forms.Button()
                lblPlayerList = New System.Windows.Forms.Label()
                lstPlayerList = New System.Windows.Forms.ListBox()
                btnLeave = New System.Windows.Forms.Button()
                lblInfo = New System.Windows.Forms.Label()
                grpGameControls.SuspendLayout()

                'Set controls
                grpGameControls.Controls.Add(btnSelection)
                grpGameControls.Location = New System.Drawing.Point(12, 26)
                grpGameControls.Name = "grpGameControls"
                grpGameControls.Size = New System.Drawing.Size(260, 87)
                grpGameControls.TabIndex = 8
                grpGameControls.TabStop = False

                btnSelection.Location = New System.Drawing.Point(90, 19)
                btnSelection.Name = "btnSelection"
                btnSelection.Size = New System.Drawing.Size(75, 23)
                btnSelection.TabIndex = 1
                btnSelection.Text = formatPlayerSelection(MdDataAccess.getInstanceSelection(instance.id))
                btnSelection.UseVisualStyleBackColor = True

                lblPlayerList.AutoSize = True
                lblPlayerList.Location = New System.Drawing.Point(150, 122)
                lblPlayerList.Name = "lblPlayerList"
                lblPlayerList.Size = New System.Drawing.Size(55, 13)
                lblPlayerList.TabIndex = 12
                lblPlayerList.Text = "Player List"

                lstPlayerList.FormattingEnabled = True
                lstPlayerList.Location = New System.Drawing.Point(102, 141)
                lstPlayerList.Name = "lstPlayerList"
                lstPlayerList.Size = New System.Drawing.Size(170, 108)
                lstPlayerList.TabIndex = 11

                btnLeave.Location = New System.Drawing.Point(12, 227)
                btnLeave.Name = "btnLeave"
                btnLeave.Size = New System.Drawing.Size(75, 23)
                btnLeave.TabIndex = 10
                btnLeave.Text = "Leave"
                btnLeave.UseVisualStyleBackColor = True

                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(62, 10)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(176, 13)
                lblInfo.TabIndex = 9
                lblInfo.Text = "Waiting for other players' selections."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                'Add controls
                Controls.Add(grpGameControls)
                Controls.Add(lblPlayerList)
                Controls.Add(lstPlayerList)
                Controls.Add(btnLeave)
                Controls.Add(lblInfo)
                grpGameControls.ResumeLayout(False)
            Case GameScreen.outSelection
                'Initialise controls
                lblPlayerList = New System.Windows.Forms.Label()
                lstPlayerList = New System.Windows.Forms.ListBox()
                btnLeave = New System.Windows.Forms.Button()
                lblInfo = New System.Windows.Forms.Label()

                'Set controls
                lblPlayerList.AutoSize = True
                lblPlayerList.Location = New System.Drawing.Point(150, 122)
                lblPlayerList.Name = "lblPlayerList"
                lblPlayerList.Size = New System.Drawing.Size(55, 13)
                lblPlayerList.TabIndex = 12
                lblPlayerList.Text = "Player List"

                lstPlayerList.FormattingEnabled = True
                lstPlayerList.Location = New System.Drawing.Point(102, 141)
                lstPlayerList.Name = "lstPlayerList"
                lstPlayerList.Size = New System.Drawing.Size(170, 108)
                lstPlayerList.TabIndex = 11

                btnLeave.Location = New System.Drawing.Point(12, 227)
                btnLeave.Name = "btnLeave"
                btnLeave.Size = New System.Drawing.Size(75, 23)
                btnLeave.TabIndex = 10
                btnLeave.Text = "Leave"

                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(51, 9)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(176, 26)
                lblInfo.TabIndex = 9
                lblInfo.Text = "You are out." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Waiting for other players' selections."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                'Add controls
                Controls.Add(lblPlayerList)
                Controls.Add(lstPlayerList)
                Controls.Add(btnLeave)
                Controls.Add(lblInfo)
            Case GameScreen.roundResults
                'Initialise controls
                grpGameControls = New System.Windows.Forms.GroupBox()
                btnScissors = New System.Windows.Forms.Button()
                btnPaper = New System.Windows.Forms.Button()
                btnRock = New System.Windows.Forms.Button()
                lblPlayerList = New System.Windows.Forms.Label()
                lstPlayerList = New System.Windows.Forms.ListBox()
                btnLeave = New System.Windows.Forms.Button()
                lblInfo = New System.Windows.Forms.Label()
                txtRock = New System.Windows.Forms.TextBox()
                txtPaper = New System.Windows.Forms.TextBox()
                txtScissors = New System.Windows.Forms.TextBox()
                grpGameControls.SuspendLayout()

                'Set controls
                grpGameControls.Controls.Add(txtScissors)
                grpGameControls.Controls.Add(txtPaper)
                grpGameControls.Controls.Add(txtRock)
                grpGameControls.Controls.Add(btnScissors)
                grpGameControls.Controls.Add(btnPaper)
                grpGameControls.Controls.Add(btnRock)
                grpGameControls.Location = New System.Drawing.Point(12, 26)
                grpGameControls.Name = "grpGameControls"
                grpGameControls.Size = New System.Drawing.Size(260, 87)
                grpGameControls.TabIndex = 8
                grpGameControls.TabStop = False

                btnScissors.Location = New System.Drawing.Point(171, 19)
                btnScissors.Name = "btnScissors"
                btnScissors.Size = New System.Drawing.Size(75, 23)
                btnScissors.TabIndex = 2
                btnScissors.Text = "Scissors"
                btnScissors.UseVisualStyleBackColor = True

                btnPaper.Location = New System.Drawing.Point(90, 19)
                btnPaper.Name = "btnPaper"
                btnPaper.Size = New System.Drawing.Size(75, 23)
                btnPaper.TabIndex = 1
                btnPaper.Text = "Paper"
                btnPaper.UseVisualStyleBackColor = True

                btnRock.Location = New System.Drawing.Point(9, 19)
                btnRock.Name = "btnRock"
                btnRock.Size = New System.Drawing.Size(75, 23)
                btnRock.TabIndex = 0
                btnRock.Text = "Rock"
                btnRock.UseVisualStyleBackColor = True

                lblPlayerList.AutoSize = True
                lblPlayerList.Location = New System.Drawing.Point(150, 122)
                lblPlayerList.Name = "lblPlayerList"
                lblPlayerList.Size = New System.Drawing.Size(55, 13)
                lblPlayerList.TabIndex = 12
                lblPlayerList.Text = "Player List"

                lstPlayerList.FormattingEnabled = True
                lstPlayerList.Location = New System.Drawing.Point(102, 141)
                lstPlayerList.Name = "lstPlayerList"
                lstPlayerList.Size = New System.Drawing.Size(170, 108)
                lstPlayerList.TabIndex = 11

                btnLeave.Location = New System.Drawing.Point(12, 227)
                btnLeave.Name = "btnLeave"
                btnLeave.Size = New System.Drawing.Size(75, 23)
                btnLeave.TabIndex = 10
                btnLeave.Text = "Leave"
                btnLeave.UseVisualStyleBackColor = True

                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(112, 9)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(65, 13)
                lblInfo.TabIndex = 9
                lblInfo.Text = "Round Over"
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                txtRock.Location = New System.Drawing.Point(9, 49)
                txtRock.Name = "txtRock"
                txtRock.Size = New System.Drawing.Size(75, 20)
                txtRock.TabIndex = 3
                txtRock.Text = "0"
                txtRock.TextAlign = System.Windows.Forms.HorizontalAlignment.Center

                txtPaper.Location = New System.Drawing.Point(90, 48)
                txtPaper.Name = "txtPaper"
                txtPaper.Size = New System.Drawing.Size(75, 20)
                txtPaper.TabIndex = 4
                txtPaper.Text = "0"
                txtPaper.TextAlign = System.Windows.Forms.HorizontalAlignment.Center

                txtScissors.Location = New System.Drawing.Point(171, 48)
                txtScissors.Name = "txtScissors"
                txtScissors.Size = New System.Drawing.Size(75, 20)
                txtScissors.TabIndex = 5
                txtScissors.Text = "0"
                txtScissors.TextAlign = System.Windows.Forms.HorizontalAlignment.Center

                'Add controls
                Controls.Add(grpGameControls)
                Controls.Add(lblPlayerList)
                Controls.Add(lstPlayerList)
                Controls.Add(btnLeave)
                Controls.Add(lblInfo)
                grpGameControls.ResumeLayout(False)
                grpGameControls.PerformLayout()

                'Add action counts to the textboxes
                formatActionCount()
                'Add a lose message
            Case GameScreen.gameResultsWon
                'Initialise controls
                grpGameControls = New System.Windows.Forms.GroupBox()
                btnScissors = New System.Windows.Forms.Button()
                btnPaper = New System.Windows.Forms.Button()
                btnRock = New System.Windows.Forms.Button()
                btnLeave = New System.Windows.Forms.Button()
                lblInfo = New System.Windows.Forms.Label()
                lblWinStreak = New System.Windows.Forms.Label()
                grpGameControls.SuspendLayout()

                'Set controls
                grpGameControls.Controls.Add(btnScissors)
                grpGameControls.Controls.Add(btnPaper)
                grpGameControls.Controls.Add(btnRock)
                grpGameControls.Location = New System.Drawing.Point(12, 134)
                grpGameControls.Name = "grpGameControls"
                grpGameControls.Size = New System.Drawing.Size(260, 87)
                grpGameControls.TabIndex = 3
                grpGameControls.TabStop = False

                btnScissors.Location = New System.Drawing.Point(171, 19)
                btnScissors.Name = "btnScissors"
                btnScissors.Size = New System.Drawing.Size(75, 23)
                btnScissors.TabIndex = 2
                btnScissors.Text = "Scissors"
                btnScissors.UseVisualStyleBackColor = True

                btnPaper.Location = New System.Drawing.Point(90, 19)
                btnPaper.Name = "btnPaper"
                btnPaper.Size = New System.Drawing.Size(75, 23)
                btnPaper.TabIndex = 1
                btnPaper.Text = "Paper"
                btnPaper.UseVisualStyleBackColor = True

                btnRock.Location = New System.Drawing.Point(9, 19)
                btnRock.Name = "btnRock"
                btnRock.Size = New System.Drawing.Size(75, 23)
                btnRock.TabIndex = 0
                btnRock.Text = "Rock"
                btnRock.UseVisualStyleBackColor = True

                btnLeave.Location = New System.Drawing.Point(12, 227)
                btnLeave.Name = "btnLeave"
                btnLeave.Size = New System.Drawing.Size(75, 23)
                btnLeave.TabIndex = 5
                btnLeave.Text = "Leave"
                btnLeave.UseVisualStyleBackColor = True

                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(109, 9)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(52, 13)
                lblInfo.TabIndex = 4
                lblInfo.Text = "You won."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                lblWinStreak.AutoSize = True
                lblWinStreak.Location = New System.Drawing.Point(99, 42)
                lblWinStreak.Name = "lblWinStreak"
                lblWinStreak.Size = New System.Drawing.Size(72, 13)
                lblWinStreak.TabIndex = 6
                lblWinStreak.Text = "Win Streak: " & MdDataAccess.getPlayerInfo(instance.user).currentWinStreak

                'Add controls
                Controls.Add(lblWinStreak)
                Controls.Add(grpGameControls)
                Controls.Add(btnLeave)
                Controls.Add(lblInfo)
                grpGameControls.ResumeLayout(False)
            Case GameScreen.gameResultsLost
                'Initialise controls
                grpGameControls = New System.Windows.Forms.GroupBox()
                btnScissors = New System.Windows.Forms.Button()
                btnPaper = New System.Windows.Forms.Button()
                btnRock = New System.Windows.Forms.Button()
                btnLeave = New System.Windows.Forms.Button()
                lblInfo = New System.Windows.Forms.Label()
                grpGameControls.SuspendLayout()

                'Set controls
                grpGameControls.Controls.Add(btnScissors)
                grpGameControls.Controls.Add(btnPaper)
                grpGameControls.Controls.Add(btnRock)
                grpGameControls.Location = New System.Drawing.Point(12, 135)
                grpGameControls.Name = "grpGameControls"
                grpGameControls.Size = New System.Drawing.Size(260, 87)
                grpGameControls.TabIndex = 7
                grpGameControls.TabStop = False

                btnScissors.Location = New System.Drawing.Point(171, 19)
                btnScissors.Name = "btnScissors"
                btnScissors.Size = New System.Drawing.Size(75, 23)
                btnScissors.TabIndex = 2
                btnScissors.Text = "Scissors"
                btnScissors.UseVisualStyleBackColor = True

                btnPaper.Location = New System.Drawing.Point(90, 19)
                btnPaper.Name = "btnPaper"
                btnPaper.Size = New System.Drawing.Size(75, 23)
                btnPaper.TabIndex = 1
                btnPaper.Text = "Paper"
                btnPaper.UseVisualStyleBackColor = True

                btnRock.Location = New System.Drawing.Point(9, 19)
                btnRock.Name = "btnRock"
                btnRock.Size = New System.Drawing.Size(75, 23)
                btnRock.TabIndex = 0
                btnRock.Text = "Rock"
                btnRock.UseVisualStyleBackColor = True

                btnLeave.Location = New System.Drawing.Point(12, 228)
                btnLeave.Name = "btnLeave"
                btnLeave.Size = New System.Drawing.Size(75, 23)
                btnLeave.TabIndex = 9
                btnLeave.Text = "Leave"
                btnLeave.UseVisualStyleBackColor = True

                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(109, 10)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(48, 13)
                lblInfo.TabIndex = 8
                lblInfo.Text = "You lost."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                'Add controls
                Controls.Add(grpGameControls)
                Controls.Add(btnLeave)
                Controls.Add(lblInfo)
                grpGameControls.ResumeLayout(False)
            Case GameScreen.gameRestarting
                'Initialise controls
                grpGameControls = New System.Windows.Forms.GroupBox()
                btnScissors = New System.Windows.Forms.Button()
                btnPaper = New System.Windows.Forms.Button()
                btnRock = New System.Windows.Forms.Button()
                btnLeave = New System.Windows.Forms.Button()
                lblInfo = New System.Windows.Forms.Label()
                grpGameControls.SuspendLayout()

                'Set controls
                grpGameControls.Controls.Add(btnScissors)
                grpGameControls.Controls.Add(btnPaper)
                grpGameControls.Controls.Add(btnRock)
                grpGameControls.Location = New System.Drawing.Point(12, 26)
                grpGameControls.Name = "grpGameControls"
                grpGameControls.Size = New System.Drawing.Size(260, 87)
                grpGameControls.TabIndex = 3
                grpGameControls.TabStop = False

                btnScissors.Location = New System.Drawing.Point(171, 19)
                btnScissors.Name = "btnScissors"
                btnScissors.Size = New System.Drawing.Size(75, 23)
                btnScissors.TabIndex = 2
                btnScissors.Text = "Scissors"
                btnScissors.UseVisualStyleBackColor = True

                btnPaper.Location = New System.Drawing.Point(90, 19)
                btnPaper.Name = "btnPaper"
                btnPaper.Size = New System.Drawing.Size(75, 23)
                btnPaper.TabIndex = 1
                btnPaper.Text = "Paper"
                btnPaper.UseVisualStyleBackColor = True

                btnRock.Location = New System.Drawing.Point(9, 19)
                btnRock.Name = "btnRock"
                btnRock.Size = New System.Drawing.Size(75, 23)
                btnRock.TabIndex = 0
                btnRock.Text = "Rock"
                btnRock.UseVisualStyleBackColor = True

                btnLeave.Location = New System.Drawing.Point(12, 227)
                btnLeave.Name = "btnLeave"
                btnLeave.Size = New System.Drawing.Size(75, 23)
                btnLeave.TabIndex = 5
                btnLeave.Text = "Leave"
                btnLeave.UseVisualStyleBackColor = True

                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(83, 9)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(108, 13)
                lblInfo.TabIndex = 4
                lblInfo.Text = "New game is starting."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                'Add controls
                Controls.Add(grpGameControls)
                Controls.Add(btnLeave)
                Controls.Add(lblInfo)
                grpGameControls.ResumeLayout(False)
            Case GameScreen.loggedOut
                'Initialise controls
                lblInfo = New System.Windows.Forms.Label()
                btnLeave = New System.Windows.Forms.Button()

                'Set controls
                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(30, 99)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(231, 26)
                lblInfo.TabIndex = 0
                lblInfo.Text = "You have been logged out." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Please close the lobby window and log back in."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                btnLeave.Location = New System.Drawing.Point(13, 226)
                btnLeave.Name = "btnLeave"
                btnLeave.Size = New System.Drawing.Size(75, 23)
                btnLeave.TabIndex = 1
                btnLeave.Text = "Leave"
                btnLeave.UseVisualStyleBackColor = True

                'Add controls
                Controls.Add(btnLeave)
                Controls.Add(lblInfo)
            Case GameScreen.gameOver
                'Initialise controls
                lblInfo = New System.Windows.Forms.Label()
                btnLeave = New System.Windows.Forms.Button()

                'Set controls
                lblInfo.AutoSize = True
                lblInfo.Location = New System.Drawing.Point(30, 99)
                lblInfo.Name = "lblInfo"
                lblInfo.Size = New System.Drawing.Size(231, 26)
                lblInfo.TabIndex = 0
                lblInfo.Text = "This game has ended." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Join another game."
                lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter

                btnLeave.Location = New System.Drawing.Point(13, 226)
                btnLeave.Name = "btnLeave"
                btnLeave.Size = New System.Drawing.Size(75, 23)
                btnLeave.TabIndex = 1
                btnLeave.Text = "Leave"
                btnLeave.UseVisualStyleBackColor = True

                'Add controls
                Controls.Add(btnLeave)
                Controls.Add(lblInfo)
        End Select
        'Add functionality to the leave button
        AddHandler btnLeave.Click, AddressOf UserLeave
        'Let the form lay the controls out again.
        ResumeLayout(True)
        'Keep track of what screen is loaded
        storedGameScreen = prScreen
        'Make sure list boxes have data
        UpdatePlayerList()
    End Sub

    ''' <summary>
    ''' This enum defines the layouts that the form can change to at runti
    ''' </summary>
    Private Enum GameScreen
        waiting
        moveSelection
        moveSelected
        outSelection
        roundResults
        gameResultsWon
        gameResultsLost
        gameRestarting
        gameOver
        loggedOut
    End Enum

    Friend WithEvents grpGameControls As GroupBox
    Friend WithEvents btnRock As Button
    Friend WithEvents btnPaper As Button
    Friend WithEvents btnScissors As Button
    Friend WithEvents txtRock As TextBox
    Friend WithEvents txtPaper As TextBox
    Friend WithEvents txtScissors As TextBox
    Friend WithEvents btnSelection As Button
    Friend WithEvents lstPlayerList As ListBox
    Friend WithEvents btnLeave As Button
    Friend WithEvents lblInfo As Label
    Friend WithEvents lblPlayerList As Label
    Friend WithEvents lblWinStreak As Label


#End Region

End Class