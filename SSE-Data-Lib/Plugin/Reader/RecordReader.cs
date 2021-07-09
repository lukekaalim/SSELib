using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SSE.Plugin.Reader
{
	/// <summary>
	/// Result directly parses records on this level,
	/// but only parses the header for a group, and marks the index
	/// where it was read for quick traversal later.
	/// </summary>
	public struct Result
	{
		public struct GroupRef
		{
			public Group.GroupHeader GroupHeader { get; set; }
			public long DataOffset { get; set; }
			public long DataSize => GroupHeader.DataSize;
		}
		public List<Record.RecordHeader> records;
		public List<GroupRef> groupRefs;
	}

	public static class RecordScanner
	{
		public struct ScannedHeaders
		{
			public struct ScanResult<T>
			{
				public long ScanOffset { get; set; }
				public T Scanned { get; set; }

				public ScanResult(long offset, T scanned)
				{
					ScanOffset = offset;
					Scanned = scanned;
				}
			}
			public List<ScanResult<Group.GroupHeader>> Groups { get; set; }
			public List<ScanResult<Record.RecordHeader>> Records { get; set; }
		}

		public static async Task<ScannedHeaders> ScanHeaders(Stream streamToScan, long bytesToScan)
		{
			var scannedHeaders = new ScannedHeaders()
			{
				Groups = new List<ScannedHeaders.ScanResult<Group.GroupHeader>>(),
				Records = new List<ScannedHeaders.ScanResult<Record.RecordHeader>>(),
			};
			long endIndex = streamToScan.Position + bytesToScan;

			// Re-use the same allocation for each header, as a
			// record header and group header are the same size (24 bytes)
			byte[] headerBytes = new byte[24];
			while (streamToScan.Position < endIndex)
			{
				await streamToScan.ReadAsync(headerBytes);
				string type = Encoding.UTF8.GetString(headerBytes, 0, 4);

				switch (type)
				{
					case "GRUP":
						var groupHeader = Group.GroupHeader.Parse(headerBytes);
						var scannedHeader = new ScannedHeaders.ScanResult<Group.GroupHeader>(streamToScan.Position - 24, groupHeader);
						scannedHeaders.Groups.Add(scannedHeader);
						// Skip past the records in the group
						streamToScan.Seek(groupHeader.DataSize, SeekOrigin.Current);
						break;
					default:
						var recordHeader = Record.RecordHeader.Parse(headerBytes);
						var scannedRecord = new ScannedHeaders.ScanResult<Record.RecordHeader>(streamToScan.Position - 24, recordHeader);
						scannedHeaders.Records.Add(scannedRecord);
						// SKip past the fields in the record
						streamToScan.Seek(recordHeader.DataSize, SeekOrigin.Current);
						break;
				}
			}
			return scannedHeaders;
		}
	}

	public class RecordReader
	{

		Stream recordStream;

		public RecordReader(Stream recordStream)
		{
			this.recordStream = recordStream;
		}

		public async Task<Result> Read(long offset, long length)
		{
			recordStream.Seek(offset, SeekOrigin.Begin);
			var result = new Result()
			{
				records = new List<Record.RecordHeader>(),
				groupRefs = new List<Result.GroupRef>()
			};
			long end = offset + length;
			byte[] bytes = new byte[24];
			while (recordStream.Position < end)
			{
				await recordStream.ReadAsync(bytes, 0, 24);
				string type = Encoding.UTF8.GetString(bytes, 0, 4);

				switch (type)
				{
					case "GRUP":
						var groupHeader = Group.GroupHeader.Parse(bytes);
						var groupRef = new Result.GroupRef() {
							GroupHeader = groupHeader,
							DataOffset = recordStream.Position
						};
						result.groupRefs.Add(groupRef);
						// Skip past the records in the group
						recordStream.Seek(groupHeader.DataSize, SeekOrigin.Current);
						break;
					default:
						var recordHeader = Record.RecordHeader.Parse(bytes);
						result.records.Add(recordHeader);
						// SKip past the fields in the record
						recordStream.Seek(recordHeader.DataSize, SeekOrigin.Current);
						break;
				}
			}
			return result;
		}

		public async Task<Result> ReadGroup(Result.GroupRef groupRef)
		{
			return await Read(groupRef.DataOffset, groupRef.GroupHeader.DataSize);
		}
		/// <summary>
		/// Read the entire group into memory, then parse all the records.
		/// </summary>
		/// <param name="groupRef"></param>
		/// <returns></returns>
		public async Task<Result> ReadGroupBytes(Result.GroupRef groupRef)
		{
			recordStream.Seek(groupRef.DataOffset, SeekOrigin.Begin);
			byte[] bytes = new byte[groupRef.GroupHeader.DataSize];
			await recordStream.ReadAsync(bytes);
			return RecordParser.Parse(bytes, 0, bytes.Length, groupRef.DataOffset);
		}
	}

	public static class RecordParser
	{
		public static Result Parse(byte[] data, long offset, long length, long dataOffset = 0)
		{
			var result = new Result()
			{
				records = new List<Record.RecordHeader>(),
				groupRefs = new List<Result.GroupRef>()
			};
			long index = offset;

			while (index < (length + offset))
			{
				string type = Encoding.UTF8.GetString(data, (int)(index), 4);
				switch (type)
				{
					case "GRUP":
						var groupHeader = Group.GroupHeader.Parse(data, index);
						result.groupRefs.Add(new Result.GroupRef() {
							GroupHeader = groupHeader,
							DataOffset = index + Group.GroupHeader.HeaderSize + dataOffset
						});
						index += groupHeader.GroupSize;
						break;
					default:
						var record = Record.RecordHeader.Parse(data, (int)index);
						result.records.Add(record);
						index += record.RecordSize;
						break;
				}
			}
			return result;
		}
	}
}