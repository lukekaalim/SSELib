using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SSE
{
	/// <summary>
	/// BSA Version 105
	/// </summary>
	public struct Archive105: IAsyncDisposable
	{
		public string fileId;
		public UInt32 version;
		public UInt32 offset;
		[Flags]
		public enum ArchiveFlags: UInt32
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
		public ArchiveFlags archiveFlags;
		public UInt32 folderCount;
		public UInt32 fileCount;
		public UInt32 totalFolderNameLength;
		public UInt32 totalFileNameLength;
		[Flags]
		public enum FileFlags: UInt32
		{
			Meshes = 1,
			Textures =2,
			Menus = 4,
			Sounds = 8,
			Voices = 16,
			Shaders = 32,
			Trees = 64,
			Fonts = 128,
			Miscellaneous = 256,
		}
		public FileFlags fileFlags;

		public struct FolderRecord
		{
			/// <summary>
			/// Complex Hash Algorithm
			/// </summary>
			public Hash nameHash;
			public UInt32 count;
			public UInt32 padding;
			public UInt64 offset;

			public static FolderRecord From(Byte[] bytes, int offset)
			{
				return new FolderRecord()
				{
					nameHash = Hash.From(bytes, offset + 0),
					count = BitConverter.ToUInt32(bytes, offset + 8),
					padding = BitConverter.ToUInt32(bytes, offset + 12),
					offset = BitConverter.ToUInt64(bytes, offset + 16),
				};
			}
		}
		FolderRecord[] folderRecords;
		public struct FileRecord
		{
			public Hash nameHash;
			public UInt32 size;
			/// <summary>
			/// If the 30th bit (0x40000000) is set in the size:
			/// If files are default compressed, this file is not compressed.
			/// If files are default not compressed, this file is compressed.
			/// If the file is compressed the file data will have the specification
			/// of Compressed File block. In addition, the size of compressed data
			/// is considered to be the ulong "original size" plus the compressed
			/// data size (4 + compressed size).
			/// </summary>
			public bool compressionBit;
			public UInt32 offset;
			public static FileRecord From(Byte[] bytes, int offset)
			{
				return new FileRecord()
				{
					nameHash = Hash.From(bytes, offset + 0),
					// Mask out the 30th bit
					size = BitConverter.ToUInt32(bytes, offset + 8) & 0x01111111,
					// And mask out every bit BUT the 30th
					compressionBit = (BitConverter.ToUInt32(bytes, offset + 8) & 0x10000000) == 1,
					offset = BitConverter.ToUInt32(bytes, offset + 12),
				};
			}
		}
		public struct FileRecordBlock
		{
			public BZString name;
			public FileRecord[] fileRecords;
			public static FileRecordBlock From(Byte[] bytes, int offset, int fileCount)
			{
				var name = BZString.From(bytes, offset);
				var fileRecords = new FileRecord[fileCount];

				for (int i = 0; i < fileCount; i++)
					fileRecords[i] = FileRecord.From(bytes, offset + (name.length + 2) + (i * 16));

				return new FileRecordBlock()
				{
					name = name,
					fileRecords = fileRecords,
				};
			}
		}

		/// <summary>
		/// Underlying Stream Datasource
		/// </summary>
		Stream stream;

		public static async Task<Archive105> Open(Stream stream)
		{
			Byte[] headerBytes = new Byte[36];
			await stream.ReadAsync(headerBytes, 0, 36);

			var fileId = Encoding.UTF8.GetString(headerBytes, 0, 4);
			var version = BitConverter.ToUInt32(headerBytes, 4);
			if (fileId != "BSA\0" || version != 105)
				throw new Exception("Unsupported BSA Version or ID!");

			var folderCount = BitConverter.ToUInt32(headerBytes, 16);
			var fileCount = BitConverter.ToUInt32(headerBytes, 20);
			var totalFileNameLength = BitConverter.ToUInt32(headerBytes, 28);
			var totalFolderNameLength = BitConverter.ToUInt32(headerBytes, 24);
			var flags = (ArchiveFlags)BitConverter.ToUInt32(headerBytes, 12);

			// the size of all the folder records
			int allFolderRecordSize = (int)(folderCount * 24);
			// the size of all the file (and file block) records
			int allFileRecordBlockSize = (int)((folderCount * 1) + totalFolderNameLength + ((int)fileCount * 16));

			FolderRecord[] folderRecords = new FolderRecord[folderCount];
			FileRecordBlock[] fileRecordBlocks = new FileRecordBlock[folderCount];

			Byte[] folderAndFileRecordBytes = new Byte[stream.Length];
			await stream.ReadAsync(folderAndFileRecordBytes, 0, (int)stream.Length);

			string testString = Encoding.UTF8.GetString(folderAndFileRecordBytes);

			for (int i = 0; i < folderCount; i++)
			{
				var folderRecord = FolderRecord.From(folderAndFileRecordBytes, i * 24);
				folderRecords[i] = folderRecord;
				fileRecordBlocks[i] = FileRecordBlock.From(folderAndFileRecordBytes, (int)(folderRecord.offset - totalFolderNameLength), (int)folderRecord.count);
			}



			return new Archive105()
			{
				fileId = Encoding.UTF8.GetString(headerBytes, 0, 4),
				version = BitConverter.ToUInt32(headerBytes, 4),
				offset = BitConverter.ToUInt32(headerBytes, 8),
				archiveFlags = flags,
				folderCount = folderCount,
				fileCount = fileCount,
				totalFolderNameLength = totalFolderNameLength,
				totalFileNameLength = totalFileNameLength,
				fileFlags = (FileFlags)BitConverter.ToUInt32(headerBytes, 32),
				folderRecords = folderRecords,
				stream = stream
			};
		}

		public async ValueTask DisposeAsync()
		{
			if (stream != null)
				await stream.DisposeAsync();
		}
	}
}