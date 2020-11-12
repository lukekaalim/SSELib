using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SSE
{
	/// <summary>
	/// BSA Version 105
	/// </summary>
	public partial struct Archive
	{
		public Header header;
		public RecordDatabase database;
		public FileInfo archiveFile;
		public static async Task<Archive> Open(string archivePath)
		{
			return await Open(new FileInfo(archivePath));
		}

		public static async Task<Archive> Open(FileInfo archiveFile)
		{
			using var stream = archiveFile.OpenRead();

			Byte[] headerBytes = new Byte[Header.size];
			await stream.ReadAsync(headerBytes, 0, Header.size);
			var header = new Header(headerBytes, 0);

			var databaseSize = RecordDatabase.CalculateSize(header);
			Byte[] databaseBytes = new Byte[databaseSize];
			await stream.ReadAsync(databaseBytes, 0, databaseSize);
			var database = new RecordDatabase(databaseBytes, 0, header);

			Byte[] filenameBytes = new Byte[header.totalFileNameLength];
			await stream.ReadAsync(filenameBytes, 0, (int)header.totalFileNameLength);

			return new Archive()
			{
				header = header,
				database = database,
				archiveFile = archiveFile,
			};
		}

		public async ValueTask<Byte[]> Read(string path)
		{
			FileRecord record;
			if (database.recordsByFilepath.TryGetValue(path, out record))
			{
				using var stream = archiveFile.OpenRead();
				Byte[] content = new Byte[record.size];
				stream.Seek((int)record.offset, SeekOrigin.Begin);
				await stream.ReadAsync(content, 0, (int)record.size);
				var isArchiveCompressed = header.archiveFlags.HasFlag(ArchiveFlags.CompressedArchive);
				var isFileCompressed = isArchiveCompressed ? !record.compressionBit : record.compressionBit;
				return content;
			}
			return null;
		}
	}
}