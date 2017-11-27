using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using afDigitizerDll_32Wrapper;

namespace MT.APS100.TesterDriver
{
    public class mtDigitizer
    {
        public afDigitizer Digitizer;

        public mtDigitizer()
        {
            Digitizer = new afDigitizer();
        }

        public int Initialize(string DigitizerLOaddr, string DigitizerRFaddr, Instr10MHzReference DigitizerRef)
        {
            int status = -1;
            int isActive = 0;
            const int bufferLength = 24;
            string serialNoRF = ""; // Digitizer RF serial number
            string serialNoLO = ""; // Digitizer LO serial number

            Console.WriteLine("Digitizer is booting...");

            status = Digitizer.ClearErrors();
            status = Digitizer.EepromCacheEnable_Set(1); // Must set before instrument is booted
            status = Digitizer.BootInstrument(DigitizerLOaddr, DigitizerRFaddr, 0);
            CheckDigitizerStatus(status);

            status = Digitizer.IsActive_Get(ref isActive);

            if (isActive == -1 && status == 0) // Digitizer booted, no warning or error
            {
                // Get serial numbers, verifiy resource is created
                status = Digitizer.RF.Resource.SerialNumber_Get(ref serialNoRF, bufferLength);
                status = Digitizer.LO.Resource.SerialNumber_Get(ref serialNoLO, bufferLength);
                Console.WriteLine("Digitizer booted successfully");
                Console.WriteLine("Digitizer RF serial number: {0}", serialNoRF);
                Console.WriteLine("Digitizer LO serial number: {0}", serialNoLO);
            }
            else if (isActive == 0 || status < 0) // Digitizer did not boot, fatal error
            {
                Console.WriteLine("Digitizer did not boot\n");
                return -1;
            }

            status = Digitizer.RF.Reference_Set(afDigitizerDll_rfrmReferenceMode_t.afDigitizerDll_rfrmExternalDaisy);
            CheckDigitizerStatus(status);

            status = Digitizer.RF.ExternalReference_Set(afDigitizerDll_erExternalReference_t.afDigitizerDll_erLockTo10MHz);
            CheckDigitizerStatus(status);

            // Setup 301x reference for Digitizer
            if (DigitizerRef == Instr10MHzReference.OCXO) // Use internal OCXO reference (3011 only)
            {
                status = Digitizer.LO.Reference_Set(afDigitizerDll_lormReferenceMode_t.afDigitizerDll_lormOCXO);
                CheckDigitizerStatus(status);

                Console.WriteLine("Digitizer 10 MHz reference is set to internal OCXO\n");
            }
            else if (DigitizerRef == Instr10MHzReference.intern) // Use internal reference
            {
                status = Digitizer.LO.Reference_Set(afDigitizerDll_lormReferenceMode_t.afDigitizerDll_lormInternal);
                CheckDigitizerStatus(status);

                Console.WriteLine("Digitizer 10 MHz reference is set to internal\n");
            }
            else if (DigitizerRef == Instr10MHzReference.externTerm) // Use external reference and terminate it
            {
                status = Digitizer.LO.Reference_Set(afDigitizerDll_lormReferenceMode_t.afDigitizerDll_lormExternalTerminated);
                CheckDigitizerStatus(status);

                Console.WriteLine("Digitizer 10 MHz reference is set to external terminated\n");
            }
            else if (DigitizerRef == Instr10MHzReference.externDaisy) // Use external reference and daisy chain it
            {
                status = Digitizer.LO.Reference_Set(afDigitizerDll_lormReferenceMode_t.afDigitizerDll_lormExternalDaisy);
                CheckDigitizerStatus(status);

                Console.WriteLine("Digitizer 10 MHz reference is set to external daisy chain\n");
            }

            status = Digitizer.RF.RFInputLevel_Set(30);
            //status = Digitizer.RF.CentreFrequency_Set(centerFrequency);
            status = Digitizer.RF.InputSource_Set(afDigitizerDll_isInputSource_t.afDigitizerDll_isRFInput);
            status = Digitizer.RF.FrontEndMode_Set(afDigitizerDll_femFrontEndMode_t.afDigitizerDll_femAuto);

            // Trigger setup
            status = Digitizer.Trigger.Source_Set(afDigitizerDll_tsTrigSource_t.afDigitizerDll_tsPXI_TRIG_0);
            //status = Digitizer.Trigger.Source_Set(afDigitizerDll_tsTrigSource_t.afDigitizerDll_tsFRONT_SMB);
            status = Digitizer.Trigger.TType_Set(afDigitizerDll_ttTrigType_t.afDigitizerDll_ttEdge);
            status = Digitizer.Trigger.EdgeGatePolarity_Set(afDigitizerDll_egpPolarity_t.afDigitizerDll_egpPositive);
            //status = Digitizer.Trigger.PreEdgeTriggerSamples_Set(preTrigSamples);
            //status = Digitizer.Trigger.PostGateTriggerSamples_Set(0);
            //status = Digitizer.Trigger.OffsetDelay_Set(trigDelaySamples);
            status = Digitizer.Trigger.SwTriggerMode_Set(afDigitizerDll_swtSwTrigMode_t.afDigitizerDll_swtArmed);

            // Sampling setup
            status = Digitizer.Modulation.Mode_Set(afDigitizerDll_mmModulationMode_t.afDigitizerDll_mmGeneric);
            status = Digitizer.Modulation.GenericDecimationRatio_Set(1);
            //status = Digitizer.Modulation.GenericSamplingFrequency_Set(31e6);
            //status = Digitizer.Modulation.DecimatedSamplingFrequency_Get(ref decimatedSampleFreq);

            status = Digitizer.RF.RemoveDCOffset_Set(1);
            status = Digitizer.Capture.SampleDataType_Set(afDigitizerDll_sdtSampleDataType_t.afDigitizerDll_sdtIQData);
            status = Digitizer.Capture.IQ.Resolution_Set(afDigitizerDll_iqrIQResolution_t.afDigitizerDll_iqrAuto);
            //status = Digitizer.RF.GetBandwidth(centerFreq, span, afDigitizerDll_frFlatnessRequired_t.afDigitizerDll_frLessThanOnedB, ref bandwidth);

            // Correction
            status = Digitizer.RF.OptimizeTemperatureCorrection();
            status = Digitizer.RF.AutoTemperatureOptimization_Set(afDigitizerDll_atoAutoTemperatureOptimization_t.afDigitizerDll_atoEnable);
            //status = Digitizer.RF.LevelCorrection_Get(ref correction);

            status = Digitizer.Capture.PipeliningEnable_Set(0);

            return status;
        }

