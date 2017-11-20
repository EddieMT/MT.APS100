using MT.APS100.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MT.APS100.Service
{
    public class Importer
    {
        public Tuple<List<TestLimit>, List<Limit>> BuildCSVStream(string _type, string _readPath)
        {
            List<TestLimit> limits = new List<TestLimit>();
            List<Limit> Limitsheader = new List<Limit>();
            bool HeaderCheck = true;
            using (StreamReader r = new StreamReader(_readPath))
            {
                string _limits = "Seed";

                do
                {
                    Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                    string[] x = CSVParser.Split(_limits);

                    if (x.Length > 1)
                    {
                        if (x[0] != "Test Number" && HeaderCheck)
                        {
                            Limit limitheader = new Limit();
                            limitheader.Header = x[0];
                            limitheader.Limits = x[1];
                            Limitsheader.Add(limitheader);
                            HeaderCheck = true;
                        }
                        else
                        {
                            HeaderCheck = false;
                        }

                        if (HeaderCheck == false && x[0] != "Test Number")
                        {
                            TestLimit limit = new TestLimit();
                            limit.TestNumber = x[0];
                            limit.TestName = x[1];
                            limit.Units = x[2];
                            limit.HardBinNumber = Convert.ToInt32(x[3]);
                            limit.HardBinName = x[4];
                            limit.HardBinPF = x[5];
                            limit.SoftBinNumber = Convert.ToInt32(x[6]);
                            limit.SoftBinName = x[7];
                            limit.SoftBinPF = x[8];
                            limit.FTLower = x[9];
                            limit.FTUpper = x[10];
                            limit.QALower = x[11];
                            limit.QAUpper = x[12];
                            limit.Offset1 = x[13];
                            limits.Add(limit);
                        }
                    }
                } while ((_limits = r.ReadLine()) != null);
            }
            return Tuple.Create(limits, Limitsheader);
        }
    }
}