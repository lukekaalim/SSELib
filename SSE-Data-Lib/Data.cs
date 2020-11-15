using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SSE
{
	public class Data
	{
		Profile profile;
		List<Plugin> plugins;
		List<Archive> archives;

		public Dictionary<string, StringLookupTable> stringTables;
		public ResourceLookupTable resources;

		public static async Task<Data> Open(DirectoryInfo dataDirectory, DirectoryInfo profileDirectory)
		{
			Profile profile = await Profile.Read(profileDirectory);

			FileInfo[] filesInDirectory = dataDirectory.GetFiles();

			List<Plugin> plugins = await profile.loadOrder.plugins
				.Select(pluginName => new FileInfo(Path.Combine(dataDirectory.FullName, pluginName)))
				.ToAsyncEnumerable()
				.SelectAwait(file => new ValueTask<Plugin>(Plugin.Load(file)))
				.ToListAsync();

			List<Archive> archives = await plugins
				.SelectMany(plugin => filesInDirectory.Where(file =>
					file.Name.StartsWith(Path.GetFileNameWithoutExtension(plugin.pluginFile.Name), StringComparison.OrdinalIgnoreCase)
					&& string.Equals(file.Extension, ".bsa", StringComparison.OrdinalIgnoreCase)
				))
				.ToAsyncEnumerable()
				.SelectAwait(async archivePath => await Archive.Open(archivePath))
				.ToListAsync();

			ResourceLookupTable resources = new ResourceLookupTable(archives, dataDirectory);
			Dictionary<string, StringLookupTable> stringTables = await plugins
				.ToAsyncEnumerable()
				.SelectAwait(async plugin =>
					(
						plugin,
						StringLookupTable.ParseNullTerminated(
							await resources.GetFileAsync(
								$"strings\\{Path.GetFileNameWithoutExtension(plugin.pluginFile.Name)}_english.strings"
							)
						)
					)
				)
				.ToDictionaryAsync(k => Path.GetFileNameWithoutExtension(k.plugin.pluginFile.Name), k => k.Item2);

			return new Data()
			{
				profile = profile,
				plugins = plugins,
				archives = archives,
				resources = resources,
				stringTables = stringTables
			};
		}

		public async Task<Dictionary<UInt32, (UInt32, Plugin.Record, Plugin)>> GetAllRecordsOfType(string recordType)
		{
			var records = await plugins
				.ToAsyncEnumerable()
				.SelectManyAwait(async plugin =>
					plugin
					.EnumerateGroupRecords(recordType)
					.Select(record => (record.id.ResolveFormID(profile.loadOrder, plugin), record, plugin)))
				.ToListAsync();
			var dictionary = records
				.ToLookup(entry => entry.Item1)
				.ToDictionary(lookup => lookup.Key, lookup => lookup.Last());

			return dictionary;
		}
	}
}
