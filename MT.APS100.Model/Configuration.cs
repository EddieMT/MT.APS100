using System;
using System.Collections.Generic;

namespace MT.APS100.Model
{
    public class Configuration
    {
        public string ProjectName { get; set; }
        public bool ContinueOnFail { get; set; }
        public bool StopOnFail { get; set; }
        public bool StopOnAllFail { get; set; }
        public bool StopOnAlarm { get; set; }
        public bool ContinueOnAlarm { get; set; }
        public bool UserCalibration { get; set; }
        public int CalibrationExpiration { get; set; }
        public bool GoldUnitEnabled { get; set; }
        public int QAInlineEnabled { get; set; }
        public bool QAOfflineEnabled { get; set; }
        public int NumberOfSites { get; set; }
        public int LogNthDevice { get; set; }
        public List<Sites> SiteName { get; set; } = new List<Sites>();
    }
    public class Sites
    {
        public string SiteName { get; set; }
        public bool SiteValue { get; set; }
    }
}