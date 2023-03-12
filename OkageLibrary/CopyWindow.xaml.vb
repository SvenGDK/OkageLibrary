Imports System.ComponentModel
Imports System.IO

Public Class CopyWindow

    Dim WithEvents CopyWorker As New BackgroundWorker() With {.WorkerReportsProgress = True}

    Public CopyFrom As String
    Public CopyTo As String = "C:\USBTemp\"

    Public FilesCopyCount As Integer
    Public DirsCopyCount As Integer
    Public FilesCopiedCount As Integer
    Public DirsCopiedCount As Integer

    Public Restore As Boolean = False

    Private Sub CopyWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        'Create temporary folder
        If Not Directory.Exists("C:\USBTemp") Then
            Directory.CreateDirectory("C:\USBTemp")
        End If

        'Get files count
        For Each Files In Directory.GetFiles(CopyFrom)
            FilesCopyCount += 1
        Next

        'Get folders count
        For Each Directories In Directory.GetDirectories(CopyFrom)
            DirsCopyCount += 1
        Next

        If Restore = False Then
            'Set status
            CopyProgressBar.Maximum = FilesCopyCount + DirsCopyCount
            StatusTextBlock.Text = "Backing up 0/" + FilesCopyCount.ToString + " files and 0/" + DirsCopyCount.ToString + " directories from the USB."

            'Backup everything from the USB drive
            CopyWorker.RunWorkerAsync()
        Else
            'Set status
            CopyProgressBar.Maximum = FilesCopyCount + DirsCopyCount
            StatusTextBlock.Text = "Restoring 0/" + FilesCopyCount.ToString + " files and 0/" + DirsCopyCount.ToString + " directories on the USB."

            'Restore everything to the USB drive
            CopyWorker.RunWorkerAsync()
        End If


    End Sub

    Private Sub CopyWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles CopyWorker.DoWork

        For Each Files In Directory.GetFiles(CopyFrom)
            Dim FileName As String = Path.GetFileName(Files)

            'Copy the files on \ to the temp folder
            File.Copy(Files, CopyTo + FileName, True)

            'Report progress
            FilesCopiedCount += 1
            CopyWorker.ReportProgress(1)
        Next

        For Each Directories In Directory.GetDirectories(CopyFrom)
            'Get the name of the directory
            Dim DirInfo = New DirectoryInfo(Directories)

            'Exclude hidden directories like 'System Volume Information'
            If Not (DirInfo.Attributes And FileAttributes.Hidden) = FileAttributes.Hidden Then
                'Copy directories on \ with content to the temp folder
                My.Computer.FileSystem.CopyDirectory(Directories, CopyTo + DirInfo.Name, True)
            End If

            'Report progress
            DirsCopiedCount += 1
            CopyWorker.ReportProgress(1)
        Next

    End Sub

    Private Sub CopyWorker_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles CopyWorker.ProgressChanged

        CopyProgressBar.Value += 1

        If Restore = False Then
            If StatusTextBlock.Dispatcher.CheckAccess() = False Then
                StatusTextBlock.Dispatcher.BeginInvoke(Sub() StatusTextBlock.Text = "Backing up " + FilesCopiedCount.ToString + "/" + FilesCopyCount.ToString + " files and " +
                                                       DirsCopiedCount.ToString + "/" + DirsCopyCount.ToString + " directories from the USB.")
            Else
                StatusTextBlock.Text = "Backing up " + FilesCopiedCount.ToString + "/" + FilesCopyCount.ToString + " files and " +
                                                       DirsCopiedCount.ToString + "/" + DirsCopyCount.ToString + " directories from the USB."
            End If
        Else
            If StatusTextBlock.Dispatcher.CheckAccess() = False Then
                StatusTextBlock.Dispatcher.BeginInvoke(Sub() StatusTextBlock.Text = "Restoring " + FilesCopiedCount.ToString + "/" + FilesCopyCount.ToString + " files and " +
                                                       DirsCopiedCount.ToString + "/" + DirsCopyCount.ToString + " directories on the USB.")
            Else
                StatusTextBlock.Text = "Restoring " + FilesCopiedCount.ToString + "/" + FilesCopyCount.ToString + " files and " +
                                                       DirsCopiedCount.ToString + "/" + DirsCopyCount.ToString + " directories on the USB."
            End If
        End If
    End Sub

    Private Sub CopyWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles CopyWorker.RunWorkerCompleted
        DialogResult = True
        Close()
    End Sub

End Class
