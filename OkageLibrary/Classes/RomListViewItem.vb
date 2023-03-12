Public Class RomListViewItem

    Private _RomFileName As String
    Private _RomFileSize As String
    Private _RomFilePath As String

    Public Property RomFileName As String
        Get
            Return _RomFileName
        End Get
        Set
            _RomFileName = Value
        End Set
    End Property

    Public Property RomFileSize As String
        Get
            Return _RomFileSize
        End Get
        Set
            _RomFileSize = Value
        End Set
    End Property

    Public Property RomFilePath As String
        Get
            Return _RomFilePath
        End Get
        Set
            _RomFilePath = Value
        End Set
    End Property

End Class