        public bool CheckDigitizerRefLock()
        {
            int status = -1;
            bool refLockStatus = false;
            int DigitizerRefLockLO = 0; // Digitizer LO frequency reference lock status
            int DigitizerRefLockRF = 0; // Digitizer RF frequency reference lock status

            status = Digitizer.LO.ReferenceLocked_Get(ref DigitizerRefLockLO);
            status = Digitizer.RF.ReferenceLocked_Get(ref DigitizerRefLockRF);

            if (DigitizerRefLockLO == -1 && DigitizerRefLockRF == -1) // Digitizer reference is locked
            {
                Console.WriteLine("Digitizer reference is locked");
                refLockStatus = true;
            }
            else // Digitizer reference is not locked
            {
                Console.WriteLine("Digitizer LO frequency lock status = {0}", DigitizerRefLockLO);
                Console.WriteLine("Digitizer RF frequency lock status = {0}", DigitizerRefLockRF);
                Console.WriteLine("Digitizer reference is not locked");
            }

            return refLockStatus;
        }

        public void CheckDigitizerStatus(int errorCode)
        {
            int status = -1;
            const int bufferLength = 256;
            string errorMessage = "";

            if (errorCode < 0) // Fatal error
            {
                status = Digitizer.ErrorMessage_Get(ref errorMessage, bufferLength);
                Console.WriteLine("Digitizer fatal error {0}: {1}", errorCode, errorMessage);
            }
            else if (errorCode > 0) // Warning
            {
                status = Digitizer.ErrorMessage_Get(ref errorMessage, bufferLength);
                Console.WriteLine("Digitizer warning {0}: {1}", errorCode, errorMessage);
            }
        }

