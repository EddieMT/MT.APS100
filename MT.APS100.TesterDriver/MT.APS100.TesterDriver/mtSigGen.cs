using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using afSigGenDll_32Wrapper;
using System.IO;

namespace MT.APS100.TesterDriver
{
    public class mtSigGen
    {
        public afSigGen SigGen;

        public mtSigGen()
        {
            SigGen = new afSigGen();
        }

        public int Initialize(string SigGenLOaddr, string SigGenRFaddr, Instr10MHzReference SigGenRef, bool enhArb_avail)
        {
            int status = -1;
            int isActive = 0;
            const uint bufferLength = 24;
            string serialNoRF = ""; // Signal Generator RF serial number
            string serialNoLO = ""; // Signal Generator LO serial number

            Console.WriteLine("Signal generator is booting...");

            status = SigGen.ClearErrors();
            status = SigGen.EepromCacheEnable_Set(1); // Must set before instrument is booted
            status = SigGen.BootInstrument(SigGenLOaddr, SigGenRFaddr, 0);
            CheckSigGenStatus(status);

            status = SigGen.IsActive_Get(ref isActive);

            if (isActive == -1 && status == 0) // Signal Generator booted, no warning or error
            {
                // Get serial numbers, verifiy resource is created
                status = SigGen.RF.Resource.SerialNumber_Get(ref serialNoRF, bufferLength);
                status = SigGen.LO.Resource.SerialNumber_Get(ref serialNoLO, bufferLength);

                Console.WriteLine("Signal generator booted successfully");
                Console.WriteLine("Signal generator RF serial number: {0}", serialNoRF);
                Console.WriteLine("Signal generator LO serial number: {0}", serialNoLO);
            }
            else if (isActive == 0 || status < 0) // Signal Generator did not boot, fatal error
            {
                Console.WriteLine("Signal generator did not boot\n");
                return -1;
            }

            status = SigGen.VCO.ExternalReference_Set(1); // 1 = enable use of external 10MHz reference
            CheckSigGenStatus(status);

            // Setup 301x reference for Signal Generator
            if (SigGenRef == Instr10MHzReference.OCXO) // Use internal OCXO reference (3011 only)
            {
                status = SigGen.LO.Reference_Set(afSigGenDll_lormReferenceMode_t.afSigGenDll_lormOCXO);
                CheckSigGenStatus(status);

                Console.WriteLine("Signal generator 10 MHz reference is set to internal OCXO\n");
            }
            else if (SigGenRef == Instr10MHzReference.intern) // Use internal reference
            {
                status = SigGen.LO.Reference_Set(afSigGenDll_lormReferenceMode_t.afSigGenDll_lormInternal);
                CheckSigGenStatus(status);

                Console.WriteLine("Signal generator 10 MHz reference is set to internal\n");
            }
            else if (SigGenRef == Instr10MHzReference.externTerm) // Use external reference and terminate it
            {
                status = SigGen.LO.Reference_Set(afSigGenDll_lormReferenceMode_t.afSigGenDll_lormExternalTerminated);
                CheckSigGenStatus(status);

                Console.WriteLine("Signal generator 10 MHz reference is set to external terminated\n");
            }
            else if (SigGenRef == Instr10MHzReference.externDaisy) // Use external reference and daisy chain it
            {
                status = SigGen.LO.Reference_Set(afSigGenDll_lormReferenceMode_t.afSigGenDll_lormExternalDaisy);
                CheckSigGenStatus(status);

                Console.WriteLine("Signal generator 10 MHz reference is set to external daisy chain\n");
            }

            status = SigGen.LO.Trigger.Mode_Set(afSigGenDll_lotmTriggerMode_t.afSigGenDll_lotmNone);
            CheckSigGenStatus(status);

            status = SigGen.Mode_Set(afSigGenDll_mMode_t.afSigGenDll_mFull);
            CheckSigGenStatus(status);

            status = SigGen.RF.CurrentLevel_Set(-136);
            //status = SigGen.RF.CurrentFrequency_Set(100e6);
            status = SigGen.RF.ModulationSource_Set(afSigGenDll_msModulationSource_t.afSigGenDll_msCW); //afSigGenDll_msARB

            if (enhArb_avail) // Enhanced ARB control is not supported for the AF3020, AF3020A and AF3025 models
            {
                status = SigGen.ARBControlMode_Set(afSigGenDll_amArbControlMode_t.afSigGenDll_amEnhanced); // Enhanced ARB functionality
                status = SigGen.EnhARB.PlayMode_Set(afSigGenDll_eaEnhArbPlayMode_t.afSigGenDll_eaContinuous); //afSigGenDll_eaSingle
                status = SigGen.EnhARB.TerminationMode_Set(afSigGenDll_eaEnhArbPlayTermination_t.afSigGenDll_eaptImmediate);
                status = SigGen.EnhARB.ExternalTrigger.Enable_Set(0);
                status = SigGen.EnhARB.StopPlaying();
            }
            else // Standard ARB control
            {
                status = SigGen.ARBControlMode_Set(afSigGenDll_amArbControlMode_t.afSigGenDll_amStandard); // Standard ARB functionality
                status = SigGen.ARB.SingleShotMode_Set(1);
                status = SigGen.ARB.ExternalTrigger.Enable_Set(0);
                status = SigGen.ARB.StopPlaying();
            }

            // Trigger routing: ARB marker at beginning of waveform is sent out SMB
            status = SigGen.RF.Routing.Reset();
            status = SigGen.RF.Routing.SetConnect(afSigGenDll_rmRoutingMatrix_t.afSigGenDll_rmPXI_TRIG_0, afSigGenDll_rmRoutingMatrix_t.afSigGenDll_rmARB_MARKER_1);
            status = SigGen.RF.Routing.SetOutputEnable(afSigGenDll_rmRoutingMatrix_t.afSigGenDll_rmPXI_TRIG_0, 1);
            //status = SigGen.RF.Routing.SetConnect(afSigGenDll_rmRoutingMatrix_t.afSigGenDll_rmFRONT_SMB, afSigGenDll_rmRoutingMatrix_t.afSigGenDll_rmARB_MARKER_1);
            //status = SigGen.RF.Routing.SetOutputEnable(afSigGenDll_rmRoutingMatrix_t.afSigGenDll_rmFRONT_SMB, 1);

            status = SigGen.RF.CurrentOutputEnable_Set(1); // RF on

            return status;
        }

