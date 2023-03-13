Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Forms
Imports DiscUtils
Imports DiscUtils.Iso9660

Public Class RetroRepacker

    Dim ExtractedISOFolder As String

    Dim CoreInISO As String
    Dim CoreFileName As String
    Dim InfoFileName As String

    Public Sub OpenISOFile(ISOFile As String)
        Using isoStream As FileStream = File.OpenRead(ISOFile)

            Dim cd As New CDReader(isoStream, True)

            'Check which emulator ISO has been selected so we can filter file extensions when adding roms
            Dim CoresInISO As String() = cd.GetFiles("CORES\") 'Gets actually ALL cores, but in this case there's only 1 in the ISO file

            If CoresInISO(0) IsNot Nothing Then

                If CoresInISO(0).Contains("PICODRIVE_LIBRETRO_PS2.ELF") Then
                    CoreInISO = "PicoDrive"
                    CoreFileName = "PICODRIVE_LIBRETRO_PS2.ELF"
                    InfoFileName = "PICODRIVE_LIBRETRO.INFO"
                ElseIf CoresInISO(0).Contains("QUICKNES_LIBRETRO_PS2.ELF") Then
                    CoreInISO = "QuickNES"
                    CoreFileName = "QUICKNES_LIBRETRO_PS2.ELF"
                    InfoFileName = "QUICKNES_LIBRETRO.INFO"
                ElseIf CoresInISO(0).Contains("ATARI800_LIBRETRO_PS2.ELF") Then
                    CoreInISO = "Atari800"
                    CoreFileName = "ATARI800_LIBRETRO_PS2.ELF"
                    InfoFileName = "ATARI800_LIBRETRO.INFO"
                ElseIf CoresInISO(0).Contains("PRBOOM_LIBRETRO_PS2.ELF") Then
                    CoreInISO = "Doom"
                    CoreFileName = "PRBOOM_LIBRETRO_PS2.ELF"
                    InfoFileName = "PRBOOM_LIBRETRO.INFO"
                ElseIf CoresInISO(0).Contains("GAMBATTE_LIBRETRO_PS2.ELF") Then
                    CoreInISO = "Gambatte"
                    CoreFileName = "GAMBATTE_LIBRETRO_PS2.ELF"
                    InfoFileName = "GAMBATTE_LIBRETRO.INFO"
                ElseIf CoresInISO(0).Contains("HANDY_LIBRETRO_PS2.ELF") Then
                    CoreInISO = "Lynx"
                    CoreFileName = "HANDY_LIBRETRO_PS2.ELF"
                    InfoFileName = "HANDY_LIBRETRO.INFO"
                ElseIf CoresInISO(0).Contains("MEDNAFEN_WSWAN_LIBRETRO_PS.ELF") Then
                    CoreInISO = "MednafenWsan"
                    CoreFileName = "MEDNAFEN_WSWAN_LIBRETRO_PS.ELF"
                    InfoFileName = "MEDNAFEN_WSWAN_LIBRETRO.INFO"
                ElseIf CoresInISO(0).Contains("RACE_LIBRETRO_PS2.ELF") Then
                    CoreInISO = "NeoGeoPocket"
                    CoreFileName = "RACE_LIBRETRO_PS2.ELF"
                    InfoFileName = "RACE_LIBRETRO.INFO"
                ElseIf CoresInISO(0).Contains("SMSPLUS_LIBRETRO_PS2.ELF") Then
                    CoreInISO = "SMSPlus"
                    CoreFileName = "SMSPLUS_LIBRETRO_PS2.ELF"
                    InfoFileName = "SMSPLUS_LIBRETRO.INFO"
                ElseIf CoresInISO(0).Contains("SNES9X2002_LIBRETRO_PS2.ELF") Then
                    CoreInISO = "Snes9x"
                    CoreFileName = "SNES9X2002_LIBRETRO_PS2.ELF"
                    InfoFileName = "SNES9X2002_LIBRETRO.INFO"
                Else
                    CoreInISO = "Unknown"
                End If

            Else
                CoreInISO = "Not found"
            End If

            'Load games already that are already in the ISO file (we could do that also after extraction and reading files in .\RETROARCH\DOWNLOADS\)
            For Each Game In cd.GetFiles("RETROARCH\DOWNLOADS\")
                Dim GameRomInfo As DiscFileInfo = cd.GetFileInfo(Game)
                Dim RomFileSize As String = FormatNumber(GameRomInfo.Length / 1024, 2) + " KB" 'Return as KB
                Dim NewRomListViewItem As New RomListViewItem() With {.RomFileName = GameRomInfo.Name.Replace(";1", ""), .RomFileSize = RomFileSize, .RomFilePath = GameRomInfo.FullName.Replace(";1", "")}

                AvailableRomsListView.Items.Add(NewRomListViewItem)
            Next

        End Using
    End Sub

    Public Sub ExtractISOFile(ISOFile As String)

        'The name will be used as output folder name
        Dim FileNameOfISO As String = Path.GetFileNameWithoutExtension(ISOFile)
        ExtractedISOFolder = FileNameOfISO

        'Extract the file using 7z
        Using SevenZip As New Process()
            SevenZip.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\7z.exe"
            SevenZip.StartInfo.Arguments = "x -y """ + ISOFile + """" + " -o""" + My.Computer.FileSystem.CurrentDirectory + "\Temp\" + FileNameOfISO + """"
            SevenZip.StartInfo.UseShellExecute = False
            SevenZip.StartInfo.CreateNoWindow = True
            SevenZip.Start()
        End Using

    End Sub

    Public Sub RecreateISOFile(ISOOutputPath As String)

        'First create the XML template
        If MakeNewXML(My.Computer.FileSystem.CurrentDirectory + "\Temp\" + ExtractedISOFolder, ISOOutputPath) = 1 Then

            Dim ProcessOutput As String = ""
            Using MakeISOProcess As New Process()
                MakeISOProcess.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\mkpsxiso.exe"
                MakeISOProcess.StartInfo.Arguments = "-y """ + My.Computer.FileSystem.CurrentDirectory + "\Tools\ISO.xml"""
                MakeISOProcess.StartInfo.RedirectStandardOutput = True
                MakeISOProcess.StartInfo.UseShellExecute = False
                MakeISOProcess.StartInfo.CreateNoWindow = True

                MakeISOProcess.Start()

                Dim ShellReader As StreamReader = MakeISOProcess.StandardOutput
                ProcessOutput = ShellReader.ReadToEnd()

                ShellReader.Close()
            End Using

            If ProcessOutput.Contains("ISO image generated successfully.") Then
                If MsgBox("ISO successfully written at " + ISOOutputPath + vbCrLf + "Add to the game library ?", MsgBoxStyle.YesNo, "ISO re-created") = MsgBoxResult.Yes Then

                    'Find the PS2 Backup Manager (if open) and add to the GamesListView
                    Dim GameItem As New GameListViewItem() With {.GameTitle = CoreInISO, .GameID = "SLUS-20090", .GameRegion = "International", .GameFilePath = ISOOutputPath}
                    For Each Win In Windows.Application.Current.Windows()
                        If Win.ToString = "OkageLibrary.PS2BackupManager" Then
                            CType(Win, PS2BackupManager).GamesListView.Items.Add(GameItem)
                            Exit For
                        End If
                    Next

                    If Not File.Exists(My.Computer.FileSystem.CurrentDirectory + "\games.list") Then
                        File.Create(My.Computer.FileSystem.CurrentDirectory + "\games.list")
                    End If

                    'Add new line to games.list
                    Using GamesFileWriter As New StreamWriter(My.Computer.FileSystem.CurrentDirectory + "\games.list", True)
                        GamesFileWriter.WriteLine(CoreInISO + ";SLUS-20090;International;" + ISOOutputPath)
                        GamesFileWriter.Close()
                    End Using

                    'Clear the RomsListView
                    AvailableRomsListView.Items.Clear()

                End If
            Else
                MsgBox("Could not write the ISO file.", MsgBoxStyle.Exclamation, "Error creating ISO file")
            End If

        End If

    End Sub

    Private Sub BrowseISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISOButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Choose your emulator .iso file", .Multiselect = False, .Filter = "ISO files (*.iso)|*.iso"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOTextBox.Text = OFD.FileName

            'Clear previous items
            AvailableRomsListView.Items.Clear()

            'Open the ISO and get a list of already installed roms
            OpenISOFile(OFD.FileName)
            'Extract the ISO temporarly so we can add files and re-pack the ISO
            ExtractISOFile(OFD.FileName)
        End If

    End Sub

    Private Sub AddNewRomButton_Click(sender As Object, e As RoutedEventArgs) Handles AddNewRomButton.Click

        Dim OFD As New OpenFileDialog() With {.Title = "Select your game roms", .Multiselect = True}

        'Apply a filter if the core is known - CoreInISO was set in OpenISOFile()
        Select Case CoreInISO
            Case "PicoDrive"
                OFD.Filter = "BIN files (*.bin)|*.bin|GEN files (*.gen)|*.gen|SMD files (*.smd)|*.smd|MD files (*.md)|*.md|32X files (*.32x)|*.32x|CUE files (*.cue)|*.cue|ISO files (*.iso)|*.iso|SMS files (*.sms)|*.sms|68K files (*.68k)|*.68k|CHD files (*.chd)|*.chd"
            Case "QuickNES"
                OFD.Filter = "NES files (*.nes)|*.nes"
            Case "Atari800"
                OFD.Filter = "XFD files (*.xfd)|*.xfd|ATR files (*.atr)|*.atr|ATX files (*.atx)|*.atx|CDM files (*.cdm)|*.cdm|CAS files (*.cas)|*.cas|BIN files (*.bin)|*.bin|A52 files (*.a52)|*.a52|XEX files (*.xex)|*.xex|ZIP files (*.zip)|*.zip"
            Case "Doom"
                OFD.Filter = "WAD files (*.wad)|*.wad"
            Case "Gambatte"
                OFD.Filter = "GB files (*.gb)|*.gb|GBC files (*.gbc)|*.gbc|DMG files (*.dmg)|*.dmg"
            Case "Lynx"
                OFD.Filter = "LNX files (*.lnx)|*.lnx"
            Case "MednafenWsan"
                OFD.Filter = "WS files (*.ws)|*.ws|WSC files (*.wsc)|*.wsc|PC2 files (*.pc2)|*.pc2"
            Case "NeoGeoPocket"
                OFD.Filter = "NGP files (*.ngp)|*.ngp|NGC files (*.ngc)|*.ngc"
            Case "SMSPlus"
                OFD.Filter = "SMS files (*.sms)|*.sms|BIN files (*.bin)|*.bin|ROM files (*.rom)|*.rom|GG files (*.gg)|*.gg|COL files (*.col)|*.col"
            Case "Snes9x"
                OFD.Filter = "SMC files (*.smc)|*.smc|SFC files (*.sfc)|*.sfc|SWC files (*.swc)|*.swc|FIG files (*.fig)|*.fig|BS files (*.bs)|*.bs|ST files (*.st)|*.st"
        End Select

        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            For Each SelectedRom In OFD.FileNames
                Dim RomFileSize As String = (New FileInfo(SelectedRom).Length / 1024).ToString + " KB" 'Return as KB
                Dim NewRomListViewItem As New RomListViewItem() With {.RomFileName = Path.GetFileNameWithoutExtension(SelectedRom), .RomFileSize = RomFileSize, .RomFilePath = SelectedRom}

                'Add the rom to the RomsListView
                AvailableRomsListView.Items.Add(NewRomListViewItem)

                'Copy the rom to the temporarly extracted ISO folder
                Dim RomFileName As String = Path.GetFileName(SelectedRom)
                File.Copy(SelectedRom, My.Computer.FileSystem.CurrentDirectory + "\Temp\" + ExtractedISOFolder + "\RETROARCH\DOWNLOADS\" + RomFileName, True)
            Next

        End If

    End Sub

    Private Sub SelectOutputFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles SelectOutputFolderButton.Click

        Dim FBD As New FolderBrowserDialog() With {.Description = "Select the output folder", .ShowNewFolderButton = True}

        'Set output folder
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            OutputFolderTextBox.Text = FBD.SelectedPath
        End If

    End Sub

    Private Sub CreateISOButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateISOButton.Click

        'Check if output folder is not empty
        If Not String.IsNullOrWhiteSpace(OutputFolderTextBox.Text) Then

            If Not String.IsNullOrWhiteSpace(SelectedISOTextBox.Text) Then
                'Get the filename from the selected ISO
                Dim NewISOFileName As String = Path.GetFileName(SelectedISOTextBox.Text)
                RecreateISOFile(OutputFolderTextBox.Text + "\" + NewISOFileName)
            Else
                'Set the filename according to the previously extracted iso folder name
                Dim NewISOFileName As String = ExtractedISOFolder + ".iso"
                RecreateISOFile(OutputFolderTextBox.Text + "\" + NewISOFileName)
            End If

        Else
            MsgBox("Please specify an output folder.", MsgBoxStyle.Exclamation, "Could not create ISO")
        End If

    End Sub

    Private Sub RemoveSelectedRomButton_Click(sender As Object, e As RoutedEventArgs) Handles RemoveSelectedRomButton.Click

        'Check if a rom is selected
        If AvailableRomsListView.SelectedItem IsNot Nothing Then
            Dim SelectedRom As RomListViewItem = CType(AvailableRomsListView.SelectedItem, RomListViewItem)

            'Check if the rom still exists in the temporarly extracted iso
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Temp\" + ExtractedISOFolder + "\RETROARCH\DOWNLOADS\" + SelectedRom.RomFileName) Then
                If MsgBox("Do you really want to delete this file from the ISO ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then
                    'Delete the file from the temporarly extracted iso
                    File.Delete(My.Computer.FileSystem.CurrentDirectory + "\Temp\" + ExtractedISOFolder + "\RETROARCH\DOWNLOADS\" + SelectedRom.RomFileName)
                    'Remove the rom from the list
                    AvailableRomsListView.Items.Remove(AvailableRomsListView.SelectedItem)
                    AvailableRomsListView.Items.Refresh()
                End If
            Else
                If MsgBox("This rom seems to be already deleted. Remove it from the list ?", MsgBoxStyle.YesNo, "Rom not found") = MsgBoxResult.Yes Then
                    'Remove the rom from the list
                    AvailableRomsListView.Items.Remove(AvailableRomsListView.SelectedItem)
                    AvailableRomsListView.Items.Refresh()
                End If
            End If
        Else
            MsgBox("Please select a rom from the list.", MsgBoxStyle.Exclamation, "Could delete the selected rom file")
        End If

    End Sub

    Private Sub RetroRepacker_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing

        'Currently only deletes the current ISO - will be changed
        If Directory.Exists(My.Computer.FileSystem.CurrentDirectory + "\Temp\" + ExtractedISOFolder) Then
            Directory.Delete(My.Computer.FileSystem.CurrentDirectory + "\Temp\" + ExtractedISOFolder, True)
        End If

    End Sub

    Private Function MakeNewXML(ExtractedISOPath As String, ISOOutput As String) As Integer
        Try

            'Remove the previous XML
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ISO.xml") Then
                File.Delete(My.Computer.FileSystem.CurrentDirectory + "\Tools\ISO.xml")
            End If

            Dim LicensePath As String = My.Computer.FileSystem.CurrentDirectory + "\Tools\LIC\LICENSEA.DAT"
            Dim RetroArchCFGPath As String = ExtractedISOPath + "\RETROARCH.CFG"
            Dim GameIDFilePath As String = ExtractedISOPath + "\SLUS_200.90"
            Dim SYSTEMCNFPath As String = ExtractedISOPath + "\SYSTEM.CNF"
            Dim CoreFolderPath As String = ExtractedISOPath + "\CORES\"
            Dim InfoFolderPath As String = ExtractedISOPath + "\INFO\"
            Dim RetroArchFolderPath As String = ExtractedISOPath + "\RETROARCH\"

            'Create the ISO.xml
            Dim ISOXMLDocument As XDocument = <?xml version="1.0" encoding="UTF-8"?>
                                              <iso_project image_name=<%= ISOOutput %> no_xa="1">
                                                  <track type="data">
                                                      <identifiers
                                                          system="PLAYSTATION"
                                                          application="PLAYSTATION"
                                                          volume=<%= CoreInISO %>
                                                          volume_set=<%= CoreInISO %>
                                                          publisher="SvenGDK"
                                                          data_preparer="MKPSXISO"
                                                          copyright="RETROARCH"
                                                      />
                                                      <license file=<%= LicensePath %>/>
                                                      <directory_tree>

                                                          <file name="SLUS_200.90" type="data" source=<%= GameIDFilePath %>/>
                                                          <file name="SYSTEM.CNF" type="data" source=<%= SYSTEMCNFPath %>/>

                                                          <dir name="CORES" source=<%= CoreFolderPath %>>
                                                              <file name=<%= CoreFileName %> type="data" source=<%= CoreFolderPath + CoreFileName %>/>
                                                          </dir>

                                                          <dir name="INFO" source=<%= InfoFolderPath %>>
                                                              <file name=<%= InfoFileName %> type="data" source=<%= InfoFolderPath + InfoFileName %>/>
                                                          </dir>

                                                          <dir name="RETROARCH" source=<%= RetroArchFolderPath %>>
                                                              <dir name="DOWNLOADS" source=<%= RetroArchFolderPath + "DOWNLOADS\" %>>

                                                              </dir>
                                                          </dir>
                                                      </directory_tree>
                                                  </track>
                                              </iso_project>

            'Add RETROARCH.CFG if it existed on the ISO
            If File.Exists(RetroArchCFGPath) Then
                ISOXMLDocument.Root.Element("track").Element("directory_tree").Add(<file name="RETROARCH.CFG" type="data" source=<%= RetroArchCFGPath %>/>)
            End If

            'Add roms to the XML
            For Each GameRom In Directory.GetFiles(ExtractedISOPath + "\RETROARCH\DOWNLOADS\")
                Dim GameFileName As String = Path.GetFileName(GameRom)
                ISOXMLDocument.Root.Element("track").Element("directory_tree").Elements("dir")(2).Element("dir").Add(<file name=<%= GameFileName %> type="data" source=<%= GameRom %>/>)
            Next

            'Save the XML file
            ISOXMLDocument.Save(My.Computer.FileSystem.CurrentDirectory + "\Tools\ISO.xml")

            Return 1
        Catch ex As Exception
            Return 0
        End Try
    End Function

End Class
