using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Mrr : StdfRecord
    {
        public Mrr()
        {
            RecordHeader = new RecordHeader(1, 20);

            DISP_COD = ' ';
            RecordHeader.REC_LEN += 1;
            USR_DESC = string.Empty;
            RecordHeader.REC_LEN += 1;
            EXC_DESC = string.Empty;
            RecordHeader.REC_LEN += 1;
        }

        public DateTime FINISH_T { get; set; }
        public char DISP_COD { get; set; }
        public string USR_DESC { get; set; }
        public string EXC_DESC { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteDateTime(FINISH_T);

            writer.WriteCharacter(DISP_COD);

            writer.WriteString(USR_DESC);

            writer.WriteString(EXC_DESC);
        }
    }
}
