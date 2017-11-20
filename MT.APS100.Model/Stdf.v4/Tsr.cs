using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Tsr : StdfRecord
    {
        public Tsr()
        {
            RecordHeader = new RecordHeader(10, 30);

            HEAD_NUM = 255;
            RecordHeader.REC_LEN += 1;
            TEST_TYP = ' ';
            RecordHeader.REC_LEN += 1;
            EXEC_CNT = uint.MaxValue;
            RecordHeader.REC_LEN += 4;
            FAIL_CNT = uint.MaxValue;
            RecordHeader.REC_LEN += 4;
            ALRM_CNT = uint.MaxValue;
            RecordHeader.REC_LEN += 4;
            TEST_NAM = string.Empty;
            RecordHeader.REC_LEN += 1;
            SEQ_NAME = string.Empty;
            RecordHeader.REC_LEN += 1;
            TEST_LBL = string.Empty;
            RecordHeader.REC_LEN += 1;
        }

        public byte HEAD_NUM { get; set; }
        public byte SITE_NUM { get; set; }
        /// <summary>
        /// Known values are: P, F, M
        /// </summary>
        public char TEST_TYP { get; set; }
        public uint TEST_NUM { get; set; }
        public uint EXEC_CNT { get; set; }
        public uint FAIL_CNT { get; set; }
        public uint ALRM_CNT { get; set; }
        public string TEST_NAM { get; set; }
        public string SEQ_NAME { get; set; }
        public string TEST_LBL { get; set; }
        public byte OPT_FLAG { get; set; }
        public float TEST_TIM { get; set; }
        public float TEST_MIN { get; set; }
        public float TEST_MAX { get; set; }
        public float TST_SUMS { get; set; }
        public float TST_SQRS { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteByte(HEAD_NUM);

            writer.WriteByte(SITE_NUM);

            writer.WriteCharacter(TEST_TYP);

            writer.WriteUInt32(TEST_NUM);

            writer.WriteUInt32(EXEC_CNT);

            writer.WriteUInt32(FAIL_CNT);

            writer.WriteUInt32(ALRM_CNT);

            writer.WriteString(TEST_NAM);

            writer.WriteString(SEQ_NAME);

            writer.WriteString(TEST_LBL);

            writer.WriteByte(OPT_FLAG);

            writer.WriteSingle(TEST_TIM);

            writer.WriteSingle(TEST_MIN);

            writer.WriteSingle(TEST_MAX);

            writer.WriteSingle(TST_SUMS);

            writer.WriteSingle(TST_SQRS);
        }
    }
}
