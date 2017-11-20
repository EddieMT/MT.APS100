using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Prr : StdfRecord
    {
        public Prr()
        {
            RecordHeader = new RecordHeader(5, 20);

            SOFT_BIN = ushort.MaxValue;
            RecordHeader.REC_LEN += 2;
            X_COORD = short.MinValue;
            RecordHeader.REC_LEN += 2;
            Y_COORD = short.MinValue;
            RecordHeader.REC_LEN += 2;
            TEST_T = 0;
            RecordHeader.REC_LEN += 4;
            PART_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            PART_TXT = string.Empty;
            RecordHeader.REC_LEN += 1;
        }

        public byte HEAD_NUM { get; set; }
        public byte SITE_NUM { get; set; }
        public byte PART_FLG { get; set; }
        public ushort NUM_TEST { get; set; }
        /// <summary>
        /// While ushort, valid bins must be 0 - 32,767
        /// </summary>
        public ushort HARD_BIN { get; set; }
        /// <summary>
        /// While ushort, valid bins must be 0 - 32,767
        /// </summary>
        public ushort SOFT_BIN { get; set; }
        public short X_COORD { get; set; }
        public short Y_COORD { get; set; }
        public uint TEST_T { get; set; }
        public string PART_ID { get; set; }
        public string PART_TXT { get; set; }
        public byte[] PART_FIX { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteByte(HEAD_NUM);

            writer.WriteByte(SITE_NUM);

            writer.WriteByte(PART_FLG);

            writer.WriteUInt16(NUM_TEST);

            writer.WriteUInt16(HARD_BIN);

            writer.WriteUInt16(SOFT_BIN);

            writer.WriteInt16(X_COORD);

            writer.WriteInt16(Y_COORD);

            writer.WriteUInt32(TEST_T);

            writer.WriteString(PART_ID);

            writer.WriteString(PART_TXT);

            if (PART_FIX == null)
            {
                writer.WriteByte((byte)0);
            }
            else
            {
                writer.WriteByte((byte)PART_FIX.Length);
                writer.WriteByteArray(PART_FIX);
            }
        }
    }
}
