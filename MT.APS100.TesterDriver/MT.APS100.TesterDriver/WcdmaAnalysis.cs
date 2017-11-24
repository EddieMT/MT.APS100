using System;
using Aeroflex.PXI.Analysis;
using Aeroflex.PXI.Analysis.Common;

namespace MT.APS100.TesterDriver
{
    public class WcdmaAnalysis
    {
        public UmtsUplink Wcdma { get; set; }
        public int numOfSamples { get; set; }
        public float[] iData { get; set; }
        public float[] qData { get; set; }
        public int numOfChans { get; set; }
        public int numOfSlots { get; set; }
        //public int numOfSlotsToMeas { get; set; }
        public double digSpan { get; set; }
        public double sampleFreq { get; set; }
        public double centerFreq { get; set; }
        public double[] aclrChanFreqs { get; set; }
        public float[] aclrLevels { get; set; }
        public float aclr_C { get; set; }
        public float aclr_L2 { get; set; }
        public float aclr_L1 { get; set; }
        public float aclr_U1 { get; set; }
        public float aclr_U2 { get; set; }
        public float correction { get; set; }
        public double chanSpacing = 0;
        public double chanBandwidth = 0;
        public double measSpanLTE = 0;
        public float power { get; set; }
        public float evmRms { get; set; }
        public double tag { get; set; }
        public double key { get; set; }
        public UmtsUplinkMeasurement measurements { get; set; }
        public bool threadComplete { get; set; }

        public WcdmaAnalysis()
        {
            Wcdma = new UmtsUplink();
        }

        ~WcdmaAnalysis()
        {
            Wcdma.DestroyObject();
        }

        public void AnalysisSetup()
        {
            aclrChanFreqs = new double[numOfChans];
            aclrChanFreqs[0] = 0;
            aclrChanFreqs[1] = -5e6;
            aclrChanFreqs[2] = 5e6;
            aclrChanFreqs[3] = -5e6 * 2;
            aclrChanFreqs[4] = 5e6 * 2;

            Wcdma.Configuration.SetAclrFrequencies(aclrChanFreqs);
            Wcdma.Configuration.AnalysisMode = UmtsUplinkAnaMode.RandomSlot;
            Wcdma.Configuration.AnalysisRegion = UmtsUplinkAnalysisRegion.FullSlot;
            Wcdma.Configuration.CDESpreadingFactor = 256;
            Wcdma.Configuration.ChannelDetectMode = UmtsUplinkChannelDetectMode.AutoCase1;
            Wcdma.Configuration.DigitizerSpan = digSpan;
            Wcdma.Configuration.MeasurementSpan = numOfChans * 5e6;
            Wcdma.Configuration.RfLevelCal = correction;
            Wcdma.Configuration.SamplingFreq = sampleFreq;
            Wcdma.Configuration.ScramblingCode = 0;

            iData = new float[numOfSamples];
            qData = new float[numOfSamples];

            //------------------------- Get Setup Constraints (for debug and development purposes) -------------------------
            /*
            double samplingFreq = 0;
            double scramblingCode = 0;
            UmtsUplinkAnaMode analysisMode = 0;
            double cdeSpreadingFactor = 0;

            samplingFreq = Wcdma.Configuration.SamplingFreq;
            scramblingCode = Wcdma.Configuration.ScramblingCode;
            analysisMode = Wcdma.Configuration.AnalysisMode;
            cdeSpreadingFactor = Wcdma.Configuration.CDESpreadingFactor;

            Console.WriteLine("Setup parameters for WCDMA {0} MHz", centerFreq / 1e6);
            Console.WriteLine("Sampling frequency = {0} MHz", samplingFreq / 1e6);
            Console.WriteLine("Scrambling code = {0}", scramblingCode);
            Console.WriteLine("Analysis mode = {0}", analysisMode);
            Console.WriteLine("CDE spreading factor = {0}", cdeSpreadingFactor);
            */
        }

        public void WcdmaRefLevelSet(float refLevel)
        {
            this.correction = refLevel;
            Wcdma.Configuration.RfLevelCal = refLevel;
        }

        public void WcdmaDigSpanSet(double span)
        {
            this.digSpan = span;
            Wcdma.Configuration.DigitizerSpan = span;
        }

        public void WcdmaCenterFreq(double freq)
        {
            this.centerFreq = freq;
            Wcdma.Configuration.SpectrumFreqAxisCentre = freq;
        }

        public void SampleSize(int samples)
        {
            this.numOfSamples = samples;
            iData = new float[samples];
            qData = new float[samples];
        }

        public int Analyze()
        {
            Wcdma.Configuration.RfLevelCal = correction;

            try
            {
                Wcdma.Analyse((Aeroflex.PXI.Analysis.UmtsUplinkMeasurement)measurements, iData, qData, 0, iData.Length);

                if (measurements == UmtsUplinkMeasurement.ComputeACLR)
                {
                    aclrLevels = Wcdma.Results.GetAclr(aclrChanFreqs.Length);
                }
                else if (measurements == (UmtsUplinkMeasurement.ComputeACLR | UmtsUplinkMeasurement.ModAccuracy))
                {
                    aclrLevels = Wcdma.Results.GetAclr(aclrChanFreqs.Length);
                    evmRms = Wcdma.Results.CompositeEvmRms;
                }

                if (numOfChans == 3)
                {
                    aclr_C = aclrLevels[0];
                    aclr_L1 = aclrLevels[1];
                    aclr_U1 = aclrLevels[2];
                }

                else if (numOfChans == 5)
                {
                    aclr_C = aclrLevels[0];
                    aclr_L2 = aclrLevels[1];
                    aclr_L1 = aclrLevels[2];
                    aclr_U1 = aclrLevels[3];
                    aclr_U2 = aclrLevels[4];
                }

                return 0;
            }
            catch (AnalysisErrorException ex)
            {
                Console.WriteLine("WCDMA analysis error exception: " + ex.Message);
                return -1;
            }
            catch (AnalysisException ex)
            {
                Console.WriteLine("WCDMA analysis exception: " + ex.Message);
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("WCDMA exception: " + ex.Message);
                return -1;
            }
        }
    }
}
