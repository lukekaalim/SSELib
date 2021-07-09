using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE
{
	/// <summary>
	/// https://en.uesp.net/wiki/Tes5Mod:String_Table_File_Format
	/// </summary>
	public class StringTable
	{
		public readonly struct Header
		{
			public static int SizeInBytes => sizeof(uint) * 2;

			public readonly uint count;
			public readonly uint size;
			public Header(byte[] bytes, int offset = 0)
			{
				count = BitConverter.ToUInt32(bytes, offset + 0);
				size = BitConverter.ToUInt32(bytes, offset + 4);
			}
		}

		public readonly struct DictionaryEntry
		{
			public static int SizeInBytes => sizeof(uint) * 2;

			public readonly uint id;
			public readonly uint stringOffset;
			public DictionaryEntry(byte[] bytes, int offset = 0)
			{
				id = BitConverter.ToUInt32(bytes, offset + 0);
				stringOffset = BitConverter.ToUInt32(bytes, offset + 4);
			}
		}

		public enum StringType
		{
			NullTerminated,
			LengthPrefixed
		}

		Dictionary<uint, int> stringOffsetByID;
		Dictionary<uint, int> stringLengthByID;

		Streamable source;
		StringType stringType;

		public static async Task<StringTable> Open(Streamable source, StringType stringType = StringType.NullTerminated) {
			using var stream = source.OpenStream();

			var headerBytes = new byte[Header.SizeInBytes];
			await stream.ReadAsync(headerBytes);
			var header = new Header(headerBytes);

			var tableBytes = new byte[DictionaryEntry.SizeInBytes * header.count];
			await stream.ReadAsync(tableBytes);

			var stringOffsetByID = Enumerable
				.Range(0, (int)header.count)
				.Select(index => new DictionaryEntry(tableBytes, index * DictionaryEntry.SizeInBytes))
				.AsParallel()
				.ToDictionary(entry => entry.id, entry => (int)entry.stringOffset);

			var dataBytes = new byte[stream.Length - stream.Position];
			await stream.ReadAsync(dataBytes);

			Func<byte[], int, string> stringParsingMethod = stringType switch
			{
				StringType.LengthPrefixed => ReadLengthPrefixedString,
				StringType.NullTerminated => ReadNullTerminatedString,
				_ => throw new NotImplementedException()
			};

			var stringLengthByID = stringOffsetByID
				.ToDictionary(
					entry => entry.Key,
					entry => Encoding.UTF8.GetByteCount(stringParsingMethod(dataBytes, entry.Value))
				);

			return new StringTable()
			{
				stringOffsetByID = stringOffsetByID,
				stringLengthByID = stringLengthByID,
				stringType = stringType,
				source = source
			};
		}

		static string ReadNullTerminatedString(byte[] bytes, int offset)
		{
			var nextNullChar = Array.IndexOf(bytes, (byte)0, offset);
			var length = nextNullChar - offset;
			return Encoding.UTF8.GetString(bytes, offset, length);
		}

		static string ReadLengthPrefixedString(byte[] bytes, int offset)
		{
			var length = bytes[offset];
			return Encoding.UTF8.GetString(bytes, offset + 1, length);
		}

		public async Task<string> Read(uint stringId)
		{
			using var stream = source.OpenStream();
			int stringOffset;
			if (!stringOffsetByID.TryGetValue(stringId, out stringOffset))
				throw new Exception("String with that ID is not found");
			int stringLength;
			if (!stringLengthByID.TryGetValue(stringId, out stringLength))
				throw new Exception("String with that ID is not found");

			byte[] stringBytes = new byte[stringLength];

			stream.Seek(stringOffset, SeekOrigin.Begin);
			await stream.ReadAsync(stringBytes);

			return stringType switch
			{
				StringType.LengthPrefixed => ReadLengthPrefixedString(stringBytes, 0),
				StringType.NullTerminated => ReadNullTerminatedString(stringBytes, 0),
				_ => throw new NotImplementedException(),
			};
		}
	}
}
