using NPOI.HPSF;
using NPOI.HSSF.Model;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Emre
{
    public static class Program
    {
        public static HSSFWorkbook Hssfworkbook { get; set; }
        public static ICellStyle Headerstyle { get; set; }
        static void Main(string[] args)
        {
            var outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Logfiles", "EmreC.xls");
            //var inputFiles = Directory.EnumerateFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"LOG\LOG_ASYA\bak204\10.201.107.204_3195180\20180806020514"), "*.cfg").ToList();
            var inputFiles = Directory.EnumerateFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Logfiles"), "*.cfg").ToList();
            SetExcelTemplate();
            CreateExcel(inputFiles, outputPath);
        }

        private static void SetExcelTemplate()
        {
            Hssfworkbook = new HSSFWorkbook();
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "YE-MA SOFT";
            Hssfworkbook.DocumentSummaryInformation = dsi;

            IFont font1 = Hssfworkbook.CreateFont();
            font1.Color = HSSFColor.Blue.Index;
            font1.Boldweight = (short)FontBoldWeight.Bold;
            font1.FontHeightInPoints = 10;

            Headerstyle = Hssfworkbook.CreateCellStyle();
            Headerstyle.SetFont(font1);
        }

        public static void CreateExcel(List<string> filePaths, string outputPath)
        {
            ISheet sheet1 = Hssfworkbook.CreateSheet("Logfile");
            sheet1.DefaultColumnWidth = 50;
            //Header'lar için row oluşturma
            IRow hrow = sheet1.CreateRow(0);

            var headerList = MainClass.Headers;
            var subHeaderList = SubClass.Headers;
            SetHeaders(hrow, headerList, subHeaderList);

            var ortakIpler = new List<OrtakIp>();
            // 0'da header'lar olduğundan rowCount 1 den başlar
            var rowCount = 1;
            var fixRowCount = 1;
            ISheet fixSheet = Hssfworkbook.CreateSheet("Check Qos");
            fixSheet.DefaultColumnWidth = 50;
            foreach (var filePath in filePaths)
            {
                StreamReader sw = new StreamReader(filePath);
                var mainString = sw.ReadToEnd();
                var parsedString = mainString.Split('#').ToList();
                var rnRemoved = parsedString.Remove("\r\n");
                var blackRemoved = parsedString.Remove("");
                var sysName = GetFolderName(parsedString.Single(x => x.ToLower().Contains("sysname")));
                sw.Close();
                var mainClasses = CreateMainClassList(parsedString);
                foreach (var mainClass in mainClasses)
                {
                    ortakIpler.Add(new OrtakIp(sysName, mainClass.Id, mainClass.Ip));
                    rowCount = SetMainClassRow(sheet1, headerList, subHeaderList, rowCount, sysName, mainClass);

                    foreach (var subClass in mainClass.SubClasses)
                    {
                        ortakIpler.Add(new OrtakIp(sysName, subClass.Id, subClass.IP));
                        rowCount = SetSubClassRow(sheet1, headerList, rowCount, sysName, subClass);
                    }

                    using (var fs = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        Hssfworkbook.Write(fs);
                    }
                }
                fixRowCount = CreateFixSheet(fixSheet, mainClasses, sysName, outputPath, fixRowCount);
            }
            ortakIpler = ortakIpler.Where(x => x.Ip != null).ToList();
            SetOrtakIp(ortakIpler.Where(x => !x.Ip.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)).ToList(), outputPath);
        }

        private static int CreateFixSheet(ISheet fixSheet, List<MainClass> mainClasses, string sysName, string outputPath, int rowCount)
        {
            //Header'lar için row oluşturma
            IRow hrow = fixSheet.CreateRow(0);
            string[] headers = new[] { "NE_Name", "Id", "Description" , "QOS_Profile_Name", "FIX_QOS_Description", "Check_QOS"};
            for (int i = 0; i < headers.Count(); i++)
            {
                hrow.CreateCell(i).SetCellValue(headers.ElementAt(i));
                hrow.GetCell(i).CellStyle = Headerstyle;
            }
            foreach (var mainClass in mainClasses)
            {
                foreach (var subClass in mainClass.SubClasses.Where(x => x.Description.Contains("FIX")))
                {
                    IRow subRow = fixSheet.CreateRow(rowCount++);
                    var subValues = new[] { sysName, subClass.Id, subClass.Description, subClass.QOS_Profile_Name, subClass.FIX_QOS_Description,
                                            subClass.Check_QOS.ToString()};
                    for (int i = 0; i < subValues.Count(); i++)
                    {
                        subRow.CreateCell(i).SetCellValue(subValues.ElementAt(i));
                    }
                }
            }

            using (var fs = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                Hssfworkbook.Write(fs);
            }
            return rowCount;
        }

        private static int SetSubClassRow(ISheet sheet1, string[] headerList, int rowCount, string sysName, SubClass subClass)
        {
            IRow subRow = sheet1.CreateRow(rowCount++);
            var subValues = new[] { subClass.Vlan_Type.ToNullString(), subClass.Vlan_Value.ToNullString(), subClass.Mtu.ToNullString(), subClass.IP.ToNullString(),
                                subClass.Subnet_Mask.ToNullString(), subClass.IP_Relay_Address.ToNullString(), subClass.IP_Binding.ToNullString(), subClass.Isis_Enable, subClass.Isis_Bdf,
                                subClass.Isis_Enable_Value.ToNullString(), subClass.Isis_Cost.ToNullString(), subClass.Isis_Enable_Level.ToNullString(),
                                subClass.Isis_Circuit_Type.ToNullString(), subClass.Isis_Circuit_Level.ToNullString(), subClass.Isis_Small,
                                subClass.Mpls_Enable, subClass.Mpls_Te_Enable, subClass.Mpls_Rsvp_Enable, subClass.Mpls_Mtu.ToNullString(), subClass.Trust_Upstream.ToNullString(),
                                subClass.Trust_Value.ToNullString(), subClass.L2vc_Id.ToNullString(), subClass.L2vc_Policy.ToNullString(),
                                subClass.QOS_Profile_Name };

            var cellNumber = headerList.Count();
            for (int i = 0; i < subValues.Count(); i++)
            {
                subRow.CreateCell(cellNumber).SetCellValue(subValues.ElementAt(i) == string.Empty ? "NULL" : subValues.ElementAt(i));
                cellNumber++;
            }

            subRow.CreateCell(0).SetCellValue(sysName == string.Empty ? "NULL" : sysName);
            subRow.CreateCell(1).SetCellValue(subClass.Id == string.Empty ? "NULL" : subClass.Id);
            subRow.CreateCell(2).SetCellValue(subClass.Description == string.Empty ? "NULL" : subClass.Description);


            for (int i = 3; i < headerList.Count(); i++)
            {
                subRow.CreateCell(i).SetCellValue("NULL");
            }
            subRow.CreateCell(3).SetCellValue("Disable");
            subRow.CreateCell(4).SetCellValue("Disable");
            subRow.CreateCell(11).SetCellValue("Disable");
            subRow.GetCell(5).SetCellValue(subClass.Trap_Input_Rate.ToString() == string.Empty ? "NULL" : subClass.Trap_Input_Rate.ToString());
            subRow.GetCell(6).SetCellValue(subClass.Trap_Input_Resure_Rate.ToString() == string.Empty ? "NULL" : subClass.Trap_Input_Resure_Rate.ToString());
            subRow.GetCell(7).SetCellValue(subClass.Trap_Output_Rate.ToString() == string.Empty ? "NULL" : subClass.Trap_Output_Rate.ToString());
            subRow.GetCell(8).SetCellValue(subClass.Trap_Output_Resure_Rate.ToString() == string.Empty ? "NULL" : subClass.Trap_Output_Resure_Rate.ToString());
            return rowCount;
        }

        private static int SetMainClassRow(ISheet sheet1, string[] headerList, string[] subHeaderList, int rowCount, string sysName, MainClass mainClass)
        {
            IRow row = sheet1.CreateRow(rowCount++);
            var values = new[] { sysName, mainClass.Id, mainClass.Description, mainClass.Shutdown, mainClass.Dcn, mainClass.Trap_Input_Rate.ToString(),
                    mainClass.Trap_Input_Resure_Rate.ToString(), mainClass.Trap_Output_Rate.ToString(), mainClass.Trap_Output_Resure_Rate.ToString(),
                    mainClass.Carrier_Time.ToString(), mainClass.Clock_Priority.ToString(), mainClass.Clock_Sync };

            for (int i = 0; i < values.Count(); i++)
                row.CreateCell(i).SetCellValue(values.ElementAt(i) == string.Empty ? "NULL" : values.ElementAt(i));

            for (int i = headerList.Count(); i < subHeaderList.Count() + headerList.Count(); i++)
            {
                row.CreateCell(i).SetCellValue("NULL");
            }
            row.GetCell(15).SetCellValue(mainClass.Ip);
            row.GetCell(26).SetCellValue("Disable");
            row.GetCell(27).SetCellValue("Disable");
            row.GetCell(28).SetCellValue("Disable");
            row.GetCell(29).SetCellValue("Disable");
            return rowCount;
        }

        private static void SetHeaders(IRow hrow, string[] headerList, string[] subHeaderList)
        {
            for (int i = 0; i < headerList.Count(); i++)
            {
                hrow.CreateCell(i).SetCellValue(headerList.ElementAt(i) == null ? "NULL" : headerList.ElementAt(i));
                hrow.GetCell(i).CellStyle = Headerstyle;
            }
            var maimHeaderCount = headerList.Count();
            for (int i = 0; i < subHeaderList.Count(); i++)
            {
                hrow.CreateCell(maimHeaderCount).SetCellValue(subHeaderList.ElementAt(i) == null ? "NULL" : subHeaderList.ElementAt(i));
                hrow.GetCell(maimHeaderCount).CellStyle = Headerstyle;
                maimHeaderCount++;
            }
        }

        private static string GetFolderName(string name)
        {
            var nameSplited = name.Split(' ');
            return nameSplited.LastOrDefault().Replace("\r\n", "");
        }

        private static void SetOrtakIp(List<OrtakIp> ortakIpler, string outputPath)
        {
            var dublicateIp = new List<OrtakIp>();
            foreach (var ortakIp in ortakIpler.ToArray())
            {
                if (ortakIpler.Count(x => x.Ip.Equals(ortakIp.Ip)) > 1)
                {
                    dublicateIp.Add(ortakIp);
                }
            }
            CreateExcelForCommonIp(dublicateIp, outputPath);
        }

        private static void CreateExcelForCommonIp(List<OrtakIp> dublicateIp, string outputPath)
        {
            ISheet sheet1 = Hssfworkbook.CreateSheet("Duplicate Ip");
            sheet1.DefaultColumnWidth = 50;

            IFont font1 = Hssfworkbook.CreateFont();
            font1.Color = HSSFColor.Blue.Index;
            font1.Boldweight = (short)FontBoldWeight.Bold;
            font1.FontHeightInPoints = 10;

            ICellStyle headerstyle = Hssfworkbook.CreateCellStyle();
            headerstyle.SetFont(font1);

            IRow hrow = sheet1.CreateRow(0);
            var headerList = new List<string>() { "Name", "Id", "Ip" };

            for (int i = 0; i < headerList.Count(); i++)
            {
                hrow.CreateCell(i).SetCellValue(headerList.ElementAt(i) == null ? "NULL" : headerList.ElementAt(i));
                hrow.GetCell(i).CellStyle = headerstyle;
            }

            var rowCount = 1;
            for (int i = 0; i < dublicateIp.Count(); i++)
            {
                IRow row = sheet1.CreateRow(rowCount++);
                var values = new[] { dublicateIp.ElementAt(i).Name, dublicateIp.ElementAt(i).Id, dublicateIp.ElementAt(i).Ip };

                for (int j = 0; j < values.Count(); j++)
                    row.CreateCell(j).SetCellValue(values.ElementAt(j) == string.Empty ? "NULL" : values.ElementAt(j));
            }
            using (var fs = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                Hssfworkbook.Write(fs);
            }
        }

        private static List<MainClass> CreateMainClassList(List<string> parsedString)
        {
            var mainClassStrings = GetMainClassesStrings(parsedString);
            var mainClasses = new List<MainClass>();
            //İnterface listesindeki her interface'i alıp,bizim MAİNCLASS'a atıyor.
            foreach (var inter in mainClassStrings)
            {
                string[] stringSeparators = new string[] { "\r\n" };
                var splitedItem = inter.Split(stringSeparators, StringSplitOptions.None).ToList();
                var mainClass = new MainClass(splitedItem);
                mainClass.SetClassProperties();
                mainClasses.Add(mainClass);
            }

            var subClassStrings = GetSubClasses(parsedString);
            foreach (var item in subClassStrings)
            {
                //Sub clasısın hangi main classa ait olduğunu buluyor
                var mainClassName = item.Split(' ').ElementAt(1).Split('.').FirstOrDefault();
                var mainClass = mainClasses.Single(x => x.Id.Equals(mainClassName, StringComparison.CurrentCultureIgnoreCase));
                
                //subclassı ait olduğu mainclassa ekleme kısmı
                string[] stringSeparators = new string[] { "\r\n" };
                var splitedItem = item.Split(stringSeparators, StringSplitOptions.None).ToList();
                var subClass = new SubClass(splitedItem);
                subClass.SetProperties();
                mainClass.SubClasses.Add(subClass);
            }
            return mainClasses;
        }

        //İnterface'lerin bir listesini oluşturuyor,yani interface ve Eth içerenleri alıp listesini oluşturuyor.
        private static List<string> GetMainClassesStrings(List<string> parsedString)
        {
            var interfaces = parsedString.Where(x => x.StartsWith("\r\ninterface") && x.Contains("Eth")).ToList();
            return interfaces.Where(x => !(x.Split(' ').FirstOrDefault(y => y.Contains("Eth")) as string).Contains(".")).ToList();
        }

        private static List<string> GetSubClasses(List<string> parsedString)
        {
            var interfaces = parsedString.Where(x => x.StartsWith("\r\ninterface") && x.Contains("Eth")).ToList();
            return interfaces.Where(x => (x.Split(' ').ElementAt(1).Contains("."))).ToList();
        }

        public static string ToNullString(this string value)
        {
            return value is null ? "NULL" : value;
        }

        public static string ToNullString(this int? value)
        {
            return value is null ? "NULL" : value.ToString();
        }
    }
}
