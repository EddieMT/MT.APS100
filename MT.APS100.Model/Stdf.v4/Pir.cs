using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Pir : StdfRecord
    {
        public Pir()
        {
            RecordHeader = new RecordHeader(5, 10);
        }

        public byte HEAD_NUM { get; set; }
        public byte SITE_NUM { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteByte(HEAD_NUM);

            writer.WriteByte(SITE_NUM);
        }
    }
}
