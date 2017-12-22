using ConsoleWinSCP.Model;
using System.Collections.Generic;

namespace ConsoleWinSCP.Interface
{    
    public interface IDataService
    {
        /// <summary>取得檔案清單</summary>
        /// <param name="filePath">xml實體檔案路徑</param>
        /// <returns></returns>
        List<TransferRecord> GetRecords(string filePath);

        /// <summary>取得xml檔案路徑</summary>
        /// <returns></returns>
        string GetFilePath();
    }
}