        public bool CheckSigGenRefLock()
        {
            int status = -1;
            bool refLockStatus = false;
            int SigGenRefLockLO = 0; // Signal Generator LO frequency reference lock status
            int SigGenRefLockRF = 0; // Signal Generator RF frequency reference lock status

            status = SigGen.LO.ReferenceLocked_Get(ref SigGenRefLockLO);
            status = SigGen.RF.ReferenceLocked_Get(ref SigGenRefLockRF);

            if (SigGenRefLockLO == -1 && SigGenRefLockRF == -1) // Signal Generator reference is locked
            {
                Console.WriteLine("Signal generator reference is locked");
                refLockStatus = true;
            }
            else // Signal Generator reference is not locked
            {
                Console.WriteLine("Signal generator LO frequency lock status = {0}", SigGenRefLockLO);
                Console.WriteLine("Signal generator RF frequency lock status = {0}", SigGenRefLockRF);
                Console.WriteLine("Signal generator reference is not locked");
            }

            return refLockStatus;
        }

        public void CheckSigGenStatus(int errorCode)
        {
            int status = -1;
            const uint bufferLength = 256;
            string errorMessage = "";

            if (errorCode < 0) // Fatal error
            {
                status = SigGen.ErrorMessage_Get(ref errorMessage, bufferLength);
                Console.WriteLine("SigGen fatal error {0}: {1}", errorCode, errorMessage);
            }
            else if (errorCode > 0) // Warning
            {
                status = SigGen.ErrorMessage_Get(ref errorMessage, bufferLength);
                Console.WriteLine("SigGen warning {0}: {1}", errorCode, errorMessage);
            }
        }

        public void MuteSigGen()
        {
            int status = -1;

            status = SigGen.EnhARB.StopPlaying();
            status = SigGen.RF.CurrentLevel_Set(-136);
            status = SigGen.RF.CurrentFrequency_Set(100e6);
            status = SigGen.RF.ModulationSource_Set(afSigGenDll_msModulationSource_t.afSigGenDll_msCW);
        }

        public int LoadModulationFiles(string instrName, List<CalData> calSetup)
        {
            int status = -1;
            int numOfMododulationFiles = 0;
            bool modulationFileExists = false;
            string waveformFile = "";
            string[] waveformList = new string[128];
            //string wavePath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Waveforms\\");
            string wavePath = Path.Combine(Directory.GetParent(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath)).FullName, "Waveforms\\");


            //-------------------- Generate List of Unique Modulation Files from Cal Config --------------------

