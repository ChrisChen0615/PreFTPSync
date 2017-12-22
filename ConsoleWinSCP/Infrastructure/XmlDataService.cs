using ConsoleWinSCP.Interface;
using ConsoleWinSCP.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleWinSCP.Infrastructure
{
    public class XmlDataService : IDataService
    {        
        public XmlDataService()
        {
        }

        /// <summary>取得檔案清單</summary>
        /// <param name="filePath">xml實體檔案路徑</param>
        /// <returns></returns>
        public List<TransferRecord> GetRecords(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("xml檔案路徑不得為空!");

            string xmlString = null;

            using (StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                xmlString = streamReader.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(xmlString))
            {
                var xmlFiles = xmlString.DeserializeXml<XMLOptions.Files>();
                
                return xmlFiles.ListFileTag.Select(p => new TransferRecord()
                {
                    FileName = p.Name,
                    SHA256 = p.SHA256,
                    OperationType = (Operation)Enum.Parse(typeof(Operation), p.OperationType)                    
                }).ToList();
            }

            return null;
        }

        /// <summary>取得xml檔案路徑</summary>
        /// <returns></returns>
        public string GetFilePath()
        {
            return ConfigurationManager.AppSettings["XmlFilePath"];
        }
    }
}
