using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using ConsoleWinSCP.Interface;
using System.IO;
using System.Collections.Generic;
using ConsoleWinSCP.Model;
using System.Linq;
using System;

namespace ConsoleWinSCP.Infrastructure.Tests
{
    [TestClass()]
    public class XmlServiceTests
    {
        private string _xmlFilePath = @"D:\TestFTP\list.xml";
        private string _xmlString = @"<?xml version='1.0' ?>
<Files>
    <FileTag>
        <Name>core.pdf</Name>
        <SHA256></SHA256>
        <OperationType>Download</OperationType>
    </FileTag>
    <FileTag>
        <Name>系統教育訓練教材_1_偵結數位卷建檔.docx</Name>
        <SHA256>A790C40E4AC7BBD6A786519187733DC5074C3E1600202342702417671AEBDE56</SHA256>
        <OperationType>Download</OperationType>
    </FileTag>
    <FileTag>
        <Name>系統教育訓練教材_2_公訴數位卷建檔.docx</Name>
        <SHA256>9AEA6104CA3EA622B30C76040AF47B9378642ABD02506200ABBBC541CFDB3A7D</SHA256>
        <OperationType>Download</OperationType>
    </FileTag>
    <FileTag>
        <Name>系統教育訓練教材_3_公訴案件辦理及數位卷證應用.docx</Name>
        <SHA256>2BEA56339F76690B366034D3B93DB2B82FF0010C8BE81A571ADCAAAAC6487F01</SHA256>
        <OperationType>Download</OperationType>
    </FileTag>
    <FileTag>
        <Name>系統教育訓練教材_4_系統客製化PDF編輯器操作.docx</Name>
        <SHA256>B4052020B28EB182BBDD3CA728A8E4E2377FA3704845D666E213827C19B0DE12</SHA256>
        <OperationType>Download</OperationType>
    </FileTag>
    <FileTag>
        <Name>系統教育訓練教材_5_系統管理.docx</Name>
        <SHA256>BA5467AFC856BBEE954F1C33230E88D6034B7943B06E7E97642BF92E0FB1FA77</SHA256>
        <OperationType>Download</OperationType>
    </FileTag>
    <FileTag>
        <Name>法務部晶片鎖管理系統操作手冊_20161130.pdf</Name>
        <SHA256>16C7684D5D066C89ECF09E2964AFC6522D19780D80993055AF749444D6DDE21E</SHA256>
        <OperationType>Download</OperationType>
    </FileTag>
    <FileTag>
        <Name>偵書上線教育訓練_10602-偵書1060205-all.pptx</Name>
        <SHA256>85A089BD8E6C54F7EFD1501DA0839ADD5574B8279D372BCBA27117423F59D6B2</SHA256>
        <OperationType>Download</OperationType>
    </FileTag>
    <FileTag>
        <Name>檢事官上線教育訓練_10602-檢事官1060218_3.pptx</Name>
        <SHA256>CC937E00DF67A084C85C4D2E55972FAC78E22C5EA9A2CF00B7ABC5AF6D90D4BC</SHA256>
        <OperationType>Download</OperationType>
    </FileTag>
    <FileTag>
        <Name>樣本卷證_NEW.7z</Name>
        <SHA256></SHA256>
        <OperationType>Download</OperationType>
    </FileTag>
</Files>";

        [TestMethod()]
        public void GetRecordsTest()
        {
            string xmlString = _xmlString;
            var result = new List<TransferRecord>();
            if (!string.IsNullOrWhiteSpace(xmlString))
            {
                var xmlFiles = xmlString.DeserializeXml<XMLOptions.Files>();

                result = xmlFiles.ListFileTag.Select(p => new TransferRecord()
                {
                    FileName = p.Name,
                    SHA256 = p.SHA256,
                    OperationType = (Operation)Enum.Parse(typeof(Operation), p.OperationType)
                }).ToList();
            }
            Assert.Fail();
        }
    }
}