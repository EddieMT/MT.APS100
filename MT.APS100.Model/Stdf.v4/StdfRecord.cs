using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.APS100.Model.Stdf.v4
{
	public abstract class StdfRecord
	{
		public RecordHeader RecordHeader { get; set; }

		public abstract void WriteRecord(BinaryWriter writer);
	}

	public class RecordHeader
	{
		public RecordHeader(byte type, byte subtype)
		{
			REC_LEN = 0;
			REC_TYP = type;
			REC_SUB = subtype;
		}

		public ushort REC_LEN { get; set; }
		public byte REC_TYP { get; set; }
		public byte REC_SUB { get; set; }
	}
}
