using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE.Records.RecordTypes
{
	public struct CELLRecord
	{
		public struct XCLCRecord
		{
			Int32 x;
			Int32 y;
			UInt32 flags;
		}
		public struct XCLLRecord
		{

		}

		public struct TVDTRecord
		{

		}

		public struct MHDTRecord
		{

		}
		public struct XCGDRecord
		{

		}
		public struct LNAMRecord
		{

		}
		public struct XWCURecord
		{

		}

		public ZString edid;
		public LString full;
		UInt16 data;

		public static CELLRecord From(SSEPlugin.Record record, TES4Record plugin)
		{
			return new CELLRecord()
			{
				edid = record.GetFirstField("EDID", ZString.From),
				full = record.GetFirstField("FULL", v => LString.From(v, plugin))
			};
		}
	}
}
