using System;
using System.Collections.Generic;
using System.Text;

namespace SSE
{
	public partial struct Archive
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

		public struct Header
		{
			public static int size = 36;

			public string fileId;
			public UInt32 version;
			public UInt32 offset;
			public ArchiveFlags archiveFlags;
			public UInt32 folderCount;
			public UInt32 fileCount;
			public UInt32 totalFolderNameLength;
			public UInt32 totalFileNameLength;
			public FileFlags fileFlags;

			public Header(Byte[] bytes, int bytesOffset)
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
	}
}
