Imports System.IO

Public Class Worker

    Inherits System.ComponentModel.Component

    ' Declares the variables you will use to hold your thread objects.

    Public WorkerThread As System.Threading.Thread

    Public filelist_file As String = ""
    Public result As String = ""

    Public Event WorkerComplete(ByVal Result As String)
    Public Event WorkerProgress(ByVal value As Integer)
    Public Event WorkerAlert(ByVal message As String)


#Region " Component Designer generated code "

    Public Sub New(ByVal Container As System.ComponentModel.IContainer)
        MyClass.New()

        'Required for Windows.Forms Class Composition Designer support
        Container.Add(Me)
    End Sub

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Component overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        components = New System.ComponentModel.Container
    End Sub

#End Region

    Private Sub Error_Handler(ByVal message As String)
        Try
            Dim Display_Message1 As New Display_Message(message)
            Display_Message1.ShowDialog()
        Catch ex As Exception
            MsgBox("An error occurred in File List Deleter's error handling routine. The application will try to recover from this serious error.", MsgBoxStyle.Critical, "Critical Error Encountered")
        End Try
    End Sub

    Public Sub ChooseThreads(ByVal threadNumber As Integer)
        Try
            ' Determines which thread to start based on the value it receives.
            Select Case threadNumber
                Case 1
                    ' Sets the thread using the AddressOf the subroutine where
                    ' the thread will start.
                    WorkerThread = New System.Threading.Thread(AddressOf WorkerExecute)
                    ' Starts the thread.
                    WorkerThread.Start()

            End Select
        Catch ex As Exception
            Error_Handler(ex.Message)
        End Try
    End Sub

    Public Sub WorkerExecute()
        Dim count_total As Long
        count_total = 0
        Dim count_total_succeeded As Long
        count_total_succeeded = 0
        RaiseEvent WorkerAlert("Process Initiated (" & Now() & ")" & vbCrLf)
        RaiseEvent WorkerAlert("Parsing file listing: " & filelist_file & vbCrLf)
        RaiseEvent WorkerAlert("---------------------------------" & vbCrLf)
        Dim filereader As StreamReader
        Try


            filereader = New StreamReader(filelist_file, True)
            Dim lineread As String = filereader.ReadLine
            Dim filetomanipulate As File



            Do While Not lineread Is Nothing
                lineread = lineread.Trim()
                count_total = count_total + 1
                RaiseEvent WorkerProgress(count_total)

                RaiseEvent WorkerAlert("Deleting " & lineread & "...")
                If filetomanipulate.Exists(lineread) = True Then
                    Try

                        filetomanipulate.Delete(lineread)
                        count_total_succeeded = count_total_succeeded + 1
                        RaiseEvent WorkerAlert("OK" & vbCrLf)
                    Catch ex As Exception
                        RaiseEvent WorkerAlert("FAIL (" & ex.Message & ")" & vbCrLf)
                    End Try
                Else
                    RaiseEvent WorkerAlert("FAIL (File to delete doesn't exist)" & vbCrLf)
                End If

                lineread = filereader.ReadLine
            Loop
            Try
                Dim unprocessed As String = filereader.ReadToEnd()
                If unprocessed.Trim().Length > 0 And Not unprocessed.Trim() = "" Then
                    RaiseEvent WorkerAlert("Warning: Unparsed data has been located in the file listing. It is possible that some files from the list may have been ignored." & vbCrLf)
                End If
            Catch ex As Exception
                RaiseEvent WorkerAlert("Error: The following error occurred by running the EOF check: " & ex.Message & vbCrLf)
            End Try


            filereader.Close()

        Catch ex As Exception
            filereader.Close()
            RaiseEvent WorkerAlert("Error: The following error was trapped by the Worker string: " & ex.Message & vbCrLf)
            Error_Handler(ex.ToString())
        End Try
        RaiseEvent WorkerAlert("---------------------------------" & vbCrLf)
        RaiseEvent WorkerAlert(count_total & " file entries located" & vbCrLf & count_total_succeeded & " files successfully removed" & vbCrLf & "Process Completed (" & Now() & ")" & vbCrLf)
        RaiseEvent WorkerComplete(count_total & " file entries located" & vbCrLf & count_total_succeeded & " files successfully removed" & vbCrLf & "Process Completed (" & Now() & ")" & vbCrLf)
        WorkerThread.Abort()
    End Sub



    Private Function DosShellCommand(ByVal AppToRun As String) As String
        Dim s As String = ""
        Try
            Dim myProcess As Process = New Process

            myProcess.StartInfo.FileName = "cmd.exe"
            myProcess.StartInfo.UseShellExecute = False
            myProcess.StartInfo.CreateNoWindow = True
            myProcess.StartInfo.RedirectStandardInput = True
            myProcess.StartInfo.RedirectStandardOutput = True
            myProcess.StartInfo.RedirectStandardError = True
            myProcess.Start()
            Dim sIn As StreamWriter = myProcess.StandardInput
            sIn.AutoFlush = True

            Dim sOut As StreamReader = myProcess.StandardOutput
            Dim sErr As StreamReader = myProcess.StandardError
            sIn.Write(AppToRun & _
               System.Environment.NewLine)
            sIn.Write("exit" & System.Environment.NewLine)
            s = sOut.ReadToEnd()
            If Not myProcess.HasExited Then
                myProcess.Kill()
            End If

            'MessageBox.Show("The 'dir' command window was closed at: " & myProcess.ExitTime & "." & System.Environment.NewLine & "Exit Code: " & myProcess.ExitCode)

            sIn.Close()
            sOut.Close()
            sErr.Close()
            myProcess.Close()
            'MessageBox.Show(s)
        Catch ex As Exception
            Error_Handler("An """ & ex.Message & """ error occurred while launching DOS shell. The program will attempt to recover shortly.")
        End Try
        Return s
    End Function

    

End Class
