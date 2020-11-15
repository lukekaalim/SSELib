using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SSE
{
	public class Profile
	{
		public static string pluginsFileName = "plugins.txt";

		public LoadOrder loadOrder;
		public static async Task<Profile> Read(DirectoryInfo profileDirectory)
		{
			var pluginFile = new FileInfo(Path.Combine(profileDirectory.FullName, pluginsFileName));
			var loadOrder = await LoadOrder.Read(pluginFile);

			return new Profile()
			{
				loadOrder = loadOrder
			};
		}
	}
}
