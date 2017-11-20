using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Sbr : StdfRecord
    {
        public Sbr()
        {
            RecordHeader = new RecordHeader(1, 50);

            HEAD_NUM = 255;
            RecordHeader.REC_LEN += 1;
            SBIN_PF = ' ';
            RecordHeader.REC_LEN += 1;
            SBIN_NAM = string.Empty;
            RecordHeader.REC_LEN += 1;
        }

        public byte HEAD_NUM { get; set; }
        public byte SITE_NUM { get; set; }
        /// <summary>
        /// While ushort, valid bins must be 0 - 32,767
        /// </summary>
        public ushort SBIN_NUM { get; set; }
        public uint SBIN_CNT { get; set; }
        /// <summary>
        /// Known values are P, F
        /// </summary>
        public char SBIN_PF { get; set; }
        public string SBIN_NAM { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteByte(HEAD_NUM);

            writer.WriteByte(SITE_NUM);

            writer.WriteUInt16(SBIN_NUM);

            writer.WriteUInt32(SBIN_CNT);

            writer.WriteCharacter(SBIN_PF);

            writer.WriteString(SBIN_NAM);
        }
    }
}
