Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Windows.Forms

Class MainWindow

    Private ReadOnly Magic As UInteger = &HEA6E

    Dim TotalBytes As Integer
    Dim UseMod As Boolean = False
    Dim WithEvents SenderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}

    Public Structure WorkerArgs
        Private _DeviceIP As IPAddress
        Private _FileToSend As String
        Private _ChunkSize As Integer

        Public Property DeviceIP As IPAddress
            Get
                Return _DeviceIP
            End Get
            Set
                _DeviceIP = Value
            End Set
        End Property

        Public Property FileToSend As String
            Get
                Return _FileToSend
            End Get
            Set
                _FileToSend = Value
            End Set
        End Property

        Public Property ChunkSize As Integer
            Get
                Return _ChunkSize
            End Get
            Set
                _ChunkSize = Value
            End Set
        End Property
    End Structure

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\homebrew.list") Then
            For Each Homebrew In File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\homebrew.list")

                If Not String.IsNullOrWhiteSpace(Homebrew) Then

                    Dim HomebrewName As String = Homebrew.Split(";"c)(0)
                    Dim HomebrewConsole As String = Homebrew.Split(";"c)(1)
                    Dim HomebrewFirmware As String = Homebrew.Split(";"c)(2)
                    Dim HomebrewPath As String = Homebrew.Split(";"c)(3)

                    Dim HomebrewItem As New HomebrewListViewItem() With {.FileName = HomebrewName, .Console = HomebrewConsole, .Firmware = HomebrewFirmware, .FilePath = HomebrewPath}
                    HomebrewListView.Items.Add(HomebrewItem)

                End If

            Next
        Else
            File.Create(My.Computer.FileSystem.CurrentDirectory + "\games.list")
        End If

    End Sub

    Private Sub AddNewELFButton_Click(sender As Object, e As RoutedEventArgs) Handles AddNewELFButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select an .elf file", .Filter = "ELF files (*.elf)|*.elf"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            Dim HomebrewFileName As String = Path.GetFileNameWithoutExtension(OFD.FileName)

            Dim HomebrewName As String = ""
            Dim HomebrewConsole As String = ""
            Dim HomebrewFirmware As String = ""
            Dim HomebrewPath As String = OFD.FileName

            If OFD.FileName.Contains("PS4") Then
                HomebrewConsole = "PS4"

                Dim HomebrewFileNameSplitArray As String() = HomebrewFileName.Split(New String() {"-PS4-"}, StringSplitOptions.None)
                HomebrewName = HomebrewFileNameSplitArray(0).Replace("-"c, " ")
                HomebrewFirmware = HomebrewFileNameSplitArray(1).Replace("-"c, ".")


            ElseIf OFD.FileName.Contains("PS5") Then
                HomebrewConsole = "PS5"

                Dim HomebrewFileNameSplitArray As String() = HomebrewFileName.Split(New String() {"-PS5-"}, StringSplitOptions.None)
                HomebrewName = HomebrewFileNameSplitArray(0).Replace("-"c, " ")
                HomebrewFirmware = HomebrewFileNameSplitArray(1).Replace("-"c, ".")

            Else
                HomebrewConsole = "Unknown"
                HomebrewFirmware = "Unknown"

            End If

            'Add to the HomebrewListView
            Dim HomebrewItem As New HomebrewListViewItem() With {.FileName = HomebrewName, .Console = HomebrewConsole, .Firmware = HomebrewFirmware, .FilePath = HomebrewPath}
            HomebrewListView.Items.Add(HomebrewItem)

            'Add new line to homebrew.list
            Using HomebrewFileWriter As New StreamWriter(My.Computer.FileSystem.CurrentDirectory + "\homebrew.list", True)
                HomebrewFileWriter.WriteLine(HomebrewName + ";" + HomebrewConsole + ";" + HomebrewFirmware + ";" + HomebrewPath)
                HomebrewFileWriter.Close()
            End Using

        End If
    End Sub

    Private Sub DeleteSelectedELFButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteSelectedELFButton.Click

        If HomebrewListView.SelectedItem IsNot Nothing Then

            If MsgBox("Do you really want to remove this ELF from the homebrew library ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then
                Dim SelectedHomebrew As HomebrewListViewItem = CType(HomebrewListView.SelectedItem, HomebrewListViewItem)

                'Get the line of the selected homebrew
                Dim LineOfHomebrew As Integer
                Dim HomebrewlistLines = File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\homebrew.list")
                For i = 0 To HomebrewlistLines.Length - 1
                    If HomebrewlistLines(i).StartsWith(SelectedHomebrew.FileName) Then
                        LineOfHomebrew = i
                    End If
                Next

                'Remove it from the homebrew.list
                Dim HomebrewlistLinesAsList As List(Of String) = File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\homebrew.list").ToList
                HomebrewlistLinesAsList.RemoveAt(LineOfHomebrew)

                'Write the new homebrew.list
                File.WriteAllLines(My.Computer.FileSystem.CurrentDirectory + "\homebrew.list", HomebrewlistLinesAsList)

                HomebrewListView.Items.Remove(HomebrewListView.SelectedItem)
            End If

        Else
            MsgBox("Please select an ELF file from the list.", MsgBoxStyle.Exclamation, "Could not remove ELF")
        End If

    End Sub

    Private Sub PS2BackupManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles PS2BackupManagerButton.Click
        Dim NewPS2BackupManager As New PS2BackupManager() With {.ShowActivated = True, .ConsoleIP = IPTextBox.Text}
        If UseMod = True Then NewPS2BackupManager.UseModELF = True 'Tell the backup manager to use the modified network loader and send larger chunks
        NewPS2BackupManager.Show()
    End Sub

    Private Sub RetroRepackerButton_Click(sender As Object, e As RoutedEventArgs) Handles RetroRepackerButton.Click
        Dim NewRetroRepacker As New RetroRepacker() With {.ShowActivated = True}
        NewRetroRepacker.Show()
    End Sub

    Private Sub FormatUSBButton_Click(sender As Object, e As RoutedEventArgs) Handles FormatUSBButton.Click
        Dim NewUSBFormatTool As New USBFormat() With {.ShowActivated = True}
        NewUSBFormatTool.Show()
    End Sub

    Public Sub SendNewFile(DeviceIP As IPAddress, FileToSend As String)

        Dim FileInfos As New FileInfo(FileToSend)
        Dim FileSizeAsLong As Long = FileInfos.Length
        Dim FileSizeAsULong As ULong = CULng(FileInfos.Length)

        Dim MagicBytes = BytesConverter.ToLittleEndian(Magic)
        Dim NewFileSizeBytes = BytesConverter.ToLittleEndian(FileSizeAsULong)

        Using SenderSocket As New Socket(SocketType.Stream, ProtocolType.Tcp) With {.ReceiveTimeout = 3000}

            SenderSocket.Connect(DeviceIP, 9045)

            SenderSocket.Send(MagicBytes)
            SenderSocket.Send(NewFileSizeBytes)

            Dim BytesRead As Integer

            Dim TotalBytes As Integer
            Dim SendBytes As Integer

            Dim Buffer(4096 - 1) As Byte

            'Open the file and send chunks of 4096
            Using SenderFileStream As New FileStream(FileToSend, FileMode.Open, FileAccess.Read)

                TotalBytes = CInt(SenderFileStream.Length)

                Do
                    BytesRead = SenderFileStream.Read(Buffer, 0, Buffer.Length)

                    If BytesRead > 0 Then
                        'Send
                        SenderSocket.Send(Buffer, 0, BytesRead, SocketFlags.None)
                        SendBytes += 4096

                        If SendStatusTextBlock.Dispatcher.CheckAccess() = False Then
                            SendStatusTextBlock.Dispatcher.BeginInvoke(Sub() SendStatusTextBlock.Text = "Sending file: " + SendBytes.ToString + " of " + TotalBytes.ToString + " sent.")
                        Else
                            SendStatusTextBlock.Text = "Sending file: " + SendBytes.ToString + " of " + TotalBytes.ToString + " sent."
                        End If

                        If SendProgressBar.Dispatcher.CheckAccess() = False Then
                            SendProgressBar.Dispatcher.BeginInvoke(Sub() SendProgressBar.Value += 4096)
                        Else
                            SendProgressBar.Value += 4096
                        End If

                    End If
                Loop While BytesRead > 0

            End Using

            SenderSocket.Close()

        End Using
    End Sub

    Private Sub SendELFButton_Click(sender As Object, e As RoutedEventArgs) Handles SendELFButton.Click

        If HomebrewListView.SelectedItem IsNot Nothing And Not String.IsNullOrWhiteSpace(IPTextBox.Text) Then

            Dim DeviceIP As IPAddress

            Try
                DeviceIP = IPAddress.Parse(IPTextBox.Text)
            Catch ex As FormatException
                MsgBox("Could not send selected ELF. Please check your IP.", MsgBoxStyle.Exclamation, "Error sending file")
                Exit Sub
            End Try

            Dim SelectedELF As HomebrewListViewItem = CType(HomebrewListView.SelectedItem, HomebrewListViewItem)

            'Check if the modified network game loader has been sent
            If SelectedELF.FileName.Contains("mod network game loader") Then UseMod = True Else UseMod = False

            'Set the progress bar maximum and TotalBytes to send
            SendProgressBar.Value = 0
            SendProgressBar.Maximum = CDbl(New FileInfo(SelectedELF.FilePath).Length)
            TotalBytes = CInt(New FileInfo(SelectedELF.FilePath).Length)

            'Start sending
            SenderWorker.RunWorkerAsync(New WorkerArgs() With {.DeviceIP = DeviceIP, .FileToSend = SelectedELF.FilePath})
        End If

    End Sub

    Private Sub SenderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles SenderWorker.DoWork

        Dim CurrentWorkerArgs As WorkerArgs = CType(e.Argument, WorkerArgs)

        Dim FileInfos As New FileInfo(CurrentWorkerArgs.FileToSend)
        Dim FileSizeAsLong As Long = FileInfos.Length
        Dim FileSizeAsULong As ULong = CULng(FileInfos.Length)

        Dim MagicBytes = BytesConverter.ToLittleEndian(Magic)
        Dim NewFileSizeBytes = BytesConverter.ToLittleEndian(FileSizeAsULong)

        Using SenderSocket As New Socket(SocketType.Stream, ProtocolType.Tcp) With {.ReceiveTimeout = 3000}

            SenderSocket.Connect(CurrentWorkerArgs.DeviceIP, 9045)

            SenderSocket.Send(MagicBytes)
            SenderSocket.Send(NewFileSizeBytes)

            Dim BytesRead As Integer
            Dim SendBytes As Integer
            Dim Buffer(4096 - 1) As Byte

            'Open the file and send chunks of 4096
            Using SenderFileStream As New FileStream(CurrentWorkerArgs.FileToSend, FileMode.Open, FileAccess.Read)

                Do
                    BytesRead = SenderFileStream.Read(Buffer, 0, Buffer.Length)

                    If BytesRead > 0 Then
                        'Socket.Send returs the number of send bytes so we can set SendBytes easily
                        SendBytes += SenderSocket.Send(Buffer, 0, BytesRead, SocketFlags.None)

                        'Update the status text
                        If SendStatusTextBlock.Dispatcher.CheckAccess() = False Then
                            SendStatusTextBlock.Dispatcher.BeginInvoke(Sub() SendStatusTextBlock.Text = "Sending file: " + SendBytes.ToString + " bytes of " + TotalBytes.ToString + " bytes sent.")
                        Else
                            SendStatusTextBlock.Text = "Sending file: " + SendBytes.ToString + " of " + TotalBytes.ToString + " sent."
                        End If

                        'Update the status progress bar
                        If SendProgressBar.Dispatcher.CheckAccess() = False Then
                            SendProgressBar.Dispatcher.BeginInvoke(Sub() SendProgressBar.Value += 4096)
                        Else
                            SendProgressBar.Value += 4096
                        End If

                    End If
                Loop While BytesRead > 0

            End Using

            'Close the connection
            SenderSocket.Close()

        End Using

    End Sub

    Private Sub SenderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles SenderWorker.RunWorkerCompleted

        If Not e.Cancelled Then
            If MsgBox("ELF successfully sent!" + vbCrLf + "Clear the progress status ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then

                'Reset the progress status
                If SendStatusTextBlock.Dispatcher.CheckAccess() = False Then
                    SendStatusTextBlock.Dispatcher.BeginInvoke(Sub() SendStatusTextBlock.Text = "Status :")
                Else
                    SendStatusTextBlock.Text = "Status :"
                End If

                If SendProgressBar.Dispatcher.CheckAccess() = False Then
                    SendProgressBar.Dispatcher.BeginInvoke(Sub() SendProgressBar.Value = 0)
                Else
                    SendProgressBar.Value = 0
                End If

            End If
        End If

    End Sub

End Class
