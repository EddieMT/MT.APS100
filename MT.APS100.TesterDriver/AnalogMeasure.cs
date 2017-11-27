using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.DAQmx;

namespace MT.APS100.TesterDriver
{
    public class AnalogMeasure
    {
        private bool writeToConsole = false;

        public NationalInstruments.DAQmx.Device myDevice;
        public NationalInstruments.DAQmx.Task myTask;       // Main Task variable which gets called in the Main Function
        public NationalInstruments.DAQmx.Task myVTask;
        public AnalogMultiChannelReader reader;
        public AnalogMultiChannelReader readerV;
        public int totalSamples { get; set; }               // Global container for the number of samples to acquire
        public int acquiredSamplesCount { get; set; }       // Iteration variable which holds the number of samples currently acquired  
        public string NI6284 = "Dev1";
        public string productType { get; set; }
        public long productNumber { get; set; }
        public NationalInstruments.DAQmx.ProductCategory productCat { get; set; }
        public long PXIChassisNumber { get; set; }
        public long PXISlotNumber { get; set; }
        public long serialNo { get; set; }
        public string[] myChannels { get; set; }

        public int numOfChannels { get; set; }
        public string[] channels { get; set; }
        public double sampleRate { get; set; }
        public double[] rangeMins { get; set; }
        public double[] rangeMaxs { get; set; }
        public double[] senseOhms { get; set; }
        public bool[] invertChan { get; set; }
        public string startTrigSource { get; set; }
        public AITerminalConfiguration AIinput { get; set; }
        public double[,] data { get; set; }
        public double[,] dataV { get; set; }

        public int numOfVChannels { get; set; }
        public string[] channelsV { get; set; }
        public double[] rangeVMins { get; set; }
        public double[] rangeVMaxs { get; set; }
        public AITerminalConfiguration AIVinput { get; set; }

        public AnalogMeasure()
        {
            writeToConsole = false;
        }

        public AnalogMeasure(string ID)
        {
            NI6284 = ID;
            writeToConsole = true;
        }

        public AnalogMeasure(string ID, bool verbose)
        {
            NI6284 = ID;
            writeToConsole = verbose;
        }

        ~ AnalogMeasure()
        {
            //myTask.Dispose();
            
        }

        //check module and get module info
        public int Initialize()
        {
            if (writeToConsole) Console.WriteLine("AI module is booting...");
            serialNo = -999;
            myDevice = NationalInstruments.DAQmx.DaqSystem.Local.LoadDevice(NI6284);
            productType = myDevice.ProductType;
            productNumber = myDevice.ProductNumber;
            productCat = myDevice.ProductCategory;
            PXIChassisNumber = myDevice.PxiChassisNumber;
            PXISlotNumber = myDevice.PxiSlotNumber;
            serialNo = myDevice.SerialNumber;

            myChannels = myDevice.GetPhysicalChannels(PhysicalChannelTypes.All, PhysicalChannelAccess.All);

            if (writeToConsole) Console.WriteLine("AI module serial number: {0}\n", serialNo);

            if (serialNo == -999)
            {
                if (writeToConsole) Console.WriteLine("AI module did not boot.\n");
                return -1;
            }
            else
                return 0;
        }

