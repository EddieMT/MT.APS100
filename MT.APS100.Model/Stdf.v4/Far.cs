using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Far : StdfRecord
    {
        public Far()
        {
            RecordHeader = new RecordHeader(0, 10);
        }

        public byte CPU_TYPE { get; set; }
        public byte STDF_VER { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteByte(CPU_TYPE);

            writer.WriteByte(STDF_VER);
        }
    }
}
