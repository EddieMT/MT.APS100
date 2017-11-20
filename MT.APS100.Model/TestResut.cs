using System.Collections.Generic;

namespace MT.APS100.Model
{
    public class AppResult
    {
        public AppResult(int testnumber, List<double> testresult)
        {
            TestNumber = testnumber;
            TestResult = testresult;
        }

        public int TestNumber { get; set; }
        public List<double> TestResult { get; set; }
    }

    public class TestResult
    {
        public uint TestNumber { get; set; }
        public string TestName { get; set; }
        public string Unit { get; set; }
        public double FTLower { get; set; }
        public double FTUpper { get; set; }
        public double QALower { get; set; }
        public double QAUpper { get; set; }
        public double Offset { get; set; }
        public double Result { get; set; }
        public int Factor { get; set; }
        public bool PFFlag { get; set; }
        public byte SiteNumber { get; set; }
        public string FunctionName { get; set; }
        public double Duration { get; set; }
    }

    public class DisplayTestResult
    {
        public uint TestNumber { get; set; }
        public string TestName { get; set; }
        public string Unit { get; set; }
        public double FTLower { get; set; }
        public double FTUpper { get; set; }
        public double QALower { get; set; }
        public double QAUpper { get; set; }
        public double Offset { get; set; }
        public List<double> Results { get; set; } = new List<double>();
        public int Factor { get; set; }
        public List<string> DisplayResults { get; set; } = new List<string>();
        public List<bool> PFFlags { get; set; } = new List<bool>();
        public string FunctionName { get; set; }
        public double Duration { get; set; }
    }

    public class PartResult
    {
        public byte SiteNumber { get; set; }
        public uint PartID { get; set; }
        public bool isSuccess { get; set; }
        public Bin SoftBin { get; set; }
        public Bin HardBin { get; set; }
        public double Duration { get; set; }
        public ushort NumberOfTests { get; set; }
        public List<TestResult> TestResults { get; set; }
    }

    public class Bin
    {
        public ushort BinNum { get; set; }
        public string BinName { get; set; }
        public BinPassFail BinPF { get; set; }
    }

    public class BinResult
    {
        public ushort BinNum { get; set; }
        public string BinName { get; set; }
        public string HardBinName { get; set; }
        public List<int> SubTotal { get; set; } = new List<int>();
        public List<string> SubYield { get; set; } = new List<string>();
        public int Total { get; set; }
        public string Yield { get; set; }
    }
}