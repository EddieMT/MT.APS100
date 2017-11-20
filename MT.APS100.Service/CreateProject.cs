using MT.APS100.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MT.APS100.Service
{
    public static class CreateProject
    {
        static public List<PartResult> Summary { get; set; }
        static public LotInfo Header { get; set; }
        private static string _rootprojectimport = @"c:\MerlinTest\Project";
        private static string _rootprojectexport = @"c:\MerlinTest\Data";
        private static string _rootprojectsystem = @"c:\MerlinTest\System";
        private static int SiteCount = 0;
        private static int AllPassTotal = 0;
        private static int _passsum = 0;
        private static int _failsum = 0;
        private static int softbintotalpass = 0;
        private static string HardBinName = string.Empty;

        public static void CSVExport(string writepath, List<TestLimit> limits)
        {
            //Begin Header Write///////////////////////////////////////////////////////////////////////////////////////
            string delimeter = ",";
            List<string> headercolumns = new List<string>();
            headercolumns.Add("FileName:" + ",");
            headercolumns.Add("Device_Folder:" + ",");
            headercolumns.Add("Summary report" + ",");
            headercolumns.Add("Start_Time:" + "," + string.Format("{0:yyyy/MM/dd HH:mm:ss}", Header.START_T));
            headercolumns.Add("End_Time:" + "," + string.Format("{0:yyyy/MM/dd HH:mm:ss}", Header.FINISH_T));
            headercolumns.Add("TesterOS_Version:" + "," + Header.TesterOSVersion.ToString());
            headercolumns.Add("Test_Temp:" + "," + Header.TST_TEMP.ToString());
            headercolumns.Add("Tester_ID:" + "," + Header.TesterID.ToString());
            headercolumns.Add("Operator_ID:" + "," + Header.OperatorID.ToString());
            headercolumns.Add("Customer_ID:" + "," + Header.CustomerID.ToString());
            headercolumns.Add("Device_Name:" + "," + Header.DeviceName.ToString());
            headercolumns.Add("Customer_LotNo:" + "," + Header.CustomerLotNo.ToString());
            headercolumns.Add("Sub_LotNo:" + "," + Header.SubLotNo.ToString());
            headercolumns.Add("Test_Code:" + "," + Header.TestCode.ToString());
            headercolumns.Add("Mode_Code:" + "," + Header.ModeCode.ToString());
            headercolumns.Add("Test_BinNo:" + "," + Header.TestBinNo.ToString());
            headercolumns.Add("Mother_Fab:" + "," + Header.MotherFab.ToString());
            headercolumns.Add("Wafer_Version:" + "," + Header.WaferVersion.ToString());
            headercolumns.Add("Handler_ID:" + "," + Header.HAND_ID.ToString());
            headercolumns.Add("Loadboard_ID:" + "," + Header.LOAD_ID.ToString());
            headercolumns.Add("Channel_Map:" + ",");
            headercolumns.Add("BIN");

            int length = headercolumns.Count; using (System.IO.TextWriter writer = File.CreateText(writepath))
            {
                foreach (string item in headercolumns)
                {
                    writer.WriteLine(string.Join(delimeter, item));
                    if (item == "Device_Folder:," || item == "Summary report," || item == "Channel_Map:,")
                    {
                        writer.WriteLine();
                    }
                }
                writer.WriteLine();
            }
            //End Heder Write/////////////////////////////////////////////////////////////////////////////

            //Begin Total Bin Result Write//////////////////////////////////////////////////////////////
            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }
            File.AppendAllText(writepath, "," + "BinResult");

            //Site Count query
            var sites = Summary.GroupBy(s => s.SiteNumber).Select(t => new { SiteNumber = t.Key });
            SiteCount = sites.Count();

            StringBuilder sitedata = new StringBuilder();

            sitedata.Append("," + "," + "Site1-" + SiteCount + "," + "BinNo." + "," + "BinResult" + "," + "Total");
            foreach (var myval in sites)
            {
                sitedata.Append("," + "Site " + myval.SiteNumber);
            }
            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }
            File.AppendAllText(writepath, sitedata.ToString());


            //pass fail total query
            var testdata = Summary.GroupBy(x => x.HardBin.BinPF).Select(BP => new
            {
                pass = BP.Count(p => p.HardBin.BinPF == BinPassFail.Pass),
                fail = BP.Count(f => f.HardBin.BinPF == BinPassFail.Fail)
            }).ToList();

            //pass fail by site query
            var PFResult = Summary.GroupBy(x => x.SiteNumber).Select(BP => new
            {
                pass = BP.Count(p => p.HardBin.BinPF == BinPassFail.Pass),
                fail = BP.Count(f => f.HardBin.BinPF == BinPassFail.Fail)
            });

            var passsumm = PFResult.Sum(x => x.pass);
            _passsum = passsumm;
            var failsum = PFResult.Sum(x => x.fail);
            _failsum = failsum;

            var HardBinResults = Summary.GroupBy(x => new { x.HardBin.BinPF, x.HardBin.BinName, x.HardBin.BinNum, x.SiteNumber }).Select(BP => new
            {
                BP.Key.BinNum,
                BP.Key.BinName,
                BP.Key.SiteNumber,
                BP.Key.BinPF,
                pass = BP.Count(p => p.HardBin.BinPF == BinPassFail.Pass),
                fail = BP.Count(f => f.HardBin.BinPF == BinPassFail.Fail)
            }).OrderBy(z => z.BinNum).ToList();

            foreach (var val in limits)
            {
                if (!HardBinResults.Exists(x => x.BinName == val.HardBinName))
                {
                    var result = new { BinNum = (ushort)val.HardBinNumber, BinName = val.HardBinName, SiteNumber = (byte)1, BinPF = BinPassFail.Pass, pass = 0, fail = 0 };
                    HardBinResults.Add(result);
                }
            }
            //Added ordering after changes occur
            var orderedhardbins = HardBinResults.OrderBy(x => x.BinNum);
            List<BinSummary> summaries = new List<BinSummary>();
            foreach (var hbresults in orderedhardbins)
            {
                foreach (var limit in limits)
                {
                    if (limit.HardBinName == hbresults.BinName)
                    {
                        HardBinName = limit.HardBinName;
                    }
                }
                //SiteTotals sitetotals = new SiteTotals();
                BinSummary binsummary = new BinSummary();
                binsummary.SiteCount = SiteCount;
                binsummary.BinName = hbresults.BinName;
                binsummary.BinNumber = hbresults.BinNum.ToString();
                binsummary.SiteNumber = hbresults.SiteNumber.ToString();
                //SiteTotals sitetotals = new SiteTotals();
                int y = 1;
                for(int x = 0; x < SiteCount; x++)
                {
                    SiteTotals sitetotals = new SiteTotals();
                    sitetotals.SiteName = y.ToString();
                    sitetotals.PassValue = 0;
                    sitetotals.FailValue = 0;
                    sitetotals.BinStatus = "Unknown";
                    binsummary.SiteName.Add(sitetotals);
                    y++;
                }

                foreach (var results in HardBinResults)
                {
                    if (hbresults.BinName == results.BinName)
                    {
                       if(results.pass > 0)
                        {
                            binsummary.SiteName[results.SiteNumber - 1].PassValue = results.pass;
                            binsummary.SiteName[results.SiteNumber - 1].BinStatus = results.BinPF.ToString();
                            binsummary.BinSummaryTotal = results.pass == 0 ? binsummary.BinSummaryTotal : binsummary.BinSummaryTotal + results.pass;
                        }
                       else
                        {
                            binsummary.SiteName[results.SiteNumber - 1].PassValue = results.fail;
                            binsummary.SiteName[results.SiteNumber - 1].BinStatus = results.BinPF.ToString();
                            binsummary.BinSummaryTotal = results.fail == 0 ? binsummary.BinSummaryTotal : binsummary.BinSummaryTotal + results.fail;
                        }
                    }
                }
                string existingbin = string.Empty;
                existingbin = summaries.Where(x => x.BinName == binsummary.BinName).Select(x => x.BinName).SingleOrDefault();

                if (existingbin == null)
                {
                    if (binsummary.BinName == "AllPass")
                    {
                        AllPassTotal = binsummary.BinSummaryTotal;
                    }
                    summaries.Add(binsummary);
                }
            }
            var hardpassbin = summaries.Find(x => x.BinName == "Pass");

            int hardpassbinindex = summaries.FindIndex(x => x.BinName == "Pass");
            summaries.RemoveAt(hardpassbinindex);
            summaries.Insert(0, hardpassbin);
            StringBuilder BinData = new StringBuilder();
            BinData.Append("," + "," + "," + "0" + "," + "Pass" + "," + _passsum);
            foreach (var myval in PFResult)
            {
                BinData.Append("," + myval.pass);
            }

            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }
            File.AppendAllText(writepath, BinData.ToString());

            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }

            StringBuilder faildata = new StringBuilder();
            faildata.Append("," + "," + "," + "1" + "," + "Fail" + "," + _failsum );


            foreach (var myval in PFResult)
            {
                faildata.Append("," + myval.fail);
            }
            File.AppendAllText(writepath, faildata.ToString());

            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }

            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }

            File.AppendAllText(writepath, "," + "HardBins");

            StringBuilder hardbin = new StringBuilder();

            hardbin.Append("," + "," + "Site1-" + SiteCount.ToString() + "," + "BinNo." + "," + "HardBins" + "," + "Bin result" + "," + "Total");

           
            foreach (var myval in sites)
            {
                hardbin.Append("," + "Site " + myval.SiteNumber);
            }

            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }
            File.AppendAllText(writepath, hardbin.ToString());

            StringBuilder hardbintotals = new StringBuilder();
            StringBuilder siteland = new StringBuilder();

            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }

            //////////////////////////////StringBuilder passTotals = new StringBuilder();
            //////////////////////////////passTotals.Append("," + "," + "," + "1" + "," + "AllPass" + "," + ",");
            //////////////////////////////foreach (var myval in PFResult)
            //////////////////////////////{
            //////////////////////////////    passTotals.Append("," + myval.pass);
            //////////////////////////////}
            //////////////////////////////File.AppendAllText(writepath, passTotals.ToString());
            //////////////using (TextWriter writer2 = File.AppendText(writepath))
            //////////////{
            //////////////    writer2.WriteLine();
            //////////////}

            foreach (BinSummary binsummary in summaries) 
            { 
                //if (binsummary.BinName != "AllPass")
                //{
                    //string flag = summaries[0].SiteName.Where(x => x.BinStatus == "Pass" || x.BinStatus == "Fail").Select(x => x.BinStatus).SingleOrDefault();
                string flag = "True";
                    StringBuilder sitecount = new StringBuilder();
                    {
                        foreach(SiteTotals siteflag in binsummary.SiteName)
                        {
                        if (siteflag.BinStatus == "Pass")
                        {
                            flag = "Pass";
                            //flag = siteflag.BinStatus == "Pass" ? "Pass" : "Fail";
                        }
                        if(siteflag.BinStatus == "Fail")
                        {
                            flag = "Fail";
                        }
                        }
                        sitecount.Append("," + "," + ",");
                        sitecount.Append(binsummary.BinNumber + "," + binsummary.BinName + "," + flag + "," + binsummary.BinSummaryTotal + ",");
                        foreach(SiteTotals totals in binsummary.SiteName)
                        {
                            sitecount.Append(totals.PassValue == 0 ? totals.FailValue : totals.PassValue);
                            sitecount.Append(",");
                        }
                        File.AppendAllText(writepath, sitecount.ToString());
                        using (TextWriter writer2 = File.AppendText(writepath))
                        {
                            writer2.WriteLine();
                        }
                    }
               // }
            }
            /////////////////////////////////////////////////////////////////// End Hard Bins
            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }
            
            File.AppendAllText(writepath, "," + "SoftBins");

            StringBuilder softbin = new StringBuilder();

            softbin.Append("," + "," + "Site1-" + SiteCount.ToString() + "," + "BinNo." + "," + "SoftBins" + "," + "HardBins" + "," + "Bin result" + "," + "Total");

            foreach (var myval in sites)
            {
                softbin.Append("," + "Site " + myval.SiteNumber);
            }
            
            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }
            File.AppendAllText(writepath, softbin.ToString());

            var SoftBinResults = Summary.GroupBy(x => new { x.SoftBin.BinPF, x.SoftBin.BinName, x.SoftBin.BinNum, x.SiteNumber }).Select(BP => new
            {
                BP.Key.BinNum,
                BP.Key.BinName,
                BP.Key.SiteNumber,
                BP.Key.BinPF,
                pass = BP.Count(p => p.SoftBin.BinPF == BinPassFail.Pass),
                fail = BP.Count(f => f.SoftBin.BinPF == BinPassFail.Fail)
            }).OrderBy(z => z.BinNum).ToList();

            foreach(var val in limits)
            {
                if(!SoftBinResults.Exists(x => x.BinName == val.SoftBinName))
                {
                    var result = new { BinNum = (ushort)val.SoftBinNumber, BinName = val.SoftBinName, SiteNumber = (byte)1, BinPF = BinPassFail.Pass, pass = 0, fail = 0 };
                    SoftBinResults.Add(result);
                }
            }

       
           var orderedlist = SoftBinResults.OrderBy(x => x.BinNum).ToList();
            List<BinSummary> softsummaries = new List<BinSummary>();
            foreach (var sbresults in orderedlist)
            {
                
                //SiteTotals sitetotals = new SiteTotals();
                foreach (var limit in limits)
                {
                    if (limit.SoftBinName == sbresults.BinName)
                    {
                        HardBinName = limit.HardBinName;
                    }
                }


                BinSummary binsummary = new BinSummary();
                binsummary.SiteCount = SiteCount;
                binsummary.BinName = sbresults.BinName;
                binsummary.HardBinName = HardBinName;
                binsummary.BinNumber = sbresults.BinNum.ToString();
                binsummary.SiteNumber = sbresults.SiteNumber.ToString();
                //SiteTotals sitetotals = new SiteTotals();
                int y = 1;
                for (int x = 0; x < SiteCount; x++)
                {
                    SiteTotals sitetotals = new SiteTotals();
                    sitetotals.SiteName = y.ToString();
                    sitetotals.PassValue = 0;
                    sitetotals.FailValue = 0;
                    binsummary.SiteName.Add(sitetotals);
                    y++;
                }

                foreach (var results in SoftBinResults)
                {
                    if (sbresults.BinName == results.BinName)
                    {
                        if (results.pass > 0)
                        {
                            binsummary.SiteName[results.SiteNumber - 1].PassValue = results.pass;
                            binsummary.SiteName[results.SiteNumber - 1].BinStatus = results.BinPF.ToString();
                            binsummary.BinSummaryTotal = results.pass == 0 ? binsummary.BinSummaryTotal : binsummary.BinSummaryTotal + results.pass;
                        }
                        else
                        {
                            binsummary.SiteName[results.SiteNumber - 1].PassValue = results.fail;
                            binsummary.SiteName[results.SiteNumber - 1].BinStatus = results.BinPF.ToString();
                            binsummary.BinSummaryTotal = results.fail == 0 ? binsummary.BinSummaryTotal : binsummary.BinSummaryTotal + results.fail;
                        }
                    }
                }
                string existingbin = string.Empty;
                existingbin = softsummaries.Where(x => x.BinName == binsummary.BinName).Select(x => x.BinName).SingleOrDefault();
                if (existingbin == null)
                {
                    if (binsummary.BinName == "AllPass")
                    {
                        softbintotalpass = binsummary.BinSummaryTotal;
                    }
                    // bool existingbin = false;
                    //existingbin = softsummaries.Exists(x => x.BinName == "AllPass");
                    //if (existingbin == false)
                    //{

                    //}
                    softsummaries.Add(binsummary);                    
                }
            }

            // File.AppendAllText(writepath, sitedata.ToString());
            var passbin = softsummaries.Find(x => x.BinName == "AllPass");
            int passbinindex = softsummaries.FindIndex(x => x.BinName == "AllPass");
            softsummaries.RemoveAt(passbinindex);
            softsummaries.Insert(0, passbin);
            foreach (BinSummary binsummary in softsummaries)
            {
                string flag = string.Empty;
                //if (binsummary.BinName != "AllPass")
                //{
                    StringBuilder sitecount = new StringBuilder();
                    {
                        foreach(SiteTotals softflag in binsummary.SiteName)
                        {
                        if (softflag.BinStatus == "Pass")
                        {
                            flag = "Pass";
                            //flag = siteflag.BinStatus == "Pass" ? "Pass" : "Fail";
                        }
                        if (softflag.BinStatus == "Fail")
                        {
                            flag = "Fail";
                        }
                    }
                        sitecount.Append("," + "," + ",");
                        sitecount.Append(binsummary.BinNumber + "," + binsummary.BinName + "," + binsummary.HardBinName + "," + flag + "," + binsummary.BinSummaryTotal + ",");
                        foreach (SiteTotals totals in binsummary.SiteName)
                        {
                            sitecount.Append(totals.PassValue == 0 ? totals.FailValue : totals.PassValue);
                            sitecount.Append(",");
                        }
                        using (TextWriter writer2 = File.AppendText(writepath))
                        {
                            writer2.WriteLine();
                        }
                        File.AppendAllText(writepath, sitecount.ToString());
                    }
                //}
            }

            ////////////////////////////////////////////////////////

            var hardbinnumber = Summary.GroupBy(x => new { x.HardBin.BinName, x.HardBin.BinNum }).Select(y => new { BinNumber = y.Key }).OrderBy(z => z.BinNumber.BinNum);

            var PFHardBins = Summary.GroupBy(x => new { x.HardBin.BinName, x.HardBin.BinNum }).Select(BP => new
            {
                BinNumber = BP.Key,
                pass = BP.Count(p => p.HardBin.BinPF == BinPassFail.Pass),
                fail = BP.Count(f => f.HardBin.BinPF == BinPassFail.Fail),

            });

            //var SoftBinResults = Summary.GroupBy(x => new { x.SoftBin.BinPF, x.SoftBin.BinName, x.SoftBin.BinNum, x.SiteNumber }).Select(BP => new
            //{
            //    BP.Key.BinNum,
            //    BP.Key.BinName,
            //    BP.Key.SiteNumber,
            //    pass = BP.Count(p => p.SoftBin.BinPF == BinPassFail.Pass),
            //    fail = BP.Count(f => f.SoftBin.BinPF == BinPassFail.Fail),
            //}).OrderBy(z => z.BinNum);
      
            var testresults = Summary.First().TestResults;
            StringBuilder TestNumber = new StringBuilder();
            StringBuilder TestName = new StringBuilder();
            StringBuilder Units = new StringBuilder();
            StringBuilder LowerLimit = new StringBuilder();
            StringBuilder UpperLimit = new StringBuilder();
            StringBuilder TestResults = new StringBuilder();
            foreach (TestResult result in testresults)
            {
                TestNumber.Append("," + result.TestNumber);
                TestName.Append("," + result.TestName);
                Units.Append("," + result.Unit);
                LowerLimit.Append("," + result.FTLower);
                UpperLimit.Append("," + result.FTUpper);
                TestResults.Append("," + result.Result);
            }

            //File.AppendAllText(writepath, TestNumber.ToString());
            //End Total Bin Result Write



        }

        public static void CreateCSV(string writepath, List<TestLimit> limits)
        {
            string delimeter = ",";
            List<string> headercolumns = new List<string>();
            headercolumns.Add("Product" + "," + Header.DeviceName.ToString());
            headercolumns.Add("Revision" + "," + Header.DeviceName.ToString());
            headercolumns.Add("Start Time" + "," + string.Format("{0:yyyy/MM/dd HH:mm:ss}", Header.START_T));
            headercolumns.Add("End Time" + "," + string.Format("{0:yyyy/MM/dd HH:mm:ss}", Header.FINISH_T));
            headercolumns.Add("Comment" + "," + "Where are the comments?");
            headercolumns.Add("Operator ID" + "," + Header.OperatorID.ToString());
            headercolumns.Add("Tester ID" + "," + Header.TesterID.ToString());
            headercolumns.Add("Lot ID" + "," + Header.CustomerLotNo.ToString());
            headercolumns.Add("Session ID" + "," + "Where is Session ID?");
            headercolumns.Add("Test Program Name" + "," + Header.ProgramName.ToString());
            headercolumns.Add("Test Type" + "," + Header.ModeCode.ToString());
            headercolumns.Add("# of sites" + "," + "Hard coded 4");
            headercolumns.Add("# of devices tested" + "," + Summary.Count.ToString());
            headercolumns.Add("# of pass devices" + "," + "Have not pulled passed items yet");
            headercolumns.Add("Yield" + "," + "Have not pulled yield items yet");

            int length = headercolumns.Count; using (System.IO.TextWriter writer = File.CreateText(writepath))
            {
                foreach (string item in headercolumns)
                {
                    writer.WriteLine(string.Join(delimeter, item));
                }
                writer.WriteLine();
            }

            StringBuilder headers = new StringBuilder();
            headers.Append("," + "Site" + "," + "X-coordinate" + "," + "Y-Coordinate" + "," + "Pass Fail" + "," + "Hard Bin" + "," + "Soft Bin" + "," + "Test Time" + "," + "Test Number");
            File.AppendAllText(writepath, headers.ToString());

            //var testresults = Summary.First().TestResults;
            StringBuilder TestNumber = new StringBuilder();
            StringBuilder TestName = new StringBuilder();
            StringBuilder Units = new StringBuilder();
            StringBuilder LowerLimit = new StringBuilder();
            StringBuilder UpperLimit = new StringBuilder();
            foreach (var limit in limits)
            {
                if (limit.TestNumber == "-1")
                    continue;

                TestNumber.Append("," + limit.TestNumber);
                TestName.Append("," + limit.TestName);
                Units.Append("," + limit.Units);
                LowerLimit.Append("," + limit.FTLower);
                UpperLimit.Append("," + limit.FTUpper);
            }

            File.AppendAllText(writepath, TestNumber.ToString());

            StringBuilder tn = new StringBuilder();
            StringBuilder units = new StringBuilder();
            StringBuilder ll = new StringBuilder();
            StringBuilder ul = new StringBuilder();

            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }
            tn.Append("," + "," + "," + "," + "," + "," + "," + "," + "Test Name");
            units.Append("," + "," + "," + "," + "," + "," + "," + "," + "Units");
            ll.Append("," + "," + "," + "," + "," + "," + "," + "," + "Lower Limit");
            ul.Append("Device #" + "," + "," + "," + "," + "," + "," + "," + "," + "Upper Limit");

            File.AppendAllText(writepath, tn.ToString());
            File.AppendAllText(writepath, TestName.ToString());

            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }
            File.AppendAllText(writepath, units.ToString());
            File.AppendAllText(writepath, Units.ToString());
            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }
            File.AppendAllText(writepath, ll.ToString());
            File.AppendAllText(writepath, LowerLimit.ToString());
            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }
            File.AppendAllText(writepath, ul.ToString());
            File.AppendAllText(writepath, UpperLimit.ToString());

            ////using (TextWriter writer2 = File.AppendText(writepath))
            ////{
            ////    writer2.WriteLine();
            ////}
            ////File.AppendAllText(writepath, TestResults.ToString());

            using (TextWriter writer2 = File.AppendText(writepath))
            {
                writer2.WriteLine();
            }

            {
                int length3 = Summary.Count; using (System.IO.StreamWriter writer = File.AppendText(writepath))
                //int length3 = Summary.Count; using (System.IO.TextWriter writer = File.(writepath))
                {
                    writer.WriteLine();
                    int x = 1;
                    foreach (PartResult device in Summary)
                    {
                        writer.WriteAsync(x.ToString() + "," + device.SiteNumber.ToString() + "," + "Xcor" + "," + "Ycor" + "," + (device.isSuccess == false ? "Fail" : "Pass") +
                            "," + device.HardBin.BinNum.ToString() + "," + device.SoftBin.BinNum.ToString() + "," + device.Duration.ToString() + ","
                            );

                        x++;
                        foreach (TestResult result in device.TestResults)
                        {
                            writer.Write("," + result.Result.ToString());
                        }
                        writer.WriteLine();

                        writer.Flush();
                    }
                }

                //////int length3 = Summary.Count; using (System.IO.StreamWriter writer = File.AppendText(writepath))
                //////{
                //////    writer.WriteLine();
                //////    int x = 1;
                //////    foreach (PartResult device in Summary)
                //////    {
                //////        writer.Write(x.ToString() + "," + device.SiteNumber.ToString() + "," + "Xcor" + "," + "Ycor" + "," +
                //////        device.isSuccess.ToString() + "," + device.HardBin.BinNum.ToString() + "," + device.SoftBin.BinNum.ToString() + "," +
                //////        device.Duration.ToString());
                //////        writer.WriteLine();
                //////        x++;
                //////    }
                //////}

                //int length4 = Summary.Count; using (System.IO.StreamWriter writer = File.AppendText(writepath))
                //{
                //    writer.WriteLine();
                //    foreach (PartResult device in Summary)
                //    {
                //        foreach(TestResult result in device.TestResults)
                //        {
                //            result.
                //        }
                //        writer.Write(x.ToString() + "," + device.SiteNumber.ToString() + "," + "Xcor" + "," + "Ycor" + "," +
                //        device.isSuccess.ToString() + "," + device.HardBin.BinNum.ToString() + "," + device.SoftBin.BinNum.ToString() + "," +
                //        device.Duration.ToString());
                //        writer.WriteLine();
                //        x++;
                //    }
                //}
            }
        }
        public static void CreateSubFolderStructure(string ProjectName)
        {
            Directory.CreateDirectory(_rootprojectimport + @"\" + ProjectName);
            Directory.CreateDirectory(_rootprojectimport + @"\" + ProjectName + @"\Config");
            Directory.CreateDirectory(_rootprojectimport + @"\" + ProjectName + @"\Limit");
            Directory.CreateDirectory(_rootprojectimport + @"\" + ProjectName + @"\Flow");
            Directory.CreateDirectory(_rootprojectexport + @"\" + ProjectName);
            //CreateConfigFile(_rootprojectimport + @"\" + ProjectName + @"\Config.csv");
            //Directory.CreateDirectory(_rootprojectimport + @"\" + ProjectName + @"\" + @"UserDocs\" + "TestPlan");
            //string _testplan = _rootprojectimport + @"\" + ProjectName + @"\" + @"UserDocs\" + "TestPlan";
            //Directory.CreateDirectory(_rootprojectimport + @"\" + ProjectName + @"\" + @"UserDocs\" + @"Data\Correlation");
        }
        public static bool CheckRootExists()
        {
            return Directory.Exists(_rootprojectimport);
        }
        public static bool CheckExportExists()
        {
            return Directory.Exists(_rootprojectexport);
        }

        public static bool CheckProjectExists(string ProjectName)
        {
            return Directory.Exists(_rootprojectimport + @"\" + ProjectName);
        }

        public static bool CheckSystemExists()
        {
            return Directory.Exists(_rootprojectsystem);
        }

        public static bool CreateFolderStructure(string ProjectImport)
        {
            if (!CheckProjectExists(ProjectImport))
            {
                if (CheckRootExists())
                {
                    CreateSubFolderStructure(ProjectImport);
                    //return true;
                }
                else
                {
                    Directory.CreateDirectory(_rootprojectimport);
                    CreateSubFolderStructure(ProjectImport);
                    //return true;
                }
                if (CheckExportExists())
                {
                    CreateFolderStructure(ProjectImport);
                    //return true;
                }
                else
                {
                    Directory.CreateDirectory(_rootprojectexport);
                    CreateSubFolderStructure(ProjectImport);
                    //return true;
                }
                if (CheckSystemExists())
                {
                    CreateFolderStructure(ProjectImport);
                }
                else
                {
                    Directory.CreateDirectory(_rootprojectsystem);
                    CreateFolderStructure(ProjectImport);
                }
            }
            return true;
        }



        public static void CreateConfigFile(Configuration configuration)
        {
            string ConfigPath = _rootprojectimport + @"\" + configuration.ProjectName + @"\Config\" + configuration.ProjectName + @".csv";
            if (System.IO.File.Exists(ConfigPath))
            {
                File.Delete(ConfigPath);
            }
            using (System.IO.StreamWriter writer = File.AppendText(ConfigPath))
            {
                writer.Write("Continue On Fail" + "," + configuration.ContinueOnFail);
                writer.WriteLine();
                writer.Write("Stop On Fail" + "," + configuration.StopOnFail);
                writer.WriteLine();
                writer.Write("Continue On All Fail" + "," + configuration.StopOnAllFail);
                writer.WriteLine();
                writer.Write("Stop On Alarm" + "," + configuration.StopOnAlarm);
                writer.WriteLine();
                writer.Write("Continue On Alarm" + "," + configuration.ContinueOnAlarm);
                writer.WriteLine();
                writer.Write("Gold Unit Enable/Disable" + "," + configuration.GoldUnitEnabled);
                writer.WriteLine();
                writer.Write("User Calibration" + "," + configuration.UserCalibration);
                writer.WriteLine();
                writer.Write("Calibration Expiration" + "," + configuration.CalibrationExpiration.ToString());
                writer.WriteLine();
                writer.Write("Offline QA Enable" + "," + configuration.QAOfflineEnabled);
                writer.WriteLine();
                writer.Write("Inline enabled nth device" + "," + configuration.QAInlineEnabled.ToString());
                writer.WriteLine();
                writer.Write("Log nth Device" + "," + configuration.LogNthDevice.ToString());
                writer.WriteLine();
                writer.Write("Number of Sites" + "," + configuration.NumberOfSites.ToString());

                foreach (Sites site in configuration.SiteName)
                {
                    writer.WriteLine();
                    writer.Write(site.SiteName + "," + site.SiteValue);
                }
            }
        }
    }
}