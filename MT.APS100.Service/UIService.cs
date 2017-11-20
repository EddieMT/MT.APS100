using MT.APS100.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MT.APS100.Service
{
    public class UIService
    {
        private string uiLogPath { get; set; }
        private EnvironmentConfig enviConfig;
        private FtpWeb ftpDownload;

        public UIService()
        {
            try
            {
                enviConfig = GetEnviConfig();
            }
            catch
            {
                throw new FileLoadException(string.Format("Invalid environment config file '{0}', please contact the developer!", FileStructure.ENVIRONMENT_CONFIG_FILE_PATH));
            }
            uiLogPath = Path.Combine(enviConfig.ProductionWorkspace, "UILog", string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".log");
            CreateDir(Path.GetDirectoryName(uiLogPath));

            ftpDownload = new FtpWeb(enviConfig.ServerProgIP.IP, string.Empty, enviConfig.ServerProgIP.User, enviConfig.ServerProgIP.Password);
        }

        public EnvironmentConfig GetEnviConfig()
        {
            if (enviConfig == null)
            {
                if (!File.Exists(FileStructure.ENVIRONMENT_CONFIG_FILE_PATH))
                    throw new Exception("Environmentconfig file is missing, please contact the developer!");

                enviConfig = new EnvironmentConfig();

                using (StreamReader sr = new StreamReader(FileStructure.ENVIRONMENT_CONFIG_FILE_PATH))
                {
                    string line = string.Empty;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("#"))
                            continue;

                        if (line.ToLower().Contains("transfermode") && line.Contains("="))
                        {
                            if (line.Split('=')[1].Trim() == TransferMode.FTP.ToString())
                                enviConfig.TransferMode = TransferMode.FTP;
                            else if (line.Split('=')[1].Trim() == TransferMode.Server.ToString())
                                enviConfig.TransferMode = TransferMode.Server;
                            else if (line.Split('=')[1].Trim() == TransferMode.Default.ToString())
                                enviConfig.TransferMode = TransferMode.Default;
                        }

                        if (line.ToLower().Contains("productionworkspace") && line.Contains("="))
                        {
                            enviConfig.ProductionWorkspace = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("server_data_ip") && line.Contains("=") && !line.ToLower().Contains("{server_data_ip}"))
                        {
                            enviConfig.ServerDataIP.IP = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("server_data_user") && line.Contains("="))
                        {
                            enviConfig.ServerDataIP.User = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("server_data_password") && line.Contains("="))
                        {
                            enviConfig.ServerDataIP.Password = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("server_pgm_ip") && line.Contains("=") && !line.ToLower().Contains("{server_pgm_ip}"))
                        {
                            enviConfig.ServerProgIP.IP = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("server_pgm_user") && line.Contains("="))
                        {
                            enviConfig.ServerProgIP.User = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("server_pgm_password") && line.Contains("="))
                        {
                            enviConfig.ServerProgIP.Password = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("local_dlog_dir") && line.Contains("="))
                        {
                            enviConfig.LocalDlogDir = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("local_prog_dir") && line.Contains("="))
                        {
                            enviConfig.LocalProgDir = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("server_dlog_dir") && line.Contains("="))
                        {
                            enviConfig.ServerDlogDir = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("server_prog_dir") && line.Contains("="))
                        {
                            enviConfig.ServerProgDir = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("server_mesfile_dir") && line.Contains("="))
                        {
                            enviConfig.ServerMESFileDir = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("tester_type") && line.Contains("=") && !line.ToLower().Contains("{tester_type}"))
                        {
                            enviConfig.TesterType = line.Split('=')[1].Trim();
                        }

                        if (line.ToLower().Contains("ui_clear_pgm_flag") && line.Contains("="))
                        {
                            enviConfig.UIClearProg = line.Split('=')[1].Trim() == "1" ? true : false;
                        }

                        if (line.ToLower().Contains("start_clear_pgm_flag") && line.Contains("="))
                        {
                            enviConfig.StartClearProg = line.Split('=')[1].Trim() == "1" ? true : false;
                        }

                        if (line.ToLower().Contains("exit_clear_pgm_flag") && line.Contains("="))
                        {
                            enviConfig.ExitClearProg = line.Split('=')[1].Trim() == "1" ? true : false;
                        }

                        if (line.ToLower().Contains("alarm_m01_flag") && line.Contains("="))
                        {
                            enviConfig.AlarmM01 = line.Split('=')[1].Trim() == "1" ? true : false;
                        }

                        if (line.ToLower().Contains("alarm_m02_flag") && line.Contains("="))
                        {
                            enviConfig.AlarmM02 = line.Split('=')[1].Trim() == "1" ? true : false;
                        }

                        if (line.ToLower().Contains("alarm_m03_flag") && line.Contains("="))
                        {
                            enviConfig.AlarmM03 = line.Split('=')[1].Trim() == "1" ? true : false;
                        }

                        if (line.ToLower().Contains("alarm_m04_flag") && line.Contains("="))
                        {
                            enviConfig.AlarmM04 = line.Split('=')[1].Trim() == "1" ? true : false;
                        }
                    }
                    sr.Close();
                }

                enviConfig.LocalProgDir = Path.Combine(enviConfig.ProductionWorkspace, enviConfig.LocalProgDir.TrimStart('/').TrimStart('\\').Replace('/', '\\'));
                enviConfig.LocalDlogDir = Path.Combine(enviConfig.ProductionWorkspace, enviConfig.LocalDlogDir.TrimStart('/').TrimStart('\\').Replace('/', '\\'));

                if (enviConfig.ServerDlogDir.ToLower().Contains("{server_data_ip}"))
                {
                    string oldValue = "{server_data_ip}";
                    int startIndex = enviConfig.ServerDlogDir.ToLower().IndexOf(oldValue);
                    enviConfig.ServerDlogDir = enviConfig.ServerDlogDir.Remove(startIndex, oldValue.Length);
                    enviConfig.ServerDlogDir = enviConfig.ServerDlogDir.Insert(startIndex, enviConfig.ServerDataIP.IP);
                }

                if (enviConfig.ServerProgDir.ToLower().Contains("{server_pgm_ip}"))
                {
                    string oldValue = "{server_pgm_ip}";
                    int startIndex = enviConfig.ServerProgDir.ToLower().IndexOf(oldValue);
                    enviConfig.ServerProgDir = enviConfig.ServerProgDir.Remove(startIndex, oldValue.Length);
                    enviConfig.ServerProgDir = enviConfig.ServerProgDir.Insert(startIndex, enviConfig.ServerProgIP.IP);
                }

                if (enviConfig.ServerProgDir.ToLower().Contains("{tester_type}"))
                {
                    string oldValue = "{tester_type}";
                    int startIndex = enviConfig.ServerProgDir.ToLower().IndexOf(oldValue);
                    enviConfig.ServerProgDir = enviConfig.ServerProgDir.Remove(startIndex, oldValue.Length);
                    enviConfig.ServerProgDir = enviConfig.ServerProgDir.Insert(startIndex, enviConfig.TesterType);
                }

                if (enviConfig.ServerMESFileDir.ToLower().Contains("{server_pgm_ip}"))
                {
                    string oldValue = "{server_pgm_ip}";
                    int startIndex = enviConfig.ServerMESFileDir.ToLower().IndexOf(oldValue);
                    enviConfig.ServerMESFileDir = enviConfig.ServerMESFileDir.Remove(startIndex, oldValue.Length);
                    enviConfig.ServerMESFileDir = enviConfig.ServerMESFileDir.Insert(startIndex, enviConfig.ServerProgIP.IP);
                }

                if (enviConfig.TransferMode == TransferMode.Default || enviConfig.TransferMode == TransferMode.Server)
                {
                    if (enviConfig.ServerDlogDir.FirstOrDefault() == '/')
                        enviConfig.ServerDlogDir = enviConfig.ServerDlogDir.Substring(1);
                    if (enviConfig.ServerProgDir.FirstOrDefault() == '/')
                        enviConfig.ServerProgDir = enviConfig.ServerProgDir.Substring(1);
                    if (enviConfig.ServerMESFileDir.FirstOrDefault() == '/')
                        enviConfig.ServerMESFileDir = enviConfig.ServerMESFileDir.Substring(1);
                }
            }

            return enviConfig;
        }

        public Dictionary<string, List<string>> GetPasswordList()
        {
            string passwordFilePath = Path.Combine(enviConfig.ProductionWorkspace, "password", "password.txt");
            ClearDir(Path.GetDirectoryName(passwordFilePath));
            CreateDir(Path.GetDirectoryName(passwordFilePath));
            if (enviConfig.TransferMode == TransferMode.FTP)
            {
                if (!ftpDownload.FileExists(enviConfig.ServerProgDir + @"/password/password.txt"))
                {
                    throw new Exception("Password file is missing, please contact the developer!");
                }
                ftpDownload.Download(enviConfig.ServerProgDir + @"/password/password.txt", passwordFilePath);
            }
            else
            {
                if (!File.Exists(enviConfig.ServerProgDir + @"\password\password.txt"))
                {
                    throw new Exception("Password file is missing, please contact the developer!");
                }
                File.Copy(enviConfig.ServerProgDir + @"\password\password.txt", passwordFilePath, true);
            }

            Dictionary<string, List<string>> dictPassword = new Dictionary<string, List<string>>();

            using (StreamReader sr = new StreamReader(passwordFilePath))
            {
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;

                    string[] s = line.Split('=');
                    if (s.Length != 2)
                        continue;

                    if (dictPassword.Keys.Contains(s[0].Trim()))
                    {
                        dictPassword[s[0].Trim()].Add(s[1].Trim());
                    }
                    else
                    {
                        dictPassword.Add(s[0].Trim(), new List<string>() { s[1].Trim() });
                    }
                }
                sr.Close();
            }

            return dictPassword;
        }

        public List<BasicInfo> GetBasicInfoFromBarcodeFile(string barcodeFilePath)
        {
            List<BasicInfo> list = new List<BasicInfo>();

            using (StreamReader sr = new StreamReader(barcodeFilePath))
            {
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;

                    if (line == string.Empty)
                        continue;

                    string[] s = line.Split(',');

                    BasicInfo item = new BasicInfo();
                    item.ProgramName = s[0].Trim();
                    item.ZipFile = s[1].Trim();
                    item.ProgramFolder = s[2].Trim();
                    item.TestFlow = s[3].Trim();
                    item.TesterOSVersion = s[4].Trim();
                    item.ModeCode = s[5].Trim();
                    if (s.Length != 6)
                        continue;
                    list.Add(item);
                }
                sr.Close();
            }

            return list;
        }

        public BasicInfo GetBasicInfoFromMESFile(string mesFilePath)
        {
            BasicInfo item = new BasicInfo();

            using (StreamReader sr = new StreamReader(mesFilePath))
            {
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;

                    string[] s = line.Split(':');

                    if (s[0].Trim() == "Sub_LotNo")
                        item.SubLotNo = s[1].Trim();

                    if (s[0].Trim() == "Customer_LotNo")
                        item.CustomerLotNo = s[1].Trim();

                    if (s[0].Trim() == "Customer_ID")
                        item.CustomerID = s[1].Trim();

                    if (s[0].Trim() == "Device_Name")
                        item.DeviceName = s[1].Trim();

                    if (s[0].Trim() == "PID_Name")
                        item.PIDName = s[1].Trim();

                    if (s[0].Trim() == "Program_Name")
                        item.ProgramName = s[1].Trim();

                    if (s[0].Trim() == "Mode_Code")
                        item.ModeCode = s[1].Trim();

                    if (s[0].Trim() == "Quantity")
                        item.Quantity = s[1].Trim();

                    if (s[0].Trim() == "Package_Type")
                        item.PackageType = s[1].Trim();

                    if (s[0].Trim() == "PIN_Count")
                        item.PINCount = s[1].Trim();

                    if (s[0].Trim() == "Tester_ID")
                        item.TesterID = s[1].Trim();

                    if (s[0].Trim() == "Loadboard_ID")
                        item.LoadboardID = s[1].Trim();

                    if (s[0].Trim() == "Handler_ID")
                        item.HandlerID = s[1].Trim();

                    if (s[0].Trim() == "Test_Temp")
                        item.TestTemp = s[1].Trim();

                    if (s[0].Trim() == "Operator_ID")
                        item.OperatorID = s[1].Trim();

                    if (s[0].Trim() == "Date_Code")
                        item.DateCode = s[1].Trim();

                    if (s[0].Trim() == "PASS_BIN")
                        item.PASSBIN = s[1].Trim();

                    if (s[0].Trim() == "TesterOS_Version")
                        item.TesterOSVersion = s[1].Trim();

                    if (s[0].Trim() == "Zipfile_Name")
                        item.ZipFile = s[1].Trim();

                    if (s[0].Trim() == "Program_Folder")
                        item.ProgramFolder = s[1].Trim();

                    if (s[0].Trim() == "Test_Flow")
                        item.TestFlow = s[1].Trim();

                    if (s[0].Trim() == "Soak_Time")
                        item.SoakTime = s[1].Trim();

                }
                sr.Close();
            }

            return item;
        }

        public Recipe GetRecipe(string recipeFilePath)
        {
            Recipe recipe = new Recipe();
            
            #region assign default value
            recipe.DatalogConfiguration.datalog_name_rule_1 = @"{Customer_ID}/{Device_Name}/{Customer_LotNo}/{Sub_LotNo}/";
            recipe.DatalogConfiguration.datalog_name_rule_2 = @"{Customer_LotNo}_{Sub_LotNo}_{Test_BinNo}_{Test_Code}_{Start_Time}";
            #endregion

            using (StreamReader sr = new StreamReader(recipeFilePath))
            {
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;

                    string[] s = line.Split('=');

                    if (s[0].Trim() == "datalog_name_rule_1")
                        recipe.DatalogConfiguration.datalog_name_rule_1 = s[1].Trim();

                    if (s[0].Trim() == "datalog_name_rule_2")
                        recipe.DatalogConfiguration.datalog_name_rule_2 = s[1].Trim();

                    if (s[0].Trim().ToLower().StartsWith("[input_oi_"))
                    {
                        UIInputItem item = new UIInputItem();
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith("#"))
                                continue;

                            s = line.Split('=');

                            if (s[0].StartsWith("[") && s[0].EndsWith("]"))
                                break;

                            if (s[0].Trim() == "Name")
                                item.Name = s[1].Trim();
                            else if (s[0].Trim() == "Pattern")
                                item.Pattern = s[1].Trim();
                            else if (s[0].Trim() == "Index")
                                item.Index = s[1].Trim();

                            if (!string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Pattern) && !string.IsNullOrEmpty(item.Index))
                            {
                                recipe.UIInputItemConfiguration.listUIInputItems.Add(item);
                                break;
                            }
                        }
                    }

                    if (s[0].Trim().ToLower() == "[testcode_config]")
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith("#"))
                                continue;

                            s = line.Split('=');

                            if (s[0].StartsWith("[") && s[0].EndsWith("]"))
                                break;

                            if (s.Length == 2)
                            {
                                try
                                {
                                    recipe.TestCodeConvertConfiguration.dictTestCode.Add(s[0].Trim(), s[1].Trim());
                                }
                                catch
                                { }
                            }
                        }
                    }

                    if (s[0].Trim().ToLower() == "[stdf_variable_config]")
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith("#"))
                                continue;

                            s = line.Split('=');

                            if (s[0].StartsWith("[") && s[0].EndsWith("]"))
                                break;

                            if (s.Length == 2)
                            {
                                try
                                {
                                    recipe.SpecialSTDFCondition.dictSTDFVariable.Add(s[0].Trim(), s[1].Trim());
                                }
                                catch
                                { }
                            }
                        }
                    }

                    if (s[0].Trim().ToLower() == "[stdf_constant_config]")
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.StartsWith("#"))
                                continue;

                            s = line.Split('=');

                            if (s[0].StartsWith("[") && s[0].EndsWith("]"))
                                break;

                            if (s.Length == 2)
                            {
                                try
                                {
                                    recipe.SpecialSTDFCondition.dictSTDFConstant.Add(s[0].Trim(), s[1].Trim());
                                }
                                catch
                                { }
                            }
                        }
                    }
                }
                sr.Close();
            }

            return recipe;
        }

        public void DeleteWorkspace(string property)
        {
            var isClearRequired = typeof(EnvironmentConfig).GetProperty(property).GetValue(enviConfig);
            if ((bool)isClearRequired)
            {
                ClearDir(enviConfig.LocalProgDir);
            }
        }

        public void CreateWorkspace()
        {
            CreateDir(enviConfig.LocalDlogDir);
            CreateDir(enviConfig.LocalProgDir);
        }

        public void SaveUILog(string operation, MTResponse res)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(uiLogPath, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    text = @"{time} {operation} {status} {message}";

                    text = text.Replace("{time}", string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now));
                    text = text.Replace("{operation}", operation);
                    text = text.Replace("{status}", res.ResponseStatus.ToString());
                    text = text.Replace("{message}", res.Message);

                    sw.WriteLine(text);
                }
            }
        }

        public void DecompressFile(string zipFilePath)
        {
            using (ZipService zipService = new ZipService())
            {
                if (Path.GetExtension(zipFilePath) == ".gz")
                {
                    zipService.DecompressFileToDestDirectory(zipFilePath, Path.GetDirectoryName(zipFilePath));

                    zipService.DecompressFileToDestDirectory(zipFilePath.Remove(zipFilePath.Length - 3, 3), Path.GetDirectoryName(zipFilePath));
                }
                else if (Path.GetExtension(zipFilePath) == ".zip")
                {
                    zipService.DecompressFileToDestDirectory(zipFilePath, Path.GetDirectoryName(zipFilePath));
                }
                else
                {
                    throw new Exception("Unsupported compressed type, please contact developer!");
                }
            }
        }

        #region IO
        public bool TryConnect()
        {
            if (enviConfig.TransferMode == TransferMode.FTP)
            {
                bool isFTPConnect = false;
                for (int i = 0; i < 3; i++)
                {
                    if (ftpDownload.Connect())
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

        public bool DownloadFile(string sourcePath, string destPath, bool needDecompress = false)
        {
            if (enviConfig.TransferMode == TransferMode.FTP)
            {
                if (!ftpDownload.FileExists(sourcePath))
                    return false;
                CreateDir(Path.GetDirectoryName(destPath));
                ftpDownload.Download(sourcePath, destPath);
            }
            else
            {
                if (!File.Exists(sourcePath))
                    return false;
                CreateDir(Path.GetDirectoryName(destPath));
                File.Copy(sourcePath, destPath, true);
            }

            if (needDecompress)
                DecompressFile(destPath);

            return true;
        }

        public void ClearDir(string dir)
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }

        public void CreateDir(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
        #endregion
    }
}