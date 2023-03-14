Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Windows.Forms

Public Class PS2BackupManager

    Dim WithEvents PSXDatacenterBrowser As New WebBrowser()
    Dim WithEvents SenderWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}

    Dim AddToList As Boolean = False
    Dim SelectedISO As String
    Dim TotalBytes As Long

    Public ConsoleIP As String
    Public UseModELF As Boolean = False
    Private ReadOnly Magic As UInteger = &HEA6E

    Private Sub PS2BackupManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\games.list") Then

            For Each Game In File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\games.list")

                If Not String.IsNullOrWhiteSpace(Game) Then

                    Dim Title As String = Game.Split(";"c)(0)
                    Dim ID As String = Game.Split(";"c)(1)
                    Dim Region As String = Game.Split(";"c)(2)
                    Dim GamePath As String = Game.Split(";"c)(3)

                    Dim GameItem As New GameListViewItem() With {.GameTitle = Title, .GameID = ID, .GameRegion = Region, .GameFilePath = GamePath}
                    GamesListView.Items.Add(GameItem)

                End If

            Next

        End If

    End Sub

    Private Sub PSXDatacenterBrowser_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles PSXDatacenterBrowser.DocumentCompleted

        Dim GameTitle As String = ""
        Dim GameID As String = ""
        Dim GameRegion As String = ""
        Dim GameGenre As String = ""
        Dim GameDeveloper As String = ""
        Dim GamePublisher As String = ""
        Dim GameReleaseDate As String = ""
        Dim GameDescription As String = ""

        Try
            'Get game infos
            Dim infoTable As HtmlElement = PSXDatacenterBrowser.Document.GetElementById("table4")
            Dim infoRows As HtmlElementCollection = PSXDatacenterBrowser.Document.GetElementsByTagName("tr")

            'Game Title
            If infoRows.Item(4).InnerText IsNot Nothing Then
                GameTitle = infoRows.Item(4).InnerText.Split(New String() {"OFFICIAL TITLE "}, StringSplitOptions.RemoveEmptyEntries)(0)
            End If

            'Game ID
            If infoRows.Item(6).InnerText IsNot Nothing Then
                GameID = infoRows.Item(6).InnerText.Split(New String() {"SERIAL NUMBER(S) "}, StringSplitOptions.RemoveEmptyEntries)(0)
            End If

            'Region
            If infoRows.Item(7).InnerText IsNot Nothing Then
                Dim Region As String = infoRows.Item(7).InnerText.Split(New String() {"REGION "}, StringSplitOptions.RemoveEmptyEntries)(0)
                Select Case Region
                    Case "PAL"
                        GameRegion = "Europe"
                    Case "NTSC-U"
                        GameRegion = "US"
                    Case "NTSC-J"
                        GameRegion = "Japan"
                End Select
            End If

            'Genre
            If infoRows.Item(8).InnerText IsNot Nothing Then
                GameGenre = infoRows.Item(8).InnerText.Split(New String() {"GENRE / STYLE "}, StringSplitOptions.RemoveEmptyEntries)(0)
            End If

            'Developer
            If infoRows.Item(9).InnerText IsNot Nothing Then
                GameDeveloper = infoRows.Item(9).InnerText.Split(New String() {"DEVELOPER "}, StringSplitOptions.RemoveEmptyEntries)(0)
            End If

            'Publisher
            If infoRows.Item(10).InnerText IsNot Nothing Then
                GamePublisher = infoRows.Item(10).InnerText.Split(New String() {"PUBLISHER "}, StringSplitOptions.RemoveEmptyEntries)(0)
            End If

            'Release Date
            If infoRows.Item(11).InnerText IsNot Nothing Then
                GameReleaseDate = infoRows.Item(11).InnerText.Split(New String() {"DATE RELEASED "}, StringSplitOptions.RemoveEmptyEntries)(0)
            End If

            'Get the game cover
            If PSXDatacenterBrowser.Document.GetElementById("table2") IsNot Nothing Then
                GameCoverImage.Source = New BitmapImage(New Uri(PSXDatacenterBrowser.Document.GetElementById("table2").GetElementsByTagName("img")(1).GetAttribute("src")))
            End If

            'Get the game description
            If PSXDatacenterBrowser.Document.GetElementById("table16") IsNot Nothing Then
                GameDescription = PSXDatacenterBrowser.Document.GetElementById("table16").GetElementsByTagName("tr")(0).InnerText
            End If

            GameTitleTextBlock.Text = GameTitle
            GameDescriptionTextBlock.Text = GameDescription
            RegionTextBlock.Text = GameRegion
            GenreTextBlock.Text = GameGenre
            GameIDTextBlock.Text = GameID
            PublisherTextBlock.Text = GamePublisher
            ReleaseDateTextBlock.Text = GameReleaseDate
            DeveloperTextBlock.Text = GameDeveloper

            'Checks before adding to the GamesListView
            If GameTitle = "" Then GameTitle = "Unknown"
            If GameID = "" Then GameID = "Unknown"
            If GameRegion = "" Then GameRegion = "Unknown"

            If AddToList = True Then
                'Add to the GamesListView and games.list
                AddToGameList(GameTitle, GameID, GameRegion)
                AddToList = False
            End If

        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try

    End Sub

    Private Sub AddNewGameButton_Click(sender As Object, e As RoutedEventArgs) Handles AddNewGameButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select an .iso file", .Filter = "ISO files (*.iso)|*.iso"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            Dim GameID As String = GetGameID(OFD.FileName)
            SelectedISO = OFD.FileName

            If GameID = "ID not found" Then
                'Add to the GamesListView
                Dim GameItem As New GameListViewItem() With {.GameTitle = "Not found", .GameID = GameID, .GameRegion = "Unknown", .GameFilePath = OFD.FileName}
                GamesListView.Items.Add(GameItem)

                If Not File.Exists(My.Computer.FileSystem.CurrentDirectory + "\games.list") Then
                    File.Create(My.Computer.FileSystem.CurrentDirectory + "\games.list")
                End If

                'Add new line to games.list
                Using GamesFileWriter As New StreamWriter(My.Computer.FileSystem.CurrentDirectory + "\games.list", True)
                    GamesFileWriter.WriteLine("Unknown;Unknown;Unknown;" + OFD.FileName)
                    GamesFileWriter.Close()
                End Using
            Else
                GameID = GameID.Replace(".", "").Replace("_", "-").Trim()
                AddToList = True
                Try
                    PSXDatacenterBrowser.Navigate("https://psxdatacenter.com/psx2/games2/" + GameID + ".html")
                Catch ex As Exception
                    MsgBox("Could not load game informations from PSXDatacenter.", MsgBoxStyle.Exclamation, "No information found for this Game ID")
                End Try
            End If

        End If
    End Sub

    Public Function GetGameID(GameISO As String) As String

        Dim GameID As String = ""

        Using SevenZip As New Process()
            SevenZip.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\7z.exe"
            SevenZip.StartInfo.Arguments = "l -ba """ + GameISO + """"
            SevenZip.StartInfo.RedirectStandardOutput = True
            SevenZip.StartInfo.UseShellExecute = False
            SevenZip.StartInfo.CreateNoWindow = True
            SevenZip.Start()

            'Read the output
            Dim OutputReader As StreamReader = SevenZip.StandardOutput
            Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.None)

            For Each Line As String In ProcessOutput
                If Line.Contains("SLES_") Or Line.Contains("SLUS_") Or Line.Contains("SCES_") Or Line.Contains("SCUS_") Then
                    If Line.Contains("Volume:") Then 'ID found in the ISO Header
                        GameID = Line.Split(New String() {"Volume: "}, StringSplitOptions.RemoveEmptyEntries)(1)
                        Exit For
                    Else 'ID found in the ISO files
                        GameID = String.Join(" ", Line.Split(New Char() {}, StringSplitOptions.RemoveEmptyEntries)).Split(" "c)(5).Trim()
                        Exit For
                    End If
                End If
            Next
        End Using

        If GameID = "" Then
            Return "ID not found"
        Else
            Return GameID
        End If

    End Function

    Private Sub AddToGameList(Title As String, ID As String, Region As String)

        'Add to the GamesListView
        Dim GameItem As New GameListViewItem() With {.GameTitle = Title, .GameID = ID, .GameRegion = Region, .GameFilePath = SelectedISO}
        GamesListView.Items.Add(GameItem)

        'Add new line to games.list
        Using GamesFileWriter As New StreamWriter(My.Computer.FileSystem.CurrentDirectory + "\games.list", True)
            GamesFileWriter.WriteLine(Title + ";" + ID + ";" + Region + ";" + SelectedISO)
            GamesFileWriter.Close()
        End Using

    End Sub

    Private Sub RemoveSelectedGameButton_Click(sender As Object, e As RoutedEventArgs) Handles RemoveSelectedGameButton.Click

        If GamesListView.SelectedItem IsNot Nothing Then
            Dim SelectedGame As GameListViewItem = CType(GamesListView.SelectedItem, GameListViewItem)

            If MsgBox("Do you really want to remove " + SelectedGame.GameTitle + " from the games library ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

                'Get the line of the selected game
                Dim LineOfGame As Integer
                Dim GameslistLines = File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\homebrew.list")
                For i = 0 To GameslistLines.Length - 1
                    If GameslistLines(i).StartsWith(SelectedGame.GameTitle) Then
                        LineOfGame = i
                    End If
                Next

                'Remove it from the games.list
                Dim GamesLinesAsList As List(Of String) = File.ReadAllLines(My.Computer.FileSystem.CurrentDirectory + "\games.list").ToList
                GamesLinesAsList.RemoveAt(LineOfGame)

                'Write the new games.list
                File.WriteAllLines(My.Computer.FileSystem.CurrentDirectory + "\games.list", GamesLinesAsList)

                GamesListView.Items.Remove(GamesListView.SelectedItem)
            End If

        End If

    End Sub

    Private Sub SendGameButton_Click(sender As Object, e As RoutedEventArgs) Handles SendGameButton.Click

        If GamesListView.SelectedItem IsNot Nothing And Not String.IsNullOrWhiteSpace(ConsoleIP) Then

            Dim SelectedGame As GameListViewItem = CType(GamesListView.SelectedItem, GameListViewItem)

            If MsgBox("Send " + SelectedGame.GameFilePath + " to the console ?", MsgBoxStyle.YesNo, "Confirm") = MsgBoxResult.Yes Then
                'Show progress bar
                GameDescriptionTextBlock.Height = 230
                SendStatusTextBlock.Visibility = Visibility.Visible
                SendProgressBar.Visibility = Visibility.Visible
                SendGameButton.IsEnabled = False

                Dim DeviceIP As IPAddress = IPAddress.Parse(ConsoleIP)
                Dim GameFileInfo As New FileInfo(SelectedGame.GameFilePath)

                'Set the progress bar maximum and TotalBytes to send
                SendProgressBar.Value = 0
                SendProgressBar.Maximum = CDbl(GameFileInfo.Length)
                TotalBytes = GameFileInfo.Length

                'Start sending
                Dim WorkArgs As New MainWindow.WorkerArgs() With {.DeviceIP = DeviceIP, .FileToSend = SelectedGame.GameFilePath}
                If UseModELF = True Then WorkArgs.ChunkSize = 63488 Else WorkArgs.ChunkSize = 4096
                SenderWorker.RunWorkerAsync(WorkArgs)
            End If

        End If

    End Sub

    Private Sub GamesListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GamesListView.SelectionChanged

        If GamesListView.SelectedItem IsNot Nothing Then
            Dim GameID As String = CType(GamesListView.SelectedItem, GameListViewItem).GameID
            Try
                PSXDatacenterBrowser.Navigate("https://psxdatacenter.com/psx2/games2/" + GameID + ".html")
            Catch ex As Exception
                MsgBox("Could not load game informations from PSXDatacenter.", MsgBoxStyle.Exclamation, "No information found for this Game ID")
            End Try
        End If

    End Sub

    Private Sub SenderWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles SenderWorker.DoWork

        Dim CurrentWorkerArgs As MainWindow.WorkerArgs = CType(e.Argument, MainWindow.WorkerArgs)

        Dim FileInfos As New FileInfo(CurrentWorkerArgs.FileToSend)
        Dim FileSizeAsLong As Long = FileInfos.Length
        Dim FileSizeAsULong As ULong = CULng(FileInfos.Length)

        Dim MagicBytes = BytesConverter.ToLittleEndian(Magic)
        Dim NewFileSizeBytes = BytesConverter.ToLittleEndian(FileSizeAsULong)
        Dim ChunkSize As Integer = CurrentWorkerArgs.ChunkSize

        Using SenderSocket As New Socket(SocketType.Stream, ProtocolType.Tcp) With {.ReceiveTimeout = 3000}

            'Connect to the console
            SenderSocket.Connect(CurrentWorkerArgs.DeviceIP, 9045)

            'Send the magic
            SenderSocket.Send(MagicBytes)
            'Send the file size
            SenderSocket.Send(NewFileSizeBytes)

            Dim BytesRead As Integer
            Dim SendBytes As Long
            Dim Buffer(ChunkSize - 1) As Byte

            'Open the file and send
            Using SenderFileStream As New FileStream(CurrentWorkerArgs.FileToSend, FileMode.Open, FileAccess.Read)

                Do
                    BytesRead = SenderFileStream.Read(Buffer, 0, Buffer.Length)

                    If BytesRead > 0 Then
                        'Send bytes
                        SendBytes += SenderSocket.Send(Buffer, 0, BytesRead, SocketFlags.None)

                        'Update the status text
                        If SendStatusTextBlock.Dispatcher.CheckAccess() = False Then
                            SendStatusTextBlock.Dispatcher.BeginInvoke(Sub() SendStatusTextBlock.Text = "Sending file: " + GetReadableSizeString(SendBytes) + " of " + GetReadableSizeString(TotalBytes) + " sent.")
                        Else
                            SendStatusTextBlock.Text = "Sending file: " + GetReadableSizeString(SendBytes) + " of " + GetReadableSizeString(TotalBytes) + " sent."
                        End If

                        'Update the status progress bar
                        If SendProgressBar.Dispatcher.CheckAccess() = False Then
                            SendProgressBar.Dispatcher.BeginInvoke(Sub() SendProgressBar.Value += ChunkSize)
                        Else
                            SendProgressBar.Value += ChunkSize
                        End If

                    End If
                Loop While BytesRead > 0

            End Using

            SenderSocket.Close()

        End Using

    End Sub

    Private Sub SenderWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles SenderWorker.RunWorkerCompleted

        'Hide progress
        If GameDescriptionTextBlock.Dispatcher.CheckAccess() = False Then
            GameDescriptionTextBlock.Dispatcher.BeginInvoke(Sub() GameDescriptionTextBlock.Height = 310)
        Else
            GameDescriptionTextBlock.Height = 310
        End If
        If SendStatusTextBlock.Dispatcher.CheckAccess() = False Then
            SendStatusTextBlock.Dispatcher.BeginInvoke(Sub() SendStatusTextBlock.Visibility = Visibility.Hidden)
        Else
            SendStatusTextBlock.Visibility = Visibility.Hidden
        End If
        If SendProgressBar.Dispatcher.CheckAccess() = False Then
            SendProgressBar.Dispatcher.BeginInvoke(Sub() SendProgressBar.Visibility = Visibility.Hidden)
        Else
            SendProgressBar.Visibility = Visibility.Hidden
        End If
        If SendGameButton.Dispatcher.CheckAccess() = False Then
            SendGameButton.Dispatcher.BeginInvoke(Sub() SendGameButton.IsEnabled = True)
        Else
            SendGameButton.IsEnabled = True
        End If

        If Not e.Cancelled Then
            MsgBox("Game successfully sent!" + vbCrLf + "Please report the game compatibility.", MsgBoxStyle.Information, "Success")
        End If

    End Sub

    Public Function GetReadableSizeString(Value As Long) As String
        Dim DoubleBytes As Double
        DoubleBytes = CDbl(Value / 1048576)
        Return FormatNumber(DoubleBytes, 2) & " MB"
    End Function

End Class
