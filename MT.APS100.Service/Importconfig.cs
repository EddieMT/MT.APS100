using MT.APS100.Model;
using System.IO;
using System.Text.RegularExpressions;

namespace MT.APS100.Service
{
    public class Importconfig
    {
        public Configuration ImportConfigurationData(string flowPath)
        {
            string dir = Path.GetDirectoryName(Path.GetDirectoryName(flowPath));
            string name = Path.GetFileNameWithoutExtension(flowPath);
            string cfgFilePath = Path.Combine(dir, "Config", name + ".csv");

            Configuration configuration = new Configuration();

            using (StreamReader r = new StreamReader(cfgFilePath))
            {
                string _limits = "Seed";
                do
                {
                    Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                    string[] x = CSVParser.Split(_limits);

                    if (x.Length > 1)
                    {
                        if (x[0] == "Continue On Fail")
                        {
                            configuration.ContinueOnFail = bool.Parse(x[1]);
                        }
                        if (x[0] == "Stop On Fail")
                        {
                            configuration.StopOnFail = bool.Parse(x[1]);
                        }
                        if (x[0] == "Continue On All Fail")
                        {
                            configuration.StopOnAllFail = bool.Parse(x[1]);
                        }
                        if (x[0] == "Stop On Alarm")
                        {
                            configuration.StopOnAlarm = bool.Parse(x[1]);
                        }
                        if (x[0] == "Continue On Alarm")
                        {
                            configuration.ContinueOnAlarm = bool.Parse(x[1]);
                        }
                        if (x[0] == "Gold Unit Enable/Disable")
                        {
                            configuration.GoldUnitEnabled = bool.Parse(x[1]);
                        }
                        if (x[0] == "User Calibration")
                        {
                            configuration.UserCalibration = bool.Parse(x[1]);
                        }
                        if (x[0] == "Calibration Expiration")
                        {
                            configuration.CalibrationExpiration = int.Parse(x[1]);
                        }
                        if (x[0] == "Offline QA Enable")
                        {
                            configuration.QAOfflineEnabled = bool.Parse(x[1]);
                        }
                        if (x[0] == "Inline enabled nth device")
                        {
                            configuration.QAInlineEnabled = int.Parse(x[1]);
                        }
                        if (x[0] == "Log nth Device")
                        {
                            configuration.LogNthDevice = int.Parse(x[1]);
                        }
                        if (x[0] == "Number of Sites")
                        {
                            configuration.NumberOfSites = int.Parse(x[1]);
                        }

                        if (configuration.NumberOfSites > 0 && (string)x[0] != "Number of Sites")
                        {
                            Sites site = new Sites();
                            site.SiteName = x[0];
                            site.SiteValue = bool.Parse(x[1]);
                            configuration.SiteName.Add(site);
                        }
                    }
                } while ((_limits = r.ReadLine()) != null);
            }
            return configuration;
        }
    }
}