using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace MT.APS100.TesterDriver
{
    public class Tester
    {
        private const string configPath = @"C:\MerlinTest\Config\PXIConfig.txt";

        public pe32h Digital;
        public AnalogMeasure AI;
        public SwitchMatrix Matrix;
        public PowerSupply io488PS;
        public mtSigGen SigGen1;
        public mtSigGen SigGen2;
        public mtDigitizer Digitizer;

        private string SRC_LO_RESOURCE_1;
        private string SRC_RF_RESOURCE_1;
        private string SRC_LO_RESOURCE_2;
        private string SRC_RF_RESOURCE_2;
        private string DIG_LO_RESOURCE_1;
        private string DIG_RF_RESOURCE_1;
        private string N6700C_RESOURCE;
        private string NI_PXI_DAQ1;
        private string NI_PXI_DAQ2;
        private string NOISE_SOURCE;
        private string PWR_METER_RESOURCE = "USB::0x0aad::*";

        public Tester()
        {
            LoadPXIConfig();

            if (!string.IsNullOrEmpty(N6700C_RESOURCE))
                io488PS = new PowerSupply(N6700C_RESOURCE);

            Digital = new pe32h();

            if (!string.IsNullOrEmpty(NI_PXI_DAQ1))
                AI = new AnalogMeasure(NI_PXI_DAQ1);

            if (!string.IsNullOrEmpty(SRC_LO_RESOURCE_1) && !string.IsNullOrEmpty(SRC_RF_RESOURCE_1))
                SigGen1 = new mtSigGen();

            if (!string.IsNullOrEmpty(SRC_LO_RESOURCE_2) && !string.IsNullOrEmpty(SRC_RF_RESOURCE_2))
                SigGen2 = new mtSigGen();

            if (!string.IsNullOrEmpty(DIG_LO_RESOURCE_1) && !string.IsNullOrEmpty(DIG_RF_RESOURCE_1))
                Digitizer = new mtDigitizer();

            Matrix = new SwitchMatrix(Digital);
        }

        public int Initialize()
        {
            int status = -1;

            if (io488PS != null)
                status = io488PS.Initialize();

            if (status == 0)
                status = Digital.Initialize();

            if (status == 0)
                status = Matrix.Initialize();

            if (AI != null && status == 0)
                status = AI.Initialize();

            if (SigGen1 != null && status == 0)
                status = SigGen1.Initialize(SRC_LO_RESOURCE_1, SRC_RF_RESOURCE_1, Instr10MHzReference.OCXO, true);

            if (SigGen2 != null && status == 0)
                status = SigGen2.Initialize(SRC_LO_RESOURCE_2, SRC_RF_RESOURCE_2, Instr10MHzReference.externDaisy, true);

            if (Digitizer != null && status == 0)
                status = Digitizer.Initialize(DIG_LO_RESOURCE_1, DIG_RF_RESOURCE_1, Instr10MHzReference.externTerm);

            return status;
        }

        private void LoadPXIConfig()
        {
            using (StreamReader sr = new StreamReader(configPath))
            {
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("SRC_LO_RESOURCE_1") && line.Contains("="))
                    {
                        SRC_LO_RESOURCE_1 = line.Split('=')[1].Trim().Trim('"');
                    }

                    if (line.Contains("SRC_RF_RESOURCE_1") && line.Contains("="))
                    {
                        SRC_RF_RESOURCE_1 = line.Split('=')[1].Trim().Trim('"');
                    }

                    if (line.Contains("SRC_LO_RESOURCE_2") && line.Contains("="))
                    {
                        SRC_LO_RESOURCE_2 = line.Split('=')[1].Trim().Trim('"');
                    }

                    if (line.Contains("SRC_RF_RESOURCE_2") && line.Contains("="))
                    {
                        SRC_RF_RESOURCE_2 = line.Split('=')[1].Trim().Trim('"');
                    }

                    if (line.Contains("DIG_LO_RESOURCE_1") && line.Contains("="))
                    {
                        DIG_LO_RESOURCE_1 = line.Split('=')[1].Trim().Trim('"');
                    }

                    if (line.Contains("DIG_RF_RESOURCE_1") && line.Contains("="))
                    {
                        DIG_RF_RESOURCE_1 = line.Split('=')[1].Trim().Trim('"');
                    }

                    if (line.Contains("N6700C_RESOURCE") && line.Contains("="))
                    {
                        N6700C_RESOURCE = line.Split('=')[1].Trim().Trim('"');
                    }

                    if (line.Contains("NI_PXI_DAQ1") && line.Contains("="))
                    {
                        NI_PXI_DAQ1 = line.Split('=')[1].Trim().Trim('"');
                    }

                    if (line.Contains("NI_PXI_DAQ2") && line.Contains("="))
                    {
                        NI_PXI_DAQ2 = line.Split('=')[1].Trim().Trim('"');
                    }

                    if (line.Contains("NOISE_SOURCE") && line.Contains("="))
                    {
                        NOISE_SOURCE = line.Split('=')[1].Trim().Trim('"');
                    }
                }
            }
        }

        #region UserCal
        private string programName;
        private string calConfigFile;
        private string calDataFile_RF;
        private string calDataFile_DC;
        PowerMeter calMeter;
        Amplifier calGain;
        Attenuation calAtten;
        List<CalData> calSetup;
        private rsnrpz PowerMeter;

        public void UserCal(string ProgramDir)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(ProgramDir);
            programName = directoryInfo.Name;
            calConfigFile = Path.Combine(ProgramDir, "Calibration", programName + "_Cal_Config.csv");
            calDataFile_RF = Path.Combine(Util.USERCAL_DIR, programName + "_RF_Cal_Data.csv");
            calDataFile_DC = Path.Combine(Util.USERCAL_DIR, programName + "_DC_Cal_Data.csv");
            calMeter = new PowerMeter();
            calGain = new Amplifier();
            calAtten = new Attenuation();
            calSetup = new List<CalData>();

            int status = -1;
            DialogResult response = DialogResult.None;

            response = MessageBox.Show("Perform User Calibration?", "Merlin Test Technologies", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (response == DialogResult.No) return;

            //-------------------- DC sense current calibration --------------------

            response = MessageBox.Show("Perform DC Sense Calibration?", "Merlin Test Technologies", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (response == DialogResult.Yes)
            {
                Console.WriteLine("********** DC Sense Calibration **********\n");
                InstrumentStartup_DC(); // Initialize DC instruments
                status = Calibration_DC_Sense(0); // Perform user calibration (DC sense)
                InstrumentShutdown_DC(); // Close DC instruments
            }

            //-------------------- RF calibration --------------------

            response = MessageBox.Show("Perform RF Calibration?", "Merlin Test Technologies", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (response == DialogResult.Yes)
            {
                Console.WriteLine("********** RF User Calibration **********\n");
                status = ReadUserCalConfigFile(); // Read user calibration configuration file
                if (status == 0)
                {
                    InstrumentStartup_RF(); // Initialize RF instruments
                    status = Calibration_RF(0); // Perform user calibration (RF)
                }
                InstrumentShutdown_RF(); // Close RF instruments
            }
        }

        private void InstrumentStartup_DC()
        {
            io488PS.Initialize();
            AI.Initialize();
        }

        private void InstrumentStartup_RF()
        {
            //-------------------- Boot Instruments --------------------

            bool enhArb_avail = true;

            Digital.Initialize();
            Matrix.Initialize();
            if (SigGen1 != null)
                SigGen1.Initialize(SRC_LO_RESOURCE_1, SRC_RF_RESOURCE_1, Instr10MHzReference.OCXO, enhArb_avail);
            if (SigGen2 != null)
                SigGen2.Initialize(SRC_LO_RESOURCE_2, SRC_RF_RESOURCE_2, Instr10MHzReference.externDaisy, enhArb_avail);
            Digitizer.Initialize(DIG_LO_RESOURCE_1, DIG_RF_RESOURCE_1, Instr10MHzReference.externTerm);
            Util.WaitTime(100e-3);

            //-------------------- Check 10 MHz Reference Lock --------------------

            bool refLockStatus = false;

            if (SigGen1 != null)
                refLockStatus = SigGen1.CheckSigGenRefLock();
            if (SigGen2 != null)
                refLockStatus = SigGen2.CheckSigGenRefLock();
            refLockStatus = Digitizer.CheckDigitizerRefLock();

            //-------------------- Load Modulation Waveforms --------------------

            if (SigGen1 != null)
                SigGen1.LoadModulationFiles("SG1", calSetup);
            if (SigGen2 != null)
                SigGen2.LoadModulationFiles("SG2", calSetup);
        }

        private void InstrumentShutdown_DC()
        {
            AI.clearSetupAI();
            io488PS.IOClose();
        }

        private void InstrumentShutdown_RF()
        {
            long status = -1;

            // Power off digital module / switch matrix
            Digital.reset(1);
            Digital.cpu_df(1, 0, 0, 0);

            if (SigGen1 != null)
            {
                status = SigGen1.RFCurrentOutputEnable_Set(0); // RF off
                status = SigGen1.CloseInstrument();
            }
            if (SigGen2 != null)
            {
                status = SigGen2.RFCurrentOutputEnable_Set(0); // RF off
                status = SigGen2.CloseInstrument();
            }

            status = Digitizer.CloseInstrument();
        }

        private int Calibration_DC_Sense(uint testNumber)
        {
            DialogResult response = DialogResult.None;

            response = MessageBox.Show("Initializing DC supply instrument:\nPlease disconnect V+ and GND power supply connections from DUT.\nMake sure nothing is connected to DPS.", "Keysight", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            if (response == DialogResult.Cancel) return -1;

            //-------------------- Configure DC Supply Instrument --------------------

            // Supply 1 and Supply 2
            //io488PS.IO.WriteString("VOLT 3.6, (@1:2);");

            // Supply 1
            io488PS.IOWriteString("VOLT 3.6, (@1);");
            io488PS.IOWriteString("CURR 0.1, (@1);");
            //io488PS.IO.WriteString("OUTP ON, (@1);");

            // Supply 2
            io488PS.IOWriteString("VOLT 3.6, (@2);");
            io488PS.IOWriteString("CURR 0.1, (@2);");
            //io488PS.IO.WriteString("OUTP ON, (@2);");

            // Supply 1 and Supply 2
            io488PS.IOWriteString("OUTP ON, (@1:2);");

            //-------------------- Configure Analog Measure Instrument --------------------

            string[] channels = new string[2] { "ai3", "ai1" };
            double[] rangeMins = new double[2] { -0.0001, -0.0001 };
            double[] rangeMaxs = new double[2] { 0.0001, 0.0001 };
            double[] senseOhms = new double[2] { 5, 20 }; //{ 1.667, 20 };
            bool[] invertChan = new bool[2] { true, false };

            AI.numOfChannels = 2;
            AI.channels = channels;
            AI.AIinput = AITerminalConfiguration.Differential;
            AI.rangeMins = rangeMins;
            AI.rangeMaxs = rangeMaxs;
            AI.senseOhms = senseOhms;
            AI.invertChan = invertChan;
            AI.sampleRate = 50e3;
            AI.totalSamples = 5000;
            AI.setupMeasurement();
            AI.startTrigSource = "None";
            AI.setTriggerSource();
            AI.setSampleClockandSamples();

            Util.WaitTime(250e-3);

            // Measure current
            double[] i;
            double i_result_supply1 = 0;
            double i_result_supply2 = 0;

            //AI.startAI();
            i = AI.getCurrentAverage();
            double[,] iSamples = AI.data;
            //AI.stopAI();

            i_result_supply1 = i[1];
            i_result_supply2 = i[0];

            Console.WriteLine("DPS 1 sense current:\t{0:F3} uA", i_result_supply1 * 1e6);
            Console.WriteLine("DPS 2 sense current:\t{0:F3} uA", i_result_supply2 * 1e6);
            Console.WriteLine();

            //-------------------- Shutdown DC Supply Instrument --------------------

            io488PS.IOWriteString("VOLT 0, (@1:2);");
            io488PS.IOWriteString("CURR 0.05, (@1:2);");
            io488PS.IOWriteString("OUTP OFF, (@1:2);");

            //-------------------- Write Calibration Data to File --------------------

            Console.WriteLine("Storing calibration results to file: {0}\n", Path.GetFileName(calDataFile_DC));

            StreamWriter file = new StreamWriter(calDataFile_DC);

            if (file == null)
            {
                Console.WriteLine("ERROR: Cannot write to DC Sense cal data file. Unable to store calibration results.\n");
                return -1;
            }

            DateTime timeStamp = DateTime.Now;

            // Calibration file header
            file.WriteLine("Product:,");
            file.WriteLine("Revision:,");
            file.WriteLine("Test Program:,");
            file.WriteLine("Date:,{0},", timeStamp);
            file.WriteLine("Tester ID:,");
            file.WriteLine("Comment:,");
            file.WriteLine(",,");
            file.WriteLine("DC Supply 1,DC Supply 2");

            // Calibration file data
            file.Write("{0:F9},{1:F9},", i_result_supply1, i_result_supply2);

            file.Close();

            return 0;
        }

        private int ReadUserCalConfigFile()
        {
            Console.WriteLine("Loading calibration configuration file: {0}\n", Path.GetFileName(calConfigFile));

            CalImport Importer = new CalImport();
            var CongigData = Importer.ImportCalConfig(calConfigFile);
            calSetup = CongigData.Item1;
            calMeter = CongigData.Item2;
            calGain = CongigData.Item3;
            calAtten = CongigData.Item4;

            if (calSetup.Count() <= 0) // No calibration configuration settings defined
            {
                Console.WriteLine("ERROR: No calibration configuration settings defined!\n");
                return -1;
            }

            return 0;
        }

        private int WriteUserCalDataFile()
        {
            Console.WriteLine("Storing calibration results to file: {0}\n", Path.GetFileName(calDataFile_RF));

            System.IO.StreamWriter file = new System.IO.StreamWriter(calDataFile_RF);

            if (file == null)
            {
                Console.WriteLine("ERROR: Cannot write to RF cal data file. Unable to store calibration results.\n");
                return -1;
            }

            string[] directories = null;
            string modulationFile = "";
            DateTime timeStamp = DateTime.Now;

            // Calibration file header
            file.WriteLine("Product:,,,,,,,,,,,,,,,,,,,,");
            file.WriteLine("Revision:,,,,,,,,,,,,,,,,,,,,");
            file.WriteLine("Test Program:,,,,,,,,,,,,,,,,,,,,");
            file.WriteLine("Date:,{0},,,,,,,,,,,,,,,,,,,", timeStamp);
            file.WriteLine("Tester ID:,,,,,,,,,,,,,,,,,,,,");
            file.WriteLine("Comment:,,,,,,,,,,,,,,,,,,,,");
            file.WriteLine("Source Path Attenuators:,{0},{1},{2},{3},,,,,,,,,,,,,,,,", calAtten.srcAtten[0], calAtten.srcAtten[1], calAtten.srcAtten[2], calAtten.srcAtten[3]);
            file.WriteLine("Measure Path Attenuators,{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},,,,,,,,", calAtten.measAtten[0], calAtten.measAtten[1], calAtten.measAtten[2], calAtten.measAtten[3], calAtten.measAtten[4], calAtten.measAtten[5], calAtten.measAtten[6], calAtten.measAtten[7], calAtten.measAtten[8], calAtten.measAtten[9], calAtten.measAtten[10], calAtten.measAtten[11]);
            file.WriteLine(",,,,,,,,,,,,,,,,,,,,");
            file.WriteLine("Source,Source Path,Frequency,Level,Modulation Type,Modulation File,Duty Cycle,Cal Factor (Cal),Cal Factor (Src),Cal Factor (M1),Cal Factor (M2),Cal Factor (M3),Cal Factor (M4),Cal Factor (M5),Cal Factor (M6),Cal Factor (M7),Cal Factor (M8),Cal Factor (M9),Cal Factor (M10),Cal Factor (M11),Cal Factor (M12)");

            // Calibration file data
            for (int calIndex = 0; calIndex < calSetup.Count(); calIndex++)
            {
                file.Write("{0},{1},", calSetup[calIndex].srcSelect, calSetup[calIndex].srcPath);
                file.Write("{0:F3},{1:F3},", calSetup[calIndex].srcFreq, calSetup[calIndex].srcLevel);

                directories = calSetup[calIndex].modulationFile.Split('\\');
                modulationFile = directories[directories.Length - 1];

                file.Write("{0},{1},{2:F3},", calSetup[calIndex].modulationType, modulationFile, calSetup[calIndex].dutyCycle);
                file.Write("{0:F3},{1:F3},", calSetup[calIndex].calCalFactor, calSetup[calIndex].srcCalFactor);

                for (uint measIndex = 0; measIndex < 11; measIndex++)
                {
                    file.Write("{0:F3},", calSetup[calIndex].measCalFactor[measIndex]);
                }

                file.Write("{0:F3}\n", calSetup[calIndex].measCalFactor[11]);
            }

            file.Close();

            return 0;
        }

        private int Calibration_RF(uint testNumber)
        {
            int status = -1;
            int calRecord = 1;
            int tmpRecord = 1;
            int numOfCalRecords = calSetup.Count();

            double Pin = 0;
            double headroom = 10;
            double margin = 8;
            double lowLimit = 0;
            double highLimit = 0;
            bool displayHeader = true;

            double ampGain = calGain.gain;
            const double matrixLoss = 4; // Estimated
            //const bool switchMatrixRev0 = true;

            string str = "";
            bool retry = false;
            bool calFail = false;
            bool directCalEnabled = true;
            bool modFilePlayingSigGen1 = false;
            bool modFilePlayingSigGen2 = false;
            DialogResult response = DialogResult.None;

            string lastSrcSelect = "";
            string lastSrcPath = "";
            string lastMeasPath = "";
            string currentSrcPath = "";
            string currentMeasPath = "";
            string currentModulation = "";
            string currentModulationSigGen1 = "";
            string currentModulationSigGen2 = "";

            double srcPower = 0;
            double srcError = 0;
            double mtrResult = -999;
            double digitizerLevel = 30;
            double[] digResult = new double[256];

            // Initialize Rohde & Schwarz power meter
            if (calMeter.Available)
            {
                status = InitializePowerMeter();
                if (status != 0) return -1;
            }
            else
            {
                directCalEnabled = true;
            }

            //-------------------- Source Path Calibration --------------------

            if (calMeter.Available)
            {
                do
                {
                    calRecord = 1;
                    calFail = false;
                    retry = false;

                    // Reset calibration instruments
                    if (SigGen1 != null) SigGen1.MuteSigGen();
                    modFilePlayingSigGen1 = false;
                    currentModulationSigGen1 = "CW";

                    if (SigGen2 != null) SigGen2.MuteSigGen();
                    modFilePlayingSigGen2 = false;
                    currentModulationSigGen2 = "CW";

                    for (int srcIndex = 0; srcIndex < 4; srcIndex++) // Start with source path S1 and end with S4 
                    {
                        currentSrcPath = "S" + (srcIndex + 1).ToString(); // Source path is S1 thru S4

                        for (int calIndex = 0; calIndex < numOfCalRecords; calIndex++) // Cycle through all cal config table entries (rows)
                        {
                            if (calSetup[calIndex].srcPath == currentSrcPath)
                            {
                                // Prompt user to connect power meter
                                if (currentSrcPath != lastSrcPath)
                                {
                                    str = "User Calibration (Source Path):\nPlease connect Source Output " + currentSrcPath + " cable to Power Meter.";
                                    response = MessageBox.Show(str, "Merlin Test Technologies", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                                    if (response == DialogResult.Cancel) return -1;
                                }

                                // Configure RF signal generator routing
                                if (calSetup[calIndex].srcSelect != lastSrcSelect)
                                {
                                    // Turn off last RF source if switching to new RF source
                                    if (calSetup[calIndex].srcSelect == "SG1" && SigGen2 != null)
                                        status = SigGen2.RFCurrentLevel_Set(-136);
                                    else if (calSetup[calIndex].srcSelect == "SG2" && SigGen1 != null)
                                        status = SigGen1.RFCurrentLevel_Set(-136);

                                    Matrix.ConnectSigGen(calSetup[calIndex].srcSelect);
                                    lastSrcSelect = calSetup[calIndex].srcSelect;
                                    Util.WaitTime(1e-3);
                                }

                                // Configure RF source routing
                                if (calSetup[calIndex].srcPath != lastSrcPath)
                                {
                                    Matrix.ConnectSourcePath(calSetup[calIndex].srcPath);
                                    lastSrcPath = calSetup[calIndex].srcPath;
                                    Util.WaitTime(1e-3);
                                }

                                // Configure RF signal generator
                                Pin = calSetup[calIndex].srcLevel - ampGain + matrixLoss + calAtten.srcAtten[srcIndex];

                                if (calSetup[calIndex].srcSelect == "SG1")
                                {
                                    SigGen1.PlayModulationFile(calSetup[calIndex].modulationType, calSetup[calIndex].modulationFile, ref modFilePlayingSigGen1, ref currentModulationSigGen1);
                                    status = SigGen1.RFCurrentFrequency_Set(calSetup[calIndex].srcFreq);
                                    status = SigGen1.RFCurrentLevel_Set(Pin);
                                    SigGen1.CheckSigGenStatus(status);
                                }
                                else if (calSetup[calIndex].srcSelect == "SG2")
                                {
                                    SigGen2.PlayModulationFile(calSetup[calIndex].modulationType, calSetup[calIndex].modulationFile, ref modFilePlayingSigGen2, ref currentModulationSigGen2);
                                    status = SigGen2.RFCurrentFrequency_Set(calSetup[calIndex].srcFreq);
                                    status = SigGen2.RFCurrentLevel_Set(Pin);
                                    SigGen2.CheckSigGenStatus(status);
                                }

                                // Configure power meter frequency and duty cycle
                                status = PowerMeter.chan_setCorrectionFrequency(1, calSetup[calIndex].srcFreq);
                                if (calSetup[calIndex].dutyCycle <= 0 || calSetup[calIndex].dutyCycle >= 100) status = PowerMeter.corr_configureDutyCycle(1, false, 100); // Duty cycle default is 100% if erroneously defined
                                else status = PowerMeter.corr_configureDutyCycle(1, true, calSetup[calIndex].dutyCycle);

                                Util.WaitTime(100e-3);

                                //status = SigGen1.Calibrate.IQ.CurrentFrequency();

                                // Measure RF power (determine target input power offset error)
                                for (uint errorLoop = 0; errorLoop < 2; errorLoop++)
                                {
                                    if (errorLoop == 1) // Need to make an additional measurement for WLAN only
                                    {
                                        if (calSetup[calIndex].srcSelect == "SG1")
                                        {
                                            status = SigGen1.RFCurrentLevel_Set(Pin);
                                            SigGen1.CheckSigGenStatus(status);
                                        }
                                        else if (calSetup[calIndex].srcSelect == "SG2")
                                        {
                                            status = SigGen2.RFCurrentLevel_Set(Pin);
                                            SigGen2.CheckSigGenStatus(status);
                                        }

                                        Util.WaitTime(100e-3);
                                    }

                                    status = PowerMeter.meass_readMeasurement(1, 2000, out mtrResult);
                                    CheckPowerMeterStatus();
                                    if (mtrResult <= 0) mtrResult = 1e-32; // Set minimum result to 1e-32 to avoid log10(0) condition
                                    srcPower = (10 * Math.Log10(mtrResult) + 30.0);
                                    srcError = srcPower - calSetup[calIndex].srcLevel;
                                    Pin -= srcError;

                                    if (calSetup[calIndex].modulationType != "WLAN") break;
                                }

                                if (calSetup[calIndex].srcSelect == "SG1")
                                {
                                    status = SigGen1.RFCurrentLevel_Set(Pin);
                                    SigGen1.CheckSigGenStatus(status);
                                }
                                else if (calSetup[calIndex].srcSelect == "SG2")
                                {
                                    status = SigGen2.RFCurrentLevel_Set(Pin);
                                    SigGen2.CheckSigGenStatus(status);
                                }

                                Util.WaitTime(100e-3);

                                // Measure RF power
                                status = PowerMeter.meass_readMeasurement(1, 2000, out mtrResult);
                                CheckPowerMeterStatus();
                                if (mtrResult <= 0) mtrResult = 1e-32; // Set minimum result to 1e-32 to avoid log10(0) condition
                                srcPower = (10 * Math.Log10(mtrResult) + 30.0);
                                calSetup[calIndex].srcCalFactor = srcPower - Pin;

                                // Verify calibration results are valid
                                lowLimit = (int)(-calAtten.srcAtten[srcIndex] + ampGain - matrixLoss - margin);
                                highLimit = (int)(-calAtten.srcAtten[srcIndex] + ampGain - matrixLoss + margin);
                                if ((calSetup[calIndex].srcCalFactor < lowLimit) || (calSetup[calIndex].srcCalFactor > highLimit)) calFail = true;

                                // Write calibration results to console window
                                OutputCalData(displayHeader, calRecord, currentSrcPath, calSetup[calIndex].srcFreq, lowLimit, calSetup[calIndex].srcCalFactor, highLimit);
                                displayHeader = false;

                                calRecord++;
                            }
                        }
                    }

                    if (calFail)
                    {
                        response = MessageBox.Show("User Calibration (Source Path) FAILED!", "Merlin Test Technologies", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        if (response == DialogResult.Abort) return -1;
                        else if (response == DialogResult.Retry) retry = true;
                        else if (response == DialogResult.Ignore) retry = false;
                    }

                    tmpRecord = calRecord;
                    Console.WriteLine();

                } while (retry == true);

                PowerMeter.Dispose(); // Close Rohde & Schwarz power meter session
            }

            //-------------------- Measure Path Calibration --------------------

            do
            {
                calRecord = tmpRecord;
                calFail = false;
                retry = false;

                // Reset calibration instruments
                if (SigGen1 != null) SigGen1.MuteSigGen();
                modFilePlayingSigGen1 = false;
                currentModulationSigGen1 = "CW";

                if (SigGen2 != null) SigGen2.MuteSigGen();
                modFilePlayingSigGen2 = false;
                currentModulationSigGen2 = "CW";

                for (int srcIndex = 0; srcIndex < 4; srcIndex++) // Start with source cable S1 and end with S4
                {
                    currentSrcPath = "S" + (srcIndex + 1).ToString(); // Source path is S1 thru S4

                    for (int calIndex = 0; calIndex < numOfCalRecords; calIndex++) // Cycle through all cal config table entries (rows)
                    {
                        if (calSetup[calIndex].srcPath == currentSrcPath)
                        {
                            for (int measIndex = 0; measIndex < 12; measIndex++) // Start with measure cable M1 and end with M12
                            {
                                currentMeasPath = "M" + (measIndex + 1).ToString(); // Measure path is M1 thru M12

                                for (int subIndex = calIndex; subIndex < numOfCalRecords; subIndex++) // Cycle through all Mx entries (rows)
                                {
                                    if ((calSetup[subIndex].srcPath == currentSrcPath) && (calSetup[subIndex].measPath[measIndex].MeasurePopulated != ""))
                                    {
                                        // Prompt user to connect source and measure paths
                                        if (currentSrcPath != lastSrcPath || currentMeasPath != lastMeasPath)
                                        {
                                            str = "User Calibration (Measure Path):\nPlease connect Source Output " + currentSrcPath + " cable to Measure Input " + currentMeasPath + " cable.";
                                            response = MessageBox.Show(str, "Merlin Test Technologies", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                                            if (response == DialogResult.Cancel) return -1;
                                        }

                                        // Configure RF signal generator routing
                                        if (calSetup[subIndex].srcSelect != lastSrcSelect)
                                        {
                                            // Turn off last RF source if switching to new RF source
                                            if (calSetup[subIndex].srcSelect == "SG1" && SigGen2 != null) status = SigGen2.RFCurrentLevel_Set(-136);
                                            else if (calSetup[subIndex].srcSelect == "SG2" && SigGen1 != null) status = SigGen1.RFCurrentLevel_Set(-136);

                                            Matrix.ConnectSigGen(calSetup[subIndex].srcSelect);
                                            lastSrcSelect = calSetup[subIndex].srcSelect;
                                            Util.WaitTime(1e-3);
                                        }

                                        // Configure RF source routing
                                        if ((calSetup[subIndex].srcPath != lastSrcPath) || directCalEnabled)
                                        {
                                            Matrix.ConnectSourcePath(calSetup[subIndex].srcPath);
                                            lastSrcPath = calSetup[subIndex].srcPath;
                                            Util.WaitTime(1e-3);
                                        }

                                        // Configure RF measure routing
                                        if ((currentMeasPath != lastMeasPath) || directCalEnabled)
                                        {
                                            //if (switchMatrixRev0) Matrix.ConnectDigitizer("D1");
                                            Matrix.ConnectMeasurePath(currentMeasPath);
                                            lastMeasPath = currentMeasPath;
                                            Util.WaitTime(1e-3);
                                        }

                                        // Configure RF signal generator
                                        Pin = calSetup[subIndex].srcLevel - calSetup[subIndex].srcCalFactor;

                                        if (calSetup[subIndex].srcSelect == "SG1")
                                        {
                                            SigGen1.PlayModulationFile(calSetup[subIndex].modulationType, calSetup[subIndex].modulationFile, ref modFilePlayingSigGen1, ref currentModulationSigGen1);
                                            status = SigGen1.RFCurrentFrequency_Set(calSetup[subIndex].srcFreq);
                                            status = SigGen1.RFCurrentLevel_Set(Pin);
                                            SigGen1.CheckSigGenStatus(status);
                                            currentModulation = currentModulationSigGen1; // Used to select CalCapture() function settings
                                        }
                                        else if (calSetup[subIndex].srcSelect == "SG2")
                                        {
                                            SigGen2.PlayModulationFile(calSetup[subIndex].modulationType, calSetup[subIndex].modulationFile, ref modFilePlayingSigGen2, ref currentModulationSigGen2);
                                            status = SigGen2.RFCurrentFrequency_Set(calSetup[subIndex].srcFreq);
                                            status = SigGen2.RFCurrentLevel_Set(Pin);
                                            SigGen2.CheckSigGenStatus(status);
                                            currentModulation = currentModulationSigGen2; // Used to select CalCapture() function settings
                                        }

                                        // Configure RF digitizer
                                        if (calMeter.Available) digitizerLevel = (Pin + calSetup[subIndex].srcCalFactor) - calAtten.measAtten[measIndex] - matrixLoss + headroom;
                                        else digitizerLevel = calSetup[calIndex].srcLevel - calAtten.measAtten[measIndex] - matrixLoss + headroom;
                                        if (digitizerLevel > 30) digitizerLevel = 30;
                                        if (digitizerLevel < -99) digitizerLevel = -99;
                                        status = Digitizer.RFRFInputLevel_Set(digitizerLevel);
                                        status = Digitizer.RFCentreFrequency_Set(calSetup[subIndex].srcFreq);

                                        Util.WaitTime(100e-3);

                                        //status = SigGen.Calibrate.IQ.CurrentFrequency();

                                        // Measure RF power
                                        if (currentModulation == "CW") CalCapture(calSetup[subIndex].srcFreq, 1e6, true, 1, 0.5e-3, 0.5e-3, 0e-6, 0.5e-3, false, ref digResult);
                                        else if (currentModulation == "LTE-FDD") CalCapture(calSetup[subIndex].srcFreq, 31e6, true, 1, 0.5e-3, 0.5e-3, 0e-6, 0.5e-3, true, ref digResult);
                                        else if (currentModulation == "LTE-TDD") CalCapture(calSetup[subIndex].srcFreq, 31e6, true, 1, 0.5e-3, 0.5e-3, 0e-6, 0.5e-3, true, ref digResult);
                                        else if (currentModulation == "TD-SCDMA") CalCapture(calSetup[subIndex].srcFreq, 9.68e6, true, 1, 0.6625e-3, 0.6625e-3, 0e-6, 0.6625e-3, true, ref digResult);
                                        else if (currentModulation == "WCDMA") CalCapture(calSetup[subIndex].srcFreq, 25e6, true, 1, 0.5e-3, 0.5e-3, 0e-6, 0.5e-3, true, ref digResult);
                                        else if (currentModulation == "WLAN") CalCapture(calSetup[subIndex].srcFreq, 160e6, true, 1, 0.184e-3, 0.184e-3, 0e-6, 0.184e-3, true, ref digResult);

                                        calSetup[subIndex].measCalFactor[measIndex] = digResult[0] - calSetup[subIndex].srcLevel;

                                        // Verify calibration results are valid
                                        if (calMeter.Available)
                                        {
                                            lowLimit = (int)(-calAtten.measAtten[measIndex] - matrixLoss - margin);
                                            highLimit = (int)(-calAtten.measAtten[measIndex] - matrixLoss + margin);
                                        }
                                        else
                                        {
                                            lowLimit = (int)(-calAtten.srcAtten[srcIndex] - calAtten.measAtten[measIndex] + ampGain - 2 * matrixLoss - margin);
                                            highLimit = (int)(-calAtten.srcAtten[srcIndex] - calAtten.measAtten[measIndex] + ampGain - 2 * matrixLoss + margin);
                                        }
                                        if ((calSetup[subIndex].measCalFactor[measIndex] < lowLimit) || (calSetup[subIndex].measCalFactor[measIndex] > highLimit)) calFail = true;

                                        // Write calibration results to console window
                                        OutputCalData(displayHeader, calRecord, currentMeasPath, calSetup[subIndex].srcFreq, lowLimit, calSetup[subIndex].measCalFactor[measIndex], highLimit);
                                        displayHeader = false;

                                        calRecord++;

                                        // Calibrate internal cal (direct) path
                                        if (directCalEnabled)
                                        {
                                            // Configure RF source routing
                                            Matrix.ConnectSourcePath("CAL");
                                            Util.WaitTime(1e-3);

                                            // Configure RF measure routing
                                            //if (switchMatrixRev0) Matrix.ConnectDigitizer("CAL");
                                            //else Matrix.ConnectMeasurePath("CAL");
                                            Matrix.ConnectMeasurePath("CAL");
                                            Util.WaitTime(1e-3);

                                            // Configure RF digitizer
                                            if (calMeter.Available) digitizerLevel = Pin + ampGain - matrixLoss + headroom;
                                            //if (calMeter.Available) digitizerLevel = Pin - calAtten.measAtten[measIndex] + ampGain - matrixLoss + headroom;
                                            else digitizerLevel = (Pin - calSetup[subIndex].srcCalFactor) + ampGain - matrixLoss + headroom;
                                            if (digitizerLevel > 30) digitizerLevel = 30;
                                            if (digitizerLevel < -99) digitizerLevel = -99;
                                            status = Digitizer.RFRFInputLevel_Set(digitizerLevel);

                                            Util.WaitTime(100e-3);

                                            // Measure RF power
                                            if (currentModulation == "CW") CalCapture(calSetup[subIndex].srcFreq, 1e6, true, 1, 0.5e-3, 0.5e-3, 0e-6, 0.5e-3, false, ref digResult);
                                            else if (currentModulation == "LTE-FDD") CalCapture(calSetup[subIndex].srcFreq, 31e6, true, 1, 0.5e-3, 0.5e-3, 0e-6, 0.5e-3, true, ref digResult);
                                            else if (currentModulation == "LTE-TDD") CalCapture(calSetup[subIndex].srcFreq, 31e6, true, 1, 0.5e-3, 0.5e-3, 0e-6, 0.5e-3, true, ref digResult);
                                            else if (currentModulation == "TD-SCDMA") CalCapture(calSetup[subIndex].srcFreq, 9.68e6, true, 1, 0.6625e-3, 0.6625e-3, 0e-6, 0.6625e-3, true, ref digResult);
                                            else if (currentModulation == "WCDMA") CalCapture(calSetup[subIndex].srcFreq, 25e6, true, 1, 0.5e-3, 0.5e-3, 0e-6, 0.5e-3, true, ref digResult);
                                            else if (currentModulation == "WLAN") CalCapture(calSetup[subIndex].srcFreq, 160e6, true, 1, 0.184e-3, 0.184e-3, 0e-6, 0.184e-3, true, ref digResult);

                                            if (calMeter.Available)
                                            {
                                                calSetup[subIndex].calCalFactor = digResult[0] - calSetup[subIndex].srcLevel;
                                            }
                                            else
                                            {
                                                calSetup[subIndex].calCalFactor = digResult[0] - Pin;
                                            }

                                            // Verify calibration results are valid
                                            if (calMeter.Available)
                                            {
                                                lowLimit = (int)(-1.5 + calAtten.srcAtten[srcIndex] - margin);
                                                highLimit = (int)(-1.5 + calAtten.srcAtten[srcIndex] + margin);
                                            }
                                            else
                                            {
                                                lowLimit = (int)(-calAtten.srcAtten[srcIndex] + ampGain - matrixLoss - margin);
                                                highLimit = (int)(-calAtten.srcAtten[srcIndex] + ampGain - matrixLoss + margin);
                                            }

                                            if ((calSetup[subIndex].calCalFactor < lowLimit) || (calSetup[subIndex].calCalFactor > highLimit)) calFail = true;

                                            // Write calibration results to console window
                                            OutputCalData(displayHeader, calRecord, "CAL", calSetup[subIndex].srcFreq, lowLimit, calSetup[subIndex].calCalFactor, highLimit);

                                            calRecord++;
                                        }
                                    }
                                }
                            }

                            calIndex = numOfCalRecords; // Force end of source cal loop
                        }
                    }
                }

                if (calFail)
                {
                    response = MessageBox.Show("User Calibration (Measure Path) FAILED!", "Merlin Test Technologies", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    if (response == DialogResult.Abort) return -1;
                    else if (response == DialogResult.Retry) retry = true;
                    else if (response == DialogResult.Ignore) retry = false;
                }

                Console.WriteLine();

            } while (retry == true);

            // Reset calibration instruments
            if (SigGen1 != null) SigGen1.MuteSigGen();
            modFilePlayingSigGen1 = false;
            currentModulationSigGen1 = "CW";

            if (SigGen2 != null) SigGen2.MuteSigGen();
            modFilePlayingSigGen2 = false;
            currentModulationSigGen2 = "CW";

            //Store calibration results to file
            status = WriteUserCalDataFile();

            // Inform user that user calibration is complete
            response = MessageBox.Show("RF user calibration complete!", "Merlin Test Technologies", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

            return 0;
        }

        private int InitializePowerMeter()
        {
            int status = -1;
            const int bufferSize = 256;
            DialogResult response = DialogResult.None;
            StringBuilder meterManuf = new StringBuilder();
            StringBuilder meterType = new StringBuilder();
            StringBuilder meterSerial = new StringBuilder();

            response = MessageBox.Show("Initializing power meter:\nPlease connect Power Meter to Tester PC.\nMake sure nothing is connected to Power Meter.", "Rohde & Schwarz", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            if (response == DialogResult.Cancel) return -1;

            Console.WriteLine("\nPower meter is initializing...");

            try // Initialize Power Meter
            {
                PowerMeter = new rsnrpz(PWR_METER_RESOURCE, true, true);
            }
            catch (Exception) // Power Meter did not initialize properly
            {
                Console.WriteLine("Power meter did not initialize\n");
                return -1;
                //ProgramExit();
            }

            CheckPowerMeterStatus();

            // Get serial numbers, verifiy resource is created
            status = PowerMeter.chan_info(1, "Manufacturer", bufferSize, meterManuf);
            status = PowerMeter.chan_info(1, "Type", bufferSize, meterType);
            status = PowerMeter.chan_info(1, "Serial", bufferSize, meterSerial);

            Console.WriteLine("Power meter initialized successfully");
            Console.WriteLine("Power meter manufacturer: {0}", meterManuf);
            Console.WriteLine("Power meter type: {0}", meterType);
            Console.WriteLine("Power meter serial number: {0}\n", meterSerial);

            // Zeroing
            status = PowerMeter.chan_zero(1);

            // Set aperature time (should be multiple of the waveform period)
            status = PowerMeter.chan_setContAvAperture(1, 200e-3);

            // Smoothing off
            status = PowerMeter.chan_setContAvSmoothingEnabled(1, false);

            // Deactivate automatic determination of filter bandwidth
            status = PowerMeter.avg_setAutoEnabled(1, false);

            // Set averaging filter mode (increase number for better repeatability)
            status = PowerMeter.avg_configureAvgManual(1, 4);

            // Number of averages
            status = PowerMeter.avg_setCount(1, 4);

            // Set trigger source to immediate
            status = PowerMeter.trigger_setSource(1, 3); //RSNRPZ_TRIGGER_SOURCE_IMMEDIATE

            // Check error state
            status = PowerMeter.errorCheckState(false);

            return 0;
        }

        private void CheckPowerMeterStatus()
        {
            int status = -1;
            int errorCode = -1;
            StringBuilder errorMessage = new StringBuilder();

            status = PowerMeter.error_query(out errorCode, errorMessage);

            if (errorCode != 0)
            {
                Console.WriteLine("Power Meter warning/error {0}: {1}", errorCode, errorMessage);
            }
        }

        private void OutputCalData(bool displayHeader, int calRecord, string port, double freq, double lowLimit, double result, double highLimit)
        {
            // Header
            if (displayHeader) Console.WriteLine("\nTest\tPort\tFrequency\tLow Limit\tResult\t\tHigh Limit\n");

            // Data
            Console.Write("{0}\t{1}\t", calRecord, port);
            if (freq < 1000e6) Console.Write(" "); // Column alignment
            if (freq < 100e6) Console.Write("  "); // Column alignment
            Console.Write("{0:F0} MHz\t{1:F1} dB\t", freq / 1e6, lowLimit);
            if ((lowLimit >= 0 && lowLimit < 10) || (lowLimit > 10) || (lowLimit < 0 && lowLimit >= -10)) Console.Write("\t"); // Column alignment
            Console.Write("{0:F3} dB\t{1:F1} dB", result, highLimit);
            //Console.Write("{0:F0} MHz\t{1:F1} dB\t{2:F3} dB\t{3:F1} dB", freq / 1e6, lowLimit, result, highLimit);
            //Console.Write("{0}\t{1}\t{2:F0} MHz\t{3:F1} dB\t{4:F3} dB\t{5:F1} dB", calRecord, currentMeasPath, calSetup[subIndex].srcFreq / 1e6, lowLimit, calSetup[subIndex].measCalFactor[measIndex], highLimit);
            if (result < lowLimit || result > highLimit) Console.Write(" <= (F)\n"); // Mark test failure
            else Console.Write("\n");
        }

        private void CalCapture(double frequency, double sampleRate, bool useFastPower, int numOfFastPowerMeasurements, double dwell, double measStepLength, double measOffset, double measLength, bool extTrig, ref double[] powerResult)
        {
            int status = -1;

            ulong counter = 0;
            int triggerDetect = 0;
            int captureComplete = 0;
            int overload = 0;

            int isFastPowerEnabled = 0;
            int numOfMeasurementsAvail = 0;
            int fastMeasLengthSamples = 0;
            int fastStepLengthSamples = 0;
            int fastOffsetLengthSamples = 0;
            double[] fastPowers = new double[numOfFastPowerMeasurements];

            uint numOfSamples = (uint)(dwell * sampleRate);
            uint preTrigSamples = (uint)(sampleRate * measOffset); // Need to capture guard samples before waveform starts

            status = Digitizer.ModulationGenericSamplingFrequency_Set(sampleRate);
            status = Digitizer.TriggerPreEdgeTriggerSamples_Set(preTrigSamples);

            if (extTrig) status = Digitizer.TriggerSource_Set(mtDigitizerDll_tsTrigSource_t.mtDigitizerDll_tsPXI_TRIG_0);
            else status = Digitizer.TriggerSource_Set(mtDigitizerDll_tsTrigSource_t.mtDigitizerDll_tsSW_TRIG);

            status = Digitizer.TriggerSwTriggerMode_Set(mtDigitizerDll_swtSwTrigMode_t.mtDigitizerDll_swtArmed);

            if (useFastPower)
            {
                status = Digitizer.CaptureIQPowerIsAvailable_Get(ref isFastPowerEnabled);
                if (isFastPowerEnabled == 0) Console.WriteLine("Fast Power is not supported!");

                fastMeasLengthSamples = (int)(measLength * sampleRate);
                fastStepLengthSamples = (int)(measStepLength * sampleRate);
                fastOffsetLengthSamples = (int)(measOffset * sampleRate);

                //if ((numOfFastPowerMeasurements * measStepLength) > dwell)
                //{
                //	Console.WriteLine("Fast Power requires the step length * the number of measurements to be less than or equal to the total dwell time.");
                //}

                //if ((measStepLength + measOffset) > measStepLength)
                //{
                //	Console.WriteLine("Fast Power requires the measurement offset and measurement length to be less than or equal to the step length.");
                //}

                fastMeasLengthSamples += (fastMeasLengthSamples % 4 == 0) ? 0 : (4 - (fastMeasLengthSamples % 4)); // Number of measurement samples must be a multiple of 4 (round up)

                if (fastMeasLengthSamples > fastStepLengthSamples) fastStepLengthSamples = fastMeasLengthSamples;

                status = Digitizer.CaptureIQPowerNumOfSteps_Set(numOfFastPowerMeasurements);
                Digitizer.CheckDigitizerStatus(status);

                status = Digitizer.CaptureIQPowerSetParameters(fastStepLengthSamples, fastOffsetLengthSamples, fastMeasLengthSamples);
                Digitizer.CheckDigitizerStatus(status);

                status = Digitizer.CaptureIQTriggerArm((uint)(fastStepLengthSamples) * (uint)(numOfFastPowerMeasurements));
                Util.WaitTime(1e-3);
            }

            // Wait for trigger detection
            counter = 0;
            triggerDetect = 0;
            do
            {
                status = Digitizer.TriggerDetected_Get(ref triggerDetect);
                Util.WaitTime(1e-6);
                counter++;
            } while ((triggerDetect == 0) && (counter < 1e6));

            if (triggerDetect == 0) Console.WriteLine("WARNING: No trigger detected!");

            // Wait for capture to complete
            counter = 0;
            captureComplete = 0;
            do
            {
                status = Digitizer.CaptureIQCaptComplete_Get(ref captureComplete); // Do not use Capture Complete if pipelining is enabled
                Util.WaitTime(1e-6);
                counter++;
            } while ((captureComplete == 0) && (counter < 1e6));

            if (captureComplete == 0) Console.WriteLine("WARNING: Incomplete data capture!");

            // Check for ADC overload
            status = Digitizer.CaptureIQADCOverload_Get(ref overload);
            if (overload != 0) Console.WriteLine("WARNING: ADC overload!");

            // Measure power
            if (useFastPower && (isFastPowerEnabled != 0))
            {
                status = Digitizer.CaptureIQPowerNumMeasurementsAvail_Get(ref numOfMeasurementsAvail);

                if (numOfMeasurementsAvail >= numOfFastPowerMeasurements)
                {
                    //status = Digitizer.Capture.IQ.Power.GetSingleMeasurement(1, ref fastPowers[0]);
                    status = Digitizer.CaptureIQPowerGetAllMeasurements(numOfFastPowerMeasurements, fastPowers);

                    for (int i = 0; i < numOfFastPowerMeasurements; i++)
                    {
                        powerResult[i] = fastPowers[i];
                    }
                }
            }
            else // Not using Fast Power
            {
                float[] iData = new float[numOfSamples]; // Capture I data
                float[] qData = new float[numOfSamples]; // Capture Q data

                //float sumI2Q2 = 0;
                double correction = 0;
                status = Digitizer.RFLevelCorrection_Get(ref correction);
                status = Digitizer.CaptureIQCaptMem(numOfSamples, iData, qData);
                //sumI2Q2 = linearPowerSum(numOfSamples, iData, qData);
                //powerResult[0] = powerIndBm(numOfSamples, sumI2Q2, correction);
                powerResult[0] = PowerIQ(iData, qData, numOfSamples, correction);
            }

            // Plot captured IQ data (debug)
            bool debugDataPlot = false;
            if (debugDataPlot)
            {
                double correction = 0;
                float[] iData = new float[numOfSamples]; // Capture I data
                float[] qData = new float[numOfSamples]; // Capture Q data

                status = Digitizer.RFLevelCorrection_Get(ref correction);
                status = Digitizer.CaptureIQCaptMem(numOfSamples, iData, qData);
                Digitizer.CheckDigitizerStatus(status);

                // Plot power (digitizer front end) ---------------------------------
                double[] iqPower = new double[fastStepLengthSamples];
                for (uint i = 0; i < fastStepLengthSamples; i++)
                {
                    iqPower[i] = 10 * Math.Log10((iData[i] * iData[i]) + (qData[i] * qData[i])) /*- dutCalFactorOut*/ + correction;
                }

                // Plot power (DUT output) ------------------------------------------
                double[] iqPowerDUT = new double[fastStepLengthSamples];
                for (uint i = 0; i < fastStepLengthSamples; i++)
                {
                    iqPowerDUT[i] = 10 * Math.Log10((iData[i] * iData[i]) + (qData[i] * qData[i])) /*- dutCalFactorOut*/ + correction;
                }


            }
        }

        private double PowerIQ(float[] iData, float[] qData, uint samples, double correction)
        {
            // Calculate I^2 + Q^2 sum
            // Calculate dBm power from I^2 + Q^2 sum

            float sum = 0f;
            double power = 0;

            for (uint i = 0; i < samples; i++)
            {
                sum += (iData[i] * iData[i] + qData[i] * qData[i]);
            }

            power = 10 * Math.Log10(sum / samples) + correction;

            return power;
        }
        #endregion
    }

    class Program
    {
        static void Main()
        {
            
        }
    }
}
