
namespace ConsoleWinSCP.Model
{
    /// <summary>伺服器類型</summary>
    public enum FtpType
    {
        SFTP, SCP, FTP, Webdav
    }

    /// <summary>加密協定</summary>
    public enum FtpSecure
    {
        None, Implicit, Explicit = 3
    }

    /// <summary>伺服器連線後操作動作(上傳或下載)</summary>
    public enum Operation
    {
        Upload, Download
    }
    
}
