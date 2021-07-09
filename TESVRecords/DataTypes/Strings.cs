using System;
using System.Collections.Generic;
using System.Text;

namespace SSE.TESVRecord.DataTypes
{
	public struct ZString
	{
		public string Value { get; set; }
		public int Length => Value.IndexOf('\0');
		public string Content => Value.Substring(0, Length);

		public static ZString Parse(byte[] stringBytes)
		{
			return new ZString()
			{
				Value = Encoding.UTF8.GetString(stringBytes),
			};
		}
	}
}
