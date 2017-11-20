using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using MT.APS100.Model;

namespace MT.APS100.Service
{
    public class FlowService
    {
        private string dllPath;
        private string flwPath;
		private string calPath;
        private string limitPath;
        private string programName;
        private List<TestFunction> testFlow;
        private List<TestLimit> limits;
        private bool[] actionOnFail;
        private uint siteNum;
        private FlowWorkMode workMode;
        private const double INVALID = 999999999;
		private const string ALLPASS = "-1";
		private Stopwatch swapp = new Stopwatch();

        public FlowService(string FlwPath, uint siteNumber, FlowWorkMode flowWorkMode)
        {
            flwPath = FlwPath;
            siteNum = siteNumber;
            workMode = flowWorkMode;

            string name = Path.GetFileNameWithoutExtension(flwPath);
            programName = name.ToLower();

            testFlow = GetTestFlows();
            
            string dir = Path.GetDirectoryName(Path.GetDirectoryName(flwPath));
			dllPath = Path.Combine(dir, "Debug", name + ".dll");
            if (!File.Exists(dllPath))
            {
                throw new Exception("Program dll does not exist!");
            }

            limitPath = Path.Combine(dir, "Limit", name + ".csv");
            if (!File.Exists(limitPath))
            {
                throw new Exception("Limit file does not exist!");
            }
            limits = GetLimits();
            ValidateLimits();

            calPath = Path.Combine(FileStructure.USERCAL_DIR, name + ".csv");
        }

        public int Load()
        {
            Assembly program = Assembly.LoadFrom(dllPath);
            Type type = program.GetType(program.GetTypes().FirstOrDefault(x => x.Name.ToLower() == programName).FullName);
            object obj = Activator.CreateInstance(type);
            MethodInfo mi = type.GetMethod("ApplicationLoad");
            return (int)mi.Invoke(obj, new object[2] { calPath, false });
        }

        public List<DisplayTestResult> Start(int loop = 1)
        {
            Assembly program = Assembly.LoadFrom(dllPath);
            Type type = program.GetType(program.GetTypes().FirstOrDefault(x => x.Name.ToLower() == programName).FullName);
            object obj = Activator.CreateInstance(type);

            resetActionOnFail();
            List<DisplayTestResult> DisplayTestResults = new List<DisplayTestResult>();
            
            foreach (TestFunction tf in testFlow)
            {
                if (isAllFailed() && workMode != FlowWorkMode.ContinueOnFail)
                    break;

                MethodInfo mi = type.GetMethod(tf.Name);

                object[] parameters = new object[tf.Parameters.Count];

                for (int j = 0; j < tf.Parameters.Count; j++)
                {
                    FunctionParameter fp = tf.Parameters[j];

                    parameters[j] = ValueTranslator(fp.Type, fp.Value);
                }

                swapp.Start();
                var res = mi.Invoke(obj, parameters);
                swapp.Stop();
				DisplayTestResults.AddRange(ResultParser((List<AppResult>)res, tf.Name, swapp.Elapsed.TotalMilliseconds));
				swapp.Reset();
            }

            return DisplayTestResults;
        }

