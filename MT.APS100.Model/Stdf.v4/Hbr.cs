using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Hbr : StdfRecord
    {
        public Hbr()
        {
            RecordHeader = new RecordHeader(1, 40);

            HEAD_NUM = 255;
            RecordHeader.REC_LEN += 1;
            HBIN_PF = ' ';
            RecordHeader.REC_LEN += 1;
            HBIN_NAM = string.Empty;
            RecordHeader.REC_LEN += 1;
        }

        public byte HEAD_NUM { get; set; }
        public byte SITE_NUM { get; set; }
        /// <summary>
        /// While ushort, valid bins must be 0 - 32,767
        /// </summary>
        public ushort HBIN_NUM { get; set; }
        public uint HBIN_CNT { get; set; }
        /// <summary>
        /// Known values are P, F
        /// </summary>
        public char HBIN_PF { get; set; }
        public string HBIN_NAM { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteByte(HEAD_NUM);

            writer.WriteByte(SITE_NUM);

            writer.WriteUInt16(HBIN_NUM);

            writer.WriteUInt32(HBIN_CNT);

            writer.WriteCharacter(HBIN_PF);

            writer.WriteString(HBIN_NAM);
        }
    }
}
