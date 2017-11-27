using System;
using Aeroflex.PXI.Analysis;
using Aeroflex.PXI.Analysis.Common;

namespace MT.APS100.TesterDriver
{
    public class TdscdmaAnalysis
    {
        public Tdscdma TdscdmaObject { get; set; }
        public int objID { get; set; }
        public int numOfChans { get; set; }
        public int numOfSamples { get; set; }
        public float[] iData { get; set; }
        public float[] qData { get; set; }
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
        public float power { get; set; }
        public double evmRms { get; set; }
        public TdscdmaMeasurement measurements { get; set; }
        public bool threadComplete { get; set; }

        public TdscdmaAnalysis()
        {
            TdscdmaObject = new Tdscdma();
        }

        ~TdscdmaAnalysis()
        {
            TdscdmaObject.DestroyObject();
        }

        public void AnalysisSetup()
        {
            uint numOfChans = 5;
            aclrChanFreqs = new double[numOfChans];

            //------------------------- Setup TD-SCDMA ACLR Parameters -------------------------

            aclrChanFreqs[0] = -1.6e6 * 2;
            aclrChanFreqs[1] = -1.6e6;
            aclrChanFreqs[2] = 0;
            aclrChanFreqs[3] = 1.6e6;
            aclrChanFreqs[4] = 1.6e6 * 2;

            TdscdmaObject.Configuration.AclrMode = TdscdmaAclrMode.AclrModeDefault;
            TdscdmaObject.Configuration.SetAclrUserFreqs(aclrChanFreqs);
            TdscdmaObject.Configuration.BasicMidambleCodeAutoDetect = true;
            TdscdmaObject.Configuration.ChannelConfigAutoDetect = true;
            TdscdmaObject.Configuration.ChannelThresholdAutoDetect = true;
            TdscdmaObject.Configuration.ForceCodeGroup = TdscdmaForceCodeGroup.AnyCode;
            TdscdmaObject.Configuration.ScrambleCode = 0;
            TdscdmaObject.Configuration.SyncDLCode = 0;
            TdscdmaObject.Configuration.SyncULCode = 0;
            TdscdmaObject.Configuration.UplinkSwitchPoint = TdscdmaTimeSlot.Ts1;
            //TdscdmaObject.Configuration.RfLevelCal = correction;
            TdscdmaObject.Configuration.SamplingFreq = sampleFreq;
            TdscdmaObject.Configuration.SlotFreqRef = TdscdmaSlotFrequencyRef.UpPtsFreqRef;
            TdscdmaObject.Configuration.SlotThreshold = -30;
            TdscdmaObject.Configuration.SlotTimeRef = TdscdmaSlotTimeRef.UpPtsTimeRef;
            TdscdmaObject.Configuration.SlotToAnalyse = TdscdmaTimeSlot.Ts1;
            TdscdmaObject.Configuration.SyncMode = TdscdmaSyncMode.UpPtsSync;
            TdscdmaObject.Configuration.SpectrumFreqAxisCentre = centerFreq;
            TdscdmaObject.Configuration.SpectrumFreqAxisScaleUnits = FreqUnits.MHz;
            TdscdmaObject.Configuration.SpectrumMaskType = TdscdmaSpectrumMask.SpectrumMaskStdRel7;
            TdscdmaObject.Configuration.TransmitMaskNumSlots = 1;
            TdscdmaObject.Configuration.TransmitMaskAveragingEnabled = false;

            iData = new float[numOfSamples];
            qData = new float[numOfSamples];

            //------------------------- Get Setup Constraints (for debug and development purposes) -------------------------

            //double minMeasSpanEVM = 0;
            //double minMeasSpanACLR = 0;

            //minMeasSpanEVM = TdscdmaObject.Configuration.GetMinSamplingFreq(TdscdmaMeasurement.MeasAclr, out sampleFreq);
            //minMeasSpanACLR = TdscdmaObject.Configuration.GetMinSamplingFreq(TdscdmaMeasurement.MeasModAccuracy, out sampleFreq);

            //Console.WriteLine("Setup parameters for TD-SCDMA {0} MHz", centerFreq / 1e6);
            //Console.WriteLine("Minimumm measurement span for EVM = {0} MHz", minMeasSpanEVM / 1e6);
            //Console.WriteLine("Minimumm measurement span for ACLR = {0} MHz", minMeasSpanACLR / 1e6);
        }

        public void RefLevelSet(float refLevel)
        {
            this.correction = refLevel;
            TdscdmaObject.Configuration.RfLevelCal = refLevel;
        }

        public void CenterFreq(double freq)
        {
            this.centerFreq = freq;
            TdscdmaObject.Configuration.SpectrumFreqAxisCentre = freq;
        }

        public void SampleFreq(double freq)
        {
            this.sampleFreq = freq;
            TdscdmaObject.Configuration.SamplingFreq = freq;
        }

        public void SampleSize(int samples)
        {
            this.numOfSamples = samples;
            iData = new float[samples];
            qData = new float[samples];
        }

        public int Analyze()
        {
            TdscdmaObject.Configuration.RfLevelCal = correction;

            try
            {
                TdscdmaObject.Analyse((Aeroflex.PXI.Analysis.TdscdmaMeasurement)measurements, iData, qData);

                if (measurements == TdscdmaMeasurement.MeasAclr)
                {
                    aclrLevels = TdscdmaObject.Results.GetAclrResults();
                }
                else if (measurements == (TdscdmaMeasurement.MeasAclr | TdscdmaMeasurement.MeasModAccuracy))
                {
                    aclrLevels = TdscdmaObject.Results.GetAclrResults();
                    evmRms = TdscdmaObject.Results.DataEvmRms;
                }

                aclr_C = aclrLevels[2];
                aclr_L2 = aclrLevels[0];
                aclr_L1 = aclrLevels[1];
                aclr_U1 = aclrLevels[3];
                aclr_U2 = aclrLevels[4];

                return 0;
            }
            catch (AnalysisErrorException ex)
            {
                Console.WriteLine("TD-SCDMA analysis error exception: " + ex.Message);
                return -1;
            }
            catch (AnalysisException ex)
            {
                Console.WriteLine("TD-SCDMA analysis exception: " + ex.Message);
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("TD-SCDMA exception: " + ex.Message);
                return -1;
            }
        }
    }

    public class TdscdmaThreadObject
    {
        public int index { get; set; }
        public object tdscdmaLock = new object();

        public TdscdmaThreadObject()
        {
        }
    }
}
