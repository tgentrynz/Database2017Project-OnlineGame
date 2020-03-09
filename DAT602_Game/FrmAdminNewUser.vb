Public Class FrmAdminNewUser
    Private Sub btnCreate_Click(sender As Object, e As EventArgs) Handles btnCreate.Click
        Dim newUser As Integer?
        Dim err As Boolean = False
        Dim msg As String = ""
        If (MdDataValidation.checkUsername(txtUsername.Text) And MdDataValidation.checkPassword(txtPassword.Text)) Then
            newUser = MdDataAccess.createPlayer(txtUsername.Text, txtPassword.Text)
            If (Not newUser.HasValue) Then
                msg = vbCrLf & "Database error."
                err = True
            End If
        Else
            msg = vbCrLf & "Invalid username or password." & vbCrLf & "Passwords require a number and neither can include spaces."
            err = True
        End If
        If (err) Then
            MessageBox.Show("There was an error creating the account." & msg, "Account creation error.", MessageBoxButtons.OK)
        End If
        Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Close()
    End Sub
End Class