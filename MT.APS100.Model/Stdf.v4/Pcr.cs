using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Pcr : StdfRecord
    {
        public Pcr()
        {
            RecordHeader = new RecordHeader(1, 30);

            HEAD_NUM = 255;
            RecordHeader.REC_LEN += 1;
            RTST_CNT = uint.MaxValue;
            RecordHeader.REC_LEN += 4;
            ABRT_CNT = uint.MaxValue;
            RecordHeader.REC_LEN += 4;
            GOOD_CNT = uint.MaxValue;
            RecordHeader.REC_LEN += 4;
            FUNC_CNT = uint.MaxValue;
            RecordHeader.REC_LEN += 4;
        }

        public byte HEAD_NUM { get; set; }
        public byte SITE_NUM { get; set; }
        public uint PART_CNT { get; set; }
        public uint RTST_CNT { get; set; }
        public uint ABRT_CNT { get; set; }
        public uint GOOD_CNT { get; set; }
        public uint FUNC_CNT { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteByte(HEAD_NUM);

            writer.WriteByte(SITE_NUM);

            writer.WriteUInt32(PART_CNT);

            writer.WriteUInt32(RTST_CNT);

            writer.WriteUInt32(ABRT_CNT);

            writer.WriteUInt32(GOOD_CNT);

            writer.WriteUInt32(FUNC_CNT);
        }
    }
}
