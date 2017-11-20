using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Ptr : StdfRecord
    {
        public Ptr()
        {
            RecordHeader = new RecordHeader(15, 10);

            TEST_TXT = string.Empty;
            RecordHeader.REC_LEN += 1;
            ALARM_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            UNITS = string.Empty;
            RecordHeader.REC_LEN += 1;
            C_RESFMT = string.Empty;
            RecordHeader.REC_LEN += 1;
            C_LLMFMT = string.Empty;
            RecordHeader.REC_LEN += 1;
            C_HLMFMT = string.Empty;
            RecordHeader.REC_LEN += 1;
        }

        public uint TEST_NUM { get; set; }
        public byte HEAD_NUM { get; set; }
        public byte SITE_NUM { get; set; }
        public byte TEST_FLG { get; set; }
        public byte PARM_FLG { get; set; }
        public float? RESULT { get; set; }
        public string TEST_TXT { get; set; }
        public string ALARM_ID { get; set; }
        public byte OPT_FLAG { get; set; }
        /// <summary>
        /// Known values are: 15, 12, 9, 6, 3, 2, 0, -3, -6, -9, -12
        /// </summary>
        public sbyte RES_SCAL { get; set; }
        /// <summary>
        /// Known values are: 15, 12, 9, 6, 3, 2, 0, -3, -6, -9, -12
        /// </summary>
        public sbyte LLM_SCAL { get; set; }
        /// <summary>
        /// Known values are: 15, 12, 9, 6, 3, 2, 0, -3, -6, -9, -12
        /// </summary>
        public sbyte HLM_SCAL { get; set; }
        public float? LO_LIMIT { get; set; }
        public float? HI_LIMIT { get; set; }
        public string UNITS { get; set; }
        public string C_RESFMT { get; set; }
        public string C_LLMFMT { get; set; }
        public string C_HLMFMT { get; set; }
        public float? LO_SPEC { get; set; }
        public float? HI_SPEC { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteUInt32(TEST_NUM);

            writer.WriteByte(HEAD_NUM);

            writer.WriteByte(SITE_NUM);

            writer.WriteByte(TEST_FLG);

            writer.WriteByte(PARM_FLG);

            writer.WriteSingle(RESULT.GetValueOrDefault(1));

            writer.WriteString(TEST_TXT);

            writer.WriteString(ALARM_ID);

            writer.WriteByte(OPT_FLAG);

            writer.WriteSByte(RES_SCAL);

            writer.WriteSByte(LLM_SCAL);

            writer.WriteSByte(HLM_SCAL);

            if (LO_LIMIT.HasValue)
                writer.WriteSingle(LO_LIMIT.Value);

            if (HI_LIMIT.HasValue)
                writer.WriteSingle(HI_LIMIT.Value);

            writer.WriteString(UNITS);

            writer.WriteString(C_RESFMT);

            writer.WriteString(C_LLMFMT);

            writer.WriteString(C_HLMFMT);

            if (LO_SPEC.HasValue)
                writer.WriteSingle(LO_SPEC.Value);

            if (HI_SPEC.HasValue)
                writer.WriteSingle(HI_SPEC.Value);
        }
    }
}
