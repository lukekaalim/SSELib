using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SSE.TESVArchive
{
	public class BSA105Header
	{
		public static long HeaderSize = 36;

		public string FileID { get; private set; }
		public uint Version { get; private set; }
		public uint FolderRecordOffset { get; private set; }

		public BSA105ArchiveFlags ArchiveFlags { get; private set; }
		public uint FolderCount { get; private set; }
		public uint FileCount { get; private set; }
		public uint TotalFolderNameLength { get; private set; }
		public uint TotalFileNameLength { get; private set; }
		public BSA105FileFlags FileFlags { get; private set; }

		// public int FolderRecordsTotalSize => FolderRecord.ByteSize * (int)folderCount;
		// public int FileRecordsTotalSize => FileRecord.ByteSize * (int)fileCount;
		// public int FileRecordBlockTotalSize => (int)totalFolderNameLength + (int)folderCount + FileRecordsTotalSize;
		// public int TotalRecordsSize => FolderRecordsTotalSize + FileRecordBlockTotalSize + (int)totalFileNameLength;

		public static BSA105Header Parse(byte[] headerBytes, int offset)
		{
			var fileId = Encoding.UTF8.GetString(headerBytes, offset + 0, 4);
			var version = BitConverter.ToUInt32(headerBytes, offset + 4);
			var folderRecordOffset = BitConverter.ToUInt32(headerBytes, offset + 8);

			var archiveFlags = (BSA105ArchiveFlags)BitConverter.ToUInt32(headerBytes, offset + 12);
			var folderCount = BitConverter.ToUInt32(headerBytes, offset + 16);
			var fileCount = BitConverter.ToUInt32(headerBytes, offset + 20);
			var totalFolderNameLength = BitConverter.ToUInt32(headerBytes, offset + 24);
			var totalFileNameLength = BitConverter.ToUInt32(headerBytes, offset + 28);
			var fileFlags = (BSA105FileFlags)BitConverter.ToUInt32(headerBytes, offset + 32);

			return new BSA105Header()
			{
				FileID = fileId,
				Version = version,
				FolderRecordOffset = folderRecordOffset,
				ArchiveFlags = archiveFlags,
				FolderCount = folderCount,
				FileCount = fileCount,
				TotalFolderNameLength = totalFolderNameLength,
				TotalFileNameLength = totalFileNameLength,
				FileFlags = fileFlags,
			};
		}
	}

	public struct BSA105FolderRecord
    {
		public static int FolderRecordSize => BSAHash.HashSize + 16;

		public BSAHash NameHash { get; set; }
		public uint FileCount { get; set; }
		public uint FileRecordOffset { get; set; }

		public static BSA105FolderRecord Parse(byte[] folderBytes, int byteOffset)
		{
			var nameHash = new BSAHash(folderBytes, byteOffset + 0);
			var fileCount = BitConverter.ToUInt32(folderBytes, byteOffset + 8);
			// buffer byteOffset + 12
			var fileRecordOffset = BitConverter.ToUInt32(folderBytes, byteOffset + 16);
			// buffer byteOffset + 20

			return new BSA105FolderRecord()
			{
				NameHash = nameHash,
				FileCount = fileCount,
				FileRecordOffset = fileRecordOffset
			};
		}
	}

	public struct FileRecordBlock
	{
		public BZString FolderName { get; set; }
		public List<FileRecord> FileRecords { get; set; }

		public static FileRecordBlock Parse(byte[] blockBytes, int byteOffset, int fileCount)
		{
			var folderName = new BZString(blockBytes, byteOffset);
			var fileRecords = new List<FileRecord>(fileCount);
			for (int i = 0; i < fileCount; i++)
            {
				var recordOffset = byteOffset + folderName.length + (i * FileRecord.ByteSize);
				var record = FileRecord.Parse(blockBytes, recordOffset);
				fileRecords.Add(record);
			}

			return new FileRecordBlock()
			{
				FolderName = folderName,
				FileRecords = fileRecords,
			};
		}
	}
	public struct FileRecord
	{
		public static int ByteSize => BSAHash.HashSize + 8;

		public bool CompressionBit => (SizeAndCompression & 0x00000001u) == 1;
		public uint Size => SizeAndCompression & 0xFFFFFFFEu;

		public BSAHash NameHash { get; set; }
		public uint SizeAndCompression { get; set; }
		public uint DataOffset { get; set; }

		public static FileRecord Parse(byte[] recordBytes, int offset)
        {
			var nameHash = BSAHash.Parse(recordBytes, offset);
			var sizeAndCompression = BitConverter.ToUInt32(recordBytes, offset + 8);
			var dataOffset = BitConverter.ToUInt32(recordBytes, offset + 12);
			return new FileRecord()
			{
				NameHash = nameHash,
				SizeAndCompression = sizeAndCompression,
				DataOffset = dataOffset,
			};
		}
	}

	public static class ArchiveScanner
    {
		public static async Task<BSA105Header> ReadHeader(Stream stream)
        {
			stream.Seek(0, SeekOrigin.Begin);
			var headerBytes = new byte[BSA105Header.HeaderSize];
			await stream.ReadAsync(headerBytes, 0, headerBytes.Length);
			return BSA105Header.Parse(headerBytes, 0);
        }
		public static async Task<List<BSA105FolderRecord>> ReadFolderRecords(Stream stream, BSA105Header header)
		{
			stream.Seek(BSA105Header.HeaderSize, SeekOrigin.Begin);

			var recordBytes = new byte[BSA105FolderRecord.FolderRecordSize * header.FolderCount];
			var records = new List<BSA105FolderRecord>((int)header.FolderCount);
			await stream.ReadAsync(recordBytes, 0, recordBytes.Length);

			for (int i = 0; i < header.FolderCount; i++)
            {
				var record = BSA105FolderRecord.Parse(recordBytes, i * BSA105FolderRecord.FolderRecordSize);
				records.Add(record);
			}

			return records;
        }
		public static async Task<FileRecordBlock> ReadFileRecordBlock (Stream stream, BSA105Header header, BSA105FolderRecord folder)
        {
			stream.Seek(folder.FileRecordOffset - header.TotalFileNameLength, SeekOrigin.Begin);

			var nameLength = stream.ReadByte();
			var blockBytes = new byte[nameLength + folder.FileCount * FileRecord.ByteSize];
			await stream.ReadAsync(blockBytes, 1, blockBytes.Length - 1);
			blockBytes[0] = (byte)nameLength;

			return FileRecordBlock.Parse(blockBytes, 0, (int)folder.FileCount);
		}

		public static async Task<List<string>> ReadFileNameBlock (Stream stream, BSA105Header headers)
		{
			stream.Seek(
				(
					BSA105Header.HeaderSize +
					(BSA105FolderRecord.FolderRecordSize * headers.FolderCount) +
					(FileRecord.ByteSize * headers.FileCount) +
					(headers.TotalFolderNameLength + headers.FolderCount)
				),
				SeekOrigin.Begin
			);
			var blockBytes = new byte[headers.TotalFileNameLength];
			await stream.ReadAsync(blockBytes, 0, blockBytes.Length);
			var blockString = Encoding.UTF8.GetString(blockBytes);
			return blockString.Split('\0').ToList();
		}
	}

	public class ArchiveStreamReader
	{
		public Stream ArchiveStream { get; set; }

		public BSA105Header Header { get; set; }
		public List<BSA105FolderRecord> Folders { get; set; }
		public List<FileRecordBlock> FileRecordBlocks { get; set; }
		public List<string> FileNames { get; set; }

		public Dictionary<string, FileRecord> RecordsByPath { get; set; }

		public static async Task<ArchiveStreamReader> Load(Stream archiveStream)
        {
			var header = await ArchiveScanner.ReadHeader(archiveStream);
			var folders = await ArchiveScanner.ReadFolderRecords(archiveStream, header);
			var blocks = new List<FileRecordBlock>();
			foreach (var folder in folders)
            {
				blocks.Add(await ArchiveScanner.ReadFileRecordBlock(archiveStream, header, folder));
			}
			var names = await ArchiveScanner.ReadFileNameBlock(archiveStream, header);

			var recordsByPath = BuildRecordsByPath(blocks, names);

			return new ArchiveStreamReader()
			{
				ArchiveStream = archiveStream,
				Header = header,
				Folders = folders,
				FileRecordBlocks = blocks,
				FileNames = names.ToList(),
				RecordsByPath = recordsByPath,
			};
		}

		static Dictionary<string, FileRecord> BuildRecordsByPath(List<FileRecordBlock> blocks, List<string> names)
		{
			var paths = new Dictionary<string, FileRecord>();
			int recordIndex = 0;
			foreach (var block in blocks)
			{
				var folderpath = block.FolderName.content;
				foreach (var file in block.FileRecords)
				{
					var filename = names[recordIndex];
					recordIndex++;
					paths.Add($"{folderpath}\\{filename}", file);
				}
			}
			return paths;
		}

		public async Task<byte[]> ReadFile(FileRecord record)
        {
			ArchiveStream.Seek(record.DataOffset, 0);
			var fileBytes = new byte[record.Size];
			await ArchiveStream.ReadAsync(fileBytes, 0, fileBytes.Length);
			return fileBytes;
        }

		public FileRecord GetRecord(string path)
        {
			return RecordsByPath[path];
		}
	}
}