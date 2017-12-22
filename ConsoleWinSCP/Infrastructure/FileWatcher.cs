using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Configuration;

namespace ConsoleWinSCP.Infrastructure
{
    /// <summary>檔案清單監聽</summary>
    public class FileWatcher
    {
        public delegate void DelGetFileName(string fileName);
        public event DelGetFileName EvtGetFileName;

        private DirectoryInfo dirInfo;
        private FileSystemWatcher _watch;
        //List<string> FileList;
        public List<string> FileList { get; set; }
        public string XmlFilePath { get; set; }

        public FileWatcher()
        {
            XmlFilePath = ConfigurationManager.AppSettings["XmlFilePath"];

            _watch = new FileSystemWatcher();
            FileList = new List<string>();

            //設定所要監控的資料夾
            _watch.Path = XmlFilePath;            

            //設定所要監控的變更類型
            _watch.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            //設定所要監控的檔案
            //_watch.Filter = "*.xml";

            //設定是否監控子資料夾
            _watch.IncludeSubdirectories = true;

            //設定是否啟動元件，此部分必須要設定為 true，不然事件是不會被觸發的
            _watch.EnableRaisingEvents = true;

            //設定觸發事件
            _watch.Created += new FileSystemEventHandler(_watch_Created);
            //_watch.Deleted += new FileSystemEventHandler(_watch_Deleted);
        }

        public void Init()
        {            
            dirInfo = new DirectoryInfo(XmlFilePath);
            foreach (var fi in dirInfo.GetFiles())
            {
                FileList.Add(fi.Name);
            }            
        }

        /// <summary>當所監控的資料夾有建立檔案時觸發</summary>
        private void _watch_Created(object sender, FileSystemEventArgs e)
        {
            EvtGetFileName(e.Name);
        }

        /// <summary>當所監控的資料夾有檔案有被刪除時觸發</summary>
        //private void _watch_Deleted(object sender, FileSystemEventArgs e)
        //{
        //    FileList.Remove(e.Name);
        //    Console.WriteLine($"刪除檔案:{e.Name},刪除時間:{DateTime.Now.ToString()}");
        //    Console.WriteLine($"目前檔案數:{FileList.Count}");
        //}
    }
}