            for (int calIndex = 0; calIndex < calSetup.Count(); calIndex++) // Cycle through each calibration entry
            {
                if ((calSetup[calIndex].srcSelect == instrName) && (String.IsNullOrEmpty(calSetup[calIndex].modulationFile) == false)) // Check to see if modulation waveform required
                {
                    modulationFileExists = false;

                    for (int modIndex = 0; modIndex < numOfMododulationFiles; modIndex++)
                    {
                        if (waveformList[modIndex] == calSetup[calIndex].modulationFile)
                        {
                            modulationFileExists = true;
                        }
                    }

                    if (!modulationFileExists)
                    {
                        waveformList[numOfMododulationFiles++] = calSetup[calIndex].modulationFile;
                    }

                    calSetup[calIndex].modulationFile = wavePath + calSetup[calIndex].modulationFile;
                }
            }

            //-------------------- Load Modulation Files --------------------

            if (waveformList[0] != null) Console.WriteLine("\nLoading modulation files for signal generator {0}", instrName);

            bool enhArb_avail = true; // Enhanced ARB control is not supported for the AF3020, AF3020A and AF3025 models

            if (enhArb_avail) status = SigGen.EnhARB.StopPlaying();
            else status = SigGen.ARB.StopPlaying();

            for (int modIndex = 0; modIndex < numOfMododulationFiles; modIndex++)
            {
                waveformFile = wavePath + waveformList[modIndex];

                if (enhArb_avail) status = SigGen.EnhARB.Catalogue.AddFile(waveformFile);
                else status = SigGen.ARB.Catalogue.AddFile(waveformFile);
                CheckSigGenStatus(status);

                if (status == 0) // ARB file loaded successfully
                {
                    Console.WriteLine("Modulation file is loaded: {0}", waveformList[modIndex]);
                }
                else if (status < 0) // Fatal error
                {
                    return -1;
                }
            }

            if (waveformList.Length > 0)
                Console.WriteLine();

            return 0;
        }

        public int PlayModulationFile(string modulationType, string modulationFile, ref bool modFilePlaying, ref string currentModulation)
        {
            int status = -1;
            bool enhArb_avail = true;

            // Must stop ARB before changing waveforms
            if (modFilePlaying && (modulationType != currentModulation))
            {
                if (enhArb_avail) status = SigGen.EnhARB.StopPlaying();
                else status = SigGen.ARB.StopPlaying();
                modFilePlaying = false;
            }

            // Play modulation file (or CW)
            if (modulationType == "CW" || modulationType == "")
            {
                status = SigGen.RF.ModulationSource_Set(afSigGenDll_msModulationSource_t.afSigGenDll_msCW);

                modFilePlaying = false;
                currentModulation = "CW";
            }
            else if ((modulationType == "LTE-FDD") || (modulationType == "LTE-TDD") || (modulationType == "TD-SCDMA") || (modulationType == "WCDMA") || (modulationType == "WLAN"))
            {
                status = SigGen.RF.ModulationSource_Set(afSigGenDll_msModulationSource_t.afSigGenDll_msARB);

                if (enhArb_avail) status = SigGen.EnhARB.Catalogue.PlayFile(modulationFile);
                else status = SigGen.ARB.Catalogue.PlayFile(modulationFile);

                modFilePlaying = true;
                currentModulation = modulationType;
            }
            else // Error
            {
                Console.WriteLine("WARNING: Unsupported modulation format ({0}). Defaulting to CW.", modulationType);

                status = SigGen.RF.ModulationSource_Set(afSigGenDll_msModulationSource_t.afSigGenDll_msCW);

                modFilePlaying = false;
                currentModulation = "CW";

                return -1;
            }

            return 0;
        }

        public int RFAttenuatorHold_Set(int attenuatorHold)
        {
            return SigGen.RF.AttenuatorHold_Set(attenuatorHold);
        }

        public int RFCurrentLevel_Set(double currentLevel)
        {
            return SigGen.RF.CurrentLevel_Set(currentLevel);
        }

        public int RFCurrentFrequency_Set(double currentFrequency)
        {
            return SigGen.RF.CurrentFrequency_Set(currentFrequency);
        }

        public int RFCurrentOutputEnable_Set(int currentOutputEnable)
        {
            return SigGen.RF.CurrentOutputEnable_Set(currentOutputEnable);
        }

        public int RFModulationSource_Set(mtSigGenDll_msModulationSource_t modulationSource)
        {
            return SigGen.RF.ModulationSource_Set((afSigGenDll_msModulationSource_t)modulationSource);
        }

        public int EnhARBStopPlaying()
        {
            return SigGen.EnhARB.StopPlaying();
        }

        public int EnhARBCataloguePlayFile(string FileName)
        {
            return SigGen.EnhARB.Catalogue.PlayFile(FileName);
        }

        public int CloseInstrument()
        {
            return SigGen.CloseInstrument();
        }
    }
}
