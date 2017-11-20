using MT.APS100.Model;
using MT.APS100.Model.Stdf.v4;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MT.APS100.Service
{
    public class STDFService : IDisposable
    {
        private StdfFileWriter sfw;

        public STDFService(string STDFFilePath)
        {
            sfw = new StdfFileWriter(STDFFilePath);
        }

        public void SaveHeader(LotInfo lotInfo)
        {
            WriteFar();
            WriteMir(lotInfo);
            WriteSdr(4, lotInfo);
        }

        public void SaveBody(List<PartResult> partResults)
        {
            foreach (var part in partResults)
            {
                WritePir(part.SiteNumber);
                WriteGdr(part.PartID);
                foreach (var test in part.TestResults)
                {
                    WritePtr(test);
                }
                WritePrr(part);
            }
        }

        public void SaveFooter(List<PartResult> fullPartResults,
                                DateTime finishTime)
        {
			List<TestResult> fullTestResults = new List<TestResult>();
            fullPartResults.ForEach(x => fullTestResults.AddRange(x.TestResults));
            WriteTsr(fullTestResults);
            WriteHbr(fullPartResults);
            WriteSbr(fullPartResults);
            WritePcr(fullPartResults);
            WriteMrr(finishTime);
        }

        #region Header

        private void WriteFar()
        {
            Far far = new Far();
            far.CPU_TYPE = 2;
            far.RecordHeader.REC_LEN += 1;
            far.STDF_VER = 4;
            far.RecordHeader.REC_LEN += 1;
            sfw.WriteRecord(far);
        }

        private void WriteMir(LotInfo lotinfo)
        {
            Mir mir = new Mir();
            mir.SETUP_T = lotinfo.SETUP_T;
            mir.RecordHeader.REC_LEN += 4;
            mir.START_T = lotinfo.START_T;
            mir.RecordHeader.REC_LEN += 4;
            mir.STAT_NUM = 1;
            mir.RecordHeader.REC_LEN += 1;
            mir.MODE_COD = lotinfo.MODE_COD;
            mir.RTST_COD = lotinfo.RTST_COD;
            mir.LOT_ID = lotinfo.CustomerLotNo.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.LOT_ID.Length + 1);
            mir.PART_TYP = lotinfo.DeviceName.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.PART_TYP.Length + 1);
            mir.NODE_NAM = lotinfo.TesterID.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.NODE_NAM.Length + 1);
            mir.TSTR_TYP = lotinfo.TSTR_TYP;
            mir.RecordHeader.REC_LEN += (ushort)(mir.TSTR_TYP.Length + 1);
            mir.JOB_NAM = lotinfo.ProgramName.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.JOB_NAM.Length + 1);
            mir.JOB_REV = lotinfo.JOB_REV;
            mir.RecordHeader.REC_LEN += (ushort)(mir.JOB_REV.Length);
            mir.EXEC_TYP = lotinfo.EXEC_TYP;
            mir.RecordHeader.REC_LEN += (ushort)(mir.EXEC_TYP.Length);
            mir.EXEC_VER = lotinfo.EXEC_VER;
            mir.RecordHeader.REC_LEN += (ushort)(mir.EXEC_VER.Length);
            mir.OPER_NAM = lotinfo.OperatorID.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.OPER_NAM.Length);
            mir.FAMLY_ID = lotinfo.CustomerID.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.FAMLY_ID.Length);
            mir.SBLOT_ID = lotinfo.SubLotNo.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.SBLOT_ID.Length);
            mir.TEST_COD = lotinfo.ModeCode.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.TEST_COD.Length);
            mir.USER_TXT = lotinfo.TestBinNo.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.USER_TXT.Length);
            mir.FLOW_ID = lotinfo.TestCode.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.FLOW_ID.Length);
            mir.TST_TEMP = lotinfo.TST_TEMP;
            mir.RecordHeader.REC_LEN += (ushort)(mir.TST_TEMP.Length);
            mir.FACIL_ID = lotinfo.FACIL_ID;
            mir.RecordHeader.REC_LEN += (ushort)(mir.FACIL_ID.Length);
            mir.SETUP_ID = lotinfo.SETUP_ID;
            mir.RecordHeader.REC_LEN += (ushort)(mir.SETUP_ID.Length);
            mir.DSGN_REV = lotinfo.DeviceName.ToString();
            mir.RecordHeader.REC_LEN += (ushort)(mir.DSGN_REV.Length);
            sfw.WriteRecord(mir);
        }

        private void WriteSdr(int numberOfSites, LotInfo lotInfo)
        {
            Sdr sdr = new Sdr();
            sdr.HEAD_NUM = 1;
            sdr.RecordHeader.REC_LEN += 1;
            sdr.SITE_GRP = 1;
            sdr.RecordHeader.REC_LEN += 1;
            sdr.SITE_CNT = (byte)numberOfSites;
            sdr.RecordHeader.REC_LEN += 1;
            sdr.SITE_NUM = sitesGenerator(numberOfSites);// new byte[] { 1, 2, 3, 4, ... };
            sdr.RecordHeader.REC_LEN += (ushort)sdr.SITE_NUM.Length;
            sdr.HAND_ID = lotInfo.HAND_ID;
            sdr.RecordHeader.REC_LEN += (ushort)(sdr.HAND_ID.Length);
            sdr.LOAD_ID = lotInfo.LOAD_ID;
            sdr.RecordHeader.REC_LEN += (ushort)(sdr.LOAD_ID.Length);
            sfw.WriteRecord(sdr);
        }

        private byte[] sitesGenerator(int numberOfSites)
        {
            byte[] arraySites = new byte[numberOfSites];

            for (byte i = 0; i < numberOfSites; i++)
            {
                arraySites[i] = (byte)(i + 1);
            }

            return arraySites;
        }

        #endregion Header

        #region Body & repeat

        private void WritePir(byte siteNumber)
        {
            Pir pir = new Pir();
            pir.HEAD_NUM = 1;
            pir.RecordHeader.REC_LEN += 1;
            pir.SITE_NUM = siteNumber;
            pir.RecordHeader.REC_LEN += 1;
            sfw.WriteRecord(pir);
        }

        private void WriteGdr(uint partID)
        {
            Gdr gdr = new Gdr();
            gdr.FLD_CNT = 1;
            gdr.RecordHeader.REC_LEN += 2;
            string content = "PART_ID," + partID.ToString();
            gdr.GenericData = new object[1];
            gdr.GenericData[0] = content;
            gdr.RecordHeader.REC_LEN += (ushort)(1 + 1 + content.Length);
            sfw.WriteRecord(gdr);
        }

        #region repeat

        //private void WriteBps(string functionName)
        //{
        //    Bps bps = new Bps();
        //    bps.SEQ_NAME = functionName;
        //    bps.RecordHeader.REC_LEN += (ushort)bps.SEQ_NAME.Length;
        //    sfw.WriteRecord(bps);
        //}

        private void WritePtr(TestResult testResult)
        {
            Ptr ptr = new Ptr();
            ptr.TEST_NUM = testResult.TestNumber;
            ptr.RecordHeader.REC_LEN += 4;
            ptr.HEAD_NUM = 1;
            ptr.RecordHeader.REC_LEN += 1;
            ptr.SITE_NUM = testResult.SiteNumber;
            ptr.RecordHeader.REC_LEN += 1;
            ptr.TEST_FLG = (byte)(testResult.PFFlag ? 0x00 : 0x80);
            ptr.RecordHeader.REC_LEN += 1;
            ptr.RESULT = (float)(testResult.Result * Math.Pow(10, -testResult.Factor));
            ptr.RecordHeader.REC_LEN += 4;
            ptr.TEST_TXT = testResult.TestName;
            ptr.RecordHeader.REC_LEN += (ushort)(ptr.TEST_TXT.Length);
            ptr.RES_SCAL = (sbyte)testResult.Factor;
            ptr.RecordHeader.REC_LEN += 1;
            ptr.LLM_SCAL = (sbyte)testResult.Factor;
            ptr.RecordHeader.REC_LEN += 1;
            ptr.HLM_SCAL = (sbyte)testResult.Factor;
            ptr.RecordHeader.REC_LEN += 1;
            ptr.LO_LIMIT = (float)(testResult.FTLower * Math.Pow(10, -testResult.Factor));
            ptr.RecordHeader.REC_LEN += 4;
            ptr.HI_LIMIT = (float)(testResult.FTUpper * Math.Pow(10, -testResult.Factor));
            ptr.RecordHeader.REC_LEN += 4;
            ptr.PARM_FLG = setPARM_FLG(ptr.RESULT.Value, ptr.LO_LIMIT.Value, ptr.HI_LIMIT.Value);
            ptr.RecordHeader.REC_LEN += 1;
            ptr.UNITS = setUNITS(testResult.Unit);
            ptr.RecordHeader.REC_LEN += (ushort)(ptr.UNITS.Length);
            ptr.OPT_FLAG = setOPT_FLAG((sbyte)testResult.Factor, ptr.LO_SPEC, ptr.HI_SPEC, ptr.LO_LIMIT, ptr.HI_LIMIT);
            ptr.RecordHeader.REC_LEN += 1;
            ptr.C_RESFMT = "%.3f";
            ptr.RecordHeader.REC_LEN += (ushort)(ptr.C_RESFMT.Length);
            ptr.C_LLMFMT = "%.3f";
            ptr.RecordHeader.REC_LEN += (ushort)(ptr.C_LLMFMT.Length);
            ptr.C_HLMFMT = "%.3f";
            ptr.RecordHeader.REC_LEN += (ushort)(ptr.C_HLMFMT.Length);
            sfw.WriteRecord(ptr);
        }

        #endregion repeat

        private void WritePrr(PartResult partResult)
        {
            Prr prr = new Prr();
            prr.HEAD_NUM = 1;
            prr.RecordHeader.REC_LEN += 1;
            prr.SITE_NUM = partResult.SiteNumber;
            prr.RecordHeader.REC_LEN += 1;
            prr.PART_FLG = partResult.isSuccess ? (byte)0x00 : (byte)0x80;//0x00:pass; 0x80:fail
            prr.RecordHeader.REC_LEN += 1;
            prr.NUM_TEST = partResult.NumberOfTests;
            prr.RecordHeader.REC_LEN += 2;
            prr.HARD_BIN = partResult.HardBin.BinNum;
            prr.RecordHeader.REC_LEN += 2;
            prr.SOFT_BIN = partResult.SoftBin.BinNum;
            prr.TEST_T = (uint)partResult.Duration;
            prr.PART_ID = partResult.PartID.ToString();
            prr.RecordHeader.REC_LEN += (ushort)(prr.PART_ID.Length);
            prr.RecordHeader.REC_LEN += (prr.PART_FIX == null) ? (ushort)1 : (ushort)prr.PART_FIX.Length;
            sfw.WriteRecord(prr);
        }

        #endregion Body & repeat

        #region Footer

        private void WriteTsr(List<TestResult> list)
        {
            Tsr tsr;
            var groupBySite = list.GroupBy(x => x.SiteNumber);
            foreach (var colBySite in groupBySite)
            {
                List<TestResult> itemsBySite = colBySite.ToList();
                var groupByTestName = itemsBySite.GroupBy(x => x.TestName);
                foreach (var colByTestName in groupByTestName)
                {
                    List<TestResult> itemsByTestNumber = colByTestName.ToList();
                    tsr = new Tsr();
                    tsr.HEAD_NUM = 1;
                    tsr.SITE_NUM = colBySite.Key;
                    tsr.RecordHeader.REC_LEN += 1;
                    tsr.TEST_TYP = 'P';
                    tsr.TEST_NUM = itemsByTestNumber.First(x => x.TestName == colByTestName.Key).TestNumber;
                    tsr.RecordHeader.REC_LEN += 4;
                    tsr.EXEC_CNT = (uint)itemsByTestNumber.Count;
                    tsr.FAIL_CNT = (uint)itemsByTestNumber.Count(x => x.PFFlag == false);
                    tsr.ALRM_CNT = 0;
                    tsr.TEST_NAM = colByTestName.Key;
                    tsr.RecordHeader.REC_LEN += (ushort)tsr.TEST_NAM.Length;
                    tsr.TEST_LBL = tsr.TEST_NAM;
                    tsr.RecordHeader.REC_LEN += (ushort)tsr.TEST_LBL.Length;
                    tsr.OPT_FLAG = (byte)0xC8;
                    tsr.RecordHeader.REC_LEN += 1;
                    tsr.TEST_TIM = (float)itemsByTestNumber.Average(x => x.Duration);
                    tsr.RecordHeader.REC_LEN += 4;
                    tsr.TEST_MIN = (float)itemsByTestNumber.Min(x => x.Result);
                    tsr.RecordHeader.REC_LEN += 4;
                    tsr.TEST_MAX = (float)itemsByTestNumber.Max(x => x.Result);
                    tsr.RecordHeader.REC_LEN += 4;
                    tsr.TST_SUMS = (float)itemsByTestNumber.Sum(x => x.Result);
                    tsr.RecordHeader.REC_LEN += 4;
                    double sqrs = 0;
                    itemsByTestNumber.ForEach(x => sqrs += Math.Pow(x.Result, 2.0));
                    tsr.TST_SQRS = (float)sqrs;
                    tsr.RecordHeader.REC_LEN += 4;
                    sfw.WriteRecord(tsr);
                }
            }

            var group = list.GroupBy(x => x.TestName);
            foreach (var col in group)
            {
                List<TestResult> items = col.ToList<TestResult>();
                tsr = new Tsr();
                tsr.HEAD_NUM = 255;
                tsr.SITE_NUM = 0;
                tsr.RecordHeader.REC_LEN += 1;
                tsr.TEST_TYP = 'P';
                tsr.TEST_NUM = items.First(x => x.TestName == col.Key).TestNumber;
                tsr.RecordHeader.REC_LEN += 4;
                tsr.EXEC_CNT = (uint)items.Count;
                tsr.FAIL_CNT = (uint)items.Count(x => x.PFFlag == false);
                tsr.ALRM_CNT = 0;
                tsr.TEST_NAM = col.Key;
                tsr.RecordHeader.REC_LEN += (ushort)tsr.TEST_NAM.Length;
                tsr.TEST_LBL = tsr.TEST_NAM;
                tsr.RecordHeader.REC_LEN += (ushort)tsr.TEST_LBL.Length;
                tsr.OPT_FLAG = (byte)0xC8;
                tsr.RecordHeader.REC_LEN += 1;
                tsr.TEST_TIM = (float)items.Average(x => x.Duration);
                tsr.RecordHeader.REC_LEN += 4;
                tsr.TEST_MIN = (float)items.Min(x => x.Result);
                tsr.RecordHeader.REC_LEN += 4;
                tsr.TEST_MAX = (float)items.Max(x => x.Result);
                tsr.RecordHeader.REC_LEN += 4;
                tsr.TST_SUMS = (float)items.Sum(x => x.Result);
                tsr.RecordHeader.REC_LEN += 4;
                double sqrs = 0;
                items.ForEach(x => sqrs += Math.Pow(x.Result, 2.0));
                tsr.TST_SQRS = (float)sqrs;
                tsr.RecordHeader.REC_LEN += 4;
                sfw.WriteRecord(tsr);
            }
        }

        private void WriteHbr(List<PartResult> list)
        {
            Hbr hbr;
            var groupBySite = list.GroupBy(x => x.SiteNumber);
            foreach (var colBySite in groupBySite)
            {
                List<PartResult> itemsBySite = colBySite.ToList<PartResult>();
                var groupByBin = itemsBySite.GroupBy(x => x.HardBin);
                foreach (var colByBin in groupByBin)
                {
                    List<PartResult> itemsByBin = colByBin.ToList<PartResult>();
                    hbr = new Hbr();
                    hbr.HEAD_NUM = 1;
                    hbr.SITE_NUM = colBySite.Key;
                    hbr.RecordHeader.REC_LEN += 1;
                    hbr.HBIN_NUM = colByBin.Key.BinNum;
                    hbr.RecordHeader.REC_LEN += 2;
                    hbr.HBIN_CNT = (uint)itemsByBin.Count;
                    hbr.RecordHeader.REC_LEN += 4;
                    hbr.HBIN_PF = (char)colByBin.Key.BinPF;
                    hbr.HBIN_NAM = colByBin.Key.BinName;
                    hbr.RecordHeader.REC_LEN += (ushort)hbr.HBIN_NAM.Length;
                    sfw.WriteRecord(hbr);
                }
            }

            var group = list.GroupBy(x => x.HardBin);
            foreach (var col in group)
            {
                List<PartResult> items = col.ToList<PartResult>();
                hbr = new Hbr();
                hbr.HEAD_NUM = 255;
                hbr.SITE_NUM = 0;
                hbr.RecordHeader.REC_LEN += 1;
                hbr.HBIN_NUM = col.Key.BinNum;
                hbr.RecordHeader.REC_LEN += 2;
                hbr.HBIN_CNT = (uint)items.Count;
                hbr.RecordHeader.REC_LEN += 4;
                hbr.HBIN_PF = (char)col.Key.BinPF;
                hbr.HBIN_NAM = col.Key.BinName;
                hbr.RecordHeader.REC_LEN += (ushort)hbr.HBIN_NAM.Length;
                sfw.WriteRecord(hbr);
            }
        }

        private void WriteSbr(List<PartResult> list)
        {
            Sbr sbr;
            var groupBySite = list.GroupBy(x => x.SiteNumber);
            foreach (var colBySite in groupBySite)
            {
                List<PartResult> itemsBySite = colBySite.ToList<PartResult>();
                var groupByBin = itemsBySite.GroupBy(x => x.SoftBin);
                foreach (var colByBin in groupByBin)
                {
                    List<PartResult> itemsByBin = colByBin.ToList<PartResult>();
                    sbr = new Sbr();
                    sbr.HEAD_NUM = 1;
                    sbr.SITE_NUM = colBySite.Key;
                    sbr.RecordHeader.REC_LEN += 1;
                    sbr.SBIN_NUM = colByBin.Key.BinNum;
                    sbr.RecordHeader.REC_LEN += 2;
                    sbr.SBIN_CNT = (uint)itemsByBin.Count;
                    sbr.RecordHeader.REC_LEN += 4;
                    sbr.SBIN_PF = (char)colByBin.Key.BinPF;
                    sbr.SBIN_NAM = colByBin.Key.BinName;
                    sbr.RecordHeader.REC_LEN += (ushort)sbr.SBIN_NAM.Length;
                    sfw.WriteRecord(sbr);
                }
            }

            var group = list.GroupBy(x => x.SoftBin);
            foreach (var col in group)
            {
                List<PartResult> items = col.ToList<PartResult>();
                sbr = new Sbr();
                sbr.HEAD_NUM = 255;
                sbr.SITE_NUM = 0;
                sbr.RecordHeader.REC_LEN += 1;
                sbr.SBIN_NUM = col.Key.BinNum;
                sbr.RecordHeader.REC_LEN += 2;
                sbr.SBIN_CNT = (uint)items.Count;
                sbr.RecordHeader.REC_LEN += 4;
                sbr.SBIN_PF = (char)col.Key.BinPF;
                sbr.SBIN_NAM = col.Key.BinName;
                sbr.RecordHeader.REC_LEN += (ushort)sbr.SBIN_NAM.Length;
                sfw.WriteRecord(sbr);
            }
        }

        private void WritePcr(List<PartResult> list)
        {
            Pcr pcr;
            var group = list.GroupBy(x => x.SiteNumber);
            foreach (var col in group)
            {
                List<PartResult> items = col.ToList<PartResult>();
                pcr = new Pcr();
                pcr.HEAD_NUM = 1;
                pcr.SITE_NUM = col.Key;
                pcr.RecordHeader.REC_LEN += 1;
                pcr.PART_CNT = (uint)items.Count;
                pcr.RecordHeader.REC_LEN += 4;
                pcr.RTST_CNT = 0;
                pcr.ABRT_CNT = 0;
                pcr.GOOD_CNT = (uint)items.Count(x => x.isSuccess == true);
                pcr.FUNC_CNT = 0;
                sfw.WriteRecord(pcr);
            }

            pcr = new Pcr();
            pcr.HEAD_NUM = 255;
            pcr.SITE_NUM = 0;
            pcr.RecordHeader.REC_LEN += 1;
            pcr.PART_CNT = (uint)list.Count;
            pcr.RecordHeader.REC_LEN += 4;
            pcr.RTST_CNT = 0;
            pcr.ABRT_CNT = 0;
            pcr.GOOD_CNT = (uint)list.Count(x => x.isSuccess == true);
            pcr.FUNC_CNT = 0;
            sfw.WriteRecord(pcr);
        }

        private void WriteMrr(DateTime finishTime)
        {
            Mrr mrr = new Mrr();
            mrr.FINISH_T = finishTime;
            mrr.RecordHeader.REC_LEN += 4;
            sfw.WriteRecord(mrr);
        }

        #endregion Footer

        #region private methods

        private string setUNITS(string UNITS)
        {
            switch (UNITS)
            {
                case "V":
                    UNITS = "V";
                    break;

                case "mA":
                case "uA":
                    UNITS = "A";
                    break;
            }

            return UNITS;
        }

        private byte setPARM_FLG(float RESULT, float LO_LIMIT, float HI_LIMIT)
        {
            if (RESULT >= LO_LIMIT && RESULT <= HI_LIMIT)
                return 0xC0;
            else if (RESULT < LO_LIMIT)
                return 0xD0;
            else
                return 0xC8;
        }

        private byte setOPT_FLAG(sbyte factor, float? LO_SPEC, float? HI_SPEC, float? LO_LIMIT, float? HI_LIMIT)
        {
            byte PARM_FLG = 0x0E;

            if (factor == sbyte.MaxValue)
            {
                SetBit(PARM_FLG, 0);
            }

            if (!LO_SPEC.HasValue)
            {
                SetBit(PARM_FLG, 2);
            }

            if (!HI_SPEC.HasValue)
            {
                SetBit(PARM_FLG, 3);
            }

            if (!LO_LIMIT.HasValue)
            {
                SetBit(PARM_FLG, 6);
            }

            if (!HI_LIMIT.HasValue)
            {
                SetBit(PARM_FLG, 7);
            }

            return PARM_FLG;
        }

        //index从0开始
        //获取取第index位
        private int GetBit(byte b, int index) { return ((b & (1 << index)) > 0) ? 1 : 0; }

        //将第index位设为1
        private byte SetBit(byte b, int index) { return (byte)(b | (1 << index)); }

        //将第index位设为0
        private byte ClearBit(byte b, int index) { return (byte)(b & (byte.MaxValue - (1 << index))); }

        //将第index位取反
        private byte ReverseBit(byte b, int index) { return (byte)(b ^ (byte)(1 << index)); }
        #endregion private methods

        public void Dispose()
        {
            sfw.Dispose();
        }
    }
}