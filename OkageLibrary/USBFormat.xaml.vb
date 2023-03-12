Imports System.IO
Imports System.Management

Public Class USBFormat

    Dim SelectedDrive As DriveInfo
    Dim SelectedDriveLetter As String
    Dim SelectedDriveNumber As Integer

    Private Sub StartButton_Click(sender As Object, e As RoutedEventArgs) Handles StartButton.Click

        If MsgBox("Do you really want to format the selected USB drive ?" + vbCrLf + "Letter: " + SelectedDriveLetter + vbCrLf + "DiskPart Number: " + SelectedDriveNumber.ToString, MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

            'Lock UI temporarly
            LockOrUnlockUI()

            'Convert to MBR if selected
            If ConvertToMBRCheckBox.IsChecked Then

                'Check if a convert is necessary
                If IsGPT(SelectedDriveLetter.Remove(2)) Then

                    'Temporarly copy the data to C:\USBTemp so we can restore them after formating
                    If KeepDataCheckBox.IsChecked Then

                        'Create a new CopyWindow dialog
                        Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True, .WindowStartupLocation = WindowStartupLocation.CenterScreen, .CopyFrom = SelectedDriveLetter}
                        If NewCopyWindow.ShowDialog() = True Then
                            'Files are backed up, now convert and format the drive
                            ConvertAndFormat()
                        End If

                    Else
                        ConvertAndFormat()
                    End If

                Else
                    'Only inform that the drive already uses MBR if ConvertToMBRCheckBox.IsChecked
                    If MsgBox("The selected drive already uses MBR, continue formating ?", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

                        'Temporarly copy the data to C:\USBTemp so we can restore them after formating
                        If KeepDataCheckBox.IsChecked Then

                            'Create a new CopyWindow dialog
                            Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True, .WindowStartupLocation = WindowStartupLocation.CenterScreen, .CopyFrom = SelectedDriveLetter}
                            If NewCopyWindow.ShowDialog() = True Then
                                'Files are backed up, now format the drive
                                FormatDrive()
                            End If

                        Else
                            'Format completely
                            FormatDrive()
                        End If

                    Else
                        Exit Sub
                    End If

                End If

            End If

        End If

    End Sub

    Private Sub USBFormat_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        'List removable drives in 'Ready' state
        For Each Drive As DriveInfo In DriveInfo.GetDrives()
            If Drive.DriveType = DriveType.Removable And Drive.IsReady Then
                DriveListComboBox.Items.Add(Drive.Name + vbTab + Drive.VolumeLabel)
            End If
        Next

    End Sub

    Private Sub DriveListComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles DriveListComboBox.SelectionChanged
        If DriveListComboBox.SelectedItem IsNot Nothing Then
            Try
                Dim SelectedDriveItem As String = e.AddedItems(0).ToString
                'Set the selected drive
                SelectedDrive = New DriveInfo(SelectedDriveItem.Split(CChar(vbTab))(0))
                SelectedDriveLetter = New DriveInfo(SelectedDriveItem.Split(CChar(vbTab))(0)).Name
                'Set the disk number of the selected drive
                SelectedDriveNumber = CInt(GetPhysicalDriveId(SelectedDrive.Name.Replace("\", ""), True))
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try
        End If
    End Sub

    Public Sub FormatDrive()
        Dim FormatStartInfo As New ProcessStartInfo With {
            .FileName = "format.com",
            .Arguments = "/fs:exFAT /v:OkageUSB /q " + SelectedDriveLetter.Remove(2),
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .RedirectStandardOutput = True,
            .RedirectStandardInput = True
        }

        'Start format without user interaction
        Dim FormatProcess As Process = Process.Start(FormatStartInfo)
        Dim ProcessInputStream As StreamWriter = FormatProcess.StandardInput
        ProcessInputStream.Write(vbCr & vbLf)

        FormatProcess.WaitForExit()

        If KeepDataCheckBox.IsChecked Then
            'Show info about restoring process
            MsgBox("USB drive formatted with success, files will now be restored, please wait.", MsgBoxStyle.Information, "Format completed, now restoring")

            'Create a new CopyWindow dialog and set CopyFrom to "C:\USBTemp\", .CopyTo = USB & Restore to "True"
            Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True, .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                .CopyFrom = "C:\USBTemp\", .CopyTo = SelectedDriveLetter, .Restore = True, .Title = "Restoring data to USB"}

            If NewCopyWindow.ShowDialog() = True Then
                MsgBox("USB drive formatted and restored with success !", MsgBoxStyle.Information, "Completed")
            End If

        Else
            MsgBox("USB drive formatted with success !", MsgBoxStyle.Information, "Completed")
        End If

        'Unlock UI
        LockOrUnlockUI()

    End Sub

    Private Sub ConvertAndFormat()

        'Write the diskpart query
        Using CommandFileWriter As New StreamWriter(My.Computer.FileSystem.CurrentDirectory + "\Tools\convert.txt", False)
            CommandFileWriter.WriteLine("select disk " + SelectedDriveNumber.ToString)
            CommandFileWriter.WriteLine("clean")
            CommandFileWriter.WriteLine("convert mbr")
            CommandFileWriter.WriteLine("create partition primary")
            CommandFileWriter.WriteLine("format fs=exFAT label=""OkageUSB"" quick")
            CommandFileWriter.WriteLine("assign letter=" + SelectedDriveLetter.Remove(2))
        End Using

        'Start the process
        Dim ProcessOutput As String = ""
        Using DiskPartProcess As New Process()
            DiskPartProcess.StartInfo.FileName = "diskpart"
            DiskPartProcess.StartInfo.Arguments = "/s """ + My.Computer.FileSystem.CurrentDirectory + "\Tools\convert.txt"""
            DiskPartProcess.StartInfo.RedirectStandardOutput = True
            DiskPartProcess.StartInfo.UseShellExecute = False
            DiskPartProcess.StartInfo.CreateNoWindow = True

            DiskPartProcess.Start()

            Dim ShellReader As StreamReader = DiskPartProcess.StandardOutput
            ProcessOutput = ShellReader.ReadToEnd()

            ShellReader.Close()
        End Using

        'Restore if checkbox was selected
        If KeepDataCheckBox.IsChecked Then
            'Show info about restoring process (this delays also a bit)
            Activate() 'Sometimes the window can fall back and the MsgBox dialog doesn't appear, re-activate just in case
            MsgBox("USB drive converted and formatted with success, files will now be restored, please wait.", MsgBoxStyle.Information, "Convert & Format completed, now restoring")

            'Create a new CopyWindow dialog and set CopyFrom to "C:\USBTemp\", .CopyTo = USB & Restore to "True"
            Dim NewCopyWindow As New CopyWindow() With {.ShowActivated = True, .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    .CopyFrom = "C:\USBTemp\", .CopyTo = SelectedDriveLetter, .Restore = True, .Title = "Restoring data to USB"}

            If NewCopyWindow.ShowDialog() = True Then
                MsgBox("USB drive converted, formatted and restored with success !", MsgBoxStyle.Information, "Completed")
            End If

        Else
            MsgBox("USB drive converted and formatted with success !", MsgBoxStyle.Information, "Completed")
        End If

        'Unlock UI
        LockOrUnlockUI()

    End Sub

    Private Function IsGPT(Drive As String) As Boolean
        Dim PartitionStyle As String = ""
        Dim DriveLetter As String = ""
        Dim PhysicalDrive As String = ""

        Using devs As New ManagementClass("Win32_Diskdrive")
            Dim moc As ManagementObjectCollection = devs.GetInstances()
            For Each mo As ManagementObject In moc
                PhysicalDrive = CStr(mo("DeviceId"))
                For Each b As ManagementObject In mo.GetRelated("Win32_DiskPartition")
                    PartitionStyle = CStr(b("Type"))
                    For Each c As ManagementBaseObject In b.GetRelated("Win32_LogicalDisk")
                        DriveLetter = CStr(c("Name"))
                        If (DriveLetter = Drive) And PartitionStyle.Contains("GPT") Then Return True
                    Next
                Next
            Next
        End Using

        Return False
    End Function

    Private Function GetPhysicalDriveId(drvName As String, Optional ReturnIndex As Boolean = False) As String
        Dim DeviceID As String = ""
        Using LogicalDiskQueryResults As New ManagementObjectSearcher("ASSOCIATORS OF {Win32_LogicalDisk.DeviceID='" & (drvName & "'} WHERE AssocClass = Win32_LogicalDiskToPartition"))
            For Each mo As ManagementObject In LogicalDiskQueryResults.Get
                Using DiskPartitionQueryResults As New ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" & (mo("DeviceID").ToString & "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition"))
                    For Each partition As ManagementObject In DiskPartitionQueryResults.Get
                        DeviceID = partition(If(ReturnIndex, "Index", "DeviceID")).ToString
                    Next
                End Using
            Next
        End Using
        Return DeviceID
    End Function

    Private Sub LockOrUnlockUI()

        If DriveListComboBox.IsEnabled Then
            DriveListComboBox.IsEnabled = False
            ConvertToMBRCheckBox.IsEnabled = False
            KeepDataCheckBox.IsEnabled = False
            StartButton.IsEnabled = False
        Else
            DriveListComboBox.IsEnabled = True
            ConvertToMBRCheckBox.IsEnabled = True
            KeepDataCheckBox.IsEnabled = True
            StartButton.IsEnabled = True
        End If

    End Sub

End Class
