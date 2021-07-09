using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SSE.Plugin.Reader
{
	public class PluginFileReader: IDisposable
	{
		Stream pluginStream;
		RecordReader recordReader;

		public struct Plugin
		{
			public List<Record.RecordHeader> records { get; set; }
			public List<GroupSet> groups { get; set; }
		}

		public struct GroupSet
		{
			public Group.GroupHeader header { get; set; }
			public List<Record.RecordHeader> records { get; set; }
			public List<GroupSet> groups { get; set; }
		}


		public PluginFileReader(FileInfo pluginFile)
		{
			pluginStream = pluginFile.OpenRead();
			recordReader = new RecordReader(pluginStream);
		}
		/*
		GroupSet ReadGroup(Group group)
		{
			var result = RecordParser.Parse(group.data, 0, group.data.Length);
			return new GroupSet()
			{
				records = result.records.Select(r => r.Record).ToList(),
				header = group.header,
				groups = result.groups.Select(g => ReadGroup(g.Group)).ToList()
			};
		}
		*/

		/*
		public async Task<Plugin> ReadAll()
		{
			var result = await recordReader.Read(0, (int)pluginStream.Length);
			return new Plugin()
			{
				records = result.records.Select(r => r.Record).ToList(),
				groups = result.groups.Select(g => ReadGroup(g.Group)).ToList(),
			};
		}
		*/

		public async Task<List<Result.GroupRef>> ReadTopLevelGroup()
		{
			var result = await recordReader.Read(0, (int)pluginStream.Length);
			return result.groupRefs;
		}

		public async Task<GroupSet> ReadToSet (Result.GroupRef groupRef, int depth)
		{
			var result = await recordReader.ReadGroup(groupRef);
			return new GroupSet()
			{
				header = groupRef.GroupHeader,
				records = result.records,
				groups = depth > 0 ? await result.groupRefs.ToAsyncEnumerable().SelectAwait(async r => await ReadToSet(r, depth - 1)).ToListAsync() : new List<GroupSet>(),
			};
		}

		public async Task<GroupSet> ReadCELLS()
		{
			var groups = await ReadTopLevelGroup();
			var cellGroup = groups.Find(g => g.GroupHeader.RecordType == "CELL");
			return await ReadToSet(cellGroup, 4);
		}

		public void Dispose()
		{
			pluginStream.Dispose();
		}
	}


	public class PluginReader
	{
		Stream pluginStream;
		List<RecordScanner.ScannedHeaders.ScanResult<Group.GroupHeader>> topLevelGroups;

		async public static Task<PluginReader> LoadStream(Stream pluginStream)
		{
			var scan = await RecordScanner.ScanHeaders(pluginStream, pluginStream.Length);
			return new PluginReader() {
				pluginStream = pluginStream,
				topLevelGroups = scan.Groups
			};
		}

		async public Task<Group> ReadGroupType(string groupType)
		{
			var groupScan = topLevelGroups.Find(s => s.Scanned.RecordType == groupType);
			var groupBytes = new byte[groupScan.Scanned.GroupSize];
			pluginStream.Seek(groupScan.ScanOffset, SeekOrigin.Begin);
			await pluginStream.ReadAsync(groupBytes);
			var group = Group.Parse(groupBytes);
			return group;
		}
	}
}
