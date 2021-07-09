using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SSE.SSEPlugin;

namespace SSE.Records.Group
{
	public class PluginFileParser
	{
		FileInfo pluginFile;

		public PluginFileParser(FileInfo pluginFile)
		{
			this.pluginFile = pluginFile;
		}

		public async Task<List<SSEPlugin.GroupReference>> ReadTopLevelGroups()
		{
			using var pluginStream = pluginFile.OpenRead();
			var reader = new RecordReader(pluginStream);
			var result = await reader.Read(0, (int)pluginStream.Length);

			return result.groups;
		}

		public async Task<RecordReader.Result> ReadCELLGroup()
		{
			using var pluginStream = pluginFile.OpenRead();
			var reader = new RecordReader(pluginStream);
			var result = await reader.Read(0, (int)pluginStream.Length);

			var cellGroup = result.groups.Find(groupRef => Encoding.UTF8.GetString(groupRef.group.label) == "CELL");

			var groupContentBytes = new byte[cellGroup.recordsSize];
			pluginStream.Seek((int)(cellGroup.startOffset + SSEPlugin.Group.fixedSize), SeekOrigin.Begin);
			await pluginStream.ReadAsync(groupContentBytes);
			Console.WriteLine(Encoding.UTF8.GetString(groupContentBytes).Substring(0, 512));

			var cellResult =  await reader.Read((int)(cellGroup.startOffset + SSEPlugin.Group.fixedSize), (int)cellGroup.recordsSize);

			var moreGroup = cellResult.groups[0];
			var more = await reader.Read((int)(moreGroup.startOffset + SSEPlugin.Group.fixedSize), (int)moreGroup.recordsSize);
			var evenMoreGroup = more.groups[0];
			var evenMore = await reader.Read((int)(evenMoreGroup.startOffset + SSEPlugin.Group.fixedSize), (int)evenMoreGroup.recordsSize);

			return evenMore;
		}
	}

	public class RecordReader
	{
		public struct Result {
			public List<Record> records;
			public List<GroupReference> groups;
		}
		
		Stream recordStream;

		public RecordReader(Stream recordStream)
		{
			this.recordStream = recordStream;
		}

		public async Task<Result> Read(int offset, int length)
		{
			recordStream.Seek(offset, SeekOrigin.Begin);
			var result = new Result() { records = new List<Record>(), groups = new List<GroupReference>() };
			int end = offset + length;
			while (recordStream.Position < end)
			{
				byte[] typeBytes = new byte[4];
				await recordStream.ReadAsync(typeBytes, 0, 4);
				string type = Encoding.UTF8.GetString(typeBytes);
				
				var startOffset = recordStream.Seek(-4, SeekOrigin.Current);
				switch (type)
				{
					case "GRUP":
						{
							result.groups.Add(new GroupReference() {
								group = await SSEPlugin.Group.Read(recordStream),
								startOffset = (uint)startOffset
							});
							break;
						}
					default:
						{
							result.records.Add(await Record.Read(recordStream));
							break;
						}
				}
				Console.WriteLine($"DISTANCE: {recordStream.Position} END: {end}");
			}
			return result;
		}
	}
}
