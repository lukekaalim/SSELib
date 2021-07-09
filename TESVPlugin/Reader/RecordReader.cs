using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SSE.TESVPlugin.Reader
{
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
			public List<ScanResult<GroupHeader>> Groups { get; set; }
			public List<ScanResult<RecordHeader>> Records { get; set; }
		}

		public static async Task<ScannedHeaders> ScanHeaders(Stream streamToScan, long bytesToScan)
		{
			var scannedHeaders = new ScannedHeaders()
			{
				Groups = new List<ScannedHeaders.ScanResult<GroupHeader>>(),
				Records = new List<ScannedHeaders.ScanResult<RecordHeader>>(),
			};
			long endIndex = streamToScan.Position + bytesToScan;

			// Re-use the same allocation for each header, as a
			// record header and group header are the same size (24 bytes)
			byte[] headerBytes = new byte[24];
			while (streamToScan.Position < endIndex)
			{
				await streamToScan.ReadAsync(headerBytes, 0, headerBytes.Length);
				string type = Encoding.UTF8.GetString(headerBytes, 0, 4);

				switch (type)
				{
					case "GRUP":
						var groupHeader = GroupHeader.Parse(headerBytes);
						var scannedHeader = new ScannedHeaders.ScanResult<GroupHeader>(streamToScan.Position - 24, groupHeader);
						scannedHeaders.Groups.Add(scannedHeader);
						// Skip past the records in the group
						streamToScan.Seek(groupHeader.DataSize, SeekOrigin.Current);
						break;
					default:
						var recordHeader = RecordHeader.Parse(headerBytes);
						var scannedRecord = new ScannedHeaders.ScanResult<RecordHeader>(streamToScan.Position - 24, recordHeader);
						scannedHeaders.Records.Add(scannedRecord);
						// SKip past the fields in the record
						streamToScan.Seek(recordHeader.DataSize, SeekOrigin.Current);
						break;
				}
			}
			return scannedHeaders;
		}
	}
}