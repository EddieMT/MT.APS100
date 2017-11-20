using MT.APS100.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.IO;
using System.Linq;

namespace MT.APS100.Service
{
    public class DataService
    {
        private LotInfo lotInfo;
        private STDFService stdfService;
        private TransferMode transferMode;
        private FtpWeb ftpUpload;

        private string stdfPath = string.Empty;
        private string csvPath = string.Empty;
        private string csvsumPath = string.Empty;
        private string configPath = string.Empty;
        private string htmlPath = string.Empty;
        private string mtcsvPath = string.Empty;

        private string csvDirServer = string.Empty;
        private string singlefileDirServer = string.Empty;
        private string fulllotfileDirServer = string.Empty;
        private string stdtxtDirServer = string.Empty;
        private string compressedStdfPath = string.Empty;
        private string compressedMTcsvPath = string.Empty;

        public DataService(EnvironmentConfig enviConfig, LotInfo LotInfo, Recipe Recipe = null, string subcon = "")
        {
            lotInfo = LotInfo;
            lotInfo.SETUP_T = DateTime.Now;
            lotInfo.START_T = DateTime.Now;
            transferMode = enviConfig.TransferMode;

            ftpUpload = new FtpWeb(enviConfig.ServerDataIP.IP, string.Empty, enviConfig.ServerDataIP.User, enviConfig.ServerDataIP.Password);

            if (subcon == "JCET" && Recipe != null)
            {
                string tc = ConvertTestCode(lotInfo.TestCode.ToString(), Recipe.TestCodeConvertConfiguration.dictTestCode);
                string prefixDir = ApplyNameRule(Recipe.DatalogConfiguration.datalog_name_rule_1, subcon, tc);
                string prefixDirLocal = enviConfig.LocalDlogDir + prefixDir;
                string csvDirLocal = prefixDirLocal + "\\csvdir\\";
                CreateLocalDir(csvDirLocal);
                string singlefileDirLocal = prefixDirLocal + "\\SingleFile\\";
                CreateLocalDir(singlefileDirLocal);
                string fulllotfileDirLocal = prefixDirLocal + "\\FullLotFile\\";
                CreateLocalDir(fulllotfileDirLocal);
                string stdtxtDirLocal = prefixDirLocal + "\\STD_TXT\\";
                CreateLocalDir(stdtxtDirLocal);

                CreateServerDir(enviConfig.ServerDlogDir, prefixDir);

                string logname = ApplyNameRule(Recipe.DatalogConfiguration.datalog_name_rule_2, subcon, tc);
                stdfPath = singlefileDirLocal + logname + ".stdf";
                csvPath = csvDirLocal + logname + ".csv";
                configPath = csvDirLocal + lotInfo.ModeCode.ToString() + "_" + lotInfo.TesterID.ToString() + "_config.txt";
                htmlPath = fulllotfileDirLocal + logname + "_" + lotInfo.ModeCode.ToString() + ".html";
                csvsumPath = fulllotfileDirLocal + logname + "_" + lotInfo.ModeCode.ToString() + ".csv";
                mtcsvPath = singlefileDirLocal + logname + ".csv";
            }
            else
            {
                string dirLocal = Path.Combine(enviConfig.LocalDlogDir, lotInfo.ProgramName.ToString());
                CreateLocalDir(dirLocal);
                string dirLocalFullLot = Path.Combine(dirLocal, "FullLot");
                CreateLocalDir(dirLocalFullLot);

                string logname = lotInfo.ProgramName.ToString() + "_" + lotInfo.SubLotNo.ToString() + "_" + lotInfo.TestCode.ToString() + "_" + string.Format("{0:yyyyMMddHHmmss}", lotInfo.START_T);
                string logname_sum = lotInfo.ProgramName.ToString() + "_" + lotInfo.SubLotNo.ToString() + "_" + lotInfo.TestCode.ToString() + "_SUM_" + string.Format("{0:yyyyMMddHHmmss}", lotInfo.START_T);
                stdfPath = Path.Combine(dirLocal, logname + ".stdf");
                mtcsvPath = Path.Combine(dirLocal, logname + ".csv");
                csvPath = Path.Combine(dirLocal, logname_sum + ".csv");
                configPath = Path.Combine(dirLocal, lotInfo.TesterID.ToString() + "_sum_history.txt");
                htmlPath = Path.Combine(dirLocalFullLot, logname + ".html");
                csvsumPath = Path.Combine(dirLocalFullLot, logname + ".csv");
            }

            stdfService = new STDFService(stdfPath);
        }

