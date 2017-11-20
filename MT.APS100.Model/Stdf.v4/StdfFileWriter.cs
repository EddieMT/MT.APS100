﻿using System;
using System.Collections.Generic;
using System.IO;

namespace MT.APS100.Model.Stdf.v4
{
	/// <summary>
	/// StdfFileWriter provides a "what you expect" API for writing STDF files.
	/// You provide a path and then call <see cref="WriteRecord"/> or <see cref="WriteRecords"/>
	/// to write to the file.
	/// You can provide an endianness, or have the endianness inferred from the first record
	/// (which must be either <see cref="StartOfStreamRecord"/> or <see cref="Far"/>.
	/// </summary>
	/// <seealso cref="StdfOutputDirectory"/>
	public sealed class StdfFileWriter : IDisposable
	{
		//RecordConverterFactory _Factory;
		Stream _Stream;
		Endian _Endian;
		public StdfFileWriter(string path, Endian endian, bool debug = false)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			_Stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			_Endian = endian;
			//if (debug)
			//{
			//    _Factory = new RecordConverterFactory() { Debug = debug };
			//    StdfV4Specification.RegisterRecords(_Factory);
			//}
			//else
			//{
			//    _Factory = new RecordConverterFactory(StdfFile._V4ConverterFactory);
			//}
		}

		public StdfFileWriter(string path, bool debug = false) : this(path, Endian.Unknown, debug) { }

		/// <summary>
		/// Writes a single record to the file, returning the number of bytes written
		/// </summary>
		/// <param name="record"></param>
		//public int WriteRecord(StdfRecord record)
		//{
		//    if (record == null)
		//        throw new ArgumentNullException("record");
		//    if (_Endian == Endian.Unknown)
		//    {
		//        //we must be able to infer the endianness based on the first record
		//        //if (record.GetType() == typeof(StartOfStreamRecord))
		//        //{
		//        //    var sos = (StartOfStreamRecord)record;
		//        //    _Endian = sos.Endian;
		//        //    return 0;
		//        //}
		//        //else 
		//        if (record.GetType() == typeof(Far))
		//        {
		//            InferEndianFromFar((Far)record);
		//        }
		//        if (_Endian == Endian.Unknown) throw new InvalidOperationException(@"Cannot write record without being able to infer the endianness of the output file. Use a Far as the first record.");
		//    }
		//    var writer = new STDF.BinaryWriter(_Stream, _Endian, false);
		//    var ur = _Factory.Unconvert(record, _Endian);
		//    writer.WriteHeader(new RecordHeader((ushort)ur.Content.Length, ur.RecordType));
		//    _Stream.Write(ur.Content, 0, ur.Content.Length);
		//    return ur.Content.Length + 4;
		//}
		public int WriteRecord<T>(T record) where T : StdfRecord
		{
			if (record.GetType() == typeof(Far))
			{
				InferEndianFromFar(record as Far);
			}

			record.WriteRecord(new BinaryWriter(_Stream, _Endian, false));

			return 0;
		}

		private void InferEndianFromFar(Far far)
		{
			if (far == null)
				throw new ArgumentNullException("far");
			switch (far.CPU_TYPE)
			{
				case 0:
				case 1:
					_Endian = Endian.Big;
					break;
				default:
					_Endian = Endian.Little;
					break;
			}
		}

		/// <summary>
		/// Writes a stream of records to the file.
		/// </summary>
		/// <param name="records"></param>
		public int WriteRecords(IEnumerable<StdfRecord> records)
		{
			int bytesWritten = 0;
			if (records != null)
			{
				foreach (var r in records)
				{
					if (r != null)
						bytesWritten += WriteRecord(r);
				}
			}
			return bytesWritten;
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_Stream != null) _Stream.Dispose();
		}

		#endregion
	}
}