        public int CaptureIQADCOverload_Get(ref int pADCOverload)
        {
            return Digitizer.Capture.IQ.ADCOverload_Get(ref pADCOverload);
        }

        public int CaptureIQCaptComplete_Get(ref int pCaptComplete)
        {
            return Digitizer.Capture.IQ.CaptComplete_Get(ref pCaptComplete);
        }

        public int CaptureIQCaptMem(uint numberOfIQSamples, float[] iBuffer, float[] qBuffer)
        {
            return Digitizer.Capture.IQ.CaptMem(numberOfIQSamples, iBuffer, qBuffer);
        }

        public int CaptureIQPowerNumMeasurementsAvail_Get(ref int pNumMeasurementsAvail)
        {
            return Digitizer.Capture.IQ.Power.NumMeasurementsAvail_Get(ref pNumMeasurementsAvail);
        }

        public int CaptureIQPowerNumOfSteps_Set(int numOfSteps)
        {
            return Digitizer.Capture.IQ.Power.NumOfSteps_Set(numOfSteps);
        }

        public int CaptureIQPowerGetAllMeasurements(int numMeasurements, double[] powers)
        {
            return Digitizer.Capture.IQ.Power.GetAllMeasurements(numMeasurements, powers);
        }

        public int CaptureIQPowerIsAvailable_Get(ref int pIsAvailable)
        {
            return Digitizer.Capture.IQ.Power.IsAvailable_Get(ref pIsAvailable);
        }

        public int CaptureIQPowerSetParameters(int stepLength, int measOffset, int measLength)
        {
            return Digitizer.Capture.IQ.Power.SetParameters(stepLength, measOffset, measLength);
        }

        public int CaptureIQTriggerArm(uint samples)
        {
            return Digitizer.Capture.IQ.TriggerArm(samples);
        }

        public int TriggerDetected_Get(ref int pTrigger_Detected)
        {
            return Digitizer.Trigger.Detected_Get(ref pTrigger_Detected);
        }

        public int TriggerPreEdgeTriggerSamples_Set(uint preEdgeTriggerSamples)
        {
            return Digitizer.Trigger.PreEdgeTriggerSamples_Set(preEdgeTriggerSamples);
        }

        public int TriggerSource_Set(mtDigitizerDll_tsTrigSource_t trigger_Source)
        {
            return Digitizer.Trigger.Source_Set((afDigitizerDll_tsTrigSource_t)trigger_Source);
        }

        public int TriggerSwTriggerMode_Set(mtDigitizerDll_swtSwTrigMode_t swTriggerMode)
        {
            return Digitizer.Trigger.SwTriggerMode_Set((afDigitizerDll_swtSwTrigMode_t)swTriggerMode);
        }

        public int RFRFInputLevel_Set(double RFInputLevel)
        {
            return Digitizer.RF.RFInputLevel_Set(RFInputLevel);
        }

        public int RFLevelCorrection_Get(ref double pLevelCorrection)
        {
            return Digitizer.RF.LevelCorrection_Get(ref pLevelCorrection);
        }

        public int RFCentreFrequency_Set(double CenterFrequency)
        {
            return Digitizer.RF.CentreFrequency_Set(CenterFrequency);
        }

        public int ModulationGenericSamplingFrequency_Set(double GenericSamplingFrequency)
        {
            return Digitizer.Modulation.GenericSamplingFrequency_Set(GenericSamplingFrequency);
        }

        public int CloseInstrument()
        {
            return Digitizer.CloseInstrument();
        }
    }
}