        //setup a current measurement
        public void setupMeasurement()
        {
            string channel = "";

            try
            {
                myTask = new NationalInstruments.DAQmx.Task();

                // Create a channels
                for (int i = 0; i < numOfChannels; i++)
                {
                    double senseValue = senseOhms[i];
                    channel = string.Concat(NI6284, "/", channels[i]);
                    //myTask.AIChannels.CreateVoltageChannel(channel, "", AIinput, rangeMins[i], rangeMaxs[i], NationalInstruments.DAQmx.AIVoltageUnits.Volts);

                    myTask.AIChannels.CreateCurrentChannel(channel, "", NationalInstruments.DAQmx.AITerminalConfiguration.Differential, rangeMins[i], rangeMaxs[i], senseValue, AICurrentUnits.Amps);
                }

                // Configure Timing Specs    
                myTask.Timing.ConfigureSampleClock("", sampleRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, totalSamples);

                try
                {
                    myTask.Control(TaskAction.Verify);  //validates setup is ok
                }
                catch (DaqException exception)
                {
                    myTask.Dispose();
                    Console.WriteLine(exception.Message);
                }

                reader = new AnalogMultiChannelReader(myTask.Stream);  //will get data back through this
                data = new double[numOfChannels, totalSamples];  //data container
            }
            catch (DaqException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        //setup a voltage measurement
        public void setupVMeasurement()
        {
            string channel = "";

            try
            {
                myVTask = new NationalInstruments.DAQmx.Task();

                // Create a channels
                for (int i = 0; i < numOfVChannels; i++)
                {
                    channel = string.Concat(NI6284, "/", channelsV[i]);
                    myVTask.AIChannels.CreateVoltageChannel(channel, "", (NationalInstruments.DAQmx.AITerminalConfiguration)AIVinput, rangeVMins[i], rangeVMaxs[i], NationalInstruments.DAQmx.AIVoltageUnits.Volts);
                }

                // Configure Timing Specs    
                myVTask.Timing.ConfigureSampleClock("", sampleRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, totalSamples);

                try
                {
                    myVTask.Control(TaskAction.Verify);  //validates setup is ok
                }
                catch (DaqException exception)
                {
                    myVTask.Dispose();
                    Console.WriteLine(exception.Message);
                }

                readerV = new AnalogMultiChannelReader(myVTask.Stream);  //will get data back through this
                dataV = new double[numOfVChannels, totalSamples];  //data container
            }
            catch (DaqException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }


        //setup current sample clock and number of samples to take
        public void setSampleClockandSamples()
        {
            // Configure Timing Specs    
            myTask.Timing.ConfigureSampleClock("", sampleRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, totalSamples);

            data = new double[numOfChannels, totalSamples];
        }

        //setup voltage sample clock and number of samples to take
        public void setVSampleClockandSamples()
        {
            // Configure Timing Specs    
            myVTask.Timing.ConfigureSampleClock("", sampleRate, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, totalSamples);

            dataV = new double[numOfVChannels, totalSamples];
        }

        //setup current start trigger
        public void setTriggerSource()
        {
            if(startTrigSource == "None")
                myTask.Triggers.StartTrigger.ConfigureNone();
            else
                myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(startTrigSource, DigitalEdgeStartTriggerEdge.Rising);
        }

        //setup voltage start trigger
        public void setVTriggerSource()
        {
            if (startTrigSource == "None")
                myVTask.Triggers.StartTrigger.ConfigureNone();
            else
                myVTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(startTrigSource, DigitalEdgeStartTriggerEdge.Rising);
        }

        //setups current hardware to be ready for measurement
        public void commitAI()
        {
            myTask.Control(TaskAction.Commit);
        }

        //starts current measurement
        public void startAI()
        {
            try
            {
                myTask.Control(TaskAction.Start);
            }
            catch (DaqException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        //stops current measurement
        public void stopAI()
        {
            myTask.Control(TaskAction.Stop);
        }

        //clears out all measurement tasks so you can setup a new AI
        public void clearSetupAI()
        {
            myTask.Dispose();
            //myVTask.Dispose();
        }

        //get current measured samples back
        public double[] getCurrentAverage()
        {
            double[] averageCurrent = new double[numOfChannels];
            double[] dataSingle = new double[numOfChannels];

            //try
            //{
            //    acquiredSamplesCount = 0;
            //    do
            //    {

            //        //Read the data from the channels
            //        dataSingle = reader.ReadSingleSample();
            //        for (int j = 0; j < numOfChannels; j++)
            //        {
            //            if (invertChan[j])
            //                data[j, acquiredSamplesCount] = -dataSingle[j];
            //            else
            //                data[j, acquiredSamplesCount] = dataSingle[j];
            //            averageCurrent[j] += data[j, acquiredSamplesCount];
            //        }
            //        acquiredSamplesCount++;
            //    }
            //    while (acquiredSamplesCount < totalSamples);
            //}
            //catch (DaqException exception)
            //{
            //    Console.WriteLine(exception.Message);
            //}

            try
            {
                data = reader.ReadMultiSample(totalSamples);
            }
            catch (DaqException exception)
            {
                Console.WriteLine(exception.Message);
            }

            for (int i = 0; i < totalSamples; i++)
            {
                for (int j = 0; j < numOfChannels; j++)
                {
                    if (invertChan[j])
                        data[j, i] = -data[j, i];
                    else
                        data[j, i] = data[j, i];

                    averageCurrent[j] += data[j, i];
                }
            }

            for (int j = 0; j < numOfChannels; j++)
                averageCurrent[j] /= totalSamples;

            return averageCurrent;
        }

        //get voltage measured samples back
        public double[] getVoltageAverage()
        {
            double[] averageVoltage = new double[numOfVChannels];
            double[] dataSingle = new double[numOfChannels];

            //try
            //{
            //    acquiredSamplesCount = 0;
            //    do
            //    {

            //        //Read the data from the channels
            //        dataSingle = reader.ReadSingleSample();
            //        for (int j = 0; j < numOfChannels; j++)
            //        {
            //            if (invertChan[j])
            //                data[j, acquiredSamplesCount] = -dataSingle[j];
            //            else
            //                data[j, acquiredSamplesCount] = dataSingle[j];
            //            averageCurrent[j] += data[j, acquiredSamplesCount];
            //        }
            //        acquiredSamplesCount++;
            //    }
            //    while (acquiredSamplesCount < totalSamples);
            //}
            //catch (DaqException exception)
            //{
            //    Console.WriteLine(exception.Message);
            //}

            try
            {
                data = readerV.ReadMultiSample(totalSamples);
            }
            catch (DaqException exception)
            {
                Console.WriteLine(exception.Message);
            }

            for (int i = 0; i < totalSamples; i++)
            {
                for (int j = 0; j < numOfVChannels; j++)
                {
                    data[j, i] = data[j, i];
                    averageVoltage[j] += data[j, i];
                }
            }

            for (int j = 0; j < numOfVChannels; j++)
                averageVoltage[j] /= totalSamples;

            return averageVoltage;
        }

    }
}
