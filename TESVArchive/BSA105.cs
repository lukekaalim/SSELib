using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SSE.TESVArchive
{

	/// <summary>
	/// A string prefixed with a byte length and terminated with a zero (\x00).
	/// </summary>
	public readonly struct BZString
	{
		public readonly string content;
		public readonly int length;
		public BZString(byte[] bytes, int offset)
		{
			length = bytes[offset];
			content = Encoding.UTF8.GetString(bytes, offset + 1, length - 1);
		}
		public static explicit operator string(BZString b) => b.content;
	}

	public readonly struct BSAHash
	{
		public static int HashSize => sizeof(ulong);
		public readonly ulong value;
		static uint GetMaskForExtension (string extension)
		{
			switch (extension)
			{
				case ".kf":
					return 0x80u;
				case ".nif":
					return 0x8000u;
				case ".dds":
					return 0x8080u;
				case ".wav":
					return 0x80000000u;
				default:
					return 0x0;
			}
		}

		public BSAHash(byte[] bytes, int offset) => value = BitConverter.ToUInt64(bytes, offset);
		public BSAHash(ulong value) => this.value = value;
		public static BSAHash HashPath(string name)
		{
			name = name.Replace('/', '\\');
			return HashFile(Path.ChangeExtension(name, null), Path.GetExtension(name));
		}
		public static BSAHash HashFile(string name, string extension)
		{
			name = name.ToLowerInvariant();
			extension = extension.ToLowerInvariant();
			var hashBytes = new byte[]
			{
				(byte)(name.Length == 0 ? '\0' : name[name.Length - 1]),
				(byte)(name.Length < 3 ? '\0' : name[name.Length - 2]),
				(byte)name.Length,
				(byte)name[0]
			};
			var hash1 = BitConverter.ToUInt32(hashBytes, 0) | GetMaskForExtension(extension);

			uint hash2 = 0;
			for (var i = 1; i < name.Length - 2; i++)
				hash2 = hash2 * 0x1003f + (byte)name[i];

			uint hash3 = 0;
			for (var i = 0; i < extension.Length; i++)
				hash3 = hash3 * 0x1003f + (byte)extension[i];

			return new BSAHash((((ulong)(hash2 + hash3)) << 32) + hash1);
		}

		public static implicit operator ulong(BSAHash h) => h.value;
	}

	[Flags]
	public enum BSA105ArchiveFlags : uint
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
	public enum BSA105FileFlags : uint
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

		//public int FolderRecordsTotalSize => FolderRecord.ByteSize * (int)folderCount;
		//public int FileRecordsTotalSize => FileRecord.ByteSize * (int)fileCount;
		//public int FileRecordBlockTotalSize => (int)totalFolderNameLength + (int)folderCount + FileRecordsTotalSize;
		//public int TotalRecordsSize => FolderRecordsTotalSize + FileRecordBlockTotalSize + (int)totalFileNameLength;

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

	public class FolderRecord
	{
		public static int ByteSize => BSAHash.HashSize + (sizeof(uint) * 2) + sizeof(ulong);

		public readonly BSAHash nameHash;
		public readonly uint fileCount;

		public readonly ulong offset;

		public FolderRecord Parse(byte[] folderBytes, int index , BSA105Header header)
		{
			var byteOffset = index * ByteSize;

			var nameHash = new BSAHash(bytes, byteOffset + 0);
			var fileCount = BitConverter.ToUInt32(bytes, byteOffset + 8);
			var offset = BitConverter.ToUInt64(bytes, byteOffset + 16);

			var recordOffset = (int)offset - BSA105Header.HeaderSize - (int)header.totalFileNameLength;
			var block = new FileRecordBlock(bytes, recordOffset, (int)fileCount);
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
		public static int ByteSize => BSAHash.HashSize + (sizeof(uint) * 2);
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
		public FileNameBlock(byte[] bytes, BSA105Header header)
		{
			int offset = header.FolderRecordsTotalSize + header.FileRecordBlockTotalSize;
			int length = (int)header.totalFileNameLength;
			int count = (int)header.fileCount;

			names = Encoding.UTF8.GetString(bytes, offset, length)
				.Split('\0', count, StringSplitOptions.RemoveEmptyEntries);
		}
	}

	public class BSA105
	{
		BSA105Header Header { get; set; }
		/*
		*/
		/*
		public readonly FileInfo file;
		public readonly BSA105Header header;
		public readonly FolderRecord[] folders;
		public readonly FileNameBlock filenames;

		public BSA105(FileInfo file, BSA105Header header, FolderRecord[] folders, FileNameBlock filenames)
		{
			this.file = file;
			this.header = header;
			this.folders = folders;
			this.filenames = filenames;
		}

		public static async Task<BSA105> Open(FileInfo file)
		{
			using var stream = file.OpenRead();

			byte[] headerBytes = new byte[BSA105Header.HeaderSize];
			await stream.ReadAsync(headerBytes);
			var header = new BSA105Header(headerBytes);

			byte[] recordsBytes = new byte[header.TotalRecordsSize];
			await stream.ReadAsync(recordsBytes);

			var folders = new FolderRecord[header.folderCount];
			var filenames = new FileNameBlock(recordsBytes, header);

			for (int i = 0; i < header.folderCount; i++)
				folders[i] = new FolderRecord(recordsBytes, i, header);

			return new BSA105(file, header, folders, filenames);
		}
		*/
	}
}