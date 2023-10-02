Private Sub Import()
    
    '************************************************************************
    '  Product Name: DN Database Import Auftrag 1.9
    '  Author: Branislav Juhás
    '  Date: 2022-07-08
    '  Part of the DN Software Heritage
    '************************************************************************
    
    ' Declarations
    Dim Content() As String
    Dim File As Object
    Dim Header As String
    Dim I As Long
    Dim ID As Long
    Dim Inputs() As String
    Dim Path As String
    Dim PathNotNull As Boolean
    Dim Skipped As Integer

    Header = "AWUWF0TCRTAEG1PB9NACDNHENKELVISIONNJCEP8XHDNHENKELVISIONW2P1R7QTDNHENKELVISIONR77KK0EBDNHENKELVISION"

    ' Open the file dialog and get the selected file path
    With Application.FileDialog(3)
        .AllowMultiSelect = False
        .Filters.Clear
        .Filters.Add "DN Auftrag Import File", "*.dnfa", 1
        .Show

        ' Check if a file was selected
        If .SelectedItems.Count > 0 Then
            Path = .SelectedItems(1)
            PathNotNull = True
        End If
    End With

    ' If a file was selected, continue
    If PathNotNull Then
        ' Open the file
        Set File = CreateObject("ADODB.Stream")
        File.Charset = "utf-8"
        File.Open
        File.LoadFromFile (Path)
        
        ' Load the content of the file into an array
        Content = Split(File.ReadText(), vbCrLf)
        
        ' Close the file and release the object
        File.Close
        Set File = Nothing
        
        ' Check if the file is in the correct format and decide if it is Fauf
        If Content(6) <> Header Then
            MsgBox "Selected file is not valid DN Auftrag Import File format", vbCritical, "Error"
            Exit Sub
        End If
        
        ' Loop through the content of the file and insert into the database
        For I = 7 To UBound(Content)
            Inputs = Split(Content(I), vbTab)

            ' Check if the array contains enough elements
            If (UBound(Inputs) - LBound(Inputs) >= 8) Then
                ' Check if the first column is not empty and insert the record based on the length of the value
                ' If the length is 9, it is a Fauf, otherwise it is an Auftragsnummer
                If Inputs(0) <> "" Then
                    If Len(Inputs(0)) = 9 Then
                        CurrentDb.Execute "INSERT INTO Fehler (Fauf) VALUES ('" & Inputs(0) & "');", dbFailOnError
                    Else
                        CurrentDb.Execute "INSERT INTO Fehler (Auftragsnummer) VALUES ('" & Inputs(0) & "');", dbFailOnError
                    End If

                    ID = CurrentDb.OpenRecordset("SELECT @@IDENTITY AS ID").Fields("ID").Value
                    
                    ' Update the values for each column
                    If Inputs(1) <> "" Then
                        CurrentDb.Execute "UPDATE Fehler SET Ort = '" & Inputs(1) & "' WHERE ID = " & ID & ";", dbFailOnError
                    End If

                    If Inputs(2) <> "" Then
                        CurrentDb.Execute "UPDATE Fehler SET Fehler = '" & Inputs(2) & "' WHERE ID = " & ID & ";", dbFailOnError
                    End If

                    If Inputs(3) <> "" Then
                        CurrentDb.Execute "UPDATE Fehler SET BMK = '" & Inputs(3) & "' WHERE ID = " & ID & ";", dbFailOnError
                    End If
                    
                    If Inputs(4) <> "" Then
                        CurrentDb.Execute "UPDATE Fehler SET Verursacher = '" & Inputs(4) & "' WHERE ID = " & ID & ";", dbFailOnError
                    End If
                    
                    If Inputs(5) <> "" Then
                        CurrentDb.Execute "UPDATE Fehler SET Klassifizierung = '" & Inputs(5) & "' WHERE ID = " & ID & ";", dbFailOnError
                    End If
                    
                    If Inputs(6) <> "" Then
                        CurrentDb.Execute "UPDATE Fehler SET Fehlerart = '" & Inputs(6) & "' WHERE ID = " & ID & ";", dbFailOnError
                    End If
                    
                    If Inputs(7) <> "" Then
                        CurrentDb.Execute "UPDATE Fehler SET Name = '" & Inputs(7) & "' WHERE ID = " & ID & ";", dbFailOnError
                    End If
                    
                    If Inputs(8) <> "" Then
                        CurrentDb.Execute "UPDATE Fehler SET Erfassungsdatum = '" & CDate(Inputs(8)) & "' WHERE ID = " & ID & ";", dbFailOnError
                    End If

                    CurrentDb.Execute "UPDATE Fehler SET Klassifizierung_NR = 0 WHERE ID = " & ID & ";", dbFailOnError
                    CurrentDb.Execute "UPDATE Fehler SET Zeit = 0 WHERE ID = " & ID & ";", dbFailOnError
                
                Else
                    Skipped = Skipped + 1
                End If
            End If
        Next I

        ' Write the reprt to the user
        MsgBox "DN Import finished succesfully:" & vbCrLf & vbCrLf & "Records imported:       " & I - 7 - Skipped & vbCrLf & "Records skipped:         " & Skipped, vbInformation, "DN Import Finished"
    End If
End Sub