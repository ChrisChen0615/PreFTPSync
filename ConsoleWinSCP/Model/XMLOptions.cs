using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

namespace ConsoleWinSCP.Model
{
    public class XMLOptions
    {
        /// <summary>傳輸檔案清單</summary>
        [Serializable()]
        public partial class Files
        {
            [XmlElement("FileTag")]
            public List<FileTag> ListFileTag { get; set; } = new List<FileTag>();
        }

        /// <summary>傳輸單一檔案物件</summary>
        [Serializable()]
        public partial class FileTag
        {
            /// <summary>檔案名稱</summary>
            public string Name { get; set; }

            /// <summary>檔案名稱sha256編碼</summary>
            public string SHA256 { get; set; }

            /// <summary>檔案操作動作(上傳 or 下載)</summary>
            public string OperationType { get; set; }
        }

        /// <summary>傳輸後檔案物件(傳輸後記錄用)</summary>
        [Serializable()]
        public class FinishXmlFile
        {
            public string FileName { get; set; }
            public string LocalPaht { get; set; }
            public string RemotePath { get; set; }
            public bool Done { get; set; }
        }

        /// <summary>紀錄傳輸檔案清單</summary>
        /// <param name="files"></param>
        public void CreateXmlFile(List<TransferRecord> files,string filepath)
        {
            var xml = files.Select(f => new FinishXmlFile()
            {
                FileName = f.FileName,
                LocalPaht = f.LocalFilePath,
                RemotePath = f.RemoteFilePath,
                Done = f.Done
            }).ToList();

            XmlSerializer serializer = new XmlSerializer(typeof(List<FinishXmlFile>));
            using (TextWriter writer = new StreamWriter(filepath))
            {
                serializer.Serialize(writer, xml);
            }
        }
    }
}