        private List<DisplayTestResult> ResultParser(List<AppResult> appResults, string functionName, double duration)
        {
            //ATENTION:
            //APS100, application will always give a fixed length of result, which is same as the site number
            //MT1600, application may adjust the legnth of result based on actionOnFail settings

            var grpAppResults = appResults.GroupBy(x => x.TestNumber);
            foreach(var item in grpAppResults)
            {
                if(item.ToList().Count > 1)
                {
                    throw new Exception(string.Format("System error: test item {0} is duplicated in program, please contact the developer!", item.Key));
                }
            }

            List<DisplayTestResult> list = new List<DisplayTestResult>();

            foreach (var item in appResults)
            {
                if (item.TestResult.Count != siteNum)
                    throw new Exception(string.Format("System error: site number is not matching between program({0}) and program({1}), please contact the developer!", item.TestResult.Count, siteNum));

                TestLimit limit = limits.First(x => x.TestNumber == item.TestNumber.ToString());
                if (limit != null)
                {
                    if (isAllFailed() && workMode != FlowWorkMode.ContinueOnFail)
                        break;

                    DisplayTestResult tr = new DisplayTestResult();
                    tr.TestNumber = uint.Parse(limit.TestNumber);
                    tr.TestName = limit.TestName;
                    tr.Unit = limit.Units;
                    tr.FTLower = double.Parse(limit.FTLower);
                    tr.FTUpper = double.Parse(limit.FTUpper);
                    tr.QALower = double.Parse(limit.QALower);
                    tr.QAUpper = double.Parse(limit.QAUpper);
                    tr.Offset = double.Parse(limit.Offset1);
                    tr.Factor = UnitParser(limit.Units);
                    for (int i = 0; i < item.TestResult.Count; i++)
                    {
                        if (actionOnFail[i] || workMode == FlowWorkMode.ContinueOnFail 
                            || (isAllFailed() && workMode == FlowWorkMode.StopOnAllFail))
                        {
                            tr.Results.Add(item.TestResult[i]);
                            double disVal = item.TestResult[i] * Math.Pow(10, tr.Factor) + tr.Offset;
                            tr.DisplayResults.Add(string.Format("{0:N6}", disVal));
                            tr.PFFlags.Add((tr.FTLower <= disVal) && (disVal <= tr.FTUpper));
                        }
                        else
                        {
                            tr.Results.Add(INVALID);
                            tr.DisplayResults.Add(string.Empty);
                            tr.PFFlags.Add(false);
                        }
                    }
                    tr.FunctionName = functionName;
                    tr.Duration = duration / appResults.Count;
                    list.Add(tr);

                    for (int i = 0; i < tr.PFFlags.Count; i++)
                    {
                        if (actionOnFail[i])
                        {
                            actionOnFail[i] = tr.PFFlags[i];
                        }
                    }
                }
            }

            return list;
        }

        public List<TestResult> ResultParser(List<DisplayTestResult> appResults)
        {
            List<TestResult> list = new List<TestResult>();
            foreach (var item in appResults)
            {
                for (int i = 0; i < item.Results.Count; i++)
                {
                    if (item.Results[i] == INVALID && item.DisplayResults[i] == string.Empty && item.PFFlags[i] == false)
                        continue;

                    TestResult tr = new TestResult();
                    tr.TestNumber = item.TestNumber;
                    tr.TestName = item.TestName;
                    tr.Unit = item.Unit;
                    tr.FTLower = item.FTLower;
                    tr.FTUpper = item.FTUpper;
                    tr.QALower = item.QALower;
                    tr.QAUpper = item.QAUpper;
                    tr.Offset = item.Offset;
                    tr.Factor = item.Factor;
                    tr.Result = item.Results[i] * Math.Pow(10, tr.Factor) + tr.Offset;                    
                    tr.PFFlag = item.PFFlags[i];
                    tr.SiteNumber = (byte)(i + 1);
                    tr.FunctionName = item.FunctionName;
                    tr.Duration = item.Duration;
                    list.Add(tr);
                }
            }

            return list;
        }

        public List<PartResult> ResultParser(List<TestResult> testResults, ref uint partID)
        {
            List<PartResult> list = new List<PartResult>();

            foreach (var col in testResults.GroupBy(x => x.SiteNumber))
            {
                List<TestResult> items = col.ToList();
                PartResult pr = new PartResult();
                pr.SiteNumber = col.Key;
                pr.PartID = ++partID;
                pr.isSuccess = items.Any(x => x.PFFlag == false) ? false : true;
                pr.SoftBin = SoftBinParser(items);
                pr.HardBin = HardBinParser(pr.SoftBin);
                pr.Duration = (double)items.Sum(x => x.Duration);
                pr.NumberOfTests = (ushort)items.Count;
                pr.TestResults = items;
                list.Add(pr);
            }

            return list;
        }

        private int UnitParser(string unit)
        {
            switch (unit)
            {
                case "fs":
                    return 15;
                case "pA":
                case "pW":
                case "ps":
                    return 12;
                case "nA":
                case "nW":
                case "ns":
                    return 9;
                case "uA":
                case "uV":
                case "uW":
                case "us":
                    return 6;
                case "mA":
                case "mV":
                case "mW":
                case "ms":
                    return 3;
                case "kW":
                case "kHz":
                    return -3;
                case "MHz":
                    return -6;
                case "GHz":
                    return -9;
                default:
                    return 0;
            }
        }

