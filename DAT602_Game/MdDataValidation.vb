Module MdDataValidation
    Const USERNAME_LENGTH As Integer = 16

    ''' <summary>
    ''' Ensures that a username string is at least one character long, no longer than 16 characters and includes no whitespaces.
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Function checkUsername(input As String) As Boolean
        Dim output As Boolean = True
        'Check for white spaces
        If ((Aggregate c In input Where Char.IsWhiteSpace(c) Into Count()) <> 0) Then
            output = False
        End If
        'Check length
        If (input.Length > USERNAME_LENGTH Or input.Length = 0) Then
            output = False
        End If
        Return output
    End Function

    ''' <summary>
    ''' Ensures that a password string is not empty, no longer than 16 characters and includes no whitespaces.
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    Public Function checkPassword(input As String) As Boolean
        Dim output As Boolean = True
        'Check for white space
        If ((Aggregate c In input Where Char.IsWhiteSpace(c) Into Count()) <> 0) Then
            output = False
        End If
        'Check length
        If (input.Length > USERNAME_LENGTH Or input.Length = 0) Then
            output = False
        End If
        'Check Numbers
        If ((Aggregate c In input Where Char.IsDigit(c) Into Count()) = 0) Then
            output = False
        End If
        Return output
    End Function
End Module
