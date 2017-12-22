using System;
using System.IO;
using System.Security.Cryptography;

namespace ConsoleWinSCP.Model
{
    public class TransferRecord
    {
        /// <summary>檔案名稱</summary>
        public string FileName { get; set; }

        /// <summary>檔案本機路徑</summary>
        public string LocalFilePath { get; set; }

        /// <summary>檔案伺服器路徑</summary>
        public string RemoteFilePath { get; set; }

        /// <summary>驗證碼by清單</summary>
        public string SHA256 { get; set; }

        /// <summary>驗證碼by實體檔案(上傳前取得或下載後取得)</summary>
        public string FileSHA256 { get; set; }

        /// <summary>是否傳輸成功</summary>
        public bool Done { get; set; } = false;

        /// <summary>檔案大小</summary>
        public long TransferFileSize { get; set; }

        /// <summary>傳輸進度</summary>
        public string TransferProgress { get; set; }

        /// <summary>傳輸花費時間</summary>
        public TimeSpan TransferElapsed { get; set; }

        /// <summary>傳輸速度(kB/s)</summary>
        public long TransferSpeed { get; set; } = 0;

        /// <summary>嘗試傳輸次數</summary>
        public int ProcessTimes { get; set; } = 0;
                
        /// <summary>是否重新嘗試傳輸(ex.檔案不存在時，不用重新傳輸，故此值為false)</summary>
        public bool NeedToReProcess { get; set; } = true;

        /// <summary>上傳 or 下載</summary>
        public Operation OperationType { get; set; }

        /// <summary>錯誤訊息</summary>
        public string ErrorMessage { get; set; }

        /// <summary>錯誤代碼</summary>
        public string ErrorCode { get; set; }

        /// <summary>傳輸狀態代碼</summary>
        public string Status { get; set; }

        /// <summary>計算平均傳輸速率(kB/s)</summary>
        public void CalAveRate()
        {
            long lenKB = (TransferFileSize / 1024).ToLongOrDefault(0);            
            TransferSpeed = lenKB / Math.Ceiling(TransferElapsed.TotalSeconds).ToLongOrDefault(1);
        }

        /// <summary>根據檔案實體路徑取得SHA256</summary>
        public void GetFileSHA256()
        {
            using (FileStream stream = File.OpenRead(LocalFilePath))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] hash = sha.ComputeHash(stream);
                FileSHA256 = BitConverter.ToString(hash).Replace("-", String.Empty);                
            }
        }

        /// <summary>比對清單SHA256與實體檔案SHA256</summary>
        /// <returns>true|成功、false|失敗</returns>
        public bool ComparisonHash()
        {
            if (string.IsNullOrWhiteSpace(SHA256) || string.IsNullOrWhiteSpace(FileSHA256))            
                return false;

            if (SHA256 != FileSHA256)
                return false;

            return true;
        }
    }
}