        private Bin SoftBinParser(List<TestResult> items, bool FirstOrLast = true)
        {
            Bin softBin = new Bin();
            if (items.Any(x => x.PFFlag == false))
            {
                TestResult item;
                if (FirstOrLast)
                {
                    item = items.First(x => x.PFFlag == false);
                }
                else
                {
                    item = items.Last(x => x.PFFlag == false);
                }
                TestLimit limit = limits.First(x => x.TestNumber == item.TestNumber.ToString());
                softBin.BinName = limit.SoftBinName;
                softBin.BinNum = (ushort)limit.SoftBinNumber;
                softBin.BinPF = (limit.SoftBinPF == BinPassFail.Pass.ToDescription()) ? BinPassFail.Pass : (limit.SoftBinPF == BinPassFail.Fail.ToDescription()) ? BinPassFail.Fail : BinPassFail.Unknown;
            }
            else
            {
                TestLimit limit = limits.First(x => x.TestNumber == ALLPASS);
                softBin.BinName = limit.SoftBinName;
                softBin.BinNum = (ushort)limit.SoftBinNumber;
                softBin.BinPF = BinPassFail.Pass;
            }
            return softBin;
        }

        private Bin HardBinParser(Bin softBin)
        {
            Bin hardBin = new Bin();
            TestLimit limit = limits.First(x => x.SoftBinNumber == softBin.BinNum);
            hardBin.BinName = limit.HardBinName;
            hardBin.BinNum = (ushort)limit.HardBinNumber;
            hardBin.BinPF = (limit.HardBinPF == BinPassFail.Pass.ToDescription()) ? BinPassFail.Pass : (limit.HardBinPF == BinPassFail.Fail.ToDescription()) ? BinPassFail.Fail : BinPassFail.Unknown;
            return hardBin;
        }

        public void Unload()
        {
            Assembly program = Assembly.LoadFrom(dllPath);
            Type type = program.GetType(program.GetTypes().FirstOrDefault(x => x.Name.ToLower() == programName).FullName);
            object obj = Activator.CreateInstance(type);
            MethodInfo mi = type.GetMethod("ApplicationUnload");
            object[] parameters = new object[0];
            int res = (int)mi.Invoke(obj, parameters);
        }

        private List<TestFunction> GetTestFlows()
        {
            List<TestFunction> list = new List<TestFunction>();

            XElement rootNode = XElement.Load(flwPath);
            IEnumerable<XElement> testFlows = from target in rootNode.Descendants("testflow")
                                              select target;
            foreach (XElement testFlow in testFlows)
            {
                TestFunction tf = new TestFunction();
                tf.ID = Convert.ToInt32(testFlow.Element("id").Value);
                tf.Name = testFlow.Element("testfunction").Value;

                IEnumerable<XElement> testFunctionParameters = from target in testFlow.Descendants("testfunctionparameter")
                                                               select target;
                foreach (XElement testFunctionParameter in testFunctionParameters)
                {
                    FunctionParameter fp = new FunctionParameter();
                    fp.Name = testFunctionParameter.Element("parametername").Value;
                    fp.Type = testFunctionParameter.Element("parametertype").Value;
                    fp.Value = testFunctionParameter.Element("parametervalue").Value;
                    tf.Parameters.Add(fp);
                }

                list.Add(tf);
            }

            return list;
        }

        public List<TestLimit> GetLimits()
        {
            if (limits == null)
            {
                Importer importer = new Importer();
                var vals = importer.BuildCSVStream("Limits", limitPath);
                return vals.Item1;
            }
            else
                return limits;
        }

