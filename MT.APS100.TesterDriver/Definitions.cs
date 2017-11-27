using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.TesterDriver
{
    public enum AITerminalConfiguration
    {
        Nrse = 10078,
        Rse = 10083,
        Differential = 10106,
        Pseudodifferential = 12529
    }

    [Flags]
    public enum TdscdmaMeasurement
    {
        MeasNotDefined = 0,
        MeasLocateSlot = 1,
        MeasModAccuracy = 2,
        MeasCcdf = 4,
        MeasSpectrum = 8,
        MeasOccupiedBandwidth = 16,
        MeasSpectrumMask = 32,
        MeasAclr = 64,
        MeasTransmitMask = 128
    }

    [Flags]
    public enum LteMeasurement
    {
        MeasNotDefined = 0,
        MeasModAccuracy = 1,
        MeasCcdf = 2,
        MeasSpectrum = 4,
        MeasOccupiedBandwidth = 8,
        MeasSpectrumMask = 16,
        MeasAclr = 32,
        MeasPower = 64,
        MeasSyncToSlot = 128,
        MeasReuseSync = 256
    }

    [Flags]
    public enum UmtsUplinkMeasurement
    {
        MeasNotDefined = 0,
        ModAccuracy = 1,
        QPSKModAccuracy = 2,
        SlotPower = 4,
        AveragePower = 8,
        ComputeACLR = 16,
        SpectrumAnalysis = 32,
        PhaseDiscontinuity = 64,
        Ccdf = 128,
        MeasSyncToSlot = 256
    }

    public enum Instr10MHzReference
    {
        OCXO,
        intern,
        externTerm,
        externDaisy
    }

    public enum mtDigitizerDll_tsTrigSource_t
    {
        mtDigitizerDll_tsPXI_TRIG_0 = 0,
        mtDigitizerDll_tsPXI_TRIG_1 = 1,
        mtDigitizerDll_tsPXI_TRIG_2 = 2,
        mtDigitizerDll_tsPXI_TRIG_3 = 3,
        mtDigitizerDll_tsPXI_TRIG_4 = 4,
        mtDigitizerDll_tsPXI_TRIG_5 = 5,
        mtDigitizerDll_tsPXI_TRIG_6 = 6,
        mtDigitizerDll_tsPXI_TRIG_7 = 7,
        mtDigitizerDll_tsPXI_STAR = 8,
        mtDigitizerDll_tsPXI_LBL_0 = 9,
        mtDigitizerDll_tsPXI_LBL_1 = 10,
        mtDigitizerDll_tsPXI_LBL_2 = 11,
        mtDigitizerDll_tsPXI_LBL_3 = 12,
        mtDigitizerDll_tsPXI_LBL_4 = 13,
        mtDigitizerDll_tsPXI_LBL_5 = 14,
        mtDigitizerDll_tsPXI_LBL_6 = 15,
        mtDigitizerDll_tsPXI_LBL_7 = 16,
        mtDigitizerDll_tsPXI_LBL_8 = 17,
        mtDigitizerDll_tsPXI_LBL_9 = 18,
        mtDigitizerDll_tsPXI_LBL_10 = 19,
        mtDigitizerDll_tsPXI_LBL_11 = 20,
        mtDigitizerDll_tsPXI_LBL_12 = 21,
        mtDigitizerDll_tsLVDS_MARKER_0 = 22,
        mtDigitizerDll_tsLVDS_MARKER_1 = 23,
        mtDigitizerDll_tsLVDS_MARKER_2 = 24,
        mtDigitizerDll_tsLVDS_MARKER_3 = 25,
        mtDigitizerDll_tsLVDS_AUX_0 = 26,
        mtDigitizerDll_tsLVDS_AUX_1 = 27,
        mtDigitizerDll_tsLVDS_AUX_2 = 28,
        mtDigitizerDll_tsLVDS_AUX_3 = 29,
        mtDigitizerDll_tsLVDS_AUX_4 = 30,
        mtDigitizerDll_tsLVDS_SPARE_0 = 31,
        mtDigitizerDll_tsSW_TRIG = 32,
        mtDigitizerDll_tsLVDS_MARKER_4 = 33,
        mtDigitizerDll_tsINT_TIMER = 34,
        mtDigitizerDll_tsINT_TRIG = 35,
        mtDigitizerDll_tsFRONT_SMB = 36
    }

    public enum mtSigGenDll_msModulationSource_t
    {
        mtSigGenDll_msLVDS = 0,
        mtSigGenDll_msARB = 1,
        mtSigGenDll_msCW = 3,
        mtSigGenDll_msAM = 4,
        mtSigGenDll_msFM = 5,
        mtSigGenDll_msExtAnalog = 6
    }

    public enum mtDigitizerDll_swtSwTrigMode_t
    {
        mtDigitizerDll_swtImmediate = 0,
        mtDigitizerDll_swtArmed = 1
    }
}