        private string ApplyNameRule(string nameRule, string subcon, string testcode)
        {
            return nameRule.Replace("{Customer_LotNo}", lotInfo.CustomerLotNo.ToString())
                            .Replace("{Sub_LotNo}", lotInfo.SubLotNo.ToString())
                            .Replace("{Test_BinNo}", lotInfo.TestBinNo.ToString())
                            .Replace("{Test_Code}", testcode)
                            .Replace("{Start_Time}", string.Format("{0:yyyyMMddHHmmss}", lotInfo.START_T))
                            .Replace("{Subcon}", subcon)
                            .Replace("{Tester_ID}", lotInfo.TesterID.ToString())
                            .Replace("{Mode_Code}", lotInfo.ModeCode.ToString())
                            .Replace("{Device_Name}", lotInfo.DeviceName.ToString())
                            .Replace("{Program_Name}", lotInfo.ProgramName.ToString())
                            .Replace("{Customer_ID}", lotInfo.CustomerID.ToString());
        }

        private string ConvertTestCode(string testCode, Dictionary<string, string> dict)
        {
            if (dict.Any(x => x.Key == testCode))
                return dict[testCode];
            else
                return testCode;
        }

        public void SaveHeader()
        {
            stdfService.SaveHeader(lotInfo);
        }

        public void SaveDetails(List<PartResult> partResults)
        {
            stdfService.SaveBody(partResults);
        }

        public void SaveSummary(List<PartResult> fullPartResults, List<TestLimit> limits)
        {
            lotInfo.FINISH_T = DateTime.Now;

            CreateProject.Header = lotInfo;
            CreateProject.Summary = fullPartResults;
            CreateProject.CreateCSV(mtcsvPath, limits);
            CreateProject.CSVExport(csvPath, limits);

            stdfService.SaveFooter(fullPartResults, lotInfo.FINISH_T);
            stdfService.Dispose();
        }

        public bool CheckSTDFExist()
        {
            if (!File.Exists(stdfPath))
                return false;

            return true;
        }

        public bool CheckCSVExist()
        {
            var list = GetCSVFiles();

            if (list.Count > 0)
            {
                foreach (var path in list)
                {
                    if (!File.Exists(path))
                        return false;
                }
            }
            else
                return false;

            return true;
        }

        public bool CheckConfigExist()
        {
            if (!File.Exists(configPath))
                return false;

            return true;
        }

        public void LoadCSV()
        {

        }

        public void LoadConfig()
        {

        }

        public void CompressFile(ZipMode zipMode)
        {
            using (ZipService zipService = new ZipService())
            {
                if (zipMode == ZipMode.zip)
                {
                    compressedStdfPath = stdfPath + ".zip";
                    zipService.CompressFile(stdfPath, compressedStdfPath, "zip");

                    compressedMTcsvPath = mtcsvPath + ".zip";
                    zipService.CompressFile(mtcsvPath, compressedMTcsvPath, "zip");
                }
                else if (zipMode == ZipMode.gz)
                {
                    compressedStdfPath = stdfPath + ".gz";
                    zipService.CompressFile(stdfPath, compressedStdfPath, "gzip");

                    compressedMTcsvPath = mtcsvPath + ".gz";
                    zipService.CompressFile(mtcsvPath, compressedMTcsvPath, "gzip");
                }
                else if (zipMode == ZipMode.tgz)
                {
                    string tarPath = stdfPath + ".tar";
                    zipService.CompressFile(stdfPath, tarPath, "tar");
                    compressedStdfPath = tarPath + ".gz";
                    zipService.CompressFile(tarPath, compressedStdfPath, "gzip");

                    tarPath = mtcsvPath + ".tar";
                    zipService.CompressFile(mtcsvPath, tarPath, "tar");
                    compressedMTcsvPath = tarPath + ".gz";
                    zipService.CompressFile(tarPath, compressedMTcsvPath, "gzip");
                }
            }
        }

