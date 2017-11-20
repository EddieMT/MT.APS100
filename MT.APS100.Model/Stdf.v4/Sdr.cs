using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Sdr : StdfRecord
    {
        public  Sdr()
        {
            RecordHeader = new RecordHeader(1, 80);

            HAND_TYP = string.Empty;
            RecordHeader.REC_LEN += 1;
            HAND_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            CARD_TYP = string.Empty;
            RecordHeader.REC_LEN += 1;
            CARD_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            LOAD_TYP = string.Empty;
            RecordHeader.REC_LEN += 1;
            LOAD_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            DIB_TYP = string.Empty;
            RecordHeader.REC_LEN += 1;
            DIB_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            CABL_TYP = string.Empty;
            RecordHeader.REC_LEN += 1;
            CABL_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            CONT_TYP = string.Empty;
            RecordHeader.REC_LEN += 1;
            CONT_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            LASR_TYP = string.Empty;
            RecordHeader.REC_LEN += 1;
            LASR_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
            EXTR_TYP = string.Empty;
            RecordHeader.REC_LEN += 1;
            EXTR_ID = string.Empty;
            RecordHeader.REC_LEN += 1;
        }

        public byte HEAD_NUM { get; set; }
        public byte SITE_GRP { get; set; }
        public byte SITE_CNT { get; set; }
        public byte[] SITE_NUM { get; set; }
        public string HAND_TYP { get; set; }
        public string HAND_ID { get; set; }
        public string CARD_TYP { get; set; }
        public string CARD_ID { get; set; }
        public string LOAD_TYP { get; set; }
        public string LOAD_ID { get; set; }
        public string DIB_TYP { get; set; }
        public string DIB_ID { get; set; }
        public string CABL_TYP { get; set; }
        public string CABL_ID { get; set; }
        public string CONT_TYP { get; set; }
        public string CONT_ID { get; set; }
        public string LASR_TYP { get; set; }
        public string LASR_ID { get; set; }
        public string EXTR_TYP { get; set; }
        public string EXTR_ID { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteByte(HEAD_NUM);

            writer.WriteByte(SITE_GRP);

            writer.WriteByte(SITE_CNT);

            writer.WriteByteArray(SITE_NUM);

            writer.WriteString(HAND_TYP);

            writer.WriteString(HAND_ID);

            writer.WriteString(CARD_TYP);

            writer.WriteString(CARD_ID);

            writer.WriteString(LOAD_TYP);

            writer.WriteString(LOAD_ID);

            writer.WriteString(DIB_TYP);

            writer.WriteString(DIB_ID);

            writer.WriteString(CABL_TYP);

            writer.WriteString(CABL_ID);

            writer.WriteString(CONT_TYP);

            writer.WriteString(CONT_ID);

            writer.WriteString(LASR_TYP);

            writer.WriteString(LASR_ID);

            writer.WriteString(EXTR_TYP);

            writer.WriteString(EXTR_ID);
    }
    }
}
