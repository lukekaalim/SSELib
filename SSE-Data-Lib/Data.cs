using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SSE
{
	public class Data: IAsyncDisposable
	{
		DirectoryInfo dataDirectory;
		DirectoryInfo profileDirectory;

		static string[] defaultPlugins = new string[] { "skyrim.esm", "update.esm", "dawnguard.esm", "hearthfires.esm", "dragonborn.esm" };

		public async Task<Data> Open(DirectoryInfo dataDirectory, DirectoryInfo profileDirectory)
		{
			FileInfo pluginOrderFile = new FileInfo(Path.Combine(profileDirectory.FullName, "plugins.txt"));
			
			return new Data()
			{
				dataDirectory = dataDirectory,
				profileDirectory = profileDirectory
			};
		}

		public ValueTask DisposeAsync()
		{
			throw new NotImplementedException();
		}
	}
}
