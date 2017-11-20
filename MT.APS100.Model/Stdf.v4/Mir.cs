using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Mir : StdfRecord
    {
        public Mir()
        {
            RecordHeader = new RecordHeader(1, 10);

            MODE_COD = ' ';
            RecordHeader.REC_LEN += 1;
            RTST_COD = ' ';
            RecordHeader.REC_LEN += 1;
            PROT_COD = ' ';
            RecordHeader.REC_LEN += 1;
            BURN_TIM = ushort.MaxValue;
            RecordHeader.REC_LEN += 2;
            CMOD_COD = ' ';
            RecordHeader.REC_LEN += 1;
            JOB_REV = string.Empty;
            RecordHeader.REC_LEN += 1;
            SBLOT_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            OPER_NAM = string.Empty;
            RecordHeader.REC_LEN += 1;
            EXEC_TYP = string.Empty;
            RecordHeader.REC_LEN += 1;
            EXEC_VER = string.Empty;
            RecordHeader.REC_LEN += 1;
            TEST_COD = string.Empty;
            RecordHeader.REC_LEN += 1;
            TST_TEMP = string.Empty;
            RecordHeader.REC_LEN += 1;
            USER_TXT = string.Empty;
            RecordHeader.REC_LEN += 1;
            AUX_FILE = string.Empty;
            RecordHeader.REC_LEN += 1;
            PKG_TYP = string.Empty;
            RecordHeader.REC_LEN += 1;
            FAMLY_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            DATE_COD = string.Empty;
            RecordHeader.REC_LEN += 1;
            FACIL_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            FLOOR_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            PROC_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            OPER_FRQ = string.Empty;
            RecordHeader.REC_LEN += 1;
            SPEC_NAM = string.Empty;
            RecordHeader.REC_LEN += 1;
            SPEC_VER = string.Empty;
            RecordHeader.REC_LEN += 1;
            FLOW_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            SETUP_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            DSGN_REV = string.Empty;
            RecordHeader.REC_LEN += 1;
            ENG_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            ROM_COD = string.Empty;
            RecordHeader.REC_LEN += 1;
            SERL_NUM = string.Empty;
            RecordHeader.REC_LEN += 1;
            SUPR_NAM = string.Empty;
            RecordHeader.REC_LEN += 1;
        }

        public DateTime SETUP_T { get; set; }
        public DateTime START_T { get; set; }
        public byte STAT_NUM { get; set; }
        /// <summary>
        /// Known values are: A, C, D, E, M, P, Q, 0-9
        /// </summary>
        public char MODE_COD { get; set; }
        /// <summary>
        /// Known values are: Y, N, 0-9
        /// </summary>
        public char RTST_COD { get; set; }
        /// <summary>
        /// Known values are A-Z, 0-9
        /// </summary>
        public char PROT_COD { get; set; }
        public ushort BURN_TIM { get; set; }
        /// <summary>
        /// Known values are A-Z, 0-9
        /// </summary>
        public char CMOD_COD { get; set; }
        public string LOT_ID { get; set; }
        public string PART_TYP { get; set; }
        public string NODE_NAM { get; set; }
        public string TSTR_TYP { get; set; }
        public string JOB_NAM { get; set; }
        public string JOB_REV { get; set; }
        public string SBLOT_ID { get; set; }
        public string OPER_NAM { get; set; }
        public string EXEC_TYP { get; set; }
        public string EXEC_VER { get; set; }
        public string TEST_COD { get; set; }
        public string TST_TEMP { get; set; }
        public string USER_TXT { get; set; }
        public string AUX_FILE { get; set; }
        public string PKG_TYP { get; set; }
        public string FAMLY_ID { get; set; }
        public string DATE_COD { get; set; }
        public string FACIL_ID { get; set; }
        public string FLOOR_ID { get; set; }
        public string PROC_ID { get; set; }
        public string OPER_FRQ { get; set; }
        public string SPEC_NAM { get; set; }
        public string SPEC_VER { get; set; }
        public string FLOW_ID { get; set; }
        public string SETUP_ID { get; set; }
        public string DSGN_REV { get; set; }
        public string ENG_ID { get; set; }
        public string ROM_COD { get; set; }
        public string SERL_NUM { get; set; }
        public string SUPR_NAM { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteDateTime(SETUP_T);

            writer.WriteDateTime(START_T);

            writer.WriteByte(STAT_NUM);

            writer.WriteCharacter(MODE_COD);

            writer.WriteCharacter(RTST_COD);

            writer.WriteCharacter(PROT_COD);

            writer.WriteUInt16(BURN_TIM);

            writer.WriteCharacter(CMOD_COD);

            writer.WriteString(LOT_ID);

            writer.WriteString(PART_TYP);

            writer.WriteString(NODE_NAM);

            writer.WriteString(TSTR_TYP);

            writer.WriteString(JOB_NAM);

            writer.WriteString(JOB_REV);

            writer.WriteString(SBLOT_ID);

            writer.WriteString(OPER_NAM);

            writer.WriteString(EXEC_TYP);

            writer.WriteString(EXEC_VER);

            writer.WriteString(TEST_COD);

            writer.WriteString(TST_TEMP);

            writer.WriteString(USER_TXT);

            writer.WriteString(AUX_FILE);

            writer.WriteString(PKG_TYP);

            writer.WriteString(FAMLY_ID);

            writer.WriteString(DATE_COD);

            writer.WriteString(FACIL_ID);

            writer.WriteString(FLOOR_ID);

            writer.WriteString(PROC_ID);

            writer.WriteString(OPER_FRQ);

            writer.WriteString(SPEC_NAM);

            writer.WriteString(SPEC_VER);

            writer.WriteString(FLOW_ID);

            writer.WriteString(SETUP_ID);

            writer.WriteString(DSGN_REV);

            writer.WriteString(ENG_ID);

            writer.WriteString(ROM_COD);

            writer.WriteString(SERL_NUM);

            writer.WriteString(SUPR_NAM);
        }
    }
}
