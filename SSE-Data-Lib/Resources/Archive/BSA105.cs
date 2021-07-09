using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Immutable;

namespace SSE.Resources.Archive
{
	/// <summary>
	/// BSA Version 105
	/// </summary>
	public partial class BSA105
	{
		public readonly struct Header
		{
			[Flags]
			public enum ArchiveFlags : UInt32
			{
				IncludeDirectoryNames = 0,
				IncludeFileNames = 1,
				/// <summary>
				/// This does not mean all files are compressed.
				/// It means they are compressed by default.
				/// </summary>
				CompressedArchive = 2,
				RetainDirectorynames = 4,
				RetainFileNames = 8,
				RetainFileNameOffsets = 16,
				Xbox360Archive = 32,
				RetainStringsDuringStartup = 64,
				/// <summary>
				///  Indicates the file data blocks begin with a
				///  bstring containing the full path of the file.
				///  For example, in "Skyrim - Textures.bsa" the
				///  first data block is
				///  $2B textures\effects\fxfluidstreamdripatlus.dds
				///  ($2B indicating the name is 43 bytes).
				///  The data block begins immediately after the bstring.
				/// </summary>
				EmbedFileNames = 128,
				/// <summary>
				/// This can only be used with Bit 3
				/// (Compress Archive). This is an Xbox 360
				/// only compression algorithm.
				/// </summary>
				XMemCodec = 256
			}

			[Flags]
			public enum FileFlags : UInt32
			{
				Meshes = 1,
				Textures = 2,
				Menus = 4,
				Sounds = 8,
				Voices = 16,
				Shaders = 32,
				Trees = 64,
				Fonts = 128,
				Miscellaneous = 256,
			}

			public static int size = 36;

			public readonly string fileId;
			public readonly UInt32 version;
			public readonly UInt32 offset;
			public readonly ArchiveFlags archiveFlags;
			public readonly UInt32 folderCount;
			public readonly UInt32 fileCount;
			public readonly UInt32 totalFolderNameLength;
			public readonly UInt32 totalFileNameLength;
			public readonly FileFlags fileFlags;

			public int FolderRecordsTotalSize => FolderRecord.ByteSize * (int)folderCount;
			public int FileRecordsTotalSize => FileRecord.ByteSize * (int)fileCount;
			public int FileRecordBlockTotalSize => (int)totalFolderNameLength + (int)folderCount + FileRecordsTotalSize;
			public int TotalRecordsSize => FolderRecordsTotalSize + FileRecordBlockTotalSize + (int)totalFileNameLength;

			public Header(byte[] bytes, int bytesOffset = 0)
			{
				fileId = Encoding.UTF8.GetString(bytes, bytesOffset + 0, 4);
				version = BitConverter.ToUInt32(bytes, bytesOffset + 4);

				if (fileId != "BSA\0" || version != 105)
					throw new Exception("Unsupported BSA Version or ID!");

				offset = BitConverter.ToUInt32(bytes, bytesOffset + 8);
				archiveFlags = (ArchiveFlags)BitConverter.ToUInt32(bytes, bytesOffset + 12);
				folderCount = BitConverter.ToUInt32(bytes, bytesOffset + 16);
				fileCount = BitConverter.ToUInt32(bytes, bytesOffset + 20);
				totalFolderNameLength = BitConverter.ToUInt32(bytes, bytesOffset + 24);
				totalFileNameLength = BitConverter.ToUInt32(bytes, bytesOffset + 28);
				fileFlags = (FileFlags)BitConverter.ToUInt32(bytes, bytesOffset + 32);
			}
		}
		public readonly struct FolderRecord
		{
			public static int ByteSize => BSAHash.ByteSize + (sizeof(uint) * 2) + sizeof(ulong);
			public readonly BSAHash nameHash;
			public readonly uint fileCount;
			public readonly uint padding;
			public readonly ulong offset;
			public readonly FileRecordBlock block;

			public FolderRecord(byte[] bytes, int index, Header header)
			{
				var byteOffset = index * ByteSize;

				nameHash = new BSAHash(bytes, byteOffset + 0);
				fileCount = BitConverter.ToUInt32(bytes, byteOffset + 8);
				padding = BitConverter.ToUInt32(bytes, byteOffset + 12);
				offset = BitConverter.ToUInt64(bytes, byteOffset + 16);

				var recordOffset = (int)offset - Header.size - (int)header.totalFileNameLength;
				block = new FileRecordBlock(bytes, recordOffset, (int)fileCount);
			}
		}
		public readonly struct FileRecordBlock
		{
			public readonly BZString name;
			public readonly FileRecord[] fileRecords;
			public FileRecordBlock(byte[] recordsBytes, int offset, int count)
			{
				name = new BZString(recordsBytes, offset);
				fileRecords = new FileRecord[count];
				for (int i = 0; i < count; i++)
					fileRecords[i] = new FileRecord(recordsBytes, offset + (name.length + 1) + (i * 16));
			}
		}
		public readonly struct FileRecord
		{
			public static int ByteSize => BSAHash.ByteSize + (sizeof(uint) * 2);
			public bool CompressionBit => (sizeAndCompression & 0x00000001) == 1;
			public uint Size => sizeAndCompression & 0xFFFFFFFE;

			public readonly BSAHash nameHash;
			public readonly uint sizeAndCompression;
			public readonly uint offset;
			public FileRecord(byte[] bytes, int byteOffset)
			{
				nameHash = new BSAHash(bytes, byteOffset + 0);
				sizeAndCompression = BitConverter.ToUInt32(bytes, byteOffset + 8);
				offset = BitConverter.ToUInt32(bytes, byteOffset + 12);
			}
		}
		public readonly struct FileNameBlock
		{
			public readonly string[] names;
			public FileNameBlock(byte[] bytes, Header header)
			{
				int offset = header.FolderRecordsTotalSize + header.FileRecordBlockTotalSize;
				int length = (int)header.totalFileNameLength;
				int count = (int)header.fileCount;

				names = Encoding.UTF8.GetString(bytes, offset, length)
					.Split('\0', count, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		public readonly FileInfo file;
		public readonly Header header;
		public readonly FolderRecord[] folders;
		public readonly FileNameBlock filenames;

		public BSA105(FileInfo file, Header header, FolderRecord[] folders, FileNameBlock filenames)
		{
			this.file = file;
			this.header = header;
			this.folders = folders;
			this.filenames = filenames;
		}

		public static async Task<BSA105> Open(FileInfo file)
		{
			using var stream = file.OpenRead();

			byte[] headerBytes = new byte[Header.size];
			await stream.ReadAsync(headerBytes);
			var header = new Header(headerBytes);

			byte[] recordsBytes = new byte[header.TotalRecordsSize];
			await stream.ReadAsync(recordsBytes);

			var folders = new FolderRecord[header.folderCount];
			var filenames = new FileNameBlock(recordsBytes, header);

			for (int i = 0; i < header.folderCount; i++)
				folders[i] = new FolderRecord(recordsBytes, i, header);

			return new BSA105(file, header, folders, filenames);
		}
	}
}