		public void ValidateLimits()
		{
			if (limits.Count > 0)
			{
                Dictionary<int, int> softbinMap = new Dictionary<int, int>();
                List<Bin> softBins = new List<Bin>();
                List<Bin> hardBins = new List<Bin>();
				foreach(var limit in limits)
				{
					if (string.IsNullOrEmpty(limit.HardBinName))
					{
                        throw new Exception(string.Format("Item {0} has no Hard Bin Name defined!", limit.TestNumber));
                    }

					if (limit.HardBinPF != BinPassFail.Pass.ToDescription() && limit.HardBinPF != BinPassFail.Fail.ToDescription())
					{
                        throw new Exception(string.Format("Item {0} has invalid Pass and Fail bin flag!", limit.TestNumber));
                    }

					if (string.IsNullOrEmpty(limit.SoftBinName))
					{
                        throw new Exception(string.Format("Item {0} has no Soft Bin Name defined!", limit.TestNumber));
                    }

					if (limit.SoftBinPF != BinPassFail.Pass.ToDescription() && limit.SoftBinPF != BinPassFail.Fail.ToDescription())
					{
                        throw new Exception(string.Format("Item {0} has invalid Pass and Fail bin flag!", limit.TestNumber));
                    }

					if (limit.SoftBinPF == BinPassFail.Fail.ToDescription())
					{
						uint uRes;
						double dFTLower, dFTUpper, dQALower, dQAUpper, dRes;
						if (!uint.TryParse(limit.TestNumber, out uRes))
						{
                            throw new Exception(string.Format("{0} has invalid test number!", limit.TestName));
                        }

						if (!double.TryParse(limit.FTLower, out dFTLower))
						{
                            throw new Exception(string.Format("{0} has invalid FT lower limit!", limit.TestName));
                        }

						if (!double.TryParse(limit.FTUpper, out dFTUpper))
						{
                            throw new Exception(string.Format("{0} has invalid FT upper limit!", limit.TestName));
                        }

						if (!double.TryParse(limit.QALower, out dQALower))
						{
                            throw new Exception(string.Format("{0} has invalid QA lower limit!", limit.TestName));
                        }

						if (!double.TryParse(limit.QAUpper, out dQAUpper))
						{
                            throw new Exception(string.Format("{0} has invalid QA upper limit!", limit.TestName));
                        }

						if (!double.TryParse(limit.Offset1, out dRes))
						{
                            throw new Exception(string.Format("{0} has invalid offset!", limit.TestName));
                        }

						if (dFTLower > dFTUpper)
						{
                            throw new Exception(string.Format("{0} has invalid FT limit", limit.TestName));
                        }

						if (dQALower > dQAUpper)
						{
                            throw new Exception(string.Format("{0} has invalid QA limit", limit.TestName));
                        }
					}

                    if (softbinMap.Any(x=>x.Key == limit.SoftBinNumber))
                    {
                        if (softbinMap[limit.SoftBinNumber] != limit.HardBinNumber)
                        {
                            throw new Exception(string.Format("Soft bin {0} has two different hard bins {1} and {2}!",
                                limit.SoftBinNumber, limit.HardBinNumber, softbinMap[limit.SoftBinNumber]));
                        }
                    }
                    else
                    {
                        softbinMap.Add(limit.SoftBinNumber, limit.HardBinNumber);
                    }

                    //TODO: To make the BinNum as the primary key of a bin
                    var bin = new Bin() { BinNum = (ushort)limit.SoftBinNumber, BinName = limit.SoftBinName, BinPF = (limit.HardBinPF == BinPassFail.Pass.ToDescription()) ? BinPassFail.Pass : (limit.HardBinPF == BinPassFail.Fail.ToDescription()) ? BinPassFail.Fail : BinPassFail.Unknown };
				}
			}
			else
			{
				throw new Exception("No valid limits. Please check the limit file!");
			}
		}

		private object ValueTranslator(string type, string value)
        {
            object res;

            switch (type)
            {
                case "Int32":
                    res = Convert.ToInt32(value);
                    break;
                case "Double":
                    res = Convert.ToDouble(value);
                    break;
                case "Boolean":
                    if (value.ToUpper() == "TRUE")
                    {
                        res = true;
                    }
                    else
                    {
                        res = false;
                    }
                    break;
                case "Int32 Array":
                    res = ListValueTranslator<int>(value, int.Parse);
                    break;
                case "Double Array":
                    res = ListValueTranslator<double>(value, double.Parse);
                    break;
                default:
                    res = value;
                    break;
            }

