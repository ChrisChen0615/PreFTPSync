using ConsoleWinSCP.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WinSCP;
using FtpSecure = WinSCP.FtpSecure;

namespace ConsoleWinSCP.Infrastructure
{
    public class WinSCPClient
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private Config _cfg = null;
        private Session _session = null;
        private bool _isConnect = false;
        private SessionOptions _sessionOptions = null;
        
        public WinSCPClient(Config config)
        {
            _cfg = config;
            InitRemoteServer(config);
        }

        /// <summary>初始化伺服器連線資訊物件</summary>
        private void InitRemoteServer(Config config)
        {
            var _cfg = config;
            switch (_cfg.ProtocolType)
            {
                case FtpType.SFTP:
                    _sessionOptions = new SessionOptions()
                    {
                        Protocol = (Protocol)_cfg.ProtocolType,
                        HostName = _cfg.HostName,
                        UserName = _cfg.UserName,
                        Password = _cfg.Password,
                        SshHostKeyFingerprint = _cfg.SshHostKeyFingerprint
                    };
                    break;
                case FtpType.FTP:
                    _sessionOptions = new SessionOptions()
                    {
                        Protocol = (Protocol)_cfg.ProtocolType,
                        HostName = _cfg.HostName,
                        UserName = _cfg.UserName,
                        Password = _cfg.Password,
                        //TlsHostCertificateFingerprint = _cfg.TlsHostCertificateFingerprint,
                        PortNumber = _cfg.PortNumber,
                        FtpSecure = (FtpSecure)_cfg.FtpSecureType
                    };
                    break;
                default:
                    throw new Exception("伺服器種類設定檔(ProtocolType)有誤");
            }
        }

        /// <summary>執行上傳或下載(由TransferRecord.OperationType定義)</summary>
        public bool Operate(List<TransferRecord> records)
        {
            // 檢查是否連線
            if (!_isConnect) Connect();

            foreach (var record in records)
            {
                try
                {
                    switch (record.OperationType)                    
                    {
                        case Operation.Download:
                            DownloadFile(record);
                            break;
                        case Operation.Upload:
                            UploadFile(record);
                            break;
                        default:
                            throw new Exception("伺服器操作行為未設定");
                    }
                }
                //承接TransferOperationResult.check()的failure
                catch (Exception ex)
                {
                    record.Done = false;
                    switch (record.OperationType)
                    {
                        case Operation.Download:
                            record.Status = "41";
                            break;
                        case Operation.Upload:
                            record.Status = "21";
                            break;
                    }
                    record.ErrorMessage = $"{record.OperationType.ToString()} {record.FileName}失敗，訊息:{ex.Message}";
                    log.Error($"{record.ErrorMessage}");
                    continue;
                }
            }

            DisConnect();

            return true;
        }

        /// <summary>伺服器連線</summary>
        private void Connect()
        {
            //todo:連線失敗 嘗試重新連線 session.ReconnectTime,Default is 120 seconds,open前設定
            try
            {
                if (!_isConnect)
                {                    
                    _session = new Session();
                    _session.Open(_sessionOptions);

                    _isConnect = true;
                }
            }
            catch (SessionRemoteException ex)
            {
                _isConnect = false;
                throw new Exception($"伺服器連線失敗，請檢查設定檔、伺服器:{ex.Message}");
            }
            catch (Exception ex)
            {
                _isConnect = false;
                throw new Exception($"伺服器連線失敗:{ex.Message}");
            }
        }
        
        /// <summary>伺服器連線中止</summary>
        private void DisConnect()
        {
            if (_isConnect)
            {
                if (_session.Opened)
                {
                    _session.Dispose();
                    _isConnect = false;
                }
            }
        }
        
