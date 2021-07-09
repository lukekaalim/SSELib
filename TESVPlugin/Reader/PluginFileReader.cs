using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static SSE.TESVPlugin.Reader.RecordScanner.ScannedHeaders;

namespace SSE.TESVPlugin.Reader
{
	public class PluginStreamReader
	{
		Stream pluginStream;
		List<ScanResult<GroupHeader>> topLevelGroups;

		Dictionary<uint, ScanResult<RecordHeader>> scanByFormID;
		Dictionary<uint, ScanResult<RecordHeader>> interiorCellByFormId;
		Dictionary<uint, ScanResult<GroupHeader>> cellChildrenByFormId;

		async public static Task<PluginStreamReader> LoadStream(Stream pluginStream)
		{
			var topLevelScan = await RecordScanner.ScanHeaders(pluginStream, pluginStream.Length);
			var exclude = new HashSet<int>(new string[] { "CELL", "WRLD" }.Select(s => BitConverter.ToInt32(Encoding.UTF8.GetBytes(s), 0)));

			var topLevelGroupScan = topLevelScan.Groups
				.Where(s => !exclude.Contains(s.Scanned.Label));
			var scanByFormID = new Dictionary<uint, ScanResult<RecordHeader>>();

			foreach (var recordGroup in topLevelGroupScan)
			{
				pluginStream.Seek(recordGroup.ScanOffset + GroupHeader.HeaderSize, SeekOrigin.Begin);
				var scan = await RecordScanner.ScanHeaders(pluginStream, recordGroup.Scanned.DataSize);
				foreach (var record in scan.Records)
				{
					scanByFormID.Add(record.Scanned.Id, record);
				}
			}

			var interiorCellByFormId = new Dictionary<uint, ScanResult<RecordHeader>>();
			var cellChildrenByFormId = new Dictionary<uint, ScanResult<GroupHeader>>();

			var cellGroup = topLevelScan.Groups.Find(g => g.Scanned.Label == BitConverter.ToInt32(Encoding.UTF8.GetBytes("CELL"), 0));
			pluginStream.Seek(cellGroup.ScanOffset + GroupHeader.HeaderSize, SeekOrigin.Begin);
			var cellScan = await RecordScanner.ScanHeaders(pluginStream, cellGroup.Scanned.DataSize);
			foreach (var cellBlock in cellScan.Groups)
			{
				pluginStream.Seek(cellBlock.ScanOffset + GroupHeader.HeaderSize, SeekOrigin.Begin);
				var cellBlockScan = await RecordScanner.ScanHeaders(pluginStream, cellBlock.Scanned.DataSize);
				foreach (var cellSubBlock in cellBlockScan.Groups)
				{
					pluginStream.Seek(cellSubBlock.ScanOffset + GroupHeader.HeaderSize, SeekOrigin.Begin);
					var cellSubBlockScan = await RecordScanner.ScanHeaders(pluginStream, cellSubBlock.Scanned.DataSize);
					foreach (var cellChildren in cellSubBlockScan.Groups)
					{
						var formid = BitConverter.ToUInt32(BitConverter.GetBytes(cellChildren.Scanned.Label), 0);
						cellChildrenByFormId.Add(formid, cellChildren);
					}
					foreach (var cell in cellSubBlockScan.Records)
					{
						interiorCellByFormId.Add(cell.Scanned.Id, cell);
					}
				}
			}

			return new PluginStreamReader() {
				pluginStream = pluginStream,
				topLevelGroups = topLevelScan.Groups,
				scanByFormID = scanByFormID,
				interiorCellByFormId = interiorCellByFormId,
				cellChildrenByFormId = cellChildrenByFormId,
			};
		}

		public async Task<Record> ReadRecordFromFormID(uint formId)
		{
			var scan = scanByFormID[formId];
			var recordBytes = new byte[scan.Scanned.RecordSize];
			pluginStream.Seek(scan.ScanOffset, SeekOrigin.Begin);
			await pluginStream.ReadAsync(recordBytes, 0, recordBytes.Length);
			var record = Record.Parse(recordBytes);
			return record;
		}

		public async Task<(Record, Group)> ReadCell(uint formId)
		{
			var cellScan = interiorCellByFormId[formId];
			var cellChildrenScan = cellChildrenByFormId[formId];

			var cellBytes = new byte[cellScan.Scanned.RecordSize];
			pluginStream.Seek(cellScan.ScanOffset, SeekOrigin.Begin);
			await pluginStream.ReadAsync(cellBytes, 0, cellBytes.Length);

			var cellChildrenBytes = new byte[cellChildrenScan.Scanned.GroupSize];
			pluginStream.Seek(cellChildrenScan.ScanOffset, SeekOrigin.Begin);
			await pluginStream.ReadAsync(cellChildrenBytes, 0, cellChildrenBytes.Length);

			var cell = Record.Parse(cellBytes, 0);
			var cellChildren = Group.Parse(cellChildrenBytes, 0);

			return (cell, cellChildren);
		}

		async public Task<Group> ReadGroupType(string groupType)
		{
			var groupInt = BitConverter.ToInt32(Encoding.UTF8.GetBytes(groupType), 0);
			var groupScan = topLevelGroups.Find(s => s.Scanned.Label == groupInt);
			var groupBytes = new byte[groupScan.Scanned.GroupSize];
			pluginStream.Seek(groupScan.ScanOffset, SeekOrigin.Begin);
			await pluginStream.ReadAsync(groupBytes, 0, groupBytes.Length);
			var group = Group.Parse(groupBytes);
			return group;
		}
	}
}
