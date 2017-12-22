using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConsoleWinSCP.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWinSCP.Interface;
using ConsoleWinSCP.Model;
using System.IO;
using System.Security.Cryptography;


namespace ConsoleWinSCP.Infrastructure.Tests
{
    [TestClass()]
    public class WinSCPClientTests
    {
        private Config cfg = new Config()
        {
            ProtocolType = FtpType.FTP,
            HostName = "192.168.0.227",
            UserName = "ftpuser",
            Password = "p2ssW1rd",
            //TlsHostCertificateFingerprint = "4d:d5:9e:b1:64:13:ce:fd:49:a9:d0:d1:ba:ae:9f:07:04:8d:e1:e4",
            FtpSecureType = FtpSecure.None,            
            LocalPath = @"D:\TestFTP\operate\",
            RemotePath = "/operate/"
        };

        private string _xmlFilePath = @"D:\TestFTP\TEST.xml";
        private string _xmlString = @"<?xml version='1.0' ?>
<Files>
    <FileTag>
        <Name>1GB.bin</Name>
        <SHA256>DA87281C9F9AB6CEF8F9362935F4FC864DB94606D52212614894F1253461A762</SHA256>
        <OperationType>Upload</OperationType>
    </FileTag>
</Files>";


        //                new TransferRecord() { FileName="core.pdf" ,SHA256="",OperationType=Operation.Download },
        //new TransferRecord() { FileName="系統教育訓練教材_1_偵結數位卷建檔.docx" ,SHA256="A790C40E4AC7BBD6A786519187733DC5074C3E1600202342702417671AEBDE56",OperationType=Operation.Download }
        //new TransferRecord() { FileName="系統教育訓練教材_2_公訴數位卷建檔.docx" ,SHA256="9AEA6104CA3EA622B30C76040AF47B9378642ABD02506200ABBBC541CFDB3A7D",OperationType=Operation.Download },
        //new TransferRecord() { FileName="系統教育訓練教材_3_公訴案件辦理及數位卷證應用.docx" ,SHA256="2BEA56339F76690B366034D3B93DB2B82FF0010C8BE81A571ADCAAAAC6487F01" ,OperationType=Operation.Download},
        //new TransferRecord() { FileName="系統教育訓練教材_4_系統客製化PDF編輯器操作.docx" ,SHA256="B4052020B28EB182BBDD3CA728A8E4E2377FA3704845D666E213827C19B0DE12" ,OperationType=Operation.Download},
        //new TransferRecord() { FileName="系統教育訓練教材_5_系統管理.docx" ,SHA256="BA5467AFC856BBEE954F1C33230E88D6034B7943B06E7E97642BF92E0FB1FA77" ,OperationType=Operation.Download},
        //new TransferRecord() { FileName="法務部晶片鎖管理系統操作手冊_20161130.pdf" ,SHA256="16C7684D5D066C89ECF09E2964AFC6522D19780D80993055AF749444D6DDE21E" ,OperationType=Operation.Download},
        //new TransferRecord() { FileName="偵書上線教育訓練_10602-偵書1060205-all.pptx" ,SHA256="85A089BD8E6C54F7EFD1501DA0839ADD5574B8279D372BCBA27117423F59D6B2" ,OperationType=Operation.Download},
        //new TransferRecord() { FileName="檢事官上線教育訓練_10602-檢事官1060218_3.pptx" ,SHA256="CC937E00DF67A084C85C4D2E55972FAC78E22C5EA9A2CF00B7ABC5AF6D90D4BC" ,OperationType=Operation.Download},
        //new TransferRecord() { FileName="樣本卷證_NEW.7z" ,SHA256="FAE1CB584B61B2FD045843CCFD4927765A90F9EDF9FE018550DE1821182A2B4D" ,OperationType=Operation.Download}

        [TestMethod()]
        public void WinSCPClientTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void OperateTest()
        {
            var client = new WinSCPClient(cfg);

            // 取得檔案
            string xmlString = _xmlString;
            var files = new List<TransferRecord>();
            if (!string.IsNullOrWhiteSpace(xmlString))
            {
                var xmlFiles = xmlString.DeserializeXml<XMLOptions.Files>();

                files = xmlFiles.ListFileTag.Select(p => new TransferRecord()
                {
                    FileName = p.Name,
                    SHA256 = p.SHA256,
                    OperationType = (Operation)Enum.Parse(typeof(Operation), p.OperationType)
                }).ToList();
            }

            client.Operate(files);
            Assert.AreEqual(files.FirstOrDefault().SHA256, files.FirstOrDefault().FileSHA256);            
        }

        [TestMethod()]
        public void UploadFileTest()
        {
            
            Assert.Fail();
        }

        [TestMethod()]
        public void DownloadFileTest()
        {
            var cfg = new Config();
            cfg.ProtocolType = FtpType.FTP;
            cfg.HostName = "192.168.0.226";
            cfg.UserName = "ftpuser";
            cfg.Password = "ftpuser";
            cfg.TlsHostCertificateFingerprint = "4d:d5:9e:b1:64:13:ce:fd:49:a9:d0:d1:ba:ae:9f:07:04:8d:e1:e4";
            cfg.FtpSecureType = FtpSecure.Explicit;
            cfg.SpeedLimit = 0;
            cfg.LocalPath = @"D:\TestFTP\download\";
            cfg.RemotePath = "/download/";            
                        
            var records = new List<TransferRecord>()
            {
                new TransferRecord() { FileName="core.pdf" ,SHA256="",OperationType=Operation.Download },
new TransferRecord() { FileName="系統教育訓練教材_1_偵結數位卷建檔.docx" ,SHA256="A790C40E4AC7BBD6A786519187733DC5074C3E1600202342702417671AEBDE56",OperationType=Operation.Download }
            };

            var client = new WinSCPClient(cfg);
            StringBuilder error = new StringBuilder();
            client.Operate(records);
            foreach (var r in records)
            {
                if (!r.Done) error.AppendLine(r.ErrorMessage);
            }
            Assert.AreEqual(string.Empty, error.ToString());
        }
        
        [TestMethod]
        public void GetFileSHA256()
        {
            string FileSHA256 = string.Empty;

            using (FileStream f = File.OpenRead(@"D:\TestFTP\operate\10GB.bin"))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] hash = sha.ComputeHash(f);
                FileSHA256 = BitConverter.ToString(hash).Replace("-", String.Empty);
            }

            //Assert.AreEqual("6CBDF4E13EEAF37849DA6A9705D25C5B268F4B70382147CE11EB253EDFF8442E", FileSHA256);
            Assert.AreEqual("DA87281C9F9AB6CEF8F9362935F4FC864DB94606D52212614894F1253461A762", FileSHA256);
        }
    }
}