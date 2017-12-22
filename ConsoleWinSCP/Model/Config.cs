using System;
using System.Configuration;

namespace ConsoleWinSCP.Model
{
    public class Config
    {
        /// <summary>伺服器類型</summary>
        public FtpType ProtocolType { get; set; }

        /// <summary>伺服器位址IP或domain name</summary>
        public string HostName { get; set; }

        /// <summary>登入帳號</summary>
        public string UserName { get; set; }

        /// <summary>登入密碼</summary>
        public string Password { get; set; }


        #region For SFTP server setting
        /// <summary>ssh host key</summary>
        public string SshHostKeyFingerprint { get; set; }
        #endregion


        #region For FTPS server setting
        /// <summary>FTPS PORT</summary>
        public int PortNumber { get; set; }

        /// <summary>SSL</summary>
        public string TlsHostCertificateFingerprint { get; set; }

        /// <summary>Ftp Secure加密協定</summary>
        public FtpSecure FtpSecureType { get; set; }
        #endregion

        /// <summary>伺服器操作動作(上傳 or 下載)</summary>
        public Operation Operation { get; set; }

        /// <summary>傳輸速度限制，0為不限速</summary>
        public int SpeedLimit { get; set; }

        /// <summary>本機資料路徑(上傳檔案的資料夾/下載檔案的資料夾)</summary>
        public string LocalPath { get; set; }

        /// <summary>伺服器資料路徑(上傳檔案的資料夾/下載檔案的資料夾)</summary>
        public string RemotePath { get; set; }

        /// <summary>傳輸完成後記錄檔路徑</summary>
        public string FinishXMLFilePath { get; set; }

        public void Init()
        {
            ProtocolType = (FtpType)Enum.Parse(typeof(FtpType), ConfigurationManager.AppSettings["ProtocolType"]);
            HostName = ConfigurationManager.AppSettings["HostName"];
            UserName = ConfigurationManager.AppSettings["UserName"];
            Password = ConfigurationManager.AppSettings["Password"];

            switch (ProtocolType)
            {
                case FtpType.SFTP:
                    SshHostKeyFingerprint = ConfigurationManager.AppSettings["SshHostKeyFingerprint"];
                    break;
                case FtpType.FTP:
                    PortNumber = ConfigurationManager.AppSettings["PortNumber"].ToInt16OrDefault(21);
                    TlsHostCertificateFingerprint = ConfigurationManager.AppSettings["TlsHostCertificateFingerprint"];
                    FtpSecureType = (FtpSecure)Enum.Parse(typeof(FtpSecure), ConfigurationManager.AppSettings["FtpSecureType"]);
                    break;
            }

            SpeedLimit = ConfigurationManager.AppSettings["SpeedLimit"].ToInt16OrDefault(0);

            LocalPath = ConfigurationManager.AppSettings["LocalPath"];
            RemotePath = ConfigurationManager.AppSettings["RemotePath"];
            FinishXMLFilePath = ConfigurationManager.AppSettings["FinishXMLFilePath"];
        }
    }
}