        public void SaveConfig()
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(configPath, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    text = @"[Header Info {TestCode}]
TesterOS_Version:    {TesterOSVersion}
Start_Time:    {START_T}
End_Time:    {FINISH_T}
Customer_LotNo:    {CustomerLotNo}
Program_Name:    {ProgramName}
Sub_LotNo:    {SubLotNo}
Device_Name:    {DeviceName}
Test_Code:    {TestCode}
Mode_Code:    {ModeCode}
Operator_ID:    {OperatorID}
Test_BinNo:    {TestBinNo}
Tester_ID:    {TesterID}
Customer_ID:    {CustomerID}
CSV File:   {csvPath}";

                    text = text.Replace("{TestCode}", lotInfo.TestCode.ToString());
                    text = text.Replace("{TesterOSVersion}", lotInfo.TesterOSVersion);
                    text = text.Replace("{START_T}", string.Format("{0:yyyyMMddHHmmss}", lotInfo.START_T));
                    text = text.Replace("{FINISH_T}", string.Format("{0:yyyyMMddHHmmss}", lotInfo.FINISH_T));
                    text = text.Replace("{CustomerLotNo}", lotInfo.CustomerLotNo.ToString());
                    text = text.Replace("{ProgramName}", lotInfo.ProgramName.ToString());
                    text = text.Replace("{SubLotNo}", lotInfo.SubLotNo.ToString());
                    text = text.Replace("{DeviceName}", lotInfo.DeviceName.ToString());
                    text = text.Replace("{TestCode}", lotInfo.TestCode.ToString());
                    text = text.Replace("{ModeCode}", lotInfo.ModeCode.ToString());
                    text = text.Replace("{OperatorID}", lotInfo.OperatorID.ToString());
                    text = text.Replace("{TestBinNo}", lotInfo.TestBinNo.ToString());
                    text = text.Replace("{TesterID}", lotInfo.TesterID.ToString());
                    text = text.Replace("{CustomerID}", lotInfo.CustomerID.ToString());
                    text = text.Replace("{csvPath}", csvPath);

                    sw.Write(text);
                    sw.WriteLine();
                }
            }
        }

        public void SaveFullLot(Dictionary<LotInfo, List<PartResult>> fulllot, List<TestLimit> limits)
        {
            HTMLBuilder(fulllot, limits);

            CSVBuilder(fulllot, limits);
        }

        private void HTMLBuilder(Dictionary<LotInfo, List<PartResult>> fulllot, List<TestLimit> limits)
        {
            int totalTested = 0;
            StringBuilder strHTMLBuilder = new StringBuilder();
            strHTMLBuilder.Append("<html>");
            strHTMLBuilder.Append("<head>");
            strHTMLBuilder.Append("</head>");
            strHTMLBuilder.Append("<body>");
            strHTMLBuilder.Append("<p>");
            strHTMLBuilder.Append("Header Info");
            strHTMLBuilder.Append("</p>");
            ExportDatatableToHtml(strHTMLBuilder, HTMLHeaderInfoBuilder(fulllot));
            strHTMLBuilder.Append("<p>");
            strHTMLBuilder.Append("HardBins");
            strHTMLBuilder.Append("</p>");
            ExportDatatableToHtml(strHTMLBuilder, HTMLHardBinsBuilder(fulllot, limits, ref totalTested));
            strHTMLBuilder.Append("<p>");
            strHTMLBuilder.Append("Total Tested:" + totalTested.ToString());
            strHTMLBuilder.Append("</p>");
            strHTMLBuilder.Append("<p>");
            strHTMLBuilder.Append("SoftBins");
            strHTMLBuilder.Append("</p>");
            ExportDatatableToHtml(strHTMLBuilder, HTMLSoftBinsBuilder(fulllot, limits, totalTested));
            strHTMLBuilder.Append("<p>");
            strHTMLBuilder.Append("Total Tested:" + totalTested.ToString());
            strHTMLBuilder.Append("</p>");
            strHTMLBuilder.Append("</body>");
            strHTMLBuilder.Append("</html>");

            string Htmltext = strHTMLBuilder.ToString();

            File.WriteAllText(htmlPath, Htmltext);
        }

        private DataTable HTMLHeaderInfoBuilder(Dictionary<LotInfo, List<PartResult>> fulllot)
        {
            DataTable dtHeaderInfo = new DataTable();

            dtHeaderInfo.Columns.Add("ConfigurationOptions", typeof(string));
            foreach (var lot in fulllot)
            {
                dtHeaderInfo.Columns.Add(lot.Key.TestCode.ToString(), typeof(string));
            }

            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "TesterOS_Version"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Customer_LotNo"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Test_Temp"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Wafer_Version"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Program_Name"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Start_Time"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Sub_LotNo"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Device_Name"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Test_Code"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Mode_Code"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Operator_ID"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Test_BinNo"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Tester_ID"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Customer_ID"));
            dtHeaderInfo.Rows.Add(HTMLHeaderInfoBuilder(fulllot, "Mother_Fab"));

            return dtHeaderInfo;
        }

        private object[] HTMLHeaderInfoBuilder(Dictionary<LotInfo, List<PartResult>> fulllot, string option)
        {
            List<object> values = new List<object>();
            values.Add(option);
            foreach (var lot in fulllot)
            {
                if (option == "ConfigurationOptions")
                    values.Add(lot.Key.TestCode.ToString());
                else if (option == "TesterOS_Version")
                    values.Add(lot.Key.TesterOSVersion);
                else if (option == "Customer_LotNo")
                    values.Add(lot.Key.CustomerLotNo.ToString());
                else if (option == "Test_Temp")
                    values.Add(lot.Key.TST_TEMP);
                else if (option == "Wafer_Version")
                    values.Add(lot.Key.WaferVersion.ToString());
                else if (option == "Program_Name")
                    values.Add(lot.Key.ProgramName.ToString());
                else if (option == "Start_Time")
                    values.Add(string.Format("{0:yyyyMMdd_HHmmss}", lot.Key.START_T));
                else if (option == "Sub_LotNo")
                    values.Add(lot.Key.SubLotNo.ToString());
                else if (option == "Device_Name")
                    values.Add(lot.Key.DeviceName.ToString());
                else if (option == "Test_Code")
                    values.Add(lot.Key.TestCode.ToString());
                else if (option == "Mode_Code")
                    values.Add(lot.Key.ModeCode.ToString());
                else if (option == "Operator_ID")
                    values.Add(lot.Key.OperatorID.ToString());
                else if (option == "Test_BinNo")
                    values.Add(lot.Key.TestBinNo.ToString());
                else if (option == "Tester_ID")
                    values.Add(lot.Key.TesterID.ToString());
                else if (option == "Customer_ID")
                    values.Add(lot.Key.CustomerID.ToString());
                else if (option == "Mother_Fab")
                    values.Add(lot.Key.MotherFab.ToString());
            }
            return values.ToArray();
        }

        private DataTable HTMLHardBinsBuilder(Dictionary<LotInfo, List<PartResult>> fulllot, List<TestLimit> limits, ref int totalTested)
        {
            var grpHBlimits = limits.GroupBy(x => new { x.HardBinName, x.HardBinNumber, x.HardBinPF });
            DataTable dtHardBins = new DataTable();

            dtHardBins.Columns.Add("BinName", typeof(string));
            dtHardBins.Columns.Add("BinID", typeof(string));
            foreach (var lot in fulllot)
            {
                dtHardBins.Columns.Add(lot.Key.TestCode.ToString(), typeof(string));
            }
            dtHardBins.Columns.Add("Final", typeof(string));
            dtHardBins.Columns.Add("Percent", typeof(string));

            int[] total = new int[fulllot.Count + 1];

            List<List<object>> data = new List<List<object>>();
            foreach (var limit in grpHBlimits)
            {
                List<object> values = new List<object>();
                values.Add(limit.Key.HardBinName);
                values.Add(limit.Key.HardBinNumber);
                int final = 0;
                int index = 0;
                string lastKey = string.Empty;
                foreach (var lot in fulllot)
                {
                    string currentKey = lot.Key.TestCode.ToString().Split('.')[0];
                    int count = lot.Value.Count(x => x.HardBin.BinName == limit.Key.HardBinName);
                    values.Add(count);
                    total[index] += count;
                    index++;
                    if (limit.Key.HardBinPF == BinPassFail.Pass.ToDescription())
                        final += count;
                    else
                    {
                        if (currentKey != lastKey)
                            final = count;
                        else
                            final += count;
                    }
                    lastKey = currentKey;
                }
                values.Add(final);
                total[index] += final;
                index = 0;
                data.Add(values);
            }

            totalTested = total[total.Length - 1];

            foreach (var row in data)
            {
                double percent = (totalTested == 0) ? 0 : (int)row[row.Count - 1] / (double)totalTested * 100;
                row.Add(string.Format("{0:N3}%", percent));
                dtHardBins.Rows.Add(row.ToArray());
            }

            List<object> rowTotal = new List<object>();
            rowTotal.Add("");
            rowTotal.Add("Total");
            for (int i = 0; i < total.Length; i++)
            {
                rowTotal.Add(total[i]);
            }
            rowTotal.Add("");
            dtHardBins.Rows.Add(rowTotal.ToArray());

            return dtHardBins;
        }

        private DataTable HTMLSoftBinsBuilder(Dictionary<LotInfo, List<PartResult>> fulllot, List<TestLimit> limits, int totalTested)
        {
            var grpSBlimits = limits.GroupBy(x => new { x.SoftBinName, x.SoftBinNumber, x.SoftBinPF });
            DataTable dtSoftBins = new DataTable();

            dtSoftBins.Columns.Add("BinName", typeof(string));
            dtSoftBins.Columns.Add("BinID", typeof(string));
            foreach (var lot in fulllot)
            {
                dtSoftBins.Columns.Add(lot.Key.TestCode.ToString(), typeof(string));
            }
            dtSoftBins.Columns.Add("Final", typeof(string));
            dtSoftBins.Columns.Add("Percent", typeof(string));

            List<List<object>> data = new List<List<object>>();
            foreach (var limit in grpSBlimits)
            {
                List<object> values = new List<object>();
                values.Add(limit.Key.SoftBinName);
                values.Add(limit.Key.SoftBinNumber);
                int final = 0;
                string lastKey = string.Empty;
                foreach (var lot in fulllot)
                {
                    string currentKey = lot.Key.TestCode.ToString().Split('.')[0];
                    int count = lot.Value.Count(x => x.SoftBin.BinName == limit.Key.SoftBinName);
                    values.Add(count);
                    if (limit.Key.SoftBinPF == BinPassFail.Pass.ToDescription())
                        final += count;
                    else
                    {
                        if (currentKey != lastKey)
                            final = count;
                        else
                            final += count;
                    }
                    lastKey = currentKey;
                }
                values.Add(final);
                data.Add(values);
            }

            foreach (var row in data)
            {
                double percent = (totalTested == 0) ? 0 : (int)row[row.Count - 1] / (double)totalTested * 100;
                row.Add(string.Format("{0:N3}%", percent));
                dtSoftBins.Rows.Add(row.ToArray());
            }

            return dtSoftBins;
        }

        private void ExportDatatableToHtml(StringBuilder strHTMLBuilder, DataTable dt)
        {
            //strHTMLBuilder.Append("<table border='1px' cellpadding='1' cellspacing='1' bgcolor='lightyellow' style='font-family:Garamond; font-size:smaller'>");
            strHTMLBuilder.Append("<table border='1'>");
            strHTMLBuilder.Append("<tr>");
            foreach (DataColumn myColumn in dt.Columns)
            {
                strHTMLBuilder.Append("<td>");
                strHTMLBuilder.Append(myColumn.ColumnName);
                strHTMLBuilder.Append("</td>");

            }
            strHTMLBuilder.Append("</tr>");

            foreach (DataRow myRow in dt.Rows)
            {
                strHTMLBuilder.Append("<tr>");
                foreach (DataColumn myColumn in dt.Columns)
                {
                    strHTMLBuilder.Append("<td>");
                    strHTMLBuilder.Append(myRow[myColumn.ColumnName].ToString());
                    strHTMLBuilder.Append("</td>");

                }
                strHTMLBuilder.Append("</tr>");
            }
            strHTMLBuilder.Append("</table>");
        }

        private void CSVBuilder(Dictionary<LotInfo, List<PartResult>> fulllot, List<TestLimit> limits)
        {
            Dictionary<string, List<PartResult>> fulllotcopy = new Dictionary<string, List<PartResult>>();
            foreach (var lot in fulllot)
            {
                string lotKey = lot.Key.TestCode.ToString().Split('.')[0];
                if (fulllotcopy.Any(x => x.Key == lotKey))
                {
                    fulllotcopy[lotKey].AddRange(lot.Value);
                }
                else
                {
                    fulllotcopy.Add(lotKey, lot.Value);
                }
            }

            CSVHardBinsBuilder(fulllotcopy, limits);

            CSVSoftBinsBuilder(fulllotcopy, limits);
        }

        private void CSVHardBinsBuilder(Dictionary<string, List<PartResult>> fulllot, List<TestLimit> limits)
        {
            DataTable dtHardBins = new DataTable();

            dtHardBins.Columns.Add("HBINID", typeof(string));
            foreach (var lot in fulllot)
            {
                dtHardBins.Columns.Add(lot.Key, typeof(string));
            }
            dtHardBins.Columns.Add("Final", typeof(string));

            var grpHBlimits = limits.GroupBy(x => new { x.HardBinName, x.HardBinNumber, x.HardBinPF });

            foreach (var limit in grpHBlimits)
            {
                List<object> values = new List<object>();
                values.Add(limit.Key.HardBinNumber);
                int final = 0;
                foreach (var lot in fulllot)
                {
                    int count = lot.Value.Count(x => x.HardBin.BinName == limit.Key.HardBinName);
                    values.Add(count);
                    if (limit.Key.HardBinPF == BinPassFail.Pass.ToDescription())
                        final += count;
                    else
                        final = count;
                }
                values.Add(final);
                dtHardBins.Rows.Add(values.ToArray());
            }

            SaveCSV(dtHardBins, FileMode.Create);
        }

        private void CSVSoftBinsBuilder(Dictionary<string, List<PartResult>> fulllot, List<TestLimit> limits)
        {
            DataTable dtSoftBins = new DataTable();

            dtSoftBins.Columns.Add("SBINID", typeof(string));
            foreach (var lot in fulllot)
            {
                dtSoftBins.Columns.Add(lot.Key.ToString(), typeof(string));
            }
            dtSoftBins.Columns.Add("Final", typeof(string));

            var grpSBlimits = limits.GroupBy(x => new { x.SoftBinName, x.SoftBinNumber, x.SoftBinPF });

            foreach (var limit in grpSBlimits)
            {
                List<object> values = new List<object>();
                values.Add(limit.Key.SoftBinNumber);
                int final = 0;
                foreach (var lot in fulllot)
                {
                    int count = lot.Value.Count(x => x.SoftBin.BinName == limit.Key.SoftBinName);
                    values.Add(count);
                    if (limit.Key.SoftBinPF == BinPassFail.Pass.ToDescription())
                        final += count;
                    else
                        final = count;
                }
                values.Add(final);
                dtSoftBins.Rows.Add(values.ToArray());
            }

            SaveCSV(dtSoftBins, FileMode.Append);
        }

        private void SaveCSV(DataTable dt, FileMode fileMode)
        {
            using (FileStream fs = new FileStream(csvsumPath, fileMode, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.WriteLine();

                    string data = "";
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        data += dt.Columns[i].ColumnName.ToString();
                        if (i < dt.Columns.Count - 1)
                        {
                            data += ",";
                        }
                    }
                    sw.WriteLine(data);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data = "";
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            data += dt.Rows[i][j].ToString();
                            if (j < dt.Columns.Count - 1)
                            {
                                data += ",";
                            }
                        }
                        sw.WriteLine(data);
                    }
                }
            }
        }

        public void CreateLocalDir(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public void CreateServerDir(string serverDir, string prefixDir)
        {
            if (transferMode == TransferMode.FTP)
            {
                prefixDir = prefixDir.Replace('\\', '/');
                ftpUpload.MakeDirs(prefixDir);

                ftpUpload.GotoDirectory(prefixDir, true);
                csvDirServer = serverDir + prefixDir + "/csvdir/";
                if (!ftpUpload.DirectoryExist("csvdir"))
                    ftpUpload.MakeDir("csvdir");
                singlefileDirServer = serverDir + prefixDir + "/SingleFile/";
                if (!ftpUpload.DirectoryExist("SingleFile"))
                    ftpUpload.MakeDir("SingleFile");
                fulllotfileDirServer = serverDir + prefixDir + "/FullLotFile/";
                if (!ftpUpload.DirectoryExist("FullLotFile"))
                    ftpUpload.MakeDir("FullLotFile");
                stdtxtDirServer = serverDir + prefixDir + "/STD_TXT/";
                if (!ftpUpload.DirectoryExist("STD_TXT"))
                    ftpUpload.MakeDir("STD_TXT");
            }
            else
            {
                csvDirServer = serverDir + prefixDir + "\\csvdir\\";
                if (!Directory.Exists(csvDirServer))
                    Directory.CreateDirectory(csvDirServer);
                singlefileDirServer = serverDir + prefixDir + "\\SingleFile\\";
                if (!Directory.Exists(singlefileDirServer))
                    Directory.CreateDirectory(singlefileDirServer);
                fulllotfileDirServer = serverDir + prefixDir + "\\FullLotFile\\";
                if (!Directory.Exists(fulllotfileDirServer))
                    Directory.CreateDirectory(fulllotfileDirServer);
                stdtxtDirServer = serverDir + prefixDir + "\\STD_TXT\\";
                if (!Directory.Exists(stdtxtDirServer))
                    Directory.CreateDirectory(stdtxtDirServer);
            }
        }

        public bool TryConnect()
        {
            if (transferMode == TransferMode.FTP)
            {
                bool isFTPConnect = false;
                for (int i = 0; i < 3; i++)
                {
                    if (ftpUpload.Connect())
                    {
                        isFTPConnect = true;
                        break;
                    }
                }
                return isFTPConnect;
            }
            else
                return true;
        }

        public void UploadData()
        {
            if (transferMode == TransferMode.FTP)
            {
                ftpUpload.Upload2(singlefileDirServer + Path.GetFileName(stdfPath), stdfPath);
                ftpUpload.Upload2(singlefileDirServer + Path.GetFileName(compressedStdfPath), compressedStdfPath);

                ftpUpload.Upload2(singlefileDirServer + Path.GetFileName(mtcsvPath), mtcsvPath);
                ftpUpload.Upload2(singlefileDirServer + Path.GetFileName(compressedMTcsvPath), compressedMTcsvPath);
            }
            else
            {
                File.Copy(stdfPath, singlefileDirServer + Path.GetFileName(stdfPath), true);
                File.Copy(compressedStdfPath, singlefileDirServer + Path.GetFileName(compressedStdfPath), true);

                File.Copy(mtcsvPath, singlefileDirServer + Path.GetFileName(mtcsvPath), true);
                File.Copy(compressedMTcsvPath, singlefileDirServer + Path.GetFileName(compressedMTcsvPath), true);
            }
        }

        public void UploadFulllot()
        {
            var list = GetCSVFiles();
            if (transferMode == TransferMode.FTP)
            {
                foreach(var path in list)
                {
                    ftpUpload.Upload2(csvDirServer + Path.GetFileName(path), path);
                }
                ftpUpload.Upload2(csvDirServer + Path.GetFileName(configPath), configPath);
                ftpUpload.Upload2(fulllotfileDirServer + Path.GetFileName(htmlPath), htmlPath);
                ftpUpload.Upload2(fulllotfileDirServer + Path.GetFileName(csvsumPath), csvsumPath);
            }
            else
            {
                foreach (var path in list)
                {
                    File.Copy(path, csvDirServer + Path.GetFileName(path), true);
                }
                File.Copy(configPath, csvDirServer + Path.GetFileName(configPath), true);
                File.Copy(htmlPath, fulllotfileDirServer + Path.GetFileName(htmlPath), true);
                File.Copy(csvsumPath, fulllotfileDirServer + Path.GetFileName(csvsumPath), true);
            }
        }

        private List<string> GetCSVFiles()
        {
            List<string> list = new List<string>();

            using (StreamReader sr = new StreamReader(configPath))
            {
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!line.Contains("CSV File:"))
                        continue;

                    list.Add(line.Replace("CSV File:", "").TrimStart().TrimEnd());
                }
                sr.Close();
            }

            return list;
        }

        public string GetDataLocation()
        {
            return Path.GetFullPath(stdfPath);
        }

        public string GetSumDataLocation()
        {
            return Path.GetFullPath(htmlPath);
        }
    }
}