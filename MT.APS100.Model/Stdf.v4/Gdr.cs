using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
    public class Gdr : StdfRecord
    {
        public Gdr()
        {
            RecordHeader = new RecordHeader(50, 10);
        }

        public ushort FLD_CNT { get; set; }
        public object[] GenericData { get; set; }

        public override void WriteRecord(BinaryWriter writer)
        {
            writer.WriteHeader(RecordHeader);

            writer.WriteUInt16(FLD_CNT);

            for (int i = 0; i < GenericData.Length; i++)
            {
                object o = GenericData[i];
                if (o is byte)
                {
                    writer.WriteByte((byte)1);
                    writer.WriteByte((byte)o);
                }
                else if (o is ushort)
                {
                    writer.WriteByte((byte)2);
                    writer.WriteUInt16((ushort)o);
                }
                else if (o is uint)
                {
                    writer.WriteByte((byte)3);
                    writer.WriteUInt32((uint)o);
                }
                else if (o is sbyte)
                {
                    writer.WriteByte((byte)4);
                    writer.WriteSByte((sbyte)o);
                }
                else if (o is short)
                {
                    writer.WriteByte((byte)5);
                    writer.WriteInt16((short)o);
                }
                else if (o is int)
                {
                    writer.WriteByte((byte)6);
                    writer.WriteInt32((int)o);
                }
                else if (o is float)
                {
                    writer.WriteByte((byte)7);
                    writer.WriteSingle((float)o);
                }
                else if (o is double)
                {
                    writer.WriteByte((byte)8);
                    writer.WriteDouble((double)o);
                }
                else if (o is string)
                {
                    writer.WriteByte((byte)10);
                    writer.WriteString((string)o);
                }
                else if (o is byte[])
                {
                    writer.WriteByte((byte)11);
                    writer.WriteByteArray((byte[])o);
                }
                else if (o is BitArray)
                {
                    writer.WriteByte((byte)12);
                    writer.WriteBitArray((BitArray)o);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(@"Don't know how to write {0} to a GDR.", o.GetType()));
                }
                //TODO: how to deal with nibble?
            }
        }
    }
}
