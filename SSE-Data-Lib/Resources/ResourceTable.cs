using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace SSE.Resources
{
	public readonly struct FileResource : Streamable
	{
		readonly FileInfo file;
		public FileResource(FileInfo file) => this.file = file;

		public Stream OpenStream()
		{
			return file.OpenRead();
		}
	}

	public readonly struct ArchiveResource : Streamable
	{
		public readonly FileInfo archiveFile;
		public readonly BSAHash folderHash;
		public readonly BSAHash fileHash;

		public readonly int offset;
		public readonly int length;
		public readonly bool compressed;

		public ArchiveResource(Archive.BSA105 archive, Archive.BSA105.FolderRecord folder, Archive.BSA105.FileRecord file)
		{
			archiveFile = archive.file;
			folderHash = folder.nameHash;
			fileHash = file.nameHash;

			offset = (int)file.offset;
			length = (int)file.Size;
			compressed = archive
				.header
				.archiveFlags
				.HasFlag(Archive.BSA105.Header.ArchiveFlags.CompressedArchive) ^ file.CompressionBit;
		}
		public Stream OpenStream()
		{
			var archiveStream = archiveFile.OpenRead();
			archiveStream.Seek(offset, SeekOrigin.Begin);
			if (compressed)
				throw new NotImplementedException();
			return new ReadableStreamSlice(archiveStream, offset, length);
		}
	}

	public class ResourceTable
	{
		Dictionary<(ulong, ulong), ArchiveResource> resources;
		DirectoryInfo overwrite;

		public ResourceTable(List<Archive.BSA105> archives, DirectoryInfo dataDirectory)
		{
			var files = archives
				.SelectMany(archive =>
					archive.folders.SelectMany(folder =>
						folder.block.fileRecords.Select(file =>
							(archive, folder, file))))
				.ToList();

			resources = new Dictionary<(ulong, ulong), ArchiveResource>();
			overwrite = dataDirectory;

			foreach (var (archive, folder, file) in files)
				resources[(folder.nameHash, file.nameHash)] = new ArchiveResource(archive, folder, file);
		}

		public Streamable GetResource(string filePath)
		{
			string directoryName = Path.GetDirectoryName(filePath);
			string fileName = Path.GetFileName(filePath);

			ArchiveResource archiveResource;
			var hashPair = (BSAHash.HashPath(directoryName), BSAHash.HashPath(fileName));
			if (resources.TryGetValue(hashPair, out archiveResource))
				return archiveResource;

			string fullFilePath = Path.Combine(overwrite.FullName, filePath);
			if (File.Exists(fullFilePath))
				return new FileResource(new FileInfo(fullFilePath));

			throw new FileNotFoundException("Could not find file in archive or local overwrite", filePath);
		}
	}
}
