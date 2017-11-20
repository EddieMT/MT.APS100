using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Bps : StdfRecord
    {
        public Bps()
        {
            RecordHeader = new RecordHeader(20, 10);

            SEQ_NAME = string.Empty;
            RecordHeader.REC_LEN += 1;
        }

        public string SEQ_NAME { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteString(SEQ_NAME);
        }
    }
}
