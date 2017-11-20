using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model
{
    public class BinSummary
    {
        public BinSummary()
        {
            this.SiteName = new List<SiteTotals>();
        }
        public string BinNumber { get; set; }
        public string BinName { get; set; }
        public string HardBinName { get; set; }
        public string BinResult { get; set; }
        public string SiteNumber { get; set; }
        public int BinAmount { get; set; }
        public int SiteCount { get; set; }
        public int BinSummaryTotal { get; set; }
        public List<SiteTotals> SiteName { get; set; }
    }

    public class SiteTotals
    {
        public string SiteName { get; set; }
        public int PassValue { get; set; }
        public int FailValue { get; set; }
        public string BinStatus { get; set; }
    }
}
