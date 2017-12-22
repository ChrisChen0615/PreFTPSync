using ConsoleWinSCP.Infrastructure;
using ConsoleWinSCP.Interface;
using ConsoleWinSCP.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleWinSCP
{
    public class Program
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        //private static string filepath = null;
        static FileWatcher watcher = null;
        static Config cfg = null;
        static WinSCPClient client = null;
        static List<Task> tasks = new List<Task>();

        public static void Main(string[] args)
        {
            List<string> error = new List<string>();            
            //IDataService xmlService = new XmlDataService();
            //XMLOptions xml = new XMLOptions();
            //FileWatcher watcher = new FileWatcher();

            try
            {
                log.Info("**********Application Start**********");
                //建立xml資料夾監聽
                watcher = new FileWatcher();
                watcher.Init();

                //初始化伺服端設定檔
                cfg = new Config();
                cfg.Init();

                //var client = new WinSCPClient(cfg);
                //建立winscp client
                client = new WinSCPClient(cfg);

                //監聽新增事件
                watcher.EvtGetFileName += new FileWatcher.DelGetFileName(GetFileName);
                
                //初始化，xml資料夾內檔案執行傳輸
                foreach (var oneXml in watcher.FileList)
                {
                    Task task = new Task(() =>
                    {
                        MainOperate(oneXml);
                    });
                    tasks.Add(task);
                }

                while (0 < tasks.Count)
                {
                    tasks[0].Start();
                    Task.WaitAny(tasks[0]);

                    var temp = tasks.ToList();
                    temp.RemoveAt(0);
                    tasks = temp;
                }
                
                
                

                //var cfg = new Config();
                //cfg.Init();

                ////var client = new WinSCPClient(cfg);
                //client = new WinSCPClient(cfg);
                //// 取得檔案
                //var filepath = xmlService.GetFilePath();
                //var files = xmlService.GetRecords(filepath);

                //client.Operate(files);

                //foreach (var f in files)
                //{
                //    if (f.Done)
                //    {
                //        error.Add($"[完成]檔案名稱:{f.FileName}\n傳輸速率:{f.TransferSpeed.ToString("#,#")}(kB/s)");
                //    }
                //    else
                //    {
                //        error.Add($"[失敗]檔案名稱:{f.FileName}\n{f.ErrorMessage}");
                //    }
                //}

                ////傳輸完成後產生紀錄檔
                //xml.CreateXmlFile(files, $"{cfg.FinishXMLFilePath}{DateTime.Now.ToString("yyyyMMddHHmm")}.xml");
            }
            catch (ArgumentException ex)
            {
                error.Add($"伺服器連線資訊設定檔錯誤: {ex.Message}");
                Console.WriteLine($"伺服器連線資訊設定檔錯誤: {ex.Message}");
            }
            catch (Exception ex)
            {
                error.Add($"Error: {ex.Message}");
            }
            finally
            {
                if (error.Count > 0)
                {
                    foreach (var er in error)
                    {
                        log.Error(er);
                        Console.WriteLine(er);
                    }
                }
                log.Info("**********Application End**********");
                Console.WriteLine("The End.");
                
                Console.ReadKey();
            }
        }

        private static void GetFileName(string fileName)
        {            
            Console.WriteLine($"main接收:{fileName}");

            Task task = new Task(() =>
            {
                MainOperate(fileName);
            });

            tasks.Add(task);
        }

        private static void MainOperate(string fileName)
        {
            IDataService xmlService = new XmlDataService();
            XMLOptions xml = new XMLOptions();
                        
            // 取得檔案
            var fullfielpath = Path.Combine(watcher.XmlFilePath, fileName);
            var files = xmlService.GetRecords(fullfielpath);

            Console.WriteLine($"開始傳輸檔案，清單:{fileName}");

            client.Operate(files);

            //傳輸完成後產生紀錄檔
            xml.CreateXmlFile(files, $"{cfg.FinishXMLFilePath}{DateTime.Now.ToString("yyyyMMddHHmm")}.xml");
        }
    }    
}