        /// <summary>下載</summary>
        /// <param name="record"></param>
        public void DownloadFile(TransferRecord record)
        {
            var defaultLocalPath = _cfg.LocalPath;
            var defaultRemotePath = _cfg.RemotePath;

            TransferOptions transferOptions = new TransferOptions();
            transferOptions.TransferMode = TransferMode.Automatic;
            //恢復續傳
            transferOptions.OverwriteMode = OverwriteMode.Resume;

            //傳輸速度(kb/s)
            if (!(_cfg.SpeedLimit == 0))
            {
                transferOptions.SpeedLimit = _cfg.SpeedLimit;
            }

            Stopwatch sw = new Stopwatch();            

            string filePath = Path.Combine(defaultRemotePath, record.FileName);
            string localFilePath = Path.Combine(defaultLocalPath, record.FileName);

            // 嘗試次數
            record.ProcessTimes++;
            record.LocalFilePath = localFilePath;
            record.RemoteFilePath = filePath;
            record.Status = "40";

            log.Info($"檔名:{record.FileName}");
            log.Info($"本機路徑:{record.LocalFilePath}");
            log.Info($"遠端路徑:{record.RemoteFilePath}");

            if (_session.FileExists(filePath))
            {
                //取得檔案長度(位元組)
                record.TransferFileSize = _session.GetFileInfo(filePath).Length;                
                log.Info("開始下載");
                sw.Start();
                var transferResult = _session.GetFiles(filePath, defaultLocalPath, false, transferOptions);
                sw.Stop();
                record.TransferElapsed = sw.Elapsed;

                /*
                 * transferResult.IsSuccess:Is true, if .failures is empty collection
                 * If there's a failure, the transferResult.Check() throws.
                */
                transferResult.Check();
                record.CalAveRate();
                log.Info($"檔案大小(位元組):{record.TransferFileSize.ToString("#,#")}");
                log.Info($"均速(kB/s):{record.TransferSpeed.ToString("#,#")}");

                if (transferResult.IsSuccess)
                {
                    //下載後驗證碼檢查
                    record.GetFileSHA256();
                    if (record.ComparisonHash())
                    {                        
                        record.Done = true;
                        record.Status = "50";
                        log.Info("[完成]");
                    }
                    else
                    {
                        record.Done = false;                        
                        record.ErrorCode = "SHA";
                        record.Status = "41";
                        record.ErrorMessage = $"檔案下載失敗,訊息:檔案SHA256驗證碼失敗";
                        log.Error($"[失敗]{record.ErrorMessage}");
                        if (File.Exists(localFilePath))
                        {
                            File.Delete(localFilePath);
                        }
                    }
                }
                else
                {
                    // 加入錯誤訊息
                    record.Done = false;
                    record.ErrorCode = "DownLoad";
                    record.Status = "41";

                    var error = transferResult.Transfers.FirstOrDefault();                    
                    if (error != null)
                    {
                        record.ErrorMessage = $"檔案下載失敗,訊息:{ error.Error.Message}";
                        log.Error($"[失敗]{record.ErrorMessage}");
                    }
                }
            }
            else
            {
                record.Done = false;
                record.Status = "41";
                record.ErrorMessage = $"檔案下載失敗,訊息:檔案不存在";
                log.Error($"[失敗]{record.ErrorMessage}");
            }
        }

        public void UploadFile(TransferRecord record)
        {
            var defaultLocalPath = _cfg.LocalPath;
            var defaultRemotePath = _cfg.RemotePath;

            TransferOptions transferOptions = new TransferOptions();
            transferOptions.TransferMode = TransferMode.Automatic;
            //switch (_cfg.ProtocolType)
            //{
            //    case FtpType.SFTP:
            //        //恢復續傳
            //        transferOptions.ResumeSupport.State = TransferResumeSupportState.On;
            //        break;
            //    case FtpType.FTP:
            //        //no way...
            //        break;
            //}
            //傳輸速度(KB/s)
            if (!(_cfg.SpeedLimit == 0))
            {
                transferOptions.SpeedLimit = _cfg.SpeedLimit;
            }            

            Stopwatch sw = new Stopwatch();            

            string filePath = Path.Combine(defaultLocalPath, record.FileName);
            string remoteFilePath = Path.Combine(defaultRemotePath, record.FileName);

            // 嘗試次數
            record.ProcessTimes++;
            record.LocalFilePath = filePath;
            record.RemoteFilePath = remoteFilePath;
            record.Status = "20";

            Console.WriteLine($"檔名:{record.FileName}");
            Console.WriteLine($"本機路徑:{record.LocalFilePath}");
            Console.WriteLine($"遠端路徑:{record.RemoteFilePath}");

            log.Info($"檔名:{record.FileName}");
            log.Info($"本機路徑:{record.LocalFilePath}");
            log.Info($"遠端路徑:{record.RemoteFilePath}");

            //file exist or not
            if (File.Exists(filePath))
            {
                //上傳前驗證碼檢查
                record.GetFileSHA256();
                if (!record.ComparisonHash())
                {
                    record.Done = false;
                    record.ErrorCode = "SHA";
                    record.Status = "21";
                    record.ErrorMessage = "檔案上傳失敗,訊息:檔案SHA256驗證碼失敗";
                    log.Error($"[失敗]{record.ErrorMessage}");
                    Console.WriteLine($"[失敗]{record.ErrorMessage}");
                    return;
                }

                log.Info("開始上傳");
                Console.WriteLine("開始上傳");
                sw.Start();
                var transferResult = _session.PutFiles(filePath, defaultRemotePath, false, transferOptions);
                sw.Stop();
                record.TransferElapsed = sw.Elapsed;

                //Throw the first failure in the list
                transferResult.Check();
                //檔案長度(位元組)
                record.TransferFileSize = _session.GetFileInfo(remoteFilePath).Length;
                record.CalAveRate();
                log.Info($"檔案大小(位元組):{record.TransferFileSize.ToString("#,#")}");
                log.Info($"均速(kB/s):{record.TransferSpeed.ToString("#,#")}");
                Console.WriteLine($"檔案大小(位元組):{record.TransferFileSize.ToString("#,#")}");
                Console.WriteLine($"均速(kB/s):{record.TransferSpeed.ToString("#,#")}");

                if (transferResult.IsSuccess)
                {                    
                    record.Done = true;
                    record.Status = "30";
                    log.Info("[完成]");
                    Console.WriteLine("[完成]");
                }
                else
                {
                    record.Done = false;
                    record.ErrorCode = "UpLoad";
                    record.Status = "21";

                    var error = transferResult.Transfers.FirstOrDefault();
                    if (error != null)
                    {
                        record.ErrorMessage = $"檔案上傳失敗,訊息:{ error.Error.Message}";
                        log.Error($"[失敗]{record.ErrorMessage}");
                        Console.WriteLine($"[失敗]{record.ErrorMessage}");
                    }
                }
            }
            else
            {
                record.Done = false;
                record.Status = "21";
                record.ErrorMessage = $"檔案上傳失敗,訊息:檔案不存在";
                log.Error($"[失敗]{record.ErrorMessage}");
                Console.WriteLine($"[失敗]{record.ErrorMessage}");
            }
        }
    }
}
