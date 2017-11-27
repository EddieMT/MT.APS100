using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MT.APS100.TesterDriver
{
    public class CalData
    {
        public CalData()
        {
            measCalFactor = new double[12];
            this.measPath = new List<Measures>();
        }
        
        public string srcSelect;
        public string srcPath;
        public double srcFreq;
        public double srcLevel;
        public string modulationType;
        public string modulationFile;
        public double dutyCycle;
        public double calCalFactor;
        public double srcCalFactor;
        public double[] measCalFactor;
        public List<Measures> measPath { get; set; }
    }

    public class CalKey
    {
        public int testNumber;
        public string srcSelect;
        public string srcPath;
        public double srcFreq;
        public string modulationType;
    }

    public class PowerMeter
    {
        public bool Available { get; set; }
    }

    public class Amplifier
    {
        public double gain = 0;
    }

    public class Attenuation
    {
        public double[] srcAtten = new double[4];
        public double[] measAtten = new double[12];
    }

    public class Measures
    {
        public string MeasureName { get; set; }
        public string MeasurePopulated { get; set; }
    }

    public class CalImport
    {
        public Tuple<List<CalData>, PowerMeter, Amplifier, Attenuation> ImportCalConfig(string calConfigFile)
        {
            int counter = 0;
            bool flag = true;
            string line = " ";
            string[] splitLine = null;
            PowerMeter usePowerMeter = new PowerMeter();
            Amplifier amp = new Amplifier();
            Attenuation attenuation = new Attenuation();
            List<CalData> caldatum = new List<CalData>();

            try
            {
                using (StreamReader stream = new StreamReader(calConfigFile))
                {
                    Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                    do
                    {
                        splitLine = CSVParser.Split(line);

                        if (flag)
                        {
                            // Header information is on rows 1 through 4 of the Cal_Config.csv file
                            for (int row = 0; row < 4; row++)
                            {
                                line = stream.ReadLine(); // Read rows 1 - 4
                            }

                            // Power meter starts on row 5 of the Cal_Config.csv file
                            line = stream.ReadLine();  // Read row 5
                            splitLine = CSVParser.Split(line);
                            usePowerMeter.Available = (char.Parse(splitLine[1]) == 'Y' ? true : false) || (char.Parse(splitLine[1]) == 'y' ? true : false);

                            // Amplifier gain starts on row 6 of the Cal_Config.csv file
                            line = stream.ReadLine();  // Read row 6
                            splitLine = CSVParser.Split(line);
                            amp.gain = double.Parse(splitLine[1]);

                            // Source path attenuation data starts on row 7 of the Cal_Config.csv file
                            line = stream.ReadLine();  // Read row 7
                            splitLine = CSVParser.Split(line);
                            for (int srcIndex = 0; srcIndex < 4; srcIndex++)
                            {
                                attenuation.srcAtten[srcIndex] = double.Parse(splitLine[srcIndex + 1]);
                            }

                            // Measure path attenuation data starts on row 8 of the Cal_Config.csv file
                            line = stream.ReadLine(); // Read row 8
                            splitLine = CSVParser.Split(line);
                            for (int measIndex = 0; measIndex < 12; measIndex++)
                            {
                                attenuation.measAtten[measIndex] = double.Parse(splitLine[measIndex + 1]);
                            }

                            // Calibration configuration data starts on row 11 of the Cal_Config.csv file
                            for (int row = 0; row < 3; row++)
                            {
                                line = stream.ReadLine(); // Read rows 9 - 11
                            }
                            splitLine = CSVParser.Split(line);

                            flag = false;
                        }

                        if (splitLine.Length > 1)
                        {
                            CalData caldata = new CalData();
                            caldata.srcSelect = splitLine[0];
                            caldata.srcPath = splitLine[1];
                            caldata.srcFreq = double.Parse(splitLine[2]);
                            caldata.srcLevel = double.Parse(splitLine[3]);
                            caldata.modulationType = splitLine[4];
                            caldata.modulationFile = splitLine[5];
                            caldata.dutyCycle = double.Parse(splitLine[6]);

                            counter = 1;

                            for (int measIndex = 9; measIndex < 21; measIndex++)
                            {
                                Measures measures = new Measures();
                                measures.MeasureName = "Meas" + counter.ToString();
                                measures.MeasurePopulated = splitLine[measIndex];

                                caldata.measPath.Add(measures);
                                counter++;
                            }

                            caldatum.Add(caldata);
                        }

                    } while ((line = stream.ReadLine()) != null);
                }
            }
            catch (FileNotFoundException error)
            {
                Console.WriteLine("\n{0}", error.Message);
            }

            return Tuple.Create(caldatum, usePowerMeter, amp, attenuation);
        }

        public Tuple<List<CalData>, Attenuation> ImportCalData(string calConfigFile)
        {
            int counter = 0;
            bool flag = true;
            string line = " ";
            string[] splitLine = null;
            Attenuation attenuation = new Attenuation();
            List<CalData> caldatum = new List<CalData>();

            try
            {
                using (StreamReader stream = new StreamReader(calConfigFile))
                {
                    Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                    do
                    {
                        splitLine = CSVParser.Split(line);

                        if (flag)
                        {
                            // Header information is on rows 1 through 6 of the Cal_Config.csv file
                            for (int row = 0; row < 6; row++)
                            {
                                line = stream.ReadLine(); // Read rows 1 - 6
                            }

                            // Source path attenuation data starts on row 7 of the Cal_Config.csv file
                            line = stream.ReadLine();  // Read row 7
                            splitLine = CSVParser.Split(line);
                            for (int srcIndex = 0; srcIndex < 4; srcIndex++)
                            {
                                attenuation.srcAtten[srcIndex] = double.Parse(splitLine[srcIndex + 1]);
                            }

                            // Measure path attenuation data starts on row 8 of the Cal_Config.csv file
                            line = stream.ReadLine(); // Read row 8
                            splitLine = CSVParser.Split(line);
                            for (int measIndex = 0; measIndex < 12; measIndex++)
                            {
                                attenuation.measAtten[measIndex] = double.Parse(splitLine[measIndex + 1]);
                            }

                            // Calibration configuration data starts on row 11 of the Cal_Config.csv file
                            for (int row = 0; row < 3; row++)
                            {
                                line = stream.ReadLine(); // Read rows 9 - 11
                            }
                            splitLine = CSVParser.Split(line);

                            flag = false;
                        }

                        if (splitLine.Length > 1)
                        {
                            CalData caldata = new CalData();
                            caldata.srcSelect = splitLine[0];
                            caldata.srcPath = splitLine[1];
                            caldata.srcFreq = double.Parse(splitLine[2]);
                            caldata.srcLevel = double.Parse(splitLine[3]);
                            caldata.modulationType = splitLine[4];
                            caldata.modulationFile = splitLine[5];
                            caldata.dutyCycle = double.Parse(splitLine[6]);
                            caldata.calCalFactor = double.Parse(splitLine[7]);
                            caldata.srcCalFactor = double.Parse(splitLine[8]);

                            counter = 1;

                            for (int measIndex = 9; measIndex < 21; measIndex++)
                            {
                                //Measures measures = new Measures();
                                //measures.MeasureName = "Meas" + counter.ToString();
                                //measures.MeasurePopulated = splitLine[measIndex];

                                //caldata.measPath.Add(measures);

                                caldata.measCalFactor[measIndex - 9] = double.Parse(splitLine[measIndex]);
                                counter++;
                            }

                            caldatum.Add(caldata);
                        }

                    } while ((line = stream.ReadLine()) != null);
                }
            }
            catch (FileNotFoundException error)
            {
                Console.WriteLine("\n{0}", error.Message);
            }

            return Tuple.Create(caldatum, attenuation /*, usePowerMeter*/);
        }
    }
}
