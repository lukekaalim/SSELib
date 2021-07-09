using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using SSE.Resources;
using SSE.Resources.Archive;

using SSE.Records;

namespace SSE
{
	/// <summary>
	/// Represents a complete set of TESV resources
	/// </summary>
	public interface IData
	{

	}

	public class Data
	{
		ResourceTable resources;
		RecordTable records;

		static async Task<List<SSEPlugin>> LoadPlugins(DirectoryInfo dataDirectory, Profile profile)
		{
			var path = dataDirectory.FullName;
			var names = profile.loadOrder.plugins;
			var files = names.Select(name => new FileInfo(Path.Combine(path, name)));
			var plugins = await files
				.ToAsyncEnumerable()
				.SelectAwait(async file => await SSEPlugin.Load(file))
				.ToListAsync();
			return plugins;
		}

		static async Task<List<BSA105>> LoadArchives(DirectoryInfo dataDirectory, Profile profile)
		{
			var allArchives = dataDirectory
				.GetFiles()
				.Where(file => string.Equals(file.Extension, ".bsa", StringComparison.OrdinalIgnoreCase));
			var names = profile.loadOrder.plugins
				.Select(name => Path.GetFileNameWithoutExtension(name));
			var files = names
				.SelectMany(name => allArchives
					.Where(file => file.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase)));
			var archives = await files
				.ToAsyncEnumerable()
				.SelectAwait(async file => await BSA105.Open(file))
				.ToListAsync();
			return archives;
		}

		public static async Task<Data> Open(DirectoryInfo dataDirectory, DirectoryInfo profileDirectory)
		{
			var profile = await Profile.Read(profileDirectory);
			var plugins = await LoadPlugins(dataDirectory, profile);
			var archives = await LoadArchives(dataDirectory, profile);

			return new Data()
			{
				resources = new ResourceTable(archives, dataDirectory),
				records = new RecordTable(plugins)
			};
		}
	}
}
