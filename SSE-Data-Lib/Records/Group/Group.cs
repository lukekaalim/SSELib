using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE.Records.Group
{
	public struct TopLevelGroup
	{
		/// <summary>
		/// Total size of all records in this top level group
		/// </summary>
		public uint recordsSize;
		/// <summary>
		/// Offset in bytes from the start of this file to the start of the group
		/// </summary>
		public uint startOffset;
		/// <summary>
		/// The type of records this group contains
		/// </summary>
		public int recordType;


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
