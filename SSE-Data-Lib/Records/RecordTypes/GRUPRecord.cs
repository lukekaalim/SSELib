﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE.Records.RecordTypes
{
	public struct GRUPRecord
	{
		/// <summary>
		/// Total size of all subrecords in this top level group
		/// </summary>
		public uint recordsSize;
		/// <summary>
		/// Offset in bytes from the start of this file to the start of the group
		/// </summary>
		public uint startOffset;


		/// <summary>
		/// Size of the GRUP header declaration
		/// </summary>
		public static uint headerSize = 24;

		/// <summary>
		/// Total size in bytes of this header and all the records in the group
		/// </summary>
		public uint totalSize => headerSize + recordsSize;

		/// <summary>
		/// The offset of the next byte that is not part of this group
		/// </summary>
		public uint endOffset => startOffset + totalSize;
	}
}