            return res;
        }

        private List<T> ListValueTranslator<T>(string value, Func<string, T> TPhase)
        {
            List<T> li = new List<T>();
            string[] ls = value.Trim().Split(',');
            foreach (string s in ls)
            {
                li.Add(TPhase(s));
            }
            return li;
        }

        public void processCal(bool calRequired, int calExpired)
        {
            if (!Directory.Exists(FileStructure.USERCAL_DIR))
                Directory.CreateDirectory(FileStructure.USERCAL_DIR);

            if (!File.Exists(calPath))
            {
                UserCal();
            }
            else
            {
                if (calRequired)
                {
                    UserCal();
                }
                else
                {
                    DateTime lastModifiedDate = File.GetLastWriteTime(calPath);
                    TimeSpan ts = DateTime.Now.Subtract(lastModifiedDate);
                    if (ts.Days > calExpired)
                    {
                        File.Delete(calPath);
                        UserCal();
                    }
                }
            }
        }

        public void UserCal()
        {
            Assembly program = Assembly.LoadFrom(dllPath);
            Type type = program.GetType(program.GetTypes().FirstOrDefault(x => x.Name.ToLower() == programName).FullName);
            object obj = Activator.CreateInstance(type);
            object[] parameters = new object[0];

            MethodInfo mi = type.GetMethod("ApplicationLoad");
            mi.Invoke(obj, new object[2] { calPath, true });

            mi = type.GetMethod("ApplicationStart");
            mi.Invoke(obj, parameters);

            mi = type.GetMethod("UserCal");
            var res = mi.Invoke(obj, new object[1] { calPath });

            mi = type.GetMethod("ApplicationEnd");
            mi.Invoke(obj, parameters);

            mi = type.GetMethod("ApplicationUnload");
            mi.Invoke(obj, parameters);
        }

        private void resetActionOnFail()
        {
            actionOnFail = new bool[siteNum];
            for (int i = 0; i < actionOnFail.Length; i++)
            {
                actionOnFail[i] = true;
            }
        }

        private bool isAllFailed()
        {
            for (int i = 0; i < actionOnFail.Length; i++)
            {
                if (actionOnFail[i])
                    return false;
            }
            return true;
        }

        #region new
        private object program;
        private MethodInfo[] methods;

        public FlowService(string programName, string dllPath, string flwPath)
        {
            Assembly assembly = Assembly.LoadFrom(dllPath);
            Type type = assembly.GetType(assembly.GetTypes().First(x => x.Name == programName).FullName);
            program = Activator.CreateInstance(type);
            methods = type.GetMethods();

            testFlow = GetTestFlow(flwPath);
        }

        public int Load(string calPath)
        {
            MethodInfo mi = methods.First(x => x.Name == "ApplicationLoad");
            return (int)mi.Invoke(program, new object[2] { calPath, false });
        }

        public int Unload_New()
        {
            MethodInfo mi = methods.First(x => x.Name == "ApplicationUnload");
            return (int)mi.Invoke(program, new object[0]);
        }

        public List<DisplayTestResult> Start_New()
        {
            return new List<DisplayTestResult>();
        }

        public List<TestFunction> GetTestFlow(string flwPath)
        {
            List<TestFunction> list = new List<TestFunction>();

            XElement rootNode = XElement.Load(flwPath);
            IEnumerable<XElement> testFunctions = from target in rootNode.Descendants("TestFunction")
                                                  select target;
            foreach (XElement testFunction in testFunctions)
            {
                TestFunction tf = new TestFunction();
                tf.ID = Convert.ToInt32(testFunction.Element("ID").Value);
                tf.Name = testFunction.Element("Name").Value;

                IEnumerable<XElement> parameters = from target in testFunction.Descendants("Parameter")
                                                   select target;
                foreach (XElement parameter in parameters)
                {
                    FunctionParameter fp = new FunctionParameter();
                    fp.Name = parameter.Element("Name").Value;
                    fp.Type = parameter.Element("Type").Value;
                    fp.Value = parameter.Element("Value").Value;
                    tf.Parameters.Add(fp);
                }

                list.Add(tf);
            }

            return list;
        }
        #endregion
    }
}