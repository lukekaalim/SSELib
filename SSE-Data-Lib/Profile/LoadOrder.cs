using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE
{
	public class LoadOrder
	{
		static string[] newLineStrings = new string[] { "\r\n", "\r", "\n" };
		static string commentCharacter = "#";
		static char activePluginCharacter = '*';
		static List<string> defaultPlugins = new List<string>(new string[] { "skyrim.esm", "update.esm", "dawnguard.esm", "hearthfires.esm", "dragonborn.esm" });

		public List<string> plugins;

		public static async Task<LoadOrder> Read(FileInfo pluginsFile)
		{
			using var stream = pluginsFile.OpenRead();
			var reader = new StreamReader(stream);
			var pluginContents = await reader.ReadToEndAsync();

			var pluginsInPluginFile = pluginContents
				.Split(newLineStrings, StringSplitOptions.None)
				.Select(line => line.Trim())
				.Where(line => !line.StartsWith(commentCharacter) && line.Length > 0)
				.Where(line => line.StartsWith(activePluginCharacter))
				.Select(line => line.Trim(activePluginCharacter))
				.ToList();

			var allPlugins = defaultPlugins.Concat(pluginsInPluginFile).ToList();

			return new LoadOrder()
			{
				plugins = allPlugins,
			};
		}
	}
}
