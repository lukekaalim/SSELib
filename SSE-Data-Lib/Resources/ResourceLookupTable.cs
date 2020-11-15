using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace SSE
{
	public class ResourceLookupTable
	{
		Dictionary<string, FileInfo> overwriteFiles;
		Dictionary<string, Archive> archiveFiles;

		public ResourceLookupTable(List<Archive> archives, DirectoryInfo dataDirectory)
		{
			var overwritePairs = dataDirectory
				.GetFiles("*", SearchOption.AllDirectories)
				.Select(file => KeyValuePair.Create(Path.GetRelativePath(dataDirectory.FullName, file.FullName).ToLower(), file));

			var archivePairs = archives.SelectMany(archive =>
				archive.database.recordsByFilepath.Select(kvp =>
					KeyValuePair.Create(kvp.Key.ToLower(), archive)));

			overwriteFiles = new Dictionary<string, FileInfo>(overwritePairs);
			archiveFiles = archivePairs
				.ToLookup(entry => entry.Key, entry => entry.Value)
				.ToDictionary(entry => entry.Key, entry => entry.Last());
		}
		public async Task<Byte[]> GetFileAsync(string filename)
		{
			var strings = archiveFiles.Keys.Where(key => key.StartsWith("strings")).ToList();
			Archive archive;
			if (archiveFiles.TryGetValue(filename.ToLower(), out archive))
				return await archive.Read(filename);
			FileInfo file;
			if (overwriteFiles.TryGetValue(filename.ToLower(), out file))
				return await File.ReadAllBytesAsync(file.FullName);

			throw new FileNotFoundException("Could not find file in archive or local overwrite", filename);
		}
	}
}
