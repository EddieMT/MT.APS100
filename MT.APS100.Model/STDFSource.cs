using System;

namespace MT.APS100.Model
{
    public class STDFSource
    {
        public STDFSource(LotInfo lotInfo)
        {
            SETUP_T = DateTime.Now;
            START_T = DateTime.Now;
            NODE_NAM = lotInfo.TesterID.Value;
            OPER_NAM = lotInfo.OperatorID.Value;
            FAMLY_ID = lotInfo.CustomerID.Value;
            PART_TYP = lotInfo.DeviceName.Value;
            LOT_ID = lotInfo.CustomerLotNo.Value;
            SBLOT_ID = lotInfo.SubLotNo.Value;
            JOB_NAM = lotInfo.ProgramName.Value;
            TEST_COD = lotInfo.ModeCode.Value;
            USER_TXT = lotInfo.TestBinNo.Value;
            FLOW_ID = lotInfo.TestCode.Value;
            TST_TEMP = string.Empty;
            FACIL_ID = string.Empty;
            HAND_ID = string.Empty;
            LOAD_ID = string.Empty;
            TSTR_TYP = string.Empty;
            EXEC_TYP = string.Empty;
            EXEC_VER = string.Empty;
            RTST_COD = 'N';
            JOB_REV = string.Empty;
            MODE_COD = 'P';
            DSGN_REV = lotInfo.DeviceName.Value;
        }

        public DateTime SETUP_T { get; set; }
        public DateTime START_T { get; set; }
        public string NODE_NAM { get; set; }
        public string OPER_NAM { get; set; }
        public string FAMLY_ID { get; set; }
        public string PART_TYP { get; set; }
        public string LOT_ID { get; set; }
        public string SBLOT_ID { get; set; }
        public string JOB_NAM { get; set; }
        public string TEST_COD { get; set; }
        public string USER_TXT { get; set; }
        public string FLOW_ID { get; set; }
        public string TST_TEMP { get; set; }
        public string FACIL_ID { get; set; }
        public string HAND_ID { get; set; }
        public string LOAD_ID { get; set; }
        public string TSTR_TYP { get; set; }
        public string EXEC_TYP { get; set; }
        public string EXEC_VER { get; set; }
        public char RTST_COD { get; set; }
        public string JOB_REV { get; set; }
        public string SETUP_ID { get { return "PRODUCTION"; } }
        public DateTime FINISH_T { get; set; }
        public char MODE_COD { get; set; }
        public string DSGN_REV { get; set; }
    }
}