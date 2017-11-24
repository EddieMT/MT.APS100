using System;
using Aeroflex.PXI.Analysis;
using Aeroflex.PXI.Analysis.Common;

namespace MT.APS100.TesterDriver
{
    public enum AclrMode { Utra, Eutra };

    public class LteAnalysis
    {
        public LteUplink LteFdd { get; set; }
        public int numOfSamples { get; set; }
        public float[] iData { get; set; }
        public float[] qData { get; set; }
        public float eutraBW { get; set; }
        public int numOfChans { get; set; }
        public int numOfSlots { get; set; }
        //public int numOfSlotsToMeas { get; set; }
        public double digSpan { get; set; }
        public double sampleFreq { get; set; }
        public double centerFreq { get; set; }
        public double[] aclrChanFreqs { get; set; }
        public double[] aclrChanBWs { get; set; }
        public float[] aclrChanAlphas { get; set; }
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
        public double evmRms { get; set; }
        public AclrMode mode { get; set; }
        public LteMeasurement measurements { get; set; }
        public bool threadComplete { get; set; }

        public LteAnalysis()
        {
            LteFdd = new LteUplink();
        }

        ~LteAnalysis()
        {
            LteFdd.DestroyObject();
        }

        public void AnalysisSetup()
        {
            int eutraBWSelect = (int)(eutraBW * 10);

            // ACP settings for generic E-UTRA setup
            chanSpacing = eutraBW * 1e6;
            chanBandwidth = 0.9 * eutraBW * 1e6;
            measSpanLTE = 0;

            //------------------------- Setup LTE-FDD E-UTRA ACLR Parameters -------------------------

            if (mode == AclrMode.Eutra)
            {
                #region LTE E-UTRA ACLR Setup

                // Setup ACP
                aclrChanBWs = new double[numOfChans];   // Spectrum bandwidth to measure
                aclrChanFreqs = new double[numOfChans]; // Spectrum offset frequencies to measure
                aclrChanAlphas = new float[numOfChans]; // Spectrum alpha to measure

                if (numOfChans == 1)
                {
                    aclrChanBWs[0] = chanBandwidth; //E-UTRA Carrier BW

                    aclrChanFreqs[0] = 0;

                    measSpanLTE = eutraBW * 1e6;
                }
                else if (numOfChans == 3)
                {
                    aclrChanBWs[0] = chanBandwidth; //E-UTRA Carrier BW
                    aclrChanBWs[1] = chanBandwidth; //E-UTRA Carrier BW
                    aclrChanBWs[2] = chanBandwidth; //E-UTRA Carrier BW

                    aclrChanFreqs[0] = 0;
                    aclrChanFreqs[1] = -chanSpacing;
                    aclrChanFreqs[2] = chanSpacing;

                    measSpanLTE = (numOfChans - 1) * chanSpacing + chanBandwidth;
                }
                else if (numOfChans == 5)
                {
                    aclrChanBWs[0] = chanBandwidth; //E-UTRA Carrier BW
                    aclrChanBWs[1] = chanBandwidth; //E-UTRA Carrier BW
                    aclrChanBWs[2] = chanBandwidth; //E-UTRA Carrier BW
                    aclrChanBWs[3] = chanBandwidth; //E-UTRA Carrier BW
                    aclrChanBWs[4] = chanBandwidth; //E-UTRA Carrier BW

                    aclrChanFreqs[0] = 0;
                    aclrChanFreqs[1] = -2 * chanSpacing;
                    aclrChanFreqs[2] = -chanSpacing;
                    aclrChanFreqs[3] = chanSpacing;
                    aclrChanFreqs[4] = 2 * chanSpacing;
                }

                measSpanLTE = (numOfChans - 1) * chanSpacing + chanBandwidth;

                if (eutraBW == 20) digSpan = 100e6; // Special case-- we can push bandwidth with minimal rolloff at 98MHz for this ACP case

                #endregion LTE E-UTRA ACLR Setup
            }
            else
            {
                #region LTE UTRA ACLR Setup

                // Setup ACP
                aclrChanBWs = new double[numOfChans];   // Spectrum bandwidth to measure
                aclrChanFreqs = new double[numOfChans]; // Spectrum offset frequencies to measure
                aclrChanAlphas = new float[numOfChans]; // Spectrum alpha to measure

                if (numOfChans == 1)
                {
                    aclrChanBWs[0] = chanBandwidth;

                    aclrChanFreqs[0] = 0;

                    aclrChanAlphas[0] = 0;

                    measSpanLTE = eutraBW * 1e6;
                }
                else if (numOfChans == 3)
                {
                    aclrChanBWs[0] = chanBandwidth;
                    aclrChanBWs[1] = 3.84e6;
                    aclrChanBWs[2] = 3.84e6;

                    aclrChanFreqs[0] = 0;
                    aclrChanFreqs[1] = -(eutraBW * 1e6) / 2 - (5e6 / 2);
                    aclrChanFreqs[2] = (eutraBW * 1e6) / 2 + (5e6 / 2);

                    aclrChanAlphas[0] = 0;
                    aclrChanAlphas[1] = 0.22f;
                    aclrChanAlphas[2] = 0.22f;

                    measSpanLTE = 2 * ((eutraBW * 1e6) / 2 + 3 * (5e6 / 2)) + 3.84e6;
                }
                else if (numOfChans == 5)
                {
                    aclrChanBWs[0] = chanBandwidth;
                    aclrChanBWs[1] = 3.84e6;
                    aclrChanBWs[2] = 3.84e6;
                    aclrChanBWs[3] = 3.84e6;
                    aclrChanBWs[4] = 3.84e6;

                    aclrChanFreqs[0] = 0;
                    aclrChanFreqs[1] = -(eutraBW * 1e6) / 2 - 3 * (5e6 / 2);
                    aclrChanFreqs[2] = -(eutraBW * 1e6) / 2 - (5e6 / 2);
                    aclrChanFreqs[3] = (eutraBW * 1e6) / 2 + (5e6 / 2);
                    aclrChanFreqs[4] = (eutraBW * 1e6) / 2 + 3 * (5e6 / 2);

                    aclrChanAlphas[0] = 0;
                    aclrChanAlphas[1] = 0.22f;
                    aclrChanAlphas[2] = 0.22f;
                    aclrChanAlphas[3] = 0.22f;
                    aclrChanAlphas[4] = 0.22f;

                    measSpanLTE = 2 * ((eutraBW * 1e6) / 2 + 3 * (5e6 / 2)) + 3.84e6;
                }

                #endregion LTE UTRA ACLR Setup
            }

            LteFdd.Configuration.SetAclrConfig(aclrChanBWs, aclrChanFreqs, aclrChanAlphas);

            if (centerFreq < 1e9) digSpan = 36e6;
            else digSpan = 90e6;

            LteFdd.Configuration.DigitizerSpan = digSpan;
            LteFdd.Configuration.SamplingFreq = sampleFreq;
            //LteFdd.Configuration.RfLevelCal = (float)correction;

            switch (eutraBWSelect)
            {
                case 14:
                    LteFdd.Configuration.Bandwidth = LteBandwidth.Bandwidth1M4Hz;
                    break;
                case 30:
                    LteFdd.Configuration.Bandwidth = LteBandwidth.Bandwidth3MHz;
                    break;
                case 50:
                    LteFdd.Configuration.Bandwidth = LteBandwidth.Bandwidth5MHz;
                    break;
                case 100:
                    LteFdd.Configuration.Bandwidth = LteBandwidth.Bandwidth10MHz;
                    break;
                case 150:
                    LteFdd.Configuration.Bandwidth = LteBandwidth.Bandwidth15MHz;
                    break;
                case 200:
                    LteFdd.Configuration.Bandwidth = LteBandwidth.Bandwidth20MHz;
                    break;
                default:
                    Console.WriteLine("E-UTRA bandwidth specified is not supported!");
                    break;
            }

            LteFdd.Configuration.SpectrumFreqAxisCentre = centerFreq;
            LteFdd.Configuration.PuschPresent = true;
            LteFdd.Configuration.NumSlotsToAnalyse = numOfSlots;
            LteFdd.Configuration.NumSlotsToAnalyseIncludeAll = false;
            LteFdd.Configuration.EvmWindowPosition = LteEvmWindowPosition.EvmWindowPositionMiddle;
            LteFdd.Configuration.HalfSubcarrierShift = true;
            LteFdd.Configuration.MeasurementSpan = measSpanLTE;
            LteFdd.Configuration.CellID = 0;
            LteFdd.Configuration.PuschDmrsDeltaSS = 0;
            LteFdd.Configuration.AnalysisMode = LteUplinkAnalysisMode.RandomSlot;
            LteFdd.Configuration.ResourceBlockAutoDetect = true;
            LteFdd.Configuration.TrackingPhase = true;

            // Not used but could be for specific waveform configurations (below is hard coded)
            /*
            bool RBAutoDetect;
            RBAutoDetect = LteEutra[i].Configuration.ResourceBlockAutoDetect;

            if (!RBAutoDetect)
            {
                for (int slotIndex = 0; slotIndex < 20; slotIndex++)
                {
                    // If auto detect is off set the slot parameters: slot, rb offset, num of rbs, mod type, dmrss
                    LteEutra[i].Configuration.AddPuschWithParams(slotIndex, 0, 10, LteModType.LteModType_Qpsk, 0);
                }
            }
            */

            iData = new float[numOfSamples];
            qData = new float[numOfSamples];

            //------------------------- Get Setup Constraints (for debug and development purposes) -------------------------
            /*
            double minMeasSpanEVM = 0;
            double minMeasSpanACLR = 0;
            double minSampFreqEVM = 0;
            double minSampFreqACLR = 0;
            double recommendedSampFreqEVM = 0;
            double recommendedSampFreqACLR = 0;

            // E-UTRA
            minMeasSpanEVM = LteFdd.Configuration.GetMinMeasurementSpan(LteMeasurement.MeasModAccuracy);
            minMeasSpanACLR = LteFdd.Configuration.GetMinMeasurementSpan(LteMeasurement.MeasAclr);
            minSampFreqEVM = LteFdd.Configuration.GetMinSamplingFreq(LteMeasurement.MeasModAccuracy);
            minSampFreqACLR = LteFdd.Configuration.GetMinSamplingFreq(LteMeasurement.MeasAclr);
            recommendedSampFreqEVM = LteFdd.Configuration.GetRecommendedSamplingFreq(LteMeasurement.MeasModAccuracy);
            recommendedSampFreqACLR = LteFdd.Configuration.GetRecommendedSamplingFreq(LteMeasurement.MeasAclr);

            Console.WriteLine("Setup parameters for LTE-FDD E-UTRA {0} MHz", centerFreq / 1e6);
            Console.WriteLine("Minimum measurement span for EVM = {0} MHz", minMeasSpanEVM / 1e6);
            Console.WriteLine("Minimum measurement span for ACLR = {0} MHz", minMeasSpanACLR / 1e6);
            Console.WriteLine("Minimum sampling frequency for EVM = {0} MHz", minSampFreqEVM / 1e6);
            Console.WriteLine("Minimum sampling frequency for ACLR = {0} MHz", minSampFreqACLR / 1e6);
            Console.WriteLine("Recommended sampling frequency for EVM = {0} MHz", recommendedSampFreqEVM / 1e6);
            Console.WriteLine("Recommended sampling frequency for ACLR = {0} MHz\n", recommendedSampFreqACLR / 1e6);

            // UTRA
            minMeasSpanEVM = LteFdd.Configuration.GetMinMeasurementSpan(LteMeasurement.MeasModAccuracy);
            minMeasSpanACLR = LteFdd.Configuration.GetMinMeasurementSpan(LteMeasurement.MeasAclr);
            minSampFreqEVM = LteFdd.Configuration.GetMinSamplingFreq(LteMeasurement.MeasModAccuracy);
            minSampFreqACLR = LteFdd.Configuration.GetMinSamplingFreq(LteMeasurement.MeasAclr);
            recommendedSampFreqEVM = LteFdd.Configuration.GetRecommendedSamplingFreq(LteMeasurement.MeasModAccuracy);
            recommendedSampFreqACLR = LteFdd.Configuration.GetRecommendedSamplingFreq(LteMeasurement.MeasAclr);

            Console.WriteLine("Setup parameters for LTE-FDD UTRA {0} MHz", centerFreq / 1e6);
            Console.WriteLine("Minimum measurement span for EVM = {0} MHz", minMeasSpanEVM / 1e6);
            Console.WriteLine("Minimum measurement span for ACLR = {0} MHz", minMeasSpanACLR / 1e6);
            Console.WriteLine("Minimum sampling frequency for EVM = {0} MHz", minSampFreqEVM / 1e6);
            Console.WriteLine("Minimum sampling frequency for ACLR = {0} MHz", minSampFreqACLR / 1e6);
            Console.WriteLine("Recommended sampling frequency for EVM = {0} MHz", recommendedSampFreqEVM / 1e6);
            Console.WriteLine("Recommended sampling frequency for ACLR = {0} MHz\n", recommendedSampFreqACLR / 1e6);
            */
        }

