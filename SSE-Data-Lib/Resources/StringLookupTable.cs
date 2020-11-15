using System;
using System.Collections.Generic;
using System.Text;

namespace SSE
{
	public struct StringLookupTable
	{
		public Dictionary<UInt32, string> strings;

		public static StringLookupTable ParseNullTerminated(Byte[] bytes)
		{
			UInt32 count = BitConverter.ToUInt32(bytes, 0);
			UInt32 dataSize = BitConverter.ToUInt32(bytes, 4);
			var strings = new Dictionary<uint, string>();
			var dataStartOffset = 8 + (count * 8);
			for (int i = 0; i < count; i++)
			{
				var entryOffset = 8 + (i * 8);
				var stringID = BitConverter.ToUInt32(bytes, entryOffset);
				var stringOffset = dataStartOffset + BitConverter.ToUInt32(bytes, entryOffset + 4);
				// todo: we lose some data here by converting the uint to int... :\
				var stringEnd = Array.IndexOf(bytes, (Byte)0, (int)stringOffset);
				var stringLength = (stringEnd == -1) ? bytes.Length - stringOffset : stringEnd - stringOffset;

				strings.Add(stringID, Encoding.UTF8.GetString(bytes, (int)stringOffset, (int)(stringLength)));
			}
			return new StringLookupTable()
			{
				strings = strings,
			};
		}
		public static StringLookupTable ParseLengthPrefixedy(Byte[] bytes)
		{
			UInt32 count = BitConverter.ToUInt32(bytes, 0);
			UInt32 dataSize = BitConverter.ToUInt32(bytes, 4);
			var strings = new Dictionary<uint, string>();
			var dataStartOffset = 8 + (count * 8);
			for (int i = 0; i < count; i++)
			{
				var entryOffset = 8 + (i * 8);
				var stringID = BitConverter.ToUInt32(bytes, entryOffset);
				var stringOffset = dataStartOffset + BitConverter.ToUInt32(bytes, entryOffset + 4);
				// todo: we lose some data here by converting the uint to int... :\
				var stringEnd = Array.IndexOf(bytes, (Byte)0, (int)stringOffset);
				var stringLength = stringEnd - stringOffset;

				strings.Add(stringID, Encoding.UTF8.GetString(bytes, (int)stringOffset, (int)(stringLength)));
			}
			return new StringLookupTable()
			{
				strings = strings,
			};
		}
	}
}