        public void LteRefLevelSet(float refLevel)
        {
            this.correction = refLevel;
            LteFdd.Configuration.RfLevelCal = refLevel;
        }

        public void LteDigSpanSet(double span)
        {
            this.digSpan = span;
            LteFdd.Configuration.DigitizerSpan = span;
        }

        public void LteCenterFreq(double freq)
        {
            this.centerFreq = freq;
            LteFdd.Configuration.SpectrumFreqAxisCentre = freq;
        }

        public void SampleSize(int samples)
        {
            this.numOfSamples = samples;
            iData = new float[samples];
            qData = new float[samples];
        }

        public int Analyze()
        {
            LteFdd.Configuration.RfLevelCal = correction;

            try
            {
                LteFdd.Analyse((Aeroflex.PXI.Analysis.Common.LteMeasurement)measurements, iData, qData, 0, iData.Length);

                if (measurements == LteMeasurement.MeasAclr)
                {
                    aclrLevels = LteFdd.Results.GetAclrResults();
                }
                else if (measurements == (LteMeasurement.MeasAclr | LteMeasurement.MeasModAccuracy))
                {
                    aclrLevels = LteFdd.Results.GetAclrResults();
                    evmRms = LteFdd.Results.EvmRms;
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
                Console.WriteLine("LTE analysis error exception: " + ex.Message);
                return -1;
            }
            catch (AnalysisException ex)
            {
                Console.WriteLine("LTE analysis exception: " + ex.Message);
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LTE exception: " + ex.Message);
                return -1;
            }
        }
    }
}
